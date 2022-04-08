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
	public class MeshDistanceMaterial : Material
	{
		public static ContentRef<MeshDistanceMaterial> Default { get; private set; }

		internal static void InitDefaultContent()
		{
			DefaultContent.InitType<MeshDistanceMaterial>(new Dictionary<string, MeshDistanceMaterial>
			{
				{ "Default", new MeshDistanceMaterial() }
			});
		}

		public Vector3 referencePosition;
		public float nearDistance;
		public float farDistance;

		// Public Variables also shown in Editor
		public Vector3 ReferencePosition { get { return this.referencePosition; } set { this.referencePosition = value; } }
		public float NearDistance { get { return this.nearDistance; } set { this.nearDistance = value; } }
		public float FarDistance { get { return this.farDistance; } set { this.farDistance = value; } }

		// Methods
		public override THREE.Materials.Material GetThreeMaterial()
		{
			var mat = new THREE.Materials.MeshDistanceMaterial();
			mat.ReferencePosition = new THREE.Math.Vector3(ReferencePosition.X, ReferencePosition.Y, ReferencePosition.Z);
			mat.NearDistance = NearDistance;
			mat.FarDistance = FarDistance;
			return mat;
		}

		protected override MaterialType Type { get { return MaterialType.MeshDistance; } }

		public MeshDistanceMaterial() : base() { }
	}
}
