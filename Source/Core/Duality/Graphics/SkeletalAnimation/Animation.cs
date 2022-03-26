using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duality.Graphics.SkeletalAnimation
{
	public class Animation
	{
		public readonly string Name;
		public readonly float Length;
		internal Track[] Tracks;

		public Animation(string name, float length)
		{
			Name = name;
			Length = length;
		}

		public override string ToString()
		{
			return $"{Name} {Length}s";
		}
	}
}
