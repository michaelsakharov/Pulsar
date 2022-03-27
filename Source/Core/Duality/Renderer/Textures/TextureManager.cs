using OpenTK.Graphics.OpenGL;
using OGL = OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duality.Renderer.Textures
{
    public class TextureManager
    {
        private readonly int[] _activeTextures;

		public TextureManager()
		{
			_activeTextures = new int[20];
			for (var i = 0; i < _activeTextures.Length; i++)
			{
				_activeTextures[i] = -1;
			}
		}
		public void SetPixelData<T>(int handle, TextureTarget target, int width, int height, T[] data, PixelFormat format, PixelInternalFormat internalFormat, PixelType type, bool mipmap = true)
            where T : struct
        {
			var newTarget = (OGL.TextureTarget)target;

			// Upload texture data to OpenGL
			GL.BindTexture(newTarget, handle);

            if (target == TextureTarget.TextureCubeMap)
            {
                for (var i = 0; i < 6; i++)
                {
                    GL.TexImage2D(OGL.TextureTarget.TextureCubeMapPositiveX + i, 0, (OGL.PixelInternalFormat)(int)internalFormat, width, height, 0, (OGL.PixelFormat)(int)format, (OGL.PixelType)(int)type, data);
                }
                if (mipmap)
                    GL.GenerateMipmap(GenerateMipmapTarget.TextureCubeMap);
            }
            else
            {
                GL.TexImage2D(newTarget, 0, (OGL.PixelInternalFormat)(int)internalFormat, width, height, 0, (OGL.PixelFormat)(int)format, (OGL.PixelType)(int)type, data);
                if (mipmap)
                    GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

                GL.TexParameter(newTarget, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                GL.TexParameter(newTarget, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            }
        }

        public int GetActiveTexture(int textureUnit)
        {
            return _activeTextures[textureUnit];
        }

        public void Bind(int textureUnit, int handle)
        {
            if (_activeTextures[textureUnit] == handle)
                return;

            _activeTextures[textureUnit] = handle;
			GLWrapper.BindMultiTexture(TextureUnit.Texture0 + textureUnit, OGL.TextureTarget.Texture2D, handle);
		}
    }
}
