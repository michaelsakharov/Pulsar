using System.Collections.Generic;
using Duality.Drawing;

namespace Duality.DebugDraw
{
	public struct GizmosPrimitive
	{
		public List<Vector3> vertices;
		public ColorRgba color;
		public Matrix4 matrix;
	}
}
