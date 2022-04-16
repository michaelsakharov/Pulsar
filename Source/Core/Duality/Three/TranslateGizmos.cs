using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Duality.DebugDraw;
using Duality.Drawing;

namespace Duality.Three
{
	static class TranslateGizmos
	{
		// just some remnant code from an side experiment, this code doesnt do anything useful anymore
		static Vector3 Translate(Vector3 p, Ray mouseRay, int screenWidth, int screenHeight)
		{
			// X Axis Draw
			Gizmos.DrawLine(p, p + new Vector3(1, 0, 0), ColorRgba.Red);
			Gizmos.DrawConeWithDirection(p + new Vector3(1.1f, 0, 0), new Vector3(-1, 0, 0), 0.1f, 15, ColorRgba.Red);
			// X Axis Logic
			BoundingBox XBox = BoundingBox.CreateFromCenterSize(p + new Vector3(0.6f, 0, 0), new Vector3(0.9f, 0.1f, 0.1f));
			//if (mouseRay.Intersects(XBox).HasValue)
			//{ 
			//}

			// Y Axis Draw
			Gizmos.DrawLine(p, p + new Vector3(0, 1, 0), ColorRgba.Yellow);
			Gizmos.DrawConeWithDirection(p + new Vector3(0, 1.1f, 0), new Vector3(0, -1, 0), 0.1f, 15, ColorRgba.Yellow);
			// Y Axis Logic
			BoundingBox YBox = BoundingBox.CreateFromCenterSize(p + new Vector3(0, 0.6f, 0), new Vector3(0.1f, 0.9f, 0.1f));

			// Z Axis Draw
			Gizmos.DrawLine(p, p + new Vector3(0, 0, 1), ColorRgba.Blue);
			Gizmos.DrawConeWithDirection(p + new Vector3(0, 0, 1.1f), new Vector3(0, 0, -1), 0.1f, 15, ColorRgba.Blue);
			// Z Axis Logic
			BoundingBox ZBox = BoundingBox.CreateFromCenterSize(p + new Vector3(0, 0, 0.6f), new Vector3(0.1f, 0.1f, 0.9f));

			return p;
		}

	}
}
