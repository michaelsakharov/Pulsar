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
	public class PointsComponent : Component, ICmpInitializable, IDisposable
	{
		[DontSerialize] protected bool _meshDirty = true; // Start Dirty
		[DontSerialize] protected bool _inScene = false;

		[DontSerialize] THREE.Objects.Points threeMesh;

		public ContentRef<Mesh> _mesh = null;
		/// <summary>
		/// [GET / SET] The <see cref="Mesh"/> that is to be rendered by this component.
		/// </summary>
		public ContentRef<Mesh> Mesh
		{
			get { return this._mesh; }
			set { this._mesh = value; _meshDirty = true; }
		}

		public ContentRef<Material> _Material = PointsMaterial.Default.As<Material>();
		/// <summary>
		/// [GET / SET] The <see cref="Material"/> used by default if no material is assign from the Materials variable.
		/// </summary>
		public ContentRef<Material> Material
		{
			get { return this._Material; }
			set { this._Material = value; }
		}


		void ICmpInitializable.OnActivate()
		{
			if (threeMesh == null && Mesh.IsAvailable)
			{
				CreateThreeMesh();
			}
			DualityApp.PreRender += DualityApp_PreRender;
		}

		private void DualityApp_PreRender(Scene scene, Camera camera)
		{
			if (_meshDirty)
			{
				_meshDirty = false;
				// Mesh was changed
				if (threeMesh != null)
				{
					// Destroy the old Mesh
					if (_inScene)
						Scene.ThreeScene.Remove(threeMesh);
					_inScene = false;
					threeMesh = null;
					// If the new Mesh is loaded and is in memory
					if (Mesh.IsAvailable)
					{
						// Load the new mesh into this component
						CreateThreeMesh();
					}
				}
			}

			if (threeMesh != null && Mesh.IsAvailable)
			{
				threeMesh.Material = ((Material != null && Material.IsAvailable) ? Material.Res : PointsMaterial.Default.Res).GetThreeMaterial();
				threeMesh.CastShadow = true;
				threeMesh.ReceiveShadow = true;


				if (Duality.Resources.Scene.Current.MoveWorldInsteadOfCamera)
				{
					Vector3 Pos = this.GameObj.Transform.Pos - camera.GameObj.Transform.Pos;
					threeMesh.Position.Set((float)Pos.X, (float)Pos.Y, (float)Pos.Z);
				}
				else
				{
					threeMesh.Position.Set((float)this.GameObj.Transform.Pos.X, (float)this.GameObj.Transform.Pos.Y, (float)this.GameObj.Transform.Pos.Z);
				}

				threeMesh.Rotation.Set((float)this.GameObj.Transform.Rotation.X, (float)this.GameObj.Transform.Rotation.Y, (float)this.GameObj.Transform.Rotation.Z, THREE.Math.RotationOrder.YXZ);
				threeMesh.Scale.Set((float)this.GameObj.Transform.Scale.X, (float)this.GameObj.Transform.Scale.Y, (float)this.GameObj.Transform.Scale.Z);
			}
		}

		void ICmpInitializable.OnDeactivate()
		{
			if(threeMesh != null)
			{
				_inScene = false;
				Scene.ThreeScene.Remove(threeMesh);
				threeMesh = null;
			}
			DualityApp.PreRender -= DualityApp_PreRender;
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
					geometry.Vertices.Add(vertex);

				foreach (var color in submesh.Colors)
					geometry.Colors.Add(color);

				var geometry2 = new BufferGeometry().FromDirectGeometry(geometry);

				threeMesh = new THREE.Objects.Points(geometry2, (submesh.Material != null && submesh.Material.IsAvailable) ? submesh.Material.Res.GetThreeMaterial() : MeshBasicMaterial.Default.Res.GetThreeMaterial());
				_inScene = true;
				Scene.ThreeScene.Add(threeMesh);
			}
		}

		void IDisposable.Dispose()
		{
			if (threeMesh != null)
			{
				threeMesh.Dispose();
				if(_inScene)
					Scene.ThreeScene.Remove(threeMesh);
				_inScene = false;
			}
		}
	}
}