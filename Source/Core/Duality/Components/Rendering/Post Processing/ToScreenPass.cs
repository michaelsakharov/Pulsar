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
        public GLUniforms depthuniforms;

        public ShaderMaterial material;
        public ShaderMaterial depthMaterial;

        public ToScreenPass()
		{
			var shader = new CopyShader();

			this.uniforms = UniformsUtils.CloneUniforms(shader.Uniforms);

			material = new ShaderMaterial();
			material.Uniforms = uniforms;
			material.VertexShader = shader.VertexShader;
			material.FragmentShader = shader.FragmentShader;

			var depthshader = new ToScreenDepthShader();

			this.depthuniforms = UniformsUtils.CloneUniforms(depthshader.Uniforms);

			depthMaterial = new ShaderMaterial();
			depthMaterial.Uniforms = depthuniforms;
			depthMaterial.VertexShader = depthshader.VertexShader;
			depthMaterial.FragmentShader = depthshader.FragmentShader;

			this.fullScreenQuad = new FullScreenQuad(this.material);
		}

		public override void Render(GLRenderTarget writeBuffer, GLRenderTarget readBuffer, bool? maskActive = null)
		{
			if (true)
			{
				(this.uniforms["tDiffuse"] as GLUniform)["value"] = readBuffer.Texture;
				//(this.uniforms["tDiffuse"] as GLUniform)["value"] = composer.NormalBuffer.Texture;
				this.fullScreenQuad.material = material;
			}
			else
			{
				(this.depthuniforms["tDepth"] as GLUniform)["value"] = composer.DepthBuffer.Texture;
				(this.depthuniforms["cameraNear"] as GLUniform)["value"] = camera.Near;
				(this.depthuniforms["cameraFar"] as GLUniform)["value"] = camera.Far;
				this.fullScreenQuad.material = depthMaterial;
			}

			DualityApp.GraphicsBackend.SetRenderTarget(null);
			this.fullScreenQuad.Render(DualityApp.GraphicsBackend);
		}

        public override void SetSize(float width, float height)
        {
            
        }
    }

	public class ToScreenDepthShader : ShaderMaterial
	{
		public ToScreenDepthShader()
		{
			Defines.Add("PERSPECTIVE_CAMERA", "1");

			Uniforms = new GLUniforms{

				{ "tDepth", new GLUniform{{ "value", null } } },
				{ "cameraNear", new GLUniform{{ "value", null } } },
				{ "cameraFar", new GLUniform{{ "value", null } } }
				};

			VertexShader = @"
			varying vec2 vUv; 

             void main() {

				vUv = uv;
			    gl_Position = projectionMatrix * modelViewMatrix * vec4( position, 1.0 );
		     }


                ";

			FragmentShader = @"
		uniform sampler2D tDepth; 

		uniform float cameraNear;
		uniform float cameraFar;

		varying vec2 vUv;

		#include <packing>

		float getDepth( const in vec2 screenPosition ) {
			return unpackRGBAToDepth( texture2D( tDepth, screenPosition ) );
		}

		void main() {

			float depth = getDepth( vUv );
			gl_FragColor = vec4(vec3(1.0 - depth), 1.0 );

		}

		";
		}
	}
}
