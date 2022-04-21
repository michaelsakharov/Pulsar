using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Duality.Components;
using Duality.Drawing;
using Duality.Editor;
using Duality.Properties;
using Duality.Resources;
using THREE.Cameras;
using THREE.Lights;
using THREE.Math;
using THREE.Objects;

namespace Duality.Graphics.Components
{
	[RequiredComponent(typeof(Transform))]
	[EditorHintCategory(CoreResNames.CategoryGraphics)]
	[EditorHintImage(CoreResNames.ImageFragmentShader)]
	public class LensflareComponent : Component, ICmpInitializable, ICmpUpdatable, ICmpEditorUpdatable, IDisposable
	{
		[DontSerialize] Lensflare Lensflare;
		[DontSerialize] bool _isDirty;

		private FlareElement[] lensflareElements = new FlareElement[0];
		public FlareElement[] LensflareElements
		{
			get { return lensflareElements; }
			set { lensflareElements = value; _isDirty = true; }
		}

		void ICmpInitializable.OnActivate()
		{
			CreateLensflare();
		}

		void ICmpInitializable.OnDeactivate()
		{
			if (Lensflare != null)
			{
				Scene.ThreeScene.Remove(Lensflare);
				Lensflare.Dispose();
				Lensflare = null;
			}
		}

		void ICmpUpdatable.OnUpdate()
		{
			UpdateLensflare();
		}

		void ICmpEditorUpdatable.OnUpdate()
		{
			UpdateLensflare();
		}

		void UpdateLensflare()
		{
			if (Lensflare != null)
			{
				Lensflare.Position.Set((float)this.GameObj.Transform.Pos.X, (float)this.GameObj.Transform.Pos.Y, (float)this.GameObj.Transform.Pos.Z);
				Lensflare.Rotation.Set((float)this.GameObj.Transform.Rotation.X, (float)this.GameObj.Transform.Rotation.Y, (float)this.GameObj.Transform.Rotation.Z, THREE.Math.RotationOrder.XYZ);
				Lensflare.Scale.Set((float)this.GameObj.Transform.Scale.X, (float)this.GameObj.Transform.Scale.Y, (float)this.GameObj.Transform.Scale.Z);
			}

			if (_isDirty)
			{
				_isDirty = false;
				// Recreate the Flare
				CreateLensflare();
			}
		}

		void CreateLensflare()
		{
			bool isAllValid = LensflareElements != null && LensflareElements.Length > 0;
			foreach(var element in LensflareElements)
			{
				if (element == null || element.Texture == null || element.Texture.IsAvailable == false)
				{
					isAllValid = false;
					_isDirty = true;
					return;
				}
			}

			if (Lensflare != null)
			{
				Scene.ThreeScene.Remove(Lensflare);
				Lensflare.Dispose();
				Lensflare = null;
			}

			if (isAllValid)
			{
				Lensflare = new Lensflare();
				foreach (var element in LensflareElements)
					Lensflare.AddElement(new LensflareElement(element.Texture.Res.ThreeTexture, element.Size, element.Distance, new Color(element.Color.R / 255f, element.Color.G / 255f, element.Color.B / 255f)));

				Lensflare.Position.Set((float)this.GameObj.Transform.Pos.X, (float)this.GameObj.Transform.Pos.Y, (float)this.GameObj.Transform.Pos.Z);
				Lensflare.Rotation.Set((float)this.GameObj.Transform.Rotation.X, (float)this.GameObj.Transform.Rotation.Y, (float)this.GameObj.Transform.Rotation.Z, THREE.Math.RotationOrder.XYZ);
				Lensflare.Scale.Set((float)this.GameObj.Transform.Scale.X, (float)this.GameObj.Transform.Scale.Y, (float)this.GameObj.Transform.Scale.Z);

				Scene.ThreeScene.Add(Lensflare);
			}
		}

		void IDisposable.Dispose()
		{
			if (Lensflare != null)
			{
				Scene.ThreeScene.Remove(Lensflare);
				Lensflare.Dispose();
				Lensflare = null;
			}
		}
	}

	[Serializable]
	public class FlareElement
	{
		public ContentRef<Texture> Texture;
		public float Size;
		public float Distance;
		public ColorRgba Color;
	}
}