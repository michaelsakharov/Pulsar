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
    public class Visualize : BaseEffect
    {
		private DrawTechnique _shader;
        private ShaderParams _shaderParams;

        public Visualize(BatchBuffer quadMesh)
            : base(quadMesh)
        {
        }

        internal override void LoadResources()
        {
            //_shader = resourceManager.Load<Duality.Resources.Shader>("/shaders/post/visualize");
			_shader = new DrawTechnique(Shader.LoadEmbeddedShaderSource("shaders/post/visualize.glsl"), "");
		}

        public void Render(VisualizationMode mode, Duality.Components.Camera camera, RenderTarget gbuffer, RenderTarget ssao, RenderTarget input, RenderTarget output)
        {
            if (_shaderParams == null)
            {
                _shaderParams = new ShaderParams();
                _shader.BindUniformLocations(_shaderParams);
            }

			DualityApp.GraphicsBackend.BeginPass(output, Vector4.Zero);

            int ssaoHandle = 0;
            if (ssao != null)
                ssaoHandle = ssao.Textures[0].Handle;

			DualityApp.GraphicsBackend.BeginInstance(_shader.Handle,
                new int[] { gbuffer.Textures[0].Handle, gbuffer.Textures[1].Handle, gbuffer.Textures[2].Handle, gbuffer.Textures[3].Handle, ssaoHandle },
                new int[] { DualityApp.GraphicsBackend.DefaultSamplerNoFiltering, DualityApp.GraphicsBackend.DefaultSamplerNoFiltering, DualityApp.GraphicsBackend.DefaultSamplerNoFiltering, DualityApp.GraphicsBackend.DefaultSamplerNoFiltering, DualityApp.GraphicsBackend.DefaultSamplerNoFiltering, DualityApp.GraphicsBackend.DefaultSamplerNoFiltering });
			DualityApp.GraphicsBackend.BindShaderVariable(_shaderParams.SamplerGBuffer0, 0);
            DualityApp.GraphicsBackend.BindShaderVariable(_shaderParams.SamplerGBuffer1, 1);
            DualityApp.GraphicsBackend.BindShaderVariable(_shaderParams.SamplerGBuffer2, 2);
            DualityApp.GraphicsBackend.BindShaderVariable(_shaderParams.SamplerGBuffer3, 3);
            DualityApp.GraphicsBackend.BindShaderVariable(_shaderParams.SamplerSSAO, 4);
            DualityApp.GraphicsBackend.BindShaderVariable(_shaderParams.SamplerCSM, 7);
            DualityApp.GraphicsBackend.BindShaderVariable(_shaderParams.VisualizationMode, (int)mode);

            var clipPlanes = new Vector2(camera.NearClipDistance, camera.FarClipDistance);
			DualityApp.GraphicsBackend.BindShaderVariable(_shaderParams.CameraClipPlanes, ref clipPlanes);

			DualityApp.GraphicsBackend.DrawMesh(_quadMesh.MeshHandle);

			DualityApp.GraphicsBackend.EndPass();
        }

        class ShaderParams
        {
            public int VisualizationMode = 0;
            public int SamplerGBuffer0 = 0;
            public int SamplerGBuffer1 = 0;
            public int SamplerGBuffer2 = 0;
            public int SamplerGBuffer3 = 0;
            public int SamplerSSAO = 0;
            public int SamplerCSM = 0;
            public int CameraClipPlanes = 0;
        }
    }
}
