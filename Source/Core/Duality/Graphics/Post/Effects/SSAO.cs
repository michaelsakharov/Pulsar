using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Duality.Drawing;
using Duality.Renderer.RenderTargets;
using Duality.Resources;

namespace Duality.Graphics.Post.Effects
{
    public class SSAO : BaseEffect
    {
        private DrawTechnique _shader;
        private DrawTechnique _shaderBlur;
        private SSAOShaderParams _shaderParams;
        private RenderTarget _renderTarget;
        private RenderTarget _renderTargetBlur;
        private Vector3[] _sampleKernel;
        private Duality.Resources.Texture _noiseTexture;
        private int _noiseSampler;

        public SSAO(Backend backend, BatchBuffer quadMesh) : base(backend, quadMesh)
        {
            var width = backend.Width;
            var height = backend.Height;

            _renderTarget = _backend.CreateRenderTarget("ssao", new Definition(width, height, false, new List<Definition.Attachment>()
                {
                    new Definition.Attachment(Definition.AttachmentPoint.Color, Renderer.PixelFormat.Rgba, Renderer.PixelInternalFormat.Rgba8, Renderer.PixelType.Float, 0),
                }));

            _renderTargetBlur = _backend.CreateRenderTarget("ssao_blur", new Definition(width, height, false, new List<Definition.Attachment>()
                {
                    new Definition.Attachment(Definition.AttachmentPoint.Color, Renderer.PixelFormat.Rgba, Renderer.PixelInternalFormat.Rgba8, Renderer.PixelType.Float, 0),
                }));

			var rnd = new System.Random();
			var noise = new Vector3[16];
			for (var i = 0; i < noise.Length; i++)
            {
                noise[i] = new Vector3(
                    rnd.NextFloat(-1.0f, 2.0f),
					rnd.NextFloat(-1.0f, 2.0f),
                    0.0f
                    );
			
                noise[i] = noise[i].Normalized;
            }

            var noiseData = GetBytes(noise);
			//_noiseTexture = _backend.CreateTexture("ssao_noise", 4, 4, Renderer.PixelFormat.Rgb, Renderer.PixelInternalFormat.Rgb16f, Renderer.PixelType.Float, noiseData, false);
			_noiseTexture = new Texture(new Pixmap(new PixelData(4, 4, noiseData)));


			_noiseSampler = _backend.RenderSystem.CreateSampler(new Dictionary<Renderer.SamplerParameterName, int>
            {
                { Renderer.SamplerParameterName.TextureMinFilter, (int)Renderer.TextureMinFilter.Nearest },
                { Renderer.SamplerParameterName.TextureMagFilter, (int)Renderer.TextureMagFilter.Nearest },
                { Renderer.SamplerParameterName.TextureWrapS, (int)Renderer.TextureWrapMode.Repeat },
                { Renderer.SamplerParameterName.TextureWrapT, (int)Renderer.TextureWrapMode.Repeat }
            });

            _sampleKernel = new Vector3[64];
            for (var i = 0; i < _sampleKernel.Length; i++)
            {
                var scale = (float)i / (float)_sampleKernel.Length;
                var v = new Vector3(
						rnd.NextFloat(-1.0f, 2.0f),
                        rnd.NextFloat(-1.0f, 2.0f),
                        rnd.NextFloat(0.0f, 2.0f)
                    );

                v *= (0.1f + 0.9f * scale * scale);
                _sampleKernel[i] = v;
            }
        }

        private ColorRgba[] GetBytes(Vector3[] data)
        {
			ColorRgba[] arr = new ColorRgba[data.Length];

            for (var i = 0; i < data.Length; i++)
            {
				arr[i] = new ColorRgba(data[i].X, data[i].Y, data[i].Z);

			}

            return arr;
        }

