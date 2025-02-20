using Com.IsartDigital.SokoVolt.Managers;
using Godot;
using System;

// Author : Noe Sales

namespace Com.IsartDigital.SokoVolt.GameObjects {
	
	public partial class LevelSelectorTesla : Sprite2D
	{
		[Export] public LevelSelectorBolt electricBolt;
		[Export] public Node2D impactEffect;
		[Export] public Button levelButton;

		public bool isConnected = false;
		public bool levelUnlocked = false;
		public int level;

        [Signal] public delegate void ButtonLoadLevelEventHandler(int pLevel);

        public override void _Ready()
        {
            if (LevelSelector.GetInstance() != null && LevelManager.GetInstance() != null) Init();
            else CallDeferred(nameof(Init));
        }

        private void Init()
        {
            LevelSelector.GetInstance().UnlockAllLevel += UnlockLevel;
            ButtonLoadLevel += LevelManager.GetInstance().LevelLoader;

            levelButton.Pressed += () => EmitSignal(nameof(ButtonLoadLevel), level);
        }

        private void UnlockLevel()
        {
            GD.Print("Level unlocked");
            electricBolt.animationPlayer.Play("start_animation");
            levelUnlocked = true;
        }

        private void LockLevel()
        {
            electricBolt.animationPlayer.Play("end_animation");
        }

        public override void _Process(double delta)
        {
			isConnected = (levelUnlocked == true) ? true : false;
            impactEffect.Visible = (isConnected == true) ? true : false;
        }
    }
}
