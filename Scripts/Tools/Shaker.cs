using Godot;
using System.Collections.Generic;
using static Com.IsartDigital.SokoVolt.Tools.ObjectProperties;

namespace Com.IsartDigital.SokoVolt
{

    public partial class Shaker : Node
	{
        [ExportGroup("Targets")]

        [Export] private Node[] _targets;
        
        [ExportGroup("General")]
        
        [Export] private float duration = 2f;
        [Export] private Vector2 amplitude = Vector2.One * 5f;
        [Export(PropertyHint.Range, "0,1,or_greater")] private float step = 0.048f;
        [Export(PropertyHint.Range,"0,30,radians_as_degrees")] private float noise = 15f;
        [Export] private bool inverseControlNodes = false;

        [ExportGroup("Attack")]
		
        [Export] private Tween.TransitionType transition_=Tween.TransitionType.Sine;
        [Export] private Tween.EaseType ease_ = Tween.EaseType.InOut;
        [Export] private float duration_ = 0.25f;
        
        [ExportGroup("Release")]
        
        [Export] private Tween.TransitionType _transition = Tween.TransitionType.Sine;
        [Export] private Tween.EaseType _ease=Tween.EaseType.InOut;
        [Export] private float _duration = 0.25f;

        private Vector2 current;
        private Vector2 next;
        private List<Node> targets;
        private List<Vector2> origins;
        
        private float amplitudeMax;
        private Vector2 currentAmplitude;

        private Tween shake;
        private Tween loop;
        private float intensity;

        public RandomNumberGenerator random = new RandomNumberGenerator();

        public override void _Ready()
		{
            random.Randomize();
        }

        public void Start() {

            Stop();
            // TODO: on pourrait éviter de refaire toute cette partie en utilisant des getter/setter

            amplitude = amplitude.Abs();
            amplitudeMax = Mathf.Max(amplitude.X, amplitude.Y);
            current = Vector2.FromAngle(Mathf.Pi * 2 * random.Randf()) * amplitudeMax;

            int lLength = _targets.Length;
            targets=new List<Node>();
            origins= new List<Vector2>();

            for (int i = 0; i < lLength; i++)
            {
                if (_targets[i] is Node2D || _targets[i] is Control) targets.Add(_targets[i]);
                else GD.Print(Name + ": " + _targets[i].Name + " n'est pas un Node2D ou un Control et sera ignoré.");
            }

            int lCount=targets.Count;

            if (lCount==0) {
                GD.Print("Aucune cible du Shake, Start ignoré.");
                return;
            }

            for (int i = 0;i<lCount;i++) origins.Add((Vector2)targets[i].Get(POSITION));

            intensity = 0f;

            shake = CreateTween();
            shake.TweenProperty(this, nameof(intensity), 1, duration_).SetTrans(transition_).SetEase(ease_);
            shake.TweenInterval(duration);
            shake.TweenProperty(this, nameof(intensity), 0, _duration).SetTrans(_transition).SetEase(_ease);
            shake.Finished += Stop;

            Loop();

        }

        public void Stop() {
            if (targets == null) return;
            int lLength = targets.Count;

            for (int i = 0; i < lLength; i++) targets[i].Set(POSITION, origins[i]);

            loop?.Kill();
            shake?.Kill();
            shake = null;
        }

        public bool isPlaying ()
        {
            return shake != null;
        }

        public void Loop() {

            next = - Vector2.FromAngle(current.Angle()+random.RandfRange(-noise,noise))*amplitudeMax;
            
            // correction de l'angle pour une amplitude non homogène
            if (amplitude.X < amplitudeMax && Mathf.Abs(next.X) > amplitude.X)
            {
                next.X = Mathf.Sign(next.X)*amplitude.X;
                next.Y = Mathf.Sign(next.Y)*Mathf.Sqrt(amplitudeMax* amplitudeMax - next.X * next.X);
            }
            else if (amplitude.Y < amplitudeMax && Mathf.Abs(next.Y)> amplitude.Y)
            {
                next.Y = Mathf.Sign(next.Y)*amplitude.Y;
                next.X = Mathf.Sign(next.X) * Mathf.Sqrt(amplitudeMax* amplitudeMax - next.Y * next.Y);
            }

            loop = CreateTween().SetParallel();
            int lCount = origins.Count;
            Vector2 lNext;
            for (int i = 0; i < lCount; i++)
            {
                lNext = targets[i] is Control && inverseControlNodes ? -next : next;
                loop.TweenProperty(targets[i], POSITION, origins[i] + lNext*intensity, step);
            }
            current = next;
            loop.Finished += Loop;
        }

	}



}
