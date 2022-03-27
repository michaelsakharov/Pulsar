using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Duality.Renderer;
using Duality.Renderer.RenderTargets;
using Duality.Resources;

namespace Duality.Graphics.Post.Effects
{
	public class Bloom : BaseEffect
	{
		private DrawTechnique _highPassShader;
		private DrawTechnique _blurCombineShader;
		private DrawTechnique _blurHorizontalShader;
		private DrawTechnique _blurVerticalShader;
		private DrawTechnique _quadShader;

		private HighPassShaderParams _highPassParams;
		private QuadShaderParams _quadParams;
		private BlurShaderParams _blurHorizontalParams;
		private BlurShaderParams _blurVerticalParams;
		private BlurCombineShaderParams _blurCombineParams;

		private readonly RenderTarget[][] _blurTargets;

		private readonly int _blurSampler;

		public Bloom(Backend backend, BatchBuffer quadMesh)
			: base(backend, quadMesh)
		{
			// Setup rendertargets
			var width = backend.Width;
			var height = backend.Height;

			_blurTargets = new RenderTarget[5][];
			for (var i = 0; i < _blurTargets.Length; i++)
			{
				_blurTargets[i] = new RenderTarget[2];
			}

			var n = 1;
			for (var i = 0; i < _blurTargets.Length; i++)
			{
				for (var j = 0; j < _blurTargets[i].Length; j++)
				{
					var w = width / n;
					var h = height / n;

					_blurTargets[i][j] = _backend.CreateRenderTarget("bloom_blur_" + n + "_" + j, new Definition(w, h, false, new List<Definition.Attachment>()
					{
						new Definition.Attachment(Definition.AttachmentPoint.Color, Renderer.PixelFormat.Rgba, Renderer.PixelInternalFormat.Rgba16f, Renderer.PixelType.Float, 0),
					}));
				}

				n *= 2;
			}

			_blurSampler = _backend.RenderSystem.CreateSampler(new Dictionary<SamplerParameterName, int>
			{
				{ SamplerParameterName.TextureMinFilter, (int)TextureMinFilter.Linear },
				{ SamplerParameterName.TextureMagFilter, (int)TextureMagFilter.Linear },
				{ SamplerParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge },
				{ SamplerParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge }
			});
		}

		internal override void LoadResources()
		{
			//_highPassShader = resourceManager.Load<Duality.Resources.Shader>("/shaders/post/highpass");
			//_blurHorizontalShader = resourceManager.Load<Duality.Resources.Shader>("/shaders/post/blur", "BLUR_HORIZONTAL");
			//_blurVerticalShader = resourceManager.Load<Duality.Resources.Shader>("/shaders/post/blur", "BLUR_VERTICAL");
			//_quadShader = resourceManager.Load<Duality.Resources.Shader>("/shaders/post/quad");
			//_blurCombineShader = resourceManager.Load<Duality.Resources.Shader>("/shaders/post/combine_blur");
			_highPassShader = new DrawTechnique(Shader.LoadEmbeddedShaderSource("shaders/post/highpass.glsl"), "");
			_blurHorizontalShader = new DrawTechnique(Shader.LoadEmbeddedShaderSource("shaders/post/blur.glsl"), "BLUR_HORIZONTAL");
			_blurVerticalShader = new DrawTechnique(Shader.LoadEmbeddedShaderSource("shaders/post/blur.glsl"), "BLUR_VERTICAL");
			_quadShader = new DrawTechnique(Shader.LoadEmbeddedShaderSource("shaders/post/quad.glsl"), "");
			_blurCombineShader = new DrawTechnique(Shader.LoadEmbeddedShaderSource("shaders/post/combine_blur.glsl"), "");
		}

		private RenderTarget Downsample(RenderTarget source, RenderTarget destination)
		{
			_backend.BeginPass(destination);
			_backend.BeginInstance(_quadShader.Handle, new int[] { source.Textures[0].Handle },
				samplers: new int[] { _blurSampler });
			_backend.BindShaderVariable(_quadParams.SamplerScene, 0);

			_backend.DrawMesh(_quadMesh.MeshHandle);
			_backend.EndPass();

			return destination;
		}

		private RenderTarget Upsample(RenderTarget source, RenderTarget destination)
		{
			_backend.BeginPass(destination);
			_backend.BeginInstance(_quadShader.Handle, new int[] { source.Textures[0].Handle },
				samplers: new int[] { _blurSampler });
			_backend.BindShaderVariable(_quadParams.SamplerScene, 0);

			_backend.DrawMesh(_quadMesh.MeshHandle);
			_backend.EndPass();

			return destination;
		}

		private RenderTarget Blur(HDRSettings settings, RenderTarget source, RenderTarget[] targets)
		{
			var blurTargetX = targets[0];
			var blurTargetY = targets[1];

			Vector2 blurTextureSize = new Vector2(source.Width, source.Height);

			// Blur horizontal
			_backend.BeginPass(blurTargetX);
			_backend.BeginInstance(_blurHorizontalShader.Handle, new int[] { source.Textures[0].Handle },
				samplers: new int[] { _backend.DefaultSamplerNoFiltering });
			_backend.BindShaderVariable(_blurHorizontalParams.SamplerScene, 0);
			_backend.BindShaderVariable(_blurHorizontalParams.BlurSigma, settings.BlurSigma);
			_backend.BindShaderVariable(_blurHorizontalParams.TextureSize, ref blurTextureSize);

			_backend.DrawMesh(_quadMesh.MeshHandle);
			_backend.EndPass();

			// Blur vertical
			_backend.BeginPass(blurTargetY);
			_backend.BeginInstance(_blurVerticalShader.Handle, new int[] { blurTargetX.Textures[0].Handle },
				samplers: new int[] { _backend.DefaultSamplerNoFiltering });
			_backend.BindShaderVariable(_blurVerticalParams.SamplerScene, 0);
			_backend.BindShaderVariable(_blurVerticalParams.BlurSigma, settings.BlurSigma);
			_backend.BindShaderVariable(_blurVerticalParams.TextureSize, ref blurTextureSize);

			_backend.DrawMesh(_quadMesh.MeshHandle);
			_backend.EndPass();

			return blurTargetY;
		}

