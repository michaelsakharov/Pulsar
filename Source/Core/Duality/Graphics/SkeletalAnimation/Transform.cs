using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duality.Graphics.SkeletalAnimation
{
	public struct Transform
	{
		public Vector3 Position;
		public Quaternion Orientation;

		public Transform(Vector3 position, Quaternion orientation)
		{
			Position = position;
			Orientation = orientation;
		}
	}
}
