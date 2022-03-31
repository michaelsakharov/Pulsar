using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duality.MeshImporter.Meshes
{
	interface IMeshImporter
	{
		Mesh Import(Stream stream, string hint);
	}
}
