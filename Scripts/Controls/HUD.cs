using Godot;
using System;

//Author : Ferlat Thibaud 
namespace Com.IsartDigital.ProjectName{
	
	public partial class HUD : Control
	{
		#region GetInstance
		static private HUD instance;
		
		static public HUD GetInstance () {
			if (instance == null) instance = new HUD();
			return instance;
		}

		private HUD ():base() {}
		#endregion

		[Export] private Button undoButton, redoButton, mainMenuButton;
		public override void _Ready()
		{
			#region instance
		if (instance != null){  
			QueueFree();
			GD.Print(nameof(HUD) + " Instance already exist, destroying the last added.");
			return;
		}
		instance = this;
		#endregion
			Init(); 
		}

		private void Init()
		{
			CustomMinimumSize = GetViewportRect().Size;	
		}

		public override void _Process(double pDelta)
		{

		}

		#region dispose
		protected override void Dispose(bool pDisposing)
		{
			if (pDisposing && instance == this) instance = null;
			base.Dispose(pDisposing);
		}
		#endregion
	}
}
