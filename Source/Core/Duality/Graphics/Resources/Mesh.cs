using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Duality.Renderer;
using Duality.Resources;

namespace Duality.Graphics.Resources
{
    public class Mesh : IDisposable
    {
        private Backend _backend;

        public SubMesh[] SubMeshes { get; set; }
        public SkeletalAnimation.Skeleton Skeleton { get; set; }

        public Mesh(Backend backend)
        {
            _backend = backend;

            SubMeshes = new SubMesh[0];
        }

        public void Dispose()
        {
            foreach (var subMesh in SubMeshes)
            {
                _backend.RenderSystem.DestroyBuffer(subMesh.VertexBufferHandle);
                _backend.RenderSystem.DestroyBuffer(subMesh.IndexBufferHandle);
                _backend.RenderSystem.DestroyMesh(subMesh.Handle);

                if (subMesh.Material != null)
                {
                    subMesh.Material = null;
                }
            }

            SubMeshes = null;
        }
    }

    public class SubMesh
    {
        public Duality.Resources.Material Material;
        public BoundingSphere BoundingSphere;
        public BoundingBox BoundingBox;
        public VertexFormat VertexFormat;
        public int TriangleCount;
        public byte[] VertexData;
        public byte[] IndexData;

        internal int VertexBufferHandle;
        internal int IndexBufferHandle;
        internal int Handle;
    }
}
