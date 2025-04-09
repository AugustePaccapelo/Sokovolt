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
		[Export] public GpuParticles2D inTeslaParticles, bodyParticles;
		private float timer;
		public static bool canTravel = false;

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

			MovableHaveFinish += (pSender) => canTravel = true;
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
                GD.Print("Player TP to NextTesla");
				dectetor.Monitorable = false;
                //canTravel = false;
				MoveTo(pTesla.x, pTesla.y, GridManager.GetInstance().grid);
				//GetTree().CreateTimer(0.f).Timeout += () => canTravel = true;
				GetTree().CreateTimer(0.25f).Timeout += () => InputManager.canPlayerMove = true;
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
                if (IsQueuedForDeletion() || GetTree() == null) return;

				//MoveTo((int)pStep.X, (int)pStep.Y, GridManager.GetInstance().grid);
				CustomSignals.GetInstance().EmitSignal(CustomSignals.SignalName.Move, new Vector2(pStep.X - x, pStep.Y - y));
                GridManager.GetInstance().StockGridState();
                GridManager.GetInstance().PrintGrid();

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
