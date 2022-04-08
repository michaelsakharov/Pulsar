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
	public class MeshToonMaterial : Material
	{
		public static ContentRef<MeshToonMaterial> Default { get; private set; }

		internal static void InitDefaultContent()
		{
			DefaultContent.InitType<MeshToonMaterial>(new Dictionary<string, MeshToonMaterial>
			{
				{ "Default", new MeshToonMaterial() }
			});
		}

		// Methods
		public override THREE.Materials.Material GetThreeMaterial()
		{
			var mat = new THREE.Materials.MeshToonMaterial();
			return mat;
		}

		protected override MaterialType Type { get { return MaterialType.MeshToon; } }

		public MeshToonMaterial() : base() { }
	}
}
