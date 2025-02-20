using Com.IsartDigital.SokoVolt.Managers;
using Godot;
using System;
using System.Collections.Generic;

// Author : Noé Sales

namespace Com.IsartDigital.SokoVolt
{

	public partial class LevelSelector : Control
	{
		[Export] private Button buttonRight;
		[Export] private Button buttonLeft;
        [Export] private CompressedTexture2D texture;
        [Export] private PackedScene teslaScene;
        private Sprite2D tesla;
        private Sprite2D nextTesla;
		private Button levelButton;
		private Button nextButton;
		private Vector2 screenSize = new Vector2();
		private int levelNumb = 0;
		private int levelNumbMax = 5;
		private const string LEVEL_PREFIXE = "Level : ";
		private const float MARGIN = 50.0f;
		private Vector2 buttonSize = new Vector2(60, 40);
		private Vector2 teslaSize = new Vector2(855, 1071);
        private bool alreadyPress = false;

        [Signal] public delegate void ButtonLoadLevelEventHandler(int pLevel);

        #region Singleton
        static private LevelSelector instance;

		private LevelSelector() { }

		static public LevelSelector GetInstance()
		{
			if (instance == null) instance = new LevelSelector();
			return instance;

		}
		#endregion

		public override void _Ready()
		{
			#region Singleton Ready
			if (instance != null)
			{
				QueueFree();
				GD.Print(nameof(LevelSelector) + " Instance already exist, destroying the last added.");
				return;
			}

			instance = this;
            #endregion

            screenSize = GetViewportRect().Size;

            #region FirstButtonInit
            ButtonLoadLevel += LevelManager.GetInstance().LevelLoader;

            tesla = CreateTesla(new Vector2(screenSize.X + teslaSize.X, 310));
            //levelButton = CreateButton(new Vector2(screenSize.X + buttonSize.X, screenSize.Y / 2));
            Tween lTweenNextButton = CreateTween();
            lTweenNextButton.TweenProperty(tesla, "position", new Vector2(577, 310), 0.5f).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Elastic);
			lTweenNextButton.Finished += () => levelButton.Disabled = false;
            #endregion

            #region ButtonParam

            buttonRight.Pressed += () => SwitchLevel(buttonRight);
            buttonLeft.Pressed += () => SwitchLevel(buttonLeft);

            buttonLeft.GlobalPosition = new Vector2(0 + MARGIN, screenSize.Y / 2);
            buttonRight.GlobalPosition = new Vector2(screenSize.X - MARGIN - buttonSize.X, screenSize.Y / 2);
            #endregion
        }

        private void SwitchLevel(Button pButton)
        {
            if (!alreadyPress) //If selection button are unPress, continue fonction
            {
                alreadyPress = true;

                AnimateButton(pButton); //Play the animation

                if ((pButton.Name == buttonLeft.Name && levelNumb > 0) ||
                    (pButton.Name == buttonRight.Name && levelNumb < levelNumbMax))
                {
                    int lDirection = (pButton.Name == buttonLeft.Name) ? -1 : 1; //return -1 if it's leftButton and return 1 if it's rightButton
                    levelNumb += lDirection;

                    MoveLevelButton(lDirection);
                }
                else
                {
                    alreadyPress = false;
                }
            }
        }

        private void AnimateButton(Button pButton)
        {
            Tween lTweenButton = CreateTween();
            lTweenButton.TweenProperty(pButton, "scale", new Vector2(0.6f, 0.6f), 0.2f);
            lTweenButton.TweenProperty(pButton, "scale", new Vector2(1f, 1f), 0.2f)
                        .SetEase(Tween.EaseType.Out)
                        .SetTrans(Tween.TransitionType.Elastic);
            lTweenButton.Finished += lTweenButton.Kill;
        }

        private void MoveLevelButton(int pDirection)
        {
            Tween lTween = CreateTween();
            lTween.TweenProperty(tesla, "position", new Vector2(pDirection * (-screenSize.X - teslaSize.X), 0), 0.3f).AsRelative();

            Vector2 newTeslaPosition = (pDirection == -1)
                ? new Vector2(-50 - teslaSize.X, 310) //if it's left button the nextButton will spawn on the left of the screen
                : new Vector2(screenSize.X + teslaSize.X, 310);// if it's right button the next button will spawn on the right 

            nextTesla = CreateTesla(newTeslaPosition);

            Tween lTweenNextButton = CreateTween();
            lTweenNextButton.TweenProperty(nextTesla, "position", new Vector2(screenSize.X / 2, 310), 0.5f)
                            .SetEase(Tween.EaseType.InOut)
                            .SetTrans(Tween.TransitionType.Elastic); //Entrance animation for the button

            lTweenNextButton.Finished += () =>
            {
                levelButton.Disabled = false;
                alreadyPress = false;
            };

            lTween.Finished += () => {
                tesla.QueueFree();
                tesla = nextTesla;
                nextTesla = new Sprite2D();
            };
        }

        private Button GetTeslaButton(Sprite2D pTesla)
        {
            Button lButton = pTesla.GetNode<Button>("Button");
            lButton.Pressed += () => EmitSignal(nameof(ButtonLoadLevel), levelNumb); //Connect Button to a signal for launch the good level
            return lButton;
        }
        private Sprite2D CreateTesla(Vector2 pPos)
        {
            Sprite2D lTesla = new Sprite2D();
            lTesla = teslaScene.Instantiate() as Sprite2D;
            AddChild(lTesla);
            lTesla.Position = pPos;
            levelButton = GetTeslaButton(lTesla);
            Label lLabel = lTesla.GetNode<Label>("Label");
            lLabel.Text = LEVEL_PREFIXE + levelNumb;

            return lTesla;
        }

        protected override void Dispose(bool pDisposing)
		{
			instance = null;
			base.Dispose(pDisposing);
		}
	}
}
