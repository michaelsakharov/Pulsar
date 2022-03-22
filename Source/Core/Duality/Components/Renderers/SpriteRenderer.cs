using System;

using Duality.Resources;
using Duality.Editor;
using Duality.Properties;
using Duality.Cloning;
using Duality.Drawing;

namespace Duality.Components.Renderers
{
	/// <summary>
	/// Renders a sprite to represent the <see cref="GameObject"/>.
	/// </summary>
	[ManuallyCloned]
	[EditorHintCategory(CoreResNames.CategoryGraphics)]
	[EditorHintImage(CoreResNames.ImageSpriteRenderer)]
	public class SpriteRenderer : Renderer, ICmpSpriteRenderer
	{
		/// <summary>
		/// Specifies how the sprites uv-Coordinates are calculated.
		/// </summary>
		[Flags]
		public enum UVMode
		{
			/// <summary>
			/// The uv-Coordinates are constant, stretching the supplied texture to fit the SpriteRenderers dimensions.
			/// </summary>
			Stretch        = 0x0,
			/// <summary>
			/// The u-Coordinate is calculated based on the available horizontal space, allowing the supplied texture to be
			/// tiled across the SpriteRenderers width.
			/// </summary>
			WrapHorizontal = 0x1,
			/// <summary>
			/// The v-Coordinate is calculated based on the available vertical space, allowing the supplied texture to be
			/// tiled across the SpriteRenderers height.
			/// </summary>
			WrapVertical   = 0x2,
			/// <summary>
			/// The uv-Coordinates are calculated based on the available space, allowing the supplied texture to be
			/// tiled across the SpriteRenderers size.
			/// </summary>
			WrapBoth       = WrapHorizontal | WrapVertical
		}
		/// <summary>
		/// Specifies whether the sprite should be flipped on a given axis.
		/// </summary>
		[Flags]
		public enum FlipMode
		{
			/// <summary>
			/// The sprite will not be flipped at all.
			/// </summary>
			None       = 0x0,
			/// <summary>
			/// The sprite will be flipped on its horizontal axis.
			/// </summary>
			Horizontal = 0x1,
			/// <summary>
			/// The sprite will be flipped on its vertical axis.
			/// </summary>
			Vertical   = 0x2
		}


		protected Rect                 rect        = Rect.Align(Alignment.Center, 0, 0, 256, 256);
		protected ContentRef<Material> sharedMat   = Material.DualityIcon;
		protected BatchInfo            customMat   = null;
		protected ColorRgba            colorTint   = ColorRgba.White;
		protected UVMode               rectMode    = UVMode.Stretch;
		protected float                offset      = 0.0f;
		protected FlipMode             flipMode    = FlipMode.None;
		protected int                  spriteIndex = -1;
		[DontSerialize] protected VertexC1P3T2[] vertices = null;


