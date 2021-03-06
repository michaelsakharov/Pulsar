namespace Duality.Editor.Plugins.CamView
{
	partial class CamView
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CamView));
			this.stateSelector = new System.Windows.Forms.ToolStripComboBox();
			this.viewToEditSeparator = new System.Windows.Forms.ToolStripSeparator();
			this.renderToPerspectiveSeparator = new System.Windows.Forms.ToolStripSeparator();
			this.camSelector = new System.Windows.Forms.ToolStripComboBox();
			this.toolbarCamera = new System.Windows.Forms.ToolStrip();
			this.layerSelector = new System.Windows.Forms.ToolStripDropDownButton();
			this.objectVisibilitySelector = new System.Windows.Forms.ToolStripDropDownButton();
			this.perspectiveDropDown = new System.Windows.Forms.ToolStripDropDownButton();
			this.renderSetupSelector = new System.Windows.Forms.ToolStripDropDownButton();
			this.showBgColorDialog = new System.Windows.Forms.ToolStripButton();
			this.snapToGridSelector = new System.Windows.Forms.ToolStripDropDownButton();
			this.snapToGridInactiveItem = new System.Windows.Forms.ToolStripMenuItem();
			this.snapToGridPixelPerfectItem = new System.Windows.Forms.ToolStripMenuItem();
			this.snapToGrid16Item = new System.Windows.Forms.ToolStripMenuItem();
			this.snapToGrid32Item = new System.Windows.Forms.ToolStripMenuItem();
			this.snapToGrid64Item = new System.Windows.Forms.ToolStripMenuItem();
			this.snapToGridCustomItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolbarCamera.SuspendLayout();
			this.SuspendLayout();
			// 
			// stateSelector
			// 
			this.stateSelector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.stateSelector.FlatStyle = System.Windows.Forms.FlatStyle.Standard;
			this.stateSelector.Name = "stateSelector";
			this.stateSelector.Size = new System.Drawing.Size(121, 25);
			this.stateSelector.ToolTipText = "Select the editing state of this View";
			this.stateSelector.DropDown += new System.EventHandler(this.stateSelector_DropDown);
			this.stateSelector.DropDownClosed += new System.EventHandler(this.stateSelector_DropDownClosed);
			this.stateSelector.SelectedIndexChanged += new System.EventHandler(this.stateSelector_SelectedIndexChanged);
			// 
			// viewToEditSeparator
			// 
			this.viewToEditSeparator.Name = "viewToEditSeparator";
			this.viewToEditSeparator.Size = new System.Drawing.Size(6, 25);
			// 
			// renderToPerspectiveSeparator
			// 
			this.renderToPerspectiveSeparator.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.renderToPerspectiveSeparator.ForeColor = System.Drawing.SystemColors.ControlText;
			this.renderToPerspectiveSeparator.Name = "renderToPerspectiveSeparator";
			this.renderToPerspectiveSeparator.Size = new System.Drawing.Size(6, 25);
			// 
			// camSelector
			// 
			this.camSelector.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.camSelector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.camSelector.FlatStyle = System.Windows.Forms.FlatStyle.Standard;
			this.camSelector.Name = "camSelector";
			this.camSelector.Size = new System.Drawing.Size(121, 25);
			this.camSelector.ToolTipText = "Select the Camera output to display in this view";
			this.camSelector.DropDown += new System.EventHandler(this.camSelector_DropDown);
			this.camSelector.DropDownClosed += new System.EventHandler(this.camSelector_DropDownClosed);
			this.camSelector.SelectedIndexChanged += new System.EventHandler(this.camSelector_SelectedIndexChanged);
			// 
			// toolbarCamera
			// 
			this.toolbarCamera.AutoSize = false;
			this.toolbarCamera.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(212)))), ((int)(((byte)(212)))));
			this.toolbarCamera.GripMargin = new System.Windows.Forms.Padding(0);
			this.toolbarCamera.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolbarCamera.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.stateSelector,
            this.layerSelector,
            this.objectVisibilitySelector,
            this.viewToEditSeparator,
            this.perspectiveDropDown,
            this.renderToPerspectiveSeparator,
            this.renderSetupSelector,
            this.showBgColorDialog,
            this.camSelector,
            this.snapToGridSelector});
			this.toolbarCamera.Location = new System.Drawing.Point(0, 0);
			this.toolbarCamera.Name = "toolbarCamera";
			this.toolbarCamera.Size = new System.Drawing.Size(651, 25);
			this.toolbarCamera.TabIndex = 1;
			this.toolbarCamera.Text = "toolStrip";
			// 
			// layerSelector
			// 
			this.layerSelector.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.layerSelector.Image = global::Duality.Editor.Plugins.CamView.Properties.Resources.layers;
			this.layerSelector.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.layerSelector.Name = "layerSelector";
			this.layerSelector.Size = new System.Drawing.Size(29, 22);
			this.layerSelector.Text = "Select visible layers";
			this.layerSelector.DropDownOpening += new System.EventHandler(this.layerSelector_DropDownOpening);
			this.layerSelector.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.layerSelector_DropDownItemClicked);
			// 
			// objectVisibilitySelector
			// 
			this.objectVisibilitySelector.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.objectVisibilitySelector.Image = global::Duality.Editor.Plugins.CamView.Properties.Resources.ObjectVisibility;
			this.objectVisibilitySelector.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.objectVisibilitySelector.Name = "objectVisibilitySelector";
			this.objectVisibilitySelector.Size = new System.Drawing.Size(29, 22);
			this.objectVisibilitySelector.Text = "Select visible objects";
			this.objectVisibilitySelector.DropDownOpening += new System.EventHandler(this.objectVisibilitySelector_DropDownOpening);
			// 
			// perspectiveDropDown
			// 
			this.perspectiveDropDown.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.perspectiveDropDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.perspectiveDropDown.Image = global::Duality.Editor.Plugins.CamView.Properties.Resources.shape_perspective;
			this.perspectiveDropDown.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.perspectiveDropDown.Name = "perspectiveDropDown";
			this.perspectiveDropDown.Size = new System.Drawing.Size(29, 22);
			this.perspectiveDropDown.Text = "Select Perspective Mode";
			this.perspectiveDropDown.DropDownOpening += new System.EventHandler(this.perspectiveDropDown_DropDownOpening);
			this.perspectiveDropDown.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.perspectiveDropDown_DropDownItemClicked);
			// 
			// renderSetupSelector
			// 
			this.renderSetupSelector.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.renderSetupSelector.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.renderSetupSelector.Image = global::Duality.Editor.Plugins.CamView.Properties.Resources.RenderSetup;
			this.renderSetupSelector.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.renderSetupSelector.Name = "renderSetupSelector";
			this.renderSetupSelector.Size = new System.Drawing.Size(29, 22);
			this.renderSetupSelector.Text = "Select the RenderSetup to use";
			this.renderSetupSelector.DropDownOpening += new System.EventHandler(this.renderSetupSelector_DropDownOpening);
			// 
			// showBgColorDialog
			// 
			this.showBgColorDialog.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.showBgColorDialog.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.showBgColorDialog.Image = ((System.Drawing.Image)(resources.GetObject("showBgColorDialog.Image")));
			this.showBgColorDialog.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.showBgColorDialog.Name = "showBgColorDialog";
			this.showBgColorDialog.Size = new System.Drawing.Size(23, 22);
			this.showBgColorDialog.Text = "Change Background Color";
			this.showBgColorDialog.Click += new System.EventHandler(this.showBgColorDialog_Click);
			// 
			// snapToGridSelector
			// 
			this.snapToGridSelector.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.snapToGridSelector.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.snapToGridInactiveItem,
            this.snapToGridPixelPerfectItem,
            this.snapToGrid16Item,
            this.snapToGrid32Item,
            this.snapToGrid64Item,
            this.snapToGridCustomItem});
			this.snapToGridSelector.Image = global::Duality.Editor.Plugins.CamView.Properties.Resources.SnapToGrid;
			this.snapToGridSelector.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.snapToGridSelector.Name = "snapToGridSelector";
			this.snapToGridSelector.Size = new System.Drawing.Size(29, 22);
			this.snapToGridSelector.Text = "Snap to Grid";
			this.snapToGridSelector.DropDownOpening += new System.EventHandler(this.snapToGridSelector_DropDownOpening);
			this.snapToGridSelector.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.snapToGridSelector_DropDownItemClicked);
			// 
			// snapToGridInactiveItem
			// 
			this.snapToGridInactiveItem.Name = "snapToGridInactiveItem";
			this.snapToGridInactiveItem.Size = new System.Drawing.Size(140, 22);
			this.snapToGridInactiveItem.Text = "Don\'t";
			// 
			// snapToGridPixelPerfectItem
			// 
			this.snapToGridPixelPerfectItem.Name = "snapToGridPixelPerfectItem";
			this.snapToGridPixelPerfectItem.Size = new System.Drawing.Size(140, 22);
			this.snapToGridPixelPerfectItem.Text = "Pixel-Perfect";
			// 
			// snapToGrid16Item
			// 
			this.snapToGrid16Item.Name = "snapToGrid16Item";
			this.snapToGrid16Item.Size = new System.Drawing.Size(140, 22);
			this.snapToGrid16Item.Tag = "";
			this.snapToGrid16Item.Text = "16 x 16";
			// 
			// snapToGrid32Item
			// 
			this.snapToGrid32Item.Name = "snapToGrid32Item";
			this.snapToGrid32Item.Size = new System.Drawing.Size(140, 22);
			this.snapToGrid32Item.Text = "32 x 32";
			// 
			// snapToGrid64Item
			// 
			this.snapToGrid64Item.Name = "snapToGrid64Item";
			this.snapToGrid64Item.Size = new System.Drawing.Size(140, 22);
			this.snapToGrid64Item.Text = "64 x 64";
			// 
			// snapToGridCustomItem
			// 
			this.snapToGridCustomItem.Name = "snapToGridCustomItem";
			this.snapToGridCustomItem.Size = new System.Drawing.Size(140, 22);
			this.snapToGridCustomItem.Text = "Custom";
			// 
			// CamView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(212)))), ((int)(((byte)(212)))));
			this.ClientSize = new System.Drawing.Size(651, 438);
			this.Controls.Add(this.toolbarCamera);
			this.DockAreas = ((WeifenLuo.WinFormsUI.Docking.DockAreas)((WeifenLuo.WinFormsUI.Docking.DockAreas.Float | WeifenLuo.WinFormsUI.Docking.DockAreas.Document)));
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "CamView";
			this.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.Document;
			this.Text = "CamView";
			this.toolbarCamera.ResumeLayout(false);
			this.toolbarCamera.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ToolStripComboBox stateSelector;
		private System.Windows.Forms.ToolStripSeparator viewToEditSeparator;
		private Duality.Editor.Controls.ToolStrip.ToolStripNumericUpDown focusDist;
		private System.Windows.Forms.ToolStripSeparator renderToPerspectiveSeparator;
		private System.Windows.Forms.ToolStripButton showBgColorDialog;
		private System.Windows.Forms.ToolStripComboBox camSelector;
		private System.Windows.Forms.ToolStrip toolbarCamera;
		private System.Windows.Forms.ToolStripDropDownButton layerSelector;
		private System.Windows.Forms.ToolStripDropDownButton perspectiveDropDown;
		private System.Windows.Forms.ToolStripDropDownButton snapToGridSelector;
		private System.Windows.Forms.ToolStripMenuItem snapToGridInactiveItem;
		private System.Windows.Forms.ToolStripMenuItem snapToGrid16Item;
		private System.Windows.Forms.ToolStripMenuItem snapToGrid32Item;
		private System.Windows.Forms.ToolStripMenuItem snapToGridPixelPerfectItem;
		private System.Windows.Forms.ToolStripMenuItem snapToGrid64Item;
		private System.Windows.Forms.ToolStripMenuItem snapToGridCustomItem;
		private System.Windows.Forms.ToolStripDropDownButton objectVisibilitySelector;
		private System.Windows.Forms.ToolStripDropDownButton renderSetupSelector;

	}
}