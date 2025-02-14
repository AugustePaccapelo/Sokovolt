using Godot;
using System;

// Author : Auguste Paccapelo

namespace Com.IsartDigital.SokoVolt.GameObjects
{
	public partial class Door : GameObject
    {
		// ---------- VARIABLES ---------- \\

		#region // ----- Singleton ----- \\

		static private Door instance;

		static public Door GetInstance()
		{
			if (instance == null) instance = new Door();
			return instance;
		}

		#endregion

		// ----- Paths ----- \\

		// ----- Nodes ----- \\

		// ----- Others ----- \\

		// ---------- FONCTIONS ---------- \\

		// ----- Constructor & Ready & Process ----- \\

		private Door() : base() { }

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
