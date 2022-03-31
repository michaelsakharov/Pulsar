using System;
using System.Collections.Generic;
using System.Text;
using Duality.Components;
using Duality.Renderer;

namespace Duality.Graphics.Pipelines
{
	public class DeferredPipeline
	{
		private Graphics.Deferred.DeferredRenderer DeferredRenderer;
		private Graphics.Deferred.ShadowRenderer ShadowRenderer;
		private Graphics.Deferred.ShadowBufferRenderer ShadowBufferRenderer;
		private Graphics.Post.PostEffectManager PostEffectManager;

		private Graphics.SpriteBatch SpriteRenderer;

		private int Width;
		private int Height;

		public DeferredPipeline(int width, int height)
		{
			Width = width;
			Height = height;

			ShadowRenderer = new Graphics.Deferred.ShadowRenderer();
			DeferredRenderer = new Graphics.Deferred.DeferredRenderer(ShadowRenderer, Width, Height);
			ShadowBufferRenderer = new Graphics.Deferred.ShadowBufferRenderer(Width, Height);
			PostEffectManager = new Graphics.Post.PostEffectManager(Width, Height);

			SpriteRenderer = DualityApp.GraphicsBackend.CreateSpriteBatch();
		}

		/// <summary>
		/// Feed render commands to the graphics backend.
		/// Only override this method if you wish to customize the rendering pipeline.
		/// </summary>
		public void RenderStage(float deltaTime, Stage stage, Camera camera)
		{
			DualityApp.GraphicsBackend.BeginScene();

			if (camera != null)
			{
				var gbuffer = DeferredRenderer.RenderGBuffer(stage, camera);
				var sunLight = stage.GetSunLight();

				// Prepare shadow buffer for sunlight
				RenderTarget shadows = null;
				if (sunLight != null && sunLight.CastShadows)
				{
					Profile.TimeShadowsGeneration.BeginMeasure();
					List<RenderTarget>  csm = ShadowRenderer.RenderCSM(gbuffer, sunLight, stage, camera, out var viewProjections, out var clipDistances);
					Profile.TimeShadowsGeneration.EndMeasure();

					ShadowBufferRenderer.DebugCascades = PostEffectManager.VisualizationMode == Graphics.Post.VisualizationMode.CSM;

					Profile.TimeShadowsRender.BeginMeasure();
					shadows = ShadowBufferRenderer.Render(camera, gbuffer, csm, viewProjections, clipDistances, DeferredRenderer.Settings.ShadowQuality);
					Profile.TimeShadowsRender.EndMeasure();
				}

				// Light + post, ssao needed for ambient so we render it first
				Profile.TimeSSAO.BeginMeasure();
				var ssao = PostEffectManager.RenderSSAO(camera, gbuffer);
				Profile.TimeSSAO.EndMeasure();
				var lightOutput = DeferredRenderer.RenderLighting(stage, camera, shadows, ssao);
				var postProcessedResult = PostEffectManager.Render(camera, stage, gbuffer, lightOutput, shadows, deltaTime);

				DualityApp.GraphicsBackend.BeginPass(null, Vector4.Zero, ClearFlags.Color);

				//SpriteRenderer.RenderQuad(postProcessedResult.Textures[0], Vector2.Zero, new Vector2(Width, Height));
				SpriteRenderer.RenderQuad(postProcessedResult.Textures[0], camera.Viewport.Pos, camera.Viewport.Size);
				SpriteRenderer.Render(Width, Height);

				//DoRenderUI(deltaTime);
				//SpriteRenderer.Render(WindowSize.X, WindowSize.Y);
			}

			DualityApp.GraphicsBackend.EndScene();
		}

		public void Resize(int width, int height)
		{
			Width = width;
			Height = height;

			//if (ShadowRenderer != null) ShadowRenderer.Resize(width, height);
			if (DeferredRenderer != null) DeferredRenderer.Resize(width, height);
			if (ShadowBufferRenderer != null) ShadowBufferRenderer.Resize(width, height);
			//if (SpriteRenderer != null) SpriteRenderer.Resize(width, height);
			if (PostEffectManager != null) PostEffectManager.Resize(width, height);
		}

		public void ChangleGLContext(ContextReference ctx)
		{

		}

		public void Dispose()
		{
			//if (ShadowRenderer != null) ShadowRenderer.Dispose();
			//if (DeferredRenderer != null) DeferredRenderer.Dispose();
			//if (ShadowBufferRenderer != null) ShadowBufferRenderer.Dispose();
			//if (PostEffectManager != null) PostEffectManager.Dispose();

			if (SpriteRenderer != null) SpriteRenderer.Dispose();
		}

	}
}
