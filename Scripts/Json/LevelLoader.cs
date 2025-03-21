using System;
using Com.IsartDigital.SokoVolt.GameObjects;
using Com.IsartDigital.SokoVolt.GameObjects.Movables;
using Com.IsartDigital.SokoVolt.Managers;
using Godot;

//Author : Ferlat Thibaud 
namespace Com.IsartDigital.SokoVolt {
	
	public partial class LevelLoader : Node
	{
		[Export] private PackedScene cellScene, playerScene, boxScene, wallScene, electricWallScene, goalBulbScene, generatorScene, doorScene; 

		public static int  levelHeight{get; private set;}
		public static int levelWidth{get; private set;}
		public static bool playerCanMove = false;	

		public static int parCount{get; private set;}

		GridManager gridInstance; 



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
			IsoManager.Init(Utils.TILE_WIDTH, Utils.TILE_HEIGHT); 
		}

		public void LoadLevel(int pLevel, string pLevelPath, Node2D pObjectContainer)
		{
			string lJsonContent = JsonTool.ReadFileContents(pLevelPath);
            playerCanMove = false;

            if (!JsonTool.TryParseJson(lJsonContent, out Godot.Collections.Dictionary lRootDict))
			{
				GD.PrintErr("Erreur : Impossible de parser le fichier JSON.");
				return;
			}

			// Vérifier si le JSON contient les niveaux
			if (!lRootDict.ContainsKey(JsonKeys.LEVEL_DESIGN_KEY))
			{
				GD.PrintErr("Erreur : Pas de clé 'levelDesign' dans le JSON.");
				return;
			}

			Godot.Collections.Array lLevelList = (Godot.Collections.Array)lRootDict[JsonKeys.LEVEL_DESIGN_KEY]; 

			if (pLevel < 0 || pLevel >= lLevelList.Count)
			{
				GD.PrintErr($"Erreur : Index de niveau invalide ({pLevel}).");
				return;
			}

			// Récupérer le niveau sélectionné
			Godot.Collections.Dictionary lLevelData = (Godot.Collections.Dictionary)lLevelList[pLevel];

			// Récupérer la map
			Godot.Collections.Array lMapArray = (Godot.Collections.Array)lLevelData[JsonKeys.MAP_KEY];

			// Convertir la map en tableau de strings
			string[] lLevelMap = new string[lMapArray.Count];
			for (int i = 0; i < lMapArray.Count; i++)
			{
				lLevelMap[i] = lMapArray[i].ToString();
			}

			// Lire la portée des caisses Tesla (si elle est définie dans le JSON)
			Godot.Collections.Array lBoxRangesArray = lLevelData.ContainsKey(JsonKeys.BOX_RANGE_KEY) ? 
			(Godot.Collections.Array)lLevelData[JsonKeys.BOX_RANGE_KEY] : new Godot.Collections.Array();

			int lPar = -1; // Valeur par défaut en cas d'erreur

			if (lLevelData.ContainsKey(JsonKeys.PAR_KEY))
			{
				Variant lParVariant = lLevelData[JsonKeys.PAR_KEY];	
				lPar = int.Parse(lParVariant.ToString()); 
			}

			parCount = lPar;

			//BoxRange
			int[] lBoxRanges = new int[lBoxRangesArray.Count];
			// Convertir les valeurs en int
			for (int i = 0; i < lBoxRangesArray.Count; i++)
			{
				lBoxRanges[i] = (int)lBoxRangesArray[i];
			}


			levelWidth = lMapArray.Count > 0 ? lMapArray[0].ToString().Length : 0;
			levelHeight = lMapArray.Count; 

			// Redimensionner la grille dynamiquement
			gridInstance.SetNewLevel(new Cell[levelWidth, levelHeight]);

			gridInstance.CenterGrid(); 

			GD.Print($"Chargement du niveau {pLevel} - Taille : {levelWidth}x{levelHeight}");

			int lBoxIndex = 0; 

			// Charger le niveau
			for (int y = 0; y < levelHeight; y++)
			{
				string lRow = lMapArray[y].ToString();

				for (int x = 0; x < levelWidth; x++)
				{
					if (x >= lRow.Length) continue; // Évite les erreurs si une ligne est plus courte

					char lTile = lRow[x];

					Cell lCell = Utils.Spawner(cellScene, x, y, pObjectContainer) as Cell;
					gridInstance.grid[x, y] = lCell;

					GameObject lObj = null; 

					switch(lTile)
					{
						case JsonKeys.PLAYER :
							lObj = Utils.Spawner(playerScene, x, y, pObjectContainer) as Player;
							GridManager.GetInstance().player = lObj as Player;
							break;

						case JsonKeys.BOX :
							lObj = Utils.Spawner(boxScene, x, y, pObjectContainer) as BoxTesla;

							// Vérifier qu'on a une portée disponible et l'appliquer
							if (lBoxIndex < lBoxRanges.Length && lObj != null)
							{
								((BoxTesla)lObj).SetRange(lBoxRanges[lBoxIndex]); // Assigner la portée
								lBoxIndex++; // Passer à la portée suivante
							}
							else
							{
								GD.PrintErr($"Aucune portée définie pour la BoxTesla à ({x},{y}) !");
							}
							break;

						case JsonKeys.WALL :
							lObj = Utils.Spawner(wallScene, x, y, pObjectContainer) as Wall;
							break;

						case JsonKeys.ELECTRIC_WALL :
							lObj = Utils.Spawner(electricWallScene, x, y, pObjectContainer) as ElectricWall;
							break;

						case JsonKeys.GOAL_BULB :
							lObj = Utils.Spawner(goalBulbScene, x, y, pObjectContainer) as GoalBulb;
							break;

						case JsonKeys.GENERATOR :
							lObj = Utils.Spawner(generatorScene, x, y, pObjectContainer) as Generator;
							break;

						case JsonKeys.DOOR :
							lObj = Utils.Spawner(doorScene, x, y, pObjectContainer) as Door;
							break;
					}

					if (lObj != null)
					{
						if (lCell == null)
						{
							GD.PrintErr($"Erreur: lCell est null à la position ({x}, {y}) !");
							return; // Évite l'erreur en quittant la fonction
						}
						lCell.SetContent(lObj);
						lObj.SetCell(lCell);
						lObj.Init(x, y);
						if(!(lObj is Player))lObj.ZIndex = IsoManager.GetZIndex(new Vector2(x,y));
						else lObj.ZIndex = 1000;
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
