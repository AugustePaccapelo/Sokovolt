using Com.IsartDigital.SokoVolt;
using Godot;
using System;

//author : Noe Sales

namespace Com.IsartDigital.ProjectName
{
	public partial class TitleCard : Control
	{
		#region Singleton
		static private TitleCard instance;
		private TitleCard() { }

		static public TitleCard GetInstance()
		{
			if(instance == null) instance = new TitleCard();
			return instance;
		}

		#endregion

		[Export] TextureRect logoISART;
		[Export] TextureRect logoGame;

		private Color finalColor = new Color(1, 1, 1, 1);
		private Color startColor = new Color(1, 1, 1, 0);

		public override void _Ready()
		{
			#region Singelton
			if (instance != null)
			{
				QueueFree();
				GD.Print(nameof(TitleCard) + "Instance already exist, destroying the last added");
				return;
			}

			instance = this;
			#endregion
			logoISART.Modulate = startColor;
			Tween lTween = CreateTween();
			lTween.TweenProperty(logoISART, "modulate", finalColor, 0.7f);
			lTween.TweenProperty(logoISART, "modulate", startColor, 0.7f);
            lTween.TweenProperty(logoGame, "modulate", finalColor, 0.7f);
            lTween.TweenProperty(logoGame, "modulate", startColor, 0.7f);
            lTween.Finished += AnimationFinished;
		}

        public override void _Process(double delta)
        {
            if(Input.IsMouseButtonPressed(MouseButton.Left)) QueueFree();
        }

		public void AnimationFinished()
		{
			//LoginScreen.GetInstance().AnimationLoginEnter();
			QueueFree();
		}

        protected override void Dispose(bool pDisposing)
		{
			instance = null;
			base.Dispose(pDisposing);
		}

	}
}
