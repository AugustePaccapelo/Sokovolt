using Godot;
using System;

// Author : Noe Sales

namespace Com.IsartDigital.SokoVolt.GameObjects {
	
	public partial class LevelSelectorWheel : Sprite2D
	{
		[Export] private bool turnRight = false;
		private float speed;

        public override void _Ready()
        {
            speed = 0.1f / Scale.X;
        }

        public override void _Process(double pDelta)
		{
			float lDelta = (float)pDelta;
			if(turnRight)RotationDegrees += speed;
			else RotationDegrees -= speed;
		}

		protected override void Dispose(bool pDisposing)
		{

		}
	}
}
