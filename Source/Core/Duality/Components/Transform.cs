using System;

using Duality.Editor;
using Duality.Properties;
using Duality.Cloning;

namespace Duality.Components
{
	/// <summary>
	/// Represents the location, rotation and scale of a <see cref="GameObject"/>, relative to its <see cref="GameObject.Parent"/>.
	/// </summary>
	[ManuallyCloned]
	[EditorHintCategory(CoreResNames.CategoryNone)]
	[EditorHintImage(CoreResNames.ImageTransform)]
	public sealed class Transform : Component, ICmpAttachmentListener, ICmpSerializeListener
	{
		private const float MinScale = 0.0000001f;

		private Vector3   pos             = Vector3.Zero;
		private float     angle           = 0.0f;
		private Vector3   scale           = Vector3.One;
		private bool      ignoreParent    = false;

		// Cached values, recalc on change
		private Vector3   posAbs          = Vector3.Zero;
		private float     angleAbs        = 0.0f;
		private Vector3   scaleAbs        = Vector3.One;

		[DontSerialize] private Vector2 rotationDirAbs = new Vector2(0.0f, -1.0f);


		/// <summary>
		/// [GET / SET] The objects position in local space of its parent object.
		/// </summary>
		public Vector3 LocalPos
		{
			get { return this.pos; }
			set
			{ 
				// Update position
				this.pos = value;
				this.UpdateAbs();
				this.ResetVelocity();
			}
		}
		/// <summary>
		/// [GET / SET] The objects angle / rotation in local space of its parent object, in radians.
		/// </summary>
		public float LocalRotation
		{
			get { return this.angle; }
			set 
			{ 
				// Update angle
				this.angle = MathF.NormalizeAngle(value);
				this.UpdateAbs();
				this.ResetAngleVelocity();
			}
		}
		/// <summary>
		/// [GET / SET] The objects scale in local space of its parent object.
		/// </summary>
		public Vector3 LocalScale
		{
			get { return this.scale; }
			set
			{
				this.scale.X = MathF.Max(value.X, MinScale);
				this.scale.Y = MathF.Max(value.Y, MinScale);
				this.scale.Z = MathF.Max(value.Z, MinScale);
				this.UpdateAbs();
			}
		}
		
		/// <summary>
		/// [GET / SET] The objects position in world space.
		/// </summary>
		public Vector3 Pos
		{
			get { return this.posAbs; }
			set 
			{ 
				// Update position
				this.posAbs = value;

				Transform parent = this.ParentTransform;
				if (parent != null)
				{
					this.pos = this.posAbs;
					Vector3.Subtract(ref this.pos, ref parent.posAbs, out this.pos);
					Vector3.Divide(ref this.pos, ref parent.scaleAbs, out this.pos);
					MathF.TransformCoord(ref this.pos.X, ref this.pos.Y, -parent.angleAbs);
				}
				else
				{
					this.pos = this.posAbs;
				}

				this.UpdateAbsChild();
				this.ResetVelocity();
			}
		}
		/// <summary>
		/// [GET / SET] The objects angle / rotation in world space, in radians.
		/// </summary>
		public float Rotation
		{
			get { return this.angleAbs; }
			set 
			{ 
				// Update angle
				this.angleAbs = MathF.NormalizeAngle(value);

				Transform parent = this.ParentTransform;
				if (parent != null)
					this.angle = MathF.NormalizeAngle(this.angleAbs - parent.angleAbs);
				else
					this.angle = this.angleAbs;

				this.UpdateRotationDirAbs();
				this.UpdateAbsChild();
				this.ResetAngleVelocity();
			}
		}
		/// <summary>
		/// [GET / SET] The objects scale in world space.
		/// </summary>
		public Vector3 Scale
		{
			get { return this.scaleAbs; }
			set 
			{ 
				this.scaleAbs.X = MathF.Max(value.X, MinScale);
				this.scaleAbs.Y = MathF.Max(value.Y, MinScale);
				this.scaleAbs.Z = MathF.Max(value.Z, MinScale);

				Transform parent = this.ParentTransform;
				if (parent != null)
					this.scale = this.scaleAbs / parent.scaleAbs;
				else
					this.scale = value;

				this.UpdateAbsChild();
			}
		}
		/// <summary>
		/// [GET] The objects directional forward (zero degree angle) vector in world space.
		/// </summary>
		public Vector3 Forward
		{
			get 
			{ 
				return new Vector3(
					this.rotationDirAbs.X,
					this.rotationDirAbs.Y,
					0.0f);
			}
		}
		/// <summary>
		/// [GET] The objects directional right (90 degree angle) vector in world space.
		/// </summary>
		public Vector3 Right
		{
			get 
			{
				return new Vector3(
					-this.rotationDirAbs.Y,
					this.rotationDirAbs.X,
					0.0f);
			}
		}

