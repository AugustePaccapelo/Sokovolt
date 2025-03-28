using Godot;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Com.IsartDigital.SokoVolt.Managers;
using System.Threading.Tasks;
using System.Data;
using System.Linq;
using static EnumSong;
using RobotnikSokoban.Scripts.Managers;

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

        
        ///////
        private Vector2 lastPlayerPos;
        private List<Vector2> lastPreviewTargets = new();
        private List<LightningNode> previewLines = new();
        

        public override void _Ready()
        {
           ConnectionManagers.boxTeslasList.Add(this);
            Init();
        }


        public override void _Process(double pDelta)
        {
            float lDelta = (float)pDelta;
            base._Process(pDelta);
            RayCastDetector(); 

            Player.GetInstance().MovableHaveFinish += (Movable _) => TryDisplayPreviewIfPlayerNearby();
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
            SongManager.Instance.ambientDict[EnumSong.AmbientSong.Piece].Play();

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
            energize = true;
            ClearPreviewLines(); 
            
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
                lightning.DestructionFinished += lightning.QueueFree;
            }
        }


        public void ShowPotentialConnections()
        {
            Cell[,] lGrid = gridManager.grid;
            Vector2 lPos = new(x, y);
            List<Vector2> lDirections = new(directionScan);
            List<Vector2> lNewTargets = new();

            foreach (Vector2 lDir in lDirections)
            {
                for (int i = 1; i <= range + 1; i++)
                {
                    Vector2 lScanPos = lPos + lDir * i;

                    int lX = (int)lScanPos.X;
                    int lY = (int)lScanPos.Y;

                    if (lX < 0 || lX >= lGrid.GetLength(0) || lY < 0 || lY >= lGrid.GetLength(1))
                        break;

                    GameObject lContent = lGrid[lX, lY].GetContent();

                    if (lContent is Wall)
                        break;

                    if (lContent is BoxTesla || lContent is GoalBulb || lContent is Generator)
                    {
                        lNewTargets.Add(lScanPos);
                        break;
                    }

                    if (i == range + 1)
                    {
                        lNewTargets.Add(lScanPos);
                        break;
                    }
                }
            }

            if (lNewTargets.SequenceEqual(lastPreviewTargets)) return;

            ClearPreviewLines();
            lastPreviewTargets = lNewTargets;

            foreach (Vector2 lTarget in lNewTargets)
            {
                LightningNode lPreview = lightningNodeScene.Instantiate<LightningNode>();
                lPreview.startPoint = GlobalPosition;
                lPreview.endPoint = Utils.SetPosition(this, (int)lTarget.X, (int)lTarget.Y, false);
                lPreview.SetPreview(true);
                lPreview.ZIndex = 50; 
                AddChild(lPreview);
                lPreview.StartLightning();
                previewLines.Add(lPreview);
            }
        }





        public void TryDisplayPreviewIfPlayerNearby()
        {
            if (energize)
            {
                ClearPreviewLines();
                return;
            }

            Vector2 lPlayerPos = new(Player.GetInstance().x, Player.GetInstance().y);

            if (lPlayerPos == lastPlayerPos) return;
            lastPlayerPos = lPlayerPos;

            Vector2 lDelta = lPlayerPos - new Vector2(x, y);

            if ((Mathf.Abs(lDelta.X) == 1 && lDelta.Y == 0) || (Mathf.Abs(lDelta.Y) == 1 && lDelta.X == 0))
                ShowPotentialConnections();
            else
                ClearPreviewLines();
        }



        public void ClearPreviewLines()
        {
            foreach (var l in previewLines)
                l.QueueFree();

            previewLines.Clear();
            lastPreviewTargets.Clear();
        }




        protected override void Dispose(bool pDisposing)
        {
        }
    }
}


