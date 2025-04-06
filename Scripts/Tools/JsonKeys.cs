using Godot;
using System;

// Author : Ferlat Thibaud 

namespace Com.IsartDigital.SokoVolt {
	
	public class JsonKeys 
	{

		// ----- Json
		public const string LEVEL_DESIGN_KEY = "levelDesign"; 
		public const string MAP_KEY = "map"; 
		public const string BOX_RANGE_KEY = "boxRange";

		public const string PAR_KEY = "par";
		public const string AUTHOR_KEY = "Author";
		public const string LEVELS_JSONS_PATH = "res://Scripts/Json/Levels.json";

		// ----- Map
		public const char PLAYER = '@'; 
		public const char BOX = '$';
		public const char WALL = '#';
		public const char ELECTRIC_WALL = '*';
		public const char GOAL_BULB = '.';
		public const char GENERATOR = '/';
		public const char DOOR = '|';


	}
}
