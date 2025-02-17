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
		
		[Export] private PackedScene cellScene, playerScene, boxScene, wallScene, electricWallScene, goalBulbScene, generatorScene, doorScene; 

		[Export] private Node2D objectsContainer;

		[Export] private Label stepLabel;

		private int step = 0;

 
		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

		private const int GRID_WIDTH = 9; // ==================================> A mettre dans le Json du level loader
		private const int GRID_HEIGHT = 7;
		private const string STEP_LABEL_PREFIXE = "STEP : ";

		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

		public Cell[,] grid { get; private set; } = new Cell[GRID_WIDTH, GRID_HEIGHT];
		public static Vector2 gridOffset; 
		private Player player;


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
			LevelManager.GetInstance().LoadLevel += LoadLevel;
		}

		public override void _Process(double pDelta)
		{
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

			if (Input.IsActionJustPressed("ui_right")) MovePlayer(1, 0);
			if (Input.IsActionJustPressed("ui_left")) MovePlayer(-1, 0); //=================================> Besoin d'un input manager 
			if (Input.IsActionJustPressed("ui_down")) MovePlayer(0, 1);
			if (Input.IsActionJustPressed("ui_up")) MovePlayer(0, -1);

			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
		}

		public void LoadLevel() // ====================================> A basculer dans un LevelLoader
		{
			CenterGrid();
			stepLabel.Show();

			string [] lTestLevel = 
			{
				"#########",
				"#/    # #",
				"# . $   #",
				"#   @ * #",
				"#   $ . #",
				"#  #   |#",
				"#########"
			};

			for(int y = 0; y < GRID_HEIGHT; y++)
			{
				for(int x = 0; x < GRID_WIDTH; x++)
				{
					char lTile = lTestLevel[y][x];

					Cell lCell = Utils.Spawner(cellScene, x, y, objectsContainer) as Cell;

					grid[x, y] = lCell;

					GameObject lObj = null; 

					switch(lTile)
					{
						case '@':
							lObj = Utils.Spawner(playerScene, x, y, objectsContainer) as Player;
							player = lObj as Player;
							break;
						case '$':
							lObj = Utils.Spawner(boxScene, x, y, objectsContainer) as BoxTesla;
							break;
						case '#':
							lObj = Utils.Spawner(wallScene, x, y, objectsContainer) as Wall;
							break;
						case '*':
							lObj = Utils.Spawner(electricWallScene, x, y, objectsContainer) as ElectricWall;
							break;
						case '.':
							lObj = Utils.Spawner(goalBulbScene, x, y, objectsContainer) as GoalBulb;
							break;
						case '/':
							lObj = Utils.Spawner(generatorScene, x, y, objectsContainer) as Generator;
							break;
						case '|':
							lObj = Utils.Spawner(doorScene, x, y, objectsContainer) as Door;
							break;
					}

					if(lObj != null)
					{
						lCell.SetContent(lObj);
						lObj.SetCell(lCell);
						lObj.Init(x, y);
					}
				}
			}

			PrintGrid();
				
		}

		public void CenterGrid()
		{
			Vector2 lScreenSize = GetViewportRect().Size; 
			float lGridWidth = GRID_WIDTH * Utils.TILE_SIZE - Utils.TILE_SIZE;  
			float lGridHeight = GRID_HEIGHT * Utils.TILE_SIZE - Utils.TILE_SIZE; 

			gridOffset = new Vector2
			(
				(lScreenSize.X - lGridWidth) / 2,
				(lScreenSize.Y - lGridHeight) / 2
			);
		}


		private void MovePlayer(int pDx, int pDy)
		{
			int lNewX = player.x + pDx;
			int lNewY = player.y + pDy;

			if (lNewX < 0 || lNewX >= GRID_WIDTH || lNewY < 0 || lNewY >= GRID_HEIGHT)
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

				if (lNewBoxX < 0 || lNewBoxX >= GRID_WIDTH || lNewBoxY < 0 || lNewBoxY >= GRID_HEIGHT)
					return;

				Cell lNewBoxCell = grid[lNewBoxX, lNewBoxY];
				if (lNewBoxCell.GetContent() == null)
				{
					lBox.MoveTo(lNewBoxX, lNewBoxY, grid);
					player.MoveTo(lNewX, lNewY, grid);
					UpdateStepLabel();
				}
			}

			PrintGrid();
		}

		private void UpdateStepLabel()
		{
			step++;
			stepLabel.Text = STEP_LABEL_PREFIXE + step;
		}

		private void PrintGrid()	//=================================> Provisoir pour test 
		{
			string lGridString = "";

			for (int y = 0; y < GRID_HEIGHT; y++)
			{
				for (int x = 0; x < GRID_WIDTH; x++)
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
