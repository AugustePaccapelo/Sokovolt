using Godot;
using System;
using Com.IsartDigital.SokoVolt.GameObjects;
using Com.IsartDigital.SokoVolt.GameObjects.Movables;
using System.Collections.Generic;
using Com.IsartDigital.Sokovolt;
using System.Data;
using System.Linq;
using Com.IsartDigital.SokoVolt.Tools;
using static Com.IsartDigital.SokoVolt.Tools.ObjectProperties;
using System.Threading.Tasks;


//Author : Ferlat Thibaud 
namespace Com.IsartDigital.SokoVolt.Managers {

	public partial class GridManager : Manager
	{
		#region GetInstance
		static private GridManager instance;

		static public GridManager GetInstance() {
			if (instance == null) instance = new GridManager();
			return instance;
		}

		private GridManager() : base() { }
		#endregion

		RandomNumberGenerator rand = new RandomNumberGenerator(); 

		//Grid Gestion 
		public Cell[,] grid { get; private set; }
		public List<Cell[,]> gridStates = new List<Cell[,]>();
		private int actualGridStateIndex = 0;
		public static Vector2 gridOffset;
		public Player player;

		//Step Counter 
		public const string STEP_LABEL_PREFIXE = "STEP : ";
		public int step { get; private set; } = 0;

		//Ref 
		private GameManager gameManager;
		private HUD hud; 
        //UndoRedo 
        private bool playerWasOnTesla; 

		//LevelsAnimation
		[Export] private PackedScene thunderEffectScene, pistonScene; 
		Node2D vortex; 
		private const string VORTEX_PATH = "res://Assets/GameObjects/LevelAnimation/vecteezy_spiral-vortex-element_27720416.png"; 


		public override void _Ready()
		{
			#region instance
		if (instance != null){  
			QueueFree();
			GD.Print(nameof(GridManager) + " Instance already exist, destroying the last added.");
			return;
		}
		instance = this;
			#endregion

			base._Ready();
        }

        public override void _Process(double pDelta)
		{
        }

        public override void Init()
        {
            base.Init();
			hud = HUD.GetInstance();
			gameManager = GameManager.GetInstance();
			SignalsConnetion();
        }

		private void  SignalsConnetion()
		{
			CustomSignals lSignal = CustomSignals.GetInstance();
			lSignal.LoadLevel  += (level) => LoadNewLevel(level, JsonKeys.LEVELS_JSONS_PATH, gameManager.objectsContainer);
            lSignal.Move += OnMovePlayer;
            lSignal.UndoRedo += UndoRedo;
            lSignal.Retry += Retry;

            lSignal.UndoButton += () => UndoRedo(-1);
            lSignal.RedoButton += () => UndoRedo(1);

			lSignal.GameFinished += EndLevelAnimation; 
		}



		#region // ----- Load Level ----- \\

		public void LoadNewLevel(int pLevelToLoad, string pLevelPath, Node2D pObjectContainer) // ==================> Change Level with index(start at 0)
		{
			ResetStepCounter();
			hud.Visible = true;
			hud.displayInGame.Visible = true;	
			LevelLoader.GetInstance().LoadLevel(pLevelToLoad, pLevelPath, pObjectContainer);
			CenterGrid(); 

			if (grid == null)  // Avoid null state 
				return;

			StockGridState();
			StartIntroAnimation();
		}

		public void SetNewLevel(Cell[,] pNewGrid)
		{
			grid = pNewGrid;
			gridStates.Clear();
			actualGridStateIndex = 0;
		}


		public void ClearGrid()
		{
			if (grid != null)
			{
				for (int y = 0; y < LevelLoader.levelHeight; y++)
				{
					for (int x = 0; x < LevelLoader.levelWidth; x++)
					{
						if (grid[x, y] != null)
						{
							grid[x, y].SetContent(null);
							grid[x, y] = null; 
						}
					}
				}
			}

			foreach (Node lChild in gameManager.objectsContainer.GetChildren())
			{
				if (lChild is GameObject || lChild is Cell)
				{
					lChild.QueueFree(); 
				}
			}

			// Reset grid history
			grid = null;
			gridStates.Clear();
			actualGridStateIndex = 0;

			hud.Visible = false;

			GD.Print("ClearGrid: Lvl deleted !");
		}

