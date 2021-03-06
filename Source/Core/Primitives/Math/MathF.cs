using System;

namespace Duality
{
	/// <summary>
	/// Provides math utility methods and double versions of <see cref="System.Math"/> to fit
	/// Duality <see cref="double"/> arithmetics. 
	/// </summary>
	public static class MathF
	{
		/// <summary>
		/// Euler's number, base of the natural logarithm. Approximately 2.7182818284.
		/// </summary>
		public const double E = System.Math.E;
		/// <summary>
		/// Mmmhh... pie!
		/// </summary>
		public const double Pi = System.Math.PI;

		/// <summary>
		/// Equals <see cref="Pi"/> / 2.
		/// </summary>
		public const double PiOver2 = Pi / 2.0f;
		/// <summary>
		/// Equals <see cref="Pi"/> / 3.
		/// </summary>
		public const double PiOver3 = Pi / 3.0f;
		/// <summary>
		/// Equals <see cref="Pi"/> / 4.
		/// </summary>
		public const double PiOver4 = Pi / 4.0f;
		/// <summary>
		/// Equals <see cref="Pi"/> / 6.
		/// </summary>
		public const double PiOver6 = Pi / 6.0f;
		/// <summary>
		/// Equals 2 * <see cref="Pi"/>.
		/// </summary>
		public const double TwoPi = Pi * 2.0f;
		
		/// <summary>
		/// Another way to write RadAngle1
		/// </summary>
		public const double Deg2Rad = (Pi * 2f) / 360f;
		
		/// <summary>
		/// Rad to Deg
		/// </summary>
		public const double Rad2Deg = 360f / (Pi * 2f);
		
		/// <summary>
		/// A one degree angle in radians.
		/// </summary>
		public const double RadAngle1 = TwoPi / 360.0f;
		/// <summary>
		/// A 30 degree angle in radians. Equals <see cref="PiOver6"/>.
		/// </summary>
		public const double RadAngle30 = PiOver6;
		/// <summary>
		/// A 45 degree angle in radians. Equals <see cref="PiOver4"/>.
		/// </summary>
		public const double RadAngle45 = PiOver4;
		/// <summary>
		/// A 90 degree angle in radians. Equals <see cref="PiOver2"/>.
		/// </summary>
		public const double RadAngle90 = PiOver2;
		/// <summary>
		/// A 180 degree angle in radians. Equals <see cref="Pi"/>.
		/// </summary>
		public const double RadAngle180 = Pi;
		/// <summary>(PI * 2) / 360
		/// A 270 degree angle in radians. Equals <see cref="Pi"/>.
		/// </summary>
		public const double RadAngle270 = Pi + PiOver2;
		/// <summary>
		/// A 360 degree angle in radians. Equals <see cref="TwoPi"/>.
		/// </summary>
		public const double RadAngle360 = TwoPi;

		private static Random rnd = new Random((int)(DateTime.Now.Ticks % int.MaxValue));
		/// <summary>
		/// [GET / SET] Global random number generator. Is never null.
		/// </summary>
		public static Random Rnd
		{
			get { return rnd; }
			set { rnd = value ?? new Random(); }
		}


		/// <summary>
		/// Converts the specified double value to decimal and clamps it if necessary.
		/// </summary>
		/// <param name="v"></param>
		public static decimal SafeToDecimal(double v)
		{
			if (double.IsNaN(v))
				return decimal.Zero;
			else if (v <= (double)decimal.MinValue || double.IsNegativeInfinity(v))
				return decimal.MinValue;
			else if (v >= (double)decimal.MaxValue || double.IsPositiveInfinity(v))
				return decimal.MaxValue;
			else
				return (decimal)v;
		}

		/// <summary>
		/// Returns the absolute value of a <see cref="double"/>.
		/// </summary>
		/// <param name="v">A number.</param>
		/// <returns>The absolute value of the number.</returns>
		public static double Abs(double v)
		{
			return v < 0 ? -v : v;
		}
		/// <summary>
		/// Returns the absolute value of a <see cref="int"/>.
		/// </summary>
		/// <param name="v">A number.</param>
		/// <returns>The absolute value of the number.</returns>
		public static int Abs(int v)
		{
			return v < 0 ? -v : v;
		}

		/// <summary>
		/// Returns the lowest whole-number bigger than the specified one. (Rounds up)
		/// </summary>
		/// <param name="v">A number.</param>
		/// <returns>The rounded number.</returns>
		/// <seealso cref="Floor"/>
		public static double Ceiling(double v)
		{
			return (double)System.Math.Ceiling(v);
		}
		/// <summary>
		/// Returns the highest whole-number smaller than the specified one. (Rounds down)
		/// </summary>
		/// <param name="v">A number.</param>
		/// <returns>The rounded number.</returns>
		/// <seealso cref="Ceiling"/>
		public static double Floor(double v)
		{
			return (double)System.Math.Floor(v);
		}
		/// <summary>
		/// Returns the highest whole-number smaller than the specified one. (Rounds down)
		/// </summary>
		/// <param name="v">A number.</param>
		/// <returns>The rounded number as an int.</returns>
		/// <seealso cref="Ceiling"/>
		public static int FloorToInt(double v)
		{
			return (int)System.Math.Floor(v);
		}

