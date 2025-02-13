using Godot;
using System;
using System.Data.Common;

// Author : Ferlat Thibaud 

namespace Com.IsartDigital.ProjectName {
	
	public partial class Movable : GameObject
	{

		public virtual void MoveTo(int pX, int pY, Cell[,] pGrid)
		{
			Cell lOldCell = pGrid[x, y];
			Cell lNewCell = pGrid[pX, pY];

			if(lOldCell.GetContent() == this)
				lOldCell.SetContent(null);

			x = pX;
			y = pY;

			lNewCell.SetContent(this);
			
			Utils.SetPosition(this, x, y); 

			UpdateZindex();
		}

		private void UpdateZindex()
		{
			ZIndex = y;
		}
	}
}
