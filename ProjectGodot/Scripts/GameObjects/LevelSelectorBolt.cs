using Godot;
using System;
using System.Runtime.CompilerServices;

// Author : Noe Sales

namespace Com.IsartDigital.SokoVolt.GameObjects {
	
	public partial class LevelSelectorBolt : Node2D
	{
		[Export] public AnimationPlayer animationPlayer;
        //[Export] public GpuParticles2D sparks;
        //[Export] public GpuParticles2D flare;
        [Export] public Line2D bolt;
        [Export] public PointLight2D light;
        [Export] public bool animationActive = true;
        [Export] public bool hideBolt = true;

        public override void _Ready()
        {
            //animationPlayer.Active = animationActive;
            //sparks.Emitting = false;
            //flare.Emitting = false;
            if (hideBolt) bolt.Hide();
            light.Energy = 0;
        }
    }
}
