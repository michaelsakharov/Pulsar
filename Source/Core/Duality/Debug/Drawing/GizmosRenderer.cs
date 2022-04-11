using System;
using System.Collections.Generic;
using THREE;
using THREE.Core;
using THREE.Materials;
using THREE.Objects;
using THREE.Scenes;

namespace Duality.DebugDraw
{
	public class GizmosRenderer
	{
		List<GizmosPrimitive> activePrimitives = new List<GizmosPrimitive>();

		LineSegments activeMesh;

		public static GizmosRenderer Instance;

		public void AddPrimitive(GizmosPrimitive p)
		{
			activePrimitives.Add(p);
		}

		public BufferGeometry ConstructGeometry()
		{
			List<float> positions = new List<float>();
			List<float> colors = new List<float>();
			List<int> indices = new List<int>();

			// Collect all primitives into geometry buffers.
			int i, j;
			var indexOffset = 0;
			for (i = 0; i < activePrimitives.Count; i++)
			{
				var p = activePrimitives[i];

				// Vertices/colors.
				for (j = 0; j < p.vertices.Count; j++)
				{
					var v = p.vertices[j] * p.matrix;
					positions.Add(v.X);
					positions.Add(v.Y);
					positions.Add(v.Z);
					colors.Add(p.color.R / 255f);
					colors.Add(p.color.G / 255f);
					colors.Add(p.color.B / 255f);
				}

				// Indices.
				for (j = 0; j < p.vertices.Count - 1; j++)
				{
					indices.Add(indexOffset + j);
					indices.Add(indexOffset + j + 1);
				}
				indexOffset += p.vertices.Count;
			}

			var geometry = new BufferGeometry();
			geometry.SetIndex(indices, 1);
			geometry.SetAttribute("position", new BufferAttribute<float>(positions.ToArray(), 3));
			geometry.SetAttribute("color", new BufferAttribute<float>(colors.ToArray(), 3));
			geometry.ComputeBoundingSphere();
			return geometry;
		}

		public void Update(Scene scene)
		{
			scene.Remove(activeMesh);
			if (activeMesh != null)
			{
				scene.Remove(activeMesh);
				activeMesh.Dispose();
				activeMesh = null;
			}

			if (activePrimitives.Count == 0)
				return;

			// Create geometry and add to scene.
			var geometry = ConstructGeometry();
			var material = new LineBasicMaterial() { VertexColors = true };
			activeMesh = new LineSegments(geometry, material);
			scene.Add(activeMesh);

			// Clear primitives from this frame.
			activePrimitives.Clear();
		}
	}
}
