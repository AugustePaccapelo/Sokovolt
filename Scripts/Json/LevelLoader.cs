using Godot;
using System;

//Author : Ferlat Thibaud 
namespace Com.IsartDigital.Sokoban {
	
	public partial class LevelLoader : Node
	{
		#region GetInstance
		static private LevelLoader instance;
		
		static public LevelLoader GetInstance () {
			if (instance == null) instance = new LevelLoader();
			return instance;
		}

		private LevelLoader ():base() {}
		#endregion

		public override void _Ready()
		{
			#region instance
		if (instance != null){  
			QueueFree();
			GD.Print(nameof(LevelLoader) + " Instance already exist, destroying the last added.");
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
