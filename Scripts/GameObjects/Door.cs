using System.Runtime.Serialization.Formatters;
using Com.IsartDigital.SokoVolt.Managers;
using Godot;

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

		// ----- Nodes ----- \\
		private CustomSignals signals;

		[Export] private Node2D visual;
		[Export] private Node2D closedVisual;
        [Export] private Node2D openedVisual;
        [Export] private GpuParticles2D openedVisualParticles;
		[Export] private AnimatedSprite2D openCloseAnimation;

        // ----- Others ----- \\
		public bool isOpen { get; private set; }

        // ---------- FONCTIONS ---------- \\

        // ----- Ready & Process ----- \\

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
            openCloseAnimation.Frame = 34;
            visual.ZIndex -= 1;
		}
		public override void _Process(double pDelta)
		{
			float lDelta = (float)pDelta;
			base._Process(lDelta);
        }

		// ----- My Functions ----- \\

		public void Open()
		{
			isOpen = true;
			openedVisual.Visible = true; 
			openCloseAnimation.PlayBackwards();
			
			Camera2D lCamera = GameManager.GetInstance().camera;
			AnimationManager.GetInstance().CameraZoomTraveling(lCamera, 0.5f, 0.8f, Position, GameManager.GetInstance().cameraDefaultPos, 1.5f);
        }

        public void Close()
		{
			if(isOpen) openCloseAnimation.Play();
			openedVisual.Visible = false;
			isOpen = false;
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