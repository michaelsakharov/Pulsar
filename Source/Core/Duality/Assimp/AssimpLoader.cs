using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using Duality.Resources;
using Duality.Drawing;
using Assimp;
using Scene = Assimp.Scene;
using Mesh = Duality.Resources.Mesh;
using Material = Duality.Resources.Material;
using Vector3D = Assimp.Vector3D;
using QuaternionD = Assimp.Quaternion;

namespace Duality.Assimp
{
	class MeshMaterialBinding
	{
		private string meshName;
		private SubMesh mesh;
		private Material material;

		private MeshMaterialBinding() { }    // Do not allow default constructor

		public MeshMaterialBinding(string meshName, SubMesh mesh, Material material)
		{
			this.meshName = meshName;
			this.mesh = mesh;
			this.material = material;
		}

		public SubMesh Mesh
		{
			get
			{
				return mesh;
			}
		}
		public Material Material
		{
			get
			{
				return material;
			}
		}
		public string MeshName
		{
			get
			{
				return meshName;
			}
		}
	}

	public class AssimpImporter
	{
		public static (List<SubMesh>, Skeleton) Load(string meshPath, float scaleX = 1, float scaleY = 1, float scaleZ = 1)
		{
			if (!File.Exists(meshPath))
				return (null, null);

			AssimpContext importer = new AssimpContext();
			Scene scene = importer.ImportFile(meshPath);
			if (scene == null)
				return (null, null);

			string parentDir = Directory.GetParent(meshPath).FullName;

			// Materials
			//List<Material> uMaterials = new List<Material>();
			//if (scene.HasMaterials)
			//{
			//	foreach (var m in scene.Materials)
			//	{
			//		MeshPhysicalMaterial uMaterial = new MeshPhysicalMaterial();
			//
			//		// Albedo
			//		if (m.HasColorDiffuse)
			//		{
			//			ColorRgba color = new ColorRgba(
			//				m.ColorDiffuse.R,
			//				m.ColorDiffuse.G,
			//				m.ColorDiffuse.B,
			//				m.ColorDiffuse.A
			//			);
			//			uMaterial.Color = color;
			//		}
			//
			//		// Emission
			//		if (m.HasColorEmissive)
			//		{
			//			ColorRgba color = new ColorRgba(
			//				m.ColorEmissive.R,
			//				m.ColorEmissive.G,
			//				m.ColorEmissive.B,
			//				m.ColorEmissive.A
			//			);
			//			uMaterial.Emissive = color;
			//			uMaterial.EmissiveIntensity = color.A / 255f;
			//		}
			//
			//		// Reflectivity
			//		if (m.HasReflectivity)
			//		{
			//			uMaterial.Reflectivity = m.Reflectivity;
			//		}
			//
			//		// Texture
			//		if (m.HasTextureDiffuse)
			//		{
			//			//Texture2D uTexture = new Texture2D(2, 2);
			//			//string texturePath = Path.Combine(parentDir, m.TextureDiffuse.FilePath);
			//			//
			//			//byte[] byteArray = File.ReadAllBytes(texturePath);
			//			//bool isLoaded = uTexture.LoadImage(byteArray);
			//			//if (!isLoaded)
			//			//{
			//			//	throw new Exception("Cannot find texture file: " + texturePath);
			//			//}
			//			//
			//			//uMaterial.SetTexture("_MainTex", uTexture);
			//		}
			//
			//		uMaterials.Add(uMaterial);
			//	}
			//}

			// Mesh

			Skeleton skeleton = ParseSkeleton(scene);

			List<MeshMaterialBinding> uMeshAndMats = new List<MeshMaterialBinding>();
			if (scene.HasMeshes)
			{
				foreach (var m in scene.Meshes)
				{
					List<Vector3> uVertices = new List<Vector3>();
					List<Vector3> uNormals = new List<Vector3>();
					List<Vector2> uUv = new List<Vector2>();
					List<int> uIndices = new List<int>();

					// Vertices
					if (m.HasVertices)
					{
						foreach (var v in m.Vertices)
						{
							uVertices.Add(new Vector3(-v.X, v.Y, v.Z));
						}
					}

					// Normals
					if (m.HasNormals)
					{
						foreach (var n in m.Normals)
						{
							uNormals.Add(new Vector3(-n.X, n.Y, n.Z));
						}
					}

					// Triangles
					if (m.HasFaces)
					{
						foreach (var f in m.Faces)
						{
							// Ignore degenerate faces
							if (f.IndexCount == 1 || f.IndexCount == 2)
								continue;

							for (int i = 0; i < (f.IndexCount - 2); i++)
							{
								uIndices.Add(f.Indices[i + 2]);
								uIndices.Add(f.Indices[i + 1]);
								uIndices.Add(f.Indices[0]);
							}
						}
					}

					// Uv (texture coordinate) 
					if (m.HasTextureCoords(0))
					{
						foreach (var uv in m.TextureCoordinateChannels[0])
						{
							uUv.Add(new Vector2(uv.X, uv.Y));
						}
					}

					BoneVertex[] boneVertex = new BoneVertex[uVertices.Count];

					// Map bone weights if they are available
					if (m.BoneCount > 0)
					{
						for (var i = 0; i < m.BoneCount; i++)
						{
							var bone = m.Bones[i];

							if (bone.VertexWeightCount == 0)
								continue;

							for (var w = 0; w < bone.VertexWeightCount; w++)
							{
								var index = bone.VertexWeights[w].VertexID;

								if (boneVertex[index].BoneAssignments == null)
									boneVertex[index].BoneAssignments = new List<BoneAssignment>();

								boneVertex[index].BoneAssignments.Add(new BoneAssignment
								{
									BoneIndex = index,
									Weight = bone.VertexWeights[w].Weight
								});
								boneVertex[index].BoneCount++;
							}
						}
					}

					// Fix the bones and stuff
					for (int i = 0; i < boneVertex.Length; i++)
					{
						// We need four!
						while (boneVertex[i].BoneAssignments.Count < 4)
						{
							boneVertex[i].BoneAssignments.Add(new BoneAssignment());
						}

						// We only support 4 weight per vertex, drop the ones with the lowest weight
						if (boneVertex[i].BoneAssignments.Count > 4)
						{
							boneVertex[i].BoneAssignments = boneVertex[i].BoneAssignments.OrderByDescending(b => b.Weight).Take(4).ToList();
						}

						// Normalize it
						var totalWeight = boneVertex[i].BoneAssignments.Sum(b => b.Weight);
						for (var b = 0; b < 4; b++)
						{
							boneVertex[i].BoneAssignments[b].Weight = boneVertex[i].BoneAssignments[b].Weight / totalWeight;
						}
					}

					SubMesh uMesh = new SubMesh();
					uMesh.Vertices = uVertices;
					uMesh.Normals = uNormals;
					uMesh.Triangles = uIndices;
					uMesh.Uvs = uUv;
					uMesh.SkinIndices = new List<Vector4>();
					uMesh.SkinWeights = new List<Vector4>();
					for (int i = 0; i < boneVertex.Length; i++)
					{
						uMesh.SkinIndices.Add(new Vector4(boneVertex[i].BoneAssignments[0].BoneIndex, boneVertex[i].BoneAssignments[1].BoneIndex, boneVertex[i].BoneAssignments[2].BoneIndex, boneVertex[i].BoneAssignments[3].BoneIndex));
						uMesh.SkinWeights.Add(new Vector4(boneVertex[i].BoneAssignments[0].Weight, boneVertex[i].BoneAssignments[1].Weight, boneVertex[i].BoneAssignments[2].Weight, boneVertex[i].BoneAssignments[3].Weight));
					}

					//uMeshAndMats.Add(new MeshMaterialBinding(m.Name, uMesh, uMaterials[m.MaterialIndex]));
					uMeshAndMats.Add(new MeshMaterialBinding(m.Name, uMesh, null));
				}
			}

			List<SubMesh> subMeshes = new List<SubMesh>();
			foreach (var mesh in uMeshAndMats)
			{
				subMeshes.Add(mesh.Mesh);
			}
			return (subMeshes, skeleton);

			// Create GameObjects from nodes
			//GameObject NodeToGameObject(Node node)
			//{
			//	GameObject uOb = new GameObject(node.Name);
			//
			//	// Set Mesh
			//	if (node.HasMeshes)
			//	{
			//		foreach (var mIdx in node.MeshIndices)
			//		{
			//			var uMeshAndMat = uMeshAndMats[mIdx];
			//
			//			GameObject uSubOb = new GameObject(uMeshAndMat.MeshName);
			//			uSubOb.AddComponent<Graphics.Components.MeshComponent>();
			//			uSubOb.GetComponent<Graphics.Components.MeshComponent>().Mesh = uMeshAndMat.Mesh;
			//			uSubOb.GetComponent<Graphics.Components.MeshComponent>().DefaultMaterial = uMeshAndMat.Material;
			//			uSubOb.Parent = uOb;
			//			uSubOb.GetComponent<Components.Transform>().LocalScale = new Vector3(scaleX, scaleY, scaleZ);
			//		}
			//	}
			//
			//	// Transform
			//	// Decompose Assimp transform into scale, rot and translaction 
			//	Vector3D aScale = new Vector3D();
			//	QuaternionD aQuat = new QuaternionD();
			//	Vector3D aTranslation = new Vector3D();
			//	node.Transform.Decompose(out aScale, out aQuat, out aTranslation);
			//
			//	// Convert Assimp transfrom into Unity transform and set transformation of game object 
			//	Quaternion uQuat = new Quaternion(aQuat.X, aQuat.Y, aQuat.Z, aQuat.W);
			//	var euler = uQuat.EulerAngles;
			//	uOb.GetComponent<Components.Transform>().LocalScale = new Vector3(aScale.X, aScale.Y, aScale.Z);
			//	uOb.GetComponent<Components.Transform>().LocalPos = new Vector3(aTranslation.X, aTranslation.Y, aTranslation.Z);
			//	uOb.GetComponent<Components.Transform>().LocalRotation = new Vector3(euler.X, -euler.Y, euler.Z);
			//
			//	if (node.HasChildren)
			//	{
			//		foreach (var cn in node.Children)
			//		{
			//			var uObChild = NodeToGameObject(cn);
			//			uObChild.Parent = uOb;
			//		}
			//	}
			//
			//	return uOb;
			//}

			//return NodeToGameObject(scene.RootNode);
		}