		[EditorHintFlags(MemberFlags.Invisible)]
		public override float BoundRadius
		{
			get { return this.rect.BoundingRadius * this.gameobj.Transform.Scale.Length; }
		}
		/// <summary>
		/// [GET / SET] The rectangular area the sprite occupies. Relative to the <see cref="GameObject"/>.
		/// </summary>
		[EditorHintDecimalPlaces(1)]
		public Rect Rect
		{
			get { return this.rect; }
			set { this.rect = value; }
		}
		/// <summary>
		/// [GET / SET] The <see cref="Duality.Resources.Material"/> that is used for rendering the sprite.
		/// </summary>
		public ContentRef<Material> SharedMaterial
		{
			get { return this.sharedMat; }
			set { this.sharedMat = value; }
		}
		/// <summary>
		/// [GET / SET] A custom, local <see cref="Duality.Drawing.BatchInfo"/> overriding the <see cref="SharedMaterial"/>,
		/// allowing this sprite to look unique without having to create its own <see cref="Duality.Resources.Material"/> Resource.
		/// However, this feature should be used with caution: Performance is better using <see cref="SharedMaterial">shared Materials</see>.
		/// </summary>
		public BatchInfo CustomMaterial
		{
			get { return this.customMat; }
			set { this.customMat = value; }
		}
		/// <summary>
		/// [GET / SET] A color by which the sprite is tinted.
		/// </summary>
		public ColorRgba ColorTint
		{
			get { return this.colorTint; }
			set { this.colorTint = value; }
		}
		/// <summary>
		/// [GET / SET] Specifies how the sprites uv-Coordinates are calculated.
		/// </summary>
		public UVMode RectMode
		{
			get { return this.rectMode; }
			set { this.rectMode = value; }
		}
		/// <summary>
		/// [GET / SET] A depth / Z offset that affects the order in which objects are drawn. If you want to assure an object is drawn after another one,
		/// just assign a higher Offset value to the background object.
		/// </summary>
		public float DepthOffset
		{
			get { return this.offset; }
			set { this.offset = value; }
		}
		/// <summary>
		/// [GET / SET] Specifies whether the sprite should be flipped on a given axis when redered.
		/// </summary>
		public FlipMode Flip
		{
			get { return this.flipMode; }
			set { this.flipMode = value; }
		}
		/// <summary>
		/// [GET / SET] The sprite index that is displayed by this renderer.
		/// </summary>
		public int SpriteIndex
		{
			get { return this.spriteIndex; }
			set { this.ApplySpriteAnimation(value, value, 0.0f); }
		}

		
		protected Texture RetrieveMainTex()
		{
			if (this.customMat != null)
				return this.customMat.MainTexture.Res;
			else if (this.sharedMat.IsAvailable)
				return this.sharedMat.Res.MainTexture.Res;
			else
				return null;
		}
		protected DrawTechnique RetrieveDrawTechnique()
		{
			if (this.customMat != null)
				return this.customMat.Technique.Res;
			else if (this.sharedMat.IsAvailable)
				return this.sharedMat.Res.Technique.Res;
			else
				return null;
		}
		protected void PrepareVertices(ref VertexC1P3T2[] vertices, IDrawDevice device, ColorRgba mainClr, Rect uvRect)
		{
			Vector3 posTemp = this.gameobj.Transform.Pos;

			Vector3 edge1 = new Vector3(this.rect.TopLeft.X, this.rect.TopLeft.Y, 0);
			Vector3 edge2 = new Vector3(this.rect.BottomLeft.X, this.rect.BottomLeft.Y, 0);
			Vector3 edge3 = new Vector3(this.rect.BottomRight.X, this.rect.BottomRight.Y, 0);
			Vector3 edge4 = new Vector3(this.rect.TopRight.X, this.rect.TopRight.Y, 0);

			if ((this.flipMode & FlipMode.Horizontal) != FlipMode.None)
			{ 
				edge1.X = -edge1.X;
				edge2.X = -edge2.X;
				edge3.X = -edge3.X;
				edge4.X = -edge4.X;
			}
			if ((this.flipMode & FlipMode.Vertical) != FlipMode.None)
			{
				edge1.Y = -edge1.Y;
				edge2.Y = -edge2.Y;
				edge3.Y = -edge3.Y;
				edge4.Y = -edge4.Y;
			}

			edge1 = this.GameObj.Transform.GetWorldPoint(edge1);
			edge2 = this.GameObj.Transform.GetWorldPoint(edge2);
			edge3 = this.GameObj.Transform.GetWorldPoint(edge3);
			edge4 = this.GameObj.Transform.GetWorldPoint(edge4);
            
			float left   = uvRect.X;
			float right  = uvRect.RightX;
			float top    = uvRect.Y;
			float bottom = uvRect.BottomY;

			if (vertices == null || vertices.Length != 4) vertices = new VertexC1P3T2[4];

			vertices[0].Pos.X = posTemp.X + edge1.X;
			vertices[0].Pos.Y = posTemp.Y + edge1.Y;
			vertices[0].Pos.Z = posTemp.Z + edge1.Z;
			vertices[0].DepthOffset = this.offset;
			vertices[0].TexCoord.X = left;
			vertices[0].TexCoord.Y = top;
			vertices[0].Color = mainClr;

			vertices[1].Pos.X = posTemp.X + edge2.X;
			vertices[1].Pos.Y = posTemp.Y + edge2.Y;
			vertices[1].Pos.Z = posTemp.Z + edge2.Z;
			vertices[1].DepthOffset = this.offset;
			vertices[1].TexCoord.X = left;
			vertices[1].TexCoord.Y = bottom;
			vertices[1].Color = mainClr;

			vertices[2].Pos.X = posTemp.X + edge3.X;
			vertices[2].Pos.Y = posTemp.Y + edge3.Y;
			vertices[2].Pos.Z = posTemp.Z + edge3.Z;
			vertices[2].DepthOffset = this.offset;
			vertices[2].TexCoord.X = right;
			vertices[2].TexCoord.Y = bottom;
			vertices[2].Color = mainClr;
				
			vertices[3].Pos.X = posTemp.X + edge4.X;
			vertices[3].Pos.Y = posTemp.Y + edge4.Y;
			vertices[3].Pos.Z = posTemp.Z + edge4.Z;
			vertices[3].DepthOffset = this.offset;
			vertices[3].TexCoord.X = right;
			vertices[3].TexCoord.Y = top;
			vertices[3].Color = mainClr;
		}
		protected void GetUVRect(Texture mainTex, int spriteIndex, out Rect uvRect)
		{
			// Determine the rect area of the texture to be displayed
			if (mainTex == null)
				uvRect = new Rect(1.0f, 1.0f);
			else if (spriteIndex != -1)
				mainTex.LookupAtlas(spriteIndex, out uvRect);
			else
				uvRect = new Rect(mainTex.UVRatio);

			// Determine wrap-around and stretch behavior if the displayed rect size does
			// not equal the rect size that would be required for a 1:1 display.
			if (mainTex != null)
			{
				Vector2 fullSize = mainTex.ContentSize * (uvRect.Size / mainTex.UVRatio);
				if ((this.rectMode & UVMode.WrapHorizontal) != 0)
					uvRect.W *= this.rect.W / fullSize.X;
				if ((this.rectMode & UVMode.WrapVertical) != 0)
					uvRect.H *= this.rect.H / fullSize.Y;
			}
		}

