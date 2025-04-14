using Godot;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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
        public Vector2 realEndPoint;
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
        [Export] private int width = 35;
        [Export] private float randomRatioLengthMin = 0.5f;
        [Export] private float randomRatioLengthMax = 2f;

        [Export] public float lifeTime = -1f;

        [Export] private Color[] allColors;
        [Export] private Color innerLineColor;

        [Export] float linesWidth = 6f;
        [Export] float innerLineWidth = 1.5f;

        private RandomNumberGenerator rand = new RandomNumberGenerator();

        // Signals to communicate lighning state
        [Signal] public delegate void SpawnFinishedEventHandler();
        [Signal] public delegate void DestructionFinishedEventHandler();

        private int numLightningSpawned = 0;
        private int numLightningDestructed = 0;

        private Color previewColor = new Color(1, 1, 1, 0.15f);
        private Color notPreviewColor = new Color(1, 1, 1, 1);

        //Lightning preview 
        [Export] private float connectDuration = 1.8f;  
        [Export] private float disconnectDuration = 1f;
        private Timer lightningTimer;
        private bool isConnected = false;
        public bool isPreview = false;
        private List<Vector2> initialPreviewPoints = new List<Vector2>();



        // ---------- FUNCTIONS ---------- \\

        // ----- My Functions ----- \\
        public void StartLightning()
        {
            rand.Randomize();

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

                lSingleLightning.DefaultColor = allColors[i % allColors.Length];

                lSingleLightning.Width = linesWidth;

                SetVariables(lSingleLightning);

                AddChild(lSingleLightning);
            }
        }
        private void SetVariables(SingleLightning pSingleLightning)
        {
            pSingleLightning.vectorDirector = vectorDirector;
            pSingleLightning.endPoint = realEndPoint;

            pSingleLightning.spawningSpeed = spawnSpeed;
            pSingleLightning.movingSpeed = movingSpeed;
            pSingleLightning.destroyingSpeed = destroyingSpeed;

            pSingleLightning.marginStart = marginStart;
            pSingleLightning.width = width;
            pSingleLightning.lifeTime = lifeTime;
            pSingleLightning.innerLineWidth = innerLineWidth;
            pSingleLightning.innerColor = innerLineColor;
        }
        public void StopLightning()
        {
            foreach (SingleLightning lLightning in allLightningsList)
            {
                lLightning.lifeTime = 0f;
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
           
            // Limit the Y to a set width
            if (lPoint.Y < -width) lPoint.Y = -width;
            if (lPoint.Y > width) lPoint.Y = width;
            
            // If the point has passed the startPoint, adding it to the list of all points and Create a new Vector
            if (lPoint.X >= marginStart)
            {
                pListPoints.Insert(1, lPoint);
                NewPointVector(pLightning);
            }

            
            if (lPoint.X < marginStart)
            {
                if (lPoint.X < 0) lPoint.X = 0;
                float lRatio = lPoint.X / marginStart;
                lPoint.X = marginStart;
                lPoint.Y *= lRatio;
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

       
        private void InitPreviewTimer()
        {
            lightningTimer = new Timer();
            AddChild(lightningTimer);
            lightningTimer.WaitTime = connectDuration;
            lightningTimer.OneShot = false;
            lightningTimer.Timeout += OnPreviewTimerFinished;
            lightningTimer.Start();
        }
        private void OnPreviewTimerFinished()
        {
             if (!isConnected)
            {
                startPoint = initialPreviewPoints[0]; 
                endPoint = initialPreviewPoints[1]; 
                StartLightning();  
                lightningTimer.WaitTime = connectDuration;  
            }
            else
            {
                StopLightning();  
                lightningTimer.WaitTime = disconnectDuration;  
            }

            isConnected = !isConnected; 
            lightningTimer.Start();  
        }
        public void SetPreview(bool pIsPreview)
        {
            isPreview = pIsPreview;
            if(isPreview)
            {   
                InitPreviewTimer(); 
                initialPreviewPoints.Clear(); 
                initialPreviewPoints.Add(startPoint); 
                initialPreviewPoints.Add(endPoint); 
            }
            Modulate = isPreview ? previewColor : notPreviewColor;
        }
    }
}
