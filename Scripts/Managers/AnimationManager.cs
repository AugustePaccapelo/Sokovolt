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

        public Tween CameraZoomTraveling(Camera2D pCamera, float pMoveTime, float pWaitTime, Vector2 pFinalPos, Vector2 pFromPos, float pZoom)
        {
            Tween lTween = CreateTween().SetParallel(true).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.In);
            lTween.TweenProperty(pCamera, "position", pFinalPos, pMoveTime);
            lTween.TweenProperty(pCamera, "zoom", new Vector2(pZoom, pZoom), pMoveTime);
            lTween.Finished += () =>
            {
                Tween lDelayTween = CreateTween();
                lDelayTween.TweenInterval(pWaitTime);

                lDelayTween.Finished += () =>
                {
                    Tween lTween2 = CreateTween().SetParallel(true).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.In);
                    lTween2.TweenProperty(pCamera, "zoom", new Vector2(1, 1), pMoveTime);
                    lTween2.TweenProperty(pCamera, "position", pFromPos, pMoveTime);
                };
            };

            return lTween;
        }
    }
}
