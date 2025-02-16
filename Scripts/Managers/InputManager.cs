using Com.IsartDigital.SokoVolt.Managers;
using Godot;
using System;

// Author : A. Dylan Montenegro Utrela

namespace Com.IsartDigital.SokoVolt.Managers
{
	public partial class InputManager : Manager
    {
		[Signal] public delegate void MoveEventHandler(Vector2 pDirection);
        [Signal] public delegate void RedoEventHandler();

        public override void Init()
        {
            base.Init();
            Move += GridManager.GetInstance().OnMovePlayer;
        }

        public override void _Process(double pDelta)
		{
			float lDelta = (float)pDelta;
            if (Input.IsActionJustPressed("ui_right")) EmitSignal(SignalName.Move, new Vector2(1, 0));
            if (Input.IsActionJustPressed("ui_left")) EmitSignal(SignalName.Move, new Vector2(-1, 0)); 
            if (Input.IsActionJustPressed("ui_down")) EmitSignal(SignalName.Move, new Vector2(0, 1));
            if (Input.IsActionJustPressed("ui_up")) EmitSignal(SignalName.Move, new Vector2(0, -1));
        }

        protected override void Dispose(bool pDisposing)
		{
			base.Dispose(pDisposing);
		}
	}
}
