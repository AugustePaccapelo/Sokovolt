using Godot;
using System;
using System.Security.Cryptography;
using System.Text;
using Com.IsartDigital.SokoVolt;
using System.Text.RegularExpressions;
using Godot.Collections;

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
        }

        private static string PasswordHashing(string pPassword) // to encrypt the password by Auguste
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

		private Dictionary GetUserData()
		{
			if (!FileAccess.FileExists(jsonFilePath)) return new Dictionary();
            string content = FileAccess.Open(jsonFilePath, FileAccess.ModeFlags.Read).GetAsText();
            return JsonTool.TryParseJson(content, out Dictionary data) ? data : new Dictionary();
        }

        private void SaveUserData(Dictionary data)
        {
            using var file = FileAccess.Open(jsonFilePath, FileAccess.ModeFlags.Write);
            file.StoreString(Json.Stringify(data, "\t"));
        }

        public void SaveUserProgress(string pName, int totalScore, Dictionary levelData)
        {
            Dictionary lUsersData = GetUserData();
            if (!lUsersData.ContainsKey(pName)) return;

            var user = (Dictionary)lUsersData[pName];
            user["totalScore"] = totalScore;
            user["levels"] = levelData;
            SaveUserData(lUsersData);
        }

        public bool RegisterUser(string pName, string pPassword)
        {
            string lDirectoryPath = jsonFilePath.GetBaseDir();
            if (!DirAccess.DirExistsAbsolute(lDirectoryPath)) DirAccess.MakeDirRecursiveAbsolute(lDirectoryPath);

            Dictionary lUsersData = GetUserData();

            if (lUsersData.ContainsKey(pName)) return false;

            string lHashedPassword = PasswordHashing(pPassword);
            lUsersData[pName] = new Dictionary
            {
                { "password", lHashedPassword },
                { "totalScore", 0 },
                { "levels", new Dictionary() }
            };
            
            SaveUserData(lUsersData);
            return true;
        }

        private string GetUserPassword(string pName)
        {
            Dictionary lUsersData = GetUserData();
            if (lUsersData.ContainsKey(pName))
            {
                Dictionary lUserDict;
                JsonTool.TryParseJson(lUsersData[pName].ToString(), out lUserDict);
                return lUserDict.ContainsKey("password") ? lUserDict["password"].ToString() : null;
            }
            return null;
        }

        public bool LoginUser(string pName, string pPassword, bool isAlreadyLogged = false) // connects the users 
        {
            string lJsonContent = JsonTool.ReadFileContents(jsonFilePath);
            Dictionary lUsersData;

            if (string.IsNullOrEmpty(lJsonContent) || !JsonTool.TryParseJson(lJsonContent, out lUsersData)) return false;
            string lStoredPassword = GetUserPassword(pName);
            return isAlreadyLogged || lStoredPassword == PasswordHashing(pPassword);
        }

        public string GetLastUser()
        {
            if (!FileAccess.FileExists(lastUserFilePath)) return null;
            using var lFile = FileAccess.Open(lastUserFilePath, FileAccess.ModeFlags.Read);
            string lUser = lFile.GetAsText();
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
    }
}