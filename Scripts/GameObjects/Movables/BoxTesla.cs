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
                Vector2 lCellPosition = Utils.GetCellPos(this);


                int x = (int)lCellPosition.X;
                int y = (int)lCellPosition.Y;

            
                // GD.Print(x+","+y);



            }



            protected override void Dispose(bool pDisposing)
            {

            }
        }
    }