		/// <summary>
		/// [GET / SET] Specifies whether the <see cref="Transform"/> component should behave as if 
		/// it was part of a root object. When true, it behaves the same as if it didn't have a 
		/// parent <see cref="Transform"/>.
		/// </summary>
		public bool IgnoreParent
		{
			get { return this.ignoreParent; }
			set
			{
				if (this.ignoreParent != value)
				{
					this.ignoreParent = value;
					this.UpdateRel();
				}
			}
		}
		private Transform ParentTransform
		{
			get
			{
				if (this.ignoreParent) return null;
				if (this.gameobj == null) return null;

				GameObject parent = this.gameobj.Parent;
				if (parent == null) return null;

				return parent.Transform;
			}
		}


		/// <summary>
		/// Transforms a position from local space of this object to world space.
		/// </summary>
		/// <param name="local"></param>
		public Vector3 GetWorldPoint(Vector3 local)
		{
			//return Vector3.Transform(local, this.GetWorldMatrix());
			return new Vector3(
				local.X * this.scaleAbs.X * -this.rotationDirAbs.Y + local.Y * this.scaleAbs.X * -this.rotationDirAbs.X + this.posAbs.X,
				local.X * this.scaleAbs.Y * this.rotationDirAbs.X + local.Y * this.scaleAbs.Y * -this.rotationDirAbs.Y + this.posAbs.Y,
				local.Z * this.scaleAbs.Z + this.posAbs.Z);
		}
		/// <summary>
		/// Transforms a position from world space to local space of this object.
		/// </summary>
		/// <param name="world"></param>
		public Vector3 GetLocalPoint(Vector3 world)
		{
			//return Vector3.Transform(world, this.GetLocalMatrix());
			float inverseScaleX = 1f / this.scaleAbs.X;
			float inverseScaleY = 1f / this.scaleAbs.Y;
			float inverseScaleZ = 1f / this.scaleAbs.Z;
			return new Vector3(
				((world.X - this.posAbs.X) * -this.rotationDirAbs.Y + (world.Y - this.posAbs.Y) * this.rotationDirAbs.X) * inverseScaleX,
				((world.X - this.posAbs.X) * -this.rotationDirAbs.X + (world.Y - this.posAbs.Y) * -this.rotationDirAbs.Y) * inverseScaleY,
				(world.Z - this.posAbs.Z) * inverseScaleZ);
		}

		/// <summary>
		/// Moves the object by the given local offset. This will be treated as movement, rather than teleportation.
		/// </summary>
		/// <param name="value"></param>
		public void MoveByLocal(Vector3 value)
		{
			this.pos += value; 
			this.UpdateAbs();
		}

		/// <summary>
		/// Updates the Transforms world space data all at once. This change is
		/// not regarded as a continuous movement, but as a hard teleport.
		/// </summary>
		public void SetTransform(Vector3 pos, float angle, Vector3 scale)
		{
			this.posAbs = pos;
			this.angleAbs = angle;
			this.scaleAbs = scale;

			this.UpdateRel();
			this.UpdateAbsChild();

			this.ResetVelocity();
			this.ResetAngleVelocity();
		}
		/// <summary>
		/// Updates the Transforms world space data all at once. This change is
		/// not regarded as a continuous movement, but as a hard teleport.
		/// </summary>
		/// <param name="other"></param>
		public void SetTransform(Transform other)
		{
			if (other == this) return;
			this.SetTransform(other.Pos, other.Rotation, other.Scale);
		}
		
		private void SubscribeParentEvents()
		{
			if (this.gameobj == null) return;

			this.gameobj.EventParentChanged += this.gameobj_EventParentChanged;
			if (this.gameobj.Parent != null)
			{
				Transform parentTransform = this.gameobj.Parent.Transform;
				if (parentTransform == null)
					this.gameobj.Parent.EventComponentAdded += this.Parent_EventComponentAdded;
				else
					this.gameobj.Parent.EventComponentRemoving += this.Parent_EventComponentRemoving;
			}
		}
		private void UnsubscribeParentEvents()
		{
			if (this.gameobj == null) return;

			this.gameobj.EventParentChanged -= this.gameobj_EventParentChanged;
			if (this.gameobj.Parent != null)
			{
				this.gameobj.Parent.EventComponentAdded -= this.Parent_EventComponentAdded;
				this.gameobj.Parent.EventComponentRemoving -= this.Parent_EventComponentRemoving;
			}
		}

		void ICmpAttachmentListener.OnAddToGameObject()
		{
			this.SubscribeParentEvents();
			this.UpdateRel();
		}
		void ICmpAttachmentListener.OnRemoveFromGameObject()
		{
			this.UnsubscribeParentEvents();
			this.UpdateRel();
		}
		void ICmpSerializeListener.OnLoaded()
		{
			this.SubscribeParentEvents();
			this.UpdateRel();

			// Recalculate values we didn't serialize
			this.UpdateRotationDirAbs();
		}
		void ICmpSerializeListener.OnSaved() { }
		void ICmpSerializeListener.OnSaving() { }

