using System;
namespace SmartIrrigation
{
	public class SmartIrrigationConf
	{
		public int MinValueForIrrigation { get; set; }
		public int CheckMoistureAfterIrrigationHours { get; set; }
		public int CheckMoistureAftertNotIrrigationHours { get; set; }
		public int MinutesIrrigation { get; set; } = 3;
	}
}
