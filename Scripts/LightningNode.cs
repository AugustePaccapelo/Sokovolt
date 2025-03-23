using Godot;
using System;
using System.Collections.Generic;

// Author : Auguste Paccapelo

namespace Com.IsartDigital.SokoVolt
{
    public partial class LightningNode : Node2D
    {
        // ---------- VARIABLES ---------- \\

        // ----- Paths ----- \\
        [Export] private PackedScene singleLightningScene;

        // ----- Nodes ----- \\
        public List<SingleLightning> allLightningsList = new List<SingleLightning>();

        // ----- Others ----- \\

        // Positions & direction
        [Export] public Vector2 startPoint = new Vector2(500f, 50f);
        [Export] public float unitSize = 91.648f;

        [Export] public Vector2 endPoint;
        private Vector2 realEndPoint;
        public Vector2 vectorDirector;
        [Export] public int numTurn = 3;
        private Vector2 direction;

        // Ligthnings Parameters
        [Export] private float minAngle = 30f;
        [Export] private float maxAngle = 80f;
        [Export] private float spawnSpeed = 5f;
        [Export] private float movingSpeed = 3f;
        [Export] private float destroyingSpeed = 5f;
        [Export] private int numLigthning = 3;
        [Export] private int marginStart = 15;
        [Export] private int marginSide = 35;
        [Export] private float randomRatioLengthMin = 0.5f;
        [Export] private float randomRatioLengthMax = 2f;

        [Export] public float lifeTime = 0f;

        private List<Color> allColors = new List<Color>
        {
            Colors.Blue, Colors.DarkBlue, Colors.DeepSkyBlue,
        };

        // Signals to communicate lighning state
        [Signal] public delegate void SpawnFinishedEventHandler();
        [Signal] public delegate void DestructionFinishedEventHandler();

        private int numLightningSpawned = 0;
        private int numLightningDestructed = 0;

        // ---------- FUNCTIONS ---------- \\

        // ----- Ready & Process ----- \\

        public override void _Ready()
        {
            base._Ready();

            StartLightning();
        }

        public override void _Process(double pDelta)
        {
            float lDelta = (float)pDelta;

            base._Process(lDelta);
        }

        // ----- My Functions ----- \\
        public void StartLightning()
        {
            numLightningSpawned = 0;
            numLightningDestructed = 0;
            GlobalPosition = startPoint;
            vectorDirector = (endPoint - startPoint);
            Rotation = vectorDirector.Angle();

            direction = Vector2.Right;
            realEndPoint = direction * vectorDirector.Length();
            vectorDirector = direction * unitSize;

            SingleLightning lSingleLightning;

            for (int i = 0; i < numLigthning; i++)
            {
                lSingleLightning = singleLightningScene.Instantiate<SingleLightning>();
                allLightningsList.Add(lSingleLightning);

                lSingleLightning.side = i % 2 == 0 ? 1 : -1;

                lSingleLightning.DefaultColor = allColors[i % allColors.Count];

                SetVariables(lSingleLightning);

                AddChild(lSingleLightning);
            }
        }

        private void SetVariables(SingleLightning pSingleLightning)
        {
            pSingleLightning.startPoint = Vector2.Zero;
            pSingleLightning.direction = direction;
            pSingleLightning.vectorDirector = vectorDirector;
            pSingleLightning.endPoint = realEndPoint;
            pSingleLightning.unitSize = unitSize;

            pSingleLightning.minAngle = minAngle;
            pSingleLightning.maxAngle = maxAngle;
            pSingleLightning.spawningSpeed = spawnSpeed;
            pSingleLightning.movingSpeed = movingSpeed;
            pSingleLightning.destroyingSpeed = destroyingSpeed;
            pSingleLightning.numTurn = numTurn;
            pSingleLightning.marginStart = marginStart;
            pSingleLightning.marginSide = marginSide;
            pSingleLightning.lifeTime = lifeTime;
            pSingleLightning.randomRatioLengthMin = randomRatioLengthMin;
            pSingleLightning.randomRatioLengthMax = randomRatioLengthMax;
        }

        public void StopLightning()
        {
            foreach (SingleLightning lLightning in allLightningsList)
            {
                lLightning.lifeTime = 0.01f;
            }
        }

        public void NewLightningSpawned()
        {
            numLightningSpawned++;
            if (numLightningSpawned == numLigthning)
                EmitSignal(SignalName.SpawnFinished);
        }

        public void NewLightningDestroyed()
        {
            numLightningDestructed++;
            if (numLightningDestructed == numLigthning)
                EmitSignal(SignalName.DestructionFinished);
        }

        // ----- Destructor ----- \\

        protected override void Dispose(bool pDisposing)
        {
            base.Dispose(pDisposing);
        }
    }
}
