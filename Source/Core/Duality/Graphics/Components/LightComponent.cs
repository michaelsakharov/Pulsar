using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Duality.Graphics.Components
{
    public class LightComponent : Component, ICmpInitializable
	{
		public LighType Type { get; set; } = LighType.Directional;
		public Vector3 Color { get; set; } = new Vector3(1, 1, 1);
        public float Intensity { get; set; } = 1.0f;
		public float Range { get; set; } = 10;
        public float InnerAngle { get; set; }
        public float OuterAngle { get; set; }
        public bool CastShadows { get; set; } = false;
        public bool Enabled { get; set; } = true;
        public float ShadowBias { get; set; } = 0.001f;
        public float ShadowNearClipDistance { get; set; } = 0.0005f;

        void ICmpInitializable.OnActivate()
        {
			Duality.Resources.Scene.Stage.AddLightComponent(this);
        }

        void ICmpInitializable.OnDeactivate()
        {
			Duality.Resources.Scene.Stage.RemoveLightComponent(this);
        }
    }
}
