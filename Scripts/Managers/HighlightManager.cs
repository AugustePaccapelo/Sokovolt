using Godot;
using System;
using System.Collections.Generic;

namespace Com.IsartDigital.SokoVolt.Tools {

    public partial class HighlightManager : Node {

        private static HighlightManager instance;
        public static HighlightManager GetInstance() => instance;

        [Export] private PackedScene highlightFXScene;

        // Permet de cibler plusieurs nodes par "nom logique"
        private Dictionary<string, List<Node2D>> highlightTargets = new();
        private List<Node2D> highlightInstances = new();


        public override void _Ready() {
            if (instance != null) {
                QueueFree();
                GD.PrintErr("HighlightManager already exists, destroying duplicate.");
                return;
            }
            instance = this;
            CustomSignals.GetInstance().GoToMainMenu += ClearAllTargets;
        }

        public void RegisterTarget(string pName, Node2D pTarget) {
            if (!highlightTargets.ContainsKey(pName))
                highlightTargets[pName] = new List<Node2D>();

            if (!highlightTargets[pName].Contains(pTarget))
                highlightTargets[pName].Add(pTarget);
        }

        public void Highlight(string pTargetName) {
            ClearHighlights(); // 🔥 Avant tout, on reset

            if (!highlightTargets.ContainsKey(pTargetName) || highlightTargets[pTargetName].Count == 0) {
                GD.PrintErr($"[HighlightManager] No targets registered for: {pTargetName}");
                return;
            }

            foreach (Node2D lTarget in highlightTargets[pTargetName]) {
                if (!IsInstanceValid(lTarget)) continue;

                if (highlightFXScene != null) {
                    Node2D fx = highlightFXScene.Instantiate<Node2D>();
                    lTarget.AddChild(fx);
                    fx.GlobalPosition = lTarget.GlobalPosition;

                    highlightInstances.Add(fx); // 👈 On garde une référence
                }

                Tween tween = lTarget.CreateTween();
                tween.TweenProperty(lTarget, "modulate:a", 0.2f, 0.15f);
                tween.TweenProperty(lTarget, "modulate:a", 1.0f, 0.15f);
                tween.TweenProperty(lTarget, "modulate:a", 0.2f, 0.15f);
                tween.TweenProperty(lTarget, "modulate:a", 1.0f, 0.15f);
            }
        }
        
        public void ClearHighlights() {
            foreach (Node2D fx in highlightInstances) {
                if (IsInstanceValid(fx))
                    fx.QueueFree();
            }
            highlightInstances.Clear();
        }
        
        private void ClearAllTargets()
        {
            highlightTargets.Clear();
            highlightTargets.Clear();
        }


    }
}
