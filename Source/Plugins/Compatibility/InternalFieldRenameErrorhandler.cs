using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Duality;
using Duality.Serialization;

namespace Duality.Plugins.Compatibility
{
	public class InternalFieldRenameErrorhandler : SerializeErrorHandler
	{
		public override void HandleError(SerializeError error)
		{
			AssignFieldError assignFieldError = error as AssignFieldError;
			if (assignFieldError == null) return;


			return;
		}
	}
}
