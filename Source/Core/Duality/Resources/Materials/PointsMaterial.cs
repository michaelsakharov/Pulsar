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
	public class PointsMaterial : Material
	{
		public static ContentRef<PointsMaterial> Default { get; private set; }

		internal static void InitDefaultContent()
		{
			DefaultContent.InitType<PointsMaterial>(new Dictionary<string, PointsMaterial>
			{
				{ "Default", new PointsMaterial() }
			});
		}

		public float Size = 1f;

		// Methods
		public override THREE.Materials.Material GetThreeMaterial()
		{
			if (cachedMaterial == null)
				cachedMaterial = new THREE.Materials.PointsMaterial();
			base.SetupBaseMaterialSettings(cachedMaterial);
			(cachedMaterial as THREE.Materials.PointsMaterial).Size = Size;

			return cachedMaterial;
		}

		protected override MaterialType Type { get { return MaterialType.Points; } }

		public PointsMaterial() : base() { }
	}
}
