using Godot;
using System;

// Author : Noe Sales

namespace Com.IsartDigital.SokoVolt.Managers {
	
	public partial class AnimationManager : Manager
	{
        #region Singleton
        static private AnimationManager instance;

        private AnimationManager() { }

        static public AnimationManager GetInstance()
        {
            if (instance == null) instance = new AnimationManager();
            return instance;

        }
        #endregion

        public override void _Ready()
        {
            base._Ready();
            #region Singleton Ready
            if (instance != null)
            {
                QueueFree();
                GD.Print(nameof(AnimationManager) + " Instance already exist, destroying the last added.");
                return;
            }

            instance = this;
            #endregion
        }

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
