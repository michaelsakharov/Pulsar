using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;
using System.Windows.Forms;

using Duality;
using Duality.Components;
using Duality.Resources;
using Duality.Drawing;

using Duality.Editor;
using Duality.Editor.Forms;
using Duality.Editor.Plugins.CamView.Properties;
using Duality.Editor.Plugins.CamView.UndoRedoActions;
using Duality.DebugDraw;
using OpenTK;

namespace Duality.Editor.Plugins.CamView.CamViewStates
{
	public abstract class ObjectEditorCamViewState : CamViewState
	{
		private   bool                 actionAllowed       = true;
		private   bool                 actionIsClone       = false;
		private   ObjectEditorAction   action              = ObjectEditorAction.None;
		private   bool                 selectionStatsValid = false;
		private   Vector3              selectionCenter     = Vector3.Zero;
		private   double			   selectionRadius     = 0.0;
		private   ObjectEditorAction   mouseoverAction     = ObjectEditorAction.None;
		private   ObjectEditorSelObj   mouseoverObject     = null;
		private   ObjectEditorAction   drawSelGizmoState   = ObjectEditorAction.None;
		protected List<ObjectEditorSelObj> actionObjSel    = new List<ObjectEditorSelObj>();
		protected List<ObjectEditorSelObj> allObjSel       = new List<ObjectEditorSelObj>();
		protected List<ObjectEditorSelObj> indirectObjSel  = new List<ObjectEditorSelObj>();
		private THREE.Controls.TransformControls controls;


		public ObjectEditorAction ObjAction
		{
			get { return this.action; }
		}
		public IEnumerable<ObjectEditorSelObj> SelectedObjects
		{
			get { return this.allObjSel; }
		}
		public bool MouseActionAllowed
		{
			get { return this.actionAllowed; }
			protected set
			{
				this.actionAllowed = value;
				if (!this.actionAllowed)
				{
					this.mouseoverAction = ObjectEditorAction.None;
					this.mouseoverObject = null;
					if (this.action != ObjectEditorAction.None)
					{
						this.EndAction();
						this.UpdateAction();
					}
				}
			}
		}
		private ObjectEditorAction VisibleObjAction
		{
			get
			{
				return 
					(this.drawSelGizmoState != ObjectEditorAction.None ? this.drawSelGizmoState : 
					(this.action != ObjectEditorAction.None ? this.action :
					this.mouseoverAction));
			}
		}

		protected override bool HasCameraFocusPosition
		{
			get { return this.allObjSel.Any(); }
		}
		protected override Vector3 CameraFocusPosition
		{
			get { return this.selectionCenter; }
		}
		protected override bool IsActionInProgress
		{
			get { return base.IsActionInProgress || this.action != ObjectEditorAction.None; }
		}
		

		public virtual ObjectEditorSelObj PickSelObjAt(int x, int y)
		{
			return null;
		}
		public virtual void SelectObjects(IEnumerable<ObjectEditorSelObj> selObjEnum, SelectMode mode = SelectMode.Set) {}
		public virtual void ClearSelection() {}
		protected virtual void PostPerformAction(IEnumerable<ObjectEditorSelObj> selObjEnum, ObjectEditorAction action) {}

		public virtual void DeleteObjects(IEnumerable<ObjectEditorSelObj> objEnum) {}
		public virtual List<ObjectEditorSelObj> CloneObjects(IEnumerable<ObjectEditorSelObj> objEnum) { return new List<ObjectEditorSelObj>(); }

