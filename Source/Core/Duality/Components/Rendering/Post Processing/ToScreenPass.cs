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

namespace Duality.Postprocessing
{
    public class ToScreenPass : Pass
    {
        public GLUniforms uniforms;

        public ShaderMaterial material;

        public ToScreenPass()
		{
			var shader = new CopyShader();

			this.uniforms = UniformsUtils.CloneUniforms(shader.Uniforms);

			material = new ShaderMaterial();
			material.Uniforms = uniforms;
			material.VertexShader = shader.VertexShader;
			material.FragmentShader = shader.FragmentShader;

			this.fullScreenQuad = new FullScreenQuad(this.material);
        }

		public override void Render(GLRenderTarget writeBuffer, GLRenderTarget readBuffer, bool? maskActive = null)
		{
			(this.uniforms["tDiffuse"] as GLUniform)["value"] = readBuffer.Texture;

			DualityApp.GraphicsBackend.SetRenderTarget(null);
			this.fullScreenQuad.Render(DualityApp.GraphicsBackend);
		}

        public override void SetSize(float width, float height)
        {
            
        }
    }
}
