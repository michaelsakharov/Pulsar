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
	public class ShadowMaterial : Material
	{
		public static ContentRef<ShadowMaterial> Default { get; private set; }

		internal static void InitDefaultContent()
		{
			DefaultContent.InitType<ShadowMaterial>(new Dictionary<string, ShadowMaterial>
			{
				{ "Default", new ShadowMaterial() }
			});
		}

		// Methods
		public override THREE.Materials.Material GetThreeMaterial()
		{
			if(cachedMaterial == null)
				cachedMaterial = new THREE.Materials.ShadowMaterial();
			base.SetupBaseMaterialSettings(cachedMaterial);
			return cachedMaterial;
		}

		protected override MaterialType Type { get { return MaterialType.Shadow; } }

		public ShadowMaterial() : base()
		{
			Transparent = true;
			Color = new ColorRgba(0, 0, 0, 0);
		}
	}
}
