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
using Duality.Editor.Plugins.CamView.CamViewLayers;
using Duality.Editor.Plugins.CamView.Properties;
using Duality.Editor.Plugins.CamView.UndoRedoActions;
using OpenTK;

namespace Duality.Editor.Plugins.CamView.CamViewStates
{
	public abstract class CamViewState : CamViewClient, IHelpProvider
	{
		[Flags]
		public enum UserGuideType
		{
			None		= 0x0,

			Position	= 0x1,
			Scale		= 0x2,

			All			= Position | Scale
		}
		public enum CameraAction
		{
			None,
			Move,
			MoveWASD,

			// Alternate movement (Spacebar pressed)
			DragScene,
		}

		private Vector3       camVel                 = Vector3.Zero;
		private Point         camActionBeginLoc      = Point.Empty;
		private Vector3       camActionBeginLocSpace = Vector3.Zero;
		private CameraAction  camAction              = CameraAction.None;
		private bool          camActionAllowed       = true;
		private bool          camTransformChanged    = false;
		private bool          camBeginDragScene      = false;
		private bool          engineUserInput        = false;
		private UserGuideType snapToUserGuides       = UserGuideType.All;
		private bool          mouseover              = false;
		private List<Type>    lastActiveLayers       = new List<Type>();
		private List<string>  lastObjVisibility      = new List<string>();
		private TimeSpan      renderedGameTime       = TimeSpan.MinValue;
		private int           renderFrameLast        = -1;
		private bool          renderFrameScheduled   = false;
		private bool	      KeyWIsDown			 = false;
		private bool	      KeyAIsDown			 = false;
		private bool	      KeySIsDown			 = false;
		private bool	      KeyDIsDown			 = false;
		private bool	      KeyEIsDown			 = false;
		private bool	      KeyQIsDown			 = false;


		public abstract string StateName { get; }

		protected virtual bool IsActionInProgress
		{
			get { return false; }
		}
		protected virtual bool HasCameraFocusPosition
		{
			get { return false; }
		}
		protected virtual Vector3 CameraFocusPosition
		{
			get { return Vector3.Zero; }
		}

		public bool IsActive
		{
			get { return this.View != null && this.View.ActiveState == this; }
		}
		public bool EngineUserInput
		{
			get { return this.engineUserInput; }
			protected set { this.engineUserInput = value; }
		}
		public virtual Rect RenderedViewport
		{
			get { return new Rect(this.RenderedImageSize); }
		}
		public virtual Point2 RenderedImageSize
		{
			get
			{
				return new Point2(
					this.RenderableControl.ClientSize.Width, 
					this.RenderableControl.ClientSize.Height);
			}
		}
		public bool CameraActionAllowed
		{
			get { return this.camActionAllowed; }
			protected set
			{ 
				this.camActionAllowed = value;
				if (!this.camActionAllowed && this.camAction != CameraAction.None)
				{
					this.camAction = CameraAction.None;
					this.Invalidate();
				}
			}
		}
		public bool Mouseover
		{
			get { return this.mouseover; }
		}
		public bool CamActionRequiresCursor
		{
			get { return this.camBeginDragScene; }
		}
		public CameraAction CamAction
		{
			get { return this.camAction; }
		}
		public UserGuideType SnapToUserGuides
		{
			get { return this.snapToUserGuides; }
			protected set { this.snapToUserGuides = value; }
		}


