using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using Duality.Resources;
using Duality.Backend;

namespace Duality.Drawing
{
	[DontSerialize]
	public class DrawDevice : IDrawDevice, IDisposable
	{
		/// <summary>
		/// Represents a drawing input from <see cref="AddVertices"/> using 
		/// dynamically gathered vertices.
		/// </summary>
		private struct VertexDrawItem
		{
			/// <summary>
			/// The offset in the shared vertex buffer where this drawing items vertex data begins.
			/// </summary>
			public ushort Offset;
			/// <summary>
			/// The number of vertices that are rendered by this drawing item.
			/// </summary>
			public ushort Count;
			/// <summary>
			/// The index of the vertex buffer that is used by this drawing item.
			/// </summary>
			public byte BufferIndex;
			/// <summary>
			/// The <see cref="VertexDeclaration"/> type index that describes the vertices used by this drawing item.
			/// </summary>
			public byte TypeIndex;
			/// <summary>
			/// The <see cref="VertexMode"/> that describes how the vertices are assembled into geometry.
			/// </summary>
			public VertexMode Mode;
			/// <summary>
			/// The material / <see cref="BatchInfo"/> that describes how the vertices will be rendered.
			/// </summary>
			public BatchInfo Material;

			/// <summary>
			/// Determines whether this item could share the same <see cref="DrawBatch"/>
			/// with the specified other item.
			/// </summary>
			/// <param name="other"></param>
			/// <returns></returns>
			public bool CanShareBatchWith(ref VertexDrawItem other)
			{
				return
					this.Mode == other.Mode &&
					this.TypeIndex == other.TypeIndex &&
					this.BufferIndex == other.BufferIndex &&
					this.Material.Equals(other.Material);
			}
			/// <summary>
			/// Determines whether the specified item could be appended as-is to this item.
			/// </summary>
			/// <param name="other"></param>
			/// <returns></returns>
			public bool CanAppend(ref VertexDrawItem other)
			{
				return
					this.Offset + this.Count == other.Offset &&
					this.Mode == other.Mode &&
					this.TypeIndex == other.TypeIndex &&
					this.BufferIndex == other.BufferIndex &&
					this.Material.Equals(other.Material);
			}

			public override string ToString()
			{
				VertexDeclaration declaration = VertexDeclaration.Get(this.TypeIndex);
				return string.Format(
					"{0}[{1}@{2}] as {3}, '{4}'",
					declaration.DataType.Name,
					this.Count,
					this.Offset,
					this.Mode,
					this.Material);
			}
		}
		/// <summary>
		/// Represents an item in a sorting queue that is associated with 
		/// a <see cref="VertexDrawItem"/>. This struct is actually a union
		/// that uses either <see cref="SortDepth"/> or <see cref="SortIndex"/>
		/// depending on whether sorting is done based on material or depth.
		/// </summary>
		[StructLayout(LayoutKind.Explicit)]
		private struct SortItem
		{
			/// <summary>
			/// A drawing buffer index where the associated <see cref="VertexDrawItem"/>
			/// can be found.
			/// </summary>
			[FieldOffset(0)] public int DrawItemIndex;
			/// <summary>
			/// The material sorting index for this item.
			/// </summary>
			[FieldOffset(4)] public int SortIndex;
			/// <summary>
			/// The depth sorting reference value for this item.
			/// </summary>
			[FieldOffset(4)] public float SortDepth;

			/// <summary>
			/// Determines whether this item shares roughly the specified reference depth.
			/// </summary>
			/// <param name="otherDepth"></param>
			/// <returns></returns>
			public bool CanShareDepth(float otherDepth)
			{
				 return Math.Abs(this.SortDepth - otherDepth) < 0.00001f;
			}
			/// <summary>
			/// Adjusts this items <see cref="SortDepth"/> value to account for a merge
			/// with another item.
			/// </summary>
			/// <param name="otherDepth"></param>
			/// <param name="count"></param>
			/// <param name="otherCount"></param>
			public void MergeDepth(float otherDepth, int count, int otherCount)
			{
				this.SortDepth = 
					(float)(count * this.SortDepth + otherCount * otherDepth) /
					(float)(count + otherCount);
			}

			public override string ToString()
			{
				return string.Format(
					"Item {0}, Sort Index {1} / Depth {2:F}",
					this.DrawItemIndex,
					this.SortIndex,
					this.SortDepth);
			}
		}
		
		
		/// <summary>
		/// The default reference distance for perspective rendering.
		/// </summary>
		public const float DefaultFocusDist	= 10.0f;

		
		private bool                      disposed         = false;
		private float                     nearZ            = 0.1f;
		private float                     farZ             = 10000.0f;
		private float                     focusDist        = DefaultFocusDist;
		private ClearFlag                 clearFlags       = ClearFlag.All;
		private ColorRgba                 clearColor       = ColorRgba.TransparentBlack;
		private float                     clearDepth       = 1.0f;
		private Vector2                   targetSize       = Vector2.Zero;
		private Rect                      viewportRect     = Rect.Empty;
		private Vector3                   viewerPos        = Vector3.Zero;
		private Vector3					  viewerAngle	   = Vector3.Zero;
		private ContentRef<RenderTarget>  renderTarget     = null;
		private ProjectionMode            projection       = ProjectionMode.Perspective;
		private Matrix4                   matView          = Matrix4.Identity;
		private Matrix4                   matProjection    = Matrix4.Identity;
		private Matrix4                   matFinal         = Matrix4.Identity;
		private Matrix4                   matFinalInv      = Matrix4.Identity;
		private VisibilityFlag            visibilityMask   = VisibilityFlag.All;
		private int                       pickingIndex     = 0;
		private ShaderParameterCollection shaderParameters = new ShaderParameterCollection();

