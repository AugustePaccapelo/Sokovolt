using Com.IsartDigital.SokoVolt.Tools;
using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Com.IsartDigital.SokoVolt
{
	public partial class DialogBox : Node2D
	{
		[Export] private MarginContainer dialogBox; 
		[Export] private AnimatedSprite2D dialoger; 
		[Export] public RichTextLabel text;
		[Export] public Button nextButton;

		private List<string> dialogues = new();
		private string currentLine = "";
		private int dialogueIndex = 0;
		private bool isTyping = false;
		private Vector2 lOriginalScale;
		private int visibleTargetLength = 0;
		
		private CancellationTokenSource typingTokenSource;





		public override void _Ready()
		{
			nextButton.Pressed += OnNextPressed;
			CustomSignals.GetInstance().GoToMainMenu += OnGoToMainMenu;
			CustomSignals.GetInstance().GameFinished += (_, _, _) => OnLevelCompleted();
			Hide();
			lOriginalScale = dialogBox.Scale;
		}

		public void ShowDialogues(List<string> pLines)
		{
			dialogues = pLines;
			dialogueIndex = 0;
			Show();
			AnimateIn();
		}

		private void AnimateIn()
		{
		
			text.Text = "";
			text.VisibleCharacters = 0;

			Tween lTween = CreateTween();

			dialogBox.Modulate = new Color(1, 1, 1, 0);
			dialogBox.Scale = lOriginalScale * 0.5f;

			lTween.TweenProperty(dialogBox, "modulate:a", 1.0f, 0.2f);
			lTween.TweenProperty(dialogBox, ObjectProperties.SCALE, lOriginalScale, 0.2f);
			lTween.TweenCallback(Callable.From(StartTyping));
		}


		private void AnimateOut(bool pEndOfDialogue = false)
		{
			Tween lTween = CreateTween();

			Vector2 lTargetScale = lOriginalScale * 0.5f;

			lTween.TweenProperty(dialogBox, "modulate:a", 0.0f, 0.2f);
			lTween.TweenProperty(dialogBox, ObjectProperties.SCALE, lTargetScale, 0.2f);

			if (pEndOfDialogue)
			{
				lTween.TweenCallback(Callable.From(Hide));
			}
			else
			{
				lTween.TweenCallback(Callable.From(AnimateIn));
			}
		}


		private async void StartTyping()
		{

			isTyping = true;
			currentLine = dialogues[dialogueIndex];
			text.Text = currentLine;
			text.VisibleCharacters = 0;

			DialogManager.GetInstance().OnDialogueLineDisplayed(dialogueIndex);

			typingTokenSource = new CancellationTokenSource();
			CancellationToken token = typingTokenSource.Token;

			try
			{
				for (int i = 0; i <= currentLine.Length; i++)
				{
					text.VisibleCharacters = i;
					await ToSignal(GetTree().CreateTimer(0.02f), ObjectProperties.TIME_OUT);

					if (token.IsCancellationRequested)
						break;
				}
			}
			catch (TaskCanceledException)
			{
				// Optionnel : log
				GD.Print("Typing cancelled");
			}

			text.VisibleCharacters = currentLine.Length;
			isTyping = false;
			
		}
		
		public void SetDialogerFrame(int frameIndex)
		{
			if (dialoger != null)
				dialoger.Frame = frameIndex;
		}





		private void OnNextPressed()
		{
			
			if (isTyping)
			{
				typingTokenSource?.Cancel(); 
				isTyping = false;
				return;
			}

			dialogueIndex++;

			if (dialogueIndex < dialogues.Count)
			{
				AnimateOut();
			}
			else
			{
				AnimateOut(true);
			}
		}




		public void Reset(bool pAnimated = false)
		{
			isTyping = false;
			dialogues.Clear();
			dialogueIndex = 0;
			text.VisibleCharacters = 0;
			text.Text = "";

			if (pAnimated)
			{
				AnimateOut(true);
			}
			else
			{
				Hide();
				dialogBox.Scale = lOriginalScale;
				dialogBox.Modulate = new Color(1, 1, 1, 1);
			}
		}

		private void OnGoToMainMenu()
		{
			Reset(true); 
		}

		private void OnLevelCompleted()
		{
			Reset(false); 
		}

	}

	
}
