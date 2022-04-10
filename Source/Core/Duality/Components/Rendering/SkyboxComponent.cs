﻿using System;
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
using THREE.Renderers.gl;

namespace Duality.Graphics.Components
{
	[RequiredComponent(typeof(Transform))]
	[EditorHintCategory(CoreResNames.CategoryGraphics)]
	[EditorHintImage(CoreResNames.ImageFragmentShader)]
	public class SkyboxComponent : Component, ICmpInitializable, ICmpUpdatable, ICmpEditorUpdatable, IDisposable
	{

		[DontSerialize] Sky sky;
		[DontSerialize] THREE.Math.Vector3 Sunlight;

		public float turbidity = 10;
		public float rayleigh = 3;
		public float mieCoefficient = 0.005f;
		public float mieDirectionalG = 0.7f;
		public float elevation = 2;
		public float azimuth = 180;

		[EditorHintDecimalPlaces(2), EditorHintIncrement(1.0f), EditorHintRange(1f, 20.0f, 5f, 15.0f)]
		public float Turbidity { get { return this.turbidity; } set { this.turbidity = value; } }

		[EditorHintDecimalPlaces(2), EditorHintIncrement(0.1f), EditorHintRange(0.5f, 5.0f, 1f, 4.0f)]
		public float Rayleigh { get { return this.rayleigh; } set { this.rayleigh = value; } }

		[EditorHintDecimalPlaces(3), EditorHintIncrement(0.1f), EditorHintRange(0.001f, 0.01f, 0.0025f, 0.004f)]
		public float MieCoefficient { get { return this.mieCoefficient; } set { this.mieCoefficient = value; } }

		[EditorHintDecimalPlaces(2), EditorHintIncrement(0.1f), EditorHintRange(0.1f, 1f, 0.3f, 0.7f)]
		public float MieDirectionalG { get { return this.mieDirectionalG; } set { this.mieDirectionalG = value; } }

		[EditorHintDecimalPlaces(2), EditorHintIncrement(0.1f), EditorHintRange(0.1f, 180f, 1f, 10f)]
		public float Elevation { get { return this.elevation; } set { this.elevation = value; } }

		[EditorHintDecimalPlaces(2), EditorHintIncrement(0.1f), EditorHintRange(0.1f, 180f, 1f, 180f)]
		public float Azimuth { get { return this.azimuth; } set { this.azimuth = value; } }

		void ICmpInitializable.OnActivate()
		{
			CreateSkybox();
		}

		void ICmpInitializable.OnDeactivate()
		{
			if (sky != null)
			{
				Scene.ThreeScene.Remove(sky);
				sky.Dispose();
				sky = null;
			}
		}

		void ICmpUpdatable.OnUpdate()
		{
			UpdateSkybox();
		}

		void ICmpEditorUpdatable.OnUpdate()
		{
			UpdateSkybox();
		}

		void UpdateSkybox()
		{
			sky.Position.Set(this.GameObj.Transform.Pos.X, this.GameObj.Transform.Pos.Y, this.GameObj.Transform.Pos.Z);
			sky.Rotation.Set(this.GameObj.Transform.Rotation.X, this.GameObj.Transform.Rotation.Y, this.GameObj.Transform.Rotation.Z, THREE.Math.RotationOrder.XYZ);
			sky.Scale.Set(this.GameObj.Transform.Scale.X, this.GameObj.Transform.Scale.Y, this.GameObj.Transform.Scale.Z);

			(sky.Material as THREE.Materials.ShaderMaterial).Uniforms["turbidity"] = new GLUniform { { "value", Turbidity } };
			(sky.Material as THREE.Materials.ShaderMaterial).Uniforms["rayleigh"] = new GLUniform { { "value", Rayleigh } };
			(sky.Material as THREE.Materials.ShaderMaterial).Uniforms["mieCoefficient"] = new GLUniform { { "value", MieCoefficient } };
			(sky.Material as THREE.Materials.ShaderMaterial).Uniforms["mieDirectionalG"] = new GLUniform { { "value", MieDirectionalG } };

			float phi = MathUtils.DegToRad(90 - Elevation);
			float theta = MathUtils.DegToRad(Azimuth);

			Sunlight.SetFromSphericalCoords(1, phi, theta);
			(sky.Material as THREE.Materials.ShaderMaterial).Uniforms["sunPosition"] = new GLUniform { { "value", Sunlight } };
		}

		void CreateSkybox()
		{
			if (sky != null)
			{
				Scene.ThreeScene.Remove(sky);
				sky.Dispose();
				sky = null;
			}

			sky = new Sky();

			Sunlight = new THREE.Math.Vector3();
			UpdateSkybox();

			Scene.ThreeScene.Add(sky);
		}

		void IDisposable.Dispose()
		{
			if (sky != null)
			{
				Scene.ThreeScene.Remove(sky);
				sky.Dispose();
				sky = null;
			}
		}
	}
}