		private RenderOptions                  renderOptions      = new RenderOptions();
		private RenderStats                    renderStats        = new RenderStats();
		private List<BatchInfo>                tempMaterialPool   = new List<BatchInfo>();
		private int                            tempMaterialIndex  = 0;
		private VertexBatchStore               drawVertices       = new VertexBatchStore(ushort.MaxValue);
		private RawList<VertexDrawItem>        drawBuffer         = new RawList<VertexDrawItem>();
		private RawList<SortItem>              sortBufferSolid    = new RawList<SortItem>();
		private RawList<SortItem>              sortBufferBlended  = new RawList<SortItem>();
		private RawList<SortItem>              sortBufferTemp     = new RawList<SortItem>();
		private RawList<DrawBatch>             batchBufferSolid   = new RawList<DrawBatch>();
		private RawList<DrawBatch>             batchBufferBlended = new RawList<DrawBatch>();
		private RawListPool<VertexDrawRange>   batchIndexPool     = new RawListPool<VertexDrawRange>();
		private RawList<RawList<VertexBuffer>> vertexBuffers      = new RawList<RawList<VertexBuffer>>();
		private int                            numRawBatches      = 0;


		public bool Disposed
		{
			get { return this.disposed; }
		}
		public Vector3 ViewerPos
		{
			get { return this.viewerPos; }
			set
			{
				if (this.viewerPos == value) return;
				this.viewerPos = value;
				this.UpdateMatrices();
			}
		}
		public Vector3 ViewerAngle
		{
			get { return this.viewerAngle; }
			set
			{
				if (this.viewerAngle == value) return;
				this.viewerAngle = value;
				this.UpdateMatrices();
			}
		}
		public float FocusDist
		{
			get { return this.focusDist; }
			set
			{
				if (this.focusDist == value) return;
				this.focusDist = value;
				this.UpdateMatrices();
			}
		}
		public VisibilityFlag VisibilityMask
		{
			get { return this.visibilityMask; }
			set { this.visibilityMask = value; }
		}
		public float NearZ
		{
			get { return this.nearZ; }
			set
			{
				if (this.nearZ == value) return;
				this.nearZ = value;
				this.UpdateMatrices();
			}
		}
		public float FarZ
		{
			get { return this.farZ; }
			set
			{
				if (this.farZ == value) return;
				this.farZ = value;
				this.UpdateMatrices();
			}
		}
		/// <summary>
		/// [GET / SET] The clear color to apply when clearing the color buffer.
		/// </summary>
		public ColorRgba ClearColor
		{
			get { return this.clearColor; }
			set { this.clearColor = value; }
		}
		/// <summary>
		/// [GET / SET] The clear depth to apply when clearing the depth buffer
		/// </summary>
		public float ClearDepth
		{
			get { return this.clearDepth; }
			set { this.clearDepth = value; }
		}
		/// <summary>
		/// [GET / SET] Specifies which buffers to clean before rendering with this device.
		/// </summary>
		public ClearFlag ClearFlags
		{
			get { return this.clearFlags; }
			set { this.clearFlags = value; }
		}
		/// <summary>
		/// [GET / SET] Specified the projection that is applied when rendering the world.
		/// </summary>
		public ProjectionMode Projection
		{
			get { return this.projection; }
			set
			{
				if (this.projection == value) return;
				this.projection = value;
				this.UpdateMatrices();
			}
		}
		public ContentRef<RenderTarget> Target
		{
			get { return this.renderTarget; }
			set { this.renderTarget = value; }
		}
		public int PickingIndex
		{
			get { return this.pickingIndex; }
			set { this.pickingIndex = value; }
		}
		public bool IsPicking
		{
			get { return this.pickingIndex != 0; }
		}
		public Rect ViewportRect
		{
			get { return this.viewportRect; }
			set { this.viewportRect = value; }
		}
		public Vector2 TargetSize
		{
			get { return this.targetSize; }
			set
			{
				if (this.targetSize == value) return;
				this.targetSize = value;
				this.UpdateMatrices();
			}
		}
		/// <summary>
		/// [GET] Provides access to the drawing devices shared <see cref="ShaderParameterCollection"/>,
		/// which allows to specify a parameter value globally across all materials rendered by this
		/// <see cref="DrawDevice"/>.
		/// </summary>
		public ShaderParameterCollection ShaderParameters
		{
			get { return this.shaderParameters; }
		}

		
		public void Dispose()
		{
			if (this.disposed) return;
			this.disposed = true;

			// Set big object references to null to make
			// sure they're garbage collected even when keeping
			// a reference to the disposed DrawDevice around.
			this.renderOptions = null;
			this.renderStats = null;
			this.tempMaterialPool = null;
			this.drawVertices = null;
			this.drawBuffer = null;
			this.sortBufferSolid = null;
			this.sortBufferBlended = null;
			this.sortBufferTemp = null;
			this.batchBufferSolid = null;
			this.batchBufferBlended = null;
			this.batchIndexPool = null;
		}

		
		/// <summary>
		/// Returns the scale factor of objects that are located at the specified world space Z position.
		/// </summary>
		/// <param name="z"></param>
		public float GetScaleAtZ(float z)
		{
			if (this.projection == ProjectionMode.Screen)
				return 1.0f;
			else if (this.projection == ProjectionMode.Perspective)
				return this.focusDist / Math.Max(z - this.viewerPos.Z, this.nearZ);
			else
				return this.focusDist / DefaultFocusDist;
		}

