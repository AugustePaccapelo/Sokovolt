using Godot;
using System;

// Author : Ferlat Thibaud 

namespace Com.IsartDigital.SokoVolt.GameObjects {
	
	public partial class GameObject : Node2D
	{
		public int x, y;
		protected Cell curentCell;

		public override void _Ready()
		{

		}

		public void Init(int pX, int pY)
		{
			x = pX;
			y = pY;
		}

		public override void _Process(double pDelta)
		{
			float lDelta = (float)pDelta; 
		}

		public void SetCell(Cell pNewCell)
		{
			curentCell = pNewCell;
			GlobalPosition = pNewCell.GlobalPosition;
		}

		protected override void Dispose(bool pDisposing)
		{

		}
	}
}