		#endregion



		#region // ----- Grid Centering ----- \\
		public void CenterGrid()
		{
			Vector2 lScreenSize = GetViewportRect().Size;

			Vector2 lIsoTopLeft = IsoManager.ModelToIsoView(Vector2.Zero);
			Vector2 lIsoBottomRight = IsoManager.ModelToIsoView(new Vector2(LevelLoader.levelWidth - 1, LevelLoader.levelHeight - 1));

			float lIsoWidth = Math.Abs(lIsoBottomRight.X - lIsoTopLeft.X);
			float lIsoHeight = Math.Abs(lIsoBottomRight.Y - lIsoTopLeft.Y);

			gridOffset = new Vector2(
				(lScreenSize.X - lIsoWidth) / 2,
				(lScreenSize.Y - lIsoHeight) / 2
			);
		}

		#endregion



		#region // ----- Player and Boxs Movement ----- \\

		// Player move call (triggered by input signal)
		public void OnMovePlayer(Vector2 pPlayerDirection)
		{
			MovePlayer((int)pPlayerDirection.X, (int)pPlayerDirection.Y);
		}

		// Handles all move logic: player alone or pushing Tesla
		private void MovePlayer(int pDx, int pDy)
		{
			playerWasOnTesla = grid[player.x, player.y].GetContent() is BoxTesla;

			int newX = player.x + pDx;
			int newY = player.y + pDy;

			if (OutOfGrid(newX, newY)) return;

			Cell targetCell = grid[newX, newY];
			GameObject content = targetCell.GetContent();

			// Move into empty or door cell
			if (content == null || content is Door)
			{
				player.MoveTo(newX, newY, grid);
				StockGridState();
			}
			// Pushing a Tesla (box)
			else if (content is BoxTesla box)
			{
				int boxTargetX = newX + pDx;
				int boxTargetY = newY + pDy;

				if (OutOfGrid(boxTargetX, boxTargetY)) return;

				Cell boxTargetCell = grid[boxTargetX, boxTargetY];

				// If cell behind is empty, move box + player
				if (boxTargetCell.GetContent() == null || boxTargetCell.GetContent() is Door)
				{
					box.MoveTo(boxTargetX, boxTargetY, grid);
					//Player.canTravel = false;
					box.MovableHaveFinish += (sender) => MovableFinished(sender, box);
                    player.MoveTo(newX, newY, grid);
					StockGridState();
				}
			}
			else return;

			//PrintGrid();
		}

		private void MovableFinished(Movable pSender, BoxTesla pBox)
		{
			//Player.canTravel = true;
			pBox.MovableHaveFinish -= (sender) => MovableFinished(sender, pBox);
		}

		// Prevents moves outside the grid
		private bool OutOfGrid(int pX, int pY)
		{
			return pX < 0 || pX >= LevelLoader.levelWidth || pY < 0 || pY >= LevelLoader.levelHeight;
		}
		


        // ----- PathFinding ----- \\

        // Called when a cell is clicked (mouse/touch)
		public void HandleCellClicked(Vector2 pTargetPos)
		{
			int x = (int)pTargetPos.X;
			int y = (int)pTargetPos.Y;
			if (OutOfGrid(x, y)) return;

			Cell targetCell = grid[x, y];
			GameObject content = targetCell.GetContent();
			Vector2 start = new Vector2(player.x, player.y);
			Vector2 end = new Vector2(x, y);

			// If clicked next to a Tesla, attempt to push it
			if (content is BoxTesla && (end - start).Length() == 1)
			{
				OnMovePlayer(end - start);
				return;
			}

			// Can't walk into a closed door
			//if (content is Door door && !door.isOpen) return;

			// If cell is empty or cell is door, move
			if (content == null || content is Door || content is BoxTesla)
			{
				InputManager.canPlayerMove = false;
				var path = PathFinding.FindPath(start, end, grid);
				if (path != null && path.Count > 0)
				{
					StockGridState();
					player.MoveAlongPath(path);
				}
			}
		}


