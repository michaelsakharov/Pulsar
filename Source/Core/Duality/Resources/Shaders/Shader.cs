using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

using Duality.Drawing;
using Duality.Editor;
using Duality.Cloning;
using Duality.Backend;
using OpenTK.Graphics.OpenGL;

namespace Duality.Resources
{
	/// <summary>
	/// Represents an OpenGL Shader in an abstract form.
	/// </summary>
	[ExplicitResourceReference()]
	public abstract class Shader : Resource
	{
		private string source = null;

		/// <summary>
		/// The shader stage at which this shader will be used.
		/// </summary>
		protected abstract ShaderType Type { get; }
		
		/// <summary>
		/// [GET] The shaders source code.
		/// </summary>
		[EditorHintFlags(MemberFlags.Invisible)]
		public string Source
		{
			get { return this.source; }
			set
			{
				this.source = value;
			}
		}


		protected Shader() {}

		protected Shader(string sourceCode)
		{
			this.Source = sourceCode;
		}

	}
}
