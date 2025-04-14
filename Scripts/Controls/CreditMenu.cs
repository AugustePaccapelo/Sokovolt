using Godot;
using System;

// Author : Noe Sales

namespace Com.IsartDigital.SokoVolt {
	
	public partial class CreditMenu : Control
	{
        #region Singleton
        static private CreditMenu instance;
        private CreditMenu() { }

        static public CreditMenu GetInstance()
        {
            if (instance == null) instance = new CreditMenu();
            return instance;
        }

        #endregion

        public override void _Ready()
		{
            #region Singelton
            if (instance != null)
            {
                QueueFree();
                GD.Print(nameof(CreditMenu) + "Instance already exist, destroying the last added");
                return;
            }

            instance = this;
            #endregion
        }

        public override void _Process(double pDelta)
		{
			float lDelta = (float)pDelta;

		}

        protected override void Dispose(bool pDisposing)
        {
            instance = null;
            base.Dispose(pDisposing);
        }
    }
}
