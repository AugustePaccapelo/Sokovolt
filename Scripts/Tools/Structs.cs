using Godot;
using System;

// Author : Auguste Paccapelo

namespace Com.IsartDigital.Tools
{
	public struct LoginScreenNames
	{
		public const string POS_HOLDER = "PosHolder";
		public const string VBOX_SCREEN_HOLDER = "VBoxScreenHolder";
		public const string LABEL_SCREEN_NAME = "LabName";
		public const string LABEL_USERNAME = "LabUsername";
		public const string LABEL_PASSWORD = "LabPassword";
		public const string LABEL_CONFIRM_PASSWORD = "LabConfirmPassword";
		public const string INPUT_USERNAME = "InputUsername";
		public const string INPUT_PASSWORD = "InputPassword";
		public const string INPUT_CONFIRM_PASSWORD = "InputConfirmPassword";
		public const string BUTTON_CONFIRM = "ButtonConfirm";
		public const string BUTTON_CHANGE_SCREEN = "ChangeScreen";
		public const string CHECK_STAY_LOGGED = "StayLogged";
    }

	public struct LoginScreenAnimations
	{
		public const string VBOX_USERNAME = "Username";
		public const string LABEL_SCREEN_NAME = "Name";
		public const string BUTTON_CHANGE_SCREEN = "Create";
		public const string VBOX_PASSWORD = "Password";
		public const string BUTTON_CONFIRM = "Confirm";
	}
}
