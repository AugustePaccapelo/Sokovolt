using Godot;
using System;

//Author : Ferlat Thibaud 
namespace Com.IsartDigital.ProjectName{
	
	public partial class Door : GameObject
	{
		#region GetInstance
		static private Door instance;
		
		static public Door GetInstance () {
			if (instance == null) instance = new Door();
			return instance;
		}

		private Door ():base() {}
		#endregion

		public override void _Ready()
		{
			#region instance
		if (instance != null){  
			QueueFree();
			GD.Print(nameof(Door) + " Instance already exist, destroying the last added.");
			return;
		}
		instance = this;
		#endregion

		}

		public override void _Process(double pDelta)
		{

		}

		#region dispose
		protected override void Dispose(bool pDisposing)
		{
			if (pDisposing && instance == this) instance = null;
			base.Dispose(pDisposing);
		}
		#endregion
	}
}
