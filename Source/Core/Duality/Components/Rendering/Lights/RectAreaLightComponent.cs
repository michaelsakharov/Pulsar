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
	public class RectAreaLightComponent : Component, ICmpInitializable, IDisposable
	{

		[DontSerialize] RectAreaLight Light;

		private ColorRgba color = ColorRgba.White;
		public ColorRgba Color { get { return this.color; } set { this.color = value; } }

		private float intensity = 0.6f;
		public float Intensity { get { return this.intensity; } set { this.intensity = value; } }

		private float decay = 1;
		public float Decay { get { return this.decay; } set { this.decay = value; } }

		private float penumbra = 0.1f;
		public float Penumbra { get { return this.penumbra; } set { this.penumbra = value; } }

		private int width = 1;
		public int Width { get { return this.width; } set { this.width = value; } }

		private int height = 1;
		public int Height { get { return this.height; } set { this.height = value; } }



		void ICmpInitializable.OnActivate()
		{
			CreateLight();
			DualityApp.PreRender += DualityApp_PreRender;
		}

		private void DualityApp_PreRender(Scene scene, Duality.Components.Camera camera)
		{
			if (Duality.Resources.Scene.Current.MoveWorldInsteadOfCamera)
			{
				Vector3 Pos = this.GameObj.Transform.Pos - camera.GameObj.Transform.Pos;
				Light.Position.Set((float)Pos.X, (float)Pos.Y, (float)Pos.Z);
			}
			else
			{
				Light.Position.Set((float)this.GameObj.Transform.Pos.X, (float)this.GameObj.Transform.Pos.Y, (float)this.GameObj.Transform.Pos.Z);
			}
			Light.Rotation.Set((float)this.GameObj.Transform.Rotation.X, (float)this.GameObj.Transform.Rotation.Y, (float)this.GameObj.Transform.Rotation.Z, THREE.Math.RotationOrder.XYZ);
			Light.Scale.Set((float)this.GameObj.Transform.Scale.X, (float)this.GameObj.Transform.Scale.Y, (float)this.GameObj.Transform.Scale.Z);

			Light.Color = new THREE.Math.Color(Color.R / 255f, Color.G / 255f, Color.B / 255f);
			Light.Intensity = Intensity;
			Light.Decay = Decay;
			Light.Penumbra = Penumbra;

			Light.Width = Width;
			Light.Height = Height;
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
			DualityApp.PreRender -= DualityApp_PreRender;
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

			Light.Position.Set((float)this.GameObj.Transform.Pos.X, (float)this.GameObj.Transform.Pos.Y, (float)this.GameObj.Transform.Pos.Z);
			Light.Rotation.Set((float)this.GameObj.Transform.Rotation.X, (float)this.GameObj.Transform.Rotation.Y, (float)this.GameObj.Transform.Rotation.Z, THREE.Math.RotationOrder.YXZ);
			Light.Scale.Set((float)this.GameObj.Transform.Scale.X, (float)this.GameObj.Transform.Scale.Y, (float)this.GameObj.Transform.Scale.Z);

			Light.Color = new THREE.Math.Color(Color.R / 255f, Color.G / 255f, Color.B / 255f);
			Light.Intensity = Intensity;
			Light.Decay = Decay;
			Light.Penumbra = Penumbra;

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