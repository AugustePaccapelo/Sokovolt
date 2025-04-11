using Godot;
using System;

// Author : Soukai William
namespace Com.IsartDigital.SokoVolt.GameObjects { 
	
	public partial class Generator : GameObject
	{
		[Export] public Marker2D connectionPoint;
		public override void _Ready()
		{
			ConnectionManagers.generatorList.Add(this);
			Init();
		}

        private void Init()
        {
           
        }

        public override void _Process(double pDelta)
		{
			float lDelta = (float)pDelta;

		}
		
	}
}
