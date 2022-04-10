using System;
using System.Collections.Generic;
using System.Linq;

using Duality.IO;
using Duality.Editor;
using Duality.Cloning;
using Duality.Drawing;
using Duality.Resources;
using Duality.Properties;

namespace Duality.Components
{
	/// <summary>
	/// A Camera is responsible for rendering the current <see cref="Duality.Resources.Scene"/>.
	/// </summary>
	[RequiredComponent(typeof(Transform))]
	[EditorHintCategory(CoreResNames.CategoryGraphics)]
	[EditorHintImage(CoreResNames.ImageCamera)]
	public sealed class Camera : Component, ICmpInitializable
	{
		private bool orthographic = false;

		[DontSerialize] public Matrix4? CustomViewMatrix = null;

		public float NearClipDistance = 0.1f;
		public float FarClipDistance = 1000f;
		private float orthographicSize = 1000f;

		public float Fov = 70;


		[DontSerialize] private BoundingFrustum Frustum = new BoundingFrustum(Matrix4.Identity);

		[EditorHintFlags(MemberFlags.Invisible)]
		public Quaternion Orientation
		{
			get
			{
				return this.GameObj.Transform.Quaternion;
			}
		}

		public bool useCustomViewPort = false;
		public Rect CustomViewport = new Rect(0, 0, 800, 600);

		[EditorHintFlags(MemberFlags.Invisible)]
		public Rect Viewport
		{
			get
			{
				if (useCustomViewPort)
					return CustomViewport;
				return new Rect(0, 0, DualityApp.WindowSize.X, DualityApp.WindowSize.Y);
			}
		}

		/// <summary>
		/// [GET / SET] The lowest Z value that can be displayed by the device.
		/// </summary>
		[EditorHintDecimalPlaces(0)]
		[EditorHintIncrement(1.0f)]
		[EditorHintRange(0.01f, 1000000.0f, 0.2f, 5.0f)]
		public float NearZ
		{
			get { return this.NearClipDistance; }
			set { this.NearClipDistance = value; }
		}
		/// <summary>
		/// [GET / SET] The highest Z value that can be displayed by the device.
		/// </summary>
		[EditorHintDecimalPlaces(0)]
		[EditorHintIncrement(100.0f)]
		[EditorHintRange(0.02f, float.MaxValue, 5.0f, 100000.0f)]
		public float FarZ
		{
			get { return this.FarClipDistance; }
			set { this.FarClipDistance = value; }
		}
		/// <summary>
		/// [GET / SET] Reference distance for calculating the view projection. When using <see cref="ProjectionMode.Perspective"/>, 
		/// an object this far away from the Camera will always appear in its original size and without offset.
		/// </summary>
		[EditorHintDecimalPlaces(1)]
		[EditorHintIncrement(1.0f)]
		[EditorHintRange(1.0f, 245, 10.0f, 100.0f)]
		public float FieldOfView
		{
			get { return this.Fov; }
			set { this.Fov = MathF.Max(value, 1f); }
		}
		/// <summary>
		/// [GET / SET] Orthographic projection mode size
		/// </summary>
		[EditorHintDecimalPlaces(1)]
		[EditorHintIncrement(1.0f)]
		[EditorHintRange(0.1f, 1000, 1.0f, 100.0f)]
		public float OrthographicSize
		{
			get { return this.orthographicSize; }
			set { this.orthographicSize = value; }
		}
		/// <summary>
		/// [GET / SET] The projection mode
		/// </summary>
		public bool Orthographic
		{
			get { return this.orthographic; }
			set { this.orthographic = value; }
		}

		public THREE.Cameras.Camera GetTHREECamera()
		{
			if (Orthographic == false)
			{
				THREE.Cameras.PerspectiveCamera camera = new THREE.Cameras.PerspectiveCamera();
				camera.Fov = FieldOfView;
				camera.Aspect = DualityApp.GraphicsBackend.AspectRatio;
				camera.Near = NearZ;
				camera.Far = FarZ;
				camera.Position.X = this.GameObj.Transform.Pos.X;
				camera.Position.Y = this.GameObj.Transform.Pos.Y;
				camera.Position.Z = this.GameObj.Transform.Pos.Z;
				camera.Rotation.Set(this.GameObj.Transform.Rotation.X, this.GameObj.Transform.Rotation.Y, this.GameObj.Transform.Rotation.Z, THREE.Math.RotationOrder.YXZ);
				camera.UpdateProjectionMatrix();
				return camera;
			}
			else
			{
				THREE.Cameras.OrthographicCamera camera = new THREE.Cameras.OrthographicCamera();

				camera.Left = -OrthographicSize;
				camera.CameraRight = OrthographicSize;
				camera.Top = OrthographicSize;
				camera.Bottom = -OrthographicSize;

				camera.Fov = FieldOfView;
				camera.Aspect = DualityApp.GraphicsBackend.AspectRatio;
				camera.Near = NearZ;
				camera.Far = FarZ;
				camera.Position.X = this.GameObj.Transform.Pos.X;
				camera.Position.Y = this.GameObj.Transform.Pos.Y;
				camera.Position.Z = this.GameObj.Transform.Pos.Z;
				camera.Rotation.Set(this.GameObj.Transform.Rotation.X, this.GameObj.Transform.Rotation.Y, this.GameObj.Transform.Rotation.Z, THREE.Math.RotationOrder.YXZ);
				camera.UpdateProjectionMatrix();
				return camera;
			}
		}

		void ICmpInitializable.OnActivate()
		{
			if(DualityApp.ExecContext == DualityApp.ExecutionContext.Game)
			{
				Scene.Camera = this;
			}
		}
		void ICmpInitializable.OnDeactivate()
		{
		}
	}
}
