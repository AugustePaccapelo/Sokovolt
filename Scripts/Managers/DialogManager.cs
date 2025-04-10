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
						"Utilise les flèches pour te déplacer.",
						"Pousse les caisses Tesla pour activer les circuits."
					};

				case 2:
					return new List<string>
					{
						"Rappelle-toi, tu peux appuyer sur R pour recommencer !"
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
