﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

using Duality.Drawing;
using Duality.Editor;
using Duality.Cloning;
using Duality.Backend;
using OpenTK.Graphics.OpenGL;
using Duality.Graphics.Shaders;

namespace Duality.Resources
{
	/// <summary>
	/// Represents an OpenGL Shader in an abstract form.
	/// </summary>
	[ExplicitResourceReference()]
	public abstract class Shader : Resource
	{
		public static ContentRef<Shader> DualityIcon { get; private set; }

		private static readonly ShaderFieldInfo[] EmptyFields = new ShaderFieldInfo[0];

		private static List<string> commonChunks = null;

		/// <summary>
		/// [GET] A list of shader source code chunks that are shared among all loaded shaders.
		/// They contain builtin Duality functions and other shared code.
		/// </summary>
		public static IReadOnlyList<string> CommonSourceChunks
		{
			get
			{
				if (commonChunks == null)
				{
					commonChunks = new List<string>();
					commonChunks.Add(LoadEmbeddedShaderSource("BuiltinShaderFunctions.glsl"));
				}
				return commonChunks;
			}
		}

		public static string LoadEmbeddedShaderSource(string name)
		{
			using (Stream stream = DefaultContent.GetEmbeddedResourceStream(name.Replace("/", ".")))
			{
				if (stream == null)
					return "";
				using (StreamReader reader = new StreamReader(stream))
				{
					return reader.ReadToEnd();
				}
			}
		}


		private string source = null;

		[DontSerialize] private NativeShaderPart native   = null;
		[DontSerialize] private bool              compiled = false;
		[DontSerialize] private ShaderFieldInfo[] fields   = null;


		/// <summary>
		/// [GET] The shaders native backend. Don't use this unless you know exactly what you're doing.
		/// </summary>
		[EditorHintFlags(MemberFlags.Invisible)]
		public NativeShaderPart Native
		{
			get
			{
				if (!this.compiled)
					this.Compile(); 
				return this.native; 
			}
		}

		/// <summary>
		/// [GET] The shaders native backend Handle. Don't use this unless you know exactly what you're doing.
		/// </summary>
		[EditorHintFlags(MemberFlags.Invisible)]
		public int Handle
		{
			get
			{
				if (!this.compiled)
					this.Compile(); 
				return this.native.Handle; 
			}
		}
		/// <summary>
		/// The shader stage at which this shader will be used.
		/// </summary>
		protected abstract ShaderType Type { get; }
		/// <summary>
		/// [GET] Whether this shader has been compiled yet or not.
		/// </summary>
		public bool Compiled
		{
			get { return this.compiled; }
		}
		/// <summary>
		/// [GET] A list of fields that are declared in this shader. May trigger compiling the
		/// shader if it wasn't compiled yet.
		/// </summary>
		public IReadOnlyList<ShaderFieldInfo> DeclaredFields
		{
			get
			{
				if (!this.compiled)
					this.Compile();
				return this.fields ?? EmptyFields;
			}
		}
		/// <summary>
		/// [GET] The shaders source code.
		/// </summary>
		[EditorHintFlags(MemberFlags.Invisible)]
		public string Source
		{
			get { return this.source; }
			set
			{
				this.compiled = false;
				this.source = value;
			}
		}


		protected Shader() {}
		protected Shader(string sourceCode)
		{
			this.Source = sourceCode;
		}


		/// <summary>
		/// Compiles the shader. This is done automatically when loading the shader
		/// or attaching it to a <see cref="DrawTechnique"/>.
		/// </summary>
		public void Compile(string parameters = "")
		{
			Logs.Core.Write("Compiling {0} shader '{1}'...", this.Type, this.FullName);
			Logs.Core.PushIndent();

			if (string.IsNullOrEmpty(this.source))
			{
				Logs.Core.PopIndent();
				throw new InvalidOperationException("Can't compile a shader without any source code specified.");
			}

			if (this.native == null)
				this.native = new NativeShaderPart();

			// Preprocess the source code to include builtin shader functions
			string processedSource = null;
			ShaderFieldInfo[] fields = null;
			try
			{
				ShaderSourceBuilder builder = new ShaderSourceBuilder();
				string typeConditional = string.Format("SHADERTYPE_{0}", this.Type).ToUpperInvariant();
				builder.SetConditional(typeConditional, true);
				if (!string.IsNullOrWhiteSpace(parameters))
				{
					foreach (var param in parameters.Split(';'))
					{
						// #define param
						builder.SetConditional(param, true);
					}
				}
				builder.SetMainChunk(this.source);
				foreach (string sharedChunk in CommonSourceChunks)
				{
					builder.AddSharedChunk(sharedChunk);
				}
				processedSource = builder.Build();
				fields = builder.Fields.ToArray();
			}
			catch (Exception e)
			{
				Logs.Core.WriteError("Failed to preprocess shader:{1}{0}", LogFormat.Exception(e), Environment.NewLine);
			}

			// Process Dependancies
			if (processedSource != null)
			{
				Preprocessor processer = new Preprocessor();
				try
				{
					processedSource = processer.Process(processedSource);
					if (processer.Failed)
					{
						processedSource = null; // No valid shader
						Logs.Core.WriteError(processer.Error, Environment.NewLine);
					}
				}
				catch (Exception e)
				{
					Logs.Core.WriteError("Failed to load dependencies shader:{1}{0}", LogFormat.Exception(e), Environment.NewLine);
				}
			}

			// Load the shader on the backend side
			if (processedSource != null)
			{
				try
				{
					this.native.LoadSource(processedSource, this.Type);
				}
				catch (Exception e)
				{
					Logs.Core.WriteError("Failed to compile shader:{1}{0}", LogFormat.Exception(e), Environment.NewLine);
				}
			}

			this.fields = fields;

			this.compiled = true;
			Logs.Core.PopIndent();
		}

		protected override void OnLoaded()
		{
			this.Compile();
			base.OnLoaded();
		}
		protected override void OnDisposing(bool manually)
		{
			base.OnDisposing(manually);
			if (this.native != null)
			{
				this.native.Dispose();
				this.native = null;
			}
		}
		protected override void OnCopyDataTo(object target, ICloneOperation operation)
		{
			base.OnCopyDataTo(target, operation);
			Shader targetShader = target as Shader;
			if (this.compiled) targetShader.Compile();
		}
	}
}
