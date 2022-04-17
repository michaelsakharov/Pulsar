using System;
using System.Collections.Generic;

using Duality.Editor;
using Duality.Properties;
using Duality.Drawing;
using Duality.Cloning;
using Duality.Backend;
using Duality.IO;
using THREE.Textures;

namespace Duality.Resources
{
	/// <summary>
	/// A Texture refers to pixel data stored in video memory
	/// </summary>
	/// <seealso cref="Duality.Resources.Pixmap"/>
	[ExplicitResourceReference(typeof(Pixmap))]
	[EditorHintCategory(CoreResNames.CategoryGraphics)]
	[EditorHintImage(CoreResNames.ImageTexture)]
	public class Texture : Resource
	{
		/// <summary>
		/// [GET] A Texture showing the Duality icon.
		/// </summary>
		public static ContentRef<Texture> DualityIcon { get; private set; }
		/// <summary>
		/// [GET] A Texture showing the Duality icon without the text on it.
		/// </summary>
		public static ContentRef<Texture> DualityIconB { get; private set; }
		/// <summary>
		/// A Texture showing the Duality logo.
		/// </summary>
		public static ContentRef<Texture> DualityLogoBig { get; private set; }
		/// <summary>
		/// A Texture showing the Duality logo.
		/// </summary>
		public static ContentRef<Texture> DualityLogoMedium { get; private set; }
		/// <summary>
		/// A Texture showing the Duality logo.
		/// </summary>
		public static ContentRef<Texture> DualityLogoSmall { get; private set; }
		/// <summary>
		/// [GET] A plain white 1x1 Texture. Can be used as a dummy.
		/// </summary>
		public static ContentRef<Texture> White { get; private set; }
		/// <summary>
		/// [GET] A 256x256 black and white checkerboard texture.
		/// </summary>
		public static ContentRef<Texture> Checkerboard { get; private set; }
		/// <summary>
		/// [GET] Specular Integartion
		/// </summary>
		public static ContentRef<Texture> SpecularIntegartion { get; private set; }
		public static ContentRef<Texture> ColorCorrectLUT { get; private set; }
		public static ContentRef<Texture> DefaultNormalMap { get; private set; }

		internal static void InitDefaultContent()
		{
			DefaultContent.InitType<Texture>(new Dictionary<string, Texture>
			{
				{ "DualityIcon", new Texture(Pixmap.DualityIcon) },
				{ "DualityIconB", new Texture(Pixmap.DualityIconB) },
				{ "DualityLogoBig", new Texture(Pixmap.DualityLogoBig) },
				{ "DualityLogoMedium", new Texture(Pixmap.DualityLogoMedium) },
				{ "DualityLogoSmall", new Texture(Pixmap.DualityLogoSmall) },
				{ "White", new Texture(Pixmap.White) },
				{ "DefaultNormalMap", new Texture(
					Pixmap.DefaultNormalMap,
					TextureSizeMode.Default,
					MagnificationFilters.NearestFilter,
					MinificationFilters.NearestFilter,
					WrappingModes.RepeatWrapping,
					WrappingModes.RepeatWrapping,
					WrappingModes.RepeatWrapping) },
				{ "Checkerboard", new Texture(
					Pixmap.Checkerboard,
					TextureSizeMode.Default,
					MagnificationFilters.NearestFilter,
					MinificationFilters.NearestFilter,
					WrappingModes.RepeatWrapping,
					WrappingModes.RepeatWrapping,
					WrappingModes.RepeatWrapping) },
				{ "SpecularIntegartion", new Texture(
					Pixmap.SpecularIntegartion,
					TextureSizeMode.Default,
					MagnificationFilters.NearestFilter,
					MinificationFilters.NearestFilter,
					WrappingModes.RepeatWrapping,
					WrappingModes.RepeatWrapping,
					WrappingModes.RepeatWrapping) },
				{ "ColorCorrectLUT", new Texture(
					Pixmap.ColorCorrectLUT,
					TextureSizeMode.Default,
					MagnificationFilters.NearestFilter,
					MinificationFilters.NearestFilter,
					WrappingModes.RepeatWrapping,
					WrappingModes.RepeatWrapping,
					WrappingModes.RepeatWrapping) },
			});
		}


		private ContentRef<Pixmap> basePixmap = null;
		private Point2 size = Point2.Zero;
		private TextureSizeMode texSizeMode = TextureSizeMode.Default;

