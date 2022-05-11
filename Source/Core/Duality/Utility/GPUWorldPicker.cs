using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using THREE;
using THREE.Cameras;
using THREE.Core;
using THREE.Materials;
using THREE.Math;
using THREE.Objects;
using THREE.Renderers;
using THREE.Renderers.gl;
using THREE.Renderers.Shaders;
using THREE.Scenes;

namespace Duality.Utility
{
	public class GPUWorldPicker
	{
		GLRenderer renderer;
		Scene scene;
		Camera camera;

		GLRenderTarget pickingTarget;
		Scene emptyScene;
		byte[] pixelBuffer;
		Color clearColor;
		Dictionary<int, ShaderMaterial> materialCache;
		public List<Object3D> Whitelist;

		public GPUWorldPicker()
		{
			// This is the 1x1 pixel render target we use to do the picking
			var options = new System.Collections.Hashtable();
			options.Add("minFilter", Constants.NearestFilter);
			options.Add("magFilter", Constants.NearestFilter);
			options.Add("format", Constants.RGBAFormat);
			options.Add("encoding", Constants.LinearEncoding);
			pickingTarget = new GLRenderTarget(1, 1, options);

			// We need to be inside of .render in order to call renderBufferDirect in renderList() so create an empty scene
			// and use the onAfterRender callback to actually render geometry for picking.
			emptyScene = new Scene();
			emptyScene.OnAfterRender = RenderList;

			// RGBA is 4 channels.
			pixelBuffer = new byte[4 * pickingTarget.Width * pickingTarget.Height];
			clearColor = new Color(0xffffff);
			materialCache = new Dictionary<int, ShaderMaterial>();
			Whitelist = null;
		}

		public int Pick(int x, int y, GLRenderer renderer, Scene scene, Camera camera)
		{
			this.renderer = renderer;
			this.scene = scene;
			this.camera = camera;

			var w = renderer.Width;
			var h = renderer.Height;

			// Set the projection matrix to only look at the pixel we are interested in.
			camera.SetViewOffset(w, h, x, y, 1, 1);

			// Cache cameras current state
			var currRenderTarget = renderer.GetRenderTarget();
			var currAlpha = renderer.GetClearAlpha();
			var currClearColor = renderer.GetClearColor();

			renderer.SetRenderTarget(pickingTarget);
			renderer.SetClearColor(clearColor);
			renderer.Clear();
			renderer.Render(emptyScene, camera);
			renderer.ReadRenderTargetPixels(pickingTarget, new THREE.Math.Vector2(0, 0), pickingTarget.Width, pickingTarget.Height, 0, pixelBuffer);

			// Resume cameras cached state
			renderer.SetRenderTarget(currRenderTarget);
			renderer.SetClearColor(currClearColor, currAlpha);

			camera.ClearViewOffset();

			var val = (pixelBuffer[0] << 24) + (pixelBuffer[1] << 16) + (pixelBuffer[2] << 8) + pixelBuffer[3];
			return val;
		}

		void RenderList(GLRenderer renderer, Scene scene, Camera camera)
		{
			// This is the magic, these render lists are still filled with valid data.  So we can
			// submit them again for picking and save lots of work!
			//var renderList = renderer.GetRenderList().Get(scene, 0);
			var renderList = this.renderer.GetRenderLists().Get(this.scene, camera);
			foreach (var item in renderList.Opaque)
				ProcessItem(item);
			//renderList.Transmissive.forEach(processItem);
			foreach (var item in renderList.Transparent)
				ProcessItem(item);
		}

