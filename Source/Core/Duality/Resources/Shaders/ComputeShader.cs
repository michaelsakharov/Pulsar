using System;
using System.IO;

using Duality.Properties;
using Duality.Editor;


namespace Duality.Resources
{
	/// <summary>
	/// Represents an OpenGL ComputeShader.
	/// </summary>
	[EditorHintCategory(CoreResNames.CategoryGraphics)]
	[EditorHintImage(CoreResNames.ImageFragmentShader)]
	public class ComputeShader : Shader
	{
		internal static void InitDefaultContent()
		{
			DefaultContent.InitType<ComputeShader>(".compute", stream =>
			{
				using (StreamReader reader = new StreamReader(stream))
				{
					string code = reader.ReadToEnd();
					return new ComputeShader(code);
				}
			});
		}


		protected override ShaderType Type
		{
			get { return ShaderType.ComputeShader; }
		}
		
		public ComputeShader() : base(string.Empty) {}
		public ComputeShader(string sourceCode) : base(sourceCode) {}
	}
}
