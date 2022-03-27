using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;

namespace Duality.MeshImporter.Meshes.Converters
{
    class AssimpConverter : IMeshImporter
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

		public unsafe Mesh Import(Stream stream)
		{
			var api = Silk.NET.Assimp.Assimp.GetApi();

			byte[] buffer = ReadStreamFully(stream, 0);

			//IntPtr bufferPtr = Marshal.AllocHGlobal(buffer.Length);
			//Marshal.Copy(buffer, 0, bufferPtr, buffer.Length);

			//Silk.NET.Assimp.Scene* model = api.ImportFileFromMemory((byte*)bufferPtr, (uint)buffer.Length, (uint)(Silk.NET.Assimp.PostProcessSteps.CalculateTangentSpace | Silk.NET.Assimp.PostProcessSteps.LimitBoneWeights | Silk.NET.Assimp.PostProcessSteps.FlipUVs), "fbx");
			Silk.NET.Assimp.Scene* model = api.ImportFileFromMemory(ref buffer[0], (uint)buffer.Length, (uint)(Silk.NET.Assimp.PostProcessSteps.CalculateTangentSpace | Silk.NET.Assimp.PostProcessSteps.LimitBoneWeights | Silk.NET.Assimp.PostProcessSteps.FlipUVs), "fbx");

			//Marshal.FreeHGlobal(bufferPtr);

			var mesh = new Mesh();

			var nameToIndex = new Dictionary<string, int>();
			//ParseSkeleton(mesh, model, nameToIndex);

			for (uint p = 0; p < model->MNumMeshes; p++)
			{
				var meshToImport = model->MMeshes[p];

				// Validate sub mesh data
				if (meshToImport->MPrimitiveTypes != (uint)Silk.NET.Assimp.PrimitiveType.PrimitiveTypeTriangle)
				{
					Logs.Core.Write($"Invalid prrimitive type, should be triangle. {meshToImport->MName.AsString}");
					continue;
				}

				var subMesh = new SubMesh();

				// Create vertex format
				if (meshToImport->MNumBones > 0)
				{
					subMesh.VertexFormat = new Renderer.VertexFormat(new Renderer.VertexFormatElement[]
					{
							new Renderer.VertexFormatElement(Renderer.VertexFormatSemantic.Position, Renderer.VertexPointerType.Float, 3, 0),
							new Renderer.VertexFormatElement(Renderer.VertexFormatSemantic.Normal, Renderer.VertexPointerType.Float, 3, sizeof(float) * 3),
							new Renderer.VertexFormatElement(Renderer.VertexFormatSemantic.Tangent, Renderer.VertexPointerType.Float, 3, sizeof(float) * 6),
							new Renderer.VertexFormatElement(Renderer.VertexFormatSemantic.TexCoord, Renderer.VertexPointerType.Float, 2, sizeof(float) * 9),
							new Renderer.VertexFormatElement(Renderer.VertexFormatSemantic.BoneIndex, Renderer.VertexPointerType.Float, 4, sizeof(float) * 11),
							new Renderer.VertexFormatElement(Renderer.VertexFormatSemantic.BoneWeight, Renderer.VertexPointerType.Float, 4, sizeof(float) * 15),
					});
				}
				else
				{
					subMesh.VertexFormat = new Renderer.VertexFormat(new Renderer.VertexFormatElement[]
					{
							new Renderer.VertexFormatElement(Renderer.VertexFormatSemantic.Position, Renderer.VertexPointerType.Float, 3, 0),
							new Renderer.VertexFormatElement(Renderer.VertexFormatSemantic.Normal, Renderer.VertexPointerType.Float, 3, sizeof(float) * 3),
							new Renderer.VertexFormatElement(Renderer.VertexFormatSemantic.Tangent, Renderer.VertexPointerType.Float, 3, sizeof(float) * 6),
							new Renderer.VertexFormatElement(Renderer.VertexFormatSemantic.TexCoord, Renderer.VertexPointerType.Float, 2, sizeof(float) * 9),
					});
				}

				subMesh.TriangleCount = (int)meshToImport->MNumFaces;

				Vertex[] vertices = new Vertex[meshToImport->MNumVertices];

				var positions = meshToImport->MVertices;
				var normals = meshToImport->MNormals;
				var tangents = meshToImport->MTangents;
				var texCoords = meshToImport->MTextureCoords.Element0;
				for (uint v = 0; v < meshToImport->MNumVertices; v++)
				{
					vertices[v].Position = new Vector3(positions[v].X, positions[v].Y, positions[v].Z);
					vertices[v].Normal = new Vector3(normals[v].X, normals[v].Y, normals[v].Z);
					vertices[v].Tangent = new Vector3(tangents[v].X, tangents[v].Y, tangents[v].Z);
					vertices[v].TexCoord = new Vector2(texCoords[v].X, texCoords[v].Y);
					vertices[v].BoneAssignments = new List<BoneAssignment>();
				}

				subMesh.BoundingSphere = BoundingSphere.CreateFromPoints(vertices.Select(x => x.Position));
				subMesh.BoundingBox = BoundingBox.CreateFromSphere(subMesh.BoundingSphere);

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

				using (var memStream = new MemoryStream(((int)meshToImport->MNumVertices) * subMesh.VertexFormat.Size))
				{
					using (var writer = new BinaryWriter(memStream))
					{
						for (int i = 0; i < vertices.Length; i++)
						{
							writer.Write(vertices[i].Position);
							writer.Write(vertices[i].Normal);
							writer.Write(vertices[i].Tangent);
							writer.Write(vertices[i].TexCoord);

							if (meshToImport->MNumBones > 0)
							{
								for (var b = 0; b < 4; b++)
								{
									writer.Write((float)vertices[i].BoneAssignments[b].BoneIndex);
								}

								for (var b = 0; b < 4; b++)
								{
									writer.Write(vertices[i].BoneAssignments[b].Weight);
								}
							}
						}

						subMesh.Vertices = memStream.GetBuffer();
					}
				}

				var indices = new List<int>((int)meshToImport->MNumFaces * 3);
				for (uint f = 0; f < meshToImport->MNumFaces; f++)
				{
					var face = meshToImport->MFaces[f];

					for (uint i = 0; i < face.MNumIndices; i++)
						indices.Add((int)face.MIndices[i]);
				}

				using (var memStream = new MemoryStream(sizeof(int) * indices.Count))
				{
					using (var writer = new BinaryWriter(memStream))
					{
						foreach (var index in indices)
						{
							writer.Write(index);
						}
						subMesh.Indices = memStream.GetBuffer();
					}
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

				mesh.SubMeshes.Add(subMesh);

			}

			return mesh;
		}

        //private void ParseSkeleton(Mesh mesh, Scene model, Dictionary<string, int> nameToIndex)
        //{
        //    // Create skeleton if any of the models have bones
        //    var bones = new Dictionary<string, Bone>();
		//
        //    // Fetch all bones first
        //    foreach (var meshToImport in model.Meshes)
        //    {
        //        if (meshToImport.HasBones)
        //        {
        //            foreach (var bone in meshToImport.Bones)
        //            {
        //                if (!bones.ContainsKey(bone.Name))
        //                {
        //                    bones.Add(bone.Name, bone);
        //                }
        //            }
        //        }
        //    }
		//
        //    if (bones.Any())
        //    {
        //        mesh.Skeleton = new Skeletons.Skeleton
        //        {
        //            Bones = new List<Skeletons.Transform>(),
        //            Animations = new List<Skeletons.Animation>()
        //        };
        //        
        //        // Find actual root node
        //        var rootNode = model.RootNode;
        //        if (!bones.ContainsKey(rootNode.Name))
        //        {
        //            foreach (var child in rootNode.Children)
        //            {
        //                if (bones.ContainsKey(child.Name))
        //                {
        //                    rootNode = child;
        //                    break;
        //                }
        //            }
        //        }
		//
        //        var rootNodeName = rootNode.Name;
		//
        //        // Skeleton system assumes that the root node is located at index 0, so we reserve a slot for it
        //        mesh.Skeleton.Bones.Add(new Skeletons.Transform());
        //        nameToIndex[rootNodeName] = 0;
		//
        //        foreach (var bone in bones)
        //        {
        //            var bindPose = bone.Value.OffsetMatrix;
        //            bindPose.DecomposeNoScaling(out var rotation, out var transalation);
		//
        //            var transform = new Skeletons.Transform
        //            {
        //                Position = new Vector3(transalation.X, transalation.Y, transalation.Z),
        //                Orientation = new Quaternion(rotation.X, rotation.Y, rotation.Z, rotation.W)
        //            };
		//
        //            if (bone.Key == rootNodeName)
        //            {
        //                mesh.Skeleton.Bones[0] = transform;
        //            }
        //            else
        //            {
        //                mesh.Skeleton.Bones.Add(transform);
        //                nameToIndex.Add(bone.Key, mesh.Skeleton.Bones.Count - 1);
        //            }
        //        }
		//
        //        // Parse bone hierarchy
        //        var parentIndexes = new Dictionary<string, int>();
        //        var nameToNode = new Dictionary<string, Node>();
        //        ParseParents(nameToIndex, parentIndexes, nameToNode, rootNode, true);
		//
        //        foreach (var node in nameToNode.Values)
        //        {
        //            var index = nameToIndex[node.Name];
		//
        //            node.Transform.DecomposeNoScaling(out var rotation, out var transalation);
		//
        //            mesh.Skeleton.Bones[index] = new Skeletons.Transform
        //            {
        //                Position = new Vector3(transalation.X, transalation.Y, transalation.Z),
        //                Orientation = new Quaternion(rotation.X, rotation.Y, rotation.Z, rotation.W)
        //            };
        //        }
		//
        //        mesh.Skeleton.BoneParents = new List<int>(mesh.Skeleton.Bones.Count);
        //        for (var i = 0; i < mesh.Skeleton.Bones.Count; i++)
        //        {
        //            mesh.Skeleton.BoneParents.Add(0);
        //        }
		//
        //        foreach (var parentIndex in parentIndexes)
        //        {
        //            if (nameToIndex.ContainsKey(parentIndex.Key))
        //            {
        //                mesh.Skeleton.BoneParents[nameToIndex[parentIndex.Key]] = parentIndex.Value;
        //            }
        //            else
        //            {
        //                Logs.Core.WriteWarning($"Missing bone binding for node {parentIndex.Key}");
        //            }
        //        }
		//
        //        // Parse animations
        //        foreach (var animationToImport in model.Animations)
        //        {
        //            var animation = new Skeletons.Animation
        //            {
        //                Name = animationToImport.Name,
        //                Tracks = new List<Skeletons.Track>(),
        //                Length = (float)(animationToImport.DurationInTicks / animationToImport.TicksPerSecond)
        //            };
		//
        //            foreach (var nodeAnimation in animationToImport.NodeAnimationChannels)
        //            {
        //                // Skip missing bones
        //                if (!bones.ContainsKey(nodeAnimation.NodeName))
        //                    continue;
		//
        //                var track = new Skeletons.Track
        //                {
        //                    BoneIndex = nameToIndex[nodeAnimation.NodeName],
        //                    KeyFrames = new List<Skeletons.KeyFrame>()
        //                };
		//
        //                var defBonePoseInv = nameToNode[nodeAnimation.NodeName].Transform;
        //                defBonePoseInv.Inverse();
		//
        //                for (var i = 0; i < nodeAnimation.PositionKeys.Count; i++)
        //                {
        //                    var position = nodeAnimation.PositionKeys[i].Value;
        //                    var rotation = nodeAnimation.RotationKeys[i].Value;
		//
        //                    var fullTransform = new Matrix4x4(rotation.GetMatrix()) * Matrix4x4.FromTranslation(position);
        //                    var poseToKey = fullTransform * defBonePoseInv;
        //                    poseToKey.DecomposeNoScaling(out var rot, out var pos);
		//
        //                    var time = nodeAnimation.PositionKeys[i].Time / animationToImport.TicksPerSecond;
		//
        //                    track.KeyFrames.Add(new Skeletons.KeyFrame
        //                    {
        //                        Time = (float)time,
        //                        Transform = new Skeletons.Transform
        //                        {
        //                            Position = new Vector3(pos.X, pos.Y, pos.Z),
        //                            Orientation = new Quaternion(rot.X, rot.Y, rot.Z, rot.W)
        //                        }
        //                    });
        //                }
		//
        //                animation.Tracks.Add(track);
        //            }
		//
        //            mesh.Skeleton.Animations.Add(animation);
        //        }
        //    }
        //}

        //private void ParseParents(Dictionary<string, int> nameToIndex, Dictionary<string, int> parentIndexes, Dictionary<string, Node> nameToNode, Node node, bool isRootNode)
        //{
        //    if (nameToIndex.ContainsKey(node.Name))
        //    {
        //        nameToNode.Add(node.Name, node);
        //    }
        //    
        //    if (isRootNode)
        //    {
        //        parentIndexes.Add(node.Name, -1); // This is a root node!
        //    }
        //    else if (nameToIndex.ContainsKey(node.Parent.Name))
        //    {
        //        parentIndexes.Add(node.Name, nameToIndex[node.Parent.Name]);
        //    }
		//
        //    if (node.HasChildren)
        //    {
        //        foreach (var child in node.Children)
        //        {
        //            ParseParents(nameToIndex, parentIndexes, nameToNode, child, false);
        //        }
        //    }
        //}

        struct Vertex
        {
            public Vector3 Position;
            public Vector3 Normal;
            public Vector3 Tangent;
            public Vector2 TexCoord;
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
