using Godot;
using System;
using System.Data.SqlTypes;

// Author : Ferlat Thibaud 

namespace Com.IsartDigital.SokoVolt.GameObjects.Movables {
	
	public partial class BoxTesla : Movable
	{
		public override void _Ready()
		{

		}

		public override void _Process(double pDelta)
		{
			float lDelta = (float)pDelta;

		}

        public override void MoveTo(int pX, int pY, Cell[,] pGrid)
        {
            base.MoveTo(pX, pY, pGrid);

			CustomSignals lSignals = CustomSignals.GetInstance();

			lSignals.EmitSignal(CustomSignals.SignalName.BoxTeslaMoved);
        }

        protected override void Dispose(bool pDisposing)
		{

		}
	}
}
