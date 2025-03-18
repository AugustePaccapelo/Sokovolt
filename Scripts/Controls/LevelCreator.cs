using Com.IsartDigital.SokoVolt.GameObjects;
using Com.IsartDigital.SokoVolt.Managers;
using Godot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;

// Author : Noé Sales

namespace Com.IsartDigital.SokoVolt
{

	public partial class LevelCreator : Control
	{

		#region Singleton
		static private LevelCreator instance;

		private LevelCreator() { }

		static public LevelCreator GetInstance()
		{
			if (instance == null) instance = new LevelCreator();
			return instance;

		}
        #endregion

        [Export] private Button mainMenuButton, newLevelButton, loadLevelButton, menuCustomLevelButton, returnButton, saveButton;
        [Export] private TextEdit loadLevelText, levelName;
        [Export] private LevelCreatorItems wallTexture, electricWallTexture, teslaTexture, bulbTexture, generatorTexture, playerSpawnTexture, doorTexture;
        [Export] private PackedScene wallScene, electricWallScene, teslaScene, bulbScene, generatorScene, playerSpawnScene, doorScene, tileScene, customLevelLabelScene;
        [Export] private VBoxContainer buttonContainer, deleteButtonContainer, labelContainer;
        [Export] private Json customLevelTemplate;
        private Panel newLevelBackground, loadLevelBackground, customLevelMenuBackground, backGrid;
        private LevelCreatorItems actualItem;
		private bool canPick = false;
        private TextureRect hoveredItem;
        private float tileSize = 50;
        private float space = 5;
        private Node2D cellContainer;

        private const string TESLA_NAME = "teslaTexture";
        private const string WALL_NAME = "wallTexture";
        private const string GENERATOR_NAME = "generatorTexture";
        private const string BULB_NAME = "bulbTexture";
        private const string PLAYERSPAWN_NAME = "playerSpawnTexture";
        private const string DOOR_NAME = "doorTexture";
        private const string ELECTRIC_WALL_NAME = "electricWallTexture";

        private const int LENGHT = 11;

        Dictionary<Vector2, LevelCreatorTile> gridDico = new Dictionary<Vector2, LevelCreatorTile>();

        List<Vector2[]> gridList = new List<Vector2[]>();

		public override void _Ready()
		{
			#region Singleton Ready
			if (instance != null)
			{
				QueueFree();
				GD.Print(nameof(LevelCreator) + " Instance already exist, destroying the last added.");
				return;
			}

			instance = this;
			#endregion

			mainMenuButton.Pressed += () => {
                HUD.GetInstance().Show();
                CustomSignals.GetInstance().EmitSignal(CustomSignals.SignalName.GoToMainMenu);
                };
            newLevelButton.Pressed += CreateNewLevel;
            loadLevelButton.Pressed += () => LoadLevel(loadLevelText.Text);
            menuCustomLevelButton.Pressed += OpenCustomLevelsMenu;
            returnButton.Pressed += Return;
            saveButton.Pressed += CreateJSON;

            returnButton.Hide();

            #region Mouse & Item signal Connection
            wallTexture.MouseEntered += () =>
			{
				canPick = true;
				hoveredItem = wallTexture;
            };
            wallTexture.MouseExited += () =>
            {
                canPick = false;
                hoveredItem = null;
            };

            teslaTexture.MouseEntered += () =>
            {
                canPick = true;
                hoveredItem = teslaTexture;
            };
            teslaTexture.MouseExited += () =>
            {
                canPick = false;
                hoveredItem = null;
            };

            bulbTexture.MouseEntered += () =>
            {
                canPick = true;
                hoveredItem = bulbTexture;
            };
            bulbTexture.MouseExited += () =>
            {
                canPick = false;
                hoveredItem = null;
            };

            generatorTexture.MouseEntered += () =>
            {
                canPick = true;
                hoveredItem = generatorTexture;
            };
            generatorTexture.MouseExited += () =>
            {
                canPick = false;
                hoveredItem = null;
            };

            doorTexture.MouseEntered += () =>
            {
                canPick = true;
                hoveredItem = doorTexture;
            };
            doorTexture.MouseExited += () =>
            {
                canPick = false;
                hoveredItem = null;
            };

            playerSpawnTexture.MouseEntered += () =>
            {
                canPick = true;
                hoveredItem = playerSpawnTexture;
            };
            playerSpawnTexture.MouseExited += () =>
            {
                canPick = false;
                hoveredItem = null;
            };

            electricWallTexture.MouseEntered += () =>
            {
                canPick = true;
                hoveredItem = electricWallTexture;
            };
            electricWallTexture.MouseExited += () =>
            {
                canPick = false;
                hoveredItem = null;
            };

            #endregion

            newLevelBackground = GetNode<Panel>("NewLevelBackGround");
            loadLevelBackground = GetNode<Panel>("LoadLevelBackGround");
            cellContainer = GetNode<Node2D>("CellContainer");
            customLevelMenuBackground = GetNode<Panel>("CustomLevelListBackGround");
            backGrid = newLevelBackground.GetNode<Panel>("BackGrid");

            newLevelBackground.Visible = loadLevelBackground.Visible = customLevelMenuBackground.Visible = returnButton.Visible = false;

            //OpenCustomLevelsMenu();

        }

