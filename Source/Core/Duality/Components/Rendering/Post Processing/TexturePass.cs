using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using THREE.Materials;
using THREE.Renderers;
using THREE.Renderers.gl;
using THREE.Renderers.Shaders;
using THREE.Shaders;
using THREE.Textures;

namespace Duality.Postprocessing
{
    public class TexturePass : Pass
    {
        private Texture map;
        private float opacity;
        private GLUniforms uniforms;
        private ShaderMaterial material;

        public TexturePass(Texture map,float? opacity = null) : base()
        {
            var shader = new CopyShader();

            this.map = map;
            this.opacity = opacity != null ? opacity.Value : 1.0f;
            this.uniforms = UniformsUtils.CloneUniforms(shader.Uniforms);

            this.material = new ShaderMaterial { 
                Uniforms = this.uniforms,
                VertexShader = shader.VertexShader,
                FragmentShader = shader.FragmentShader,
                DepthTest = false,
                DepthWrite=false
            };

            this.NeedsSwap = false;

            this.fullScreenQuad = new FullScreenQuad();
        }
        public override void Render(GLRenderTarget writeBuffer, GLRenderTarget readBuffer, bool? maskActive = null)
        {
            var oldAutoClear = DualityApp.GraphicsBackend.AutoClear;
			DualityApp.GraphicsBackend.AutoClear = false;

            this.fullScreenQuad.material = this.material;

            (this.uniforms["opacity"] as GLUniform)["value"] = this.opacity;
            (this.uniforms["tDiffuse"] as GLUniform)["value"] = this.map;
            this.material.Transparent = (this.opacity < 1.0);

			DualityApp.GraphicsBackend.SetRenderTarget(readBuffer);
            if (this.Clear) DualityApp.GraphicsBackend.Clear();
            this.fullScreenQuad.Render(DualityApp.GraphicsBackend);

			DualityApp.GraphicsBackend.AutoClear = oldAutoClear;
        }

        public override void SetSize(float width, float height)
        {
            
        }
    }
}
