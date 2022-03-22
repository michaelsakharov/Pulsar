﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using Duality.Resources;
using Duality.Backend;

namespace Duality.Drawing
{
	/// <summary>
	/// Describes a continuous range of vertex indices to be rendered.
	/// </summary>
	/// <see cref="DrawBatch"/>
	public struct VertexDrawRange
	{
		/// <summary>
		/// Index of the first vertex to be rendered.
		/// </summary>
		public int Index;
		/// <summary>
		/// The number of vertices to be rendered, starting from <see cref="Index"/>.
		/// </summary>
		public int Count;

		public VertexDrawRange(int index, int count)
		{
			this.Index = index;
			this.Count = count;
		}

		public override string ToString()
		{
			return string.Format(
				"[{0} - {1}]", 
				this.Index, 
				this.Index + this.Count - 1);
		}
	}
}
