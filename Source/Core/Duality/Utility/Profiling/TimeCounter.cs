using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System;

namespace Duality
{
	public class TimeCounter : ProfileCounter
	{
		// Measurement
		private	Stopwatch	watch				= new Stopwatch();
		private double		value				= 0.0;
		private double		lastValue			= 0.0;
		// Report data
		private	double		accumValue			= 0.0;
		private double		accumMaxValue		= double.MinValue;
		private double		accumMinValue		= double.MaxValue;
		private double[]	valueGraph			= new double[ValueHistoryLen];
		private	int			valueGraphCursor	= 0;


		internal double Value
		{
			get { return this.value; }
		}
		public double LastValue
		{
			get { return this.lastValue; }
		}
		public double[] ValueGraph
		{
			get { return this.valueGraph; }
		}
		public int ValueGraphCursor
		{
			get { return this.valueGraphCursor; }
		}


		public void BeginMeasure()
		{
			this.watch.Restart();
		}
		public void EndMeasure()
		{
			this.value += this.watch.ElapsedTicks * 1000.0 / Stopwatch.Frequency;
			this.used = true;
		}
		public void Add(double value)
		{
			this.value += value;
			this.used = true;
		}
		public void Set(double value)
		{
			this.value = value;
			this.used = true;
		}
		public override void ResetFrame()
		{
			this.lastUsed = this.used;
			this.used = false;

			this.lastValue = this.value;
			this.value = 0.0;
		}
		public override void ResetAll()
		{
			this.ResetFrame();
			this.accumValue = 0.0;
			this.accumMaxValue = double.MinValue;
			this.accumMinValue = double.MaxValue;
			this.sampleCount = 0;
		}

		public override void GetReportData(out ProfileReportCounterData data)
		{
			data = new ProfileReportCounterData();
			data.Severity = MathF.Clamp(this.lastValue / Time.MillisecondsPerFrame, 0.0, 1.0);
				data.LastValue = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:F}", this.lastValue);
			if (this.IsSingleValue)
			{
					data.AverageValue = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:F}", this.lastValue);
			}
			else
			{
				if (this.sampleCount > 0)
				{
					data.AverageValue = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:F}", (double)(this.accumValue / (double)this.sampleCount));
					data.MinValue = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:F}", this.accumMinValue);
					data.MaxValue = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:F}", this.accumMaxValue);
				}
					data.SampleCount = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", this.sampleCount);
			}
		}
		protected override void OnFrameTick()
		{
			if (this.used)
			{
				this.sampleCount++;
				this.accumMaxValue = MathF.Max(this.value, this.accumMaxValue);
				this.accumMinValue = MathF.Min(this.value, this.accumMinValue);
				this.accumValue += this.value;
			}
			this.valueGraph[this.valueGraphCursor] = this.value;
			this.valueGraphCursor = (this.valueGraphCursor + 1) % this.valueGraph.Length;
			this.ResetFrame();
		}
	}
}
