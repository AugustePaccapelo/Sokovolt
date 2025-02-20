using Godot;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Com.IsartDigital.SokoVolt.Managers;
using System.Threading.Tasks;
using System.Data;

// Author : Ferlat Thibaud 

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
				
				ConnectionSearch();
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

               GD.Print(lGrid [1,1].GetContent().GetType());


                int x = (int)lCellPosition.X;
                int y = (int)lCellPosition.Y;


                for (int i =1 ; i <= range; i++)
                {
                    for (int j = lLength - 1; j >= 0; j--)
                    {
                        Vector2 scan = lcurentDirectionScan[j]*i;
                        // = lGrid[(int)scan.X,(int)scan.Y];
                        if ( this is BoxTesla lTesla)
                        {
                            if (lTesla.energize is true)
                            {
                                
                                return;

                            }
                        }
                        else if (this is Wall )
                        {
                            lcurentDirectionScan.RemoveAt(j);
                        }
                    }
                }
            }
            public static (int, int)? TrouverPosition<T>(T[,] tableau, T element)
            {
                for (int i = 0; i < tableau.GetLength(0); i++) 
                {
                    for (int j = 0; j < tableau.GetLength(1); j++) 
                    {
                        if (EqualityComparer<T>.Default.Equals(tableau[i, j], element))
                        {
                            return (i, j); 
                        }
                    }
                }
                return null;
            }




            protected override void Dispose(bool pDisposing)
            {

            }
        }
    }


