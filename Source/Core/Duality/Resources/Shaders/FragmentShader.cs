using System;
using System.IO;

using Duality.Properties;
using Duality.Editor;
using System.Collections.Generic;

namespace Duality.Resources
{
	/// <summary>
	/// Represents an OpenGL FragmentShader.
	/// </summary>
	[EditorHintCategory(CoreResNames.CategoryGraphics)]
	[EditorHintImage(CoreResNames.ImageFragmentShader)]
	public class FragmentShader : Shader
	{
		/// <summary>
		/// [GET] A minimal FragmentShader. It performs a texture lookup
		/// and applies vertex-coloring.
		/// </summary>
		public static ContentRef<FragmentShader> Default	{ get; private set; }

		internal static void InitDefaultContent()
		{
			DefaultContent.InitType<FragmentShader>(new Dictionary<string, FragmentShader>
			{
				{ "Default", new FragmentShader(@"void main() { gl_FragColor = vec4(1.0,1.0,1.0,1.0); }") }
			});
		}


		protected override ShaderType Type
		{
			get { return ShaderType.Fragment; }
		}
		
		public FragmentShader() : base(Default.IsAvailable ? Default.Res.Source : string.Empty) {}
		public FragmentShader(string sourceCode) : base(sourceCode) {}
	}
}
