﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;
using OGL = OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Duality.Renderer.Meshes;
using Duality;
using Duality.Backend;

namespace Duality.Renderer
{
    public class ContextReference
    {
        public Action SwapBuffers;
        public OpenTK.Graphics.IGraphicsContext Context;
    }

    /// <summary>
    /// Core render system, a thin wrapper over OpenGL with some basic resource management functions.
    /// 
    /// All resources are exposed as integer handles, invalid or expired (unloaded) handles can be used 
    /// without any issues. If anything is wrong with the handle then a default resource will be used instead.
    /// 
    /// A single OpenGL context is owned and managed by this class, resource transfer to opengl is done in a seperate work 
    /// queue exposed through a single addToWorkQueue method, sent to the class in the constructor.
    /// Important! The work queue has to be processed on the same thread that created the RenderSystem instance. Ie the thread
    /// that owns the OpenGL context.
    /// 
    /// The work queue is only used so that the resource loading functions can be called from a background loading thread and so that 
    /// the opengl upload is done on the correct thread.
    /// 
    /// </summary>
    public class RenderSystem : IDisposable
    {
        private readonly Textures.TextureManager _textureManager;
        private readonly Meshes.MeshManager _meshManager;
        private readonly Meshes.BufferManager _bufferManager;
        private readonly Shaders.ShaderManager _shaderManager;
        private readonly RenderTargets.RenderTargetManager _renderTargetManager;
        private readonly RenderStates.RenderStateManager _renderStateManager;
        private readonly Samplers.SamplerManager _samplerManager;

        private DebugProc _debugProcCallback;

        private bool _disposed = false;
        private readonly Action<Action> _addToWorkQueue;

        public delegate void OnLoadedCallback(int handle, bool success, string errors);

        public RenderSystem(Action<Action> addToWorkQueue)
        {
            _addToWorkQueue = addToWorkQueue ?? throw new ArgumentNullException(nameof(addToWorkQueue));

            var graphicsMode = new GraphicsMode(32, 24, 0, 0);


            var major = GL.GetInteger(GetPName.MajorVersion);
            var minor = GL.GetInteger(GetPName.MinorVersion);

            GLWrapper.Initialize();

            _textureManager = new Textures.TextureManager();
            _bufferManager = new Meshes.BufferManager();
            _meshManager = new Meshes.MeshManager(_bufferManager);
            _shaderManager = new Shaders.ShaderManager();
            _renderTargetManager = new RenderTargets.RenderTargetManager();
            _renderStateManager = new RenderStates.RenderStateManager();
            _samplerManager = new Samplers.SamplerManager();

            var initialRenderState = _renderStateManager.CreateRenderState(false, true, true, BlendingFactorSrc.Zero, BlendingFactorDest.One, CullFaceMode.Back, true, DepthFunction.Less);
            _renderStateManager.ApplyRenderState(initialRenderState);

            GL.FrontFace(FrontFaceDirection.Ccw);
            GL.Enable(EnableCap.TextureCubeMapSeamless);

            _debugProcCallback = DebugCallback;
            GL.DebugMessageCallback(_debugProcCallback, IntPtr.Zero);
            GL.Enable(EnableCap.DebugOutput);

            GL.PatchParameter(PatchParameterInt.PatchVertices, 3);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (!isDisposing || _disposed)
                return;

            _samplerManager.Dispose();
            _meshManager.Dispose();
            _bufferManager.Dispose();
            _renderTargetManager.Dispose();

            _disposed = true;
        }

        private void DebugCallback(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam)
        {
            if (severity == DebugSeverity.DebugSeverityLow || severity == DebugSeverity.DebugSeverityMedium || severity == DebugSeverity.DebugSeverityHigh)
            {
                var msg = Marshal.PtrToStringAnsi(message, length);
                //Log.Information("GL Debug Callback: {@Message}", new { severity, type, msg });
            }
        }

        public void BindTexture(int handle, int textureUnit)
        {
            _textureManager.Bind(textureUnit, handle);
		}