        #endregion



        #region // ----- Undo/Redo/Retry ----- \\

        public static bool currentlyUndoRedo; 

       // Triggers undo or redo with index
		private void UndoRedo(int pAmount)
		{
			int amount = pAmount;
			
			// Special case if player moved off a Tesla
			if (!(player.curentCell.GetContent() is BoxTesla) && playerWasOnTesla)
				amount *= 2;

			SetGridState(actualGridStateIndex + amount);
		}

		// Creates a copy of the grid (for history)
		private Cell[,] CopyGrid(Cell[,] original)
		{
			int w = LevelLoader.levelWidth;
			int h = LevelLoader.levelHeight;
			Cell[,] newGrid = new Cell[w, h];

			for (int y = 0; y < h; y++)
			{
				for (int x = 0; x < w; x++)
				{
					if (original[x, y] != null)
					{
						newGrid[x, y] = new Cell();
						newGrid[x, y].SetContent(original[x, y].GetContent());
					}
					else
					{
						newGrid[x, y] = null;
						GD.PrintErr("grid is null !!");
					}
				}
			}
			return newGrid;
		}

		// Saves current grid state for undo/redo
		public void StockGridState()
		{
			// Remove future states if we undo then move again
			if (actualGridStateIndex < gridStates.Count - 1)
				gridStates.RemoveRange(actualGridStateIndex + 1, gridStates.Count - (actualGridStateIndex + 1));

			gridStates.Add(CopyGrid(grid));
			actualGridStateIndex = gridStates.Count - 1;
			UpdateStepLabel();
		}

		// Load a specific past/future state
		public void SetGridState(int pIndexState)
		{
			if (pIndexState < 0 || pIndexState >= gridStates.Count)
				return;

            currentlyUndoRedo = true;

            grid = CopyGrid(gridStates[pIndexState]);
			actualGridStateIndex = pIndexState;
			UpdateStepLabel();
			UpdateObjectsFromGrid();

            // Cooldown to avoid tesla detection 
            GetTree().CreateTimer(1).Timeout += () => currentlyUndoRedo = false;
        }



		// Replaces positions of movable objects from grid content
		private void UpdateObjectsFromGrid()
		{
			//Player.canTravel = false;
			for (int y = 0; y < LevelLoader.levelHeight; y++)
			{
				for (int x = 0; x < LevelLoader.levelWidth; x++)
				{
					Cell cell = grid[x, y];
					if (cell == null) continue;

					if (cell.GetContent() is Movable movable)
						movable.MoveTo(x, y, grid);
				}
			}
		}

		// Resets grid to first state (restart)
		private void Retry()
		{
			SetGridState(0);
			CustomSignals.GetInstance().EmitSignal(CustomSignals.SignalName.StartRecherche);
        }


		#endregion
		


		#region // ----- Step Counter ----- \\
		private void UpdateStepLabel()
		{
			step = actualGridStateIndex;
			hud.stepLabel.Text = Tr(STEP_LABEL_PREFIXE) + step;
		}
		private void ResetStepCounter()
		{
			step = 0;
			hud.stepLabel.Text = STEP_LABEL_PREFIXE + step;
		}
		#endregion

		#region // ----- Finished Level animation -----//