		[DontSerialize] private THREE.Textures.Texture threeTex = null;
		[DontSerialize] private int pxWidth = 0;
		[DontSerialize] private int pxHeight = 0;
		[DontSerialize] private int texWidth = 0;
		[DontSerialize] private int texHeight = 0;
		[DontSerialize] private Vector2 uvRatio = new Vector2(1.0f, 1.0f);
		[DontSerialize] private bool needsReload = true;
		[DontSerialize] private Rect[] atlas = null;

		private int _Anisotropy = 1;
		private Vector2 _Center = new Vector2(0, 0);
		private Vector2 _Offset = new Vector2(0, 0);
		private WrappingModes _WrapS = WrappingModes.RepeatWrapping;
		private WrappingModes _WrapT = WrappingModes.RepeatWrapping;
		private WrappingModes _WrapR = WrappingModes.RepeatWrapping;
		private Vector2 _Repeat = new Vector2(1, 1);
		private MagnificationFilters _MagFilter = MagnificationFilters.LinearFilter;
		private MinificationFilters _MinFilter = MinificationFilters.LinearMipmapLinearFilter;
		private MappingModes _Mapping = MappingModes.UVMapping;
		private float _Rotation = 0;
		private bool _FlipY = false;

		[EditorHintFlags(MemberFlags.AffectsOthers), EditorHintRange(1, 2)] public int Anisotropy { get { return this._Anisotropy; } set { this._Anisotropy = value; this.needsReload = true; } }
		[EditorHintFlags(MemberFlags.AffectsOthers)] public Vector2 Center { get { return this._Center; } set { this._Center = value; this.needsReload = true; } }
		[EditorHintFlags(MemberFlags.AffectsOthers)] public Vector2 Offset { get { return this._Offset; } set { this._Offset = value; this.needsReload = true; } }
		[EditorHintFlags(MemberFlags.AffectsOthers)] public WrappingModes WrapS { get { return this._WrapS; } set { this._WrapS = value; this.needsReload = true; } }
		[EditorHintFlags(MemberFlags.AffectsOthers)] public WrappingModes WrapT { get { return this._WrapT; } set { this._WrapT = value; this.needsReload = true; } }
		[EditorHintFlags(MemberFlags.AffectsOthers)] public WrappingModes WrapR { get { return this._WrapR; } set { this._WrapR = value; this.needsReload = true; } }
		[EditorHintFlags(MemberFlags.AffectsOthers)] public Vector2 Repeat { get { return this._Repeat; } set { this._Repeat = value; this.needsReload = true; } }
		[EditorHintFlags(MemberFlags.AffectsOthers)] public MagnificationFilters MagFilter { get { return this._MagFilter; } set { this._MagFilter = value; this.needsReload = true; } }
		[EditorHintFlags(MemberFlags.AffectsOthers)] public MinificationFilters MinFilter { get { return this._MinFilter; } set { this._MinFilter = value; this.needsReload = true; } }
		[EditorHintFlags(MemberFlags.AffectsOthers)] public MappingModes Mapping { get { return this._Mapping; } set { this._Mapping = value; this.needsReload = true; } }
		[EditorHintFlags(MemberFlags.AffectsOthers)] public float Rotation { get { return this._Rotation; } set { this._Rotation = value; this.needsReload = true; } }
		[EditorHintFlags(MemberFlags.AffectsOthers)] public bool FlipY { get { return this._FlipY; } set { this._FlipY = value; this.needsReload = true; } }

		/// <summary>
		/// [GET] The width of the internal texture that has been allocated, in pixels.
		/// </summary>
		[EditorHintFlags(MemberFlags.Invisible)]
		public int InternalWidth
		{
			get { return this.texWidth; }
		}
		/// <summary>
		/// [GET] The height of the internal texture that has been allocated, in pixels.
		/// </summary>
		[EditorHintFlags(MemberFlags.Invisible)]
		public int InternalHeight
		{
			get { return this.texHeight; }
		}
		/// <summary>
		/// [GET] The size of the internal texture that has been allocated, in pixels.
		/// </summary>
		[EditorHintFlags(MemberFlags.Invisible)]
		public Point2 InternalSize
		{
			get { return new Point2(this.texWidth, this.texHeight); }
		}
		/// <summary>
		/// [GET] The width of the texture area that is actually used, in pixels.
		/// </summary>
		[EditorHintFlags(MemberFlags.Invisible)]
		public int ContentWidth
		{
			get { return this.pxWidth; }
		}
		/// <summary>
		/// [GET] The height of the texture area that is actually used, in pixels.
		/// </summary>
		[EditorHintFlags(MemberFlags.Invisible)]
		public int ContentHeight
		{
			get { return this.pxHeight; }
		}
		/// <summary>
		/// [GET] The size of the texture area that is actually used, in pixels.
		/// </summary>
		[EditorHintFlags(MemberFlags.Invisible)]
		public Point2 ContentSize
		{
			get { return new Point2(this.pxWidth, this.pxHeight); }
		}
		/// <summary>
		/// [GET] The backends native texture. You shouldn't use this unless you know exactly what you're doing.
		/// </summary>
		[EditorHintFlags(MemberFlags.Invisible)]
		public THREE.Textures.Texture ThreeTexture
		{
			get { return this.threeTex; }
		}

