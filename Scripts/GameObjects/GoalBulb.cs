using Com.IsartDigital.SokoVolt.GameObjects.Movables;
using Com.IsartDigital.SokoVolt.Managers;
using Godot;
using System;
using System.Collections.Generic;

// Author : Auguste Paccapelo

namespace Com.IsartDigital.SokoVolt.GameObjects
{
	public partial class GoalBulb : GameObject
	{
		// ---------- VARIABLES ---------- \\

		// ----- Paths ----- \\

		// ----- Nodes ----- \\
		private GameManager gameManager;
		private GridManager gridManager;
		private CustomSignals signals;

		[Export] private Polygon2D onPol;
        [Export] private Polygon2D offPol;
		[Export] private Node2D allLights;
		[Export] private Light2D rotatingLight;

        // ----- Others ----- \\
        Action<float> doAction;

		public bool isTurnedOn { get; private set; }
		private float lightRotatingSpeed = 30f;

		// ---------- FUNCTIONS ---------- \\

		// ----- Ready & Process ----- \\

		public override void _Ready()
		{
			base._Ready();
			signals = CustomSignals.GetInstance();
            signals.BoxTeslaMoved += BoxTeslaMoved;

            gameManager = GameManager.GetInstance();
			gameManager.AddGoalBulb(this);
            gridManager = GridManager.GetInstance();

			BoxTeslaMoved();
        }

		public override void _Process(double pDelta)
		{
			float lDelta = (float)pDelta;

			base._Process(lDelta);

			doAction(lDelta);
		}

		// ----- My Functions ----- \\

		private void InitTurnedOff()
		{
            doAction = TurnedOff;
			isTurnedOn = false;
			onPol.Hide();
			offPol.Show();
			allLights.Hide();
            signals.EmitSignal(CustomSignals.SignalName.GoalBulbStateChanged);
        }

		private void TurnedOff(float pDelta)
		{

		}

		private void InitTurnOn()
		{
            doAction = TurnedOn;
			isTurnedOn = true;
			offPol.Hide();
			onPol.Show();
			allLights.Show();
            signals.EmitSignal(CustomSignals.SignalName.GoalBulbStateChanged);
        }

		private void TurnedOn(float pDelta)
		{
			rotatingLight.Rotation += Mathf.DegToRad(lightRotatingSpeed) * pDelta;
		}

		private void BoxTeslaMoved()
		{
			Cell[,] lGrid = gridManager.grid;

			if (HasConnectedTeslaClose(lGrid)) InitTurnOn();
			else InitTurnedOff();
        }

		private bool HasConnectedTeslaClose(Cell[,] pGrid)
		{
			int lLength = pGrid.GetLength(0);
			int lHeight = pGrid.GetLength(1);

			for (int j = -1; j < 2; j++)
			{
                if (y + j < 0 || y + j >= lLength) continue;

                for (int i = -1; i < 2; i++)
                {
                    if (x + i < 0 || x + i >= lLength) continue;

					GameObject lContent = pGrid[x + i, y + j].GetContent();

					if (lContent is BoxTesla lBoxTesla) return true;
                }
            }

			return false;
		}

        // ----- Destructor ----- \\

        protected override void Dispose(bool pDisposing)
        {
            if (pDisposing)
            {
                signals.BoxTeslaMoved -= BoxTeslaMoved; // Déconnecte le signal pour éviter qu'il appelle un objet supprimé
            }
            base.Dispose(pDisposing);
        }

    }
}
