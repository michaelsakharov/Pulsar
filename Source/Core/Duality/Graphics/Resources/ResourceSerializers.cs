﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duality.Graphics.Resources
{
	public static class ResourceSerializers
	{
		public static void Init(Duality.Resources.ResourceManager resourceManager, Backend backend, Duality.IO.FileSystem fileSystem, ShaderHotReloadConfig shaderHotReloadConfig)
		{
            var shaderLoader = new ShaderSerializer(backend, fileSystem, resourceManager);

            resourceManager.AddResourceSerializer(new TextureSerializer(backend, fileSystem));
			resourceManager.AddResourceSerializer(shaderLoader);
			resourceManager.AddResourceSerializer(new MeshSerializer(backend, resourceManager, fileSystem));
			resourceManager.AddResourceSerializer(new SkeletonSerializer(fileSystem));
			resourceManager.AddResourceSerializer(new MaterialSerializer(resourceManager, fileSystem));
			resourceManager.AddResourceSerializer(new BitmapFontSerializer(resourceManager, fileSystem));

            backend.ConfigureShaderHotReloading(shaderLoader, shaderHotReloadConfig);
		}
	}
}