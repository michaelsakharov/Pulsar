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

        public Visualize(Backend backend, BatchBuffer quadMesh)
            : base(backend, quadMesh)
        {
        }

        internal override void LoadResources()
        {
            //_shader = resourceManager.Load<Duality.Resources.Shader>("/shaders/post/visualize");
			_shader = new DrawTechnique(Shader.LoadEmbeddedShaderSource("shaders/post/visualize.glsl"), "");
		}

        public void Render(VisualizationMode mode, Duality.Components.Camera camera, RenderTarget gbuffer, RenderTarget ssao, RenderTarget csmShadowBuffer, RenderTarget input, RenderTarget output)
        {
            if (_shaderParams == null)
            {
                _shaderParams = new ShaderParams();
                _shader.BindUniformLocations(_shaderParams);
            }

            _backend.BeginPass(output, Vector4.Zero);

            int ssaoHandle = 0;
            if (ssao != null)
                ssaoHandle = ssao.Textures[0].Handle;

            _backend.BeginInstance(_shader.Handle,
                new int[] { gbuffer.Textures[0].Handle, gbuffer.Textures[1].Handle, gbuffer.Textures[2].Handle, gbuffer.Textures[3].Handle, ssaoHandle, csmShadowBuffer.Textures[0].Handle },
                new int[] { _backend.DefaultSamplerNoFiltering, _backend.DefaultSamplerNoFiltering, _backend.DefaultSamplerNoFiltering, _backend.DefaultSamplerNoFiltering, _backend.DefaultSamplerNoFiltering, _backend.DefaultSamplerNoFiltering });
            _backend.BindShaderVariable(_shaderParams.SamplerGBuffer0, 0);
            _backend.BindShaderVariable(_shaderParams.SamplerGBuffer1, 1);
            _backend.BindShaderVariable(_shaderParams.SamplerGBuffer2, 2);
            _backend.BindShaderVariable(_shaderParams.SamplerGBuffer3, 3);
            _backend.BindShaderVariable(_shaderParams.SamplerSSAO, 4);
            _backend.BindShaderVariable(_shaderParams.SamplerCSM, 7);
            _backend.BindShaderVariable(_shaderParams.VisualizationMode, (int)mode);

            var clipPlanes = new Vector2(camera.NearClipDistance, camera.FarClipDistance);
            _backend.BindShaderVariable(_shaderParams.CameraClipPlanes, ref clipPlanes);

            _backend.DrawMesh(_quadMesh.MeshHandle);

            _backend.EndPass();
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
