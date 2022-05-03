using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using Duality.Resources;
using Duality.Drawing;

namespace Duality.Assimp
{
	public class AssimpLoader
	{

		/// <summary>
		/// Reads a stream until the end is reached into a byte array. Based on
		/// <a href="http://www.yoda.arachsys.com/csharp/readbinary.html">Jon Skeet's implementation</a>.
		/// It is up to the caller to dispose of the stream.
		/// </summary>
		/// <param name="stream">Stream to read all bytes from</param>
		/// <param name="initialLength">Initial buffer length, default is 32K</param>
		/// <returns>The byte array containing all the bytes from the stream</returns>
		public static byte[] ReadStreamFully(Stream stream, int initialLength)
		{
			if (initialLength < 1)
			{
				initialLength = 32768; //Init to 32K if not a valid initial length
			}

			byte[] buffer = new byte[initialLength];
			int position = 0;
			int chunk;

			while ((chunk = stream.Read(buffer, position, buffer.Length - position)) > 0)
			{
				position += chunk;

				//If we reached the end of the buffer check to see if there's more info
				if (position == buffer.Length)
				{
					int nextByte = stream.ReadByte();

					//If -1 we reached the end of the stream
					if (nextByte == -1)
					{
						return buffer;
					}

					//Not at the end, need to resize the buffer
					byte[] newBuffer = new byte[buffer.Length * 2];
					Array.Copy(buffer, newBuffer, buffer.Length);
					newBuffer[position] = (byte)nextByte;
					buffer = newBuffer;
					position++;
				}
			}

			//Trim the buffer before returning
			byte[] toReturn = new byte[position];
			Array.Copy(buffer, toReturn, position);
			return toReturn;
		}

		public static unsafe (List<SubMesh>, Skeleton) Import(Stream stream, string hint)
		{
			var api = Silk.NET.Assimp.Assimp.GetApi();

			byte[] buffer = ReadStreamFully(stream, 0);

			Silk.NET.Assimp.Scene* model = api.ImportFileFromMemory(ref buffer[0], (uint)buffer.Length, (uint)(Silk.NET.Assimp.PostProcessSteps.LimitBoneWeights | Silk.NET.Assimp.PostProcessSteps.FlipUVs), hint);

			var nameToIndex = new Dictionary<string, int>();
			Skeleton skeleton = ParseSkeleton(model, nameToIndex);

			List<SubMesh> Meshes = new List<SubMesh>();

			for (uint p = 0; p < model->MNumMeshes; p++)
			{
				var meshToImport = model->MMeshes[p];

				// Validate sub mesh data
				if (meshToImport->MPrimitiveTypes != (uint)Silk.NET.Assimp.PrimitiveType.PrimitiveTypeTriangle)
				{
					Logs.Core.Write($"Invalid primitive type, should be triangle. {meshToImport->MName.AsString}");
					continue;
				}

				var subMesh = new SubMesh();
				subMesh.Material = MeshPhongMaterial.Default.As<Material>();
				subMesh.Vertices = new List<Vector3>();
				subMesh.Colors = new List<ColorRgba>();
				subMesh.Normals = new List<Vector3>();
				subMesh.Uvs = new List<Vector2>();
				subMesh.Uvs2 = new List<Vector2>();
				subMesh.SkinIndices = new List<Vector4>();
				subMesh.SkinWeights = new List<Vector4>();
				subMesh.Groups = new List<Vector3>();

				Vertex[] vertices = new Vertex[meshToImport->MNumVertices];
				var positions = meshToImport->MVertices;
				var normals = meshToImport->MNormals;
				var texCoords = meshToImport->MTextureCoords.Element0;
				//var texCoords2 = meshToImport->MTextureCoords.Element1;
				for (uint v = 0; v < meshToImport->MNumVertices; v++)
				{
					vertices[v].Position = new Vector3(positions[v].X, positions[v].Y, positions[v].Z);
					vertices[v].Normal = new Vector3(normals[v].X, normals[v].Y, normals[v].Z);
					vertices[v].TexCoord = new Vector2(texCoords[v].X, texCoords[v].Y);
					//vertices[v].TexCoord2 = new Vector2(texCoords2[v].X, texCoords2[v].Y);
					vertices[v].BoneAssignments = new List<BoneAssignment>();
				}

				// Map bone weights if they are available
				if (meshToImport->MNumBones > 0)
				{
					for (var i = 0; i < meshToImport->MNumBones; i++)
					{
						var bone = meshToImport->MBones[i];

						if (bone->MNumWeights == 0)
							continue;

						for (var w = 0; w < bone->MNumWeights; w++)
						{
							var index = bone->MWeights[w].MVertexId;

							vertices[index].BoneAssignments.Add(new BoneAssignment
							{
								BoneIndex = nameToIndex[bone->MName.AsString],
								Weight = bone->MWeights[w].MWeight
							});
							vertices[index].BoneCount++;
						}
					}
				}

				// Fix the bones and stuff
				for (int i = 0; i < vertices.Length; i++)
				{
					// We need four!
					while (vertices[i].BoneAssignments.Count < 4)
					{
						vertices[i].BoneAssignments.Add(new BoneAssignment());
					}

					// We only support 4 weight per vertex, drop the ones with the lowest weight
					if (vertices[i].BoneAssignments.Count > 4)
					{
						vertices[i].BoneAssignments = vertices[i].BoneAssignments.OrderByDescending(b => b.Weight).Take(4).ToList();
					}

					// Normalize it
					var totalWeight = vertices[i].BoneAssignments.Sum(b => b.Weight);
					for (var b = 0; b < 4; b++)
					{
						vertices[i].BoneAssignments[b].Weight = vertices[i].BoneAssignments[b].Weight / totalWeight;
					}
				}

				for (int i = 0; i < vertices.Length; i++)
				{
					subMesh.Vertices.Add(vertices[i].Position);
					subMesh.Normals.Add(vertices[i].Normal);
					subMesh.Uvs.Add(vertices[i].TexCoord);
					//subMesh.Uvs2.Add(vertices[i].TexCoord2);
					subMesh.SkinIndices.Add(new Vector4(vertices[i].BoneAssignments[0].BoneIndex, vertices[i].BoneAssignments[1].BoneIndex, vertices[i].BoneAssignments[2].BoneIndex, vertices[i].BoneAssignments[3].BoneIndex));
					subMesh.SkinWeights.Add(new Vector4(vertices[i].BoneAssignments[0].Weight, vertices[i].BoneAssignments[1].Weight, vertices[i].BoneAssignments[2].Weight, vertices[i].BoneAssignments[3].Weight));
				}

				// Cannot load Materials from Models at the time of writing this in 
				//if (model.HasMaterials)
				//{
				//    var material = model.Materials[meshToImport.MaterialIndex].Name;
				//
				//    if (!material.StartsWith("/materials/"))
				//    {
				//        material = "/materials/" + material;
				//    }
				//
				//    subMesh.Material = material;
				//}
				//else
				//{
				//    subMesh.Material = "no_material";
				//}

				Meshes.Add(subMesh);
			}

			return (Meshes, skeleton);
		}

