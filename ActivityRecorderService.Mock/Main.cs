using System;
using System.ServiceModel;

namespace ActivityRecorderService.Mock
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			using (ServiceHost serviceHost = new ServiceHost(typeof(ActivityRecorderMockService)))
			{
				try
				{
					serviceHost.Open();
					Console.WriteLine("The JobCTRL MOCK service v" + ActivityRecorderMockService.Version + " is ready."); //make sure cctor is called so timers are started
					Console.WriteLine("Press <ENTER> to terminate services.");
					Console.WriteLine();
					Console.ReadLine();

					// Close the ServiceHostBase to shutdown the service.
					serviceHost.Close();
				}
				catch (CommunicationException ce)
				{
					Console.WriteLine("An exception occured: {0}", ce.Message);
					serviceHost.Abort();
				}
			}
		}
	}
}
