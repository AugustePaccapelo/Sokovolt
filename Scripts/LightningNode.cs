using Godot;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;

// Author : Auguste Paccapelo

namespace Com.IsartDigital.SokoVolt
{
	public partial class LightningNode : Node2D
	{
		// ---------- VARIABLES ---------- \\

		// ----- Paths ----- \\
		[Export] private PackedScene singleLighningScene;

		// ----- Nodes ----- \\
		public List<SingleLigthning> allLightningsList = new List<SingleLigthning>();

		// ----- Others ----- \\

		// Positions & direction
		[Export] public Vector2 startPoint = new Vector2(500f, 50f);
		[Export] public Vector2 direction = Vector2.One;
		[Export] public int numCellToTravel = 2;
		[Export] public Vector2 cellSize = new Vector2(150f, 150f);

        public Vector2 endPoint;
        public Vector2 vectorDirector;
        public int numTurn = 3;

		// Ligthnings Parameters
		[Export] private float minAngle = 10f;
		[Export] private float maxAngle = 40f;
		[Export] private float speed = 2f;
		[Export] private int numLigthning = 1;
		[Export] private int marginStart = 30;
        [Export] private int marginSide = 60;
        [Export] private float randomRatioLengthMin = 0.5f;
        [Export] private float randomRatioLengthMax = 2f;

        [Export] public float lifeTime = 0f;

        private List<Color> allColors = new List<Color>
        {
            Colors.Blue, Colors.DarkBlue, Colors.DeepSkyBlue,
        };

        // ---------- FUNCTIONS ---------- \\

        // ----- Ready & Process ----- \\

        public override void _Ready()
		{
			base._Ready();

			//endPoint = startPoint + numCellToTravel * cellSize * direction;
			direction = (endPoint - startPoint).Normalized();
			vectorDirector = direction * cellSize.X;

			SingleLigthning lSingleLightning;

		    for	(int i = 0; i < numLigthning; i++)
			{
				lSingleLightning = singleLighningScene.Instantiate<SingleLigthning>();
				allLightningsList.Add(lSingleLightning);

				lSingleLightning.startPoint = startPoint;
				lSingleLightning.direction = direction;
				lSingleLightning.vectorDirector = vectorDirector;
				lSingleLightning.endPoint = endPoint;
				lSingleLightning.cellSize = cellSize;

				lSingleLightning.minAngle = minAngle;
				lSingleLightning.maxAngle = maxAngle;
				lSingleLightning.speed = speed;
				lSingleLightning.numTurn = numTurn;
				lSingleLightning.side = i%2 == 0 ? 1 : -1;
				lSingleLightning.marginStart = marginStart;
				lSingleLightning.marginSide = marginSide;
				lSingleLightning.lifeTime = lifeTime;
				lSingleLightning.randomRatioLengthMin = randomRatioLengthMin;
				lSingleLightning.randomRatioLengthMax = randomRatioLengthMax;

				lSingleLightning.DefaultColor = allColors[i % allColors.Count];

				AddChild(lSingleLightning);
			}
		}

		public override void _Process(double pDelta)
		{
			float lDelta = (float)pDelta;

			base._Process(lDelta);
		}

		// ----- My Functions ----- \\

		// ----- Destructor ----- \\

		protected override void Dispose(bool pDisposing)
		{
			base.Dispose(pDisposing);
		}
	}
}
