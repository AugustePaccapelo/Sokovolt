using Com.IsartDigital.SokoVolt.Managers;
using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

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
		[Export] GpuParticles2D inTeslaParticles;
		private float timer;

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
			if (pTesla.playerCanBeDetected)
			{
				timer = 0;
				isTraveling = true;
				LevelLoader.playerCanMove = false;
                inTeslaParticles.Show();
                GD.Print("Player TP to NextTesla");
				dectetor.Monitorable = false;
				pTesla.playerCanBeDetected = false;
				MoveTo(pTesla.x, pTesla.y, GridManager.GetInstance().grid);
				GetTree().CreateTimer(1).Timeout += () => pTesla.playerCanBeDetected = true;
				GetTree().CreateTimer(0.5f).Timeout += () => LevelLoader.playerCanMove = true;
                isTraveling = false;
            }
            else if (pTesla.nextBoxTesla == null) GD.Print("nextTesla est null");
        }

        public override void MoveTo(int pX, int pY, Cell[,] pGrid)
        {
            base.MoveTo(pX, pY, pGrid);
			CustomSignals.GetInstance().EmitSignal(CustomSignals.SignalName.PlayerMoved);
        }

        public async void MoveAlongPath(List<Vector2> pPath)
        {
            if (pPath == null || pPath.Count == 0) return;

            foreach (Vector2 pStep in pPath)
            {
                MoveTo((int)pStep.X, (int)pStep.Y, GridManager.GetInstance().grid);
				GridManager.GetInstance().StockGridState();
                await ToSignal(GetTree().CreateTimer(0.2f), "timeout");
            }
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
