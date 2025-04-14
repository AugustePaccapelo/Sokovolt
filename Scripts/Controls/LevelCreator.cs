using Com.IsartDigital.SokoVolt.GameObjects;
using Com.IsartDigital.SokoVolt.GameObjects.Movables;
using Com.IsartDigital.SokoVolt.Managers;
using Godot;
using RobotnikSokoban.Scripts.Managers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using static EnumSong;

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
        [Export] private Button mainMenuButton, newLevelButton, loadLevelButton, menuCustomLevelButton, saveButton, applyButton,
            bulbButton, doorButton, playerButton, batteryButton, teslaButton, wallButton, electricWallButton;
        [Export] public Button returnButton;
        [Export] private LineEdit loadLevelText, levelName, sizeXText, sizeYText;
        [Export] private LevelCreatorItems wallTexture, electricWallTexture, teslaTexture, bulbTexture, generatorTexture, playerSpawnTexture, doorTexture;
        [Export] private PackedScene wallScene, electricWallScene, teslaScene, bulbScene, generatorScene, playerSpawnScene, doorScene, tileScene, customLevelLabelScene;
        [Export] private VBoxContainer buttonContainer, deleteButtonContainer, labelContainer;
        [Export] private Json customLevelTemplate;
        [Export] private Label gridSizeLabel;
        [Export] private TextureRect backGround, screenBorder;
        [Export] private ColorRect screenEffect;
        [Export] private CompressedTexture2D batteryNormal, batteryHover, batteryPressed, 
            electricWallNormal, electricWallHover, electricWallPressed, 
            bulbNormal, bulbHover, bulbPressed, 
            playerNormal, playerHover, playerPressed, 
            teslaNormal, teslaHover, teslaPressed,
            tileNormal, tileHover, tilePressed,
            wallNormal, wallHover, wallPressed;
        private Panel newLevelBackground, customLevelMenuBackground, backGrid, menu;
        private LevelCreatorItems actualItem;
		private bool canPick = false;
		public static bool inLevelCreator = false;
        private TextureRect hoveredItem;
        private float tileSize = 50, space = 5;
        private Vector2 margin = new Vector2(370,220);
        private Node2D cellContainer;
        private int lenghtX = 11, lenghtY = 11, maxObject = 1;
        private string customLevelsFolderPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), CUSTOM_LEVELS_PATH);
        private AmbientSong lastPlayedSong;
        #endregion

        #region Const & List
        private const string TESLA_TYPE = "teslaTexture";
        private const string WALL_TYPE = "wallTexture";
        private const string GENERATOR_TYPE = "generatorTexture";
        private const string BULB_TYPE = "bulbTexture";
        private const string PLAYERSPAWN_TYPE = "playerSpawnTexture";
        private const string DOOR_TYPE = "doorTexture";
        private const string ELECTRIC_WALL_TYPE = "electricWallTexture";
        private const string SOKOVOLT_PATH = "Sokovolt";
        private const string CUSTOM_LEVELS_PATH = "Sokovolt/CustomLevels";
        private const string NEW_LEVEL_BACKGROUND_PATH = "NewLevelBackGround";
        private const string CELL_CONTAINER_PATH = "CellContainer";
        private const string CUSTOM_LEVEL_LIST_BACKGROUND_PATH = "CustomLevelListBackGround";
        private const string BACKGRID_PATH = "BackGrid";
        private const string MENU_PATH = "Menu";
        private const int LENGHT_MAX = 11;
        private const int LENGHT_MIN = 3;
        Dictionary<Vector2, LevelCreatorTile> gridDico = new Dictionary<Vector2, LevelCreatorTile>();

        GameManager gameManager; 
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
            applyButton.Pressed += ChangeGridSize;

            bulbButton.Pressed += () => ItemPick(bulbTexture);
            doorButton.Pressed += () => ItemPick(doorTexture);
            playerButton.Pressed += () => ItemPick(playerSpawnTexture);
            batteryButton.Pressed += () => ItemPick(generatorTexture);
            teslaButton.Pressed += () => ItemPick(teslaTexture);
            wallButton.Pressed += () => ItemPick(wallTexture);
            electricWallButton.Pressed += () => ItemPick(electricWallTexture);

            sizeXText.TextChanged += (newSize) =>
            {
                if (int.TryParse(newSize, out int result))
                {
                    lenghtX = result;
                }
            };

            sizeYText.TextChanged += (newSize) =>
            {
                if (int.TryParse(newSize, out int result))
                {
                    lenghtY = result;
                }
            };

            #endregion

            #region GetNode
            newLevelBackground = GetNode<Panel>(NEW_LEVEL_BACKGROUND_PATH);
            cellContainer = GetNode<Node2D>(CELL_CONTAINER_PATH);
            customLevelMenuBackground = GetNode<Panel>(CUSTOM_LEVEL_LIST_BACKGROUND_PATH);
            backGrid = newLevelBackground.GetNode<Panel>(BACKGRID_PATH);
            menu = GetNode<Panel>(MENU_PATH);

            gameManager = GameManager.GetInstance();
            #endregion

            RegisterMouseSignals(
            wallTexture, teslaTexture, bulbTexture,
            generatorTexture, doorTexture,
            playerSpawnTexture, electricWallTexture
            );

            newLevelBackground.Visible = customLevelMenuBackground.Visible = returnButton.Visible = false;
        }
        public override void _Process(double pDelta)
		{
			float lDelta = (float)pDelta;
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
        private void ItemPick(LevelCreatorItems pItem)
        {
            actualItem?.QueueFree();
            LevelCreatorItems lItem = null;

            if (pItem == wallTexture)
            {
                lItem = wallScene.Instantiate() as LevelCreatorItems;
                lItem.type = WALL_TYPE;
            }
            else if (pItem == teslaTexture)
            {
                lItem = teslaScene.Instantiate() as LevelCreatorItems;
                if (lItem.teslaRange != null) lItem.teslaRange.Value = teslaTexture.teslaRange.Value;
                lItem.type = TESLA_TYPE;
            }
            else if (pItem == bulbTexture)
            {
                lItem = bulbScene.Instantiate() as LevelCreatorItems;
                lItem.type = BULB_TYPE;
            }
            else if (pItem == generatorTexture)
            {
                lItem = generatorScene.Instantiate() as LevelCreatorItems;
                lItem.type = GENERATOR_TYPE;
            }
            else if (pItem == playerSpawnTexture)
            {
                lItem = playerSpawnScene.Instantiate() as LevelCreatorItems;
                lItem.type = PLAYERSPAWN_TYPE;
            }
            else if (pItem == electricWallTexture)
            {
                lItem = electricWallScene.Instantiate() as LevelCreatorItems;
                lItem.type = ELECTRIC_WALL_TYPE;
            }
            else if (pItem == doorTexture)
            {
                lItem = doorScene.Instantiate() as LevelCreatorItems;
                lItem.type = DOOR_TYPE;
            }

            cellContainer.AddChild(lItem);
            actualItem = lItem;
            actualItem.MouseFilter = MouseFilterEnum.Ignore; //Disable collision with mouse
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
            hoveredItem = pTexture;

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

            if (lX < 0 || lY < 0 || lX > lenghtX - 1 || lY > lenghtY - 1)
            {
                return new Vector2(-1, -1);
            }
            return new Vector2(lX, lY);
        }

        #endregion

        #region JsonFonction
        private void CreateJSON()
        {
            string lFileName = CreateCustomLevelsFolder() + levelName.Text + ".json";

            SongManager.Instance.ambientDict[EnumSong.AmbientSong.LevelCreatorClick].Play();

            if (!Godot.FileAccess.FileExists(lFileName) && levelName.Text.Length > 0) //Check if a file with the same name already exist
            {
                string[] lMap = new string[lenghtY]; //Stock all the JsonKeys
                List<int> lBoxRange = new List<int>();

                //Local variable to check if the minimum required object is placed
                bool lHasDoor = false, lHasPlayerSpawn = false, lHasGenerator = false, lHasBulb = false;
                int lDoorCounter = 0, lPlayerSpawnCounter = 0;

                for (int y = 0; y < lenghtY; y++)
                {
                    string lRow = ""; //Character write in Json file
                    for (int x = 0; x < lenghtX; x++)
                    {
                        Vector2 lTileIndex = new Vector2(x, y); //Get the pos of each tile

                        if (gridDico.TryGetValue(lTileIndex, out LevelCreatorTile lTile) && lTile.content != null)// Check the content of each Tile
                        {
                            switch (lTile.content.type)//Write the correct character according to the content of the tile
                            {
                                case WALL_TYPE:
                                    lRow += JsonKeys.WALL;
                                    break;

                                case TESLA_TYPE:
                                    lRow += JsonKeys.BOX;
                                    if (lTile.content.teslaRange != null)
                                    {
                                        lBoxRange.Add((int)lTile.content.teslaRange.Value); //Put the range of each tesla in the list stocked in Json
                                    }
                                    break;

                                case BULB_TYPE:
                                    lRow += JsonKeys.GOAL_BULB;
                                    lHasBulb = true;
                                    break;

                                case GENERATOR_TYPE:
                                    lRow += JsonKeys.GENERATOR;
                                    lHasGenerator = true;
                                    break;

                                case PLAYERSPAWN_TYPE:
                                    lRow += JsonKeys.PLAYER;
                                    lHasPlayerSpawn = true;
                                    lPlayerSpawnCounter++;
                                    break;

                                case DOOR_TYPE:
                                    lRow += JsonKeys.DOOR;
                                    lHasDoor = true;
                                    lDoorCounter++;
                                    break;

                                case ELECTRIC_WALL_TYPE:
                                    lRow += JsonKeys.ELECTRIC_WALL;
                                    break;

                                default:
                                    lRow += ' '; //if not content in tile
                                    break;
                            }

                            if (lDoorCounter > 1 || lPlayerSpawnCounter > 1)
                            {
                                AnimationManager.GetInstance().BounceAnimation(lTile.content, 0.5f, Colors.Red, 0.2f);
                                lDoorCounter = lPlayerSpawnCounter = 0;
                                return;
                            }
                        }
                        else
                        {
                            lRow += ' '; //Double check
                        }
                    }
                    lMap[y] = lRow; //Put the JsonKey at the good place in lMap
                }

                //Checking the number of elements
                if (!lHasBulb)
                {
                    AnimationManager.GetInstance().BounceAnimation(bulbButton, 0.5f, Colors.Red, 0.2f);
                    return;
                }
                if (!lHasDoor)
                {
                    AnimationManager.GetInstance().BounceAnimation(doorButton, 0.5f, Colors.Red, 0.2f);
                    return;
                }
                if (!lHasPlayerSpawn)
                {
                    AnimationManager.GetInstance().BounceAnimation(playerButton, 0.5f, Colors.Red, 0.2f);
                    return;
                }
                if (!lHasGenerator)
                {
                    AnimationManager.GetInstance().BounceAnimation(batteryButton, 0.5f, Colors.Red, 0.2f);
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

                using Godot.FileAccess lCreateFile = Godot.FileAccess.Open(lFileName, Godot.FileAccess.ModeFlags.Write); //Create the file and open it for write
                lCreateFile.StoreString(lJson); //Write lJson variable inside
                AnimationManager.GetInstance().BounceAnimation(levelName, 0.5f, Colors.Green, 0.4f);
                AnimationManager.GetInstance().BounceAnimation(saveButton, 0.5f, Colors.Green, 0.4f);
                GD.Print("File created successfully: " + lFileName);
            }
            else
            {
                AnimationManager.GetInstance().BounceAnimation(levelName, 0.5f, Colors.Red, 0.2f);
                GD.PrintErr("File already exists or name is empty.");
            }
        }
        private string CreateCustomLevelsFolder()
        {
            //Get the path to Documents
            string lDocumentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);

            //Full file path
            string lSokovoltFolder = Path.Combine(lDocumentsPath, SOKOVOLT_PATH);

            //If the folders do not exist, we create them
            if (!Directory.Exists(lSokovoltFolder))
            {
                Directory.CreateDirectory(lSokovoltFolder);
            }
            if (!Directory.Exists(customLevelsFolderPath))
            {
                Directory.CreateDirectory(customLevelsFolderPath);
            }
            return customLevelsFolderPath + "/";
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

            Button lPlayButton = CreateButton("PLAY", lButtonSize, pLevelName);
            Button lDeleteButton = CreateButton("Delete Level", lButtonSize, pLevelName);
            Label lLabel = customLevelLabelScene.Instantiate<Label>();
            lLabel.CustomMinimumSize = lLabelSize;
            lLabel.Text = pLevelName;

            buttonContainer.AddChild(lPlayButton);
            deleteButtonContainer.AddChild(lDeleteButton);
            labelContainer.AddChild(lLabel);
        }
        private Button CreateButton(string pText, Vector2 pSize, string pLevelName)
        {
            Button lButton = new Button
            {
                CustomMinimumSize = pSize,
                Text = pText
            };
            if (pText == "PLAY")
            {
                lButton.Pressed += () =>
                {
                    Tween lTween = GameManager.GetInstance().MenuTrans.ActiveTrans(1, 0.2f);
                    lTween.Finished += () => LoadLevel(pLevelName);
                };
            }
            else if (pText == "Delete Level")
            {
                lButton.Pressed += () =>
                {
                    Tween lTween = GameManager.GetInstance().MenuTrans.ActiveTrans(1, 0.2f);
                    lTween.Finished += () => DeleteLevel(pLevelName);
                };
            }
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
                        string lItemType = actualItem.type; //Get the selected item in a local variable

                        //Create new instance of object
                        LevelCreatorItems lNewItem = null;

                        switch (lItemType) //Checks the type of the object using its name
                        {
                            case WALL_TYPE:
                                lNewItem = wallScene.Instantiate<LevelCreatorItems>();
                                break;
                            case TESLA_TYPE:
                                lNewItem = teslaScene.Instantiate<LevelCreatorItems>();
                                if (lNewItem.teslaRange != null)
                                    lNewItem.teslaRange.Value = teslaTexture.teslaRange.Value; //If it's a Tesla it receives the selected length
                                break;
                            case BULB_TYPE:
                                lNewItem = bulbScene.Instantiate<LevelCreatorItems>();
                                break;
                            case GENERATOR_TYPE:
                                lNewItem = generatorScene.Instantiate<LevelCreatorItems>();
                                break;
                            case PLAYERSPAWN_TYPE:
                                lNewItem = playerSpawnScene.Instantiate<LevelCreatorItems>();
                                break;
                            case DOOR_TYPE:
                                lNewItem = doorScene.Instantiate<LevelCreatorItems>();
                                break;
                            case ELECTRIC_WALL_TYPE:
                                lNewItem = electricWallScene.Instantiate<LevelCreatorItems>();
                                break;
                        }

                        if (lNewItem != null)
                        {
                            lNewItem.Scale *= 0.3f;
                            lNewItem.type = lItemType;

                            // Add to Tile
                            lTile.content = lNewItem;
                            lTile.AddChild(lNewItem);
                            lNewItem.Position = Vector2.Zero;

                            // Update Dictionnary
                            gridDico[lGridIndex] = lTile;

                            // Play random song except the last played one
                            AudioStreamPlayer lsound = SongManager.Instance.PlayRandomInListExcept(EnumSong.popList, lastPlayedSong, SongManager.Instance.ambientDict);
                            lsound.PitchScale = 1f;
                            // Update last played song to the song just played
                            lastPlayedSong = EnumSong.popList[Utils.Random.RandiRange(0, EnumSong.popList.Count - 1)];

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

                    // Play random song except the last played one
                    AudioStreamPlayer lsound = SongManager.Instance.PlayRandomInListExcept(EnumSong.popList, lastPlayedSong, SongManager.Instance.ambientDict);
                    lsound.PitchScale = 0.8f;
                    // Update last played song to the song just played
                    lastPlayedSong = EnumSong.popList[Utils.Random.RandiRange(0, EnumSong.popList.Count - 1)];
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

            for (int x = 0; x < lenghtX; x++)
            {
                lPos = new Vector2(backGrid.Position.X + space + ((tileSize + space) * x), backGrid.Position.Y + space);

                for (int y = 0; y < lenghtY; y++)
                {
                    lTile = new LevelCreatorTile();
                    lTile.Color = new Color(0.5f, 0.5f, 0.5f, 1);
                    lTile.Size = new Vector2(tileSize, tileSize);
                    cellContainer.AddChild(lTile);
                    lTile.Position = new Vector2(lPos.X, lPos.Y + ((tileSize + space) * y));
                    gridDico.Add(PixelToGrid(lTile.Position), lTile);
                    if (x == 0 || x == lenghtX - 1 || y == 0 || y == lenghtY - 1)
                    {
                        LevelCreatorItems lItem = wallScene.Instantiate() as LevelCreatorItems;
                        lItem.Scale *= 0.3f;
                        lItem.type = WALL_TYPE;
                        lTile.content = lItem;
                        lTile.AddChild(lItem);
                        lTile.canBeRemove = false;
                        lItem.Position = Vector2.Zero;
                    }
                }
            }
            GD.Print("Grid Create");
        }

        private void ChangeGridSize()
        {
            if (lenghtX > LENGHT_MAX)
            {
                sizeXText.Text = LENGHT_MAX.ToString();
                lenghtX = LENGHT_MAX;
            }
            if (lenghtY > LENGHT_MAX)
            {
                sizeYText.Text = LENGHT_MAX.ToString();
                lenghtY = LENGHT_MAX;
            }
            if (lenghtX < LENGHT_MIN)
            {
                sizeXText.Text = LENGHT_MIN.ToString();
                lenghtX = LENGHT_MIN;
            }
            if (lenghtY < LENGHT_MIN)
            {
                sizeYText.Text = LENGHT_MIN.ToString();
                lenghtY = LENGHT_MIN;
            }

            // Check that the size is within the defined limits
            if (lenghtX >= LENGHT_MIN && lenghtX <= LENGHT_MAX && lenghtY >= LENGHT_MIN && lenghtY <= LENGHT_MAX)
            {
                int lBorder = 5;
                backGrid.Size = new Vector2(lenghtX * (tileSize + space) + lBorder, lenghtY * (tileSize + space) + lBorder);
                actualItem?.QueueFree();
                actualItem = null;
                gridDico.Clear();
                ClearChildren(cellContainer, gameManager.objectsContainer);
                CreateGrid();
            }
            else
            {
                GD.PrintErr($"Grid size out of bounds: X={lenghtX}, Y={lenghtY}");
            }
        }

        #endregion

        #region MenuFonction
        private void Return()
        {
            menu.Visible = mainMenuButton.Visible = screenBorder.Visible = screenEffect.Visible = true;
            returnButton.Hide();
            newLevelBackground.Visible = customLevelMenuBackground.Visible = false;
            levelName.Text = "";

            //Centralized deletion of container children
            ClearChildren(cellContainer, gameManager.objectsContainer, buttonContainer, labelContainer, deleteButtonContainer);
            gridDico.Clear();

            //Deleting specific elements
            HUD.GetInstance().winScreen?.QueueFree();
            var player = Player.GetInstance();
            player?.QueueFree(); // Pour le retirer de la scène
            HUD.GetInstance().Hide();
            CustomSignals.GetInstance().EmitSignal(nameof(CustomSignals.UnLoadLevel));
            InputManager.inGame = false;
            backGround.Show();

            SongManager.Instance.ambientDict[EnumSong.AmbientSong.levelCreatorMenuClick].Play();

            actualItem?.QueueFree();
            actualItem = null;
        }
        private void CreateNewLevel()
		{
            newLevelBackground.Visible = returnButton.Visible = true;
            menu.Hide();
            lenghtX = lenghtY = LENGHT_MAX;
            sizeXText.Text = sizeYText.Text = null;
            SongManager.Instance.ambientDict[EnumSong.AmbientSong.levelCreatorMenuClick].Play();
            ChangeGridSize();
        }
		private void LoadLevel(string pLevelName)
		{
            if(pLevelName.Length == 0) return;
            SongManager.Instance.ambientDict[EnumSong.AmbientSong.levelCreatorMenuClick].Play();
            backGround.Hide();
            screenEffect.Hide();
            screenBorder.Hide();
            HUD.GetInstance().Show();
            customLevelMenuBackground.Hide();
            string lPath = customLevelsFolderPath + "/" + pLevelName + ".json";
            returnButton.Visible = true;
            menu.Visible = mainMenuButton.Visible = false;
            GridManager.GetInstance().LoadNewLevel(0, lPath, GameManager.GetInstance().objectsContainer);
            InputManager.inGame = true;
            CustomMaskOcluder.instance.SetBackgroundVisibility(true);
        }
		private void OpenCustomLevelsMenu()
		{
            menu.Hide();
            SongManager.Instance.ambientDict[EnumSong.AmbientSong.levelCreatorMenuClick].Play();
            customLevelMenuBackground.Visible = returnButton.Visible = true;
            DirContents(customLevelsFolderPath); //send folder reference to DirContents fonction
        }
        private void DeleteLevel(string pLevelName)
        {
            SongManager.Instance.ambientDict[EnumSong.AmbientSong.levelCreatorMenuClick].Play();
            string lFileName = customLevelsFolderPath + "/" + pLevelName + ".json";
            GD.PrintErr($"Suppression de {lFileName}");

            File.Delete(lFileName); //Delete file in folder
            ClearChildren(cellContainer, gameManager.objectsContainer, buttonContainer, labelContainer, deleteButtonContainer); //Clear containers
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