		protected void DrawSelectionMarkers(IEnumerable<ObjectEditorSelObj> obj)
		{
			Vector3 forward = Vector3.Forward;
			Vector3 right = Vector3.Right;
			Vector3 down = Vector3.Up;
		
			foreach (ObjectEditorSelObj selObj in obj)
			{
				if (!selObj.HasTransform) continue;
				Vector3 posTemp = selObj.Pos;
				BoundingBox radTemp = selObj.BoundRadius;
		
				// Draw selection marker
				if (selObj.ShowPos)
				{
					Gizmos.DrawLine(
						new Vector3((posTemp - right * 0.5f).X, (posTemp - right * 0.5f).Y, posTemp.Z),
						new Vector3((posTemp + right * 0.5f).X, (posTemp + right * 0.5f).Y, posTemp.Z), 
						ColorRgba.White);
					Gizmos.DrawLine(
						new Vector3((posTemp - down * 0.5f).X, (posTemp - down * 0.5f).Y, posTemp.Z),
						new Vector3((posTemp + down * 0.5f).X, (posTemp + down * 0.5f).Y, posTemp.Z), 
						ColorRgba.White);
					Gizmos.DrawLine(
						new Vector3(posTemp.X, posTemp.Y, (posTemp - forward * 0.5f).Z),
						new Vector3(posTemp.X, posTemp.Y, (posTemp + forward * 0.5f).Z), 
						ColorRgba.White);
				}
		
				// Draw angle marker
				if (selObj.ShowAngle)
				{
					//posTemp = selObj.Pos + radTemp * right * MathF.Sin(selObj.Angle - camAngle) - radTemp * down * MathF.Cos(selObj.Angle - camAngle);
					//canvas.DrawLine(selObj.Pos.X, selObj.Pos.Y, selObj.Pos.Z, posTemp.X, posTemp.Y, posTemp.Z);
				}

				// Draw boundary
				if (selObj.ShowBoundRadius && radTemp.Size.Length > 0.0f)
				{
					var pos = radTemp.Center;
					pos = pos.Length == 0 ? posTemp : pos;
					var size = radTemp.Size;
					//Gizmos.DrawBoundingBox(new Vector3(pos.X, pos.Y, pos.Z), new Vector3(size.X, size.Y, size.Z), selObj.Angle, ColorRgba.White);
					Gizmos.DrawBoundingBox(new Vector3(pos.X, pos.Y, pos.Z), new Vector3(size.X, size.Y, size.Z), Vector3.Zero, ColorRgba.White);
				}
			}
		}
		
		protected void BeginAction(ObjectEditorAction action)
		{
			if (action == ObjectEditorAction.None) return;
			Point mouseLoc = this.PointToClient(Cursor.Position);
			
			this.ValidateSelectionStats();
			
			this.StopCameraMovement();
			
			this.action = action;
			
			// Update snap-to-grid settings for this action
			if (this.actionObjSel.Count > 1)
			{
				// When moving multiple objects, snap only relative to the original selection center, so individual grid alignment is retained
				this.EditingUserGuide.SnapPosOrigin = this.selectionCenter;
				this.EditingUserGuide.SnapScaleOrigin = Vector3.One;
			}
			else
			{
				this.EditingUserGuide.SnapPosOrigin = Vector3.Zero;
				this.EditingUserGuide.SnapScaleOrigin = Vector3.One;
			}
			
			if (Sandbox.State == SandboxState.Playing)
				Sandbox.Freeze();
			
			this.OnBeginAction(this.action);
		}

		protected void EndAction()
		{
			if (this.action == ObjectEditorAction.None) return;
			Point mouseLoc = this.PointToClient(Cursor.Position);
			
			if (Sandbox.State == SandboxState.Playing)
				Sandbox.UnFreeze();
			
			this.OnEndAction(this.action);
			this.action = ObjectEditorAction.None;
			
			if (this.actionIsClone)
			{
				this.actionIsClone = false;
				UndoRedoManager.EndMacro(UndoRedoManager.MacroDeriveName.FromFirst);
			}
			UndoRedoManager.Finish();
		}

		protected void UpdateAction()
		{
			Point mouseLoc = this.PointToClient(Cursor.Position);
			
			if (this.action == ObjectEditorAction.Transform)
				this.UpdateObjMove(mouseLoc);
			else
				this.UpdateMouseover(mouseLoc);
			
			if (this.action != ObjectEditorAction.None)
				this.InvalidateSelectionStats();
		}