		/// <summary>
		/// Projects a <see cref="Vector3"/> from model space into screen space.
		/// The source point is transformed from model space to world space by the world matrix,
		/// then from world space to view space by the view matrix, and
		/// finally from view space to screen space by the projection matrix.
		/// </summary>
		/// <param name="source">The <see cref="Vector3"/> to project.</param>
		/// <param name="projection">The projection <see cref="Matrix4"/>.</param>
		/// <param name="view">The view <see cref="Matrix4"/>.</param>
		/// <param name="world">The world <see cref="Matrix4"/>.</param>
		/// <returns></returns>
		public Vector3 Project(Vector3 source, Matrix4 projection, Matrix4 view, Matrix4 world)
		{
			Matrix4 matrix = Matrix4.Multiply(Matrix4.Multiply(world, view), projection);
			Vector3 vector = Vector3.Transform(source, matrix);
			float a = (((source.X * matrix.M14) + (source.Y * matrix.M24)) + (source.Z * matrix.M34)) + matrix.M44;
			if (!WithinEpsilon(a, 1f))
			{
				vector.X = vector.X / a;
				vector.Y = vector.Y / a;
				vector.Z = vector.Z / a;
			}
			vector.X = (((vector.X + 1f) * 0.5f) * this.viewportRect.W) + this.viewportRect.X;
			vector.Y = (((-vector.Y + 1f) * 0.5f) * this.viewportRect.H) + this.viewportRect.Y;
			//vector.Z = (vector.Z * (this.maxDepth - this.minDepth)) + this.minDepth;
			return vector;
		}

		public Vector3 Project(Vector3 source)
		{
			return this.Project(source, this.matProjection, this.matView, Matrix4.Identity);
		}

		/// <summary>
		/// Unprojects a <see cref="Vector3"/> from screen space into model space.
		/// The source point is transformed from screen space to view space by the inverse of the projection matrix,
		/// then from view space to world space by the inverse of the view matrix, and
		/// finally from world space to model space by the inverse of the world matrix.
		/// Note source.Z must be less than or equal to MaxDepth.
		/// </summary>
		/// <param name="source">The <see cref="Vector3"/> to unproject.</param>
		/// <param name="projection">The projection <see cref="Matrix4"/>.</param>
		/// <param name="view">The view <see cref="Matrix4"/>.</param>
		/// <param name="world">The world <see cref="Matrix4"/>.</param>
		/// <returns></returns>
		public Vector3 Unproject(Vector3 source, Matrix4 projection, Matrix4 view, Matrix4 world)
		{
			Matrix4 matrix = Matrix4.Invert(Matrix4.Multiply(Matrix4.Multiply(world, view), projection));
			source.X = (((source.X - this.viewportRect.X) / ((float)this.viewportRect.W)) * 2f) - 1f;
			source.Y = -((((source.Y - this.viewportRect.Y) / ((float)this.viewportRect.H)) * 2f) - 1f);
			//source.Z = (source.Z - this.minDepth) / (this.maxDepth - this.minDepth);
			Vector3 vector = Vector3.Transform(source, matrix);
			float a = (((source.X * matrix.M14) + (source.Y * matrix.M24)) + (source.Z * matrix.M34)) + matrix.M44;
			if (!WithinEpsilon(a, 1f))
			{
				vector.X = vector.X / a;
				vector.Y = vector.Y / a;
				vector.Z = vector.Z / a;
			}
			return vector;

		}

		public Vector3 Unproject(Vector3 source)
		{
			return this.Unproject(source, this.matProjection, this.matView, Matrix4.Identity);
		}

		private static bool WithinEpsilon(float a, float b)
		{
			float num = a - b;
			return ((-1.401298E-45f <= num) && (num <= float.Epsilon));
		}

