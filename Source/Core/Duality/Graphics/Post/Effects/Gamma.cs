using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Duality.Resources;

namespace Duality.Graphics.Post.Effects
{
	public class Gamma : BaseEffect
	{
		private DrawTechnique _shader;
		private GammaShaderParams _shaderParams;

        public bool EnableColorCorrection { get; set; } = false;

        public Gamma(BatchBuffer quadMesh)
			: base(quadMesh)
		{
		}

		internal override void LoadResources()
		{
			//_shader = resourceManager.Load<Duality.Resources.Shader>("/shaders/post/gamma");
			_shader = new DrawTechnique(Shader.LoadEmbeddedShaderSource("shaders/post/gamma.glsl"), "");
		}

		public void Render(RenderTarget input, RenderTarget output)
		{
			if (_shaderParams == null)
			{
				_shaderParams = new GammaShaderParams();
				_shader.BindUniformLocations(_shaderParams);
			}

			DualityApp.GraphicsBackend.BeginPass(output, new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
			DualityApp.GraphicsBackend.BeginInstance(_shader.Handle, new int[] { input.Textures[0].Handle, Texture.ColorCorrectLUT.Res.Handle },
				samplers: new int[] { DualityApp.GraphicsBackend.DefaultSamplerNoFiltering, DualityApp.GraphicsBackend.DefaultSamplerNoFiltering });
			DualityApp.GraphicsBackend.BindShaderVariable(_shaderParams.SamplerScene, 0);
			DualityApp.GraphicsBackend.BindShaderVariable(_shaderParams.SamplerColorCorrect, 1);
			DualityApp.GraphicsBackend.BindShaderVariable(_shaderParams.EnableColorCorrection, EnableColorCorrection ? 1 : 0);

			DualityApp.GraphicsBackend.DrawMesh(_quadMesh.MeshHandle);
			DualityApp.GraphicsBackend.EndPass();
		}

		class GammaShaderParams
		{
			public int SamplerScene = 0;
            public int SamplerColorCorrect = 0;
            public int EnableColorCorrection = 0;
        }
	}
}