		protected void InvalidateSelectionStats()
		{
			this.selectionStatsValid = false;
		}
		private void ValidateSelectionStats()
		{
			if (this.selectionStatsValid) return;

			List<ObjectEditorSelObj> transformObjSel = this.allObjSel.Where(s => s.HasTransform).ToList();

			this.selectionCenter = Vector3.Zero;
			this.selectionRadius = 0.0f;

			foreach (ObjectEditorSelObj s in transformObjSel)
				this.selectionCenter += s.Pos;
			if (transformObjSel.Count > 0) this.selectionCenter /= transformObjSel.Count;

			foreach (ObjectEditorSelObj s in transformObjSel)
			{
				var size = s.BoundRadius.Size;
				this.selectionRadius = MathF.Max(this.selectionRadius, (size + (s.Pos - this.selectionCenter)).Length);
			}

			this.selectionStatsValid = true;
		}

		protected void UpdateMouseover(Point mouseLoc)
		{
			ObjectEditorSelObj lastMouseoverObject = this.mouseoverObject;
			ObjectEditorAction lastMouseoverAction = this.mouseoverAction;
			
			if (this.actionAllowed && !this.CamActionRequiresCursor && this.CamAction == CameraAction.None)
			{
				this.ValidateSelectionStats();
			
				// Determine object at mouse position
				this.mouseoverObject = this.PickSelObjAt(mouseLoc.X, mouseLoc.Y);
			}
			else
			{
				this.mouseoverObject = null;
				this.mouseoverAction = ObjectEditorAction.None;
			}
			
			// If mouseover changed..
			if (this.mouseoverObject != lastMouseoverObject ||
				this.mouseoverAction != lastMouseoverAction)
			{
				// Adjust mouse cursor based on proposed action
				if (this.mouseoverAction == ObjectEditorAction.Transform)
					this.Cursor = CursorHelper.ArrowActionMove;
				else
					this.Cursor = CursorHelper.Arrow;
			}
			
			// Redraw if action gizmos might be visible
			if (this.actionAllowed)
				this.Invalidate();
		}

		private void UpdateObjMove(Point mouseLoc)
		{
			if (CameraComponent != null)
			{
				if (controls == null || controls.camera.Id != CameraComponent.GetTHREECamera().Id)
				{
					controls = new THREE.Controls.TransformControls((View.RenderableControl as GLControl), CameraComponent.GetTHREECamera());
					controls.PropertyChanged += Control_PropertyChanged;
				}
			}
		}