		/// <summary>
		/// Rounds the specified value.
		/// </summary>
		/// <param name="v">A number.</param>
		/// <returns>The rounded number.</returns>
		public static double Round(double v)
		{
			return (double)System.Math.Round(v);
		}
		/// <summary>
		/// Rounds the specified value to a certain number of fraction digits.
		/// </summary>
		/// <param name="v">A number.</param>
		/// <param name="digits">The number of fraction digits to round to.</param>
		/// <returns>The rounded number.</returns>
		public static double Round(double v, int digits)
		{
			return (double)System.Math.Round(v, digits);
		}
		/// <summary>
		/// Rounds the specified value.
		/// </summary>
		/// <param name="v">A number.</param>
		/// <param name="mode">Specifies what happens if the value is exactly inbetween two numbers.</param>
		/// <returns>The rounded number.</returns>
		public static double Round(double v, MidpointRounding mode)
		{
			return (double)System.Math.Round(v, mode);
		}
		/// <summary>
		/// Rounds the specified value to a certain number of fraction digits.
		/// </summary>
		/// <param name="v">A number.</param>
		/// <param name="digits">The number of fraction digits to round to.</param>
		/// <param name="mode">Specifies what happens if the value is exactly inbetween two numbers.</param>
		/// <returns>The rounded number.</returns>
		public static double Round(double v, int digits, MidpointRounding mode)
		{
			return (double)System.Math.Round(v, digits, mode);
		}

		/// <summary>
		/// Rounds the specified value to an integer value.
		/// </summary>
		/// <param name="v">A number.</param>
		/// <returns>The rounded number as <see cref="int"/>.</returns>
		/// <seealso cref="Round(double)"/>
		public static int RoundToInt(double v)
		{
			return (int)System.Math.Round(v);
		}
		/// <summary>
		/// Rounds the specified value to an integer value.
		/// </summary>
		/// <param name="v">A number.</param>
		/// <param name="mode">Specifies what happens if the value is exactly inbetween two numbers.</param>
		/// <returns>The rounded number as <see cref="int"/>.</returns>
		/// <seealso cref="Round(double, MidpointRounding)"/>
		public static int RoundToInt(double v, MidpointRounding mode)
		{
			return (int)System.Math.Round(v, mode);
		}

		/// <summary>
		/// Returns the sign of a value.
		/// </summary>
		/// <param name="v">A number.</param>
		/// <returns>-1 if negative, 1 if positive and 0 if zero.</returns>
		public static double Sign(double v)
		{
			return v < 0.0f ? -1.0f : (v > 0.0f ? 1.0f : 0.0f);
		}
		/// <summary>
		/// Returns the sign of a value.
		/// </summary>
		/// <param name="v">A number.</param>
		/// <returns>-1 if negative, 1 if positive and 0 if zero.</returns>
		public static int Sign(int v)
		{
			return v < 0 ? -1 : (v > 0 ? 1 : 0);
		}

		/// <summary>
		/// Returns a numbers square root.
		/// </summary>
		/// <param name="v">A number.</param>
		/// <returns>The numbers square root.</returns>
		public static double Sqrt(double v)
		{
			return (double)System.Math.Sqrt(v);
		}

		/// <summary>
		/// Returns the factorial of an integer value.
		/// </summary>
		/// <param name="n">A number.</param>
		/// <returns>The factorial of the number.</returns>
		public static int Factorial(int n)
		{
			int r = 1;
			for (; n > 1; n--) r *= n;
			return System.Math.Abs(r);
		}

		/// <summary>
		/// Returns the lower of two values.
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <returns>The lowest value.</returns>
		public static double Min(double v1, double v2)
		{
			return v1 < v2 ? v1 : v2;
		}
		/// <summary>
		/// Returns the lowest of three values.
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <param name="v3"></param>
		/// <returns>The lowest value.</returns>
		public static double Min(double v1, double v2, double v3)
		{
			double min = v1;
			if (v2 < min) min = v2;
			if (v3 < min) min = v3;
			return min;
		}
		/// <summary>
		/// Returns the lowest of four values.
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <param name="v3"></param>
		/// <param name="v4"></param>
		/// <returns>The lowest value.</returns>
		public static double Min(double v1, double v2, double v3, double v4)
		{
			double min = v1;
			if (v2 < min) min = v2;
			if (v3 < min) min = v3;
			if (v4 < min) min = v4;
			return min;
		}
		/// <summary>
		/// Returns the lowest of any number of values.
		/// </summary>
		/// <param name="v"></param>
		/// <returns>The lowest value.</returns>
		public static double Min(params double[] v)
		{
			double min = v[0];
			for (int i = 1; i < v.Length; i++)
			{
				if (v[i] < min) min = v[i];
			}
			return min;
		}
		/// <summary>
		/// Returns the lower of two values.
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <returns>The lowest value.</returns>
		public static int Min(int v1, int v2)
		{
			return v1 < v2 ? v1 : v2;
		}
		/// <summary>
		/// Returns the lowest of three values.
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <param name="v3"></param>
		/// <returns>The lowest value.</returns>
		public static int Min(int v1, int v2, int v3)
		{
			int min = v1;
			if (v2 < min) min = v2;
			if (v3 < min) min = v3;
			return min;
		}
		/// <summary>
		/// Returns the lowest of four values.
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <param name="v3"></param>
		/// <param name="v4"></param>
		/// <returns>The lowest value.</returns>
		public static int Min(int v1, int v2, int v3, int v4)
		{
			int min = v1;
			if (v2 < min) min = v2;
			if (v3 < min) min = v3;
			if (v4 < min) min = v4;
			return min;
		}
		/// <summary>
		/// Returns the lowest of any number of values.
		/// </summary>
		/// <param name="v"></param>
		/// <returns>The lowest value.</returns>
		public static int Min(params int[] v)
		{
			int min = v[0];
			for (int i = 1; i < v.Length; i++)
			{
				if (v[i] < min) min = v[i];
			}
			return min;
		}
		
