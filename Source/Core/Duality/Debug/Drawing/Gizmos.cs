using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Duality.Drawing;

namespace Duality.DebugDraw
{
	public static class Gizmos
	{
		const float DEG_TO_RAD = (float)Math.PI / 180f;

		static List<Vector3> SphereVertices;
		static List<Vector3> BoxVertices;

		public static bool DrawLightGizmos = true;

		static Gizmos()
		{
			//SphereVertices
			SphereVertices = new List<Vector3>();
			// Decreasing these angles will increase complexity of sphere.
			var dtheta = 35; var dphi = 35;

			for (var theta = -90; theta <= (90 - dtheta); theta += dtheta)
			{
				for (var phi = 0; phi <= (360 - dphi); phi += dphi)
				{
					SphereVertices.Add(new Vector3(
						(float)Math.Cos(theta * DEG_TO_RAD) * (float)Math.Cos(phi * DEG_TO_RAD),
						(float)Math.Cos(theta * DEG_TO_RAD) * (float)Math.Sin(phi * DEG_TO_RAD),
						(float)Math.Sin(theta * DEG_TO_RAD)
					));

					SphereVertices.Add(new Vector3(
						(float)Math.Cos((theta + dtheta) * DEG_TO_RAD) * (float)Math.Cos(phi * DEG_TO_RAD),
						(float)Math.Cos((theta + dtheta) * DEG_TO_RAD) * (float)Math.Sin(phi * DEG_TO_RAD),
						(float)Math.Sin((theta + dtheta) * DEG_TO_RAD)
					));

					SphereVertices.Add(new Vector3(
						(float)Math.Cos((theta + dtheta) * DEG_TO_RAD) * (float)Math.Cos((phi + dphi) * DEG_TO_RAD),
						(float)Math.Cos((theta + dtheta) * DEG_TO_RAD) * (float)Math.Sin((phi + dphi) * DEG_TO_RAD),
						(float)Math.Sin((theta + dtheta) * DEG_TO_RAD)
					));

					if ((theta > -90) && (theta < 90))
					{
						SphereVertices.Add(new Vector3(
							(float)Math.Cos(theta * DEG_TO_RAD) * (float)Math.Cos((phi + dphi) * DEG_TO_RAD),
							(float)Math.Cos(theta * DEG_TO_RAD) * (float)Math.Sin((phi + dphi) * DEG_TO_RAD),
							(float)Math.Sin(theta * DEG_TO_RAD)
						));
					}
				}
			}

			// Box Vertices
			BoxVertices = new List<Vector3>();

			float[] edgeCoord = new float[] { 0.5f, 0.5f, 0.5f };

			for (var i = 0; i < 4; i++)
			{
				for (var j = 0; j < 3; j++)
				{
					var pa = new Vector3(edgeCoord[0], edgeCoord[1], edgeCoord[2]);

					var otherCoord = j % 3;
					edgeCoord[otherCoord] = edgeCoord[otherCoord] * -1;
					var pb = new Vector3(edgeCoord[0], edgeCoord[1], edgeCoord[2]);

					BoxVertices.Add(pb);
					BoxVertices.Add(pa);
				}

				edgeCoord = new float[] { -0.5f, -0.5f, -0.5f };

				if (i < 3)
				{
					edgeCoord[i] = edgeCoord[i] * -1;
				}
			}
		}

		public static void DrawLine(Vector3 v0, Vector3 v1, ColorRgba color) { DrawLine(v0, v1, color, Matrix4.Identity); }

		public static void DrawLine(Vector3 v0, Vector3 v1, ColorRgba color, Matrix4 matrix)
		{
			var p = new GizmosPrimitive();
			p.matrix = matrix;
			p.vertices = new List<Vector3>() { v0, v1 };
			p.color = color;
			GizmosRenderer.Instance.AddPrimitive(p);
		}

		public static void DrawLineStrip(List<Vector3> points, ColorRgba color) { DrawLineStrip(points, color, Matrix4.Identity); }
		public static void DrawLineStrip(List<Vector3> points, ColorRgba color, Matrix4 matrix)
		{
			if (points == null)
			{
				Logs.Core.WriteError("Gizmos.DrawLineStripLine points parameter cannot be null and must have atleast 2 points.");
				return;
			}
			if (points.Count < 2)
			{
				Logs.Core.WriteError("Gizmos.DrawLineStripLine must have at least 2 points.");
				return;
			}
			var p = new GizmosPrimitive();
			p.matrix = matrix;
			p.vertices = points;
			p.color = color;
			GizmosRenderer.Instance.AddPrimitive(p);
		}

