using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Duality.Graphics.Resources;

namespace Duality.Graphics.Components
{
    public class MeshComponent : RenderableComponent, ICmpUpdatable
	{
		[DontSerialize] protected bool _meshDirty = false;

		// Used for world space transform
		[DontSerialize] private BoundingSphere _boundingSphereLocalSpace;

		public ContentRef<Mesh> _mesh = null;

		/// <summary>
		/// [GET / SET] The <see cref="Mesh"/> that is to be rendered by this component.
		/// </summary>
		public ContentRef<Mesh> Mesh
		{
			get { return this._mesh; }
			set { this._mesh = value; }
		}

		protected virtual void UpdateDerviedMeshSettings()
        {
            if (Mesh.IsAvailable == false)
                return;

            BoundingBox = Mesh.Res.SubMeshes[0].BoundingBox;
            BoundingSphere = Mesh.Res.SubMeshes[0].BoundingSphere;

            for (var i = 1; i < Mesh.Res.SubMeshes.Length; i++)
            {
                BoundingSphere = BoundingSphere.CreateMerged(BoundingSphere, Mesh.Res.SubMeshes[i].BoundingSphere);
                BoundingBox = BoundingBox.CreateMerged(BoundingBox, Mesh.Res.SubMeshes[i].BoundingBox);
            }

            _boundingSphereLocalSpace = BoundingSphere;

            _meshDirty = false;
        }

		void ICmpUpdatable.OnUpdate()
		{
            if (_meshDirty)
            {
                UpdateDerviedMeshSettings();
            }

			var world = gameobj.Transform.WorldMatrix;
			_boundingSphereLocalSpace.Transform(ref world, out BoundingSphere);
        }

        public override void PrepareRenderOperations(BoundingFrustum frustum, RenderOperations operations)
		{
			if (Mesh.IsAvailable == false)
				return;

			var world = gameobj.Transform.WorldMatrix;

			for (var i = 0; i < Mesh.Res.SubMeshes.Length; i++)
            {
                var subMesh = Mesh.Res.SubMeshes[i];

				if (subMesh.Material.IsAvailable == false) continue;

				Mesh.Res.SubMeshes[i].BoundingSphere.Transform(ref world, out var subMeshBoundingSphere);

                if (frustum == null || frustum.Intersects(subMeshBoundingSphere))
                {
                    operations.Add(subMesh.Handle, world, subMesh.Material.Res, null, false, CastShadows);
                }
            }
        }
    }
}
