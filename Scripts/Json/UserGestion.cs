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

		private string jsonFilePath = ProjectSettings.GlobalizePath("user://"); // GlobalizePath gives the correct path for each operating system, "user://" is the static path for where the game/games data is saved
		private string json;

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
		}

		private class User
		{
			public string name { get; set; }
			public string password { get; set; }
		}

		private static string PasswordHashing(string pPassword) // to encrypt the password ==== pour Imperator Augustus aka Gaius Julius Caesar Octavianus le GOAT
		{
			using (SHA256 pSha256 = SHA256.Create()) ; // to use
			return pPassword;
		}

		private void SaveTextToFile() // to finish
		{

		}

		private void SaveUsers(List<User> pUsers) // this saves users in a list in a json file
		{
			json = JsonSerializer.Serialize(pUsers, new JsonSerializerOptions { WriteIndented = true });
			File.WriteAllText(jsonFilePath, json);
		}

		private List<User> GetUsers() // this fetches registered users by reading the json file
		{
			if (!File.Exists(jsonFilePath)) return new List<User>();

			json = File.ReadAllText(jsonFilePath);
			return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
		}

		public bool RegistedUser(string pName, string pPassword) // register new users 
		{
			List<User> pUsers = GetUsers();
			if (pUsers.Exists(u => u.name == pName))
			{
				GD.Print("Username already taken!");
				return false;
			}

			pUsers.Add(new User { name = pName, password = PasswordHashing(pPassword) });
			SaveUsers(pUsers);
			GD.Print("User registered");
			return true;
		}

		public bool LoginUser(string pName, string pPassword) // connects the users 
		{
			List<User> pUsers = GetUsers();
			string pHashedPwd = PasswordHashing(pPassword);
			if (pUsers.Exists(u => u.name == pName && u.password == pPassword))
			{
				GD.Print("Login successful");
				return true;
			}
			GD.Print("The name or password is incorrect");
			return false;
		}
	}
}