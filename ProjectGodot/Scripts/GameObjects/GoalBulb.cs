using Com.IsartDigital.SokoVolt.GameObjects.Movables;
using Com.IsartDigital.SokoVolt.Managers;
using Com.IsartDigital.SokoVolt.Tools;
using Godot;
using RobotnikSokoban.Scripts.Managers;
using System;

// Author : Auguste Paccapelo

namespace Com.IsartDigital.SokoVolt.GameObjects
{
	public partial class GoalBulb : GameObject
	{
		// ---------- VARIABLES ---------- \\

		// ----- Nodes ----- \\
		private GameManager gameManager;
		private GridManager gridManager;
		private CustomSignals signals;

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
            signals.BoxTeslaCalculsDone += BoxTeslaMoved;

            gameManager = GameManager.GetInstance();
			gameManager.AddGoalBulb(this);
            gridManager = GridManager.GetInstance();
			
			BoxTeslaMoved();
			
			HighlightManager.GetInstance()?.RegisterTarget("GoalBulb", this);
        }

		public override void _Process(double pDelta)
		{
            base._Process(pDelta);
            float lDelta = (float)pDelta;
			
			doAction(lDelta);
        }

		// ----- My Functions ----- \\

		private void InitTurnedOff()
		{
            doAction = TurnedOff;
			isTurnedOn = false;
            allLights.Hide();
            signals.EmitSignal(CustomSignals.SignalName.GoalBulbStateChanged);
        }
		private void TurnedOff(float pDelta) { }
		private void InitTurnOn()
		{
            doAction = TurnedOn;
			isTurnedOn = true;
			allLights.Show();
            SongManager.Instance.ambientDict[EnumSong.AmbientSong.bulbOn].Play();
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

					if (lContent is BoxTesla lBoxTesla)
						if (lBoxTesla.energize) return true;
                }
            }
			return false;
		}

        // ----- Destructor ----- \\

        protected override void Dispose(bool pDisposing)
        {
            if (pDisposing)
            {
				gameManager.RemoveGoalBulb(this);
                signals.BoxTeslaCalculsDone -= BoxTeslaMoved; // Déconnecte le signal pour éviter qu'il appelle un objet supprimé
            }
            base.Dispose(pDisposing);
        }
    }
}