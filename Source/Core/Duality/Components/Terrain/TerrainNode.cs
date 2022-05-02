using System.Collections;
using System.Collections.Generic;
using Duality.DebugDraw;
using Duality.Drawing;
using Duality.Resources;

namespace Duality.Graphics.Components
{
	public class TerrainNode
	{
		public double X;
		public double Y;
		public double Size;
		public int Level;
		public int MaxLevel;
		public bool isSubdivided;

		public TerrainNode NW;
		public TerrainNode NE;
		public TerrainNode SW;
		public TerrainNode SE;

		public TerrainComponent Terrain;

		public const int leafResolution = 10;
		public const double LODMultiplier = 1f;

		public TerrainNode(double x, double y, double size, int level, int maxlevel, TerrainComponent terrain)
		{
			this.X = x;
			this.Y = y;
			this.Size = size;
			this.Level = level;
			this.MaxLevel = maxlevel;
			this.Terrain = terrain;

			isSubdivided = false;
		}

		public void Update()
		{
			if (Terrain.Target != null &&  Vector3.Distance(Terrain.Target.Pos, GetCenter()) < (Size * LODMultiplier))
			{
				Subdivide();
			}
			else
			{
				Merge();
			}
			if (isSubdivided)
			{
				NW.Update();
				NE.Update();
				SW.Update();
				SE.Update();
			}
		}

		Vector3 GetCenter()
		{
			double halfSize = Size * 0.5;
			return new Vector3(X + halfSize, 0, Y + halfSize);
		}

		void Merge()
		{
			// Don't merge if this isnt already subdivided
			if (isSubdivided)
			{
				NW.Merge();
				NW = null;
				NE.Merge();
				NE = null;
				SW.Merge();
				SW = null;
				SE.Merge();
				SE = null;

				isSubdivided = false;
			}
		}

		void Subdivide()
		{
			// Don't subdivide if this is a Leaf Node
			if (Level == MaxLevel)
				return;

			// Don't subdivide if this isnt already subdivided
			if (isSubdivided) return;

			double halfSize = Size * 0.5;

			NW = new TerrainNode(X, Y, halfSize, Level + 1, MaxLevel, Terrain);
			NE = new TerrainNode(X + halfSize, Y, halfSize, Level + 1, MaxLevel, Terrain);
			SW = new TerrainNode(X, Y + halfSize, halfSize, Level + 1, MaxLevel, Terrain);
			SE = new TerrainNode(X + halfSize, Y + halfSize, halfSize, Level + 1, MaxLevel, Terrain);

			isSubdivided = true;
		}

		public void Dispose()
		{
			if (isSubdivided)
			{
				NW.Dispose();
				NW = null;
				NE.Dispose();
				NE = null;
				SW.Dispose();
				SW = null;
				SE.Dispose();
				SE = null;
			}
			else
			{
				// Destroy our mesh if we have one
			}


			isSubdivided = false;
		}

		public void Draw()
		{
			if (isSubdivided)
			{
				NW.Draw();
				NE.Draw();
				SW.Draw();
				SE.Draw();
			}
			else
			{
				// Initially created in Unity so this concept needs to be ported to Pulsar
				// In Unity I did a quick DrawMesh, But in pulsar we probably want these to be Children of the Terrain object
				// Ideally with the least overhead possible, making them pulsar gameobjects comes with a lot of overhead
				// we can go a lil more direct and make them THREE.js Objects and child them to eachother then position the Root at the position of the terrain
				// that seems like the better route, only issue is making them THREE.js Objects comes agian with the overhead of said objects
				// Possibly some way to replicate Unity's DrawMesh method might be nice, but Three.js's structure makes that hard todo
				// Could do it like our Gizmos renderer, but Hold it for the ENtire frame, so all cameras will render it instead of a single Render call
				// should probably do that for Gizmos as well honestly
				//if (leafMesh == null)
				//{
				//	leafMesh = new Mesh();
				//
				//	float step = Size / leafResolution;
				//	var vertices = new Vector3[(leafResolution + 1) * (leafResolution + 1)];
				//	Vector3[] normals = new Vector3[(leafResolution + 1) * (leafResolution + 1)];
				//	for (int i = 0, y = 0; y <= leafResolution; y++)
				//	{
				//		for (int x = 0; x <= leafResolution; x++, i++)
				//		{
				//			vertices[i] = new Vector3(x * step, 0, y * step);
				//			float xPos = X + vertices[i].x;
				//			float yPos = Y + vertices[i].z;
				//
				//			// Terrain Height Offset
				//			vertices[i].Y = Terrain.GetHeightAt(xPos, yPos);
				//
				//			normals[i] = Terrain.GetNormalAt(xPos, yPos);
				//		}
				//	}
				//	leafMesh.vertices = vertices;
				//	leafMesh.normals = normals;
				//
				//	int[] triangles = new int[leafResolution * leafResolution * 6];
				//	for (int ti = 0, vi = 0, y = 0; y < leafResolution; y++, vi++)
				//	{
				//		for (int x = 0; x < leafResolution; x++, ti += 6, vi++)
				//		{
				//			triangles[ti] = vi;
				//			triangles[ti + 3] = triangles[ti + 2] = vi + 1;
				//			triangles[ti + 4] = triangles[ti + 1] = vi + leafResolution + 1;
				//			triangles[ti + 5] = vi + leafResolution + 2;
				//		}
				//	}
				//	leafMesh.triangles = triangles;
				//
				//	leafMesh.RecalculateNormals();
				//	leafMesh.RecalculateBounds();
				//}
				//// Draw the mesh
				//Graphics.DrawMesh(leafMesh, new Vector3(X, 0, Y), Quaternion.identity, Terrain.TerrainMat, 0);
			}
		}

		public void DrawDebug()
		{
			Gizmos.DrawLine(new Vector3(X, 0, Y), new Vector3(X + Size, 0, Y), ColorRgba.Green);
			Gizmos.DrawLine(new Vector3(X + Size, 0, Y), new Vector3(X + Size, 0, Y + Size), ColorRgba.Green);
			Gizmos.DrawLine(new Vector3(X + Size, 0, Y + Size), new Vector3(X, 0, Y + Size), ColorRgba.Green);
			Gizmos.DrawLine(new Vector3(X, 0, Y + Size), new Vector3(X, 0, Y), ColorRgba.Green);
			if (isSubdivided)
			{
				NW.DrawDebug();
				NE.DrawDebug();
				SW.DrawDebug();
				SE.DrawDebug();
			}
		}
	}
}