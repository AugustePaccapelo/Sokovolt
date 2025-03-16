using Com.IsartDigital.SokoVolt.Managers;
using Godot;
using System;

// Author : Noe Sales

namespace Com.IsartDigital.SokoVolt.GameObjects {
	
	public partial class LevelSelectorTesla : Sprite2D
	{
		[Export] public LevelSelectorBolt electricBolt;
		[Export] public Node2D electricBoltConstant;
		[Export] public Padlock padLock;
		[Export] public Button levelButton;
		[Export] public PointLight2D[] lightEmission;

        public LevelSelectorTesla nextTesla;
		public bool levelUnlocked = false;
		public int level;
        private bool allLevelAreUnlocked = false;

        [Signal] public delegate void ButtonLoadLevelEventHandler(int pLevel);

        public override void _Ready()
        {
            if (LevelSelector.GetInstance() != null && LevelManager.GetInstance() != null) Init();
            else CallDeferred(nameof(Init));
        }

        private void Init()
        {
            LevelSelector.GetInstance().UnlockAllLevel += UnlockAll;
            ButtonLoadLevel += LevelManager.GetInstance().LevelLoaderFonc;
            levelButton.Pressed += LevelUnlockedCheck;
        }

        private void LevelUnlockedCheck()
        {
            if (levelUnlocked)
            {
                EmitSignal(nameof(ButtonLoadLevel), level);
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
            levelButton.Disabled = false;
            electricBolt.animationPlayer.Play("start_animation");
            Tween lTween = CreateTween();
            foreach (PointLight2D light in lightEmission) lTween.TweenProperty(light, "energy", 3, 0.5f);
            padLock.Open();
            levelUnlocked = true;
        }

        private void LockLevel()
        {
            levelButton.Disabled = true;
            electricBolt.animationPlayer.Play("end_animation");
            Tween lTween = CreateTween();
            foreach (PointLight2D light in lightEmission) lTween.TweenProperty(light, "energy", 0, 0.5f);
            padLock.Close();
            levelUnlocked = false;
        }
    }
}
