using Godot;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Com.IsartDigital.SokoVolt.Managers;
using System.Threading.Tasks;
using System.Data;

// Author : Soukai William

namespace Com.IsartDigital.SokoVolt.GameObjects.Movables
{

    public partial class BoxTesla : Movable
    {
        [Signal] public delegate void PlayerCollideEventHandler(BoxTesla lTesla);
        RayCast2D rayCast;
        static List<BoxTesla> boxTeslasList = new List<BoxTesla>();
        [Export] private Line2D electriLine2D;
        public BoxTesla nextBoxTesla = null;
        [Export] public bool energize { get; private set; }
        GridManager gridManager = GridManager.GetInstance();
        private List<BoxTesla> bonxInRangeList;

        private List<Vector2> directionScan = new List<Vector2>()
        {
            Vector2.Up,
            new Vector2(1, 1),
            new Vector2(-1, 1),
            new Vector2(1, -1),
            new Vector2(-1, -1),
            Vector2.Down,
            Vector2.Left,
            Vector2.Right
        };

        private int length;
        private bool signalEmit = false;
        public bool playerCanBeDetected = true;
        Vector2 LastPos = Vector2.Zero;

        //Range tesla gestion 
        public int range { get; private set; }
        [Export] private Label rangeLabel;

        public override void _Ready()
        {
            Init();

        }


        public override void _Process(double pDelta)
        {
            float lDelta = (float)pDelta;
            base._Process(pDelta);
            RayCastDetector(); 
        }

        private void Init()
        {
            CallDeferred(nameof(UpdateRangeLabel));
            length = directionScan.Count;
            MovableHaveFinish += (Movable pSender) => { Searching(pSender);};

        }

        private void UpdateRangeLabel()
        {
            rangeLabel.Text = range.ToString();
        }


        public void SetRange(int pRange)
        {
            range = pRange;
        }


        public override void MoveTo(int pX, int pY, Cell[,] pGrid)
        {

            base.MoveTo(pX, pY, pGrid);
            CustomSignals lSignals = CustomSignals.GetInstance();
            lSignals.EmitSignal(CustomSignals.SignalName.BoxTeslaMoved);

        }

        private void Searching(Movable pMovable)
        {
            if (LastPos != Utils.GetCellPos(this) && pMovable is BoxTesla)
            {
                LastPos = Utils.GetCellPos(this);
                ConnectionSearch();
            }
        }

        private RayCast2D CreateRayCast(Vector2 pPos, Vector2 pTargetPos)
        {
            RayCast2D lRay = new RayCast2D();
            AddChild(lRay);
            PlayerCollide += Player.GetInstance().InsideTesla;
            lRay.Position = pPos;
            lRay.TargetPosition = pTargetPos;
            lRay.CollideWithAreas = true;
            return lRay;
        }

        public void ConnectionSearch()
        {

            Cell[,] lGrid = gridManager.grid;
            Vector2 lCellPosition = Utils.GetCellPos(this);
            List<Vector2> lcurentDirectionScan = new List<Vector2>(directionScan);
            List<int> lIndicesToRemove = new List<int>();
            bonxInRangeList = new List<BoxTesla>();
            GameObject ObjToConecte = null;

            for (int i = 1; i <= range + 1; i++)
            {
                for (int j = lcurentDirectionScan.Count - 1; j >= 0; j--)
                {
                    Vector2 scanPos = lCellPosition + lcurentDirectionScan[j] * i;
                    int x = (int)scanPos.X;
                    int y = (int)scanPos.Y;

                    if (x < 0 || x >= lGrid.GetLength(0) || y < 0 || y >= lGrid.GetLength(1))
                        continue;

                    GameObject GOToScan = lGrid[x, y].GetContent();

                    switch (GOToScan)
                    {
                        case BoxTesla lTesla:
                            if (lTesla.energize && ObjToConecte == null)
                            {
                                ObjToConecte= lTesla;
                                boxTeslasList.Add(this);
                                nextBoxTesla = lTesla;
                            }
                            else
                            {
                                bonxInRangeList.Add(lTesla);
                            }

                            break;
                        case Generator:
                            if (ObjToConecte==null)
                            {
                                ObjToConecte= GOToScan;
                            }


                            break;
                        case Wall:
                            lIndicesToRemove.Add(j);
                            break;
                    }


                }
                lIndicesToRemove.Sort((a, b) => b.CompareTo(a));
                foreach (int index in lIndicesToRemove)
                {
                    if (index >= 0 && index < lcurentDirectionScan.Count)
                        lcurentDirectionScan.RemoveAt(index);
                }

                lIndicesToRemove.Clear();
            }

            LineDeconnection();
            energize = false;
            nextBoxTesla = null;
            if (ObjToConecte != null)
            {
                LineConnection(ObjToConecte);
                BoxToUpdated();
            }
            else
            {
                LineDeconnection();
                energize = false;
                nextBoxTesla = null;   
            }
            GD.Print("in range"+bonxInRangeList.Count);
        }
        private void RayCastDetector()
        {
            if (rayCast != null && !signalEmit && rayCast.IsColliding() && playerCanBeDetected)
            {
                GodotObject lArea = rayCast.GetCollider();
                if (IsInstanceValid(lArea))
                {
                    EmitSignal(nameof(PlayerCollide), this);
                    GD.Print("Emit signal");
                    signalEmit = true;
                }
            }
            else if (rayCast != null && !rayCast.IsColliding()) signalEmit = false;
        }

        private void LineConnection(GameObject objToConnect)
        {
            int lPointCount = electriLine2D.GetPointCount();
            if (electriLine2D.GetPointCount() >= 1) LineDeconnection();
            GD.Print(electriLine2D.GetPointCount());
            electriLine2D.AddPoint(ToLocal(objToConnect.GlobalPosition), 1);
            energize = true;
            rayCast = CreateRayCast(electriLine2D.Points[0], electriLine2D.Points[1]);
            electriLine2D.Visible = true;
        }



        private void LineDeconnection()
        {
            if(rayCast != null) rayCast.QueueFree();
            electriLine2D.Visible = false;
            if (electriLine2D.GetPointCount() > 1) electriLine2D.RemovePoint(1);

        }

        private void BoxToUpdated()
        {

            foreach (var BOX in bonxInRangeList)
            {
                BOX.ConnectionSearch();
            }
            
        }

        protected override void Dispose(bool pDisposing)
        {

        }
    }
}


