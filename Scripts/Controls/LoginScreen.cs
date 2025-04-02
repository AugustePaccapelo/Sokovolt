using Com.IsartDigital.Sokovolt;
using Com.IsartDigital.Tools;
using Godot;
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

		// ----- Nodes ----- \\
		[ExportGroup("LoginScreen")]
		[Export] private Control loginNode;
		private VBoxContainer vBoxHolderLogin;
		private TextEdit inputLoginUsername;
		private LineEdit inputLoginPassword;
        private Button buttonLoginConfirm, buttonLoginGoCreate;
		[Export] private Label labelLoginError;
		private Label labelLoginName, labelLoginUsername, labelLoginPassword;
		private CheckBox checkLoginStayLogged;
		[Export] private VBoxContainer vContLoginUser, vContLoginPass;
		[Export] private Piston loginPiston;

		private Control loginPosHolder;
		private Vector2 animPosLoginName, animPosLoginButtonConfirm, animPosLoginButtonChangeScreen, animPosLoginPassword;
		private List<Vector2> animPosLoginUserName = new List<Vector2>();

        [ExportGroup("CreateScreen")]
        [Export] private Control createNode;
        private VBoxContainer vBoxHolderCreate;
        private TextEdit inputCreateUsername;
		private LineEdit inputCreatePassword, inputCreateConfirmPassword;
        private Button buttonCreateConfirm, buttonCreateGoLogin;
		private Label labelCreateName, labelCreateUsername, labelCreatePassword, labelCreateConfirmPassword;
		[Export] private Label labelCreateErrorPasswords, labelCreateErrorUsername;
		[Export] private VBoxContainer vBoxCreateUser, vBoxCreatePass, vBoxCreateConfirmPass;
        private CheckBox checkCreateStayLogged;

        // ----- Others ----- \\
        [Signal] public delegate void StartGameEventHandler();

		private string userName = "";
		private string password = "";

		public UserGestion userGestion;

		private Vector2 screenSize;

		public bool skipLogin;

        // ---------- FUNCTIONS ---------- \\

        // ----- Ready ----- \\

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

			GetLoginChilds();
			GetCreateChilds();

			GetAllLoginPos();
			
            screenSize = GetWindow().Size;
            Size = screenSize;

			createNode.Hide();

            CustomMinimumSize = GetViewportRect().Size;

			buttonLoginGoCreate.Pressed += ButtonChangeToCreate;
			buttonCreateGoLogin.Pressed += ButtonChangeToLogin;
            buttonLoginConfirm.Pressed += ButtonPressedLogin;
			buttonCreateConfirm.Pressed += ButtonPressedCreate;
        }

		// ----- My Functions ----- \\

		private void GetLoginChilds()
		{
			vBoxHolderLogin = loginNode.GetNode<VBoxContainer>(LoginScreenNames.VBOX_SCREEN_HOLDER);
			labelLoginName = vBoxHolderLogin.GetNode<Label>(LoginScreenNames.LABEL_SCREEN_NAME);
			labelLoginUsername = vContLoginUser.GetNode<Label>(LoginScreenNames.LABEL_USERNAME);
            inputLoginUsername = vContLoginUser.GetNode<TextEdit>(LoginScreenNames.INPUT_USERNAME);
			labelLoginPassword = vContLoginPass.GetNode<Label>(LoginScreenNames.LABEL_PASSWORD);
            inputLoginPassword = vContLoginPass.GetNode<LineEdit>(LoginScreenNames.INPUT_PASSWORD);
			buttonLoginConfirm = vBoxHolderLogin.GetNode<Button>(LoginScreenNames.BUTTON_CONFIRM);
            buttonLoginGoCreate = vBoxHolderLogin.GetNode<Button>(LoginScreenNames.BUTTON_CHANGE_SCREEN);
			checkLoginStayLogged = vBoxHolderLogin.GetNode<CheckBox>(LoginScreenNames.CHECK_STAY_LOGGED);

			loginPosHolder = loginNode.GetNode<Control>(LoginScreenNames.POS_HOLDER);
        }
		private void GetCreateChilds()
		{
            vBoxHolderCreate = createNode.GetNode<VBoxContainer>(LoginScreenNames.VBOX_SCREEN_HOLDER);
            labelCreateName = vBoxHolderCreate.GetNode<Label>(LoginScreenNames.LABEL_SCREEN_NAME);
			labelCreateUsername = vBoxCreateUser.GetNode<Label>(LoginScreenNames.LABEL_USERNAME);
			inputCreateUsername = vBoxCreateUser.GetNode<TextEdit>(LoginScreenNames.INPUT_USERNAME);
			labelCreatePassword = vBoxCreatePass.GetNode<Label>(LoginScreenNames.LABEL_PASSWORD);
			inputCreatePassword = vBoxCreatePass.GetNode<LineEdit>(LoginScreenNames.INPUT_PASSWORD);
			labelCreateConfirmPassword = vBoxCreateConfirmPass.GetNode<Label>(LoginScreenNames.LABEL_CONFIRM_PASSWORD);
			inputCreateConfirmPassword = vBoxCreateConfirmPass.GetNode<LineEdit>(LoginScreenNames.INPUT_CONFIRM_PASSWORD);
			buttonCreateConfirm = vBoxHolderCreate.GetNode<Button>(LoginScreenNames.BUTTON_CONFIRM);
            buttonCreateGoLogin = vBoxHolderCreate.GetNode<Button>(LoginScreenNames.BUTTON_CHANGE_SCREEN);
			checkCreateStayLogged = vBoxHolderCreate.GetNode<CheckBox>(LoginScreenNames.CHECK_STAY_LOGGED);
        }
		private void GetAllLoginPos()
		{
			foreach (Control lChild in loginPosHolder.GetChildren())
			{
				switch (lChild.Name)
				{
					case LoginScreenAnimations.LABEL_SCREEN_NAME:
						animPosLoginName = lChild.GlobalPosition;
                        break;
					case LoginScreenAnimations.BUTTON_CONFIRM:
						animPosLoginButtonConfirm = lChild.GlobalPosition;
						break;
					case LoginScreenAnimations.BUTTON_CHANGE_SCREEN:
						animPosLoginButtonChangeScreen = lChild.GlobalPosition;
						break;
					case LoginScreenAnimations.VBOX_PASSWORD:
						animPosLoginPassword = lChild.GlobalPosition;
                        break;
					case LoginScreenAnimations.VBOX_USERNAME:
						ChildToList(lChild, animPosLoginUserName);
						break;
				}
			}
		}
		private void ChildToList(Control pNode, List<Vector2> pList)
		{
			foreach (Control lChild in pNode.GetChildren())
			{
				pList.Add(lChild.GlobalPosition);
			}
		}
		private void ButtonPressedLogin()
		{
			labelLoginError.Hide();
            userName = inputLoginUsername.Text;
			password = inputLoginPassword.Text;

			if (userGestion.LoginUser(userName, password))
			{
				if (checkLoginStayLogged.ButtonPressed) userGestion.SaveLastUser(userName);
				else userGestion.SaveLastUser();
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

			userName = inputCreateUsername.Text;
            password = inputCreatePassword.Text;

			if (userGestion.RegisterUser(userName, password))
			{
				if (checkCreateStayLogged.ButtonPressed) userGestion.SaveLastUser(userName);
				else userGestion.SaveLastUser();
				GD.Print(password);
				CustomSignals.GetInstance().EmitSignal(CustomSignals.SignalName.GoToMainMenu);
                Hide();
			}
			else labelCreateErrorUsername.Show();
        }
		public void AnimationLoginEnter()
		{
			Tween lTween = CreateTween().SetParallel();

			// Label Login animation
			lTween.TweenProperty(labelLoginName, "global_position", labelLoginName.GlobalPosition, 0.5f).From(animPosLoginName)
				.SetTrans(Tween.TransitionType.Quart).SetEase(Tween.EaseType.Out);
			lTween.TweenProperty(labelLoginName, "scale", labelLoginName.Scale, 0.5f).From(new Vector2(labelLoginName.Scale.X * 0.75f, 0))
				.SetTrans(Tween.TransitionType.Quart).SetEase(Tween.EaseType.Out);

			// Button Login animation
			lTween.TweenProperty(buttonLoginConfirm, "global_position", buttonLoginConfirm.GlobalPosition, 0.5f).From(animPosLoginButtonConfirm)
                .SetTrans(Tween.TransitionType.Quart).SetEase(Tween.EaseType.Out);
            lTween.TweenProperty(buttonLoginConfirm, "scale", buttonLoginConfirm.Scale, 0.5f).From(new Vector2(buttonLoginConfirm.Scale.X * 0.75f, 0))
                .SetTrans(Tween.TransitionType.Quart).SetEase(Tween.EaseType.Out);

			// Button create animation
			lTween.TweenProperty(buttonLoginGoCreate, "global_position", buttonLoginGoCreate.GlobalPosition, 1.5f).From(animPosLoginButtonChangeScreen);
			//lTween.TweenProperty(loginPiston, "global_position", loginPiston.GlobalPosition, 1.25f).From(new Vector2(loginPiston.GlobalPosition.X, screenSize.Y));
			loginPiston.Extend();
			

			// Password label and input animation
			lTween.TweenProperty(vContLoginPass, "global_position", vContLoginPass.GlobalPosition, 1.5f).From(animPosLoginPassword);

			// Username label and Input animation
			Tween lUserTween = CreateTween();
            lUserTween.TweenProperty(vContLoginUser, "global_position", animPosLoginUserName[1], 0.5f).From(animPosLoginUserName[0]);
            lUserTween.Chain().TweenProperty(vContLoginUser, "global_position", animPosLoginUserName[2], 0.75f).From(animPosLoginUserName[1])
				.SetTrans(Tween.TransitionType.Bounce).SetEase(Tween.EaseType.Out);
            lUserTween.Chain().TweenProperty(vContLoginUser, "global_position", vContLoginUser.GlobalPosition, 0.25f).From(animPosLoginUserName[2]);			

            lTween.Play();
			lUserTween.Play();

        }
		private void AnimationLoginExit() { }
        private void AnimationCreateEnter() { }
        private void AnimationCreateExit() { }
        private void ButtonChangeToLogin()
		{
			createNode.Hide();
            labelLoginError.Hide();
            loginNode.Show();
			AnimationLoginEnter();
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