		/// <summary>
		/// Creates a Ray translating screen cursor position into screen position
		/// </summary>
		/// <param name="screenPoint"></param>
		/// <returns></returns>
		public Ray CalculateScreenPointRay(Vector2 screenPoint)
		{
			// create 2 positions in screenspace using the cursor position. 0 is as
			// close as possible to the camera, 1 is as far away as possible.
			Vector3 nearSource = new Vector3(screenPoint, 0f);
			Vector3 farSource = new Vector3(screenPoint, 1f);

			// use Viewport.Unproject to tell what those two screen space positions
			// would be in world space. we'll need the projection matrix and view
			// matrix, which we have saved as member variables. We also need a world
			// matrix, which can just be identity.
			Vector3 nearPoint = this.Unproject(nearSource);
			Vector3 farPoint = this.Unproject(farSource);

			// find the direction vector that goes from the nearPoint to the farPoint
			// and normalize it....
			Vector3 direction = farPoint - nearPoint;
			direction.Normalize();

			// and then create a new ray using nearPoint as the source.
			return new Ray(nearPoint, direction);
		}

		/// <summary>
		/// Transforms screen space to world space positions. The screen positions Z coordinate [0, 1] is
		/// interpreted as the interpolated location between the Near and Far clip planes, a Z of 0 is on the near clip plane, a Z of 1 would be on the Far Clip Plane
		/// </summary>
		/// <param name="screenPos"></param>
		public Vector3 GetWorldPos(Vector3 screenPos)
		{
			//Ray ray = this.CalculateScreenPointRay(screenPos.Xy);
			//return ray.Position + (ray.Direction * screenPos.Z);
			return this.Unproject(screenPos);
		}
		/// <summary>
		/// Transforms world space to screen space positions.
		/// </summary>
		/// <param name="worldPos"></param>
		public Vector2 GetScreenPos(Vector3 worldPos)
		{
			var result = this.Project(worldPos);
			return new Vector2(result.X, result.Y);
		}
		
		/// <summary>
		/// Determines whether a point or sphere is inside the devices viewing frustum,
		/// given a world space position and radius.
		/// </summary>
		/// <param name="worldPos">The points world space position.</param>
		/// <param name="radius">A world space radius around the point.</param>
		public bool IsSphereInView(Vector3 worldPos, float radius)
		{
			BoundingFrustum frustum = new BoundingFrustum(this.matView * this.matProjection);
			var result = frustum.Contains(new BoundingSphere(worldPos, radius));
			return result == ContainmentType.Intersects || result == ContainmentType.Contains;
		}
		
		/// <summary>
		/// Rents a temporary material instance which can be used for rendering. The instance
		/// is returned implicitly when the device is done with the current rendering operation.
		/// </summary>
		public BatchInfo RentMaterial()
		{
			int index = this.tempMaterialIndex++;

			if (index >= this.tempMaterialPool.Count)
				this.tempMaterialPool.Add(new BatchInfo());

			return this.tempMaterialPool[index];
		}
		
