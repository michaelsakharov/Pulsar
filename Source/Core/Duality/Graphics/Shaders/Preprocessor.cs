using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Duality.Resources;

namespace Duality.Graphics.Shaders
{
    /// <summary>
    /// Processes glsl shaders, just #include for now
    /// </summary>
    public class Preprocessor
    {
        private static readonly Regex _preprocessorIncludeRegex = new Regex(@"^#include\s""([ \t\w /]+)""", RegexOptions.Multiline);
        public List<string> Dependencies { get; } = new List<string>();

		public bool Failed = false;
		public string Error = "";

        public string Process(string source)
        {
            return _preprocessorIncludeRegex.Replace(source, PreprocessorImportReplacer);
        }

		string PreprocessorImportReplacer(Match match)
		{
			if (Failed)
				return string.Empty;

			var name = match.Groups[1].Value.TrimStart('/');
			var nameWithExtension = name + ".glsl";

			Dependencies.Add(nameWithExtension);

			if (ContentProvider.HasContent(name))
			{
				var shader = ContentProvider.RequestContent<Duality.Resources.Shader>(name);
				if (shader.IsAvailable)
				{
					return Process(shader.Res.Source);
				}
				else
				{
					// A dependancy isnt available to load
					Failed = true;
					Error = "Dependancy Shader " + name + " Could not be found in the project!";
					return Error;
				}
			}
			else
			{
				// Maybe a Resource
				string embedded = Shader.LoadEmbeddedShaderSource(nameWithExtension);
				if (string.IsNullOrEmpty(embedded) == false)
				{
					return Process(embedded);
				}
				else
				{
					// A dependancy isnt available to load
					Failed = true;
					Error = "Dependancy Shader " + name + " Could not be found in the project!";
					return Error;
				}
			}
		}
    }
}