		/// <summary>
		/// Called when the <see cref="CamViewState"/> is entered.
		/// Use this for overall initialization of the state.
		/// </summary>
		internal protected virtual void OnEnterState()
		{
			this.RestoreActiveLayers();
			this.RestoreObjectVisibility();

			Control control = this.RenderableControl;
			control.Paint		+= this.RenderableControl_Paint;
			control.MouseDown	+= this.RenderableControl_MouseDown;
			control.MouseUp		+= this.RenderableControl_MouseUp;
			control.MouseMove	+= this.RenderableControl_MouseMove;
			control.MouseWheel	+= this.RenderableControl_MouseWheel;
			control.MouseLeave	+= this.RenderableControl_MouseLeave;
			control.KeyDown		+= this.RenderableControl_KeyDown;
			control.KeyUp		+= this.RenderableControl_KeyUp;
			control.GotFocus	+= this.RenderableControl_GotFocus;
			control.LostFocus	+= this.RenderableControl_LostFocus;
			control.DragDrop	+= this.RenderableControl_DragDrop;
			control.DragEnter	+= this.RenderableControl_DragEnter;
			control.DragLeave	+= this.RenderableControl_DragLeave;
			control.DragOver	+= this.RenderableControl_DragOver;
			control.Resize		+= this.RenderableControl_Resize;
			this.View.PerspectiveChanged	+= this.View_FocusDistChanged;
			this.View.CurrentCameraChanged	+= this.View_CurrentCameraChanged;
			DualityEditorApp.UpdatingEngine += this.DualityEditorApp_UpdatingEngine;
			DualityEditorApp.ObjectPropertyChanged += this.DualityEditorApp_ObjectPropertyChanged;

			Scene.Leaving += this.Scene_Changed;
			Scene.Entered += this.Scene_Changed;
			Scene.GameObjectParentChanged += this.Scene_Changed;
			Scene.GameObjectsAdded += this.Scene_Changed;
			Scene.GameObjectsRemoved += this.Scene_Changed;
			Scene.ComponentAdded += this.Scene_Changed;
			Scene.ComponentRemoving += this.Scene_Changed;

			if (Scene.Current != null) this.Scene_Changed(this, EventArgs.Empty);
			
			this.OnCurrentCameraChanged(new CamView.CameraChangedEventArgs(null, this.CameraComponent));

			if (this.IsViewVisible)
				this.OnShown();
		}
		/// <summary>
		/// Called when the <see cref="CamViewState"/> is left.
		/// Use this for overall termination of the state.
		/// </summary>
		internal protected virtual void OnLeaveState() 
		{
			if (this.IsViewVisible)
				this.OnHidden();

			this.Cursor = CursorHelper.Arrow;

			Control control = this.RenderableControl;
			control.Paint		-= this.RenderableControl_Paint;
			control.MouseDown	-= this.RenderableControl_MouseDown;
			control.MouseUp		-= this.RenderableControl_MouseUp;
			control.MouseMove	-= this.RenderableControl_MouseMove;
			control.MouseWheel	-= this.RenderableControl_MouseWheel;
			control.MouseLeave	-= this.RenderableControl_MouseLeave;
			control.KeyDown		-= this.RenderableControl_KeyDown;
			control.KeyUp		-= this.RenderableControl_KeyUp;
			control.GotFocus	-= this.RenderableControl_GotFocus;
			control.LostFocus	-= this.RenderableControl_LostFocus;
			control.DragDrop	-= this.RenderableControl_DragDrop;
			control.DragEnter	-= this.RenderableControl_DragEnter;
			control.DragLeave	-= this.RenderableControl_DragLeave;
			control.DragOver	-= this.RenderableControl_DragOver;
			control.Resize		-= this.RenderableControl_Resize;
			this.View.PerspectiveChanged			-= this.View_FocusDistChanged;
			this.View.CurrentCameraChanged			-= this.View_CurrentCameraChanged;
			DualityEditorApp.UpdatingEngine			-= this.DualityEditorApp_UpdatingEngine;
			DualityEditorApp.ObjectPropertyChanged	-= this.DualityEditorApp_ObjectPropertyChanged;
			
			Scene.Leaving -= this.Scene_Changed;
			Scene.Entered -= this.Scene_Changed;
			Scene.GameObjectParentChanged -= this.Scene_Changed;
			Scene.GameObjectsAdded -= this.Scene_Changed;
			Scene.GameObjectsRemoved -= this.Scene_Changed;
			Scene.ComponentAdded -= this.Scene_Changed;
			Scene.ComponentRemoving -= this.Scene_Changed;

			this.SaveActiveLayers();
			this.SaveObjectVisibility();

			// Final Camera cleanup
			this.OnCurrentCameraChanged(new CamView.CameraChangedEventArgs(this.CameraComponent, null));
		}
		/// <summary>
		/// Called when the <see cref="CamViewState"/> becomes visible to the user, e.g.
		/// by being entered, selecting the multi-document tab that contains its parent <see cref="CamView"/>
		/// or similar.
		/// </summary>
		internal protected virtual void OnShown() { }
		/// <summary>
		/// Called when the <see cref="CamViewState"/> becomes hidden from the user, e.g.
		/// by being left, deselecting the multi-document tab that contains its parent <see cref="CamView"/>
		/// or similar.
		/// </summary>
		internal protected virtual void OnHidden() { }
		
