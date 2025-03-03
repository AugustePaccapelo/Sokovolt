using Godot;
using System;
using System.Data.Common;

// Author : Ferlat Thibaud 

namespace Com.IsartDigital.SokoVolt.GameObjects.Movables {
	
	public partial class Movable : GameObject
	{
		//Animation Lerp 
		
 		private Vector2 targetPosition; 
		private bool isMoving; 
		private float moveSpeed = 15; 

		//Signals 
		[Signal] public delegate void MovableHaveFinishEventHandler(Movable sender); 
		public virtual void MoveTo(int pX, int pY, Cell[,] pGrid)
		{
			Cell lOldCell = pGrid[x, y];
			Cell lNewCell = pGrid[pX, pY];

			if(lOldCell.GetContent() == this)
				lOldCell.SetContent(null);

			x = pX;
			y = pY;

			lNewCell.SetContent(this);

			targetPosition = Utils.SetPosition(this, x, y, false); 
			isMoving = true; 

			UpdateZindex();
		}

		private void UpdateZindex()
		{
			ZIndex = y;
		}

        public override void _Process(double pDelta)
        {
			float lDelta = (float)pDelta;
			if(isMoving)
			{
				GlobalPosition = GlobalPosition.Lerp(targetPosition, moveSpeed * lDelta); 

				if(GlobalPosition.DistanceTo(targetPosition) < 1f)
				{
					GlobalPosition = targetPosition; 
					EmitSignal(nameof(MovableHaveFinish), this); 
					isMoving = false; 
				}
			}
        }
    }
}
