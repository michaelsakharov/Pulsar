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
		public bool Orthographic = false;

		[DontSerialize] public Matrix4? CustomViewMatrix = null;

		public float NearClipDistance = 0.1f;
		public float FarClipDistance = 1000f;

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
		/// Projects a <see cref="Vector3"/> from model space into screen space.
		/// The source point is transformed from model space to world space by the world matrix,
		/// then from world space to view space by the view matrix, and
		/// finally from view space to screen space by the projection matrix.
		/// </summary>
		/// <param name="source">The <see cref="Vector3"/> to project.</param>
		/// <param name="projection">The projection <see cref="Matrix4"/>.</param>
		/// <param name="view">The view <see cref="Matrix4"/>.</param>
		/// <param name="world">The world <see cref="Matrix4"/>.</param>
		/// <returns></returns>
		public Vector3 Project(Vector3 source, Matrix4 projection, Matrix4 view, Matrix4 world)
		{
			Matrix4 matrix = Matrix4.Multiply(Matrix4.Multiply(world, view), projection);
			Vector3 vector = Vector3.Transform(source, matrix);
			float a = (((source.X * matrix.M14) + (source.Y * matrix.M24)) + (source.Z * matrix.M34)) + matrix.M44;
			if (!WithinEpsilon(a, 1f))
			{
				vector.X = vector.X / a;
				vector.Y = vector.Y / a;
				vector.Z = vector.Z / a;
			}
			vector.X = (((vector.X + 1f) * 0.5f) * this.Viewport.W) + this.Viewport.X;
			vector.Y = (((-vector.Y + 1f) * 0.5f) * this.Viewport.H) + this.Viewport.Y;
			//vector.Z = (vector.Z * (this.maxDepth - this.minDepth)) + this.minDepth;
			return vector;
		}

		public Vector3 Project(Vector3 source)
		{
			return this.Project(source, GetProjectionMatrix(), GetViewMatrix(), Matrix4.Identity);
		}

		/// <summary>
		/// Unprojects a <see cref="Vector3"/> from screen space into model space.
		/// The source point is transformed from screen space to view space by the inverse of the projection matrix,
		/// then from view space to world space by the inverse of the view matrix, and
		/// finally from world space to model space by the inverse of the world matrix.
		/// Note source.Z must be less than or equal to MaxDepth.
		/// </summary>
		/// <param name="source">The <see cref="Vector3"/> to unproject.</param>
		/// <param name="projection">The projection <see cref="Matrix4"/>.</param>
		/// <param name="view">The view <see cref="Matrix4"/>.</param>
		/// <param name="world">The world <see cref="Matrix4"/>.</param>
		/// <returns></returns>
		public Vector3 Unproject(Vector3 source, Matrix4 projection, Matrix4 view, Matrix4 world)
		{
			Matrix4 matrix = Matrix4.Invert(Matrix4.Multiply(Matrix4.Multiply(world, view), projection));
			source.X = (((source.X - this.Viewport.X) / ((float)this.Viewport.W)) * 2f) - 1f;
			source.Y = -((((source.Y - this.Viewport.Y) / ((float)this.Viewport.H)) * 2f) - 1f);
			//source.Z = (source.Z - this.minDepth) / (this.maxDepth - this.minDepth);
			Vector3 vector = Vector3.Transform(source, matrix);
			float a = (((source.X * matrix.M14) + (source.Y * matrix.M24)) + (source.Z * matrix.M34)) + matrix.M44;
			if (!WithinEpsilon(a, 1f))
			{
				vector.X = vector.X / a;
				vector.Y = vector.Y / a;
				vector.Z = vector.Z / a;
			}
			return vector;

		}

		public Vector3 Unproject(Vector3 source)
		{
			return this.Unproject(source, GetProjectionMatrix(), GetViewMatrix(), Matrix4.Identity);
		}

		private static bool WithinEpsilon(float a, float b)
		{
			float num = a - b;
			return ((-1.401298E-45f <= num) && (num <= float.Epsilon));
		}

		/// <summary>
		/// Creates a Ray translating screen cursor position into screen position
		/// </summary>
		/// <param name="screenPoint"></param>
		/// <returns></returns>
		public Ray CalculateScreenPointRay(Vector2 screenPoint)
		{
			// create 2 positions in screenspace using the cursor position. 0 is as
			// close as possible to the camera, 1 is as far away as possible.
			Vector3 nearSource = new Vector3(screenPoint, 0f);
			Vector3 farSource = new Vector3(screenPoint, 1f);

			// use Viewport.Unproject to tell what those two screen space positions
			// would be in world space. we'll need the projection matrix and view
			// matrix, which we have saved as member variables. We also need a world
			// matrix, which can just be identity.
			Vector3 nearPoint = this.Unproject(nearSource);
			Vector3 farPoint = this.Unproject(farSource);

			// find the direction vector that goes from the nearPoint to the farPoint
			// and normalize it....
			Vector3 direction = farPoint - nearPoint;
			direction.Normalize();

			// and then create a new ray using nearPoint as the source.
			return new Ray(nearPoint, direction);
		}

		/// <summary>
		/// Transforms screen space to world space positions. The screen positions Z coordinate [0, 1] is
		/// interpreted as the interpolated location between the Near and Far clip planes, a Z of 0 is on the near clip plane, a Z of 1 would be on the Far Clip Plane
		/// </summary>
		/// <param name="screenPos"></param>
		public Vector3 GetWorldPos(Vector3 screenPos)
		{
			//Ray ray = this.CalculateScreenPointRay(screenPos.Xy);
			//return ray.Position + (ray.Direction * screenPos.Z);
			return this.Unproject(screenPos);
		}
		/// <summary>
		/// Transforms world space to screen space positions.
		/// </summary>
		/// <param name="worldPos"></param>
		public Vector2 GetScreenPos(Vector3 worldPos)
		{
			var result = this.Project(worldPos);
			return new Vector2(result.X, result.Y);
		}

		/// <summary>
		/// Determines whether a point or sphere is inside the devices viewing frustum,
		/// given a world space position and radius.
		/// </summary>
		/// <param name="worldPos">The points world space position.</param>
		/// <param name="radius">A world space radius around the point.</param>
		public bool IsSphereInView(Vector3 worldPos, float radius)
		{
			BoundingFrustum frustum = new BoundingFrustum(GetViewMatrix() * GetProjectionMatrix());
			var result = frustum.Contains(new BoundingSphere(worldPos, radius));
			return result == ContainmentType.Intersects || result == ContainmentType.Contains;
		}

		/// <summary>
		/// Determines whether a point or sphere is inside the devices viewing frustum,
		/// given a world space position and radius.
		/// </summary>
		/// <param name="center">The points world space position.</param>
		/// <param name="size">A world space size of bounds.</param>
		public bool IsBoundsInView(Vector3 center, Vector3 size)
		{
			BoundingFrustum frustum = new BoundingFrustum(GetViewMatrix() * GetProjectionMatrix());
			var result = frustum.Contains(BoundingBox.CreateFromCenterSize(center, size));
			return result == ContainmentType.Intersects || result == ContainmentType.Contains;
		}

		public void GetUpVector(out Vector3 up)
		{
			up = GameObj.Transform.Up;
		}

		public void GetRightVector(out Vector3 right)
		{
			right = GameObj.Transform.Right;
		}

		public Matrix4 GetViewMatrix()
		{
			GetViewMatrix(out Matrix4 result, false);
			return result;
		}

		public void GetViewMatrix(out Matrix4 viewMatrix, bool noTranslation = false)
		{
			if (CustomViewMatrix != null)
			{
				viewMatrix = CustomViewMatrix.Value;
				return;
			}
			Vector3 position = noTranslation ? Vector3.Zero : GameObj.Transform.Pos;
			////viewMatrix = Matrix4.CreateLookAt(position, position + GameObj.Transform.Forward, GameObj.Transform.Up);
			//viewMatrix = Matrix4.CreateLookAt(position, position + GameObj.Transform.Forward, Vector3.Up);

			var matrix = Matrix4.CreateFromYawPitchRoll(GameObj.Transform.Rotation.Y, this.GameObj.Transform.Rotation.X, this.GameObj.Transform.Rotation.Z);
			var target = (position) + Vector3.Transform(Vector3.Forward, matrix);

			viewMatrix = Matrix4.CreateLookAt(position, target, Vector3.Up);
		}

		public Matrix4 GetProjectionMatrix()
		{
			GetProjectionMatrix(out Matrix4 result);
			return result;
		}

		public void GetProjectionMatrix(out Matrix4 projectionMatrix)
		{
			if (Orthographic)
				Matrix4.CreateOrthographicOffCenter(0.0f, Viewport.X, Viewport.Y, 0.0f, MathF.Max(NearClipDistance, 0.01f), FarClipDistance, out projectionMatrix);
			else
				Matrix4.CreatePerspectiveFieldOfView(MathF.DegToRad(Fov), Viewport.W / Viewport.H, MathF.Max(NearClipDistance, 0.01f), FarClipDistance, out projectionMatrix);
		}

		public BoundingFrustum GetFrustum()
		{
			Matrix4 view, projection;
			
			GetViewMatrix(out view);
			GetProjectionMatrix(out projection);
			
			Frustum.Matrix = view * projection;

			return Frustum;
		}

		public THREE.Cameras.Camera GetTHREECamera()
		{
			THREE.Cameras.Camera camera = new THREE.Cameras.Camera();
			camera.Fov = FieldOfView;
			camera.Aspect = DualityApp.GraphicsBackend.AspectRatio;
			camera.Near = NearZ;
			camera.Far = FarZ;
			camera.Position.X = this.GameObj.Transform.Pos.X;
			camera.Position.Y = this.GameObj.Transform.Pos.Y;
			camera.Position.Z = this.GameObj.Transform.Pos.Z;
			camera.Rotation.Set(this.GameObj.Transform.Rotation.X, this.GameObj.Transform.Rotation.Y, this.GameObj.Transform.Rotation.Z, THREE.Math.RotationOrder.XYZ);
			return camera;
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