        public void GenreateMips(int handle)
        {
            //OGL.TextureTarget target;
            //var openGLHandle = _textureManager.GetOpenGLHande(handle, out target);

            if (GLWrapper.ExtDirectStateAccess)
            {
                GL.Ext.GenerateTextureMipmap(handle, OGL.TextureTarget.Texture2D);
            }
            else
            {
                var current = _textureManager.GetActiveTexture(0);

                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(OGL.TextureTarget.Texture2D, handle);
                GL.GenerateMipmap((OGL.GenerateMipmapTarget)(int)OGL.TextureTarget.Texture2D);

                GL.BindTexture(OGL.TextureTarget.Texture2D, current);
            }
        }

        public int CreateBuffer(BufferTarget target, bool mutable, VertexFormat vertexFormat = null)
        {
            return _bufferManager.Create(target, mutable, vertexFormat);
        }

        public void DestroyBuffer(int handle)
        {
            _addToWorkQueue(() => _bufferManager.Destroy(handle));
        }

        public void SetBufferData<T>(int handle, T[] data, bool stream, bool async)
            where T : struct
        {
            if (async)
            {
                _addToWorkQueue(() =>
                {
                    _bufferManager.SetData(handle, data, stream);
                });
            }
            else
            {
                _bufferManager.SetData(handle, data, stream);
            }
        }

        public void SetBufferDataDirect(int handle, IntPtr length, IntPtr data, bool stream)
        {
            _bufferManager.SetDataDirect(handle, length, data, stream);
        }

        public int CreateMesh(int triangleCount, int vertexBuffer, int indexBuffer, bool async, IndexType indexType = IndexType.UnsignedInt)
        {
            var handle = _meshManager.Create();

            if (async)
            {
                _addToWorkQueue(() =>
                {
                    _meshManager.Initialize(handle, triangleCount, vertexBuffer, indexBuffer, indexType);
                });
            }
            else
            {
                _meshManager.Initialize(handle, triangleCount, vertexBuffer, indexBuffer, indexType);
            }

            return handle;
        }

        public void DestroyMesh(int handle)
        {
            _addToWorkQueue(() =>
            {
                _meshManager.Destroy(handle);
            });
        }

        public void SetMeshDataDirect(int handle, int triangleCount, IntPtr vertexDataLength, IntPtr indexDataLength, IntPtr vertexData, IntPtr indexData, bool stream)
        {
            int vertexBufferId, indexBufferId;
            _meshManager.GetMeshData(handle, out vertexBufferId, out indexBufferId);

            _bufferManager.SetDataDirect(vertexBufferId, vertexDataLength, vertexData, stream);
            _bufferManager.SetDataDirect(indexBufferId, indexDataLength, indexData, stream);

            _meshManager.SetTriangleCount(handle, triangleCount);
        }

