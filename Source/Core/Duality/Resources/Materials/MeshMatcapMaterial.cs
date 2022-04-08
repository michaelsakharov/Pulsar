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
	public class MeshMatcapMaterial : Material
	{
		public static ContentRef<MeshMatcapMaterial> Default { get; private set; }

		internal static void InitDefaultContent()
		{
			DefaultContent.InitType<MeshMatcapMaterial>(new Dictionary<string, MeshMatcapMaterial>
			{
				{ "Default", new MeshMatcapMaterial() }
			});
		}

		// Methods
		public override THREE.Materials.Material GetThreeMaterial()
		{
			var mat = new THREE.Materials.MeshMatcapMaterial();
			return mat;
		}

		protected override MaterialType Type { get { return MaterialType.MeshMatcap; } }

		public MeshMatcapMaterial() : base() { }
	}
}
