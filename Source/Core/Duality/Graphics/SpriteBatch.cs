using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Duality.Renderer;
using Duality.Resources;

namespace Duality.Graphics
{
    [Flags]
    public enum SpriteFlags
    {
        None = 0x0,
        Srgb = 0x01,
        DistanceField = 0x02,
        AlphaBlend = 0x04
    }

    public class SpriteBatch
    {
        private readonly BatchBuffer _buffer;
        private readonly DrawTechnique _shader;
        private ShaderParams _params;
        private readonly List<QuadInfo> _quads = new List<QuadInfo>();
        private int _lastQuad = 0;

        private readonly int _renderStateAlphaBlend;
        private readonly int _renderStateNoAlphaBlend;

        private Vector4 _shaderSettings = new Vector4(0, 0, 1.0f / 16.0f, 0);
        private readonly int[] _samplers = new int[] { 0 };

        private readonly int _samplerDistanceField;

        /// <summary>
        /// Should always be created through Backend.CreateSpriteBatch
        /// </summary>
        internal SpriteBatch(Renderer.RenderSystem renderSystem)
        {
            if (DualityApp.GraphicsBackend == null)
                throw new ArgumentNullException("backend");
            if (renderSystem == null)
                throw new ArgumentNullException("renderSystem");

            _buffer = new BatchBuffer(renderSystem, new Renderer.VertexFormat(new Renderer.VertexFormatElement[]
                {
                    new Renderer.VertexFormatElement(Renderer.VertexFormatSemantic.Position, Renderer.VertexPointerType.Float, 3, 0),
                    new Renderer.VertexFormatElement(Renderer.VertexFormatSemantic.TexCoord, Renderer.VertexPointerType.Float, 2, sizeof(float) * 3),
                    new Renderer.VertexFormatElement(Renderer.VertexFormatSemantic.Color, Renderer.VertexPointerType.Float, 4, sizeof(float) * 5),
                }), 32);

            //_shader = resourceManager.Load<Duality.Resources.Shader>("/shaders/sprite");
            //_shader = ContentProvider.RequestContent<Duality.Resources.Shader>("shaders/sprite").Res;
			_shader = new DrawTechnique(Shader.LoadEmbeddedShaderSource("shaders/sprite.glsl"), "");

			_quads = new List<QuadInfo>();
            for (var i = 0; i < 32; i++)
            {
                _quads.Add(new QuadInfo());
            }

            _renderStateAlphaBlend = DualityApp.GraphicsBackend.CreateRenderState(true, false, false, Renderer.BlendingFactorSrc.SrcAlpha, Renderer.BlendingFactorDest.OneMinusSrcAlpha, Renderer.CullFaceMode.Front);
            _renderStateNoAlphaBlend = DualityApp.GraphicsBackend.CreateRenderState(false, false, false, Renderer.BlendingFactorSrc.Zero, Renderer.BlendingFactorDest.One, Renderer.CullFaceMode.Front);

            _samplerDistanceField = DualityApp.GraphicsBackend.RenderSystem.CreateSampler(new Dictionary<SamplerParameterName, int>
            {
                { SamplerParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapNearest },
                { SamplerParameterName.TextureMagFilter, (int)TextureMagFilter.Linear }
            });
        }

        public void RenderQuad(Duality.Resources.Texture texture, Vector2 position)
        {
            RenderQuad(texture, position, new Vector2(texture.Width, texture.Height), Vector4.One);
        }

        public void RenderQuad(Duality.Resources.Texture texture, Vector2 position, Vector4 color)
        {
            RenderQuad(texture, position, new Vector2(texture.Width, texture.Height), color);
        }

        public void RenderQuad(Duality.Resources.Texture texture, Vector2 position, Vector2 size)
        {
            RenderQuad(texture, position, size, Vector4.One);
        }

        public void RenderQuad(Duality.Resources.Texture texture, Vector2 position, Vector2 size, Vector4 color)
        {
            RenderQuad(texture, position, size, Vector2.Zero, Vector2.One, color);
        }

        public void RenderQuad(Duality.Resources.Texture texture, Vector2 position, Vector2 size, Vector2 uvPosition, Vector2 uvSize)
        {
            RenderQuad(texture, position, size, uvPosition, uvSize, Vector4.One);
        }

