using System.Collections.Generic;
using Duality.Renderer;
using Duality.Renderer.RenderTargets;
using Duality.Resources;

namespace Duality.Graphics.Deferred
{
    public class DeferredRenderer
    {
        private LightParams[] _lightParams;

        private Vector2 _screenSize;

        private readonly RenderTarget _gbuffer;
        private readonly RenderTarget _lightAccumulationTarget;

        private BatchBuffer _quadMesh;

        private DrawTechnique[] _lightShaders;


        private bool _initialized = false;

        private int _directionalRenderState;

        private readonly RenderOperations _renderOperations = new RenderOperations();

        private int[] _lightTextureBinds = new int[5];
        private int[] _lightSamplers = new int[5];

        public DeferredRenderer(int width, int height)
        {

            _screenSize = new Vector2(width, height);

            _gbuffer = DualityApp.GraphicsBackend.CreateRenderTarget("gbuffer", new Definition(width, height, true, new List<Definition.Attachment>()
            {
                new Definition.Attachment(Definition.AttachmentPoint.Color, PixelFormat.Rgba, PixelInternalFormat.Rgba8, PixelType.UnsignedByte, 0), // Color
                new Definition.Attachment(Definition.AttachmentPoint.Color, PixelFormat.Rgba, PixelInternalFormat.Rgba16f, PixelType.Float, 1), // Normal
                new Definition.Attachment(Definition.AttachmentPoint.Color, PixelFormat.Rgba, PixelInternalFormat.Rgba8, PixelType.UnsignedByte, 2), // Specular
                new Definition.Attachment(Definition.AttachmentPoint.Color, PixelFormat.Rgba, PixelInternalFormat.Rgba32f, PixelType.Float, 3), // Position
                new Definition.Attachment(Definition.AttachmentPoint.Depth, PixelFormat.DepthComponent, PixelInternalFormat.DepthComponent32f, PixelType.Float, 0) // Depth
            }));

            _lightAccumulationTarget = DualityApp.GraphicsBackend.CreateRenderTarget("light_accumulation", new Definition(width, height, false, new List<Definition.Attachment>()
            {
                new Definition.Attachment(Definition.AttachmentPoint.Color, PixelFormat.Rgba, PixelInternalFormat.Rgba16f, PixelType.Float, 0)
            }));

			// Init light shaders
			var lightTypes = new string[] { "DIRECTIONAL_LIGHT", "POINT_LIGHT" };
            var lightPermutations = new string[] { "NO_SHADOWS", "SHADOWS" };

            _lightShaders = new DrawTechnique[lightTypes.Length * lightPermutations.Length];
            _lightParams = new LightParams[lightTypes.Length * lightPermutations.Length];

            for (var l = 0; l < lightTypes.Length; l++)
            {
                var lightType = lightTypes[l];
                for (var p = 0; p < lightPermutations.Length; p++)
                {
                    var index = l * lightPermutations.Length + p;
                    var defines = lightType + ";" + lightPermutations[p];

					_lightShaders[index] = new DrawTechnique(Shader.LoadEmbeddedShaderSource("shaders/deferred/light.glsl"), defines);
					_lightParams[index] = new LightParams();
                }
            }

			_quadMesh = DualityApp.GraphicsBackend.CreateBatchBuffer();
            _quadMesh.Begin();
            _quadMesh.AddQuad(new Vector2(-1, -1), new Vector2(2, 2), Vector2.Zero, new Vector2(1, 1));
            _quadMesh.End();

            _directionalRenderState = DualityApp.GraphicsBackend.CreateRenderState(true, false, false, BlendingFactorSrc.One, BlendingFactorDest.One, CullFaceMode.Back, true, DepthFunction.Lequal);
        }

		public void Resize(int width, int height)
		{

			_screenSize = new Vector2(width, height);
			DualityApp.GraphicsBackend.ResizeRenderTarget(_gbuffer, width, height);
			DualityApp.GraphicsBackend.ResizeRenderTarget(_lightAccumulationTarget, width, height);
		}

        public void InitializeHandles()
        {
            for (var i = 0; i < _lightParams.Length; i++)
            {
                _lightShaders[i].BindUniformLocations(_lightParams[i]);
            }
        }

        private void Initialize()
        {
            if (!_initialized)
            {
                InitializeHandles();
                _initialized = true;
            }
        }

        public RenderTarget RenderGBuffer(Stage stage, Duality.Components.Camera camera)
        {
            Initialize();

            // Init common matrices
            camera.GetViewMatrix(out var view);
            camera.GetProjectionMatrix(out var projection);

            // Render scene to GBuffer
            var clearColor = stage.ClearColor;
            clearColor.W = 0;
			DualityApp.GraphicsBackend.ProfileBeginSection(Profiler.GBuffer);
			DualityApp.GraphicsBackend.BeginPass(_gbuffer, clearColor, ClearFlags.All);
            RenderScene(stage, camera, ref view, ref projection);
			DualityApp.GraphicsBackend.EndPass();
			DualityApp.GraphicsBackend.ProfileEndSection(Profiler.GBuffer);

            return _gbuffer;
        }

