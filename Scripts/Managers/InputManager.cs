using Godot;
using System;
using System.Collections.Generic;

// Author : A. Dylan Montenegro Utrela

namespace Com.IsartDigital.SokoVolt.Managers
{
	public partial class InputManager : Manager
	{
		#region Singleton
		static private InputManager instance;

		private InputManager() { }

		static public InputManager GetInstance()
		{
			if (instance == null) instance = new InputManager();
			return instance;
		}
		#endregion

		[Signal] public delegate void MoveEventHandler(Vector2 pDirection);
		[Signal] public delegate void UndoRedoEventHandler(int pPosition);
		[Signal] public delegate void RetryEventHandler();

		public override void _Ready()
		{
			#region Singleton
			if (instance != null)
			{
				QueueFree();
				GD.Print(nameof(InputManager) + " INSTANCE ALREADY EXISTS, DESTROYING THE LAST ADDED");
				return;
			}

			instance = this;
			#endregion

			base._Ready();
		}

		public override void _Input(InputEvent @event)
		{
			if (@event is InputEventKey eventKey && eventKey.Pressed)
			{
                if (Input.IsActionJustPressed("Up")) EmitSignal(SignalName.Move, Vector2.Up);
                if (Input.IsActionJustPressed("Down")) EmitSignal(SignalName.Move, Vector2.Down);
                if (Input.IsActionJustPressed("Left")) EmitSignal(SignalName.Move, Vector2.Left);
                if (Input.IsActionJustPressed("Right")) EmitSignal(SignalName.Move, Vector2.Right);
                if (Input.IsActionJustPressed("Undo")) EmitSignal(SignalName.UndoRedo, -1);
				else if (Input.IsActionJustPressed("Redo")) EmitSignal(SignalName.UndoRedo, 1);
				else if(Input.IsActionJustPressed("Retry")) EmitSignal(SignalName.Retry);
            }
		}

		protected override void Dispose(bool pDisposing)
		{
			instance = null;
			base.Dispose(pDisposing);
		}
	}
}
