using Godot;
using System;

// Author : Ferlat Thibaud 

namespace Com.IsartDigital.SokoVolt.GameObjects {
	
	public partial class AnimationPiston : Node2D
	{
        [Export] public Node2D topPart; // Partie mobile du piston (le bras)
		private const int START_POS = 600; 

     	public async void Launch(Cell pTargetCell, Vector2 pFinalPosition, float pDelay)
		{
			topPart.GlobalPosition = new Vector2(GlobalPosition.X, pFinalPosition.Y + START_POS);
			ZIndex -= 40; 
			if (pTargetCell == null) return;

			// Position de départ sous la scène
			Vector2 lStartPos = pFinalPosition + new Vector2(0, 600);
			pTargetCell.GlobalPosition = lStartPos;

			if (pTargetCell.GetContent() != null)
				pTargetCell.GetContent().GlobalPosition = lStartPos;

			// ⏱ Décalage personnalisé
			await ToSignal(GetTree().CreateTimer(pDelay), "timeout");

			// Animation bras du piston
			Tween lPistonTween = CreateTween();
			lPistonTween.TweenProperty(topPart, "position:y", 0, 0.6f)
				.SetTrans(Tween.TransitionType.Back)
				.SetEase(Tween.EaseType.Out);

			await ToSignal(GetTree().CreateTimer(0.1f), "timeout");

			// Animation de montée
			Tween lTileTween = CreateTween();
			lTileTween.Parallel().TweenProperty(pTargetCell, "global_position", pFinalPosition, 0.6f)
				.SetTrans(Tween.TransitionType.Back)
				.SetEase(Tween.EaseType.Out);

			if (pTargetCell.GetContent() != null)
			{
				lTileTween.Parallel().TweenProperty(pTargetCell.GetContent(), "global_position", pFinalPosition, 0.6f)
					.SetTrans(Tween.TransitionType.Back)
					.SetEase(Tween.EaseType.Out);
			}

			await ToSignal(lTileTween, "finished");
			QueueFree();
		}

	}
}
