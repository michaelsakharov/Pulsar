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

		private Vector3			pos             = Vector3.Zero;
		private Quaternion		rotation        = Quaternion.Identity;
		private Vector3			scale           = Vector3.One;
		private bool			ignoreParent    = false;


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
				this.ResetVelocity();
			}
		}
		/// <summary>
		/// [GET / SET] The objects angle / rotation in local space of its parent object, in radians.
		/// </summary>
		public Quaternion LocalRotation
		{
			get { return this.rotation; }
			set
			{
				// Update angle
				this.rotation = value;
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
			}
		}
		
		/// <summary>
		/// [GET / SET] The objects position in world space.
		/// </summary>
		public Vector3 Pos
		{
			get 
			{ 
				Vector3 pos = this.pos;
				if (this.ParentTransform != null)
				{
					pos = Vector3.Transform(pos, this.ParentTransform.GetWorldMatrix());
				}
				return pos;
			}
			set 
			{ 
				// Update position
				Vector3 parentPos = this.ParentTransform != null ? this.ParentTransform.Pos : Vector3.Zero;
				this.pos = value - parentPos;
				this.ResetVelocity();
			}
		}
		/// <summary>
		/// [GET / SET] The objects angle / rotation in world space, in radians.
		/// </summary>
		public Quaternion Rotation
		{
			get 
			{ 
				Quaternion rot = this.rotation;
				if (this.ParentTransform != null)
				{
					rot = Quaternion.Concatenate(this.ParentTransform.Rotation, rot);
				}
				return rot;
			}
			set 
			{
				// Update angle
				Quaternion parentRot = this.ParentTransform != null ? this.ParentTransform.Rotation : Quaternion.Identity;
				this.rotation = Quaternion.Concatenate(Quaternion.Inverse(parentRot), value);
			}
		}
		/// <summary>
		/// [GET / SET] The objects scale in world space.
		/// </summary>
		public Vector3 Scale
		{
			get
			{
				Vector3 scale = this.scale;
				if (this.ParentTransform != null)
				{
					scale *= this.ParentTransform.Scale;
				}
				return scale;
			}
			set 
			{ 
				Vector3 target = new Vector3(MathF.Max(value.X, MinScale), MathF.Max(value.Y, MinScale), MathF.Max(value.Z, MinScale));

				Vector3 parentScale = this.ParentTransform != null ? this.ParentTransform.Scale : Vector3.One;
				this.scale = target / parentScale;
			}
		}

		public Vector3 Forward { get { return Vector3.Transform(Vector3.UnitZ, this.Rotation); } }

		public Vector3 Up { get { return Vector3.Transform(Vector3.UnitY, this.Rotation); } }

		public Vector3 Right { get { return Vector3.Transform(Vector3.UnitX, this.Rotation); } }

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
			return Vector3.Transform(local, this.GetWorldMatrix());
		}
		/// <summary>
		/// Transforms a position from world space to local space of this object.
		/// </summary>
		/// <param name="world"></param>
		public Vector3 GetLocalPoint(Vector3 world)
		{
			return Vector3.Transform(world, this.GetLocalMatrix());
		}

		/// <summary>
		/// Moves the object by the given local offset. This will be treated as movement, rather than teleportation.
		/// </summary>
		/// <param name="value"></param>
		public void MoveByLocal(Vector3 value)
		{
			this.pos += value; 
		}

		/// <summary>
		/// Updates the Transforms world space data all at once. This change is
		/// not regarded as a continuous movement, but as a hard teleport.
		/// </summary>
		public void SetTransform(Vector3 pos, Quaternion angle, Vector3 scale)
		{
			this.Pos = pos;
			this.Rotation = angle;
			this.Scale = scale;

			this.ResetVelocity();
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
		}
		void ICmpAttachmentListener.OnRemoveFromGameObject()
		{
			this.UnsubscribeParentEvents();
		}
		void ICmpSerializeListener.OnLoaded()
		{
			this.SubscribeParentEvents();
		}
		void ICmpSerializeListener.OnSaved() { }
		void ICmpSerializeListener.OnSaving() { }

		private void gameobj_EventParentChanged(object sender, GameObjectParentChangedEventArgs e)
		{
		}
		private void Parent_EventComponentAdded(object sender, ComponentEventArgs e)
		{
			Transform cmpTransform = e.Component as Transform;
			if (cmpTransform != null)
			{
				cmpTransform.GameObj.EventComponentAdded -= this.Parent_EventComponentAdded;
				cmpTransform.GameObj.EventComponentRemoving += this.Parent_EventComponentRemoving;
			}
		}
		private void Parent_EventComponentRemoving(object sender, ComponentEventArgs e)
		{
			Transform cmpTransform = e.Component as Transform;
			if (cmpTransform != null)
			{
				cmpTransform.GameObj.EventComponentAdded += this.Parent_EventComponentAdded;
				cmpTransform.GameObj.EventComponentRemoving -= this.Parent_EventComponentRemoving;
			}
		}
		
		private void ResetVelocity()
		{
			if (this.gameobj == null) return;
			VelocityTracker tracker = this.gameobj.GetComponent<VelocityTracker>();
			if (tracker != null)
				tracker.ResetVelocity(this.Pos);
		}

		public Matrix4 GetWorldMatrix()
		{
			Matrix4 mat = Matrix4.CreateScale(this.scale)
				* Matrix4.CreateFromQuaternion(this.rotation)
				* Matrix4.CreateTranslation(this.pos);

			if (this.ParentTransform != null)
			{
				mat *= this.ParentTransform.GetWorldMatrix();
			}

			return mat;
		}

		public Matrix4 GetLocalMatrix()
		{
			return Matrix4.Invert(this.GetWorldMatrix());
		}

		protected override void OnCopyDataTo(object targetObj, ICloneOperation operation)
		{
			base.OnCopyDataTo(targetObj, operation);
			Transform target = targetObj as Transform;

			target.ignoreParent   = this.ignoreParent;

			target.pos            = this.pos;
			target.rotation          = this.rotation;
			target.scale          = this.scale;
		}

		[System.Diagnostics.Conditional("DEBUG")]
		internal void CheckValidTransform()
		{
			MathF.CheckValidValue(this.pos);
			MathF.CheckValidValue(this.scale);
			MathF.CheckValidValue(this.rotation);
		}
	}
}
