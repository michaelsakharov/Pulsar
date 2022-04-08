using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Duality.Resources;
using THREE.Math;

namespace Duality.Graphics.Components
{
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

			if (threeMesh != null && GameObj.Transform != null && Mesh.IsAvailable)
			{
				foreach (var submesh in threeMesh)
				{
					//submesh.Material = ((Material != null && Material.IsAvailable) ? Material.Res : MeshBasicMaterial.Default.Res).GetThreeMaterial();
					submesh.Material = MeshLambertMaterial.Default.Res.GetThreeMaterial();
					submesh.Material.Color = new Color().SetHex(0x7777ff);
					submesh.CastShadow = true;
					submesh.ReceiveShadow = true;
					submesh.Position.Set(this.GameObj.Transform.Pos.X, this.GameObj.Transform.Pos.Y, this.GameObj.Transform.Pos.Z);
					submesh.Rotation.Set(this.GameObj.Transform.Rotation.X, this.GameObj.Transform.Rotation.Y, this.GameObj.Transform.Rotation.Z, THREE.Math.RotationOrder.YXZ);
					submesh.Scale.Set(this.GameObj.Transform.Scale.X, this.GameObj.Transform.Scale.Y, this.GameObj.Transform.Scale.Z);
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