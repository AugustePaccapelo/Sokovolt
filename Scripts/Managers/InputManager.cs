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

        public override void _Input(InputEvent @event) // to optimize
        {

            if (@event is InputEventKey pEventKey && pEventKey.Pressed && LevelLoader.playerCanMove)
            {
                Vector2 lMousePos = pMouseEvent.Position;
                Vector2 lTargetPos = IsoManager.IsoViewToModel(lMousePos - GridManager.gridOffset);
                GridManager.GetInstance().HandleCellClick(lTargetPos);
            }

            if (@event is InputEventKey pEventKey && pEventKey.Pressed)
			{
                if (Input.IsActionJustPressed("Up")) CustomSignals.GetInstance().EmitSignal(CustomSignals.SignalName.Move, Vector2.Up);
                if (Input.IsActionJustPressed("Down")) CustomSignals.GetInstance().EmitSignal(CustomSignals.SignalName.Move, Vector2.Down);
                if (Input.IsActionJustPressed("Left")) CustomSignals.GetInstance().EmitSignal(CustomSignals.SignalName.Move, Vector2.Left);
                if (Input.IsActionJustPressed("Right")) CustomSignals.GetInstance().EmitSignal(CustomSignals.SignalName.Move, Vector2.Right);
                if (Input.IsActionJustPressed("Undo")) CustomSignals.GetInstance().EmitSignal(CustomSignals.SignalName.UndoRedo, -1);
                else if (Input.IsActionJustPressed("Redo")) CustomSignals.GetInstance().EmitSignal(CustomSignals.SignalName.UndoRedo, 1);
                else if (Input.IsActionJustPressed("Retry")) CustomSignals.GetInstance().EmitSignal(CustomSignals.SignalName.Retry);
            }
        }

        protected override void Dispose(bool pDisposing)
        {
            instance = null;
            base.Dispose(pDisposing);
        }
    }
}
