using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using Duality;
using Duality.Components;
using Duality.Resources;
using Duality.Drawing;
using Duality.Editor;
using Duality.Editor.Forms;
using Duality.Editor.UndoRedoActions;
using Duality.Editor.Plugins.CamView.UndoRedoActions;

namespace Duality.Editor.Plugins.CamView.CamViewStates
{
	public class SceneEditorSelGameObj : ObjectEditorSelObj
	{
		private	GameObject	gameObj;

		public override object ActualObject
		{
			get { return this.gameObj == null || this.gameObj.Disposed ? null : this.gameObj; }
		}
		public override bool HasTransform
		{
			get { return this.gameObj != null && !this.gameObj.Disposed && this.gameObj.Transform != null; }
		}
		public override Vector3 Pos
		{
			get { return this.gameObj.Transform.Pos; }
			set { this.gameObj.Transform.Pos = value; }
		}
		public override Vector3 Angle
		{
			get { return this.gameObj.Transform.Rotation; }
			set { this.gameObj.Transform.Rotation = value; }
		}
		public override Vector3 Scale
		{
			get { return this.gameObj.Transform.Scale; }
			set { this.gameObj.Transform.Scale = value; }
		}
		public override THREE.Math.Box3 BoundRadius
		{
			get
			{
				List<int> ids = Scene.GetThreeIDsByGameObject(this.gameObj);
				THREE.Math.Box3 boundingBox = null;
				foreach (int id in ids)
				{
					var obj = Scene.ThreeScene.GetObjectById(id);
					if (obj != null)
					{
						if (boundingBox == null)
							boundingBox = new THREE.Math.Box3().SetFromObject(obj);
						else
							boundingBox.ExpandByObject(obj);
					}
				}
				//return new Vector3(boundingBox.GetSize().X, boundingBox.GetSize().Y, boundingBox.GetSize().Z);
				//return boundingBox != null ? new BoundingBox(new Vector3(boundingBox.Min.X, boundingBox.Min.Y, boundingBox.Min.Z), new Vector3(boundingBox.Max.X, boundingBox.Max.Y, boundingBox.Max.Z)) : new BoundingBox(Vector3.Zero, Vector3.Zero);
				return boundingBox != null ? boundingBox : new THREE.Math.Box3(new THREE.Math.Vector3(-0.1f, -0.1f, -0.1f), new THREE.Math.Vector3(0.1f, 0.1f, 0.1f));
			}
		}
		public override bool ShowAngle
		{
			get { return true; }
		}

		public SceneEditorSelGameObj(GameObject obj)
		{
			this.gameObj = obj;
		}

		public override bool IsActionAvailable(ObjectEditorAction action)
		{
			if (action == ObjectEditorAction.Move ||
				action == ObjectEditorAction.Rotate ||
				action == ObjectEditorAction.Scale)
				return this.HasTransform;
			return false;
		}
		public override string UpdateActionText(ObjectEditorAction action, bool performing)
		{
			if (action == ObjectEditorAction.Move)
			{
				return
					string.Format("X:{0,7:0}/n", this.gameObj.Transform.LocalPos.X) +
					string.Format("Y:{0,7:0}/n", this.gameObj.Transform.LocalPos.Y) +
					string.Format("Z:{0,7:0}", this.gameObj.Transform.LocalPos.Z);
			}
			else if (action == ObjectEditorAction.Scale)
			{
				return
					string.Format("Scale X:{0,7:0}/n", this.gameObj.Transform.LocalScale.X) +
					string.Format("Scale Y:{0,7:0}/n", this.gameObj.Transform.LocalScale.Y) +
					string.Format("Scale Z:{0,7:0}", this.gameObj.Transform.LocalScale.Z);
			}
			else if (action == ObjectEditorAction.Rotate)
			{
				Vector3 rotation = this.gameObj.Transform.LocalRotation;
				return
					string.Format("Rotation X:{0,7:0}/n", rotation.X) +
					string.Format("Rotation Y:{0,7:0}/n", rotation.Y) +
					string.Format("Rotation Z:{0,7:0}", rotation.Z);
			}

			return base.UpdateActionText(action, performing);
		}
	}
}
