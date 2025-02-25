using Godot;
using System;

// Author : Noe Sales

namespace Com.IsartDigital.SokoVolt.GameObjects {
	
	public partial class LevelSelectorBolt : Node2D
	{
		[Export] public AnimationPlayer animationPlayer;
        [Export] public GpuParticles2D sparks;
        [Export] public GpuParticles2D flare;
        [Export] public Line2D bolt;
        [Export] public PointLight2D light;
        [Export] public bool animationActive = true;

        public override void _Ready()
        {
            animationPlayer.Active = animationActive;
            sparks.Emitting = false;
            flare.Emitting = false;
            bolt.Hide();
            light.Energy = 0;
        }
    }
}
