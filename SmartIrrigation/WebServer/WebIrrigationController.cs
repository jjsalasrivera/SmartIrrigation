using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Webservices;
using System;

namespace SmartIrrigation.WebServer
{
	public class WebIrrigationController
	{

		[Method(RequestMethod.GET, "last-values")]
		public IrrigationStatus LastValue()
		{
			return Irrigationer.Instance.GetStatus();
		}

		[Method(RequestMethod.POST, "force-read")]
		public ResponseStatus ForceRead()
		{
			Irrigationer.Instance.ForceRead();

			return ResponseStatus.OK;
		}

		[Method(RequestMethod.POST, "force-irrigation")]
		public ResponseStatus ForceIrrigation()
		{
			Irrigationer.Instance.ForceIrrigation();

			return ResponseStatus.OK;
		}
	}
}