		public override void _Process(double pDelta)
		{
			float lDelta = (float)pDelta;
			MouseOn();
			PlaceItem();
            if (actualItem != null) actualItem.Position = GetLocalMousePosition();
		}

        private void CreateJSON()
        {
            string lFileName = "res://Scripts/Json/CustomLevels/" + levelName.Text + ".json";

            if (!FileAccess.FileExists(lFileName) && levelName.Text.Length > 0)
            {
                string[] lMap = new string[LENGHT];
                List<int> boxRange = new List<int>();

                bool hasDoor = false;
                bool hasPlayerSpawn = false;
                bool hasGenerator = false;
                bool hasBulb = false;

                for (int y = 0; y < LENGHT; y++)
                {
                    string row = "";
                    for (int x = 0; x < LENGHT; x++)
                    {
                        Vector2 cellIndex = new Vector2(x, y);

                        if (gridDico.TryGetValue(cellIndex, out LevelCreatorTile lTile) && lTile.content != null)
                        {
                            switch (lTile.content.Name)
                            {
                                case WALL_NAME:
                                    row += JsonKeys.WALL;
                                    break;

                                case TESLA_NAME:
                                    row += JsonKeys.BOX;
                                    if (lTile.content.teslaRange != null)
                                    {
                                        boxRange.Add((int)lTile.content.teslaRange.Value);
                                    }
                                    break;

                                case BULB_NAME:
                                    row += JsonKeys.GOAL_BULB;
                                    hasBulb = true;
                                    break;

                                case GENERATOR_NAME:
                                    row += JsonKeys.GENERATOR;
                                    hasGenerator = true;
                                    break;

                                case PLAYERSPAWN_NAME:
                                    row += JsonKeys.PLAYER;
                                    hasPlayerSpawn = true;
                                    break;

                                case DOOR_NAME:
                                    row += JsonKeys.DOOR;
                                    hasDoor = true;
                                    break;

                                case ELECTRIC_WALL_NAME:
                                    row += JsonKeys.ELECTRIC_WALL;
                                    break;

                                default:
                                    row += ' ';
                                    break;
                            }
                        }
                        else
                        {
                            row += ' ';
                        }
                    }
                    lMap[y] = row;
                }

                // Vérification des éléments obligatoires
                if (!hasDoor || !hasPlayerSpawn || !hasGenerator || !hasBulb)
                {
                    GD.PrintErr("Éléments manquants : une porte, un playerspawn, un générateur et au moins une ampoule sont requis.");
                    return;
                }

                // Construction du JSON final
                string lJson = "{\n" +
                    $"  \"{JsonKeys.LEVEL_DESIGN_KEY}\": [\n" +
                    "    {\n" +
                    $"      \"{JsonKeys.PAR_KEY}\": 0,\n" +
                    $"      \"{JsonKeys.MAP_KEY}\": [\n";

                for (int i = 0; i < lMap.Length; i++)
                {
                    lJson += $"        \"{lMap[i]}\"";
                    if (i < lMap.Length - 1) lJson += ",";
                    lJson += "\n";
                }

                lJson += $"      ],\n" +
                    $"      \"{JsonKeys.BOX_RANGE_KEY}\": [";

                // Écriture des valeurs de boxRange
                for (int i = 0; i < boxRange.Count; i++)
                {
                    lJson += boxRange[i];
                    if (i < boxRange.Count - 1) lJson += ", ";
                }

                lJson += $"]\n" +
                    "    }\n" +
                    "  ]\n" +
                    "}";

                using FileAccess lCreateFile = FileAccess.Open(lFileName, FileAccess.ModeFlags.Write);
                lCreateFile.StoreString(lJson);

                GD.Print("File created successfully: " + lFileName);
            }
            else
            {
                GD.PrintErr("File already exists or name is empty.");
            }
        }


