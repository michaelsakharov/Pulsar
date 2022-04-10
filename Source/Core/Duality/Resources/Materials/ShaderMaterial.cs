using System;
using System.IO;

using Duality.Properties;
using Duality.Editor;
using System.Collections.Generic;
using Duality.Drawing;
using THREE.Renderers.gl;
using OpenTK.Graphics.ES10;
using System.Collections;

namespace Duality.Resources
{
	/// <summary>
	/// Represents an Three Material.
	/// </summary>
	[EditorHintCategory(CoreResNames.CategoryMaterials)]
	[EditorHintImage(CoreResNames.ImageMaterial)]
	public class ShaderMaterial : Material
	{
		public static ContentRef<ShaderMaterial> Default { get; private set; }
	
		internal static void InitDefaultContent()
		{
			DefaultContent.InitType<ShaderMaterial>(new Dictionary<string, ShaderMaterial>
			{
				{ "Default", new ShaderMaterial() }
			});
		}

		private ContentRef<VertexShader> vertex = VertexShader.Default;
		public ContentRef<VertexShader> Vertex
		{
			get { return vertex; }
			set { vertex = value; _isDirty = true; }
		}

		private ContentRef<FragmentShader> fragment = FragmentShader.Default;
		public ContentRef<FragmentShader> Fragment
		{
			get { return fragment; }
			set { fragment = value; _isDirty = true; }
		}

		[DontSerialize] bool _isDirty;

		// Methods
		public override THREE.Materials.Material GetThreeMaterial()
		{
			if (cachedMaterial == null || _isDirty)
			{
				if(cachedMaterial != null) cachedMaterial.Dispose();
				cachedMaterial = new THREE.Materials.ShaderMaterial();
			}
			base.SetupBaseMaterialSettings(cachedMaterial);
			(cachedMaterial as THREE.Materials.ShaderMaterial).FragmentShader = Fragment.IsAvailable ? Fragment.Res.Source : FragmentShader.Default.Res.Source;
			(cachedMaterial as THREE.Materials.ShaderMaterial).VertexShader = Vertex.IsAvailable ? Vertex.Res.Source : VertexShader.Default.Res.Source;

			return cachedMaterial;
		}
		
		public void SetUniforms(GLUniforms Uniforms)
		{
			var mat = GetThreeMaterial() as THREE.Materials.ShaderMaterial;
			mat.Uniforms = Uniforms;
			mat.UniformsNeedUpdate = true;
		}
		
		public void SetExtensions(THREE.Materials.ShaderMaterial.Extensions extensions)
		{
			var mat = GetThreeMaterial() as THREE.Materials.ShaderMaterial;
			mat.extensions = extensions;
		}
		
		public void SetAttributes(GLAttributes attributes)
		{
			var mat = GetThreeMaterial() as THREE.Materials.ShaderMaterial;
			mat.Attributes = attributes;
		}
		
		public void SetLights(bool lights)
		{
			var mat = GetThreeMaterial() as THREE.Materials.ShaderMaterial;
			mat.Lights = lights;
		}
		
		public void SetDefaultAttributeValues(Hashtable defaultAttributeValues)
		{
			var mat = GetThreeMaterial() as THREE.Materials.ShaderMaterial;
			mat.DefaultAttributeValues = defaultAttributeValues;
		}
		
		public void SetIndex0AttributeName(string index0AttributeName)
		{
			var mat = GetThreeMaterial() as THREE.Materials.ShaderMaterial;
			mat.Index0AttributeName = index0AttributeName;
		}

		protected override MaterialType Type { get { return MaterialType.Shader; } }
	
		public ShaderMaterial() : base() { }
	}
}
