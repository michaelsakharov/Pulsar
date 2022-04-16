using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using THREE.Cameras;
using THREE.Renderers;

namespace Duality.Postprocessing
{
    public class MaskPass : Pass
    {

        public bool Inverse;

        public MaskPass() : base()
        {
            this.Clear = true;
            this.NeedsSwap = false;
            this.Inverse = false;
        }
        public override void Render(GLRenderTarget writeBuffer, GLRenderTarget readBuffer, bool? maskActive=null)
        {
            var state = DualityApp.GraphicsBackend.state;

            // don't update color or depth
            state.buffers.color.SetMask(false);
            state.buffers.depth.SetMask(false);

            // lock buffers

            state.buffers.color.SetLocked(true);
            state.buffers.depth.SetLocked(true);

            // set up stencil

            int writeValue,clearValue;

            if (this.Inverse)
            {

                writeValue = 0;
                clearValue = 1;

            }
            else
            {

                writeValue = 1;
                clearValue = 0;

            }

            state.buffers.stencil.SetTest(true);
            state.buffers.stencil.SetOp(THREE.Constants.ReplaceStencilOp, THREE.Constants.ReplaceStencilOp, THREE.Constants.ReplaceStencilOp);
            unchecked
            {
                state.buffers.stencil.SetFunc(THREE.Constants.AlwaysStencilFunc, writeValue, (int)0xffffffff);
            }
            state.buffers.stencil.SetClear(clearValue);
            state.buffers.stencil.SetLocked(true);

            // draw into the stencil buffer

            DualityApp.GraphicsBackend.SetRenderTarget(readBuffer);
            if (this.Clear) DualityApp.GraphicsBackend.Clear();
            DualityApp.GraphicsBackend.Render(this.scene, this.camera);

            DualityApp.GraphicsBackend.SetRenderTarget(writeBuffer);
            if (this.Clear) DualityApp.GraphicsBackend.Clear();
            DualityApp.GraphicsBackend.Render(this.scene, this.camera);

            // unlock color and depth buffer for subsequent rendering

            state.buffers.color.SetLocked(false);
            state.buffers.depth.SetLocked(false);

            // only render where stencil is set to 1

            state.buffers.stencil.SetLocked(false);
            unchecked
            {
                state.buffers.stencil.SetFunc(THREE.Constants.EqualStencilFunc, 1, (int)0xffffffff); // draw if == 1
            }
            state.buffers.stencil.SetOp(THREE.Constants.KeepStencilOp, THREE.Constants.KeepStencilOp, THREE.Constants.KeepStencilOp);
            state.buffers.stencil.SetLocked(true);
        }

        public override void SetSize(float width, float height)
        {
           
        }
    }

    public class ClearMaskPass : Pass
    {
        public ClearMaskPass() : base()
        {
            this.NeedsSwap = false;
        }
        public override void Render(GLRenderTarget writeBuffer, GLRenderTarget readBuffer, bool? maskActive = null)
        {
            DualityApp.GraphicsBackend.state.buffers.stencil.SetLocked(false);
            DualityApp.GraphicsBackend.state.buffers.stencil.SetTest(false);
        }

        public override void SetSize(float width, float height)
        {
            
        }
    }
}
