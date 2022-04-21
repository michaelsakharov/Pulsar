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
	[EditorHintCategory(CoreResNames.CategoryMaterials)]
	[EditorHintImage(CoreResNames.ImageMaterial)]
	public class MeshDepthMaterial : Material
	{
		public static ContentRef<MeshDepthMaterial> Default { get; private set; }

		internal static void InitDefaultContent()
		{
			DefaultContent.InitType<MeshDepthMaterial>(new Dictionary<string, MeshDepthMaterial>
			{
				{ "Default", new MeshDepthMaterial() }
			});
		}

		// Methods
		public override THREE.Materials.Material GetThreeMaterial()
		{
			if (cachedMaterial == null)
				cachedMaterial = new THREE.Materials.MeshDepthMaterial();
			base.SetupBaseMaterialSettings(cachedMaterial);
			return cachedMaterial;
		}

		protected override MaterialType Type { get { return MaterialType.MeshDepth; } }

		public MeshDepthMaterial() : base() { }
	}
}
