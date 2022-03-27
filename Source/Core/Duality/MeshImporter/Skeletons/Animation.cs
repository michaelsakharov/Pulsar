﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duality.MeshImporter.Skeletons
{
	class Animation
	{
		public string Name;
		public float Length;
		public List<Track> Tracks = new List<Track>();
	}
}
