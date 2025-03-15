using Godot;
using System;

// Author : Noe Sales

namespace Com.IsartDigital.ProjectName {
	
	public partial class test_rope : Node2D
	{
		[Export] Node2D area;


		public override void _Ready()
		{

		}

		public override void _Process(double pDelta)
		{
			float lDelta = (float)pDelta;
			area.Position = GetLocalMousePosition();
		}

		protected override void Dispose(bool pDisposing)
		{

		}
	}
}
