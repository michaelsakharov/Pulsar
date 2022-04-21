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
	public class MeshPhongMaterial : Material
	{
		public static ContentRef<MeshPhongMaterial> Default { get; private set; }

		internal static void InitDefaultContent()
		{
			DefaultContent.InitType<MeshPhongMaterial>(new Dictionary<string, MeshPhongMaterial>
			{
				{ "Default", new MeshPhongMaterial() }
			});
		}

		// Methods
		public override THREE.Materials.Material GetThreeMaterial()
		{
			if (cachedMaterial == null)
				cachedMaterial = new THREE.Materials.MeshPhongMaterial();
			base.SetupBaseMaterialSettings(cachedMaterial);
			return cachedMaterial;
		}

		protected override MaterialType Type { get { return MaterialType.MeshPhong; } }

		public MeshPhongMaterial() : base() { }
	}
}
