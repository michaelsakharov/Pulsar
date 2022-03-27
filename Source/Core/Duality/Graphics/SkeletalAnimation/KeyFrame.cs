using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duality.Graphics.SkeletalAnimation
{
	[Serializable]
	struct KeyFrame
	{
		public float Time;
		public Transform Transform;
	}
}
