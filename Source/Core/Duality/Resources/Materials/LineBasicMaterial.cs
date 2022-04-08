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
		private ColorRgba color = ColorRgba.White;

		public float lineWidth = 1.0f;


		// Public Variables also shown in Editor
		/// <summary> [GET / SET] The main color of the material </summary>
		public ColorRgba Color { get { return this.color; } set { this.color = value; } }

		/// <summary> [GET / SET] The main color of the material </summary>
		public float LineWidth { get { return this.lineWidth; } set { this.lineWidth = value; } }


		// Methods
		public override THREE.Materials.Material GetThreeMaterial()
		{
			var mat = new THREE.Materials.LineBasicMaterial();
			mat.Color = new THREE.Math.Color(Color.R, Color.G, Color.B);
			mat.LineWidth = LineWidth;
			mat.LineCap = "round";
			mat.LineJoin = "round";
			return mat;
		}

		protected override MaterialType Type { get { return MaterialType.LineBasic; } }

		public LineBasicMaterial() : base() { }
	}
}
