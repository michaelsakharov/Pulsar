using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duality.Renderer
{
	public enum DepthFunction
	{
		Never = 512,
		Less = 513,
		Equal = 514,
		Lequal = 515,
		Greater = 516,
		Notequal = 517,
		Gequal = 518,
		Always = 519,
	}
}
