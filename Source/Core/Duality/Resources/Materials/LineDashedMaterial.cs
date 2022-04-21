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
	public class LineDashedMaterial : Material
	{
		public static ContentRef<LineDashedMaterial> Default { get; private set; }

		internal static void InitDefaultContent()
		{
			DefaultContent.InitType<LineDashedMaterial>(new Dictionary<string, LineDashedMaterial>
			{
				{ "Default", new LineDashedMaterial() }
			});
		}


		// Variables
		public float lineWidth = 1.0f;

		public float scale = 1.0f;
		public float dashSize = 3.0f;
		public float gapSize = 1.0f;


		// Public Variables also shown in Editor
		/// <summary> [GET / SET] The main color of the material </summary>
		public float LineWidth { get { return this.lineWidth; } set { this.lineWidth = value; } }

		/// <summary> [GET / SET] The main color of the material </summary>
		public float Scale { get { return this.scale; } set { this.scale = value; } }

		/// <summary> [GET / SET] The main color of the material </summary>
		public float DashSize { get { return this.dashSize; } set { this.dashSize = value; } }

		/// <summary> [GET / SET] The main color of the material </summary>
		public float GapSize { get { return this.gapSize; } set { this.gapSize = value; } }


		// Methods
		public override THREE.Materials.Material GetThreeMaterial()
		{
			if (cachedMaterial == null)
				cachedMaterial = new THREE.Materials.LineDashedMaterial();

			base.SetupBaseMaterialSettings(cachedMaterial);
			cachedMaterial.LineWidth = LineWidth;
			(cachedMaterial as THREE.Materials.LineDashedMaterial).LineCap = "round";
			(cachedMaterial as THREE.Materials.LineDashedMaterial).LineJoin = "round";

			(cachedMaterial as THREE.Materials.LineDashedMaterial).Scale = scale;
			(cachedMaterial as THREE.Materials.LineDashedMaterial).DashSize = dashSize;
			(cachedMaterial as THREE.Materials.LineDashedMaterial).GapSize = gapSize;
			return cachedMaterial;
		}

		protected override MaterialType Type { get { return MaterialType.LineDashed; } }

		public LineDashedMaterial() : base() { }
	}
}
