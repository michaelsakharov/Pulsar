﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using Duality;
using Duality.Components;
using Duality.Components.Renderers;
using Duality.Resources;
using Duality.Drawing;
using Duality.Editor;


namespace Duality.Editor.Plugins.Base.DataConverters
{
	public class ComponentFromMaterial : DataConverter
	{
		public override Type TargetType
		{
			get { return typeof(SpriteRenderer); }
		}

		public override bool CanConvertFrom(ConvertOperation convert)
		{
			return 
				convert.AllowedOperations.HasFlag(ConvertOperation.Operation.CreateObj) && 
				convert.CanPerform<Material>();
		}
		public override bool Convert(ConvertOperation convert)
		{
			// If we already have a renderer in the result set, consider generating
			// another one to be not the right course of action.
			if (convert.Result.OfType<ICmpRenderer>().Any())
				return false;

			List<object> results = new List<object>();
			List<Material> availData = convert.Perform<Material>().ToList();

			// Generate objects
			foreach (Material mat in availData)
			{
				if (convert.IsObjectHandled(mat)) continue;
				Texture mainTex = mat.MainTexture.Res;
				Pixmap basePixmap = (mainTex != null) ? mainTex.BasePixmap.Res : null;
				GameObject gameobj = convert.Result.OfType<GameObject>().FirstOrDefault();
				bool hasAnimation = (mainTex != null && basePixmap != null && basePixmap.Atlas != null && basePixmap.Atlas.Count > 0);
				
				// Determine the size of the displayed sprite
				Vector2 spriteSize;
				if (hasAnimation)
				{
					Rect atlasRect = basePixmap.LookupAtlas(0);
					spriteSize = atlasRect.Size;
				}
				else if (mainTex != null)
				{
					spriteSize = mainTex.ContentSize;

					// If we're dealing with default content, clamp sprite size to
					// something easily visible in order to avoid 1x1 sprites for
					// default White / Black or similar fallback textures.
					if (mainTex.IsDefaultContent)
						spriteSize = Vector2.Max(spriteSize, new Vector2(32.0f, 32.0f));
				}
				else
				{
					spriteSize = Pixmap.Checkerboard.Res.Size;
				}

				// Create a sprite Component in any case
				SpriteRenderer sprite = convert.Result.OfType<SpriteRenderer>().FirstOrDefault();
				if (sprite == null && gameobj != null) sprite = gameobj.GetComponent<SpriteRenderer>();
				if (sprite == null) sprite = new SpriteRenderer();
				sprite.SharedMaterial = mat;
				sprite.Rect = Rect.Align(Alignment.Center, 0.0f, 0.0f, spriteSize.X, spriteSize.Y);
				results.Add(sprite);

				// If we have animation data, create an animator component as well
				if (hasAnimation)
				{
					SpriteAnimator animator = convert.Result.OfType<SpriteAnimator>().FirstOrDefault();
					if (animator == null && gameobj != null) animator = gameobj.GetComponent<SpriteAnimator>();
					if (animator == null) animator = new SpriteAnimator();
					animator.AnimDuration = 5.0f;
					animator.FrameCount = basePixmap.Atlas.Count;
					results.Add(animator);
				}

				convert.SuggestResultName(sprite, mat.Name);
				convert.MarkObjectHandled(mat);
			}

			convert.AddResult(results);
			return false;
		}
	}
}
