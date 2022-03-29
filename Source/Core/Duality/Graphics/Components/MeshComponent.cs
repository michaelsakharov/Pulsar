using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Duality.Graphics.Resources;

namespace Duality.Graphics.Components
{
    public class MeshComponent : RenderableComponent, ICmpUpdatable, ICmpEditorUpdatable
	{
		[DontSerialize] protected bool _meshDirty = true; // Start Dirty

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

		void ICmpEditorUpdatable.OnUpdate()
		{
            if (_meshDirty)
            {
                UpdateDerviedMeshSettings();
            }

			var world = gameobj.Transform.WorldMatrix;
			_boundingSphereLocalSpace.Transform(ref world, out BoundingSphere);
        }

		public void GetWorldMatrix(out Matrix4 world)
		{
			var scale = Matrix4.CreateScale(gameobj.Transform.Scale);
			var rotation = Matrix4.Rotate(gameobj.Transform.Quaternion);
			var translation = Matrix4.CreateTranslation(gameobj.Transform.Pos);

			Matrix4.Multiply(ref scale, ref rotation, out var rotationScale);
			Matrix4.Multiply(ref rotationScale, ref translation, out world);
		}

		public override void PrepareRenderOperations(BoundingFrustum frustum, RenderOperations operations)
		{
			if (Mesh.IsAvailable == false)
				return;

			var world = gameobj.Transform.WorldMatrix;
			//GetWorldMatrix(out var world);

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