		public static void DrawArrow(Vector3 pStart, Vector3 pEnd, float arrowSize, ColorRgba color) { DrawArrow(pStart, pEnd, arrowSize, color, Matrix4.Identity); }

		public static void DrawArrow(Vector3 pStart, Vector3 pEnd, float arrowSize, ColorRgba color, Matrix4 matrix)
		{
			var p = new GizmosPrimitive();
			p.matrix = matrix;
			p.color = color;
			p.vertices = new List<Vector3>();

			p.vertices.Add(pStart);
			p.vertices.Add(pEnd);

			var dir = new Vector3();
			dir = (pEnd - pStart);
			dir.Normalize();

			Vector3 right = new Vector3();
			var dot = Vector3.Dot(dir, Vector3.UnitY);
			if (dot > 0.99 || dot < -0.99)
			{
				right = Vector3.Cross(dir, Vector3.UnitX);
			}
			else
			{
				right = Vector3.Cross(dir, Vector3.UnitY);
			}

			var top = new Vector3();
			top = Vector3.Cross(right, dir);

			dir *= arrowSize;
			right *= arrowSize;
			top *= arrowSize;

			// Right slant.
			p.vertices.Add(pEnd);
			p.vertices.Add((pEnd - right) - dir);

			// Left slant.
			p.vertices.Add(pEnd);
			p.vertices.Add((pEnd - right) - dir);

			// Top slant.
			p.vertices.Add(pEnd);
			p.vertices.Add((pEnd - top) - dir);

			// Bottom slant.
			p.vertices.Add(pEnd);
			p.vertices.Add((pEnd - top) - dir);

			GizmosRenderer.Instance.AddPrimitive(p);
		}

		public static void DrawBoundingBox(Vector3 center, Vector3 size, Vector3 rotation, ColorRgba color)
		{
			Matrix4 matrix = Matrix4.Identity;
			matrix *= Matrix4.CreateScale(size);
			matrix *= Matrix4.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z);
			matrix *= Matrix4.CreateTranslation(center);
			DrawBoundingBox(color, matrix); 
		}
		public static void DrawBoundingBox(ColorRgba color, Matrix4 matrix)
		{
			var p = new GizmosPrimitive();

			p.matrix = matrix; ;

			p.color = color;
			p.vertices = BoxVertices;
			GizmosRenderer.Instance.AddPrimitive(p);
		}


		public static void DrawSphere(Vector3 center, Vector3 size, ColorRgba color)
		{
			Matrix4 matrix = Matrix4.Identity;
			matrix *= Matrix4.CreateScale(size);
			matrix *= Matrix4.CreateTranslation(center);
			DrawSphere(color, matrix);
		}
		public static void DrawSphere(ColorRgba color, Matrix4 matrix)
		{
			var p = new GizmosPrimitive();

			p.matrix = matrix;

			p.color = color;
			p.vertices = SphereVertices;
			GizmosRenderer.Instance.AddPrimitive(p);
		}

		/// <summary>
		/// Construct a rectangle in space
		/// </summary>
		/// <param name="origin">The center point of the rectangle</param>
		/// <param name="normal"> The normal of the rectangle.</param>
		/// <param name="width">The width of the rectangle.</param>
		/// <param name="height">The height of the rectangle.</param>
		/// <param name="color">The color of the rectangle.</param>
		/// <param name="crossed">
		/// Optional, if true a cross will be drawn through opposite borders of
		/// the rectangle.
		/// </param>
		public static void DrawRectangle(Vector3 origin, Vector3 normal, float width, float height, ColorRgba color, bool crossed = false)
		{
			var (left, up) = GetComponentsFromNormal(normal);

			DrawLine(origin + up * height + left * width, origin + up * height - left * width, color);
			DrawLine(origin + up * height - left * width, origin - up * height - left * width, color);
			DrawLine(origin - up * height - left * width, origin - up * height + left * width, color);
			DrawLine(origin - up * height + left * width, origin + up * height + left * width, color);

			if (crossed)
			{
				DrawLine(origin - up * height - left * width, origin + up * height + left * width, color);
				DrawLine(origin - up * height + left * width, origin + up * height - left * width, color);
			}
		}