		/// <summary>
		/// Adds a parameterized set of vertices to the drawing devices rendering schedule.
		/// </summary>
		/// <typeparam name="T">The type of vertex data to add.</typeparam>
		/// <param name="material">The <see cref="Duality.Drawing.BatchInfo"/> to use for rendering the vertices.</param>
		/// <param name="vertexMode">The vertices drawing mode.</param>
		/// <param name="vertexBuffer">
		/// A vertex data buffer that stores the vertices to add. Ownership of the buffer
		/// remains at the callsite, while the <see cref="IDrawDevice"/> copies the required
		/// data into internal storage.
		/// </param>
		/// <param name="vertexCount">The number of vertices to add, from the beginning of the buffer.</param>
		public void AddVertices<T>(BatchInfo material, VertexMode vertexMode, T[] vertexBuffer, int vertexCount) where T : struct, IVertexData
		{
			if (vertexCount == 0) return;
			if (vertexBuffer == null || vertexBuffer.Length == 0) return;
			if (vertexCount > vertexBuffer.Length) vertexCount = vertexBuffer.Length;
			if (material == null) material = Material.Checkerboard.Res.Info;

			// Move the added vertices to an internal shared buffer
			VertexSlice<T> slice = this.drawVertices.Rent<T>(vertexCount);
			Array.Copy(vertexBuffer, 0, slice.Data, slice.Offset, slice.Length);

			// In picking mode, override incoming vertices material and vertex colors
			// to generate a lookup texture by which we can retrieve each pixels object.
			if (this.pickingIndex != 0)
			{
				ColorRgba clr = new ColorRgba((this.pickingIndex << 8) | 0xFF);
				for (int i = 0; i < vertexCount; ++i)
					slice.Data[slice.Offset + i].Color = clr;

				material = this.RentMaterial(material);
				material.Technique = DrawTechnique.Picking;
				material.MainColor = ColorRgba.White;
			}
			else if (material.Technique == null || !material.Technique.IsAvailable)
			{
				material = this.RentMaterial(material);
				material.Technique = DrawTechnique.Solid;
			}

			// Aggregate all info we have about our incoming vertices
			VertexDrawItem drawItem = new VertexDrawItem
			{
				Offset = (ushort)slice.Offset,
				Count = (ushort)slice.Length,
				BufferIndex = (byte)(this.drawVertices.GetBatchCount<T>() - 1),
				TypeIndex = (byte)VertexDeclaration.Get<T>().TypeIndex,
				Mode = vertexMode,
				Material = material
			};
			
			// Determine whether we need depth sorting and calculate a reference depth
			bool sortByDepth = (this.projection == ProjectionMode.Screen) || material.Technique.Res.NeedsZSort;
			RawList<SortItem> sortBuffer = sortByDepth ? this.sortBufferBlended : this.sortBufferSolid;
			SortItem sortItem = new SortItem();
			if (sortByDepth)
			{
				sortItem.SortDepth = this.CalcZSortIndex<T>(slice.Data, slice.Offset, slice.Length);
			}

			// Determine whether we can batch the new vertex item with the previous one
			int prevDrawIndex = -1;
			if (vertexMode.IsBatchableMode() && sortBuffer.Count > 0)
			{
				// Since we require a complete material match to append items, we can
				// assume that the previous items sortByDepth value is the same as ours.
				SortItem prevSortItem = sortBuffer.Data[sortBuffer.Count - 1];

				// Compare the previously added item with the new one. If everything
				// except the vertices themselves is the same, we can append them directly.
				if (this.drawBuffer.Data[prevSortItem.DrawItemIndex].CanAppend(ref drawItem) && 
					(!sortByDepth || sortItem.CanShareDepth(prevSortItem.SortDepth)))
				{
					prevDrawIndex = prevSortItem.DrawItemIndex;
				}
			}

			// Append the new item directly to the previous one, or add it as a new one
			if (prevDrawIndex != -1)
			{
				if (sortByDepth)
				{
					sortBuffer.Data[sortBuffer.Count - 1].MergeDepth(
						sortItem.SortDepth, 
						this.drawBuffer.Data[prevDrawIndex].Count, 
						drawItem.Count);
				}
				this.drawBuffer.Data[prevDrawIndex].Count += drawItem.Count;
			}
			else
			{
				sortItem.DrawItemIndex = this.drawBuffer.Count;
				sortBuffer.Add(sortItem);
				this.drawBuffer.Add(drawItem);
			}

			++this.numRawBatches;
		}
		/// <summary>
		/// Adds an already prepared batch to the drawing devices rendering schedule.
		/// </summary>
		/// <param name="batch"></param>
		public void AddBatch(DrawBatch batch)
		{
			bool sortByDepth = (this.projection == ProjectionMode.Screen) || batch.Material.Technique.Res.NeedsZSort;
			RawList<DrawBatch> batchBuffer = sortByDepth ? this.batchBufferBlended : this.batchBufferSolid;
			batchBuffer.Add(batch);
			++this.numRawBatches;
		}
		/// <summary>
		/// Generates a single drawcall that renders a fullscreen quad using the specified material.
		/// Assumes that the <see cref="DrawDevice"/> is set up to render in screen space.
		/// </summary>
		/// <param name="material"></param>
		/// <param name="resizeMode"></param>
		public void AddFullscreenQuad(BatchInfo material, TargetResize resizeMode)
		{
			Texture tex = material.MainTexture.Res;
			Vector2 uvRatio = tex != null ? tex.UVRatio : Vector2.One;
			Point2 inputSize = tex != null ? tex.ContentSize : Point2.Zero;

			// Fit the input material rect to the output size according to rendering step config
			Vector2 targetSize = resizeMode.Apply(inputSize, this.TargetSize);
			Rect targetRect = Rect.Align(
				Alignment.Center, 
				this.TargetSize.X * 0.5f, 
				this.TargetSize.Y * 0.5f, 
				targetSize.X, 
				targetSize.Y);

			// Fit the target rect to actual pixel coordinates to avoid unnecessary filtering offsets
			targetRect.X = (int)targetRect.X;
			targetRect.Y = (int)targetRect.Y;
			targetRect.W = MathF.Ceiling(targetRect.W);
			targetRect.H = MathF.Ceiling(targetRect.H);

			VertexC1P3T2[] vertices = new VertexC1P3T2[4];

			vertices[0].Pos = new Vector3(targetRect.LeftX, targetRect.TopY, 0.0f);
			vertices[1].Pos = new Vector3(targetRect.RightX, targetRect.TopY, 0.0f);
			vertices[2].Pos = new Vector3(targetRect.RightX, targetRect.BottomY, 0.0f);
			vertices[3].Pos = new Vector3(targetRect.LeftX, targetRect.BottomY, 0.0f);

			vertices[0].TexCoord = new Vector2(0.0f, 0.0f);
			vertices[1].TexCoord = new Vector2(uvRatio.X, 0.0f);
			vertices[2].TexCoord = new Vector2(uvRatio.X, uvRatio.Y);
			vertices[3].TexCoord = new Vector2(0.0f, uvRatio.Y);

			vertices[0].Color = ColorRgba.White;
			vertices[1].Color = ColorRgba.White;
			vertices[2].Color = ColorRgba.White;
			vertices[3].Color = ColorRgba.White;

			this.AddVertices(material, VertexMode.Quads, vertices);
		}

