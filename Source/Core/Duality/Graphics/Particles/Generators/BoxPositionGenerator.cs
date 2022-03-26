using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duality.Graphics.Particles.Generators
{
	public class BoxPositionGenerator : IParticleGenerator
	{
		public Vector3 Position = Vector3.Zero;
		public Vector3 MaxStartPosOffset = Vector3.Zero;

		public void Generate(float deltaTime, ParticleData particles, int startId, int endId)
		{
			var minPosition = Position - MaxStartPosOffset;
			var maxPosition = Position + MaxStartPosOffset;

			Random rnd = new Random();
			for (var i = startId; i < endId; i++)
			{
				particles.Position[i].X = rnd.NextFloat(minPosition.X, maxPosition.X);
				particles.Position[i].Y = rnd.NextFloat(minPosition.Y, maxPosition.Y);
				particles.Position[i].Z = rnd.NextFloat(minPosition.Z, maxPosition.Z);
			}
		}
	}
}