		/// <summary>
		/// Returns the higher of two values.
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <returns>The highest value.</returns>
		public static double Max(double v1, double v2)
		{
			return v1 > v2 ? v1 : v2;
		}
		/// <summary>
		/// Returns the highest of three values.
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <param name="v3"></param>
		/// <returns>The highest value.</returns>
		public static double Max(double v1, double v2, double v3)
		{
			double max = v1;
			if (v2 > max) max = v2;
			if (v3 > max) max = v3;
			return max;
		}
		/// <summary>
		/// Returns the highest of four values.
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <param name="v3"></param>
		/// <param name="v4"></param>
		/// <returns>The highest value.</returns>
		public static double Max(double v1, double v2, double v3, double v4)
		{
			double max = v1;
			if (v2 > max) max = v2;
			if (v3 > max) max = v3;
			if (v4 > max) max = v4;
			return max;
		}
		/// <summary>
		/// Returns the highest of any number of values.
		/// </summary>
		/// <param name="v"></param>
		/// <returns>The highest value.</returns>
		public static double Max(params double[] v)
		{
			double max = v[0];
			for (int i = 1; i < v.Length; i++)
			{
				if (v[i] > max) max = v[i];
			}
			return max;
		}
		/// <summary>
		/// Returns the higher of two values.
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <returns>The highest value.</returns>
		public static int Max(int v1, int v2)
		{
			return v1 > v2 ? v1 : v2;
		}
		/// <summary>
		/// Returns the highest of three values.
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <param name="v3"></param>
		/// <returns>The highest value.</returns>
		public static int Max(int v1, int v2, int v3)
		{
			int max = v1;
			if (v2 > max) max = v2;
			if (v3 > max) max = v3;
			return max;
		}
		/// <summary>
		/// Returns the highest of four values.
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <param name="v3"></param>
		/// <param name="v4"></param>
		/// <returns>The highest value.</returns>
		public static int Max(int v1, int v2, int v3, int v4)
		{
			int max = v1;
			if (v2 > max) max = v2;
			if (v3 > max) max = v3;
			if (v4 > max) max = v4;
			return max;
		}
		/// <summary>
		/// Returns the highest of any number of values.
		/// </summary>
		/// <param name="v"></param>
		/// <returns>The highest value.</returns>
		public static int Max(params int[] v)
		{
			int max = v[0];
			for (int i = 1; i < v.Length; i++)
			{
				if (v[i] > max) max = v[i];
			}
			return max;
		}

		/// <summary>
		/// Clamps a value between minimum and maximum.
		/// </summary>
		/// <param name="v">The value to clamp.</param>
		/// <param name="min">The minimum value that can't be deceeded.</param>
		/// <param name="max">The maximum value that can't be exceeded.</param>
		/// <returns>The clamped value.</returns>
		public static double Clamp(double v, double min, double max)
		{
			return v < min ? min : (v > max ? max : v);
		}

		/// <summary>
		/// Clamps a value between 0 and 1 inclusive.
		/// </summary>
		/// <param name="v">The value to clamp.</param>
		/// <returns>The clamped value.</returns>
		public static double Clamp01(double v)
		{
			return v < 0.0f ? 0.0f : (v > 1.0f ? 1.0f : v);
		}

		/// <summary>
		/// Clamps a value between minimum and maximum.
		/// </summary>
		/// <param name="v">The value to clamp.</param>
		/// <param name="min">The minimum value that can't be deceeded.</param>
		/// <param name="max">The maximum value that can't be exceeded.</param>
		/// <returns>The clamped value.</returns>
		public static int Clamp(int v, int min, int max)
		{
			return v < min ? min : (v > max ? max : v);
		}

		/// <summary>
		/// Performs linear interpolation between two values.
		/// </summary>
		/// <param name="a">The first anchor value.</param>
		/// <param name="b">The second anchor value.</param>
		/// <param name="ratio">Ratio between first and second anchor. Zero will result in anchor a, one will result in anchor b.</param>
		public static double Lerp(double a, double b, double ratio)
		{
			return a + ratio * (b - a);
		}

		/// <summary>
		/// Performs inverse linear interpolation between two anchor values.
		/// </summary>
		/// <param name="min">The first anchor value.</param>
		/// <param name="max">The second anchor value.</param>
		/// <param name="value">The value between both anchor values.</param>
		public static double InvLerp(double min, double max, double value)
		{
			return (value - min) / (max - min);
		}

		/// <summary>
		/// Performs inverse linear interpolation between two anchor values.
		/// </summary>
		/// <param name="min">The first anchor value.</param>
		/// <param name="max">The second anchor value.</param>
		/// <param name="value">The value between both anchor values.</param>
		public static double InvLerp(int min, int max, int value)
		{
			return (double) (value - min) / (max - min);
		}

