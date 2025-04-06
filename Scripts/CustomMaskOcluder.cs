using Godot;
using Godot.Collections;
using System;
using System.Runtime.CompilerServices;

namespace Com.IsartDigital.SokoVolt.GameObjects 
{
    public partial class CustomMaskOcluder : Node2D
    {
		static public CustomMaskOcluder instance; 
        [Export] private Polygon2D maskPolygon;
        [Export] private AnimatedSprite2D backGround; 
        private ViewportTexture maskTexture;
        private Vector2 lastViewportSize;
        private const int MASK_Z_INDEX = 100;

		private Dictionary<CanvasItem, Material> originalMaterials = new Dictionary<CanvasItem, Material>();

		//Shader Properties
		private const string MASK_POSITION = "mask_position"; 
		private const string MASK_TEXTURE = "mask_texture"; 
		private const string VIEWPORT_SIZE = "viewport_size"; 


        public override void _Ready()
        {
			instance = this; 
            InitializeMaskSystem();
            CustomSignals.GetInstance().GoToMainMenu += () => SetBackgroundVisibility(false);
            CustomSignals.GetInstance().LoadLevel += (int pLevel) => SetBackgroundVisibility(true);

        }

      

        public override void _Process(double delta)
        {
            var currentSize = GetViewportRect().Size;
            if (currentSize != lastViewportSize)
            {
                lastViewportSize = currentSize;
                foreach (Node lChild in GetChildren())
                {
                    if (lChild is CanvasItem { Material: ShaderMaterial mat } && 
                        mat.Shader != null && 
                        mat.Shader.Code.Contains(MASK_POSITION))
                    {
                        mat.SetShaderParameter(VIEWPORT_SIZE, currentSize);
                    }
                }
            }
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

            var polyCopy = maskPolygon.Duplicate() as Polygon2D;
            viewport.AddChild(polyCopy);
            maskTexture = viewport.GetTexture();

            // Configure main mask
            maskPolygon.Material = new ShaderMaterial
            {
                Shader = new Shader
                {
                    Code = @"shader_type canvas_item;
                            render_mode unshaded;
                            void fragment() { COLOR = vec4(0.0); }"
                }
            };
            maskPolygon.ZIndex = MASK_Z_INDEX;

            lastViewportSize = GetViewportRect().Size;
        }

    	public void ApplyOcclusionTo(Node pNode)
		{
			if (maskTexture == null)
				return;
			

			if (pNode is CanvasItem canvasItem)
			{
				// Canvas no Material => Set null
				if (!originalMaterials.ContainsKey(canvasItem))
					originalMaterials[canvasItem] = canvasItem.Material;

				// Temporary Shader
				ShaderMaterial lMat = new ShaderMaterial
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

				lMat.SetShaderParameter(MASK_TEXTURE, maskTexture);
				lMat.SetShaderParameter(MASK_POSITION, maskPolygon.GlobalPosition);
				lMat.SetShaderParameter(VIEWPORT_SIZE, GetViewportRect().Size);

				canvasItem.Material = lMat;
			}
		}

		public Dictionary<CanvasItem, Material> GetOriginalMaterials()
		{
			return originalMaterials;
		}

        private void SetBackgroundVisibility(bool pVisible)
        {
            backGround.Visible = pVisible;
        }

    }
}