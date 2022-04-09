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
			set { this._mesh = value; }
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
				// Mesh was changed
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
			var geometry = new THREE.Core.Geometry();

			foreach (var submesh in Mesh.Res.SubMeshes)
			{
				foreach (var vertex in submesh.Vertices)
					geometry.Vertices.Add(new THREE.Math.Vector3(vertex.X, vertex.Y, vertex.Z));

				foreach (var color in submesh.Colors)
					geometry.Colors.Add(new THREE.Math.Color(color.R, color.G, color.B));

				foreach (var face in submesh.Faces)
					geometry.Faces.Add(new THREE.Core.Face3(face.a, face.b, face.c, new THREE.Math.Vector3(face.normal.X, face.normal.Y, face.normal.Z)));

				foreach (var normal in submesh.Normals)
					geometry.Normals.Add(new THREE.Math.Vector3(normal.X, normal.Y, normal.Z));

				foreach (var uv in submesh.Uvs)
					geometry.Uvs.Add(new THREE.Math.Vector2(uv.X, uv.Y));

				if(threeMesh == null)
				{
					threeMesh = new List<THREE.Objects.Mesh>();
				}
				threeMesh.Add(new THREE.Objects.Mesh(geometry, (submesh.Material != null && submesh.Material.IsAvailable) ? submesh.Material.Res.GetThreeMaterial() : MeshBasicMaterial.Default.Res.GetThreeMaterial()));
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