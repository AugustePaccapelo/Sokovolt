using Godot;
using System;

// Author : Noe Sales

namespace Com.IsartDigital.SokoVolt.GameObjects {
	
	public partial class LevelCreatorTile : ColorRect
	{
		public LevelCreatorItems content = null;
		public const int SIZE = 50;
		public bool canBeRemove = true;

        public override void _Ready()
		{
            content = null;
        }

		public override void _Process(double pDelta)
		{
			float lDelta = (float)pDelta;
			if(content != null && !canBeRemove) SelfModulate = Colors.Red;
			else if(content != null) SelfModulate = Colors.Orange;
			else SelfModulate = Colors.Green;
		}

		protected override void Dispose(bool pDisposing)
		{

		}
	}
}
