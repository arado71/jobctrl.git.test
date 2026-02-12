using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.Text;
using Tct.ActivityRecorderService;
using Tct.ActivityRecorderService.Proxy;
using Tct.ActivityRecorderService.SilverLight;

namespace ActivityRecorderServiceHost
{
	class Program
	{
		static void Main(string[] args)
		{
			bool isProxyMode;
			try
			{
				var configValue = ConfigurationManager.AppSettings["ProxyMode"];
				isProxyMode = configValue != null && bool.Parse(configValue);
			}
			catch (Exception ex)
			{
				Console.WriteLine("An exception occured: {0}", ex.Message);
				isProxyMode = false;
			}

			using (ServiceHost serviceHostSl = new ServiceHost(typeof(CrossDomainService)))
			using (ServiceHost serviceHost = new ServiceHost(isProxyMode ? typeof(ProxyService) : typeof(ActivityRecorderService)))
			{
				try
				{
					serviceHostSl.Open();
					Console.WriteLine("The cross domain service is ready.");
					serviceHost.Open();
					Console.WriteLine("The JobCTRL service v" + ActivityRecorderService.Version + " is ready."); //make sure cctor is called so timers are started
					Console.WriteLine("Press <ENTER> to terminate services.");
					Console.WriteLine();
					Console.ReadLine();

					// Close the ServiceHostBase to shutdown the service.
					serviceHost.Close();
					serviceHostSl.Close();
				}
				catch (CommunicationException ce)
				{
					Console.WriteLine("An exception occured: {0}", ce.Message);
					serviceHost.Abort();
					serviceHostSl.Abort();
				}
			}
		}
	}
}
