using Com.IsartDigital.SokoVolt.Tools;
using Godot;
using System;
using System.Collections.Generic;

namespace Com.IsartDigital.SokoVolt {

	public partial class DialogManager : Node {

		#region GetInstance
		private static DialogManager instance;
		public static DialogManager GetInstance () {
			if (instance == null) instance = new DialogManager();
			return instance;
		}
		private DialogManager(): base() {}
		#endregion

		private PackedScene dialogBoxScene;
		public DialogBox dialogBox;

		private int currentLevel = -1;
		private Dictionary<(int, int), Action> dialogueTriggers = new();

		public override void _Ready() {
			if (instance != null) {
				QueueFree();
				GD.Print(nameof(DialogManager) + " instance already exists, destroying the last added.");
				return;
			}
			instance = this;
			GetDialogBox();
			InitDialogueTriggers(); // initialise les événements
		}

		private async void GetDialogBox() {
			await ToSignal(GetTree().CreateTimer(0.1f), ObjectProperties.TIME_OUT);
			dialogBox = HUD.GetInstance().dialogBox;
		}

		public async void TriggerDialogueForLevel(int pLevel) {
			await ToSignal(CustomSignals.GetInstance(), CustomSignals.SignalName.DisplayDialog);
			await ToSignal(GetTree().CreateTimer(1f), ObjectProperties.TIME_OUT);

			currentLevel = pLevel; // stocke le niveau actuel
			List<string> lDialogues = GetDialoguesForLevel(pLevel + 1);

			if (lDialogues != null && lDialogues.Count > 0) {
				dialogBox.ShowDialogues(lDialogues);
			} else {
				GD.PrintErr("Aucun dialogue trouvé pour le niveau " + pLevel);
			}
		}

		public void OnDialogueLineDisplayed(int pLineIndex) {
			HighlightManager.GetInstance().ClearHighlights(); // 🔥 reset avant trigger

			var key = (currentLevel + 1, pLineIndex);

			if (dialogueTriggers.ContainsKey(key))
				dialogueTriggers[key].Invoke();
			
		}



		private void InitDialogueTriggers() {
			// Niveau 1
			dialogueTriggers[(1, 2)] = () => HighlightManager.GetInstance().Highlight("Generator");
			dialogueTriggers[(1, 4)] = () => HighlightManager.GetInstance().Highlight("GoalBulb");
			dialogueTriggers[(1, 6)] = () => HighlightManager.GetInstance().Highlight("BoxTesla");
			dialogueTriggers[(1, 12)] = () => HighlightManager.GetInstance().Highlight("Door");

			// Niveau 2
			dialogueTriggers[(2, 2)] = () => HighlightManager.GetInstance().Highlight("ElectricWall");
			
			
		}

		private List<string> GetDialoguesForLevel(int pLevel)
		{
			if (!AudioSettings.frEN) //FR
			{
				switch (pLevel)
				{
					case 1:
						return new List<string>
						{
							"Bienvenue dans SokoVolt !",
							"Je suis l’architecte de cette vieille usine électrique.",
							"Tu vois ce gros bidule bleu avec des antennes ?",
							"C’est notre générateur. Il produit de l’énergie.",
							"Regarde cette ampoule... elle est bien triste sans lumière.",
							"Ton objectif : lui transmettre l’énergie du générateur.",
							"Comment ? Grâce à ces boîtes avec des antennes : les Teslas.",
							"Approche-les les unes des autres pour faire circuler le courant.",
							"Utilise les flèches pour te déplacer et pousser les Teslas.",
							"Petit conseil : la dernière Tesla doit être collée à l’ampoule.",
							"Tu remarqueras que certains éléments du système fonctionnent encore.",
							"Inspire-toi d’eux pour comprendre comment tout ça marche.",
							"Ho et n'oublie pas de sortir par la porte une fois ouverte !",
							"Bonne chance... et ne me déçois pas !"
						};
					case 2:
						return new List<string>
						{
							"Hmmm... encore un souci dans le bloc 2 de mon usine ?",
							"Cette zone est un peu spéciale, elle est sécurisée.",
							"Tu remarqueras ces murs électriques :",
							"Ils bloquent le passage, mais pas le courant.",
							"Tu peux connecter des Teslas à travers eux.",
							"Essaie donc de rétablir l’énergie dans cette zone !"
						};
					case 3:
						return new List<string>
						{
							"On dirait qu’on va devoir travailler ensemble un bon moment...",
							"Un déséquilibre atomique a mis tous mes blocs HS.",
							"Regarde : la porte et l’ampoule sont derrière ce mur.",
							"Mais toi, tu as une capacité spéciale : tu peux voyager dans le courant électrique !",
							"Essaie de connecter la première Tesla, puis traverse l’arc électrique.",
							"Tu verras, c’est plutôt marrant.",
							"Ah, et à propos : les Teslas peuvent se connecter à distance...",
							"...mais seulement si elles sont **dans leur portée**.",
							"Regarde bien les chiffres ou les lignes d’aperçu qui apparaissent : c’est leur **range électrique**.",
							"Plus la distance est grande, plus c’est difficile de relier.",
							"Joue bien avec ça si tu veux activer toute la zone !"
						};
				}
			}
			else //EN
			{
				switch (pLevel)
				{
					case 1:
						return new List<string>
						{
							"Welcome to SokoVolt!",
							"I'm the architect of this old electric factory.",
							"You see that big blue thing with antennas?",
							"That's our generator. It produces energy.",
							"Look at that lightbulb... pretty sad without light, right?",
							"Your goal: transfer the energy from the generator to it.",
							"How? With those antenna boxes: the Teslas.",
							"Push them close together to make the current flow.",
							"Use the arrow keys to move and push the Teslas.",
							"Pro tip: the last Tesla must touch the bulb.",
							"You'll see that some parts of the system still work.",
							"Use that to understand how it all connects.",
							"Oh, and don't forget to exit through the door when it's open!",
							"Good luck... and don't disappoint me!"
						};
					case 2:
						return new List<string>
						{
							"Hmmm... more trouble in block 2 of my factory?",
							"This area's a bit special—it's secure.",
							"See those electric walls?",
							"They block your way, but not the current.",
							"You can connect Teslas through them.",
							"Try restoring energy in this zone!"
						};
					case 3:
						return new List<string>
						{
							"Looks like we'll be working together for a while...",
							"An atomic imbalance shut down all the blocks.",
							"See? The door and bulb are behind that wall.",
							"But you have a special ability: you can travel through electricity!",
							"Try connecting the first Tesla, then cross the electric arc.",
							"You'll see—it's fun.",
							"Oh, and by the way: Teslas can connect from a distance...",
							"...but only if they're **within range**.",
							"Watch the numbers or preview lines that appear—that's their **electric range**.",
							"The bigger the gap, the harder it is to link them.",
							"Use that wisely if you want to power the whole zone!"
						};
				}
			}

			// Par défaut
			return new List<string>();
		}




		protected override void Dispose(bool pDisposing) {
			if (pDisposing && instance == this) instance = null;
			base.Dispose(pDisposing);
		}
	}
}
