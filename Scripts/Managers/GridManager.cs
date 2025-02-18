using Godot;
using System;
using Com.IsartDigital.SokoVolt.GameObjects;
using Com.IsartDigital.SokoVolt.GameObjects.Movables;

//Author : Ferlat Thibaud 
namespace Com.IsartDigital.SokoVolt.Managers {
	
	public partial class GridManager : Manager
	{
		#region GetInstance
		static private GridManager instance;
		
		static public GridManager GetInstance () {
			if (instance == null) instance = new GridManager();
			return instance;
		}

		private GridManager ():base() {}
		#endregion

		public Cell[,] grid { get; private set;}
		public static Vector2 gridOffset; 
		public Player player;

		//Step Counter 
		private const string STEP_LABEL_PREFIXE = "STEP : "; 
		[Export] private Label stepLabel; 
		private int step = 0; 


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

			LevelManager.GetInstance().LoadLevel += () => LoadNewLevel(4);

            InputManager.GetInstance().Move += OnMovePlayer;
        }

        public override void _Process(double pDelta)
		{

        }

        public override void Init()
        {
            base.Init();
        }

		public void LoadNewLevel(int pLevelToLoad) // ==================> Charger un niveau avec son index (commence à 0)
		{
			LevelLoader.GetInstance().LoadLevel(pLevelToLoad);
			CenterGrid(); 
		}

		public void SetNewLevel(Cell[,] pNewGrid)
		{
			grid = pNewGrid;
		}

		public void CenterGrid()
		{
			Vector2 lScreenSize = GetViewportRect().Size; 
			float lGridWidth = LevelLoader.levelWidth * Utils.TILE_SIZE - Utils.TILE_SIZE;  
			float lGridHeight = LevelLoader.levelHeight * Utils.TILE_SIZE - Utils.TILE_SIZE; 

			gridOffset = new Vector2
			(
				(lScreenSize.X - lGridWidth) / 2,
				(lScreenSize.Y - lGridHeight) / 2
			);
		}

		public void OnMovePlayer(Vector2 pPlayerDirection)
		{
			MovePlayer((int)pPlayerDirection.X, (int)pPlayerDirection.Y);
		}

        private void MovePlayer(int pDx, int pDy)
		{
            int lNewX = player.x + pDx;
			int lNewY = player.y + pDy;

			if(OutOfGrid(lNewX, lNewY))
				return;
			
			Cell lNewCell = grid[lNewX, lNewY];
			GameObject lContent = lNewCell.GetContent();

			if (lContent == null || lContent is Door)
			{
				player.MoveTo(lNewX, lNewY, grid);
				UpdateStepLabel();
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
					UpdateStepLabel();
				}
			}
			else return;

			PrintGrid();
		}

		private bool OutOfGrid(int pX, int pY)
		{
			return pX < 0 || pX >= LevelLoader.levelWidth || pY < 0 || pY >= LevelLoader.levelHeight;
		}

		private void UpdateStepLabel()
		{
			step++;
			stepLabel.Text = STEP_LABEL_PREFIXE + step;
		}
	
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


		#region dispose
		protected override void Dispose(bool pDisposing)
		{
			if (pDisposing && instance == this) instance = null;
			base.Dispose(pDisposing);
		}
		#endregion
	}
}