		internal protected virtual void SaveUserData(XElement node)
		{
			if (this.IsActive) this.SaveActiveLayers();
			if (this.IsActive) this.SaveObjectVisibility();

			XElement activeLayersNode = new XElement("ActiveLayers");
			foreach (Type t in this.lastActiveLayers)
			{
				XElement typeEntry = new XElement("Item", t.GetTypeId());
				activeLayersNode.Add(typeEntry);
			}
			if (!activeLayersNode.IsEmpty)
				node.Add(activeLayersNode);

			XElement objVisibilityNode = new XElement("ObjectVisibility");
			foreach (string typeId in this.lastObjVisibility)
			{
				XElement typeEntry = new XElement("Item", typeId);
				objVisibilityNode.Add(typeEntry);
			}
			if (!objVisibilityNode.IsEmpty)
				node.Add(objVisibilityNode);
		}
		internal protected virtual void LoadUserData(XElement node)
		{
			XElement activeLayersNode = node.Element("ActiveLayers");
			if (activeLayersNode != null)
			{
				this.lastActiveLayers.Clear();
				foreach (XElement layerNode in activeLayersNode.Elements("Item"))
				{
					Type layerType = ReflectionHelper.ResolveType(layerNode.Value);
					if (layerType != null) this.lastActiveLayers.Add(layerType);
				}
			}
			
			XElement objVisibilityNode = node.Element("ObjectVisibility");
			if (objVisibilityNode != null)
			{
				this.lastObjVisibility.Clear();
				foreach (XElement typeNode in objVisibilityNode.Elements("Item"))
				{
					// Try to resolve the type. If it's successful, keep its type ID.
					// This will also correct renamed types if resolving invokes an error handler.
					Type type = ReflectionHelper.ResolveType(typeNode.Value);
					if (type != null) this.lastObjVisibility.Add(type.GetTypeId());
				}
			}

			if (this.IsActive) this.RestoreActiveLayers();
			if (this.IsActive) this.RestoreObjectVisibility();
		}

		protected virtual void DrawSelection()
		{
		}

		protected virtual void OnRenderState()
		{
			// Render here!
			DualityApp.GraphicsBackend.ShadowMap.Enabled = true;
			DualityApp.GraphicsBackend.ShadowMap.type = THREE.Constants.PCFSoftShadowMap;

			DualityApp.InvokePreRender(Scene.Current, CameraComponent);

			DrawSelection();

			//DualityApp.GraphicsBackend.Render(Scene.ThreeScene, CameraComponent.GetTHREECamera());
			CameraComponent.Render();

			DualityApp.InvokePostRender(Scene.Current, CameraComponent);
		}

		private float _currentYaw = 0.3f;
		private float _currentPitch = 0.3f;

