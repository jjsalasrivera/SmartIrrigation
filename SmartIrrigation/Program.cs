using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Threading;
using log4net;
using log4net.Config;
using Newtonsoft.Json;
using SmartIrrigation.WebServer;

namespace SmartIrrigation
{
    class Program
    {
        //private static string ModuleName = Assembly.GetExecutingAssembly().GetName().Name;
        private static readonly ILog _log = LogManager.GetLogger( MethodBase.GetCurrentMethod().DeclaringType );

        static void Main(string[] args)
        {
            _initLog4Net();
            _log.Info( "Initializing..." );

            SmartIrrigationConf conf = null;
            try
            {
                var cfgName = $"{Assembly.GetExecutingAssembly().GetName().Name}.json";
                conf = JsonConvert.DeserializeObject<SmartIrrigationConf>( File.ReadAllText( cfgName ) );
            }
            catch( Exception ex )
            {
                _log.Error( $"Fatal Error reading conf {ex.Message}" );
                Environment.Exit( -1 );
            }

            var irrigationer = Irrigationer.Instance;
            var webService = new WebService( irrigationer );

            irrigationer.Start( conf );
			webService.InitServer();

            _log.Info( "Initialized" );

            Thread.CurrentThread.Join();

            irrigationer.Stop();
        }

        private static void _initLog4Net()
        {
            Assembly EntryAssembly = Assembly.GetEntryAssembly();
            string AppPath = string.Format( "{0}{1}", Path.GetDirectoryName( EntryAssembly.Location ), Path.DirectorySeparatorChar );
            Directory.SetCurrentDirectory( AppPath );
            var logRepository = LogManager.GetRepository( Assembly.GetEntryAssembly() );
            XmlConfigurator.Configure( logRepository, new FileInfo( ConfigurationManager.OpenExeConfiguration( ConfigurationUserLevel.None ).FilePath ) );

            _log.Info( AppPath + ", " + Environment.CurrentDirectory );
            _log.Info( "Initializating application" );
        }
    }
}
