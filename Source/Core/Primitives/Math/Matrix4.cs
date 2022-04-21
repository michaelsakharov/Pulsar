using System;
using System.Runtime.InteropServices;

namespace Duality
{
	/// <summary>
	/// Represents a 4x4 matrix containing 3D rotation, scale, transform, and projection.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct Matrix4 : IEquatable<Matrix4>
	{
		/// <summary>
		/// The identity matrix.
		/// </summary>
		public static readonly Matrix4 Identity = new Matrix4(Vector4.UnitX, Vector4.UnitY, Vector4.UnitZ, Vector4.UnitW);
		/// <summary>
		/// The zero matrix.
		/// </summary>
		public static readonly Matrix4 Zero = new Matrix4(Vector4.Zero, Vector4.Zero, Vector4.Zero, Vector4.Zero);

		/// <summary>
		/// Top row of the matrix.
		/// </summary>
		public Vector4 Row0;
		/// <summary>
		/// 2nd row of the matrix.
		/// </summary>
		public Vector4 Row1;
		/// <summary>
		/// 3rd row of the matrix.
		/// </summary>
		public Vector4 Row2;
		/// <summary>
		/// Bottom row of the matrix.
		/// </summary>
		public Vector4 Row3;


		/// <summary>
		/// Gets the first column of this matrix.
		/// </summary>
		public Vector4 Column0
		{
			get { return new Vector4(this.Row0.X, this.Row1.X, this.Row2.X, this.Row3.X); }
			set { this.Row0.X = value.X; this.Row1.X = value.Y; this.Row2.X = value.Z; this.Row3.X = value.W; }
		}
		/// <summary>
		/// Gets the second column of this matrix.
		/// </summary>
		public Vector4 Column1
		{
			get { return new Vector4(this.Row0.Y, this.Row1.Y, this.Row2.Y, this.Row3.Y); }
			set { this.Row0.Y = value.X; this.Row1.Y = value.Y; this.Row2.Y = value.Z; this.Row3.Y = value.W; }
		}
		/// <summary>
		/// Gets the third column of this matrix.
		/// </summary>
		public Vector4 Column2
		{
			get { return new Vector4(this.Row0.Z, this.Row1.Z, this.Row2.Z, this.Row3.Z); }
			set { this.Row0.Z = value.X; this.Row1.Z = value.Y; this.Row2.Z = value.Z; this.Row3.Z = value.W; }
		}
		/// <summary>
		/// Gets the fourth column of this matrix.
		/// </summary>
		public Vector4 Column3
		{
			get { return new Vector4(this.Row0.W, this.Row1.W, this.Row2.W, this.Row3.W); }
			set { this.Row0.W = value.X; this.Row1.W = value.Y; this.Row2.W = value.Z; this.Row3.W = value.W; }
		}
		/// <summary>
		/// Gets or sets the value at row 1, column 1 of this instance.
		/// </summary>
		public double M11 { get { return this.Row0.X; } set { this.Row0.X = value; } }
		/// <summary>
		/// Gets or sets the value at row 1, column 2 of this instance.
		/// </summary>
		public double M12 { get { return this.Row0.Y; } set { this.Row0.Y = value; } }
		/// <summary>
		/// Gets or sets the value at row 1, column 3 of this instance.
		/// </summary>
		public double M13 { get { return this.Row0.Z; } set { this.Row0.Z = value; } }
		/// <summary>
		/// Gets or sets the value at row 1, column 4 of this instance.
		/// </summary>
		public double M14 { get { return this.Row0.W; } set { this.Row0.W = value; } }
		/// <summary>
		/// Gets or sets the value at row 2, column 1 of this instance.
		/// </summary>
		public double M21 { get { return this.Row1.X; } set { this.Row1.X = value; } }
		/// <summary>
		/// Gets or sets the value at row 2, column 2 of this instance.
		/// </summary>
		public double M22 { get { return this.Row1.Y; } set { this.Row1.Y = value; } }
		/// <summary>
		/// Gets or sets the value at row 2, column 3 of this instance.
		/// </summary>
		public double M23 { get { return this.Row1.Z; } set { this.Row1.Z = value; } }
		/// <summary>
		/// Gets or sets the value at row 2, column 4 of this instance.
		/// </summary>
		public double M24 { get { return this.Row1.W; } set { this.Row1.W = value; } }
		/// <summary>
		/// Gets or sets the value at row 3, column 1 of this instance.
		/// </summary>
		public double M31 { get { return this.Row2.X; } set { this.Row2.X = value; } }
		/// <summary>
		/// Gets or sets the value at row 3, column 2 of this instance.
		/// </summary>
		public double M32 { get { return this.Row2.Y; } set { this.Row2.Y = value; } }
		/// <summary>
		/// Gets or sets the value at row 3, column 3 of this instance.
		/// </summary>
		public double M33 { get { return this.Row2.Z; } set { this.Row2.Z = value; } }
		/// <summary>
		/// Gets or sets the value at row 3, column 4 of this instance.
		/// </summary>
		public double M34 { get { return this.Row2.W; } set { this.Row2.W = value; } }
		/// <summary>
		/// Gets or sets the value at row 4, column 1 of this instance.
		/// </summary>
		public double M41 { get { return this.Row3.X; } set { this.Row3.X = value; } }
		/// <summary>
		/// Gets or sets the value at row 4, column 2 of this instance.
		/// </summary>
		public double M42 { get { return this.Row3.Y; } set { this.Row3.Y = value; } }
		/// <summary>
		/// Gets or sets the value at row 4, column 3 of this instance.
		/// </summary>
		public double M43 { get { return this.Row3.Z; } set { this.Row3.Z = value; } }
		/// <summary>
		/// Gets or sets the value at row 4, column 4 of this instance.
		/// </summary>
		public double M44 { get { return this.Row3.W; } set { this.Row3.W = value; } }

		/// <summary>
		/// Gets or sets the value at a specified row and column.
		/// </summary>
		public double this[int rowIndex, int columnIndex]
		{
			get
			{
				if (rowIndex == 0) return this.Row0[columnIndex];
				else if (rowIndex == 1) return this.Row1[columnIndex];
				else if (rowIndex == 2) return this.Row2[columnIndex];
				else if (rowIndex == 3) return this.Row3[columnIndex];
				throw new IndexOutOfRangeException("You tried to access this matrix at: (" + rowIndex + ", " + columnIndex + ")");
			}
			set
			{
				if (rowIndex == 0) this.Row0[columnIndex] = value;
				else if (rowIndex == 1) this.Row1[columnIndex] = value;
				else if (rowIndex == 2) this.Row2[columnIndex] = value;
				else if (rowIndex == 3) this.Row3[columnIndex] = value;
				else throw new IndexOutOfRangeException("You tried to set this matrix at: (" + rowIndex + ", " + columnIndex + ")");
			}
		}

		/// <summary>
		/// Gets the determinant of this matrix.
		/// </summary>
		public double Determinant
		{
			get
			{
				double m11 = this.Row0.X, m12 = this.Row0.Y, m13 = this.Row0.Z, m14 = this.Row0.W,
						m21 = this.Row1.X, m22 = this.Row1.Y, m23 = this.Row1.Z, m24 = this.Row1.W,
						m31 = this.Row2.X, m32 = this.Row2.Y, m33 = this.Row2.Z, m34 = this.Row2.W,
						m41 = this.Row3.X, m42 = this.Row3.Y, m43 = this.Row3.Z, m44 = this.Row3.W;
				return
					m11 * m22 * m33 * m44 - m11 * m22 * m34 * m43 + m11 * m23 * m34 * m42 - m11 * m23 * m32 * m44
					+ m11 * m24 * m32 * m43 - m11 * m24 * m33 * m42 - m12 * m23 * m34 * m41 + m12 * m23 * m31 * m44
					- m12 * m24 * m31 * m43 + m12 * m24 * m33 * m41 - m12 * m21 * m33 * m44 + m12 * m21 * m34 * m43
					+ m13 * m24 * m31 * m42 - m13 * m24 * m32 * m41 + m13 * m21 * m32 * m44 - m13 * m21 * m34 * m42
					+ m13 * m22 * m34 * m41 - m13 * m22 * m31 * m44 - m14 * m21 * m32 * m43 + m14 * m21 * m33 * m42
					- m14 * m22 * m33 * m41 + m14 * m22 * m31 * m43 - m14 * m23 * m31 * m42 + m14 * m23 * m32 * m41;
			}
		}

		/// <summary>
		/// Gets or sets the values along the main diagonal of the matrix.
		/// </summary>
		public Vector4 Diagonal
		{
			get
			{
				return new Vector4(this.Row0.X, this.Row1.Y, this.Row2.Z, this.Row3.W);
			}
			set
			{
				this.Row0.X = value.X;
				this.Row1.Y = value.Y;
				this.Row2.Z = value.Z;
				this.Row3.W = value.W;
			}
		}

		/// <summary>
		/// Gets the trace of the matrix, the sum of the values along the diagonal.
		/// </summary>
		public double Trace { get { return this.Row0.X + this.Row1.Y + this.Row2.Z + this.Row3.W; } }

		#region Public Constructors

		/// <summary>
		/// Constructs a new instance.
		/// </summary>
		/// <param name="row0">Top row of the matrix.</param>
		/// <param name="row1">Second row of the matrix.</param>
		/// <param name="row2">Third row of the matrix.</param>
		/// <param name="row3">Bottom row of the matrix.</param>
		public Matrix4(Vector4 row0, Vector4 row1, Vector4 row2, Vector4 row3)
		{
			this.Row0 = row0;
			this.Row1 = row1;
			this.Row2 = row2;
			this.Row3 = row3;
		}
		/// <summary>
		/// Constructs a new instance.
		/// </summary>
		/// <param name="m00">First item of the first row of the matrix.</param>
		/// <param name="m01">Second item of the first row of the matrix.</param>
		/// <param name="m02">Third item of the first row of the matrix.</param>
		/// <param name="m03">Fourth item of the first row of the matrix.</param>
		/// <param name="m10">First item of the second row of the matrix.</param>
		/// <param name="m11">Second item of the second row of the matrix.</param>
		/// <param name="m12">Third item of the second row of the matrix.</param>
		/// <param name="m13">Fourth item of the second row of the matrix.</param>
		/// <param name="m20">First item of the third row of the matrix.</param>
		/// <param name="m21">Second item of the third row of the matrix.</param>
		/// <param name="m22">Third item of the third row of the matrix.</param>
		/// <param name="m23">First item of the third row of the matrix.</param>
		/// <param name="m30">Fourth item of the fourth row of the matrix.</param>
		/// <param name="m31">Second item of the fourth row of the matrix.</param>
		/// <param name="m32">Third item of the fourth row of the matrix.</param>
		/// <param name="m33">Fourth item of the fourth row of the matrix.</param>
		public Matrix4(
			double m00, double m01, double m02, double m03,
			double m10, double m11, double m12, double m13,
			double m20, double m21, double m22, double m23,
			double m30, double m31, double m32, double m33)
		{
			this.Row0 = new Vector4(m00, m01, m02, m03);
			this.Row1 = new Vector4(m10, m11, m12, m13);
			this.Row2 = new Vector4(m20, m21, m22, m23);
			this.Row3 = new Vector4(m30, m31, m32, m33);
		}

		#endregion


		
		#region Public Properties

		/// <summary>
		/// The backward vector formed from the third row M31, M32, M33 elements.
		/// </summary>
		public Vector3 Backward
		{
			get
			{
				return new Vector3(this.M31, this.M32, this.M33);
			}
			set
			{
				this.M31 = value.X;
				this.M32 = value.Y;
				this.M33 = value.Z;
			}
		}
		
		/// <summary>
		/// The down vector formed from the second row -M21, -M22, -M23 elements.
		/// </summary>
		public Vector3 Down
		{
			get
			{
				return new Vector3(-this.M21, -this.M22, -this.M23);
			}
			set
			{
				this.M21 = -value.X;
				this.M22 = -value.Y;
				this.M23 = -value.Z;
			}
		}
		
		/// <summary>
		/// The forward vector formed from the third row -M31, -M32, -M33 elements.
		/// </summary>
		public Vector3 Forward
		{
			get
			{
				return new Vector3(-this.M31, -this.M32, -this.M33);
			}
			set
			{
				this.M31 = -value.X;
				this.M32 = -value.Y;
				this.M33 = -value.Z;
			}
		}
		
		/// <summary>
		/// The left vector formed from the first row -M11, -M12, -M13 elements.
		/// </summary>
		public Vector3 Left
		{
			get
			{
				return new Vector3(-this.M11, -this.M12, -this.M13);
			}
			set
			{
				this.M11 = -value.X;
				this.M12 = -value.Y;
				this.M13 = -value.Z;
			}
		}
		
		/// <summary>
		/// The right vector formed from the first row M11, M12, M13 elements.
		/// </summary>
		public Vector3 Right
		{
			get
			{
				return new Vector3(this.M11, this.M12, this.M13);
			}
			set
			{
				this.M11 = value.X;
				this.M12 = value.Y;
				this.M13 = value.Z;
			}
		}
		
		/// <summary>
		/// Position stored in this matrix.
		/// </summary>
		public Vector3 Translation
		{
			get
			{
				return new Vector3(this.M41, this.M42, this.M43);
			}
			set
			{
				this.M41 = value.X;
				this.M42 = value.Y;
				this.M43 = value.Z;
			}
		}
		
		/// <summary>
		/// The upper vector formed from the second row M21, M22, M23 elements.
		/// </summary>
		public Vector3 Up
		{
			get
			{
				return new Vector3(this.M21, this.M22, this.M23);
			}
			set
			{
				this.M21 = value.X;
				this.M22 = value.Y;
				this.M23 = value.Z;
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Creates a new <see cref="Matrix4"/> which contains sum of two matrixes.
		/// </summary>
		/// <param name="matrix1">The first matrix to add.</param>
		/// <param name="matrix2">The second matrix to add.</param>
		/// <returns>The result of the matrix addition.</returns>
		public static Matrix4 Add(Matrix4 matrix1, Matrix4 matrix2)
		{
			matrix1.M11 += matrix2.M11;
			matrix1.M12 += matrix2.M12;
			matrix1.M13 += matrix2.M13;
			matrix1.M14 += matrix2.M14;
			matrix1.M21 += matrix2.M21;
			matrix1.M22 += matrix2.M22;
			matrix1.M23 += matrix2.M23;
			matrix1.M24 += matrix2.M24;
			matrix1.M31 += matrix2.M31;
			matrix1.M32 += matrix2.M32;
			matrix1.M33 += matrix2.M33;
			matrix1.M34 += matrix2.M34;
			matrix1.M41 += matrix2.M41;
			matrix1.M42 += matrix2.M42;
			matrix1.M43 += matrix2.M43;
			matrix1.M44 += matrix2.M44;
			return matrix1;
		}

		/// <summary>
		/// Creates a new <see cref="Matrix4"/> which contains sum of two matrixes.
		/// </summary>
		/// <param name="matrix1">The first matrix to add.</param>
		/// <param name="matrix2">The second matrix to add.</param>
		/// <param name="result">The result of the matrix addition as an output parameter.</param>
		public static void Add(ref Matrix4 matrix1, ref Matrix4 matrix2, out Matrix4 result)
		{
			result = new Matrix4();
			result.M11 = matrix1.M11 + matrix2.M11;
			result.M12 = matrix1.M12 + matrix2.M12;
			result.M13 = matrix1.M13 + matrix2.M13;
			result.M14 = matrix1.M14 + matrix2.M14;
			result.M21 = matrix1.M21 + matrix2.M21;
			result.M22 = matrix1.M22 + matrix2.M22;
			result.M23 = matrix1.M23 + matrix2.M23;
			result.M24 = matrix1.M24 + matrix2.M24;
			result.M31 = matrix1.M31 + matrix2.M31;
			result.M32 = matrix1.M32 + matrix2.M32;
			result.M33 = matrix1.M33 + matrix2.M33;
			result.M34 = matrix1.M34 + matrix2.M34;
			result.M41 = matrix1.M41 + matrix2.M41;
			result.M42 = matrix1.M42 + matrix2.M42;
			result.M43 = matrix1.M43 + matrix2.M43;
			result.M44 = matrix1.M44 + matrix2.M44;

		}

		/// <summary>
		/// Creates a new <see cref="Matrix4"/> for spherical billboarding that rotates around specified object position.
		/// </summary>
		/// <param name="objectPosition">Position of billboard object. It will rotate around that vector.</param>
		/// <param name="cameraPosition">The camera position.</param>
		/// <param name="cameraUpVector">The camera up vector.</param>
		/// <param name="cameraForwardVector">Optional camera forward vector.</param>
		/// <returns>The <see cref="Matrix4"/> for spherical billboarding.</returns>
		public static Matrix4 CreateBillboard(Vector3 objectPosition, Vector3 cameraPosition,
			Vector3 cameraUpVector, Nullable<Vector3> cameraForwardVector)
		{
			Matrix4 result;

			// Delegate to the other overload of the function to do the work
			CreateBillboard(ref objectPosition, ref cameraPosition, ref cameraUpVector, cameraForwardVector, out result);

			return result;
		}

		/// <summary>
		/// Creates a new <see cref="Matrix4"/> for spherical billboarding that rotates around specified object position.
		/// </summary>
		/// <param name="objectPosition">Position of billboard object. It will rotate around that vector.</param>
		/// <param name="cameraPosition">The camera position.</param>
		/// <param name="cameraUpVector">The camera up vector.</param>
		/// <param name="cameraForwardVector">Optional camera forward vector.</param>
		/// <param name="result">The <see cref="Matrix4"/> for spherical billboarding as an output parameter.</param>
		public static void CreateBillboard(ref Vector3 objectPosition, ref Vector3 cameraPosition,
			ref Vector3 cameraUpVector, Vector3? cameraForwardVector, out Matrix4 result)
		{
			Vector3 vector;
			Vector3 vector2;
			Vector3 vector3;
			vector.X = objectPosition.X - cameraPosition.X;
			vector.Y = objectPosition.Y - cameraPosition.Y;
			vector.Z = objectPosition.Z - cameraPosition.Z;
			double num = vector.LengthSquared;
			if (num < 0.0001)
			{
				vector = cameraForwardVector.HasValue ? -cameraForwardVector.Value : Vector3.Forward;
			}
			else
			{
				Vector3.Multiply(ref vector, 1 / MathF.Sqrt(num), out vector);
			}
			Vector3.Cross(ref cameraUpVector, ref vector, out vector3);
			vector3.Normalize();
			Vector3.Cross(ref vector, ref vector3, out vector2);
			result = new Matrix4();
			result.M11 = vector3.X;
			result.M12 = vector3.Y;
			result.M13 = vector3.Z;
			result.M14 = 0;
			result.M21 = vector2.X;
			result.M22 = vector2.Y;
			result.M23 = vector2.Z;
			result.M24 = 0;
			result.M31 = vector.X;
			result.M32 = vector.Y;
			result.M33 = vector.Z;
			result.M34 = 0;
			result.M41 = objectPosition.X;
			result.M42 = objectPosition.Y;
			result.M43 = objectPosition.Z;
			result.M44 = 1;
		}

		/// <summary>
		/// Creates a new <see cref="Matrix4"/> for cylindrical billboarding that rotates around specified axis.
		/// </summary>
		/// <param name="objectPosition">Object position the billboard will rotate around.</param>
		/// <param name="cameraPosition">Camera position.</param>
		/// <param name="rotateAxis">Axis of billboard for rotation.</param>
		/// <param name="cameraForwardVector">Optional camera forward vector.</param>
		/// <param name="objectForwardVector">Optional object forward vector.</param>
		/// <returns>The <see cref="Matrix4"/> for cylindrical billboarding.</returns>
		public static Matrix4 CreateConstrainedBillboard(Vector3 objectPosition, Vector3 cameraPosition,
			Vector3 rotateAxis, Nullable<Vector3> cameraForwardVector, Nullable<Vector3> objectForwardVector)
		{
			Matrix4 result;
			CreateConstrainedBillboard(ref objectPosition, ref cameraPosition, ref rotateAxis,
				cameraForwardVector, objectForwardVector, out result);
			return result;
		}

		/// <summary>
		/// Creates a new <see cref="Matrix4"/> for cylindrical billboarding that rotates around specified axis.
		/// </summary>
		/// <param name="objectPosition">Object position the billboard will rotate around.</param>
		/// <param name="cameraPosition">Camera position.</param>
		/// <param name="rotateAxis">Axis of billboard for rotation.</param>
		/// <param name="cameraForwardVector">Optional camera forward vector.</param>
		/// <param name="objectForwardVector">Optional object forward vector.</param>
		/// <param name="result">The <see cref="Matrix4"/> for cylindrical billboarding as an output parameter.</param>
		public static void CreateConstrainedBillboard(ref Vector3 objectPosition, ref Vector3 cameraPosition,
			ref Vector3 rotateAxis, Vector3? cameraForwardVector, Vector3? objectForwardVector, out Matrix4 result)
		{
			double num;
			Vector3 vector;
			Vector3 vector2;
			Vector3 vector3;
			vector2.X = objectPosition.X - cameraPosition.X;
			vector2.Y = objectPosition.Y - cameraPosition.Y;
			vector2.Z = objectPosition.Z - cameraPosition.Z;
			double num2 = vector2.LengthSquared;
			if (num2 < 0.0001)
			{
				vector2 = cameraForwardVector.HasValue ? -cameraForwardVector.Value : Vector3.Forward;
			}
			else
			{
				Vector3.Multiply(ref vector2, 1 / MathF.Sqrt(num2), out vector2);
			}
			Vector3 vector4 = rotateAxis;
			Vector3.Dot(ref rotateAxis, ref vector2, out num);
			if (Math.Abs(num) > 0.9982547)
			{
				if (objectForwardVector.HasValue)
				{
					vector = objectForwardVector.Value;
					Vector3.Dot(ref rotateAxis, ref vector, out num);
					if (Math.Abs(num) > 0.9982547)
					{
						num = ((rotateAxis.X * Vector3.Forward.X) + (rotateAxis.Y * Vector3.Forward.Y)) + (rotateAxis.Z * Vector3.Forward.Z);
						vector = (Math.Abs(num) > 0.9982547) ? Vector3.Right : Vector3.Forward;
					}
				}
				else
				{
					num = ((rotateAxis.X * Vector3.Forward.X) + (rotateAxis.Y * Vector3.Forward.Y)) + (rotateAxis.Z * Vector3.Forward.Z);
					vector = (Math.Abs(num) > 0.9982547) ? Vector3.Right : Vector3.Forward;
				}
				Vector3.Cross(ref rotateAxis, ref vector, out vector3);
				vector3.Normalize();
				Vector3.Cross(ref vector3, ref rotateAxis, out vector);
				vector.Normalize();
			}
			else
			{
				Vector3.Cross(ref rotateAxis, ref vector2, out vector3);
				vector3.Normalize();
				Vector3.Cross(ref vector3, ref vector4, out vector);
				vector.Normalize();
			}
			result = new Matrix4();
			result.M11 = vector3.X;
			result.M12 = vector3.Y;
			result.M13 = vector3.Z;
			result.M14 = 0;
			result.M21 = vector4.X;
			result.M22 = vector4.Y;
			result.M23 = vector4.Z;
			result.M24 = 0;
			result.M31 = vector.X;
			result.M32 = vector.Y;
			result.M33 = vector.Z;
			result.M34 = 0;
			result.M41 = objectPosition.X;
			result.M42 = objectPosition.Y;
			result.M43 = objectPosition.Z;
			result.M44 = 1;

		}

		/// <summary>
		/// Creates a new <see cref="Matrix4"/> which contains the rotation moment around specified axis.
		/// </summary>
		/// <param name="axis">The axis of rotation.</param>
		/// <param name="angle">The angle of rotation in radians.</param>
		/// <returns>The rotation <see cref="Matrix4"/>.</returns>
		public static Matrix4 CreateFromAxisAngle(Vector3 axis, double angle)
		{
			Matrix4 result;
			CreateFromAxisAngle(ref axis, angle, out result);
			return result;
		}

		/// <summary>
		/// Creates a new <see cref="Matrix4"/> which contains the rotation moment around specified axis.
		/// </summary>
		/// <param name="axis">The axis of rotation.</param>
		/// <param name="angle">The angle of rotation in radians.</param>
		/// <param name="result">The rotation <see cref="Matrix4"/> as an output parameter.</param>
		public static void CreateFromAxisAngle(ref Vector3 axis, double angle, out Matrix4 result)
		{
			double x = axis.X;
			double y = axis.Y;
			double z = axis.Z;
			double num2 = MathF.Sin(angle);
			double num = MathF.Cos(angle);
			double num11 = x * x;
			double num10 = y * y;
			double num9 = z * z;
			double num8 = x * y;
			double num7 = x * z;
			double num6 = y * z;
			result = new Matrix4();
			result.M11 = num11 + (num * (1 - num11));
			result.M12 = (num8 - (num * num8)) + (num2 * z);
			result.M13 = (num7 - (num * num7)) - (num2 * y);
			result.M14 = 0;
			result.M21 = (num8 - (num * num8)) - (num2 * z);
			result.M22 = num10 + (num * (1 - num10));
			result.M23 = (num6 - (num * num6)) + (num2 * x);
			result.M24 = 0;
			result.M31 = (num7 - (num * num7)) + (num2 * y);
			result.M32 = (num6 - (num * num6)) - (num2 * x);
			result.M33 = num9 + (num * (1 - num9));
			result.M34 = 0;
			result.M41 = 0;
			result.M42 = 0;
			result.M43 = 0;
			result.M44 = 1;
		}

		/// <summary>
		/// Creates a new rotation <see cref="Matrix4"/> from a <see cref="Quaternion"/>.
		/// </summary>
		/// <param name="quaternion"><see cref="Quaternion"/> of rotation moment.</param>
		/// <returns>The rotation <see cref="Matrix4"/>.</returns>
		public static Matrix4 CreateFromQuaternion(Quaternion quaternion)
		{
			Matrix4 result;
			CreateFromQuaternion(ref quaternion, out result);
			return result;
		}

		/// <summary>
		/// Creates a new rotation <see cref="Matrix4"/> from a <see cref="Quaternion"/>.
		/// </summary>
		/// <param name="quaternion"><see cref="Quaternion"/> of rotation moment.</param>
		/// <param name="result">The rotation <see cref="Matrix4"/> as an output parameter.</param>
		public static void CreateFromQuaternion(ref Quaternion quaternion, out Matrix4 result)
		{
			double num9 = quaternion.X * quaternion.X;
			double num8 = quaternion.Y * quaternion.Y;
			double num7 = quaternion.Z * quaternion.Z;
			double num6 = quaternion.X * quaternion.Y;
			double num5 = quaternion.Z * quaternion.W;
			double num4 = quaternion.Z * quaternion.X;
			double num3 = quaternion.Y * quaternion.W;
			double num2 = quaternion.Y * quaternion.Z;
			double num = quaternion.X * quaternion.W;
			result = new Matrix4();
			result.M11 = 1 - (2 * (num8 + num7));
			result.M12 = 2 * (num6 + num5);
			result.M13 = 2 * (num4 - num3);
			result.M14 = 0;
			result.M21 = 2 * (num6 - num5);
			result.M22 = 1 - (2 * (num7 + num9));
			result.M23 = 2 * (num2 + num);
			result.M24 = 0;
			result.M31 = 2 * (num4 + num3);
			result.M32 = 2 * (num2 - num);
			result.M33 = 1 - (2 * (num8 + num9));
			result.M34 = 0;
			result.M41 = 0;
			result.M42 = 0;
			result.M43 = 0;
			result.M44 = 1;
		}

		public static Matrix4 Rotate(Quaternion q)
		{
			Vector3 axis;
			double angle;
			q.ToAxisAngle(out axis, out angle);
			return CreateFromAxisAngle(axis, angle);
		}

		/// <summary>
		/// Creates a new rotation <see cref="Matrix4"/> from the specified yaw, pitch and roll values.
		/// </summary>
		/// <param name="yaw">The yaw rotation value in radians.</param>
		/// <param name="pitch">The pitch rotation value in radians.</param>
		/// <param name="roll">The roll rotation value in radians.</param>
		/// <returns>The rotation <see cref="Matrix4"/>.</returns>
		/// <remarks>For more information about yaw, pitch and roll visit http://en.wikipedia.org/wiki/Euler_angles.
		/// </remarks>
		public static Matrix4 CreateFromYawPitchRoll(double yaw, double pitch, double roll)
		{
			Matrix4 matrix;
			CreateFromYawPitchRoll(yaw, pitch, roll, out matrix);
			return matrix;
		}

		/// <summary>
		/// Creates a new rotation <see cref="Matrix4"/> from the specified yaw, pitch and roll values.
		/// </summary>
		/// <param name="yaw">The yaw rotation value in radians.</param>
		/// <param name="pitch">The pitch rotation value in radians.</param>
		/// <param name="roll">The roll rotation value in radians.</param>
		/// <param name="result">The rotation <see cref="Matrix4"/> as an output parameter.</param>
		/// <remarks>For more information about yaw, pitch and roll visit http://en.wikipedia.org/wiki/Euler_angles.
		/// </remarks>
		public static void CreateFromYawPitchRoll(double yaw, double pitch, double roll, out Matrix4 result)
		{
			Quaternion quaternion;
			Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll, out quaternion);
			CreateFromQuaternion(ref quaternion, out result);
		}

		/// <summary>
		/// Creates a new viewing <see cref="Matrix4"/>.
		/// </summary>
		/// <param name="cameraPosition">Position of the camera.</param>
		/// <param name="cameraTarget">Lookup vector of the camera.</param>
		/// <param name="cameraUpVector">The direction of the upper edge of the camera.</param>
		/// <returns>The viewing <see cref="Matrix4"/>.</returns>
		public static Matrix4 CreateLookAt(Vector3 cameraPosition, Vector3 cameraTarget, Vector3 cameraUpVector)
		{
			Matrix4 matrix;
			CreateLookAt(ref cameraPosition, ref cameraTarget, ref cameraUpVector, out matrix);
			return matrix;
		}

		public static void ExtractRotation(Matrix4 matrix, ref Quaternion rotation)
		{
			if (double.IsNaN(matrix.M11))
				return;

			double sx = (Math.Sign(matrix.M11 * matrix.M12 * matrix.M13 * matrix.M14) < 0) ? -1.0 : 1.0;
			double sy = (Math.Sign(matrix.M21 * matrix.M22 * matrix.M23 * matrix.M24) < 0) ? -1.0 : 1.0;
			double sz = (Math.Sign(matrix.M31 * matrix.M32 * matrix.M33 * matrix.M34) < 0) ? -1.0 : 1.0;

			sx *= (double)Math.Sqrt(matrix.M11 * matrix.M11 + matrix.M12 * matrix.M12 + matrix.M13 * matrix.M13);
			sy *= (double)Math.Sqrt(matrix.M21 * matrix.M21 + matrix.M22 * matrix.M22 + matrix.M23 * matrix.M23);
			sz *= (double)Math.Sqrt(matrix.M31 * matrix.M31 + matrix.M32 * matrix.M32 + matrix.M33 * matrix.M33);

			if (sx == 0.0 || sy == 0.0 || sz == 0.0)
			{
				rotation = Quaternion.Identity;
				return;
			}

			var m = new Matrix4(matrix.M11 / sx, matrix.M12 / sx, matrix.M13 / sx, 0.0,
								   matrix.M21 / sy, matrix.M22 / sy, matrix.M23 / sy, 0.0,
								   matrix.M31 / sz, matrix.M32 / sz, matrix.M33 / sz, 0.0,
								   0.0, 0.0, 0.0, 1.0);

			rotation = Quaternion.CreateFromRotationMatrix(m);
		}

		/// <summary>
		/// Creates a new viewing <see cref="Matrix4"/>.
		/// </summary>
		/// <param name="cameraPosition">Position of the camera.</param>
		/// <param name="cameraTarget">Lookup vector of the camera.</param>
		/// <param name="cameraUpVector">The direction of the upper edge of the camera.</param>
		/// <param name="result">The viewing <see cref="Matrix4"/> as an output parameter.</param>
		public static void CreateLookAt(ref Vector3 cameraPosition, ref Vector3 cameraTarget, ref Vector3 cameraUpVector, out Matrix4 result)
		{
			var vector = Vector3.Normalize(cameraPosition - cameraTarget);
			var vector2 = Vector3.Normalize(Vector3.Cross(cameraUpVector, vector));
			var vector3 = Vector3.Cross(vector, vector2);
			result = new Matrix4();
			result.M11 = vector2.X;
			result.M12 = vector3.X;
			result.M13 = vector.X;
			result.M14 = 0;
			result.M21 = vector2.Y;
			result.M22 = vector3.Y;
			result.M23 = vector.Y;
			result.M24 = 0;
			result.M31 = vector2.Z;
			result.M32 = vector3.Z;
			result.M33 = vector.Z;
			result.M34 = 0;
			result.M41 = -Vector3.Dot(vector2, cameraPosition);
			result.M42 = -Vector3.Dot(vector3, cameraPosition);
			result.M43 = -Vector3.Dot(vector, cameraPosition);
			result.M44 = 1;
		}

		/// <summary>
		/// Creates a new projection <see cref="Matrix4"/> for orthographic view.
		/// </summary>
		/// <param name="width">Width of the viewing volume.</param>
		/// <param name="height">Height of the viewing volume.</param>
		/// <param name="zNearPlane">Depth of the near plane.</param>
		/// <param name="zFarPlane">Depth of the far plane.</param>
		/// <returns>The new projection <see cref="Matrix4"/> for orthographic view.</returns>
		public static Matrix4 CreateOrthographic(double width, double height, double zNearPlane, double zFarPlane)
		{
			Matrix4 matrix;
			CreateOrthographic(width, height, zNearPlane, zFarPlane, out matrix);
			return matrix;
		}

		/// <summary>
		/// Creates a new projection <see cref="Matrix4"/> for orthographic view.
		/// </summary>
		/// <param name="width">Width of the viewing volume.</param>
		/// <param name="height">Height of the viewing volume.</param>
		/// <param name="zNearPlane">Depth of the near plane.</param>
		/// <param name="zFarPlane">Depth of the far plane.</param>
		/// <param name="result">The new projection <see cref="Matrix4"/> for orthographic view as an output parameter.</param>
		public static void CreateOrthographic(double width, double height, double zNearPlane, double zFarPlane, out Matrix4 result)
		{
			result = new Matrix4();
			result.M11 = 2 / width;
			result.M12 = result.M13 = result.M14 = 0;
			result.M22 = 2 / height;
			result.M21 = result.M23 = result.M24 = 0;
			result.M33 = 1 / (zNearPlane - zFarPlane);
			result.M31 = result.M32 = result.M34 = 0;
			result.M41 = result.M42 = 0;
			result.M43 = zNearPlane / (zNearPlane - zFarPlane);
			result.M44 = 1;
		}

		/// <summary>
		/// Creates a new projection <see cref="Matrix4"/> for customized orthographic view.
		/// </summary>
		/// <param name="left">Lower x-value at the near plane.</param>
		/// <param name="right">Upper x-value at the near plane.</param>
		/// <param name="bottom">Lower y-coordinate at the near plane.</param>
		/// <param name="top">Upper y-value at the near plane.</param>
		/// <param name="zNearPlane">Depth of the near plane.</param>
		/// <param name="zFarPlane">Depth of the far plane.</param>
		/// <param name="result">The new projection <see cref="Matrix4"/> for customized orthographic view as an output parameter.</param>
		public static void CreateOrthographicOffCenter(double left, double right, double bottom, double top, double zNearPlane, double zFarPlane, out Matrix4 result)
		{
			result = new Matrix4();
			result.M11 = (double)(2.0 / ((double)right - (double)left));
			result.M12 = 0.0;
			result.M13 = 0.0;
			result.M14 = 0.0;
			result.M21 = 0.0;
			result.M22 = (double)(2.0 / ((double)top - (double)bottom));
			result.M23 = 0.0;
			result.M24 = 0.0;
			result.M31 = 0.0;
			result.M32 = 0.0;
			result.M33 = (double)(1.0 / ((double)zNearPlane - (double)zFarPlane));
			result.M34 = 0.0;
			result.M41 = (double)(((double)left + (double)right) / ((double)left - (double)right));
			result.M42 = (double)(((double)top + (double)bottom) / ((double)bottom - (double)top));
			result.M43 = (double)((double)zNearPlane / ((double)zNearPlane - (double)zFarPlane));
			result.M44 = 1.0;
		}

		/// <summary>
		/// Creates a new projection <see cref="Matrix4"/> for perspective view.
		/// </summary>
		/// <param name="width">Width of the viewing volume.</param>
		/// <param name="height">Height of the viewing volume.</param>
		/// <param name="nearPlaneDistance">Distance to the near plane.</param>
		/// <param name="farPlaneDistance">Distance to the far plane.</param>
		/// <returns>The new projection <see cref="Matrix4"/> for perspective view.</returns>
		public static Matrix4 CreatePerspective(double width, double height, double nearPlaneDistance, double farPlaneDistance)
		{
			Matrix4 matrix;
			CreatePerspective(width, height, nearPlaneDistance, farPlaneDistance, out matrix);
			return matrix;
		}

		/// <summary>
		/// Creates a new projection <see cref="Matrix4"/> for perspective view.
		/// </summary>
		/// <param name="width">Width of the viewing volume.</param>
		/// <param name="height">Height of the viewing volume.</param>
		/// <param name="nearPlaneDistance">Distance to the near plane.</param>
		/// <param name="farPlaneDistance">Distance to the far plane, or <see cref="double.PositiveInfinity"/>.</param>
		/// <param name="result">The new projection <see cref="Matrix4"/> for perspective view as an output parameter.</param>
		public static void CreatePerspective(double width, double height, double nearPlaneDistance, double farPlaneDistance, out Matrix4 result)
		{
			if (nearPlaneDistance <= 0)
			{
				throw new ArgumentException("nearPlaneDistance <= 0");
			}
			if (farPlaneDistance <= 0)
			{
				throw new ArgumentException("farPlaneDistance <= 0");
			}
			if (nearPlaneDistance >= farPlaneDistance)
			{
				throw new ArgumentException("nearPlaneDistance >= farPlaneDistance");
			}

			double negFarRange = double.IsPositiveInfinity(farPlaneDistance) ? -1.0 : farPlaneDistance / (nearPlaneDistance - farPlaneDistance);

			result = new Matrix4();
			result.M11 = (2.0 * nearPlaneDistance) / width;
			result.M12 = result.M13 = result.M14 = 0.0;
			result.M22 = (2.0 * nearPlaneDistance) / height;
			result.M21 = result.M23 = result.M24 = 0.0;
			result.M33 = negFarRange;
			result.M31 = result.M32 = 0.0;
			result.M34 = -1.0;
			result.M41 = result.M42 = result.M44 = 0.0;
			result.M43 = nearPlaneDistance * negFarRange;
		}

		/// <summary>
		/// Creates a new projection <see cref="Matrix4"/> for perspective view with field of view.
		/// </summary>
		/// <param name="fieldOfView">Field of view in the y direction in radians.</param>
		/// <param name="aspectRatio">Width divided by height of the viewing volume.</param>
		/// <param name="nearPlaneDistance">Distance to the near plane.</param>
		/// <param name="farPlaneDistance">Distance to the far plane, or <see cref="double.PositiveInfinity"/>.</param>
		/// <returns>The new projection <see cref="Matrix4"/> for perspective view with FOV.</returns>
		public static Matrix4 CreatePerspectiveFieldOfView(double fieldOfView, double aspectRatio, double nearPlaneDistance, double farPlaneDistance)
		{
			Matrix4 result;
			CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, nearPlaneDistance, farPlaneDistance, out result);
			return result;
		}

		/// <summary>
		/// Creates a new projection <see cref="Matrix4"/> for perspective view with field of view.
		/// </summary>
		/// <param name="fieldOfView">Field of view in the y direction in radians.</param>
		/// <param name="aspectRatio">Width divided by height of the viewing volume.</param>
		/// <param name="nearPlaneDistance">Distance of the near plane.</param>
		/// <param name="farPlaneDistance">Distance of the far plane, or <see cref="double.PositiveInfinity"/>.</param>
		/// <param name="result">The new projection <see cref="Matrix4"/> for perspective view with FOV as an output parameter.</param>
		public static void CreatePerspectiveFieldOfView(double fieldOfView, double aspectRatio, double nearPlaneDistance, double farPlaneDistance, out Matrix4 result)
		{
			if ((fieldOfView <= 0) || (fieldOfView >= 3.141593))
			{
				throw new ArgumentException("fieldOfView <= 0 or >= PI");
			}
			if (nearPlaneDistance <= 0)
			{
				throw new ArgumentException("nearPlaneDistance <= 0");
			}
			if (farPlaneDistance <= 0)
			{
				throw new ArgumentException("farPlaneDistance <= 0");
			}
			if (nearPlaneDistance >= farPlaneDistance)
			{
				throw new ArgumentException("nearPlaneDistance >= farPlaneDistance");
			}

			double yScale = 1.0 / (double)Math.Tan((double)fieldOfView * 0.5);
			double xScale = yScale / aspectRatio;
			double negFarRange = double.IsPositiveInfinity(farPlaneDistance) ? -1.0 : farPlaneDistance / (nearPlaneDistance - farPlaneDistance);

			result = new Matrix4();
			result.M11 = xScale;
			result.M12 = result.M13 = result.M14 = 0.0;
			result.M22 = yScale;
			result.M21 = result.M23 = result.M24 = 0.0;
			result.M31 = result.M32 = 0.0;
			result.M33 = negFarRange;
			result.M34 = -1.0;
			result.M41 = result.M42 = result.M44 = 0.0;
			result.M43 = nearPlaneDistance * negFarRange;
		}

		/// <summary>
		/// Creates a new projection <see cref="Matrix4"/> for customized perspective view.
		/// </summary>
		/// <param name="left">Lower x-value at the near plane.</param>
		/// <param name="right">Upper x-value at the near plane.</param>
		/// <param name="bottom">Lower y-coordinate at the near plane.</param>
		/// <param name="top">Upper y-value at the near plane.</param>
		/// <param name="nearPlaneDistance">Distance to the near plane.</param>
		/// <param name="farPlaneDistance">Distance to the far plane.</param>
		/// <param name="result">The new <see cref="Matrix4"/> for customized perspective view as an output parameter.</param>
		public static void CreatePerspectiveOffCenter(double left, double right, double bottom, double top, double nearPlaneDistance, double farPlaneDistance, out Matrix4 result)
		{
			if (nearPlaneDistance <= 0)
			{
				throw new ArgumentException("nearPlaneDistance <= 0");
			}
			if (farPlaneDistance <= 0)
			{
				throw new ArgumentException("farPlaneDistance <= 0");
			}
			if (nearPlaneDistance >= farPlaneDistance)
			{
				throw new ArgumentException("nearPlaneDistance >= farPlaneDistance");
			}
			result = new Matrix4();
			result.M11 = (2 * nearPlaneDistance) / (right - left);
			result.M12 = result.M13 = result.M14 = 0;
			result.M22 = (2 * nearPlaneDistance) / (top - bottom);
			result.M21 = result.M23 = result.M24 = 0;
			result.M31 = (left + right) / (right - left);
			result.M32 = (top + bottom) / (top - bottom);
			result.M33 = farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
			result.M34 = -1;
			result.M43 = (nearPlaneDistance * farPlaneDistance) / (nearPlaneDistance - farPlaneDistance);
			result.M41 = result.M42 = result.M44 = 0;
		}

		/// <summary>
		/// Creates a new rotation <see cref="Matrix4"/> around X axis.
		/// </summary>
		/// <param name="radians">Angle in radians.</param>
		/// <returns>The rotation <see cref="Matrix4"/> around X axis.</returns>
		public static Matrix4 CreateRotationX(double radians)
		{
			Matrix4 result;
			CreateRotationX(radians, out result);
			return result;
		}

		/// <summary>
		/// Creates a new rotation <see cref="Matrix4"/> around X axis.
		/// </summary>
		/// <param name="radians">Angle in radians.</param>
		/// <param name="result">The rotation <see cref="Matrix4"/> around X axis as an output parameter.</param>
		public static void CreateRotationX(double radians, out Matrix4 result)
		{
			result = Matrix4.Identity;

			double val1 = MathF.Cos(radians);
			double val2 = MathF.Sin(radians);

			result.M22 = val1;
			result.M23 = val2;
			result.M32 = -val2;
			result.M33 = val1;
		}

		/// <summary>
		/// Creates a new rotation <see cref="Matrix4"/> around Y axis.
		/// </summary>
		/// <param name="radians">Angle in radians.</param>
		/// <returns>The rotation <see cref="Matrix4"/> around Y axis.</returns>
		public static Matrix4 CreateRotationY(double radians)
		{
			Matrix4 result;
			CreateRotationY(radians, out result);
			return result;
		}

		/// <summary>
		/// Creates a new rotation <see cref="Matrix4"/> around Y axis.
		/// </summary>
		/// <param name="radians">Angle in radians.</param>
		/// <param name="result">The rotation <see cref="Matrix4"/> around Y axis as an output parameter.</param>
		public static void CreateRotationY(double radians, out Matrix4 result)
		{
			result = Matrix4.Identity;

			double val1 = MathF.Cos(radians);
			double val2 = MathF.Sin(radians);

			result.M11 = val1;
			result.M13 = -val2;
			result.M31 = val2;
			result.M33 = val1;
		}

		/// <summary>
		/// Creates a new rotation <see cref="Matrix4"/> around Z axis.
		/// </summary>
		/// <param name="radians">Angle in radians.</param>
		/// <returns>The rotation <see cref="Matrix4"/> around Z axis.</returns>
		public static Matrix4 CreateRotationZ(double radians)
		{
			Matrix4 result;
			CreateRotationZ(radians, out result);
			return result;
		}

		/// <summary>
		/// Creates a new rotation <see cref="Matrix4"/> around Z axis.
		/// </summary>
		/// <param name="radians">Angle in radians.</param>
		/// <param name="result">The rotation <see cref="Matrix4"/> around Z axis as an output parameter.</param>
		public static void CreateRotationZ(double radians, out Matrix4 result)
		{
			result = Matrix4.Identity;

			double val1 = MathF.Cos(radians);
			double val2 = MathF.Sin(radians);

			result.M11 = val1;
			result.M12 = val2;
			result.M21 = -val2;
			result.M22 = val1;
		}

		/// <summary>
		/// Creates a new scaling <see cref="Matrix4"/>.
		/// </summary>
		/// <param name="scale">Scale value for all three axises.</param>
		/// <returns>The scaling <see cref="Matrix4"/>.</returns>
		public static Matrix4 CreateScale(double scale)
		{
			Matrix4 result;
			CreateScale(scale, scale, scale, out result);
			return result;
		}

		/// <summary>
		/// Creates a new scaling <see cref="Matrix4"/>.
		/// </summary>
		/// <param name="scale">Scale value for all three axises.</param>
		/// <param name="result">The scaling <see cref="Matrix4"/> as an output parameter.</param>
		public static void CreateScale(double scale, out Matrix4 result)
		{
			CreateScale(scale, scale, scale, out result);
		}

		/// <summary>
		/// Creates a new scaling <see cref="Matrix4"/>.
		/// </summary>
		/// <param name="xScale">Scale value for X axis.</param>
		/// <param name="yScale">Scale value for Y axis.</param>
		/// <param name="zScale">Scale value for Z axis.</param>
		/// <returns>The scaling <see cref="Matrix4"/>.</returns>
		public static Matrix4 CreateScale(double xScale, double yScale, double zScale)
		{
			Matrix4 result;
			CreateScale(xScale, yScale, zScale, out result);
			return result;
		}

		/// <summary>
		/// Creates a new scaling <see cref="Matrix4"/>.
		/// </summary>
		/// <param name="xScale">Scale value for X axis.</param>
		/// <param name="yScale">Scale value for Y axis.</param>
		/// <param name="zScale">Scale value for Z axis.</param>
		/// <param name="result">The scaling <see cref="Matrix4"/> as an output parameter.</param>
		public static void CreateScale(double xScale, double yScale, double zScale, out Matrix4 result)
		{
			result = new Matrix4();
			result.M11 = xScale;
			result.M12 = 0;
			result.M13 = 0;
			result.M14 = 0;
			result.M21 = 0;
			result.M22 = yScale;
			result.M23 = 0;
			result.M24 = 0;
			result.M31 = 0;
			result.M32 = 0;
			result.M33 = zScale;
			result.M34 = 0;
			result.M41 = 0;
			result.M42 = 0;
			result.M43 = 0;
			result.M44 = 1;
		}

		/// <summary>
		/// Creates a new scaling <see cref="Matrix4"/>.
		/// </summary>
		/// <param name="scales"><see cref="Vector3"/> representing x,y and z scale values.</param>
		/// <returns>The scaling <see cref="Matrix4"/>.</returns>
		public static Matrix4 CreateScale(Vector3 scales)
		{
			Matrix4 result;
			CreateScale(ref scales, out result);
			return result;
		}

		/// <summary>
		/// Creates a new scaling <see cref="Matrix4"/>.
		/// </summary>
		/// <param name="scales"><see cref="Vector3"/> representing x,y and z scale values.</param>
		/// <param name="result">The scaling <see cref="Matrix4"/> as an output parameter.</param>
		public static void CreateScale(ref Vector3 scales, out Matrix4 result)
		{
			result = new Matrix4();
			result.M11 = scales.X;
			result.M12 = 0;
			result.M13 = 0;
			result.M14 = 0;
			result.M21 = 0;
			result.M22 = scales.Y;
			result.M23 = 0;
			result.M24 = 0;
			result.M31 = 0;
			result.M32 = 0;
			result.M33 = scales.Z;
			result.M34 = 0;
			result.M41 = 0;
			result.M42 = 0;
			result.M43 = 0;
			result.M44 = 1;
		}

		public static Matrix4 TRS(Vector3 position, Vector3 rotationAngles, Vector3 scale)
		{
			return CreateTranslation(position) * CreateFromYawPitchRoll(rotationAngles.X, rotationAngles.Y, rotationAngles.Z) * CreateScale(scale);
		}

		/// <summary>
		/// Creates a new <see cref="Matrix4"/> that flattens geometry into a specified <see cref="Plane"/> as if casting a shadow from a specified light source. 
		/// </summary>
		/// <param name="lightDirection">A vector specifying the direction from which the light that will cast the shadow is coming.</param>
		/// <param name="plane">The plane onto which the new matrix should flatten geometry so as to cast a shadow.</param>
		/// <returns>A <see cref="Matrix4"/> that can be used to flatten geometry onto the specified plane from the specified direction. </returns>
		public static Matrix4 CreateShadow(Vector3 lightDirection, Plane plane)
		{
			Matrix4 result;
			CreateShadow(ref lightDirection, ref plane, out result);
			return result;
		}
		
		
		/// <summary>
		/// Creates a new <see cref="Matrix4"/> that flattens geometry into a specified <see cref="Plane"/> as if casting a shadow from a specified light source. 
		/// </summary>
		/// <param name="lightDirection">A vector specifying the direction from which the light that will cast the shadow is coming.</param>
		/// <param name="plane">The plane onto which the new matrix should flatten geometry so as to cast a shadow.</param>
		/// <param name="result">A <see cref="Matrix4"/> that can be used to flatten geometry onto the specified plane from the specified direction as an output parameter.</param>
		public static void CreateShadow(ref Vector3 lightDirection, ref Plane plane, out Matrix4 result)
		{
			double dot = (plane.Normal.X * lightDirection.X) + (plane.Normal.Y * lightDirection.Y) + (plane.Normal.Z * lightDirection.Z);
			double x = -plane.Normal.X;
			double y = -plane.Normal.Y;
			double z = -plane.Normal.Z;
			double d = -plane.D;

			result = new Matrix4();
			result.M11 = (x * lightDirection.X) + dot;
			result.M12 = x * lightDirection.Y;
			result.M13 = x * lightDirection.Z;
			result.M14 = 0;
			result.M21 = y * lightDirection.X;
			result.M22 = (y * lightDirection.Y) + dot;
			result.M23 = y * lightDirection.Z;
			result.M24 = 0;
			result.M31 = z * lightDirection.X;
			result.M32 = z * lightDirection.Y;
			result.M33 = (z * lightDirection.Z) + dot;
			result.M34 = 0;
			result.M41 = d * lightDirection.X;
			result.M42 = d * lightDirection.Y;
			result.M43 = d * lightDirection.Z;
			result.M44 = dot;
		}

		/// <summary>
		/// Creates a new translation <see cref="Matrix"/>.
		/// </summary>
		/// <param name="xPosition">X coordinate of translation.</param>
		/// <param name="yPosition">Y coordinate of translation.</param>
		/// <param name="zPosition">Z coordinate of translation.</param>
		/// <returns>The translation <see cref="Matrix"/>.</returns>
		public static Matrix4 CreateTranslation(double xPosition, double yPosition, double zPosition)
		{
			Matrix4 result;
			CreateTranslation(xPosition, yPosition, zPosition, out result);
			return result;
		}

		/// <summary>
		/// Creates a new translation <see cref="Matrix4"/>.
		/// </summary>
		/// <param name="position">X,Y and Z coordinates of translation.</param>
		/// <param name="result">The translation <see cref="Matrix4"/> as an output parameter.</param>
		public static void CreateTranslation(ref Vector3 position, out Matrix4 result)
		{
			result = new Matrix4();
			result.M11 = 1;
			result.M12 = 0;
			result.M13 = 0;
			result.M14 = 0;
			result.M21 = 0;
			result.M22 = 1;
			result.M23 = 0;
			result.M24 = 0;
			result.M31 = 0;
			result.M32 = 0;
			result.M33 = 1;
			result.M34 = 0;
			result.M41 = position.X;
			result.M42 = position.Y;
			result.M43 = position.Z;
			result.M44 = 1;
		}

		/// <summary>
		/// Creates a new translation <see cref="Matrix4"/>.
		/// </summary>
		/// <param name="position">X,Y and Z coordinates of translation.</param>
		/// <returns>The translation <see cref="Matrix4"/>.</returns>
		public static Matrix4 CreateTranslation(Vector3 position)
		{
			Matrix4 result;
			CreateTranslation(ref position, out result);
			return result;
		}

		/// <summary>
		/// Creates a new translation <see cref="Matrix4"/>.
		/// </summary>
		/// <param name="xPosition">X coordinate of translation.</param>
		/// <param name="yPosition">Y coordinate of translation.</param>
		/// <param name="zPosition">Z coordinate of translation.</param>
		/// <param name="result">The translation <see cref="Matrix4"/> as an output parameter.</param>
		public static void CreateTranslation(double xPosition, double yPosition, double zPosition, out Matrix4 result)
		{
			result = new Matrix4();
			result.M11 = 1;
			result.M12 = 0;
			result.M13 = 0;
			result.M14 = 0;
			result.M21 = 0;
			result.M22 = 1;
			result.M23 = 0;
			result.M24 = 0;
			result.M31 = 0;
			result.M32 = 0;
			result.M33 = 1;
			result.M34 = 0;
			result.M41 = xPosition;
			result.M42 = yPosition;
			result.M43 = zPosition;
			result.M44 = 1;
		}

		/// <summary>
		/// Creates a new reflection <see cref="Matrix4"/>.
		/// </summary>
		/// <param name="value">The plane that used for reflection calculation.</param>
		/// <returns>The reflection <see cref="Matrix4"/>.</returns>
		public static Matrix4 CreateReflection(Plane value)
		{
			Matrix4 result;
			CreateReflection(ref value, out result);
			return result;
		}
		
		/// <summary>
		/// Creates a new reflection <see cref="Matrix4"/>.
		/// </summary>
		/// <param name="value">The plane that used for reflection calculation.</param>
		/// <param name="result">The reflection <see cref="Matrix4"/> as an output parameter.</param>
		public static void CreateReflection(ref Plane value, out Matrix4 result)
		{
			Plane plane;
			Plane.Normalize(ref value, out plane);
			double x = plane.Normal.X;
			double y = plane.Normal.Y;
			double z = plane.Normal.Z;
			double num3 = -2 * x;
			double num2 = -2 * y;
			double num = -2 * z;

			result = new Matrix4();
			result.M11 = (num3 * x) + 1;
			result.M12 = num2 * x;
			result.M13 = num * x;
			result.M14 = 0;
			result.M21 = num3 * y;
			result.M22 = (num2 * y) + 1;
			result.M23 = num * y;
			result.M24 = 0;
			result.M31 = num3 * z;
			result.M32 = num2 * z;
			result.M33 = (num * z) + 1;
			result.M34 = 0;
			result.M41 = num3 * plane.D;
			result.M42 = num2 * plane.D;
			result.M43 = num * plane.D;
			result.M44 = 1;
		}

		/// <summary>
		/// Creates a new world <see cref="Matrix4"/>.
		/// </summary>
		/// <param name="position">The position vector.</param>
		/// <param name="forward">The forward direction vector.</param>
		/// <param name="up">The upward direction vector. Usually <see cref="Vector3.Up"/>.</param>
		/// <returns>The world <see cref="Matrix4"/>.</returns>
		public static Matrix4 CreateWorld(Vector3 position, Vector3 forward, Vector3 up)
		{
			Matrix4 ret;
			CreateWorld(ref position, ref forward, ref up, out ret);
			return ret;
		}

		/// <summary>
		/// Creates a new world <see cref="Matrix4"/>.
		/// </summary>
		/// <param name="position">The position vector.</param>
		/// <param name="forward">The forward direction vector.</param>
		/// <param name="up">The upward direction vector. Usually <see cref="Vector3.Up"/>.</param>
		/// <param name="result">The world <see cref="Matrix4"/> as an output parameter.</param>
		public static void CreateWorld(ref Vector3 position, ref Vector3 forward, ref Vector3 up, out Matrix4 result)
		{
			Vector3 x, y, z;
			Vector3.Normalize(ref forward, out z);
			Vector3.Cross(ref forward, ref up, out x);
			Vector3.Cross(ref x, ref forward, out y);
			x.Normalize();
			y.Normalize();

			result = new Matrix4();
			result.Right = x;
			result.Up = y;
			result.Forward = z;
			result.Translation = position;
			result.M44 = 1;
		}

		/// <summary>
		/// Decomposes this matrix to translation, rotation and scale elements. Returns <c>true</c> if matrix can be decomposed; <c>false</c> otherwise.
		/// </summary>
		/// <param name="scale">Scale vector as an output parameter.</param>
		/// <param name="rotation">Rotation quaternion as an output parameter.</param>
		/// <param name="translation">Translation vector as an output parameter.</param>
		/// <returns><c>true</c> if matrix can be decomposed; <c>false</c> otherwise.</returns>
		public bool Decompose(out Vector3 scale, out Quaternion rotation, out Vector3 translation)
		{
			translation.X = this.M41;
			translation.Y = this.M42;
			translation.Z = this.M43;

			double xs = (Math.Sign(this.M11 * this.M12 * this.M13 * this.M14) < 0) ? -1 : 1;
			double ys = (Math.Sign(this.M21 * this.M22 * this.M23 * this.M24) < 0) ? -1 : 1;
			double zs = (Math.Sign(this.M31 * this.M32 * this.M33 * this.M34) < 0) ? -1 : 1;

			scale.X = xs * MathF.Sqrt(this.M11 * this.M11 + this.M12 * this.M12 + this.M13 * this.M13);
			scale.Y = ys * MathF.Sqrt(this.M21 * this.M21 + this.M22 * this.M22 + this.M23 * this.M23);
			scale.Z = zs * MathF.Sqrt(this.M31 * this.M31 + this.M32 * this.M32 + this.M33 * this.M33);

			if (scale.X == 0.0 || scale.Y == 0.0 || scale.Z == 0.0)
			{
				rotation = Quaternion.Identity;
				return false;
			}

			Matrix4 m1 = new Matrix4(this.M11 / scale.X, this.M12 / scale.X, this.M13 / scale.X, 0,
								   this.M21 / scale.Y, this.M22 / scale.Y, this.M23 / scale.Y, 0,
								   this.M31 / scale.Z, this.M32 / scale.Z, this.M33 / scale.Z, 0,
								   0, 0, 0, 1);

			rotation = Quaternion.CreateFromRotationMatrix(m1);
			return true;
		}

		/// <summary>
		/// Divides the elements of a <see cref="Matrix4"/> by the elements of another matrix.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix4"/>.</param>
		/// <param name="matrix2">Divisor <see cref="Matrix4"/>.</param>
		/// <returns>The result of dividing the matrix.</returns>
		public static Matrix4 Divide(Matrix4 matrix1, Matrix4 matrix2)
		{
			matrix1.M11 = matrix1.M11 / matrix2.M11;
			matrix1.M12 = matrix1.M12 / matrix2.M12;
			matrix1.M13 = matrix1.M13 / matrix2.M13;
			matrix1.M14 = matrix1.M14 / matrix2.M14;
			matrix1.M21 = matrix1.M21 / matrix2.M21;
			matrix1.M22 = matrix1.M22 / matrix2.M22;
			matrix1.M23 = matrix1.M23 / matrix2.M23;
			matrix1.M24 = matrix1.M24 / matrix2.M24;
			matrix1.M31 = matrix1.M31 / matrix2.M31;
			matrix1.M32 = matrix1.M32 / matrix2.M32;
			matrix1.M33 = matrix1.M33 / matrix2.M33;
			matrix1.M34 = matrix1.M34 / matrix2.M34;
			matrix1.M41 = matrix1.M41 / matrix2.M41;
			matrix1.M42 = matrix1.M42 / matrix2.M42;
			matrix1.M43 = matrix1.M43 / matrix2.M43;
			matrix1.M44 = matrix1.M44 / matrix2.M44;
			return matrix1;
		}

		/// <summary>
		/// Divides the elements of a <see cref="Matrix4"/> by the elements of another matrix.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix4"/>.</param>
		/// <param name="matrix2">Divisor <see cref="Matrix4"/>.</param>
		/// <param name="result">The result of dividing the matrix as an output parameter.</param>
		public static void Divide(ref Matrix4 matrix1, ref Matrix4 matrix2, out Matrix4 result)
		{
			result = new Matrix4();
			result.M11 = matrix1.M11 / matrix2.M11;
			result.M12 = matrix1.M12 / matrix2.M12;
			result.M13 = matrix1.M13 / matrix2.M13;
			result.M14 = matrix1.M14 / matrix2.M14;
			result.M21 = matrix1.M21 / matrix2.M21;
			result.M22 = matrix1.M22 / matrix2.M22;
			result.M23 = matrix1.M23 / matrix2.M23;
			result.M24 = matrix1.M24 / matrix2.M24;
			result.M31 = matrix1.M31 / matrix2.M31;
			result.M32 = matrix1.M32 / matrix2.M32;
			result.M33 = matrix1.M33 / matrix2.M33;
			result.M34 = matrix1.M34 / matrix2.M34;
			result.M41 = matrix1.M41 / matrix2.M41;
			result.M42 = matrix1.M42 / matrix2.M42;
			result.M43 = matrix1.M43 / matrix2.M43;
			result.M44 = matrix1.M44 / matrix2.M44;
		}

		/// <summary>
		/// Divides the elements of a <see cref="Matrix4"/> by a scalar.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix4"/>.</param>
		/// <param name="divider">Divisor scalar.</param>
		/// <returns>The result of dividing a matrix by a scalar.</returns>
		public static Matrix4 Divide(Matrix4 matrix1, double divider)
		{
			double num = 1 / divider;
			matrix1.M11 = matrix1.M11 * num;
			matrix1.M12 = matrix1.M12 * num;
			matrix1.M13 = matrix1.M13 * num;
			matrix1.M14 = matrix1.M14 * num;
			matrix1.M21 = matrix1.M21 * num;
			matrix1.M22 = matrix1.M22 * num;
			matrix1.M23 = matrix1.M23 * num;
			matrix1.M24 = matrix1.M24 * num;
			matrix1.M31 = matrix1.M31 * num;
			matrix1.M32 = matrix1.M32 * num;
			matrix1.M33 = matrix1.M33 * num;
			matrix1.M34 = matrix1.M34 * num;
			matrix1.M41 = matrix1.M41 * num;
			matrix1.M42 = matrix1.M42 * num;
			matrix1.M43 = matrix1.M43 * num;
			matrix1.M44 = matrix1.M44 * num;
			return matrix1;
		}

		/// <summary>
		/// Divides the elements of a <see cref="Matrix4"/> by a scalar.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix4"/>.</param>
		/// <param name="divider">Divisor scalar.</param>
		/// <param name="result">The result of dividing a matrix by a scalar as an output parameter.</param>
		public static void Divide(ref Matrix4 matrix1, double divider, out Matrix4 result)
		{
			result = new Matrix4();
			double num = 1 / divider;
			result.M11 = matrix1.M11 * num;
			result.M12 = matrix1.M12 * num;
			result.M13 = matrix1.M13 * num;
			result.M14 = matrix1.M14 * num;
			result.M21 = matrix1.M21 * num;
			result.M22 = matrix1.M22 * num;
			result.M23 = matrix1.M23 * num;
			result.M24 = matrix1.M24 * num;
			result.M31 = matrix1.M31 * num;
			result.M32 = matrix1.M32 * num;
			result.M33 = matrix1.M33 * num;
			result.M34 = matrix1.M34 * num;
			result.M41 = matrix1.M41 * num;
			result.M42 = matrix1.M42 * num;
			result.M43 = matrix1.M43 * num;
			result.M44 = matrix1.M44 * num;
		}

		/// <summary>
		/// Compares whether current instance is equal to specified <see cref="Matrix4"/> without any tolerance.
		/// </summary>
		/// <param name="other">The <see cref="Matrix4"/> to compare.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public bool Equals(Matrix4 other)
		{
			return
					this.Row0 == other.Row0 &&
					this.Row1 == other.Row1 &&
					this.Row2 == other.Row2 &&
					this.Row3 == other.Row3;
		}

		/// <summary>
		/// Compares whether current instance is equal to specified <see cref="object"/> without any tolerance.
		/// </summary>
		/// <param name="obj">The <see cref="object"/> to compare.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public override bool Equals(object obj)
		{
			if (!(obj is Matrix4))
				return false;

			return this.Equals((Matrix4)obj);
		}

		/// <summary>
		/// Gets the hash code of this <see cref="Matrix4"/>.
		/// </summary>
		/// <returns>Hash code of this <see cref="Matrix4"/>.</returns>
		public override int GetHashCode()
		{
			return this.Row0.GetHashCode() ^ this.Row1.GetHashCode() ^ this.Row2.GetHashCode() ^ this.Row3.GetHashCode();
		}

		/// <summary>
		/// Creates a new <see cref="Matrix4"/> which contains inversion of the specified matrix. 
		/// </summary>
		/// <param name="matrix">Source <see cref="Matrix4"/>.</param>
		/// <returns>The inverted matrix.</returns>
		public static Matrix4 Invert(Matrix4 matrix)
		{
			Matrix4 result;
			Invert(ref matrix, out result);
			return result;
		}

		/// <summary>
		/// Creates a new <see cref="Matrix4"/> which contains inversion of the specified matrix. 
		/// </summary>
		/// <param name="matrix">Source <see cref="Matrix4"/>.</param>
		/// <param name="result">The inverted matrix as output parameter.</param>
		public static void Invert(ref Matrix4 matrix, out Matrix4 result)
		{
			double num1 = matrix.M11;
			double num2 = matrix.M12;
			double num3 = matrix.M13;
			double num4 = matrix.M14;
			double num5 = matrix.M21;
			double num6 = matrix.M22;
			double num7 = matrix.M23;
			double num8 = matrix.M24;
			double num9 = matrix.M31;
			double num10 = matrix.M32;
			double num11 = matrix.M33;
			double num12 = matrix.M34;
			double num13 = matrix.M41;
			double num14 = matrix.M42;
			double num15 = matrix.M43;
			double num16 = matrix.M44;
			double num17 = (double)((double)num11 * (double)num16 - (double)num12 * (double)num15);
			double num18 = (double)((double)num10 * (double)num16 - (double)num12 * (double)num14);
			double num19 = (double)((double)num10 * (double)num15 - (double)num11 * (double)num14);
			double num20 = (double)((double)num9 * (double)num16 - (double)num12 * (double)num13);
			double num21 = (double)((double)num9 * (double)num15 - (double)num11 * (double)num13);
			double num22 = (double)((double)num9 * (double)num14 - (double)num10 * (double)num13);
			double num23 = (double)((double)num6 * (double)num17 - (double)num7 * (double)num18 + (double)num8 * (double)num19);
			double num24 = (double)-((double)num5 * (double)num17 - (double)num7 * (double)num20 + (double)num8 * (double)num21);
			double num25 = (double)((double)num5 * (double)num18 - (double)num6 * (double)num20 + (double)num8 * (double)num22);
			double num26 = (double)-((double)num5 * (double)num19 - (double)num6 * (double)num21 + (double)num7 * (double)num22);
			double num27 = (double)(1.0 / ((double)num1 * (double)num23 + (double)num2 * (double)num24 + (double)num3 * (double)num25 + (double)num4 * (double)num26));

			result = new Matrix4();
			result.M11 = num23 * num27;
			result.M21 = num24 * num27;
			result.M31 = num25 * num27;
			result.M41 = num26 * num27;
			result.M12 = (double)-((double)num2 * (double)num17 - (double)num3 * (double)num18 + (double)num4 * (double)num19) * num27;
			result.M22 = (double)((double)num1 * (double)num17 - (double)num3 * (double)num20 + (double)num4 * (double)num21) * num27;
			result.M32 = (double)-((double)num1 * (double)num18 - (double)num2 * (double)num20 + (double)num4 * (double)num22) * num27;
			result.M42 = (double)((double)num1 * (double)num19 - (double)num2 * (double)num21 + (double)num3 * (double)num22) * num27;
			double num28 = (double)((double)num7 * (double)num16 - (double)num8 * (double)num15);
			double num29 = (double)((double)num6 * (double)num16 - (double)num8 * (double)num14);
			double num30 = (double)((double)num6 * (double)num15 - (double)num7 * (double)num14);
			double num31 = (double)((double)num5 * (double)num16 - (double)num8 * (double)num13);
			double num32 = (double)((double)num5 * (double)num15 - (double)num7 * (double)num13);
			double num33 = (double)((double)num5 * (double)num14 - (double)num6 * (double)num13);
			result.M13 = (double)((double)num2 * (double)num28 - (double)num3 * (double)num29 + (double)num4 * (double)num30) * num27;
			result.M23 = (double)-((double)num1 * (double)num28 - (double)num3 * (double)num31 + (double)num4 * (double)num32) * num27;
			result.M33 = (double)((double)num1 * (double)num29 - (double)num2 * (double)num31 + (double)num4 * (double)num33) * num27;
			result.M43 = (double)-((double)num1 * (double)num30 - (double)num2 * (double)num32 + (double)num3 * (double)num33) * num27;
			double num34 = (double)((double)num7 * (double)num12 - (double)num8 * (double)num11);
			double num35 = (double)((double)num6 * (double)num12 - (double)num8 * (double)num10);
			double num36 = (double)((double)num6 * (double)num11 - (double)num7 * (double)num10);
			double num37 = (double)((double)num5 * (double)num12 - (double)num8 * (double)num9);
			double num38 = (double)((double)num5 * (double)num11 - (double)num7 * (double)num9);
			double num39 = (double)((double)num5 * (double)num10 - (double)num6 * (double)num9);
			result.M14 = (double)-((double)num2 * (double)num34 - (double)num3 * (double)num35 + (double)num4 * (double)num36) * num27;
			result.M24 = (double)((double)num1 * (double)num34 - (double)num3 * (double)num37 + (double)num4 * (double)num38) * num27;
			result.M34 = (double)-((double)num1 * (double)num35 - (double)num2 * (double)num37 + (double)num4 * (double)num39) * num27;
			result.M44 = (double)((double)num1 * (double)num36 - (double)num2 * (double)num38 + (double)num3 * (double)num39) * num27;

		}

		/// <summary>
		/// Creates a new <see cref="Matrix4"/> that contains linear interpolation of the values in specified matrixes.
		/// </summary>
		/// <param name="matrix1">The first <see cref="Matrix4"/>.</param>
		/// <param name="matrix2">The second <see cref="Vector2"/>.</param>
		/// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
		/// <returns>>The result of linear interpolation of the specified matrixes.</returns>
		public static Matrix4 Lerp(Matrix4 matrix1, Matrix4 matrix2, double amount)
		{
			matrix1.M11 = matrix1.M11 + ((matrix2.M11 - matrix1.M11) * amount);
			matrix1.M12 = matrix1.M12 + ((matrix2.M12 - matrix1.M12) * amount);
			matrix1.M13 = matrix1.M13 + ((matrix2.M13 - matrix1.M13) * amount);
			matrix1.M14 = matrix1.M14 + ((matrix2.M14 - matrix1.M14) * amount);
			matrix1.M21 = matrix1.M21 + ((matrix2.M21 - matrix1.M21) * amount);
			matrix1.M22 = matrix1.M22 + ((matrix2.M22 - matrix1.M22) * amount);
			matrix1.M23 = matrix1.M23 + ((matrix2.M23 - matrix1.M23) * amount);
			matrix1.M24 = matrix1.M24 + ((matrix2.M24 - matrix1.M24) * amount);
			matrix1.M31 = matrix1.M31 + ((matrix2.M31 - matrix1.M31) * amount);
			matrix1.M32 = matrix1.M32 + ((matrix2.M32 - matrix1.M32) * amount);
			matrix1.M33 = matrix1.M33 + ((matrix2.M33 - matrix1.M33) * amount);
			matrix1.M34 = matrix1.M34 + ((matrix2.M34 - matrix1.M34) * amount);
			matrix1.M41 = matrix1.M41 + ((matrix2.M41 - matrix1.M41) * amount);
			matrix1.M42 = matrix1.M42 + ((matrix2.M42 - matrix1.M42) * amount);
			matrix1.M43 = matrix1.M43 + ((matrix2.M43 - matrix1.M43) * amount);
			matrix1.M44 = matrix1.M44 + ((matrix2.M44 - matrix1.M44) * amount);
			return matrix1;
		}

		/// <summary>
		/// Creates a new <see cref="Matrix4"/> that contains linear interpolation of the values in specified matrixes.
		/// </summary>
		/// <param name="matrix1">The first <see cref="Matrix4"/>.</param>
		/// <param name="matrix2">The second <see cref="Vector2"/>.</param>
		/// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
		/// <param name="result">The result of linear interpolation of the specified matrixes as an output parameter.</param>
		public static void Lerp(ref Matrix4 matrix1, ref Matrix4 matrix2, double amount, out Matrix4 result)
		{
			result = new Matrix4();
			result.M11 = matrix1.M11 + ((matrix2.M11 - matrix1.M11) * amount);
			result.M12 = matrix1.M12 + ((matrix2.M12 - matrix1.M12) * amount);
			result.M13 = matrix1.M13 + ((matrix2.M13 - matrix1.M13) * amount);
			result.M14 = matrix1.M14 + ((matrix2.M14 - matrix1.M14) * amount);
			result.M21 = matrix1.M21 + ((matrix2.M21 - matrix1.M21) * amount);
			result.M22 = matrix1.M22 + ((matrix2.M22 - matrix1.M22) * amount);
			result.M23 = matrix1.M23 + ((matrix2.M23 - matrix1.M23) * amount);
			result.M24 = matrix1.M24 + ((matrix2.M24 - matrix1.M24) * amount);
			result.M31 = matrix1.M31 + ((matrix2.M31 - matrix1.M31) * amount);
			result.M32 = matrix1.M32 + ((matrix2.M32 - matrix1.M32) * amount);
			result.M33 = matrix1.M33 + ((matrix2.M33 - matrix1.M33) * amount);
			result.M34 = matrix1.M34 + ((matrix2.M34 - matrix1.M34) * amount);
			result.M41 = matrix1.M41 + ((matrix2.M41 - matrix1.M41) * amount);
			result.M42 = matrix1.M42 + ((matrix2.M42 - matrix1.M42) * amount);
			result.M43 = matrix1.M43 + ((matrix2.M43 - matrix1.M43) * amount);
			result.M44 = matrix1.M44 + ((matrix2.M44 - matrix1.M44) * amount);
		}

		/// <summary>
		/// Creates a new <see cref="Matrix4"/> that contains a multiplication of two matrix.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix4"/>.</param>
		/// <param name="matrix2">Source <see cref="Matrix4"/>.</param>
		/// <returns>Result of the matrix multiplication.</returns>
		public static Matrix4 Multiply(Matrix4 matrix1, Matrix4 matrix2)
		{
			double m11 = (((matrix1.M11 * matrix2.M11) + (matrix1.M12 * matrix2.M21)) + (matrix1.M13 * matrix2.M31)) + (matrix1.M14 * matrix2.M41);
			double m12 = (((matrix1.M11 * matrix2.M12) + (matrix1.M12 * matrix2.M22)) + (matrix1.M13 * matrix2.M32)) + (matrix1.M14 * matrix2.M42);
			double m13 = (((matrix1.M11 * matrix2.M13) + (matrix1.M12 * matrix2.M23)) + (matrix1.M13 * matrix2.M33)) + (matrix1.M14 * matrix2.M43);
			double m14 = (((matrix1.M11 * matrix2.M14) + (matrix1.M12 * matrix2.M24)) + (matrix1.M13 * matrix2.M34)) + (matrix1.M14 * matrix2.M44);
			double m21 = (((matrix1.M21 * matrix2.M11) + (matrix1.M22 * matrix2.M21)) + (matrix1.M23 * matrix2.M31)) + (matrix1.M24 * matrix2.M41);
			double m22 = (((matrix1.M21 * matrix2.M12) + (matrix1.M22 * matrix2.M22)) + (matrix1.M23 * matrix2.M32)) + (matrix1.M24 * matrix2.M42);
			double m23 = (((matrix1.M21 * matrix2.M13) + (matrix1.M22 * matrix2.M23)) + (matrix1.M23 * matrix2.M33)) + (matrix1.M24 * matrix2.M43);
			double m24 = (((matrix1.M21 * matrix2.M14) + (matrix1.M22 * matrix2.M24)) + (matrix1.M23 * matrix2.M34)) + (matrix1.M24 * matrix2.M44);
			double m31 = (((matrix1.M31 * matrix2.M11) + (matrix1.M32 * matrix2.M21)) + (matrix1.M33 * matrix2.M31)) + (matrix1.M34 * matrix2.M41);
			double m32 = (((matrix1.M31 * matrix2.M12) + (matrix1.M32 * matrix2.M22)) + (matrix1.M33 * matrix2.M32)) + (matrix1.M34 * matrix2.M42);
			double m33 = (((matrix1.M31 * matrix2.M13) + (matrix1.M32 * matrix2.M23)) + (matrix1.M33 * matrix2.M33)) + (matrix1.M34 * matrix2.M43);
			double m34 = (((matrix1.M31 * matrix2.M14) + (matrix1.M32 * matrix2.M24)) + (matrix1.M33 * matrix2.M34)) + (matrix1.M34 * matrix2.M44);
			double m41 = (((matrix1.M41 * matrix2.M11) + (matrix1.M42 * matrix2.M21)) + (matrix1.M43 * matrix2.M31)) + (matrix1.M44 * matrix2.M41);
			double m42 = (((matrix1.M41 * matrix2.M12) + (matrix1.M42 * matrix2.M22)) + (matrix1.M43 * matrix2.M32)) + (matrix1.M44 * matrix2.M42);
			double m43 = (((matrix1.M41 * matrix2.M13) + (matrix1.M42 * matrix2.M23)) + (matrix1.M43 * matrix2.M33)) + (matrix1.M44 * matrix2.M43);
			double m44 = (((matrix1.M41 * matrix2.M14) + (matrix1.M42 * matrix2.M24)) + (matrix1.M43 * matrix2.M34)) + (matrix1.M44 * matrix2.M44);
			matrix1.M11 = m11;
			matrix1.M12 = m12;
			matrix1.M13 = m13;
			matrix1.M14 = m14;
			matrix1.M21 = m21;
			matrix1.M22 = m22;
			matrix1.M23 = m23;
			matrix1.M24 = m24;
			matrix1.M31 = m31;
			matrix1.M32 = m32;
			matrix1.M33 = m33;
			matrix1.M34 = m34;
			matrix1.M41 = m41;
			matrix1.M42 = m42;
			matrix1.M43 = m43;
			matrix1.M44 = m44;
			return matrix1;
		}

		/// <summary>
		/// Creates a new <see cref="Matrix4"/> that contains a multiplication of two matrix.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix4"/>.</param>
		/// <param name="matrix2">Source <see cref="Matrix4"/>.</param>
		/// <param name="result">Result of the matrix multiplication as an output parameter.</param>
		public static void Multiply(ref Matrix4 matrix1, ref Matrix4 matrix2, out Matrix4 result)
		{
			double m11 = (((matrix1.M11 * matrix2.M11) + (matrix1.M12 * matrix2.M21)) + (matrix1.M13 * matrix2.M31)) + (matrix1.M14 * matrix2.M41);
			double m12 = (((matrix1.M11 * matrix2.M12) + (matrix1.M12 * matrix2.M22)) + (matrix1.M13 * matrix2.M32)) + (matrix1.M14 * matrix2.M42);
			double m13 = (((matrix1.M11 * matrix2.M13) + (matrix1.M12 * matrix2.M23)) + (matrix1.M13 * matrix2.M33)) + (matrix1.M14 * matrix2.M43);
			double m14 = (((matrix1.M11 * matrix2.M14) + (matrix1.M12 * matrix2.M24)) + (matrix1.M13 * matrix2.M34)) + (matrix1.M14 * matrix2.M44);
			double m21 = (((matrix1.M21 * matrix2.M11) + (matrix1.M22 * matrix2.M21)) + (matrix1.M23 * matrix2.M31)) + (matrix1.M24 * matrix2.M41);
			double m22 = (((matrix1.M21 * matrix2.M12) + (matrix1.M22 * matrix2.M22)) + (matrix1.M23 * matrix2.M32)) + (matrix1.M24 * matrix2.M42);
			double m23 = (((matrix1.M21 * matrix2.M13) + (matrix1.M22 * matrix2.M23)) + (matrix1.M23 * matrix2.M33)) + (matrix1.M24 * matrix2.M43);
			double m24 = (((matrix1.M21 * matrix2.M14) + (matrix1.M22 * matrix2.M24)) + (matrix1.M23 * matrix2.M34)) + (matrix1.M24 * matrix2.M44);
			double m31 = (((matrix1.M31 * matrix2.M11) + (matrix1.M32 * matrix2.M21)) + (matrix1.M33 * matrix2.M31)) + (matrix1.M34 * matrix2.M41);
			double m32 = (((matrix1.M31 * matrix2.M12) + (matrix1.M32 * matrix2.M22)) + (matrix1.M33 * matrix2.M32)) + (matrix1.M34 * matrix2.M42);
			double m33 = (((matrix1.M31 * matrix2.M13) + (matrix1.M32 * matrix2.M23)) + (matrix1.M33 * matrix2.M33)) + (matrix1.M34 * matrix2.M43);
			double m34 = (((matrix1.M31 * matrix2.M14) + (matrix1.M32 * matrix2.M24)) + (matrix1.M33 * matrix2.M34)) + (matrix1.M34 * matrix2.M44);
			double m41 = (((matrix1.M41 * matrix2.M11) + (matrix1.M42 * matrix2.M21)) + (matrix1.M43 * matrix2.M31)) + (matrix1.M44 * matrix2.M41);
			double m42 = (((matrix1.M41 * matrix2.M12) + (matrix1.M42 * matrix2.M22)) + (matrix1.M43 * matrix2.M32)) + (matrix1.M44 * matrix2.M42);
			double m43 = (((matrix1.M41 * matrix2.M13) + (matrix1.M42 * matrix2.M23)) + (matrix1.M43 * matrix2.M33)) + (matrix1.M44 * matrix2.M43);
			double m44 = (((matrix1.M41 * matrix2.M14) + (matrix1.M42 * matrix2.M24)) + (matrix1.M43 * matrix2.M34)) + (matrix1.M44 * matrix2.M44);
			result = new Matrix4();
			result.M11 = m11;
			result.M12 = m12;
			result.M13 = m13;
			result.M14 = m14;
			result.M21 = m21;
			result.M22 = m22;
			result.M23 = m23;
			result.M24 = m24;
			result.M31 = m31;
			result.M32 = m32;
			result.M33 = m33;
			result.M34 = m34;
			result.M41 = m41;
			result.M42 = m42;
			result.M43 = m43;
			result.M44 = m44;
		}

		/// <summary>
		/// Creates a new <see cref="Matrix4"/> that contains a multiplication of <see cref="Matrix4"/> and a scalar.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix4"/>.</param>
		/// <param name="scaleFactor">Scalar value.</param>
		/// <returns>Result of the matrix multiplication with a scalar.</returns>
		public static Matrix4 Multiply(Matrix4 matrix1, double scaleFactor)
		{
			matrix1.M11 *= scaleFactor;
			matrix1.M12 *= scaleFactor;
			matrix1.M13 *= scaleFactor;
			matrix1.M14 *= scaleFactor;
			matrix1.M21 *= scaleFactor;
			matrix1.M22 *= scaleFactor;
			matrix1.M23 *= scaleFactor;
			matrix1.M24 *= scaleFactor;
			matrix1.M31 *= scaleFactor;
			matrix1.M32 *= scaleFactor;
			matrix1.M33 *= scaleFactor;
			matrix1.M34 *= scaleFactor;
			matrix1.M41 *= scaleFactor;
			matrix1.M42 *= scaleFactor;
			matrix1.M43 *= scaleFactor;
			matrix1.M44 *= scaleFactor;
			return matrix1;
		}

		/// <summary>
		/// Creates a new <see cref="Matrix4"/> that contains a multiplication of <see cref="Matrix4"/> and a scalar.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix4"/>.</param>
		/// <param name="scaleFactor">Scalar value.</param>
		/// <param name="result">Result of the matrix multiplication with a scalar as an output parameter.</param>
		public static void Multiply(ref Matrix4 matrix1, double scaleFactor, out Matrix4 result)
		{
			result = new Matrix4();
			result.M11 = matrix1.M11 * scaleFactor;
			result.M12 = matrix1.M12 * scaleFactor;
			result.M13 = matrix1.M13 * scaleFactor;
			result.M14 = matrix1.M14 * scaleFactor;
			result.M21 = matrix1.M21 * scaleFactor;
			result.M22 = matrix1.M22 * scaleFactor;
			result.M23 = matrix1.M23 * scaleFactor;
			result.M24 = matrix1.M24 * scaleFactor;
			result.M31 = matrix1.M31 * scaleFactor;
			result.M32 = matrix1.M32 * scaleFactor;
			result.M33 = matrix1.M33 * scaleFactor;
			result.M34 = matrix1.M34 * scaleFactor;
			result.M41 = matrix1.M41 * scaleFactor;
			result.M42 = matrix1.M42 * scaleFactor;
			result.M43 = matrix1.M43 * scaleFactor;
			result.M44 = matrix1.M44 * scaleFactor;

		}

		/// <summary>
		/// Copy the values of specified <see cref="Matrix4"/> to the double array.
		/// </summary>
		/// <param name="matrix">The source <see cref="Matrix4"/>.</param>
		/// <returns>The array which matrix values will be stored.</returns>
		/// <remarks>
		/// Required for OpenGL 2.0 projection matrix stuff.
		/// </remarks>
		public static double[] TodoubleArray(Matrix4 matrix)
		{
			double[] matarray = {
									matrix.M11, matrix.M12, matrix.M13, matrix.M14,
									matrix.M21, matrix.M22, matrix.M23, matrix.M24,
									matrix.M31, matrix.M32, matrix.M33, matrix.M34,
									matrix.M41, matrix.M42, matrix.M43, matrix.M44
								};
			return matarray;
		}

		/// <summary>
		/// Returns a matrix with the all values negated.
		/// </summary>
		/// <param name="matrix">Source <see cref="Matrix4"/>.</param>
		/// <returns>Result of the matrix negation.</returns>
		public static Matrix4 Negate(Matrix4 matrix)
		{
			matrix.M11 = -matrix.M11;
			matrix.M12 = -matrix.M12;
			matrix.M13 = -matrix.M13;
			matrix.M14 = -matrix.M14;
			matrix.M21 = -matrix.M21;
			matrix.M22 = -matrix.M22;
			matrix.M23 = -matrix.M23;
			matrix.M24 = -matrix.M24;
			matrix.M31 = -matrix.M31;
			matrix.M32 = -matrix.M32;
			matrix.M33 = -matrix.M33;
			matrix.M34 = -matrix.M34;
			matrix.M41 = -matrix.M41;
			matrix.M42 = -matrix.M42;
			matrix.M43 = -matrix.M43;
			matrix.M44 = -matrix.M44;
			return matrix;
		}

		/// <summary>
		/// Returns a matrix with the all values negated.
		/// </summary>
		/// <param name="matrix">Source <see cref="Matrix4"/>.</param>
		/// <param name="result">Result of the matrix negation as an output parameter.</param>
		public static void Negate(ref Matrix4 matrix, out Matrix4 result)
		{
			result = new Matrix4();
			result.M11 = -matrix.M11;
			result.M12 = -matrix.M12;
			result.M13 = -matrix.M13;
			result.M14 = -matrix.M14;
			result.M21 = -matrix.M21;
			result.M22 = -matrix.M22;
			result.M23 = -matrix.M23;
			result.M24 = -matrix.M24;
			result.M31 = -matrix.M31;
			result.M32 = -matrix.M32;
			result.M33 = -matrix.M33;
			result.M34 = -matrix.M34;
			result.M41 = -matrix.M41;
			result.M42 = -matrix.M42;
			result.M43 = -matrix.M43;
			result.M44 = -matrix.M44;
		}

		/// <summary>
		/// Adds two matrixes.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix4"/> on the left of the add sign.</param>
		/// <param name="matrix2">Source <see cref="Matrix4"/> on the right of the add sign.</param>
		/// <returns>Sum of the matrixes.</returns>
		public static Matrix4 operator +(Matrix4 matrix1, Matrix4 matrix2)
		{
			matrix1.M11 = matrix1.M11 + matrix2.M11;
			matrix1.M12 = matrix1.M12 + matrix2.M12;
			matrix1.M13 = matrix1.M13 + matrix2.M13;
			matrix1.M14 = matrix1.M14 + matrix2.M14;
			matrix1.M21 = matrix1.M21 + matrix2.M21;
			matrix1.M22 = matrix1.M22 + matrix2.M22;
			matrix1.M23 = matrix1.M23 + matrix2.M23;
			matrix1.M24 = matrix1.M24 + matrix2.M24;
			matrix1.M31 = matrix1.M31 + matrix2.M31;
			matrix1.M32 = matrix1.M32 + matrix2.M32;
			matrix1.M33 = matrix1.M33 + matrix2.M33;
			matrix1.M34 = matrix1.M34 + matrix2.M34;
			matrix1.M41 = matrix1.M41 + matrix2.M41;
			matrix1.M42 = matrix1.M42 + matrix2.M42;
			matrix1.M43 = matrix1.M43 + matrix2.M43;
			matrix1.M44 = matrix1.M44 + matrix2.M44;
			return matrix1;
		}

		/// <summary>
		/// Divides the elements of a <see cref="Matrix4"/> by the elements of another <see cref="Matrix4"/>.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix4"/> on the left of the div sign.</param>
		/// <param name="matrix2">Divisor <see cref="Matrix4"/> on the right of the div sign.</param>
		/// <returns>The result of dividing the matrixes.</returns>
		public static Matrix4 operator /(Matrix4 matrix1, Matrix4 matrix2)
		{
			matrix1.M11 = matrix1.M11 / matrix2.M11;
			matrix1.M12 = matrix1.M12 / matrix2.M12;
			matrix1.M13 = matrix1.M13 / matrix2.M13;
			matrix1.M14 = matrix1.M14 / matrix2.M14;
			matrix1.M21 = matrix1.M21 / matrix2.M21;
			matrix1.M22 = matrix1.M22 / matrix2.M22;
			matrix1.M23 = matrix1.M23 / matrix2.M23;
			matrix1.M24 = matrix1.M24 / matrix2.M24;
			matrix1.M31 = matrix1.M31 / matrix2.M31;
			matrix1.M32 = matrix1.M32 / matrix2.M32;
			matrix1.M33 = matrix1.M33 / matrix2.M33;
			matrix1.M34 = matrix1.M34 / matrix2.M34;
			matrix1.M41 = matrix1.M41 / matrix2.M41;
			matrix1.M42 = matrix1.M42 / matrix2.M42;
			matrix1.M43 = matrix1.M43 / matrix2.M43;
			matrix1.M44 = matrix1.M44 / matrix2.M44;
			return matrix1;
		}

		/// <summary>
		/// Divides the elements of a <see cref="Matrix4"/> by a scalar.
		/// </summary>
		/// <param name="matrix">Source <see cref="Matrix4"/> on the left of the div sign.</param>
		/// <param name="divider">Divisor scalar on the right of the div sign.</param>
		/// <returns>The result of dividing a matrix by a scalar.</returns>
		public static Matrix4 operator /(Matrix4 matrix, double divider)
		{
			double num = 1f / divider;
			matrix.M11 = matrix.M11 * num;
			matrix.M12 = matrix.M12 * num;
			matrix.M13 = matrix.M13 * num;
			matrix.M14 = matrix.M14 * num;
			matrix.M21 = matrix.M21 * num;
			matrix.M22 = matrix.M22 * num;
			matrix.M23 = matrix.M23 * num;
			matrix.M24 = matrix.M24 * num;
			matrix.M31 = matrix.M31 * num;
			matrix.M32 = matrix.M32 * num;
			matrix.M33 = matrix.M33 * num;
			matrix.M34 = matrix.M34 * num;
			matrix.M41 = matrix.M41 * num;
			matrix.M42 = matrix.M42 * num;
			matrix.M43 = matrix.M43 * num;
			matrix.M44 = matrix.M44 * num;
			return matrix;
		}

		/// <summary>
		/// Compares whether two <see cref="Matrix4"/> instances are equal without any tolerance.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix4"/> on the left of the equal sign.</param>
		/// <param name="matrix2">Source <see cref="Matrix4"/> on the right of the equal sign.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public static bool operator ==(Matrix4 matrix1, Matrix4 matrix2)
		{
			return (
				matrix1.M11 == matrix2.M11 &&
				matrix1.M12 == matrix2.M12 &&
				matrix1.M13 == matrix2.M13 &&
				matrix1.M14 == matrix2.M14 &&
				matrix1.M21 == matrix2.M21 &&
				matrix1.M22 == matrix2.M22 &&
				matrix1.M23 == matrix2.M23 &&
				matrix1.M24 == matrix2.M24 &&
				matrix1.M31 == matrix2.M31 &&
				matrix1.M32 == matrix2.M32 &&
				matrix1.M33 == matrix2.M33 &&
				matrix1.M34 == matrix2.M34 &&
				matrix1.M41 == matrix2.M41 &&
				matrix1.M42 == matrix2.M42 &&
				matrix1.M43 == matrix2.M43 &&
				matrix1.M44 == matrix2.M44
				);
		}

		/// <summary>
		/// Compares whether two <see cref="Matrix4"/> instances are not equal without any tolerance.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix4"/> on the left of the not equal sign.</param>
		/// <param name="matrix2">Source <see cref="Matrix4"/> on the right of the not equal sign.</param>
		/// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>
		public static bool operator !=(Matrix4 matrix1, Matrix4 matrix2)
		{
			return (
				matrix1.M11 != matrix2.M11 ||
				matrix1.M12 != matrix2.M12 ||
				matrix1.M13 != matrix2.M13 ||
				matrix1.M14 != matrix2.M14 ||
				matrix1.M21 != matrix2.M21 ||
				matrix1.M22 != matrix2.M22 ||
				matrix1.M23 != matrix2.M23 ||
				matrix1.M24 != matrix2.M24 ||
				matrix1.M31 != matrix2.M31 ||
				matrix1.M32 != matrix2.M32 ||
				matrix1.M33 != matrix2.M33 ||
				matrix1.M34 != matrix2.M34 ||
				matrix1.M41 != matrix2.M41 ||
				matrix1.M42 != matrix2.M42 ||
				matrix1.M43 != matrix2.M43 ||
				matrix1.M44 != matrix2.M44
				);
		}

		/// <summary>
		/// Multiplies two matrixes.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix4"/> on the left of the mul sign.</param>
		/// <param name="matrix2">Source <see cref="Matrix4"/> on the right of the mul sign.</param>
		/// <returns>Result of the matrix multiplication.</returns>
		/// <remarks>
		/// Using matrix multiplication algorithm - see http://en.wikipedia.org/wiki/Matrix_multiplication.
		/// </remarks>
		public static Matrix4 operator *(Matrix4 matrix1, Matrix4 matrix2)
		{
			double m11 = (((matrix1.M11 * matrix2.M11) + (matrix1.M12 * matrix2.M21)) + (matrix1.M13 * matrix2.M31)) + (matrix1.M14 * matrix2.M41);
			double m12 = (((matrix1.M11 * matrix2.M12) + (matrix1.M12 * matrix2.M22)) + (matrix1.M13 * matrix2.M32)) + (matrix1.M14 * matrix2.M42);
			double m13 = (((matrix1.M11 * matrix2.M13) + (matrix1.M12 * matrix2.M23)) + (matrix1.M13 * matrix2.M33)) + (matrix1.M14 * matrix2.M43);
			double m14 = (((matrix1.M11 * matrix2.M14) + (matrix1.M12 * matrix2.M24)) + (matrix1.M13 * matrix2.M34)) + (matrix1.M14 * matrix2.M44);
			double m21 = (((matrix1.M21 * matrix2.M11) + (matrix1.M22 * matrix2.M21)) + (matrix1.M23 * matrix2.M31)) + (matrix1.M24 * matrix2.M41);
			double m22 = (((matrix1.M21 * matrix2.M12) + (matrix1.M22 * matrix2.M22)) + (matrix1.M23 * matrix2.M32)) + (matrix1.M24 * matrix2.M42);
			double m23 = (((matrix1.M21 * matrix2.M13) + (matrix1.M22 * matrix2.M23)) + (matrix1.M23 * matrix2.M33)) + (matrix1.M24 * matrix2.M43);
			double m24 = (((matrix1.M21 * matrix2.M14) + (matrix1.M22 * matrix2.M24)) + (matrix1.M23 * matrix2.M34)) + (matrix1.M24 * matrix2.M44);
			double m31 = (((matrix1.M31 * matrix2.M11) + (matrix1.M32 * matrix2.M21)) + (matrix1.M33 * matrix2.M31)) + (matrix1.M34 * matrix2.M41);
			double m32 = (((matrix1.M31 * matrix2.M12) + (matrix1.M32 * matrix2.M22)) + (matrix1.M33 * matrix2.M32)) + (matrix1.M34 * matrix2.M42);
			double m33 = (((matrix1.M31 * matrix2.M13) + (matrix1.M32 * matrix2.M23)) + (matrix1.M33 * matrix2.M33)) + (matrix1.M34 * matrix2.M43);
			double m34 = (((matrix1.M31 * matrix2.M14) + (matrix1.M32 * matrix2.M24)) + (matrix1.M33 * matrix2.M34)) + (matrix1.M34 * matrix2.M44);
			double m41 = (((matrix1.M41 * matrix2.M11) + (matrix1.M42 * matrix2.M21)) + (matrix1.M43 * matrix2.M31)) + (matrix1.M44 * matrix2.M41);
			double m42 = (((matrix1.M41 * matrix2.M12) + (matrix1.M42 * matrix2.M22)) + (matrix1.M43 * matrix2.M32)) + (matrix1.M44 * matrix2.M42);
			double m43 = (((matrix1.M41 * matrix2.M13) + (matrix1.M42 * matrix2.M23)) + (matrix1.M43 * matrix2.M33)) + (matrix1.M44 * matrix2.M43);
			double m44 = (((matrix1.M41 * matrix2.M14) + (matrix1.M42 * matrix2.M24)) + (matrix1.M43 * matrix2.M34)) + (matrix1.M44 * matrix2.M44);
			matrix1.M11 = m11;
			matrix1.M12 = m12;
			matrix1.M13 = m13;
			matrix1.M14 = m14;
			matrix1.M21 = m21;
			matrix1.M22 = m22;
			matrix1.M23 = m23;
			matrix1.M24 = m24;
			matrix1.M31 = m31;
			matrix1.M32 = m32;
			matrix1.M33 = m33;
			matrix1.M34 = m34;
			matrix1.M41 = m41;
			matrix1.M42 = m42;
			matrix1.M43 = m43;
			matrix1.M44 = m44;
			return matrix1;
		}

		/// <summary>
		/// Multiplies the elements of matrix by a scalar.
		/// </summary>
		/// <param name="matrix">Source <see cref="Matrix4"/> on the left of the mul sign.</param>
		/// <param name="scaleFactor">Scalar value on the right of the mul sign.</param>
		/// <returns>Result of the matrix multiplication with a scalar.</returns>
		public static Matrix4 operator *(Matrix4 matrix, double scaleFactor)
		{
			matrix.M11 = matrix.M11 * scaleFactor;
			matrix.M12 = matrix.M12 * scaleFactor;
			matrix.M13 = matrix.M13 * scaleFactor;
			matrix.M14 = matrix.M14 * scaleFactor;
			matrix.M21 = matrix.M21 * scaleFactor;
			matrix.M22 = matrix.M22 * scaleFactor;
			matrix.M23 = matrix.M23 * scaleFactor;
			matrix.M24 = matrix.M24 * scaleFactor;
			matrix.M31 = matrix.M31 * scaleFactor;
			matrix.M32 = matrix.M32 * scaleFactor;
			matrix.M33 = matrix.M33 * scaleFactor;
			matrix.M34 = matrix.M34 * scaleFactor;
			matrix.M41 = matrix.M41 * scaleFactor;
			matrix.M42 = matrix.M42 * scaleFactor;
			matrix.M43 = matrix.M43 * scaleFactor;
			matrix.M44 = matrix.M44 * scaleFactor;
			return matrix;
		}

		/// <summary>
		/// Subtracts the values of one <see cref="Matrix4"/> from another <see cref="Matrix4"/>.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix4"/> on the left of the sub sign.</param>
		/// <param name="matrix2">Source <see cref="Matrix4"/> on the right of the sub sign.</param>
		/// <returns>Result of the matrix subtraction.</returns>
		public static Matrix4 operator -(Matrix4 matrix1, Matrix4 matrix2)
		{
			matrix1.M11 = matrix1.M11 - matrix2.M11;
			matrix1.M12 = matrix1.M12 - matrix2.M12;
			matrix1.M13 = matrix1.M13 - matrix2.M13;
			matrix1.M14 = matrix1.M14 - matrix2.M14;
			matrix1.M21 = matrix1.M21 - matrix2.M21;
			matrix1.M22 = matrix1.M22 - matrix2.M22;
			matrix1.M23 = matrix1.M23 - matrix2.M23;
			matrix1.M24 = matrix1.M24 - matrix2.M24;
			matrix1.M31 = matrix1.M31 - matrix2.M31;
			matrix1.M32 = matrix1.M32 - matrix2.M32;
			matrix1.M33 = matrix1.M33 - matrix2.M33;
			matrix1.M34 = matrix1.M34 - matrix2.M34;
			matrix1.M41 = matrix1.M41 - matrix2.M41;
			matrix1.M42 = matrix1.M42 - matrix2.M42;
			matrix1.M43 = matrix1.M43 - matrix2.M43;
			matrix1.M44 = matrix1.M44 - matrix2.M44;
			return matrix1;
		}

		/// <summary>
		/// Inverts values in the specified <see cref="Matrix4"/>.
		/// </summary>
		/// <param name="matrix">Source <see cref="Matrix4"/> on the right of the sub sign.</param>
		/// <returns>Result of the inversion.</returns>
		public static Matrix4 operator -(Matrix4 matrix)
		{
			matrix.M11 = -matrix.M11;
			matrix.M12 = -matrix.M12;
			matrix.M13 = -matrix.M13;
			matrix.M14 = -matrix.M14;
			matrix.M21 = -matrix.M21;
			matrix.M22 = -matrix.M22;
			matrix.M23 = -matrix.M23;
			matrix.M24 = -matrix.M24;
			matrix.M31 = -matrix.M31;
			matrix.M32 = -matrix.M32;
			matrix.M33 = -matrix.M33;
			matrix.M34 = -matrix.M34;
			matrix.M41 = -matrix.M41;
			matrix.M42 = -matrix.M42;
			matrix.M43 = -matrix.M43;
			matrix.M44 = -matrix.M44;
			return matrix;
		}

		/// <summary>
		/// Creates a new <see cref="Matrix4"/> that contains subtraction of one matrix from another.
		/// </summary>
		/// <param name="matrix1">The first <see cref="Matrix4"/>.</param>
		/// <param name="matrix2">The second <see cref="Matrix4"/>.</param>
		/// <returns>The result of the matrix subtraction.</returns>
		public static Matrix4 Subtract(Matrix4 matrix1, Matrix4 matrix2)
		{
			matrix1.M11 = matrix1.M11 - matrix2.M11;
			matrix1.M12 = matrix1.M12 - matrix2.M12;
			matrix1.M13 = matrix1.M13 - matrix2.M13;
			matrix1.M14 = matrix1.M14 - matrix2.M14;
			matrix1.M21 = matrix1.M21 - matrix2.M21;
			matrix1.M22 = matrix1.M22 - matrix2.M22;
			matrix1.M23 = matrix1.M23 - matrix2.M23;
			matrix1.M24 = matrix1.M24 - matrix2.M24;
			matrix1.M31 = matrix1.M31 - matrix2.M31;
			matrix1.M32 = matrix1.M32 - matrix2.M32;
			matrix1.M33 = matrix1.M33 - matrix2.M33;
			matrix1.M34 = matrix1.M34 - matrix2.M34;
			matrix1.M41 = matrix1.M41 - matrix2.M41;
			matrix1.M42 = matrix1.M42 - matrix2.M42;
			matrix1.M43 = matrix1.M43 - matrix2.M43;
			matrix1.M44 = matrix1.M44 - matrix2.M44;
			return matrix1;
		}

		/// <summary>
		/// Creates a new <see cref="Matrix4"/> that contains subtraction of one matrix from another.
		/// </summary>
		/// <param name="matrix1">The first <see cref="Matrix4"/>.</param>
		/// <param name="matrix2">The second <see cref="Matrix4"/>.</param>
		/// <param name="result">The result of the matrix subtraction as an output parameter.</param>
		public static void Subtract(ref Matrix4 matrix1, ref Matrix4 matrix2, out Matrix4 result)
		{
			result = new Matrix4();
			result.M11 = matrix1.M11 - matrix2.M11;
			result.M12 = matrix1.M12 - matrix2.M12;
			result.M13 = matrix1.M13 - matrix2.M13;
			result.M14 = matrix1.M14 - matrix2.M14;
			result.M21 = matrix1.M21 - matrix2.M21;
			result.M22 = matrix1.M22 - matrix2.M22;
			result.M23 = matrix1.M23 - matrix2.M23;
			result.M24 = matrix1.M24 - matrix2.M24;
			result.M31 = matrix1.M31 - matrix2.M31;
			result.M32 = matrix1.M32 - matrix2.M32;
			result.M33 = matrix1.M33 - matrix2.M33;
			result.M34 = matrix1.M34 - matrix2.M34;
			result.M41 = matrix1.M41 - matrix2.M41;
			result.M42 = matrix1.M42 - matrix2.M42;
			result.M43 = matrix1.M43 - matrix2.M43;
			result.M44 = matrix1.M44 - matrix2.M44;
		}

		internal string DebugDisplayString
		{
			get
			{
				if (this == Identity)
				{
					return "Identity";
				}

				return string.Concat(
					 "( ", this.M11.ToString(), "  ", this.M12.ToString(), "  ", this.M13.ToString(), "  ", this.M14.ToString(), " )  \r\n",
					 "( ", this.M21.ToString(), "  ", this.M22.ToString(), "  ", this.M23.ToString(), "  ", this.M24.ToString(), " )  \r\n",
					 "( ", this.M31.ToString(), "  ", this.M32.ToString(), "  ", this.M33.ToString(), "  ", this.M34.ToString(), " )  \r\n",
					 "( ", this.M41.ToString(), "  ", this.M42.ToString(), "  ", this.M43.ToString(), "  ", this.M44.ToString(), " )");
			}
		}

		/// <summary>
		/// Returns a <see cref="string"/> representation of this <see cref="Matrix4"/> in the format:
		/// {M11:[<see cref="M11"/>] M12:[<see cref="M12"/>] M13:[<see cref="M13"/>] M14:[<see cref="M14"/>]}
		/// {M21:[<see cref="M21"/>] M12:[<see cref="M22"/>] M13:[<see cref="M23"/>] M14:[<see cref="M24"/>]}
		/// {M31:[<see cref="M31"/>] M32:[<see cref="M32"/>] M33:[<see cref="M33"/>] M34:[<see cref="M34"/>]}
		/// {M41:[<see cref="M41"/>] M42:[<see cref="M42"/>] M43:[<see cref="M43"/>] M44:[<see cref="M44"/>]}
		/// </summary>
		/// <returns>A <see cref="string"/> representation of this <see cref="Matrix4"/>.</returns>
		public override string ToString()
		{
			return "{M11:" + this.M11 + " M12:" + this.M12 + " M13:" + this.M13 + " M14:" + this.M14 + "}"
				+ " {M21:" + this.M21 + " M22:" + this.M22 + " M23:" + this.M23 + " M24:" + this.M24 + "}"
				+ " {M31:" + this.M31 + " M32:" + this.M32 + " M33:" + this.M33 + " M34:" + this.M34 + "}"
				+ " {M41:" + this.M41 + " M42:" + this.M42 + " M43:" + this.M43 + " M44:" + this.M44 + "}";
		}

		/// <summary>
		/// Swap the matrix rows and columns.
		/// </summary>
		/// <param name="matrix">The matrix for transposing operation.</param>
		/// <returns>The new <see cref="Matrix4"/> which contains the transposing result.</returns>
		public static Matrix4 Transpose(Matrix4 matrix)
		{
			Matrix4 ret;
			Transpose(ref matrix, out ret);
			return ret;
		}

		/// <summary>
		/// Swap the matrix rows and columns.
		/// </summary>
		/// <param name="matrix">The matrix for transposing operation.</param>
		/// <param name="result">The new <see cref="Matrix4"/> which contains the transposing result as an output parameter.</param>
		public static void Transpose(ref Matrix4 matrix, out Matrix4 result)
		{
			Matrix4 ret = new Matrix4();
			ret.M11 = matrix.M11;
			ret.M12 = matrix.M21;
			ret.M13 = matrix.M31;
			ret.M14 = matrix.M41;

			ret.M21 = matrix.M12;
			ret.M22 = matrix.M22;
			ret.M23 = matrix.M32;
			ret.M24 = matrix.M42;

			ret.M31 = matrix.M13;
			ret.M32 = matrix.M23;
			ret.M33 = matrix.M33;
			ret.M34 = matrix.M43;

			ret.M41 = matrix.M14;
			ret.M42 = matrix.M24;
			ret.M43 = matrix.M34;
			ret.M44 = matrix.M44;

			result = ret;
		}

		#endregion

	}
}
