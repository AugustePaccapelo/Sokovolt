using Com.IsartDigital.SokoVolt.Managers;
using Godot;
using System;
using System.Runtime.CompilerServices;

//Author : Ferlat Thibaud 
namespace Com.IsartDigital.SokoVolt{
	
	public partial class HUD : Control
	{
		#region GetInstance
		static private HUD instance;
		
		static public HUD GetInstance () {
			if (instance == null) instance = new HUD();
			return instance;
		}

		private HUD ():base() {}
		#endregion

		[Signal] public delegate void UndoButtonEventHandler();
		[Signal] public delegate void RedoButtonEventHandler();

		[Export] public Button undoButton, redoButton, mainMenuButton;
		[Export] public Label scoreLabel, stepLabel, winLabel; 
		public override void _Ready()
		{
			#region instance
		if (instance != null){  
			QueueFree();
			GD.Print(nameof(HUD) + " Instance already exist, destroying the last added.");
			return;
		}
		instance = this;
		#endregion
			Init(); 
		}

		private void Init()
		{
			CustomMinimumSize = GetViewportRect().Size;	
			undoButton.Pressed += () => EmitSignal(nameof(UndoButton));
			redoButton.Pressed += () => EmitSignal(nameof(RedoButton));
			mainMenuButton.Pressed += () => CustomSignals.GetInstance().EmitSignal(CustomSignals.SignalName.GoToMainMenu);
			winLabel.Hide();
		}

		public void GameFinished()
		{
			winLabel.Show();
			Tween lTween = CreateTween();
			lTween.Finished += () => ReturnToMenu();
			lTween.TweenProperty(winLabel, "modulate", Colors.White, 2).From(Colors.Transparent);
			lTween.Play();
		}

		private void ReturnToMenu()
		{
			CustomSignals.GetInstance().EmitSignal(CustomSignals.SignalName.GoToMainMenu);
			winLabel.Hide();
        }

		public override void _Process(double pDelta)
		{

		}

		#region dispose
		protected override void Dispose(bool pDisposing)
		{
			if (pDisposing && instance == this) instance = null;
			base.Dispose(pDisposing);
		}
		#endregion
	}
}
