using Com.IsartDigital.SokoVolt;
using Com.IsartDigital.SokoVolt.Managers;
using Godot;
using System;
using System.Threading.Tasks; // Nécessaire pour async/await

// Author : Noe Sales

namespace Com.IsartDigital.Sokovolt
{
    public partial class WinScreen : Node2D
    {
        [Export] private Node2D area;
        [Export] private PackedScene thunderScene;
        [Export] private Sprite2D clockwises;
        [Export] private RigidBody2D screen;
        [Export] private Label starCountLabel;
        [Export] private Label stepsCountLabel;
        [Export] private Label scoreLabel;
        [Export] private ColorRect screenEffect;
        [Export] private Node2D particlesGroup;
        [Export] private Button nextLevelButton;
        private ShaderMaterial shaderEffect;
        [Export] private Sprite2D[] batteries;
        private int counter = 0;
        public static int actualLevel = 0;
        public int finalScore = 0;
        public int earnedStars = 0;    
        private int thunderNumb = 4;

        private const string STAR_LABEL_PREFIXE = "X ";
        private const string SCORE_LABEL_PREFIXE = "SCORE : ";

        public override void _Ready()
        {
            screen.Position = new Vector2(800, 583);
            shaderEffect = (ShaderMaterial)screenEffect.Material;
            stepsCountLabel.Text = STAR_LABEL_PREFIXE + 0000;
            scoreLabel.Text = SCORE_LABEL_PREFIXE + 0000;
            nextLevelButton.Pressed += () =>
            {
                CustomSignals.GetInstance().EmitSignal(CustomSignals.SignalName.GoToNextLevel, actualLevel + 1);
                QueueFree();
            };
        }

        private WinScreenThunder CreateThunder()
        {
            WinScreenThunder lThunder = thunderScene.Instantiate<WinScreenThunder>();
            AddChild(lThunder);
            lThunder.Position = new Vector2(GD.RandRange(0, 1920), -80);
            return lThunder;
        }

        public void UpdateStats(int pScore, int pSteps)
        {
            finalScore = pScore;
            Tween lTween = CreateTween().SetParallel(true);
            lTween.TweenProperty(stepsCountLabel, "text", Tr(GridManager.STEP_LABEL_PREFIXE) + pSteps, 1f);
            lTween.TweenProperty(scoreLabel, "text", SCORE_LABEL_PREFIXE + pScore, 1f);
            UserGestion.GetInstance().SaveUserProgress(actualLevel, finalScore, earnedStars);
            UserGestion.GetInstance().UnlockLevel(actualLevel + 1);
            GD.Print("player progress saved and next level has been unlocked");
        }

        public async void StarSysteme(int pCount)
        {
            earnedStars = pCount;
            if (counter <= 3)
            {
                for (int i = 0; i < pCount; i++)
                {
                    for (int y = 0; y < thunderNumb; y++)
                    {
                        WinScreenThunder lThunder = CreateThunder();
                        lThunder.ActiveThunder(batteries[counter], WinScreenThunder.THUNDER_ANIMATION);
                    }
                    shaderEffect.SetShaderParameter("scanline_alpha", 3);
                    particlesGroup.Show();
                    counter++;
                    starCountLabel.Text = STAR_LABEL_PREFIXE + counter;
                    await Wait(0.6f);
                    shaderEffect.SetShaderParameter("scanline_alpha", 0.9f);
                    await Wait(0.4f);
                }
            }
        }

        // Fonction pour attendre une durée spécifique avec Timer
        private async Task Wait(float seconds)
        {
            Timer timer = new Timer();
            timer.WaitTime = seconds;
            timer.OneShot = true;
            AddChild(timer);
            timer.Start();

            // Attend le signal 'timeout' du Timer
            await ToSignal(timer, "timeout");

            timer.QueueFree();
        }

        public override void _Process(double pDelta)
        {
            area.Position = GetLocalMousePosition();
            clockwises.RotationDegrees += 5;
        }

        protected override void Dispose(bool pDisposing)
        {
        }
    }
}
