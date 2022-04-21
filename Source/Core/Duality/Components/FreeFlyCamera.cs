using System;
using System.Collections.Generic;
using System.Linq;

using Duality.Editor;
using Duality.Resources;
using Duality.Properties;
using Duality.Audio;
using Duality.Cloning;

namespace Duality.Components
{
	/// <summary>
	/// Provides functionality to emit sound.
	/// </summary>
	[RequiredComponent(typeof(Transform))]
	[RequiredComponent(typeof(Camera))]
	[EditorHintCategory(CoreResNames.CategoryNone)]
	[EditorHintImage(CoreResNames.ImageVelocityTracker)]
	public sealed class FreeFlyCamera : Component, ICmpUpdatable
	{

		private float _currentYaw;
		private float _currentPitch;
		private double normalSpeed = 1;
		private double fastSpeed = 10;
		private double veryfastSpeed = 100;
		private Vector3 camVel;

		void ICmpUpdatable.OnUpdate()
		{
			double speed = normalSpeed;
			if (DualityApp.Keyboard.KeyPressed(Input.Key.ShiftLeft)) speed = fastSpeed;
			if (DualityApp.Keyboard.KeyPressed(Input.Key.ControlLeft)) speed = veryfastSpeed;

			if (DualityApp.Mouse.ButtonPressed(Input.MouseButton.Right))
			{
				this._currentYaw += (float)(DualityApp.Mouse.Vel.X * 0.01);
				this._currentPitch += (float)(DualityApp.Mouse.Vel.Y * 0.01);

				//camObj.Transform.Rotation = new Vector3(this._currentYaw, this._currentPitch, 0f);
				this.GameObj.Transform.Rotation = Quaternion.CreateFromYawPitchRoll(this._currentYaw, this._currentPitch, 0f).EulerAngles;

				float forward = 0;
				float right = 0;
				float up = 0;

				if (DualityApp.Keyboard.KeyPressed(Input.Key.W)) forward += 1;
				if (DualityApp.Keyboard.KeyPressed(Input.Key.S)) forward -= 1;

				if (DualityApp.Keyboard.KeyPressed(Input.Key.D)) right += 1;
				if (DualityApp.Keyboard.KeyPressed(Input.Key.A)) right -= 1;

				if (DualityApp.Keyboard.KeyPressed(Input.Key.E)) up += 1;
				if (DualityApp.Keyboard.KeyPressed(Input.Key.Q)) up -= 1;

				Vector3 movement = (GameObj.Transform.Forward * forward) + (GameObj.Transform.Right * right) + (GameObj.Transform.Up * up);

				camVel = movement * speed;
				//GameObj.Transform.Pos = camVel;
				//camVel = movement * 0.1f;
			}
			else
			{
				camVel *= 0.98f;
			}
			GameObj.Transform.MoveByLocal(camVel * Time.DeltaTime);
			//GameObj.Transform.Pos += camVel;
		}
	}
}