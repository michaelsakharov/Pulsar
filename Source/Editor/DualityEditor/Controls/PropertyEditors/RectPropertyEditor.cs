using System;
using System.Linq;

using AdamsLair.WinForms.PropertyEditing.Templates;

using Duality;

namespace Duality.Editor.Controls.PropertyEditors
{
	[PropertyEditorAssignment(typeof(Rect))]
	public class RectPropertyEditor : VectorPropertyEditor
	{
		public override object DisplayedValue
		{
			get 
			{ 
				return new Rect((double)this.editor[0].Value, (double)this.editor[1].Value, (double)this.editor[2].Value, (double)this.editor[3].Value);
			}
		}


		public RectPropertyEditor() : base(4, 2)
		{
			this.editor[0].Edited += this.editorX_Edited;
			this.editor[1].Edited += this.editorY_Edited;
			this.editor[2].Edited += this.editorW_Edited;
			this.editor[3].Edited += this.editorH_Edited;
		}


		protected override void OnGetValue()
		{
			base.OnGetValue();
			object[] values = this.GetValue().ToArray();

			this.BeginUpdate();
			if (!values.Any())
			{
				this.editor[0].Value = 0;
				this.editor[1].Value = 0;
				this.editor[2].Value = 0;
				this.editor[3].Value = 0;
			}
			else
			{
				var valNotNull = values.NotNull();
				double avgX = valNotNull.Average(o => ((Rect)o).X);
				double avgY = valNotNull.Average(o => ((Rect)o).Y);
				double avgW = valNotNull.Average(o => ((Rect)o).W);
				double avgH = valNotNull.Average(o => ((Rect)o).H);

				this.editor[0].Value = MathF.SafeToDecimal(avgX);
				this.editor[1].Value = MathF.SafeToDecimal(avgY);
				this.editor[2].Value = MathF.SafeToDecimal(avgW);
				this.editor[3].Value = MathF.SafeToDecimal(avgH);

				this.multiple[0] = (values.Any(o => o == null) || values.Any(o => ((Rect)o).X != avgX));
				this.multiple[1] = (values.Any(o => o == null) || values.Any(o => ((Rect)o).Y != avgY));
				this.multiple[2] = (values.Any(o => o == null) || values.Any(o => ((Rect)o).W != avgW));
				this.multiple[3] = (values.Any(o => o == null) || values.Any(o => ((Rect)o).H != avgH));
			}
			this.EndUpdate();
		}
		protected override void ApplyDefaultSubEditorConfig(NumericEditorTemplate subEditor)
		{
			base.ApplyDefaultSubEditorConfig(subEditor);
			subEditor.DecimalPlaces = 0;
			subEditor.Increment = 1;
		}

		private void editorX_Edited(object sender, EventArgs e)
		{
			this.HandleValueEdited<Rect>((oldVal, newVal) => new Rect(newVal.X, oldVal.Y, oldVal.W, oldVal.H));
		}
		private void editorY_Edited(object sender, EventArgs e)
		{
			this.HandleValueEdited<Rect>((oldVal, newVal) => new Rect(oldVal.X, newVal.Y, oldVal.W, oldVal.H));
		}
		private void editorW_Edited(object sender, EventArgs e)
		{
			this.HandleValueEdited<Rect>((oldVal, newVal) => new Rect(oldVal.X, oldVal.Y, newVal.W, oldVal.H));
		}
		private void editorH_Edited(object sender, EventArgs e)
		{
			this.HandleValueEdited<Rect>((oldVal, newVal) => new Rect(oldVal.X, oldVal.Y, oldVal.W, newVal.H));
		}
	}
}

