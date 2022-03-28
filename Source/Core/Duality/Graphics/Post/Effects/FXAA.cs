using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Duality.Resources;

namespace Duality.Graphics.Post.Effects
{
	public class FXAA : BaseEffect
	{
		private DrawTechnique _shader;
		private FXAAShaderParams _shaderParams;

		public FXAA(BatchBuffer quadMesh)
			: base(quadMesh)
		{
		}

		internal override void LoadResources()
		{
			//_shader = resourceManager.Load<Duality.Resources.Shader>("shaders/post/fxaavert");
			_shader = new DrawTechnique(Shader.LoadEmbeddedShaderSource("shaders/post/fxaa.glsl"), "");
		}

		public void Render(RenderTarget input, RenderTarget output)
		{
			if (_shaderParams == null)
			{
				_shaderParams = new FXAAShaderParams();
				_shader.BindUniformLocations(_shaderParams);
			}

			var screenSize = new Vector2(input.Textures[0].Width, input.Textures[0].Height);

			DualityApp.GraphicsBackend.BeginPass(output, new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
			DualityApp.GraphicsBackend.BeginInstance(_shader.Handle, new int[] { input.Textures[0].Handle },
				samplers: new int[] { DualityApp.GraphicsBackend.DefaultSamplerNoFiltering });
			DualityApp.GraphicsBackend.BindShaderVariable(_shaderParams.SamplerScene, 0);
			DualityApp.GraphicsBackend.BindShaderVariable(_shaderParams.TextureSize, ref screenSize);

			DualityApp.GraphicsBackend.DrawMesh(_quadMesh.MeshHandle);
			DualityApp.GraphicsBackend.EndPass();
		}

		class FXAAShaderParams
		{
			public int SamplerScene = 0;
			public int TextureSize = 0;
		}
	}
}
