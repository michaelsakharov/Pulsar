using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Duality.Graphics.Resources;
using Duality.Resources;

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

		public ContentRef<Material>[] _material = null;
		/// <summary>
		/// [GET / SET] The <see cref="Mesh"/> that is to be rendered by this component.
		/// </summary>
		public ContentRef<Material>[] Material
		{
			get { return this._material; }
			set { this._material = value; }
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

		public override void OnActivate()
		{
			this.GameObj.Transform.OnChanged += Transform_OnChanged;
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

		private void Transform_OnChanged(object sender, EventArgs e)
		{
			_meshDirty = true;
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

			if(Material == null)
			{
				Material = new ContentRef<Material>[Mesh.Res.SubMeshes.Length];
				for (var i = 0; i < Mesh.Res.SubMeshes.Length; i++)
				{
					Material[i] = Mesh.Res.SubMeshes[i].Material;
				}
			}

			for (var i = 0; i < Mesh.Res.SubMeshes.Length; i++)
            {
                var subMesh = Mesh.Res.SubMeshes[i];

				var mat = Material[i];
				if(mat == null || mat.IsAvailable == false)
				{
					mat = subMesh.Material;
				}
				else
				{
					if (mat.IsAvailable == false) continue;
				}

				if (mat.IsAvailable == false) continue;

				Mesh.Res.SubMeshes[i].BoundingSphere.Transform(ref world, out var subMeshBoundingSphere);

                if (frustum == null || frustum.Intersects(subMeshBoundingSphere))
                {
                    operations.Add(subMesh.Handle, world, mat.Res, null, false, CastShadows);
                }
            }
        }
    }
}
