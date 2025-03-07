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
		[ExportGroup("LoginScreen")]
		[Export] private Control loginNode;
		[Export] private TextEdit inputLoginUsername;
		[Export] private LineEdit inputLoginPassword;
        [Export] private Button buttonLoginConfirm, buttonLoginGoCreate;
		[Export] private Label labelLoginName, labelLoginUsername, labelLoginPassword, labelLoginError;

        [ExportGroup("CreateScreen")]
        [Export] private Control createNode;
		[Export] private TextEdit inputCreateUsername;
		[Export] private LineEdit inputCreatePassword, inputCreateConfirmPassword;
        [Export] private Button buttonCreateConfirm, buttonCreateGoLogin;
		[Export] private Label labelCreateName, labelCreateUsername, labelCreatePassword, labelCreateConfirmPassword, labelCreateErrorPasswords, labelCreateErrorUsername;

        // ----- Others ----- \\
        [Signal] public delegate void StartGameEventHandler();

		private string username = "";
		private string password = "";

		public UserGestion userGestion;

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

			CustomSignals.GetInstance().GoToLoginScreen += () =>
			{
                loginNode.Show();
                AnimationLoginEnter();
            };

			createNode.Hide();

            CustomMinimumSize = GetViewportRect().Size;

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
			labelLoginError.Hide();
            username = inputLoginUsername.Text;
			password = inputLoginPassword.Text;

			if (userGestion.LoginUser(username, password))
			{
				CustomSignals.GetInstance().EmitSignal(CustomSignals.SignalName.GoToMainMenu);
				Hide();
			}
			else labelLoginError.Show();
		}

		private void ButtonPressedCreate()
		{
			labelCreateErrorPasswords.Hide();
			labelCreateErrorUsername.Hide();
            string lPassword = inputCreatePassword.Text;
			string lPasswordConfirm = inputCreateConfirmPassword.Text;
			if (lPassword != lPasswordConfirm)
			{
				labelCreateErrorPasswords.Show();
				return;
			}

			username = inputCreateUsername.Text;
            password = inputCreatePassword.Text;

			if (userGestion.RegisterUser(username, password))
			{
                CustomSignals.GetInstance().EmitSignal(CustomSignals.SignalName.GoToMainMenu);
                Hide();
			}
			else labelCreateErrorUsername.Show();
        }

		private void AnimationLoginEnter()
		{
			
		}

		private void AnimationLoginExit()
		{

		}

        private void AnimationCreateEnter()
        {

        }

        private void AnimationCreateExit()
        {

        }

        private void ButtonChangeToLogin()
		{
			createNode.Hide();
            labelLoginError.Hide();
            loginNode.Show();
		}

		private void ButtonChangeToCreate()
		{
			loginNode.Hide();
            labelCreateErrorPasswords.Hide();
            labelCreateErrorUsername.Hide();
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
