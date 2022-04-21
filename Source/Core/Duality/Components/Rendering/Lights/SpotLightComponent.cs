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
	public class SpotLightComponent : Component, ICmpInitializable, IDisposable
	{

		[DontSerialize] SpotLight Light;

		private ColorRgba color = ColorRgba.White;
		public ColorRgba Color { get { return this.color; } set { this.color = value; } }

		private float intensity = 1;
		public float Intensity { get { return this.intensity; } set { this.intensity = value; } }

		private float distance = 0;
		public float Distance { get { return this.distance; } set { this.distance = value; } }

		private float decay = 1;
		public float Decay { get { return this.decay; } set { this.decay = value; } }

		private float penumbra = 0.1f;
		public float Penumbra { get { return this.penumbra; } set { this.penumbra = value; } }

		private float angle = 0.4f;
		public float Angle { get { return this.angle; } set { this.angle = value; } }

		private float nearClip = 1;
		public float NearClip { get { return this.nearClip; } set { this.nearClip = value; } }

		private float farClip = 1000;
		public float FarClip { get { return this.farClip; } set { this.farClip = value; } }

		private float fov = 120;
		public float Fov { get { return this.fov; } set { this.fov = value; } }

		private bool castShadow = true;
		public bool CastShadow { get { return this.castShadow; } set { this.castShadow = value; } }

		void ICmpInitializable.OnActivate()
		{
			DualityApp.PreRender += DualityApp_PreRender;
		}

		private void DualityApp_PreRender(Scene scene, Duality.Components.Camera camera)
		{
			if(Light == null)
				CreateLight(camera);

			if(Gizmos.DrawLightGizmos)
				Gizmos.DrawCone(this.GameObj.Transform.Pos, this.GameObj.Transform.Rotation, Distance, Angle, Color);

			Vector3 forward = GameObj.Transform.Forward;
			if (Duality.Resources.Scene.Current.MoveWorldInsteadOfCamera)
			{
				Vector3 Pos = this.GameObj.Transform.Pos - camera.GameObj.Transform.Pos;
				Light.Position.Set((float)Pos.X, (float)Pos.Y, (float)Pos.Z);
				Light.Target.Position.Set((float)Pos.X + (float)forward.X, (float)Pos.Y + (float)forward.Y, (float)Pos.Z + (float)forward.Z);
			}
			else
			{
				Light.Position.Set((float)this.GameObj.Transform.Pos.X, (float)this.GameObj.Transform.Pos.Y, (float)this.GameObj.Transform.Pos.Z);
				Light.Target.Position.Set((float)this.GameObj.Transform.Pos.X + (float)forward.X, (float)this.GameObj.Transform.Pos.Y + (float)forward.Y, (float)this.GameObj.Transform.Pos.Z + (float)forward.Z);
			}

			Light.Color = new THREE.Math.Color(Color.R / 255f, Color.G / 255f, Color.B / 255f);
			Light.Intensity = Intensity;
			Light.Distance = Distance;
			Light.Decay = Decay;
			Light.Penumbra = Penumbra;
			Light.Angle = Angle;

			Light.CastShadow = CastShadow;
			Light.Shadow.Camera.Near = NearClip;
			Light.Shadow.Camera.Far = FarClip;
			Light.Shadow.Camera.Fov = Fov;
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

		void CreateLight(Duality.Components.Camera camera)
		{
			Light = new SpotLight(new Color().SetHex(0xffffff));

			Light.CastShadow = CastShadow;
			Light.Shadow.Camera.Near = NearClip;
			Light.Shadow.Camera.Far = FarClip;
			Light.Shadow.Camera.Fov = Fov;
			Light.Shadow.MapSize.Set(512, 512);

			Vector3 forward = GameObj.Transform.Forward;
			if (Duality.Resources.Scene.Current.MoveWorldInsteadOfCamera)
			{
				Vector3 Pos = this.GameObj.Transform.Pos - camera.GameObj.Transform.Pos;
				Light.Position.Set((float)Pos.X, (float)Pos.Y, (float)Pos.Z);
				Light.Target.Position.Set((float)Pos.X + (float)forward.X, (float)Pos.Y + (float)forward.Y, (float)Pos.Z + (float)forward.Z);
			}
			else
			{
				Light.Position.Set((float)this.GameObj.Transform.Pos.X, (float)this.GameObj.Transform.Pos.Y, (float)this.GameObj.Transform.Pos.Z);
				Light.Target.Position.Set((float)this.GameObj.Transform.Pos.X + (float)forward.X, (float)this.GameObj.Transform.Pos.Y + (float)forward.Y, (float)this.GameObj.Transform.Pos.Z + (float)forward.Z);
			}

			Light.Color = new THREE.Math.Color(Color.R / 255f, Color.G / 255f, Color.B / 255f);
			Light.Intensity = Intensity;
			Light.Distance = Distance;
			Light.Decay = Decay;
			Light.Penumbra = Penumbra;
			Light.Angle = Angle;

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