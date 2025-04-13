using Com.IsartDigital.Sokovolt;
using Com.IsartDigital.SokoVolt.GameObjects.Movables;
using Com.IsartDigital.SokoVolt.Managers;
using Com.IsartDigital.SokoVolt.Tools;
using Godot;
using RobotnikSokoban.Scripts.Managers;
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
		[Export] public Label parLabel, stepLabel, winLabel, authorLabel; 
		
		//WinScreen
		[Export] public PackedScene winScreenScene;
		[Export] public Control displayInGame; 
		[Export] public DialogBox dialogBox;
		public WinScreen winScreen;
		private int storedNumStar, storedScore, storedNumStep;

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
			UndoRedo();
            mainMenuButton.Pressed += ReturnToMenu;
			CustomSignals lSignals = CustomSignals.GetInstance();
			lSignals.GameFinished +=  GameFinished; 
		}

		private void UndoRedo()
		{
            undoButton.Pressed += () => CustomSignals.GetInstance().EmitSignal(CustomSignals.SignalName.UndoButton);
            redoButton.Pressed += () => CustomSignals.GetInstance().EmitSignal(CustomSignals.SignalName.RedoButton);
        }

        public void GameFinished(int pNumStar, int pScore, int pNumStep)
		{
			CustomSignals.GetInstance().EndLevelAnimation -= SpawnWinScreen;
			CustomSignals.GetInstance().EndLevelAnimation += SpawnWinScreen;

		
			storedNumStar = pNumStar;
			storedScore = pScore;
			storedNumStep = pNumStep;

			GD.PrintErr("GameFinished called, waiting for EndLevelAnimation...");
		}
		
		private void SpawnWinScreen()
		{
            InputManager.canPlayerMove = false;
			Tween lTween = CreateTween();
			winScreen = winScreenScene.Instantiate() as WinScreen;
			AddChild(winScreen);
			winScreen.Position = new Vector2(0, -900);
			winScreen.ZIndex = 50;
			lTween.TweenProperty(winScreen, ObjectProperties.POSITION, Vector2.Zero, 1f);
			winScreen.UpdateStats(storedScore, storedNumStep);
			winScreen.StarSysteme(storedNumStar);
		}

		
		private void ReturnToMenu()
		{
            if(winScreen != null)
				winScreen.QueueFree();
			
            CustomSignals.GetInstance().EmitSignal(CustomSignals.SignalName.GoToMainMenu);
			SongManager.Instance.Crossfade(EnumSong.AmbientSong.AmbianceGamemusic, EnumSong.AmbientSong.AmbianceMenumusic, 0.5f, -5);

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
