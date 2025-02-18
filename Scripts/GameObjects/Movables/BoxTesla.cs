using Godot;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Com.IsartDigital.SokoVolt.Managers;

// Author : Ferlat Thibaud 

namespace Com.IsartDigital.SokoVolt.GameObjects.Movables {
	
    namespace Com.IsartDigital.SokoVolt.GameObjects.Movables {
	
        public partial class BoxTesla : Movable
        {
            static List<BoxTesla> boxTeslasList = new List<BoxTesla>();
            [Export] private int teslaRange;
            [Export] public bool  energize { get; private set; }
            GridManager gridManager = GridManager.GetInstance();
            public override void _Ready()
            {

                ConnectionSearch();


            }

            public override void _Process(double pDelta)
            {
                float lDelta = (float)pDelta;

            }

            public override void MoveTo(int pX, int pY, Cell[,] pGrid)
            {
                base.MoveTo(pX, pY, pGrid);

                CustomSignals lSignals = CustomSignals.GetInstance();

                lSignals.EmitSignal(CustomSignals.SignalName.BoxTeslaMoved);
            }

            private void ConnectionSearch()
            {
                Vector2 cellPosition = Utils.GetCellPos(this);


                int x = (int)cellPosition.X;
                int y = (int)cellPosition.Y;

            
                GD.Print(x+","+y);



            }



            protected override void Dispose(bool pDisposing)
            {

            }
        }
    }

}
