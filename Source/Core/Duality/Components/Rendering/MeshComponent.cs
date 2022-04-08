using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Duality.Resources;

namespace Duality.Graphics.Components
{
	public class MeshComponent : Component, ICmpInitializable, ICmpUpdatable, ICmpEditorUpdatable, IDisposable
	{
		[DontSerialize] protected bool _meshDirty = true; // Start Dirty
		[DontSerialize] protected bool _inScene = false;

		[DontSerialize] THREE.Objects.Mesh mesh;

		public ContentRef<Material> _material = null;
		/// <summary>
		/// [GET / SET] The <see cref="Mesh"/> that is to be rendered by this component.
		/// </summary>
		public ContentRef<Material> Material
		{
			get { return this._material; }
			set { this._material = value; }
		}

		void ICmpInitializable.OnActivate()
		{
			if (mesh == null)
			{
				mesh = new THREE.Objects.Mesh(new THREE.Geometries.BoxGeometry(1, 1, 1), (Material != null && Material.IsAvailable) ? Material.Res.GetThreeMaterial() : MeshBasicMaterial.Default.Res.GetThreeMaterial());
				_inScene = true;
				Scene.ThreeScene.Add(mesh);
			}
			UpdateMeshObject();
		}

		void ICmpInitializable.OnDeactivate()
		{
			if(mesh != null)
			{
				_inScene = false;
				Scene.ThreeScene.Remove(mesh);
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
			if (mesh != null && GameObj.Transform != null)
			{
				mesh.Position.Set(this.GameObj.Transform.Pos.X, this.GameObj.Transform.Pos.Y, this.GameObj.Transform.Pos.Z);
				mesh.Rotation.Set(this.GameObj.Transform.Rotation.X, this.GameObj.Transform.Rotation.Y, this.GameObj.Transform.Rotation.Z, THREE.Math.RotationOrder.XYZ);
				mesh.Scale.Set(this.GameObj.Transform.Scale.X, this.GameObj.Transform.Scale.Y, this.GameObj.Transform.Scale.Z);
			}
		}

		void IDisposable.Dispose()
		{
			if (mesh != null)
			{
				mesh.Dispose();
				if(_inScene)
					Scene.ThreeScene.Remove(mesh);
			}
		}
	}
}