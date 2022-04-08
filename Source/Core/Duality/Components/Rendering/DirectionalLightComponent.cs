using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Duality.Drawing;
using Duality.Resources;
using THREE.Cameras;
using THREE.Lights;
using THREE.Math;

namespace Duality.Graphics.Components
{
	public class DirectionalLightComponent : Component, ICmpInitializable, ICmpUpdatable, ICmpEditorUpdatable, IDisposable
	{

		[DontSerialize] DirectionalLight Light;

		private ColorRgba color = ColorRgba.White;
		public ColorRgba Color { get { return this.color; } set { this.color = value; } }

		private float intensity = 0;
		public float Intensity { get { return this.intensity; } set { this.intensity = value; } }

		void ICmpInitializable.OnActivate()
		{
			CreateLight();
		}

		void ICmpInitializable.OnDeactivate()
		{
			if (Light != null)
			{
				Scene.ThreeScene.Remove(Light);
				Scene.ThreeScene.Remove(Light.Target);
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

			Light.Color = new THREE.Math.Color(Color.R / 255, Color.G / 255, Color.B / 255);
			Light.Intensity = Intensity;

			Vector3 forward = GameObj.Transform.Forward;
			Light.Target.Position.Set(this.GameObj.Transform.Pos.X + forward.X, this.GameObj.Transform.Pos.Y + forward.Y, this.GameObj.Transform.Pos.Z + forward.Z);
		}

		void CreateLight()
		{
			if(Light != null)
			{
				Scene.ThreeScene.Remove(Light);
				Scene.ThreeScene.Remove(Light.Target);
				Light.Dispose();
				Light = null;
			}

			Light = new DirectionalLight(new Color().SetHex(0xffffff));

			Light.CastShadow = true;
			Light.Shadow.Camera.Near = 1;
			Light.Shadow.Camera.Far = 1000;
			Light.Shadow.Camera.Fov = 50;
			(Light.Shadow.Camera as OrthographicCamera).Left = -50;
			(Light.Shadow.Camera as OrthographicCamera).CameraRight = 50;
			(Light.Shadow.Camera as OrthographicCamera).Top = 50;
			(Light.Shadow.Camera as OrthographicCamera).Bottom = -50;
			Light.Shadow.MapSize.Set(2048, 2048);

			Light.Position.Set(this.GameObj.Transform.Pos.X, this.GameObj.Transform.Pos.Y, this.GameObj.Transform.Pos.Z);
			Light.Rotation.Set(this.GameObj.Transform.Rotation.X, this.GameObj.Transform.Rotation.Y, this.GameObj.Transform.Rotation.Z, THREE.Math.RotationOrder.YXZ);
			Light.Scale.Set(this.GameObj.Transform.Scale.X, this.GameObj.Transform.Scale.Y, this.GameObj.Transform.Scale.Z);

			Light.Color = new THREE.Math.Color(Color.R / 255, Color.G / 255, Color.B / 255);
			Light.Intensity = Intensity;

			Vector3 forward = GameObj.Transform.Forward;
			Light.Target.Position.Set(this.GameObj.Transform.Pos.X + forward.X, this.GameObj.Transform.Pos.Y + forward.Y, this.GameObj.Transform.Pos.Z + forward.Z);

			Scene.ThreeScene.Add(Light);
			Scene.ThreeScene.Add(Light.Target);
		}

		void IDisposable.Dispose()
		{
			if (Light != null)
			{
				Scene.ThreeScene.Remove(Light);
				Scene.ThreeScene.Remove(Light.Target);
				Light.Dispose();
				Light = null;
			}
		}
	}
}