﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Duality.Renderer.RenderTargets;
using Duality.Resources;

namespace Duality.Graphics.Post.Effects
{
	public class AdaptLuminance : BaseEffect
	{
		private DrawTechnique _luminanceMapShader;
		private DrawTechnique _adaptLuminanceShader;

		private LuminanceMapShaderParams _luminanceMapParams;
		private AdaptLuminanceShaderParams _adaptLuminanceParams;

		private readonly RenderTarget _luminanceTarget;
		private readonly RenderTarget[] _adaptLuminanceTargets;

		private int _currentLuminanceTarget = 0;

		public AdaptLuminance(BatchBuffer quadMesh)
			: base(quadMesh)
		{
			// Setup render targets
			_luminanceTarget = DualityApp.GraphicsBackend.CreateRenderTarget("avg_luminance", new Definition(1024, 1024, false, new List<Definition.Attachment>()
			{
				new Definition.Attachment(Definition.AttachmentPoint.Color, Renderer.PixelFormat.Rgba, Renderer.PixelInternalFormat.R32f, Renderer.PixelType.Float, 0, true),
			}));

			_adaptLuminanceTargets = new RenderTarget[]
			{
				DualityApp.GraphicsBackend.CreateRenderTarget("adapted_luminance_0", new Definition(1, 1, false, new List<Definition.Attachment>()
				{
					new Definition.Attachment(Definition.AttachmentPoint.Color, Renderer.PixelFormat.Rgba, Renderer.PixelInternalFormat.R32f, Renderer.PixelType.Float, 0),
				})),
				DualityApp.GraphicsBackend.CreateRenderTarget("adapted_luminance_1", new Definition(1, 1, false, new List<Definition.Attachment>()
				{
					new Definition.Attachment(Definition.AttachmentPoint.Color, Renderer.PixelFormat.Rgba, Renderer.PixelInternalFormat.R32f, Renderer.PixelType.Float, 0),
				}))
			};
		}

		internal override void LoadResources()
		{
			//_luminanceMapShader = resourceManager.Load<Duality.Resources.Shader>("/shaders/post/luminance_map");
			//_adaptLuminanceShader = resourceManager.Load<Duality.Resources.Shader>("/shaders/post/adapt_luminance");
			_luminanceMapShader = new DrawTechnique(Shader.LoadEmbeddedShaderSource("shaders/post/luminance_map.glsl"), "");
			_adaptLuminanceShader = new DrawTechnique(Shader.LoadEmbeddedShaderSource("shaders/post/adapt_luminance.glsl"), "");
		}

		public RenderTarget Render(HDRSettings settings, RenderTarget input, float deltaTime)
		{
			if (_luminanceMapParams == null)
			{
				_luminanceMapParams = new LuminanceMapShaderParams();
				_adaptLuminanceParams = new AdaptLuminanceShaderParams();

				_luminanceMapShader.BindUniformLocations(_luminanceMapShader);
				_adaptLuminanceShader.BindUniformLocations(_adaptLuminanceParams);
			}

			// Calculate luminance
			DualityApp.GraphicsBackend.BeginPass(_luminanceTarget);
			DualityApp.GraphicsBackend.BeginInstance(_luminanceMapShader.Handle, new int[] { input.Textures[0].Handle },
				samplers: new int[] { DualityApp.GraphicsBackend.DefaultSamplerNoFiltering });
			DualityApp.GraphicsBackend.BindShaderVariable(_luminanceMapParams.SamplerScene, 0);

			DualityApp.GraphicsBackend.DrawMesh(_quadMesh.MeshHandle);
			DualityApp.GraphicsBackend.EndPass();

			DualityApp.GraphicsBackend.GenerateMips(_luminanceTarget.Textures[0].Handle);

			// Adapt luminace
			var adaptedLuminanceTarget = _adaptLuminanceTargets[_currentLuminanceTarget];
			var adaptedLuminanceSource = _adaptLuminanceTargets[_currentLuminanceTarget == 0 ? 1 : 0];
			_currentLuminanceTarget = (_currentLuminanceTarget + 1) % 2;

			DualityApp.GraphicsBackend.BeginPass(adaptedLuminanceTarget);
			DualityApp.GraphicsBackend.BeginInstance(_adaptLuminanceShader.Handle, new int[] { adaptedLuminanceSource.Textures[0].Handle, _luminanceTarget.Textures[0].Handle },
				samplers: new int[] { DualityApp.GraphicsBackend.DefaultSamplerNoFiltering, DualityApp.GraphicsBackend.DefaultSamplerMipMapNearest });
			DualityApp.GraphicsBackend.BindShaderVariable(_adaptLuminanceParams.SamplerLastLuminacne, 0);
			DualityApp.GraphicsBackend.BindShaderVariable(_adaptLuminanceParams.SamplerCurrentLuminance, 1);
			DualityApp.GraphicsBackend.BindShaderVariable(_adaptLuminanceParams.TimeDelta, deltaTime);
			DualityApp.GraphicsBackend.BindShaderVariable(_adaptLuminanceParams.Tau, settings.AdaptationRate);

			DualityApp.GraphicsBackend.DrawMesh(_quadMesh.MeshHandle);
			DualityApp.GraphicsBackend.EndPass();

			return adaptedLuminanceTarget;
		}

		class LuminanceMapShaderParams
		{
			public int SamplerScene = 0;
		}

		class AdaptLuminanceShaderParams
		{
			public int SamplerLastLuminacne = 0;
			public int SamplerCurrentLuminance = 0;
			public int TimeDelta = 0;
			public int Tau = 0;
		}
	}
}
