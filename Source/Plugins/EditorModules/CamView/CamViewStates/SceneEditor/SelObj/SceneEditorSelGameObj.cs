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
		public override float BoundRadius
		{
			get
			{
				ICmpRenderer renderer = this.gameObj.GetComponent<ICmpRenderer>();
				if (renderer == null)
				{
					if (this.gameObj.Transform != null)
						return CamView.DefaultDisplayBoundRadius * this.gameObj.Transform.Scale.Length;
					else
						return CamView.DefaultDisplayBoundRadius;
				}

				CullingInfo info;
				renderer.GetCullingInfo(out info);
				return info.Radius;
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