		protected virtual void OnUpdateState()
		{
			Camera cam = this.CameraComponent;
			GameObject camObj = this.CameraObj;
			Point cursorPos = this.PointToClient(Cursor.Position);

			double unscaledTimeMult = Time.TimeMult / Time.TimeScale;

			this.camTransformChanged = false;
			
			if (this.camAction == CameraAction.DragScene)
			{
				Vector2 curPos = new Vector2(cursorPos.X, cursorPos.Y);
				Vector2 lastPos = new Vector2(this.camActionBeginLoc.X, this.camActionBeginLoc.Y);
				this.camActionBeginLoc = new Point((int)curPos.X, (int)curPos.Y);

				double refZ = (this.HasCameraFocusPosition && camObj.Transform.Pos.Z < this.CameraFocusPosition.Z - cam.NearZ) ? this.CameraFocusPosition.Z : 0.0f;
				//if (camObj.Transform.Pos.Z >= refZ - cam.NearZ)
				//	refZ = camObj.Transform.Pos.Z + MathF.Abs(cam.FocusDist);

				Vector2 targetOff = (-(curPos - lastPos) / 1f);
				Vector2 targetVel = targetOff / unscaledTimeMult;
				MathF.TransformCoord(ref targetVel.X, ref targetVel.Y, camObj.Transform.Rotation.Z);
				this.camVel.Z *= MathF.Pow(0.9f, unscaledTimeMult);
				this.camVel += (new Vector3(targetVel, this.camVel.Z) - this.camVel) * unscaledTimeMult;
				this.camTransformChanged = true;
			}
			else if (this.camAction == CameraAction.Move)
			{
				Vector3 moveVec = new Vector3(
					cursorPos.X - this.camActionBeginLoc.X,
					cursorPos.Y - this.camActionBeginLoc.Y,
					this.camVel.Z);

				const float BaseSpeedCursorLen = 25.0f;
				const float BaseSpeed = 3.0f;
				moveVec.X = BaseSpeed * MathF.Sign(moveVec.X) * MathF.Pow(MathF.Abs(moveVec.X) / BaseSpeedCursorLen, 1.5f);
				moveVec.Y = BaseSpeed * MathF.Sign(moveVec.Y) * MathF.Pow(MathF.Abs(moveVec.Y) / BaseSpeedCursorLen, 1.5f);
				moveVec.Z *= MathF.Pow(0.9f, unscaledTimeMult);

				MathF.TransformCoord(ref moveVec.X, ref moveVec.Y, camObj.Transform.Rotation.Z);

				this.camVel = moveVec;
				this.camTransformChanged = true;
			}
			else if (this.camAction == CameraAction.MoveWASD)
			{
				// calculate mouse movement
				Vector2 mouseVec = new Vector2(cursorPos.X - this.camActionBeginLoc.X, cursorPos.Y - this.camActionBeginLoc.Y);
				this.camActionBeginLoc = cursorPos; // Update Begic Mouse Pos

				this._currentYaw += (float)mouseVec.X * 0.01f;
				this._currentPitch += (float)mouseVec.Y * 0.01f;

				//camObj.Transform.Rotation = new Vector3(this._currentYaw, this._currentPitch, 0f);
				camObj.Transform.Rotation = Quaternion.CreateFromYawPitchRoll(this._currentYaw, this._currentPitch, 0f).EulerAngles;

				float forward = 0;
				float right = 0;
				float up = 0;

				if (this.KeyWIsDown) forward += 1;
				if (this.KeySIsDown) forward -= 1;

				if (this.KeyDIsDown) right += 1;
				if (this.KeyAIsDown) right -= 1;

				if (this.KeyEIsDown) up += 1;
				if (this.KeyQIsDown) up -= 1;

				Vector3 movement = (camObj.Transform.Forward * forward) + (camObj.Transform.Right * right) + (camObj.Transform.Up * up);
				//camObj.Transform.Pos += movement;

				this.camVel = movement * 0.1f;
				this.camTransformChanged = true;
			}
			else if (this.camVel.Length > 0.01f)
			{
				this.camVel *= MathF.Pow(0.9f, unscaledTimeMult);
				this.camTransformChanged = true;
			}
			else
			{
				this.camTransformChanged = this.camTransformChanged || (this.camVel != Vector3.Zero);
				this.camVel = Vector3.Zero;
			}
			if (this.camTransformChanged)
			{
				camObj.Transform.MoveByLocal(this.camVel * unscaledTimeMult);
				this.View.OnCamTransformChanged();
				this.Invalidate();
			}

			// If the game simulation has advanced since we last rendered, schedule 
			// the next frame to be rendered. This will be true every frame in sandbox
			// play mode, and once every sandbox single-step.
			if (Time.GameTimer != this.renderedGameTime)
				this.Invalidate();

			// If we previously skipped a repaint event because we already rendered
			// a frame with that number, perform another repaint once we've entered
			// the next frame. This will make sure we won't forget about previous
			// one-shot invalidate calls just because we were already done rendering that
			// frame.
			if (this.renderFrameScheduled && this.renderFrameLast != Time.FrameCount)
				this.Invalidate();
			this.Invalidate();
		}

