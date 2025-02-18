using Godot;
using System;
using System.Collections.Generic;

// Author : Ferlat Thibaud 

namespace Com.IsartDigital.SokoVolt.GameObjects.Movables {
	
	public partial class BoxTesla : Movable
    {
        [Export] private int teslaRange;
		[Export] private bool energize;
        static List<BoxTesla> boxTeslaList = new List<BoxTesla>();
		public override void _Ready()
        {
            


        }

		public override void _Process(double pDelta)
		{
			float lDelta = (float)pDelta;

		}

        private void connectionSearch()
        {
            Vector2 cellpossission = Utils.GetCellPos(this);


        }

		 
		protected override void Dispose(bool pDisposing)
		{

		}
	}
}
