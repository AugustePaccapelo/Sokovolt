using System;
using Com.IsartDigital.Sokovolt;
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
				GD.PrintErr("Error : Failed to parse JSON.");
				return;
			}

			// Check if the json contain lvls 
			if (!lRootDict.ContainsKey(JsonKeys.LEVEL_DESIGN_KEY))
			{
				GD.PrintErr("Error : JSON does not contain level design data.");
				return;
			}

			Godot.Collections.Array lLevelList = (Godot.Collections.Array)lRootDict[JsonKeys.LEVEL_DESIGN_KEY]; 

			if (pLevel < 0 || pLevel >= lLevelList.Count)
			{
				GD.PrintErr($"Error : Level {pLevel} is out of range.");
				return;
			}

			// Peek the selected lvl 
			Godot.Collections.Dictionary lLevelData = (Godot.Collections.Dictionary)lLevelList[pLevel];

			// Peek the map 
			Godot.Collections.Array lMapArray = (Godot.Collections.Array)lLevelData[JsonKeys.MAP_KEY];

			// Convert map into string array
			string[] lLevelMap = new string[lMapArray.Count];
			for (int i = 0; i < lMapArray.Count; i++)
			{
				lLevelMap[i] = lMapArray[i].ToString();
			}

			// Read tesla range 
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
			// Convert values to array 
			for (int i = 0; i < lBoxRangesArray.Count; i++)
			{
				lBoxRanges[i] = (int)lBoxRangesArray[i];
			}


			levelWidth = lMapArray.Count > 0 ? lMapArray[0].ToString().Length : 0;
			levelHeight = lMapArray.Count; 

			// Set grid size 
			gridInstance.SetNewLevel(new Cell[levelWidth, levelHeight]);

			gridInstance.CenterGrid(); 

			GD.Print($"Load level {pLevel} - Size : {levelWidth}x{levelHeight}");

			int lBoxIndex = 0; 

			// Load lvl 
			for (int y = 0; y < levelHeight; y++)
			{
				string lRow = lMapArray[y].ToString();

				for (int x = 0; x < levelWidth; x++)
				{
					if (x >= lRow.Length) continue; // Avoid errors for shorter lines 

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

							// Check existing range 
							if (lBoxIndex < lBoxRanges.Length && lObj != null)
							{
								((BoxTesla)lObj).SetRange(lBoxRanges[lBoxIndex]); // Asign range
								lBoxIndex++; // Pass to next range
							}
							else
							{
								GD.PrintErr($"No range for the box tesla at ({x},{y}) !");
							}
							break;

						//Spawn objects 
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
					WinScreen.actualLevel = pLevel;

					if (lObj != null)
					{
						if (lCell == null)
						{
							GD.PrintErr($"Error: cell is null at position ({x}, {y}) !");
							return; // Avoid null error 
						}
						lCell.SetContent(lObj);
						lObj.SetCell(lCell);
						lObj.Init(x, y);
						
						//Set Iso ZIndex for obj 
						if(!(lObj is Player))lObj.ZIndex = IsoManager.GetZIndex(new Vector2(x,y));
						// Set up the player ZIndex 
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
