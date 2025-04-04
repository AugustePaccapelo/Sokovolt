using Com.IsartDigital.Sokovolt;
using Com.IsartDigital.SokoVolt.GameObjects;
using Com.IsartDigital.SokoVolt.GameObjects.Movables;
using Godot;
using System.Collections.Generic;

// Author : Auguste Paccapelo

namespace Com.IsartDigital.SokoVolt.Managers
{
	public partial class GameManager : Manager
	{
		// ---------- VARIABLES ---------- \\

		#region // ----- Singleton ----- \\

		static private GameManager instance;

		static public GameManager GetInstance()
		{
			if (instance == null) instance = new GameManager();
			return instance;
		}

        #endregion

        // ----- Nodes ----- \\
        [Export] public Node2D objectsContainer;
        [Export] public MenuTrans MenuTrans;
        [Export] public Camera2D camera;
        [Export] private Polygon2D mouse;

        HUD hud;

        // Managers
        private GridManager gridManager;
		private CustomSignals signals;
		private UserGestion userGestion;

		// GameObjects
		public Door door;
		private List<GoalBulb> allGoalBulbs = new List<GoalBulb>();

		// ----- Others ----- \\
		private List<int> scorePerStar = new List<int> { 1000, 2000, 5000 };
		private int currentLevel;

        // ---------- FUNCTIONS ---------- \\

        // ----- Ready & Init & Process ----- \\

        public override void _Ready()
		{
			#region // ----- Singleton ----- \\

			if (instance != null)
			{
				GD.Print(Name + " Instance already exist, destroying the last added.");
				QueueFree();
				return;
			}

			instance = this;

			#endregion

			base._Ready();
		}
		public override void Init()
		{
            hud = HUD.GetInstance();
			userGestion = UserGestion.GetInstance();
            signals = CustomSignals.GetInstance();
            signals.PlayerMoved += PlayerHasMoved;
            signals.GoalBulbStateChanged += GoalBulbStateChanged;
			signals.LoadLevel += NewLevelLoaded;
            gridManager = GridManager.GetInstance();
        }
        public override void _Process(double pDelta)
		{
            base._Process(pDelta);
            float lDelta = (float)pDelta;

            HideMouse();
        }

        // ----- My Functions ----- \\

        private void HideMouse()
		{
            mouse.Position = GetLocalMousePosition();
			if (Input.MouseMode != Input.MouseModeEnum.Hidden) Input.MouseMode = Input.MouseModeEnum.Hidden;
        }

        public void AddGoalBulb(GoalBulb pGoalBulb)
		{
			allGoalBulbs.Add(pGoalBulb);
		}
		public void RemoveGoalBulb(GoalBulb pGoalBulb)
		{
			allGoalBulbs.Remove(pGoalBulb);
		}
		private void GoalBulbStateChanged()
		{
            foreach (GoalBulb lGoalBulb in allGoalBulbs)
			{
				if (!lGoalBulb.isTurnedOn)
				{
					door?.Close();
                    return;
                }
			}
            door?.Open();
            if (currentLevel <= 3) gridManager.HandleCellClicked(new Vector2(door.x, door.y)); // ! PLACE HOLDER ! \\
		}
        private void PlayerHasMoved()
        {
            Player lPlayer = Player.GetInstance();
            if (door.isOpen && lPlayer.x == door.x && lPlayer.y == door.y)
            {
                GD.Print("Player has exited!");
                GameFinished();
            }
        }
        private async void GameFinished()
		{
            int lNumStep = GridManager.GetInstance().step;
            int lPar = LevelLoader.parCount;
            int lNumStar;

			if (lNumStep <= lPar)
				lNumStar = 3;
			else lNumStar = lNumStep <= lPar * 1.5f ? 2 : 1;

			int lScore = scorePerStar[lNumStar - 1] - lNumStep;

            userGestion.SaveUserProgress(WinScreen.actualLevel, lScore, lNumStar); // saves scores/stars
			userGestion.UnlockLevel(WinScreen.actualLevel + 1); // level unlocked with json

			LevelSelector lSelector = LevelSelector.GetInstance();

			if (lSelector != null && lSelector.teslaDictionnary.ContainsKey(WinScreen.actualLevel + 1))
			{
				lSelector.teslaDictionnary[WinScreen.actualLevel + 1].UnlockLevel();
				GD.Print("Level unlocked");
			}

            GD.PrintErr(lNumStar + " " + lScore + " " + lNumStep); 

			await ToSignal(GetTree().CreateTimer(0.3f), "timeout");

			hud.displayInGame.Visible = false; 
            CustomSignals.GetInstance().EmitSignal(CustomSignals.SignalName.GameFinished, lNumStar, lScore, lNumStep);
        }
		private void NewLevelLoaded(int pLevel) => currentLevel = pLevel;

		// ----- Destructor ----- \\

		protected override void Dispose(bool pDisposing)
		{
            #region // ----- Singleton ----- \\

            if (pDisposing && instance == this) instance = null;

            #endregion

            base.Dispose(pDisposing);
		}
	}
}