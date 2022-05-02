#region --- License ---
/*
Copyright (c) 2006 - 2008 The Open Toolkit library.

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
of the Software, and to permit persons to whom the Software is furnished to do
so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

Note: This code has been heavily modified for the Duality framework.

	*/
#endregion

using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Duality
{
	/// <summary>
	/// Represents a 2D vector using two single-precision floating-point numbers.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct Vector2Int : IEquatable<Vector2Int>
	{
		/// <summary>
		/// Defines a unit-length Vector2Int that points along the X-axis.
		/// </summary>
		public static readonly Vector2Int UnitX = new Vector2Int(1, 0);
		/// <summary>
		/// Defines a unit-length Vector2Int that points along the Y-axis.
		/// </summary>
		public static readonly Vector2Int UnitY = new Vector2Int(0, 1);
		/// <summary>
		/// Defines a zero-length Vector2Int.
		/// </summary>
		public static readonly Vector2Int Zero = new Vector2Int(0, 0);
		/// <summary>
		/// Defines an instance with all components set to 1.
		/// </summary>
		public static readonly Vector2Int One = new Vector2Int(1, 1);

		/// <summary>
		/// The X component of the Vector2Int.
		/// </summary>
		public int X;
		/// <summary>
		/// The Y component of the Vector2Int.
		/// </summary>
		public int Y;

		/// <summary>
		/// Constructs a new instance.
		/// </summary>
		/// <param name="value">The value that will initialize this instance.</param>
		public Vector2Int(int value)
		{
			this.X = value;
			this.Y = value;
		}
		/// <summary>
		/// Constructs a new Vector2.
		/// </summary>
		/// <param name="x">The x coordinate of the net Vector2Int.</param>
		/// <param name="y">The y coordinate of the net Vector2Int.</param>
		public Vector2Int(int x, int y)
		{
			this.X = x;
			this.Y = y;
		}
		/// <summary>
		/// Constructs a new Vector2.
		/// </summary>
		/// <param name="x">The x coordinate of the net Vector2Int (Will be casted to Int).</param>
		/// <param name="y">The y coordinate of the net Vector2Int (Will be casted to Int).</param>
		public Vector2Int(double x, double y)
		{
			this.X = (int)x;
			this.Y = (int)y;
		}
		/// <summary>
		/// Constructs a new vector from angle and length.
		/// </summary>
		/// <param name="angle"></param>
		/// <param name="length"></param>
		public static Vector2Int FromAngleLength(int angle, int length)
		{
			return new Vector2Int((int)Math.Sin(angle) * length, (int)Math.Cos(angle) * -length);
		}

		/// <summary>
		/// Gets the length (magnitude) of the vector.
		/// </summary>
		/// <seealso cref="LengthSquared"/>
		public double Length
		{
			get
			{
				return (double)System.Math.Sqrt(this.X * this.X + this.Y * this.Y);
			}
		}
		/// <summary>
		/// Gets the square of the vector length (magnitude).
		/// </summary>
		/// <remarks>
		/// This property avoids the costly square root operation required by the Length property. This makes it more suitable
		/// for comparisons.
		/// </remarks>
		/// <see cref="Length"/>
		public double LengthSquared
		{
			get
			{
				return this.X * this.X + this.Y * this.Y;
			}
		}

		/// <summary>
		/// Gets or sets the value at the index of the Vector.
		/// </summary>
		public int this[int index]
		{
			get
			{
				switch (index)
				{
					case 0: return this.X;
					case 1: return this.Y;
					default: throw new IndexOutOfRangeException("Vector2Int access at index: " + index);
				}
			}
			set
			{
				switch (index)
				{
					case 0: this.X = value; return;
					case 1: this.Y = value; return;
					default: throw new IndexOutOfRangeException("Vector2Int access at index: " + index);
				}
			}
		}

		/// <summary>
		/// Adds two vectors.
		/// </summary>
		/// <param name="a">Left operand.</param>
		/// <param name="b">Right operand.</param>
		/// <param name="result">Result of operation.</param>
		public static void Add(ref Vector2Int a, ref Vector2Int b, out Vector2Int result)
		{
			result = new Vector2Int(a.X + b.X, a.Y + b.Y);
		}
		/// <summary>
		/// Subtract one Vector from another
		/// </summary>
		/// <param name="a">First operand</param>
		/// <param name="b">Second operand</param>
		/// <param name="result">Result of subtraction</param>
		public static void Subtract(ref Vector2Int a, ref Vector2Int b, out Vector2Int result)
		{
			result = new Vector2Int(a.X - b.X, a.Y - b.Y);
		}
		/// <summary>
		/// Multiplies a vector by a scalar.
		/// </summary>
		/// <param name="vector">Left operand.</param>
		/// <param name="scale">Right operand.</param>
		/// <param name="result">Result of the operation.</param>
		public static void Multiply(ref Vector2Int vector, int scale, out Vector2Int result)
		{
			result = new Vector2Int(vector.X * scale, vector.Y * scale);
		}
		/// <summary>
		/// Multiplies a vector by the components of a vector (scale).
		/// </summary>
		/// <param name="vector">Left operand.</param>
		/// <param name="scale">Right operand.</param>
		/// <param name="result">Result of the operation.</param>
		public static void Multiply(ref Vector2Int vector, ref Vector2Int scale, out Vector2Int result)
		{
			result = new Vector2Int(vector.X * scale.X, vector.Y * scale.Y);
		}
		/// <summary>
		/// Divide a vector by the components of a vector (scale).
		/// </summary>
		/// <param name="vector">Left operand.</param>
		/// <param name="scale">Right operand.</param>
		/// <param name="result">Result of the operation.</param>
		public static void Divide(ref Vector2Int vector, ref Vector2Int scale, out Vector2Int result)
		{
			result = new Vector2Int(vector.X / scale.X, vector.Y / scale.Y);
		}

		/// <summary>
		/// Calculate the component-wise minimum of two vectors
		/// </summary>
		/// <param name="a">First operand</param>
		/// <param name="b">Second operand</param>
		/// <returns>The component-wise minimum</returns>
		public static Vector2Int Min(Vector2Int a, Vector2Int b)
		{
			a.X = a.X < b.X ? a.X : b.X;
			a.Y = a.Y < b.Y ? a.Y : b.Y;
			return a;
		}
		/// <summary>
		/// Calculate the component-wise minimum of two vectors
		/// </summary>
		/// <param name="a">First operand</param>
		/// <param name="b">Second operand</param>
		/// <param name="result">The component-wise minimum</param>
		public static void Min(ref Vector2Int a, ref Vector2Int b, out Vector2Int result)
		{
			result.X = a.X < b.X ? a.X : b.X;
			result.Y = a.Y < b.Y ? a.Y : b.Y;
		}

		/// <summary>
		/// Calculate the component-wise maximum of two vectors
		/// </summary>
		/// <param name="a">First operand</param>
		/// <param name="b">Second operand</param>
		/// <returns>The component-wise maximum</returns>
		public static Vector2Int Max(Vector2Int a, Vector2Int b)
		{
			a.X = a.X > b.X ? a.X : b.X;
			a.Y = a.Y > b.Y ? a.Y : b.Y;
			return a;
		}
		/// <summary>
		/// Calculate the component-wise maximum of two vectors
		/// </summary>
		/// <param name="a">First operand</param>
		/// <param name="b">Second operand</param>
		/// <param name="result">The component-wise maximum</param>
		public static void Max(ref Vector2Int a, ref Vector2Int b, out Vector2Int result)
		{
			result.X = a.X > b.X ? a.X : b.X;
			result.Y = a.Y > b.Y ? a.Y : b.Y;
		}

		/// <summary>
		/// Calculate the dot (scalar) product of two vectors
		/// </summary>
		/// <param name="left">First operand</param>
		/// <param name="right">Second operand</param>
		/// <returns>The dot product of the two inputs</returns>
		public static double Dot(Vector2Int left, Vector2Int right)
		{
			return left.X * right.X + left.Y * right.Y;
		}
		/// <summary>
		/// Calculate the dot (scalar) product of two vectors
		/// </summary>
		/// <param name="left">First operand</param>
		/// <param name="right">Second operand</param>
		/// <param name="result">The dot product of the two inputs</param>
		public static void Dot(ref Vector2Int left, ref Vector2Int right, out double result)
		{
			result = left.X * right.X + left.Y * right.Y;
		}

		/// <summary>
		/// Returns a new Vector that is the linear blend of the 2 given Vectors
		/// </summary>
		/// <param name="a">First input vector</param>
		/// <param name="b">Second input vector</param>
		/// <param name="blend">The blend factor. a when blend=0, b when blend=1.</param>
		/// <returns>a when blend=0, b when blend=1, and a linear combination otherwise</returns>
		public static Vector2Int Lerp(Vector2Int a, Vector2Int b, int blend)
		{
			a.X = blend * (b.X - a.X) + a.X;
			a.Y = blend * (b.Y - a.Y) + a.Y;
			return a;
		}
		/// <summary>
		/// Returns a new Vector that is the linear blend of the 2 given Vectors
		/// </summary>
		/// <param name="a">First input vector</param>
		/// <param name="b">Second input vector</param>
		/// <param name="blend">The blend factor. a when blend=0, b when blend=1.</param>
		/// <param name="result">a when blend=0, b when blend=1, and a linear combination otherwise</param>
		public static void Lerp(ref Vector2Int a, ref Vector2Int b, int blend, out Vector2Int result)
		{
			result.X = blend * (b.X - a.X) + a.X;
			result.Y = blend * (b.Y - a.Y) + a.Y;
		}

		/// <summary>
		/// Adds the specified instances.
		/// </summary>
		/// <param name="left">Left operand.</param>
		/// <param name="right">Right operand.</param>
		/// <returns>Result of addition.</returns>
		public static Vector2Int operator +(Vector2Int left, Vector2Int right)
		{
			return new Vector2Int(
				left.X + right.X, 
				left.Y + right.Y);
		}
		/// <summary>
		/// Subtracts the specified instances.
		/// </summary>
		/// <param name="left">Left operand.</param>
		/// <param name="right">Right operand.</param>
		/// <returns>Result of subtraction.</returns>
		public static Vector2Int operator -(Vector2Int left, Vector2Int right)
		{
			return new Vector2Int(
				left.X - right.X, 
				left.Y - right.Y);
		}
		/// <summary>
		/// Negates the specified instance.
		/// </summary>
		/// <param name="vec">Operand.</param>
		/// <returns>Result of negation.</returns>
		public static Vector2Int operator -(Vector2Int vec)
		{
			return new Vector2Int(
				-vec.X, 
				-vec.Y);
		}
		/// <summary>
		/// Multiplies the specified instance by a scalar.
		/// </summary>
		/// <param name="vec">Left operand.</param>
		/// <param name="scale">Right operand.</param>
		/// <returns>Result of multiplication.</returns>
		public static Vector2Int operator *(Vector2Int vec, double scale)
		{
			return new Vector2Int(
				vec.X * scale, 
				vec.Y * scale);
		}
		/// <summary>
		/// Multiplies the specified instance by a scalar.
		/// </summary>
		/// <param name="scale">Left operand.</param>
		/// <param name="vec">Right operand.</param>
		/// <returns>Result of multiplication.</returns>
		public static Vector2Int operator *(double scale, Vector2Int vec)
		{
			return vec * scale;
		}
		/// <summary>
		/// Scales the specified instance by a vector.
		/// </summary>
		/// <param name="vec">Left operand.</param>
		/// <param name="scale">Right operand.</param>
		/// <returns>Result of multiplication.</returns>
		public static Vector2Int operator *(Vector2Int vec, Vector2Int scale)
		{
			return new Vector2Int(
				vec.X * scale.X, 
				vec.Y * scale.Y);
		}
		/// <summary>
		/// Divides the specified instance by a scalar.
		/// </summary>
		/// <param name="vec">Left operand</param>
		/// <param name="scale">Right operand</param>
		/// <returns>Result of the division.</returns>
		public static Vector2Int operator /(Vector2Int vec, double scale)
		{
			return vec * (1.0f / scale);
		}
		/// <summary>
		/// Divides the specified instance by a vector.
		/// </summary>
		/// <param name="vec">Left operand</param>
		/// <param name="scale">Right operand</param>
		/// <returns>Result of the division.</returns>
		public static Vector2Int operator /(Vector2Int vec, Vector2Int scale)
		{
			return new Vector2Int(
				vec.X / scale.X, 
				vec.Y / scale.Y);
		}
		/// <summary>
		/// Compares the specified instances for equality.
		/// </summary>
		/// <param name="left">Left operand.</param>
		/// <param name="right">Right operand.</param>
		/// <returns>True if both instances are equal; false otherwise.</returns>
		public static bool operator ==(Vector2Int left, Vector2Int right)
		{
			return left.Equals(right);
		}
		/// <summary>
		/// Compares the specified instances for inequality.
		/// </summary>
		/// <param name="left">Left operand.</param>
		/// <param name="right">Right operand.</param>
		/// <returns>True if both instances are not equal; false otherwise.</returns>
		public static bool operator !=(Vector2Int left, Vector2Int right)
		{
			return !left.Equals(right);
		}

		public static implicit operator Vector2(Vector2Int s)
		{
			return new Vector2(s.X, s.Y);
		}

		public static implicit operator Vector2Int(Vector2 s)
		{
			return new Vector2Int(s.X, s.Y);
		}

		/// <summary>
		/// Returns a System.String that represents the current Vector2.
		/// </summary>
		public override string ToString()
		{
			return string.Format("({0:F}, {1:F})", this.X, this.Y);
		}
		/// <summary>
		/// Returns the hashcode for this instance.
		/// </summary>
		/// <returns>A System.Int32 containing the unique hashcode for this instance.</returns>
		public override int GetHashCode()
		{
			return this.X.GetHashCode() ^ this.Y.GetHashCode();
		}
		/// <summary>
		/// Indicates whether this instance and a specified object are equal.
		/// </summary>
		/// <param name="obj">The object to compare to.</param>
		/// <returns>True if the instances are equal; false otherwise.</returns>
		public override bool Equals(object obj)
		{
			if (!(obj is Vector2Int))
				return false;

			return this.Equals((Vector2Int)obj);
		}

		/// <summary>
		/// Indicates whether the current vector is equal to another vector.
		/// </summary>
		/// <param name="other">A vector to compare with this vector.</param>
		/// <returns>true if the current vector is equal to the vector parameter; otherwise, false.</returns>
		public bool Equals(Vector2Int other)
		{
			return
				this.X == other.X &&
				this.Y == other.Y;
		}
	}
}
