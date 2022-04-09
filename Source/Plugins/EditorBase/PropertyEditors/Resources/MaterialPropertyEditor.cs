using System.Collections.Generic;
using System.Reflection;

using Duality;
using Duality.Resources;
using Duality.Editor;

namespace Duality.Editor.Plugins.Base.PropertyEditors
{
	[PropertyEditorAssignment(typeof(Material), PropertyEditorAssignmentAttribute.PrioritySpecialized)]
	public class MaterialPropertyEditor : ResourcePropertyEditor
	{
		public MaterialPropertyEditor() : base()
		{
			this.Indent = 0;
		}

		protected override void BeforeAutoCreateEditors()
		{
			base.BeforeAutoCreateEditors();
			ResourcePropertyEditor content = new ResourcePropertyEditor();
			content.EditedType = this.EditedType;
			content.Getter = this.GetValue;
			content.Setter = this.SetValues;
			content.Hints = HintFlags.None;
			content.HeaderHeight = 0;
			content.HeaderValueText = null;
			content.PreventFocus = true;
			this.ParentGrid.ConfigureEditor(content);
			this.AddPropertyEditor(content);
			content.Expanded = true;
		}
		protected override bool IsAutoCreateMember(MemberInfo info)
		{
			return false;
		}
	}
}
