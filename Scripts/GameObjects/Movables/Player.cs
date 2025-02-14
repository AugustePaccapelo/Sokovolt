using Godot;
using System;
using System.Runtime.CompilerServices;

//Author : Ferlat Thibaud 
namespace Com.IsartDigital.SokoVolt.GameObjects.Movables {
	
	public partial class Player : Movable
	{
		#region GetInstance
		static private Player instance;
        static public Player GetInstance(int pX = 0, int pY = 0)
        {
            if (instance == null)
            {
                instance = new Player();
            }
            return instance;

        }
		private Player() : base(){}
		
		#endregion
		


		public override void _Ready()
		{
			#region instance
		if (instance != null){  
			QueueFree();
			GD.Print(nameof(Player) + " Instance already exist, destroying the last added.");
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
