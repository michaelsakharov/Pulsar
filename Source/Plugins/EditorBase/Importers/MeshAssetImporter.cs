using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;

using Duality;
using Duality.IO;
using Duality.Drawing;
using Duality.Resources;
using Duality.Editor;
using Duality.Editor.AssetManagement;
using Duality.Graphics.Resources;

namespace Duality.Editor.Plugins.Base
{
	public class MeshAssetImporter : AssetImporter<Mesh>
	{
		private static readonly string[] SourceFileExtensions = new[] { ".obj", ".fbx", ".x", ".dae" };


		public override string Id
		{
			get { return "BasicMeshAssetImporter"; }
		}
		public override string Name
		{
			get { return "Mesh Importer"; }
		}
		public override int Priority
		{
			get { return PriorityGeneral; }
		}
		protected override string[] SourceFileExts
		{
			get { return SourceFileExtensions; }
		}


		protected override void ImportResource(ContentRef<Mesh> resourceRef, AssetImportInput input, IAssetImportEnvironment env)
		{
			Mesh resource = resourceRef.Res;
			resource.LoadMesh(new FileStream(input.Path, FileMode.Open), System.IO.Path.GetExtension(input.Path));
		}
		protected override void ExportResource(ContentRef<Mesh> resourceRef, string path, IAssetExportEnvironment env)
		{
			Mesh resource = resourceRef.Res;

			Logs.Core.WriteError("Exporting a Mesh is currently not supported!");
		}
	}
}
