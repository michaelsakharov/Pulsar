using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using THREE.Materials;
using THREE.Renderers;
using THREE.Renderers.gl;
using THREE.Renderers.Shaders;
using THREE.Shaders;

namespace Duality.Postprocessing
{
    public class HalftonePass : Pass
    {
        public GLUniforms uniforms;
        public ShaderMaterial material;
        public HalftonePass(float? width=null,float? height=null,Hashtable parameter=null) : base()
        {
            var halftoneShader = new HalftoneShader();
            uniforms = UniformsUtils.CloneUniforms(halftoneShader.Uniforms);
            this.material = new ShaderMaterial{
                    Uniforms = this.uniforms,
                    FragmentShader = halftoneShader.FragmentShader,
                    VertexShader = halftoneShader.VertexShader
                };

            // set params

            (this.uniforms["width"] as GLUniform)["value"] = width;
            (this.uniforms["height"] as GLUniform)["value"] = height;

            if (parameter != null)
            {


                foreach (DictionaryEntry key in parameter)
                {

                    if (key.Value != null && this.uniforms.Contains(key.Key))
                    {
                        (this.uniforms[key.Key] as GLUniform)["value"] = key.Value;
                    }
                }
            }
            this.fullScreenQuad = new FullScreenQuad(this.material);
        }
		public override void Render(GLRenderTarget writeBuffer, GLRenderTarget readBuffer, bool? maskActive = null)
		{
			(this.material.Uniforms["tDiffuse"] as GLUniform)["value"] = readBuffer.Texture;

			if (this.Clear) DualityApp.GraphicsBackend.Clear();
			this.fullScreenQuad.Render(DualityApp.GraphicsBackend);
		}

        public override void SetSize(float width, float height)
        {
            (this.uniforms["width"] as GLUniform)["value"] = width;
            (this.uniforms["height"] as GLUniform)["value"] = height;
        }
    }
}
