using System;

using Duality.Editor;
using Duality.Properties;
using Duality.Cloning;

namespace Duality.Components
{
	/// <summary>
	/// Keeps track of this objects linear and angular velocity by accumulating all
	/// movement (but not teleportation) of its <see cref="Transform"/> component.
	/// </summary>
	[ManuallyCloned]
	[EditorHintCategory(CoreResNames.CategoryNone)]
	[EditorHintImage(CoreResNames.ImageVelocityTracker)]
	[RequiredComponent(typeof(Transform))]
	public sealed class VelocityTracker : Component, ICmpUpdatable, ICmpSerializeListener
	{
		[DontSerialize] private Vector3 velocity      = Vector3.Zero;
		[DontSerialize] private Vector3 posDiff       = Vector3.Zero;
		[DontSerialize] private Vector3 lastPosition  = Vector3.Zero;


		/// <summary>
		/// [GET] The objects measured velocity in world space. The value is internally smoothed
		/// over several frames to filter out fluctuations due to framerate variations.
		/// </summary>
		public Vector3 Vel
		{
			get { return this.velocity; }
		}
		/// <summary>
		/// [GET] The objects measured continuous position change in world space between the last two frames.
		/// Note that this value can fluctuate depending on framerate variations during simulation.
		/// </summary>
		public Vector3 LastMovement
		{
			get { return this.posDiff; }
		}


		/// <summary>
		/// Resets the objects velocity value for next frame to zero, assuming the
		/// specified world space position as a basis for further movement.
		/// </summary>
		/// <param name="worldPos"></param>
		public void ResetVelocity(Vector3 worldPos)
		{
			this.lastPosition = worldPos;
		}
		
		void ICmpUpdatable.OnUpdate()
		{
			// Calculate velocity values from last frames movement
			if (MathF.Abs(Time.TimeMult) > (float.Epsilon))
			{
				Transform transform = this.GameObj.Transform;
				Vector3 pos = transform.Pos;

				this.posDiff = pos - this.lastPosition;

				Vector3 lastVelocity = this.posDiff / Time.TimeMult;

				this.velocity += (lastVelocity - this.velocity) * 0.25f * Time.TimeMult;
				this.lastPosition = pos;
			}
		}
		void ICmpSerializeListener.OnLoaded()
		{
			Transform transform = this.GameObj.Transform;
			this.lastPosition = transform.Pos;
		}
		void ICmpSerializeListener.OnSaved() { }
		void ICmpSerializeListener.OnSaving() { }

		protected override void OnCopyDataTo(object targetObj, ICloneOperation operation)
		{
			base.OnCopyDataTo(targetObj, operation);
			VelocityTracker target = targetObj as VelocityTracker;
			target.lastPosition   = this.lastPosition;
			target.posDiff        = this.posDiff;
			target.velocity       = this.velocity;
		}
	}
}
