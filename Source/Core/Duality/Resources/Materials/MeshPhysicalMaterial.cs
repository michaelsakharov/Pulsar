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
	public class MeshPhysicalMaterial : Material
	{
		public static ContentRef<MeshPhysicalMaterial> Default { get; private set; }

		internal static void InitDefaultContent()
		{
			DefaultContent.InitType<MeshPhysicalMaterial>(new Dictionary<string, MeshPhysicalMaterial>
			{
				{ "Default", new MeshPhysicalMaterial() }
			});
		}

		// Methods
		public override THREE.Materials.Material GetThreeMaterial()
		{
			var mat = new THREE.Materials.MeshPhysicalMaterial();
			base.SetupBaseMaterialSettings(mat);
			return mat;
		}

		protected override MaterialType Type { get { return MaterialType.MeshPhysical; } }

		public MeshPhysicalMaterial() : base() { }
	}
}
