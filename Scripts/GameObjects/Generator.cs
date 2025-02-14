using Godot;
using System;

// Author : Ferlat Thibaud 

namespace Com.IsartDigital.SokoVolt.GameObjects { 
	
	public partial class Generator : GameObject
	{

		#region GetInstance
		static private Generator instance;
        static public Generator GetInstance(int pX = 0, int pY = 0)
        {
            if (instance == null)
            {
                instance = new Generator();
            }
            return instance;

        }
		private Generator() : base(){}
		
		#endregion
		public override void _Ready()
		{
			#region instance
		if (instance != null){  
			QueueFree();
			GD.Print(nameof(Generator) + " Instance already exist, destroying the last added.");
			return;
		}
		instance = this;
		#endregion
		}

		public override void _Process(double pDelta)
		{
			float lDelta = (float)pDelta;

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