		public void PrepareForDrawcalls()
		{
			// Recalculate matrices according to current mode
			this.UpdateMatrices();
		}
		public void Render()
		{
			if (DualityApp.GraphicsBackend == null) return;

			// Prepare forwarding the collected data and parameters to the graphics backend
			this.UploadVertexData();
			this.AggregateBatches();
			this.UpdateBuiltinShaderParameters();

			bool overlayMode = (this.projection == ProjectionMode.Screen);
			this.renderOptions.ClearFlags = this.clearFlags;
			this.renderOptions.ClearColor = this.clearColor;
			this.renderOptions.ClearDepth = this.clearDepth;
			this.renderOptions.Viewport = this.viewportRect;
			this.renderOptions.ViewMatrix = this.matView;
			this.renderOptions.ProjectionMatrix = this.matProjection;
			this.renderOptions.DepthTest = !overlayMode;
			this.renderOptions.DepthWrite = !overlayMode;
			this.renderOptions.Target = this.renderTarget.IsAvailable ? this.renderTarget.Res.Native : null;
			this.renderOptions.ShaderParameters = this.shaderParameters;

			this.renderStats.Reset();

			// Invoke graphics backend functionality to do the rendering
			DualityApp.GraphicsBackend.BeginRendering(this, this.renderOptions, this.renderStats);
			{
				Profile.TimeProcessDrawcalls.BeginMeasure();

				// Sorted as needed by batch optimizer
				DualityApp.GraphicsBackend.Render(this.batchBufferSolid);
				// Z-Sorted, back to Front
				DualityApp.GraphicsBackend.Render(this.batchBufferBlended);

				Profile.TimeProcessDrawcalls.EndMeasure();
			}
			DualityApp.GraphicsBackend.EndRendering();
			Profile.StatNumDrawcalls.Add(this.renderStats.DrawCalls);

			// Reset all temp materials and return them to the pool
			for (int i = 0; i < this.tempMaterialIndex; i++)
			{
				this.tempMaterialPool[i].Technique = DrawTechnique.Mask;
				this.tempMaterialPool[i].Reset();
			}
			this.tempMaterialIndex = 0;

			// Clear all working buffers for vertex and drawcall processing
			this.drawBuffer.Clear();
			this.sortBufferSolid.Clear();
			this.sortBufferBlended.Clear();
			this.sortBufferTemp.Clear();
			this.batchBufferSolid.Clear();
			this.batchBufferBlended.Clear();
			this.drawVertices.Clear();
			this.batchIndexPool.Reset();
		}


		private float CalcZSortIndex<T>(T[] vertices, int offset, int length) where T : struct, IVertexData
		{
			// Require double precision, so we don't get "z fighting" issues in our sort.
			double zSortIndex = 0.0d;
			for (int i = offset; i < offset + length; i++)
			{
				zSortIndex += vertices[i].Pos.Z + vertices[i].DepthOffset;
			}
			return (float)(zSortIndex / (double)length);
		}
		
		private void UpdateMatrices()
		{
			this.UpdateViewMatrix();
			this.UpdateProjectionMatrix();
			this.matFinal = this.matView * this.matProjection;
			Matrix4.Invert(ref this.matFinal, out this.matFinalInv);
		}
		private void UpdateViewMatrix()
		{
			this.matView = Matrix4.Identity;
			if (this.projection != ProjectionMode.Screen)
			{
				var matrix = Matrix4.CreateFromYawPitchRoll(this.viewerAngle.Y, this.viewerAngle.X, this.viewerAngle.Z);
				var target = (this.viewerPos) + Vector3.Transform(Vector3.Forward, matrix);

				this.matView = Matrix4.CreateLookAt(this.viewerPos, target, Vector3.Up);
			}
		}
		private void UpdateProjectionMatrix()
		{
			Rect targetRect = new Rect(this.targetSize);

			if (this.projection == ProjectionMode.Screen)
			{
				// When rendering in screen space, all reasonable positive depth should be valid,
				// so we'll ignore any of the projection specific near and far plane settings.
				// These values give us a linear depth precision of ~0.006 at 24 bit
				Matrix4.CreateOrthographicOffCenter( targetRect.X, targetRect.X + targetRect.W, targetRect.Y + targetRect.H, targetRect.Y, 0.0f, 100000.0f, out this.matProjection);

				// Flip Z direction from "out of the screen" to "into the screen".
				this.matProjection = Matrix4.CreateScale(1.0f, 1.0f, -1.0f) * this.matProjection;
			}
			else if (this.projection == ProjectionMode.Orthographic)
			{
				float clampedNear = MathF.Max(this.nearZ, 0.01f);
				Matrix4.CreateOrthographic(targetRect.W, targetRect.H, clampedNear, this.farZ, out this.matProjection);
			}
			else
			{
				float clampedNear = MathF.Max(this.nearZ, 0.01f);
				this.matProjection = Matrix4.CreatePerspectiveFieldOfView(MathF.DegToRad(70), targetRect.W / targetRect.H, clampedNear, this.farZ);
			}
		}

