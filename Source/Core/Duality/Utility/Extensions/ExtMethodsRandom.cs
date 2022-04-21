using System;
using System.Collections.Generic;
using System.Linq;
using Duality.Drawing;

namespace Duality
{
	/// <summary>
	/// Provides extension methods for <see cref="System.Random">random number generators</see>.
	/// </summary>
	public static class ExtMethodsRandom
	{
		/// <summary>
		/// Returns a random byte.
		/// </summary>
		/// <param name="r">A random number generator.</param>
		public static byte NextByte(this Random r)
		{
			return (byte)(r.Next() % 256);
		}
		/// <summary>
		/// Returns a random byte.
		/// </summary>
		/// <param name="r">A random number generator.</param>
		/// <param name="max">Exclusive maximum value.</param>
		public static byte NextByte(this Random r, byte max)
		{
			return (byte)(r.Next() % ((int)max + 1));
		}
		/// <summary>
		/// Returns a random byte.
		/// </summary>
		/// <param name="r">A random number generator.</param>
		/// <param name="min">Inclusive minimum value.</param>
		/// <param name="max">Exclusive maximum value.</param>
		public static byte NextByte(this Random r, byte min, byte max)
		{
			return (byte)(min + (r.Next() % ((int)max - min + 1)));
		}

		/// <summary>
		/// Returns a random double.
		/// </summary>
		/// <param name="r">A random number generator.</param>
		public static double NextDouble(this Random r)
		{
			return r.NextDouble();
		}
		/// <summary>
		/// Returns a random double.
		/// </summary>
		/// <param name="r">A random number generator.</param>
		/// <param name="max">Exclusive maximum value.</param>
		public static double NextDouble(this Random r, double max)
		{
			return max * r.NextDouble();
		}
		/// <summary>
		/// Returns a random double.
		/// </summary>
		/// <param name="r">A random number generator.</param>
		/// <param name="min">Inclusive minimum value.</param>
		/// <param name="max">Exclusive maximum value.</param>
		public static double NextDouble(this Random r, double min, double max)
		{
			return min + (max - min) * r.NextDouble();
		}
		public static void NextDouble(this Random r, ref Vector4 min, ref Vector4 max, out Vector4 result)
		{
			double x = r.NextDouble(min.X, max.X);
			double y = r.NextDouble(min.Y, max.Y);
			double z = r.NextDouble(min.Z, max.Z);
			double w = r.NextDouble(min.W, max.W);
			result = new Vector4(x, y, z, w);
		}
		public static void NextDouble(this Random r, ref Vector3 min, ref Vector3 max, out Vector3 result)
		{
			double x = r.NextDouble(min.X, max.X);
			double y = r.NextDouble(min.Y, max.Y);
			double z = r.NextDouble(min.Z, max.Z);
			result = new Vector3(x, y, z);
		}

		/// <summary>
		/// Returns a random bool.
		/// </summary>
		/// <param name="r">A random number generator.</param>
		public static bool NextBool(this Random r)
		{
			return r.NextDouble() > 0.5;
		}
		
		/// <summary>
		/// Returns a random <see cref="Vector2"/> with length one.
		/// </summary>
		/// <param name="r">A random number generator.</param>
		public static Vector2 NextVector2(this Random r)
		{
			double angle = r.NextDouble(0.0, MathF.RadAngle360);
			return new Vector2(MathF.Sin(angle), -MathF.Cos(angle));
		}
		/// <summary>
		/// Returns a random <see cref="Vector2"/>.
		/// </summary>
		/// <param name="r">A random number generator.</param>
		/// <param name="radius">Length of the vector.</param>
		public static Vector2 NextVector2(this Random r, double radius)
		{
			double angle = r.NextDouble(0.0, MathF.RadAngle360);
			return new Vector2(MathF.Sin(angle), -MathF.Cos(angle)) * radius;
		}
		/// <summary>
		/// Returns a random <see cref="Vector2"/>.
		/// </summary>
		/// <param name="r">A random number generator.</param>
		/// <param name="minRadius">Minimum length of the vector</param>
		/// <param name="maxRadius">Maximum length of the vector</param>
		public static Vector2 NextVector2(this Random r, double minRadius, double maxRadius)
		{
			return r.NextVector2(r.NextDouble(minRadius, maxRadius));
		}
		/// <summary>
		/// Returns a random <see cref="Vector2"/> pointing to a position inside the specified rect.
		/// </summary>
		/// <param name="r">A random number generator.</param>
		/// <param name="x">Rectangle that contains the random vector.</param>
		/// <param name="y">Rectangle that contains the random vector.</param>
		/// <param name="w">Rectangle that contains the random vector.</param>
		/// <param name="h">Rectangle that contains the random vector.</param>
		public static Vector2 NextVector2(this Random r, double x, double y, double w, double h)
		{
			return new Vector2(r.NextDouble(x, x + w), r.NextDouble(y, y + h));
		}
		/// <summary>
		/// Returns a random <see cref="Vector2"/> pointing to a position inside the specified rect.
		/// </summary>
		/// <param name="r">A random number generator.</param>
		/// <param name="rect">Rectangle that contains the random vector.</param>
		public static Vector2 NextVector2(this Random r, Rect rect)
		{
			return new Vector2(r.NextDouble(rect.X, rect.X + rect.W), r.NextDouble(rect.Y, rect.Y + rect.H));
		}
		