		protected virtual void OnSceneChanged()
		{
			this.Invalidate();
		}
		protected virtual void OnCurrentCameraChanged(CamView.CameraChangedEventArgs e) {}
		protected virtual void OnGotFocus() {}
		protected virtual void OnLostFocus() {}
		protected virtual void OnResize() {}

		protected virtual void OnDragEnter(DragEventArgs e) {}
		protected virtual void OnDragOver(DragEventArgs e) {}
		protected virtual void OnDragDrop(DragEventArgs e) {}
		protected virtual void OnDragLeave(EventArgs e) {}

		protected virtual void OnKeyDown(KeyEventArgs e) {}
		protected virtual void OnKeyUp(KeyEventArgs e) {}
		protected virtual void OnMouseDown(MouseEventArgs e) {}
		protected virtual void OnMouseUp(MouseEventArgs e) {}
		protected virtual void OnMouseMove(MouseEventArgs e) {}
		protected virtual void OnMouseWheel(MouseEventArgs e) {}
		protected virtual void OnMouseLeave(EventArgs e) {}
		protected virtual void OnCamActionRequiresCursorChanged(EventArgs e) {}

		protected void OnMouseMove()
		{
			Point mousePos = this.PointToClient(Cursor.Position);
			this.OnMouseMove(new MouseEventArgs(Control.MouseButtons, 0, mousePos.X, mousePos.Y, 0));
		}
		

		protected void StopCameraMovement()
		{
			this.camVel = Vector3.Zero;
		}

		protected void SetDefaultActiveLayers(params Type[] activeLayers)
		{
			this.lastActiveLayers = activeLayers.ToList();
		}
		protected void SaveActiveLayers()
		{
			this.lastActiveLayers = this.View.ActiveLayers.Select(l => l.GetType()).ToList();
		}
		protected void RestoreActiveLayers()
		{
			this.View.SetActiveLayers(this.lastActiveLayers);
		}
		protected void SetDefaultObjectVisibility(params Type[] visibleObjectTypes)
		{
			this.lastObjVisibility.Clear();
			foreach (Type type in visibleObjectTypes)
			{
				this.lastObjVisibility.Add(type.GetTypeId());
			}
		}
		protected void SaveObjectVisibility()
		{
			this.lastObjVisibility.Clear();
			foreach (Type type in this.View.ObjectVisibility.MatchingTypes)
			{
				this.lastObjVisibility.Add(type.GetTypeId());
			}
		}
		protected void RestoreObjectVisibility()
		{
			IEnumerable<Type> resolvedTypes = 
				this.lastObjVisibility
				.Select(typeId => ReflectionHelper.ResolveType(typeId))
				.NotNull();
			this.View.SetObjectVisibility(resolvedTypes);
		}
		
		protected void CollectLayerDrawcalls()
		{
			var layers = this.View.ActiveLayers.ToArray();
			layers.StableSort((a, b) => a.Priority - b.Priority);
			foreach (var layer in layers)
			{
				layer.OnCollectDrawcalls();
			}
		}
		protected void CollectLayerWorldOverlayDrawcalls()
		{
			var layers = this.View.ActiveLayers.ToArray();
			layers.StableSort((a, b) => a.Priority - b.Priority);
			foreach (var layer in layers)
			{
				layer.OnCollectWorldOverlayDrawcalls();
			}
		}
		protected void CollectLayerOverlayDrawcalls()
		{
			var layers = this.View.ActiveLayers.ToArray();
			layers.StableSort((a, b) => a.Priority - b.Priority);
			foreach (var layer in layers)
			{
				layer.OnCollectOverlayDrawcalls();
			}
		}
		protected void CollectLayerBackgroundDrawcalls()
		{
			var layers = this.View.ActiveLayers.ToArray();
			layers.StableSort((a, b) => a.Priority - b.Priority);
			foreach (var layer in layers)
			{
				layer.OnCollectBackgroundDrawcalls();
			}
		}

