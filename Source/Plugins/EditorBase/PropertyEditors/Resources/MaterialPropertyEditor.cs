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
		}
		protected override bool IsAutoCreateMember(MemberInfo info)
		{
			return false;
		}
	}
}
