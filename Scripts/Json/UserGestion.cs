using Godot;
using System;
using System.Security.Cryptography;
using System.Text;
using Com.IsartDigital.SokoVolt;
using System.Text.RegularExpressions;
using Godot.Collections;
using System.Collections.Generic;
using Com.IsartDigital.SokoVolt.Managers;

// Author : A. Dylan Montenegro Utrela

namespace Com.IsartDigital.Sokovolt
{
	public partial class UserGestion : Manager
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

		private const string JSON_FILE_PATH = "user://Json//UserData.Json";
        private const string LAST_USER_FILE_PATH = "user://Json/LastUser.json";
        private const string LOCAL_SCORE_PATH = "user://Json/LocalScore.json";
        public string currentUser {  get; private set; } = null;
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

            base._Ready();
        }

        public override void Init()
        {
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
			if (!FileAccess.FileExists(JSON_FILE_PATH)) return new Dictionary();
            string lContent = FileAccess.Open(JSON_FILE_PATH, FileAccess.ModeFlags.Read).GetAsText();
            return JsonTool.TryParseJson(lContent, out Dictionary pData) ? pData : new Dictionary();
        }

        private void SaveUserData(Dictionary data)
        {
            string lDirPath = JSON_FILE_PATH.GetBaseDir();

            if (!DirAccess.DirExistsAbsolute(lDirPath))
                DirAccess.MakeDirRecursiveAbsolute(lDirPath);

            using var lFile = FileAccess.Open(JSON_FILE_PATH, FileAccess.ModeFlags.Write);
            string lOutPut = Json.Stringify(data, "\t"); 
            lFile.StoreString(lOutPut);
        }

        public void SaveUserProgress(int pLevel, int pScore, int pStars)
        {
            Dictionary lUsersData = GetUserData();
            string lCurrentUser = currentUser;

            if (string.IsNullOrEmpty(lCurrentUser) || !lUsersData.ContainsKey(lCurrentUser)) return;
            Dictionary lUser = (Dictionary)lUsersData[lCurrentUser];  

            if (!lUser.ContainsKey("levels")) lUser["levels"] = new Dictionary();
            Dictionary lLevels = (Dictionary)lUser["levels"];
            string lLevelKey = $"level{pLevel}"; 
            int lSavedScore = 0;    
            int lSavedStars = 0;

            if (lLevels.ContainsKey(lLevelKey)) 
            {
                Dictionary lSavedLevel = (Dictionary) lLevels[lLevelKey];
                lSavedScore = lSavedLevel.ContainsKey("score") ? (int)lSavedLevel["score"] : 0;
                lSavedStars = lSavedLevel.ContainsKey("stars") ? (int)lSavedLevel["stars"] : 0;
            }

            int lBestScore = Math.Max(lSavedScore, pScore); 
            int lBestStars = Math.Max(lSavedStars, pStars);
            Dictionary lCurrentLevelData = lLevels.ContainsKey(lLevelKey) ? (Dictionary)lLevels[lLevelKey] : new Dictionary();

            lCurrentLevelData["score"] = lBestScore;
            lCurrentLevelData["stars"] = lBestStars;

            if (!lCurrentLevelData.ContainsKey("locked")) lCurrentLevelData["locked"] = true;   

            lLevels[lLevelKey] = lCurrentLevelData; 
            lUser["levels"] = lLevels;

            if (!lUser.ContainsKey("totalScore")) lUser["totalScore"] = 0; 

            int lNewScore = lBestScore - lSavedScore; 

            if (lNewScore > 0) lUser["totalScore"] = (int)lUser["totalScore"] + lNewScore;

            lUsersData[lCurrentUser] = lUser;
            SaveUserData(lUsersData);
            SaveToScoreLocally(lCurrentUser, (int)lUser["totalScore"]);
            customSignals.EmitSignal(CustomSignals.SignalName.LevelCompleted, pLevel, lBestStars, lBestScore, (int)lUser["totalScore"]);
            UnlockLevel(pLevel + 1);
        }

        public void UnlockLevel(int pLevel)
        {
            Dictionary lUserData = GetUserData();
            string lCurrentUser = currentUser;

            if (string.IsNullOrEmpty(lCurrentUser) || !lUserData.ContainsKey(lCurrentUser)) return;

            Dictionary lUser = (Dictionary)lUserData[lCurrentUser];

            if (!lUser.ContainsKey("levels")) return;

            Dictionary lLevels = (Dictionary)lUser["levels"];
            string lLevelKey = $"level{pLevel}"; 

            if (!lLevels.ContainsKey(lLevelKey)) return;

            Dictionary lLevelData = (Dictionary)lLevels[lLevelKey];
            lLevelData["locked"] = false;
            lLevels[lLevelKey] = lLevelData;
            lUser["levels"] = lLevels;
            lUserData[lCurrentUser] = lUser;
            SaveUserData(lUserData);
            customSignals.EmitSignal(CustomSignals.SignalName.LevelUnlock, pLevel);
        }

        public List<int> GetUnlockedLevels()
        {
            List<int> lUnlockedLevels = new List<int>();
            Dictionary lUserData = GetUserData();
            string lCurrentUser = currentUser;
            GD.Print("active user : " + lCurrentUser);

            if (string.IsNullOrEmpty(lCurrentUser) || !lUserData.ContainsKey(lCurrentUser)) return lUnlockedLevels;

            Dictionary lUser = (Dictionary)lUserData[lCurrentUser];
            GD.Print("Donnees utilisateur : " + Json.Stringify(lUser, "\t"));

            if (!lUser.ContainsKey("levels"))  return lUnlockedLevels;

            Dictionary lLevels = (Dictionary)lUser["levels"];

            foreach (string lLevelKey in lLevels.Keys)
            {
                Dictionary lLevelData = (Dictionary)lLevels[lLevelKey];

                if (lLevelData.ContainsKey("locked") && !(bool)lLevelData["locked"])
                {
                    if (int.TryParse(lLevelKey.Replace("level", ""), out int pLevelIndex))
                        lUnlockedLevels.Add(pLevelIndex);
                }
            }
            GD.Print("Level unlocked : " + string.Join(", ", lUnlockedLevels));
            return lUnlockedLevels;
        }

        public bool RegisterUser(string pName, string pPassword)
        {
            string lDirectoryPath = JSON_FILE_PATH.GetBaseDir();
            if (!DirAccess.DirExistsAbsolute(lDirectoryPath)) DirAccess.MakeDirRecursiveAbsolute(lDirectoryPath);

            Dictionary lUsersData = GetUserData(); 

            if (lUsersData.ContainsKey(pName)) return false;

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
            currentUser = pName;
            return true;
        }

        private Dictionary GenerateLevels()
        {
            Dictionary lLevels = new Dictionary();
            string lLevelPath = "res://Scripts/Json/Levels.json";

            if (!FileAccess.FileExists(lLevelPath)) return lLevels; 

            string lContent = FileAccess.Open(lLevelPath, FileAccess.ModeFlags.Read).GetAsText();

            if (!JsonTool.TryParseJson(lContent, out Dictionary pLevelDict)) return lLevels;
            if (!pLevelDict.ContainsKey("levelDesign")) return lLevels;

            Godot.Collections.Array lLevelList = (Godot.Collections.Array)pLevelDict["levelDesign"]; 

            for (int i = 0; i < lLevelList.Count; i++)
            {
                Dictionary lLevelData = (Dictionary)lLevelList[i]; 
                string lLevelKey = $"level{i}";
                bool lIsLocked = (i == 0) ? false : (bool)lLevelData.GetValueOrDefault("locked", true);  

                Dictionary lUserData = new Dictionary() 
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

        public bool LoginUser(string pName, string pPassword, bool isAlreadyLogged = false)
        {
            string lJsonContent = JsonTool.ReadFileContents(JSON_FILE_PATH);
            Dictionary lUsersData;

            if (string.IsNullOrEmpty(lJsonContent) || !JsonTool.TryParseJson(lJsonContent, out lUsersData)) return false;
            string lStoredPassword = GetUserPassword(pName);

            if(isAlreadyLogged || lStoredPassword == PasswordHashing(pPassword))
            {
                currentUser = pName;
            }
            return isAlreadyLogged || lStoredPassword == PasswordHashing(pPassword); 
        }

        public string GetLastUser()
        {
            if (!FileAccess.FileExists(LAST_USER_FILE_PATH)) return null;
            using var lFile = FileAccess.Open(LAST_USER_FILE_PATH, FileAccess.ModeFlags.Read); 
            string lUser = lFile.GetAsText();
            lFile.Close();
            return lUser;
        }

        public void SaveLastUser(string pName = null)
        {
            if (pName is null)
            {
                DirAccess.RemoveAbsolute(LAST_USER_FILE_PATH);
                return;
            }
            using var lFile = FileAccess.Open(LAST_USER_FILE_PATH, FileAccess.ModeFlags.Write);
            lFile.StoreString(pName);
            lFile.Close();
        }

        private void SaveToScoreLocally(string pUser, int pTotalScore)
        {
            Dictionary lScores = new Dictionary();

            if (FileAccess.FileExists(LOCAL_SCORE_PATH))
            {
                string lContent = FileAccess.Open(LOCAL_SCORE_PATH, FileAccess.ModeFlags.Read).GetAsText();
                JsonTool.TryParseJson(lContent, out lScores);   
            }
            if (!lScores.ContainsKey(pUser) || (int)lScores[pUser] < pTotalScore)
            {
                lScores[pUser] = pTotalScore;
                GD.Print("Updated score");
            }

            using var lFile = FileAccess.Open(LOCAL_SCORE_PATH, FileAccess.ModeFlags.Write);
            lFile.StoreString(Json.Stringify(lScores, "\t"));
        }

        public Dictionary GetAllUserScore()
        {
            if (!FileAccess.FileExists(LOCAL_SCORE_PATH)) return new Dictionary();
            string lContent = FileAccess.Open(LOCAL_SCORE_PATH, FileAccess.ModeFlags.Read).GetAsText();
            return JsonTool.TryParseJson(lContent, out Dictionary pScores) ? pScores : new Dictionary(); 
        }

    }
}