using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Duality.Graphics.Components
{
    public abstract class RenderableComponent : Component, ICmpInitializable
	{
        [DataMember] public bool CastShadows { get; set; } = true;

        public BoundingSphere BoundingSphere;
        public BoundingBox BoundingBox;

		public abstract void PrepareRenderOperations(BoundingFrustum frustum, RenderOperations operations);

		void ICmpInitializable.OnActivate()
		{
			Duality.Resources.Scene.Stage.AddRenderableComponent(this);
			OnActivate();
		}

		public virtual void OnActivate()
		{

		}

		void ICmpInitializable.OnDeactivate()
		{
			Duality.Resources.Scene.Stage.RemoveRenderableComponent(this);
		}
    }
}
