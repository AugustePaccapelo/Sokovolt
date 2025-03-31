using Godot;
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
        [Export] public Vector2 startPoint;
        [Export] public float unitSize = 91.648f;

        [Export] public Vector2 endPoint;
        public Vector2 vectorDirector;
        [Export] public int numTurn = 3;
        private Vector2 direction;

        // Ligthnings Parameters
        [Export] private float minAngle = 30f;
        [Export] private float maxAngle = 80f;
        [Export] private float spawnSpeed = 5f;
        [Export] private float movingSpeed = 3f;
        [Export] private float destroyingSpeed = 5f;
        [Export] private float oscillatingSpeed = 5f;
        [Export] private int numLigthning = 3;
        [Export] private int marginStart = 15;
        [Export] private int width = 35;
        [Export] private float randomRatioLengthMin = 0.5f;
        [Export] private float randomRatioLengthMax = 2f;

        [Export] public float lifeTime = 0f;

        private List<Color> allColors = new List<Color>
        {
            Colors.DeepSkyBlue, Colors.Blue, Colors.DarkBlue,
        };

        private RandomNumberGenerator rand = new RandomNumberGenerator();

        // Signals to communicate lighning state
        [Signal] public delegate void SpawnFinishedEventHandler();
        [Signal] public delegate void DestructionFinishedEventHandler();

        private int numLightningSpawned = 0;
        private int numLightningDestructed = 0;

        // ---------- FUNCTIONS ---------- \\

        // ----- My Functions ----- \\
        public void StartLightning()
        {
            rand.Randomize();

            numLightningSpawned = 0;
            numLightningDestructed = 0;
            GlobalPosition = startPoint;
            vectorDirector = (endPoint - startPoint);
            startPoint = Vector2.Zero;
            Rotation = vectorDirector.Angle();

            direction = Vector2.Right;
            endPoint = direction * vectorDirector.Length();
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
            pSingleLightning.vectorDirector = vectorDirector;
            pSingleLightning.endPoint = endPoint;

            pSingleLightning.spawningSpeed = spawnSpeed;
            pSingleLightning.movingSpeed = movingSpeed;
            pSingleLightning.destroyingSpeed = destroyingSpeed;

            pSingleLightning.marginStart = marginStart;
            pSingleLightning.width = width;
            pSingleLightning.lifeTime = lifeTime;
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

        public Vector2 CalculateFirstPoint(List<Vector2> pListPoints, SingleLightning pLightning)
        {
            // Get first point
            pLightning.nextPoint = pListPoints[1] - pLightning.nextPointVector;
            
            Vector2 lPoint = pLightning.nextPoint;
            
            float lRatio = lPoint.X / marginStart;
            lPoint.X = lPoint.X < marginStart ? lPoint.X : marginStart;
            lPoint.Y *= lRatio;

            // Limit the Y to a set width
            if (lPoint.Y < -width) lPoint.Y = -width;
            if (lPoint.Y > width) lPoint.Y = width;
            
            // If the point has passed the startPoint, adding it to the list of all points and Create a new Vector
            if (lPoint.X >= marginStart)
            {
                //GD.Print(pLightning.nextPoint);
                pListPoints.Insert(1, lPoint);
                NewPointVector(pLightning);
            }
            return lPoint;
        }

        public void NewPointVector(SingleLightning pLightning)
        {
            // Create a new Vector for a new point
            Vector2 lVector = pLightning.nextPointVector;
            lVector = vectorDirector / (numTurn + 1);
            float lAngle = Mathf.DegToRad(rand.RandfRange(minAngle, maxAngle)) * pLightning.side;
            float lLength = rand.RandfRange(randomRatioLengthMin, randomRatioLengthMax) * lVector.Length();
            pLightning.side *= -1;
            lVector = PolarToCart(lLength, lAngle);
            pLightning.nextPointVector = lVector;
        }

        private Vector2 PolarToCart(float pRadius, float pAngle)
        {
            float lX = pRadius * Mathf.Cos(pAngle);
            float lY = pRadius * Mathf.Sin(pAngle);
            return new Vector2(lX, lY);
        }
    }
}
