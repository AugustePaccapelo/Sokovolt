using Com.IsartDigital.Sokovolt;
using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

// Author : Noe Sales

namespace Com.IsartDigital.SokoVolt {
	
	public partial class ScoreBoard : Node2D
	{
		[Export] private VBoxContainer nameContainer, scoreContainer, numberContainer, personalScoreContainer;
		[Export] private HBoxContainer globalScoreContainer;
		[Export] private Button personalScoreButton, globalScoreButton;
        [Export] private Label scoreLabel;

		private Dictionary scoreDico;
        private UserGestion userGestion;
        private string currentUser;
        private List<string> users = new List<string>();

		public override void _Ready()
		{
			userGestion = UserGestion.GetInstance();
            currentUser = userGestion.currentUser;
            scoreDico = userGestion.GetAllUserScore();
			foreach (string key in scoreDico.Keys)
			{
				users.Add(key);
			}
			UpdateGlobalScoreboard();

            personalScoreButton.Pressed += () =>
            {
                globalScoreContainer.Hide();
                personalScoreContainer.Show();
            };
            globalScoreButton.Pressed += () =>
            {
                personalScoreContainer.Hide();
                globalScoreContainer.Show();
            };
        }

        private void UpdateGlobalScoreboard()
        {
            string lUsername;
            int lScore;

            //We put the dictionary in a sortable list
            List<KeyValuePair<string, int>> lSortedScores = new List<KeyValuePair<string, int>>();

            foreach (string key in scoreDico.Keys)
            {
                lSortedScores.Add(new KeyValuePair<string, int>(key, (int)scoreDico[key]));
            }

            //Sort descending
            lSortedScores.Sort((a, b) => b.Value.CompareTo(a.Value));

            //We clean the containers
            foreach (var item in nameContainer.GetChildren()) item.QueueFree();
            foreach (var item in numberContainer.GetChildren()) item.QueueFree();
            foreach (var item in scoreContainer.GetChildren()) item.QueueFree();

            bool isUserInTop10 = false;
            int userRank = -1;

            for (int i = 0; i < lSortedScores.Count; i++)
            {
                if (lSortedScores[i].Key == currentUser)
                    userRank = i;

                //Show only the first 10
                if (i < 10)
                {
                    lUsername = lSortedScores[i].Key;
                    lScore = lSortedScores[i].Value;

                    if (lUsername == currentUser) isUserInTop10 = true;

                    //Rank
                    Label labelNumber = new Label();
                    switch (i)
                    {
                        case 0: labelNumber.Text = "🥇"; break;
                        case 1: labelNumber.Text = "🥈"; break;
                        case 2: labelNumber.Text = "🥉"; break;
                        default: labelNumber.Text = (i + 1).ToString() + "."; break;
                    }
                    numberContainer.AddChild(labelNumber);

                    //Name
                    Label labelName = new Label();
                    labelName.Text = lUsername;
                    nameContainer.AddChild(labelName);

                    //Score
                    Label labelScore = new Label();
                    labelScore.Text = lScore.ToString();
                    scoreContainer.AddChild(labelScore);
                }
            }

            //If the current user is not in the top 10, add it in the 11th line
            if (!isUserInTop10 && userRank != -1)
            {
                var userData = lSortedScores[userRank];

                //Real number (ex: 27)
                Label labelNumber = new Label();
                labelNumber.Text = (userRank + 1).ToString() + ".";
                numberContainer.AddChild(labelNumber);

                //Name
                Label labelName = new Label();
                labelName.Text = userData.Key;
                nameContainer.AddChild(labelName);

                //Score
                Label labelScore = new Label();
                labelScore.Text = userData.Value.ToString();
                scoreContainer.AddChild(labelScore);
            }
        }

        public void UpdatePersonalScoreBoard(int pLevel)
        {
            scoreLabel.Text = userGestion.GetScoreForLevel(pLevel).ToString();
        }


        protected override void Dispose(bool pDisposing)
		{

		}
	}
}