		private void Control_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName.Equals("dragging"))
			{
				//orbit.Enabled = !(sender as THREE.Controls.TransformControls).dragging;
			}
		}

		protected virtual void OnBeginAction(ObjectEditorAction action) {}
		protected virtual void OnEndAction(ObjectEditorAction action) {}

		protected internal override void OnEnterState()
		{
			base.OnEnterState();
		}
		protected internal override void OnLeaveState()
		{
			base.OnLeaveState();
		}

		protected override void DrawSelection()
		{
			// Assure we know how to display the current selection
			this.ValidateSelectionStats();

			List<ObjectEditorSelObj> transformObjSel = this.allObjSel.Where(s => s.HasTransform).ToList();
			Point cursorPos = this.PointToClient(Cursor.Position);
			
			// Draw indirectly selected object overlay
			this.DrawSelectionMarkers(this.indirectObjSel);
			if (this.mouseoverObject != null && !transformObjSel.Contains(this.mouseoverObject)) 
				this.DrawSelectionMarkers(new [] { this.mouseoverObject });
			
			// Draw selected object overlay
			this.DrawSelectionMarkers(transformObjSel);
			
			// Draw overall selection boundary
			if (transformObjSel.Count > 1)
			{
				Gizmos.DrawBoundingBox(this.selectionCenter, (Vector3.One * this.selectionRadius) * 2f, Vector3.Zero, ColorRgba.White);
			}
		}

		protected override void OnUpdateState()
		{
			this.ValidateSelectionStats();
			base.OnUpdateState();
			if (Sandbox.State == SandboxState.Playing)
			{
				this.InvalidateSelectionStats();
			}
		}
		protected override void OnSceneChanged()
		{
			base.OnSceneChanged();
			if (this.mouseoverObject != null && this.mouseoverObject.IsInvalid)
				this.mouseoverObject = null;
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			this.UpdateAction();
		}
		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			this.drawSelGizmoState = ObjectEditorAction.None;

			if (e.Button == MouseButtons.Left)
				this.EndAction();
		}
		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			bool alt = (Control.ModifierKeys & Keys.Alt) != Keys.None;

			this.drawSelGizmoState = ObjectEditorAction.None;

			if (this.action == ObjectEditorAction.None)
			{
				if (e.Button == MouseButtons.Left)
				{
					// To interact with an object that isn't selected yet: Select it.
					if (!this.allObjSel.Contains(this.mouseoverObject))
						this.SelectObjects(new[] { this.mouseoverObject });
					if (alt)
					{
						UndoRedoManager.BeginMacro();
						this.actionIsClone = true;
						this.SelectObjects(this.CloneObjects(this.actionObjSel));
					}
					this.BeginAction(this.mouseoverAction);
				}
			}
		}
		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);
			this.drawSelGizmoState = ObjectEditorAction.None;
		}
		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);

			this.mouseoverAction = ObjectEditorAction.None;
			this.mouseoverObject = null;
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			this.drawSelGizmoState = ObjectEditorAction.None;

			if (this.actionAllowed)
			{
				if (e.KeyCode == Keys.Menu)
				{
					// Capture the left Alt key, so focus doesn't jump to the menu.
					// We'll need Alt keys right here for those drag-clone actions.
					e.Handled = true;
				}
				else if (e.KeyCode == Keys.Delete)
				{
					List<ObjectEditorSelObj> deleteList = this.actionObjSel.ToList();
					this.ClearSelection();
					this.DeleteObjects(deleteList);
				}
				else if ((e.KeyCode == Keys.C || e.KeyCode == Keys.D) && e.Control)
				{
					List<ObjectEditorSelObj> cloneList = this.CloneObjects(this.actionObjSel);
					this.SelectObjects(cloneList);
				}
				else if (e.KeyCode == Keys.ShiftKey)				this.UpdateAction();
				else if (e.KeyCode == Keys.ControlKey)				this.UpdateAction();
			}
		}

		protected override void OnKeyUp(KeyEventArgs e)
		{
			if (e.KeyCode == Keys.ShiftKey)
			{
				this.UpdateAction();
			}
			else if (e.KeyCode == Keys.ControlKey)
			{
				this.UpdateAction();
			}

			base.OnKeyUp(e);
		}

		protected override void OnCamActionRequiresCursorChanged(EventArgs e)
		{
			base.OnCamActionRequiresCursorChanged(e);
			this.UpdateAction();
		}

		protected override void OnGotFocus()
		{
			base.OnGotFocus();

			// Re-apply the current selection to trigger a global event focusing on our local selection again.
			if (DualityApp.ExecContext != DualityApp.ExecutionContext.Terminated)
				this.SelectObjects(this.allObjSel);
		}

		protected override void OnLostFocus()
		{
			base.OnLostFocus();
			this.EndAction();
		}

		public override HelpInfo ProvideHoverHelp(Point localPos, ref bool captured)
		{
			if (this.actionAllowed && this.allObjSel.Any())
			{
				return HelpInfo.FromText(CamViewRes.CamView_Help_ObjActions, 
					CamViewRes.CamView_Help_ObjActions_Delete + "\n" +
					CamViewRes.CamView_Help_ObjActions_Clone + "\n" +
					CamViewRes.CamView_Help_ObjActions_EditClone + "\n" +
					CamViewRes.CamView_Help_ObjActions_MoveStep + "\n" +
					CamViewRes.CamView_Help_ObjActions_Focus + "\n" +
					CamViewRes.CamView_Help_ObjActions_AxisLock);
			}
			else
			{
				return base.ProvideHoverHelp(localPos, ref captured);
			}
		}
	}
}
