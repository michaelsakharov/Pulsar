using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duality.Graphics.Particles.Generators
{
	public class BasicVelocityGenerator : IParticleGenerator
	{
		public Vector3 MinStartVelocity = Vector3.Zero;
		public Vector3 MaxStartVelocity = Vector3.Zero;

		public void Generate(float deltaTime, ParticleData particles, int startId, int endId)
		{
			Random rnd = new Random();
			for (var i = startId; i < endId; i++)
			{
				rnd.NextFloat(ref MinStartVelocity, ref MaxStartVelocity, out particles.Velocity[i]);
			}
		}
	}
}