        public void RenderQuad(Duality.Resources.Texture texture, Vector2 position, Vector2 size, Vector2 uvPosition, Vector2 uvSize, Vector4 color, SpriteFlags flags = SpriteFlags.AlphaBlend, float smoothing = 1.0f / 16.0f)
        {
            if (_lastQuad == _quads.Count)
            {
                var quadsToCreate = _quads.Count / 2;
                for (var i = 0; i < quadsToCreate; i++)
                {
                    _quads.Add(new QuadInfo());
                }
            }

            var quad = _quads[_lastQuad++];
            quad.Init(texture, position, size, uvPosition, uvSize, color, flags, smoothing);
        }

        public void Render(int width, int height)
        {
            if (_params == null)
            {
                _params = new ShaderParams();
                _shader.BindUniformLocations(_params);
            }

            if (_lastQuad == 0) // Bail out
                return;

			Duality.Resources.Texture lastTexture = null;
            var lastFlags = SpriteFlags.None;
            var lastSmoothing = 0.0f;

            Matrix4.CreateOrthographicOffCenter(0.0f, width, height, 0.0f, -1.0f, 1.0f, out Matrix4 projectionMatrix);

            for (var i = 0; i < _lastQuad; i++)
            {
                var quad = _quads[i];
                if (lastTexture != quad.Texture || lastFlags != quad.Flags || lastSmoothing != quad.Smoothing)
                {
                    if (lastTexture != null)
                    {
                        _buffer.EndInline();
                        SubmitBatch(lastTexture, lastFlags, lastSmoothing, ref projectionMatrix);
                    }

                    _buffer.Begin();
                    lastFlags = quad.Flags;
                    lastTexture = quad.Texture;
                    lastSmoothing = quad.Smoothing;
                }

                _buffer.AddQuadInverseUV(quad.Position, quad.Size, quad.UvPositon, quad.UvSize, quad.Color);
            }

            _buffer.EndInline();
            SubmitBatch(lastTexture, lastFlags, lastSmoothing, ref projectionMatrix);

            _lastQuad = 0;
        }

        void SubmitBatch(Duality.Resources.Texture texture, SpriteFlags flags, float smoothing, ref Matrix4 projectionMatrix)
        {
            var alphaBlend = (flags & SpriteFlags.AlphaBlend) == SpriteFlags.AlphaBlend;

            if ((flags & SpriteFlags.DistanceField) == SpriteFlags.DistanceField)
            {
                _samplers[0] = _samplerDistanceField;
                _shaderSettings.Y = 1;
            }
            else
            {
                _samplers[0] = DualityApp.GraphicsBackend.DefaultSamplerNoFiltering;
                _shaderSettings.Y = 0;
            }

            _shaderSettings.X = (flags & SpriteFlags.Srgb) == SpriteFlags.Srgb ? 1 : 0;
            _shaderSettings.Z = smoothing;

            var renderStateId = alphaBlend ? _renderStateAlphaBlend : _renderStateNoAlphaBlend;

            DualityApp.GraphicsBackend.BeginInstance(_shader.Handle, new int[] { texture.Handle }, _samplers, renderStateId);
            DualityApp.GraphicsBackend.BindShaderVariable(_params.HandleDiffuseTexture, 0);
            DualityApp.GraphicsBackend.BindShaderVariable(_params.Settings, ref _shaderSettings);
            DualityApp.GraphicsBackend.BindShaderVariable(_params.HandleModelViewProjection, ref projectionMatrix);
            DualityApp.GraphicsBackend.DrawMesh(_buffer.MeshHandle);
			DualityApp.GraphicsBackend.EndInstance();
        }

        class ShaderParams
        {
            public int HandleModelViewProjection = 0;
            public int HandleDiffuseTexture = 0;
            public int Settings = 0;
        }

        class QuadInfo
        {
            public void Init(Duality.Resources.Texture texture, Vector2 position, Vector2 size, Vector2 uvPosition, Vector2 uvSize, Vector4 color, SpriteFlags flags, float smoothing)
            {
                Texture = texture;
                Position = position;
                Size = size;
                UvPositon = uvPosition;
                UvSize = uvSize;
                Color = color;
                Flags = flags;
                Smoothing = smoothing;
            }

            public Duality.Resources.Texture Texture;
            public Vector2 Position;
            public Vector2 Size;
            public Vector2 UvPositon;
            public Vector2 UvSize;
            public Vector4 Color;
            public SpriteFlags Flags;
            public float Smoothing = 1.0f / 16.0f;
        }
    }
}
