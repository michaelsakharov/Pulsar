using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Duality.Drawing;
using Duality.Editor;
using Duality.Properties;
using Duality.Resources;
using THREE.Cameras;
using THREE.Lights;
using THREE.Math;

namespace Duality.Graphics.Components
{
	[EditorHintCategory(CoreResNames.CategoryGraphics)]
	[EditorHintImage(CoreResNames.ImageFragmentShader)]
	public class AmbientLightComponent : Component, ICmpInitializable, ICmpUpdatable, ICmpEditorUpdatable, IDisposable
	{

		[DontSerialize] AmbientLight Light;

		private ColorRgba color = ColorRgba.White;
		public ColorRgba Color { get { return this.color; } set { this.color = value; } }

		private float intensity = 1;
		public float Intensity { get { return this.intensity; } set { this.intensity = value; } }

		void ICmpInitializable.OnActivate()
		{
			CreateLight();
		}

		void ICmpInitializable.OnDeactivate()
		{
			if (Light != null)
			{
				Scene.ThreeScene.Remove(Light);
				Light.Dispose();
				Light = null;
			}
		}

		void ICmpUpdatable.OnUpdate()
		{
			UpdateLight();
		}

		void ICmpEditorUpdatable.OnUpdate()
		{
			UpdateLight();
		}

		void UpdateLight()
		{
			Light.Color = new THREE.Math.Color(Color.R / 255f, Color.G / 255f, Color.B / 255f);
			Light.Intensity = Intensity;
		}

		void CreateLight()
		{
			if(Light != null)
			{
				Scene.ThreeScene.Remove(Light);
				Light.Dispose();
				Light = null;
			}

			Light = new AmbientLight(new Color().SetHex(0x444444));

			Light.Color = new THREE.Math.Color(Color.R / 255f, Color.G / 255f, Color.B / 255f);
			Light.Intensity = Intensity;

			Scene.ThreeScene.Add(Light);
		}

		void IDisposable.Dispose()
		{
			if (Light != null)
			{
				Scene.ThreeScene.Remove(Light);
				Light.Dispose();
				Light = null;
			}
		}
	}
}