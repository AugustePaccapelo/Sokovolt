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
		[Export] private TextEdit inputLoginUsername, inputLoginPassword;
        [Export] private Button buttonLoginConfirm, buttonLoginGoCreate;
		[Export] private Label labelLoginName, labelLoginUsername, labelLoginPassword;

        [ExportGroup("CreateScreen")]
        [Export] private Control createNode;
		[Export] private TextEdit inputCreateUsername, inputCreatePassword, inputCreateConfirmPassword;
		[Export] private Button buttonCreateConfirm, buttonCreateGoLogin;
		[Export] private Label labelCreateName, labelCreateUsername, labelCreatePassword, labelCreateConfirmPassword;

		private UIManager uiManager;

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

			createNode.Hide();
			loginNode.Hide();

			CustomMinimumSize = GetViewportRect().Size;

			uiManager = GetParent<UIManager>();

            StartGame += uiManager.GameStart;

			buttonLoginGoCreate.Pressed += ButtonChangeToCreate;
			buttonCreateGoLogin.Pressed += ButtonChangeToLogin;
            buttonLoginConfirm.Pressed += ButtonPressedLogin;
			buttonCreateConfirm.Pressed += ButtonPressedCreate;

			ButtonChangeToLogin();
        }

		public override void _Process(double pDelta)
		{
			float lDelta = (float)pDelta;

			base._Process(lDelta);
		}

		// ----- My Functions ----- \\

		private void ButtonPressedLogin()
		{
			username = inputLoginUsername.Text;
			password = inputLoginPassword.Text;

			//To remove when UserGestion finished
            EmitSignal(SignalName.StartGame);
            Hide();
			return;

            if (userGestion.LoginUser(username, password))
			{
                EmitSignal(SignalName.StartGame);
                Hide();
            }
		}

		private void ButtonPressedCreate()
		{
			string lPassword = inputCreatePassword.Text;
			string lPasswordConfirm = inputCreateConfirmPassword.Text;
			if (lPassword != lPasswordConfirm)
			{
				GD.Print("Passwords does not match !");
				return;
			}

			username = inputCreateUsername.Text;
            password = inputCreatePassword.Text;

            //To remove when UserGestion finished
            EmitSignal(SignalName.StartGame);
            Hide();
            return;

            if (userGestion.RegisterUser(username, password))
			{
                EmitSignal(SignalName.StartGame);
                Hide();
            }
        }

		public void AnimationLoginEnter()
		{
			

			Tween lTween = CreateTween();

			lTween.TweenProperty(labelLoginName, "scale", Vector2.One, 1f).From(Vector2.Zero)
				.SetTrans(Tween.TransitionType.Elastic).SetEase(Tween.EaseType.Out);
            lTween.Parallel().TweenProperty(labelLoginUsername, "global_position", labelLoginUsername.GlobalPosition, 0.25f).From(Vector2.Zero)
                .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
            lTween.Play();
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
