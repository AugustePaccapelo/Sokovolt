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
		[Export] private VBoxContainer vContLoginUser, vContLoginPass;

		[Export] private Control vContLogPosParent, vLabNameLogPosParent, vButCreateLogPosParent, vContLogPassPosParent;

		private List<Control> vContLoginUserPos = new List<Control>();
		private List<Control> labelLoginNamePos = new List<Control>();
        private List<Control> buttonGoCreateNamePos = new List<Control>();
        private List<Control> vContLogPassPos = new List<Control>();

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

		private Vector2 screenSize;

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

			foreach (Control lPos in vContLogPosParent.GetChildren()) vContLoginUserPos.Add(lPos);
            foreach (Control lPos in vLabNameLogPosParent.GetChildren()) labelLoginNamePos.Add(lPos);
            foreach (Control lPos in vButCreateLogPosParent.GetChildren()) buttonGoCreateNamePos.Add(lPos);
            foreach (Control lPos in vContLogPassPosParent.GetChildren()) vContLogPassPos.Add(lPos);
			
            screenSize = GetWindow().Size;
            Size = screenSize;

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
			Tween lTween = CreateTween().SetParallel();

			// Label Login animation
			lTween.TweenProperty(labelLoginName, "global_position", labelLoginName.GlobalPosition, 0.5f).From(labelLoginNamePos[0].GlobalPosition)
				.SetTrans(Tween.TransitionType.Quart).SetEase(Tween.EaseType.Out);
			lTween.TweenProperty(labelLoginName, "scale", labelLoginName.Scale, 0.5f).From(new Vector2(labelLoginName.Scale.X * 0.75f, 0))
				.SetTrans(Tween.TransitionType.Quart).SetEase(Tween.EaseType.Out);

			// Button Login animation
			lTween.TweenProperty(buttonLoginConfirm, "global_position", buttonLoginConfirm.GlobalPosition, 0.5f).From(new Vector2(0, buttonLoginConfirm.GlobalPosition.Y))
                .SetTrans(Tween.TransitionType.Quart).SetEase(Tween.EaseType.Out);
            lTween.TweenProperty(buttonLoginConfirm, "scale", buttonLoginConfirm.Scale, 0.5f).From(new Vector2(buttonLoginConfirm.Scale.X * 0.75f, 0))
                .SetTrans(Tween.TransitionType.Quart).SetEase(Tween.EaseType.Out);

			// Button create animation
			lTween.TweenProperty(buttonLoginGoCreate, "global_position", buttonLoginGoCreate.GlobalPosition, 1.5f).From(buttonGoCreateNamePos[0].GlobalPosition);

			// Password label and input animation
			lTween.TweenProperty(vContLoginPass, "global_position", vContLoginPass.GlobalPosition, 1.5f).From(vContLogPassPos[0].GlobalPosition);

			// Username label and Input animation
			Tween lUserTween = CreateTween();
            lUserTween.TweenProperty(vContLoginUser, "global_position", vContLoginUserPos[1].GlobalPosition, 0.75f).From(vContLoginUserPos[0].GlobalPosition);
            lUserTween.Chain().TweenProperty(vContLoginUser, "global_position", vContLoginUserPos[1].GlobalPosition, 0.5f);
            lUserTween.Chain().TweenProperty(vContLoginUser, "global_position", vContLoginUserPos[2].GlobalPosition, 0.75f).From(vContLoginUserPos[1].GlobalPosition)
				.SetTrans(Tween.TransitionType.Bounce).SetEase(Tween.EaseType.Out);
            lUserTween.Chain().TweenProperty(vContLoginUser, "global_position", vContLoginUser.GlobalPosition, 0.5f).From(vContLoginUserPos[2].GlobalPosition);			

            lTween.Play();
			lUserTween.Play();

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
