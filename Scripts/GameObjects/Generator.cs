using Godot;
using System;

// Author : Soukai William
namespace Com.IsartDigital.SokoVolt.GameObjects { 
	
	public partial class Generator : GameObject
	{

		public override void _Ready()
		{
			connectionManagers.generatorList.Add(this);
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