		private void ForceDragDropRenderUpdate()
		{
			// There is no event loop while performing a dragdrop operation, so we'll have to do
			// some minimal updates here in order to ensure smooth rendering.

			// Force immediate buffer swap and continuous repaint
			this.renderFrameLast = 0;
		}
		

		private void RenderableControl_Paint(object sender, PaintEventArgs e)
		{
			if (DualityApp.ExecContext == DualityApp.ExecutionContext.Terminated) return;
			if (DualityApp.GraphicsBackend == null) return;

			// Only allow one rendered frame per simulated frame to avoid spamming repaints
			// based on user input like OnMouseMove or similar. Remember that all buffer swaps
			// and various core updates are only performed when the WinForms app reports to
			// be idle. This certainly doesn't happen if the event queue fills up with repaint
			// events faster than can be processed.
			if (this.renderFrameLast == Time.FrameCount)
			{
				// If we skipped this repaint, schedule one once we're ready for the next
				// per-frame rendering. Otherwise we will lose one-off repaint events if
				// they happen to fall in the same frame slot.
				this.renderFrameScheduled = true;
				return;
			}
			this.renderFrameScheduled = false;
			this.renderFrameLast = Time.FrameCount;
			this.renderedGameTime = Time.GameTimer;

			// Retrieve OpenGL context
			DualityApp.GraphicsBackend.SetGraphicsContext((View.RenderableControl as GLControl).Context, View.RenderableControl.Width, View.RenderableControl.Height);

			// Perform rendering
			try
			{
				this.OnRenderState();
			}
			catch (Exception exception)
			{
				Logs.Editor.WriteError("An error occurred during CamView {1} rendering. Exception: {0}", LogFormat.Exception(exception), this.CameraComponent.ToString());
			}
			
			// Make sure the rendered result ends up on screen
			(this.RenderableControl as GLControl).SwapBuffers();
		}
		private void RenderableControl_MouseMove(object sender, MouseEventArgs e)
		{
			this.mouseover = true;
			if (!this.camBeginDragScene) this.OnMouseMove(e);
		}
		private void RenderableControl_MouseUp(object sender, MouseEventArgs e)
		{

			if (this.camBeginDragScene)
			{
				this.camAction = CameraAction.None;
				this.Cursor = CursorHelper.HandGrab;
			}
			else
			{
				if (this.camAction == CameraAction.Move && e.Button == MouseButtons.Middle)
					this.camAction = CameraAction.None;
				if (this.camAction == CameraAction.MoveWASD && e.Button == MouseButtons.Right)
					this.camAction = CameraAction.None;

				this.OnMouseUp(e);
			}

			this.Invalidate();
		}
		private void RenderableControl_MouseDown(object sender, MouseEventArgs e)
		{
			bool alt = (Control.ModifierKeys & Keys.Alt) != Keys.None;

			if (this.camBeginDragScene)
			{
				this.camActionBeginLoc = e.Location;
				if (e.Button == MouseButtons.Left)
				{
					this.camAction = CameraAction.DragScene;
					this.camActionBeginLocSpace = this.CameraObj.Transform.LocalPos;
					this.Cursor = CursorHelper.HandGrabbing;
				}
				else if (e.Button == MouseButtons.Middle)
				{
					this.camAction = CameraAction.Move;
					this.camActionBeginLocSpace = this.CameraObj.Transform.LocalPos;
				}
			}
			else
			{
				if (this.camActionAllowed && this.camAction == CameraAction.None)
				{
					this.camActionBeginLoc = e.Location;
					if (e.Button == MouseButtons.Middle)
					{
						this.camAction = CameraAction.Move;
						this.camActionBeginLocSpace = this.CameraObj.Transform.LocalPos;
					}
					else if (e.Button == MouseButtons.Right)
					{
						this.camAction = CameraAction.MoveWASD;
						this.camActionBeginLocSpace = this.CameraObj.Transform.LocalPos;
					}
				}

				this.OnMouseDown(e);
			}
		}
		private void RenderableControl_MouseWheel(object sender, MouseEventArgs e)
		{
			if (!this.mouseover) return;

			if (e.Delta != 0)
			{
				if (this.View.Orthographic == false)
				{
					GameObject camObj = this.CameraObj;
					double curVel = this.camVel.Length * MathF.Sign(this.camVel.Z);
					Vector2 curTemp = new Vector2(
						(e.X * 2.0f / this.ClientSize.Width) - 1.0f,
						(e.Y * 2.0f / this.ClientSize.Height) - 1.0f);
					MathF.TransformCoord(ref curTemp.X, ref curTemp.Y, camObj.Transform.LocalRotation.Z);

					if (MathF.Sign(e.Delta) != MathF.Sign(curVel))
						curVel = 0.0f;
					else
						curVel *= 1.5f;
					curVel += 0.015f * e.Delta;
					curVel = MathF.Sign(curVel) * MathF.Min(MathF.Abs(curVel), 500.0f);

					Vector3 movVec = new Vector3(
						MathF.Sign(e.Delta) * MathF.Sign(curTemp.X) * MathF.Pow(curTemp.X, 2.0f), 
						MathF.Sign(e.Delta) * MathF.Sign(curTemp.Y) * MathF.Pow(curTemp.Y, 2.0f), 
						1.0f);
					movVec.Normalize();
					this.camVel = movVec * curVel;
				}
				else
				{
					//this.View.FocusDist += ((float)e.Delta / 1000f) * Math.Max(this.View.FocusDist, 0.1f);
				}
			}

			this.OnMouseWheel(e);
		}
		private void RenderableControl_MouseLeave(object sender, EventArgs e)
		{
			if (!this.camBeginDragScene) this.OnMouseMove();
			this.OnMouseLeave(e);
			this.mouseover = false;

			this.Invalidate();
		}
		private void RenderableControl_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.W) this.KeyWIsDown = true;
			if (e.KeyCode == Keys.A) this.KeyAIsDown = true;
			if (e.KeyCode == Keys.S) this.KeySIsDown = true;
			if (e.KeyCode == Keys.D) this.KeyDIsDown = true;
			if (e.KeyCode == Keys.E) this.KeyEIsDown = true;
			if (e.KeyCode == Keys.Q) this.KeyQIsDown = true;

