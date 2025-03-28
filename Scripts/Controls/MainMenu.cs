using Com.IsartDigital.SokoVolt.Managers;
using Godot;
using System;

//author : Noe Sales

namespace Com.IsartDigital.SokoVolt
{
	public partial class MainMenu : Control
	{
		#region Singleton
		static private MainMenu instance;
		private MainMenu() { }

		static public MainMenu GetInstance()
		{
			if(instance == null) instance = new MainMenu();
			return instance;
		}

		#endregion

		[Export] private Button startButton;
		[Export] private Button levelCreatorButton;
		[Export] private Button unlogButton;
		[Export] private Button englishButton;
		[Export] private Button frenchButton;

		//[Signal] public delegate void StartGameEventHandler();

		private UIManager uiManager;

		public override void _Ready()
		{
			#region Singelton
			if (instance != null)
			{
				QueueFree();
				GD.Print(nameof(MainMenu) + "Instance already exist, destroying the last added");
				return;
			}

			instance = this;
			#endregion

			uiManager = GetParent<UIManager>();

            //StartGame += uiManager.GameStart;

            //startButton.Pressed += () => EmitSignal(nameof(StartGame));

            CustomSignals.GetInstance().UpdateLanguage += SetLanguage;

            startButton.Pressed += () => CustomSignals.GetInstance().EmitSignal(CustomSignals.SignalName.GoToLevelSelector);
            levelCreatorButton.Pressed += () => CustomSignals.GetInstance().EmitSignal(CustomSignals.SignalName.GoToLevelCreator);
			unlogButton.Pressed += () => CustomSignals.GetInstance().EmitSignal(CustomSignals.SignalName.GoToLoginScreen);
			englishButton.Pressed += () =>
			{
				CustomSignals.GetInstance().EmitSignal(CustomSignals.SignalName.UpdateLanguage, "en");
				englishButton.Disabled = true;
				frenchButton.Disabled = false;
			};
			frenchButton.Pressed += () =>
			{
				CustomSignals.GetInstance().EmitSignal(CustomSignals.SignalName.UpdateLanguage, "fr");
                englishButton.Disabled = false;
                frenchButton.Disabled = true;
            };
        }

		private void SetLanguage(string pLanguage)
		{
            TranslationServer.SetLocale(pLanguage);
			UpdateUI();
        }
		
		private void UpdateUI()
		{
			startButton.Text = Tr("START");
            levelCreatorButton.Text = Tr("LevelCreator");
            unlogButton.Text = Tr("Disconnect");
		}

		protected override void Dispose(bool pDisposing)
		{
			instance = null;
			base.Dispose(pDisposing);
		}

	}
}
