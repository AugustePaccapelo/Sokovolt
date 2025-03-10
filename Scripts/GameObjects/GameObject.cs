using Com.IsartDigital.SokoVolt.GameObjects.Movables;
using Godot;
using System;

// Author : Ferlat Thibaud 

namespace Com.IsartDigital.SokoVolt.GameObjects {
	
	public partial class GameObject : Node2D
	{
		public int x, y;
		protected Cell curentCell;

		public void Init(int pX, int pY)
		{
			x = pX;
			y = pY;
		}

		public void SetCell(Cell pNewCell)
		{
			curentCell = pNewCell;
			//GlobalPosition = pNewCell.GlobalPosition;
		}

		protected override void Dispose(bool pDisposing)
		{

		}
	}
}
