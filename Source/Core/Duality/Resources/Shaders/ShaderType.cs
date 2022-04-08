using System;
using System.Collections.Generic;
using System.Linq;

namespace Duality.Resources
{
	public enum ShaderType
	{
		Vertex,
		Fragment
	}

	public enum MaterialType
	{
		LineBasic,
		LineDashed,
		MeshBasic,
		MeshDepth,
		MeshDistance,
		MeshFace,
		MeshLambert,
		MeshMatcap,
		MeshNormal,
		MeshPhong,
		MeshPhysical,
		MeshStandard,
		MeshToon,
		PointCloud, // Not Implemented
		Points, // Not Implemented
		RawShader, // Not Implemented
		Shader, // Not Implemented
		Shadow, // Not Implemented
		Sprite, // Not Implemented
	}
}
