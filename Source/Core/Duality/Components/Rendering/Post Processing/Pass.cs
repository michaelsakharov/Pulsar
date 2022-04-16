using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using THREE.Renderers;

namespace Duality.Postprocessing
{
	public abstract class Pass
	{
		public bool Enabled = true;
		public bool NeedsSwap = true;
		public bool Clear = false;
		public FullScreenQuad fullScreenQuad = null;

		public THREE.Scenes.Scene scene;
		public THREE.Cameras.Camera camera;

		public EffectComposer composer = null;

		public bool AutoClear = false;

		public Pass() { }

		public abstract void SetSize(float width, float height);



		public abstract void Render(GLRenderTarget writeBuffer, GLRenderTarget readBuffer, bool? maskActive = null);

	}
}
