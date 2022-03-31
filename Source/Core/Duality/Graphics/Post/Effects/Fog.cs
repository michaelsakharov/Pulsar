using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Duality.Graphics.Resources;
using Duality.Renderer.RenderTargets;
using Duality.Resources;

namespace Duality.Graphics.Post.Effects
{
	public class Fog : BaseEffect
	{
		private DrawTechnique _shader;
		private ShaderParams _shaderParams;

		public Fog(BatchBuffer quadMesh)
			: base(quadMesh)
		{
		}

		internal override void LoadResources()
		{
			//_shader = resourceManager.Load<Duality.Resources.Shader>("/shaders/post/fog");
			_shader = new DrawTechnique(Shader.LoadEmbeddedShaderSource("shaders/post/fog.glsl"), "");
		}

		public void Render(Duality.Components.Camera camera, Stage stage, RenderTarget gbuffer, RenderTarget input, RenderTarget output)
		{
			if (_shaderParams == null)
			{
				_shaderParams = new ShaderParams();
				_shader.BindUniformLocations(_shaderParams);
			}

			Matrix4 view, projection;
			camera.GetViewMatrix(out view);
			camera.GetProjectionMatrix(out projection);

			DualityApp.GraphicsBackend.BeginPass(output, Vector4.Zero);

			var inverseViewProjectionMatrix = Matrix4.Invert(view * projection);

			DualityApp.GraphicsBackend.BeginInstance(_shader.Handle,
				new int[] { gbuffer.Textures[3].Handle, input.Textures[0].Handle },
				new int[] { DualityApp.GraphicsBackend.DefaultSamplerNoFiltering, DualityApp.GraphicsBackend.DefaultSamplerNoFiltering });
			DualityApp.GraphicsBackend.BindShaderVariable(_shaderParams.SamplerDepth, 0);
			DualityApp.GraphicsBackend.BindShaderVariable(_shaderParams.SamplerScene, 1);
			DualityApp.GraphicsBackend.BindShaderVariable(_shaderParams.InvViewProjection, ref inverseViewProjectionMatrix);

			var Pos = camera.GameObj.Transform.Pos;
			DualityApp.GraphicsBackend.BindShaderVariable(_shaderParams.CameraPosition, ref Pos);

			var sunLight = stage.GetSunLight();
            if (sunLight != null)
            {
                Vector3 unitZ = Vector3.UnitZ;
				var orient = sunLight.GameObj.Transform.Quaternion;
				Vector3.Transform(ref unitZ, ref orient, out var lightDirWS);
				lightDirWS.Normalize();

				DualityApp.GraphicsBackend.BindShaderVariable(_shaderParams.SunDir, ref lightDirWS);
            }

			DualityApp.GraphicsBackend.DrawMesh(_quadMesh.MeshHandle);

			DualityApp.GraphicsBackend.EndPass();
		}

		class ShaderParams
		{
			public int SamplerDepth = 0;
			public int SamplerScene = 0;
			public int InvViewProjection = 0;
			public int CameraPosition = 0;
			public int SunDir = 0;
		}
	}
}

