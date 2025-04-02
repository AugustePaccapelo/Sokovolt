using Godot;
using System.Collections.Generic;

// Author : PACCAPELO Auguste

namespace Com.IsartDigital.SokoVolt.Managers
{
	public partial class Manager : Node2D
	{
		// ---------- VARIABLES ---------- \\

		private static int numManager = 0;
		private static List<Manager> allManagers = new List<Manager>();

		// ---------- FONCTIONS ---------- \\

		// ----- Constructor & Ready & Init ----- \\

		protected Manager() : base()
		{
			numManager++;
		}

		public override void _Ready()
		{
			allManagers.Add(this);
			if (IsAllMAangersReady()) InitAllManagers();
			base._Ready();
		}

		public virtual void Init() { }

		// ----- My Fonctions ----- \\

		private bool IsAllMAangersReady() => numManager == allManagers.Count;
		private void InitAllManagers()
		{
			foreach (Manager lManager in allManagers) lManager.Init();
		}


		// ----- Destructor ----- \\

		protected override void Dispose(bool pDisposing)
		{
			numManager--;
			allManagers.Remove(this);
			base.Dispose(pDisposing);
		}
	}
}