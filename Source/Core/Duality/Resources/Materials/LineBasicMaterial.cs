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
	public class LineBasicMaterial : Material
	{
		public static ContentRef<LineBasicMaterial> Default { get; private set; }

		internal static void InitDefaultContent()
		{
			DefaultContent.InitType<LineBasicMaterial>(new Dictionary<string, LineBasicMaterial>
			{
				{ "Default", new LineBasicMaterial() }
			});
		}


		// Variables
		public float lineWidth = 1.0f;

		/// <summary> [GET / SET] The main color of the material </summary>
		public float LineWidth { get { return this.lineWidth; } set { this.lineWidth = value; } }


		// Methods
		public override THREE.Materials.Material GetThreeMaterial()
		{
			if (cachedMaterial == null)
				cachedMaterial = new THREE.Materials.LineBasicMaterial();

			base.SetupBaseMaterialSettings(cachedMaterial);
			cachedMaterial.LineWidth = LineWidth;
			(cachedMaterial as THREE.Materials.LineBasicMaterial).LineCap = "round";
			(cachedMaterial as THREE.Materials.LineBasicMaterial).LineJoin = "round";
			return cachedMaterial;
		}

		protected override MaterialType Type { get { return MaterialType.LineBasic; } }

		public LineBasicMaterial() : base() { }
	}
}