		private static unsafe Skeleton ParseSkeleton(Silk.NET.Assimp.Scene* model, Dictionary<string, int> nameToIndex)
		{
			Skeleton skeleton = new Skeleton
			{
				RootBone = new SkeletonTransform(),
				//Animations = new List<Animation>()
			};

			// Create skeleton if any of the models have bones
			var bones = new Dictionary<string, IntPtr>();
		
		    // Fetch all bones first
		    //foreach (var meshToImport in model.Meshes)
			for (var i = 0; i < model->MNumMeshes; i++)
			{
				var meshToImport = model->MMeshes[i];
				if (meshToImport->MNumBones > 0)
		        {
					//foreach (var bone in meshToImport.Bones)
					for (var b = 0; b < meshToImport->MNumBones; b++)
					{
						Silk.NET.Assimp.Bone* bone = meshToImport->MBones[i];
						if (!bones.ContainsKey(bone->MName))
						{
							bones.Add(bone->MName, new IntPtr(bone));
						}
					}
		        }
		    }
		
		    if (bones.Any())
		    {
		        // Find actual root node
		        var rootNode = model->MRootNode;
		        if (!bones.ContainsKey(rootNode->MName))
				{
					for (var i = 0; i < rootNode->MNumChildren; i++)
					{
						var child = rootNode->MChildren[i];
						if (bones.ContainsKey(child->MName))
		                {
		                    rootNode = child;
		                    break;
		                }
		            }
		        }
			
				// Setup Root Bone
		        var rootNodeName = rootNode->MName;
				var rootBone = new SkeletonTransform();
				var transalation = rootNode->MTransformation.Translation;
				var rotation = System.Numerics.Quaternion.CreateFromRotationMatrix(rootNode->MTransformation);
				rootBone.Position = new Vector3(transalation.X, transalation.Y, transalation.Z);
				rootBone.Orientation = new Quaternion(rotation.X, rotation.Y, rotation.Z, rotation.W);
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

		private static unsafe void ParseParents(Dictionary<string, int> nameToIndex, Dictionary<string, int> parentIndexes, Dictionary<string, IntPtr> nameToNode, Silk.NET.Assimp.Node* node, bool isRootNode)
		{
			if (nameToIndex.ContainsKey(node->MName))
			{
				nameToNode.Add(node->MName, new IntPtr(node));
			}

			if (isRootNode)
			{
				parentIndexes.Add(node->MName, -1); // This is a root node!
			}
			else if (nameToIndex.ContainsKey(node->MParent->MName))
			{
				parentIndexes.Add(node->MName, nameToIndex[node->MParent->MName]);
			}

			for (var i = 0; i < node->MNumChildren; i++)
			{
				ParseParents(nameToIndex, parentIndexes, nameToNode, node->MChildren[i], false);
			}
		}

		private static unsafe void ParseHierarchy(SkeletonTransform parent, Silk.NET.Assimp.Node* parentnode)
		{
			// Add Child Bones to Parent
			for (var i = 0; i < parentnode->MNumChildren; i++)
			{
				var node = parentnode->MChildren[i];
				var child = new SkeletonTransform();

				var transalation = node->MTransformation.Translation;
				var rotation = System.Numerics.Quaternion.CreateFromRotationMatrix(node->MTransformation);
				child.Position = new Vector3(transalation.X, transalation.Y, transalation.Z);
				child.Orientation = new Quaternion(rotation.X, rotation.Y, rotation.Z, rotation.W);

				parent.Children.Add(child);
				ParseHierarchy(child, node);
			}
		}

		struct Vertex
		{
			public Vector3 Position;
			public Vector3 Normal;
			public Vector2 TexCoord;
			public Vector2 TexCoord2;
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