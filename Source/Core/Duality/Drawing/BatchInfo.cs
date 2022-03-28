using System;
using System.Collections.Generic;
using System.Linq;
using Duality.Backend;
using Duality.Components;
using Duality.Editor;
using Duality.Resources;

namespace Duality.Drawing
{
	/// <summary>
	/// BatchInfos describe how an object, represented by a set of vertices, looks like.
	/// </summary>
	/// <seealso cref="Material"/>
	public class BatchInfo : IEquatable<BatchInfo>
	{
		private ContentRef<DrawTechnique> technique  = DrawTechnique.Solid;
		private ShaderParameterCollection parameters = null;
		private ShaderHandles _handles;

		/// <summary>
		/// [GET / SET] The <see cref="Duality.Resources.DrawTechnique"/> that is used.
		/// </summary>
		public ContentRef<DrawTechnique> Technique
		{
			get { return this.technique; }
			set { this.technique = value; }
		}
		/// <summary>
		/// [GET / SET] The main texture of the material. This property is a shortcut for
		/// a regular shader parameter as accessible via <see cref="GetTexture"/>.
		/// </summary>
		public ContentRef<Texture> MainTexture
		{
			get { return this.GetTexture(BuiltinShaderFields.MainTex); }
			set { this.SetTexture(BuiltinShaderFields.MainTex, value); }
		}
		/// <summary>
		/// [GET / SET] The main color of the material. This property is a shortcut for
		/// a regular shader parameter as accessible via <see cref="GetValue"/>.
		/// </summary>
		public ColorRgba MainColor
		{
			get
			{
				Vector4 color = this.GetValue<Vector4>(BuiltinShaderFields.MainColor);
				return new ColorRgba(color.X, color.Y, color.Z, color.W);
			}
			set
			{
				this.SetValue<Vector4>(BuiltinShaderFields.MainColor, new Vector4(
					value.R / 255.0f, 
					value.G / 255.0f, 
					value.B / 255.0f, 
					value.A / 255.0f));
			}
		}


		/// <summary>
		/// Creates a new, empty BatchInfo.
		/// </summary>
		public BatchInfo()
		{
			this.parameters = new ShaderParameterCollection();
		}
		/// <summary>
		/// Creates a new BatchInfo based on an existing <see cref="Material"/>.
		/// </summary>
		/// <param name="source"></param>
		public BatchInfo(Material source) : this(source.Info) {}
		/// <summary>
		/// Creates a new BatchInfo based on an existing BatchInfo. This is essentially a copy constructor.
		/// </summary>
		/// <param name="source"></param>
		public BatchInfo(BatchInfo source)
		{
			this.InitFrom(source);
		}
		/// <summary>
		/// Creates a new color-only BatchInfo.
		/// </summary>
		/// <param name="technique">The <see cref="Duality.Resources.DrawTechnique"/> to use.</param>
		public BatchInfo(ContentRef<DrawTechnique> technique) : this()
		{
			this.technique = technique;
		}
		/// <summary>
		/// Creates a new color-only BatchInfo.
		/// </summary>
		/// <param name="technique">The <see cref="Duality.Resources.DrawTechnique"/> to use.</param>
		/// <param name="mainColor">The <see cref="MainColor"/> to use.</param>
		public BatchInfo(ContentRef<DrawTechnique> technique, ColorRgba mainColor) : this(technique)
		{
			this.MainColor = mainColor;
		}
		/// <summary>
		/// Creates a new single-texture BatchInfo.
		/// </summary>
		/// <param name="technique">The <see cref="Duality.Resources.DrawTechnique"/> to use.</param>
		/// <param name="mainTex">The main <see cref="Duality.Resources.Texture"/> to use.</param>
		public BatchInfo(ContentRef<DrawTechnique> technique, ContentRef<Texture> mainTex) : this(technique) 
		{
			this.MainTexture = mainTex;
		}
		/// <summary>
		/// Creates a new tinted, single-texture BatchInfo.
		/// </summary>
		/// <param name="technique">The <see cref="Duality.Resources.DrawTechnique"/> to use.</param>
		/// <param name="mainColor">The <see cref="MainColor"/> to use.</param>
		/// <param name="mainTex">The main <see cref="Duality.Resources.Texture"/> to use.</param>
		public BatchInfo(ContentRef<DrawTechnique> technique, ColorRgba mainColor, ContentRef<Texture> mainTex) : this(technique)
		{
			this.MainColor = mainColor;
			this.MainTexture = mainTex;
		}


