using Godot;
using System;
using System.Security.Cryptography;
using System.Text;
using Com.IsartDigital.SokoVolt;
using System.Text.RegularExpressions;
using Godot.Collections;
using System.Collections.Generic;

// Author : A. Dylan Montenegro Utrela

namespace Com.IsartDigital.ProjectName
{
	public partial class UserGestion : Node2D
	{
		#region Singleton
		static private UserGestion instance;

		private UserGestion() { }

		static public UserGestion GetInstance()
		{
			if (instance == null) instance = new UserGestion();
			return instance;
		}
        #endregion

		private string jsonFilePath = ProjectSettings.GlobalizePath("user://Json//UserData.Json");
        private string lastUserFilePath = ProjectSettings.GlobalizePath("user://Json/LastUser.json");
        private string localScorePath = ProjectSettings.GlobalizePath("user://Json/LocalScore.json");

        CustomSignals customSignals;

        public override void _Ready()
		{
			#region Singleton
			if (instance != null)
			{
				QueueFree();
				GD.Print(nameof(UserGestion) + " INSTANCE ALREADY EXISTS, DESTROYING THE LAST ADDED");
				return;
			}

			instance = this;
			#endregion

			LoginScreen.GetInstance().userGestion = this;
            LoginScreen.GetInstance().skipLogin = !(GetLastUser() is null);
            customSignals = CustomSignals.GetInstance();
        }

        private static string PasswordHashing(string pPassword)
        {
			SHA256 lSha256 = SHA256.Create();

			byte[] lBytes = lSha256.ComputeHash(Encoding.UTF8.GetBytes(pPassword));

			string lHashedPassword = "";
			foreach (byte lByte in lBytes)
			{
				lHashedPassword += (lByte.ToString("x2"));
			}

			return lHashedPassword;
        }

		public Dictionary GetUserData()
		{
			if (!FileAccess.FileExists(jsonFilePath)) return new Dictionary();
            string lContent = FileAccess.Open(jsonFilePath, FileAccess.ModeFlags.Read).GetAsText();
            return JsonTool.TryParseJson(lContent, out Dictionary pData) ? pData : new Dictionary();
        }

        private void SaveUserData(Dictionary data)
        {
            string lDirPath = jsonFilePath.GetBaseDir();

            if (!DirAccess.DirExistsAbsolute(lDirPath))
                DirAccess.MakeDirRecursiveAbsolute(lDirPath);

            using var lFile = FileAccess.Open(jsonFilePath, FileAccess.ModeFlags.Write);
            string lOutPut = Json.Stringify(data, "\t"); // converts the dictionary into a JSON string
            lFile.StoreString(lOutPut);
        }

        public void SaveUserProgress(int pLevel, int pScore, int pStars)
        {
            Dictionary lUsersData = GetUserData();
            string lCurrentUser = GetLastUser();

            if (string.IsNullOrEmpty(lCurrentUser) || !lUsersData.ContainsKey(lCurrentUser)) return;
            Dictionary lUser = (Dictionary)lUsersData[lCurrentUser];  // get the current user's dictionay

            if (!lUser.ContainsKey("levels")) lUser["levels"] = new Dictionary();
            Dictionary lLevels = (Dictionary)lUser["levels"];
            string lLevelKey = $"level{pLevel}"; // define's the current level key ex: level0, level1... etc
            int lSavedScore = 0;    
            int lSavedStars = 0;

            if (lLevels.ContainsKey(lLevelKey)) // if level was already played, get the existing score/stars
            {
                Dictionary lSavedLevel = (Dictionary) lLevels[lLevelKey];
                lSavedScore = lSavedLevel.ContainsKey("score") ? (int)lSavedLevel["score"] : 0;
                lSavedStars = lSavedLevel.ContainsKey("stars") ? (int)lSavedLevel["stars"] : 0;
            }

            int lBestScore = Math.Max(lSavedScore, pScore); // saves the best score
            int lBestStars = Math.Max(lSavedStars, pStars); // and best stars only
            Dictionary lCurrentLevelData = new Dictionary() // create's a dictionary to store current level's final data
            {
                { "score", lBestScore },
                { "stars", lBestStars },
            };

            lLevels[lLevelKey] = lCurrentLevelData; // save the level data back into the user's levels
            lUser["levels"] = lLevels;

            if (!lUser.ContainsKey("totalScore")) lUser["totalScore"] = 0; 

            int lNewScore = lBestScore - lSavedScore; 

            if (lNewScore > 0) lUser["totalScore"] = (int)lUser["totalScore"] + lNewScore; // only add to totalScore if the new score is higher than the previous

            lUsersData[lCurrentUser] = lUser;
            SaveUserData(lUsersData);
            customSignals.EmitSignal(CustomSignals.SignalName.LevelCompleted, pLevel, lBestStars, lBestScore, (int)lUser["totalScore"]);
            SaveToScoreLocally(lCurrentUser, (int)lUser["totalscore"]);
        }

        public void UnlockLevel(int pLevel)
        {
            Dictionary lUserData = GetUserData();
            string lCurrentUser = GetLastUser();

            if (string.IsNullOrEmpty(lCurrentUser) || !lUserData.ContainsKey(lCurrentUser)) return;

            Dictionary lUser = (Dictionary)lUserData[lCurrentUser];

            if (!lUser.ContainsKey("levels")) return;

            Dictionary lLevels = (Dictionary)lUser["levels"];
            string lLevelKey = $"level{pLevel}"; // gets the key for the level to unlock

            if (!lLevels.ContainsKey(lLevelKey)) return;

            Dictionary lLevelData = (Dictionary)lLevels[lLevelKey];
            lLevels["locked"] = false;
            lLevels[lLevelKey] = lLevelData;
            lUser["levels"] = lLevels;
            lUserData[lCurrentUser] = lUser;
            SaveUserData(lUserData);
            customSignals.EmitSignal(CustomSignals.SignalName.LevelUnlock, pLevel);
        }

