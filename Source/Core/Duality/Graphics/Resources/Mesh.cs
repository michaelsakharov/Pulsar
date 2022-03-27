using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Duality.Editor;
using Duality.Properties;
using Duality.Renderer;
using Duality.Resources;

namespace Duality.Graphics.Resources
{
	[ExplicitResourceReference()]
	[EditorHintCategory(CoreResNames.CategoryGraphics)]
	[EditorHintImage(CoreResNames.ImageRigidBodyRenderer)]
	public class Mesh : Resource
	{

		internal static void InitDefaultContent()
		{
			DefaultContent.InitType<Mesh>(".dae", stream => new Mesh(stream), "meshes");
			DefaultContent.InitType<Mesh>(".fbx", stream => new Mesh(stream), "meshes");
			DefaultContent.InitType<Mesh>(".x", stream => new Mesh(stream), "meshes");
			DefaultContent.InitType<Mesh>(".obj", stream => new Mesh(stream), "meshes");
		}

		[EditorHintFlags(MemberFlags.Invisible)]
		public SubMesh[] SubMeshes;

		[EditorHintFlags(MemberFlags.Invisible)]
		public SkeletalAnimation.Skeleton Skeleton;

		public Mesh() { }

		public Mesh(Stream objStream)
		{
			var importer = new Duality.MeshImporter.Meshes.Converters.AssimpConverter();
			var mesh = importer.Import(objStream);
			SubMeshes = new SubMesh[mesh.SubMeshes.Count];
			for (int i = 0; i < mesh.SubMeshes.Count; i++)
			{
				var subMesh = mesh.SubMeshes[i];
				SubMeshes[i] = new SubMesh();
				SubMeshes[i].Material = Material.SolidWhite;
				SubMeshes[i].BoundingSphere = subMesh.BoundingSphere;
				SubMeshes[i].BoundingBox = subMesh.BoundingBox;
				SubMeshes[i].VertexFormat = subMesh.VertexFormat;
				SubMeshes[i].TriangleCount = subMesh.TriangleCount;
				SubMeshes[i].VertexData = subMesh.Vertices;
				SubMeshes[i].IndexData = subMesh.Indices;
			}
			// Todo !!!! Mesh Skeleton
			Skeleton = new SkeletalAnimation.Skeleton();
			//Skeleton.
		}

		protected override void OnDisposing(bool manually)
		{
			base.OnDisposing(manually);
			foreach (var subMesh in SubMeshes)
			{
				DualityApp.GraphicsBackend.RenderSystem.DestroyBuffer(subMesh.VertexBufferHandle);
				DualityApp.GraphicsBackend.RenderSystem.DestroyBuffer(subMesh.IndexBufferHandle);
				DualityApp.GraphicsBackend.RenderSystem.DestroyMesh(subMesh.Handle);
			}

			SubMeshes = null;
		}

		public void UploadToGPU()
		{
			foreach (SubMesh mesh in SubMeshes)
			{
				mesh.VertexBufferHandle = DualityApp.GraphicsBackend.RenderSystem.CreateBuffer(Renderer.BufferTarget.ArrayBuffer, false, mesh.VertexFormat);
				mesh.IndexBufferHandle = DualityApp.GraphicsBackend.RenderSystem.CreateBuffer(Renderer.BufferTarget.ElementArrayBuffer, false);
				DualityApp.GraphicsBackend.RenderSystem.SetBufferData(mesh.VertexBufferHandle, mesh.VertexData, false, false);
				DualityApp.GraphicsBackend.RenderSystem.SetBufferData(mesh.IndexBufferHandle, mesh.IndexData, false, false);
				mesh.Handle = DualityApp.GraphicsBackend.RenderSystem.CreateMesh(mesh.TriangleCount, mesh.VertexBufferHandle, mesh.IndexBufferHandle, false);
			}
		}

		protected override void OnLoaded()
		{
			base.OnLoaded();
			UploadToGPU();
		}

		// PLaced these here due to how many there is
		public static ContentRef<Mesh> Cube { get; private set; }
	}

	[Serializable]
    public class SubMesh
	{
		public ContentRef<Material> Material;
        public BoundingSphere BoundingSphere;
        public BoundingBox BoundingBox;
        public VertexFormat VertexFormat;
        public int TriangleCount;
        public byte[] VertexData;
        public byte[] IndexData;

        [DontSerialize] internal int VertexBufferHandle;
        [DontSerialize] internal int IndexBufferHandle;
		[DontSerialize] internal int Handle;
    }
}
