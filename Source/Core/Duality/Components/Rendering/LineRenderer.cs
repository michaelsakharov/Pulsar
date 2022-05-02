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
	public class LineRenderer : Component, ICmpInitializable, IDisposable
	{
		[DontSerialize] protected bool _Dirty = true; // Start Dirty
		[DontSerialize] protected bool _inScene = false;

		[DontSerialize] THREE.Objects.Line threeObj;

		private Vector3[] points = new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, 1, 0) };
		
		public Vector3[] Points
		{
			get { return this.points; }
			set { this.points = value; _Dirty = true; }
		}

		public ContentRef<Material> _defaultMaterial = LineBasicMaterial.Default.As<Material>();
		/// <summary>
		/// [GET / SET] The <see cref="Material"/> used by default if no material is assign from the Materials variable.
		/// </summary>
		public ContentRef<Material> DefaultMaterial
		{
			get { return this._defaultMaterial; }
			set { this._defaultMaterial = value; _Dirty = true; }
		}

		void ICmpInitializable.OnActivate()
		{
			if (threeObj == null)
			{
				CreateThreeMesh();
			}
			DualityApp.PreRender += DualityApp_PreRender;
		}

		private void DualityApp_PreRender(Scene scene, Camera camera)
		{
			if (_Dirty)
			{
				_Dirty = false;
				if (threeObj != null)
				{
					// Destroy the old Mesh
					if (_inScene)
						Scene.ThreeScene.Remove(threeObj);
					_inScene = false;
					threeObj = null;
					if (points.Length > 0)
					{
						CreateThreeMesh();
					}
				}
			}

			if (threeObj != null && points.Length > 0)
			{
				threeObj.Material = ((DefaultMaterial != null && DefaultMaterial.IsAvailable) ? DefaultMaterial.Res : LineBasicMaterial.Default.Res).GetThreeMaterial();

				if (Duality.Resources.Scene.Current.MoveWorldInsteadOfCamera)
				{
					Vector3 Pos = this.GameObj.Transform.Pos - camera.GameObj.Transform.Pos;
					threeObj.Position.Set((float)Pos.X, (float)Pos.Y, (float)Pos.Z);
				}
				else
				{
					threeObj.Position.Set((float)this.GameObj.Transform.Pos.X, (float)this.GameObj.Transform.Pos.Y, (float)this.GameObj.Transform.Pos.Z);
				}

				threeObj.Rotation.Set((float)this.GameObj.Transform.Rotation.X, (float)this.GameObj.Transform.Rotation.Y, (float)this.GameObj.Transform.Rotation.Z, THREE.Math.RotationOrder.YXZ);
				threeObj.Scale.Set((float)this.GameObj.Transform.Scale.X, (float)this.GameObj.Transform.Scale.Y, (float)this.GameObj.Transform.Scale.Z);
			}
		}

		void ICmpInitializable.OnDeactivate()
		{
			if(threeObj != null)
			{
				_inScene = false;
				Scene.ThreeScene.Remove(threeObj);
				threeObj = null;
			}
			DualityApp.PreRender -= DualityApp_PreRender;
		}

		void CreateThreeMesh()
		{
			var geom = new Geometry();
			List<THREE.Math.Vector3> threePoints = new List<THREE.Math.Vector3>();

			foreach (var ourPoint in points)
				threePoints.Add(ourPoint);
			geom.setFromPoints(threePoints);

			threeObj = new THREE.Objects.Line(geom, LineBasicMaterial.Default.Res.GetThreeMaterial());
			_inScene = true;
			Scene.ThreeScene.Add(threeObj);
		}

		void IDisposable.Dispose()
		{
			if (threeObj != null)
			{
				threeObj.Dispose();
				if(_inScene)
					Scene.ThreeScene.Remove(threeObj);
				_inScene = false;
			}
		}
	}
}