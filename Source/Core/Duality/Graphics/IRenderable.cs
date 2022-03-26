using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duality.Graphics
{
	public interface IRenderable
	{
		void AddRenderOperations(RenderOperations operations);
	}
}
