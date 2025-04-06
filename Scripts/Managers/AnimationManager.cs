using Godot;
using System;
using static Com.IsartDigital.SokoVolt.Tools.ObjectProperties;

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
			lTween.TweenProperty(pObject, SCALE, pObject.Scale * pScaleMultiplier, pTime);
			lTween.TweenProperty(pObject, MODULATE, pColor, pTime);
            lTween.SetParallel(false);
			lTween.TweenProperty(pObject, SCALE, lScale, pTime);
			lTween.TweenProperty(pObject, MODULATE, lColor, pTime);
			return lTween;
		}

        public Tween CameraZoomTraveling(Camera2D pCamera, float pMoveTime, float pWaitTime, Vector2 pFinalPos, Vector2 pFromPos, float pZoom)
        {
            Tween lTween = CreateTween().SetParallel(true).SetTrans(Tween.TransitionType.Linear).SetEase(Tween.EaseType.In);
            lTween.TweenProperty(pCamera, POSITION, pFinalPos, pMoveTime);
            lTween.TweenProperty(pCamera, ZOOM, new Vector2(pZoom, pZoom), pMoveTime);
            lTween.Finished += () =>
            {
                Tween lDelayTween = CreateTween();
                lDelayTween.TweenInterval(pWaitTime);

                lDelayTween.Finished += () =>
                {
                    Tween lTween2 = CreateTween().SetParallel(true).SetTrans(Tween.TransitionType.Linear).SetEase(Tween.EaseType.In);
                    lTween2.TweenProperty(pCamera, ZOOM, new Vector2(1, 1), pMoveTime);
                    lTween2.TweenProperty(pCamera, POSITION, pFromPos, pMoveTime);
                };
            };

            return lTween;
        }

        public Tween ShakeEffect(Node2D pObject, Vector2 pShakeValue, float pTime)
        {
            Vector2 lPos = pObject.Position;
            Tween lTween = CreateTween();
            lTween.TweenProperty(pObject, POSITION, pShakeValue, pTime).AsRelative();
            lTween.TweenProperty(pObject, POSITION, -pShakeValue, pTime).AsRelative();
            pObject.Position = lPos;
            return lTween;
        }
        public Tween RotationEffect(Node2D pObject, float pRotationValue, float pTime, Tween.TransitionType pTransType, Tween.EaseType pEaseType)
        {
            float pRotation = pObject.Rotation;
            Tween lTween = CreateTween().SetTrans(pTransType).SetEase(pEaseType);
            lTween.TweenProperty(pObject, ROTATION, pRotationValue, pTime).AsRelative();
            lTween.TweenProperty(pObject, ROTATION, -pRotationValue, pTime).AsRelative();
            pObject.Rotation = pRotation;
            return lTween;
        }

        //public Tween LightShake(Node pObject, float pRotation, float pSpeed)
        //{
        //    Tween lTween = CreateTween();
        //    lTween.TweenProperty(pObject, ROTATION, Mathf.DegToRad(pRotation), pSpeed).AsRelative();
        //    lTween.TweenProperty(pObject, ROTATION, Mathf.DegToRad(pRotation), pSpeed).AsRelative();
        //}
    }
}
