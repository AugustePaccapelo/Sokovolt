using Com.IsartDigital.SokoVolt.Managers;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Com.IsartDigital.SokoVolt.Tools;
using RobotnikSokoban.Scripts.Managers;

//Author : Ferlat Thibaud 
namespace Com.IsartDigital.SokoVolt.GameObjects.Movables {
	
	public partial class Player : Movable
	{
		#region GetInstance
		static private Player instance;
        static public Player GetInstance(int pX = 0, int pY = 0)
        {
            if (instance == null)
            {
                instance = new Player();
            }
            return instance;

        }
		private Player() : base(){}

		#endregion

		[Export] Area2D dectetor;
		[Export] public GpuParticles2D inTeslaParticles, bodyParticles,
        moveparticulr;
		private float timer;
		public static bool canTravel = false;
		private bool wasInTesla = false;

		public static bool isTraveling{get; private set;} 

		public override void _Ready()
		{
			#region instance
		if (instance != null){  
			QueueFree();
			GD.Print(nameof(Player) + " Instance already exist, destroying the last added.");
			return;
		}
		instance = this;
			#endregion
			HighlightManager.GetInstance()?.RegisterTarget("Player", this);

		}

		public override void _Process(double pDelta)
		{
			base._Process(pDelta);
			float lDelta = (float)pDelta;
			timer += lDelta;
			if (timer > 0.2f && inTeslaParticles.Visible) inTeslaParticles.Hide();
			if (timer > 3) timer = 0;
        }

        public void InsideTesla(BoxTesla pTesla)
		{
			if (canTravel)
			{
                timer = 0;
				isTraveling = true;
                InputManager.canPlayerMove = false;
                inTeslaParticles.Show();
				dectetor.Monitorable = false;
				MoveTo(pTesla.x, pTesla.y, GridManager.GetInstance().grid);
                SongManager.Instance.ambientDict[EnumSong.AmbientSong.playerTravel].Play();
                if (ConnectionManagers.lastTeslas.Contains(pTesla))
					wasInTesla = true;
					GetTree().CreateTimer(0.25f).Timeout += () => {
						InputManager.canPlayerMove = true;
						isTraveling = false;
						canTravel = false;
					};
			}
        }

        public override void MoveTo(int pX, int pY, Cell[,] pGrid)
        {
            base.MoveTo(pX, pY, pGrid);

			if(!GridManager.currentlyUndoRedo) SongManager.Instance.ambientDict[EnumSong.AmbientSong.playerMove].Play();
            CustomSignals.GetInstance().EmitSignal(CustomSignals.SignalName.PlayerMoved);
			//canTravel = false;
            moveparticulr.Emitting=true;
			if (wasInTesla) {
				canTravel = true;
				wasInTesla = false;
			}
        }

        public async void MoveAlongPath(List<Vector2> pPath)
        {
            if (pPath == null || pPath.Count == 0) return;

            foreach (Vector2 pStep in pPath)
            {
				if (IsQueuedForDeletion() || GetTree() == null) return;
				if (isTraveling) break;
                SongManager.Instance.ambientDict[EnumSong.AmbientSong.playerMove].Play();
                //MoveTo((int)pStep.X, (int)pStep.Y, GridManager.GetInstance().grid);
                CustomSignals.GetInstance().EmitSignal(CustomSignals.SignalName.Move, new Vector2(pStep.X - x, pStep.Y - y));
                //GridManager.GetInstance().StockGridState();
                //GridManager.GetInstance().PrintGrid();

                await ToSignal(GetTree().CreateTimer(0.2f), "timeout");
            }

			InputManager.canPlayerMove = true;
        }


        #region dispose
        protected override void Dispose(bool pDisposing)
		{
			if (pDisposing && instance == this) instance = null;
			base.Dispose(pDisposing);
		}
		#endregion
	}
}