		/// <summary>
		/// Updates all <see cref="BuiltinShaderFields"/> in the devices shared <see cref="ShaderParameters"/>
		/// to match its current configuration.
		/// </summary>
		private void UpdateBuiltinShaderParameters()
		{
			this.shaderParameters.Set(BuiltinShaderFields.RealTime, (float)Time.MainTimer.TotalSeconds);
			this.shaderParameters.Set(BuiltinShaderFields.GameTime, (float)Time.GameTimer.TotalSeconds);
			this.shaderParameters.Set(BuiltinShaderFields.DeltaTime, Time.DeltaTime);
			this.shaderParameters.Set(BuiltinShaderFields.FrameCount, Time.FrameCount);

			this.shaderParameters.Set(BuiltinShaderFields.CameraPosition, this.viewerPos);
			this.shaderParameters.Set(BuiltinShaderFields.CameraIsPerspective, this.projection == ProjectionMode.Perspective);
			this.shaderParameters.Set(BuiltinShaderFields.CameraFocusDist, this.focusDist);
		}
		/// <summary>
		/// Uploads all dynamically gathered vertex data to the GPU using the internal <see cref="vertexBuffers"/> pool.
		/// </summary>
		private void UploadVertexData()
		{
			// Note that there is a 1:1 mapping between gathered vertex batches and vertex buffers.
			// We'll keep all buffers around until the drawdevice is disposed, in case we might need
			// them again later.
			this.vertexBuffers.Count = Math.Max(this.vertexBuffers.Count, this.drawVertices.TypeIndexCount);
			for (int typeIndex = 0; typeIndex < this.drawVertices.TypeIndexCount; typeIndex++)
			{
				// Filter out unused vertex types
				IReadOnlyList<IVertexBatch> batches = this.drawVertices.GetBatches(typeIndex);
				if (batches == null) continue;
				if (batches.Count == 0) continue;

				// Upload all vertex batches for this vertex type
				if (this.vertexBuffers[typeIndex] == null)
					this.vertexBuffers[typeIndex] = new RawList<VertexBuffer>();
				this.vertexBuffers[typeIndex].Count = Math.Max(this.vertexBuffers[typeIndex].Count, batches.Count);
				for (int batchIndex = 0; batchIndex < batches.Count; batchIndex++)
				{
					IVertexBatch vertexBatch = batches[batchIndex];

					// Generate a VertexBuffer for this vertex type and batch index, if it didn't exist yet
					if (this.vertexBuffers[typeIndex][batchIndex] == null)
						this.vertexBuffers[typeIndex][batchIndex] = new VertexBuffer();

					// Upload the vertex batch to 
					using (PinnedArrayHandle pinned = vertexBatch.Lock())
					{
						this.vertexBuffers[typeIndex][batchIndex].LoadVertexData(
							vertexBatch.Declaration, 
							pinned.Address, 
							vertexBatch.Count);
					}
				}
			}
		}

		private static int MaterialSortComparison(SortItem first, SortItem second)
		{
			return first.SortIndex - second.SortIndex;
		}
		private static int DepthSortComparison(SortItem first, SortItem second)
		{
			if (second.SortDepth < first.SortDepth) return -1;
			if (second.SortDepth > first.SortDepth) return 1;
			if (second.SortDepth == first.SortDepth) return 0;
			if (float.IsNaN(second.SortDepth))
				return (float.IsNaN(first.SortDepth) ? 0 : -1);
			else
				return 1;
		}

