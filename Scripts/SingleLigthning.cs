using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;

// Author : Auguste Paccapelo & Sara Astuti-Aucher

namespace Com.IsartDigital.SokoVolt
{
    public partial class SingleLigthning : Line2D
    {
        // ---------- VARIABLES ---------- \\

        // ----- Paths ----- \\

        // ----- Nodes ----- \\
        [Export] private Line2D interLine;

        // ----- Others ----- \\

        public Vector2 startPoint;
        public Vector2 direction;
        public Vector2 vectorDirector;
        public Vector2 endPoint;
        public Vector2 cellSize;

        public float minAngle;
        public float maxAngle;
        public float speed;
        public int numTurn;
        public int side;
        public int marginStart;
        public int marginSide;
        public float lifeTime;
        public float randomRatioLengthMin;
        public float randomRatioLengthMax;

        private float currentLifeTime = 0f;
        private Vector2 nextPoint;
        private Vector2 nextPointVector;
        private RandomNumberGenerator rand = new RandomNumberGenerator();
        private List<Vector2> allPointsList = new List<Vector2>();
        private List<Vector2> spawningPoints = new List<Vector2>();

        private Action<float> currentState;

        // ---------- FUNCTIONS ---------- \\

        // ----- Ready & Process ----- \\

        public override void _Ready()
        {
            base._Ready();
            Closed = false;
            rand.Randomize();
            ClearPoints();

            NewPointVector();

            spawningPoints.Add(startPoint);

            Vector2 lFirstPoint = CalculateIntersection(startPoint + Vector2.Right.Rotated(vectorDirector.Angle()) * marginStart,
                vectorDirector.Angle() + Mathf.Pi * 0.5f * side, endPoint, startPoint);

            spawningPoints.Add(lFirstPoint);

            currentState = Spawning;

            interLine.Width = Width - 4.5f;
        }

        public override void _Process(double pDelta)
        {
            float lDelta = (float)pDelta;

            base._Process(lDelta);

            currentLifeTime += lDelta;

            currentState(lDelta);

            interLine.Points = Points;
        }

        // ----- My Functions ----- \\

        private void Spawning(float pDelta)
        {
            int lLength = spawningPoints.Count;

            for (int i = 1; i < lLength; i++)
            {
                spawningPoints[i] += vectorDirector * speed * pDelta;
            }

            Vector2 lPoint = CalculateFirstPoint(spawningPoints);

            Points = spawningPoints.ToArray();

            AddPoint(lPoint, 1);

            if ((spawningPoints[spawningPoints.Count - 1] - startPoint).Length() >= (endPoint - startPoint).Length())
            {
                currentState = Moving;
                allPointsList = spawningPoints;
                NewPointVector();
            }
        }

        private void Moving(float pDelta)
        {
            ClearPoints();

            MovePoints(pDelta);

            Vector2 lPoint = CalculateFirstPoint(allPointsList);

            Points = allPointsList.ToArray();

            AddPoint(lPoint, 1);

            if (lifeTime != 0 && currentLifeTime >= lifeTime)
            {
                currentState = Destructing;
            }
        }

        private void Destructing(float pDelta)
        {
            ClearPoints();

            MovePoints(pDelta);

            allPointsList[0] += vectorDirector * speed * pDelta;

            Points = allPointsList.ToArray();

            if (allPointsList.Count <= 2)
            {
                GD.Print("Finished");
                GetParent<LightningNode>().allLightningsList.Remove(this);
                QueueFree();
            }
        }

        private void MovePoints(float pDelta)
        {
            Vector2 lPoint;

            for (int i = allPointsList.Count - 2; i > 0; i--)
            {
                allPointsList[i] += vectorDirector * speed * pDelta;

                lPoint = CalculateIntersection(endPoint, vectorDirector.Angle() + Mathf.Pi * 0.5f, 
                    allPointsList[i], endPoint);
                
                if ((allPointsList[i] - lPoint).Dot(vectorDirector) > 0f)
                {
                    allPointsList.RemoveAt(i);
                }
            }
        }

        private Vector2 CalculateFirstPoint(List<Vector2> pListPoints)
        {
            Vector2 lPoint;

            nextPoint = pListPoints[1] - nextPointVector;
            lPoint = CalculateIntersection(startPoint + Vector2.Right.Rotated(vectorDirector.Angle()) * marginStart, 
                vectorDirector.Angle() + Mathf.Pi * 0.5f, pListPoints[1], nextPoint);

            Vector2 lProjOrtho = CalculateIntersection(lPoint, vectorDirector.Angle() + Mathf.Pi * 0.5f, startPoint, endPoint);

            float lVectorLength = (lPoint - lProjOrtho).Length();

            if (lVectorLength > cellSize.X * 0.5f - marginSide)
            {
                Vector2 lDirectionToPoint = (lPoint - lProjOrtho).Normalized();
                lPoint = lProjOrtho + lDirectionToPoint * (cellSize.X * 0.5f - marginSide);
            }

            if ((pListPoints[1] - lPoint).Length() >= nextPointVector.Length())
            {
                pListPoints.Insert(1, lPoint);
                NewPointVector();
            }
            return lPoint;
        }

        private Vector2 CalculateIntersection(Vector2 pD1Point, float pD1Angle, Vector2 pD2PointA, Vector2 pD2PointB)
        {
            float lD1Tan = Mathf.Tan(pD1Angle);
            float p = pD1Point.Y - lD1Tan * pD1Point.X;
            float lXTemp = pD2PointB.X - pD2PointA.X;
            if (lXTemp == 0) return pD2PointB;

            float m = (pD2PointB.Y - pD2PointA.Y) / lXTemp;
            float b = pD2PointA.Y - m * pD2PointA.X;

            float x = (b - p) / (lD1Tan - m);
            float y = m * x + b;
            return new Vector2(x, y);
        }

        //Full random
        private void NewPointVector()
        {
            nextPointVector = vectorDirector / (numTurn + 1);
            nextPointVector *= rand.RandfRange(randomRatioLengthMin, randomRatioLengthMax);
            float lAngle = Mathf.DegToRad(rand.RandfRange(minAngle, maxAngle)) * side;
            side *= -1;
            nextPointVector = nextPointVector.Rotated(lAngle);
        }

        //Angle random
        /*private void NewPointVector()
        {
            nextPointVector = vectorDirector / (numTurn + 1);
            float lAngle = Mathf.DegToRad(rand.RandfRange(minAngle, maxAngle));
            float minRad = Mathf.DegToRad(minAngle);
            float maxRad = Mathf.DegToRad(maxAngle);

            float minRatio = 0.5f;
            float maxRatio = 2f;
            float ratio = Mathf.Lerp(minRatio, maxRatio, (lAngle - minRad) / (maxRad - minRad));

            nextPointVector *= ratio;

            lAngle *= side;
            side *= -1;
            nextPointVector = nextPointVector.Rotated(lAngle);
        }*/

        // ----- Destructor ----- \\

        protected override void Dispose(bool pDisposing)
        {
            base.Dispose(pDisposing);
        }
    }
}