			if (this.camActionAllowed)
			{
				if (e.KeyCode == Keys.Space && !this.IsActionInProgress && !this.camBeginDragScene)
				{
					this.camBeginDragScene = true;
					this.OnCamActionRequiresCursorChanged(EventArgs.Empty);
					this.Cursor = CursorHelper.HandGrab;
				}
				else if (e.KeyCode == Keys.F)
				{
					if (DualityEditorApp.Selection.MainGameObject != null)
						this.View.FocusOnObject(DualityEditorApp.Selection.MainGameObject);
					else
						this.View.ResetCamera();
				}
				else if (e.Control && e.KeyCode == Keys.Left)
				{
					Vector3 pos = this.CameraObj.Transform.Pos;
					pos.X = MathF.Round(pos.X - 1.0f);
					this.CameraObj.Transform.Pos = pos;
					this.Invalidate();
				}
				else if (e.Control && e.KeyCode == Keys.Right)
				{
					Vector3 pos = this.CameraObj.Transform.Pos;
					pos.X = MathF.Round(pos.X + 1.0f);
					this.CameraObj.Transform.Pos = pos;
					this.Invalidate();
				}
				else if (e.Control && e.KeyCode == Keys.Up)
				{
					Vector3 pos = this.CameraObj.Transform.Pos;
					pos.Y = MathF.Round(pos.Y - 1.0f);
					this.CameraObj.Transform.Pos = pos;
					this.Invalidate();
				}
				else if (e.Control && e.KeyCode == Keys.Down)
				{
					Vector3 pos = this.CameraObj.Transform.Pos;
					pos.Y = MathF.Round(pos.Y + 1.0f);
					this.CameraObj.Transform.Pos = pos;
					this.Invalidate();
				}
				else if (e.Control && e.KeyCode == Keys.Add)
				{
					Vector3 pos = this.CameraObj.Transform.Pos;
					pos.Z = MathF.Round(pos.Z + 1.0f);
					this.CameraObj.Transform.Pos = pos;
					this.Invalidate();
				}
				else if (e.Control && e.KeyCode == Keys.Subtract)
				{
					Vector3 pos = this.CameraObj.Transform.Pos;
					pos.Z = MathF.Round(pos.Z - 1.0f);
					this.CameraObj.Transform.Pos = pos;
					this.Invalidate();
				}
			}

