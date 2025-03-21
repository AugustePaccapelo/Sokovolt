using Godot;
using System;
using System.Runtime.CompilerServices;

// Author : Noe Sales

namespace Com.IsartDigital.SokoVolt.GameObjects {
	
	public partial class MenuTrans : Node2D
	{
		[Export] Polygon2D rightPart, leftPart;
		public override void _Ready()
		{
			rightPart.Position = new Vector2(1920, 0);
			leftPart.Position = new Vector2(-1920, 0);
		}

		public Tween ActiveTrans(float pMoveTime, float pCloseTime)
		{
            // PREMIER TWEEN (ANIMATION INITIALE)
            Tween lTween = CreateTween().SetParallel(true);
            lTween.TweenProperty(rightPart, "position", new Vector2(3, -6), pMoveTime/4);
            lTween.TweenProperty(leftPart, "position", new Vector2(3, 0), pMoveTime / 4);

            // Une fois le premier Tween terminé...
            lTween.Finished += () =>
            {
                Tween lDelayTween = CreateTween();
                lDelayTween.TweenInterval(pCloseTime);

                lDelayTween.Finished += () =>
                {
                    Tween lTween2 = CreateTween().SetParallel(true);
                    lTween2.TweenProperty(rightPart, "position", new Vector2(1920, -6), pMoveTime / 4);
                    lTween2.TweenProperty(leftPart, "position", new Vector2(-1920, 0), pMoveTime / 4);
                };
            };
            return lTween;
        }
	}
}
