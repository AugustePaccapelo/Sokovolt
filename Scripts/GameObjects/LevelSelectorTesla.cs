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
                            lUserData[lCurrentUser] = lUserDict;
                            UserGestion.GetInstance().SaveUserData(lUserData);
                            GD.Print($"[Tesla {level}] was visually and logically unlocked (forced update)");
                        }
                    }
                }
            }
        }


        public void UnlockAll()
        {
            Dictionary userData = UserGestion.GetInstance().GetUserData();
            string currentUser = UserGestion.GetInstance().currentUser;

            if (string.IsNullOrEmpty(currentUser) || !userData.ContainsKey(currentUser)) return;

            Dictionary userDict = (Dictionary)userData[currentUser];
            if (!userDict.ContainsKey(LEVELS)) return;

            Dictionary levels = (Dictionary)userDict[LEVELS];
            string levelKey = $"level{level}";

            if (!levels.ContainsKey(levelKey)) return;

            Dictionary levelData = (Dictionary)levels[levelKey];

            if (!allLevelAreUnlocked)
            {
                // Si le niveau est verrouillé, on le déverrouille et on met à jour les données
                if (levelData.ContainsKey(LOCKED) && (bool)levelData[LOCKED])
                {
                    levelData[LOCKED] = false;
                    levels[levelKey] = levelData;
                    userDict[LEVELS] = levels;
                    userData[currentUser] = userDict;

                    UserGestion.GetInstance().SaveUserData(userData);

                    UnlockLevel();
                    GD.Print($"[Tesla {level}] was locked, now unlocked via UnlockAll()");
                }
                allLevelAreUnlocked = true;
            }
            else
            {
                if (levelData.ContainsKey(LOCKED) && !(bool)levelData[LOCKED])
                {
                    levelData[LOCKED] = true;
                    levels[levelKey] = levelData;
                    userDict[LEVELS] = levels;
                    userData[currentUser] = userDict;

                    UserGestion.GetInstance().SaveUserData(userData);

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