        public bool RegisterUser(string pName, string pPassword)
        {
            string lDirectoryPath = jsonFilePath.GetBaseDir();
            if (!DirAccess.DirExistsAbsolute(lDirectoryPath)) DirAccess.MakeDirRecursiveAbsolute(lDirectoryPath); // if it dont exist it creates it

            Dictionary lUsersData = GetUserData(); // loads the exisitng user data 

            if (lUsersData.ContainsKey(pName)) return false; // if name exist it returns false

            string lPassword = PasswordHashing(pPassword);
            Dictionary lLevels = GenerateLevels(); 
            lUsersData[pName] = new Dictionary() 
            {
                
                { "password", lPassword },
                { "totalScore", 0},
                { "levels", lLevels}
                
            };
            SaveUserData(lUsersData);
            SaveToScoreLocally(pName, 0); 
            return true;
        }

        private Dictionary GenerateLevels()
        {
            Dictionary lLevels = new Dictionary();
            string lLevelPath = "res://Scripts/Json/Levels.json";

            if (!FileAccess.FileExists(lLevelPath)) return lLevels; // return an empty dictionary

            string lContent = FileAccess.Open(lLevelPath, FileAccess.ModeFlags.Read).GetAsText();

            if (!JsonTool.TryParseJson(lContent, out Dictionary pLevelDict)) return lLevels;
            if (!pLevelDict.ContainsKey("levelDesign")) return lLevels;

            Godot.Collections.Array lLevelList = (Godot.Collections.Array)pLevelDict["levelDesign"]; // gets the array of all level definitions

            for (int i = 0; i < lLevelList.Count; i++) // loop through each level deifinition
            {
                Dictionary lLevelData = (Dictionary)lLevelList[i]; // cast the current element to a dictionary
                string lLevelKey = $"level{i}"; // create a level key like level0, level1 ..etc
                bool lIsLocked = (bool)lLevelData.GetValueOrDefault("locked", true);  // gets the "locked" status form the level definitionn, default to true

                Dictionary lUserData = new Dictionary()  // builds a new dictionary for current user's version of the level
                {
                    { "score", 0 },
                    { "stars", 0 },
                    { "locked", lIsLocked }
                };
                lLevels[lLevelKey] = lUserData; 
            }
            return lLevels;
        }

        private string GetUserPassword(string pName)
        {
            Dictionary lUsersData = GetUserData();
            if (lUsersData.ContainsKey(pName)) 
            {
                var lUserDict = (Dictionary)lUsersData[pName];
                return lUserDict.ContainsKey("password") ? lUserDict["password"].ToString() : null;
            }
            return null;
        }

        public bool LoginUser(string pName, string pPassword, bool isAlreadyLogged = false) // connects the users 
        {
            string lJsonContent = JsonTool.ReadFileContents(jsonFilePath);
            Dictionary lUsersData;

            if (string.IsNullOrEmpty(lJsonContent) || !JsonTool.TryParseJson(lJsonContent, out lUsersData)) return false; // tries to parse the content into usable dictionary, returns false if no content 
            string lStoredPassword = GetUserPassword(pName);
            return isAlreadyLogged || lStoredPassword == PasswordHashing(pPassword); // if already logged in accept it, if not compare password to check if correct
        }

        public string GetLastUser()
        {
            if (!FileAccess.FileExists(lastUserFilePath)) return null;
            using var lFile = FileAccess.Open(lastUserFilePath, FileAccess.ModeFlags.Read); // open the file in read mode
            string lUser = lFile.GetAsText(); // read the stored username from the file
            lFile.Close();
            return lUser;
        }

        public void SaveLastUser(string pName = null)
        {
            if (pName is null)
            {
                DirAccess.RemoveAbsolute(lastUserFilePath);
                return;
            }
            using var lFile = FileAccess.Open(lastUserFilePath, FileAccess.ModeFlags.Write);
            lFile.StoreString(pName);
            lFile.Close();
        }

        private void SaveToScoreLocally(string pUser, int pTotalScore)
        {
            Dictionary lScores = new Dictionary();

            if (FileAccess.FileExists(localScorePath))
            {
                string lContent = FileAccess.Open(localScorePath, FileAccess.ModeFlags.Read).GetAsText();
                JsonTool.TryParseJson(lContent, out lScores);   
            }
            if (!lScores.ContainsKey(pUser) || (int)lScores[pUser] < pTotalScore) // saves only if the user is new or the new score is better than the previous one
            {
                lScores[pUser] = pTotalScore;
                GD.Print("Updated score");
            }

            using var lFile = FileAccess.Open(localScorePath, FileAccess.ModeFlags.Write);
            lFile.StoreString(Json.Stringify(lScores, "\t"));
        }

        public Dictionary GetAllUserScore()
        {
            if (!FileAccess.FileExists(localScorePath)) return new Dictionary();
            string lContent = FileAccess.Open(localScorePath, FileAccess.ModeFlags.Read).GetAsText();
            return JsonTool.TryParseJson(lContent, out Dictionary pScores) ? pScores : new Dictionary(); 
        }
    }
}