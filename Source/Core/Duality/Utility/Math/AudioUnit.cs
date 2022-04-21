namespace Duality
{
	public static class AudioUnit
	{
		/// <summary>
		/// SI unit: radians
		/// </summary>
		public static readonly double AngleToDuality     = 1.0;
		/// <summary>
		/// SI unit: m
		/// </summary>
		public static readonly double LengthToDuality    = 100.0;
		/// <summary>
		/// SI unit: s
		/// </summary>
		public static readonly double TimeToDuality      = Time.FramesPerSecond;
		/// <summary>
		/// SI unit: m/s
		/// </summary>
		public static readonly double VelocityToDuality  = LengthToDuality / TimeToDuality;

		public static readonly double AngleToPhysical    = 1.0 / AngleToDuality;
		public static readonly double LengthToPhysical   = 1.0 / LengthToDuality;
		public static readonly double TimeToPhysical     = 1.0 / TimeToDuality;
		public static readonly double VelocityToPhysical = 1.0 / VelocityToDuality;
	}
}