		public int Width
		{
			get { return this.threeTex != null ? this.threeTex.ImageSize.Width : 0; }
		}
		public int Height
		{
			get { return this.threeTex != null ? this.threeTex.ImageSize.Height : 0; }
		}
		public bool NeedsReload
		{
			get { return this.needsReload; }
		}

		/// <summary>
		/// [GET / SET] The Textures nominal size. When create from a <see cref="BasePixmap"/>, this
		/// value will be read-only and derived from its <see cref="Pixmap.Size"/>.
		/// </summary>
		[EditorHintFlags(MemberFlags.AffectsOthers)]
		[EditorHintRange(1, int.MaxValue)]
		public Point2 Size
		{
			get { return this.size; }
			set
			{
				if (this.basePixmap.IsExplicitNull && this.size != value)
				{
					this.AdjustSize(value.X, value.Y);
					this.needsReload = true;
				}
			}
		}
		/// <summary>
		/// [GET / SET] Reference to a Pixmap that contains the pixel data that is or has been uploaded to the Texture
		/// </summary>
		[EditorHintFlags(MemberFlags.AffectsOthers)]
		public ContentRef<Pixmap> BasePixmap
		{
			get { return this.basePixmap; }
			set { if (this.basePixmap.Res != value.Res) { this.basePixmap = value; this.needsReload = true; } }
		}


		/// <summary>
		/// Sets up a new, uninitialized Texture.
		/// </summary>
		public Texture() : this(0, 0) { }
		/// <summary>
		/// Creates a new Texture based on a <see cref="Duality.Resources.Pixmap"/>.
		/// </summary>
		/// <param name="basePixmap">The <see cref="Duality.Resources.Pixmap"/> to use as source for pixel data.</param>
		/// <param name="sizeMode">Specifies behaviour in case the source data has non-power-of-two dimensions.</param>
		/// <param name="filterMag">The OpenGL filter mode for drawing the Texture bigger than it is.</param>
		/// <param name="filterMin">The OpenGL fitler mode for drawing the Texture smaller than it is.</param>
		/// <param name="wrapX">The OpenGL wrap mode on the texel x axis.</param>
		/// <param name="wrapY">The OpenGL wrap mode on the texel y axis.</param>
		/// <param name="format">The format in which OpenGL stores the pixel data.</param>
		public Texture(ContentRef<Pixmap> basePixmap,
			TextureSizeMode sizeMode = TextureSizeMode.Default,
			MagnificationFilters filterMag = MagnificationFilters.LinearFilter,
			MinificationFilters filterMin = MinificationFilters.LinearMipmapLinearFilter,
			WrappingModes wrapS = WrappingModes.ClampToEdgeWrapping,
			WrappingModes wrapT = WrappingModes.ClampToEdgeWrapping,
			WrappingModes wrapR = WrappingModes.ClampToEdgeWrapping)
		{
			this.MagFilter = filterMag;
			this.MinFilter = filterMin;
			this.WrapS = wrapS;
			this.WrapS = wrapS;
			this.WrapT = wrapT;
			this.WrapR = wrapR;
			this.LoadData(basePixmap, sizeMode);
		}
		/// <summary>
		/// Creates a new empty Texture with the specified size.
		/// </summary>
		/// <param name="width">The Textures width.</param>
		/// <param name="height">The Textures height</param>
		/// <param name="sizeMode">Specifies behaviour in case the specified size has non-power-of-two dimensions.</param>
		/// <param name="filterMag">The OpenGL filter mode for drawing the Texture bigger than it is.</param>
		/// <param name="filterMin">The OpenGL fitler mode for drawing the Texture smaller than it is.</param>
		/// <param name="wrapX">The OpenGL wrap mode on the texel x axis.</param>
		/// <param name="wrapY">The OpenGL wrap mode on the texel y axis.</param>
		/// <param name="format">The format in which OpenGL stores the pixel data.</param>
		public Texture(int width, int height,
			TextureSizeMode sizeMode = TextureSizeMode.Default,
			MagnificationFilters filterMag = MagnificationFilters.LinearFilter,
			MinificationFilters filterMin = MinificationFilters.LinearMipmapLinearFilter,
			WrappingModes wrapS = WrappingModes.ClampToEdgeWrapping,
			WrappingModes wrapT = WrappingModes.ClampToEdgeWrapping,
			WrappingModes wrapR = WrappingModes.ClampToEdgeWrapping)
		{
			this.MagFilter = filterMag;
			this.MinFilter = filterMin;
			this.WrapS = wrapS;
			this.WrapS = wrapS;
			this.WrapT = wrapT;
			this.WrapR = wrapR;
			this.texSizeMode = sizeMode;
			this.AdjustSize(width, height);
		}

