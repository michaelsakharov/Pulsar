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
using THREE.Helpers;
using THREE.Lights;
using THREE.Math;

namespace Duality.Graphics.Components
{
	[RequiredComponent(typeof(Transform))]
	[EditorHintCategory(CoreResNames.CategoryGraphics)]
	[EditorHintImage(CoreResNames.ImageFragmentShader)]
	public class RectAreaLightComponent : Component, ICmpInitializable, ICmpUpdatable, ICmpEditorUpdatable, IDisposable
	{

		[DontSerialize] RectAreaLight Light;

		private ColorRgba color = ColorRgba.White;
		public ColorRgba Color { get { return this.color; } set { this.color = value; } }

		private float intensity = 0.6f;
		public float Intensity { get { return this.intensity; } set { this.intensity = value; } }

		private int width = 1;
		public int Width { get { return this.width; } set { this.width = value; } }

		private int height = 1;
		public int Height { get { return this.height; } set { this.height = value; } }



		void ICmpInitializable.OnActivate()
		{
			CreateLight();
		}

		void ICmpInitializable.OnDeactivate()
		{
			if (Light != null)
			{
				Scene.ThreeScene.Remove(Light);
				Light.Children[0].Dispose();
				Light.Remove(Light.Children[0]);
				Light.Dispose();
				Light = null;
			}
		}

		void ICmpUpdatable.OnUpdate()
		{
			UpdateLight();
		}

		void ICmpEditorUpdatable.OnUpdate()
		{
			UpdateLight();
		}

		void UpdateLight()
		{
			Light.Position.Set(this.GameObj.Transform.Pos.X, this.GameObj.Transform.Pos.Y, this.GameObj.Transform.Pos.Z);
			Light.Rotation.Set(this.GameObj.Transform.Rotation.X, this.GameObj.Transform.Rotation.Y, this.GameObj.Transform.Rotation.Z, THREE.Math.RotationOrder.XYZ);
			Light.Scale.Set(this.GameObj.Transform.Scale.X, this.GameObj.Transform.Scale.Y, this.GameObj.Transform.Scale.Z);

			Light.Color = new THREE.Math.Color(Color.R / 255f, Color.G / 255f, Color.B / 255f);
			Light.Intensity = Intensity;

			Light.Width = Width;
			Light.Height = Height;
		}

		void CreateLight()
		{
			if(Light != null)
			{
				Scene.ThreeScene.Remove(Light);
				Light.Children[0].Dispose();
				Light.Remove(Light.Children[0]);
				Light.Dispose();
				Light = null;
			}

			Light = new RectAreaLight(new Color(), intensity, Width, Height);

			Light.Position.Set(this.GameObj.Transform.Pos.X, this.GameObj.Transform.Pos.Y, this.GameObj.Transform.Pos.Z);
			Light.Rotation.Set(this.GameObj.Transform.Rotation.X, this.GameObj.Transform.Rotation.Y, this.GameObj.Transform.Rotation.Z, THREE.Math.RotationOrder.YXZ);
			Light.Scale.Set(this.GameObj.Transform.Scale.X, this.GameObj.Transform.Scale.Y, this.GameObj.Transform.Scale.Z);

			Light.Color = new THREE.Math.Color(Color.R / 255f, Color.G / 255f, Color.B / 255f);
			Light.Intensity = Intensity;
			Light.Decay = 2;
			Light.Penumbra = 0.05f;

			Light.Width = Width;
			Light.Height = Height;

			Scene.ThreeScene.Add(Light);
			var helper = new RectAreaLightHelper(Light);
			Light.Add(helper);
		}

		void IDisposable.Dispose()
		{
			if (Light != null)
			{
				Scene.ThreeScene.Remove(Light);
				Light.Children[0].Dispose();
				Light.Remove(Light.Children[0]);
				Light.Dispose();
				Light = null;
			}
		}
	}
}