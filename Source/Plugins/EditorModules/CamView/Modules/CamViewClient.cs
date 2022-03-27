using System;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using Duality;
using Duality.Components;
using Duality.Drawing;
using Duality.Resources;

using Duality.Editor;
using Duality.Editor.Backend;
using Duality.Editor.Forms;

namespace Duality.Editor.Plugins.CamView
{
	public abstract class CamViewClient
	{
		private	CamView view = null;
		private	int pickingFrameLast = -1;


		public CamView View
		{
			get { return this.view; }
			internal set { this.view = value; }
		}
		/// <summary>
		/// [GET] Whether the parent <see cref="CamView"/> of this <see cref="CamViewClient"/> is currently
		/// visible to the user, or hidden away (e.g. by being in an inactive multi-document tab or similar).
		/// </summary>
		public bool IsViewVisible
		{
			get { return !this.view.IsHiddenDocument; }
		}
		public Size ClientSize
		{
			get { return this.view.RenderableControl.ClientSize; }
		}
		public Cursor Cursor
		{
			get { return this.view.RenderableControl.Cursor; }
			set { this.view.RenderableControl.Cursor = value; }
		}
		public ColorRgba BgColor
		{
			get { return this.view.BgColor; }
			set { this.view.BgColor = value; }
		}
		public ColorRgba FgColor
		{
			get { return this.view.FgColor; }
		}
		public bool Focused
		{
			get { return this.view.RenderableControl.Focused; }
		}
		public EditingGuide EditingUserGuide
		{
			get { return this.view.EditingUserGuides; }
		}
		internal NativeRenderableSite RenderableSite
		{
			get { return this.view == null ? null : this.view.RenderableSite; }
		}
		internal Control RenderableControl
		{
			get { return this.view == null ? null : this.view.RenderableControl; }
		}

		public Camera CameraComponent
		{
			get { return this.view.CameraComponent; }
		}
		public GameObject CameraObj
		{
			get { return this.view.CameraObj; }
		}
		

		public Point PointToClient(Point screenCoord)
		{
			return this.view.RenderableControl.PointToClient(screenCoord);
		}
		public Point PointToScreen(Point clientCoord)
		{
			return this.view.RenderableControl.PointToScreen(clientCoord);
		}
		public void Invalidate()
		{
			if (this.view == null || this.view.RenderableControl == null) return;
			this.view.RenderableControl.Invalidate();
		}
		public void Focus()
		{
			this.view.RenderableControl.Focus();
		}

		public void MakeDualityTarget()
		{
			this.view.MakeDualityTarget();
		}
	}
}
