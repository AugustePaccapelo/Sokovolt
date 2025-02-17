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
		private Button levelButton;
		private Button nextButton;
		private Vector2 screenSize = new Vector2();
		private int levelNumb = 0;
		private int levelNumbMax = 10;
		private const string LEVEL_PREFIXE = "Level : ";
		private const float MARGIN = 50.0f;
		private Vector2 buttonSize = new Vector2(60, 40);
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

            levelButton = CreateButton(new Vector2(screenSize.X + buttonSize.X, screenSize.Y / 2));
            Tween lTweenNextButton = CreateTween();
            lTweenNextButton.TweenProperty(levelButton, "position", new Vector2(screenSize.X / 2, screenSize.Y / 2), 1).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Elastic);
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
            lTween.TweenProperty(levelButton, "position", new Vector2(pDirection * (-screenSize.X - levelButton.Size.X), 0), 0.5f).AsRelative();

            Vector2 newButtonPosition = (pDirection == -1)
                ? new Vector2(-50 - buttonSize.X, screenSize.Y / 2) //if it's left button the nextButton will spawn on the left of the screen
                : new Vector2(screenSize.X + buttonSize.X, screenSize.Y / 2);// if it's right button the next button will spawn on the right 

            nextButton = CreateButton(newButtonPosition); //Give position to the Create fonction

            Tween lTweenNextButton = CreateTween();
            lTweenNextButton.TweenProperty(nextButton, "position", new Vector2(screenSize.X / 2, screenSize.Y / 2), 1)
                            .SetEase(Tween.EaseType.InOut)
                            .SetTrans(Tween.TransitionType.Elastic); //Entrance animation for the button

            lTweenNextButton.Finished += () =>
            {
                levelButton.Disabled = false;
                alreadyPress = false;
            };

            lTween.Finished += () => {
                levelButton.QueueFree();
                levelButton = nextButton;
                nextButton = new Button();
            };
        }

        private Button CreateButton(Vector2 pPos)
		{
			Button lButton = new Button();
			lButton.Disabled = true;
			AddChild(lButton);
            lButton.Size = buttonSize;
            lButton.PivotOffset = buttonSize/2;
			lButton.Position = pPos;
			lButton.Text = LEVEL_PREFIXE + levelNumb;
			lButton.Pressed += () => EmitSignal(nameof(ButtonLoadLevel), levelNumb); //Connect Button to a signal for launch the good level

            return lButton;
		}

        protected override void Dispose(bool pDisposing)
		{
			instance = null;
			base.Dispose(pDisposing);
		}
	}
}
