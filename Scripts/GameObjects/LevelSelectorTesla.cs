using Com.IsartDigital.Sokovolt;
using Com.IsartDigital.SokoVolt.Managers;
using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

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
            await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
            InitLevelStateUserData();
        }

        private void InitLevelStateUserData()
        {
            var lUserData = UserGestion.GetInstance().GetUserData(); 
            var lCurrentUser = UserGestion.GetInstance().currentUser;

            if (string.IsNullOrEmpty(lCurrentUser))
            {
                GD.PrintErr("currentUser is null, retrying later...");
                CallDeferred(nameof(InitLevelStateUserData));
                return;
            }

            if (!lUserData.ContainsKey(lCurrentUser)) return;
            var lUserDict = (Dictionary)lUserData[lCurrentUser];
            if (!lUserDict.ContainsKey("levels")) return;

            var lLevels = (Dictionary)lUserDict["levels"];
            string lLevelKey = $"level{level}"; // key of the current level ex: level0, level1, level2...

            if (lLevels.ContainsKey(lLevelKey))
            {
                var lLevelData = (Dictionary)lLevels[lLevelKey];
                bool lIsLocked = (bool)lLevelData.GetValueOrDefault("locked", true); //lIsLocked = true par defaut
                GD.Print($"[Tesla {level}] Locked: {lIsLocked}, Unlocked: {levelUnlocked}");

                if (!levelUnlocked)
                {
                    if(!lIsLocked || level == 0) UnlockLevel();
                    else LockLevel();
                }
            }
        }

        public void UnlockAll()
        {
            if (!allLevelAreUnlocked)
            {
                UnlockLevel();
                LevelSelector.GetInstance().buttonUnlockAll.Text = "LockAll";
                allLevelAreUnlocked = true;
            }
            else if (allLevelAreUnlocked && level != 0)
            {
                LockLevel();
                LevelSelector.GetInstance().buttonUnlockAll.Text = "UnlockAll";
                allLevelAreUnlocked = false;
            }
        }

        public void UnlockLevel()
        {
            electricBolt.animationPlayer.Play("start_animation");
            Tween lTween = CreateTween();
            foreach (PointLight2D light in lightEmission) lTween.TweenProperty(light, "energy", 3, 0.5f);
            padLock.Open();
            levelUnlocked = true;
            GD.Print($"[Tesla {level}] Visually unlocked");
        }

        private void LockLevel()
        {
            electricBolt.animationPlayer.Play("end_animation");
            Tween lTween = CreateTween();
            foreach (PointLight2D light in lightEmission) lTween.TweenProperty(light, "energy", 0, 0.5f);
            padLock.Close();
            levelUnlocked = false;
            GD.Print($"[Tesla {level}] Visually locked");
        }
    }
}
