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
	public class MeshComponent : Component, ICmpInitializable, IDisposable
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

		private bool castShadow = true;
		public bool CastShadow { get { return this.castShadow; } set { this.castShadow = value; } }

		private bool receiveShadow = true;
		public bool ReceiveShadow { get { return this.receiveShadow; } set { this.receiveShadow = value; } }

		private bool frustumCulled = true;
		public bool FrustumCulled { get { return this.frustumCulled; } set { this.frustumCulled = value; } }

		void ICmpInitializable.OnActivate()
		{
			if (threeMesh == null && Mesh.IsAvailable)
			{
				CreateThreeMesh();
				_inScene = true;
				foreach (var submesh in threeMesh)
					Scene.AddToThreeScene(submesh, GameObj);
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
					_inScene = false;
					foreach (var submesh in threeMesh)
						Scene.RemoveFromThreeScene(submesh);
					threeMesh = null;
					// If the new Mesh is loaded and is in memory
					if (Mesh.IsAvailable)
					{
						// Load the new mesh into this component
						CreateThreeMesh();
						_inScene = true;
						foreach (var submesh in threeMesh)
							Scene.AddToThreeScene(submesh, GameObj);
					}
				}
			}

			if (threeMesh != null && Mesh.IsAvailable)
			{
				int matID = 1;
				foreach (var submesh in threeMesh)
				{
					if (Materials != null && Materials.Count() >= matID)
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
					submesh.CastShadow = CastShadow;
					submesh.ReceiveShadow = ReceiveShadow;
					submesh.FrustumCulled = FrustumCulled;

					if (Duality.Resources.Scene.Current.MoveWorldInsteadOfCamera)
					{
						Vector3 Pos = this.GameObj.Transform.Pos - camera.GameObj.Transform.Pos;
						submesh.Position.Set((float)Pos.X, (float)Pos.Y, (float)Pos.Z);
					}
					else
					{
						submesh.Position.Set((float)this.GameObj.Transform.Pos.X, (float)this.GameObj.Transform.Pos.Y, (float)this.GameObj.Transform.Pos.Z);
					}

					submesh.Rotation.Set((float)this.GameObj.Transform.Rotation.X, (float)this.GameObj.Transform.Rotation.Y, (float)this.GameObj.Transform.Rotation.Z, THREE.Math.RotationOrder.YXZ);
					submesh.Scale.Set((float)this.GameObj.Transform.Scale.X, (float)this.GameObj.Transform.Scale.Y, (float)this.GameObj.Transform.Scale.Z);


					matID++;
				}
			}
		}

		void ICmpInitializable.OnDeactivate()
		{
			if(threeMesh != null)
			{
				_inScene = false;
				foreach (var submesh in threeMesh)
					Scene.RemoveFromThreeScene(submesh);
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

				if (submesh.Vertices != null) foreach (var vertex in submesh.Vertices) geometry.Vertices.Add(vertex);

				if (submesh.Triangles != null) foreach (var triangle in submesh.Triangles) geometry.Indices.Add(triangle);

				if (submesh.Colors != null) foreach (var color in submesh.Colors) geometry.Colors.Add(color);

				if (submesh.Normals != null) foreach (var normal in submesh.Normals) geometry.Normals.Add(normal);

				if (submesh.Uvs != null) foreach (var uv in submesh.Uvs) geometry.Uvs.Add(uv);

				if (submesh.Uvs2 != null) foreach (var uv2 in submesh.Uvs2) geometry.Uvs2.Add(uv2);

				if (submesh.SkinIndices != null) foreach (var skin in submesh.SkinIndices) geometry.SkinIndices.Add(skin);

				if (submesh.SkinWeights != null) foreach (var skin in submesh.SkinWeights) geometry.SkinWeights.Add(skin);

				if (submesh.Groups != null) foreach (var draw in submesh.Groups) geometry.Groups.Add(new THREE.Core.DrawRange() { Count = (int)draw.X, Start = (int)draw.Y, MaterialIndex = (int)draw.Z });

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
				{
					if (_inScene)
						Scene.RemoveFromThreeScene(submesh);
					submesh.Dispose();
				}
			}
		}
	}
}