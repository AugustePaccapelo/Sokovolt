using Godot;
using System;
using System.Threading.Tasks; // Nécessaire pour async/await

// Author : Noe Sales

namespace Com.IsartDigital.ProjectName
{
    public partial class WinScreen : Node2D
    {
        [Export] private Node2D area;
        [Export] private PackedScene thunderScene;
        [Export] private Sprite2D clockwises;
        [Export] private RigidBody2D screen;
        [Export] private Sprite2D[] batteries;
        private int counter = 0;
        private int thunderNumb = 4;

        public override void _Ready()
        {
            screen.Position = new Vector2(800, 583);

            StarSysteme(1);
        }

        private WinScreenThunder CreateThunder()
        {
            WinScreenThunder lThunder = thunderScene.Instantiate<WinScreenThunder>();
            AddChild(lThunder);
            lThunder.Position = new Vector2(GD.RandRange(0, 1920), -80);
            return lThunder;
        }

        private async void StarSysteme(int pCount)
        {
            if (counter <= 3)
            {
                for (int i = 0; i < pCount; i++)
                {
                    for (int y = 0; y < thunderNumb; y++)
                    {
                        WinScreenThunder lThunder = CreateThunder();
                        lThunder.ActiveThunder(counter + 1, batteries[counter]);
                    }
                    counter++;

                    await Wait(1f);
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
