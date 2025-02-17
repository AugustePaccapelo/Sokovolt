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

		[Signal] public delegate void PlayerMovedEventHandler();
		[Signal] public delegate void BoxTeslaMovedEventHandler();
	}
}
