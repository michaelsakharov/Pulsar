using System;
using System.Collections;
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
    public class BloomPass : Pass
    {
        private float strength;
        private int kernelSize;
        private float sigma;
        private int resolution;

        private GLRenderTarget renderTargetX;
        private GLRenderTarget renderTargetY;
        private ShaderMaterial materialCopy;
        private ShaderMaterial materialConvolution;
        private GLUniforms convolutionUniforms;
        private GLUniforms uniforms;
        private CopyShader copyShader;

        public static Vector2 BlurX = new Vector2(0.001953125f, 0.0f);
        public static Vector2 BlurY = new Vector2(0.0f, 0.001953125f);

        public BloomPass(float? strength=null,int? kernelSize=null,float? sigma=null,int? resolution=null) : base()
        {
            this.strength = strength != null ? strength.Value : 1.0f;
            this.kernelSize = kernelSize != null ? kernelSize.Value : 25;
            this.sigma = sigma != null ? sigma.Value : 4.0f;
            this.resolution = resolution != null ? resolution.Value : 256;

            Hashtable pars = new Hashtable();
            pars.Add("minFilter", THREE.Constants.LinearFilter);
            pars.Add("magFilter", THREE.Constants.LinearFilter);
            pars.Add("format", THREE.Constants.RGBAFormat);

            renderTargetX = new GLRenderTarget(this.resolution, this.resolution, pars);
            renderTargetX.Texture.Name = "BloomPass.x";

            renderTargetY = new GLRenderTarget(this.resolution, this.resolution, pars);
            renderTargetY.Texture.Name = "BloomPass.y";


            copyShader = new CopyShader();

            uniforms = UniformsUtils.CloneUniforms(copyShader.Uniforms);

            (uniforms["opacity"] as GLUniform)["value"] = this.strength;

            materialCopy = new ShaderMaterial { 
                Uniforms = uniforms,
                VertexShader = copyShader.VertexShader,
                FragmentShader = copyShader.FragmentShader,
                Blending = THREE.Constants.AdditiveBlending,
                Transparent = true            
            };

            ConvolutionShader convolutionShader = new ConvolutionShader();

            convolutionUniforms = UniformsUtils.CloneUniforms(convolutionShader.Uniforms);

            (convolutionUniforms["uImageIncrement"] as GLUniform)["value"] = BloomPass.BlurX;
            (convolutionUniforms["cKernel"] as GLUniform)["value"] = convolutionShader.BuildKernel(this.sigma);

            materialConvolution = new ShaderMaterial
            {
                Uniforms = convolutionUniforms,
                VertexShader = convolutionShader.VertexShader,
                FragmentShader = convolutionShader.FragmentShader
            };
            materialConvolution.Defines.Add("KERNEL_SIZE_FLOAT", this.kernelSize.ToString()+".0");
            materialConvolution.Defines.Add("KERNEL_SIZE_INT", this.kernelSize.ToString());

            //this.NeedsSwap = false;

            this.fullScreenQuad = new FullScreenQuad();
        }

        public override void Render(GLRenderTarget writeBuffer, GLRenderTarget readBuffer, bool? maskActive = null)
        {
            if (maskActive != null && maskActive.Value == true) DualityApp.GraphicsBackend.state.buffers.stencil.SetTest(false);
			
            // Render quad with blured scene into texture (convolution pass 1)
			
            this.fullScreenQuad.material = this.materialConvolution;
			
            (this.convolutionUniforms["tDiffuse"] as GLUniform)["value"] = readBuffer.Texture;
            (this.convolutionUniforms["uImageIncrement"] as GLUniform)["value"] = BloomPass.BlurX;
			
            DualityApp.GraphicsBackend.SetRenderTarget(this.renderTargetX);
            DualityApp.GraphicsBackend.Clear();
            this.fullScreenQuad.Render(DualityApp.GraphicsBackend);
			
			
            // Render quad with blured scene into texture (convolution pass 2)
			
            (this.convolutionUniforms["tDiffuse"] as GLUniform)["value"] = this.renderTargetX.Texture;
            (this.convolutionUniforms["uImageIncrement"] as GLUniform)["value"] = BloomPass.BlurY;
			
            DualityApp.GraphicsBackend.SetRenderTarget(this.renderTargetY);
            DualityApp.GraphicsBackend.Clear();
            this.fullScreenQuad.Render(DualityApp.GraphicsBackend);
			
            // Render original scene with superimposed blur to texture
			
            this.fullScreenQuad.material = this.materialCopy;

			(this.uniforms["tDiffuse"] as GLUniform)["value"] = this.renderTargetY.Texture;
			//(this.uniforms["tDiffuse"] as GLUniform)["value"] = readBuffer.Texture;
			
            if (maskActive!=null && maskActive.Value==true) DualityApp.GraphicsBackend.state.buffers.stencil.SetTest(true);

            DualityApp.GraphicsBackend.SetRenderTarget(writeBuffer);
            if (this.Clear) DualityApp.GraphicsBackend.Clear();
            this.fullScreenQuad.Render(DualityApp.GraphicsBackend);

        }

        public override void SetSize(float width, float height)
        {
           
        }
    }
}
