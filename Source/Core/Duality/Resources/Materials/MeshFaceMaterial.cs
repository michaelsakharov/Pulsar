using System;
using System.IO;

using Duality.Properties;
using Duality.Editor;
using System.Collections.Generic;
using Duality.Drawing;

namespace Duality.Resources
{
	/// <summary>
	/// Represents an Three Material.
	/// </summary>
	[EditorHintCategory(CoreResNames.CategoryGraphics)]
	[EditorHintImage(CoreResNames.ImageMaterial)]
	public class MeshFaceMaterial : Material
	{
		public static ContentRef<MeshFaceMaterial> Default { get; private set; }

		internal static void InitDefaultContent()
		{
			DefaultContent.InitType<MeshFaceMaterial>(new Dictionary<string, MeshFaceMaterial>
			{
				{ "Default", new MeshFaceMaterial() }
			});
		}

		//public List<Material> Materials;

		// Methods
		public override THREE.Materials.Material GetThreeMaterial()
		{
			var mat = new THREE.Materials.MeshFaceMaterial();
			//mat.Materials = Materials;
			return mat;
		}

		protected override MaterialType Type { get { return MaterialType.MeshFace; } }

		public MeshFaceMaterial() : base() { }
	}
}