        public RenderTarget RenderLighting(Stage stage, Duality.Components.Camera camera)
        {
            Initialize();

            // Init common matrices
            camera.GetViewMatrix(out var view);
            camera.GetProjectionMatrix(out var projection);

			// Render light accumulation
			DualityApp.GraphicsBackend.ProfileBeginSection(Profiler.Lighting);

			DualityApp.GraphicsBackend.BeginPass(_lightAccumulationTarget, new Vector4(0.0f, 0.0f, 0.0f, 1.0f), ClearFlags.All);
            RenderLights(camera, ref view, ref projection, stage.GetLights());
			DualityApp.GraphicsBackend.EndPass();

			DualityApp.GraphicsBackend.ProfileEndSection(Profiler.Lighting);

            return _lightAccumulationTarget;
        }

        private void RenderScene(Stage stage, Duality.Components.Camera camera, ref Matrix4 view, ref Matrix4 projection)
        {
            var viewProjection = view * projection;

            _renderOperations.Reset();
            stage.PrepareRenderOperations(viewProjection, _renderOperations);

            _renderOperations.GetOperations(out var operations, out var count);

			Material activeMaterial = null;
            Matrix4 worldView, world, itWorld, worldViewProjection;

            for (var i = 0; i < count; i++)
            {
                world = operations[i].WorldMatrix;

                Matrix4.Multiply(ref world, ref viewProjection, out worldViewProjection);
                Matrix4.Multiply(ref world, ref view, out worldView);

                itWorld = Matrix4.Invert(Matrix4.Transpose(world));

                if (activeMaterial == null || activeMaterial.Id != operations[i].Material.Id)
                {
                    operations[i].Material.BeginInstance(camera, 0);
                }

                operations[i].Material.BindPerObject(ref world, ref worldView, ref itWorld, ref worldViewProjection, operations[i].Skeleton);
				DualityApp.GraphicsBackend.DrawMesh(operations[i].MeshHandle, OpenTK.Graphics.OpenGL.PrimitiveType.Triangles);
            }
        }

        private void RenderLights(Duality.Components.Camera camera, ref Matrix4 view, ref Matrix4 projection, IReadOnlyCollection<Components.LightComponent> lights)
        {
			DualityApp.GraphicsBackend.ProfileBeginSection(Profiler.DirectionaLight);
            foreach (var light in lights)
            {
				if(light.Type == LighType.Directional)
					RenderDirectionalLight(camera, ref view, ref projection, light);
				else if(light.Type == LighType.PointLight)
					RenderPointLight(camera, ref view, ref projection, light);
            }
			DualityApp.GraphicsBackend.ProfileEndSection(Profiler.DirectionaLight);
        }

        private void RenderDirectionalLight(Duality.Components.Camera camera, ref Matrix4 view, ref Matrix4 projection, Components.LightComponent light)
        {
            if (light.Type != LighType.Directional)
                return;
            
            var renderStateId = _directionalRenderState;

            var viewProjection = view * projection;
            var modelViewProjection = viewProjection;

            // Convert light color to linear space
            var lightColor = light.Color * light.Intensity;

            // Select the correct shader
            var lightTypeOffset = 0;

            var shader = _lightShaders[lightTypeOffset];
            var shaderParams = _lightParams[lightTypeOffset];

            // Setup textures and begin rendering with the chosen shader

            _lightSamplers[0] = DualityApp.GraphicsBackend.DefaultSamplerNoFiltering;
            _lightSamplers[1] = DualityApp.GraphicsBackend.DefaultSamplerNoFiltering;
            _lightSamplers[2] = DualityApp.GraphicsBackend.DefaultSamplerNoFiltering;
            _lightSamplers[4] = 0;

			DualityApp.GraphicsBackend.BeginInstance(shader.Handle, new int[] { _gbuffer.Textures[0].Handle, _gbuffer.Textures[1].Handle, _gbuffer.Textures[2].Handle, 0 }, _lightSamplers, renderStateId);

            // Setup texture samplers
            DualityApp.GraphicsBackend.BindShaderVariable(shaderParams.SamplerGBuffer0, 0);
            DualityApp.GraphicsBackend.BindShaderVariable(shaderParams.SamplerGBuffer1, 1);
            DualityApp.GraphicsBackend.BindShaderVariable(shaderParams.SamplerGBuffer2, 2);
            DualityApp.GraphicsBackend.BindShaderVariable(shaderParams.SamplerDepth, 4);

            // Common uniforms
            DualityApp.GraphicsBackend.BindShaderVariable(shaderParams.ScreenSize, ref _screenSize);
            DualityApp.GraphicsBackend.BindShaderVariable(shaderParams.ModelViewProjection, ref modelViewProjection);
			DualityApp.GraphicsBackend.BindShaderVariable(shaderParams.LightColor, ref lightColor);
			var Pos = camera.GameObj.Transform.Pos;
			DualityApp.GraphicsBackend.BindShaderVariable(shaderParams.CameraPosition, ref Pos);

			var lightDirWS = light.GameObj.Transform.Forward;

			DualityApp.GraphicsBackend.BindShaderVariable(shaderParams.LightDirection, ref lightDirWS);

            var inverseViewProjectionMatrix = Matrix4.Invert(view * projection);
			DualityApp.GraphicsBackend.BindShaderVariable(shaderParams.InvViewProjection, ref inverseViewProjectionMatrix);

			DualityApp.GraphicsBackend.DrawMesh(_quadMesh.MeshHandle);

			DualityApp.GraphicsBackend.EndInstance();
        }

