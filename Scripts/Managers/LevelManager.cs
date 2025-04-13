using Com.IsartDigital.SokoVolt.GameObjects;
using Com.IsartDigital.SokoVolt.GameObjects.Movables;
using Godot;
using System;
using RobotnikSokoban.Scripts.Managers;

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

            CustomSignals.GetInstance().GoToMainMenu += UnLoadLevel; 
            CustomSignals.GetInstance().GoToNextLevel += NextLevel;
        }

        private void NextLevel(int pLevel)
        {
            Tween lTween = GameManager.GetInstance().MenuTrans.ActiveTrans(2f, 0.4f);
            UnLoadLevel();
            lTween.Finished += () => LevelLoaderFonc(pLevel);
        }

        private void UnLoadLevel()
        {
            GridManager.GetInstance().ClearGrid();
            CustomSignals.GetInstance().EmitSignal(CustomSignals.SignalName.UnLoadLevel);
            InputManager.canPlayerMove = false;
        }

        public void LevelLoaderFonc(int pLevel)
		{
			GD.Print("Level : " + pLevel);
            CustomSignals.GetInstance()?.EmitSignal(CustomSignals.SignalName.LoadLevel, pLevel);
            LevelSelector.GetInstance()?.QueueFree();
            DialogManager.GetInstance()?.TriggerDialogueForLevel(pLevel);
	            

		}

        protected override void Dispose(bool pDisposing)
		{
			instance = null;
			base.Dispose(pDisposing);
		}

	}
}
