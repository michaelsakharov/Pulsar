using System;
using System.Collections.Generic;
using System.Linq;

using Duality.Backend;
using Duality.Drawing;
using Duality.Editor;
using Duality.Cloning;
using Duality.Properties;

namespace Duality.Resources
{
	/// <summary>
	/// DrawTechniques represent the method by which a set of colors, <see cref="Duality.Resources.Texture">Textures</see> and
	/// vertex data is applied to screen. 
	/// </summary>
	/// <seealso cref="Duality.Resources.Material"/>
	/// <seealso cref="Duality.Resources.FragmentShader"/>
	/// <seealso cref="Duality.Resources.VertexShader"/>
	/// <seealso cref="Duality.Drawing.BlendMode"/>
	[EditorHintCategory(CoreResNames.CategoryGraphics)]
	[EditorHintImage(CoreResNames.ImageDrawTechnique)]
	public class DrawTechnique : Resource
	{
		/// <summary>
		/// Renders solid geometry without utilizing the alpha channel. This is the fastest default DrawTechnique.
		/// </summary>
		public static ContentRef<DrawTechnique> Solid		{ get; private set; }
		/// <summary>
		/// Renders geometry for a picking operation. This isn't used for regular rendering.
		/// </summary>
		public static ContentRef<DrawTechnique> Picking		{ get; private set; }

		internal static void InitDefaultContent()
		{
			DefaultContent.InitType<DrawTechnique>(new Dictionary<string,DrawTechnique>
			{
				{ "Solid", new DrawTechnique(VertexShader.Minimal, FragmentShader.Minimal) },

				{ "Picking", new DrawTechnique(VertexShader.Minimal, FragmentShader.Picking) },
			});
		}
		

		private ContentRef<VertexShader>   vertexShader      = VertexShader.Minimal;
		private ContentRef<FragmentShader> fragmentShader    = FragmentShader.Minimal;
		private string paremeters = "";

		[DontSerialize] private ShaderParameterCollection defaultParameters = null;
		[DontSerialize] private NativeShaderProgram      nativeShader      = null;
		[DontSerialize] private bool                      compiled          = false;
		[DontSerialize] private ShaderFieldInfo[]         shaderFields      = null;


		/// <summary>
		/// [GET] The shaders native backend. Don't use this unless you know exactly what you're doing.
		/// </summary>
		[EditorHintFlags(MemberFlags.Invisible)]
		public NativeShaderProgram NativeShader
		{
			get
			{
				if (!this.compiled)
					this.Compile();
				return this.nativeShader;
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
				return this.nativeShader.Handle;
			}
		}
		/// <summary>
		/// [GET] Returns whether the internal shader program of this <see cref="DrawTechnique"/> has been compiled.
		/// </summary>
		[EditorHintFlags(MemberFlags.Invisible)]
		public bool Compiled
		{
			get { return this.compiled; }
		}
		/// <summary>
		/// [GET] Returns an array containing information about the variables that have been declared in shader source code.
		/// May trigger compiling the technique, if it wasn't compiled already.
		/// </summary>
		public IReadOnlyList<ShaderFieldInfo> DeclaredFields
		{
			get
			{
				if (!this.compiled)
					this.Compile();
				return this.shaderFields;
			}
		}
		/// <summary>
		/// [GET / SET] The <see cref="Resources.VertexShader"/> that is used for rendering.
		/// </summary>
		public ContentRef<VertexShader> Vertex
		{
			get { return this.vertexShader; }
			set
			{
				this.vertexShader = value;
				this.compiled = false;
			}
		}
		/// <summary>
		/// [GET / SET] The <see cref="Resources.FragmentShader"/> that is used for rendering.
		/// </summary>
		public ContentRef<FragmentShader> Fragment
		{
			get { return this.fragmentShader; }
			set
			{
				this.fragmentShader = value;
				this.compiled = false;
			}
		}
		/// <summary>
		/// [GET] The set of default parameters that acts as a fallback in cases
		/// where a parameter has not been set by a <see cref="Material"/> or <see cref="BatchInfo"/>.
		/// 
		/// The result of this property should be treated as read-only.
		/// </summary>
		[EditorHintFlags(MemberFlags.Invisible)]
		public ShaderParameterCollection DefaultParameters
		{
			get
			{
				if (this.defaultParameters == null)
				{
					// Setup default values on demand - for now, just a few hardcoded ones
					this.defaultParameters = new ShaderParameterCollection();
					this.defaultParameters.Set(BuiltinShaderFields.MainColor, Vector4.One);
					this.defaultParameters.Set(BuiltinShaderFields.MainTex, Texture.White);
					this.defaultParameters.Set(BuiltinShaderFields.NormalMap, Texture.DefaultNormalMap);
				}
				return this.defaultParameters;
			}
		}