		/// <summary>
		/// Construct a circle in space
		/// </summary>
		/// <param name="origin">The center point of the circle</param>
		/// <param name="normal">
		/// The normal of the circle, the radius of the circle will be
		/// equivalent to this vector's magnitude.
		/// </param>
		/// <param name="color">The color of the circle</param>
		/// <param name="segments">
		/// Optional, the number of segments to construct the circle out of.
		/// Defaults to 32.
		/// </param>
		public static void DrawCircle(Vector3 origin, Vector3 normal, Vector2 size, ColorRgba color, int segments = 32)
		{
			var (left, up) = GetComponentsFromNormal(normal);

			left *= size.X;
			up *= size.Y;

			for (int i = 0; i < segments; i++)
			{
				float theta0 = 2f * (float)Math.PI * (float)i / segments;
				float theta1 = 2f * (float)Math.PI * (float)(i + 1) / segments;

				float x0 = (float)Math.Cos(theta0);
				float y0 = (float)Math.Sin(theta0);
				float x1 = (float)Math.Cos(theta1);
				float y1 = (float)Math.Sin(theta1);

				DrawLine(origin + (left * x0 + up * y0), origin + (left * x1 + up * y1), color);
			}
		}


		public static void DrawCone(Vector3 position, Vector3 rotation, float distance, float angle, ColorRgba color)
		{
			float coneWidth = distance * (float)MathF.Tan(angle);

			Matrix4 Matrix = Matrix4.Identity;
			Matrix *= Matrix4.CreateScale(coneWidth, coneWidth, 1f);
			Matrix *= Matrix4.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z);
			Matrix *= Matrix4.CreateTranslation(position + (Matrix4.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z).Forward * distance));
			DrawCircle(Matrix.Translation, Matrix.Forward, new Vector2(coneWidth, coneWidth), color);

			Matrix = Matrix4.Identity;
			Matrix *= Matrix4.CreateScale(coneWidth, coneWidth, -distance);
			Matrix *= Matrix4.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z);
			Matrix *= Matrix4.CreateTranslation(position);

			DrawLine(new Vector3(0, 0, 0), new Vector3(0, 0, 1), color, Matrix);
			DrawLine(new Vector3(0, 0, 0), new Vector3(1, 0, 1), color, Matrix);
			DrawLine(new Vector3(0, 0, 0), new Vector3(-1, 0, 1), color, Matrix);
			DrawLine(new Vector3(0, 0, 0), new Vector3(0, 1, 1), color, Matrix);
			DrawLine(new Vector3(0, 0, 0), new Vector3(0, -1, 1), color, Matrix);
		}


		public static void DrawConeWithDirection(Vector3 position, Vector3 direction, float distance, float angle, ColorRgba color)
		{
			float coneWidth = distance * (float)MathF.Tan(angle);


			Matrix4 MatrixWorld = Matrix4.CreateWorld(Vector3.Zero, direction, Vector3.Up);

			Matrix4 Matrix = Matrix4.Identity;
			Matrix *= Matrix4.CreateScale(coneWidth, coneWidth, 1f);
			Matrix *= MatrixWorld;
			Matrix *= Matrix4.CreateTranslation(position + (direction * distance));
			DrawCircle(Matrix.Translation, Matrix.Forward, new Vector2(coneWidth, coneWidth), color);

			Matrix = Matrix4.Identity;
			Matrix *= Matrix4.CreateScale(coneWidth, coneWidth, -distance);
			Matrix *= MatrixWorld;
			Matrix *= Matrix4.CreateTranslation(position);

			DrawLine(new Vector3(0, 0, 0), new Vector3(0, 0, 1), color, Matrix);
			DrawLine(new Vector3(0, 0, 0), new Vector3(1, 0, 1), color, Matrix);
			DrawLine(new Vector3(0, 0, 0), new Vector3(-1, 0, 1), color, Matrix);
			DrawLine(new Vector3(0, 0, 0), new Vector3(0, 1, 1), color, Matrix);
			DrawLine(new Vector3(0, 0, 0), new Vector3(0, -1, 1), color, Matrix);
		}

		public static void DrawDirectionalLight(Vector3 position, Vector3 rotation, float size, float nearClip, float farClip, ColorRgba color)
		{
			Vector3 normal = Matrix4.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z).Forward;
			DrawRectangle(position + (normal * nearClip), normal, size, size, color, true);
			DrawRectangle(position + (normal * farClip), normal, size, size, color);

			var (left, up) = GetComponentsFromNormal(normal);

			Vector3 tl = position + up * size + left * size;
			Vector3 tr = position + up * size - left * size;
			Vector3 bl = position - up * size + left * size;
			Vector3 br = position - up * size - left * size;

			DrawLine(tl + (normal * nearClip), tl + (normal * farClip), color);
			DrawLine(tr + (normal * nearClip), tr + (normal * farClip), color);
			DrawLine(bl + (normal * nearClip), bl + (normal * farClip), color);
			DrawLine(br + (normal * nearClip), br + (normal * farClip), color);


			DrawLine(position, tl + (normal * nearClip), ColorRgba.Red);
			DrawLine(position, tr + (normal * nearClip), ColorRgba.Red);
			DrawLine(position, bl + (normal * nearClip), ColorRgba.Red);
			DrawLine(position, br + (normal * nearClip), ColorRgba.Red);


			DrawLine(position, position + (normal * farClip), ColorRgba.Black);
		}

		/// <summary>
		/// Construct a cylinder in space
		/// </summary>
		/// <param name="origin">The center point of the cylinder</param>
		/// <param name="radius">The radius of the cylinder.</param>
		/// <param name="height">The height of the cylinder.</param>
		/// <param name="segments">
		/// Optional, the number of segments to construct the cylinder out of.
		/// Defaults to 16.
		/// </param>
		public static void DrawWireCylinder(Vector3 origin, float radius, float height, ColorRgba color, int segments = 16)
		{
			// Construct by hand each time instead of from mesh because we want
			// to use a non-tessalated mesh.
			Vector3 top = origin + Vector3.Up * height / 2f;
			Vector3 bot = origin - Vector3.Up * height / 2f;

			for (int i = 0; i < segments; i++)
			{
				float theta0 = 2f * (float)Math.PI * (float)i / segments;
				float theta1 = 2f * (float)Math.PI * (float)(i + 1) / segments;

				float x0 = radius * (float)Math.Cos(theta0);
				float y0 = radius * (float)Math.Sin(theta0);
				float x1 = radius * (float)Math.Cos(theta1);
				float y1 = radius * (float)Math.Sin(theta1);

				Vector3 left = Vector3.Left;
				Vector3 fore = Vector3.Forward;
				// Top circle
				Gizmos.DrawLine((top + left * x0 + fore * y0), (top + left * x1 + fore * y1), color);
				// Bottom circle
				Gizmos.DrawLine((bot + left * x0 + fore * y0), (bot + left * x1 + fore * y1), color);
				// Sides
				Gizmos.DrawLine((top + left * x0 + fore * y0), (bot + left * x0 + fore * y0), color);
			}
		}

		/// <summary>
		/// Calculates the component left and up vectors in the plane perpendicular to the
		/// specified normal.
		/// </summary>
		/// <param name="normal">A normalized vector perpendicular to the desired plane.</param>
		/// <returns>
		/// A pair of vectors perpendicular to each other and contained within the plain
		/// that is perpendicular to the given normal vector.
		/// </returns>
		private static (Vector3 left, Vector3 up) GetComponentsFromNormal(Vector3 normal)
		{
			Vector3 left = Vector3.Cross(normal, Vector3.Up).Normalized;
			Vector3 up = Vector3.Cross(left, normal).Normalized;

			// Handle the case where the normal is directly up or down. In that case the
			// cross product used to calculate the left and up vectors will be 0-length,
			// causing the circle to not draw correctly. To avoid this, we manually
			// specify the left and up vectors so that the circle will draw correctly in
			// the X-Z plane.
			if (RoughlyEqual(left.LengthSquared, 0))
			{
				left = Vector3.Left;
				up = Vector3.Forward;
			}

			return (left, up);
		}

		static bool RoughlyEqual(double a, double b)
		{
			return (Math.Abs(a - b) <= double.Epsilon);
		}
	}
}
