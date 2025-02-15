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
		GameManager gameManager;
		GridManager gridManager;
		CustomSignals signals;

		// ----- Others ----- \\

		// ---------- FONCTIONS ---------- \\

		// ----- Constructor & Ready & Process ----- \\

		protected GoalBulb () : base()
		{
            signals = CustomSignals.GetInstance();
            signals.AllManagersReady += Init;
        }

		public override void _Ready()
		{
			base._Ready();
            signals.BoxTeslaMoved += BoxTeslaMoved;
		}

		public void Init()
		{
            gameManager = GameManager.GetInstance();
			gameManager.AddGoalBulb(this);
			gridManager = GridManager.GetInstance();
		}

		public override void _Process(double pDelta)
		{
			float lDelta = (float)pDelta;

			base._Process(lDelta);
		}

		// ----- My Fonctions ----- \\

		private void BoxTeslaMoved()
		{
			Cell[,] lGrid = gridManager.grid;
			List<BoxTesla> lAllTeslasClose = GetCloseTeslas(lGrid);

			if (lAllTeslasClose.Count > 0) GD.Print(lAllTeslasClose.Count);
        }

		private List<BoxTesla> GetCloseTeslas(Cell[,] pGrid)
		{
			int lLength = pGrid.GetLength(1);
			int lHeight = pGrid.GetLength(0);

			List<BoxTesla> lCloseTeslas = new List<BoxTesla>();

			for (int j = -1; j < 2; j++)
			{
                if (y + j < 0 || y + j >= lLength) continue;

                for (int i = -1; i < 2; i++)
                {
                    if (x + i < 0 || x + i >= lLength) continue;

					GameObject lContent = pGrid[x + i, y + j].GetContent();

					if (lContent is BoxTesla lBoxTesla) lCloseTeslas.Add(lBoxTesla);
                }
            }

			return lCloseTeslas;
		}

		// ----- Destructor ----- \\

		protected override void Dispose(bool pDisposing)
		{
			base.Dispose(pDisposing);
		}
	}
}