		void ProcessItem(RenderItem renderItem)
		{
			var obj = renderItem.Object3D;
			if (Whitelist != null && !Whitelist.Contains(obj))
				return;

			if (obj is Line || obj is LineSegments || obj is LineLoop) return; // Dont support selecting Lines
			if (obj is SkinnedMesh) return; // Dont support SkinnedMesh atm as it is broken

			int objId = obj.Id;
			Material material = renderItem.Material;
			BufferGeometry geometry = renderItem.Geometry;

			var useMorphing = 0;

			if (material.MorphTargets == true)
			{
				// Wouldnt this always be true?
				if (geometry.IsBufferGeometry == true)
				{
					useMorphing = geometry.MorphAttributes.ContainsKey("position") && (geometry.MorphAttributes["position"] as List<BufferAttribute<float>>).Count > 0 ? 1 : 0;
				}
				else
				{
					useMorphing = geometry.MorphTargets.Count > 0 ? 1 : 0;
				}
			}

			var useSkinning = (obj is SkinnedMesh) ? 1 : 0;
			var useInstancing = (obj is InstancedMesh) ? 1 : 0;
			var frontSide = material.Side == Constants.FrontSide ? 1 : 0;
			var backSide = material.Side == Constants.BackSide ? 1 : 0;
			var doubleSide = material.Side == Constants.DoubleSide ? 1 : 0;
			var sprite = material.type == "SpriteMaterial" ? 1 : 0;
			var sizeAttenuation = material.SizeAttenuation ? 1 : 0;
			var index = (useMorphing << 0) | (useSkinning << 1) | (useInstancing << 2) | (frontSide << 3) | (backSide << 4) | (doubleSide << 5) | (sprite << 6) | (sizeAttenuation << 7);

			// We dont support Picking material overwriting atm
			//var renderMaterial = renderItem.Object3D.PickingMaterial ? renderItem.Object3D.PickingMaterial : materialCache[index];
			ShaderMaterial renderMaterial = materialCache.ContainsKey(index) ? materialCache[index] : null;

			if (renderMaterial == null) // Theres no material for this setup, use a Default one
			{
				string vertexShader = renderer.ShaderLib.getChunk("meshbasic_vert");
				if (sprite == 1)
				{
					vertexShader = renderer.ShaderLib.getChunk("sprite_vert");
					if (sizeAttenuation == 1) vertexShader = "#define USE_SIZEATTENUATION\n\n" + vertexShader;
				}
				var options = new System.Collections.Hashtable();
				options.Add("vertexShader", vertexShader);
				options.Add("fragmentShader", "uniform float x; uniform float y; uniform float z; uniform float w; void main() { gl_FragColor = vec4(x, y, z, w); }");
				options.Add("side", material.Side);
				renderMaterial = new ShaderMaterial(options);

				renderMaterial.VertexShader = vertexShader;
				renderMaterial.FragmentShader = "uniform float x; uniform float y; uniform float z; uniform float w; void main() { gl_FragColor = vec4(x, y, z, w); }";

				renderMaterial.Skinning = useSkinning > 0;
			    renderMaterial.MorphTargets = useMorphing > 0;

				renderMaterial.Uniforms["x"] = new GLUniform { { "value", 1f } };
				renderMaterial.Uniforms["y"] = new GLUniform { { "value", 1f } };
				renderMaterial.Uniforms["z"] = new GLUniform { { "value", 1f } };
				renderMaterial.Uniforms["w"] = new GLUniform { { "value", 1f } };

				materialCache[index] = renderMaterial;
			}
			if (sprite == 1) 
			{
				renderMaterial.Uniforms["rotation"] = new GLUniform { { "value", material.Rotation } };
				renderMaterial.Uniforms["center"] = new GLUniform { { "value", (obj as Sprite).Center } };
			}
			renderMaterial.Uniforms["x"] = new GLUniform { { "value", ((uint)objId >> 24 & 255) / 255f } };
			renderMaterial.Uniforms["y"] = new GLUniform { { "value", ((uint)objId >> 16 & 255) / 255f } };
			renderMaterial.Uniforms["z"] = new GLUniform { { "value", ((uint)objId >> 8 & 255) / 255f } };
			renderMaterial.Uniforms["w"] = new GLUniform { { "value", ((uint)objId & 255) / 255f } };
			renderMaterial.UniformsNeedUpdate = true;

			var _frustum = new Frustum();
			_frustum.SetFromProjectionMatrix(this.camera.ProjectionMatrix * camera.MatrixWorldInverse);
			if (_frustum.IntersectsObject(obj))
			{
				renderer.RenderBufferDirect(camera, null, geometry, renderMaterial, obj, null);
			}
		}

	}
}
