using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Duality.Components;
using Duality.Editor;
using Duality.Properties;
using Duality.Resources;
using THREE.Core;
using THREE.Math;

namespace Duality.Graphics.Components
{
	[RequiredComponent(typeof(Transform))]
	[EditorHintCategory(CoreResNames.CategoryGraphics)]
	[EditorHintImage(CoreResNames.ImageFragmentShader)]
	public class MeshComponent : Component, ICmpInitializable, ICmpUpdatable, ICmpEditorUpdatable, IDisposable
	{
		[DontSerialize] protected bool _meshDirty = true; // Start Dirty
		[DontSerialize] protected bool _inScene = false;

		[DontSerialize] List<THREE.Objects.Mesh> threeMesh;

		public ContentRef<Mesh> _mesh = null;
		/// <summary>
		/// [GET / SET] The <see cref="Mesh"/> that is to be rendered by this component.
		/// </summary>
		public ContentRef<Mesh> Mesh
		{
			get { return this._mesh; }
			set { this._mesh = value; _meshDirty = true; }
		}

		public ContentRef<Material> _defaultMaterial = MeshPhongMaterial.Default.As<Material>();
		/// <summary>
		/// [GET / SET] The <see cref="Material"/> used by default if no material is assign from the Materials variable.
		/// </summary>
		public ContentRef<Material> DefaultMaterial
		{
			get { return this._defaultMaterial; }
			set { this._defaultMaterial = value; }
		}

		public ContentRef<Material>[] _materials = null;
		/// <summary>
		/// [GET / SET] The <see cref="Material"/>'s used to render each mesh
		/// </summary>
		public ContentRef<Material>[] Materials
		{
			get { return this._materials; }
			set { this._materials = value; }
		}

		void ICmpInitializable.OnActivate()
		{
			if (threeMesh == null && Mesh.IsAvailable)
			{
				CreateThreeMesh();
				_inScene = true;
				foreach (var submesh in threeMesh)
					Scene.ThreeScene.Add(submesh);
			}
			UpdateMeshObject();
		}

		void ICmpInitializable.OnDeactivate()
		{
			if(threeMesh != null)
			{
				_inScene = false;
				foreach (var submesh in threeMesh)
					Scene.ThreeScene.Remove(submesh);
				threeMesh = null;
			}
		}

		void ICmpUpdatable.OnUpdate()
		{
			UpdateMeshObject();
		}

		void ICmpEditorUpdatable.OnUpdate()
		{
			UpdateMeshObject();
		}

		void UpdateMeshObject()
		{
			if (_meshDirty)
			{
				_meshDirty = false;
				// Mesh was changed
				if (threeMesh != null)
				{
					// Destroy the old Mesh
					_inScene = false;
					foreach (var submesh in threeMesh)
						Scene.ThreeScene.Remove(submesh);
					threeMesh = null;
					// If the new Mesh is loaded and is in memory
					if (Mesh.IsAvailable)
					{
						// Load the new mesh into this component
						CreateThreeMesh();
						_inScene = true;
						foreach (var submesh in threeMesh)
							Scene.ThreeScene.Add(submesh);
					}
				}
			}

			if (threeMesh != null && Mesh.IsAvailable)
			{
				int matID = 1;
				foreach (var submesh in threeMesh)
				{
					if(Materials != null && Materials.Count() >= matID)
					{
						if (Materials[matID] != null && Materials[matID].IsAvailable)
						{
							submesh.Material = Materials[matID].Res.GetThreeMaterial();
						}
						else
						{
							submesh.Material = ((DefaultMaterial != null && DefaultMaterial.IsAvailable) ? DefaultMaterial.Res : MeshBasicMaterial.Default.Res).GetThreeMaterial();
						}
					}
					else
					{
						submesh.Material = ((DefaultMaterial != null && DefaultMaterial.IsAvailable) ? DefaultMaterial.Res : MeshBasicMaterial.Default.Res).GetThreeMaterial();
					}
					submesh.CastShadow = true;
					submesh.ReceiveShadow = true;
					submesh.Position.Set(this.GameObj.Transform.Pos.X, this.GameObj.Transform.Pos.Y, this.GameObj.Transform.Pos.Z);
					submesh.Rotation.Set(this.GameObj.Transform.Rotation.X, this.GameObj.Transform.Rotation.Y, this.GameObj.Transform.Rotation.Z, THREE.Math.RotationOrder.YXZ);
					submesh.Scale.Set(this.GameObj.Transform.Scale.X, this.GameObj.Transform.Scale.Y, this.GameObj.Transform.Scale.Z);
					matID++;
				}
			}
		}

		void CreateThreeMesh()
		{
			// This needs to be Improved, Ideally we want to find a way to store a Geometry object directly
			// I think we need to re-introduce Json Saving/Loading on the THREE Port, so that
			// we can store a Mesh as a Json once loaded, and when the Mesh resource is loaded Compile the Geometry There
			// instead of here, and store it when used
			foreach (var submesh in Mesh.Res.SubMeshes)
			{
				THREE.Core.DirectGeometry geometry = new THREE.Core.DirectGeometry();

				foreach (var vertex in submesh.Vertices)
					geometry.Vertices.Add(new THREE.Math.Vector3(vertex.X, vertex.Y, vertex.Z));

				foreach (var color in submesh.Colors)
					geometry.Colors.Add(new THREE.Math.Color(color.R, color.G, color.B));

				foreach (var normal in submesh.Normals)
					geometry.Normals.Add(new THREE.Math.Vector3(normal.X, normal.Y, normal.Z));

				foreach (var uv in submesh.Uvs)
					geometry.Uvs.Add(new THREE.Math.Vector2(uv.X, uv.Y));

				foreach (var uv2 in submesh.Uvs2)
					geometry.Uvs2.Add(new THREE.Math.Vector2(uv2.X, uv2.Y));

				foreach (var skin in submesh.SkinIndices)
					geometry.SkinIndices.Add(new THREE.Math.Vector4(skin.X, skin.Y, skin.Z, skin.W));

				foreach (var skin in submesh.SkinWeights)
					geometry.SkinWeights.Add(new THREE.Math.Vector4(skin.X, skin.Y, skin.Z, skin.W));

				foreach (var draw in submesh.Groups)
					geometry.Groups.Add(new THREE.Core.DrawRange() { Count = (int)draw.X, Start = (int)draw.Y, MaterialIndex = (int)draw.Z });

				var geometry2 = new BufferGeometry().FromDirectGeometry(geometry);

				if (threeMesh == null)
				{
					threeMesh = new List<THREE.Objects.Mesh>();
				}
				var mesh = new THREE.Objects.Mesh(geometry2, (submesh.Material != null && submesh.Material.IsAvailable) ? submesh.Material.Res.GetThreeMaterial() : MeshBasicMaterial.Default.Res.GetThreeMaterial());
				threeMesh.Add(mesh);
			}
		}

		void IDisposable.Dispose()
		{
			if (threeMesh != null)
			{
				foreach (var submesh in threeMesh)
					submesh.Dispose();
				if(_inScene)
					Scene.ThreeScene.Remove(threeMesh);
			}
		}
	}
}