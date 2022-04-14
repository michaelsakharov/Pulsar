﻿using System;
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
using Duality.Utility;

namespace Duality.Editor.Plugins.CamView.CamViewStates
{
	/// <summary>
	/// Allows to move, scale and rotate the objects of a <see cref="Scene"/> while providing a
	/// preview through the lens of the editor <see cref="Camera"/>. User input is consumed
	/// by the editor and not forwarded to the game.
	/// </summary>
	public class SceneEditorCamViewState : ObjectEditorCamViewState
	{
		private ObjectSelection selBeforeDrag			= null;
		private	DateTime		dragTime				= DateTime.Now;
		private	Point			dragLastLoc				= Point.Empty;
		private GPUWorldPicker  _GPUWorldPicker			= new GPUWorldPicker();


		public override string StateName
		{
			get { return Properties.CamViewRes.CamViewState_SceneEditor_Name; }
		}
		private bool DragMustWait
		{
			get { return this.DragMustWaitProgress < 1.0f; }
		}
		private float DragMustWaitProgress
		{
			get { return MathF.Clamp((float)(DateTime.Now - this.dragTime).TotalMilliseconds / 500.0f, 0.0f, 1.0f); }
		}


		public SceneEditorCamViewState()
		{
			this.SnapToUserGuides &= (~UserGuideType.Scale);
		}

		internal protected override void OnEnterState()
		{
			base.OnEnterState();

			DualityEditorApp.SelectionChanged      += this.DualityEditorApp_SelectionChanged;
			//DualityEditorApp.ObjectPropertyChanged += this.DualityEditorApp_ObjectPropertyChanged;

			// Initial selection update
			this.ApplyEditorSelection(DualityEditorApp.Selection);
		}
		internal protected override void OnLeaveState()
		{
			base.OnLeaveState();

			// Clear selection lists, they'll be re-populated in OnEnterState again
			this.allObjSel.Clear();
			this.actionObjSel.Clear();
			this.indirectObjSel.Clear();

			DualityEditorApp.SelectionChanged      -= this.DualityEditorApp_SelectionChanged;
			//DualityEditorApp.ObjectPropertyChanged -= this.DualityEditorApp_ObjectPropertyChanged;
		}
		protected override void OnSceneChanged()
		{
			base.OnSceneChanged();
			this.InvalidateSelectionStats();
		}

		public override ObjectEditorSelObj PickSelObjAt(int x, int y)
		{
			int picked = this._GPUWorldPicker.Pick(x, y, DualityApp.GraphicsBackend, Scene.ThreeScene, CameraComponent.GetTHREECamera());
			if (picked == -1 || picked == 0) return null;
			var obj = Scene.GetGameobjectByThreeID(picked);
			if (obj != null)
			{
				if (DesignTimeObjectData.Get(obj).IsLocked) return null;
				return new SceneEditorSelGameObj(obj);
			}
			return null;
		}

		public override void ClearSelection()
		{
			base.ClearSelection();
			DualityEditorApp.Deselect(this, ObjectSelection.Category.GameObjCmp);
			this.ClearContextMenu();
		}
		public override void SelectObjects(IEnumerable<ObjectEditorSelObj> selObjEnum, SelectMode mode = SelectMode.Set)
		{
			base.SelectObjects(selObjEnum, mode);
			DualityEditorApp.Select(this, new ObjectSelection(selObjEnum.Select(s => s.ActualObject)), mode);
		}
		protected override void PostPerformAction(IEnumerable<ObjectEditorSelObj> selObjEnum, ObjectEditorAction action)
		{
			base.PostPerformAction(selObjEnum, action);
			if (action == ObjectEditorAction.Move)
			{
				DualityEditorApp.NotifyObjPropChanged(
					this,
					new ObjectSelection(selObjEnum.Select(s => (s.ActualObject as GameObject).Transform)),
					ReflectionInfo.Property_Transform_LocalPos);
			}
			else if (action == ObjectEditorAction.Rotate)
			{
				DualityEditorApp.NotifyObjPropChanged(
					this,
					new ObjectSelection(selObjEnum.Select(s => (s.ActualObject as GameObject).Transform)),
					ReflectionInfo.Property_Transform_LocalPos,
					ReflectionInfo.Property_Transform_LocalAngle);
			}
			else if (action == ObjectEditorAction.Scale)
			{
				DualityEditorApp.NotifyObjPropChanged(
					this,
					new ObjectSelection(selObjEnum.Select(s => (s.ActualObject as GameObject).Transform)),
					ReflectionInfo.Property_Transform_LocalPos,
					ReflectionInfo.Property_Transform_LocalScale);
			}
		}

