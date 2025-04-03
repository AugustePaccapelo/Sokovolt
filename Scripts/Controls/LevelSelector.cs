using Com.IsartDigital.SokoVolt.GameObjects;
using Com.IsartDigital.SokoVolt.Managers;
using Godot;
using System;
using System.Collections.Generic;
using static Com.IsartDigital.SokoVolt.Tools.ObjectProperties;
using System.Reflection.Emit;

// Author : Noé Sales

namespace Com.IsartDigital.SokoVolt
{
    public partial class LevelSelector : Control
    {
        [Export] public Button buttonRight, buttonLaunch, buttonLeft, buttonUnlockAll, buttonMainMenu;
        [Export] public Sprite2D carpetTexture;
        [Export] private CompressedTexture2D texture;
        [Export] private PackedScene teslaScene, smokeParticlesScene;
        [Export] private Node2D teslaContainer;
        [Export] private int teslaPosY = 253;

        private int actualLevel = 0;
        public static int levelNumbMax = 5;
        private int newTeslaPointPosY = 223;

        private Vector2 buttonSize = new Vector2(60, 100);
        private Vector2 teslaSize = new Vector2(855, 1071);
        private Vector2 screenSize;

        private GpuParticles2D buttonSmokeParticles;
        private LevelSelectorTesla actualTesla;
        
        private bool alreadyPress = false;

        private const string LEVEL_PREFIXE = "LevelPrefix";
        private const string LEVEL_LABEL_PATH = "Screen/LevelLabel";
        private const float MARGIN = 350.0f;

        public Dictionary<int, LevelSelectorTesla> teslaDictionnary = new Dictionary<int, LevelSelectorTesla>();

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
            InitializeLevelAtStart();

            buttonMainMenu.Pressed += MainMenu;
            buttonRight.Pressed += () => SwitchLevel(1);
            buttonLeft.Pressed += () => SwitchLevel(-1);
            buttonUnlockAll.Pressed += UnlockAll;
            buttonLaunch.Pressed += LevelUnlockedCheck;
        }

        private void MainMenu()
        {
            CustomSignals.GetInstance().EmitSignal(CustomSignals.SignalName.GoToMainMenu);
        }

        private void InitializeLevelAtStart()
        {
            Vector2 lTeslaPosition;
            LevelSelectorTesla lTesla = new LevelSelectorTesla();
            // Initialisation des niveaux dès le départ
            for (int i = 0; i <= levelNumbMax; i++)
            {
                lTeslaPosition = (i == 0) ? new Vector2(screenSize.X / 2, teslaPosY)
                    : new Vector2((screenSize.X / 2) + (screenSize.X * i), teslaPosY);

                lTesla = CreateTesla(lTeslaPosition, i);
                if(i ==0) actualTesla = lTesla;
                teslaDictionnary.Add(lTesla.level, lTesla);
            }

            for (int i = 0; i < teslaDictionnary.Count - 1; i++)
            {
                if (i != 5) teslaDictionnary[i].nextTesla = teslaDictionnary[i + 1];
                else teslaDictionnary[i].nextTesla = null;
            }
        }

        private void UnlockAll()
        {
            if (!alreadyPress)
            {
                alreadyPress = true;
                EmitSignal(nameof(UnlockAllLevel));
                GetTree().CreateTimer(1f).Timeout += () => alreadyPress = false;
            }
        }
        private void LevelUnlockedCheck()
        {
            if (actualTesla.levelUnlocked)
            {
                LevelManager.GetInstance().LevelLoaderFonc(actualTesla.level);
            }
        }

        private void SwitchLevel(int pDirection)
        {
            if (!alreadyPress && actualLevel + pDirection >= 0 && actualLevel + pDirection <= levelNumbMax)
            {
                Vector2 lNewPos;
                Tween lTween;
                Tween lTween2;
                alreadyPress = true;
                actualLevel += pDirection;
                actualTesla = teslaDictionnary[actualLevel];

                for (int i = 0; i < teslaDictionnary.Count; i++)
                {
                    lNewPos = new Vector2((i - actualLevel) * screenSize.X + screenSize.X / 2, teslaPosY);
                    lTween = CreateTween().SetParallel(true);
                    lTween.TweenProperty(teslaDictionnary[i], POSITION, lNewPos, 1f).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Elastic);
                }
                lTween2 = CreateTween();
                lTween2.TweenProperty(carpetTexture, POSITION, new Vector2(carpetTexture.Position.X + ((screenSize.X / 2) * -pDirection), carpetTexture.Position.Y), 1f).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Elastic);

                buttonSmokeParticles = smokeParticlesScene.Instantiate() as GpuParticles2D;
                if (pDirection == 1)
                {
                    buttonRight.AddChild(buttonSmokeParticles);
                    buttonSmokeParticles.Position = new Vector2(88, 143);
                }
                if (pDirection == -1)
                {
                    buttonLeft.AddChild(buttonSmokeParticles);
                    buttonSmokeParticles.Position = new Vector2(88, 143);
                }

                GetTree().CreateTimer(0.5f).Timeout += () => alreadyPress = false;
            }
        }

        private LevelSelectorTesla CreateTesla(Vector2 pPos, int pIndex)
        {
            LevelSelectorTesla lTesla = teslaScene.Instantiate<LevelSelectorTesla>();
            teslaContainer.AddChild(lTesla);
            lTesla.Position = pPos;
            lTesla.level = pIndex;
            lTesla.padLock.Show();
            if (lTesla.level == 0)
            {
                lTesla.UnlockLevel();
            }

            Godot.Label lLabel = lTesla.GetNode<Godot.Label>(LEVEL_LABEL_PATH);
            lLabel.Text = Tr(LEVEL_PREFIXE) + "\n" + pIndex;

            return lTesla;
        }
        protected override void Dispose(bool pDisposing)
        {
            instance = null;
            base.Dispose(pDisposing);
        }
    }
}