		/// <summary>
		/// Reloads this Textures pixel data. If the referred <see cref="Duality.Resources.Pixmap"/> has been modified,
		/// changes will now be visible.
		/// </summary>
		public void ReloadData()
		{
			this.LoadData(this.basePixmap, this.texSizeMode);
		}
		/// <summary>
		/// Loads the specified <see cref="Duality.Resources.Pixmap">Pixmaps</see> pixel data.
		/// </summary>
		/// <param name="basePixmap">The <see cref="Duality.Resources.Pixmap"/> that is used as pixel data source.</param>
		/// <param name="sizeMode">Specifies behaviour in case the source data has non-power-of-two dimensions.</param>
		public void LoadData(ContentRef<Pixmap> basePixmap, TextureSizeMode sizeMode)
		{
			if (threeTex != null) threeTex.Dispose();
			this.needsReload = false;
			this.basePixmap = basePixmap;
			this.texSizeMode = sizeMode;

			if (!this.basePixmap.IsExplicitNull)
			{
				PixelData pixelData = null;
				Pixmap basePixmapRes = this.basePixmap.IsAvailable ? this.basePixmap.Res : null;
				if (basePixmapRes != null)
				{
					pixelData = basePixmapRes.MainLayer;
					bool hasAtlas = (basePixmapRes.Atlas != null && basePixmapRes.Atlas.Count > 0);
					this.atlas = hasAtlas ? basePixmapRes.Atlas.ToArray() : null;
				}

				if (pixelData == null)
					pixelData = Pixmap.Checkerboard.Res.MainLayer;

				this.AdjustSize(pixelData.Width, pixelData.Height);
				if (this.texSizeMode != TextureSizeMode.NonPowerOfTwo &&
					(this.pxWidth != this.texWidth || this.pxHeight != this.texHeight))
				{
					if (this.texSizeMode == TextureSizeMode.Enlarge)
					{
						PixelData oldData = pixelData;
						pixelData = oldData.CloneResize(this.texWidth, this.texHeight);
						// Fill border pixels manually - that's cheaper than ColorTransparentPixels here.
						oldData.DrawOnto(pixelData, BlendMode.Solid, this.pxWidth, 0, 1, this.pxHeight, this.pxWidth - 1, 0);
						oldData.DrawOnto(pixelData, BlendMode.Solid, 0, this.pxHeight, this.pxWidth, 1, 0, this.pxHeight - 1);
					}
					else
						pixelData = pixelData.CloneRescale(this.texWidth, this.texHeight, ImageScaleFilter.Linear);
				}

				// Load pixel data to video memory
				threeTex = new THREE.Textures.Texture(pixelData.ToBitmap(), (int)this.Mapping, (int)this.WrapS, (int)this.WrapT, (int)this.MagFilter, (int)this.MinFilter, anisotropy: this.Anisotropy);
				threeTex.flipY = FlipY;
				threeTex.Rotation = Rotation;
				threeTex.Format = THREE.Constants.RGBFormat;
				threeTex.NeedsUpdate = true;

				// Adjust atlas to represent UV coordinates
				if (this.atlas != null)
				{
					Vector2 scale;
					scale.X = this.uvRatio.X / this.pxWidth;
					scale.Y = this.uvRatio.Y / this.pxHeight;
					for (int i = 0; i < this.atlas.Length; i++)
					{
						this.atlas[i].X *= scale.X;
						this.atlas[i].W *= scale.X;
						this.atlas[i].Y *= scale.Y;
						this.atlas[i].H *= scale.Y;
					}
				}
			}
			else
			{
				this.atlas = null;
				this.AdjustSize(this.size.X, this.size.Y);
			}
		}

