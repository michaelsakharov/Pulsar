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
	public class MeshBasicMaterial : Material
	{
		public static ContentRef<MeshBasicMaterial> Default { get; private set; }

		internal static void InitDefaultContent()
		{
			DefaultContent.InitType<MeshBasicMaterial>(new Dictionary<string, MeshBasicMaterial>
			{
				{ "Default", new MeshBasicMaterial() }
			});
		}

		// Methods
		public override THREE.Materials.Material GetThreeMaterial()
		{
			if (cachedMaterial == null)
				cachedMaterial = new THREE.Materials.MeshBasicMaterial();
			base.SetupBaseMaterialSettings(cachedMaterial);
			return cachedMaterial;
		}

		protected override MaterialType Type { get { return MaterialType.MeshBasic; } }

		public MeshBasicMaterial() : base() { }
	}
}
