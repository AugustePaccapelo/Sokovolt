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

		private LevelSelector levelSelector;

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

			MainMenu.GetInstance().StartGame += GameStart;
		}

		private void GameStart() //Execute when StartButton is press in MainMenu
		{
			MainMenu.GetInstance().QueueFree();
			levelSelector = levelSelectorScene.Instantiate() as LevelSelector;
			AddChild(levelSelector);
		}

		protected override void Dispose(bool pDisposing)
		{
			instance = null;
			base.Dispose(pDisposing);
		}

	}
}
