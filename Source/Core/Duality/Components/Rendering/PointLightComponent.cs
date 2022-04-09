﻿using System;
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
	public class PointLightComponent : Component, ICmpInitializable, ICmpUpdatable, ICmpEditorUpdatable, IDisposable
	{

		[DontSerialize] PointLight Light;

		private ColorRgba color = ColorRgba.White;
		public ColorRgba Color { get { return this.color; } set { this.color = value; } }

		private float intensity = 1;
		public float Intensity { get { return this.intensity; } set { this.intensity = value; } }

		private float distance = 100;
		public float Distance { get { return this.distance; } set { this.distance = value; } }

		private float decay = 1;
		public float Decay { get { return this.decay; } set { this.decay = value; } }

		private float nearClip = 1;
		public float NearClip { get { return this.nearClip; } set { this.nearClip = value; } }

		private float farClip = 1000;
		public float FarClip { get { return this.farClip; } set { this.farClip = value; } }

		private bool castShadow = true;
		public bool CastShadow { get { return this.castShadow; } set { this.castShadow = value; } }

		void ICmpInitializable.OnActivate()
		{
			CreateLight();
		}

		void ICmpInitializable.OnDeactivate()
		{
			if (Light != null)
			{
				Scene.ThreeScene.Remove(Light);
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

			Light.Color = new THREE.Math.Color(Color.R / 255f, Color.G / 255f, Color.B / 255f);
			Light.Intensity = Intensity;
			Light.Distance = Distance;
			Light.Decay = Decay;

			Light.CastShadow = CastShadow;
			Light.Shadow.Camera.Near = NearClip;
			Light.Shadow.Camera.Far = FarClip;
			//Light.Shadow.MapSize.Set(2048, 2048);
		}

		void CreateLight()
		{
			if(Light != null)
			{
				Scene.ThreeScene.Remove(Light);
				Light.Dispose();
				Light = null;
			}

			Light = new PointLight(new Color().SetHex(0xffffff));

			Light.CastShadow = CastShadow;
			Light.Shadow.Camera.Near = NearClip;
			Light.Shadow.Camera.Far = FarClip;
			Light.Shadow.MapSize.Set(512, 512);

			Light.Position.Set(this.GameObj.Transform.Pos.X, this.GameObj.Transform.Pos.Y, this.GameObj.Transform.Pos.Z);

			Light.Color = new THREE.Math.Color(Color.R / 255f, Color.G / 255f, Color.B / 255f);
			Light.Intensity = Intensity;
			Light.Distance = Distance;
			Light.Decay = Decay;

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