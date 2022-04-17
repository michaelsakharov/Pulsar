using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using THREE.Cameras;
using THREE.Materials;
using THREE.Math;
using THREE.Renderers;
using THREE.Scenes;

namespace Duality.Postprocessing
{
    public class RenderPass : Pass
    {
        public Material OverrideMaterial;

        public Color? ClearColor;

        public float ClearAlpha;

        public bool ClearDepth;

		private MeshDepthMaterial materialDepth;
		private MeshNormalMaterial normalMaterial;

		public RenderPass(Material overrideMaterial=null,Color? clearColor=null,float? clearAlpha=null)
        {
            this.OverrideMaterial = overrideMaterial;

            this.ClearColor = clearColor;
            if (clearAlpha == null)
                this.ClearAlpha = 1.0f;
            else 
                this.ClearAlpha = clearAlpha.Value;

            this.Clear = true;
            this.ClearDepth = false;
            this.NeedsSwap = false;

			// depth material

			this.materialDepth = new MeshDepthMaterial();
			this.materialDepth.DepthPacking = THREE.Constants.RGBADepthPacking;
			this.materialDepth.Blending = THREE.Constants.NoBlending;

			// normal material

			this.normalMaterial = new MeshNormalMaterial();
			this.normalMaterial.Blending = THREE.Constants.NoBlending;
		}

        public override void Render(GLRenderTarget writeBuffer, GLRenderTarget readBuffer,bool? maskActive=null)
        {
			// Make sure the Depth Buffer Exists now
			if(composer.DepthBuffer == null)
			{
				var pars = new Hashtable { { "minFilter", THREE.Constants.LinearFilter }, { "magFilter", THREE.Constants.LinearFilter }, { "format", THREE.Constants.RGBAFormat } };
				this.composer.DepthBuffer = new GLRenderTarget(DualityApp.WindowSize.X, DualityApp.WindowSize.Y, pars);
				this.composer.DepthBuffer.GenerateMipmaps = false;
				this.composer.DepthBuffer.Texture.Name = "RenderPass.depth";
			}
			if (composer.NormalBuffer == null)
			{
				var pars = new Hashtable { { "minFilter", THREE.Constants.LinearFilter }, { "magFilter", THREE.Constants.LinearFilter }, { "format", THREE.Constants.RGBAFormat } };
				this.composer.NormalBuffer = new GLRenderTarget(DualityApp.WindowSize.X, DualityApp.WindowSize.Y, pars);
				this.composer.NormalBuffer.GenerateMipmaps = false;
				this.composer.NormalBuffer.Texture.Name = "RenderPass.normal";
			}

			var oldAutoClear = DualityApp.GraphicsBackend.AutoClear;
            DualityApp.GraphicsBackend.AutoClear = false;

            Color oldClearColor = DualityApp.GraphicsBackend.GetClearColor();
			float oldClearAlpha = DualityApp.GraphicsBackend.GetClearAlpha();

			Material oldOverrideMaterial = this.scene.OverrideMaterial;

			// First Render Depth
			RenderOverride(materialDepth, composer.DepthBuffer, Color.Hex(0xffffff), 1.0f);

			// second Render Depth
			RenderOverride(normalMaterial, composer.NormalBuffer, Color.Hex(0x7777ff), 1.0f);

			// then Render Color/Diffuse
			this.scene.OverrideMaterial = this.OverrideMaterial;
            if (this.ClearDepth) DualityApp.GraphicsBackend.ClearDepth();
			if (this.ClearColor != null) DualityApp.GraphicsBackend.SetClearColor(this.ClearColor.Value, this.ClearAlpha);
			DualityApp.GraphicsBackend.SetRenderTarget(readBuffer);
            // TODO: Avoid using autoClear properties, see https://github.com/mrdoob/three.js/pull/15571#issuecomment-465669600
            if (this.Clear) DualityApp.GraphicsBackend.Clear(DualityApp.GraphicsBackend.AutoClearColor, DualityApp.GraphicsBackend.AutoClearDepth, DualityApp.GraphicsBackend.AutoClearStencil);
            DualityApp.GraphicsBackend.Render(this.scene, this.camera);

			if (this.OverrideMaterial != null)
				this.scene.OverrideMaterial = oldOverrideMaterial;

			if (this.ClearColor!=null)
                DualityApp.GraphicsBackend.SetClearColor(oldClearColor, oldClearAlpha);

            DualityApp.GraphicsBackend.AutoClear = oldAutoClear;
		}

		private void RenderOverride(Material overrideMaterial, GLRenderTarget renderTarget, Color? clearColor = null, float? clearAlpha = 0.0f)
		{
			var originalClearColor = DualityApp.GraphicsBackend.GetClearColor();
			var originalClearAlpha = DualityApp.GraphicsBackend.GetClearAlpha();
			var originalAutoClear = DualityApp.GraphicsBackend.AutoClear;

			if (clearAlpha == null) clearAlpha = 0.0f;

			DualityApp.GraphicsBackend.SetRenderTarget(renderTarget);
			DualityApp.GraphicsBackend.AutoClear = false;

			if ((clearColor != null) && (clearAlpha != null))
			{

				DualityApp.GraphicsBackend.SetClearColor(clearColor.Value);
				DualityApp.GraphicsBackend.SetClearAlpha(clearAlpha.Value);
				DualityApp.GraphicsBackend.Clear();

			}

			this.scene.OverrideMaterial = overrideMaterial;
			DualityApp.GraphicsBackend.Render(this.scene, this.camera);
			this.scene.OverrideMaterial = null;

			// restore original state

			DualityApp.GraphicsBackend.AutoClear = originalAutoClear;
			DualityApp.GraphicsBackend.SetClearColor(originalClearColor);
			DualityApp.GraphicsBackend.SetClearAlpha(originalClearAlpha);
		}

		public override void SetSize(float width, float height)
        {
           
        }
    }
}
