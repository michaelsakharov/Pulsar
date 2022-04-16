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
using THREE.Renderers.gl;
using THREE.Renderers.Shaders;
using THREE.Scenes;
using THREE.Shaders;

namespace Duality.Postprocessing
{
    public class BokehPass : Pass
    {
        private GLRenderTarget renderTargetDepth;
        private MeshDepthMaterial materialDepth;
        private ShaderMaterial materialBokeh;
        public GLUniforms uniforms;
		Color oldClearColor;
        public BokehPass(Hashtable parameter) : base()
        {
			var focus = parameter.ContainsKey("focus")   ? parameter["focus"] : 1.0f;
			var aspect = parameter.ContainsKey("aspect") ? parameter["aspect"]: camera.Aspect;
			var aperture = parameter.ContainsKey("aperture") ? parameter["aperture"]: 0.025f;
			var maxblur = parameter.ContainsKey("maxblur")? parameter["maxblur"]: 1.0f;

			// render targets

			var width = parameter.ContainsKey("width") ? (int)parameter["width"] : 1;
			var height = parameter.ContainsKey("height") ? (int)parameter["height"] : 1;

			this.renderTargetDepth = new GLRenderTarget(width, height,new Hashtable {
				{ "minFilter", THREE.Constants.NearestFilter },
				{ "magFilter", THREE.Constants.NearestFilter }
			} );

			this.renderTargetDepth.Texture.Name = "BokehPass.depth";

			// depth material

			this.materialDepth = new MeshDepthMaterial();
			this.materialDepth.DepthPacking = THREE.Constants.RGBADepthPacking;
			this.materialDepth.Blending = THREE.Constants.NoBlending;

			// bokeh material

			

			var bokehShader = new BokehShader();
			var bokehUniforms = UniformsUtils.CloneUniforms(bokehShader.Uniforms);

			(bokehUniforms["tDepth"] as GLUniform)["value"] = this.renderTargetDepth.Texture;

			(bokehUniforms["focus"] as GLUniform)["value"] = focus;
			(bokehUniforms["aspect"] as GLUniform)["value"] = aspect;
			(bokehUniforms["aperture"] as GLUniform)["value"] = aperture;
			(bokehUniforms["maxblur"] as GLUniform)["value"] = maxblur;
			(bokehUniforms["nearClip"] as GLUniform)["value"] = camera.Near;
			(bokehUniforms["farClip"] as GLUniform)["value"] = camera.Far;

			this.materialBokeh = new ShaderMaterial {
				Defines =bokehShader.Defines ,
				Uniforms = bokehUniforms,
			    VertexShader = bokehShader.VertexShader,
				FragmentShader = bokehShader.FragmentShader
			};

			this.uniforms = bokehUniforms;
			this.NeedsSwap = false;

			this.fullScreenQuad = new FullScreenQuad(this.materialBokeh);

			this.oldClearColor = new Color();

		}
		public override void Render(GLRenderTarget writeBuffer, GLRenderTarget readBuffer, bool? maskActive = null)
		{
			// Render depth into texture

			this.scene.OverrideMaterial = this.materialDepth;

			this.oldClearColor.Copy(DualityApp.GraphicsBackend.GetClearColor());
			var oldClearAlpha = DualityApp.GraphicsBackend.GetClearAlpha();
			var oldAutoClear = DualityApp.GraphicsBackend.AutoClear;
			DualityApp.GraphicsBackend.AutoClear = false;

			DualityApp.GraphicsBackend.SetClearColor(Color.Hex(0xffffff));
			DualityApp.GraphicsBackend.SetClearAlpha(1.0f);
			DualityApp.GraphicsBackend.SetRenderTarget(this.renderTargetDepth);
			DualityApp.GraphicsBackend.Clear();
			DualityApp.GraphicsBackend.Render(this.scene, this.camera);

			// Render bokeh composite

			(this.uniforms["tColor"] as GLUniform)["value"] = readBuffer.Texture;
			(this.uniforms["nearClip"] as GLUniform)["value"] = this.camera.Near;
			(this.uniforms["farClip"] as GLUniform)["value"] = this.camera.Far;

			DualityApp.GraphicsBackend.SetRenderTarget(writeBuffer);
			DualityApp.GraphicsBackend.Clear();
			this.fullScreenQuad.Render(DualityApp.GraphicsBackend);

			this.scene.OverrideMaterial = null;
			DualityApp.GraphicsBackend.SetClearColor(this.oldClearColor);
			DualityApp.GraphicsBackend.SetClearAlpha(oldClearAlpha);
			DualityApp.GraphicsBackend.AutoClear = oldAutoClear;
		}

        public override void SetSize(float width, float height)
        {
            
        }
    }
}
