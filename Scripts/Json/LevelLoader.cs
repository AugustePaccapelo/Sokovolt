using Com.IsartDigital.SokoVolt.GameObjects;
using Com.IsartDigital.SokoVolt.GameObjects.Movables;
using Com.IsartDigital.SokoVolt.Managers;
using Godot;

//Author : Ferlat Thibaud 
namespace Com.IsartDigital.SokoVolt {
	
	public partial class LevelLoader : Node
	{
		[Export] private PackedScene cellScene, playerScene, boxScene, wallScene, electricWallScene, goalBulbScene, generatorScene, doorScene; 
		[Export] private Node2D objectsContainer;  

		//Keys 
		private const string LEVEL_DESIGN_KEY = "levelDesign"; 
		private const string MAP_KEY = "map"; 
		private const string LEVELS_JSONS_PATH = "res://Scripts/Json/Levels.json"; 

		GridManager gridInstance; 

		public static int  levelHeight{get; private set;}
		public static int levelWidth{get; private set;}



		#region GetInstance
		static private LevelLoader instance;
		
		static public LevelLoader GetInstance () {
			if (instance == null) instance = new LevelLoader();
			return instance;
		}

		private LevelLoader ():base() {}
		#endregion

		public override void _Ready()
		{
			#region instance
		if (instance != null){  
			QueueFree();
			GD.Print(nameof(LevelLoader) + " Instance already exist, destroying the last added.");
			return;
		}
		instance = this;
		#endregion
			Init(); 
		}

		private void Init()
		{
			gridInstance = GridManager.GetInstance(); 
		}

		public void LoadLevel(int pLevel)
		{
			string lJsonContent = JsonTool.ReadFileContents(LEVELS_JSONS_PATH);

			if (!JsonTool.TryParseJson(lJsonContent, out Godot.Collections.Dictionary lRootDict))
			{
				GD.PrintErr("Erreur : Impossible de parser le fichier JSON.");
				return;
			}

			// Vérifier si le JSON contient les niveaux
			if (!lRootDict.ContainsKey(LEVEL_DESIGN_KEY))
			{
				GD.PrintErr("Erreur : Pas de clé 'levelDesign' dans le JSON.");
				return;
			}

			Godot.Collections.Array lLevelList = (Godot.Collections.Array)lRootDict[LEVEL_DESIGN_KEY]; 

			if (pLevel < 0 || pLevel >= lLevelList.Count)
			{
				GD.PrintErr($"Erreur : Index de niveau invalide ({pLevel}).");
				return;
			}

			// Récupérer le niveau sélectionné
			Godot.Collections.Dictionary lLevelData = (Godot.Collections.Dictionary)lLevelList[pLevel];

			// Récupérer la map
			Godot.Collections.Array lMapArray = (Godot.Collections.Array)lLevelData[MAP_KEY];

			// Convertir la map en tableau de strings
			string[] lLevelMap = new string[lMapArray.Count];
			for (int i = 0; i < lMapArray.Count; i++)
			{
				lLevelMap[i] = lMapArray[i].ToString();
			}


			levelWidth = lMapArray.Count > 0 ? lMapArray[0].ToString().Length : 0;
			levelHeight = lMapArray.Count;

			// Redimensionner la grille dynamiquement
			gridInstance.SetNewLevel(new Cell[levelWidth, levelHeight]);

			gridInstance.CenterGrid(); 

			GD.Print($"Chargement du niveau {pLevel} - Taille : {levelWidth}x{levelHeight}");

			// Charger le niveau
			for (int y = 0; y < levelHeight; y++)
			{
				string lRow = lMapArray[y].ToString();

				for (int x = 0; x < levelWidth; x++)
				{
					if (x >= lRow.Length) continue; // Évite les erreurs si une ligne est plus courte

					char lTile = lRow[x];

					Cell lCell = Utils.Spawner(cellScene, x, y, objectsContainer) as Cell;
					gridInstance.grid[x, y] = lCell;

					GameObject lObj = null; 

					switch(lTile)
					{
						case '@':
							lObj = Utils.Spawner(playerScene, x, y, objectsContainer) as Player;
							GridManager.GetInstance().player = lObj as Player;
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

					if (lObj != null)
					{
						lCell.SetContent(lObj);
						lObj.SetCell(lCell);
						lObj.Init(x, y);
					}
				}
			}
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
