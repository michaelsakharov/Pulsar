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
	public class SpriteMaterial : Material
	{
		public static ContentRef<SpriteMaterial> Default { get; private set; }

		internal static void InitDefaultContent()
		{
			DefaultContent.InitType<SpriteMaterial>(new Dictionary<string, SpriteMaterial>
			{
				{ "Default", new SpriteMaterial() }
			});
		}

		public float Rotation = 0;
		public bool SizeAttenuation = true;

		// Methods
		public override THREE.Materials.Material GetThreeMaterial()
		{
			var mat = new THREE.Materials.SpriteMaterial();
			base.SetupBaseMaterialSettings(mat);
			mat.Rotation = Rotation;
			mat.SizeAttenuation = SizeAttenuation;
			return mat;
		}

		protected override MaterialType Type { get { return MaterialType.Sprite; } }

		public SpriteMaterial() : base()
		{
			Color = new ColorRgba(0, 0, 0, 0);
			Transparent = true;
		}
	}
}
