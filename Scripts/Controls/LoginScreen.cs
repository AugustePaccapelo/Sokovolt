using Com.IsartDigital.ProjectName;
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
		private UserGestion userGestion;

        // ----- Others ----- \\
        [Signal] public delegate void StartGameEventHandler();

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

            userGestion = UserGestion.GetInstance();

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
			string lUsername = inputLoginUsername.Text;
			string lPassword = inputLoginPassword.Text;

			if (userGestion.LoginUser(lUsername, lPassword))
			{
				GD.Print("Login successful");
                EmitSignal(SignalName.StartGame);
                QueueFree();
            }

            else GD.Print("Invalid username or password");
		}

		private void ButtonPressedCreate()
		{
			string lUsername = inputCreateUsername.Text;
            string lPassword = inputCreatePassword.Text;
			string lPasswordConfirm = inputCreateConfirmPassword.Text;

			if (lPassword != lPasswordConfirm)
			{
				GD.Print("Passwords do not match!");
				return;
			}

			if (userGestion.RegisterUser(lUsername, lPassword))
			{
				GD.Print("Account successfully created");
				ButtonChangeToLogin();
			}
			else GD.Print("Username already taken!");

   //         EmitSignal(SignalName.StartGame);
			//QueueFree(); ask Auguste about this
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