		private void gameobj_EventParentChanged(object sender, GameObjectParentChangedEventArgs e)
		{
			this.UpdateRel();
		}
		private void Parent_EventComponentAdded(object sender, ComponentEventArgs e)
		{
			Transform cmpTransform = e.Component as Transform;
			if (cmpTransform != null)
			{
				cmpTransform.GameObj.EventComponentAdded -= this.Parent_EventComponentAdded;
				cmpTransform.GameObj.EventComponentRemoving += this.Parent_EventComponentRemoving;
				this.UpdateRel();
			}
		}
		private void Parent_EventComponentRemoving(object sender, ComponentEventArgs e)
		{
			Transform cmpTransform = e.Component as Transform;
			if (cmpTransform != null)
			{
				cmpTransform.GameObj.EventComponentAdded += this.Parent_EventComponentAdded;
				cmpTransform.GameObj.EventComponentRemoving -= this.Parent_EventComponentRemoving;
				this.UpdateRel();
			}
		}
		
		private void UpdateRotationDirAbs()
		{
			this.rotationDirAbs = new Vector2(
				MathF.Sin(this.angleAbs), 
				-MathF.Cos(this.angleAbs));
		}
		private void ResetVelocity()
		{
			if (this.gameobj == null) return;
			VelocityTracker tracker = this.gameobj.GetComponent<VelocityTracker>();
			if (tracker != null)
				tracker.ResetVelocity(this.posAbs);
		}
		private void ResetAngleVelocity()
		{
			if (this.gameobj == null) return;
			VelocityTracker tracker = this.gameobj.GetComponent<VelocityTracker>();
			if (tracker != null)
				tracker.ResetAngleVelocity(this.angleAbs);
		}

		private void UpdateAbs()
		{
			this.CheckValidTransform();

			Transform parent = this.ParentTransform;
			if (parent == null)
			{
				this.angleAbs = this.angle;
				this.posAbs = this.pos;
				this.scaleAbs = this.scale;
			}
			else
			{
				this.angleAbs = MathF.NormalizeAngle(this.angle + parent.angleAbs);
				this.scaleAbs = this.scale * parent.scaleAbs;
				this.posAbs = parent.GetWorldPoint(this.pos);
			}

			// Update cached values
			this.UpdateRotationDirAbs();

			// Update absolute children coordinates
			this.UpdateAbsChild();

			this.CheckValidTransform();
		}
		private void UpdateAbsChild()
		{
			this.CheckValidTransform();

			if (this.gameobj != null)
			{
				foreach (GameObject obj in this.gameobj.Children)
				{
					Transform transform = obj.Transform;
					if (transform == null) continue;
					if (transform.ignoreParent) continue;

					transform.UpdateAbs();
				}
			}

			this.CheckValidTransform();
		}
		private void UpdateRel()
		{
			this.CheckValidTransform();

			Transform parent = this.ParentTransform;
			if (parent == null)
			{
				this.angle = this.angleAbs;
				this.pos = this.posAbs;
				this.scale = this.scaleAbs;
			}
			else
			{
				this.angle = MathF.NormalizeAngle(this.angleAbs - parent.angleAbs);
				this.scale = this.scaleAbs / parent.scaleAbs;
				
				Vector2 parentAngleAbsDotX;
				Vector2 parentAngleAbsDotY;
				MathF.GetTransformDotVec(-parent.angleAbs, out parentAngleAbsDotX, out parentAngleAbsDotY);

				Vector3.Subtract(ref this.posAbs, ref parent.posAbs, out this.pos);
				MathF.TransformDotVec(ref this.pos, ref parentAngleAbsDotX, ref parentAngleAbsDotY);
				Vector3.Divide(ref this.pos, ref parent.scaleAbs, out this.pos);
			}

			this.CheckValidTransform();
		}

		//public Matrix4 GetWorldMatrix()
		//{
		//	Matrix4 mat = Matrix4.CreateScale(this.scale)
		//		* Matrix4.CreateFromQuaternion(this.rotation)
		//		* Matrix4.CreateTranslation(this.pos);
		//
		//	if (this.ParentTransform != null)
		//	{
		//		mat *= this.ParentTransform.GetWorldMatrix();
		//	}
		//
		//	return mat;
		//}
		//
		//public Matrix4 GetLocalMatrix()
		//{
		//	return Matrix4.Invert(this.GetWorldMatrix());
		//}

		protected override void OnCopyDataTo(object targetObj, ICloneOperation operation)
		{
			base.OnCopyDataTo(targetObj, operation);
			Transform target = targetObj as Transform;

			target.ignoreParent   = this.ignoreParent;

			target.pos            = this.pos;
			target.angle          = this.angle;
			target.scale          = this.scale;

			target.posAbs         = this.posAbs;
			target.angleAbs       = this.angleAbs;
			target.scaleAbs       = this.scaleAbs;
			target.rotationDirAbs = this.rotationDirAbs;
			
			// Update absolute transformation data, because the target is relative to a different parent.
			target.UpdateAbs();
		}

		[System.Diagnostics.Conditional("DEBUG")]
		internal void CheckValidTransform()
		{
			MathF.CheckValidValue(this.pos);
			MathF.CheckValidValue(this.scale);
			MathF.CheckValidValue(this.angle);

			MathF.CheckValidValue(this.posAbs);
			MathF.CheckValidValue(this.scaleAbs);
			MathF.CheckValidValue(this.angleAbs);
		}
	}
}