		/// <summary>
		/// Performs a SmoothStep interpolation between 0 and 1.
		/// </summary>
		/// <param name="value">The input value.</param>
		public static double SmoothStep(double value)
		{
			value = Clamp01(value);
			return (3 - 2 * value) * value * value;
		}

		/// <summary>
		/// Performs a SmoothStep interpolation between two anchor values.
		/// </summary>
		/// <param name="min">The lower bound anchor value</param>
		/// <param name="max">The upper bound anchor value</param>
		/// <param name="value">The input value.</param>
		public static double SmoothStep(double min, double max, double value)
		{
			value = Clamp01(InvLerp(min, max, value));
			return (3 - 2 * value) * value * value;
		}

		/// <summary>
		/// Returns the specified power of <see cref="E"/>.
		/// </summary>
		/// <param name="v"></param>
		public static double Exp(double v)
		{
			return (double)System.Math.Exp(v);
		}
		/// <summary>
		/// Returns the natural logarithm of a value.
		/// </summary>
		/// <param name="v"></param>
		public static double Log(double v)
		{
			return (double)System.Math.Log(v);
		}
		/// <summary>
		/// Returns the specified power of a value.
		/// </summary>
		/// <param name="v">The base value.</param>
		/// <param name="e">Specifies the power to return.</param>
		public static double Pow(double v, double e)
		{
			return (double)System.Math.Pow(v, e);
		}
		/// <summary>
		/// Returns the logarithm of a value.
		/// </summary>
		/// <param name="v">The value whichs logarithm is to be calculated.</param>
		/// <param name="newBase">The base of the logarithm.</param>
		public static double Log(double v, double newBase)
		{
			return (double)System.Math.Log(v, newBase);
		}

		/// <summary>
		/// Returns the sine value of the specified (radian) angle.
		/// </summary>
		/// <param name="angle">A radian angle.</param>
		public static double Sin(double angle)
		{
			return (double)System.Math.Sin(angle);
		}
		/// <summary>
		/// Returns the cosine value of the specified (radian) angle.
		/// </summary>
		/// <param name="angle">A radian angle.</param>
		public static double Cos(double angle)
		{
			return (double)System.Math.Cos(angle);
		}
		/// <summary>
		/// Returns the tangent value of the specified (radian) angle.
		/// </summary>
		/// <param name="angle">A radian angle.</param>
		public static double Tan(double angle)
		{
			return (double)System.Math.Tan(angle);
		}
		/// <summary>
		/// Returns the inverse sine value of the specified (radian) angle.
		/// </summary>
		/// <param name="sin">A radian angle.</param>
		public static double Asin(double sin)
		{
			return (double)System.Math.Asin(sin);
		}
		/// <summary>
		/// Returns the inverse cosine value of the specified (radian) angle.
		/// </summary>
		/// <param name="cos">A radian angle.</param>
		public static double Acos(double cos)
		{
			return (double)System.Math.Acos(cos);
		}
		/// <summary>
		/// Returns the inverse tangent value of the specified (radian) angle.
		/// </summary>
		/// <param name="tan">A radian angle.</param>
		public static double Atan(double tan)
		{
			return (double)System.Math.Atan(tan);
		}
		/// <summary>
		/// Returns the (radian) angle whose tangent is the quotient of two specified numbers.
		/// </summary>
		/// <param name="y">The y coordinate of a point. </param>
		/// <param name="x">The x coordinate of a point. </param>
		public static double Atan2(double y, double x)
		{
			return (double)System.Math.Atan2(y, x);
		}

		/// <summary>
		/// Converts degrees  to radians.
		/// </summary>
		/// <param name="deg"></param>
		public static double DegToRad(double deg)
		{
			const double factor = (double)System.Math.PI / 180.0f;
			return deg * factor;
		}

		/// <summary>
		/// Converts degrees  to radians.
		/// </summary>
		/// <param name="deg"></param>
		public static Vector3 DegToRad(Vector3 deg)
		{
			deg.X = DegToRad(deg.X);
			deg.Y = DegToRad(deg.Y);
			deg.Z = DegToRad(deg.Z);
			return deg;
		}

		/// <summary>
		/// Converts radians to degrees.
		/// </summary>
		/// <param name="rad"></param>
		public static double RadToDeg(double rad)
		{
			const double factor = 180.0f / (double)System.Math.PI;
			return rad * factor;
		}

		/// <summary>
		/// Converts radians to degrees.
		/// </summary>
		/// <param name="rad"></param>
		public static Vector3 RadToDeg(Vector3 rad)
		{
			rad.X = RadToDeg(rad.X);
			rad.Y = RadToDeg(rad.Y);
			rad.Z = RadToDeg(rad.Z);
			return rad;
		}

