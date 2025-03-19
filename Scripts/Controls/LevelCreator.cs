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

        #region Exports & Variables
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
        #endregion

        #region Const & List
        private const string TESLA_NAME = "teslaTexture";
        private const string WALL_NAME = "wallTexture";
        private const string GENERATOR_NAME = "generatorTexture";
        private const string BULB_NAME = "bulbTexture";
        private const string PLAYERSPAWN_NAME = "playerSpawnTexture";
        private const string DOOR_NAME = "doorTexture";
        private const string ELECTRIC_WALL_NAME = "electricWallTexture";
        private const int LENGHT = 11;
        Dictionary<Vector2, LevelCreatorTile> gridDico = new Dictionary<Vector2, LevelCreatorTile>();
        #endregion

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

            #region Button Connect
            mainMenuButton.Pressed += () => {
                HUD.GetInstance().Show();
                CustomSignals.GetInstance().EmitSignal(CustomSignals.SignalName.GoToMainMenu);
                };
            newLevelButton.Pressed += CreateNewLevel;
            loadLevelButton.Pressed += () => LoadLevel(loadLevelText.Text);
            menuCustomLevelButton.Pressed += OpenCustomLevelsMenu;
            returnButton.Pressed += Return;
            saveButton.Pressed += CreateJSON;
            #endregion

            #region GetNode
            newLevelBackground = GetNode<Panel>("NewLevelBackGround");
            loadLevelBackground = GetNode<Panel>("LoadLevelBackGround");
            cellContainer = GetNode<Node2D>("CellContainer");
            customLevelMenuBackground = GetNode<Panel>("CustomLevelListBackGround");
            backGrid = newLevelBackground.GetNode<Panel>("BackGrid");
            #endregion

            RegisterMouseSignals(
            wallTexture, teslaTexture, bulbTexture,
            generatorTexture, doorTexture,
            playerSpawnTexture, electricWallTexture
            );

            newLevelBackground.Visible = loadLevelBackground.Visible = customLevelMenuBackground.Visible = returnButton.Visible = false;
        }
        public override void _Process(double pDelta)
		{
			float lDelta = (float)pDelta;
			MouseOn();
			PlaceItem();
            if (actualItem != null) actualItem.Position = GetLocalMousePosition();
		}

        private Vector2 PixelToGrid(Vector2 pPosition)
        {
            float lX = (pPosition.X - backGrid.GlobalPosition.X) / (tileSize + space);
            float lY = (pPosition.Y - backGrid.GlobalPosition.Y) / (tileSize + space);

            return new Vector2(Mathf.FloorToInt(lX), Mathf.FloorToInt(lY));
        }

        #region MouseFonction
        private void MouseOn()
        {
            //If player canPick & is on a texture. Instanciate item under the mouse
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
        }
        private void RegisterMouseSignals(params TextureRect[] pTextures)
        {
            foreach (var lTexture in pTextures)
            {
                lTexture.MouseEntered += () => OnMouseEntered(lTexture);
                lTexture.MouseExited += () => OnMouseExited();
            }
        }
        private void OnMouseEntered(TextureRect pTexture)
        {
            canPick = true;
            hoveredItem = pTexture; //
        }
        private void OnMouseExited()
        {
            canPick = false;
            hoveredItem = null;
        }
        private Vector2 GetGridIndexFromMousePos()
        {
            Vector2 lMousePos = GetGlobalMousePosition();

            float lGridX = (lMousePos.X - backGrid.GlobalPosition.X) / (tileSize + space);
            float lGridY = (lMousePos.Y - backGrid.GlobalPosition.Y) / (tileSize + space);

            int lX = Mathf.FloorToInt(lGridX);
            int lY = Mathf.FloorToInt(lGridY);

            if (lX < 0 || lY < 0 || lX > LENGHT - 1 || lY > LENGHT - 1)
            {
                return new Vector2(-1, -1);
            }
            return new Vector2(lX, lY);
        }

        #endregion

        #region JsonFonction
        private void CreateJSON()
        {
            string lFileName = "res://Scripts/Json/CustomLevels/" + levelName.Text + ".json";

            if (!FileAccess.FileExists(lFileName) && levelName.Text.Length > 0) //Check if a file with the same name already exist
            {
                string[] lMap = new string[LENGHT]; //Stock all the JsonKeys
                List<int> lBoxRange = new List<int>();

                //Local variable to check if the minimum required object is placed
                bool lHasDoor = false;
                bool lHasPlayerSpawn = false;
                bool lHasGenerator = false;
                bool lHasBulb = false;

                for (int lY = 0; lY < LENGHT; lY++)
                {
                    string lRow = ""; //Character write in Json file
                    for (int lX = 0; lX < LENGHT; lX++)
                    {
                        Vector2 lTileIndex = new Vector2(lX, lY); //Get the pos of each tile

                        if (gridDico.TryGetValue(lTileIndex, out LevelCreatorTile lTile) && lTile.content != null)// Check the content of each Tile
                        {
                            switch (lTile.content.Name)//Write the correct character according to the content of the tile
                            {
                                case WALL_NAME:
                                    lRow += JsonKeys.WALL;
                                    break;

                                case TESLA_NAME:
                                    lRow += JsonKeys.BOX;
                                    if (lTile.content.teslaRange != null)
                                    {
                                        lBoxRange.Add((int)lTile.content.teslaRange.Value); //Put the range of each tesla in the list stocked in Json
                                    }
                                    break;

                                case BULB_NAME:
                                    lRow += JsonKeys.GOAL_BULB;
                                    lHasBulb = true;
                                    break;

                                case GENERATOR_NAME:
                                    lRow += JsonKeys.GENERATOR;
                                    lHasGenerator = true;
                                    break;

                                case PLAYERSPAWN_NAME:
                                    lRow += JsonKeys.PLAYER;
                                    lHasPlayerSpawn = true;
                                    break;

                                case DOOR_NAME:
                                    lRow += JsonKeys.DOOR;
                                    lHasDoor = true;
                                    break;

                                case ELECTRIC_WALL_NAME:
                                    lRow += JsonKeys.ELECTRIC_WALL;
                                    break;

                                default:
                                    lRow += ' '; //if not content in tile
                                    break;
                            }
                        }
                        else
                        {
                            lRow += ' '; //Double check
                        }
                    }
                    lMap[lY] = lRow; //Put the JsonKey at the good place in lMap
                }

                //Verification of mandatory elements
                if (!lHasDoor || !lHasPlayerSpawn || !lHasGenerator || !lHasBulb)
                {
                    GD.PrintErr("Missing Items: A door, a playerspawn, a generator, and at least one light bulb are required.");
                    return;
                }

                //Building the final JSON
                string lJson = "{\n" +
                    $"  \"{JsonKeys.LEVEL_DESIGN_KEY}\": [\n" +
                    "    {\n" +
                    $"      \"{JsonKeys.PAR_KEY}\": 0,\n" +
                    $"      \"{JsonKeys.MAP_KEY}\": [\n";

                for (int i = 0; i < lMap.Length; i++) //Write all the Characters in the Map
                {
                    lJson += $"        \"{lMap[i]}\"";
                    if (i < lMap.Length - 1) lJson += ",";
                    lJson += "\n";
                }

                lJson += $"      ],\n" +
                    $"      \"{JsonKeys.BOX_RANGE_KEY}\": [";

                //Writing boxRange values
                for (int i = 0; i < lBoxRange.Count; i++)
                {
                    lJson += lBoxRange[i];
                    if (i < lBoxRange.Count - 1) lJson += ", ";
                }

                lJson += $"]\n" +
                    "    }\n" +
                    "  ]\n" +
                    "}";

                using FileAccess lCreateFile = FileAccess.Open(lFileName, FileAccess.ModeFlags.Write); //Create the file and open it for write
                lCreateFile.StoreString(lJson); //Write lJson variable inside

                GD.Print("File created successfully: " + lFileName);
            }
            else
            {
                GD.PrintErr("File already exists or name is empty.");
            }
        }
        public void DirContents(string lPath)
        {
            using DirAccess lDir = DirAccess.Open(lPath); //Open Folder from lPath
            if (lDir != null)
            {
                lDir.ListDirBegin(); //Initializes reading of the folder
                string lFileName;

                //Returns the next file or folder in the directory and assign it to lFileName.
                //As long as the name is not empty the loop continues
                while ((lFileName = lDir.GetNext()) != "") 
                {
                    if (lDir.CurrentIsDir()) //If it's a Foler
                    {
                        GD.Print($"Found directory: {lFileName}");
                    }
                    else
                    {
                        GD.Print($"Found file: {lFileName}");
                        CreateLevelButton(lFileName.GetBaseName()); //Get the name without the extension
                    }
                }
            }
            else
            {
                GD.Print("An error occurred when trying to access the path.");
            }
        }
        private void CreateLevelButton(string pLevelName)
        {
            Vector2 lButtonSize = new Vector2(200, 200);
            Vector2 lLabelSize = new Vector2(800, 200);

            Button lPlayButton = CreateButton("Play", lButtonSize, () => LoadLevel(pLevelName));
            Button lDeleteButton = CreateButton("Delete Level", lButtonSize, () => DeleteLevel(pLevelName));
            Label lLabel = customLevelLabelScene.Instantiate<Label>();
            lLabel.CustomMinimumSize = lLabelSize;
            lLabel.Text = pLevelName;

            buttonContainer.AddChild(lPlayButton);
            deleteButtonContainer.AddChild(lDeleteButton);
            labelContainer.AddChild(lLabel);
        }
        private Button CreateButton(string pText, Vector2 pSize, Action pOnPress)
        {
            Button lButton = new Button
            {
                CustomMinimumSize = pSize,
                Text = pText
            };
            lButton.Pressed += pOnPress;
            return lButton;
        }
        #endregion

        #region GridFonction
        private void PlaceItem()
        {
            Vector2 lGridIndex = GetGridIndexFromMousePos(); //Get Mouse Position in a local variable

            if (lGridIndex != new Vector2(-1, -1) && actualItem != null && Input.IsMouseButtonPressed(MouseButton.Left))
            {
                if (gridDico.TryGetValue(lGridIndex, out LevelCreatorTile lTile)) //Get the Tile in gridDico link to the mouse position
                {
                    if (lTile.content == null)
                    {
                        string lItemType = actualItem.Name; //Get the selected item in a local variable

                        //Create new instance of object
                        LevelCreatorItems lNewItem = null;

                        switch (lItemType) //Checks the type of the object using its name
                        {
                            case WALL_NAME:
                                lNewItem = wallScene.Instantiate<LevelCreatorItems>();
                                break;
                            case TESLA_NAME:
                                lNewItem = teslaScene.Instantiate<LevelCreatorItems>();
                                if (lNewItem.teslaRange != null)
                                    lNewItem.teslaRange.Value = teslaTexture.teslaRange.Value; //If it's a Tesla it receives the selected length
                                break;
                            case BULB_NAME:
                                lNewItem = bulbScene.Instantiate<LevelCreatorItems>();
                                break;
                            case GENERATOR_NAME:
                                lNewItem = generatorScene.Instantiate<LevelCreatorItems>();
                                break;
                            case PLAYERSPAWN_NAME:
                                lNewItem = playerSpawnScene.Instantiate<LevelCreatorItems>();
                                break;
                            case DOOR_NAME:
                                lNewItem = doorScene.Instantiate<LevelCreatorItems>();
                                break;
                            case ELECTRIC_WALL_NAME:
                                lNewItem = electricWallScene.Instantiate<LevelCreatorItems>();
                                break;
                        }

                        if (lNewItem != null)
                        {
                            lNewItem.Scale *= 0.3f;
                            lNewItem.Name = lItemType;

                            // Add to Tile
                            lTile.content = lNewItem;
                            lTile.AddChild(lNewItem);
                            lNewItem.Position = Vector2.Zero;

                            // Update Dictionnary
                            gridDico[lGridIndex] = lTile;

                            GD.Print($"Placed {lItemType} at {lGridIndex}");
                        }
                    }
                }
            }

            if (Input.IsMouseButtonPressed(MouseButton.Right) && lGridIndex != new Vector2(-1, -1)) //if right click on grid delete actual grid content
            {
                if (gridDico.TryGetValue(lGridIndex, out LevelCreatorTile lTile) && lTile.content != null && lTile.canBeRemove)
                {
                    lTile.content.QueueFree();
                    lTile.content = null;
                    gridDico[lGridIndex] = lTile; //Update Dictionnary
                }
            }
            else if (Input.IsMouseButtonPressed(MouseButton.Right) && lGridIndex == new Vector2(-1, -1)) //if right click out of grid deselect actual item
            {
                actualItem?.QueueFree();
                actualItem = null;
            }
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
                        lTile.canBeRemove = false;
                        lItem.Position = Vector2.Zero;
                    }
                }
            }
        }
        #endregion

        #region MenuFonction
        private void Return()
        {
            returnButton.Hide();
            newLevelBackground.Visible = loadLevelBackground.Visible = customLevelMenuBackground.Visible = false;
            levelName.Text = "";

            //Centralized deletion of container children
            ClearChildren(cellContainer, buttonContainer, labelContainer, deleteButtonContainer);
            gridDico.Clear();

            //Deleting specific elements
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
            DirContents("res://Scripts/Json/CustomLevels/"); //send folder reference to DirContents fonction
        }
        private void DeleteLevel(string pLevelName)
        {
            string lFileName = $"res://Scripts/Json/CustomLevels/{pLevelName}.json";
            GD.PrintErr($"Suppression de {lFileName}");

            DirAccess.RemoveAbsolute(lFileName); //Delete file in folder
            ClearChildren(buttonContainer, labelContainer, deleteButtonContainer); //Clear containers
            OpenCustomLevelsMenu(); //Reload LevelCustom Menu for an update
        }
        private void ClearChildren(params Node[] pContainers) //Generic function to remove children from a node
        {
            foreach (var pContainer in pContainers)
            {
                foreach (var child in pContainer.GetChildren())
                {
                    child.QueueFree();
                }
            }
        }
        #endregion
        protected override void Dispose(bool pDisposing)
		{
			instance = null;
			base.Dispose(pDisposing);
		}
	}
}
