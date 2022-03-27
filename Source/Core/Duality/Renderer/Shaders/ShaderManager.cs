using OpenTK.Graphics.OpenGL;
using OGL = OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Duality;

namespace Duality.Renderer.Shaders
{
	public class ShaderManager
	{
		private int ActiveShaderHandle = -1;

		public void Bind(int handle)
		{
			if (ActiveShaderHandle == handle)
				return;

			ActiveShaderHandle = handle;
			GL.UseProgram(handle);
		}
	}
}
