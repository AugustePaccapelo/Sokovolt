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
        [Export] private PackedScene lightningNodeScene;
        LightningNode lightning;
        [Signal] public delegate void PlayerCollideEventHandler(BoxTesla lTesla);
        [Export] private RayCast2D rayCast;
        [Export] private Line2D electriLine2D;
        public BoxTesla nextBoxTesla = null;
        public BoxTesla prevBoxTesla = null;
        public bool energize = false;
        GridManager gridManager = GridManager.GetInstance();
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
        private bool signalEmit = false;
        public bool playerCanBeDetected = true;
        Vector2 LastPos = Vector2.Zero;
        //Range tesla gestion 
        public int range { get; private set; }
        [Export] private Label rangeLabel;

        public override void _Ready()
        {
           connectionManagers.boxTeslasList.Add(this);
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
            MovableHaveFinish += (Movable pSender) => { Searching(pSender);
            };
            CallDeferred(nameof(ConnectPlayer));
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
                CustomSignals lSignals = CustomSignals.GetInstance();
                lSignals.EmitSignal(CustomSignals.SignalName.StartRecherche);
            }
        }

        private void UpdateRayCast(Vector2 pTargetPos)
        {
            rayCast.TargetPosition = pTargetPos;
        }

        private void ConnectPlayer()
        {
            PlayerCollide += Player.GetInstance().InsideTesla;
        }

        public float ConnectionSearch(GameObject pObjectToConecte)
        {
            Cell[,] lGrid = gridManager.grid;
            Vector2 lCellPosition = Utils.GetCellPos(this);
            List<Vector2> lcurentDirectionScan = new List<Vector2>(directionScan);
            List<int> lIndicesToRemove = new List<int>();

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
                            if ( GOToScan == pObjectToConecte )
                            {
                                Vector2 lVector2 = new Vector2(pObjectToConecte.x - this.x,pObjectToConecte.y-this.y);
                              float lLength=lVector2.Length();

                              return lLength ;
                            }
                            else if (GOToScan is Wall)
                            {
                                lIndicesToRemove.Add(j);
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

            return -1;
        }
        private void RayCastDetector()
        {
            if (rayCast != null && !signalEmit && rayCast.IsColliding() && playerCanBeDetected && !GridManager.currentlyUndoRedo)
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

        public void LineConnection(GameObject objToConnect)
        {
            if (objToConnect is BoxTesla lbox)energize = true;
            
            lightning = lightningNodeScene.Instantiate<LightningNode>();
            lightning.endPoint = GlobalPosition;
            lightning.startPoint = objToConnect.GlobalPosition;
            AddChild(lightning);
            lightning.StartLightning();
            UpdateRayCast(ToLocal(objToConnect.GlobalPosition));
        }



        public void LineDeconnection()
        {
            energize = false;
            UpdateRayCast(Vector2.Zero);
            if (lightning != null)
            {
                lightning.StopLightning();
            }

        }

        protected override void Dispose(bool pDisposing)
        {
        }
    }
}


