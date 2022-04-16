using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using THREE.Materials;
using THREE.Math;
using THREE.Renderers;
using THREE.Renderers.gl;
using THREE.Renderers.Shaders;
using THREE.Shaders;

namespace Duality.Postprocessing
{
    public class DotScreenPass : Pass
    {
        public GLUniforms uniforms;
        private ShaderMaterial material;

        public DotScreenPass(Vector2? center=null,float? angle=null,float? scale=null) : base()
        {
            var shader = new DotScreenShader();

            this.uniforms = UniformsUtils.CloneUniforms(shader.Uniforms);

            if (center != null) (this.uniforms["center"] as GLUniform)["value"] = center;
            if (angle != null) (this.uniforms["angle"] as GLUniform)["value"] = angle;
            if (scale != null) (this.uniforms["scale"] as GLUniform)["value"] = scale;

            this.material = new ShaderMaterial
            {

                Uniforms = this.uniforms,
                VertexShader = shader.VertexShader,
                FragmentShader = shader.FragmentShader

            };

            this.fullScreenQuad = new FullScreenQuad(this.material);

        }
		public override void Render(GLRenderTarget writeBuffer, GLRenderTarget readBuffer, bool? maskActive = null)
		{
			(this.uniforms["tDiffuse"] as GLUniform)["value"] = readBuffer.Texture;
			((this.uniforms["tSize"] as GLUniform)["value"] as THREE.Math.Vector2).Set(readBuffer.Width, readBuffer.Height);

			if (this.Clear) DualityApp.GraphicsBackend.Clear();
			this.fullScreenQuad.Render(DualityApp.GraphicsBackend);
		}

        public override void SetSize(float width, float height)
        {
        }
    }
}
