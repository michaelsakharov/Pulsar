﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Reflection;

using Duality;
using Duality.Cloning;
using Duality.Drawing;
using Duality.Resources;
using Duality.Components;

using NUnit.Framework;

namespace Duality.Tests.Cloning.HelperObjects
{
	[CloneBehavior(CloneBehavior.Reference)]
	internal class ReferencedObject
	{
		public string TestProperty { get; set; }
	}
}
