using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using Duality;
using Duality.Components;
using Duality.Resources;
using Duality.Drawing;
using Duality.Editor;


namespace Duality.Editor.Plugins.Base.DataConverters
{
	public class TextureFromMaterial : DataConverter
	{
		public override Type TargetType
		{
			get { return typeof(Texture); }
		}

		public override bool CanConvertFrom(ConvertOperation convert)
		{
			return
				convert.AllowedOperations.HasFlag(ConvertOperation.Operation.Convert) &&
				convert.CanPerform<Material>();
		}
		public override bool Convert(ConvertOperation convert)
		{
			bool finishConvertOp = false;

			List<Material> availData = convert.Perform<Material>().ToList();

			// Append objects
			foreach (Material baseRes in availData)
			{
				if (convert.IsObjectHandled(baseRes)) continue;
				if (baseRes.Map == null || !baseRes.Map.IsAvailable) continue;

				convert.AddResult(baseRes.Map.Res);
				finishConvertOp = true;
				convert.MarkObjectHandled(baseRes);
			}

			return finishConvertOp;
		}
	}
}