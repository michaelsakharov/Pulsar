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
	public class MeshLambertMaterial : Material
	{
		public static ContentRef<MeshLambertMaterial> Default { get; private set; }

		internal static void InitDefaultContent()
		{
			DefaultContent.InitType<MeshLambertMaterial>(new Dictionary<string, MeshLambertMaterial>
			{
				{ "Default", new MeshLambertMaterial() }
			});
		}

		// Methods
		public override THREE.Materials.Material GetThreeMaterial()
		{
			var mat = new THREE.Materials.MeshLambertMaterial();
			return mat;
		}

		protected override MaterialType Type { get { return MaterialType.MeshLambert; } }

		public MeshLambertMaterial() : base() { }
	}
}
