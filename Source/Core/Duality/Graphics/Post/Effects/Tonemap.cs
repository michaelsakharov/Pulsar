using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Duality.Renderer;
using Duality.Resources;

namespace Duality.Graphics.Post.Effects
{
	public class Tonemap : BaseEffect
	{
		private DrawTechnique _shader;
		private TonemapShaderParams _shaderParams;

		private readonly int[] _textures = new int[4];
		private readonly int[] _samplers;

		public Tonemap(BatchBuffer quadMesh)
			: base(quadMesh)
		{
			int blurSampler = DualityApp.GraphicsBackend.RenderSystem.CreateSampler(new Dictionary<SamplerParameterName, int>
			{
				{ SamplerParameterName.TextureMinFilter, (int)TextureMinFilter.Linear },
				{ SamplerParameterName.TextureMagFilter, (int)TextureMagFilter.Linear },
				{ SamplerParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge },
				{ SamplerParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge }
			});

			_samplers = new int[] { DualityApp.GraphicsBackend.DefaultSamplerNoFiltering, DualityApp.GraphicsBackend.DefaultSampler, blurSampler, blurSampler };
		}

		internal override void LoadResources()
		{
			//_shader = resourceManager.Load<Duality.Resources.Shader>("/shaders/post/tonemap");
			_shader = new DrawTechnique(Shader.LoadEmbeddedShaderSource("shaders/post/tonemap.glsl"), "");
		}

		public void Render(HDRSettings settings, RenderTarget input, RenderTarget output, RenderTarget bloom, RenderTarget lensFlares, RenderTarget luminance)
		{
			if (_shaderParams == null)
			{
				_shaderParams = new TonemapShaderParams();
				_shader.BindUniformLocations(_shaderParams);
			}

			DualityApp.GraphicsBackend.BeginPass(output, new Vector4(0.0f, 0.0f, 0.0f, 1.0f));

			_textures[0] = input.Textures[0].Handle;
			_textures[1] = luminance.Textures[0].Handle;

			var activeTexture = 2;
			if (settings.EnableBloom)
				_textures[activeTexture++] = bloom.Textures[0].Handle;

			DualityApp.GraphicsBackend.BeginInstance(_shader.Handle, _textures, samplers: _samplers);
			DualityApp.GraphicsBackend.BindShaderVariable(_shaderParams.SamplerScene, 0);
			DualityApp.GraphicsBackend.BindShaderVariable(_shaderParams.SamplerBloom, 2);
			DualityApp.GraphicsBackend.BindShaderVariable(_shaderParams.SamplerLensFlares, 3);
			DualityApp.GraphicsBackend.BindShaderVariable(_shaderParams.SamplerLuminance, 1);
			DualityApp.GraphicsBackend.BindShaderVariable(_shaderParams.KeyValue, settings.KeyValue);
            DualityApp.GraphicsBackend.BindShaderVariable(_shaderParams.AutoKey, settings.AutoKey ? 1 : 0);
            DualityApp.GraphicsBackend.BindShaderVariable(_shaderParams.EnableBloom, settings.EnableBloom ? 1 : 0);
			DualityApp.GraphicsBackend.BindShaderVariable(_shaderParams.BloomStrength, settings.BloomStrength);
			DualityApp.GraphicsBackend.BindShaderVariable(_shaderParams.TonemapOperator, (int)settings.TonemapOperator);

			DualityApp.GraphicsBackend.DrawMesh(_quadMesh.MeshHandle);
			DualityApp.GraphicsBackend.EndPass();
		}

		class TonemapShaderParams
		{
			public int SamplerScene = 0;
			public int SamplerBloom = 0;
			public int SamplerLensFlares = 0;
			public int SamplerLuminance = 0;
			public int KeyValue = 0;
			public int AutoKey = 0;
			public int EnableBloom = 0;
            public int BloomStrength = 0;
            public int EnableLensFlares = 0;
			public int TonemapOperator = 0;
		}
	}
}
