using Godot;
using System;

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
			levelButton = CreateButton(new Vector2(screenSize.X + buttonSize.X, screenSize.Y / 2));

			buttonRight.Pressed += () => SwitchLevel(buttonRight);
			buttonLeft.Pressed += () => SwitchLevel(buttonLeft);


            buttonLeft.Position += new Vector2(MARGIN, 0);
			buttonRight.Position += new Vector2(-MARGIN, 0);
        }

		private void SwitchLevel(Button pButton)
		{
			if (!alreadyPress)
			{
                alreadyPress = true;
                Tween lTweenButton = CreateTween();
                lTweenButton.TweenProperty(pButton, "scale", new Vector2(0.6f, 0.6f), 0.2f);
                lTweenButton.TweenProperty(pButton, "scale", new Vector2(1f, 1f), 0.2f);
                lTweenButton.Finished += lTweenButton.Kill;

                Tween lTween = CreateTween();

                if (pButton.Name == buttonLeft.Name && levelNumb > 0)
				{
					levelNumb--;
					lTween.TweenProperty(levelButton, "position", new Vector2(screenSize.X + levelButton.Size.X, 0), 1).AsRelative();
                    nextButton = CreateButton(new Vector2(0 - buttonSize.X, screenSize.Y / 2));
                }
                else if (pButton.Name == buttonRight.Name && levelNumb < levelNumbMax)
				{
					levelNumb++;
                    lTween.TweenProperty(levelButton, "position", new Vector2(-screenSize.X - levelButton.Size.X, 0), 1).AsRelative();
                    nextButton = CreateButton(new Vector2(screenSize.X + buttonSize.X, screenSize.Y / 2));
                }
				else
				{
					alreadyPress = false;
					lTween.Kill();
				}

                lTween.Finished += () => {
                    levelButton.QueueFree();
                    levelButton = nextButton;
                    nextButton = new Button();
                };
                
            }
        }

        private Button CreateButton(Vector2 pPos)
		{
			Button lButton = new Button();
			AddChild(lButton);
            lButton.Size = buttonSize;
            lButton.PivotOffset = buttonSize/2;
			lButton.Position = pPos;
			lButton.Text = LEVEL_PREFIXE + levelNumb;
            Tween lTween = CreateTween();
            lTween.TweenProperty(lButton, "position", new Vector2(screenSize.X / 2, screenSize.Y / 2), 1);
			lTween.Finished += () => alreadyPress = false;

            return lButton;
		}

		protected override void Dispose(bool pDisposing)
		{
			instance = null;
			base.Dispose(pDisposing);
		}
	}
}
