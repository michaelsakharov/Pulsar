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
	public sealed class Transform : Component, ICmpAttachmentListener, ICmpSerializeListener, ICmpUpdatable, ICmpEditorUpdatable
	{
		private const float MinScale = 0.0000001f;

		internal Matrix4 _worldMatrix;
		private bool _dirty = true;
		private Vector3			pos             = Vector3.Zero;
		private Vector3			rotation        = Vector3.Zero;
		private Vector3			scale           = Vector3.One;
		private bool			ignoreParent    = false;

		public event EventHandler OnChanged;

		public Vector3 Pos
		{
			get
			{
				this.UpdateWorldMatrix();
				return this._worldMatrix.Translation;
			}
			set
			{
				this.UpdateWorldMatrix();
				this.pos = Vector3.Transform(value, Matrix4.Invert(this._worldMatrix));
				_dirty = true;
			}
		}

		/// <summary>
		/// This returns the Position Relative to the Main Camera, Used for rendering when Scene.MoveWorldInsteadOfCamera is true
		/// </summary>
		public Vector3 RelativePosition
		{
			get
			{
				if (Duality.Resources.Scene.Camera == null)
					return Pos; // No Camera to be relative to
				return Pos - Duality.Resources.Scene.Camera.GameObj.Transform.Pos;
			}
		}

		public Vector3 LocalPos
		{
			get { return this.pos; }
			set 
			{
				this.pos = value;
				_dirty = true;
			}
		}
		public Vector3 Rotation
		{
			get
			{
				this.UpdateWorldMatrix();
				var rotation = Quaternion.Identity;
				Matrix4.ExtractRotation(this._worldMatrix, ref rotation);
				return Quaternion.ToEuler(rotation);
			}
			set
			{
				if (this.ParentTransform != null)
				{
					var result = Quaternion.Euler(value) * Quaternion.Inverse(this.ParentTransform.Quaternion);
					this.rotation = Quaternion.ToEuler(result);
				}
				else
				{
					this.rotation = value;
				}
				_dirty = true;
			}
		}

		public Matrix4 RotationMatrix { get { return Matrix4.CreateFromYawPitchRoll(this.rotation.Y, this.rotation.X, this.rotation.Z); } }

		public Quaternion Quaternion
		{
			get
			{
				var rotation = this.Rotation;
				return Quaternion.Euler(rotation);
			}
		}

		public Quaternion LocalQuaternion { get { return Quaternion.Euler(this.rotation); } }

		public Vector3 LocalRotation
		{
			get { return this.rotation; }
			set 
			{ 
				this.rotation = value;
				_dirty = true;
			}
		}

		public Vector3 LocalScale
		{
			get { return this.scale; }
			set 
			{ 
				this.scale = value;
				_dirty = true;
			}
		}

		public Vector3 Scale
		{
			get
			{
				Vector3 scale = this.scale;
				if (this.ParentTransform != null)
					scale *= this.ParentTransform.Scale;
				return scale;
			}
			set
			{
				Vector3 parentScale = this.ParentTransform != null ? this.ParentTransform.Scale : Vector3.One;
				this.scale = value / parentScale;
				_dirty = true;
			}
		}

		public Matrix4 WorldMatrix { get { return this._worldMatrix; } }

		public Vector3 Forward { get { return this._worldMatrix.Forward; } }

		public Vector3 Backward { get { return this._worldMatrix.Backward; } }

		public Vector3 Right { get { return this._worldMatrix.Right; } }

		public Vector3 Left { get { return this._worldMatrix.Left; } }

		//public Vector3 Up { get { return Vector3.Normalize(this.Pos + Vector3.Transform(Vector3.Up, this._worldMatrix)); } }
		public Vector3 Up { get { return this._worldMatrix.Up; } }


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
				_dirty = true;
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

				_dirty = true;
				return parent.Transform;
			}
		}


		/// <summary>
		/// Transforms a position from local space of this object to world space.
		/// </summary>
		/// <param name="local"></param>
		public Vector3 GetWorldPoint(Vector3 local)
		{
			return Vector3.Transform(local, this._worldMatrix);
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
			this._dirty = true;
		}

		/// <summary>
		/// Updates the Transforms world space data all at once. This change is
		/// not regarded as a continuous movement, but as a hard teleport.
		/// </summary>
		public void SetTransform(Vector3 pos, Vector3 angle, Vector3 scale)
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

		void ICmpUpdatable.OnUpdate()
		{
			if (!this._dirty)
				return;

			this.UpdateWorldMatrix();

			OnChanged?.Invoke(this, null);
			this._dirty = false;
		}

		void ICmpEditorUpdatable.OnUpdate()
		{
			if (!this._dirty)
				return;

			this.UpdateWorldMatrix();

			OnChanged?.Invoke(this, null);
			this._dirty = false;
		}

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

		public void GetWorldMatrix(out Matrix4 world)
		{
			var scale = Matrix4.CreateScale(gameobj.Transform.Scale);
			var rotation = Matrix4.Rotate(gameobj.Transform.Quaternion);
			var translation = Matrix4.CreateTranslation(gameobj.Transform.Pos);

			Matrix4.Multiply(ref scale, ref rotation, out var rotationScale);
			Matrix4.Multiply(ref rotationScale, ref translation, out world);
		}

		public void UpdateWorldMatrix()
		{
			//GetWorldMatrix(out var test);
			//this._worldMatrix = test;
			this._worldMatrix = Matrix4.Identity;
			this._worldMatrix *= Matrix4.CreateScale(this.scale);
			this._worldMatrix *= Matrix4.CreateFromYawPitchRoll(this.rotation.Y, this.rotation.X, this.rotation.Z);
			this._worldMatrix *= Matrix4.CreateTranslation(this.pos);

			if (this.ParentTransform != null)
				this._worldMatrix *= this.ParentTransform._worldMatrix;
		}

		public Matrix4 GetLocalMatrix()
		{
			return Matrix4.Invert(this._worldMatrix);
		}

		protected override void OnCopyDataTo(object targetObj, ICloneOperation operation)
		{
			base.OnCopyDataTo(targetObj, operation);
			Transform target = targetObj as Transform;

			target.ignoreParent   = this.ignoreParent;

			target.pos            = this.pos;
			target.rotation       = this.rotation;
			target.scale          = this.scale;
			target._dirty = true;
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
