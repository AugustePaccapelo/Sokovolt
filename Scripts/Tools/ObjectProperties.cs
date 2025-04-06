using Godot;
using System;
using System.Data;
using System.Transactions;

//Author : Ferlat Thibaud 
namespace Com.IsartDigital.SokoVolt.Tools {
	public static class ObjectProperties 
	{	
		public const string POSITION = "position"; 
		public const string POSITION_Y = "position:y";
		public const string GLOBALPOSITION = "global_position"; 
		public const string MODULATE = "modulate"; 
		public const string SCALE = "scale"; 
		public const string ROTATION = "rotation"; 
		public const string ZOOM = "zoom";
		public const string ENERGY = "energy";

        public const string LEVELS = "levels";
        public const string PASSWORD = "password";
        public const string LEVELDESIGN = "levelDesign";
        public const string UPDATESCORE = "Update score";
        public const string SCORE = "score";
        public const string STARS = "stars";
        public const string LOCKED = "locked";
        public const string TOTALSCORE = "totalScore";
        public const string LOCKALL = "lockall";

        public const string TIME_OUT = "timeout"; 
		public const string FINISHED = "finished"; 
	}
}
