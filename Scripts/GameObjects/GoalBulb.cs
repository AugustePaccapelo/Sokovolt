using Godot;
using System;

// Author : Auguste Paccapelo

namespace Com.IsartDigital.Sokoban.GameObjects
{
	public partial class GoalBulb : GameObject
	{
		// ---------- VARIABLES ---------- \\

		// ----- Paths ----- \\

		// ----- Nodes ----- \\

		// ----- Others ----- \\

		// ---------- FONCTIONS ---------- \\

		// ----- Constructor & Ready & Process ----- \\

		protected GoalBulb () : base() { }

		public override void _Ready()
		{
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
			base.Dispose(pDisposing);
		}
	}
}
