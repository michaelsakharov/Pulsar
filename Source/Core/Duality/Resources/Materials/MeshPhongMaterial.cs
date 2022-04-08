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
			var mat = new THREE.Materials.MeshPhongMaterial();
			return mat;
		}

		protected override MaterialType Type { get { return MaterialType.MeshPhong; } }

		public MeshPhongMaterial() : base() { }
	}
}
