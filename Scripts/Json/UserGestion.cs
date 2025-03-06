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
            GD.Print(jsonFilePath);

            string testName = "test";
            string testPassword = "test123";

			bool registered = RegisterUser(testName, testPassword);
			GD.Print(registered ? "User 'Bruh' added." : "User 'Bruh' already exist!");
        }

        private class User
		{
			public string name { get; set; }
			public string password { get; set; }
		}

        private static string PasswordHashing(string pPassword) // to encrypt the password par Auguste
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

		public bool RegisterUser(string pName, string pPassword) // register new users 
		{
			string lDirectoryPath = jsonFilePath.GetBaseDir();

			if (!DirAccess.DirExistsAbsolute(lDirectoryPath)) DirAccess.MakeDirRecursiveAbsolute(lDirectoryPath);

			if (!FileAccess.FileExists(jsonFilePath))
			{
				using var lCreatFile = FileAccess.Open(jsonFilePath, FileAccess.ModeFlags.Write);
				lCreatFile.StoreString("{}");
				GD.Print("File created!!");
			}

			string lJsonContent = FileAccess.Open(jsonFilePath, FileAccess.ModeFlags.Read).GetAsText();
			Dictionary lUsersData;

			if (string.IsNullOrEmpty(lJsonContent) || !JsonTool.TryParseJson(lJsonContent, out lUsersData))
			{
				lUsersData = new Dictionary();
            }
			if (lUsersData.ContainsKey(pName)) return false;

			lUsersData[pName] = pPassword;
			string lNewJsonContent = Json.Stringify(lUsersData, "\t");
			using var lFile = FileAccess.Open(jsonFilePath, FileAccess.ModeFlags.Write);
			lFile.StoreString(lNewJsonContent);
			return true;
		}

		public bool LoginUser(string pName, string pPassword) // connects the users 
		{
			string lJsonContent = JsonTool.ReadFileContents(jsonFilePath);
            Dictionary lUsersData;

            if (string.IsNullOrEmpty(lJsonContent) || !JsonTool.TryParseJson(lJsonContent, out lUsersData)) return false;
			return lUsersData.ContainsKey(pName) && lUsersData[pName].ToString() == pPassword;
        }
	}
}