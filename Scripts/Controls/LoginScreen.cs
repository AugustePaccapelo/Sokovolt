using Com.IsartDigital.Sokovolt;
using Com.IsartDigital.Tools;
using Godot;
using RobotnikSokoban.Scripts.Managers;
using System.Collections.Generic;
using System.Runtime.InteropServices;

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
		[ExportGroup("Environment")]
		[Export] private LightningNode lightning;
		[Export] private Control lightningStartPos, lightningEndPos;
		[Export] private Control arrowsHolder;
		private TextureRect[] arrows;
		[Export] private float arrowMinRotation, arrowMaxRotation;
		[Export] private float arrowMinSpeed, arrowMaxSpeed;
		private float[] currentArrowsSpeeds;
		private float minTimeSpeedChange = 0.5f;
		private float maxTimeSpeedChange = 2f;
		private float[] nextSpeedChange;
		private int arrowCount;
		[Export] private Control lightHolder;
		private PointLight2D[] allLights;
		[Export] private float lightsMinOnDuration, lightsMaxOnDuration;
		[Export] private float lightsMinOffDuration, lightsMaxOffDuration;
		private int lightsCount;
		private float[] currentLightOnDurations;
		private float[] currentLightOffDurations;
		private float[] lightsEnergies;

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
		private bool isAnimated = false;

		private RandomNumberGenerator rand = new RandomNumberGenerator();

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

			rand.Randomize();

			GetLoginChilds();
			GetCreateChilds();

            screenSize = GetWindow().Size;
            Size = screenSize;

			createNode.Hide();

            CustomMinimumSize = GetViewportRect().Size;

			buttonLoginGoCreate.Pressed += ButtonChangeToCreate;
			buttonCreateGoLogin.Pressed += ButtonChangeToLogin;
            buttonLoginConfirm.Pressed += ButtonPressedLogin;
			buttonCreateConfirm.Pressed += ButtonPressedCreate;

			CustomSignals.GetInstance().GoToLoginScreen += ButtonChangeToLogin;
			CustomSignals.GetInstance().GoToLoginScreen += AnimationLoginEnter;
			CustomSignals.GetInstance().GoToMainMenu += AnimationLoginExit;

			arrowCount = arrowsHolder.GetChildCount();
			arrows = new TextureRect[arrowCount];
			currentArrowsSpeeds = new float[arrowCount];
			nextSpeedChange = new float[arrowCount];
			for (int i = 0; i < arrowCount; i++)
			{
				arrows[i] = arrowsHolder.GetChild<TextureRect>(i);
			}

			lightsCount = lightHolder.GetChildCount();
			allLights = new PointLight2D[lightsCount];
			currentLightOnDurations = new float[lightsCount];
			currentLightOffDurations = new float[lightsCount];
			lightsEnergies = new float[lightsCount];
            for (int i = 0; i < lightsCount; i++)
			{
				allLights[i] = lightHolder.GetChild<PointLight2D>(i);
				lightsEnergies[i] = allLights[i].Energy;
			}
        }

        public override void _Process(double pDelta)
        {
            base._Process(pDelta);
			float lDelta = (float)pDelta;

			if (!isAnimated) return;

			AnimateArrows(lDelta);
			AnimateLights(lDelta);
        }

		// ----- My Functions ----- \\

		private void AnimateLights(float pDelta)
		{
			for (int i = 0; i < lightsCount; i++)
			{
				if (!allLights[i].Visible && currentLightOffDurations[i] <= 0)
				{
					allLights[i].Show();
					currentLightOnDurations[i] = rand.RandfRange(lightsMinOnDuration, lightsMaxOnDuration);
					Tween lTween = CreateTween();
					lTween.TweenProperty(allLights[i], "energy", lightsEnergies[i], currentLightOnDurations[i] * 0.5f).From(0f);
					lTween.Chain().TweenProperty(allLights[i], "energy", 0f, currentLightOnDurations[i] * 0.5f).From(lightsEnergies[i]);
					lTween.Play();
				}
				else
				{
					currentLightOffDurations[i] -= pDelta;
				}

				if (allLights[i].Visible && currentLightOnDurations[i] <= 0)
				{
                    allLights[i].Hide();
                    currentLightOffDurations[i] = rand.RandfRange(lightsMinOffDuration, lightsMaxOffDuration);
                }
				else
				{
					currentLightOnDurations[i] -= pDelta;
				}
			}
		}

		private void AnimateArrows(float pDelta)
		{
			for (int i = 0; i < arrowCount; i++)
			{
				if (nextSpeedChange[i] <= 0)
				{
					nextSpeedChange[i] = rand.RandfRange(minTimeSpeedChange, maxTimeSpeedChange);
					currentArrowsSpeeds[i] = rand.RandfRange(arrowMinSpeed, arrowMaxSpeed);
				}
				else
				{
                    nextSpeedChange[i] -= pDelta;
					arrows[i].RotationDegrees += currentArrowsSpeeds[i] * pDelta;
					if (arrows[i].RotationDegrees < arrowMinRotation)
					{
						arrows[i].RotationDegrees = arrowMinRotation;
						nextSpeedChange[i] = 0f;
					}
					if (arrows[i].RotationDegrees >= arrowMaxRotation)
					{
                        arrows[i].RotationDegrees = arrowMaxRotation;
                        nextSpeedChange[i] = 0f;
                    }
                }
			}
		}

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

			if (userName != "" && userGestion.RegisterUser(userName, password))
			{
				if (checkCreateStayLogged.ButtonPressed) userGestion.SaveLastUser(userName);
				else userGestion.SaveLastUser();
                CustomSignals.GetInstance().EmitSignal(CustomSignals.SignalName.GoToMainMenu);
                Hide();
			}
			else labelCreateErrorUsername.Show();
        }
		public void AnimationLoginEnter() {
            lightning.startPoint = lightningStartPos.GlobalPosition;
            lightning.endPoint = lightningEndPos.GlobalPosition;
            lightning.StartLightning();
			isAnimated = true;
        }
		private void AnimationLoginExit() {
			lightning.StopLightning();
			isAnimated = false;
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