using Com.IsartDigital.SokoVolt.GameObjects;
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
            signals.GoalBulbStateChanged += GoalBulbStateChanged;
            gridManager = GridManager.GetInstance();
        }

		public override void _Process(double pDelta)
		{
			float lDelta = (float)pDelta;

			base._Process(lDelta);
		}

		// ----- My Functions ----- \\

		public void AddGoalBulb(GoalBulb pGoalbulb)
		{
			allGoalBulbs.Add(pGoalbulb);
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

		public void GameFinished()
		{
			int lNumStep = GridManager.GetInstance().step;

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
