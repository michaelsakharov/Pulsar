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

namespace Duality.Editor.Plugins.CamView.CamViewStates
{
	public abstract class ObjectEditorCamViewState : CamViewState
	{
		private   bool                 actionAllowed       = true;
		private   bool                 actionIsClone       = false;
		private   ObjectEditorAxisLock actionLockedAxis    = ObjectEditorAxisLock.None;
		private   ObjectEditorAction   action              = ObjectEditorAction.None;
		private   bool                 selectionStatsValid = false;
		private   Vector3              selectionCenter     = Vector3.Zero;
		private   float                selectionRadius     = 0.0f;
		private   ObjectEditorAction   mouseoverAction     = ObjectEditorAction.None;
		private   ObjectEditorSelObj   mouseoverObject     = null;
		private   bool                 mouseoverSelect     = false;
		private   ObjectEditorAction   drawSelGizmoState   = ObjectEditorAction.None;
		protected List<ObjectEditorSelObj> actionObjSel    = new List<ObjectEditorSelObj>();
		protected List<ObjectEditorSelObj> allObjSel       = new List<ObjectEditorSelObj>();
		protected List<ObjectEditorSelObj> indirectObjSel  = new List<ObjectEditorSelObj>();


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
					this.mouseoverSelect = false;
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
		public void MoveSelectionBy(Vector3 move)
		{
			if (move == Vector3.Zero) return;

			UndoRedoManager.Do(new MoveCamViewObjAction(
				this.actionObjSel, 
				obj => this.PostPerformAction(obj, ObjectEditorAction.Move), 
				move));

			this.drawSelGizmoState = ObjectEditorAction.Move;
			this.InvalidateSelectionStats();
			this.Invalidate();
		}
		public void MoveSelectionTo(Vector3 target)
		{
			this.MoveSelectionBy(target - this.selectionCenter);
		}
		public void MoveSelectionToCursor()
		{
			Point mousePos = this.PointToClient(Cursor.Position);

			THREE.Math.Vector3 cameraPosition = new THREE.Math.Vector3().SetFromMatrixPosition(CameraComponent.GetTHREECamera().MatrixWorld);
			THREE.Math.Vector3 direction = new THREE.Math.Vector3().Set(mousePos.X, mousePos.Y, 0.5f).UnProject(CameraComponent.GetTHREECamera()).Sub(cameraPosition).Normalize();

			Vector3 mouseSpaceCoord = cameraPosition + (direction * 10f);
			
			// Apply user guide snapping
			if ((this.SnapToUserGuides & UserGuideType.Position) != UserGuideType.None)
			{
				mouseSpaceCoord = this.EditingUserGuide.SnapPosition(mouseSpaceCoord);
			}
			
			this.MoveSelectionTo(mouseSpaceCoord);
		}
		public void RotateSelectionBy(float rotation)
		{
			if (rotation == 0.0f) return;
			
			UndoRedoManager.Do(new RotateCamViewObjAction(
				this.actionObjSel, 
				obj => this.PostPerformAction(obj, ObjectEditorAction.Rotate), 
				rotation));

			this.drawSelGizmoState = ObjectEditorAction.Rotate;
			this.InvalidateSelectionStats();
			this.Invalidate();
		}
		public void ScaleSelectionBy(float scale)
		{
			if (scale == 1.0f) return;

			UndoRedoManager.Do(new ScaleCamViewObjAction(
				this.actionObjSel, 
				obj => this.PostPerformAction(obj, ObjectEditorAction.Scale), 
				scale));

			this.drawSelGizmoState = ObjectEditorAction.Scale;
			this.InvalidateSelectionStats();
			this.Invalidate();
		}

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

		protected void DrawLockedAxes(float x, float y, float z, float r)
		{
			if (this.actionLockedAxis == ObjectEditorAxisLock.X)
			{
				Gizmos.DrawLine(new Vector3(x - r, y, z), new Vector3(x + r, y, z), ColorRgba.Red);
			}
			if (this.actionLockedAxis == ObjectEditorAxisLock.Y)
			{
				Gizmos.DrawLine(new Vector3(x, y - r, z), new Vector3(x, y + r, z), ColorRgba.Green);
			}
			if (this.actionLockedAxis == ObjectEditorAxisLock.Z)
			{
				Gizmos.DrawLine(new Vector3(x, y, z), new Vector3(x, y, z + r), ColorRgba.Blue);
			}
		}