		/// <summary>
		/// Initializes this <see cref="BatchInfo"/> instance to match the specified
		/// target instance exactly, e.g. use the same <see cref="Technique"/> and
		/// specify the same shader parameter values.
		/// </summary>
		public void InitFrom(BatchInfo source)
		{
			this.Reset();
			this.technique = source.technique;
			if (source.parameters != null)
			{
				if (this.parameters == null)
					this.parameters = new ShaderParameterCollection(source.parameters);
				else
					source.parameters.CopyTo(this.parameters);
			}
		}
		/// <summary>
		/// Resets all shader parameters to their default value.
		/// </summary>
		public void Reset()
		{
			if (this.parameters != null)
				this.parameters.Clear();
		}

		/// <summary>
		/// Assigns an array of values to the specified variable. All values are copied and converted into
		/// a shared internal format.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <seealso cref="ShaderParameterCollection.Set"/>
		public void SetArray<T>(string name, T[] value) where T : struct
		{
			this.parameters.Set(name, value);
		}
		/// <summary>
		/// Assigns a blittable value to the specified variable. All values are copied and converted into
		/// a shared internal format.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <seealso cref="ShaderParameterCollection.Set"/>
		public void SetValue<T>(string name, T value) where T : struct
		{
			this.parameters.Set(name, value);
		}
		/// <summary>
		/// Assigns a texture to the specified variable.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <seealso cref="ShaderParameterCollection.Set"/>
		public void SetTexture(string name, ContentRef<Texture> value)
		{
			this.parameters.Set(name, value);
		}
		/// <summary>
		/// Assigns all shader variables in batch.
		/// </summary>
		/// <param name="variables"></param>
		/// <seealso cref="ShaderParameterCollection.CopyTo"/>
		public void SetVariables(ShaderParameterCollection variables)
		{
			variables.CopyTo(this.parameters);
		}

		/// <summary>
		/// Retrieves a copy of the values that are assigned the specified variable. If the internally 
		/// stored type does not match the specified type, it will be converted before returning.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name"></param>
		/// <seealso cref="ShaderParameterCollection.TryGet"/>
		public T[] GetArray<T>(string name) where T : struct
		{
			// Retrieve the material parameter if available
			T[] result;
			if (this.parameters != null && this.parameters.TryGet(name, out result))
				return result;

			// Fall back to the used techniques default parameter value
			DrawTechnique tech = this.technique.Res;
			if (tech != null && tech.DefaultParameters.TryGet(name, out result))
				return result;

			return null;
		}
		/// <summary>
		/// Retrieves a blittable value from the specified variable. All values are copied and converted into
		/// a shared internal format.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name"></param>
		/// <seealso cref="ShaderParameterCollection.TryGet"/>
		public T GetValue<T>(string name) where T : struct
		{
			// Retrieve the material parameter if available
			T result;
			if (this.parameters != null && this.parameters.TryGet(name, out result))
				return result;

			// Fall back to the used techniques default parameter value
			DrawTechnique tech = this.technique.Res;
			if (tech != null && tech.DefaultParameters.TryGet(name, out result))
				return result;

			return default(T);
		}
		public object GetValue(string name, ShaderFieldType type)
		{
			// Retrieve the material parameter if available
			object result;
			if (this.parameters != null && this.parameters.TryGet(name, type, out result))
				return result;

			// Fall back to the used techniques default parameter value
			DrawTechnique tech = this.technique.Res;
			if (tech != null && tech.DefaultParameters.TryGet(name, type, out result))
				return result;

			return null;
		}
		/// <summary>
		/// Retrieves a texture from the specified variable.
		/// </summary>
		/// <param name="name"></param>
		/// <seealso cref="ShaderParameterCollection.TryGet"/>
		public ContentRef<Texture> GetTexture(string name)
		{
			// Retrieve the material parameter if available
			ContentRef<Texture> result;
			if (this.parameters != null && this.parameters.TryGet(name, out result))
				return result;

			// Fall back to the used techniques default parameter value
			DrawTechnique tech = this.technique.Res;
			if (tech != null && tech.DefaultParameters.TryGet(name, out result))
				return result;

			return null;
		}

