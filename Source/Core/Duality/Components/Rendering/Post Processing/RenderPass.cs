using System;
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
        }

        public override void Render(GLRenderTarget writeBuffer, GLRenderTarget readBuffer,bool? maskActive=null)
        {
            var oldAutoClear = DualityApp.GraphicsBackend.AutoClear;
            DualityApp.GraphicsBackend.AutoClear = false;

            Color? oldClearColor = null;
            float oldClearAlpha=1;

			Material oldOverrideMaterial = this.scene.OverrideMaterial;

            this.scene.OverrideMaterial = this.OverrideMaterial;

            if (this.ClearColor != null)
            {
                oldClearColor = DualityApp.GraphicsBackend.GetClearColor();
                oldClearAlpha = DualityApp.GraphicsBackend.GetClearAlpha();

                DualityApp.GraphicsBackend.SetClearColor(this.ClearColor.Value, this.ClearAlpha);
            }

            if (this.ClearDepth) DualityApp.GraphicsBackend.ClearDepth();

            DualityApp.GraphicsBackend.SetRenderTarget(readBuffer);
            // TODO: Avoid using autoClear properties, see https://github.com/mrdoob/three.js/pull/15571#issuecomment-465669600
            if (this.Clear) DualityApp.GraphicsBackend.Clear(DualityApp.GraphicsBackend.AutoClearColor, DualityApp.GraphicsBackend.AutoClearDepth, DualityApp.GraphicsBackend.AutoClearStencil);
            DualityApp.GraphicsBackend.Render(this.scene, this.camera);

            if (this.ClearColor!=null)
            {
                DualityApp.GraphicsBackend.SetClearColor(oldClearColor.Value, oldClearAlpha);
            }

            if (this.OverrideMaterial != null)
            {
                this.scene.OverrideMaterial = oldOverrideMaterial;
            }

            DualityApp.GraphicsBackend.AutoClear = oldAutoClear;
        }

        public override void SetSize(float width, float height)
        {
           
        }
    }
}
