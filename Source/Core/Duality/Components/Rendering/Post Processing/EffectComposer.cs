using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using THREE.Math;
using THREE.Renderers;
using THREE.Shaders;
using THREE.Textures;

namespace Duality.Postprocessing
{
    public class EffectComposer
    {
        public GLRenderTarget RenderTarget1;

        public GLRenderTarget RenderTarget2;

        public GLRenderTarget WriteBuffer;

		public GLRenderTarget ReadBuffer;

		public GLRenderTarget DepthBuffer;
		public GLRenderTarget NormalBuffer;

		public List<Pass> Passes = new List<Pass>();

        public ShaderPass CopyPass;
        public ToScreenPass ToScreenPass;

		internal float pixelRatio;

		internal int width;
		internal int height;

        private CopyShader copyShader = new CopyShader();

        public EffectComposer(GLRenderTarget renderTarget = null)
        {

            if(renderTarget==null)
            {
				// Use Graphics backend as Target
                var size = DualityApp.GraphicsBackend.GetSize();
				width = (int)size.X;
				height = (int)size.Y;

				pixelRatio = DualityApp.GraphicsBackend.GetPixelRatio();

				Hashtable parameters = new Hashtable();
				parameters.Add("minFilter", THREE.Constants.LinearFilter);
				parameters.Add("magFilter", THREE.Constants.LinearFilter);
				parameters.Add("format", THREE.Constants.RGBAFormat);

				renderTarget = new GLRenderTarget((int)(width * pixelRatio), (int)(height * pixelRatio), parameters);
				renderTarget.Texture.Name = "EffectComposer.rt1";
			}
            else
            {
                this.pixelRatio = 1;
                this.width = renderTarget.Width;
                this.height = renderTarget.Height;
            }

            this.RenderTarget1 = renderTarget;
            this.RenderTarget2 = (GLRenderTarget)renderTarget.Clone();
            this.RenderTarget2.Texture.Name = "EffectComposer.rt2";

            this.WriteBuffer = this.RenderTarget1;
            this.ReadBuffer = this.RenderTarget2;

            CopyPass = new ShaderPass(copyShader);
			CopyPass.composer = this;
			ToScreenPass = new ToScreenPass();
			ToScreenPass.composer = this;
		}

        public void SwapBuffers()
        {
            var tmp = this.ReadBuffer;
            this.ReadBuffer = this.WriteBuffer;
            this.WriteBuffer = tmp;
        }

        public void AddPass(Pass pass)
        {
			this.Passes.Add(pass);
            pass.SetSize(width * pixelRatio, height * pixelRatio);
			pass.composer = this;
		}

        public void InsertPass(Pass pass,int index)
        {
            //this.Passes.Splice(index, 0, pass);
            Passes.Insert(index, pass);
            pass.SetSize(this.width * this.pixelRatio, this.height * this.pixelRatio);
			pass.composer = this;
		}

        public bool IsLastEnabledPass(int passIndex)
        {
            for (var i = passIndex + 1; i < this.Passes.Count; i++)
                if (this.Passes[i].Enabled)
                    return false;
            return true;
        }

        public void Render(THREE.Scenes.Scene scene, THREE.Cameras.Camera camera)
        {
            var currentRenderTarget = DualityApp.GraphicsBackend.GetRenderTarget();

            var maskActive = false;

            Pass pass; 
            int il = this.Passes.Count;

            for (int i = 0; i < il; i++)
            {
                pass = this.Passes[i];

                if (pass.Enabled == false) continue;

				pass.camera = camera;
				pass.scene = scene;

				DualityApp.GraphicsBackend.SetRenderTarget(this.WriteBuffer);
				pass.Render(this.WriteBuffer, this.ReadBuffer, maskActive);

                if (pass.NeedsSwap)
                {
                    if (maskActive)
                    {
                        var stencil = DualityApp.GraphicsBackend.state.buffers.stencil;
                        unchecked
                        {
                            stencil.SetFunc(THREE.Constants.NotEqualStencilFunc, 1, (int)0xffffffff);
                            this.CopyPass.Render(this.WriteBuffer, this.ReadBuffer);
                            stencil.SetFunc(THREE.Constants.EqualStencilFunc, 1,(int)0xffffffff);
                        }
                    }
                    this.SwapBuffers();
                }              

                if (pass is MaskPass ) 
                {
                    maskActive = true;
                } 
                else if (pass is ClearMaskPass ) 
                {
                    maskActive = false;
                }
			}


			ToScreenPass.camera = camera;
			ToScreenPass.scene = scene;
			this.ToScreenPass.Render(this.WriteBuffer, this.ReadBuffer);

			DualityApp.GraphicsBackend.SetRenderTarget(currentRenderTarget );
        }

        public void Reset(GLRenderTarget renderTarget=null)
        {
            if (renderTarget == null)
            {

                var size = DualityApp.GraphicsBackend.GetSize(new Vector2());
                this.pixelRatio = DualityApp.GraphicsBackend.GetPixelRatio();
                this.width = (int)size.X;
                this.height = (int)size.Y;

				Hashtable parameters = new Hashtable();
				parameters.Add("minFilter", THREE.Constants.LinearFilter);
				parameters.Add("magFilter", THREE.Constants.LinearFilter);
				parameters.Add("format", THREE.Constants.RGBAFormat);
				renderTarget = new GLRenderTarget((int)(width * pixelRatio), (int)(height * pixelRatio), parameters);
				//renderTarget = (GLRenderTarget)this.RenderTarget1.Clone();
                //renderTarget.SetSize((int)(this.width * this.pixelRatio), (int)(this.height * this.pixelRatio));
				renderTarget.Texture.Name = "EffectComposer.rt1";

			}

            this.RenderTarget1.Dispose();
            this.RenderTarget2.Dispose();
            this.RenderTarget1 = renderTarget;
            this.RenderTarget2 = (GLRenderTarget)renderTarget.Clone();

            this.WriteBuffer = this.RenderTarget1;
            this.ReadBuffer = this.RenderTarget2;
        }

        public void SetSize(int width, int height)
        {
            this.width = width;
            this.height = height;

            var effectiveWidth = this.width * this.pixelRatio;
            var effectiveHeight = this.height * this.pixelRatio;

            this.RenderTarget1.SetSize((int)effectiveWidth, (int)effectiveHeight);
            this.RenderTarget2.SetSize((int)effectiveWidth, (int)effectiveHeight);

            for (var i = 0; i < this.Passes.Count; i++)
            {
                this.Passes[i].SetSize(effectiveWidth, effectiveHeight);
            }
        }

        public void SetPixelRatio(float pixelRatio)
        {
            this.pixelRatio = pixelRatio;

            this.SetSize(this.width, this.height);
        }

        public void SetPixelRatioAndSize(int width, int height, float pixelRatio)
        {
            this.pixelRatio = pixelRatio;
            this.width = width;
            this.height = height;

            this.Reset();
        }
    }
}
