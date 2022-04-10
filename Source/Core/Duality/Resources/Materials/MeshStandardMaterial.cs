﻿using System;
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
	public class MeshStandardMaterial : Material
	{
		public static ContentRef<MeshStandardMaterial> Default { get; private set; }

		internal static void InitDefaultContent()
		{
			DefaultContent.InitType<MeshStandardMaterial>(new Dictionary<string, MeshStandardMaterial>
			{
				{ "Default", new MeshStandardMaterial() }
			});
		}

		// Methods
		public override THREE.Materials.Material GetThreeMaterial()
		{
			var mat = new THREE.Materials.MeshStandardMaterial();
			base.SetupBaseMaterialSettings(mat);
			return mat;
		}

		protected override MaterialType Type { get { return MaterialType.MeshStandard; } }

		public MeshStandardMaterial() : base() { }
	}
}