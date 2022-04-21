using System;
using System.Collections.Generic;
using System.Linq;

using Duality.IO;
using Duality.Editor;
using Duality.Cloning;
using Duality.Drawing;
using Duality.Resources;
using Duality.Properties;
using Duality.Postprocessing;

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
		[DontSerialize] private bool isDirty = false;

		private bool orthographic = false;
		private bool isMainCamera = false;
		internal bool isEditorCamera = false;

		[DontSerialize] public Matrix4? CustomViewMatrix = null;

		private float NearClipDistance = 0.1f;
		private float FarClipDistance = 1000f;
		private float orthographicSize = 1000f;

		private float Fov = 70;

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
			set { this.Fov = (float)MathF.Max(value, 1f); }
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
			set { this.orthographic = value; isDirty = true; }
		}
		/// <summary>
		/// [GET / SET] If this camera should be considered the Main Camera in the scene, The main camera will be returned when doing Scene.Camera and is used as the World Origin if Scene.MoveWorldInsteadOfCamera Is set to true
		/// </summary>
		public bool IsMainCamera
		{
			get { return this.isMainCamera; }
			set { this.isMainCamera = value; isDirty = true; }
		}

		[DontSerialize] private EffectComposer composer;

		void SetupComposer()
		{
			composer = new EffectComposer();
			composer.AddPass(new RenderPass());
			DualityApp.OnResize += DualityApp_OnResize;
		}

		private void DualityApp_OnResize(int width, int height, float pixelRatio)
		{
			composer.SetPixelRatioAndSize(width, height, pixelRatio);
		}

		public void AddPass(Pass pass)
		{
			if (composer == null) SetupComposer();
			composer.AddPass(pass);
		}

		public void RemovePass(Pass pass)
		{
			if (composer == null) SetupComposer();
			composer.Passes.Remove(pass);
		}

		public void Render()
		{
			if (DualityApp.GraphicsBackend == null) return;
			if (composer == null) SetupComposer();

			// Update Gizmos, so any call to gizmos will be processed
			DualityApp.Gizmos.Update(Scene.ThreeScene, this);

			composer.Render(Scene.ThreeScene, GetTHREECamera());
			//DualityApp.GraphicsBackend.Render(Scene.ThreeScene, GetTHREECamera());
		}

		[DontSerialize] private THREE.Cameras.Camera cachedCamera;

		public THREE.Cameras.Camera GetTHREECamera()
		{
			if (cachedCamera == null || isDirty)
			{
				// Recreate the Camera
				isDirty = false;
				if (cachedCamera != null)
				{
					cachedCamera.Dispose();
					cachedCamera = null;
				}

				if (Orthographic == false)
				{
					cachedCamera = new THREE.Cameras.PerspectiveCamera();
				}
				else
				{
					cachedCamera = new THREE.Cameras.OrthographicCamera();
				}
			}

			// Update Cached Camera, and Return it
			if (cachedCamera is THREE.Cameras.OrthographicCamera)
			{
				cachedCamera.Left = -OrthographicSize;
				cachedCamera.CameraRight = OrthographicSize;
				cachedCamera.Top = OrthographicSize;
				cachedCamera.Bottom = -OrthographicSize;
			}
			cachedCamera.Fov = FieldOfView;
			cachedCamera.Aspect = DualityApp.GraphicsBackend.AspectRatio;
			cachedCamera.Near = NearZ;
			cachedCamera.Far = FarZ;
			if (Scene.Current.MoveWorldInsteadOfCamera)
			{
				// Keep Camera at 0, 0, 0, we will be moving the Mesh's and other rendered objects instead
				cachedCamera.Position.X = 0;
				cachedCamera.Position.Y = 0;
				cachedCamera.Position.Z = 0;
			}
			else
			{
				cachedCamera.Position.X = (float)this.GameObj.Transform.Pos.X;
				cachedCamera.Position.Y = (float)this.GameObj.Transform.Pos.Y;
				cachedCamera.Position.Z = (float)this.GameObj.Transform.Pos.Z;
			}
			cachedCamera.Rotation.Set((float)this.GameObj.Transform.Rotation.X, (float)this.GameObj.Transform.Rotation.Y, (float)this.GameObj.Transform.Rotation.Z, THREE.Math.RotationOrder.YXZ);
			cachedCamera.UpdateProjectionMatrix();
			return cachedCamera;
		}

		void ICmpInitializable.OnActivate()
		{
			if(DualityApp.ExecContext == DualityApp.ExecutionContext.Game)
			{
				if (IsMainCamera)
				{
					if(Scene.Camera != null && Scene.Camera.IsMainCamera)
					{
						Logs.Core.WriteWarning("There is multiple Main Cameras! Only one will work! Please make sure theres only one Main Camera in your scene!");
						return;
					}
					Scene.Camera = this;
				}
				else if (Scene.Camera == null)
				{
					// no camera is being used for the scene so just use us for now, a Main Camera may overwrite this later
					Scene.Camera = this;
				}
			}
		}
		void ICmpInitializable.OnDeactivate()
		{
			isDirty = true;
		}
	}
}
