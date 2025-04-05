using Godot;
using System;
using System.Data;
using System.Transactions;

//Author : Ferlat Thibaud 
namespace Com.IsartDigital.SokoVolt.Tools {
	public static class ObjectProperties 
	{	
		public const string POSITION = "position"; 
		public const string POSITION_X = "position:x"; 
		public const string POSITION_Y = "position:y"; 
		public const string GLOBAL_POSITION = "global_position"; 
		public const string MODULATE = "modulate"; 
		public const string SCALE = "scale"; 
		public const string ROTATION = "rotation"; 
		public const string ZOOM = "zoom"; 

		public const string TIME_OUT = "timeout"; 
		public const string FINISHED  = "finished"; 
	}
}
