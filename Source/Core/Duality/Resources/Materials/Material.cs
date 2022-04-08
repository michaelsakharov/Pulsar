using System;
using System.Collections.Generic;
using System.Linq;

using Duality.Drawing;
using Duality.Properties;
using Duality.Editor;
using Duality.Cloning;
using Duality.Components;

using THREE.Materials;

namespace Duality.Resources
{
	[EditorHintCategory(CoreResNames.CategoryGraphics)]
	[EditorHintImage(CoreResNames.ImageMaterial)]
	[ExplicitResourceReference()]
	public abstract class Material : Resource
	{
		public abstract THREE.Materials.Material GetThreeMaterial();

		/// <summary>
		/// The Material type
		/// </summary>
		protected abstract MaterialType Type { get; }

		/// <summary>
		/// Creates a new Material
		/// </summary>
		public Material()
		{
		}
		
	}
}
