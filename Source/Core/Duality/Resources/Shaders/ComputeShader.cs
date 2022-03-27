using System;
using System.IO;

using Duality.Properties;
using Duality.Editor;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using Duality.Backend;

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

		[DontSerialize] private bool initialized = false;
		[DontSerialize] private NativeShaderProgram nativeProgram = null;

		public new int Handle 
		{ 
			get 
			{
				if (!initialized)
					Initialize();
				return nativeProgram.Handle; 
			} 
		}

		public void Initialize()
		{
			initialized = true;

			// Compute shaders dont have a DrawTechnique
			// WHich also means they dont have a GL Program, so lets create one here
			//int currentProgram = GL.GetInteger(GetPName.CurrentProgram);
			if (this.nativeProgram == null)
				this.nativeProgram = new NativeShaderProgram();
			this.nativeProgram.LoadProgram(new NativeShaderPart[] { this.Native }, this.DeclaredFields);
		}

		public int GetUniform(HashedString name)
		{
			for (int i = 0; i < nativeProgram.Fields.Length; i++)
			{
				if (nativeProgram.Fields[i].Name == name)
					return nativeProgram.FieldLocations[i];
			}
			return -1;
		}

		[DontSerialize] private object _mutex = new object();
		public void BindUniformLocations<T>(T handles) where T : class
		{
			if (!initialized)
				Initialize();

			lock (_mutex)
			{
				var type = typeof(T);
				foreach (var field in type.GetFields())
				{
					if (field.FieldType != typeof(int))
						continue;

					var fieldName = field.Name;
					var uniformName = fieldName.Replace("Handle", "");
					uniformName = char.ToLower(uniformName[0]) + uniformName.Substring(1);

					int uniformLocation = GetUniform(uniformName);

					field.SetValue(handles, uniformLocation);
				}
			}
		}

		protected override void OnDisposing(bool manually)
		{
			base.OnDisposing(manually);
			if (this.nativeProgram != null)
			{
				this.nativeProgram.Dispose();
				this.nativeProgram = null;
			}
		}

		protected override ShaderType Type
		{
			get { return ShaderType.Compute; }
		}
		
		public ComputeShader() : base(string.Empty) {}
		public ComputeShader(string sourceCode) : base(sourceCode) {}
	}
}