		/// <summary>
		/// Returns a random <see cref="Vector3"/> with length one.
		/// </summary>
		/// <param name="r">A random number generator.</param>
		public static Vector3 NextVector3(this Random r)
		{
			Quaternion rot = Quaternion.Identity;
			rot *= Quaternion.CreateFromAxisAngle(Vector3.UnitZ, r.NextDouble(MathF.RadAngle360));
			rot *= Quaternion.CreateFromAxisAngle(Vector3.UnitX, r.NextDouble(MathF.RadAngle360));
			rot *= Quaternion.CreateFromAxisAngle(Vector3.UnitY, r.NextDouble(MathF.RadAngle360));
			return Vector3.Transform(Vector3.UnitX, rot);
		}
		/// <summary>
		/// Returns a random <see cref="Vector3"/>.
		/// </summary>
		/// <param name="r">A random number generator.</param>
		/// <param name="radius">Maximum length of the vector.</param>
		public static Vector3 NextVector3(this Random r, double radius)
		{
			Quaternion rot = Quaternion.Identity;
			rot *= Quaternion.CreateFromAxisAngle(Vector3.UnitZ, r.NextDouble(MathF.RadAngle360));
			rot *= Quaternion.CreateFromAxisAngle(Vector3.UnitX, r.NextDouble(MathF.RadAngle360));
			rot *= Quaternion.CreateFromAxisAngle(Vector3.UnitY, r.NextDouble(MathF.RadAngle360));
			return Vector3.Transform(new Vector3(radius, 0, 0), rot);
		}
		/// <summary>
		/// Returns a random <see cref="Vector3"/>.
		/// </summary>
		/// <param name="r">A random number generator.</param>
		/// <param name="minRadius">Minimum length of the vector</param>
		/// <param name="maxRadius">Maximum length of the vector</param>
		public static Vector3 NextVector3(this Random r, double minRadius, double maxRadius)
		{
			return r.NextVector3(r.NextDouble(minRadius, maxRadius));
		}
		/// <summary>
		/// Returns a random <see cref="Vector3"/> pointing to a position inside the specified cube.
		/// </summary>
		/// <param name="r">A random number generator.</param>
		/// <param name="x">Cube that contains the random vector.</param>
		/// <param name="y">Cube that contains the random vector.</param>
		/// <param name="z"></param>
		/// <param name="w">Cube that contains the random vector.</param>
		/// <param name="h">Cube that contains the random vector.</param>
		/// <param name="d"></param>
		public static Vector3 NextVector3(this Random r, double x, double y, double z, double w, double h, double d)
		{
			return new Vector3(r.NextDouble(x, x + w), r.NextDouble(y, y + h), r.NextDouble(z, z + d));
		}

		/// <summary>
		/// Returns a random <see cref="ColorRgba"/> with full saturation and maximum brightness.
		/// </summary>
		/// <param name="r"></param>
		public static ColorRgba NextColorRgba(this Random r)
		{
			return NextColorHsva(r).ToRgba();
		}
		/// <summary>
		/// Returns a component-wise random <see cref="ColorRgba"/>.
		/// </summary>
		/// <param name="r"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		public static ColorRgba NextColorRgba(this Random r, ColorRgba min, ColorRgba max)
		{
			return new ColorRgba(
				r.NextByte(min.R, max.R),
				r.NextByte(min.G, max.G),
				r.NextByte(min.B, max.B),
				r.NextByte(min.A, max.A));
		}
		/// <summary>
		/// Returns a random <see cref="ColorHsva"/> with full saturation and maximum brightness.
		/// </summary>
		/// <param name="r"></param>
		public static ColorHsva NextColorHsva(this Random r)
		{
			return new ColorHsva((float)NextDouble(r), 1.0f, 1.0f, 1.0f);
		}
		/// <summary>
		/// Returns a component-wise random <see cref="ColorHsva"/>.
		/// </summary>
		/// <param name="r"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		public static ColorHsva NextColorHsva(this Random r, ColorHsva min, ColorHsva max)
		{
			return new ColorHsva(
				(float)r.NextDouble(min.H, max.H),
				(float)r.NextDouble(min.S, max.S),
				(float)r.NextDouble(min.V, max.V),
				(float)r.NextDouble(min.A, max.A));
		}

