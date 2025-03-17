using Com.IsartDigital.SokoVolt.GameObjects;
using Com.IsartDigital.SokoVolt.GameObjects.Movables;
using Godot;
using System;
using System.Collections.Generic;

// Author : Auguste Paccapelo

namespace Com.IsartDigital.SokoVolt.Managers
{
	public partial class GameManager : Manager
	{
		// ---------- VARIABLES ---------- \\
		[Export] public Node2D objectsContainer;

		#region // ----- Singleton ----- \\

		static private GameManager instance;

		static public GameManager GetInstance()
		{
			if (instance == null) instance = new GameManager();
			return instance;
		}

		#endregion

		// ----- Paths ----- \\

		// ----- Nodes ----- \\

		// Managers
		private GridManager gridManager;
		private CustomSignals signals;

		// GameObjects
		public Door door;
		private List<GoalBulb> allGoalBulbs = new List<GoalBulb>();

		// ----- Others ----- \\
		private const int SCORE_1_STAR = 1000;
        private const int SCORE_2_STAR = 2000;
        private const int SCORE_3_STAR = 5000;

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
            signals = CustomSignals.GetInstance();
            signals.PlayerMoved += PlayerHasMoved;
            signals.GoalBulbStateChanged += GoalBulbStateChanged;
            gridManager = GridManager.GetInstance();
        }

        public override void _Process(double pDelta)
		{
			float lDelta = (float)pDelta;

			base._Process(lDelta);
		}

		// ----- My Functions ----- \\

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

        private void GameFinished()
		{
            int lNumStep = GridManager.GetInstance().step;
            int lPar = LevelLoader.parCount;
            int lNumStar = 0;
            int lScore = 0;
            if (lNumStep <= lPar)
            {
                lNumStar = 3;
                lScore = SCORE_3_STAR;
            }
            else if (lNumStep <= lPar * 1.5f)
            {
                lNumStar = 2;
                lScore = SCORE_2_STAR;
            }
            else
            {
                lNumStar = 1;
                lScore = SCORE_1_STAR;
            }

            lScore -= lNumStep;

            CustomSignals.GetInstance().EmitSignal(CustomSignals.SignalName.GameFinished, lNumStar, lScore, lNumStep);
        }

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
