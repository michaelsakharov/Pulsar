using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Duality.Graphics.SkeletalAnimation;

namespace Duality.Graphics.Components
{
    public class SkinnedMeshComponent : MeshComponent, ICmpUpdatable, ICmpEditorUpdatable, ICmpInitializable
	{
		[DontSerialize]
        public SkeletonInstance _skeletonInstance = null;

		void ICmpInitializable.OnDeactivate()
		{
            _skeletonInstance = null;
        }

		public AnimationState GetAnimationState(string animation)
		{
			return _skeletonInstance.GetAnimationState(animation);
		}

		public IReadOnlyList<AnimationState> AnimationStates
		{
			get
			{
				return _skeletonInstance.AnimationStates;
			}
		}

		public Skeleton Skeleton
		{
			get
			{
				return _skeletonInstance.Skeleton;
			}
		}

		protected override void UpdateDerviedMeshSettings()
        {
            base.UpdateDerviedMeshSettings();

			if (Mesh.IsAvailable)
			{
                _skeletonInstance = new SkeletonInstance(Mesh.Res);
            }
        }

		void ICmpUpdatable.OnUpdate()
		{
            _skeletonInstance?.Update();
        }

		void ICmpEditorUpdatable.OnUpdate()
		{
            _skeletonInstance?.Update();
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
				operations.Add(subMesh.Handle, world, subMesh.Material.Res, _skeletonInstance, false, CastShadows);
            }
        }
    }
}