		/// <summary>
		/// Retrieves the pixel data that is currently stored in video memory.
		/// </summary>
		public PixelData GetPixelData()
		{
			PixelData result = new PixelData();
			this.GetPixelData(result);
			return result;
		}
		/// <summary>
		/// Retrieves the pixel data that is currently stored in video memory.
		/// </summary>
		/// <param name="target">The target image to store the retrieved pixel data in.</param>
		public void GetPixelData(PixelData target)
		{
			target.FromBitmap(this.threeTex.Image);
		}

		/// <summary>
		/// Does a safe (null-checked, clamped) texture <see cref="Duality.Resources.Pixmap.Atlas"/> lookup.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="uv"></param>
		public void LookupAtlas(int index, out Rect uv)
		{
			if (this.atlas == null)
			{
				uv.X = uv.Y = 0.0f;
				uv.W = this.uvRatio.X;
				uv.H = this.uvRatio.Y;
			}
			else
			{
				uv = this.atlas[MathF.Clamp(index, 0, this.atlas.Length - 1)];
			}
		}

		/// <summary>
		/// Processes the specified size based on the Textures <see cref="TextureSizeMode"/>.
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		protected void AdjustSize(int width, int height)
		{
			this.size = new Point2(MathF.Abs(width), MathF.Abs(height));
			this.pxWidth = this.size.X;
			this.pxHeight = this.size.Y;

			if (this.texSizeMode == TextureSizeMode.NonPowerOfTwo)
			{
				this.texWidth = this.pxWidth;
				this.texHeight = this.pxHeight;
				this.uvRatio = Vector2.One;
			}
			else
			{
				this.texWidth = MathF.NextPowerOfTwo(this.pxWidth);
				this.texHeight = MathF.NextPowerOfTwo(this.pxHeight);
				if (this.pxWidth != this.texWidth || this.pxHeight != this.texHeight)
				{
					if (this.texSizeMode == TextureSizeMode.Enlarge)
					{
						this.uvRatio.X = (float)this.pxWidth / (float)this.texWidth;
						this.uvRatio.Y = (float)this.pxHeight / (float)this.texHeight;
					}
					else
						this.uvRatio = Vector2.One;
				}
				else
					this.uvRatio = Vector2.One;
			}
		}

		protected override void OnLoaded()
		{
			this.LoadData(this.basePixmap, this.texSizeMode);
			base.OnLoaded();
		}
		protected override void OnDisposing(bool manually)
		{
			base.OnDisposing(manually);

			// Dispose unmanaged Resources
			if (this.threeTex != null)
			{
				this.threeTex.Dispose();
				this.threeTex = null;
			}

			// Get rid of big data references, so the GC can collect them.
			this.basePixmap.Detach();
		}
		protected override void OnCopyDataTo(object target, ICloneOperation operation)
		{
			base.OnCopyDataTo(target, operation);
			Texture c = target as Texture;
			c.LoadData(this.basePixmap, this.texSizeMode);
		}
	}

	public enum MappingModes
	{
		UVMapping = 300,
		CubeReflectionMapping = 301,
		CubeRefractionMapping = 302,
		EquirectangularReflectionMapping = 303,
		EquirectangularRefractionMapping = 304,
		CubeUVReflectionMapping = 306,
	}

	public enum WrappingModes
	{
		RepeatWrapping = 1000,
		ClampToEdgeWrapping = 1001,
		MirroredRepeatWrapping = 1002,
	}

	public enum MagnificationFilters
	{
		NearestFilter = 1003,
		LinearFilter = 1006,
	}

	public enum MinificationFilters
	{
		NearestFilter = 1003,
		NearestMipmapNearestFilter = 1004,
		NearestMipmapLinearFilter = 1005,
		LinearFilter = 1006,
		LinearMipmapNearestFilter = 1007,
		LinearMipmapLinearFilter = 1008,
	}

	public enum TextureSizeMode
	{
		/// <summary>
		/// Enlarges the images dimensions without scaling the image, leaving
		/// the new space empty. Texture coordinates are automatically adjusted in
		/// order to display the image correctly. This preserves the images full
		/// quality but prevents tiling, if not power-of-two anyway.
		/// </summary>
		Enlarge,
		/// <summary>
		/// Stretches the image to fit power-of-two dimensions and downscales it
		/// again when displaying. This might blur the image slightly but allows
		/// tiling it.
		/// </summary>
		Stretch,
		/// <summary>
		/// The images dimensions are not affected, as OpenGL uses an actual 
		/// non-power-of-two texture. However, this might be unsupported on older hardware.
		/// </summary>
		NonPowerOfTwo,

		/// <summary>
		/// The default behaviour. Equals <see cref="Enlarge"/>.
		/// </summary>
		Default = Enlarge
	}
}