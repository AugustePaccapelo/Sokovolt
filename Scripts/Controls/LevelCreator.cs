using Com.IsartDigital.SokoVolt.GameObjects;
using Com.IsartDigital.SokoVolt.Managers;
using Godot;
using System;
using System.Collections.Generic;

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

		[Export] Button mainMenuButton;
		[Export] Button newLevelButton;
		[Export] Button loadLevelButton;
		[Export] Button menuCustomLevelButton;
		[Export] Button returnButton;
		[Export] TextEdit loadLevelText;
		[Export] LevelCreatorItems wallTexture;
		[Export] LevelCreatorItems teslaTexture;
		[Export] LevelCreatorItems bulbTexture;
		[Export] LevelCreatorItems generatorTexture;
		[Export] PackedScene wallScene;
		[Export] PackedScene teslaScene;
		[Export] PackedScene bulbScene;
		[Export] PackedScene generatorScene;
		[Export] PackedScene tileScene;
		[Export] PackedScene customLevelLabelScene;
        [Export] VBoxContainer buttonContainer;
        [Export] VBoxContainer labelContainer;
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
        

        private const int LENGHT = 11;

		Dictionary<Vector2, LevelCreatorTile> gridDico = new Dictionary<Vector2, LevelCreatorTile>();

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

			mainMenuButton.Pressed += () => CustomSignals.GetInstance().EmitSignal(CustomSignals.SignalName.GoToMainMenu);
            newLevelButton.Pressed += CreateNewLevel;
            loadLevelButton.Pressed += () => LoadLevel(loadLevelText.Text);
            menuCustomLevelButton.Pressed += OpenCustomLevelsMenu;
            returnButton.Pressed += Return;


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

            Return();

            //OpenCustomLevelsMenu();
        }

		public override void _Process(double pDelta)
		{
			float lDelta = (float)pDelta;
			MouseOn();
			PlaceItem();
            if (actualItem != null) actualItem.Position = GetLocalMousePosition();
		}

		private void MouseOn()
		{
			if (Input.IsMouseButtonPressed(MouseButton.Left) && canPick)
			{
				actualItem?.QueueFree();
                LevelCreatorItems lItem = new LevelCreatorItems();

                if (hoveredItem == wallTexture) lItem = wallScene.Instantiate() as LevelCreatorItems;
                else if (hoveredItem == teslaTexture)
				{
					lItem = teslaScene.Instantiate() as LevelCreatorItems;
					if(lItem.teslaRange != null)lItem.teslaRange.Value = teslaTexture.teslaRange.Value;
				}
				else if (hoveredItem == bulbTexture) lItem = bulbScene.Instantiate() as LevelCreatorItems;
				else if (hoveredItem == generatorTexture) lItem = generatorScene.Instantiate() as LevelCreatorItems;

                cellContainer.AddChild(lItem);
                actualItem = lItem;
                canPick = false;
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
                    actualItem.Scale *= 0.3f;
                    actualItem.Position = lTile.Position;
					lTile.content = actualItem;
                    actualItem = null;
                }
			}
		}

        private void Return()
        {
            returnButton.Hide();
            newLevelBackGround.Visible = loadLevelBackGround.Visible = customLevelMenuBackGround.Visible = false;
            if (cellContainer.GetChildren() != null)
            {
                foreach (var item in cellContainer.GetChildren()) item.QueueFree();
                gridDico.Clear();
            }
            if (buttonContainer.GetChildren() != null || labelContainer.GetChildren() != null)
            {
                foreach (var item in buttonContainer.GetChildren()) item.QueueFree();
                foreach (var item in labelContainer.GetChildren()) item.QueueFree();
            }
        }

		private void CreateNewLevel()
		{
            newLevelBackGround.Visible = returnButton.Visible = true;
			CreateGrid();
        }

		private void LoadLevel(string pLevelName)
		{
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
                        GD.Print($"Found file: {lFileName}");
                        Button lButton = new Button();
                        lButton.CustomMinimumSize = new Vector2(200, 200);
                        Label lLabel = customLevelLabelScene.Instantiate() as Label;
                        lLabel.CustomMinimumSize = new Vector2(800, 200);

                        buttonContainer.AddChild(lButton);
                        labelContainer.AddChild(lLabel);

                        lButton.Text = "Play";

                        string lName = lFileName.GetBaseName();
                        lLabel.Text = lName;

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