		public void StartIntroAnimation() 
		{
			foreach (Node2D lObject in gameManager.objectsContainer.GetChildren())
				lObject.Visible = false;
            HUD.GetInstance().mainMenuButton.Disabled = true;
            float lDelay = 0f;

			for (int y = 0; y < LevelLoader.levelHeight; y++) 
			{
				for (int x = 0; x < LevelLoader.levelWidth; x++)
				{

					Cell lCell = grid[x, y];
					if (lCell == null) continue;

					AnimationPiston lPiston = Utils.Spawner(pistonScene, x, y, gameManager.objectsContainer) as AnimationPiston;

					lCell.Visible = true;
					if (lCell.GetContent() != null) lCell.GetContent().Visible = true;

					lPiston.Launch(lCell, lPiston.GlobalPosition, lDelay);

					lDelay += 0.02f;
				}
			}

			GetTree().CreateTimer(lDelay + 1).Timeout += () => {
				Tween lTween = AnimationManager.GetInstance().CameraZoomTraveling(GameManager.GetInstance().camera, 0.3f, 0.5f, player.Position, GameManager.GetInstance().cameraDefaultPos, 2f);
                // lTween.TweenProperty(player, SCALE, new Vector2(2, 2), 0.4f);
                InputManager.canPlayerMove = true;

                lTween.Finished += () =>
				{
					player.bodyParticles.Emitting = InputManager.canPlayerMove = true;
                    HUD.GetInstance().mainMenuButton.Disabled = false;
					CustomSignals.GetInstance().EmitSignal(CustomSignals.SignalName.DisplayDialog);
                };
                
				Player.canTravel = true;
				CustomSignals.GetInstance().EmitSignal(CustomSignals.SignalName.StartRecherche);
			};
		}


		private async void EndLevelAnimation(int pNumStar, int pScore, int pNumStep)
		{
			HUD.GetInstance().mainMenuButton.Disabled = true;
			Player.canTravel = false;
			InputManager.canPlayerMove = false;
			if(LevelCreator.inLevelCreator) LevelCreator.GetInstance().returnButton.Disabled = true;
            List<Node2D> lObjectsToAnimate= new List<Node2D>();
			lObjectsToAnimate.Clear(); 

			foreach(Node2D lObject in gameManager.objectsContainer.GetChildren())
			{
				lObjectsToAnimate.Add(lObject);

				if(lObject is BoxTesla lBoxTesla)
					lBoxTesla.LineDeconnection(); 

			}

			
			Vector2 lVortexCenter = GetViewportRect().Size/2; 

			vortex = CreateVortex(lVortexCenter); 
			AddChild(vortex);

			Random lRand = new Random();
			lObjectsToAnimate = lObjectsToAnimate.OrderBy(c => lRand.Next()).ToList();

			float lBaseDelay = 0.02f; 
			float lRandDelay; 
			int lMaxDistancePropulsion = 1000;

			for (int i = 0; i < lObjectsToAnimate.Count; i++)
			{
				lRandDelay = rand.Randf()* lBaseDelay; 
				Node2D lObject = lObjectsToAnimate[i];
				if (lObject == null) continue;

				FlashElectricEffect(lObject);  

				float lRandPropulsion = rand.Randf() * lMaxDistancePropulsion; 

				if (lObject != null)
				{
					Vector2 lNewObjectPos = lObject.GlobalPosition.DirectionTo(lVortexCenter) * lRandPropulsion + lObject.GlobalPosition;
					Tween lTween = CreateTween();
					lTween.TweenProperty(lObject, POSITION, lNewObjectPos, 1f)
							.SetTrans(Tween.TransitionType.Elastic)
							.SetEase(Tween.EaseType.Out);
				}

				await ToSignal(GetTree().CreateTimer(lRandDelay), TIME_OUT);
			}

            await ToSignal(GetTree().CreateTimer(0.3f), "timeout");

            AnimateVortex(vortex);

            foreach (Node2D lObject in lObjectsToAnimate)
			{
				Tween lTween = CreateTween(); 
				lTween.TweenProperty(lObject, POSITION, lVortexCenter, 1.3f)
					.SetTrans(Tween.TransitionType.Linear)
					.SetEase(Tween.EaseType.In); 

				lTween.Finished += ()=> lObject.Visible = false; 
			}

		}