        private void MouseOn()
		{
			if (Input.IsMouseButtonPressed(MouseButton.Left) && canPick)
			{
				actualItem?.QueueFree();
                LevelCreatorItems lItem = new LevelCreatorItems();

                if (hoveredItem == wallTexture)
                {
                    lItem = wallScene.Instantiate() as LevelCreatorItems;
                    lItem.Name = WALL_NAME;
                }
                else if (hoveredItem == teslaTexture)
                {
                    lItem = teslaScene.Instantiate() as LevelCreatorItems;
                    if (lItem.teslaRange != null) lItem.teslaRange.Value = teslaTexture.teslaRange.Value;
                    lItem.Name = TESLA_NAME;
                }
                else if (hoveredItem == bulbTexture)
                {
                    lItem = bulbScene.Instantiate() as LevelCreatorItems;
                    lItem.Name = BULB_NAME;
                }
                else if (hoveredItem == generatorTexture)
                {
                    lItem = generatorScene.Instantiate() as LevelCreatorItems;
                    lItem.Name = GENERATOR_NAME;
                }
                else if (hoveredItem == playerSpawnTexture)
                {
                    lItem = playerSpawnScene.Instantiate() as LevelCreatorItems;
                    lItem.Name = PLAYERSPAWN_NAME;
                }
                else if (hoveredItem == electricWallTexture)
                {
                    lItem = electricWallScene.Instantiate() as LevelCreatorItems;
                    lItem.Name = ELECTRIC_WALL_NAME;
                }
                else if (hoveredItem == doorTexture)
                {
                    lItem = doorScene.Instantiate() as LevelCreatorItems;
                    lItem.Name = DOOR_NAME;
                }

                cellContainer.AddChild(lItem);
                actualItem = lItem;
                canPick = false;
            }

            if (Input.IsMouseButtonPressed(MouseButton.Right) && GetGridIndexFromMousePos() != new Vector2(-1, -1))
            {
                Vector2 gridIndex = GetGridIndexFromMousePos();

                if (gridDico.TryGetValue(gridIndex, out LevelCreatorTile lTile) && lTile.content != null)
                {
                    lTile.content.QueueFree();
                    lTile.content = null;

                    // Force la mise à jour du dictionnaire
                    gridDico[gridIndex] = lTile;
                }
            }

        }

        private Vector2 PixelToGrid(Vector2 position)
        {
            float x = (position.X - backGrid.GlobalPosition.X) / (tileSize + space);
            float y = (position.Y - backGrid.GlobalPosition.Y) / (tileSize + space);

            return new Vector2(Mathf.FloorToInt(x), Mathf.FloorToInt(y));
        }

        private Vector2 GetGridIndexFromMousePos()
        {
            Vector2 mousePos = GetLocalMousePosition();

            float gridX = (mousePos.X - backGrid.GlobalPosition.X) / (tileSize + space);
            float gridY = (mousePos.Y - backGrid.GlobalPosition.Y) / (tileSize + space);

            int x = Mathf.FloorToInt(gridX);
            int y = Mathf.FloorToInt(gridY);

            if (x < 0 || y < 0 || x >= LENGHT || y >= LENGHT)
            {
                return new Vector2(-1, -1);
            }

            return new Vector2(x, y);
        }


