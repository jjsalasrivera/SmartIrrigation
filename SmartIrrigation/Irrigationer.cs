using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;
using System.Device.Gpio;
using Iot.Device.Adc;
using System.Device.Spi;

namespace SmartIrrigation
{
	public class Irrigationer
	{
		private static readonly Irrigationer INSTANCE = new Irrigationer();

		private static readonly ILog _log = LogManager.GetLogger( MethodBase.GetCurrentMethod().DeclaringType );

		private const string LASTSTATUSFILE = "lastStatusFile.json";

		private const string READING = "Reading";
		private const string STANDBY = "Standby";
		private const string IRRIGATING = "Irrigating";

		private const int IRRIGATIONPIN = 0;
		private const int READTRANSISTORPIN = 3;

		private const int NOTCONNECTEDTHRESOHLD = 200;
		private const int NOTONEARTHTHRESHOLD = 2000;
		
		GpioController _gpioController = new GpioController();
		//SpiDevice _spiController = null;
		Mcp3208 _mcp3208 = null;

		private Task _taskWork;
		private CancellationTokenSource _tokenSource = new CancellationTokenSource();
		private SmartIrrigationConf _conf;

		private IrrigationStatus _irrigationStatus = null;

		private Irrigationer() { }

		public static Irrigationer Instance
		{
			get
			{
				return INSTANCE;
			}
		}

		public IrrigationStatus GetStatus()
		{
			return _irrigationStatus;
		}

		public void Start( SmartIrrigationConf conf )
		{
			_conf = conf;

			var spiSettings = new SpiConnectionSettings( 1,0 ) { ClockFrequency = 500000, Mode = SpiMode.Mode0 };
			_mcp3208 = new Mcp3208( SpiDevice.Create(spiSettings) );

			_initializeStatus();

			_gpioController.OpenPin( READTRANSISTORPIN, PinMode.Output );
			_gpioController.OpenPin( IRRIGATIONPIN, PinMode.Output );

			if( _irrigationStatus.LastRead == DateTime.MinValue)
			{
				// make a first read in order to set status for the first time
				ForceRead();
			}

			_log.Info( "start thread" );
			_taskWork = Task.Factory.StartNew( _mainWork, TaskCreationOptions.LongRunning );
		}

		public void Stop()
		{
			_tokenSource.Cancel();

			_gpioController.ClosePin( IRRIGATIONPIN );
			_gpioController.ClosePin( READTRANSISTORPIN );

			_gpioController.Dispose();
			_mcp3208.Dispose();
			//_gpioController.Dispose();
		}

		private void _initializeStatus()
		{
			_irrigationStatus = null;

			try
			{
				_irrigationStatus = JsonConvert.DeserializeObject<IrrigationStatus>( File.ReadAllText( LASTSTATUSFILE ) );
			}
			catch( Exception ex )
			{
				_log.Warn( $"Exeption reading file '{LASTSTATUSFILE}': {ex.Message}" );

				_irrigationStatus = new IrrigationStatus { LastRead = DateTime.MinValue };
			}

			_irrigationStatus.Status = STANDBY;
			_irrigationStatus.MinValue = _conf.MinValueForIrrigation;
			_irrigationStatus.MinutesIrrigation = _conf.MinutesIrrigation;
			_saveStatus();
		}

		private void _saveStatus()
		{
			File.WriteAllText( LASTSTATUSFILE, JsonConvert.SerializeObject( _irrigationStatus, Formatting.Indented ) );
		}

		private void _mainWork()
		{
			_log.Info( "Init cycle" );

			while( !_tokenSource.IsCancellationRequested )
			{
				_log.Debug( "On cycle" );

				if( DateTime.Now >= _irrigationStatus.NextRead )
				{
					_log.Info( "Cicle of read" );

					ForceRead();

					if(_irrigationStatus.Average >= _conf.MinValueForIrrigation )
					{
						_log.Info( "Forcing irrigation from cycle" );
						ForceIrrigation();

						Thread.Sleep( _conf.MinutesIrrigation * 60 * 1000 );	// Wait
					}
				}

				Thread.Sleep( 5000 );
			}
		}