        internal override void LoadResources()
        {
            //_shader = resourceManager.Load<Duality.Resources.Shader>("/shaders/post/ssao");
            //_shaderBlur = resourceManager.Load<Duality.Resources.Shader>("/shaders/post/ssao_blur");
			_shader = new DrawTechnique(Shader.LoadEmbeddedShaderSource("shaders/post/ssao.glsl"), "");
			_shaderBlur = new DrawTechnique(Shader.LoadEmbeddedShaderSource("shaders/post/ssao_blur.glsl"), "");
		}

        public RenderTarget Render(Duality.Components.Camera camera, RenderTarget gbuffer)
        {
            if (_shaderParams == null)
            {
                _shaderParams = new SSAOShaderParams();
                _shader.BindUniformLocations(_shaderParams);
            }

            camera.GetViewMatrix(out var viewMatrix);
            camera.GetProjectionMatrix(out var projectionMatrix);

            var invViewProjectionMatrix = Matrix4.Invert(viewMatrix * projectionMatrix);
            var invProjectionMatrix = Matrix4.Invert(projectionMatrix);
            var itViewMatrix = Matrix4.Invert(Matrix4.Transpose(viewMatrix));

            _backend.BeginPass(_renderTarget, new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
            _backend.BeginInstance(_shader.Handle, new int[] { gbuffer.Textures[1].Handle, gbuffer.Textures[3].Handle, _noiseTexture.Handle },
                samplers: new int[] { _backend.DefaultSamplerNoFiltering, _backend.DefaultSamplerNoFiltering, _noiseSampler });
            _backend.BindShaderVariable(_shaderParams.SamplerDepth, 1);
            _backend.BindShaderVariable(_shaderParams.SamplerGBuffer1, 0);
            _backend.BindShaderVariable(_shaderParams.SamplerNoise, 2);
            _backend.BindShaderVariable(_shaderParams.InvViewProjection, ref invViewProjectionMatrix);
            _backend.BindShaderVariable(_shaderParams.InvProjection, ref invProjectionMatrix);
            _backend.BindShaderVariable(_shaderParams.View, ref viewMatrix);
            _backend.BindShaderVariable(_shaderParams.ItView, ref itViewMatrix);
            _backend.BindShaderVariable(_shaderParams.Proj, ref projectionMatrix);
            _backend.BindShaderVariable(_shaderParams.SampleKernel, ref _sampleKernel);
			var Pos = camera.GameObj.Transform.Pos;
			_backend.BindShaderVariable(_shaderParams.CameraPosition, ref Pos);

            var tanHalfFov = (float)System.Math.Tan(camera.Fov / 2.0f);
            var aspectRatio = camera.Viewport.X / camera.Viewport.Y;

            _backend.BindShaderVariable(_shaderParams.TanHalfFov, tanHalfFov);
            _backend.BindShaderVariable(_shaderParams.AspectRatio, aspectRatio);

            var clipPlanes = new Vector2(camera.NearClipDistance, camera.FarClipDistance);
            _backend.BindShaderVariable(_shaderParams.CameraClipPlanes, ref clipPlanes);

            var size = new Vector2(gbuffer.Width, gbuffer.Height);
            _backend.BindShaderVariable(_shaderParams.ViewportResolution, ref size);

            _backend.DrawMesh(_quadMesh.MeshHandle);
            _backend.EndPass();

            _backend.BeginPass(_renderTargetBlur, new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
            _backend.BeginInstance(_shaderBlur.Handle, new int[] { _renderTarget.Textures[0].Handle }, samplers: new int[] { _backend.DefaultSamplerNoFiltering });

            _backend.DrawMesh(_quadMesh.MeshHandle);
            _backend.EndPass();

            return _renderTargetBlur;
        }

        class SSAOShaderParams
        {
            public int SamplerDepth = 0;
            public int SamplerGBuffer1 = 0;
            public int InvViewProjection = 0;
            public int InvProjection = 0;
            public int View = 0;
            public int ItView = 0;
            public int Proj = 0;
            public int ViewportResolution = 0;
            public int CameraClipPlanes = 0;
            public int TanHalfFov = 0;
            public int AspectRatio = 0;
            public int SampleKernel = 0;
            public int SamplerNoise = 0;
            public int CameraPosition = 0;
        }
    }
}
