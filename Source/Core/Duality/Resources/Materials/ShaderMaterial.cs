using System;
using System.IO;

using Duality.Properties;
using Duality.Editor;
using System.Collections.Generic;
using Duality.Drawing;
using THREE.Renderers.gl;

namespace Duality.Resources
{
	// Not Implemented Yet

	/// <summary>
	/// Represents an Three Material.
	/// </summary>
	//[EditorHintCategory(CoreResNames.CategoryGraphics)]
	//[EditorHintImage(CoreResNames.ImageMaterial)]
	//public class ShaderMaterial : Material
	//{
	//	public static ContentRef<ShaderMaterial> Default { get; private set; }
	//
	//	internal static void InitDefaultContent()
	//	{
	//		DefaultContent.InitType<ShaderMaterial>(new Dictionary<string, ShaderMaterial>
	//		{
	//			{ "Default", new ShaderMaterial() }
	//		});
	//	}
	//
	//	public ContentRef<FragmentShader> Fragment = FragmentShader.Default;
	//	public ContentRef<VertexShader> Vertex = VertexShader.Default;
	//
	//	// Methods
	//	public override THREE.Materials.Material GetThreeMaterial()
	//	{
	//		var mat = new THREE.Materials.ShaderMaterial();
	//		base.SetupBaseMaterialSettings(mat);
	//		mat.FragmentShader = Fragment.IsAvailable ? Fragment.Res.Source : FragmentShader.Default.Res.Source;
	//		mat.VertexShader = Vertex.IsAvailable ? Vertex.Res.Source : VertexShader.Default.Res.Source;
	//
	//		return mat;
	//	}
	//
	//	protected override MaterialType Type { get { return MaterialType.Shader; } }
	//
	//	public ShaderMaterial() : base() { }
	//}
}
