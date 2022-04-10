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
	public class PointCloudMaterial : Material
	{
		public static ContentRef<PointCloudMaterial> Default { get; private set; }

		internal static void InitDefaultContent()
		{
			DefaultContent.InitType<PointCloudMaterial>(new Dictionary<string, PointCloudMaterial>
			{
				{ "Default", new PointCloudMaterial() }
			});
		}

		public float Size = 1f;

		// Methods
		public override THREE.Materials.Material GetThreeMaterial()
		{
			var mat = new THREE.Materials.PointCloudMaterial();
			base.SetupBaseMaterialSettings(mat);
			mat.Size = Size;
			return mat;
		}

		protected override MaterialType Type { get { return MaterialType.PointCloud; } }

		public PointCloudMaterial() : base() { }
	}
}
