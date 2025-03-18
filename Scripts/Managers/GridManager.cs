using Godot;
using System;
using Com.IsartDigital.SokoVolt.GameObjects;
using Com.IsartDigital.SokoVolt.GameObjects.Movables;
using System.Collections.Generic;
using Com.IsartDigital.ProjectName;
using System.Data;
using System.Linq;
using Com.IsartDigital.SokoVolt.Tools;

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
        [Export] private Node2D objectsContainer;
        public List<Cell[,]> gridStates = new List<Cell[,]>();
		private int actualGridStateIndex = 0;
		public static Vector2 gridOffset;
		public Player player;

		//Step Counter 
		private const string STEP_LABEL_PREFIXE = "STEP : ";
		public int step { get; private set; } = 0;

		//Ref 
		private GameManager gameManager;
		private HUD hud; 
        //UndoRedo 
        private bool playerWasOnTesla; 

		//LevelsAnimation
		[Export] private PackedScene thunderEffectScene; 
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
			CustomSignals lSignals = CustomSignals.GetInstance();

			lSignals.LoadLevel  += LoadNewLevel;
            lSignals.Move += OnMovePlayer;
            lSignals.UndoRedo += UndoRedo;
            lSignals.Retry += Retry;

            lSignals.UndoButton += () => UndoRedo(-1);
            lSignals.RedoButton += () => UndoRedo(1);

			lSignals.GameFinished += EndLevelAnimation; 
		}



		#region // ----- Load Level ----- \\

		public void LoadNewLevel(int pLevelToLoad, string pLevelPath, Node2D pObjectContainer) // ==================> Charger un niveau avec son index (commence à 0)
		{
			ResetStepCounter();
			hud.Visible = true;
			LevelLoader.GetInstance().LoadLevel(pLevelToLoad);
			CenterGrid(); 

			if (grid == null)  // Évite d'ajouter un état vide
				return;

			StockGridState();
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

			// Réinitialise la grille et l'historique
			grid = null;
			gridStates.Clear();
			actualGridStateIndex = 0;

			// Rendre le HUD invisible
			hud.Visible = false;

			GD.Print("ClearGrid: Niveau supprimé !");
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
		public void OnMovePlayer(Vector2 pPlayerDirection)
		{
			MovePlayer((int)pPlayerDirection.X, (int)pPlayerDirection.Y);
		}

        private void MovePlayer(int pDx, int pDy)
		{
            playerWasOnTesla = grid[player.x, player.y].GetContent() is BoxTesla; 
            int lNewX = player.x + pDx;
			int lNewY = player.y + pDy;

			if(OutOfGrid(lNewX, lNewY))
				return;

			
			Cell lNewCell = grid[lNewX, lNewY];
			GameObject lContent = lNewCell.GetContent();

			if (lContent == null || lContent is Door)
			{
				player.MoveTo(lNewX, lNewY, grid);
				StockGridState();
			}
			else if (lContent is BoxTesla lBox)
			{
				int lNewBoxX = lNewX + pDx;
				int lNewBoxY = lNewY + pDy;

				if (OutOfGrid(lNewBoxX, lNewBoxY))
					return;
				
				Cell lNewBoxCell = grid[lNewBoxX, lNewBoxY];

				if (lNewBoxCell.GetContent() == null)
				{
					lBox.MoveTo(lNewBoxX, lNewBoxY, grid);
					player.MoveTo(lNewX, lNewY, grid);
					StockGridState();
				}
			}
			else return;

			PrintGrid(grid);
		}

		private bool OutOfGrid(int pX, int pY)
		{
			return pX < 0 || pX >= LevelLoader.levelWidth || pY < 0 || pY >= LevelLoader.levelHeight;
		}
		#endregion



		#region // ----- Undo/Redo/Retry ----- \\

        public static bool currentlyUndoRedo; 
        private void UndoRedo(int pAmount)
        {
            int lAmount = pAmount; 
            currentlyUndoRedo = true;
            if(!(player.curentCell.GetContent() is BoxTesla) && playerWasOnTesla) lAmount *= 2;
            SetGridState(actualGridStateIndex + lAmount);
			
            GetTree().CreateTimer(1).Timeout += () => currentlyUndoRedo = false;	
        }

		private Cell[,] CopyGrid(Cell[,] pOriginalGrid)
		{
			int lWidth = LevelLoader.levelWidth;
			int lHeight = LevelLoader.levelHeight;
			Cell[,] lNewGrid = new Cell[lWidth, lHeight];

			for (int y = 0; y < lHeight; y++)
			{
				for (int x = 0; x < lWidth; x++)
				{
					if (pOriginalGrid[x, y] != null)
					{
						lNewGrid[x, y] = new Cell();
						lNewGrid[x, y].SetContent(pOriginalGrid[x, y].GetContent());
					}
					else
					{
						lNewGrid[x, y] = null; 
						GD.PrintErr("grid is null !!"); 
					}
				}
			}

			return lNewGrid;
		}

		
		private void StockGridState()
		{
			
			if (actualGridStateIndex < gridStates.Count - 1)
				gridStates.RemoveRange(actualGridStateIndex + 1, gridStates.Count - (actualGridStateIndex + 1));
			
			gridStates.Add(CopyGrid(grid));
			actualGridStateIndex = gridStates.Count - 1;
			UpdateStepLabel();
		}


		public void SetGridState(int pIndexState)
		{
			if (pIndexState < 0 || pIndexState >= gridStates.Count)
				return;

			grid = CopyGrid(gridStates[pIndexState]);

			actualGridStateIndex = pIndexState;
			UpdateStepLabel();
			UpdateObjectsFromGrid();
			PrintGrid(grid);
		}


		private void UpdateObjectsFromGrid()
		{
			for (int y = 0; y < LevelLoader.levelHeight; y++)
			{
				for (int x = 0; x < LevelLoader.levelWidth; x++)
				{
					Cell lCell = grid[x, y];

					if (lCell == null)  // Évite le crash en cas de cellule absente
						continue;

					GameObject lContent = lCell.GetContent();

					if (lContent != null && lContent is Movable lMovable)
					{
						lMovable.MoveTo(x, y, grid);
					}
				}
			}
		}

		private void Retry()
		{
			SetGridState(0); 
		}

		#endregion
		


		#region // ----- Step Counter ----- \\
		private void UpdateStepLabel()
		{
			step = actualGridStateIndex;
			hud.stepLabel.Text = STEP_LABEL_PREFIXE + step;
		}
		private void ResetStepCounter()
		{
			step = 0;
			hud.stepLabel.Text = STEP_LABEL_PREFIXE + step;
		}
		#endregion

		#region // ----- Finished Level animation -----//

		private async void EndLevelAnimation(int pNumStar, int pScore, int pNumStep)
		{
			List<Node2D> lObjectsToAnimate= new List<Node2D>();
			lObjectsToAnimate.Clear(); 

			// Récupérer toutes les cellules existantes
			foreach(Node2D lObject in gameManager.objectsContainer.GetChildren())
			{
				lObjectsToAnimate.Add(lObject);

				if(lObject is BoxTesla lBoxTesla)
					lBoxTesla.LineDeconnection(); 

			}

			

			// Déterminer un point central (aspiration)
			Vector2 lVortexCenter = GetViewportRect().Size/2; 

			vortex = CreateVortex(lVortexCenter); 
			AddChild(vortex);

			// Mélanger aléatoirement pour rendre l'effet dynamique
			Random lRand = new Random();
			lObjectsToAnimate = lObjectsToAnimate.OrderBy(c => lRand.Next()).ToList();

			// Appliquer un effet progressif avec un délai variable
			float lBaseDelay = 0.02f; // Délai initial
			float lRandDelay; 
			for (int i = 0; i < lObjectsToAnimate.Count; i++)
			{
				lRandDelay = rand.Randf()* lBaseDelay; 
				Node2D lObject = lObjectsToAnimate[i];
				if (lObject == null) continue;

				// Effet d'électricité avant la disparition
				FlashElectricEffect(lObject);  


				// Déterminer une nouvelle position vers le vortex

				float lRandPropulsion = rand.Randf() * 1000; 

				if (lObject != null)
				{
					Vector2 lNewObjectPos = lObject.GlobalPosition.DirectionTo(lVortexCenter) * lRandPropulsion + lObject.GlobalPosition;
					Tween lTween = CreateTween();
					lTween.TweenProperty(lObject, ObjectProperties.POSITION, lNewObjectPos, 1f)
							.SetTrans(Tween.TransitionType.Elastic)
							.SetEase(Tween.EaseType.Out);
				}

				// Attendre un court moment avant d'animer la prochaine tuile
				await ToSignal(GetTree().CreateTimer(lRandDelay), ObjectProperties.TIME_OUT);
				// Augmente aléatoirement pour un effet chaotique
			}
			AnimateVortex(vortex); 

			foreach(Node2D lObject in lObjectsToAnimate)
			{
				Tween lTween = CreateTween(); 
				lTween.TweenProperty(lObject, ObjectProperties.POSITION, lVortexCenter, 1.3f)
					.SetTrans(Tween.TransitionType.Linear)
					.SetEase(Tween.EaseType.In); 

				lTween.Finished += ()=> lObject.Visible = false; 
			}

		}

		private Node2D CreateVortex(Vector2 pPosition)
		{
			Node2D lVortex = new Node2D();
			lVortex.GlobalPosition = pPosition;

			// Ajouter un Sprite pour le vortex
			Sprite2D lVortexSprite = new Sprite2D();
			lVortexSprite.Texture = GD.Load(VORTEX_PATH) as Texture2D; // Assurez-vous d’avoir une texture en spirale
			lVortexSprite.Modulate = new Color(1, 1, 1, 0); // Commence invisible
			lVortexSprite.Scale = Vector2.One * 0.1f; // Très petit au début
			lVortex.AddChild(lVortexSprite);

			return lVortex;
		}

		//  **Animation du vortex qui grossit et aspire tout**
		private void AnimateVortex(Node2D vortex)
		{
			Sprite2D lVortexSprite = vortex.GetChild<Sprite2D>(0);
			Tween lVortexTween = CreateTween();

			// Faire grossir le vortex
			lVortexTween.Parallel().TweenProperty(lVortexSprite, ObjectProperties.SCALE, Vector2.One, 0.8f)
					.SetTrans(Tween.TransitionType.Linear)
					.SetEase(Tween.EaseType.Out);

			// Augmenter l’opacité pour qu’il apparaisse
			lVortexTween.Parallel().TweenProperty(lVortexSprite, ObjectProperties.MODULATE, new Color(1, 1, 1, 1), 0.8f);

			// Rotation continue
			lVortexTween.Parallel().TweenProperty(lVortexSprite, ObjectProperties.ROTATION, Mathf.DegToRad(600), 1f)
					.SetTrans(Tween.TransitionType.Linear)
					.SetEase(Tween.EaseType.InOut);

			lVortexTween.TweenProperty(lVortexSprite, ObjectProperties.SCALE, Vector2.Zero, 0.8f)
					.SetTrans(Tween.TransitionType.Linear)
					.SetEase(Tween.EaseType.OutIn);

			lVortexTween.Finished += () => EndLevelAnimationFnished(); 
		}


		// Effet d’électricité (simulé avec un changement rapide de couleur)
		private void FlashElectricEffect(Node2D pObject)
		{
			WinScreenThunder lThunderEffect = thunderEffectScene.Instantiate() as WinScreenThunder;
			lThunderEffect.ZIndex = 45; 
			gameManager.objectsContainer.AddChild(lThunderEffect);

			lThunderEffect.ActiveThunder(pObject); 
		}

		private void EndLevelAnimationFnished()
		{
			vortex.QueueFree(); 
			CustomSignals.GetInstance().EmitSignal(CustomSignals.SignalName.EndLevelAnimation); 
		}


		#endregion



		#region // ----- Provisoir pour test ----- \\
		public void PrintGrid(Cell[,] pGrid)	//=================================> Provisoir pour test 
		{
			string lGridString = "";

			for (int y = 0; y < LevelLoader.levelHeight; y++)
			{
				for (int x = 0; x < LevelLoader.levelWidth; x++)
				{
					GameObject lContent = pGrid[x, y].GetContent();
					
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
						lGridString += "- ";  // Case vide
				}
				lGridString += "\n";  // Nouvelle ligne pour chaque rangée
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
