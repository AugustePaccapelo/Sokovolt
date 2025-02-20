using Com.IsartDigital.SokoVolt.GameObjects;
using Godot;
using System;
using System.Collections.Generic;

// Author : Noé Sales

namespace Com.IsartDigital.SokoVolt
{
    public partial class LevelSelector : Control
    {
        [Export] private Button buttonRight;
        [Export] private Button buttonLeft;
        [Export] private Button UnlockAll;
        [Export] private CompressedTexture2D texture;
        [Export] private PackedScene teslaScene;
        private List<LevelSelectorTesla> teslaList = new List<LevelSelectorTesla>();
        private int levelNumb = 0;
        private int levelNumbMax = 5;
        private const string LEVEL_PREFIXE = "Level : ";
        private const float MARGIN = 50.0f;
        private Vector2 buttonSize = new Vector2(60, 40);
        private Vector2 teslaSize = new Vector2(855, 1071);
        private bool alreadyPress = false;
        private Vector2 screenSize;

        [Signal] public delegate void UnlockAllLevelEventHandler();

        #region Singleton
        static private LevelSelector instance;
        private LevelSelector() { }

        static public LevelSelector GetInstance()
        {
            if (instance == null) instance = new LevelSelector();
            return instance;
        }

        #endregion


        public override void _Ready()
        {
            #region Singelton
            if (instance != null)
            {
                QueueFree();
                GD.Print(nameof(LevelSelector) + "Instance already exist, destroying the last added");
                return;
            }

            instance = this;
            #endregion


            screenSize = GetViewportRect().Size;

            // Initialisation des niveaux dès le départ
            for (int i = 0; i <= levelNumbMax; i++)
            {
                Vector2 lTeslaPosition = (i == 0) ? new Vector2(screenSize.X / 2, 310) : new Vector2(screenSize.X + teslaSize.X * i, 310);
                LevelSelectorTesla lTesla = CreateTesla(lTeslaPosition, i);
                teslaList.Add(lTesla);
            }

            for (int i = 0; i < teslaList.Count; i++)
            {
                teslaList[i].electricBolt.bolt.AddPoint(new Vector2(screenSize.X + 80 + teslaSize.X/2, 135));
            }

            buttonRight.Pressed += () => SwitchLevel(1);
            buttonLeft.Pressed += () => SwitchLevel(-1);
            UnlockAll.Pressed += () => EmitSignal(nameof(UnlockAllLevel));

            buttonLeft.GlobalPosition = new Vector2(0 + MARGIN, screenSize.Y / 2);
            buttonRight.GlobalPosition = new Vector2(screenSize.X - MARGIN - buttonSize.X, screenSize.Y / 2);
        }

        private void SwitchLevel(int pDirection)
        {
            if (!alreadyPress && levelNumb + pDirection >= 0 && levelNumb + pDirection <= levelNumbMax)
            {
                alreadyPress = true;
                levelNumb += pDirection;

                for (int i = 0; i < teslaList.Count; i++)
                {
                    Vector2 lNewPos = new Vector2((i - levelNumb) * screenSize.X + screenSize.X / 2, 310);
                    Tween lTween = CreateTween();
                    lTween.TweenProperty(teslaList[i], "position", lNewPos, 0.5f).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Elastic);
                }

                GetTree().CreateTimer(0.5f).Timeout += () => alreadyPress = false;
            }
        }

        private LevelSelectorTesla CreateTesla(Vector2 pPos, int pIndex)
        {
            LevelSelectorTesla lTesla = teslaScene.Instantiate<LevelSelectorTesla>();
            AddChild(lTesla);
            lTesla.Position = pPos;
            lTesla.level = pIndex;


            Label lLabel = lTesla.GetNode<Label>("Label");
            lLabel.Text = LEVEL_PREFIXE + pIndex;

            return lTesla;
        }
        protected override void Dispose(bool pDisposing)
        {
            instance = null;
            base.Dispose(pDisposing);
        }
    }
}
