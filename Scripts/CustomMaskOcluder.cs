using Godot;
using Godot.Collections;
using System;

namespace Com.IsartDigital.SokoVolt.GameObjects 
{
    public partial class CustomMaskOcluder : Node2D
    {
		static public CustomMaskOcluder instance; 
        [Export] private Polygon2D _maskPolygon;
        private ViewportTexture _maskTexture;
        private Vector2 _lastViewportSize;

		private Dictionary<CanvasItem, Material> _originalMaterials = new();


        public override void _Ready()
        {
			instance = this; 
            InitializeMaskSystem();
			// ApplyOcclusionTo(GetChild(0) as AnimatedSprite2D); 
        }

        private void InitializeMaskSystem()
        {
            // Setup mask viewport
            var viewport = new SubViewport
            {
                Size = (Vector2I)GetViewportRect().Size,
                TransparentBg = true,
                RenderTargetUpdateMode = SubViewport.UpdateMode.WhenVisible,
                HandleInputLocally = false
            };
            AddChild(viewport);

            var polyCopy = _maskPolygon.Duplicate() as Polygon2D;
            viewport.AddChild(polyCopy);
            _maskTexture = viewport.GetTexture();

            // Configure main mask
            _maskPolygon.Material = new ShaderMaterial
            {
                Shader = new Shader
                {
                    Code = @"shader_type canvas_item;
                            render_mode unshaded;
                            void fragment() { COLOR = vec4(0.0); }"
                }
            };
            _maskPolygon.ZIndex = 1000;

            _lastViewportSize = GetViewportRect().Size;
        }

    	public void ApplyOcclusionTo(Node node)
		{
			if (_maskTexture == null)
			{
				GD.PrintErr("Mask texture is null, skipping masking.");
				return;
			}

			if (node is CanvasItem canvasItem)
			{
				// Si le canvasItem n'a pas de matériel, on le sauve avec "null"
				if (!_originalMaterials.ContainsKey(canvasItem))
					_originalMaterials[canvasItem] = canvasItem.Material;

				// On applique le ShaderMaterial temporaire
				var mat = new ShaderMaterial
				{
					Shader = new Shader
					{
						Code = @"shader_type canvas_item;

						uniform sampler2D mask_texture;
						uniform vec2 mask_position;
						uniform vec2 viewport_size;

						void fragment() {
							vec2 mask_uv = SCREEN_UV - (mask_position / viewport_size);

							if (texture(mask_texture, mask_uv).a > 0.0)
								discard;

							// Préserve la couleur d’origine du node (sprite, poly, etc.)
							// Ne remplace pas COLOR sauf si nécessaire
						}
						",
					}
				};

				mat.SetShaderParameter("mask_texture", _maskTexture);
				mat.SetShaderParameter("mask_position", _maskPolygon.GlobalPosition);
				mat.SetShaderParameter("viewport_size", GetViewportRect().Size);

				canvasItem.Material = mat;
			}

			else
			{
				GD.Print($"Node {node.Name} is not a CanvasItem, skipping.");
			}
		}

		public Dictionary<CanvasItem, Material> GetOriginalMaterials()
		{
			return _originalMaterials;
		}


        public override void _Process(double delta)
        {
            var currentSize = GetViewportRect().Size;
            if (currentSize != _lastViewportSize)
            {
                _lastViewportSize = currentSize;
                foreach (Node child in GetChildren())
                {
                    if (child is CanvasItem { Material: ShaderMaterial mat } && 
                        mat.Shader != null && 
                        mat.Shader.Code.Contains("mask_position"))
                    {
                        mat.SetShaderParameter("viewport_size", currentSize);
                    }
                }
            }
        }
    }
}