			this.OnKeyDown(e);
		}
		private void RenderableControl_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.W) this.KeyWIsDown = false;
			if (e.KeyCode == Keys.A) this.KeyAIsDown = false;
			if (e.KeyCode == Keys.S) this.KeySIsDown = false;
			if (e.KeyCode == Keys.D) this.KeyDIsDown = false;
			if (e.KeyCode == Keys.E) this.KeyEIsDown = false;
			if (e.KeyCode == Keys.Q) this.KeyQIsDown = false;

			if (e.KeyCode == Keys.Space && this.camBeginDragScene)
			{
				this.camBeginDragScene = false;
				this.camAction = CameraAction.None;
				this.Cursor = CursorHelper.Arrow;
				this.OnCamActionRequiresCursorChanged(EventArgs.Empty);
			}

			this.OnKeyUp(e);
		}
		private void RenderableControl_GotFocus(object sender, EventArgs e)
		{
			this.MakeDualityTarget();
			this.OnGotFocus();
		}
		private void RenderableControl_LostFocus(object sender, EventArgs e)
		{
			if (DualityEditorApp.MainForm == null) return;

			this.camAction = CameraAction.None;
			this.OnLostFocus();
			this.Invalidate();
		}
		private void RenderableControl_DragOver(object sender, DragEventArgs e)
		{
			this.OnDragOver(e);
			this.ForceDragDropRenderUpdate();
		}
		private void RenderableControl_DragLeave(object sender, EventArgs e)
		{
			this.OnDragLeave(e);
			this.ForceDragDropRenderUpdate();
		}
		private void RenderableControl_DragEnter(object sender, DragEventArgs e)
		{
			this.OnDragEnter(e);
			this.ForceDragDropRenderUpdate();
		}
		private void RenderableControl_DragDrop(object sender, DragEventArgs e)
		{
			this.OnDragDrop(e);
			this.ForceDragDropRenderUpdate();
		}
		private void RenderableControl_Resize(object sender, EventArgs e)
		{
			if (this.ClientSize == Size.Empty) return;

			DualityApp.Resize(ClientSize.Width, ClientSize.Height);
			this.OnResize();
		}
		private void View_FocusDistChanged(object sender, EventArgs e)
		{
			if (!this.camBeginDragScene) this.OnMouseMove();
		}
		private void View_CurrentCameraChanged(object sender, CamView.CameraChangedEventArgs e)
		{
			this.OnCurrentCameraChanged(e);
		}
		private void DualityEditorApp_UpdatingEngine(object sender, EventArgs e)
		{
			this.OnUpdateState();
		}
		private void DualityEditorApp_ObjectPropertyChanged(object sender, ObjectPropertyChangedEventArgs e)
		{
			if (e.HasAnyProperty(
					ReflectionInfo.Property_Transform_LocalPos, 
					ReflectionInfo.Property_Transform_LocalAngle) &&
				e.Objects.Components.Any(c => c.GameObj == this.CameraObj))
			{
				if (!this.camBeginDragScene) this.OnMouseMove();
			}
		}
		private void Scene_Changed(object sender, EventArgs e)
		{
			this.OnSceneChanged();
		}

		public virtual HelpInfo ProvideHoverHelp(Point localPos, ref bool captured)
		{
			if (this.camActionAllowed)
			{
				return HelpInfo.FromText(CamViewRes.CamView_Help_CamActions, 
					CamViewRes.CamView_Help_CamActions_Move + "\n" +
					CamViewRes.CamView_Help_CamActions_MoveAlternate + "\n" +
					CamViewRes.CamView_Help_CamActions_MoveStep + "\n" +
					CamViewRes.CamView_Help_CamActions_Focus);
			}
			else
			{
				return null;
			}
		}
	}
}