		/// <summary>
		/// Creates a new, default DrawTechnique
		/// </summary>
		public DrawTechnique() {}
		/// <summary>
		/// Creates a new DrawTechnique using the specified <see cref="BlendMode"/> and shaders.
		/// </summary>
		public DrawTechnique(ContentRef<VertexShader> vertexShader, ContentRef<FragmentShader> fragmentShader) 
		{
			this.vertexShader = vertexShader;
			this.fragmentShader = fragmentShader;
		}
		public DrawTechnique(ContentRef<VertexShader> vertexShader, ContentRef<FragmentShader> fragmentShader, string paremeters) 
		{
			this.vertexShader = vertexShader;
			this.fragmentShader = fragmentShader;
			this.paremeters = paremeters;
		}
		public DrawTechnique(ContentRef<CompoundShader> libraryShader, string paremeters)
		{
			this.vertexShader = new VertexShader(libraryShader.Res.Source);
			this.fragmentShader = new FragmentShader(libraryShader.Res.Source);
			this.paremeters = paremeters;
		}
		public DrawTechnique(string libraryShader, string paremeters)
		{
			this.vertexShader = new VertexShader(libraryShader);
			this.fragmentShader = new FragmentShader(libraryShader);
			this.paremeters = paremeters;
		}

		/// <summary>
		/// Compiles the internal shader program of this <see cref="DrawTechnique"/>. This is 
		/// done automatically on load and only needs to be invoked manually when the technique
		/// or one of its shader dependencies changed.
		/// </summary>
		public void Compile()
		{
			Logs.Core.Write("Compiling DrawTechnique '{0}'...", this.FullName);
			Logs.Core.PushIndent();

			if (this.nativeShader == null)
				this.nativeShader = new NativeShaderProgram();

			// Create a list of all shader parts that we'll be linking
			List<Shader> parts = new List<Shader>();
			parts.Add(this.vertexShader.Res ?? VertexShader.Minimal.Res);
			parts.Add(this.fragmentShader.Res ?? FragmentShader.Minimal.Res);

			// Ensure all shader parts are compiled
			List<NativeShaderPart> nativeParts = new List<NativeShaderPart>();
			foreach (Shader part in parts)
			{
				if (!part.Compiled) part.Compile(paremeters);
				nativeParts.Add(part.Native);
			}

			// Gather shader field declarations from all shader parts
			Dictionary<string, ShaderFieldInfo> fieldMap = new Dictionary<string, ShaderFieldInfo>();
			foreach (Shader part in parts)
			{
				foreach (ShaderFieldInfo field in part.DeclaredFields)
				{
					fieldMap[field.Name] = field;
				}
			}

			// Load the program with all shader parts attached
			try
			{
				this.shaderFields = fieldMap.Values.ToArray();
				this.nativeShader.LoadProgram(nativeParts, this.shaderFields);

				// Validate that we have at least one attribute in the shader. Warn otherwise.
				if (!this.shaderFields.Any(f => f.Scope == ShaderFieldScope.Attribute))
					Logs.Core.WriteWarning("The shader doesn't seem to define any vertex attributes. Is this intended?");
			}
			catch (Exception e)
			{
				this.shaderFields = new ShaderFieldInfo[0];
				Logs.Core.WriteError("Failed to compile DrawTechnique:{1}{0}", LogFormat.Exception(e), Environment.NewLine);
			}

			// Even if we failed, we tried to compile it. Don't do it again and again.
			this.compiled = true;
			Logs.Core.PopIndent();
		}

		public int GetUniform(HashedString name)
		{
			for (int i = 0; i < NativeShader.Fields.Length; i++)
			{
				if (NativeShader.Fields[i].Name == name)
					return NativeShader.FieldLocations[i];
			}
			return -1;
		}

		[DontSerialize] private object _mutex = new object();
		public void BindUniformLocations<T>(T handles) where T : class
		{
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

		protected override void OnLoaded()
		{
			this.Compile();
			base.OnLoaded();
		}
		protected override void OnDisposing(bool manually)
		{
			base.OnDisposing(manually);
			if (this.nativeShader != null)
			{
				this.nativeShader.Dispose();
				this.nativeShader = null;
			}
		}
		protected override void OnCopyDataTo(object target, ICloneOperation operation)
		{
			base.OnCopyDataTo(target, operation);
			DrawTechnique targetTechnique = target as DrawTechnique;
			if (this.compiled) targetTechnique.Compile();
		}
	}
}
