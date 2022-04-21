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
	public class FilmGrainComponent : Component, ICmpInitializable, ICmpUpdatable, IDisposable
	{
		[DontSerialize] FilmPass _pass;
		[DontSerialize] bool _isDirty = true;
		[DontSerialize] Duality.Components.Camera _cam;

		private float strength = 1f;
		public float Strength
		{
			get { return strength; }
			set { strength = value; _isDirty = true; }
		}

		private float scanLineStrength = 0f;
		public float ScanLineStrength
		{
			get { return scanLineStrength; }
			set { scanLineStrength = value; _isDirty = true; }
		}

		private int scanLineCount = 0;
		public int ScanLineCount
		{
			get { return scanLineCount; }
			set { scanLineCount = value; _isDirty = true; }
		}

		private bool greyscale = false;
		public bool Greyscale
		{
			get { return greyscale; }
			set { greyscale = value; _isDirty = true; }
		}

		void ICmpInitializable.OnActivate()
		{
			_cam = GameObj.GetComponent<Duality.Components.Camera>();
			if (_cam == null)
				Logs.Core.WriteError("No camera found on FilmGrainComponent's GameObject");
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
				_pass = new FilmPass(Strength, ScanLineStrength, ScanLineCount, Greyscale);
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