		public override void DeleteObjects(IEnumerable<ObjectEditorSelObj> objEnum)
		{
			var objList = objEnum.Select(s => s.ActualObject as GameObject).ToList();
			if (objList.Count == 0) return;

			// Ask user if he really wants to delete stuff
			ObjectSelection objSel = new ObjectSelection(objList);
			if (!DualityEditorApp.DisplayConfirmDeleteObjects(objSel)) return;
			if (!DualityEditorApp.DisplayConfirmBreakPrefabLinkStructure(objSel)) return;

			UndoRedoManager.Do(new DeleteGameObjectAction(objList));
		}
		public override List<ObjectEditorSelObj> CloneObjects(IEnumerable<ObjectEditorSelObj> objEnum)
		{
			if (objEnum == null || !objEnum.Any()) return base.CloneObjects(objEnum);
			var objList = objEnum.Select(s => s.ActualObject as GameObject).ToList();

			CloneGameObjectAction cloneAction = new CloneGameObjectAction(objList);
			UndoRedoManager.Do(cloneAction);

			return cloneAction.Result.Select(o => new SceneEditorSelGameObj(o) as ObjectEditorSelObj).ToList();
		}

		protected override void OnDragEnter(DragEventArgs e)
		{
			base.OnDragEnter(e);

			DataObject data = e.Data as DataObject;
			if (new ConvertOperation(data, ConvertOperation.Operation.All).CanPerform<GameObject>())
			{
				e.Effect = e.AllowedEffect;
				this.dragTime = DateTime.Now;
				this.dragLastLoc = new Point(e.X, e.Y);
			}
			else
			{
				e.Effect = DragDropEffects.None;
				this.dragLastLoc = Point.Empty;
				this.dragTime = DateTime.Now;
			}
		}
		protected override void OnDragLeave(EventArgs e)
		{
			base.OnDragLeave(e);

			this.dragLastLoc = Point.Empty;
			this.dragTime = DateTime.Now;
			this.Invalidate();
			if (this.ObjAction == ObjectEditorAction.None) return;
			
			this.EndAction();

			// Destroy temporarily instantiated objects
			foreach (ObjectEditorSelObj obj in this.allObjSel)
			{
				GameObject gameObj = obj.ActualObject as GameObject;
				Scene.Current.RemoveObject(gameObj);
				gameObj.Dispose();
			}
			DualityEditorApp.Select(this, this.selBeforeDrag);
		}
		protected override void OnDragOver(DragEventArgs e)
		{
			base.OnDragOver(e);

			if (e.Effect == DragDropEffects.None) return;
			if (this.ObjAction == ObjectEditorAction.None && !this.DragMustWait)
				this.DragBeginAction(e);
			
			Point clientCoord = this.PointToClient(new Point(e.X, e.Y));
			if (Math.Abs(clientCoord.X - this.dragLastLoc.X) > 20 || Math.Abs(clientCoord.Y - this.dragLastLoc.Y) > 20)
				this.dragTime = DateTime.Now;
			this.dragLastLoc = clientCoord;
			this.Invalidate();

			if (this.ObjAction != ObjectEditorAction.None) this.UpdateAction();
		}
		protected override void OnDragDrop(DragEventArgs e)
		{
			base.OnDragDrop(e);

			if (this.ObjAction == ObjectEditorAction.None)
			{
				this.DragBeginAction(e);
				if (this.ObjAction != ObjectEditorAction.None) this.UpdateAction();
			}
			
			this.dragLastLoc = Point.Empty;
			this.dragTime = DateTime.Now;

			if (this.ObjAction != ObjectEditorAction.None) this.EndAction();

			UndoRedoManager.EndMacro();
		}

