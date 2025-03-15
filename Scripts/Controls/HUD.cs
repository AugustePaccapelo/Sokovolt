using Com.IsartDigital.ProjectName;
using Com.IsartDigital.SokoVolt.GameObjects.Movables;
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
		[Export] public PackedScene winScreenScene;
		private WinScreen winScreen;

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
			mainMenuButton.Pressed += ReturnToMenu;
		}

		public void GameFinished()
		{
            LevelLoader.playerCanMove = false;
            Tween lTween = CreateTween();
            winScreen = winScreenScene.Instantiate() as WinScreen;
			AddChild(winScreen);
			winScreen.Position = new Vector2(0, -900);
			winScreen.ZIndex = 50;
			lTween.TweenProperty(winScreen, "position", Vector2.Zero, 1f);
			lTween.Finished += () => GetTree().CreateTimer(1f).Timeout += () => winScreen.StarSysteme(2);
        }

		private void ReturnToMenu()
		{
            if (winScreen != null) winScreen.QueueFree();
            CustomSignals.GetInstance().EmitSignal(CustomSignals.SignalName.GoToMainMenu);
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
