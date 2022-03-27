using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

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

			var path = match.Groups[1].Value + ".glsl";

			Dependencies.Add(path);

			var shader = ContentProvider.RequestContent<Duality.Resources.Shader>(match.Groups[1].Value);
			if (shader.IsAvailable)
			{
				return Process(shader.Res.Source);
			}
			else
			{
				// A dependancy isnt available to load
				Failed = true;
				Error = "Dependancy " +  match.Groups[1].Value + ".glsl" + " Could not be found in the project!";
				return Error;
			}
		}
    }
}