		private void AggregateBatches()
		{
			int batchCountBefore = this.sortBufferSolid.Count + this.sortBufferBlended.Count;
			if (this.pickingIndex == 0) Profile.TimeOptimizeDrawcalls.BeginMeasure();

			// Material-sorted (solid) batches
			if (this.sortBufferSolid.Count > 0)
			{
				// Assign material and vertex type based sort indices to each item
				this.AssignMaterialSortIDs(this.sortBufferSolid, this.drawBuffer);

				// Stable sort assures maintaining draw order for batches of equal material
				this.sortBufferTemp.Count = this.sortBufferSolid.Count;
				this.sortBufferSolid.StableSort(this.sortBufferTemp, MaterialSortComparison);
				this.sortBufferTemp.Clear();

				// Sweep over the sorted draws and aggregate as many as possible into a single batch
				this.AggregateBatches(this.sortBufferSolid, this.drawBuffer, this.batchBufferSolid);
			}

			// Depth-sorted (blended) batches
			if (this.sortBufferBlended.Count > 0)
			{
				// Stable sort assures maintaining draw order for batches of equal depth
				this.sortBufferTemp.Count = this.sortBufferBlended.Count;
				this.sortBufferBlended.StableSort(this.sortBufferTemp, DepthSortComparison);
				this.sortBufferTemp.Clear();

				// Sweep over the sorted draws and aggregate as many as possible into a single batch
				this.AggregateBatches(this.sortBufferBlended, this.drawBuffer, this.batchBufferBlended);
			}

			if (this.pickingIndex == 0) Profile.TimeOptimizeDrawcalls.EndMeasure();
			int batchCountAfter = this.batchBufferSolid.Count + this.batchBufferBlended.Count;

			Profile.StatNumRawBatches.Add(this.numRawBatches);
			Profile.StatNumMergedBatches.Add(batchCountBefore);
			Profile.StatNumOptimizedBatches.Add(batchCountAfter);
			this.numRawBatches = 0;
		}
		private void AssignMaterialSortIDs(RawList<SortItem> sortItems, RawList<VertexDrawItem> drawItems)
		{
			VertexDrawItem[] drawData = drawItems.Data;
			SortItem[] sortData = sortItems.Data;
			int count = sortItems.Count;
			for (int i = 0; i < sortData.Length; i++)
			{
				if (i >= count) break;
				
				int drawIndex = sortData[i].DrawItemIndex;
				int vertexTypeIndex = drawData[drawIndex].TypeIndex;
				VertexMode vertexMode = drawData[drawIndex].Mode;
				BatchInfo material = drawData[drawIndex].Material;

				int matHash;
				unchecked
				{
					// Avoid just "cutting off" parts of the original hash,
					// as this is likely to lead to collisions.
					matHash = material.GetHashCode();
					matHash = (13 * matHash + 17 * (matHash >> 9)) % (1 << 23);
				}

				// Bit significance is used to achieve sorting by multiple traits at once.
				// The higher a traits bit significance, the higher its priority when sorting.
				sortData[i].SortIndex = 
					(((int)vertexMode & 15) << 0) | //                           XXXX  4 Bit   Vertex Mode  Offset 4
					((matHash & 8388607) << 4) |    //    XXXXXXXXXXXXXXXXXXXXXXXaaaa  23 Bit  Material     Offset 27
					((vertexTypeIndex & 15) << 27); // XXXbbbbbbbbbbbbbbbbbbbbbbbaaaa  4 Bit   Vertex Type  Offset 31
			}
		}
		private void AggregateBatches(RawList<SortItem> sortItems, RawList<VertexDrawItem> drawItems, RawList<DrawBatch> batches)
		{
			VertexDrawItem[] drawData = drawItems.Data;
			SortItem[] sortData = sortItems.Data;

			SortItem activeSortItem = sortData[0];
			VertexDrawItem activeItem = drawData[activeSortItem.DrawItemIndex];
			int beginBatchIndex = 0;

			// Find sequences of draw items that can be batched together
			int count = sortItems.Count;
			for (int sortIndex = 1; sortIndex <= count; sortIndex++)
			{
				// Skip items until we can no longer put the next one into the same batch
				if (sortIndex < count)
				{
					SortItem sortItem = sortData[sortIndex];
					if (activeItem.CanShareBatchWith(ref drawData[sortItem.DrawItemIndex]))
						continue;
				}

				// Create a batch for all previous items
				VertexBuffer vertexBuffer = this.vertexBuffers[activeItem.TypeIndex][activeItem.BufferIndex];
				DrawBatch batch = new DrawBatch(
					vertexBuffer, 
					this.batchIndexPool.Rent(sortIndex - beginBatchIndex), 
					activeItem.Mode,
					activeItem.Material);

				for (int i = beginBatchIndex; i < sortIndex; i++)
				{
					batch.VertexRanges.Add(new VertexDrawRange
					{
						Index = drawData[sortData[i].DrawItemIndex].Offset,
						Count = drawData[sortData[i].DrawItemIndex].Count
					});
				}

				batches.Add(batch);

				// Proceed with the current item being the new sharing reference
				if (sortIndex < count)
				{
					beginBatchIndex = sortIndex;
					activeSortItem = sortData[sortIndex];
					activeItem = drawData[activeSortItem.DrawItemIndex];
				}
			}
		}

		public static void RenderVoid(Rect viewportRect)
		{
			RenderVoid(viewportRect, ColorRgba.TransparentBlack);
		}
		public static void RenderVoid(Rect viewportRect, ColorRgba color)
		{
			if (DualityApp.GraphicsBackend == null) return;

			RenderOptions options = new RenderOptions
			{
				ClearFlags = ClearFlag.All,
				ClearColor = color,
				ClearDepth = 1.0f,
				Viewport = viewportRect
			};
			DualityApp.GraphicsBackend.BeginRendering(null, options);
			DualityApp.GraphicsBackend.EndRendering();
		}
	}
}
