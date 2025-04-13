using Godot;
using System;
using RobotnikSokoban.Scripts.Managers;

// Author : William Soukai

namespace Com.IsartDigital.ProjectName {
	
	public partial class button : Button
    {

        [Export] private PointLight2D light;
        public override void _Ready()
        {
            ButtonDown += OnButtonDown;
            ButtonUp += OnButtonUp;
        }

        private void OnButtonUp()
        {
            if (light != null)
                light.Show(); 
        }

        private void OnButtonDown()
        {
            if (light != null)
                light.Hide(); 
            SongManager.Instance.ambientDict[EnumSong.AmbientSong.ButonSong].Play();
        }
	}
}
