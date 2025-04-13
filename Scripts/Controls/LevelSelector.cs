using Com.IsartDigital.Sokovolt;
using Com.IsartDigital.SokoVolt.GameObjects;
using Com.IsartDigital.SokoVolt.Managers;
using Godot;
using System;
using System.Collections.Generic;
using static Com.IsartDigital.SokoVolt.Tools.ObjectProperties;
using System.Reflection.Emit;
using RobotnikSokoban.Scripts.Managers;

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
        [Export] private ScoreBoard scoreBoard;
        [Export] private int teslaPosY = 253;
        private UserGestion userGestion;

        private int actualLevel = 0;
        public static int levelNumbMax = 11;
        private int newTeslaPointPosY = 223;

        private Vector2 buttonSize = new Vector2(60, 100);
        private Vector2 teslaSize = new Vector2(855, 1071);
        private Vector2 screenSize;

        private GpuParticles2D buttonSmokeParticles;
        private LevelSelectorTesla actualTesla;

        private bool alreadyPress = false;

        private const string LEVEL_PREFIXE = "LevelPrefix";
        private const string LEVEL_LABEL_PATH = "Screen/SubViewportContainer/SubViewport/ScreenView/LevelLabel";
        private const float MARGIN = 350.0f;

        public Godot.Collections.Dictionary<int, LevelSelectorTesla> teslaDictionnary = new Godot.Collections.Dictionary<int, LevelSelectorTesla>();

        [Signal] public delegate void UnlockAllLevelEventHandler(); //

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
            buttonMainMenu.Pressed += MainMenu;
            buttonRight.Pressed += () => SwitchLevel(1);
            buttonLeft.Pressed += () => SwitchLevel(-1);
            buttonUnlockAll.Pressed += UnlockAll;
            buttonLaunch.Pressed += LevelUnlockedCheck;
            userGestion = UserGestion.GetInstance();
            //unlockedLevels = userGestion.GetUnlockedLevels();
            InitializeLevelAtStart();
            scoreBoard.UpdatePersonalScoreBoard(actualLevel);
            InitSound();
        }

        public override void _Process(double delta)
        {
            if(!SongManager.Instance.ambientDict[EnumSong.AmbientSong.elevatorNoise].Playing) SongManager.Instance.ambientDict[EnumSong.AmbientSong.elevatorNoise].Play();
            if(!SongManager.Instance.ambientDict[EnumSong.AmbientSong.machineBackSound].Playing) SongManager.Instance.ambientDict[EnumSong.AmbientSong.machineBackSound].Play();
            if(!SongManager.Instance.ambientDict[EnumSong.AmbientSong.machineBackSound2].Playing) SongManager.Instance.ambientDict[EnumSong.AmbientSong.machineBackSound2].Play();
            if(!SongManager.Instance.ambientDict[EnumSong.AmbientSong.heater].Playing) SongManager.Instance.ambientDict[EnumSong.AmbientSong.heater].Play();
            if(!SongManager.Instance.ambientDict[EnumSong.AmbientSong.mysteriousElectricity].Playing) SongManager.Instance.ambientDict[EnumSong.AmbientSong.mysteriousElectricity].Play();
        }

        private void InitSound()
        {
            SongManager.Instance.ambientDict[EnumSong.AmbientSong.elevatorNoise].Play();
            SongManager.Instance.ambientDict[EnumSong.AmbientSong.machineBackSound].Play();
            SongManager.Instance.ambientDict[EnumSong.AmbientSong.machineBackSound2].Play();
            SongManager.Instance.ambientDict[EnumSong.AmbientSong.heater].Play();
            SongManager.Instance.ambientDict[EnumSong.AmbientSong.mysteriousElectricity].Play();
        }

        private void StopSound()
        {
            SongManager.Instance.ambientDict[EnumSong.AmbientSong.elevatorNoise].Stop();
            SongManager.Instance.ambientDict[EnumSong.AmbientSong.machineBackSound].Stop();
            SongManager.Instance.ambientDict[EnumSong.AmbientSong.machineBackSound2].Stop();
            SongManager.Instance.ambientDict[EnumSong.AmbientSong.heater].Stop();
            SongManager.Instance.ambientDict[EnumSong.AmbientSong.mysteriousElectricity].Stop();
        }

        private void MainMenu()
        {
            GetTree().CreateTimer(0.5f).Timeout += () => CustomSignals.GetInstance().EmitSignal(CustomSignals.SignalName.GoToMainMenu);
        }

        //Create Tesla for each level there are
        private void InitializeLevelAtStart()
        {
            Vector2 lTeslaPosition;
            LevelSelectorTesla lTesla = new LevelSelectorTesla();
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
                if(buttonLaunch.Disabled) buttonLaunch.Disabled = false;
                EmitSignal(nameof(UnlockAllLevel));
                GetTree().CreateTimer(1f).Timeout += () => alreadyPress = false;
            }
        }

        private void LevelUnlockedCheck()
        {
            if (actualTesla.levelUnlocked)
            {
                Tween lTween = GameManager.GetInstance().MenuTrans.ActiveTrans(2f, 0.4f);
                lTween.Finished += () => LevelManager.GetInstance().LevelLoaderFonc(actualTesla.level);
            }
        }

        private void SwitchLevel(int pDirection)
        {
            if (!alreadyPress && actualLevel + pDirection >= 0 && actualLevel + pDirection <= levelNumbMax)//pDirection return 1 or -1 for the direction
            {
                Vector2 lNewPos;
                Tween lTween;
                Tween lTween2;
                alreadyPress = true;
                actualLevel += pDirection;

                if (teslaDictionnary.ContainsKey(actualLevel)) actualTesla = teslaDictionnary[actualLevel];//Update the reference of actualTesla with pDirection
                else
                {
                    alreadyPress = false;
                    return;
                }
                

                UpdateLaunchButton();

                for (int i = 0; i < teslaDictionnary.Count; i++)
                {
                    lNewPos = new Vector2((i - actualLevel) * screenSize.X + screenSize.X / 2, teslaPosY);
                    lTween = CreateTween().SetParallel(true);
                    lTween.TweenProperty(teslaDictionnary[i], POSITION, lNewPos, 1f).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Linear);//Move tesla
                }
                lTween2 = CreateTween();
                //Move Carpet
                lTween2.TweenProperty(carpetTexture, POSITION, new Vector2(carpetTexture.Position.X + ((screenSize.X / 2) * -pDirection), carpetTexture.Position.Y), 1f).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Elastic);

                //Add particles on witch button is pressed
                buttonSmokeParticles = smokeParticlesScene.Instantiate() as GpuParticles2D;
                if (pDirection == 1)
                {
                    buttonRight.AddChild(buttonSmokeParticles);
                    buttonSmokeParticles.Position = new Vector2(88, 143);
                    SongManager.Instance.ambientDict[EnumSong.AmbientSong.arrowButton].Play();
                }
                if (pDirection == -1)
                {
                    buttonLeft.AddChild(buttonSmokeParticles);
                    buttonSmokeParticles.Position = new Vector2(88, 143);
                    SongManager.Instance.ambientDict[EnumSong.AmbientSong.arrowButton].Play();
                }

                GetTree().CreateTimer(0.5f).Timeout += () => alreadyPress = false;
                SongManager.Instance.ambientDict[EnumSong.AmbientSong.treadmill].Play();
            }
            scoreBoard.UpdatePersonalScoreBoard(actualLevel); //Update ScoreBoard
        }

        private void UpdateLaunchButton()
        {
            buttonLaunch.Disabled = !(actualTesla != null && actualTesla.levelUnlocked);
        }

        private LevelSelectorTesla CreateTesla(Vector2 pPos, int pIndex)
        {
            LevelSelectorTesla lTesla = teslaScene.Instantiate<LevelSelectorTesla>();
            teslaContainer.AddChild(lTesla);
            lTesla.Position = pPos;
            lTesla.level = pIndex;//Give the index reference to the tesla
            lTesla.padLock.Show();

            Godot.Label lLabel = lTesla.GetNode<Godot.Label>(LEVEL_LABEL_PATH);
            lLabel.Text = Tr(LEVEL_PREFIXE) + "\n" + pIndex;

            return lTesla;
        }

        protected override void Dispose(bool pDisposing)
        {
            instance = null;
            base.Dispose(pDisposing);
            StopSound();
        }
    }
}
