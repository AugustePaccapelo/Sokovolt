using Godot;
using System;
using System.Collections.Generic;

// Author : Noé Sales

namespace Com.IsartDigital.SokoVolt
{

	public partial class LevelCreator : Control
	{

		#region Singleton
		static private LevelCreator instance;

		private LevelCreator() { }

		static public LevelCreator GetInstance()
		{
			if (instance == null) instance = new LevelCreator();
			return instance;

		}
		#endregion

		[Export] Button mainMenuButton;
		[Export] Button newLevelButton;
		[Export] Button loadLevelButton;

		private const int LENGHT = 11;

		List<List<Vector2>> gridPos = new List<List<Vector2>>();

		public override void _Ready()
		{
			#region Singleton Ready
			if (instance != null)
			{
				QueueFree();
				GD.Print(nameof(LevelCreator) + " Instance already exist, destroying the last added.");
				return;
			}

			instance = this;
			#endregion

			mainMenuButton.Pressed += () => CustomSignals.GetInstance().EmitSignal(CustomSignals.SignalName.GoToMainMenu);
        }

		public override void _Process(double pDelta)
		{
			float lDelta = (float)pDelta;

		}

		private void CreateGrid()
		{
			for (int x = 0; x < LENGHT; x++)
			{
				Vector2 lPosition = Vector2.Zero;

				for (int y = 0; y < LENGHT; y++)
				{

				}
			}
		}

		protected override void Dispose(bool pDisposing)
		{
			instance = null;
			base.Dispose(pDisposing);
		}
	}
}
