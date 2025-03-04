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
		[Signal] public delegate void RedoEventHandler();

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
				var lInputMap = new Dictionary<string, Vector2>
				{
					{"ui_right", Vector2.Right},
					{"ui_left", Vector2.Left},
					{"ui_down", Vector2.Down},
					{"ui_up", Vector2.Up}
				};

				foreach (var pInput in lInputMap)
				{
					if(Input.IsActionJustPressed(pInput.Key))
					{
						EmitSignal(SignalName.Move, pInput.Value);
						break;
					}
				}
			}
		}

		protected override void Dispose(bool pDisposing)
		{
			instance = null;
			base.Dispose(pDisposing);
		}
	}
}
