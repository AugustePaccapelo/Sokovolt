using Godot;
using System;
using Com.IsartDigital.SokoVolt.Tools;

// Author : Ferlat Thibaud 

namespace Com.IsartDigital.SokoVolt.GameObjects {
	
	public partial class ElectricWall : GameObject
	{	
		public override void _Ready()
		{
			HighlightManager.GetInstance()?.RegisterTarget("ElectricWall", this);
		}

		public override void _Process(double pDelta)
		{
			float lDelta = (float)pDelta;

		}

		protected override void Dispose(bool pDisposing)
		{

		}
	}
}
