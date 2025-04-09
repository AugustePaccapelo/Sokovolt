using Godot;
using System;
using System.Collections.Generic;

// Author : Auguste Paccapelo

namespace Com.IsartDigital.SokoVolt
{
    public partial class SingleLightning : Line2D
    {
        // ---------- VARIABLES ---------- \\

        // ----- Nodes ----- \\
        [Export] private Line2D interLine;
        public LightningNode parentLightning;

        // ----- Others ----- \\
        public Vector2 vectorDirector;
        public Vector2 endPoint;

        public float spawningSpeed;
        public float movingSpeed;
        public float destroyingSpeed;

        public int marginStart;
        public int width;
        public float lifeTime;
        public float innerLineWidth;
        public Color innerColor;

        public int side;
        private float currentLifeTime = 0f;
        public Vector2 nextPoint;
        public Vector2 nextPointVector;
        private List<Vector2> allPointsList = new List<Vector2>();
        private List<Vector2> spawningPoints = new List<Vector2>();

        private Action<float> currentState;

        // ---------- FUNCTIONS ---------- \\

        // ----- Ready & Process ----- \\

        public override void _Ready()
        {
            base._Ready();

            parentLightning = GetParent<LightningNode>();

            Closed = false;
            ClearPoints();

            // Adding Startpoint and the first point
            parentLightning.NewPointVector(this);

            spawningPoints.Add(Vector2.Zero);

            Vector2 lFirstPoint = new Vector2(marginStart, 0);

            spawningPoints.Add(lFirstPoint);

            currentState = Spawning;

            interLine.Width = innerLineWidth;
            interLine.DefaultColor = innerColor;
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
            // Moving all points
            int lLength = spawningPoints.Count;
            for (int i = 1; i < lLength; i++) spawningPoints[i] += vectorDirector * spawningSpeed * pDelta;

            // Calulating next point
            Vector2 lPoint = parentLightning.CalculateFirstPoint(spawningPoints, this);

            Points = spawningPoints.ToArray();

            AddPoint(lPoint, 1);

            // If first point reach the end, lighning is going in state Moving
            if (spawningPoints[spawningPoints.Count - 1].X >= endPoint.X - marginStart)
            {
                currentState = Moving;
                allPointsList = spawningPoints;
                parentLightning.NewPointVector(this);
                GetParent<LightningNode>().NewLightningSpawned();
            }
        }
        private void Moving(float pDelta)
        {
            // Move infitely all points
            ClearPoints();

            MovePoints(pDelta, movingSpeed);

            // Calculating first point
            Vector2 lPoint = parentLightning.CalculateFirstPoint(spawningPoints, this);

            Points = allPointsList.ToArray();

            AddPoint(lPoint, 1);

            if (lifeTime != -1 && currentLifeTime >= lifeTime)
            {
                currentState = Destructing;
            }
        }
        private void Destructing(float pDelta)
        {
            ClearPoints();

            MovePoints(pDelta, destroyingSpeed);

            allPointsList[0] += vectorDirector * destroyingSpeed * pDelta;

            Points = allPointsList.ToArray();

            if (allPointsList.Count <= 2)
            {
                GetParent<LightningNode>().NewLightningDestroyed();
                GetParent<LightningNode>().allLightningsList.Remove(this);
                QueueFree();
            }
        }
        private void MovePoints(float pDelta, float pSpeed)
        {
            // Move all points
            for (int i = allPointsList.Count - 2; i > 0; i--) allPointsList[i] += vectorDirector * pSpeed * pDelta;

            // Delete points when reaching endPoint
            int lLastIndex = allPointsList.Count - 2;
            if (allPointsList[lLastIndex].X >= endPoint.X - marginStart)
            {
                allPointsList.RemoveAt(lLastIndex);
            }
        }
    }
}