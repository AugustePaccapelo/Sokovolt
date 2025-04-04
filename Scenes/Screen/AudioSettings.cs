using Godot;
using System;
using RobotnikSokoban.Scripts.Managers;

// Author : William Soukai

namespace Com.IsartDigital.SokoVolt {
	
	public partial class AudioSettings : Control
    {
        [Export] private HSlider masterBar;
		[Export] private HSlider musicBar;
		[Export] private HSlider sfxBar;
        [Export] private Button englishButton;
        [Export] private Button frenchButton;
        [Export] private Button backButton;
        private const string sfx = "SFX";
        private const string music = "Music";
        private const string master = "Master";


        public static AudioSettings Instance { get; private set; }
		public override void _Ready()
		{
            if (Instance != null) {
                Free();
                GD.Print($"{nameof(AudioSettings)} Instance already exist, destroying the last added.");
                return;
            }
            Instance = this;

            masterBar.ValueChanged += MasterValue;
            musicBar.ValueChanged += MusicValue;
            sfxBar.ValueChanged += SFXValus;
            backButton.Pressed += () => CustomSignals.GetInstance().EmitSignal(CustomSignals.SignalName.GoToMainMenu);
                      
            englishButton.Pressed += () =>
            {
                SetLanguage("en");               
                englishButton.Disabled = true;
                frenchButton.Disabled = false;
            };
            frenchButton.Pressed += () =>
            {
                SetLanguage("fr");
                englishButton.Disabled = false;
                frenchButton.Disabled = true;
            };
        }

        private void SFXValus(double pValue)
        {
            AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex(sfx), (float)Mathf.LinearToDb(pValue));
            SongManager.Instance.ambientDict[EnumSong.AmbientSong.RobloxDeath].Play();
        }

        private void MusicValue(double pValue)
        {
            AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex(music), (float)Mathf.LinearToDb(pValue));
        }

        public void MasterValue(double pValue)
        {
            AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex(master), (float)Mathf.LinearToDb(pValue));
        }

        private void SetLanguage(string pLanguage)
        {
            TranslationServer.SetLocale(pLanguage);
        }



		protected override void Dispose(bool pDisposing)
		{

		}
	}
}
