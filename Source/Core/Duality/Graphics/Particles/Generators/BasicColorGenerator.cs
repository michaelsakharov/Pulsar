using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duality.Graphics.Particles.Generators
{
	public class BasicColorGenerator : IParticleGenerator
	{
		public Vector4 MinStartColor = Vector4.Zero;
		public Vector4 MaxStartColor = Vector4.Zero;

		public Vector4 MinEndColor = Vector4.Zero;
		public Vector4 MaxEndColor = Vector4.Zero;

		public void Generate(float deltaTime, ParticleData particles, int startId, int endId)
		{
			Random rnd = new Random();
			for (var i = startId; i < endId; i++)
			{
				//var angle = rnd.NextFloat(0.0f, MathF.TwoPi);

				rnd.NextFloat(ref MinStartColor, ref MaxStartColor, out particles.StartColor[i]);
				rnd.NextFloat(ref MinEndColor, ref MaxEndColor, out particles.EndColor[i]);
			}
		}
	}
}