		public void ForceRead()
		{
			if( _irrigationStatus.Status == STANDBY )
			{
				try
				{
					_log.Info("Reading sensors");

					_irrigationStatus.Status = READING;
					_saveStatus();
					// 1.- Open transistor in order give power supply to moisture sensors
					_gpioController.Write( READTRANSISTORPIN, PinValue.High );

					string notes = "";

					// 2.- Make reads for each sensors
					for( int i = 0; i < 4; i++ )
					{
						var sensorValue = _readSensor( i );

						switch( i )
						{
							case 0:
								_irrigationStatus.S1 = sensorValue;
								break;
							case 1:
								_irrigationStatus.S2 = sensorValue;
								break;
							case 2:
								_irrigationStatus.S3 = sensorValue;
								break;
							case 3:
								_irrigationStatus.S4 = sensorValue;
								break;
						}

						_log.Info( $"Value of sensor {i + 1}: {sensorValue}" );

						if( sensorValue > NOTONEARTHTHRESHOLD ) // Sensor is not on earth
						{
							notes = notes + $"Sensor {i + 1} is not on earth | ";

							_irrigationStatus.SetSensorStatus( i, true );
						}
						else if( sensorValue < NOTCONNECTEDTHRESOHLD ) // Sensor is not connected
						{
							notes = notes + $"Sensor {i + 1} is not connected | ";
							_irrigationStatus.SetSensorStatus( i, true );
						}
						else
						{
							_irrigationStatus.SetSensorStatus( i, false );
						}
					}

					_irrigationStatus.Notes = notes.Length > 0 ? notes.Substring( 0, notes.Length - 3 ) : "";

					// 3.- Close Transistor
					_gpioController.Write( READTRANSISTORPIN, PinValue.Low );

					// 4.- Update status
					_irrigationStatus.Status = STANDBY;
					_irrigationStatus.LastRead = DateTime.Now;
					_irrigationStatus.NextRead = DateTime.Now.AddHours((double) _conf.CheckMoistureAftertNotIrrigationHours );
				}
				catch( Exception ex )
				{
					_log.Warn( $"Error during reading sensors: {ex.Message}" );

					_gpioController.Write( READTRANSISTORPIN, PinValue.Low );
					_irrigationStatus.Status = STANDBY;
				}
				_saveStatus();

				_log.Info( "End read" );
			}
			else
			{
				_log.Info( $"Avoid read because status is {_irrigationStatus.Status}" );
			}
		}

		private int _readSensor( int sensor)
		{
			int v = 0;

			for( int i = 0; i < 10; i++ )
			{
				var data = _mcp3208.Read( sensor );

				_log.Debug( $"Read sensor {sensor} try {i} = {data}" );
				v += data;

				Thread.Sleep( 100 );
			}

			return v / 10;
		}

		public void ForceIrrigation()
		{
			if( _irrigationStatus.Status == STANDBY )
			{
				_log.Info( "Starting irrigation" );

				Task.Factory.StartNew( () =>
				{
					try
					{
						_irrigationStatus.Status = IRRIGATING;
						_saveStatus();

						_gpioController.Write( IRRIGATIONPIN, PinValue.High );

						Thread.Sleep( _conf.MinutesIrrigation * 60 * 1000 );

						_gpioController.Write( IRRIGATIONPIN, PinValue.Low );

						_irrigationStatus.Status = STANDBY;
						_irrigationStatus.LastIrrigation = DateTime.Now;
						_irrigationStatus.NextRead = DateTime.Now.AddHours( _conf.CheckMoistureAfterIrrigationHours );
					}
					catch( Exception ex )
					{
						_log.Warn( $"Error during irrigation: {ex.Message}" );

						_gpioController.Write( IRRIGATIONPIN, PinValue.Low );

						_irrigationStatus.Status = STANDBY;
					}
					_saveStatus();
					_log.Info( "End irrigation" );

				} );
			}
			else
			{
				_log.Info( $"Avoid irrigating because status is {_irrigationStatus.Status}" );
			}

		}
	}
}