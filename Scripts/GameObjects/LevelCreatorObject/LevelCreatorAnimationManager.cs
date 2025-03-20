using Godot;
using System;

// Author : Noe Sales

namespace Com.IsartDigital.SokoVolt.Managers {
	
	public partial class LevelCreatorAnimationManager : Node
	{
		public Tween BounceAnimation(Control pObject, float pScaleMultiplier, Color pColor, float pTime)
		{
			Vector2 lScale = pObject.Scale;
			Color lColor = pObject.SelfModulate;
			Tween lTween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Bounce);
			lTween.SetParallel(true);
			lTween.TweenProperty(pObject, "scale", pObject.Scale * pScaleMultiplier, pTime);
			lTween.TweenProperty(pObject, "modulate", pColor, pTime);
            lTween.SetParallel(false);
			lTween.TweenProperty(pObject, "scale", lScale, pTime);
			lTween.TweenProperty(pObject, "modulate", lColor, pTime);
			return lTween;
		}
    }
}
