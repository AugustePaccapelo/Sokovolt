using Com.IsartDigital.SokoVolt.Managers;
using Godot;
using System;
using System.Collections.Generic;

// Author : Auguste Paccapelo

namespace Com.IsartDigital.SokoVolt
{
	public partial class LoginScreen : Control
	{
		// ---------- VARIABLES ---------- \\

		#region // ----- Singleton ----- \\

		static private LoginScreen instance;

		static public LoginScreen GetInstance()
		{
			if (instance == null) instance = new LoginScreen();
			return instance;
		}

		#endregion

		// ----- Paths ----- \\

		// ----- Nodes ----- \\
		[Export] private Control loginNode;
		[Export] private TextEdit inputLoginUsername;
        [Export] private TextEdit inputLoginPassword;
        [Export] private Button buttonLoginConfirm;
        [Export] private Button buttonLoginGoCreate;

		[Export] private Control createNode;
		[Export] private TextEdit inputCreateUsername;
        [Export] private TextEdit inputCreatePassword;
        [Export] private TextEdit inputCreateConfirmPassword;
		[Export] private Button buttonCreateConfirm;
		[Export] private Button buttonCreateGoLogin;

		private UIManager uiManager;

        // ----- Others ----- \\
        [Signal] public delegate void StartGameEventHandler();

        private const string USERNAME_KEY = "Username";
		private const string PASSWORD_KEY = "Password";

		private Dictionary<string, string> loginInfo = new Dictionary<string, string>();

        // ---------- FUNCTIONS ---------- \\

        // ----- Ready & Process ----- \\

		public override void _Ready()
		{
			#region // ----- Singleton ----- \\

			if (instance != null)
			{
				GD.Print(Name + " Instance already exist, destroying the last added.");
				QueueFree();
				return;
			}

			instance = this;

			#endregion

			base._Ready();

			CustomMinimumSize = GetViewportRect().Size;

			uiManager = GetParent<UIManager>();

            StartGame += uiManager.GameStart;

            loginInfo.Add(USERNAME_KEY, "");
			loginInfo.Add(PASSWORD_KEY, "");

			buttonLoginGoCreate.Pressed += ButtonChangeToCreate;
			buttonCreateGoLogin.Pressed += ButtonChangeToLogin;
            buttonLoginConfirm.Pressed += ButtonPressedLogin;
			buttonCreateConfirm.Pressed += ButtonPressedCreate;
		}

		public override void _Process(double pDelta)
		{
			float lDelta = (float)pDelta;

			base._Process(lDelta);
		}

		// ----- My Functions ----- \\

		private void ButtonPressedLogin()
		{
			loginInfo[USERNAME_KEY] = inputLoginUsername.Text;
			loginInfo[PASSWORD_KEY] = inputLoginPassword.Text;
			GD.Print("Trying to login with: ", loginInfo[USERNAME_KEY], " and ", loginInfo[PASSWORD_KEY]);
			EmitSignal(SignalName.StartGame);
			QueueFree();
		}

		private void ButtonPressedCreate()
		{
			string lPassword = inputCreatePassword.Text;
			string lPasswordConfirm = inputCreateConfirmPassword.Text;
			if (lPassword != lPasswordConfirm)
			{
				GD.Print("Not same password !");
				return;
			}

			loginInfo[USERNAME_KEY] = inputCreateUsername.Text;
            loginInfo[PASSWORD_KEY] = inputCreatePassword.Text;
			GD.Print("creating account with: ", loginInfo[USERNAME_KEY], " and ", loginInfo[PASSWORD_KEY]);
            EmitSignal(SignalName.StartGame);
			QueueFree();
        }

		private void ButtonChangeToLogin()
		{
			createNode.Hide();
			loginNode.Show();
		}

		private void ButtonChangeToCreate()
		{
			loginNode.Hide();
			createNode.Show();
		}

		// ----- Destructor ----- \\

		protected override void Dispose(bool pDisposing)
		{
			#region // ----- Singleton ----- \\

			if (pDisposing && instance == this) instance = null;

			#endregion

			base.Dispose(pDisposing);
		}
	}
}
