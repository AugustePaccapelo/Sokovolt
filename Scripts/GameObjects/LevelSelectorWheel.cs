using Godot;
using System;

// Author : Noe Sales

namespace Com.IsartDigital.SokoVolt.GameObjects {
	
	public partial class LevelSelectorWheel : Sprite2D
	{
		[Export] private bool turnRight = false;
		public static float speed = 5f;
		private float rotationSpeed;

        public override void _Process(double pDelta)
		{
			float lDelta = (float)pDelta;

            rotationSpeed = speed / Scale.X;

            if (turnRight)RotationDegrees += rotationSpeed * lDelta;
			else RotationDegrees -= rotationSpeed * lDelta;
		}

		protected override void Dispose(bool pDisposing)
		{

		}
	}
}
