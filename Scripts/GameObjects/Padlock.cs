using Com.IsartDigital.SokoVolt.GameObjects;
using Godot;
using System;

// Author : Noe Sales

namespace Com.IsartDigital.SokoVolt {
	
	public partial class Padlock : Node2D
	{
		[Export] private Sprite2D topLock;
		[Export] private Sprite2D botLock;
		[Export] private LevelSelectorTesla tesla;
		[Export] private PointLight2D light;
		[Export] private PointLight2D topLight;
		[Export] private Sprite2D[] listLock;
		[Export] private PointLight2D[] listLight;

		public void Open()
		{
			Tween lTween = CreateTween().SetParallel(true);
			foreach (Sprite2D lockPart in listLock) lTween.TweenProperty(lockPart, "self_modulate", new Color(0, 1, 0), 0.5f);
            lTween.TweenProperty(topLock, "position", new Vector2(0, -100), 0.5f).AsRelative();
			lTween.TweenProperty(tesla, "modulate", new Color(0.5f,0.5f,0.5f), 0.5f);
			lTween.TweenProperty(topLight, "energy", 5, 1f);
			foreach (PointLight2D light in listLight) lTween.TweenProperty(light, "energy", 3, 1);
			lTween.SetParallel(false);
            lTween.TweenProperty(this, "modulate", new Color(0, 0, 0, -1), 0.5f).AsRelative();
        }

        public void Close()
		{
            Tween lTween = CreateTween();
            lTween.TweenProperty(this, "modulate", new Color(0, 0, 0, 1), 0.5f).AsRelative();
            lTween.SetParallel(true);
            lTween.TweenProperty(topLock, "position", new Vector2(0, 100), 0.5f).AsRelative();
            foreach (Sprite2D lockPart in listLock) lTween.TweenProperty(lockPart, "self_modulate", new Color(0.62f, 0.10f, 0.09f), 0.5f);
            lTween.TweenProperty(tesla, "modulate", new Color(0.36f, 0.36f, 0.36f), 0.5f);
            foreach (PointLight2D light in listLight) lTween.TweenProperty(light, "energy", 0, 0.5f);
            lTween.TweenProperty(topLight, "energy", 0, 0.5f);
        }
    }
}
