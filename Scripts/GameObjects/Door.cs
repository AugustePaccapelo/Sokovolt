using Com.IsartDigital.SokoVolt.GameObjects.Movables;
using Com.IsartDigital.SokoVolt.Managers;
using Godot;
using System;
using System.Runtime.CompilerServices;

// Author : Auguste Paccapelo

namespace Com.IsartDigital.SokoVolt.GameObjects
{
	public partial class Door : GameObject
    {
		// ---------- VARIABLES ---------- \\

		#region // ----- Singleton ----- \\

		static private Door instance;

		static public Door GetInstance()
		{
			if (instance == null) instance = new Door();
			return instance;
		}

		#endregion

		// ----- Paths ----- \\

		// ----- Nodes ----- \\
		private CustomSignals signals;

		[Export] private Node2D closedVisual;
        [Export] private Node2D openedVisual;
        [Export] private GpuParticles2D openedVisualParticles;
		private float globalDelta = 0;

        // ----- Others ----- \\
		public bool isOpen { get; private set; }

        // ---------- FONCTIONS ---------- \\

        // ----- Constructor & Ready & Process ----- \\

		public override void _Ready()
		{
			#region // ----- Singleton ----- \\

			if (instance != null)
			{
				GD.Print(Name + " Instance already exist, destroying the last added.");
				QueueFree();
				return;
			}

			instance = this;

			#endregion

			base._Ready();
			
			GameManager.GetInstance().door = this;
		}

		public override void _Process(double pDelta)
		{
			float lDelta = (float)pDelta;
			globalDelta = lDelta;
			base._Process(lDelta);
		}

		// ----- My Functions ----- \\

		public void Open()
		{
			isOpen = true;
			openedVisual.Show();
			closedVisual.Hide();
			Camera2D lCamera = GameManager.GetInstance().camera;
			AnimationManager.GetInstance().CameraZoomTraveling(lCamera, 0.2f, 0.1f, Position, lCamera.Position, 1.5f);
			
        }

        public void Close()
		{
			isOpen = false;
            openedVisual.Hide();
            closedVisual.Show();
        }

        // ----- Destructor ----- \\

        protected override void Dispose(bool pDisposing)
		{
			#region // ----- Singleton ----- \\

			if (pDisposing && instance == this) instance = null;

			#endregion

			GameManager.GetInstance().door = null;

			base.Dispose(pDisposing);
		}
	}
}
