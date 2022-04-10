using System;
using System.IO;

using Duality.Properties;
using Duality.Editor;
using System.Collections.Generic;

namespace Duality.Resources
{
	/// <summary>
	/// Represents an OpenGL VertexShader.
	/// </summary>
	[EditorHintCategory(CoreResNames.CategoryGraphics)]
	[EditorHintImage(CoreResNames.ImageVertexShader)]
	public class VertexShader : Shader
	{
		/// <summary>
		/// [GET] A minimal vertex shader. It performs OpenGLs default transformation
		/// and forwards a single texture coordinate and color to the fragment stage.
		/// </summary>
		public static ContentRef<VertexShader> Default { get; private set; }

		internal static void InitDefaultContent()
		{
			DefaultContent.InitType<VertexShader>(new Dictionary<string, VertexShader>
			{
				{ "Default", new VertexShader(@"void main() { gl_Position = projectionMatrix*modelViewMatrix*vec4(position,1.0); }") }
			});
		}


		protected override ShaderType Type
		{
			get { return ShaderType.Vertex; }
		}

		public VertexShader() : base(Default.IsAvailable ? Default.Res.Source : string.Empty) {}
		public VertexShader(string sourceCode) : base(sourceCode) {}
	}
}
