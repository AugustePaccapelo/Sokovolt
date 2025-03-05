using Godot;
using System;
using System.IO;
using System.Reflection.Metadata;
using System.Xml.Linq;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Com.IsartDigital.SokoVolt;
using System.Runtime.Intrinsics.Arm;

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
        private Godot.Collections.Dictionary<string, Variant> usersData = new Godot.Collections.Dictionary<string, Variant>();

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
			LoadUsers();

			string testName = "Test";
            string testPassword = "Test123";

			bool registered = RegisterUser(testName, testPassword);
			GD.Print(registered ? "User 'Test' added." : "User 'Test' already exist!");

			bool loginSuccess = LoginUser(testName, testPassword);
			GD.Print(loginSuccess ? "Login successful for 'Test'" : "Login failed for 'Test'");
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

        private void SaveUsers() // this saves users in a list in a json file
		{
			string lJson = Json.Stringify(usersData);
			JsonTool.WriteToFile(jsonFilePath, lJson); // use JsonTool instead of File.WriteAllText
		}

		//private List<User> GetUsers() // this fetches registered users by reading the json file
		//{
		//	if (!File.Exists(jsonFilePath))
		//	{
		//		GD.Print("Creating new users file");
		//		//JsonTool.WriteToFile(jsonFilePath); // ??
		//	}

  //          string lJson = JsonTool.ReadFileContents(jsonFilePath);
		//	//json = File.ReadAllText(jsonFilePath);
		//	if (string.IsNullOrEmpty(jsonFilePath)) return new List<User>();
  //          return JsonSerializer.Deserialize<List<User>>(lJson) ?? new List<User>();
		//}

		private void LoadUsers() // this fetches registered users by reading the json file
        {
			if (!File.Exists(jsonFilePath))
			{
				GD.Print("Creating new user file");
				SaveUsers();
			}
			else
			{
				string lJson = JsonTool.ReadFileContents(jsonFilePath);
				var lParsed = Json.ParseString(lJson);
				if (lParsed.VariantType == Variant.Type.Dictionary) usersData = lParsed.As<Godot.Collections.Dictionary<string, Variant>>();
			}
		}

		public bool RegisterUser(string pName, string pPassword) // register new users 
		{
			if (usersData.ContainsKey(pName))
			{
				GD.Print("Username already taken!");
				return false;
			}

			usersData[pName] = PasswordHashing(pPassword);
			SaveUsers();
			GD.Print("User registered");
			return true;	
		}

		public bool LoginUser(string pName, string pPassword) // connects the users 
		{
			string pHashedPwd = PasswordHashing(pPassword);
			if (usersData.TryGetValue(pName, out Variant pSavedPwd) && pSavedPwd.AsString() == pHashedPwd)
			{
				GD.Print("Login successful");
				return true;
			}
			GD.Print("The name or password is incorrect");
			return false;
		}
	}
}