using Godot;
using System;
using static Com.IsartDigital.SokoVolt.Tools.ObjectProperties;

// Author : Ferlat Thibaud 

namespace Com.IsartDigital.SokoVolt.GameObjects {
	
	public partial class AnimationPiston : Node2D
	{
        [Export] public Node2D topPart; // Partie mobile du piston (le bras)
		private const int START_POS = 600;
		Tween tileTween;
		Tween pistonTween;

        public async void Launch(Cell pTargetCell, Vector2 pFinalPosition, float pDelay)
		{
			HUD.GetInstance().mainMenuButton.Disabled = true;
            if (LevelCreator.inLevelCreator) LevelCreator.GetInstance().returnButton.Disabled = true;
            topPart.GlobalPosition = new Vector2(GlobalPosition.X, pFinalPosition.Y + START_POS);
			ZIndex -= 40; 
			if (pTargetCell == null) return;
			
			// Position de départ sous la scène
			Vector2 lStartPos = pFinalPosition + new Vector2(0, 600);
			pTargetCell.GlobalPosition = lStartPos;
			
			if (pTargetCell.GetContent() != null)
				pTargetCell.GetContent().GlobalPosition = lStartPos;
			
			// Décalage personnalisé
			await ToSignal(GetTree().CreateTimer(pDelay), TIME_OUT);
			
			         // Animation bras du piston
			         pistonTween = CreateTween();
			pistonTween.TweenProperty(topPart, POSITION_Y, 0, 0.6f)
				.SetTrans(Tween.TransitionType.Back)
				.SetEase(Tween.EaseType.Out);
			
			await ToSignal(GetTree().CreateTimer(0.1f), TIME_OUT);
			
			         // Animation de montée
			         tileTween = CreateTween();
			tileTween.Parallel().TweenProperty(pTargetCell, GLOBALPOSITION, pFinalPosition, 0.6f)
				.SetTrans(Tween.TransitionType.Back)
				.SetEase(Tween.EaseType.Out);

            if (pTargetCell.GetContent() != null)
			{
				tileTween.Parallel().TweenProperty(pTargetCell.GetContent(), GLOBALPOSITION, pFinalPosition, 0.6f)
					.SetTrans(Tween.TransitionType.Back)
					.SetEase(Tween.EaseType.Out);
			}
			
			tileTween.TweenProperty(topPart, POSITION_Y, START_POS - 100, 0.6f)
				.SetTrans(Tween.TransitionType.Back)
				.SetEase(Tween.EaseType.Out);
			
			tileTween.Parallel().TweenProperty(this, POSITION_Y, GlobalPosition.Y + 250, 0.3f)
				.SetTrans(Tween.TransitionType.Back)
				.SetEase(Tween.EaseType.In);

            await ToSignal(pistonTween, FINISHED);
			await ToSignal(GetTree().CreateTimer(1f), TIME_OUT);
			GetTree().CreateTimer(1f).Timeout += ButtonActivation;
            QueueFree();
        }

		private static void ButtonActivation()
		{
            HUD.GetInstance().mainMenuButton.Disabled = false;
            if (LevelCreator.inLevelCreator) LevelCreator.GetInstance().returnButton.Disabled = false;
        }
	}
}
