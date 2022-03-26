using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duality.Graphics
{
	class BlurHelper
	{
		public static void Init(ref Vector4[] blurWeights, ref Vector4[] blurOffsetsHorz, ref Vector4[] blurOffsetsVert, Vector2 texelSize, float deviation = 3.0f)
		{
			blurOffsetsHorz[0] = Vector4.Zero;
			blurOffsetsVert[0] = Vector4.Zero;
			blurWeights[0] = new Vector4(
				GaussianDistribution(0, 0, deviation), GaussianDistribution(0, 0, deviation), GaussianDistribution(0, 0, deviation),
				1.0f
				);

			for (var i = 1; i < 8; ++i)
			{
				var weight = 1.25f * GaussianDistribution((float)i, 0, deviation);
				blurWeights[i] = new Vector4(weight, weight, weight, 1.0f);
				blurOffsetsHorz[i] = new Vector4(i * texelSize.X, 0, 0, 0);
				blurOffsetsVert[i] = new Vector4(0, i * texelSize.Y, 0, 0);
			}

			for (var i = 8; i < 15; ++i)
			{
				var weight = blurWeights[i - 7].X;
				blurWeights[i] = new Vector4(weight, weight, weight, 1.0f);

				blurOffsetsHorz[i] = new Vector4(-blurOffsetsHorz[i - 7].X, 0, 0, 0);
				blurOffsetsVert[i] = new Vector4(0, -blurOffsetsVert[i - 7].Y, 0, 0);
			}
		}

		public static float Square(float x)
		{
			return x * x;
		}

		public static float GaussianDistribution(float x, float offset, float scale)
		{
			var nom = (float)System.Math.Exp(-Square(x - offset) / (2 * Square(scale)));
			var denom = scale * (float)System.Math.Sqrt(2 * System.Math.PI);

			return nom / denom;
		}
	}
}
