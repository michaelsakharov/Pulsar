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
		PointCloud,
		Points,
		RawShader, // Not Implemented
		Shader,
		Shadow,
		Sprite,
	}
}
