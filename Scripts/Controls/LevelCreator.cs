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

		[Export] private Button mainMenuButton;
		[Export] private Button newLevelButton;
		[Export] private Button loadLevelButton;
		[Export] private Button menuCustomLevelButton;
		[Export] private Button returnButton;
		[Export] private Button saveButton;
		[Export] private TextEdit loadLevelText;
		[Export] private TextEdit levelName;
		[Export] private LevelCreatorItems wallTexture;
		[Export] private LevelCreatorItems teslaTexture;
		[Export] private LevelCreatorItems bulbTexture;
		[Export] private LevelCreatorItems generatorTexture;
		[Export] private PackedScene wallScene;
		[Export] private PackedScene teslaScene;
		[Export] private PackedScene bulbScene;
		[Export] private PackedScene generatorScene;
		[Export] private PackedScene tileScene;
		[Export] private PackedScene customLevelLabelScene;
        [Export] private VBoxContainer buttonContainer;
        [Export] private VBoxContainer deletebuttonContainer;
        [Export] private VBoxContainer labelContainer;
        [Export] private Json customLevelTemplate;
		private Panel newLevelBackGround;
		private Panel loadLevelBackGround;
		private Panel customLevelMenuBackGround;
		private Panel backGrid;
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

            #endregion

            newLevelBackGround = GetNode<Panel>("NewLevelBackGround");
            loadLevelBackGround = GetNode<Panel>("LoadLevelBackGround");
            cellContainer = GetNode<Node2D>("CellContainer");
            customLevelMenuBackGround = GetNode<Panel>("CustomLevelListBackGround");
            backGrid = newLevelBackGround.GetNode<Panel>("BackGrid");

            newLevelBackGround.Visible = loadLevelBackGround.Visible = customLevelMenuBackGround.Visible = returnButton.Visible = false;

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
            levelName.Text = "";
            string lFileName = "res://Scripts/Json/CustomLevels/" + levelName.Text + ".Json";
            if (!FileAccess.FileExists(lFileName) && levelName.Text.Length>0)
            {
                using FileAccess lCreatFile = FileAccess.Open(lFileName, FileAccess.ModeFlags.Write);
                lCreatFile.StoreString("{" +
                    "\n  \"levelDesign\": [" +
                    "\n    {" +
                    "\n      \"par\": 0," +
                    "\n      \"map\": [" +
                    "\n        \"###########\"," +
                    "\n        \"#         #\"," +
                    "\n        \"#         #\"," +
                    "\n        \"#         #\"," +
                    "\n        \"#         #\"," +
                    "\n        \"#         #\"," +
                    "\n        \"#         #\"," +
                    "\n        \"#         #\"," +
                    "\n        \"#         #\"," +
                    "\n        \"#         #\"," +
                    "\n        \"###########\"" +
                    "\n      ]," +
                    "\n      \"boxRange\": []" +
                    "\n    }" +
                    "\n  ]" +
                    "\n}");
            }
            else GD.PrintErr("Name file alreadyExist or empty");
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

                cellContainer.AddChild(lItem);
                actualItem = lItem;
                canPick = false;
            }

            if (Input.IsMouseButtonPressed(MouseButton.Right) && GetGridIndexFromMousePos() != new Vector2(-1, -1))
            {
                LevelCreatorTile lTile = gridDico[GetGridIndexFromMousePos()];
                foreach (var item in lTile.GetChildren()) item.QueueFree();
                lTile.content = null;
            }
		}

        private Vector2 PixelToGrid(Vector2 pPos)
        {
            return new Vector2(Mathf.FloorToInt((pPos.X - backGrid.Position.X) / (tileSize + space)), Mathf.FloorToInt((pPos.Y - backGrid.Position.Y) / (tileSize + space)));
        }
        private Vector2 GetGridIndexFromMousePos()
        {
            Vector2 lMousePos = GetLocalMousePosition();
            Vector2 lGridMousePos = PixelToGrid(lMousePos);
            if (lGridMousePos.X < 0 || lGridMousePos.Y < 0
                || lGridMousePos.X > LENGHT || lGridMousePos.Y > LENGHT)
                lGridMousePos = new Vector2(-1, -1);

            return lGridMousePos;
        }

        private void PlaceItem()
		{
			if (GetGridIndexFromMousePos() != new Vector2(-1, -1) && actualItem != null && Input.IsMouseButtonPressed(MouseButton.Left))
			{
				GD.Print("Place item");
                LevelCreatorTile lTile = gridDico[GetGridIndexFromMousePos()];
                if (lTile.content == null)
                {
                    string lTypeItem = actualItem.Name;
                    cellContainer.RemoveChild(actualItem);
                    lTile.AddChild(actualItem);
                    actualItem.Scale *= 0.3f;
                    actualItem.Position = Vector2.Zero;
					lTile.content = actualItem;
                    actualItem = null;

                    GD.Print(lTypeItem);

                    LevelCreatorItems lItem = new LevelCreatorItems();

                    if (lTypeItem == WALL_NAME)
                    {
                        lItem = wallScene.Instantiate() as LevelCreatorItems;
                        lItem.Name = WALL_NAME;
                    }
                    else if (lTypeItem == TESLA_NAME)
                    {
                        lItem = teslaScene.Instantiate() as LevelCreatorItems;
                        if (lItem.teslaRange != null) lItem.teslaRange.Value = teslaTexture.teslaRange.Value;
                        lItem.Name = TESLA_NAME;
                    }
                    else if (lTypeItem == BULB_NAME)
                    {
                        lItem = bulbScene.Instantiate() as LevelCreatorItems;
                        lItem.Name = BULB_NAME;
                    }
                    else if (lTypeItem == GENERATOR_NAME)
                    {
                        lItem = generatorScene.Instantiate() as LevelCreatorItems;
                        lItem.Name = GENERATOR_NAME;
                    }

                    cellContainer.AddChild(lItem);
                    actualItem = lItem;
                    canPick = false;
                }
			}
            else if (GetGridIndexFromMousePos() == new Vector2(-1, -1) && actualItem != null && Input.IsMouseButtonPressed(MouseButton.Right))
            {
                actualItem.QueueFree();
                actualItem = null;
            }

        }

        private void Return()
        {
            returnButton.Hide();
            newLevelBackGround.Visible = loadLevelBackGround.Visible = customLevelMenuBackGround.Visible = false;
            levelName.Text = "";
            if (cellContainer.GetChildren() != null)
            {
                foreach (var item in cellContainer.GetChildren()) item.QueueFree();
                gridDico.Clear();
            }
            if (buttonContainer.GetChildren() != null || labelContainer.GetChildren() != null || deletebuttonContainer.GetChildren() != null)
            {
                foreach (var item in buttonContainer.GetChildren()) item.QueueFree();
                foreach (var item in labelContainer.GetChildren()) item.QueueFree();
                foreach (var item in deletebuttonContainer.GetChildren()) item.QueueFree();
            }
            HUD.GetInstance().winScreen?.QueueFree();
            HUD.GetInstance().Hide();
            CustomSignals.GetInstance().EmitSignal(nameof(CustomSignals.UnLoadLevel));

            actualItem?.QueueFree();
            actualItem = null;
        }

		private void CreateNewLevel()
		{
            newLevelBackGround.Visible = returnButton.Visible = true;
			CreateGrid();
        }

		private void LoadLevel(string pLevelName)
		{
            HUD.GetInstance().Show();
            customLevelMenuBackGround.Hide();
            string lPath = "res://Scripts/Json/CustomLevels/" + pLevelName + ".json";
            loadLevelBackGround.Visible = returnButton.Visible = true;
            GridManager.GetInstance().LoadNewLevel(0, lPath, cellContainer);
        }

		private void OpenCustomLevelsMenu()
		{
			customLevelMenuBackGround.Visible = returnButton.Visible = true;
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
                        deletebuttonContainer.AddChild(lDeleteButton);

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
            foreach (var item in deletebuttonContainer.GetChildren()) item.QueueFree();
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
                        cellContainer.AddChild(lItem);
                        lItem.Scale *= 0.3f;
                        lItem.Position = lTile.Position;
						lTile.content = lItem;
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
