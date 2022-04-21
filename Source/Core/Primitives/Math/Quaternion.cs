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
	*/
#endregion

using System;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace Duality
{
	/// <summary>
	/// Represents a Quaternion.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct Quaternion : IEquatable<Quaternion>
	{
		#region Private Fields

		private static readonly Quaternion _identity = new Quaternion(0, 0, 0, 1);

		#endregion

		#region Public Fields

		/// <summary>
		/// The x coordinate of this <see cref="Quaternion"/>.
		/// </summary>
		public double X;

		/// <summary>
		/// The y coordinate of this <see cref="Quaternion"/>.
		/// </summary>
		public double Y;

		/// <summary>
		/// The z coordinate of this <see cref="Quaternion"/>.
		/// </summary>
		public double Z;

		/// <summary>
		/// The rotation component of this <see cref="Quaternion"/>.
		/// </summary>
		public double W;

		public Vector3 Xyz { get { return new Vector3(this.X, this.Y, this.Z); } set { this.X = value.X; this.Y = value.Y; this.Z = value.Z; } }

		#endregion

		#region Constructors

		/// <summary>
		/// Constructs a quaternion with X, Y, Z and W from four values.
		/// </summary>
		/// <param name="x">The x coordinate in 3d-space.</param>
		/// <param name="y">The y coordinate in 3d-space.</param>
		/// <param name="z">The z coordinate in 3d-space.</param>
		/// <param name="w">The rotation component.</param>
		public Quaternion(double x, double y, double z, double w)
		{
			this.X = x;
			this.Y = y;
			this.Z = z;
			this.W = w;
		}

		/// <summary>
		/// Constructs a quaternion with X, Y, Z from <see cref="Vector3"/> and rotation component from a scalar.
		/// </summary>
		/// <param name="value">The x, y, z coordinates in 3d-space.</param>
		/// <param name="w">The rotation component.</param>
		public Quaternion(Vector3 value, double w)
		{
			this.X = value.X;
			this.Y = value.Y;
			this.Z = value.Z;
			this.W = w;
		}

		/// <summary>
		/// Constructs a quaternion from <see cref="Vector4"/>.
		/// </summary>
		/// <param name="value">The x, y, z coordinates in 3d-space and the rotation component.</param>
		public Quaternion(Vector4 value)
		{
			this.X = value.X;
			this.Y = value.Y;
			this.Z = value.Z;
			this.W = value.W;
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Returns a quaternion representing no rotation.
		/// </summary>
		public static Quaternion Identity
		{
			get { return _identity; }
		}

		/// <summary>
		/// 
		/// </summary>
		public Vector3 EulerAngles
		{
			get
			{ 
				return -ToEuler(this);
			}
			set
			{
				//this = CreateFromYawPitchRoll(value.Y, value.X, value.Z);
				this = Euler(-value);
			}
		}

		#endregion

		#region Internal Properties

		internal string DebugDisplayString
		{
			get
			{
				if (this == Quaternion._identity)
				{
					return "Identity";
				}

				return string.Concat(
					this.X.ToString(), " ",
					this.Y.ToString(), " ",
					this.Z.ToString(), " ",
					this.W.ToString()
				);
			}
		}

		#endregion

		#region Public Methods

		#region Euler

		public static double ArcTanAngle(double X, double Y)
		{
			if (X == 0)
			{
				if (Y == 1)
					return (double)MathF.PiOver2;
				else
					return (double)-MathF.PiOver2;
			}
			else if (X > 0)
				return (double)Math.Atan(Y / X);
			else if (X < 0)
			{
				if (Y > 0)
					return (double)Math.Atan(Y / X) + MathF.Pi;
				else
					return (double)Math.Atan(Y / X) - MathF.Pi;
			}
			else
				return 0;
		}

		//returns Euler angles that point from one point to another
		public static Vector3 AngleTo(Vector3 from, Vector3 location)
		{
			Vector3 angle = new Vector3();
			Vector3 v3 = Vector3.Normalize(location - from);
			angle.X = (double)Math.Asin(v3.Y);
			angle.Y = ArcTanAngle(-v3.Z, -v3.X);
			return angle;
		}

		public static void ToEuler(double x, double y, double z, double w, ref Vector3 result)
		{
			var rotation = new Quaternion(x, y, z, w);
			var forward = Vector3.Transform(Vector3.Forward, rotation);
			var up = Vector3.Transform(Vector3.Up, rotation);
			result = AngleTo(new Vector3(), forward);
			if (result.X == MathF.PiOver2)
			{
				result.Y = ArcTanAngle(up.Z, up.X);
				result.Z = 0;
			}
			else if (result.X == -MathF.PiOver2)
			{
				result.Y = ArcTanAngle(-up.Z, -up.X);
				result.Z = 0;
			}
			else
			{
				up = Vector3.Transform(up, Matrix4.CreateRotationY(-result.Y));
				up = Vector3.Transform(up, Matrix4.CreateRotationX(-result.X));
				result.Z = ArcTanAngle(up.Y, -up.X);
			}
		}

		public static void ToEuler(Quaternion quaternion, ref Vector3 result)
		{
			ToEuler(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W, ref result);
		}

		public static Vector3 ToEuler(Quaternion quaternion)
		{
			Vector3 result = Vector3.Zero;
			ToEuler(quaternion, ref result);
			return result;
		}

		public static Quaternion Euler(double x, double y, double z)
		{
			return Quaternion.CreateFromYawPitchRoll(y, x, z);
		}

		public static Quaternion Euler(Vector3 rotation)
		{
			return Euler(rotation.X, rotation.Y, rotation.Z);
		}
		#endregion

		#region Add

		/// <summary>
		/// Creates a new <see cref="Quaternion"/> that contains the sum of two quaternions.
		/// </summary>
		/// <param name="quaternion1">Source <see cref="Quaternion"/>.</param>
		/// <param name="quaternion2">Source <see cref="Quaternion"/>.</param>
		/// <returns>The result of the quaternion addition.</returns>
		public static Quaternion Add(Quaternion quaternion1, Quaternion quaternion2)
		{
			Quaternion quaternion;
			quaternion.X = quaternion1.X + quaternion2.X;
			quaternion.Y = quaternion1.Y + quaternion2.Y;
			quaternion.Z = quaternion1.Z + quaternion2.Z;
			quaternion.W = quaternion1.W + quaternion2.W;
			return quaternion;
		}

		/// <summary>
		/// Creates a new <see cref="Quaternion"/> that contains the sum of two quaternions.
		/// </summary>
		/// <param name="quaternion1">Source <see cref="Quaternion"/>.</param>
		/// <param name="quaternion2">Source <see cref="Quaternion"/>.</param>
		/// <param name="result">The result of the quaternion addition as an output parameter.</param>
		public static void Add(ref Quaternion quaternion1, ref Quaternion quaternion2, out Quaternion result)
		{
			result.X = quaternion1.X + quaternion2.X;
			result.Y = quaternion1.Y + quaternion2.Y;
			result.Z = quaternion1.Z + quaternion2.Z;
			result.W = quaternion1.W + quaternion2.W;
		}

		#endregion

		#region Concatenate

		/// <summary>
		/// Creates a new <see cref="Quaternion"/> that contains concatenation between two quaternion.
		/// </summary>
		/// <param name="value1">The first <see cref="Quaternion"/> to concatenate.</param>
		/// <param name="value2">The second <see cref="Quaternion"/> to concatenate.</param>
		/// <returns>The result of rotation of <paramref name="value1"/> followed by <paramref name="value2"/> rotation.</returns>
		public static Quaternion Concatenate(Quaternion value1, Quaternion value2)
		{
			Quaternion quaternion;

			double x1 = value1.X;
			double y1 = value1.Y;
			double z1 = value1.Z;
			double w1 = value1.W;

			double x2 = value2.X;
			double y2 = value2.Y;
			double z2 = value2.Z;
			double w2 = value2.W;

			quaternion.X = ((x2 * w1) + (x1 * w2)) + ((y2 * z1) - (z2 * y1));
			quaternion.Y = ((y2 * w1) + (y1 * w2)) + ((z2 * x1) - (x2 * z1));
			quaternion.Z = ((z2 * w1) + (z1 * w2)) + ((x2 * y1) - (y2 * x1));
			quaternion.W = (w2 * w1) - (((x2 * x1) + (y2 * y1)) + (z2 * z1));

			return quaternion;
		}

		/// <summary>
		/// Creates a new <see cref="Quaternion"/> that contains concatenation between two quaternion.
		/// </summary>
		/// <param name="value1">The first <see cref="Quaternion"/> to concatenate.</param>
		/// <param name="value2">The second <see cref="Quaternion"/> to concatenate.</param>
		/// <param name="result">The result of rotation of <paramref name="value1"/> followed by <paramref name="value2"/> rotation as an output parameter.</param>
		public static void Concatenate(ref Quaternion value1, ref Quaternion value2, out Quaternion result)
		{
			double x1 = value1.X;
			double y1 = value1.Y;
			double z1 = value1.Z;
			double w1 = value1.W;

			double x2 = value2.X;
			double y2 = value2.Y;
			double z2 = value2.Z;
			double w2 = value2.W;

			result.X = ((x2 * w1) + (x1 * w2)) + ((y2 * z1) - (z2 * y1));
			result.Y = ((y2 * w1) + (y1 * w2)) + ((z2 * x1) - (x2 * z1));
			result.Z = ((z2 * w1) + (z1 * w2)) + ((x2 * y1) - (y2 * x1));
			result.W = (w2 * w1) - (((x2 * x1) + (y2 * y1)) + (z2 * z1));
		}

		#endregion

		#region Conjugate

		/// <summary>
		/// Transforms this quaternion into its conjugated version.
		/// </summary>
		public void Conjugate()
		{
			this.X = -this.X;
			this.Y = -this.Y;
			this.Z = -this.Z;
		}

		/// <summary>
		/// Creates a new <see cref="Quaternion"/> that contains conjugated version of the specified quaternion.
		/// </summary>
		/// <param name="value">The quaternion which values will be used to create the conjugated version.</param>
		/// <returns>The conjugate version of the specified quaternion.</returns>
		public static Quaternion Conjugate(Quaternion value)
		{
			return new Quaternion(-value.X, -value.Y, -value.Z, value.W);
		}

		/// <summary>
		/// Creates a new <see cref="Quaternion"/> that contains conjugated version of the specified quaternion.
		/// </summary>
		/// <param name="value">The quaternion which values will be used to create the conjugated version.</param>
		/// <param name="result">The conjugated version of the specified quaternion as an output parameter.</param>
		public static void Conjugate(ref Quaternion value, out Quaternion result)
		{
			result.X = -value.X;
			result.Y = -value.Y;
			result.Z = -value.Z;
			result.W = value.W;
		}

		#endregion

		#region CreateFromAxisAngle

		/// <summary>
		/// Convert the current quaternion to axis angle representation
		/// Creates a new <see cref="Quaternion"/> from the specified axis and angle.
		/// </summary>
		/// <param name="axis">The resultant axis</param>
		/// <param name="angle">The resultant angle</param>
		public void ToAxisAngle(out Vector3 axis, out double angle)
		{
			Vector4 result = this.ToAxisAngle();
			axis = result.Xyz;
			angle = result.W;
		}

		/// <summary>
		/// Convert this instance to an axis-angle representation.
		/// </summary>
		/// <returns>A Vector4 that is the axis-angle representation of this quaternion.</returns>
		public Vector4 ToAxisAngle()
		{
			Quaternion q = this;
			if (Math.Abs(q.W) > 1.0f)
				q.Normalize();
			Vector4 result = new Vector4();
			result.W = 2.0 * (double)System.Math.Acos(q.W); // angle
			double den = (double)System.Math.Sqrt(1.0 - q.W * q.W);
			if (den > 0.0001f)
			{
				result.Xyz = q.Xyz / den;
			}
			else
			{
				// This occurs when the angle is zero. 
				// Not a problem: just set an arbitrary normalized axis.
				result.Xyz = Vector3.UnitX;
			}
			return result;
		}

		/// <summary>
		/// Creates a new <see cref="Quaternion"/> from the specified axis and angle.
		/// </summary>
		/// <param name="axis">The axis of rotation.</param>
		/// <param name="angle">The angle in radians.</param>
		/// <param name="result">The new quaternion builded from axis and angle as an output parameter.</param>
		public static void CreateFromAxisAngle(ref Vector3 axis, double angle, out Quaternion result)
		{
			double half = angle * 0.5;
			double sin = MathF.Sin(half);
			double cos = MathF.Cos(half);
			result.X = axis.X * sin;
			result.Y = axis.Y * sin;
			result.Z = axis.Z * sin;
			result.W = cos;
		}

		/// <summary>
		/// Creates a new <see cref="Quaternion"/> from the specified axis and angle.
		/// </summary>
		/// <param name="axis">The axis of rotation.</param>
		/// <param name="angle">The angle in radians.</param>
		public static Quaternion CreateFromAxisAngle(Vector3 axis, double angle)
		{
			double half = angle * 0.5;
			double sin = MathF.Sin(half);
			double cos = MathF.Cos(half);
			return new Quaternion(axis.X * sin, axis.Y * sin, axis.Z * sin, cos);
		}

		#endregion

		#region CreateFromRotationMatrix

		/// <summary>
		/// Creates a new <see cref="Quaternion"/> from the specified <see cref="Matrix4"/>.
		/// </summary>
		/// <param name="matrix">The rotation matrix.</param>
		/// <returns>A quaternion composed from the rotation part of the matrix.</returns>
		public static Quaternion CreateFromRotationMatrix(Matrix4 matrix)
		{
			Quaternion quaternion;
			double sqrt;
			double half;
			double scale = matrix.M11 + matrix.M22 + matrix.M33;

			if (scale > 0.0f)
			{
				sqrt = MathF.Sqrt(scale + 1.0f);
				quaternion.W = sqrt * 0.5;
				sqrt = 0.5 / sqrt;

				quaternion.X = (matrix.M23 - matrix.M32) * sqrt;
				quaternion.Y = (matrix.M31 - matrix.M13) * sqrt;
				quaternion.Z = (matrix.M12 - matrix.M21) * sqrt;

				return quaternion;
			}
			if ((matrix.M11 >= matrix.M22) && (matrix.M11 >= matrix.M33))
			{
				sqrt = MathF.Sqrt(1.0 + matrix.M11 - matrix.M22 - matrix.M33);
				half = 0.5 / sqrt;

				quaternion.X = 0.5 * sqrt;
				quaternion.Y = (matrix.M12 + matrix.M21) * half;
				quaternion.Z = (matrix.M13 + matrix.M31) * half;
				quaternion.W = (matrix.M23 - matrix.M32) * half;

				return quaternion;
			}
			if (matrix.M22 > matrix.M33)
			{
				sqrt = MathF.Sqrt(1.0 + matrix.M22 - matrix.M11 - matrix.M33);
				half = 0.5 / sqrt;

				quaternion.X = (matrix.M21 + matrix.M12) * half;
				quaternion.Y = 0.5 * sqrt;
				quaternion.Z = (matrix.M32 + matrix.M23) * half;
				quaternion.W = (matrix.M31 - matrix.M13) * half;

				return quaternion;
			}
			sqrt = MathF.Sqrt(1.0 + matrix.M33 - matrix.M11 - matrix.M22);
			half = 0.5 / sqrt;

			quaternion.X = (matrix.M31 + matrix.M13) * half;
			quaternion.Y = (matrix.M32 + matrix.M23) * half;
			quaternion.Z = 0.5 * sqrt;
			quaternion.W = (matrix.M12 - matrix.M21) * half;

			return quaternion;
		}

		/// <summary>
		/// Creates a new <see cref="Quaternion"/> from the specified <see cref="Matrix4"/>.
		/// </summary>
		/// <param name="matrix">The rotation matrix.</param>
		/// <param name="result">A quaternion composed from the rotation part of the matrix as an output parameter.</param>
		public static void CreateFromRotationMatrix(ref Matrix4 matrix, out Quaternion result)
		{
			double sqrt;
			double half;
			double scale = matrix.M11 + matrix.M22 + matrix.M33;

			if (scale > 0.0f)
			{
				sqrt = MathF.Sqrt(scale + 1.0f);
				result.W = sqrt * 0.5;
				sqrt = 0.5 / sqrt;

				result.X = (matrix.M23 - matrix.M32) * sqrt;
				result.Y = (matrix.M31 - matrix.M13) * sqrt;
				result.Z = (matrix.M12 - matrix.M21) * sqrt;
			}
			else
			if ((matrix.M11 >= matrix.M22) && (matrix.M11 >= matrix.M33))
			{
				sqrt = MathF.Sqrt(1.0 + matrix.M11 - matrix.M22 - matrix.M33);
				half = 0.5 / sqrt;

				result.X = 0.5 * sqrt;
				result.Y = (matrix.M12 + matrix.M21) * half;
				result.Z = (matrix.M13 + matrix.M31) * half;
				result.W = (matrix.M23 - matrix.M32) * half;
			}
			else if (matrix.M22 > matrix.M33)
			{
				sqrt = MathF.Sqrt(1.0 + matrix.M22 - matrix.M11 - matrix.M33);
				half = 0.5 / sqrt;

				result.X = (matrix.M21 + matrix.M12) * half;
				result.Y = 0.5 * sqrt;
				result.Z = (matrix.M32 + matrix.M23) * half;
				result.W = (matrix.M31 - matrix.M13) * half;
			}
			else
			{
				sqrt = MathF.Sqrt(1.0 + matrix.M33 - matrix.M11 - matrix.M22);
				half = 0.5 / sqrt;

				result.X = (matrix.M31 + matrix.M13) * half;
				result.Y = (matrix.M32 + matrix.M23) * half;
				result.Z = 0.5 * sqrt;
				result.W = (matrix.M12 - matrix.M21) * half;
			}
		}

		#endregion

		#region CreateFromYawPitchRoll

		/// <summary>
		/// Creates a new <see cref="Quaternion"/> from the specified yaw, pitch and roll angles.
		/// </summary>
		/// <param name="yaw">Yaw around the y axis in radians.</param>
		/// <param name="pitch">Pitch around the x axis in radians.</param>
		/// <param name="roll">Roll around the z axis in radians.</param>
		/// <returns>A new quaternion from the concatenated yaw, pitch, and roll angles.</returns>
		public static Quaternion CreateFromYawPitchRoll(double yaw, double pitch, double roll)
		{
			double halfRoll = roll * 0.5;
			double halfPitch = pitch * 0.5;
			double halfYaw = yaw * 0.5;

			double sinRoll = MathF.Sin(halfRoll);
			double cosRoll = MathF.Cos(halfRoll);
			double sinPitch = MathF.Sin(halfPitch);
			double cosPitch = MathF.Cos(halfPitch);
			double sinYaw = MathF.Sin(halfYaw);
			double cosYaw = MathF.Cos(halfYaw);

			return new Quaternion((cosYaw * sinPitch * cosRoll) + (sinYaw * cosPitch * sinRoll),
								  (sinYaw * cosPitch * cosRoll) - (cosYaw * sinPitch * sinRoll),
								  (cosYaw * cosPitch * sinRoll) - (sinYaw * sinPitch * cosRoll),
								  (cosYaw * cosPitch * cosRoll) + (sinYaw * sinPitch * sinRoll));
		}

		/// <summary>
		/// Creates a new <see cref="Quaternion"/> from the specified yaw, pitch and roll angles.
		/// </summary>
		/// <param name="yaw">Yaw around the y axis in radians.</param>
		/// <param name="pitch">Pitch around the x axis in radians.</param>
		/// <param name="roll">Roll around the z axis in radians.</param>
		/// <param name="result">A new quaternion from the concatenated yaw, pitch, and roll angles as an output parameter.</param>
		public static void CreateFromYawPitchRoll(double yaw, double pitch, double roll, out Quaternion result)
		{
			double halfRoll = roll * 0.5;
			double halfPitch = pitch * 0.5;
			double halfYaw = yaw * 0.5;

			double sinRoll = MathF.Sin(halfRoll);
			double cosRoll = MathF.Cos(halfRoll);
			double sinPitch = MathF.Sin(halfPitch);
			double cosPitch = MathF.Cos(halfPitch);
			double sinYaw = MathF.Sin(halfYaw);
			double cosYaw = MathF.Cos(halfYaw);

			result.X = (cosYaw * sinPitch * cosRoll) + (sinYaw * cosPitch * sinRoll);
			result.Y = (sinYaw * cosPitch * cosRoll) - (cosYaw * sinPitch * sinRoll);
			result.Z = (cosYaw * cosPitch * sinRoll) - (sinYaw * sinPitch * cosRoll);
			result.W = (cosYaw * cosPitch * cosRoll) + (sinYaw * sinPitch * sinRoll);
		}

		#endregion

		#region Divide

		/// <summary>
		/// Divides a <see cref="Quaternion"/> by the other <see cref="Quaternion"/>.
		/// </summary>
		/// <param name="quaternion1">Source <see cref="Quaternion"/>.</param>
		/// <param name="quaternion2">Divisor <see cref="Quaternion"/>.</param>
		/// <returns>The result of dividing the quaternions.</returns>
		public static Quaternion Divide(Quaternion quaternion1, Quaternion quaternion2)
		{
			Quaternion quaternion;
			double x = quaternion1.X;
			double y = quaternion1.Y;
			double z = quaternion1.Z;
			double w = quaternion1.W;
			double num14 = (((quaternion2.X * quaternion2.X) + (quaternion2.Y * quaternion2.Y)) + (quaternion2.Z * quaternion2.Z)) + (quaternion2.W * quaternion2.W);
			double num5 = 1 / num14;
			double num4 = -quaternion2.X * num5;
			double num3 = -quaternion2.Y * num5;
			double num2 = -quaternion2.Z * num5;
			double num = quaternion2.W * num5;
			double num13 = (y * num2) - (z * num3);
			double num12 = (z * num4) - (x * num2);
			double num11 = (x * num3) - (y * num4);
			double num10 = ((x * num4) + (y * num3)) + (z * num2);
			quaternion.X = ((x * num) + (num4 * w)) + num13;
			quaternion.Y = ((y * num) + (num3 * w)) + num12;
			quaternion.Z = ((z * num) + (num2 * w)) + num11;
			quaternion.W = (w * num) - num10;
			return quaternion;
		}

		/// <summary>
		/// Divides a <see cref="Quaternion"/> by the other <see cref="Quaternion"/>.
		/// </summary>
		/// <param name="quaternion1">Source <see cref="Quaternion"/>.</param>
		/// <param name="quaternion2">Divisor <see cref="Quaternion"/>.</param>
		/// <param name="result">The result of dividing the quaternions as an output parameter.</param>
		public static void Divide(ref Quaternion quaternion1, ref Quaternion quaternion2, out Quaternion result)
		{
			double x = quaternion1.X;
			double y = quaternion1.Y;
			double z = quaternion1.Z;
			double w = quaternion1.W;
			double num14 = (((quaternion2.X * quaternion2.X) + (quaternion2.Y * quaternion2.Y)) + (quaternion2.Z * quaternion2.Z)) + (quaternion2.W * quaternion2.W);
			double num5 = 1 / num14;
			double num4 = -quaternion2.X * num5;
			double num3 = -quaternion2.Y * num5;
			double num2 = -quaternion2.Z * num5;
			double num = quaternion2.W * num5;
			double num13 = (y * num2) - (z * num3);
			double num12 = (z * num4) - (x * num2);
			double num11 = (x * num3) - (y * num4);
			double num10 = ((x * num4) + (y * num3)) + (z * num2);
			result.X = ((x * num) + (num4 * w)) + num13;
			result.Y = ((y * num) + (num3 * w)) + num12;
			result.Z = ((z * num) + (num2 * w)) + num11;
			result.W = (w * num) - num10;
		}

		#endregion

		#region Dot

		/// <summary>
		/// Returns a dot product of two quaternions.
		/// </summary>
		/// <param name="quaternion1">The first quaternion.</param>
		/// <param name="quaternion2">The second quaternion.</param>
		/// <returns>The dot product of two quaternions.</returns>
		public static double Dot(Quaternion quaternion1, Quaternion quaternion2)
		{
			return ((((quaternion1.X * quaternion2.X) + (quaternion1.Y * quaternion2.Y)) + (quaternion1.Z * quaternion2.Z)) + (quaternion1.W * quaternion2.W));
		}

		/// <summary>
		/// Returns a dot product of two quaternions.
		/// </summary>
		/// <param name="quaternion1">The first quaternion.</param>
		/// <param name="quaternion2">The second quaternion.</param>
		/// <param name="result">The dot product of two quaternions as an output parameter.</param>
		public static void Dot(ref Quaternion quaternion1, ref Quaternion quaternion2, out double result)
		{
			result = (((quaternion1.X * quaternion2.X) + (quaternion1.Y * quaternion2.Y)) + (quaternion1.Z * quaternion2.Z)) + (quaternion1.W * quaternion2.W);
		}

		#endregion

		#region Equals

		/// <summary>
		/// Compares whether current instance is equal to specified <see cref="object"/>.
		/// </summary>
		/// <param name="obj">The <see cref="object"/> to compare.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public override bool Equals(object obj)
		{
			if (obj is Quaternion)
				return this.Equals((Quaternion)obj);
			return false;
		}

		/// <summary>
		/// Compares whether current instance is equal to specified <see cref="Quaternion"/>.
		/// </summary>
		/// <param name="other">The <see cref="Quaternion"/> to compare.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public bool Equals(Quaternion other)
		{
			return this.X == other.X &&
				   this.Y == other.Y &&
				   this.Z == other.Z &&
				   this.W == other.W;
		}

		#endregion

		/// <summary>
		/// Gets the hash code of this <see cref="Quaternion"/>.
		/// </summary>
		/// <returns>Hash code of this <see cref="Quaternion"/>.</returns>
		public override int GetHashCode()
		{
			return this.X.GetHashCode() + this.Y.GetHashCode() + this.Z.GetHashCode() + this.W.GetHashCode();
		}

		#region Inverse

		/// <summary>
		/// Returns the inverse quaternion which represents the opposite rotation.
		/// </summary>
		/// <param name="quaternion">Source <see cref="Quaternion"/>.</param>
		/// <returns>The inverse quaternion.</returns>
		public static Quaternion Inverse(Quaternion quaternion)
		{
			Quaternion quaternion2;
			double num2 = (((quaternion.X * quaternion.X) + (quaternion.Y * quaternion.Y)) + (quaternion.Z * quaternion.Z)) + (quaternion.W * quaternion.W);
			double num = 1 / num2;
			quaternion2.X = -quaternion.X * num;
			quaternion2.Y = -quaternion.Y * num;
			quaternion2.Z = -quaternion.Z * num;
			quaternion2.W = quaternion.W * num;
			return quaternion2;
		}

		/// <summary>
		/// Returns the inverse quaternion which represents the opposite rotation.
		/// </summary>
		/// <param name="quaternion">Source <see cref="Quaternion"/>.</param>
		/// <param name="result">The inverse quaternion as an output parameter.</param>
		public static void Inverse(ref Quaternion quaternion, out Quaternion result)
		{
			double num2 = (((quaternion.X * quaternion.X) + (quaternion.Y * quaternion.Y)) + (quaternion.Z * quaternion.Z)) + (quaternion.W * quaternion.W);
			double num = 1 / num2;
			result.X = -quaternion.X * num;
			result.Y = -quaternion.Y * num;
			result.Z = -quaternion.Z * num;
			result.W = quaternion.W * num;
		}


		/// <summary>
		/// Get the inverse of the given quaternion
		/// </summary>
		/// <param name="q">The quaternion to invert</param>
		/// <param name="result">The inverse of the given quaternion</param>
		public static void Invert(ref Quaternion q, out Quaternion result)
		{
			double lengthSq = q.LengthSquared();
			if (lengthSq != 0.0)
			{
				double i = 1.0 / lengthSq;
				result = new Quaternion(q.Xyz * -i, q.W * i);
			}
			else
			{
				result = q;
			}
		}

		#endregion

		/// <summary>
		/// Returns the magnitude of the quaternion components.
		/// </summary>
		/// <returns>The magnitude of the quaternion components.</returns>
		public double Length()
		{
			return MathF.Sqrt((this.X * this.X) + (this.Y * this.Y) + (this.Z * this.Z) + (this.W * this.W));
		}

		/// <summary>
		/// Returns the squared magnitude of the quaternion components.
		/// </summary>
		/// <returns>The squared magnitude of the quaternion components.</returns>
		public double LengthSquared()
		{
			return (this.X * this.X) + (this.Y * this.Y) + (this.Z * this.Z) + (this.W * this.W);
		}

		#region Lerp

		/// <summary>
		/// Performs a linear blend between two quaternions.
		/// </summary>
		/// <param name="quaternion1">Source <see cref="Quaternion"/>.</param>
		/// <param name="quaternion2">Source <see cref="Quaternion"/>.</param>
		/// <param name="amount">The blend amount where 0 returns <paramref name="quaternion1"/> and 1 <paramref name="quaternion2"/>.</param>
		/// <returns>The result of linear blending between two quaternions.</returns>
		public static Quaternion Lerp(Quaternion quaternion1, Quaternion quaternion2, double amount)
		{
			double num = amount;
			double num2 = 1 - num;
			Quaternion quaternion = new Quaternion();
			double num5 = (((quaternion1.X * quaternion2.X) + (quaternion1.Y * quaternion2.Y)) + (quaternion1.Z * quaternion2.Z)) + (quaternion1.W * quaternion2.W);
			if (num5 >= 0f)
			{
				quaternion.X = (num2 * quaternion1.X) + (num * quaternion2.X);
				quaternion.Y = (num2 * quaternion1.Y) + (num * quaternion2.Y);
				quaternion.Z = (num2 * quaternion1.Z) + (num * quaternion2.Z);
				quaternion.W = (num2 * quaternion1.W) + (num * quaternion2.W);
			}
			else
			{
				quaternion.X = (num2 * quaternion1.X) - (num * quaternion2.X);
				quaternion.Y = (num2 * quaternion1.Y) - (num * quaternion2.Y);
				quaternion.Z = (num2 * quaternion1.Z) - (num * quaternion2.Z);
				quaternion.W = (num2 * quaternion1.W) - (num * quaternion2.W);
			}
			double num4 = (((quaternion.X * quaternion.X) + (quaternion.Y * quaternion.Y)) + (quaternion.Z * quaternion.Z)) + (quaternion.W * quaternion.W);
			double num3 = 1 / MathF.Sqrt(num4);
			quaternion.X *= num3;
			quaternion.Y *= num3;
			quaternion.Z *= num3;
			quaternion.W *= num3;
			return quaternion;
		}

		/// <summary>
		/// Performs a linear blend between two quaternions.
		/// </summary>
		/// <param name="quaternion1">Source <see cref="Quaternion"/>.</param>
		/// <param name="quaternion2">Source <see cref="Quaternion"/>.</param>
		/// <param name="amount">The blend amount where 0 returns <paramref name="quaternion1"/> and 1 <paramref name="quaternion2"/>.</param>
		/// <param name="result">The result of linear blending between two quaternions as an output parameter.</param>
		public static void Lerp(ref Quaternion quaternion1, ref Quaternion quaternion2, double amount, out Quaternion result)
		{
			double num = amount;
			double num2 = 1 - num;
			double num5 = (((quaternion1.X * quaternion2.X) + (quaternion1.Y * quaternion2.Y)) + (quaternion1.Z * quaternion2.Z)) + (quaternion1.W * quaternion2.W);
			if (num5 >= 0f)
			{
				result.X = (num2 * quaternion1.X) + (num * quaternion2.X);
				result.Y = (num2 * quaternion1.Y) + (num * quaternion2.Y);
				result.Z = (num2 * quaternion1.Z) + (num * quaternion2.Z);
				result.W = (num2 * quaternion1.W) + (num * quaternion2.W);
			}
			else
			{
				result.X = (num2 * quaternion1.X) - (num * quaternion2.X);
				result.Y = (num2 * quaternion1.Y) - (num * quaternion2.Y);
				result.Z = (num2 * quaternion1.Z) - (num * quaternion2.Z);
				result.W = (num2 * quaternion1.W) - (num * quaternion2.W);
			}
			double num4 = (((result.X * result.X) + (result.Y * result.Y)) + (result.Z * result.Z)) + (result.W * result.W);
			double num3 = 1 / MathF.Sqrt(num4);
			result.X *= num3;
			result.Y *= num3;
			result.Z *= num3;
			result.W *= num3;

		}

		#endregion

		#region Slerp

		/// <summary>
		/// Performs a spherical linear blend between two quaternions.
		/// </summary>
		/// <param name="quaternion1">Source <see cref="Quaternion"/>.</param>
		/// <param name="quaternion2">Source <see cref="Quaternion"/>.</param>
		/// <param name="amount">The blend amount where 0 returns <paramref name="quaternion1"/> and 1 <paramref name="quaternion2"/>.</param>
		/// <returns>The result of spherical linear blending between two quaternions.</returns>
		public static Quaternion Slerp(Quaternion quaternion1, Quaternion quaternion2, double amount)
		{
			double num2;
			double num3;
			Quaternion quaternion;
			double num = amount;
			double num4 = (((quaternion1.X * quaternion2.X) + (quaternion1.Y * quaternion2.Y)) + (quaternion1.Z * quaternion2.Z)) + (quaternion1.W * quaternion2.W);
			bool flag = false;
			if (num4 < 0f)
			{
				flag = true;
				num4 = -num4;
			}
			if (num4 > 0.999999f)
			{
				num3 = 1 - num;
				num2 = flag ? -num : num;
			}
			else
			{
				double num5 = MathF.Acos(num4);
				double num6 = (double)(1.0 / Math.Sin((double)num5));
				num3 = MathF.Sin((1 - num) * num5) * num6;
				num2 = flag ? (-MathF.Sin(num * num5) * num6) : (MathF.Sin(num * num5) * num6);
			}
			quaternion.X = (num3 * quaternion1.X) + (num2 * quaternion2.X);
			quaternion.Y = (num3 * quaternion1.Y) + (num2 * quaternion2.Y);
			quaternion.Z = (num3 * quaternion1.Z) + (num2 * quaternion2.Z);
			quaternion.W = (num3 * quaternion1.W) + (num2 * quaternion2.W);
			return quaternion;
		}

		/// <summary>
		/// Performs a spherical linear blend between two quaternions.
		/// </summary>
		/// <param name="quaternion1">Source <see cref="Quaternion"/>.</param>
		/// <param name="quaternion2">Source <see cref="Quaternion"/>.</param>
		/// <param name="amount">The blend amount where 0 returns <paramref name="quaternion1"/> and 1 <paramref name="quaternion2"/>.</param>
		/// <param name="result">The result of spherical linear blending between two quaternions as an output parameter.</param>
		public static void Slerp(ref Quaternion quaternion1, ref Quaternion quaternion2, double amount, out Quaternion result)
		{
			double num2;
			double num3;
			double num = amount;
			double num4 = (((quaternion1.X * quaternion2.X) + (quaternion1.Y * quaternion2.Y)) + (quaternion1.Z * quaternion2.Z)) + (quaternion1.W * quaternion2.W);
			bool flag = false;
			if (num4 < 0f)
			{
				flag = true;
				num4 = -num4;
			}
			if (num4 > 0.999999f)
			{
				num3 = 1 - num;
				num2 = flag ? -num : num;
			}
			else
			{
				double num5 = MathF.Acos(num4);
				double num6 = (double)(1.0 / Math.Sin((double)num5));
				num3 = MathF.Sin((1 - num) * num5) * num6;
				num2 = flag ? (-MathF.Sin(num * num5) * num6) : (MathF.Sin(num * num5) * num6);
			}
			result.X = (num3 * quaternion1.X) + (num2 * quaternion2.X);
			result.Y = (num3 * quaternion1.Y) + (num2 * quaternion2.Y);
			result.Z = (num3 * quaternion1.Z) + (num2 * quaternion2.Z);
			result.W = (num3 * quaternion1.W) + (num2 * quaternion2.W);
		}

		#endregion

		#region Subtract

		/// <summary>
		/// Creates a new <see cref="Quaternion"/> that contains subtraction of one <see cref="Quaternion"/> from another.
		/// </summary>
		/// <param name="quaternion1">Source <see cref="Quaternion"/>.</param>
		/// <param name="quaternion2">Source <see cref="Quaternion"/>.</param>
		/// <returns>The result of the quaternion subtraction.</returns>
		public static Quaternion Subtract(Quaternion quaternion1, Quaternion quaternion2)
		{
			Quaternion quaternion;
			quaternion.X = quaternion1.X - quaternion2.X;
			quaternion.Y = quaternion1.Y - quaternion2.Y;
			quaternion.Z = quaternion1.Z - quaternion2.Z;
			quaternion.W = quaternion1.W - quaternion2.W;
			return quaternion;
		}

		/// <summary>
		/// Creates a new <see cref="Quaternion"/> that contains subtraction of one <see cref="Quaternion"/> from another.
		/// </summary>
		/// <param name="quaternion1">Source <see cref="Quaternion"/>.</param>
		/// <param name="quaternion2">Source <see cref="Quaternion"/>.</param>
		/// <param name="result">The result of the quaternion subtraction as an output parameter.</param>
		public static void Subtract(ref Quaternion quaternion1, ref Quaternion quaternion2, out Quaternion result)
		{
			result.X = quaternion1.X - quaternion2.X;
			result.Y = quaternion1.Y - quaternion2.Y;
			result.Z = quaternion1.Z - quaternion2.Z;
			result.W = quaternion1.W - quaternion2.W;
		}

		#endregion

		#region Multiply

		/// <summary>
		/// Creates a new <see cref="Quaternion"/> that contains a multiplication of two quaternions.
		/// </summary>
		/// <param name="quaternion1">Source <see cref="Quaternion"/>.</param>
		/// <param name="quaternion2">Source <see cref="Quaternion"/>.</param>
		/// <returns>The result of the quaternion multiplication.</returns>
		public static Quaternion Multiply(Quaternion quaternion1, Quaternion quaternion2)
		{
			Quaternion quaternion;
			double x = quaternion1.X;
			double y = quaternion1.Y;
			double z = quaternion1.Z;
			double w = quaternion1.W;
			double num4 = quaternion2.X;
			double num3 = quaternion2.Y;
			double num2 = quaternion2.Z;
			double num = quaternion2.W;
			double num12 = (y * num2) - (z * num3);
			double num11 = (z * num4) - (x * num2);
			double num10 = (x * num3) - (y * num4);
			double num9 = ((x * num4) + (y * num3)) + (z * num2);
			quaternion.X = ((x * num) + (num4 * w)) + num12;
			quaternion.Y = ((y * num) + (num3 * w)) + num11;
			quaternion.Z = ((z * num) + (num2 * w)) + num10;
			quaternion.W = (w * num) - num9;
			return quaternion;
		}

		/// <summary>
		/// Creates a new <see cref="Quaternion"/> that contains a multiplication of <see cref="Quaternion"/> and a scalar.
		/// </summary>
		/// <param name="quaternion1">Source <see cref="Quaternion"/>.</param>
		/// <param name="scaleFactor">Scalar value.</param>
		/// <returns>The result of the quaternion multiplication with a scalar.</returns>
		public static Quaternion Multiply(Quaternion quaternion1, double scaleFactor)
		{
			Quaternion quaternion;
			quaternion.X = quaternion1.X * scaleFactor;
			quaternion.Y = quaternion1.Y * scaleFactor;
			quaternion.Z = quaternion1.Z * scaleFactor;
			quaternion.W = quaternion1.W * scaleFactor;
			return quaternion;
		}

		/// <summary>
		/// Creates a new <see cref="Quaternion"/> that contains a multiplication of <see cref="Quaternion"/> and a scalar.
		/// </summary>
		/// <param name="quaternion1">Source <see cref="Quaternion"/>.</param>
		/// <param name="scaleFactor">Scalar value.</param>
		/// <param name="result">The result of the quaternion multiplication with a scalar as an output parameter.</param>
		public static void Multiply(ref Quaternion quaternion1, double scaleFactor, out Quaternion result)
		{
			result.X = quaternion1.X * scaleFactor;
			result.Y = quaternion1.Y * scaleFactor;
			result.Z = quaternion1.Z * scaleFactor;
			result.W = quaternion1.W * scaleFactor;
		}

		/// <summary>
		/// Creates a new <see cref="Quaternion"/> that contains a multiplication of two quaternions.
		/// </summary>
		/// <param name="quaternion1">Source <see cref="Quaternion"/>.</param>
		/// <param name="quaternion2">Source <see cref="Quaternion"/>.</param>
		/// <param name="result">The result of the quaternion multiplication as an output parameter.</param>
		public static void Multiply(ref Quaternion quaternion1, ref Quaternion quaternion2, out Quaternion result)
		{
			double x = quaternion1.X;
			double y = quaternion1.Y;
			double z = quaternion1.Z;
			double w = quaternion1.W;
			double num4 = quaternion2.X;
			double num3 = quaternion2.Y;
			double num2 = quaternion2.Z;
			double num = quaternion2.W;
			double num12 = (y * num2) - (z * num3);
			double num11 = (z * num4) - (x * num2);
			double num10 = (x * num3) - (y * num4);
			double num9 = ((x * num4) + (y * num3)) + (z * num2);
			result.X = ((x * num) + (num4 * w)) + num12;
			result.Y = ((y * num) + (num3 * w)) + num11;
			result.Z = ((z * num) + (num2 * w)) + num10;
			result.W = (w * num) - num9;
		}

		#endregion

		#region Negate

		/// <summary>
		/// Flips the sign of the all the quaternion components.
		/// </summary>
		/// <param name="quaternion">Source <see cref="Quaternion"/>.</param>
		/// <returns>The result of the quaternion negation.</returns>
		public static Quaternion Negate(Quaternion quaternion)
		{
			return new Quaternion(-quaternion.X, -quaternion.Y, -quaternion.Z, -quaternion.W);
		}

		/// <summary>
		/// Flips the sign of the all the quaternion components.
		/// </summary>
		/// <param name="quaternion">Source <see cref="Quaternion"/>.</param>
		/// <param name="result">The result of the quaternion negation as an output parameter.</param>
		public static void Negate(ref Quaternion quaternion, out Quaternion result)
		{
			result.X = -quaternion.X;
			result.Y = -quaternion.Y;
			result.Z = -quaternion.Z;
			result.W = -quaternion.W;
		}

		#endregion

		#region Normalize

		/// <summary>
		/// Scales the quaternion magnitude to unit length.
		/// </summary>
		public void Normalize()
		{
			double num = 1 / MathF.Sqrt((this.X * this.X) + (this.Y * this.Y) + (this.Z * this.Z) + (this.W * this.W));
			this.X *= num;
			this.Y *= num;
			this.Z *= num;
			this.W *= num;
		}

		/// <summary>
		/// Scales the quaternion magnitude to unit length.
		/// </summary>
		/// <param name="quaternion">Source <see cref="Quaternion"/>.</param>
		/// <returns>The unit length quaternion.</returns>
		public static Quaternion Normalize(Quaternion quaternion)
		{
			Quaternion result;
			double num = 1 / MathF.Sqrt((quaternion.X * quaternion.X) + (quaternion.Y * quaternion.Y) + (quaternion.Z * quaternion.Z) + (quaternion.W * quaternion.W));
			result.X = quaternion.X * num;
			result.Y = quaternion.Y * num;
			result.Z = quaternion.Z * num;
			result.W = quaternion.W * num;
			return result;
		}

		/// <summary>
		/// Scales the quaternion magnitude to unit length.
		/// </summary>
		/// <param name="quaternion">Source <see cref="Quaternion"/>.</param>
		/// <param name="result">The unit length quaternion an output parameter.</param>
		public static void Normalize(ref Quaternion quaternion, out Quaternion result)
		{
			double num = 1 / MathF.Sqrt((quaternion.X * quaternion.X) + (quaternion.Y * quaternion.Y) + (quaternion.Z * quaternion.Z) + (quaternion.W * quaternion.W));
			result.X = quaternion.X * num;
			result.Y = quaternion.Y * num;
			result.Z = quaternion.Z * num;
			result.W = quaternion.W * num;
		}

		#endregion

		/// <summary>
		/// Returns a <see cref="string"/> representation of this <see cref="Quaternion"/> in the format:
		/// {X:[<see cref="X"/>] Y:[<see cref="Y"/>] Z:[<see cref="Z"/>] W:[<see cref="W"/>]}
		/// </summary>
		/// <returns>A <see cref="string"/> representation of this <see cref="Quaternion"/>.</returns>
		public override string ToString()
		{
			return "{X:" + this.X + " Y:" + this.Y + " Z:" + this.Z + " W:" + this.W + "}";
		}

		/// <summary>
		/// Gets a <see cref="Vector4"/> representation for this object.
		/// </summary>
		/// <returns>A <see cref="Vector4"/> representation for this object.</returns>
		public Vector4 ToVector4()
		{
			return new Vector4(this.X, this.Y, this.Z, this.W);
		}

		public void Deconstruct(out double x, out double y, out double z, out double w)
		{
			x = this.X;
			y = this.Y;
			z = this.Z;
			w = this.W;
		}

		#endregion

		#region Operators

		/// <summary>
		/// Adds two quaternions.
		/// </summary>
		/// <param name="quaternion1">Source <see cref="Quaternion"/> on the left of the add sign.</param>
		/// <param name="quaternion2">Source <see cref="Quaternion"/> on the right of the add sign.</param>
		/// <returns>Sum of the vectors.</returns>
		public static Quaternion operator +(Quaternion quaternion1, Quaternion quaternion2)
		{
			Quaternion quaternion;
			quaternion.X = quaternion1.X + quaternion2.X;
			quaternion.Y = quaternion1.Y + quaternion2.Y;
			quaternion.Z = quaternion1.Z + quaternion2.Z;
			quaternion.W = quaternion1.W + quaternion2.W;
			return quaternion;
		}

		/// <summary>
		/// Divides a <see cref="Quaternion"/> by the other <see cref="Quaternion"/>.
		/// </summary>
		/// <param name="quaternion1">Source <see cref="Quaternion"/> on the left of the div sign.</param>
		/// <param name="quaternion2">Divisor <see cref="Quaternion"/> on the right of the div sign.</param>
		/// <returns>The result of dividing the quaternions.</returns>
		public static Quaternion operator /(Quaternion quaternion1, Quaternion quaternion2)
		{
			Quaternion quaternion;
			double x = quaternion1.X;
			double y = quaternion1.Y;
			double z = quaternion1.Z;
			double w = quaternion1.W;
			double num14 = (((quaternion2.X * quaternion2.X) + (quaternion2.Y * quaternion2.Y)) + (quaternion2.Z * quaternion2.Z)) + (quaternion2.W * quaternion2.W);
			double num5 = 1 / num14;
			double num4 = -quaternion2.X * num5;
			double num3 = -quaternion2.Y * num5;
			double num2 = -quaternion2.Z * num5;
			double num = quaternion2.W * num5;
			double num13 = (y * num2) - (z * num3);
			double num12 = (z * num4) - (x * num2);
			double num11 = (x * num3) - (y * num4);
			double num10 = ((x * num4) + (y * num3)) + (z * num2);
			quaternion.X = ((x * num) + (num4 * w)) + num13;
			quaternion.Y = ((y * num) + (num3 * w)) + num12;
			quaternion.Z = ((z * num) + (num2 * w)) + num11;
			quaternion.W = (w * num) - num10;
			return quaternion;
		}

		/// <summary>
		/// Compares whether two <see cref="Quaternion"/> instances are equal.
		/// </summary>
		/// <param name="quaternion1"><see cref="Quaternion"/> instance on the left of the equal sign.</param>
		/// <param name="quaternion2"><see cref="Quaternion"/> instance on the right of the equal sign.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public static bool operator ==(Quaternion quaternion1, Quaternion quaternion2)
		{
			return ((((quaternion1.X == quaternion2.X) && (quaternion1.Y == quaternion2.Y)) && (quaternion1.Z == quaternion2.Z)) && (quaternion1.W == quaternion2.W));
		}

		/// <summary>
		/// Compares whether two <see cref="Quaternion"/> instances are not equal.
		/// </summary>
		/// <param name="quaternion1"><see cref="Quaternion"/> instance on the left of the not equal sign.</param>
		/// <param name="quaternion2"><see cref="Quaternion"/> instance on the right of the not equal sign.</param>
		/// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>
		public static bool operator !=(Quaternion quaternion1, Quaternion quaternion2)
		{
			if (((quaternion1.X == quaternion2.X) && (quaternion1.Y == quaternion2.Y)) && (quaternion1.Z == quaternion2.Z))
			{
				return (quaternion1.W != quaternion2.W);
			}
			return true;
		}

		/// <summary>
		/// Multiplies two quaternions.
		/// </summary>
		/// <param name="quaternion1">Source <see cref="Quaternion"/> on the left of the mul sign.</param>
		/// <param name="quaternion2">Source <see cref="Quaternion"/> on the right of the mul sign.</param>
		/// <returns>Result of the quaternions multiplication.</returns>
		public static Quaternion operator *(Quaternion quaternion1, Quaternion quaternion2)
		{
			Quaternion quaternion;
			double x = quaternion1.X;
			double y = quaternion1.Y;
			double z = quaternion1.Z;
			double w = quaternion1.W;
			double num4 = quaternion2.X;
			double num3 = quaternion2.Y;
			double num2 = quaternion2.Z;
			double num = quaternion2.W;
			double num12 = (y * num2) - (z * num3);
			double num11 = (z * num4) - (x * num2);
			double num10 = (x * num3) - (y * num4);
			double num9 = ((x * num4) + (y * num3)) + (z * num2);
			quaternion.X = ((x * num) + (num4 * w)) + num12;
			quaternion.Y = ((y * num) + (num3 * w)) + num11;
			quaternion.Z = ((z * num) + (num2 * w)) + num10;
			quaternion.W = (w * num) - num9;
			return quaternion;
		}

		/// <summary>
		/// Multiplies the components of quaternion by a scalar.
		/// </summary>
		/// <param name="quaternion1">Source <see cref="Vector3"/> on the left of the mul sign.</param>
		/// <param name="scaleFactor">Scalar value on the right of the mul sign.</param>
		/// <returns>Result of the quaternion multiplication with a scalar.</returns>
		public static Quaternion operator *(Quaternion quaternion1, double scaleFactor)
		{
			Quaternion quaternion;
			quaternion.X = quaternion1.X * scaleFactor;
			quaternion.Y = quaternion1.Y * scaleFactor;
			quaternion.Z = quaternion1.Z * scaleFactor;
			quaternion.W = quaternion1.W * scaleFactor;
			return quaternion;
		}

		/// <summary>
		/// Subtracts a <see cref="Quaternion"/> from a <see cref="Quaternion"/>.
		/// </summary>
		/// <param name="quaternion1">Source <see cref="Vector3"/> on the left of the sub sign.</param>
		/// <param name="quaternion2">Source <see cref="Vector3"/> on the right of the sub sign.</param>
		/// <returns>Result of the quaternion subtraction.</returns>
		public static Quaternion operator -(Quaternion quaternion1, Quaternion quaternion2)
		{
			Quaternion quaternion;
			quaternion.X = quaternion1.X - quaternion2.X;
			quaternion.Y = quaternion1.Y - quaternion2.Y;
			quaternion.Z = quaternion1.Z - quaternion2.Z;
			quaternion.W = quaternion1.W - quaternion2.W;
			return quaternion;

		}

		/// <summary>
		/// Flips the sign of the all the quaternion components.
		/// </summary>
		/// <param name="quaternion">Source <see cref="Quaternion"/> on the right of the sub sign.</param>
		/// <returns>The result of the quaternion negation.</returns>
		public static Quaternion operator -(Quaternion quaternion)
		{
			Quaternion quaternion2;
			quaternion2.X = -quaternion.X;
			quaternion2.Y = -quaternion.Y;
			quaternion2.Z = -quaternion.Z;
			quaternion2.W = -quaternion.W;
			return quaternion2;
		}

		#endregion
	}
}
