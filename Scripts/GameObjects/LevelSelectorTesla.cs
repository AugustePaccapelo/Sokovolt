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

        string currentUser = UserGestion.GetInstance().currentUser;

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

            if (!lUserData.ContainsKey(currentUser)) return;
            Dictionary lUserDict = (Dictionary)lUserData[currentUser];
            if (!lUserDict.ContainsKey(LEVELS)) return;

            Dictionary lLevels = (Dictionary)lUserDict[LEVELS];
            string lLevelKey = $"level{level}";

            // Check if all levels are unlocked
            bool allUnlocked = true;
            foreach (Dictionary levelData in lLevels.Values)
            {
                if ((bool)levelData.GetValueOrDefault(LOCKED, true))
                {
                    allUnlocked = false;
                    break;
                }
            }
            allLevelAreUnlocked = allUnlocked;

            if (lLevels.ContainsKey(lLevelKey))
            {
                Dictionary lLevelData = (Dictionary)lLevels[lLevelKey];
                bool lIsLocked = (bool)lLevelData.GetValueOrDefault(LOCKED, true);

                if (!levelUnlocked)
                {
                    if (!lIsLocked || level == 0)
                    {
                        UnlockLevel();

                        // Update JSON if level 0 was visually unlocked but still marked locked
                        if (level == 0 && lIsLocked)
                        {
                            lLevelData[LOCKED] = false;
                            lLevels[lLevelKey] = lLevelData;
                            lUserDict[LEVELS] = lLevels;
                            lUserData[currentUser] = lUserDict;
                            UserGestion.GetInstance().SaveUserData(lUserData);
                            GD.Print($"[Tesla {level}] was visually and logically unlocked (forced update)");
                        }
                    }
                }
            }
        }


        public void UnlockAll()
        {
            Dictionary lUserData = UserGestion.GetInstance().GetUserData();

            if (string.IsNullOrEmpty(currentUser) || !lUserData.ContainsKey(currentUser)) return; //If currentUser is null : return

            Dictionary lUserDict = (Dictionary)lUserData[currentUser];
            if (!lUserDict.ContainsKey(LEVELS)) return; //If he don't have levels in his JSON : return

            Dictionary lLevels = (Dictionary)lUserDict[LEVELS];
            string lLevelKey = $"level{level}";

            if (!lLevels.ContainsKey(lLevelKey)) return; //if levels list don't have level reference : return

            Dictionary lLevelData = (Dictionary)lLevels[lLevelKey];

            if (!allLevelAreUnlocked)
            {
                if (lLevelData.ContainsKey(LOCKED) && (bool)lLevelData[LOCKED]) //If the level is locked, we unlock it and update the data
                {
                    lLevelData[LOCKED] = false;
                    lLevels[lLevelKey] = lLevelData;
                    lUserDict[LEVELS] = lLevels;
                    lUserData[currentUser] = lUserDict;

                    UserGestion.GetInstance().SaveUserData(lUserData);

                    UnlockLevel();
                    GD.Print($"[Tesla {level}] was locked, now unlocked via UnlockAll()");
                }
                allLevelAreUnlocked = true;
            }
            else
            {
                if (lLevelData.ContainsKey(LOCKED) && !(bool)lLevelData[LOCKED]) //If the level is unlocked, we lock it and update the data
                {
                    lLevelData[LOCKED] = true;
                    lLevels[lLevelKey] = lLevelData;
                    lUserDict[LEVELS] = lLevels;
                    lUserData[currentUser] = lUserDict;

                    UserGestion.GetInstance().SaveUserData(lUserData);

                    LockLevel();
                    GD.Print($"[Tesla {level}] was locked, now unlocked via UnlockAll()");
                }
                allLevelAreUnlocked = false;
            }
        }

        public void UnlockLevel()
        {
            electricBolt.animationPlayer.Play(START_ANIMATION);
            Tween lTween = CreateTween();
            foreach (PointLight2D light in lightEmission) lTween.TweenProperty(light, ENERGY, 3, 0.5f);
            padLock.Open();
            levelUnlocked = true;
        }

        private void LockLevel()
        {
            electricBolt.animationPlayer.Play(END_ANIMATION);
            Tween lTween = CreateTween();
            foreach (PointLight2D light in lightEmission) lTween.TweenProperty(light, ENERGY, 0, 0.5f);
            padLock.Close();
            levelUnlocked = false;
        }
    }
}
