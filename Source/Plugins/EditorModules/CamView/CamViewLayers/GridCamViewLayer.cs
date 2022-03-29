using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;

using Duality;
using Duality.Drawing;
using Duality.Resources;

using Duality.Editor.Plugins.CamView.CamViewStates;

namespace Duality.Editor.Plugins.CamView.CamViewLayers
{
	public class GridCamViewLayer : CamViewLayer
	{
	    public override string LayerName
	    {
	        get { return Properties.CamViewRes.CamViewLayer_Grid_Name; }
	    }
	    public override string LayerDesc
	    {
	        get { return Properties.CamViewRes.CamViewLayer_Grid_Desc; }
	    }
	    public override int Priority
	    {
	        get { return base.Priority - 10; }
	    }
		public override bool MouseTracking
		{
			get { return true; }
		}
	    protected internal override void OnCollectDrawcalls()
	    {
	        base.OnCollectDrawcalls();
	    }
	}
}
