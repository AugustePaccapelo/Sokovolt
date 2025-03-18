using Godot;
using System;

// Author : Auguste Paccapelo

namespace Com.IsartDigital.SokoVolt
{
	public partial class Piston : Control
	{
		// ---------- VARIABLES ---------- \\

		// ----- Paths ----- \\

		// ----- Nodes ----- \\
		[Export] TextureRect head;
		[Export] TextureRect body1;
		[Export] TextureRect body2;

		// ----- Others ----- \\
		public float timeToExtend = 1.5f;
		public float timeToRetract = 1.5f;
		public const float DISTANCE_OF_EXTEND = 100f;
		private bool canMove = true;

        // ---------- FUNCTIONS ---------- \\

        // ----- Ready & Process ----- \\

		public override void _Ready()
		{
			base._Ready();

			body1.Position -= Vector2.Up * DISTANCE_OF_EXTEND;
		}

		public override void _Process(double pDelta)
		{
			float lDelta = (float)pDelta;

			base._Process(lDelta);
		}

		// ----- My Functions ----- \\
		public void Extend()
		{
			if (!canMove) return;
			canMove = false;
			Tween lTween = CreateTween();
			lTween.TweenProperty(head, "position", head.Position + Vector2.Up * DISTANCE_OF_EXTEND, timeToExtend);
            lTween.TweenProperty(body1, "position", head.Position + Vector2.Up * DISTANCE_OF_EXTEND, timeToExtend);
			lTween.Finished += () => { canMove = true; };
        }

		public void Retract()
		{
            if (!canMove) return;
            canMove = false;
            Tween lTween = CreateTween();
            lTween.TweenProperty(head, "position", head.Position - Vector2.Up * DISTANCE_OF_EXTEND, timeToRetract);
            lTween.TweenProperty(body1, "position", head.Position - Vector2.Up * DISTANCE_OF_EXTEND, timeToRetract);
            lTween.Finished += () => { canMove = true; };
        }

		// ----- Destructor ----- \\

		protected override void Dispose(bool pDisposing)
		{
			base.Dispose(pDisposing);
		}
	}
}
