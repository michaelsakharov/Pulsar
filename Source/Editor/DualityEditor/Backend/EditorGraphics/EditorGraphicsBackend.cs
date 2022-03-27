using System;
using System.Collections.Generic;
using System.Linq;

using Duality.Drawing;
using Duality.Backend;

using OpenTK;
using OpenTK.Graphics;

namespace Duality.Editor.Backend
{
	public class EditorGraphicsBackend : IDualityBackend
	{
		string IDualityBackend.Id
		{
			get { return "DefaultOpenTKEditorGraphicsBackend"; }
		}
		string IDualityBackend.Name
		{
			get { return "GLControl (OpenTK)"; }
		}
		int IDualityBackend.Priority
		{
			get { return 0; }
		}

		bool IDualityBackend.CheckAvailable()
		{
			return true;
		}
		void IDualityBackend.Init()
		{
			// Since we'll be using only one context, we don't need sharing
			GraphicsContext.ShareContexts = false;
		}
		void IDualityBackend.Shutdown() { }

		public NativeEditorGraphicsContext CreateContext(AAQuality antialiasingQuality)
		{
			return new NativeEditorGraphicsContext(antialiasingQuality);
		}
	}
}