		/// <inheritdoc/>
		public virtual void ApplySpriteAnimation(int currentSpriteIndex, int nextSpriteIndex, float progressToNext)
		{
			this.spriteIndex = currentSpriteIndex;
		}
		/// <inheritdoc/>
		public override void Draw(IDrawDevice device)
		{
			Texture mainTex = this.RetrieveMainTex();

			Rect uvRect;
			this.GetUVRect(mainTex, this.spriteIndex, out uvRect);
			this.PrepareVertices(ref this.vertices, device, this.colorTint, uvRect);
			if (this.customMat != null)
				device.AddVertices(this.customMat, VertexMode.Quads, this.vertices);
			else
				device.AddVertices(this.sharedMat, VertexMode.Quads, this.vertices);
		}

		protected override void OnSetupCloneTargets(object targetObj, ICloneTargetSetup setup)
		{
			base.OnSetupCloneTargets(targetObj, setup);
			SpriteRenderer target = targetObj as SpriteRenderer;

			setup.HandleObject(this.customMat, target.customMat);
		}
		protected override void OnCopyDataTo(object targetObj, ICloneOperation operation)
		{
			base.OnCopyDataTo(targetObj, operation);
			SpriteRenderer target = targetObj as SpriteRenderer;
			
			target.rect      = this.rect;
			target.colorTint = this.colorTint;
			target.rectMode  = this.rectMode;
			target.offset    = this.offset;
			target.flipMode  = this.flipMode;
			target.spriteIndex = this.spriteIndex;

			operation.HandleValue(ref this.sharedMat, ref target.sharedMat);
			operation.HandleObject(this.customMat, ref target.customMat);
		}
	}
}
