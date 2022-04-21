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

namespace Duality.Graphics.Components
{
	[RequiredComponent(typeof(Transform))]
	[EditorHintCategory(CoreResNames.CategoryGraphics)]
	[EditorHintImage(CoreResNames.ImageFragmentShader)]
	public class HemisphereLightComponent : Component, ICmpInitializable, IDisposable
	{

		[DontSerialize] HemisphereLight Light;

		private ColorRgba color = ColorRgba.Blue;
		public ColorRgba Color { get { return this.color; } set { this.color = value; } }

		private ColorRgba groundColor = ColorRgba.Green;
		public ColorRgba GroundColor { get { return this.groundColor; } set { this.groundColor = value; } }

		private float intensity = 0.6f;
		public float Intensity { get { return this.intensity; } set { this.intensity = value; } }

		void ICmpInitializable.OnActivate()
		{
			CreateLight();
			DualityApp.PreRender += DualityApp_PreRender;
		}

		private void DualityApp_PreRender(Scene scene, Duality.Components.Camera camera)
		{
			//if (Duality.Resources.Scene.Current.MoveWorldInsteadOfCamera)
			//{
			//	Vector3 Pos = this.GameObj.Transform.Pos - camera.GameObj.Transform.Pos;
			//	Light.Position.Set((float)Pos.X, (float)Pos.Y, (float)Pos.Z);
			//}
			//else
			//{
				Light.Position.Set((float)this.GameObj.Transform.Pos.X, (float)this.GameObj.Transform.Pos.Y, (float)this.GameObj.Transform.Pos.Z);
			//}

			Light.Rotation.Set((float)this.GameObj.Transform.Rotation.X, (float)this.GameObj.Transform.Rotation.Y, (float)this.GameObj.Transform.Rotation.Z, THREE.Math.RotationOrder.XYZ);
			Light.Scale.Set((float)this.GameObj.Transform.Scale.X, (float)this.GameObj.Transform.Scale.Y, (float)this.GameObj.Transform.Scale.Z);

			Light.Color = new THREE.Math.Color(Color.R / 255f, Color.G / 255f, Color.B / 255f);
			Light.GroundColor = new THREE.Math.Color(GroundColor.R / 255f, GroundColor.G / 255f, GroundColor.B / 255f);
			Light.Intensity = Intensity;
		}

		void ICmpInitializable.OnDeactivate()
		{
			if (Light != null)
			{
				Scene.ThreeScene.Remove(Light);
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
				Light.Dispose();
				Light = null;
			}

			Light = new HemisphereLight(new Color(), new Color(), intensity);

			Light.Position.Set((float)this.GameObj.Transform.Pos.X, (float)this.GameObj.Transform.Pos.Y, (float)this.GameObj.Transform.Pos.Z);
			Light.Rotation.Set((float)this.GameObj.Transform.Rotation.X, (float)this.GameObj.Transform.Rotation.Y, (float)this.GameObj.Transform.Rotation.Z, THREE.Math.RotationOrder.YXZ);
			Light.Scale.Set((float)this.GameObj.Transform.Scale.X, (float)this.GameObj.Transform.Scale.Y, (float)this.GameObj.Transform.Scale.Z);

			Light.Color = new THREE.Math.Color(Color.R / 255f, Color.G / 255f, Color.B / 255f);
			Light.GroundColor = new THREE.Math.Color(GroundColor.R / 255f, GroundColor.G / 255f, GroundColor.B / 255f);
			Light.Intensity = Intensity;

			Scene.ThreeScene.Add(Light);
		}

		void IDisposable.Dispose()
		{
			if (Light != null)
			{
				Scene.ThreeScene.Remove(Light);
				Light.Dispose();
				Light = null;
			}
		}
	}
}