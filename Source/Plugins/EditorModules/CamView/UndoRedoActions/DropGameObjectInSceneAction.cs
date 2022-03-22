﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Duality;
using Duality.Cloning;
using Duality.Resources;

using Duality.Editor;
using Duality.Editor.UndoRedoActions;
using Duality.Editor.Plugins.CamView.Properties;
using Duality.Editor.Plugins.CamView.CamViewStates;

namespace Duality.Editor.Plugins.CamView.UndoRedoActions
{
	public class DropGameObjectInSceneAction : GameObjectAction
	{
		private Vector3[]		backupPos	= null;
		private Quaternion[]			backupAngle	= null;
		private	Vector3			dropAt		= Vector3.Zero;
		private Quaternion turnBy		= Quaternion.Identity;

		protected override string NameBase
		{
			get { return CamViewRes.UndoRedo_DropGameObjectInScene; }
		}
		protected override string NameBaseMulti
		{
			get { return CamViewRes.UndoRedo_DropGameObjectInSceneMulti; }
		}

		public DropGameObjectInSceneAction(IEnumerable<GameObject> obj, Vector3 dropAt, Quaternion turnBy) : base(obj.Where(o => o != null && o.Transform != null))
		{
			this.dropAt = dropAt;
			this.turnBy = turnBy;
		}

		public override void Do()
		{
			if (this.backupPos == null)
			{
				this.backupPos = new Vector3[this.targetObj.Length];
				this.backupAngle = new Quaternion[this.targetObj.Length];
				for (int i = 0; i < this.targetObj.Length; i++)
				{
					if (this.targetObj[i].Transform == null) continue;
					this.backupPos[i] = this.targetObj[i].Transform.Pos;
					this.backupAngle[i] = this.targetObj[i].Transform.Rotation;
				}
			}

			foreach (GameObject s in this.targetObj)
			{
				if (s.Transform == null) continue;
				s.Transform.Pos = this.dropAt;
				s.Transform.Rotation += this.turnBy;
			}

			if (this.turnBy != Quaternion.Identity)
			{
				DualityEditorApp.NotifyObjPropChanged(
					this,
					new ObjectSelection(this.targetObj.Transform()),
					ReflectionInfo.Property_Transform_LocalPos,
					ReflectionInfo.Property_Transform_LocalAngle);
			}
			else
			{
				DualityEditorApp.NotifyObjPropChanged(
					this,
					new ObjectSelection(this.targetObj.Transform()),
					ReflectionInfo.Property_Transform_LocalPos);
			}
		}
		public override void Undo()
		{
			if (this.backupPos == null) throw new InvalidOperationException("Can't undo what hasn't been done yet");

			for (int i = 0; i < this.targetObj.Length; i++)
			{
				if (this.targetObj[i].Transform == null) continue;
				this.targetObj[i].Transform.Pos = this.backupPos[i];
				this.targetObj[i].Transform.Rotation = this.backupAngle[i];
			}

			if (this.turnBy != Quaternion.Identity)
			{
				DualityEditorApp.NotifyObjPropChanged(
					this,
					new ObjectSelection(this.targetObj.Transform()),
					ReflectionInfo.Property_Transform_LocalPos,
					ReflectionInfo.Property_Transform_LocalAngle);
			}
			else
			{
				DualityEditorApp.NotifyObjPropChanged(
					this,
					new ObjectSelection(this.targetObj.Transform()),
					ReflectionInfo.Property_Transform_LocalPos);
			}
		}
	}
}