		/// <summary>
		/// Returns a random value from a weighted value pool.
		/// </summary>
		/// <typeparam name="T">Type of the random values.</typeparam>
		/// <param name="r">A random number generator.</param>
		/// <param name="values">A pool of values.</param>
		/// <param name="weights">One weight for each value in the pool.</param>
		public static T OneOfWeighted<T>(this Random r, IEnumerable<T> values, IEnumerable<double> weights)
		{
			double totalWeight = weights.Sum();
			double pickedWeight = r.NextDouble(totalWeight);
			
			IEnumerator<T> valEnum = values.GetEnumerator();
			if (!valEnum.MoveNext()) return default(T);

			foreach (double w in weights)
			{
				pickedWeight -= w;
				if (pickedWeight < 0.0f) return valEnum.Current;
				valEnum.MoveNext();
			}

			return default(T);
		}
		/// <summary>
		/// Returns a random value from a weighted value pool.
		/// </summary>
		/// <typeparam name="T">Type of the random values.</typeparam>
		/// <param name="r">A random number generator.</param>
		/// <param name="values">A pool of values.</param>
		/// <param name="weights">One weight for each value in the pool.</param>
		public static T OneOfWeighted<T>(this Random r, IEnumerable<T> values, params double[] weights)
		{
			return OneOfWeighted<T>(r, values, weights as IEnumerable<double>);
		}
		/// <summary>
		/// Returns a random value from a weighted value pool.
		/// </summary>
		/// <typeparam name="T">Type of the random values.</typeparam>
		/// <param name="r">A random number generator.</param>
		/// <param name="weightedValues">A weighted value pool.</param>
		public static T OneOfWeighted<T>(this Random r, IEnumerable<KeyValuePair<T, double>> weightedValues)
		{
			double totalWeight = weightedValues.Sum(v => v.Value);
			double pickedWeight = r.NextDouble(totalWeight);
			
			foreach (KeyValuePair<T, double> pair in weightedValues)
			{
				pickedWeight -= pair.Value;
				if (pickedWeight < 0.0f) return pair.Key;
			}

			return default(T);
		}
		/// <summary>
		/// Returns a random value from a weighted value pool.
		/// </summary>
		/// <typeparam name="T">Type of the random values.</typeparam>
		/// <param name="r">A random number generator.</param>
		/// <param name="weightedValues">A weighted value pool.</param>
		public static T OneOfWeighted<T>(this Random r, params KeyValuePair<T, double>[] weightedValues)
		{
			return OneOfWeighted<T>(r, weightedValues as IEnumerable<KeyValuePair<T, double>>);
		}
		/// <summary>
		/// Returns a random value from a weighted value pool.
		/// </summary>
		/// <typeparam name="T">Type of the random values.</typeparam>
		/// <param name="r">A random number generator.</param>
		/// <param name="values">A pool of values.</param>
		/// <param name="weightFunc">A weight function that provides a weight for each value from the pool.</param>
		public static T OneOfWeighted<T>(this Random r, IEnumerable<T> values, Func<T, double> weightFunc)
		{
			return OneOfWeighted<T>(r, values, values.Select(v => weightFunc(v)));
		}

		/// <summary>
		/// Returns one randomly selected element.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="r"></param>
		/// <param name="values"></param>
		public static T OneOf<T>(this Random r, IEnumerable<T> values)
		{
			return values.ElementAt(r.Next(values.Count()));
		}

		/// <summary>
		/// Shuffles the specified list of values.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="r"></param>
		/// <param name="values"></param>
		public static void Shuffle<T>(this Random r, IList<T> values)
		{
			// Fisher–Yates shuffle
			int range = values.Count;  
			while (range > 1)
			{  
				range--;
				int index = r.Next(range + 1);  
				T value = values[index];  
				values[index] = values[range];  
				values[range] = value;  
			}
		}
	}
}
