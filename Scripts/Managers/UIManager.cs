using Com.IsartDigital.SokoVolt.GameObjects;
using Godot;
using System;

//author : Noe Sales

namespace Com.IsartDigital.SokoVolt.Managers
{
	public partial class UIManager : Manager
	{
		#region Singleton
		static private UIManager instance;
		private UIManager() { }

		static public UIManager GetInstance()
		{
			if(instance == null) instance = new UIManager();
			return instance;
		}

		#endregion
		[Export] PackedScene levelSelectorScene;
		[Export] PackedScene levelCreatorScene;
		[Export] PackedScene mainMenuScene;
        [Export] PackedScene optionMenuScene;

		private LevelSelector levelSelector;
		private LevelCreator levelCreator;
        private AudioSettings optionMenu;
		private MainMenu mainMenu;
		private HUD hud; 

		public override void _Ready()
		{
			#region Singelton
			if (instance != null)
			{
				QueueFree();
				GD.Print(nameof(UIManager) + "Instance already exist, destroying the last added");
				return;
			}

			instance = this;
			#endregion

			base._Ready();

			mainMenu = MainMenu.GetInstance();

			CustomSignals lCustomSignals = CustomSignals.GetInstance();
            lCustomSignals.GoToMainMenu += BackToMainMenu;
            lCustomSignals.GoToLevelSelector += GameStart;
            lCustomSignals.GoToLevelCreator += LevelCreatorScreen;
            lCustomSignals.GoToLoginScreen += GoToLoginScreen;
			lCustomSignals.GoToOptionMenu += GoToOptionMenu;
            lCustomSignals.ExitGame += OnQuitButtonPressed;
        }

		public void GameStart() //Execute when StartButton is press in MainMenu
		{
            Tween lTween = GameManager.GetInstance().MenuTrans.ActiveTrans(1f, 0.2f);
			lTween.Finished += () =>
			{
                mainMenu.Hide();
                levelSelector = levelSelectorScene.Instantiate() as LevelSelector;
                AddChild(levelSelector);
            };
		}

        public void LevelCreatorScreen() //Execute when LevelCreatorButton is press in MainMenu
        {
            Tween lTween = GameManager.GetInstance().MenuTrans.ActiveTrans(1f, 0.2f);
			lTween.Finished += () =>
			{
                mainMenu.Hide();
                levelCreator = levelCreatorScene.Instantiate() as LevelCreator;
                AddChild(levelCreator);
                MoveChild(levelCreator, levelCreator.GetIndex() - 1);
				LevelCreator.inLevelCreator = true;
            };
        }

        private void BackToMainMenu()
		{
            Tween lTween = GameManager.GetInstance().MenuTrans.ActiveTrans(1f, 0.2f);
			lTween.Finished += () =>
			{
				foreach (var item in GameManager.GetInstance().objectsContainer.GetChildren())
				{
					if (item is AnimationPiston) item.QueueFree();
				}
                LevelSelector.GetInstance()?.QueueFree();
                LevelCreator.GetInstance()?.QueueFree(); 
                AudioSettings.Instance.Hide();
				LoginScreen.GetInstance().Hide();
                mainMenu.Show();
				LevelCreator.inLevelCreator = false;
            };
		}

        private void GoToOptionMenu()
        {
            Tween lTween = GameManager.GetInstance().MenuTrans.ActiveTrans(1f, 0.2f);
            lTween.Finished += () =>
            {
                mainMenu.Hide();
				optionMenu= AudioSettings.Instance;
                optionMenu.Show();
            };
        }

		private void GoToLoginScreen()
		{
			LoginScreen lLoginScreen = LoginScreen.GetInstance();

			if (lLoginScreen.skipLogin)
			{
				lLoginScreen.skipLogin = false;
                CustomSignals.GetInstance().EmitSignal(CustomSignals.SignalName.GoToMainMenu);
                return;
			}
			mainMenu.Hide();
			lLoginScreen.Show();
			//lLoginScreen.AnimationLoginEnter();
		}
        private void OnQuitButtonPressed()
        {
            GetTree().Quit(); 
        }

		protected override void Dispose(bool pDisposing)
		{
			instance = null;
			base.Dispose(pDisposing);
		}

	}
}
