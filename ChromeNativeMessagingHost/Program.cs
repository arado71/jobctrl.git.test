using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading;
using log4net;
using Microsoft.Win32;

namespace NativeMessagingHost
{
	class Program
	{
		private const string ContractName = "NativeMessagingHost.IChromeCaptureService";
#if FirefoxMode
		private const string ServiceEndpointAddress = "net.pipe://localhost/FirefoxCaptureService";
#else
		private const string ServiceEndpointAddress = "net.pipe://localhost/{0}CaptureService";
#endif
		private static readonly ManualResetEvent waitHandle = new ManualResetEvent(false);
		private static ILog log;
		public static Version Version = Assembly.GetExecutingAssembly().GetName().Version;

		static void Main(string[] args)
		{
			var parentProcess = ParentProcessUtilities.GetParentProcess();
			while (parentProcess?.ProcessName == "cmd")
				parentProcess = ParentProcessUtilities.GetParentProcess(parentProcess);
			Logger.Setup(parentProcess?.ProcessName);
			log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
			try
			{
				log.InfoFormat("Applicationversion: {0}", Version);
#if FirefoxMode
				var registryPath = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\App Paths\firefox.exe", "", null);
				var mess = "Firefox version: {0}";
#else
				var registryPath = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\App Paths\chrome.exe", "", null);
				var mess = "Chrome version: {0}";
#endif
				if (registryPath != null)
				{
					try
					{
						log.InfoFormat(mess, FileVersionInfo.GetVersionInfo(registryPath.ToString()).FileVersion);
					} catch (FileNotFoundException)
					{
						log.WarnFormat(mess, "Bad file location information in Registry.");
					}
				} 

				log.InfoFormat("Parent process name: {0}", parentProcess?.ProcessName);
				log.Info("Initializing communication...");
				var communication = new ExtensionCommunication();
				communication.ReceiveError += ErrorReceived;
				log.Debug("Setting up service host...");
				var serviceInstance = new ChromeCaptureService(communication, () => waitHandle.Set());
				using (var serviceHost = new ServiceHost(serviceInstance))
				{
					try
					{
						serviceHost.Description.Endpoints.Clear(); //use programmatic endpoint only
						serviceHost.AddServiceEndpoint(ContractName, new NetNamedPipeBinding(),
							string.Format(ServiceEndpointAddress, parentProcess?.ProcessName == "msedge" ? "Edge" : "Chrome"));

						var debug = serviceHost.Description.Behaviors.Find<ServiceDebugBehavior>();
						debug.IncludeExceptionDetailInFaults = true;

						//Mex have to be disabled to work with chrome
						//var metad = new ServiceMetadataBehavior { HttpGetEnabled = true, HttpGetUrl = new Uri("http://localhost/ChromeCaptureService/mex") };
						//serviceHost.Description.Behaviors.Add(metad);
						serviceHost.Open();

						log.Debug("Service host set up.");
						waitHandle.WaitOne();

						serviceHost.Close(TimeSpan.FromMilliseconds(10));
					}
					catch (CommunicationException ce)
					{
						log.Fatal("An error occured, terminating service host.", ce);
						serviceHost.Abort();
						throw;
					}
				}
			}
			catch (Exception e)
			{
				log.Error("An error occured.", e);
			}
		}

		static void ErrorReceived(object sender, EventArgs eventArgs)
		{
			log.InfoFormat("Null message received, terminating.");
			waitHandle.Set();
		}
	}
}
