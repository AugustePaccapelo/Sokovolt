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
                
                if (LastPos!=Utils.GetCellPos(this))
                {
                   LastPos = Utils.GetCellPos(this);;
                   ConnectionSearch();
                }
            }

			private void Init()
			{
				CallDeferred(nameof(UpdateRangeLabel));
               length=directionScan.Count;

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


            private void ConnectionSearch()
            {

                Cell[,] lGrid = gridManager.grid;
                Vector2 lCellPosition = Utils.GetCellPos(this);
                int lLength = length;
                List<Vector2> lcurentDirectionScan = new List<Vector2>(directionScan);
                List<int> indicesToRemove = new List<int>();

                for (int i =1 ; i <= range; i++)
                {
                    for (int j = lcurentDirectionScan.Count - 1; j >= 0; j--)
                    {
                        Vector2 scanPos = lCellPosition+lcurentDirectionScan[j]*i;
                        int x = (int)scanPos.X;
                        int y = (int)scanPos.Y;

                        if (x < 0 || x >= lGrid.GetLength(0) || y < 0 || y >= lGrid.GetLength(1))
                            continue;


                        GameObject GOToScan = lGrid[x,y ].GetContent();
                        if ( GOToScan is BoxTesla lTesla)
                        { 
                            GD.Print("Tesla");
                            if (lTesla.energize is true)
                            {

                            }
                        }
                        else if (GOToScan is null)
                        {
                            continue;
                        }
                        
                        else if (GOToScan is Generator)
                        {
                            GD.Print("Generator");
                        }
                        else if (GOToScan is GoalBulb)
                        {
                            GD.Print("GoalBulb");
                            if (energize)
                            {
                            }
                        }
                        else if (GOToScan is Wall)
                        {
                            GD.Print("Wall");
                            indicesToRemove.Add(j);
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
            }
           




            protected override void Dispose(bool pDisposing)
            {

            }
        }
    }