		private static unsafe Skeleton ParseSkeleton(Scene model)
		{
			Skeleton skeleton = new Skeleton
			{
				RootBone = new SkeletonTransform(),
				//Animations = new List<Animation>()
			};

			// Create skeleton if any of the models have bones
			var bones = new Dictionary<string, Bone>();

			// Fetch all bones first
			//foreach (var meshToImport in model.Meshes)
			for (var i = 0; i < model.MeshCount; i++)
			{
				var meshToImport = model.Meshes[i];
				if (meshToImport.BoneCount > 0)
				{
					//foreach (var bone in meshToImport.Bones)
					for (var b = 0; b < meshToImport.BoneCount; b++)
					{
						Bone bone = meshToImport.Bones[i];
						if (!bones.ContainsKey(bone.Name))
						{
							bones.Add(bone.Name, bone);
						}
					}
				}
			}

			if (bones.Any())
			{
				// Find actual root node
				var rootNode = model.RootNode;
				if (!bones.ContainsKey(rootNode.Name))
				{
					for (var i = 0; i < rootNode.ChildCount; i++)
					{
						var child = rootNode.Children[i];
						if (bones.ContainsKey(child.Name))
						{
							rootNode = child;
							break;
						}
					}
				}

				// Setup Root Bone
				var rootNodeName = rootNode.Name;
				var rootBone = new SkeletonTransform();
				rootNode.Transform.DecomposeNoScaling(out var q, out var t);
				rootBone.Position = new Vector3(t.X, t.Y, t.Z);
				rootBone.Orientation = new Quaternion(q.X, q.Y, q.Z, q.W);
				skeleton.RootBone = rootBone;


				// Parse bone hierarchy
				ParseHierarchy(skeleton.RootBone, rootNode);

				// Parse animations
				//for (var i = 0; i < model->MNumAnimations; i++)
				//{
				//	var animationToImport = model->MAnimations[i];
				//
				//	var animation = new Skeletons.Animation
				//    {
				//        Name = animationToImport->MName,
				//        Tracks = new List<Skeletons.Track>(),
				//        Length = (float)(animationToImport->MDuration / animationToImport->MTicksPerSecond) // MDuration may not be in Ticks, if not it needs to be converted
				//    };
				//
				//	for (var a = 0; a < animationToImport->MNumChannels; a++) //foreach (var nodeAnimation in animationToImport->MChannels)
				//	{
				//		var nodeAnimation = animationToImport->MChannels[a];
				//		// Skip missing bones
				//		if (!bones.ContainsKey(nodeAnimation->MNodeName))
				//            continue;
				//
				//        var track = new Track
				//        {
				//            BoneIndex = nameToIndex[nodeAnimation->MNodeName],
				//            KeyFrames = new List<KeyFrame>()
				//        };
				//
				//		var defptr = nameToNode[nodeAnimation->MNodeName];
				//		var defBonePose = ((Silk.NET.Assimp.Node*)defptr.ToPointer())->MTransformation;
				//		System.Numerics.Matrix4x4.Invert(defBonePose, out var defBonePoseInv);
				//
				//		for (var b = 0; b < nodeAnimation->MPositionKeys.Count; b++)
				//        {
				//            var position = nodeAnimation->MPositionKeys[b]->MValue;
				//            var rotation = nodeAnimation->MRotationKeys[b]->MValue;
				//
				//            var fullTransform = System.Numerics.Matrix4x4.CreateFromQuaternion(rotation) * System.Numerics.Matrix4x4.CreateTranslation(position);
				//            var poseToKey = fullTransform * defBonePoseInv;
				//
				//			var rot = poseToKey.Translation;
				//			var pos = System.Numerics.Quaternion.CreateFromRotationMatrix(poseToKey);
				//
				//			var time = nodeAnimation->MPositionKeys[b]->MTime / animationToImport->MTicksPerSecond;
				//
				//            track.KeyFrames.Add(new KeyFrame
				//            {
				//                Time = (float)time,
				//                Transform = new Transform
				//                {
				//                    Position = new Vector3(pos.X, pos.Y, pos.Z),
				//                    Orientation = new Quaternion(rot.X, rot.Y, rot.Z, rot.W)
				//                }
				//            });
				//        }
				//
				//        animation.Tracks.Add(track);
				//    }
				//
				//    mesh.Skeleton.Animations.Add(animation);
				//}
			}
			return skeleton;
		}

		private static unsafe void ParseHierarchy(SkeletonTransform parent, Node parentnode)
		{
			// Add Child Bones to Parent
			for (var i = 0; i < parentnode.ChildCount; i++)
			{
				var node = parentnode.Children[i];
				var child = new SkeletonTransform();

				node.Transform.DecomposeNoScaling(out var q, out var t);
				child.Position = new Vector3(t.X, t.Y, t.Z);
				child.Orientation = new Quaternion(q.X, q.Y, q.Z, q.W);

				parent.Children.Add(child);
				ParseHierarchy(child, node);
			}
		}

		struct BoneVertex
		{
			public List<BoneAssignment> BoneAssignments;
			public int BoneCount;
		}

		class BoneAssignment
		{
			public int BoneIndex;
			public float Weight;
		}
	}
}