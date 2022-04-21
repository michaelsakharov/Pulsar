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
    public class FilmPass : Pass
    {
        public GLUniforms uniforms;

        public ShaderMaterial material;

        public FilmPass(float? noiseIntensity,float? scanlinesIntensity,float? scanlinesCount,bool? grayscale)
        {
            var shader = new FilmShader();

            this.uniforms = UniformsUtils.CloneUniforms(shader.Uniforms);

            material = new ShaderMaterial();
            material.Uniforms = uniforms;
            material.VertexShader = shader.VertexShader;
            material.FragmentShader = shader.FragmentShader;


            if (grayscale != null) (this.uniforms["grayscale"] as GLUniform)["value"]= grayscale.Value;
            if (noiseIntensity != null) (this.uniforms["nIntensity"] as GLUniform)["value"]=noiseIntensity.Value;
            if (scanlinesIntensity != null) (this.uniforms["sIntensity"] as GLUniform)["value"] = scanlinesIntensity.Value;
            if (scanlinesCount != null) (this.uniforms["sCount"] as GLUniform)["value"] = scanlinesCount.Value;

            this.fullScreenQuad = new FullScreenQuad(this.material);
        }

		public override void Render(GLRenderTarget writeBuffer, GLRenderTarget readBuffer, bool? maskActive = null)
		{
			(this.uniforms["tDiffuse"] as GLUniform)["value"] = readBuffer.Texture;
			float currentDeltaTime = (float)(this.uniforms["time"] as GLUniform)["value"] + (float)Time.DeltaTime;
			(this.uniforms["time"] as GLUniform)["value"] = currentDeltaTime;

			if (this.Clear) DualityApp.GraphicsBackend.Clear();
			this.fullScreenQuad.Render(DualityApp.GraphicsBackend);
		}

        public override void SetSize(float width, float height)
        {
            
        }
    }
}
