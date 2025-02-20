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

		[Signal] public delegate void StartGameEventHandler();

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

            StartGame += uiManager.GameStart;

            startButton.Pressed += () => EmitSignal(nameof(StartGame));
        }
		

		protected override void Dispose(bool pDisposing)
		{
			instance = null;
			base.Dispose(pDisposing);
		}

	}
}
