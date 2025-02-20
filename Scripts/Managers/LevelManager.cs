using Godot;
using System;

//author : Noe Sales

namespace Com.IsartDigital.SokoVolt.Managers
{
	public partial class LevelManager : Manager
    {
		#region Singleton
		static private LevelManager instance;
		private LevelManager() { }

		static public LevelManager GetInstance()
		{
			if(instance == null) instance = new LevelManager();
			return instance;
		}

        #endregion
        [Signal] public delegate void LoadLevelEventHandler(int pLevel);

        [Export] private Node2D objectContainer;

        public override void _Ready()
        {
            #region Singelton
            if (instance != null)
            {
                QueueFree();
                GD.Print(nameof(LevelManager) + "Instance already exist, destroying the last added");
                return;
            }

            instance = this;
            #endregion
            
            base._Ready();
        }

        public override void Init()
        {
            base.Init();

            //UIManager.GetInstance().MainMenuButton += UnLoadLevel;
        }

        public void LevelLoader(int pLevel)
		{
			GD.Print("Level : " + pLevel);
            EmitSignal(nameof(LoadLevel), pLevel);
            LevelSelector.GetInstance().QueueFree();
		}

        protected override void Dispose(bool pDisposing)
		{
			instance = null;
			base.Dispose(pDisposing);
		}

	}
}
