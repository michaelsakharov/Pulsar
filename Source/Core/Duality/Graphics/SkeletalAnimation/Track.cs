﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duality.Graphics.SkeletalAnimation
{
	[Serializable]
	struct Track
	{
		public int BoneIndex;
		public KeyFrame[] KeyFrames;
	}
}