		/// <summary>
		/// Returns an axis-locked version of the specified vector, if requested by the user. Doesn't
		/// do anything when no axis lock is in currently active.
		/// </summary>
		/// <param name="baseVec">The base vector without any locking in place.</param>
		/// <param name="lockedVec">A reference vector that represents the base vector being locked to all axes at once.</param>
		/// <param name="beginToTarget">The movement vector to evaluate in order to determine the axes to which the base vector will be locked.</param>
		protected Vector3 ApplyAxisLock(Vector3 baseVec, Vector3 lockedVec, Vector3 beginToTarget)
		{
			bool shift = (Control.ModifierKeys & Keys.Shift) != Keys.None;
			if (!shift)
			{
				this.actionLockedAxis = ObjectEditorAxisLock.None;
				return baseVec;
			}
			else
			{
				float xWeight = MathF.Abs(Vector3.Dot(beginToTarget.Normalized, Vector3.UnitX));
				float yWeight = MathF.Abs(Vector3.Dot(beginToTarget.Normalized, Vector3.UnitY));
				float zWeight = MathF.Abs(Vector3.Dot(beginToTarget.Normalized, Vector3.UnitZ));
				
				if (xWeight >= yWeight && xWeight >= zWeight)
				{
					this.actionLockedAxis = ObjectEditorAxisLock.X;
					return new Vector3(baseVec.X, lockedVec.Y, lockedVec.Z);
				}
				else if (yWeight >= xWeight && yWeight >= zWeight)
				{
					this.actionLockedAxis = ObjectEditorAxisLock.Y;
					return new Vector3(lockedVec.X, baseVec.Y, lockedVec.Z);
				}
				else if (zWeight >= yWeight && zWeight >= xWeight)
				{
					this.actionLockedAxis = ObjectEditorAxisLock.Z;
					return new Vector3(lockedVec.X, lockedVec.Y, baseVec.Z);
				}
				return lockedVec;
			}
		}
		protected Vector2 ApplyAxisLock(Vector2 baseVec, Vector2 lockedVec, Vector2 beginToTarget)
		{
			return this.ApplyAxisLock(new Vector3(baseVec), new Vector3(lockedVec), new Vector3(beginToTarget)).Xy;
		}
		protected Vector3 ApplyAxisLock(Vector3 targetVec, Vector3 lockedVec)
		{
			return targetVec + this.ApplyAxisLock(Vector3.Zero, lockedVec - targetVec, lockedVec - targetVec);
		}
		protected Vector2 ApplyAxisLock(Vector2 targetVec, Vector2 lockedVec)
		{
			return targetVec + this.ApplyAxisLock(Vector2.Zero, lockedVec - targetVec, lockedVec - targetVec);
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
			
			if (this.action == ObjectEditorAction.Move)
				this.UpdateObjMove(mouseLoc);
			else if (this.action == ObjectEditorAction.Rotate)
				this.UpdateObjRotate(mouseLoc);
			else if (this.action == ObjectEditorAction.Scale)
				this.UpdateObjScale(mouseLoc);
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
			bool lastMouseoverSelect = this.mouseoverSelect;
			ObjectEditorSelObj lastMouseoverObject = this.mouseoverObject;
			ObjectEditorAction lastMouseoverAction = this.mouseoverAction;
			
			if (this.actionAllowed && !this.CamActionRequiresCursor && this.CamAction == CameraAction.None)
			{
				this.ValidateSelectionStats();
			
				// Determine object at mouse position
				this.mouseoverObject = this.PickSelObjAt(mouseLoc.X, mouseLoc.Y);
			
				// Determine action variables
				//Vector3 mouseSpaceCoord = this.GetWorldPos(new Vector3(mouseLoc.X, mouseLoc.Y, this.selectionCenter.Z));
				//float scale = this.GetScaleAtZ(this.selectionCenter.Z);
				float scale = 1f;
				const float boundaryThickness = 10.0f;
				bool tooSmall = this.selectionRadius * scale <= boundaryThickness * 2.0f;
				//bool mouseOverBoundary = MathF.Abs((mouseSpaceCoord - this.selectionCenter).Length - this.selectionRadius) * scale < boundaryThickness;
				//bool mouseInsideBoundary = !mouseOverBoundary && (mouseSpaceCoord - this.selectionCenter).Length < this.selectionRadius;
				//bool mouseAtCenterAxis = MathF.Abs(mouseSpaceCoord.X - this.selectionCenter.X) * scale < boundaryThickness || MathF.Abs(mouseSpaceCoord.Y - this.selectionCenter.Y) * scale < boundaryThickness;
				bool mouseOverBoundary = false;
				bool mouseInsideBoundary = false;
				bool mouseAtCenterAxis = false;
				bool shift = (Control.ModifierKeys & Keys.Shift) != Keys.None;
				bool ctrl = (Control.ModifierKeys & Keys.Control) != Keys.None;
				
				bool anySelection = this.actionObjSel.Count > 0;
				bool canMove = this.actionObjSel.Any(s => s.IsActionAvailable(ObjectEditorAction.Move));
				bool canRotate = (canMove && this.actionObjSel.Count > 1) || this.actionObjSel.Any(s => s.IsActionAvailable(ObjectEditorAction.Rotate));
				bool canScale = (canMove && this.actionObjSel.Count > 1) || this.actionObjSel.Any(s => s.IsActionAvailable(ObjectEditorAction.Scale));
				
				// Select which action to propose
				this.mouseoverSelect = false;
				if (anySelection && !tooSmall && mouseOverBoundary && mouseAtCenterAxis && this.selectionRadius > 0.0f && canScale)
					this.mouseoverAction = ObjectEditorAction.Scale;
				else if (anySelection && !tooSmall && mouseOverBoundary && canRotate)
					this.mouseoverAction = ObjectEditorAction.Rotate;
				else if (anySelection && mouseInsideBoundary && canMove)
					this.mouseoverAction = ObjectEditorAction.Move;
				else if (this.mouseoverObject != null && this.mouseoverObject.IsActionAvailable(ObjectEditorAction.Move))
				{
					this.mouseoverAction = ObjectEditorAction.Move; 
					this.mouseoverSelect = true;
				}
				else
					this.mouseoverAction = ObjectEditorAction.None;
			}
			else
			{
				this.mouseoverObject = null;
				this.mouseoverSelect = false;
				this.mouseoverAction = ObjectEditorAction.None;
			}
			
			// If mouseover changed..
			if (this.mouseoverObject != lastMouseoverObject || 
				this.mouseoverSelect != lastMouseoverSelect ||
				this.mouseoverAction != lastMouseoverAction)
			{
				// Adjust mouse cursor based on proposed action
				if (this.mouseoverAction == ObjectEditorAction.Move)
					this.Cursor = CursorHelper.ArrowActionMove;
				else if (this.mouseoverAction == ObjectEditorAction.Rotate)
					this.Cursor = CursorHelper.ArrowActionRotate;
				else if (this.mouseoverAction == ObjectEditorAction.Scale)
					this.Cursor = CursorHelper.ArrowActionScale;
				else
					this.Cursor = CursorHelper.Arrow;
			}
			
			// Redraw if action gizmos might be visible
			if (this.actionAllowed)
				this.Invalidate();
		}

