using Godot;
using System;

// Author : Ferlat Thibaud 

namespace Com.IsartDigital.ProjectName {
	
	public partial class Cell : Node2D
	{
		public int x, y; 

		private GameObject content; 

		public override void _Ready()
		{

		}

		public override void _Process(double pDelta)
		{
			float lDelta = (float)pDelta;

		}

		public void SetContent(GameObject pNewContent)
		{
			content = pNewContent;
		}

		public GameObject GetContent()
		{
			return content;
		}	

		protected override void Dispose(bool pDisposing)
		{

		}
	}
}
