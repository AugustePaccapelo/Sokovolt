using Com.IsartDigital.Sokovolt;
using Com.IsartDigital.SokoVolt.Managers;
using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using static Com.IsartDigital.SokoVolt.Tools.ObjectProperties;

// Author : Noe Sales

namespace Com.IsartDigital.SokoVolt.GameObjects {
	
	public partial class LevelSelectorTesla : Sprite2D
	{
		[Export] public LevelSelectorBolt electricBolt;
		[Export] public Node2D electricBoltConstant;
		[Export] public Padlock padLock;
		[Export] public PointLight2D[] lightEmission;

        public LevelSelectorTesla nextTesla;
		public bool levelUnlocked = false;
		public int level;
        private bool allLevelAreUnlocked = false;
        private const string START_ANIMATION = "start_animation";
        private const string END_ANIMATION = "end_animation";

        public override void _Ready()
        {
            if (LevelSelector.GetInstance() != null && LevelManager.GetInstance() != null) Init();
            else CallDeferred(nameof(Init));
        }

        private void Init()
        {
            LevelSelector.GetInstance().UnlockAllLevel += UnlockAll;
            DelayInitLevel();
        }

        private async void DelayInitLevel()
        {
            await ToSignal(GetTree().CreateTimer(0.1f), TIME_OUT);
            InitLevelStateUserData();
        }

        private void InitLevelStateUserData()
        {
            Dictionary lUserData = UserGestion.GetInstance().GetUserData(); 
            string lCurrentUser = UserGestion.GetInstance().currentUser;

            if (!lUserData.ContainsKey(lCurrentUser)) return;
            Dictionary lUserDict = (Dictionary)lUserData[lCurrentUser];
            if (!lUserDict.ContainsKey(LEVELS)) return;

            Dictionary lLevels = (Dictionary)lUserDict[LEVELS];
            string lLevelKey = $"level{level}"; // key of the current level ex: level0, level1, level2...

            if (lLevels.ContainsKey(lLevelKey))
            {
                Dictionary lLevelData = (Dictionary)lLevels[lLevelKey];
                bool lIsLocked = (bool)lLevelData.GetValueOrDefault(LOCKED, true); //lIsLocked = true par defaut

                if (!levelUnlocked)
                {
                    if(!lIsLocked || level == 0) UnlockLevel();
                }
            }
        }

        public void UnlockAll()
        {
            UserGestion lUserGestion = UserGestion.GetInstance();
            Dictionary lUserData = lUserGestion.GetUserData();
            string lCurrentUser = lUserGestion.currentUser;

            if (string.IsNullOrEmpty(lCurrentUser) || !lUserData.ContainsKey(lCurrentUser)) return;

            Dictionary lUserDict = (Dictionary)lUserData[lCurrentUser];
            if (!lUserDict.ContainsKey(LEVELS)) return;

            Dictionary lLevelsDict = (Dictionary)lUserDict[LEVELS];
            string lLevelKey = $"level{level}";

            if (!lLevelsDict.ContainsKey(lLevelKey)) return;

            Dictionary levelData = (Dictionary)lLevelsDict[lLevelKey];
            bool isCurrentlyLocked = (bool)levelData.GetValueOrDefault(LOCKED, true);

            if (!allLevelAreUnlocked)
            {
                if (isCurrentlyLocked)
                {
                    //Visual unlock + backup
                    UnlockLevel();
                    levelData[LOCKED] = false;
                    lLevelsDict[lLevelKey] = levelData;
                    lUserDict[LEVELS] = lLevelsDict;
                    lUserData[lCurrentUser] = lUserDict;
                    lUserGestion.SaveUserData(lUserData);
                }

                //Check if all levels are unlocked
                bool lAnyLocked = false;
                foreach (Dictionary lLevel in lLevelsDict.Values)
                {
                    if ((bool)lLevel.GetValueOrDefault(LOCKED, true))
                    {
                        lAnyLocked = true;
                        break;
                    }
                }

                if (!lAnyLocked)
                {
                    allLevelAreUnlocked = true;
                    LevelSelector.GetInstance().buttonUnlockAll.Text = "LockAll";
                }
            }
            else if (allLevelAreUnlocked && level != 0)
            {
                if (!isCurrentlyLocked)
                {
                    //Visual re-lock + backup
                    LockLevel();
                    levelData[LOCKED] = true;
                    lLevelsDict[lLevelKey] = levelData;
                    lUserDict[LEVELS] = lLevelsDict;
                    lUserData[lCurrentUser] = lUserDict;
                    lUserGestion.SaveUserData(lUserData);
                }

                allLevelAreUnlocked = false;
                LevelSelector.GetInstance().buttonUnlockAll.Text = "UnlockAll";
            }
        }

        public void UnlockLevel()
        {
            electricBolt.animationPlayer.Play(START_ANIMATION);
            Tween lTween = CreateTween();
            foreach (PointLight2D light in lightEmission) lTween.TweenProperty(light, ENERGY, 3, 0.5f);
            padLock.Open();
            levelUnlocked = true;
            GD.Print($"[Tesla {level}] Visually unlocked");
        }

        private void LockLevel()
        {
            electricBolt.animationPlayer.Play(END_ANIMATION);
            Tween lTween = CreateTween();
            foreach (PointLight2D light in lightEmission) lTween.TweenProperty(light, ENERGY, 0, 0.5f);
            padLock.Close();
            levelUnlocked = false;
            GD.Print($"[Tesla {level}] Visually locked");
        }
    }
}
