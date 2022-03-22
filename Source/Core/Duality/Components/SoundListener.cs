using System;

using Duality.Properties;
using Duality.Editor;

namespace Duality.Components
{
	/// <summary>
	/// When attached to a <see cref="GameObject"/>, it will become the reference point for all 3D audio.
	/// </summary>
	[RequiredComponent(typeof(Transform))]
	[RequiredComponent(typeof(VelocityTracker))]
	[EditorHintCategory(CoreResNames.CategorySound)]
	[EditorHintImage(CoreResNames.ImageSoundListener)]
	public sealed class SoundListener : Component, ICmpInitializable
	{
		public Vector3 Position
		{
			get { return this.GameObj.Transform.Pos; }
		}
		public Vector3 Velocity
		{
			get { return this.GameObj.GetComponent<VelocityTracker>().Vel; }
		}
		public Vector3 Angle
		{
			get { return this.GameObj.Transform.Rotation; }
		}

		public void MakeCurrent()
		{
			if (!this.Active) return;
			DualityApp.Sound.Listener = this;
		}

		void ICmpInitializable.OnActivate()
		{
			if (DualityApp.ExecContext != DualityApp.ExecutionContext.Editor)
				this.MakeCurrent();
		}
		void ICmpInitializable.OnDeactivate() { }
	}
}