		private void UpdateObjMove(Point mouseLoc)
		{
			//this.ValidateSelectionStats();
			//
			//// Determine where to move the object
			//float zMovement = this.CameraObj.Transform.Pos.Z - this.actionLastLocSpace.Z;
			//Vector3 mousePosSpace = this.GetWorldPos(new Vector3(mouseLoc.X, mouseLoc.Y, this.selectionCenter.Z + zMovement)); mousePosSpace.Z = 0;
			//Vector3 resetMovement = this.actionBeginLocSpace - this.actionLastLocSpace;
			//Vector3 targetMovement = mousePosSpace - this.actionLastLocSpace; targetMovement.Z = zMovement;
			//
			//// Apply user guide snapping
			//if ((this.SnapToUserGuides & UserGuideType.Position) != UserGuideType.None)
			//{
			//	Vector3 snappedCenter = this.selectionCenter;
			//	Vector3 targetPosSpace = snappedCenter + targetMovement;
			//
			//	targetPosSpace = this.EditingUserGuide.SnapPosition(targetPosSpace);
			//	targetMovement = targetPosSpace - snappedCenter;
			//}
			//
			//// Apply user axis locks
			//targetMovement = this.ApplyAxisLock(targetMovement, resetMovement, mousePosSpace - this.actionBeginLocSpace + new Vector3(0.0f, 0.0f, this.CameraObj.Transform.Pos.Z));
			//
			//// Move the selected objects accordingly
			//this.MoveSelectionBy(targetMovement);
			//
			//this.actionLastLocSpace += targetMovement;
		}
		private void UpdateObjRotate(Point mouseLoc)
		{
			//this.ValidateSelectionStats();
			//
			//Vector3 spaceCoord = this.GetWorldPos(new Vector3(mouseLoc.X, mouseLoc.Y, this.selectionCenter.Z));
			//float lastAngle = MathF.Angle(this.selectionCenter.X, this.selectionCenter.Y, this.actionLastLocSpace.X, this.actionLastLocSpace.Y);
			//float curAngle = MathF.Angle(this.selectionCenter.X, this.selectionCenter.Y, spaceCoord.X, spaceCoord.Y);
			//float rotation = curAngle - lastAngle;
			//
			//this.RotateSelectionBy(rotation);
			//
			//this.actionLastLocSpace = spaceCoord;
		}
		private void UpdateObjScale(Point mouseLoc)
		{
			//this.ValidateSelectionStats();
			//if (this.selectionRadius == 0.0f) return;
			//
			//Vector3 spaceCoord = this.GetWorldPos(new Vector3(mouseLoc.X, mouseLoc.Y, this.selectionCenter.Z));
			//float lastRadius = this.selectionRadius;
			//float curRadius = (this.selectionCenter - spaceCoord).Length;
			//
			//if ((this.SnapToUserGuides & UserGuideType.Scale) != UserGuideType.None)
			//{
			//	curRadius = this.EditingUserGuide.SnapSize(curRadius);
			//}
			//
			//float scale = MathF.Clamp(curRadius / lastRadius, 0.0001f, 10000.0f);
			//this.ScaleSelectionBy(scale);
			//
			//this.actionLastLocSpace = spaceCoord;
			//this.Invalidate();
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
			if (this.mouseoverObject != null && this.mouseoverSelect && !transformObjSel.Contains(this.mouseoverObject)) 
				this.DrawSelectionMarkers(new [] { this.mouseoverObject });
			
			// Draw selected object overlay
			this.DrawSelectionMarkers(transformObjSel);
			
			// Draw overall selection boundary
			if (transformObjSel.Count > 1)
			{
				Gizmos.DrawBoundingBox(this.selectionCenter, (Vector3.One * this.selectionRadius) * 2f, Vector3.Zero, ColorRgba.White);
			}
			
			if (this.action != ObjectEditorAction.None)
			{
				// Draw action lock axes
				this.DrawLockedAxes(this.selectionCenter.X, this.selectionCenter.Y, this.selectionCenter.Z, this.selectionRadius * 4);
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
					if (this.mouseoverSelect)
					{
						// To interact with an object that isn't selected yet: Select it.
						if (!this.allObjSel.Contains(this.mouseoverObject))
							this.SelectObjects(new [] { this.mouseoverObject });
					}
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
			this.mouseoverSelect = false;
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
				else if (e.KeyCode == Keys.G)
				{
					if (e.Alt)
					{
						this.SelectObjects(this.CloneObjects(this.actionObjSel));
						e.SuppressKeyPress = true; // Prevent menustrip from getting focused
					}
					this.MoveSelectionToCursor();
				}
				else if (!e.Control && e.KeyCode == Keys.Left)		this.MoveSelectionBy(-Vector3.UnitX);
				else if (!e.Control && e.KeyCode == Keys.Right)		this.MoveSelectionBy(Vector3.UnitX);
				else if (!e.Control && e.KeyCode == Keys.Up)		this.MoveSelectionBy(-Vector3.UnitY);
				else if (!e.Control && e.KeyCode == Keys.Down)		this.MoveSelectionBy(Vector3.UnitY);
				else if (!e.Control && e.KeyCode == Keys.Add)		this.MoveSelectionBy(Vector3.UnitZ);
				else if (!e.Control && e.KeyCode == Keys.Subtract)	this.MoveSelectionBy(-Vector3.UnitZ);
				else if (e.KeyCode == Keys.ShiftKey)				this.UpdateAction();
				else if (e.KeyCode == Keys.ControlKey)				this.UpdateAction();
			}
		}
		protected override void OnKeyUp(KeyEventArgs e)
		{
			if (e.KeyCode == Keys.ShiftKey)
			{
				this.actionLockedAxis = ObjectEditorAxisLock.None;
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