        public void MeshSetTriangleCount(int handle, int triangleCount, bool queue)
        {
            if (queue)
            {
                _addToWorkQueue(() => _meshManager.SetTriangleCount(handle, triangleCount));
            }
            else
            {
                _meshManager.SetTriangleCount(handle, triangleCount);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RenderMesh(int handle, PrimitiveType primitiveType)
        {
            _meshManager.Render(handle, primitiveType);
        }

        public unsafe void RenderMesh(DrawMeshMultiData* meshIndices, int count)
        {
            _meshManager.Render(meshIndices, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RenderMesh(int handle, PrimitiveType primitiveType, int offset, int count)
        {
            _meshManager.Render(handle, primitiveType, offset, count);
        }

        public void BeginScene(int renderTargetHandle, int width, int height)
        {
            _renderStateManager.ApplyRenderState(0);
            BindRenderTarget(renderTargetHandle);
            GL.Viewport(0, 0, width, height);
        }

        public void BeginScene(int renderTargetHandle, int x, int y, int width, int height)
        {
            _renderStateManager.ApplyRenderState(0);
            BindRenderTarget(renderTargetHandle);
            GL.Viewport(x, y, width, height);
        }

        public void BindShader(int handle)
        {
            _shaderManager.Bind(handle);
        }

        public void DispatchCompute(int numGroupsX, int numGroupsY, int numGroupsZ)
        {
            GL.DispatchCompute(numGroupsX, numGroupsY, numGroupsZ);
        }

        public void SetUniformFloat(int handle, float value)
        {
            GL.Uniform1(handle, value);
        }

        public void SetUniformInt(int handle, int value)
        {
            GL.Uniform1(handle, value);
        }

        public unsafe void SetUniformVector2(int handle, int count, float* value)
        {
            GL.Uniform2(handle, count, value);
        }

        public unsafe void SetUniformVector2u(int handle, int count, uint* value)
        {
            GL.Uniform2(handle, count, value);
        }

        public unsafe void SetUniformMatrix4(int handle, int count, float* value)
        {
            GL.UniformMatrix4(handle, count, false, value);
        }

        public unsafe void SetUniformVector3(int handle, int count, float* value)
        {
            GL.Uniform3(handle, count, value);
        }

        public unsafe void SetUniformVector4(int handle, int count, float* value)
        {
            GL.Uniform4(handle, count, value);
        }

        public unsafe void SetUniformInt(int handle, int count, int* value)
        {
            GL.Uniform1(handle, count, value);
        }

        public unsafe void SetUniformFloat(int handle, int count, float* value)
        {
            GL.Uniform1(handle, count, value);
        }

        public void BindImageTexture(int unit, int texture, TextureAccess access, SizedInternalFormat format)
        {
            //var glHandle = _textureManager.GetOpenGLHande(texture);
            GL.BindImageTexture(unit, texture, 0, false, 0, access, format);
        }

        public void BindBufferBase(int index, int handle)
        {
            _bufferManager.GetOpenGLHandle(handle, out var buffer, out var target);
            GL.BindBufferBase((BufferRangeTarget)(int)target, index, buffer);
        }

        public void BindBufferRange(int index, int handle, IntPtr offset, IntPtr size)
        {
            _bufferManager.GetOpenGLHandle(handle, out var buffer, out var target);
            GL.BindBufferRange((BufferRangeTarget)(int)target, index, buffer, offset, size);
        }

        public void Clear(Vector4 clearColor, ClearFlags flags)
        {
            GL.ClearColor(clearColor.X, clearColor.Y, clearColor.Z, clearColor.W);

            ClearBufferMask mask = 0;
            if ((flags & ClearFlags.Color) == ClearFlags.Color)
            {
                mask |= ClearBufferMask.ColorBufferBit;
            }

            if ((flags & ClearFlags.Depth) == ClearFlags.Depth)
            {
                mask |= ClearBufferMask.DepthBufferBit;
            }

            GL.Clear(mask);
        }

        public int CreateRenderState(bool enableAlphaBlend = false, bool enableDepthWrite = true, bool enableDepthTest = true, BlendingFactorSrc src = BlendingFactorSrc.Zero, BlendingFactorDest dest = BlendingFactorDest.One, CullFaceMode cullFaceMode = CullFaceMode.Back, bool enableCullFace = true, DepthFunction depthFunction = DepthFunction.Less, bool wireFrame = false, bool scissorTest = false)
        {
            return _renderStateManager.CreateRenderState(enableAlphaBlend, enableDepthWrite, enableDepthTest, src, dest, cullFaceMode, enableCullFace, depthFunction, wireFrame, scissorTest);
        }

        public void SetRenderState(int id)
        {
            _renderStateManager.ApplyRenderState(id);
        }

        public int CreateRenderTarget(RenderTargets.Definition definition, out Duality.Resources.Texture[] textureHandles, OnLoadedCallback loadedCallback)
        {
            var internalTextureHandles = new List<Duality.Resources.Texture>();

            foreach (var attachment in definition.Attachments)
            {
                if (attachment.AttachmentPoint == RenderTargets.Definition.AttachmentPoint.Color || definition.RenderDepthToTexture)
                {
					var tex = new Duality.Resources.Texture(definition.Width, definition.Height, format: ToOpenTKPixelFormat(attachment.PixelInternalFormat));
                    attachment.TextureHandle = tex.Handle;
					internalTextureHandles.Add(tex);
				}
            }

            textureHandles = internalTextureHandles.ToArray();

            var renderTargetHandle = _renderTargetManager.Create();

            _addToWorkQueue(() =>
            {
                foreach (var attachment in definition.Attachments)
                {
                    if (attachment.AttachmentPoint == RenderTargets.Definition.AttachmentPoint.Color || definition.RenderDepthToTexture)
                    {
                        var target = TextureTarget.Texture2D;
				
                        if (definition.RenderToCubeMap)
                        {
                            target = TextureTarget.TextureCubeMap;
                        }

						// Upload texture data to OpenGL
						//GL.BindTexture(target, attachment.TextureHandle);
						//
						//if (target == OGL.TextureTarget.TextureCubeMap)
						//{
						//	for (var i = 0; i < 6; i++)
						//	{
						//		GL.TexImage2D(OGL.TextureTarget.TextureCubeMapPositiveX + i, 0, (OGL.PixelInternalFormat)(int)attachment.PixelInternalFormat, definition.Width, definition.Height, 0, (OGL.PixelFormat)(int)attachment.PixelFormat, (OGL.PixelType)(int)attachment.PixelType, default(byte[]));
						//	}
						//	if (attachment.MipMaps)
						//		GL.GenerateMipmap(GenerateMipmapTarget.TextureCubeMap);
						//}
						//else
						//{
						//	GL.TexImage2D(target, 0, (OGL.PixelInternalFormat)(int)attachment.PixelInternalFormat, definition.Width, definition.Height, 0, (OGL.PixelFormat)(int)attachment.PixelFormat, (OGL.PixelType)(int)attachment.PixelType, default(byte[]));
						//	if (attachment.MipMaps)
						//		GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
						//
						//	GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
						//	GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
						//}
						_textureManager.SetPixelData<byte>(attachment.TextureHandle, target, definition.Width, definition.Height, null, attachment.PixelFormat, attachment.PixelInternalFormat, attachment.PixelType, attachment.MipMaps);
                    }
                }

                // Init render target
                _renderTargetManager.Init(renderTargetHandle, definition);

                loadedCallback?.Invoke(renderTargetHandle, true, "");
            });

            return renderTargetHandle;
		}
		private Drawing.TexturePixelFormat ToOpenTKPixelFormat(PixelInternalFormat format)
		{
			switch (format)
			{
				case PixelInternalFormat.R8: return Drawing.TexturePixelFormat.Single;
				case PixelInternalFormat.Rg8: return Drawing.TexturePixelFormat.Dual;
				case PixelInternalFormat.Rgb: return Drawing.TexturePixelFormat.Rgb;
				case PixelInternalFormat.Rgba: return Drawing.TexturePixelFormat.Rgba;

				case PixelInternalFormat.R16f: return Drawing.TexturePixelFormat.FloatSingle;
				case PixelInternalFormat.Rg16f: return Drawing.TexturePixelFormat.FloatDual;
				case PixelInternalFormat.Rgb16f: return Drawing.TexturePixelFormat.FloatRgb;
				case PixelInternalFormat.Rgba16f: return Drawing.TexturePixelFormat.FloatRgba;

				case PixelInternalFormat.CompressedRed: return Drawing.TexturePixelFormat.CompressedSingle;
				case PixelInternalFormat.CompressedRg: return Drawing.TexturePixelFormat.CompressedDual;
				case PixelInternalFormat.CompressedRgb: return Drawing.TexturePixelFormat.CompressedRgb;
				case PixelInternalFormat.CompressedRgba: return Drawing.TexturePixelFormat.CompressedRgba;
			}

			return Drawing.TexturePixelFormat.Rgba;
		}

		public void ResizeRenderTarget(int handle, int width, int height)
        {
            _addToWorkQueue(() =>
            {
                _renderTargetManager.Resize(handle, width, height);

                // Resize textures
                var definition = _renderTargetManager.GetDefinition(handle);

                foreach (var attachment in definition.Attachments)
                {
                    if (attachment.AttachmentPoint == RenderTargets.Definition.AttachmentPoint.Color || definition.RenderDepthToTexture)
                    {
                        var target = TextureTarget.Texture2D;

                        if (definition.RenderToCubeMap)
                        {
                            target = TextureTarget.TextureCubeMap;
                        }

                        GL.BindTexture((OGL.TextureTarget)(int)target, attachment.TextureHandle);

                        if (target == TextureTarget.TextureCubeMap)
                        {
                            for (var i = 0; i < 6; i++)
                            {
                                GL.TexImage2D(OGL.TextureTarget.TextureCubeMapPositiveX + i, 0, (OGL.PixelInternalFormat)(int)attachment.PixelInternalFormat, width, height, 0, (OGL.PixelFormat)(int)attachment.PixelFormat, (OGL.PixelType)(int)attachment.PixelFormat, IntPtr.Zero);
                            }
                        }
                        else
                        {
                            GL.TexImage2D((OGL.TextureTarget)(int)target, 0, (OGL.PixelInternalFormat)(int)attachment.PixelInternalFormat, width, height, 0, (OGL.PixelFormat)(int)attachment.PixelFormat, (OGL.PixelType)(int)attachment.PixelType, IntPtr.Zero);
                        }
                    }
                }
            });
        }

        public void DestroyRenderTarget(int handle)
        {
            _addToWorkQueue(() => _renderTargetManager.Destroy(handle));
        }

        public void BindRenderTarget(int handle)
        {
            DrawBuffersEnum[] drawBuffers;
            var openGLHandle = _renderTargetManager.GetOpenGLHande(handle, out drawBuffers);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, openGLHandle);

            if (drawBuffers == null)
            {
                GL.DrawBuffer(DrawBufferMode.Back);
                GL.ColorMask(true, true, true, true);
            }
            else if (drawBuffers.Length == 1 && drawBuffers[0] == DrawBuffersEnum.None)
            {
                GL.ColorMask(false, false, false, false);
            }
            else
            {
                GL.DrawBuffers(drawBuffers.Length, drawBuffers);
                GL.ColorMask(true, true, true, true);
            }
        }

        public int CreateSampler(Dictionary<SamplerParameterName, int> settings)
        {
            var handle = _samplerManager.Create();

            _addToWorkQueue(() =>
            {
                _samplerManager.Init(handle, settings);
            });

            return handle;
        }

        public void BindSampler(int textureUnit, int handle)
        {
            _samplerManager.Bind(textureUnit, handle);
        }

        public void SetWireFrameEnabled(bool enabled)
        {
            GL.PolygonMode(MaterialFace.FrontAndBack, enabled ? PolygonMode.Line : PolygonMode.Fill);
        }

        public void Scissor(bool enable, int x, int y, int w, int h)
        {
            if (enable)
            {
                GL.Enable(EnableCap.ScissorTest);
                GL.Scissor(x, y, w, h);
            }
            else
            {
                GL.Disable(EnableCap.ScissorTest);
            }
        }

        public void ReadTexture<T>(int handle, PixelFormat format, PixelType type, ref T pixels) where T : struct
        {
            //OGL.TextureTarget target;
            //var glHandle = _textureManager.GetOpenGLHande(handle, out target);
            GL.Ext.GetTextureImage<T>(handle, OGL.TextureTarget.Texture2D, 0, (OGL.PixelFormat)format, (OGL.PixelType)type, ref pixels);
        }

        public void ReadTexture<T>(int handle, PixelFormat format, PixelType type, T[] pixels) where T : struct
        {
            //OGL.TextureTarget target;
            //var glHandle = _textureManager.GetOpenGLHande(handle, out target);
            GL.Ext.GetTextureImage<T>(handle, OGL.TextureTarget.Texture2D, 0, (OGL.PixelFormat)format, (OGL.PixelType)type, pixels);
        }
    }
}