		/// <summary>
		/// Retrieves the internal representation of the specified variables numeric value.
		/// The returned array should be treated as read-only.
		/// </summary>
		/// <param name="name"></param>
		public float[] GetInternalData(string name)
		{
			// Retrieve the material parameter if available
			float[] result;
			if (this.parameters != null && this.parameters.TryGetInternal(name, out result))
				return result;

			// Fall back to the used techniques default parameter value
			DrawTechnique tech = this.technique.Res;
			if (tech != null && tech.DefaultParameters.TryGetInternal(name, out result))
				return result;

			return null;
		}
		/// <summary>
		/// Retrieves the internal representation of the specified variables texture value.
		/// The returned value should be treated as read-only.
		/// </summary>
		/// <param name="name"></param>
		public ContentRef<Texture> GetInternalTexture(string name)
		{
			// Retrieve the material parameter if available
			ContentRef<Texture> result;
			if (this.parameters != null && this.parameters.TryGetInternal(name, out result))
				return result;

			// Fall back to the used techniques default parameter value
			DrawTechnique tech = this.technique.Res;
			if (tech != null && tech.DefaultParameters.TryGetInternal(name, out result))
				return result;

			return null;
		}

		public ShaderParameterCollection GetParameters()
		{
			if (this.parameters != null)
				return this.parameters;
			if (this.technique.Res != null)
				return this.technique.Res.DefaultParameters;
			return null;
		}

		[NonSerialized] private int[] _textureHandles;
		[NonSerialized] private int[] _samplerToTexture;
		[NonSerialized] private int[] _samplers;
		[NonSerialized] private bool Initialized;

		public void Initialize()
		{
			Initialized = true;
			_handles = new ShaderHandles();

			DrawTechnique tech = this.technique.Res ?? DrawTechnique.Solid.Res;
			tech.BindUniformLocations(_handles);

			var Textures = GetParameters().GetAllTextures();

			_textureHandles = new int[Textures.Count];
			_samplerToTexture = new int[Textures.Count];
			_samplers = new int[Textures.Count];

			var i = 0;
			foreach (var samplerInfo in Textures)
			{
				_textureHandles[i] = samplerInfo.Item2.Res.Handle;
				_samplers[i] = DualityApp.GraphicsBackend.DefaultSampler;
				_samplerToTexture[i] = tech.GetUniform(samplerInfo.Item1);
				i++;
			}
		}

		public void BeginInstance(Camera camera, int renderStateId)
		{
			if (!Initialized)
			{
				Initialize();
			}

			DrawTechnique tech = this.technique.Res ?? DrawTechnique.Solid.Res;

			DualityApp.GraphicsBackend.BeginInstance(tech.Handle, _textureHandles, samplers: _samplers, renderStateId: renderStateId);
			for (var i = 0; i < _samplerToTexture.Length; i++)
			{
				DualityApp.GraphicsBackend.BindShaderVariable(_samplerToTexture[i], i);
			}

			// Bind Shader
			NativeShaderProgram nativeShader = tech.NativeShader as NativeShaderProgram;
			NativeShaderProgram.Bind(nativeShader);

			// Setup shader data
			ShaderFieldInfo[] varInfo = nativeShader.Fields;
			int[] locations = nativeShader.FieldLocations;

			DualityApp.GraphicsBackend.BindShaderVariable(_handles.Time, DualityApp.GraphicsBackend.ElapsedTime);
			Vector3 camPos = camera.GameObj.Transform.Pos;
			DualityApp.GraphicsBackend.BindShaderVariable(_handles.CameraPosition, ref camPos);

			// Setup sampler bindings and uniform data
			for (int i = 0; i < varInfo.Length; i++)
			{
				ShaderFieldInfo field = varInfo[i];
				int location = locations[i];

				if (field.Scope == ShaderFieldScope.Attribute) continue;
				//if (this.sharedShaderParameters.Contains(field.Name)) continue;

				if (field.Type == ShaderFieldType.Sampler2D)
				{
					// textures are handled Above
					//ContentRef<Texture> texRef = GetInternalTexture(field.Name);
					//NativeTexture.Bind(texRef, curSamplerIndex);
					//GL.Uniform1(location, curSamplerIndex);
				}
				else
				{
					object data = GetValue(field.Name, field.Type);
					if (data == null)
						continue;
					SetUniform(field, location, data);
				}
			}
			//NativeTexture.ResetBinding(curSamplerIndex);
		}

