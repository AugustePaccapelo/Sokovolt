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
        
        #region  Export 
        [Export] private PackedScene lightningNodeScene;
        [Export] private Line2D electriLine2D;
        [Export] private Marker2D connectionPoint;
        [Export] private Node2D visual;
        #endregion

        #region variables
        [Signal] public delegate void PlayerCollideEventHandler(BoxTesla lTesla);
        private RayCast2D rayCast;
        LightningNode lightning;
        public BoxTesla nextBoxTesla = null;
        public bool energize = false;
        GridManager gridManager = GridManager.GetInstance();
        private bool signalEmit = false;
        Vector2 LastPos = Vector2.Zero;
        #endregion
        
        #region directionScan
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
        #endregion
        
        #region  Shake_variables
        public bool canShake = false;
        private double shakeTimer = 0;
        private const double shakeInterval = 3.0;
        private const double shakeDuration = 1.0;
        private bool isShaking = false;
        private Tween shakeTweenPosition;
        private Tween shakeTweenRotation;
        #endregion

        #region Range tesla gestion 
        //Range tesla gestion 
        public int range { get; private set; }
        [Export] private Label rangeLabel;
        private Vector2 lastPlayerPos;
        private List<Vector2> lastPreviewTargets = new();
        private List<LightningNode> previewLines = new();
        #endregion

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
            
            #region shake
            //Handle shaking effect if not energized
            if (!energize)
            {
                shakeTimer += pDelta;

                if (!isShaking && shakeTimer >= shakeInterval)
                {
                    StartShake();
                    shakeTimer = 0;  
                }
                else if (isShaking && shakeTimer >= shakeDuration)
                {
                    StopShake();
                }
            }
            else
            {
                shakeTimer = 0;
                if (isShaking)
                    StopShake();
                ResetVisual();
            }
            #endregion
        }

        private void Init()
        {
            CallDeferred(nameof(UpdateRangeLabel));
            MovableHaveFinish += (Movable pSender) => Searching(pSender);
            CallDeferred(nameof(ConnectPlayer));
        }

        #region TeslaShake
        private void ResetVisual()
        {
            visual.RotationDegrees = 0;
            visual.Position = Vector2.Zero;
        }
        private void StartShake()
        {
            isShaking = true;

            shakeTweenPosition = AnimationManager.GetInstance()
                .ShakeEffect(visual, new Vector2(3, 1), 0.1f)
                .SetLoops(); // Rend l'effet continu jusqu'à l'arrêt

            shakeTweenRotation = AnimationManager.GetInstance()
                .RotationEffect(visual, Mathf.DegToRad(3), 0.1f, Tween.TransitionType.Linear, Tween.EaseType.In)
                .SetLoops();
        }
        public override void _ExitTree()
        {
            base._ExitTree();
            StopShake();
        }
        private void StopShake()
        {
            isShaking = false;

            if (shakeTweenPosition != null && shakeTweenPosition.IsValid())
                shakeTweenPosition.Kill();

            if (shakeTweenRotation != null && shakeTweenRotation.IsValid())
                shakeTweenRotation.Kill();

            ResetVisual();
        }
        #endregion


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

        #region Searchin
        //Searches for nearby connections after movement
        private void Searching(Movable pMovable)
        {
            if (LastPos != Utils.GetCellPos(this) && pMovable is BoxTesla)
            {
                LastPos = Utils.GetCellPos(this); 
                CustomSignals lSignals = CustomSignals.GetInstance();
                lSignals.EmitSignal(CustomSignals.SignalName.StartRecherche);
            }
        }
        #endregion
        #region ConnectionSearch

        /// <summary>
        /// Searches for a direct connection between this Tesla and another GameObject.
        /// </summary>
        /// <param name="pObjectToConecte">The target GameObject to connect with.</param>
        /// <returns>The distance if connected, or -1 if no connection is found.</returns>
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
                    Vector2 lScanPos = lCellPosition + lcurentDirectionScan[j] * i;
                    int x = (int)lScanPos.X;
                    int y = (int)lScanPos.Y;

                    if (x < 0 || x >= lGrid.GetLength(0) || y < 0 || y >= lGrid.GetLength(1))
                        continue;
                    GameObject lGOToScan = lGrid[x, y].GetContent();
                            if ( lGOToScan == pObjectToConecte )
                            {
                                Vector2 lVector2 = new Vector2(pObjectToConecte.x - this.x,pObjectToConecte.y-this.y);
                              float lLength=lVector2.Length();

                              return lLength ;
                            }
                            else if (lGOToScan is Wall)
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
        #endregion  
        
        #region  RayCastDetector
        //Detects if the player is within Tesla's raycast range
        /*private void UpdateRayCast(Vector2 pTargetPos)
        {
            rayCast.TargetPosition = pTargetPos;
        }*/
        private void CreateRaycast(Vector2 pTargetPos)
        {
            rayCast = new RayCast2D();
            AddChild(rayCast);
            rayCast.TargetPosition = pTargetPos;
            rayCast.CollideWithAreas = true;
            rayCast.CollideWithBodies = false;
        }

        private void DestroyRaycast()
        {
            rayCast?.QueueFree();
            rayCast = null;
        }

        private void ConnectPlayer()
        {
            PlayerCollide += Player.GetInstance().InsideTesla;
            Player.GetInstance().MovableHaveFinish += (Movable _) => TryDisplayPreviewIfPlayerNearby();
        }
        private void RayCastDetector()
        {
            if (rayCast != null && !signalEmit && rayCast.IsColliding() && Player.canTravel && !GridManager.currentlyUndoRedo)
            {
                GodotObject lArea = rayCast.GetCollider();
                if (IsInstanceValid(lArea))
                {
                    EmitSignal(nameof(PlayerCollide), this);
                    signalEmit = true;
                }
            }
            else if (rayCast != null && !rayCast.IsColliding()) signalEmit = false;
        }
        #endregion

        #region Line Management
        public void LineConnection(GameObject pObjToConnect)
        {
            energize = true;
            ClearPreviewLines(); 
            
            lightning = lightningNodeScene.Instantiate<LightningNode>();
            lightning.endPoint = connectionPoint.GlobalPosition;
            lightning.startPoint = pObjToConnect.GlobalPosition;
            AddChild(lightning);
            lightning.StartLightning();
            //UpdateRayCast(ToLocal(pObjToConnect.GlobalPosition));
            CreateRaycast(ToLocal(pObjToConnect.GlobalPosition));
        }



        public void LineDeconnection()
        {
            energize = false;
            //UpdateRayCast(Vector2.Zero);
            DestroyRaycast();
            if (lightning != null)
            {
                lightning.StopLightning();
                lightning.DestructionFinished += lightning.QueueFree;
            }
        }
        #endregion
        #region ShowPotentialConnections
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
                AddChild(lPreview);
                lPreview.startPoint = Utils.SetPosition(this, (int)lTarget.X, (int)lTarget.Y, false);
                lPreview.endPoint = GlobalPosition;
                lPreview.SetPreview(true);
                lPreview.ZIndex = 50; 
                lPreview.StartLightning();
                previewLines.Add(lPreview);
            }
        }
        #endregion

        #region TryDisplayPreviewIfPlayerNearby
        //Triggers a preview of potential connections if the player is nearby.

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
        #endregion

        protected override void Dispose(bool pDisposing)
        {
        }
    }
}