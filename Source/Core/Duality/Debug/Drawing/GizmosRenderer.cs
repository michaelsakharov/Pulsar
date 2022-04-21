using System;
using System.Collections.Generic;
using Duality.Components;
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

		public BufferGeometry ConstructGeometry(Camera camera)
		{
			List<float> positions = new List<float>();
			List<float> colors = new List<float>();
			List<int> indices = new List<int>();

			Vector3 toRelative = Vector3.Zero;
			if (Duality.Resources.Scene.Current.MoveWorldInsteadOfCamera)
			{
				toRelative = camera.GameObj.Transform.Pos;
			}

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
					v = v - toRelative;
					positions.Add((float)v.X);
					positions.Add((float)v.Y);
					positions.Add((float)v.Z);
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

		public void Update(Scene scene, Camera camera)
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
			var geometry = ConstructGeometry(camera);
			var material = new LineBasicMaterial() { VertexColors = true };
			activeMesh = new LineSegments(geometry, material);
			scene.Add(activeMesh);

			// Clear primitives from this frame.
			activePrimitives.Clear();
		}
	}
}
