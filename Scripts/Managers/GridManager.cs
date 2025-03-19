using Godot;
using System;
using Com.IsartDigital.SokoVolt.GameObjects;
using Com.IsartDigital.SokoVolt.GameObjects.Movables;
using System.Collections.Generic;
using Com.IsartDigital.ProjectName;
using System.Data;
using System.Linq;

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
			lSignal.LoadLevel  += (level) => LoadNewLevel(level, JsonKeys.LEVELS_JSONS_PATH, objectsContainer);
            lSignal.Move += OnMovePlayer;
            lSignal.UndoRedo += UndoRedo;
            lSignal.Retry += Retry;

			lSignal.UndoButton += () => UndoRedo(-1);
			lSignal.RedoButton += () => UndoRedo(1);
		}



		#region // ----- Load Level ----- \\

		public void LoadNewLevel(int pLevelToLoad, string pLevelPath, Node2D pObjectContainer) // ==================> Charger un niveau avec son index (commence à 0)
		{
			ResetStepCounter();
			hud.Visible = true;
			LevelLoader.GetInstance().LoadLevel(pLevelToLoad, pLevelPath, pObjectContainer);
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

			PrintGrid();
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
			PrintGrid();
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

		Node2D vortex; 

		private async void EndLevelAnimation(int pNumStar, int pScore, int pNumStep)
		{
			List<Cell> cells = new List<Cell>();

			// Récupérer toutes les cellules existantes
			for (int y = 0; y < LevelLoader.levelHeight; y++)
			{
				for (int x = 0; x < LevelLoader.levelWidth; x++)
				{
					if (grid[x, y] != null)
						cells.Add(grid[x, y]);
				}
			}

			

			// Déterminer un point central (aspiration)
			Vector2 vortexCenter = GetViewportRect().Size/2; 

			vortex = CreateVortex(vortexCenter); 
			gameManager.objectsContainer.AddChild(vortex);

			// Mélanger aléatoirement pour rendre l'effet dynamique
			Random lRand = new Random();
			cells = cells.OrderBy(c => lRand.Next()).ToList();

			// Appliquer un effet progressif avec un délai variable
			float lBaseDelay = 0.1f; // Délai initial
			float lRandDelay; 
			for (int i = 0; i < cells.Count; i++)
			{
				lRandDelay = rand.Randf()* lBaseDelay; 
				Cell lCell = cells[i];
				if (lCell == null) continue;

				GameObject lContent = lCell.GetContent();

				// Effet d'électricité avant la disparition
				FlashElectricEffect(lCell);  


				// Déterminer une nouvelle position vers le vortex

				float lRandPropulsion = rand.Randf() * 1000; 
				Vector2 lNewCellPos = lCell.GlobalPosition.DirectionTo(vortexCenter) * lRandPropulsion + lCell.GlobalPosition;
				Tween lTween = CreateTween();
				lTween.TweenProperty(lCell, "position", lNewCellPos, 0.6f)
					.SetTrans(Tween.TransitionType.Elastic)
					.SetEase(Tween.EaseType.Out);

				if (lContent != null)
				{
					Vector2 lNewContentPos = lContent.GlobalPosition.DirectionTo(vortexCenter) * lRandPropulsion + lContent.GlobalPosition;
					Tween lTweenTwo = CreateTween();
					lTweenTwo.TweenProperty(lContent, "position", lNewContentPos, 0.6f)
							.SetTrans(Tween.TransitionType.Elastic)
							.SetEase(Tween.EaseType.Out);
				}

				// Attendre un court moment avant d'animer la prochaine tuile
				await ToSignal(GetTree().CreateTimer(lRandDelay), "timeout");
				// Augmente aléatoirement pour un effet chaotique
			}
			AnimateVortex(vortex); 
		}

		private Node2D CreateVortex(Vector2 position)
		{
			Node2D vortex = new Node2D();
			vortex.Position = position;

			// Ajouter un Sprite pour le vortex
			Sprite2D vortexSprite = new Sprite2D();
			vortexSprite.Texture = GD.Load("res://Assets/GameObjects/LevelAnimation/vecteezy_spiral-vortex-element_27720416.png") as Texture2D; // Assurez-vous d’avoir une texture en spirale
			vortexSprite.Modulate = new Color(1, 1, 1, 0); // Commence invisible
			vortexSprite.Scale = new Vector2(0.1f, 0.1f); // Très petit au début
			vortex.AddChild(vortexSprite);

			return vortex;
		}

		//  **Animation du vortex qui grossit et aspire tout**
		private void AnimateVortex(Node2D vortex)
		{
			Sprite2D vortexSprite = vortex.GetChild<Sprite2D>(0);
			Tween vortexTween = CreateTween();

			// Faire grossir le vortex
			vortexTween.Parallel().TweenProperty(vortexSprite, "scale", Vector2.One, 2f)
					.SetTrans(Tween.TransitionType.Linear)
					.SetEase(Tween.EaseType.Out);

			// Augmenter l’opacité pour qu’il apparaisse
			vortexTween.Parallel().TweenProperty(vortexSprite, "modulate", new Color(1, 1, 1, 1), 0.8f);

			// Rotation continue
			vortexTween.Parallel().TweenProperty(vortexSprite, "rotation", Mathf.DegToRad(600), 2f)
					.SetTrans(Tween.TransitionType.Linear)
					.SetEase(Tween.EaseType.InOut);
		}

		// Méthode appelée par le Tween pour faire tourner le vortex
		private void RotateVortex(float angle)
		{
			vortex.GlobalRotation = Mathf.DegToRad(angle);
		}

		// Effet d’électricité (simulé avec un changement rapide de couleur)
		private void FlashElectricEffect(Cell cell)
		{
			WinScreenThunder lThunderEffect = thunderEffectScene.Instantiate() as WinScreenThunder; 
			gameManager.objectsContainer.AddChild(lThunderEffect);

			Color flashColor = new Color(1, 1, 0.5f); // Jaune électrique
			Color originalColor = cell.Modulate;

			Tween flashTween = CreateTween();
			flashTween.TweenProperty(cell, "modulate", flashColor, 0.1f)
					.SetTrans(Tween.TransitionType.Sine)
					.SetEase(Tween.EaseType.InOut);
			flashTween.TweenProperty(cell, "modulate", originalColor, 0.1f);

			lThunderEffect.ActiveThunder(cell); 

		}


		#endregion



		#region // ----- Provisoir pour test ----- \\
		private void PrintGrid()	//=================================> Provisoir pour test 
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
