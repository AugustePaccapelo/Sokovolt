using Godot;
using System;

// Author : Noe Sales

namespace Com.IsartDigital.ProjectName {
	
	public partial class ButtonSmokeParticles : GpuParticles2D
	{
		public override void _Ready()
		{
			Emitting = true;
			Finished += QueueFree;
		}
	}
}
