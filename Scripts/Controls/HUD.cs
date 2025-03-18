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

		[Export] public Button undoButton, redoButton, mainMenuButton;
		[Export] public Label scoreLabel, stepLabel, winLabel; 
		[Export] public PackedScene winScreenScene;
		public WinScreen winScreen;

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
			undoButton.Pressed += () => EmitSignal(CustomSignals.SignalName.UndoButton);
			redoButton.Pressed += () => EmitSignal(CustomSignals.SignalName.RedoButton);
			mainMenuButton.Pressed += ReturnToMenu;
			CustomSignals lSignals = CustomSignals.GetInstance();
			lSignals.GameFinished +=  GameFinished; 
		}

		private void UndoRedo()
		{
            if (!LevelLoader.playerCanMove) return;
            undoButton.Pressed += () => CustomSignals.GetInstance().EmitSignal(CustomSignals.SignalName.UndoButton);
            redoButton.Pressed += () => CustomSignals.GetInstance().EmitSignal(CustomSignals.SignalName.RedoButton);
        }

        public void GameFinished(int pNumStar, int pScore, int pNumStep)
		{
			CustomSignals.GetInstance().EndLevelAnimation += () =>
			{
				LevelLoader.playerCanMove = false;
				Tween lTween = CreateTween();
				winScreen = winScreenScene.Instantiate() as WinScreen;
				AddChild(winScreen);
				winScreen.Position = new Vector2(0, -900);
				winScreen.ZIndex = 50;
				lTween.TweenProperty(winScreen, "position", Vector2.Zero, 1f);
				winScreen.UpdateStats(pScore, pNumStep);
				winScreen.StarSysteme(pNumStar);
			};
           
        }

		private void ReturnToMenu()
		{
            if(winScreen != null)
			{
				GD.PrintErr("WinScreen has been queue free"); 
				winScreen.QueueFree();
			}
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
