using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duality.Graphics.Post.Effects
{
	public class BaseEffect
	{
		protected readonly BatchBuffer _quadMesh;

		public BaseEffect(BatchBuffer quadMesh)
		{
			_quadMesh = quadMesh ?? throw new ArgumentNullException("quadMesh");
		}

		internal virtual void LoadResources()
		{
		}

		public virtual void Resize(int width, int height)
		{

		}
	}
}