		/// <summary>
		/// Normalizes a value to the given circular area.
		/// </summary>
		/// <returns>The normalized value between min (inclusive) and max (exclusive).</returns>
		/// <example>
		/// <c>NormalizeVar(480, 0, 360)</c> will return 120.
		/// </example>
		public static double NormalizeVar(double var, double min, double max)
		{
			if (var >= min && var < max) return var;

			if (var < min)
				var = max + ((var - min) % max);
			else
				var = min + var % (max - min);

			return var;
		}
		/// <summary>
		/// Normalizes a value to the given circular area.
		/// </summary>
		/// <returns>The normalized value between min (inclusive) and max (exclusive).</returns>
		/// <example>
		/// <c>NormalizeVar(480, 0, 360)</c> will return 120.
		/// </example>
		public static int NormalizeVar(int var, int min, int max)
		{
			if (var >= min && var < max) return var;

			if (var < min)
				var = max + ((var - min) % max);
			else
				var = min + var % (max - min);

			return var;
		}
		/// <summary>
		/// Normalizes a radian angle to values between zero and <see cref="TwoPi"/>.
		/// </summary>
		/// <returns>The normalized value between zero and <see cref="TwoPi"/>.</returns>
		/// <example>
		/// <c>NormalizeAngle(<see cref="TwoPi"/> + <see cref="Pi"/>)</c> will return <see cref="Pi"/>.
		/// </example>
		public static double NormalizeAngle(double var)
		{
			double normalized = var % RadAngle360;
			if (normalized < 0.0f)
				normalized += RadAngle360;
			return normalized;
		}