		public RenderTarget Render(HDRSettings settings, RenderTarget input, RenderTarget luminance)
		{
			if (_highPassParams == null)
			{
				_highPassParams = new HighPassShaderParams();
				_quadParams = new QuadShaderParams();
				_blurHorizontalParams = new BlurShaderParams();
				_blurVerticalParams = new BlurShaderParams();
				_blurCombineParams = new BlurCombineShaderParams();

				_highPassShader.BindUniformLocations(_highPassParams);
				_quadShader.BindUniformLocations(_quadParams);
				_blurHorizontalShader.BindUniformLocations(_blurHorizontalParams);
				_blurVerticalShader.BindUniformLocations(_blurVerticalParams);
				_blurCombineShader.BindUniformLocations(_blurCombineParams);
			}

			// High pass
			_backend.BeginPass(_blurTargets[0][1]);
			_backend.BeginInstance(_highPassShader.Handle, new int[] { input.Textures[0].Handle, luminance.Textures[0].Handle },
				samplers: new int[] { _backend.DefaultSamplerNoFiltering, _backend.DefaultSampler });
			_backend.BindShaderVariable(_highPassParams.SamplerScene, 0);
			_backend.BindShaderVariable(_highPassParams.SamplerLuminance, 1);
			_backend.BindShaderVariable(_highPassParams.BloomThreshold, settings.BloomThreshold);
			_backend.BindShaderVariable(_highPassParams.KeyValue, settings.KeyValue);
			_backend.BindShaderVariable(_highPassParams.AutoKey, settings.AutoKey ? 1 : 0);
            _backend.BindShaderVariable(_highPassParams.TonemapOperator, (int)settings.TonemapOperator);

			_backend.DrawMesh(_quadMesh.MeshHandle);
			_backend.EndPass();

			Blur(settings, _blurTargets[0][1], _blurTargets[0]);

			for (var i = 0; i < _blurTargets.Length - 1; i++)
			{
				Downsample(_blurTargets[i][1], _blurTargets[i + 1][1]);
				Blur(settings, _blurTargets[i + 1][1], _blurTargets[i + 1]);
			}

			for (var i = _blurTargets.Length - 1; i > 0; i--)
			{
				Upsample(_blurTargets[i][1], _blurTargets[i - 1][1]);
				Blur(settings, _blurTargets[i - 1][1], _blurTargets[i - 1]);
			}

			// Combine blurs
			//_backend.BeginPass(_blurTargets[0][0]);
			//_backend.BeginInstance(_blurCombineShader.Handle, new int[] { _blurTargets[0][1].Textures[0].Handle, _blurTargets[1][1].Textures[0].Handle, _blurTargets[2][1].Textures[0].Handle, _blurTargets[3][1].Textures[0].Handle },
			//	samplers: new int[] { _backend.DefaultSamplerNoFiltering, _backend.DefaultSamplerNoFiltering, _backend.DefaultSamplerNoFiltering, _backend.DefaultSamplerNoFiltering });
			//_backend.BindShaderVariable(_blurCombineParams.SamplerBlur0, 0);
			//_backend.BindShaderVariable(_blurCombineParams.SamplerBlur1, 1);
			//_backend.BindShaderVariable(_blurCombineParams.SamplerBlur2, 2);
			//_backend.BindShaderVariable(_blurCombineParams.SamplerBlur3, 3);
			//
			//_backend.DrawMesh(_quadMesh.MeshHandle);
			//_backend.EndPass();

			return _blurTargets[0][1];
		}

		public override void Resize(int width, int height)
		{
			base.Resize(width, height);

			//Backend.ResizeRenderTarget(_blurTargets[0], width, height);
			//Backend.ResizeRenderTarget(_blurTargets[1], width / 2, height / 2);
			//Backend.ResizeRenderTarget(_blurTargets[2], width / 4, height / 4);
			//Backend.ResizeRenderTarget(_blurTargets[3], width / 8, height / 8);
			//Backend.ResizeRenderTarget(_blurTargets[4], width / 8, height / 8);
		}

		class HighPassShaderParams
		{
			public int SamplerScene = 0;
			public int SamplerLuminance = 0;
			public int BloomThreshold = 0;
			public int KeyValue = 0;
			public int AutoKey = 0;
            public int TonemapOperator = 0;
		}

		class QuadShaderParams
		{
			public int SamplerScene = 0;
		}

		class BlurShaderParams
		{
			public int ModelViewProjection = 0;
			public int SamplerScene = 0;
			public int TextureSize = 0;
			public int BlurSigma = 0;
		}

		class BlurCombineShaderParams
		{
			public int ModelViewProjection = 0;
			public int SamplerBlur0 = 0;
			public int SamplerBlur1 = 0;
			public int SamplerBlur2 = 0;
			public int SamplerBlur3 = 0;
		}
	}
}
