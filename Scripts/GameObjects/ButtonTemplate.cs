using Godot;
using System;

// Author : Noe Sales

namespace Com.IsartDigital.SokoVolt {
	
	public partial class ButtonTemplate : Button
	{
		[Export] private float scaleMultiplicator = 0.5f;
		[Export] private float during = 0.5f;
		[Export] Tween.EaseType easeType;
		[Export] Tween.TransitionType transitionType;

        public override void _Ready()
        {
			Pressed += TweenInit;
        }

        private void TweenInit()
		{
			Tween lTween = CreateTween();
			lTween.TweenProperty(this, "scale", Scale * scaleMultiplicator, during / 2).SetEase(easeType).SetTrans(transitionType);
			lTween.TweenProperty(this, "scale", new Vector2(1,1), during / 2).SetEase(easeType).SetTrans(transitionType);
        }
	}
}
