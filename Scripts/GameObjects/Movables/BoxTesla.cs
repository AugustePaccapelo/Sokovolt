using Godot;
using System;
using System.Data.SqlTypes;

// Author : Ferlat Thibaud 

namespace Com.IsartDigital.SokoVolt.GameObjects.Movables {
	
	public partial class BoxTesla : Movable
	{
		[Export] private Label rangeLabel; 
		public int range; 
		public override void _Ready()
		{	
			Init(); 
		}

		public override void _Process(double pDelta)
		{
			float lDelta = (float)pDelta;

		}

		private void Init()
		{
			rangeLabel.Text = range.ToString();
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