        private void RenderPointLight(Duality.Components.Camera camera, ref Matrix4 view, ref Matrix4 projection, Components.LightComponent light)
        {
            if (light.Type != LighType.PointLight)
                return;
            
            var renderStateId = _directionalRenderState;

            var viewProjection = view * projection;
            var modelViewProjection = viewProjection;

            // Convert light color to linear space
            var lightColor = light.Color * light.Intensity;

            // Select the correct shader
            var lightTypeOffset = 2;

            var shader = _lightShaders[lightTypeOffset];
            var shaderParams = _lightParams[lightTypeOffset];

            // Setup textures and begin rendering with the chosen shader
            _lightTextureBinds[0] = _gbuffer.Textures[0].Handle;
            _lightTextureBinds[1] = _gbuffer.Textures[1].Handle;
            _lightTextureBinds[2] = _gbuffer.Textures[2].Handle;
            _lightTextureBinds[4] = _gbuffer.Textures[3].Handle;

            _lightSamplers[0] = DualityApp.GraphicsBackend.DefaultSamplerNoFiltering;
            _lightSamplers[1] = DualityApp.GraphicsBackend.DefaultSamplerNoFiltering;
            _lightSamplers[2] = DualityApp.GraphicsBackend.DefaultSamplerNoFiltering;
            _lightSamplers[4] = DualityApp.GraphicsBackend.DefaultSamplerNoFiltering;

			DualityApp.GraphicsBackend.BeginInstance(shader.Handle, new int[] { _gbuffer.Textures[0].Handle, _gbuffer.Textures[1].Handle, _gbuffer.Textures[2].Handle, _gbuffer.Textures[3].Handle }, _lightSamplers, renderStateId);

            // Setup texture samplers
            DualityApp.GraphicsBackend.BindShaderVariable(shaderParams.SamplerGBuffer0, 0);
            DualityApp.GraphicsBackend.BindShaderVariable(shaderParams.SamplerGBuffer1, 1);
            DualityApp.GraphicsBackend.BindShaderVariable(shaderParams.SamplerGBuffer2, 2);
            DualityApp.GraphicsBackend.BindShaderVariable(shaderParams.SamplerGBuffer3, 3);
            DualityApp.GraphicsBackend.BindShaderVariable(shaderParams.SamplerDepth, 4);

            // Common uniforms
            DualityApp.GraphicsBackend.BindShaderVariable(shaderParams.ScreenSize, ref _screenSize);
            DualityApp.GraphicsBackend.BindShaderVariable(shaderParams.ModelViewProjection, ref modelViewProjection);
			DualityApp.GraphicsBackend.BindShaderVariable(shaderParams.LightColor, ref lightColor);
			var Pos = camera.GameObj.Transform.Pos;
			DualityApp.GraphicsBackend.BindShaderVariable(shaderParams.CameraPosition, ref Pos);
			var lPos = light.GameObj.Transform.Pos;
			DualityApp.GraphicsBackend.BindShaderVariable(shaderParams.LightPosition, ref lPos);
			DualityApp.GraphicsBackend.BindShaderVariable(shaderParams.LightRange, light.Range);

            var inverseViewProjectionMatrix = Matrix4.Invert(view * projection);
			DualityApp.GraphicsBackend.BindShaderVariable(shaderParams.InvViewProjection, ref inverseViewProjectionMatrix);

			DualityApp.GraphicsBackend.DrawMesh(_quadMesh.MeshHandle);

			DualityApp.GraphicsBackend.EndInstance();
        }
    }
}
