using Godot;
using System;

// Author : Auguste Paccapelo

namespace Com.IsartDigital.Sokoban.Managers
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

		public override void Init() { }

		public override void _Process(double pDelta)
		{
			float lDelta = (float)pDelta;

			base._Process(lDelta);
		}

		// ----- My Fonctions ----- \\

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
