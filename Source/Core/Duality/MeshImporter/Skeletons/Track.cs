﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duality.MeshImporter.Skeletons
{
	class Track
	{
		public int BoneIndex;
		public List<KeyFrame> KeyFrames = new List<KeyFrame>();
	}
}