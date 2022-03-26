using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Duality.Graphics.Components
{
    public class ParticleSystemComponent : RenderableComponent, ICmpUpdatable
	{
        [DataMember] public Particles.ParticleSystem ParticleSystem { get; set; }

        public override void PrepareRenderOperations(BoundingFrustum frustum, RenderOperations operations)
        {
            if (ParticleSystem == null || ParticleSystem.Renderer == null)
                return;

			var world = gameobj.Transform.WorldMatrix;
			ParticleSystem.Renderer.PrepareRenderOperations(ParticleSystem, operations, world);
        }

		void ICmpUpdatable.OnUpdate()
		{
            BoundingSphere.Center = gameobj.Transform.Pos;
            BoundingSphere.Radius = 100f;
            if (ParticleSystem != null)
            {
                ParticleSystem.Position = gameobj.Transform.Pos;
                ParticleSystem.Orientation = gameobj.Transform.Quaternion;

                ParticleSystem.Update(Time.DeltaTime);
                ParticleSystem.Renderer?.Update(ParticleSystem, Stage, Time.DeltaTime);
            }
        }
    }
}