		private void DragBeginAction(DragEventArgs e)
		{
			DataObject data = e.Data as DataObject;
			var dragObjQuery = new ConvertOperation(data, ConvertOperation.Operation.All).Perform<GameObject>();
			if (dragObjQuery != null)
			{
				List<GameObject> dragObj = dragObjQuery.ToList();

				THREE.Math.Vector3 cameraPosition = new THREE.Math.Vector3().SetFromMatrixPosition(CameraComponent.GetTHREECamera().MatrixWorld);
				THREE.Math.Vector3 direction = new THREE.Math.Vector3().Set(e.X, e.Y, 0.5f).UnProject(CameraComponent.GetTHREECamera()).Sub(cameraPosition).Normalize();

				Vector3 pos = new Vector3(cameraPosition.X, cameraPosition.Y, cameraPosition.Z) + (new Vector3(direction.X, direction.Y, direction.Z) * 10f);

				// Setup GameObjects
				CreateGameObjectAction createAction = new CreateGameObjectAction(null, dragObj);
				DropGameObjectInSceneAction dropAction = new DropGameObjectInSceneAction(dragObj, pos, this.CameraObj.Transform.Rotation);
				UndoRedoManager.BeginMacro(dropAction.Name);
				UndoRedoManager.Do(createAction);
				UndoRedoManager.Do(dropAction);
			
				// Select them & begin action
				this.selBeforeDrag = DualityEditorApp.Selection;
				this.SelectObjects(createAction.Result.Select(g => new SceneEditorSelGameObj(g) as ObjectEditorSelObj));
				this.BeginAction(ObjectEditorAction.Move);
			
				// Get focused
				this.Focus();
			
				e.Effect = e.AllowedEffect;
			}
		}

		private void DualityEditorApp_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if ((e.AffectedCategories & ObjectSelection.Category.GameObjCmp) == ObjectSelection.Category.None)
			{
				this.ClearContextMenu();
				return;
			}
			if (e.SameObjects) return;

			this.ApplyEditorSelection(e.Current);
		}

		private void ApplyEditorSelection(ObjectSelection selection)
		{
			// Update the list of all selected objects
			this.allObjSel = 
				selection.GameObjects
				.Select(g => new SceneEditorSelGameObj(g) as ObjectEditorSelObj)
				.ToList();

			// Update the list of indirectly selected objects
			{
				IEnumerable<GameObject> selectedGameObj = selection.GameObjects;
				IEnumerable<GameObject> indirectViaCmpQuery = selection.Components.GameObject();
				IEnumerable<GameObject> indirectViaChildQuery = selectedGameObj.ChildrenDeep();
				IEnumerable<GameObject> indirectQuery = 
					indirectViaCmpQuery
					.Concat(indirectViaChildQuery)
					.Except(selectedGameObj)
					.Distinct();
				this.indirectObjSel = 
					indirectQuery
					.Select(g => new SceneEditorSelGameObj(g) as ObjectEditorSelObj)
					.ToList();
			}

			// Update (parent-free) action list based on the selection changes
			{
				// Start by assuming all selected objects to be part of the action
				this.actionObjSel.Clear();
				this.actionObjSel.AddRange(this.allObjSel);

				// Remove GameObjects from the action list if their parent is part of 
				// the selection as well, so we can avoid moving the same object twice.
				for (int i = this.actionObjSel.Count - 1; i >= 0; i--)
				{
					bool parentInSelection = false;
					for (int j = 0; j < this.actionObjSel.Count; j++)
					{
						parentInSelection = this.IsAffectedByParent(
							this.actionObjSel[i].ActualObject as GameObject, 
							this.actionObjSel[j].ActualObject as GameObject);
						if (parentInSelection) break;
					}
					if (parentInSelection)
					{
						this.actionObjSel.RemoveAt(i);
					}
				}
			}

			this.InvalidateSelectionStats();
			this.UpdateAction();
			this.Invalidate();
		}
		private bool IsAffectedByParent(GameObject child, GameObject parent)
		{
			if (!child.IsChildOf(parent)) return false;
			if (child.Transform == null) return false;
			if (parent.Transform == null) return false;
			if (child.Transform.IgnoreParent) return false;
			return true;
		}
		private void ClearContextMenu()
		{
			if (this.RenderableControl == null || this.RenderableControl.ContextMenu == null)
				return;

			this.RenderableControl.ContextMenu.MenuItems.Clear();
		}
	}
}