        private void PlaceItem()
        {
            Vector2 gridIndex = GetGridIndexFromMousePos();

            if (gridIndex != new Vector2(-1, -1) && actualItem != null && Input.IsMouseButtonPressed(MouseButton.Left))
            {
                if (gridDico.TryGetValue(gridIndex, out LevelCreatorTile lTile))
                {
                    if (lTile.content == null)
                    {
                        string itemType = actualItem.Name;

                        // Crée une nouvelle instance propre de l'objet
                        LevelCreatorItems newItem = null;

                        switch (itemType)
                        {
                            case WALL_NAME:
                                newItem = wallScene.Instantiate<LevelCreatorItems>();
                                break;
                            case TESLA_NAME:
                                newItem = teslaScene.Instantiate<LevelCreatorItems>();
                                if (newItem.teslaRange != null)
                                    newItem.teslaRange.Value = teslaTexture.teslaRange.Value;
                                break;
                            case BULB_NAME:
                                newItem = bulbScene.Instantiate<LevelCreatorItems>();
                                break;
                            case GENERATOR_NAME:
                                newItem = generatorScene.Instantiate<LevelCreatorItems>();
                                break;
                            case PLAYERSPAWN_NAME:
                                newItem = playerSpawnScene.Instantiate<LevelCreatorItems>();
                                break;
                            case DOOR_NAME:
                                newItem = doorScene.Instantiate<LevelCreatorItems>();
                                break;
                            case ELECTRIC_WALL_NAME:
                                newItem = electricWallScene.Instantiate<LevelCreatorItems>();
                                break;
                        }

                        if (newItem != null)
                        {
                            newItem.Scale *= 0.3f;
                            newItem.Name = itemType;

                            // Ajout dans la tuile
                            lTile.content = newItem;
                            lTile.AddChild(newItem);
                            newItem.Position = Vector2.Zero;

                            // On met à jour le dictionnaire correctement
                            gridDico[gridIndex] = lTile;

                            GD.Print($"Placed {itemType} at {gridIndex}");
                        }

                        // Supprimer l'élément actif après placement
                        actualItem.QueueFree();
                        actualItem = null;
                    }
                }
            }

            // Suppression si on clique droit en dehors de la grille
            if (Input.IsMouseButtonPressed(MouseButton.Right) && gridIndex != new Vector2(-1, -1))
            {
                if (gridDico.TryGetValue(gridIndex, out LevelCreatorTile lTile) && lTile.content != null)
                {
                    lTile.content.QueueFree();
                    lTile.content = null;
                }
            }
        }


        private void Return()
        {
            returnButton.Hide();
            newLevelBackground.Visible = loadLevelBackground.Visible = customLevelMenuBackground.Visible = false;
            levelName.Text = "";
            if (cellContainer.GetChildren() != null)
            {
                foreach (var item in cellContainer.GetChildren()) item.QueueFree();
                gridDico.Clear();
            }
            if (buttonContainer.GetChildren() != null || labelContainer.GetChildren() != null || deleteButtonContainer.GetChildren() != null)
            {
                foreach (var item in buttonContainer.GetChildren()) item.QueueFree();
                foreach (var item in labelContainer.GetChildren()) item.QueueFree();
                foreach (var item in deleteButtonContainer.GetChildren()) item.QueueFree();
            }
            HUD.GetInstance().winScreen?.QueueFree();
            HUD.GetInstance().Hide();
            CustomSignals.GetInstance().EmitSignal(nameof(CustomSignals.UnLoadLevel));

            actualItem?.QueueFree();
            actualItem = null;
        }

		private void CreateNewLevel()
		{
            newLevelBackground.Visible = returnButton.Visible = true;
			CreateGrid();
        }

