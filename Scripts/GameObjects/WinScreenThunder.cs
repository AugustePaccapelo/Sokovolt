using Godot;
using System;

namespace Com.IsartDigital.ProjectName
{

    public partial class WinScreenThunder : Node2D
    {
        private Node2D impactEffects;
        private Line2D bolt;
        private AnimationPlayer animation;
        private Node2D targetBattery; // Stocker la batterie cible

        private const string ANIMATIONPLAYER_PATH = "ThunderAnimation";
        private const string BOLT_PATH = "Bolt";
        private const string IMPACTEFFECTS_PATH = "ImpactEffect";
        private const string THUNDER_ANIMATION = "thunderWinScreen";

        public override void _Ready()
        {
            animation = GetNode<AnimationPlayer>(ANIMATIONPLAYER_PATH);
            bolt = GetNode<Line2D>(BOLT_PATH);
            impactEffects = GetNode<Node2D>(IMPACTEFFECTS_PATH);

            animation.AnimationFinished += (animationThunder) => QueueFree();
        }

        public override void _Process(double pDelta)
        {
            if (targetBattery != null)
            {
                // Suivre la position de la batterie
                Vector2 localPos = ToLocal(targetBattery.GlobalPosition);

                // Utiliser la position globale du Line2D comme référence pour le point
                bolt.SetPointPosition(1, targetBattery.GlobalPosition - bolt.GlobalPosition);

                impactEffects.Position = localPos;
            }
        }

        public void ActiveThunder(Node2D pBattery)
        {
            targetBattery = pBattery;

            if (bolt.Points.Length < 2)
            {
                bolt.ClearPoints();
                bolt.AddPoint(Vector2.Zero);
                bolt.AddPoint(Vector2.Zero);
            }

            animation.Play(THUNDER_ANIMATION);
            pBattery.SelfModulate = new Color(1, 1, 1, 1);
            // if(pBattery.GetChild<Node2D>(0) != null)pBattery.GetChild<Node2D>(0).Show();
        }

        protected override void Dispose(bool pDisposing)
        {

        }
    }
}
