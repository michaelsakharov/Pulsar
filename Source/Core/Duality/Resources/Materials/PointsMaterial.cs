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
			var mat = new THREE.Materials.PointsMaterial();
			base.SetupBaseMaterialSettings(mat);
			mat.Size = Size;
			return mat;
		}

		protected override MaterialType Type { get { return MaterialType.Points; } }

		public PointsMaterial() : base() { }
	}
}
