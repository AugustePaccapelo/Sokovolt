using Com.IsartDigital.SokoVolt.GameObjects;
using Godot;
using System;

// Author : Auguste Paccapelo

namespace Com.IsartDigital.SokoVolt
{
	public partial class CustomSignals : Node2D
	{
		// ---------- VARIABLES ---------- \\

		#region // ----- Singleton ----- \\

		static private CustomSignals instance;

		static public CustomSignals GetInstance()
		{
			if (instance == null) instance = new CustomSignals();
			return instance;
		}

		#endregion

		[Signal] public delegate void GoToLoginScreenEventHandler();
        [Signal] public delegate void GoToMainMenuEventHandler();
        [Signal] public delegate void GoToLevelSelectorEventHandler();
        [Signal] public delegate void GoToLevelCreatorEventHandler();
        [Signal] public delegate void StartRechercheEventHandler();

        [Signal] public delegate void PlayerMovedEventHandler();
		[Signal] public delegate void BoxTeslaMovedEventHandler();
        [Signal] public delegate void BoxTeslaCalculsDoneEventHandler();
        [Signal] public delegate void GoalBulbStateChangedEventHandler();
    }
}
