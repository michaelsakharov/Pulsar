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
				//SubMeshes = new SubMesh[objMesh.SubMeshes.Count];
				for (int i = 0; i < objMesh.Children.Count; i++)
				{
					var obj3D = objMesh.Children[i];

					if(obj3D is THREE.Objects.Mesh)
					{
						SubMeshes[i] = new SubMesh();
						SubMeshes[i].Material = MeshPhongMaterial.Default;
						SubMeshes[i].Vertices = new List<Vector3>();
						SubMeshes[i].Colors = new List<ColorRgba>();
						SubMeshes[i].Faces = new List<Face>();
						SubMeshes[i].Normals = new List<Vector3>();
						SubMeshes[i].Uvs = new List<Vector2>();

						var geometry = new Geometry().FromBufferGeometry((obj3D as THREE.Objects.Mesh).Geometry as BufferGeometry);

						foreach (var vertex in geometry.Vertices)
							SubMeshes[i].Vertices.Add(new Vector3(vertex.X, vertex.Y, vertex.Z));

						foreach (var color in geometry.Colors)
							SubMeshes[i].Colors.Add(new ColorRgba(color.R, color.G, color.B));

						foreach (var face in geometry.Faces)
							SubMeshes[i].Faces.Add(new Face(face.a, face.b, face.c, new Vector3(face.Normal.X, face.Normal.Y, face.Normal.Z)));

						foreach (var normal in geometry.Normals)
							SubMeshes[i].Normals.Add(new Vector3(normal.X, normal.Y, normal.Z));

						foreach (var uv in geometry.Uvs)
							SubMeshes[i].Uvs.Add(new Vector2(uv.X, uv.Y));



						//try
						//{
						//	var vertices = ((obj3D as THREE.Objects.Mesh).Geometry as BufferGeometry).GetAttribute<float>("position") as BufferAttribute<float>;
						//	for (int v = 0; v < vertices.Array.Length; v += 3)
						//		SubMeshes[i].Vertices.Add(new Vector3(vertices.Array[v + 0], vertices.Array[v + 1], vertices.Array[v + 2]));
						//}
						//catch { }
						//
						//try
						//{
						//	var normal = ((obj3D as THREE.Objects.Mesh).Geometry as BufferGeometry).GetAttribute<float>("normal") as BufferAttribute<float>;
						//	for (int v = 0; v < normal.Array.Length; v += 3)
						//		SubMeshes[i].Normals.Add(new Vector3(normal.Array[v + 0], normal.Array[v + 1], normal.Array[v + 2]));
						//}
						//catch { }
						//
						//try
						//{
						//	var color = ((obj3D as THREE.Objects.Mesh).Geometry as BufferGeometry).GetAttribute<float>("color") as BufferAttribute<float>;
						//	for (int v = 0; v < color.Array.Length; v += 3)
						//		SubMeshes[i].Colors.Add(new ColorRgba(color.Array[v + 0], color.Array[v + 1], color.Array[v + 2]));
						//}
						//catch { }
						//
						//try
						//{
						//	var uv = ((obj3D as THREE.Objects.Mesh).Geometry as BufferGeometry).GetAttribute<float>("uv") as BufferAttribute<float>;
						//	for (int v = 0; v < uv.Array.Length; v += 2)
						//		SubMeshes[i].Uvs.Add(new Vector2(uv.Array[v + 0], uv.Array[v + 1]));
						//}
						//catch { }
					}

				}
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
		public ContentRef<MeshPhongMaterial> Material;
		public List<Vector3> Vertices;
		public List<ColorRgba> Colors;
		public List<Face> Faces;
		public List<Vector3> Normals;
		public List<Vector2> Uvs;
		//public List<Vector2> Uvs2;
		//public List<Vector4> SkinIndices;
		//public List<Vector4> SkinWeights;
	}

	[Serializable]
	public class Face
	{
		public int a;
		public int b;
		public int c;
		public Vector3 normal;
		public Face(int a, int b, int c, Vector3 normal)
		{
			this.a = a;
			this.b = b;
			this.c = c;
			this.normal = normal;
		}
	}
}