		/// <summary>
		/// Returns the distance between two points in 2d space.
		/// </summary>
		/// <param name="x1">The x-Coordinate of the first point.</param>
		/// <param name="y1">The y-Coordinate of the first point.</param>
		/// <param name="x2">The x-Coordinate of the second point.</param>
		/// <param name="y2">The y-Coordinate of the second point.</param>
		/// <returns>The distance between both points.</returns>
		public static double Distance(double x1, double y1, double x2, double y2)
		{
			return ((double)System.Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1)));
		}
		/// <summary>
		/// Returns the distance between a point and [0,0] in 2d space.
		/// </summary>
		/// <param name="x">The x-Coordinate of the point.</param>
		/// <param name="y">The y-Coordinate of the point.</param>
		/// <returns>The distance between the point and [0,0].</returns>
		public static double Distance(double x, double y)
		{
			return ((double)System.Math.Sqrt(x * x + y * y));
		}
		/// <summary>
		/// Returns the squared distance between two points in 2d space.
		/// </summary>
		/// <param name="x1">The x-Coordinate of the first point.</param>
		/// <param name="y1">The y-Coordinate of the first point.</param>
		/// <param name="x2">The x-Coordinate of the second point.</param>
		/// <param name="y2">The y-Coordinate of the second point.</param>
		/// <returns>The distance between both points.</returns>
		/// <remarks>
		/// This method is faster than <see cref="Distance(double,double,double,double)"/>. 
		/// If sufficient, such as for distance comparison, consider using this method instead.
		/// </remarks>
		public static double DistanceQuad(double x1, double y1, double x2, double y2)
		{
			return (x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1);
		}
		/// <summary>
		/// Returns the squared distance between a point and [0,0] in 2d space.
		/// </summary>
		/// <param name="x">The x-Coordinate of the point.</param>
		/// <param name="y">The y-Coordinate of the point.</param>
		/// <returns>The distance between the point and [0,0].</returns>
		/// <remarks>
		/// This method is faster than <see cref="Distance(double,double)"/>. 
		/// If sufficient, such as for distance comparison, consider using this method instead.
		/// </remarks>
		public static double DistanceQuad(double x, double y)
		{
			return x * x + y * y;
		}

		/// <summary>
		/// Calculates the angle between two points in 2D space.
		/// </summary>
		/// <param name="x1">The x-Coordinate of the first point.</param>
		/// <param name="y1">The y-Coordinate of the first point.</param>
		/// <param name="x2">The x-Coordinate of the second point.</param>
		/// <param name="y2">The y-Coordinate of the second point.</param>
		/// <returns>The angle between [x1,y1] and [x2,y2] in radians.</returns>
		public static double Angle(double x1, double y1, double x2, double y2)
		{
			return (double)((System.Math.Atan2((y2 - y1), (x2 - x1)) + PiOver2 + TwoPi) % TwoPi);
		}
		/// <summary>
		/// Calculates the angle from [0,0] to a specified point in 2D space.
		/// </summary>
		/// <param name="x">The x-Coordinate of the point.</param>
		/// <param name="y">The y-Coordinate of the point.</param>
		/// <returns>The angle between [0,0] and [x,y] in radians.</returns>
		public static double Angle(double x, double y)
		{
			return (double)((System.Math.Atan2(y, x) + PiOver2 + TwoPi) % TwoPi);
		}

		/// <summary>
		/// Assuming a circular value area, this method returns the direction to "turn" value 1 to
		/// when it comes to take the shortest way to value 2.
		/// </summary>
		/// <param name="val1">The first (source) value.</param>
		/// <param name="val2">The second (destination) value.</param>
		/// <param name="minVal">Minimum value.</param>
		/// <param name="maxVal">Maximum value.</param>
		/// <returns>-1 for "left" / lower, 1 for "right" / higher and 0 for "stay" / equal</returns>
		public static double TurnDir(double val1, double val2, double minVal, double maxVal)
		{
			val1 = MathF.NormalizeVar(val1, minVal, maxVal);
			val2 = MathF.NormalizeVar(val2, minVal, maxVal);
			if (val1 == val2) return 0.0f;

			if (System.Math.Abs(val1 - val2) > (maxVal - minVal) * 0.5f)
			{
				if (val1 > val2) return 1.0f;
				else return -1.0f;
			}
			else
			{
				if (val1 > val2) return -1.0f;
				else return 1.0f;
			}
		}
		/// <summary>
		/// Assuming a circular value area, this method returns the direction to "turn" value 1 to
		/// when it comes to take the shortest way to value 2.
		/// </summary>
		/// <param name="val1">The first (source) value.</param>
		/// <param name="val2">The second (destination) value.</param>
		/// <param name="minVal">Minimum value.</param>
		/// <param name="maxVal">Maximum value.</param>
		/// <returns>-1 for "left" / lower, 1 for "right" / higher and 0 for "stay" / equal</returns>
		public static int TurnDir(int val1, int val2, int minVal, int maxVal)
		{
			val1 = MathF.NormalizeVar(val1, minVal, maxVal);
			val2 = MathF.NormalizeVar(val2, minVal, maxVal);
			if (val1 == val2) return 0;

			if (System.Math.Abs(val1 - val2) > (maxVal - minVal) * 0.5f)
			{
				if (val1 > val2) return 1;
				else return -1;
			}
			else
			{
				if (val1 > val2) return -1;
				else return 1;
			}
		}
		/// <summary>
		/// Assuming an angular (radian) value area, this method returns the direction to "turn" value 1 to
		/// when it comes to take the shortest way to value 2.
		/// </summary>
		/// <param name="val1">The first (source) value.</param>
		/// <param name="val2">The second (destination) value.</param>
		/// <returns>-1 for "left" / lower, 1 for "right" / higher and 0 for "stay" / equal</returns>
		public static double TurnDir(double val1, double val2)
		{
			val1 = MathF.NormalizeAngle(val1);
			val2 = MathF.NormalizeAngle(val2);
			if (val1 == val2) return 0.0f;

			if (System.Math.Abs(val1 - val2) > RadAngle180)
			{
				if (val1 > val2) return 1.0f;
				else return -1.0f;
			}
			else
			{
				if (val1 > val2) return -1.0f;
				else return 1.0f;
			}
		}

		/// <summary>
		/// Calculates the distance between two values assuming a circular value area.
		/// </summary>
		/// <param name="v1">Value 1</param>
		/// <param name="v2">Value 2</param>
		/// <param name="vMin">Value area minimum</param>
		/// <param name="vMax">Value area maximum</param>
		/// <returns>Value distance</returns>
		public static double CircularDist(double v1, double v2, double vMin, double vMax)
		{
			double vTemp = System.Math.Abs(NormalizeVar(v1, vMin, vMax) - NormalizeVar(v2, vMin, vMax));
			if (vTemp * 2.0f <= vMax - vMin)
				return vTemp;
			else
				return (vMax - vMin) - vTemp;
		}
		/// <summary>
		/// Calculates the distance between two values assuming a circular value area.
		/// </summary>
		/// <param name="v1">Value 1</param>
		/// <param name="v2">Value 2</param>
		/// <param name="vMin">Value area minimum</param>
		/// <param name="vMax">Value area maximum</param>
		/// <returns>Value distance</returns>
		public static int CircularDist(int v1, int v2, int vMin, int vMax)
		{
			int vTemp = System.Math.Abs(NormalizeVar(v1, vMin, vMax) - NormalizeVar(v2, vMin, vMax));
			if (vTemp * 2 <= vMax - vMin)
				return vTemp;
			else
				return (vMax - vMin) - vTemp;
		}
		/// <summary>
		/// Calculates the distance between two angular (radian) values.
		/// </summary>
		/// <param name="v1">The first (radian) angle.</param>
		/// <param name="v2">The second (radian) angle.</param>
		/// <returns>The angular distance in radians between both angles.</returns>
		public static double CircularDist(double v1, double v2)
		{
			double diff = Math.Abs(v1 - v2) % RadAngle360;
			if (diff > RadAngle180)
				return RadAngle360 - diff;
			else
				return diff;
		}


		public static void TransformCoord(ref double xCoord, ref double yCoord, double rot, double scale)
		{
			double sin = (double)System.Math.Sin(rot);
			double cos = (double)System.Math.Cos(rot);
			double lastX = xCoord;
			xCoord = (xCoord * cos - yCoord * sin) * scale;
			yCoord = (lastX * sin + yCoord * cos) * scale;
		}
		public static void TransformCoord(ref double xCoord, ref double yCoord, double rot)
		{
			double sin = (double)System.Math.Sin(rot);
			double cos = (double)System.Math.Cos(rot);
			double lastX = xCoord;
			xCoord = xCoord * cos - yCoord * sin;
			yCoord = lastX * sin + yCoord * cos;
		}
		public static void GetTransformDotVec(double rot, double scale, out Vector2 xDot, out Vector2 yDot)
		{
			double sin = (double)System.Math.Sin(rot);
			double cos = (double)System.Math.Cos(rot);
			xDot = new Vector2(cos * scale, -sin * scale);
			yDot = new Vector2(sin * scale, cos * scale);
		}
		public static void GetTransformDotVec(double rot, Vector2 scale, out Vector2 xDot, out Vector2 yDot)
		{
			double sin = (double)System.Math.Sin(rot);
			double cos = (double)System.Math.Cos(rot);
			xDot = new Vector2(cos * scale.X, -sin * scale.X);
			yDot = new Vector2(sin * scale.Y, cos * scale.Y);
		}
		public static void GetTransformDotVec(double rot, out Vector2 xDot, out Vector2 yDot)
		{
			double sin = (double)System.Math.Sin(rot);
			double cos = (double)System.Math.Cos(rot);
			xDot = new Vector2(cos, -sin);
			yDot = new Vector2(sin, cos);
		}
		public static void TransformDotVec(ref Vector2 vec, ref Vector2 xDot, ref Vector2 yDot)
		{
			double oldX = vec.X;
			vec.X = vec.X * xDot.X + vec.Y * xDot.Y;
			vec.Y = oldX * yDot.X + vec.Y * yDot.Y;
		}
		public static void TransformDotVec(ref Vector3 vec, ref Vector2 xDot, ref Vector2 yDot)
		{
			double oldX = vec.X;
			vec.X = vec.X * xDot.X + vec.Y * xDot.Y;
			vec.Y = oldX * yDot.X + vec.Y * yDot.Y;
		}


		/// <summary>
		/// Checks, if two line segments (or infinite lines) cross and determines their mutual point.
		/// </summary>
		/// <param name="startX1">x-Coordinate of the first lines start.</param>
		/// <param name="startY1">y-Coordinate of the first lines start.</param>
		/// <param name="endX1">x-Coordinate of the first lines end.</param>
		/// <param name="endY1">y-Coordinate of the first lines end.</param>
		/// <param name="startX2">x-Coordinate of the second lines start.</param>
		/// <param name="startY2">y-Coordinate of the second lines start.</param>
		/// <param name="endX2">x-Coordinate of the second lines end.</param>
		/// <param name="endY2">y-Coordinate of the second lines end.</param>
		/// <param name="infinite">Whether the lines are considered infinite.</param>
		/// <param name="crossX">x-Coordiante at which both lines cross.</param>
		/// <param name="crossY">y-Coordinate at which both lines cross.</param>
		/// <returns>True, if the lines cross, false if not.</returns>
		public static bool LinesCross(
			double startX1, double startY1, double endX1, double endY1,
			double startX2, double startY2, double endX2, double endY2,
			out double crossX, out double crossY,
			bool infinite = false)
		{
			double n = (startY1 - startY2) * (endX2 - startX2) - (startX1 - startX2) * (endY2 - startY2);
			double d = (endX1 - startX1) * (endY2 - startY2) - (endY1 - startY1) * (endX2 - startX2);

			crossX = 0.0f;
			crossY = 0.0f;

			if (System.Math.Abs(d) < 0.0001)
				return false;
			else
			{
				double sn = (startY1 - startY2) * (endX1 - startX1) - (startX1 - startX2) * (endY1 - startY1);
				double ab = n / d;
				if (infinite)
				{
					crossX = startX1 + ab * (endX1 - startX1);
					crossY = startY1 + ab * (endY1 - startY1);
					return true;
				}
				else if (ab > 0.0 && ab < 1.0)
				{
					double cd = sn / d;
					if (cd > 0.0 && cd < 1.0)
					{
						crossX = startX1 + ab * (endX1 - startX1);
						crossY = startY1 + ab * (endY1 - startY1);
						return true;
					}
				}
			}

			return false;
		}
		/// <summary>
		/// Checks, if two line segments (or infinite lines) cross and determines their mutual point.
		/// </summary>
		/// <param name="startX1">x-Coordinate of the first lines start.</param>
		/// <param name="startY1">y-Coordinate of the first lines start.</param>
		/// <param name="endX1">x-Coordinate of the first lines end.</param>
		/// <param name="endY1">y-Coordinate of the first lines end.</param>
		/// <param name="startX2">x-Coordinate of the second lines start.</param>
		/// <param name="startY2">y-Coordinate of the second lines start.</param>
		/// <param name="endX2">x-Coordinate of the second lines end.</param>
		/// <param name="endY2">y-Coordinate of the second lines end.</param>
		/// <param name="infinite">Whether the lines are considered infinite.</param>
		/// <returns>True, if the lines cross, false if not.</returns>
		public static bool LinesCross(double startX1, double startY1, double endX1, double endY1, double startX2, double startY2, double endX2, double endY2, bool infinite = false)
		{
			return LinesCross(
				startX1, startY1, endX1, endY1,
				startX2, startY2, endX2, endY2,
				out double crossX, out double crossY,
				infinite);
		}

		/// <summary>
		/// Calculates the point on a line segment (or infinite line) that has the lowest possible
		/// distance to a point.
		/// </summary>
		/// <param name="pX">x-Coordinate of the point.</param>
		/// <param name="pY">y-Coordinate of the point.</param>
		/// <param name="lX1">x-Coordinate of the lines start.</param>
		/// <param name="lY1">y-Coordinate of the lines start.</param>
		/// <param name="lX2">x-Coordinate of the lines end.</param>
		/// <param name="lY2">y-Coordinate of the lines end.</param>
		/// <param name="infinite">Whether the line is considered infinite.</param>
		/// <returns>A point located on the specified line that is as close as possible to the specified point.</returns>
		public static Vector2 PointLineNearestPoint(
			double pX, double pY,
			double lX1, double lY1, double lX2, double lY2,
			bool infinite = false)
		{
			if (lX1 == lX2 && lY1 == lY2) return new Vector2(lX1, lY1);
			double sX = lX2 - lX1;
			double sY = lY2 - lY1;
			double q = ((pX - lX1) * sX + (pY - lY1) * sY) / (sX * sX + sY * sY);

			if (!infinite)
			{
				if (q < 0.0) q = 0.0f;
				if (q > 1.0) q = 1.0f;
			}

			return new Vector2((double)((1.0d - q) * lX1 + q * lX2), (double)((1.0d - q) * lY1 + q * lY2));
		}

		/// <summary>
		/// Calculates the distance between a point and a line segment (or infinite line).
		/// </summary>
		/// <param name="pX">x-Coordinate of the point.</param>
		/// <param name="pY">y-Coordinate of the point.</param>
		/// <param name="lX1">x-Coordinate of the lines start.</param>
		/// <param name="lY1">y-Coordinate of the lines start.</param>
		/// <param name="lX2">x-Coordinate of the lines end.</param>
		/// <param name="lY2">y-Coordinate of the lines end.</param>
		/// <param name="infinite">Whether the line is considered infinite.</param>
		/// <returns>The distance between point and line.</returns>
		public static double PointLineDistance(
			double pX, double pY,
			double lX1, double lY1, double lX2, double lY2,
			bool infinite = false)
		{
			Vector2 n = PointLineNearestPoint(pX, pY, lX1, lY1, lX2, lY2, infinite);
			return Distance(pX, pY, n.X, n.Y);
		}

		/// <summary>
		/// Returns whether or not the specified polygon is convex.
		/// </summary>
		/// <param name="vertices"></param>
		public static bool IsPolygonConvex(params Vector2[] vertices)
		{
			bool neg = false;
			bool pos = false;

			for (int a = 0; a < vertices.Length; a++)
			{
				int b = (a + 1) % vertices.Length;
				int c = (b + 1) % vertices.Length;
				Vector2 ab = vertices[b] - vertices[a];
				Vector2 bc = vertices[c] - vertices[b];

				if (ab == Vector2.Zero) return false;
				if (bc == Vector2.Zero) return false;

				double dot_product = Vector2.Dot(ab.PerpendicularLeft, bc);
				if (dot_product > 0.0f) pos = true;
				else if (dot_product < 0.0f) neg = true;

				if (neg && pos) return false;
			}

			return true;
		}
		
		/// <summary>
		/// Returns the next power of two that is larger than the specified number.
		/// </summary>
		/// <param name="n">The specified number.</param>
		/// <returns>The next power of two.</returns>
		public static int NextPowerOfTwo(int n)
		{
			if (n < 0) throw new ArgumentOutOfRangeException("n", "Must be positive.");
			return (int)System.Math.Pow(2, System.Math.Ceiling(System.Math.Log((double)n, 2)));
		}

		/// <summary>
		/// Swaps the values of two variables.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="first"></param>
		/// <param name="second"></param>
		public static void Swap<T>(ref T first, ref T second)
		{
			T temp = first;
			first = second;
			second = temp;
		}

		/// <summary>
		/// Combines two hash codes.
		/// </summary>
		/// <param name="baseHash"></param>
		/// <param name="otherHash"></param>
		public static void CombineHashCode(ref int baseHash, int otherHash)
		{
			unchecked { baseHash = baseHash * 23 + otherHash; }
		}
		/// <summary>
		/// Combines any number of hash codes.
		/// </summary>
		/// <param name="hashes"></param>
		public static int CombineHashCode(params int[] hashes)
		{
			int result = hashes[0];
			unchecked
			{
				for (int i = 1; i < hashes.Length; i++)
				{
					result = result * 23 + hashes[i];
				}
			}
			return result;
		}

		/// <summary>
		/// Throws an ArgumentOutOfRangeException, if the specified value is NaN or Infinity.
		/// </summary>
		/// <param name="value"></param>
		public static void CheckValidValue(double value)
		{
			if (double.IsNaN(value) || double.IsInfinity(value))
			{
				throw new ArgumentOutOfRangeException("value", string.Format("Invalid double value detected: {0}", value));
			}
		}
		/// <summary>
		/// Throws an ArgumentOutOfRangeException, if the specified value is NaN or Infinity.
		/// </summary>
		/// <param name="value"></param>
		public static void CheckValidValue(Vector2 value)
		{
			if (double.IsNaN(value.X) || double.IsInfinity(value.X) ||
				double.IsNaN(value.Y) || double.IsInfinity(value.Y))
			{
				throw new ArgumentOutOfRangeException("value", string.Format("Invalid double value detected: {0}", value));
			}
		}
		/// <summary>
		/// Throws an ArgumentOutOfRangeException, if the specified value is NaN or Infinity.
		/// </summary>
		/// <param name="value"></param>
		public static void CheckValidValue(Vector3 value)
		{
			if (double.IsNaN(value.X) || double.IsInfinity(value.X) ||
				double.IsNaN(value.Y) || double.IsInfinity(value.Y) ||
				double.IsNaN(value.Z) || double.IsInfinity(value.Z))
			{
				throw new ArgumentOutOfRangeException("value", string.Format("Invalid double value detected: {0}", value));
			}
		}
		/// <summary>
		/// Throws an ArgumentOutOfRangeException, if the specified value is NaN or Infinity.
		/// </summary>
		/// <param name="value"></param>
		public static void CheckValidValue(Quaternion value)
		{
			if (double.IsNaN(value.X) || double.IsInfinity(value.X) ||
				double.IsNaN(value.Y) || double.IsInfinity(value.Y) ||
				double.IsNaN(value.Z) || double.IsInfinity(value.Z) ||
				double.IsNaN(value.W) || double.IsInfinity(value.W))
			{
				throw new ArgumentOutOfRangeException("value", string.Format("Invalid double value detected: {0}", value));
			}
		}
	}
}
