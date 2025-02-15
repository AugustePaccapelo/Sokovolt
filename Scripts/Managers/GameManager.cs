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

		// GameObjects
		private List<GoalBulb> allGoalBulbs = new List<GoalBulb>();

		// ----- Others ----- \\

		// ---------- FONCTIONS ---------- \\

		// ----- Constructor & Ready & Init & Process ----- \\

		private GameManager() : base() { }

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
            gridManager = GridManager.GetInstance();

			foreach(GoalBulb lGoalBulb in allGoalBulbs) lGoalBulb.Init();
        }

		public override void _Process(double pDelta)
		{
			float lDelta = (float)pDelta;

			base._Process(lDelta);
		}

		// ----- My Fonctions ----- \\

		public void AddGoalBulb(GoalBulb pGoalbulb)
		{
			allGoalBulbs.Add(pGoalbulb);
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
