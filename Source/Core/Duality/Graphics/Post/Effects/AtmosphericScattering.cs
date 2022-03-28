using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Duality.Resources;

namespace Duality.Graphics.Post.Effects
{
    public class AtmosphericScattering : BaseEffect
    {
        private DrawTechnique _shader;
        private ShaderParams _shaderParams;

        private readonly int[] _textures = new int[3];
        private readonly int[] _samplers;

        public AtmosphericScattering(BatchBuffer quadMesh)
            : base(quadMesh)
        {
            _samplers = new int[] { DualityApp.GraphicsBackend.DefaultSamplerNoFiltering, DualityApp.GraphicsBackend.DefaultSamplerNoFiltering, DualityApp.GraphicsBackend.DefaultSamplerNoFiltering };
        }

        internal override void LoadResources()
        {
            //_shader = resourceManager.Load<Duality.Resources.Shader>("/shaders/post/atmosphericscattering");
			_shader = new DrawTechnique(Shader.LoadEmbeddedShaderSource("shaders/post/atmosphericscattering.glsl"), "");
		}

        public bool Render(Duality.Components.Camera camera, Stage stage, RenderTarget gbuffer, RenderTarget input, RenderTarget output)
        {
           var light = stage.GetSunLight();
           if (light == null)
               return false;

            if (_shaderParams == null)
            {
                _shaderParams = new ShaderParams();
                _shader.BindUniformLocations(_shaderParams);
            }

			DualityApp.GraphicsBackend.BeginPass(output, new Vector4(0.0f, 0.0f, 0.0f, 1.0f));

            _textures[0] = input.Textures[0].Handle;
            _textures[1] = gbuffer.Textures[3].Handle;
            _textures[2] = gbuffer.Textures[1].Handle;

            Vector3 unitZ = Vector3.UnitZ;
            //Vector3 lightDirWS = -Vector3.Up;
			var ori = light.GameObj.Transform.Quaternion;
			Vector3.Transform(ref unitZ, ref ori, out var lightDirWS);
            lightDirWS = -lightDirWS.Normalized;

            camera.GetViewMatrix(out var view);
            camera.GetProjectionMatrix(out var projection);
            var inverseViewProjectionMatrix = Matrix4.Invert(view * projection);

			DualityApp.GraphicsBackend.BeginInstance(_shader.Handle, _textures, samplers: _samplers);
            DualityApp.GraphicsBackend.BindShaderVariable(_shaderParams.SamplerScene, 0);
            DualityApp.GraphicsBackend.BindShaderVariable(_shaderParams.SamplerDepth, 1);
            DualityApp.GraphicsBackend.BindShaderVariable(_shaderParams.SamplerGBuffer1, 2);
            DualityApp.GraphicsBackend.BindShaderVariable(_shaderParams.SunDirection, ref lightDirWS);
            DualityApp.GraphicsBackend.BindShaderVariable(_shaderParams.InvViewProjection, ref inverseViewProjectionMatrix);
			var Pos = camera.GameObj.Transform.Pos;
			DualityApp.GraphicsBackend.BindShaderVariable(_shaderParams.CameraPosition, ref Pos);

			DualityApp.GraphicsBackend.DrawMesh(_quadMesh.MeshHandle);
			DualityApp.GraphicsBackend.EndPass();

            return true;
        }

        class ShaderParams
        {
            public int SamplerScene = 0;
            public int SamplerDepth = 0;
            public int SunDirection = 0;
            public int SamplerGBuffer1 = 0;
            public int CameraPosition = 0;
            public int InvViewProjection = 0;
        }
    }
}