		private Node2D CreateVortex(Vector2 pPosition)
		{
			Node2D lVortex = new Node2D();
			lVortex.GlobalPosition = pPosition;

			Sprite2D lVortexSprite = new Sprite2D();
			lVortexSprite.Texture = GD.Load(VORTEX_PATH) as Texture2D; 
			lVortexSprite.Modulate = new Color(1, 1, 1, 0); 
			lVortexSprite.Scale = Vector2.One * 0.1f;
			lVortex.AddChild(lVortexSprite);

			return lVortex;
		}

		//  Vortex animation 
		private void AnimateVortex(Node2D vortex)
		{
			gameManager.shaker.Start();
			Sprite2D lVortexSprite = vortex.GetChild<Sprite2D>(0);
			Tween lVortexTween = CreateTween();

			lVortexTween.Parallel().TweenProperty(lVortexSprite, SCALE, Vector2.One, 0.8f)
					.SetTrans(Tween.TransitionType.Linear)
					.SetEase(Tween.EaseType.Out);

			lVortexTween.Parallel().TweenProperty(lVortexSprite, MODULATE, new Color(1, 1, 1, 1), 0.8f);

			lVortexTween.Parallel().TweenProperty(lVortexSprite, ROTATION, Mathf.DegToRad(600), 1f)
					.SetTrans(Tween.TransitionType.Linear)
					.SetEase(Tween.EaseType.InOut);

			lVortexTween.TweenProperty(lVortexSprite, SCALE, Vector2.Zero, 0.8f)
					.SetTrans(Tween.TransitionType.Linear)
					.SetEase(Tween.EaseType.OutIn);

			lVortexTween.Finished += () => GetTree().CreateTimer(1f).Timeout += EndLevelAnimationFnished;
		}


		// Thunder effect on tiles 
		private void FlashElectricEffect(Node2D pObject)
		{
			WinScreenThunder lThunderEffect = thunderEffectScene.Instantiate() as WinScreenThunder;
			lThunderEffect.ZIndex = 45; 
			gameManager.objectsContainer.AddChild(lThunderEffect);

			lThunderEffect.ActiveThunder(pObject, WinScreenThunder.THUNDER_ANIMATION); 
		}

		private void EndLevelAnimationFnished()
		{
			vortex.QueueFree();
			CustomSignals.GetInstance().EmitSignal(CustomSignals.SignalName.EndLevelAnimation); 
			HUD.GetInstance().mainMenuButton.Disabled = false;
			if(LevelCreator.inLevelCreator) LevelCreator.GetInstance().returnButton.Disabled = false;
        }


		#endregion



		#region // ----- Provisional for testing ----- \\
		public void PrintGrid()	
		{
			string lGridString = "";

			for (int y = 0; y < LevelLoader.levelHeight; y++)
			{
				for (int x = 0; x < LevelLoader.levelWidth; x++)
				{
					GameObject lContent = grid[x, y].GetContent();
					
					if (lContent is Player)
						lGridString += "@ ";
					else if (lContent is BoxTesla)
						lGridString += "$ ";
					else if (lContent is Wall)							
						lGridString += "# ";
					else if (lContent is ElectricWall)
						lGridString += "* ";
					else if (lContent is GoalBulb)
						lGridString += ". ";
					else if (lContent is Generator)
						lGridString += "/ ";
					else if (lContent is Door)
						lGridString += "| ";
					else
						lGridString += "- ";  //Empty tile 
				}
				lGridString += "\n";  // New line for each row
			}

			GD.Print(lGridString);
		}
		#endregion

	


		#region dispose
		protected override void Dispose(bool pDisposing)
		{
			if (pDisposing && instance == this) instance = null;
			base.Dispose(pDisposing);
		}
		#endregion
	}
}