		public static void SetUniform(ShaderFieldInfo field, int location, object data)
		{
			if (field.Scope != ShaderFieldScope.Uniform) return;
			if (location == -1) return;
			switch (field.Type)
			{
				case ShaderFieldType.Bool:
				case ShaderFieldType.Int:
					DualityApp.GraphicsBackend.BindShaderVariable(location, (int)data);
					break;
				case ShaderFieldType.Float:
					DualityApp.GraphicsBackend.BindShaderVariable(location, (float)data);
					break;
				case ShaderFieldType.Vec2:
					var vec2 = (Vector2)data;
					DualityApp.GraphicsBackend.BindShaderVariable(location, ref vec2);
					break;
				case ShaderFieldType.Vec3:
					var vec3 = (Vector3)data;
					DualityApp.GraphicsBackend.BindShaderVariable(location, ref vec3);
					break;
				case ShaderFieldType.Vec4:
					var vec4 = (Vector4)data;
					DualityApp.GraphicsBackend.BindShaderVariable(location, ref vec4);
					break;
				case ShaderFieldType.Mat2:
					break;
				case ShaderFieldType.Mat3:
					//var mat3 = (Matrix3)data;
					//DualityApp.GraphicsBackend.BindShaderVariable(location, ref mat3);
					break;
				case ShaderFieldType.Mat4:
					var mat2 = (Matrix4)data;
					DualityApp.GraphicsBackend.BindShaderVariable(location, ref mat2);
					break;
			}
		}

		/// <summary>
		/// Bind the material, this will call BeginInstance on the backend
		/// It is up to the caller to call EndInstance
		/// </summary>
		/// <param name="world"></param>
		/// <param name="worldView"></param>
		/// <param name="itWorldView"></param>
		/// <param name="modelViewProjection"></param>
		public void BindPerObject(ref Matrix4 world, ref Matrix4 worldView, ref Matrix4 itWorld, ref Matrix4 modelViewProjection, Graphics.SkeletalAnimation.SkeletonInstance skeleton)
		{
			DualityApp.GraphicsBackend.BindShaderVariable(_handles.ModelViewProjection, ref modelViewProjection);
			DualityApp.GraphicsBackend.BindShaderVariable(_handles.World, ref world);
			DualityApp.GraphicsBackend.BindShaderVariable(_handles.WorldView, ref worldView);
			DualityApp.GraphicsBackend.BindShaderVariable(_handles.ItWorld, ref itWorld);

			if (skeleton != null)
			{
				DualityApp.GraphicsBackend.BindShaderVariable(_handles.Bones, ref skeleton.FinalBoneTransforms);
			}
		}

		class ShaderHandles
		{
			public int ModelViewProjection = 0;
			public int World = 0;
			public int WorldView = 0;
			public int ItWorldView = 0;
			public int Bones = 0;
			public int CameraPosition = 0;
			public int ItWorld = 0;
			public int Time = 0;
		}

		public override string ToString()
		{
			return string.Format("{0}, #{1:X8} ({2})",
				this.MainTexture.Name, 
				this.MainColor.ToIntRgba(),
				this.technique.Name);
		}
		public override int GetHashCode()
		{
			int hashCode = 17;
			unchecked
			{
				hashCode = hashCode * 23 + this.technique.GetHashCode();
				hashCode = hashCode * 23 + this.parameters.GetHashCode();
			}
			return hashCode;
		}
		public override bool Equals(object obj)
		{
			BatchInfo other = obj as BatchInfo;
			if (other != null)
				return this.Equals(other);
			else
				return false;
		}
		public bool Equals(BatchInfo other)
		{
			return
				this.technique == other.technique &&
				this.parameters.Equals(other.parameters);
		}
	}
}
