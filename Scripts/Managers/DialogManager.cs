using Com.IsartDigital.SokoVolt.Tools;
using Godot;
using System;
using System.Collections.Generic;
using System.Data;

//Author : Ferlat Thibaud 
namespace Com.IsartDigital.SokoVolt{
	
	public partial class DialogManager : Node
	{
		#region GetInstance
		static private DialogManager instance;
		
		static public DialogManager GetInstance () {
			if (instance == null) instance = new DialogManager();
			return instance;
		}

		private DialogManager ():base() {}
		#endregion

		private PackedScene dialogBoxScene; 
		public DialogBox dialogBox;
		public override void _Ready()
		{
			#region instance
		if (instance != null){  
			QueueFree();
			GD.Print(nameof(DialogManager) + " Instance already exist, destroying the last added.");
			return;
		}
		instance = this;
		#endregion
			GetDialogBox();
		}	

		public override void _Process(double pDelta)
		{

		}

		private async void GetDialogBox()
		{
			await ToSignal(GetTree().CreateTimer(0.1f), ObjectProperties.TIME_OUT);
			dialogBox = HUD.GetInstance().dialogBox;
		}

		public async void TriggerDialogueForLevel(int pLevel)
		{
			await ToSignal(CustomSignals.GetInstance(), CustomSignals.SignalName.DisplayDialog);
			await ToSignal(GetTree().CreateTimer(1f), ObjectProperties.TIME_OUT);
			List<string> lDialogues = GetDialoguesForLevel(pLevel + 1 );

			if (lDialogues != null && lDialogues.Count > 0)
			{
				dialogBox.ShowDialogues(lDialogues);
			}
			else
			{
				GD.PrintErr("Aucun dialogue trouvé pour le niveau " + pLevel);
			}
		}

		private List<string> GetDialoguesForLevel(int pLevel)
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



				// Ajoute ici d'autres niveaux

				default:
					return new List<string>(); // Aucun dialogue pour ce niveau
			}
		}

		#region dispose
		protected override void Dispose(bool pDisposing)
		{
			if (pDisposing && instance == this) instance = null;
			base.Dispose(pDisposing);
		}
		#endregion
	}
}
