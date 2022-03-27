using System;
using System.IO;

using Duality.Properties;
using Duality.Editor;


namespace Duality.Resources
{
	/// <summary>
	/// Represents an OpenGL LibraryShader.
	/// </summary>
	[EditorHintCategory(CoreResNames.CategoryGraphics)]
	[EditorHintImage(CoreResNames.ImageFragmentShader)]
	public class CompoundShader : Shader
	{
		internal static void InitDefaultContent()
		{
			DefaultContent.InitType<CompoundShader>(".glsl", stream =>
			{
				using (StreamReader reader = new StreamReader(stream))
				{
					string code = reader.ReadToEnd();
					return new CompoundShader(code);
				}
			});
		}


		protected override ShaderType Type
		{
			get { return ShaderType.Compound; }
		}
		
		public CompoundShader() : base(string.Empty) {}
		public CompoundShader(string sourceCode) : base(sourceCode) {}
	}
}
