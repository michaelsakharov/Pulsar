using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Duality.Components;
using Duality.Drawing;
using Duality.Editor;
using Duality.Postprocessing;
using Duality.Properties;
using Duality.Resources;
using THREE.Cameras;
using THREE.Lights;
using THREE.Math;
using THREE.Objects;

namespace Duality.Graphics.Components.PostProcessing
{
	[RequiredComponent(typeof(Duality.Components.Camera))]
	[EditorHintCategory(CoreResNames.CategoryGraphics)]
	[EditorHintImage(CoreResNames.ImageFragmentShader)]
	public class GlitchEffectComponent : Component, ICmpInitializable, ICmpUpdatable, IDisposable
	{
		[DontSerialize] GlitchPass _pass;
		[DontSerialize] bool _isDirty = true;
		[DontSerialize] Duality.Components.Camera _cam;

		void ICmpInitializable.OnActivate()
		{
			_cam = GameObj.GetComponent<Duality.Components.Camera>();
			if (_cam == null)
				Logs.Core.WriteError("No camera found on GlitchEffectComponent's GameObject");
		}

		void ICmpInitializable.OnDeactivate()
		{
			if (_pass != null && _cam != null)
			{
				_cam.RemovePass(_pass);
			}
		}

		void ICmpUpdatable.OnUpdate()
		{
			if (_isDirty && _cam != null)
			{
				_isDirty = false;
				if (_pass != null)
					_cam.RemovePass(_pass);
				_pass = new GlitchPass();
				_cam.AddPass(_pass);
			}
		}

		void IDisposable.Dispose()
		{
			if (_cam != null && _pass != null)
				_cam.RemovePass(_pass);
		}
	}
}