		private void LoadLevel(string pLevelName)
		{
            HUD.GetInstance().Show();
            customLevelMenuBackground.Hide();
            string lPath = "res://Scripts/Json/CustomLevels/" + pLevelName + ".json";
            loadLevelBackground.Visible = returnButton.Visible = true;
            GridManager.GetInstance().LoadNewLevel(0, lPath, cellContainer);
        }

		private void OpenCustomLevelsMenu()
		{
            customLevelMenuBackground.Visible = returnButton.Visible = true;
            DirContents("res://Scripts/Json/CustomLevels/");
        }

        public void DirContents(string path)
        {
            using DirAccess lDir = DirAccess.Open(path);
            if (lDir != null)
            {
                lDir.ListDirBegin();
                string lFileName = lDir.GetNext();
                while (lFileName != "")
                {
                    if (lDir.CurrentIsDir())
                    {
                        GD.Print($"Found directory: {lFileName}");
                    }
                    else
                    {
                        Vector2 lButtonMinimumSize = new Vector2(200, 200);
                        Vector2 lLabelMinimumSize = new Vector2(800, 200);

                        GD.Print($"Found file: {lFileName}");
                        Button lButton = new Button();
                        lButton.CustomMinimumSize = lButtonMinimumSize;
                        Button lDeleteButton = new Button();
                        lDeleteButton.CustomMinimumSize = lButtonMinimumSize;
                        Label lLabel = customLevelLabelScene.Instantiate() as Label;
                        lLabel.CustomMinimumSize = lLabelMinimumSize;

                        buttonContainer.AddChild(lButton);
                        labelContainer.AddChild(lLabel);
                        deleteButtonContainer.AddChild(lDeleteButton);

                        lButton.Text = "Play";
                        lDeleteButton.Text = "Delete Level";

                        string lName = lFileName.GetBaseName();
                        lLabel.Text = lName;

                        lDeleteButton.Pressed += () => DeleteLevel(lName);
                        lButton.Pressed += () => LoadLevel(lName);
                    }
                    lFileName = lDir.GetNext();
                }
            }
            else
            {
                GD.Print("An error occurred when trying to access the path.");
            }
        }

        private void DeleteLevel(string pLevelName)
        {
            string lFileName = "res://Scripts/Json/CustomLevels/" + pLevelName + ".Json";
            GD.PrintErr("Supression de " +  lFileName);
            DirAccess.RemoveAbsolute(lFileName);
            foreach (var item in buttonContainer.GetChildren()) item.QueueFree();
            foreach (var item in labelContainer.GetChildren()) item.QueueFree();
            foreach (var item in deleteButtonContainer.GetChildren()) item.QueueFree();
            OpenCustomLevelsMenu();
        }

        private void CreateGrid()
		{
			LevelCreatorTile lTile = tileScene.Instantiate() as LevelCreatorTile;
			Vector2 lPos;

			for (int x = 0; x < LENGHT; x++)
			{
				lPos = new Vector2(backGrid.Position.X + space + ((tileSize + space) * x), backGrid.Position.Y + space);

				for (int y = 0; y < LENGHT; y++)
				{
					lTile = new LevelCreatorTile();
					lTile.Color = new Color(0.5f, 0.5f, 0.5f, 1);
					lTile.Size = new Vector2(tileSize, tileSize);
					cellContainer.AddChild(lTile);
					lTile.Position = new Vector2(lPos.X, lPos.Y + ((tileSize + space) * y));
					gridDico.Add(PixelToGrid(lTile.Position), lTile);
					GD.Print("CreateTile");
                    if (x == 0 || x == 10 || y == 0 || y == 10)
                    {
                        LevelCreatorItems lItem = wallScene.Instantiate() as LevelCreatorItems;
                        lItem.Scale *= 0.3f;
                        lItem.Name = WALL_NAME;
						lTile.content = lItem;
                        lTile.AddChild(lItem);
                        lItem.Position = Vector2.Zero;
                    }
                }
			}
		}

		protected override void Dispose(bool pDisposing)
		{
			instance = null;
			base.Dispose(pDisposing);
		}
	}
}
