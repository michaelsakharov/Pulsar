using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Duality.Drawing;
using Duality.Editor;
using Duality.Properties;
using THREE.Core;

namespace Duality.Resources
{
	[ExplicitResourceReference()]
	[EditorHintCategory(CoreResNames.CategoryGraphics)]
	[EditorHintImage(CoreResNames.ImageRigidBodyRenderer)]
	public class Mesh : Resource
	{

		internal static void InitDefaultContent()
		{
			DefaultContent.InitType<Mesh>(".obj", stream => new Mesh(stream, "obj"), "meshes.");
		}

		[EditorHintFlags(MemberFlags.Invisible)]
		public SubMesh[] SubMeshes;

		[EditorHintFlags(MemberFlags.Invisible)]
		public Skeleton Skeleton;

		public Mesh() { }

		public Mesh(Stream objStream, string hint)
		{
			LoadMesh(objStream, hint);
		}

		public void LoadMesh(Stream objStream, string hint)
		{
			using (var reader = new StreamReader(objStream))
			{
				string value = reader.ReadToEnd();
				var objMesh = new THREE.Loaders.OBJLoader().Parse(value, "");
				SubMeshes = new SubMesh[objMesh.Children.Count];
				for (int i = 0; i < objMesh.Children.Count; i++)
				{
					var obj3D = objMesh.Children[i];

					if(obj3D is THREE.Objects.Mesh)
					{
						SubMeshes[i] = new SubMesh();
						SubMeshes[i].Material = MeshPhongMaterial.Default.As<Material>();
						SubMeshes[i].Vertices = new List<Vector3>();
						SubMeshes[i].Triangles = new List<int>();
						SubMeshes[i].Colors = new List<ColorRgba>();
						SubMeshes[i].Normals = new List<Vector3>();
						SubMeshes[i].Uvs = new List<Vector2>();
						SubMeshes[i].Uvs2 = new List<Vector2>();
						SubMeshes[i].SkinIndices = new List<Vector4>();
						SubMeshes[i].SkinWeights = new List<Vector4>();
						SubMeshes[i].Groups = new List<Vector3>();

						var tempGeometry = new Geometry().FromBufferGeometry((obj3D as THREE.Objects.Mesh).Geometry as BufferGeometry);
						var geometry = new DirectGeometry().FromGeometry(tempGeometry);
						tempGeometry.Dispose();

						foreach (var vertex in geometry.Vertices)
							SubMeshes[i].Vertices.Add(new Vector3(vertex.X, vertex.Y, vertex.Z));

						foreach (var color in geometry.Colors)
							SubMeshes[i].Colors.Add(new ColorRgba(color.R, color.G, color.B));

						foreach (var normal in geometry.Normals)
							SubMeshes[i].Normals.Add(new Vector3(normal.X, normal.Y, normal.Z));

						foreach (var uv in geometry.Uvs)
							SubMeshes[i].Uvs.Add(new Vector2(uv.X, uv.Y));

						foreach (var uv2 in geometry.Uvs2)
							SubMeshes[i].Uvs2.Add(new Vector2(uv2.X, uv2.Y));

						foreach (var skin in geometry.SkinIndices)
							SubMeshes[i].SkinIndices.Add(new Vector4(skin.X, skin.Y, skin.Z, skin.W));

						foreach (var skin in geometry.SkinWeights)
							SubMeshes[i].SkinWeights.Add(new Vector4(skin.X, skin.Y, skin.Z, skin.W));

						foreach (var draw in geometry.Groups)
							SubMeshes[i].Groups.Add(new Vector3(draw.Count, draw.Start, draw.MaterialIndex));
					}
				}
				Skeleton = new Skeleton();
			}
		}

		public static ContentRef<Mesh> Barrel { get; private set; }
		public static ContentRef<Mesh> Cone { get; private set; }
		public static ContentRef<Mesh> Cube { get; private set; }
		public static ContentRef<Mesh> Cylinder { get; private set; }
		public static ContentRef<Mesh> Disk { get; private set; }
		public static ContentRef<Mesh> DiskHole { get; private set; }
		public static ContentRef<Mesh> Dome { get; private set; }
		public static ContentRef<Mesh> DomeHalf { get; private set; }
		public static ContentRef<Mesh> DomeQuarter { get; private set; }
		public static ContentRef<Mesh> DoubleSidedTriangle { get; private set; }
		public static ContentRef<Mesh> HalfPipe { get; private set; }
		public static ContentRef<Mesh> HalfPipeEnd { get; private set; }
		public static ContentRef<Mesh> OneSidedTriangle { get; private set; }
		public static ContentRef<Mesh> Plane { get; private set; }
		public static ContentRef<Mesh> Ring { get; private set; }
		public static ContentRef<Mesh> Sphere { get; private set; }
		public static ContentRef<Mesh> Sponza { get; private set; }
		public static ContentRef<Mesh> Torus { get; private set; }
	}

	[Serializable]
	public class SubMesh
	{
		public ContentRef<Material> Material;
		public List<Vector3> Vertices;
		public List<int> Triangles;
		public List<ColorRgba> Colors;
		public List<Vector3> Normals;
		public List<Vector2> Uvs;
		public List<Vector2> Uvs2;
		public List<Vector4> SkinIndices;
		public List<Vector4> SkinWeights;
		public List<Vector3> Groups;
	}

	[Serializable]
	public class Skeleton
	{
		public SkeletonTransform RootBone;
		//public List<Animation> Animations = new List<Animation>();
	}

	[Serializable]
	public class SkeletonTransform
	{
		public Vector3 Position;
		public Quaternion Orientation;
		public List<SkeletonTransform> Children = new List<SkeletonTransform>();
	}
}