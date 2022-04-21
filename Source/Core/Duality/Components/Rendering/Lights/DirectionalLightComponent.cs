using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Duality.Components;
using Duality.DebugDraw;
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
	public class DirectionalLightComponent : Component, ICmpInitializable, IDisposable
	{

		[DontSerialize] DirectionalLight Light;

		private ColorRgba color = ColorRgba.White;
		public ColorRgba Color { get { return this.color; } set { this.color = value; } }

		private float intensity = 1;
		public float Intensity { get { return this.intensity; } set { this.intensity = value; } }

		private float size = 50;
		public float Size { get { return this.size; } set { this.size = value; } }

		private float nearClip = 1;
		public float NearClip { get { return this.nearClip; } set { this.nearClip = value; } }

		private float farClip = 1000;
		public float FarClip { get { return this.farClip; } set { this.farClip = value; } }

		private bool castShadow = true;
		public bool CastShadow { get { return this.castShadow; } set { this.castShadow = value; } }

		void ICmpInitializable.OnActivate()
		{
			CreateLight();
			DualityApp.PreRender += DualityApp_PreRender;
		}

		private void DualityApp_PreRender(Scene scene, Duality.Components.Camera camera)
		{
			if (Gizmos.DrawLightGizmos)
				Gizmos.DrawDirectionalLight(this.GameObj.Transform.Pos, this.GameObj.Transform.Rotation, Size, NearClip, FarClip, Color);

			if (Duality.Resources.Scene.Current.MoveWorldInsteadOfCamera)
			{
				Light.Position.Set((float)this.GameObj.Transform.RelativePosition.X, (float)this.GameObj.Transform.RelativePosition.Y, (float)this.GameObj.Transform.RelativePosition.Z);
			}
			else
			{
				Light.Position.Set((float)this.GameObj.Transform.Pos.X, (float)this.GameObj.Transform.Pos.Y, (float)this.GameObj.Transform.Pos.Z);
			}
			Light.Rotation.Set((float)this.GameObj.Transform.Rotation.X, (float)this.GameObj.Transform.Rotation.Y, (float)this.GameObj.Transform.Rotation.Z, THREE.Math.RotationOrder.XYZ);
			Light.Scale.Set((float)this.GameObj.Transform.Scale.X, (float)this.GameObj.Transform.Scale.Y, (float)this.GameObj.Transform.Scale.Z);

			Light.Color = new THREE.Math.Color(Color.R / 255f, Color.G / 255f, Color.B / 255f);
			Light.Intensity = Intensity;

			Vector3 forward = GameObj.Transform.Forward;
			Light.Target.Position.Set((float)this.GameObj.Transform.Pos.X + (float)forward.X, (float)this.GameObj.Transform.Pos.Y + (float)forward.Y, (float)this.GameObj.Transform.Pos.Z + (float)forward.Z);

			Light.CastShadow = CastShadow;
			Light.Shadow.Camera.Near = NearClip;
			Light.Shadow.Camera.Far = FarClip;
			//Light.Shadow.Camera.Fov = 50;
			if (Size != (Light.Shadow.Camera as OrthographicCamera).Top)
			{
				(Light.Shadow.Camera as OrthographicCamera).Left = -Size;
				(Light.Shadow.Camera as OrthographicCamera).CameraRight = Size;
				(Light.Shadow.Camera as OrthographicCamera).Top = Size;
				(Light.Shadow.Camera as OrthographicCamera).Bottom = -Size;
				(Light.Shadow.Camera as OrthographicCamera).UpdateProjectionMatrix();
			}
			//Light.Shadow.MapSize.Set(2048, 2048);
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
			DualityApp.PreRender -= DualityApp_PreRender;
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

			Light.CastShadow = CastShadow;
			Light.Shadow.Camera.Near = nearClip;
			Light.Shadow.Camera.Far = FarClip;
			//Light.Shadow.Camera.Fov = 50;
			(Light.Shadow.Camera as OrthographicCamera).Left = -Size;
			(Light.Shadow.Camera as OrthographicCamera).CameraRight = Size;
			(Light.Shadow.Camera as OrthographicCamera).Top = Size;
			(Light.Shadow.Camera as OrthographicCamera).Bottom = -Size;
			Light.Shadow.MapSize.Set(2048, 2048);

			Light.Position.Set((float)this.GameObj.Transform.Pos.X, (float)this.GameObj.Transform.Pos.Y, (float)this.GameObj.Transform.Pos.Z);
			Light.Rotation.Set((float)this.GameObj.Transform.Rotation.X, (float)this.GameObj.Transform.Rotation.Y, (float)this.GameObj.Transform.Rotation.Z, THREE.Math.RotationOrder.YXZ);
			Light.Scale.Set((float)this.GameObj.Transform.Scale.X, (float)this.GameObj.Transform.Scale.Y, (float)this.GameObj.Transform.Scale.Z);

			Light.Color = new THREE.Math.Color(Color.R / 255f, Color.G / 255f, Color.B / 255f);
			Light.Intensity = Intensity;

			Vector3 forward = GameObj.Transform.Forward;
			Light.Target.Position.Set((float)this.GameObj.Transform.Pos.X + (float)forward.X, (float)this.GameObj.Transform.Pos.Y + (float)forward.Y, (float)this.GameObj.Transform.Pos.Z + (float)forward.Z);

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