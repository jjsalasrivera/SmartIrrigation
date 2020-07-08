using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartIrrigation
{
	public class IrrigationStatus
	{
		public DateTime LastRead { get; set; }

		public DateTime LastIrrigation { get; set; }

		public string Status { get; set; }

		public int S1 { get; set; }

		public int S2 { get; set; }

		public int S3 { get; set; }

		public int S4 { get; set; }

		public int Average
		{
			get
			{
				List<int> values = new List<int>();

				if( !_s1e )
					values.Add( S1 );

				if( !_s2e )
					values.Add( S2 );

				if( !_s3e )
					values.Add( S3 );

				if( !_s4e )
					values.Add( S4 );

				return values.Count > 0 ? ( int )values.Average() : 0;
			}
		}

		public int MinValue { get; set; }

		public DateTime NextRead { get; set; }

		public int MinutesIrrigation { get; set; }

		public string Notes { get; set; }

		public void SetSensorStatus( int sensor, bool inError )
		{
			switch( sensor )
			{
				case 0:
					_s1e = inError;
					break;
				case 1:
					_s2e = inError;
					break;
				case 2:
					_s3e = inError;
					break;
				case 3:
					_s4e = inError;
					break;
			}
		}

		private bool _s1e, _s2e, _s3e, _s4e = false;
	}
}
