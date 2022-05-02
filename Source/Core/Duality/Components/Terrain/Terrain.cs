using System;
using System.Collections;
using System.Collections.Generic;
using Duality.Components;
using Duality.Drawing;
using Duality.Editor;
using Duality.Properties;
using Duality.Resources;

namespace Duality.Graphics.Components
{
	[RequiredComponent(typeof(Transform))]
	[EditorHintCategory(CoreResNames.CategoryNone)]
	[EditorHintImage(CoreResNames.ImageComponent)]
	public class TerrainComponent : Component, ICmpInitializable, ICmpUpdatable, ICmpEditorUpdatable, IDisposable
	{
		private double size = 512;
		public double Size { get { return this.size; } set { this.size = value; } }

		private double heightMultiplier = 1f;
		public double HeightMultiplier { get { return this.heightMultiplier; } set { this.heightMultiplier = value; } }

		private int maxLevel = 4;
		public int MaxLevel { get { return this.maxLevel; } set { this.maxLevel = value; } }

		private bool drawQuadtree = true;
		public bool DrawQuadtree { get { return this.drawQuadtree; } set { this.drawQuadtree = value; } }

		private ContentRef<Texture> heightmap;
		public ContentRef<Texture> Heightmap
		{
			get { return this.heightmap; }
			set { this.heightmap = value; _heightmapDirty = true; }
		}

		private ContentRef<Material> terrainMat;
		public ContentRef<Material> TerrainMat
		{
			get { return this.terrainMat; }
			set { this.terrainMat = value; }
		}

		private Transform target;
		public Transform Target
		{
			get { return this.target; }
			set { this.target = value; }
		}

		[DontSerialize] private TerrainNode Root;
		[DontSerialize] private int heightmapResolution = -1;
		[DontSerialize] private double[,] heightData;
		[DontSerialize] private bool _heightmapDirty;

		void ICmpInitializable.OnActivate()
		{
			if (Heightmap.IsAvailable)
			{
				if (Heightmap.Res.Width != Heightmap.Res.Height)
				{
					Logs.Core.WriteError("Terrain Heightmap must be a Square!");
					return;
				}
				RefreshTerrain();
			}
		}

		void ICmpInitializable.OnDeactivate()
		{
			if (Root != null)
				Root.Dispose();
		}

		void IDisposable.Dispose()
		{
			if (Root != null)
				Root.Dispose();
		}

		void ICmpUpdatable.OnUpdate() { Update(); }

		void ICmpEditorUpdatable.OnUpdate() { Update(); }

		private void Update()
		{
			// Check if we have Heightmap Data
			if (heightmapResolution != -1 && _heightmapDirty == false)
			{
				if (Root != null)
				{
					Root.Update();
					//Root.Draw();
					if (DrawQuadtree)
						Root.DrawDebug();
				}
			}
			else if(Heightmap.IsAvailable)
			{
				// We have a Heightmap now
				RefreshTerrain();
			}
		}

		public void RefreshTerrain()
		{
			_heightmapDirty = false;
			ExtractHeightData();
			if (Root != null)
				Root.Dispose();

			Root = new TerrainNode(0, 0, Size, 0, MaxLevel, this);
		}

		private void ExtractHeightData()
		{
			if (Heightmap.IsAvailable)
			{
				heightmapResolution = Heightmap.Res.Width;
				heightData = new double[heightmapResolution, heightmapResolution];
				ColorRgba[] map = Heightmap.Res.GetPixelData().Data;

				for (int y = 0; y < heightmapResolution; y++)
				{
					for (int x = 0; x < heightmapResolution; x++)
					{
						heightData[y, x] = map[y * heightmapResolution + x].Grayscale * HeightMultiplier;
					}
				}
			}
		}

		private Vector2Int ConvertLocalPosToHeightmap(double x, double y)
		{
			int xPos = MathF.FloorToInt((x / Size) * (heightmapResolution - 1));
			int yPos = MathF.FloorToInt((y / Size) * (heightmapResolution - 1));
			return new Vector2Int(MathF.Clamp(xPos, 0, heightmapResolution - 1), MathF.Clamp(yPos, 0, heightmapResolution - 1));
		}

		private double GetHeightAt(double x, double y)
		{
			Vector2Int position = ConvertLocalPosToHeightmap(x, y);
			return heightData[position.X, position.Y];
		}
	}
}