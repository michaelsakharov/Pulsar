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
		/// <summary>
		/// Defines the identity quaternion.
		/// </summary>
		public static readonly Quaternion Identity = new Quaternion(0, 0, 0, 1);


		private Vector3 xyz;
		private float w;


		/// <summary>
		/// Gets or sets an OpenTK.Vector3 with the X, Y and Z components of this instance.
		/// </summary>
		public Vector3 Xyz { get { return this.xyz; } set { this.xyz = value; } }
		/// <summary>
		/// Gets or sets the X component of this instance.
		/// </summary>
		public float X { get { return this.xyz.X; } set { this.xyz.X = value; } }
		/// <summary>
		/// Gets or sets the Y component of this instance.
		/// </summary>
		public float Y { get { return this.xyz.Y; } set { this.xyz.Y = value; } }
		/// <summary>
		/// Gets or sets the Z component of this instance.
		/// </summary>
		public float Z { get { return this.xyz.Z; } set { this.xyz.Z = value; } }
		/// <summary>
		/// Gets or sets the W component of this instance.
		/// </summary>
		public float W { get { return this.w; } set { this.w = value; } }
		/// <summary>
		/// Gets the length (magnitude) of the quaternion.
		/// </summary>
		/// <seealso cref="LengthSquared"/>
		public float Length
		{
			get
			{
				return (float)System.Math.Sqrt(this.W * this.W + this.Xyz.LengthSquared);
			}
		}
		/// <summary>
		/// Gets the square of the quaternion length (magnitude).
		/// </summary>
		public float LengthSquared
		{
			get
			{
				return this.W * this.W + this.Xyz.LengthSquared;
			}
		}
		/// <summary>
		/// Gets or Sets the Euler Angles
		/// </summary>
		public Vector3 EulerAngles
		{
			get
			{
				return ToEuler(this);
			}
			set
			{
				this = FromEuler(value);
			}
		}


		/// <summary>
		/// Construct a new Quaternion from vector and w components
		/// </summary>
		/// <param name="v">The vector part</param>
		/// <param name="w">The w part</param>
		public Quaternion(Vector3 v, float w)
		{
			this.xyz = v;
			this.w = w;
		}
		/// <summary>
		/// Construct a new Quaternion
		/// </summary>
		/// <param name="x">The x component</param>
		/// <param name="y">The y component</param>
		/// <param name="z">The z component</param>
		/// <param name="w">The w component</param>
		public Quaternion(float x, float y, float z, float w) : this(new Vector3(x, y, z), w)
		{ }

		public static Quaternion FromEuler(Vector3 v)
		{
			return FromEuler(v.Y, v.X, v.Z);
		}

		public static Quaternion FromEuler(float yaw, float pitch, float roll)
		{
			yaw *= MathF.Deg2Rad;
			pitch *= MathF.Deg2Rad;
			roll *= MathF.Deg2Rad;
			float rollOver2 = roll * 0.5f;
			float sinRollOver2 = (float)Math.Sin(rollOver2);
			float cosRollOver2 = (float)Math.Cos(rollOver2);
			float pitchOver2 = pitch * 0.5f;
			float sinPitchOver2 = (float)Math.Sin(pitchOver2);
			float cosPitchOver2 = (float)Math.Cos(pitchOver2);
			float yawOver2 = yaw * 0.5f;
			float sinYawOver2 = (float)Math.Sin(yawOver2);
			float cosYawOver2 = (float)Math.Cos(yawOver2);
			Quaternion result = new Quaternion();
			result.W = cosYawOver2 * cosPitchOver2 * cosRollOver2 + sinYawOver2 * sinPitchOver2 * sinRollOver2;
			result.X = cosYawOver2 * sinPitchOver2 * cosRollOver2 + sinYawOver2 * cosPitchOver2 * sinRollOver2;
			result.Y = sinYawOver2 * cosPitchOver2 * cosRollOver2 - cosYawOver2 * sinPitchOver2 * sinRollOver2;
			result.Z = cosYawOver2 * cosPitchOver2 * sinRollOver2 - sinYawOver2 * sinPitchOver2 * cosRollOver2;

			return result;
		}

		public static Vector3 ToEuler(Quaternion q1)
		{
			float sqw = q1.W * q1.W;
			float sqx = q1.X * q1.X;
			float sqy = q1.Y * q1.Y;
			float sqz = q1.Z * q1.Z;
			float unit = sqx + sqy + sqz + sqw; // if normalised is one, otherwise is correction factor
			float test = q1.X * q1.W - q1.Y * q1.Z;
			Vector3 v;

			if (test > 0.4995f * unit)
			{ // singularity at north pole
				v.Y = 2f * MathF.Atan2(q1.Y, q1.X);
				v.X = MathF.Pi / 2;
				v.Z = 0;
				return NormalizeAngles(v * MathF.Rad2Deg);
			}
			if (test < -0.4995f * unit)
			{ // singularity at south pole
				v.Y = -2f * MathF.Atan2(q1.Y, q1.X);
				v.X = -MathF.Pi / 2;
				v.Z = 0;
				return NormalizeAngles(v * MathF.Rad2Deg);
			}
			Quaternion q = new Quaternion(q1.W, q1.Z, q1.X, q1.Y);
			v.Y = (float)Math.Atan2(2f * q.X * q.W + 2f * q.Y * q.Z, 1 - 2f * (q.Z * q.Z + q.W * q.W));     // Yaw
			v.X = (float)Math.Asin(2f * (q.X * q.Z - q.W * q.Y));                             // Pitch
			v.Z = (float)Math.Atan2(2f * q.X * q.Y + 2f * q.Z * q.W, 1 - 2f * (q.Y * q.Y + q.Z * q.Z));      // Roll
			return NormalizeAngles(v * MathF.Rad2Deg);
		}

		static Vector3 NormalizeAngles(Vector3 angles)
		{
			angles.X = NormalizeAngle(angles.X);
			angles.Y = NormalizeAngle(angles.Y);
			angles.Z = NormalizeAngle(angles.Z);
			return angles;
		}

		static float NormalizeAngle(float angle)
		{
			while (angle > 360)
				angle -= 360;
			while (angle < 0)
				angle += 360;
			return angle;
		}

		/// <summary>
		/// Concatenates two Quaternions; the result represents the value1 rotation followed by the value2 rotation.
		/// </summary>
		/// <param name="value1">The first Quaternion rotation in the series.</param>
		/// <param name="value2">The second Quaternion rotation in the series.</param>
		/// <returns>A new Quaternion representing the concatenation of the value1 rotation followed by the value2 rotation.</returns>
		public static Quaternion Concatenate(Quaternion value1, Quaternion value2)
		{

			// Concatenate rotation is actually q2 * q1 instead of q1 * q2.
			// So that's why value2 goes q1 and value1 goes q2.
			float q1x = value2.X;
			float q1y = value2.Y;
			float q1z = value2.Z;
			float q1w = value2.W;

			float q2x = value1.X;
			float q2y = value1.Y;
			float q2z = value1.Z;
			float q2w = value1.W;

			// cross(av, bv)
			float cx = q1y * q2z - q1z * q2y;
			float cy = q1z * q2x - q1x * q2z;
			float cz = q1x * q2y - q1y * q2x;

			float dot = q1x * q2x + q1y * q2y + q1z * q2z;

			Quaternion ans = new Quaternion();

			ans.X = q1x * q2w + q2x * q1w + cx;
			ans.Y = q1y * q2w + q2y * q1w + cy;
			ans.Z = q1z * q2w + q2z * q1w + cz;
			ans.W = q1w * q2w - dot;

			return ans;
		}

		/// <summary>
		/// Convert the current quaternion to axis angle representation
		/// </summary>
		/// <param name="axis">The resultant axis</param>
		/// <param name="angle">The resultant angle</param>
		public void ToAxisAngle(out Vector3 axis, out float angle)
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

			result.W = 2.0f * (float)System.Math.Acos(q.W); // angle
			float den = (float)System.Math.Sqrt(1.0 - q.W * q.W);
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
		/// Reverses the rotation angle of this Quaterniond.
		/// </summary>
		public void Invert()
		{
			this.W = -this.W;
		}
		/// <summary>
		/// Scales the Quaternion to unit length.
		/// </summary>
		public void Normalize()
		{
			float scale = 1.0f / this.Length;
			this.Xyz *= scale;
			this.W *= scale;
		}
		/// <summary>
		/// Inverts the Vector3 component of this Quaternion.
		/// </summary>
		public void Conjugate()
		{
			this.Xyz = -this.Xyz;
		}

		/// <summary>
		/// Returns a copy of the Quaternion scaled to unit length.
		/// </summary>
		public Quaternion Normalized()
		{
			Quaternion q = this;
			q.Normalize();
			return q;
		}
		/// <summary>
		/// Returns a copy of this Quaterniond with its rotation angle reversed.
		/// </summary>
		public Quaternion Inverted()
		{
			var q = this;
			q.Invert();
			return q;
		}

		/// <summary>
		/// Add two quaternions
		/// </summary>
		/// <param name="left">The first operand</param>
		/// <param name="right">The second operand</param>
		/// <returns>The result of the addition</returns>
		public static Quaternion Add(Quaternion left, Quaternion right)
		{
			return new Quaternion(
				left.Xyz + right.Xyz,
				left.W + right.W);
		}
		/// <summary>
		/// Add two quaternions
		/// </summary>
		/// <param name="left">The first operand</param>
		/// <param name="right">The second operand</param>
		/// <param name="result">The result of the addition</param>
		public static void Add(ref Quaternion left, ref Quaternion right, out Quaternion result)
		{
			result = new Quaternion(
				left.Xyz + right.Xyz,
				left.W + right.W);
		}

		/// <summary>
		/// Subtracts two instances.
		/// </summary>
		/// <param name="left">The left instance.</param>
		/// <param name="right">The right instance.</param>
		/// <returns>The result of the operation.</returns>
		public static Quaternion Sub(Quaternion left, Quaternion right)
		{
			return  new Quaternion(
				left.Xyz - right.Xyz,
				left.W - right.W);
		}
		/// <summary>
		/// Subtracts two instances.
		/// </summary>
		/// <param name="left">The left instance.</param>
		/// <param name="right">The right instance.</param>
		/// <param name="result">The result of the operation.</param>
		public static void Sub(ref Quaternion left, ref Quaternion right, out Quaternion result)
		{
			result = new Quaternion(
				left.Xyz - right.Xyz,
				left.W - right.W);
		}

		/// <summary>
		/// Multiplies two instances.
		/// </summary>
		/// <param name="left">The first instance.</param>
		/// <param name="right">The second instance.</param>
		/// <returns>A new instance containing the result of the calculation.</returns>
		public static Quaternion Multiply(Quaternion left, Quaternion right)
		{
			Quaternion result;
			Multiply(ref left, ref right, out result);
			return result;
		}
		/// <summary>
		/// Multiplies two instances.
		/// </summary>
		/// <param name="left">The first instance.</param>
		/// <param name="right">The second instance.</param>
		/// <param name="result">A new instance containing the result of the calculation.</param>
		public static void Multiply(ref Quaternion left, ref Quaternion right, out Quaternion result)
		{
			result = new Quaternion(
				right.W * left.Xyz + left.W * right.Xyz + Vector3.Cross(left.Xyz, right.Xyz),
				left.W * right.W - Vector3.Dot(left.Xyz, right.Xyz));
		}
		/// <summary>
		/// Multiplies an instance by a scalar.
		/// </summary>
		/// <param name="quaternion">The instance.</param>
		/// <param name="scale">The scalar.</param>
		/// <param name="result">A new instance containing the result of the calculation.</param>
		public static void Multiply(ref Quaternion quaternion, float scale, out Quaternion result)
		{
			result = new Quaternion(quaternion.X * scale, quaternion.Y * scale, quaternion.Z * scale, quaternion.W * scale);
		}
		/// <summary>
		/// Multiplies an instance by a scalar.
		/// </summary>
		/// <param name="quaternion">The instance.</param>
		/// <param name="scale">The scalar.</param>
		/// <returns>A new instance containing the result of the calculation.</returns>
		public static Quaternion Multiply(Quaternion quaternion, float scale)
		{
			return new Quaternion(quaternion.X * scale, quaternion.Y * scale, quaternion.Z * scale, quaternion.W * scale);
		}

		/// <summary>
		/// Get the conjugate of the given quaternion
		/// </summary>
		/// <param name="q">The quaternion</param>
		/// <returns>The conjugate of the given quaternion</returns>
		public static Quaternion Conjugate(Quaternion q)
		{
			return new Quaternion(-q.Xyz, q.W);
		}
		/// <summary>
		/// Get the conjugate of the given quaternion
		/// </summary>
		/// <param name="q">The quaternion</param>
		/// <param name="result">The conjugate of the given quaternion</param>
		public static void Conjugate(ref Quaternion q, out Quaternion result)
		{
			result = new Quaternion(-q.Xyz, q.W);
		}

		/// <summary>
		/// Get the inverse of the given quaternion
		/// </summary>
		/// <param name="q">The quaternion to invert</param>
		/// <returns>The inverse of the given quaternion</returns>
		public static Quaternion Invert(Quaternion q)
		{
			Quaternion result;
			Invert(ref q, out result);
			return result;
		}
		/// <summary>
		/// Get the inverse of the given quaternion
		/// </summary>
		/// <param name="q">The quaternion to invert</param>
		/// <param name="result">The inverse of the given quaternion</param>
		public static void Invert(ref Quaternion q, out Quaternion result)
		{
			float lengthSq = q.LengthSquared;
			if (lengthSq != 0.0)
			{
				float i = 1.0f / lengthSq;
				result = new Quaternion(q.Xyz * -i, q.W * i);
			}
			else
			{
				result = q;
			}
		}

		/// <summary>
		/// Scale the given quaternion to unit length
		/// </summary>
		/// <param name="q">The quaternion to normalize</param>
		/// <returns>The normalized quaternion</returns>
		public static Quaternion Normalize(Quaternion q)
		{
			Quaternion result;
			Normalize(ref q, out result);
			return result;
		}
		/// <summary>
		/// Scale the given quaternion to unit length
		/// </summary>
		/// <param name="q">The quaternion to normalize</param>
		/// <param name="result">The normalized quaternion</param>
		public static void Normalize(ref Quaternion q, out Quaternion result)
		{
			float scale = 1.0f / q.Length;
			result = new Quaternion(q.Xyz * scale, q.W * scale);
		}

		/// <summary>
		/// Build a quaternion from the given axis and angle
		/// </summary>
		/// <param name="axis">The axis to rotate about</param>
		/// <param name="angle">The rotation angle in radians</param>
		/// <returns>The equivalent quaternion</returns>
		public static Quaternion FromAxisAngle(Vector3 axis, float angle)
		{
			if (axis.LengthSquared == 0.0f)
				return Identity;

			Quaternion result = Identity;

			angle *= 0.5f;
			axis.Normalize();
			result.Xyz = axis * (float)System.Math.Sin(angle);
			result.W = (float)System.Math.Cos(angle);

			return Normalize(result);
		}

		/// <summary>
		/// Builds a quaternion from the given rotation matrix
		/// </summary>
		/// <param name="matrix">A rotation matrix</param>
		/// <returns>The equivalent quaternion</returns>
		public static Quaternion FromMatrix(Matrix3 matrix)
		{
			Quaternion result;
			FromMatrix(ref matrix, out result);
			return result;
		}
		/// <summary>
		/// Builds a quaternion from the given rotation matrix
		/// </summary>
		/// <param name="matrix">A rotation matrix</param>
		/// <param name="result">The equivalent quaternion</param>
		public static void FromMatrix(ref Matrix3 matrix, out Quaternion result)
		{
			float trace = matrix.Trace;

			if (trace > 0)
			{
				float s = (float)Math.Sqrt(trace + 1) * 2;
				float invS = 1f / s;

				result.w = s * 0.25f;
				result.xyz.X = (matrix.Row2.Y - matrix.Row1.Z) * invS;
				result.xyz.Y = (matrix.Row0.Z - matrix.Row2.X) * invS;
				result.xyz.Z = (matrix.Row1.X - matrix.Row0.Y) * invS;
			}
			else
			{
				float m00 = matrix.Row0.X, m11 = matrix.Row1.Y, m22 = matrix.Row2.Z;

				if (m00 > m11 && m00 > m22)
				{
					float s = (float)Math.Sqrt(1 + m00 - m11 - m22) * 2;
					float invS = 1f / s;

					result.w = (matrix.Row2.Y - matrix.Row1.Z) * invS;
					result.xyz.X = s * 0.25f;
					result.xyz.Y = (matrix.Row0.Y + matrix.Row1.X) * invS;
					result.xyz.Z = (matrix.Row0.Z + matrix.Row2.X) * invS;
				}
				else if (m11 > m22)
				{
					float s = (float)Math.Sqrt(1 + m11 - m00 - m22) * 2;
					float invS = 1f / s;

					result.w = (matrix.Row0.Z - matrix.Row2.X) * invS;
					result.xyz.X = (matrix.Row0.Y + matrix.Row1.X) * invS;
					result.xyz.Y = s * 0.25f;
					result.xyz.Z = (matrix.Row1.Z + matrix.Row2.Y) * invS;
				}
				else
				{
					float s = (float)Math.Sqrt(1 + m22 - m00 - m11) * 2;
					float invS = 1f / s;

					result.w = (matrix.Row1.X - matrix.Row0.Y) * invS;
					result.xyz.X = (matrix.Row0.Z + matrix.Row2.X) * invS;
					result.xyz.Y = (matrix.Row1.Z + matrix.Row2.Y) * invS;
					result.xyz.Z = s * 0.25f;
				}
			}
		}

		/// <summary>
		/// Do Spherical linear interpolation between two quaternions 
		/// </summary>
		/// <param name="q1">The first quaternion</param>
		/// <param name="q2">The second quaternion</param>
		/// <param name="blend">The blend factor</param>
		/// <returns>A smooth blend between the given quaternions</returns>
		public static Quaternion Slerp(Quaternion q1, Quaternion q2, float blend)
		{
			// if either input is zero, return the other.
			if (q1.LengthSquared == 0.0f)
			{
				if (q2.LengthSquared == 0.0f)
				{
					return Identity;
				}
				return q2;
			}
			else if (q2.LengthSquared == 0.0f)
			{
				return q1;
			}


			float cosHalfAngle = q1.W * q2.W + Vector3.Dot(q1.Xyz, q2.Xyz);

			if (cosHalfAngle >= 1.0f || cosHalfAngle <= -1.0f)
			{
				// angle = 0.0f, so just return one input.
				return q1;
			}
			else if (cosHalfAngle < 0.0f)
			{
				q2.Xyz = -q2.Xyz;
				q2.W = -q2.W;
				cosHalfAngle = -cosHalfAngle;
			}

			float blendA;
			float blendB;
			if (cosHalfAngle < 0.99f)
			{
				// do proper slerp for big angles
				float halfAngle = (float)System.Math.Acos(cosHalfAngle);
				float sinHalfAngle = (float)System.Math.Sin(halfAngle);
				float oneOverSinHalfAngle = 1.0f / sinHalfAngle;
				blendA = (float)System.Math.Sin(halfAngle * (1.0f - blend)) * oneOverSinHalfAngle;
				blendB = (float)System.Math.Sin(halfAngle * blend) * oneOverSinHalfAngle;
			}
			else
			{
				// do lerp if angle is really small.
				blendA = 1.0f - blend;
				blendB = blend;
			}

			Quaternion result = new Quaternion(blendA * q1.Xyz + blendB * q2.Xyz, blendA * q1.W + blendB * q2.W);
			if (result.LengthSquared > 0.0f)
				return Normalize(result);
			else
				return Identity;
		}

		/// <summary>
		/// Adds two instances.
		/// </summary>
		/// <param name="left">The first instance.</param>
		/// <param name="right">The second instance.</param>
		/// <returns>The result of the calculation.</returns>
		public static Quaternion operator +(Quaternion left, Quaternion right)
		{
			left.Xyz += right.Xyz;
			left.W += right.W;
			return left;
		}
		/// <summary>
		/// Subtracts two instances.
		/// </summary>
		/// <param name="left">The first instance.</param>
		/// <param name="right">The second instance.</param>
		/// <returns>The result of the calculation.</returns>
		public static Quaternion operator -(Quaternion left, Quaternion right)
		{
			left.Xyz -= right.Xyz;
			left.W -= right.W;
			return left;
		}
		/// <summary>
		/// Multiplies two instances.
		/// </summary>
		/// <param name="left">The first instance.</param>
		/// <param name="right">The second instance.</param>
		/// <returns>The result of the calculation.</returns>
		public static Quaternion operator *(Quaternion left, Quaternion right)
		{
			Multiply(ref left, ref right, out left);
			return left;
		}
		/// <summary>
		/// Multiplies an instance by a scalar.
		/// </summary>
		/// <param name="quaternion">The instance.</param>
		/// <param name="scale">The scalar.</param>
		/// <returns>A new instance containing the result of the calculation.</returns>
		public static Quaternion operator *(Quaternion quaternion, float scale)
		{
			Multiply(ref quaternion, scale, out quaternion);
			return quaternion;
		}
		/// <summary>
		/// Multiplies an instance by a scalar.
		/// </summary>
		/// <param name="quaternion">The instance.</param>
		/// <param name="scale">The scalar.</param>
		/// <returns>A new instance containing the result of the calculation.</returns>
		public static Quaternion operator *(float scale, Quaternion quaternion)
		{
			return new Quaternion(quaternion.X * scale, quaternion.Y * scale, quaternion.Z * scale, quaternion.W * scale);
		}

		/// <summary>
		/// Compares two instances for equality.
		/// </summary>
		/// <param name="left">The first instance.</param>
		/// <param name="right">The second instance.</param>
		/// <returns>True, if left equals right; false otherwise.</returns>
		public static bool operator ==(Quaternion left, Quaternion right)
		{
			return left.Equals(right);
		}
		/// <summary>
		/// Compares two instances for inequality.
		/// </summary>
		/// <param name="left">The first instance.</param>
		/// <param name="right">The second instance.</param>
		/// <returns>True, if left does not equal right; false otherwise.</returns>
		public static bool operator !=(Quaternion left, Quaternion right)
		{
			return !left.Equals(right);
		}

		/// <summary>
		/// Returns a System.String that represents the current Quaternion.
		/// </summary>
		public override string ToString()
		{
			return string.Format("V: {0}, W: {1}", this.Xyz, this.W);
		}
		/// <summary>
		/// Compares this object instance to another object for equality. 
		/// </summary>
		/// <param name="other">The other object to be used in the comparison.</param>
		/// <returns>True if both objects are Quaternions of equal value. Otherwise it returns false.</returns>
		public override bool Equals(object other)
		{
			if (other is Quaternion == false) return false;
				return this == (Quaternion)other;
		}
		/// <summary>
		/// Provides the hash code for this object. 
		/// </summary>
		/// <returns>A hash code formed from the bitwise XOR of this objects members.</returns>
		public override int GetHashCode()
		{
			return this.Xyz.GetHashCode() ^ this.W.GetHashCode();
		}
		/// <summary>
		/// Compares this Quaternion instance to another Quaternion for equality. 
		/// </summary>
		/// <param name="other">The other Quaternion to be used in the comparison.</param>
		/// <returns>True if both instances are equal; false otherwise.</returns>
		public bool Equals(Quaternion other)
		{
			return this.Xyz == other.Xyz && this.W == other.W;
		}
	}
}
