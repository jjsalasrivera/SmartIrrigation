using System.Reflection;
using GenHTTP.Core;
using GenHTTP.Modules.Core;
using log4net;
using GenHTTP.Modules.Webservices;


namespace SmartIrrigation.WebServer
{
	public class WebService
    {
        private static readonly ILog _log = LogManager.GetLogger( MethodBase.GetCurrentMethod().DeclaringType );

        private readonly Irrigationer _irrigationer;
		private const string HtmlRootPath = "rootPath";


        public WebService( Irrigationer irrigationer )
        {
            _irrigationer = irrigationer;
        }

        public int InitServer()
		{
            //var api = Layout.Create().Add<WebIrrigationController>( "api" );

            // var server = Server.Create().Handler( api );

            var webRoot = Layout.Create()
              .Index( Download.FromFile( @$"./{HtmlRootPath}/index.html" ) )
              .Fallback( Static.Files( @$"./{HtmlRootPath}" ) )
              .Add<WebIrrigationController>( "api" );

            //var site = Website.Create().Content( webRoot );

            return Host.Create()
                .Console()
                .Defaults()
                .Port( 8080 )
				.Handler( webRoot )
                .Run();
           
        }
    }
}