using Godot;
using System;
using System.Collections.Generic;
using RobotnikSokoban.Scripts.Managers;
using Com.IsartDigital.SokoVolt.GameObjects.Movables;

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

        CustomSignals customSignals;

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
            customSignals = CustomSignals.GetInstance();
        }

        public override void _Input(InputEvent @event) // to optimize
        {
            if (!LevelLoader.playerCanMove) return;

            if(@event is InputEventMouseButton pMouseEvent && pMouseEvent.Pressed && pMouseEvent.ButtonIndex == MouseButton.Left)
            {
                Vector2 lMousePos = pMouseEvent.Position;
                Vector2 lTargetPos = IsoManager.IsoViewToModel(lMousePos - GridManager.gridOffset);
                GridManager.GetInstance().HandleCellClicked(lTargetPos);
                SongManager.Instance.ambientDict[EnumSong.AmbientSong.click].Play();
            }

            if (@event is InputEventKey pEventKey && pEventKey.Pressed && LevelLoader.playerCanMove)
			{
                if (Input.IsActionJustPressed("Up")) customSignals.EmitSignal(CustomSignals.SignalName.Move, Vector2.Up);
                if (Input.IsActionJustPressed("Down")) customSignals.EmitSignal(CustomSignals.SignalName.Move, Vector2.Down);
                if (Input.IsActionJustPressed("Left")) customSignals.EmitSignal(CustomSignals.SignalName.Move, Vector2.Left);
                if (Input.IsActionJustPressed("Right")) customSignals   .EmitSignal(CustomSignals.SignalName.Move, Vector2.Right);
                if (Input.IsActionJustPressed("Undo")) customSignals.EmitSignal(CustomSignals.SignalName.UndoRedo, -1);
                else if (Input.IsActionJustPressed("Redo")) customSignals.EmitSignal(CustomSignals.SignalName.UndoRedo, 1);
                else if (Input.IsActionJustPressed("Retry")) customSignals.EmitSignal(CustomSignals.SignalName.Retry);
            }
        }

        protected override void Dispose(bool pDisposing)
        {
            instance = null;
            base.Dispose(pDisposing);
        }
    }
}
