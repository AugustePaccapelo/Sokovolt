using Godot;
using System;

// Author : Noe Sales

namespace Com.IsartDigital.SokoVolt.GameObjects {
	
	public partial class LevelCreatorItems : TextureRect
	{
		[Export] public SpinBox teslaRange;
		public string type = null;
		public override void _Ready()
		{
			
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
