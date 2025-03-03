using Godot;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Com.IsartDigital.SokoVolt.Managers;
using System.Threading.Tasks;
using System.Data;

// Author : Soukai William

namespace Com.IsartDigital.SokoVolt.GameObjects.Movables {
	
        public partial class BoxTesla : Movable
        {
            static List<BoxTesla> boxTeslasList = new List<BoxTesla>();
            [Export] private Line2D electriLine2D;
            [Export] public bool  energize { get; private set; }
            GridManager gridManager = GridManager.GetInstance();
            private List<Vector2> directionScan = new List<Vector2>()
            {  
                Vector2.Up,
                new Vector2(1,1),
                new Vector2(-1,1),
                new Vector2(1,-1),
                new Vector2(-1,-1),
                Vector2.Down,
                Vector2.Left,
                Vector2.Right
            };
            private int length ;
            Vector2 LastPos = Vector2.Zero;

			//Range tesla gestion 
			public int range{get; private set;}
			[Export] private Label rangeLabel; 

            public override void _Ready()
            {
				Init(); 
               
            }


            public override void _Process(double pDelta)
            {
                float lDelta = (float)pDelta;
                base._Process(pDelta);
                
               
            }

			private void Init()
			{
			    CallDeferred(nameof(UpdateRangeLabel));
                length = directionScan.Count;
                MovableHaveFinish += (Movable pSender) => 
                {
                    Searching(pSender);
                };

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
                if (LastPos!=Utils.GetCellPos(this) && pMovable is BoxTesla)
                {
                   LastPos = Utils.GetCellPos(this);
                   ConnectionSearch();
                }
            }


            public void ConnectionSearch()
            {

                Cell[,] lGrid = gridManager.grid;
                Vector2 lCellPosition = Utils.GetCellPos(this);
                int lLength = length;
                List<Vector2> lcurentDirectionScan = new List<Vector2>(directionScan);
                List<int> indicesToRemove = new List<int>();

                for (int i =1 ; i <= range+1; i++)
                {
                    for (int j = lcurentDirectionScan.Count - 1; j >= 0; j--)
                    {
                        Vector2 scanPos = lCellPosition+lcurentDirectionScan[j]*i;
                        int x = (int)scanPos.X;
                        int y = (int)scanPos.Y;

                        if (x < 0 || x >= lGrid.GetLength(0) || y < 0 || y >= lGrid.GetLength(1))
                            continue;


                        GameObject GOToScan = lGrid[x,y ].GetContent();
                        if ( GOToScan is BoxTesla lTesla && lTesla.energize)
                        { 
                                LineConnection(GOToScan);
                                energize = true;
                                boxTeslasList.Add(this);
                                return;
                        }
                        else if (GOToScan is null)
                        {
                            continue;
                        }
                        
                        else if (GOToScan is Generator)
                        {
                            LineConnection(GOToScan);
                            energize = true;
                            boxTeslasList.Add(this);
                            return;
                        }
                        else if (GOToScan is GoalBulb) continue;
                        else if (GOToScan is Wall)
                        {
                            indicesToRemove.Add(j);
                            continue;
                        }
                    }
                    indicesToRemove.Sort((a, b) => b.CompareTo(a));
                    foreach (int index in indicesToRemove)
                    {
                        if (index >= 0 && index < lcurentDirectionScan.Count)
                            lcurentDirectionScan.RemoveAt(index);
                    }
                    indicesToRemove.Clear();
                }
                LineDeconnection();
                energize=false;
            }

            private void LineConnection(GameObject objToConnect)
            {
                int lPointCount = electriLine2D.GetPointCount();
                if (electriLine2D.GetPointCount()>=1)LineDeconnection();
                GD.Print(electriLine2D.GetPointCount());
                electriLine2D.AddPoint(ToLocal(objToConnect.GlobalPosition),1);
                electriLine2D.Visible=true;
            }

            private void LineDeconnection()
            {
                electriLine2D.Visible=false;
               if (electriLine2D.GetPointCount()>=1)electriLine2D.RemovePoint(1);
               
            }
            
            protected override void Dispose(bool pDisposing)
            {

            }
        }
    }


