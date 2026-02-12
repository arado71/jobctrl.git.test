using JobCTRL.Plugins;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.ServiceModel.Web;
using System.Threading;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.ProcessInfo;
using Tct.ActivityRecorderClient.Capturing.SoftphonePro;
using Tct.ActivityRecorderClient.Notification;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	public class PluginSoftphonePro : ICaptureExtension
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		//https://www.iana.org/assignments/service-names-port-numbers/service-names-port-numbers.xhtml
		//https://en.wikipedia.org/wiki/List_of_TCP_and_UDP_port_numbers
		private static readonly string softphoneServiceDefaultPort = "80/Temporary_Listen_Addresses";
		private static readonly string softphoneServiceElevatedPort = "36363";
		private static readonly string softphoneServiceEndpoint = "http://localhost:{Port}/SoftphonePro";
		private const string PluginId = "JobCTRL.SoftphonePro";
		private const string StateKeyName = "CallState";
		private const string NumberKeyName = "CallingNumber";
		private static SoftphoneService softphoneServiceInstance = SoftphoneService.Instance;
		private static SoftphoneConfigInjector softphoneInjector = SoftphoneConfigInjector.Instance;
		private WebServiceHost serviceHost;
		private static string endpointWithPort;
		private static DateTime lastConfigCheck;
		private readonly object lockObject = new object();
		private readonly SynchronizationContext guiContext = Platform.Factory.GetGuiSynchronizationContext();
		private readonly INotificationService notificationService = Platform.Factory.GetNotificationService();
		private const string MessageKey = "SoftphoneRestartMessage";
		private static int SoftphoneProcCount;
		private static bool IsNotificationShown;

		public string Id
		{
			get { return PluginId; }
		}

		public PluginSoftphonePro()
		{
		}

		public IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName)
		{
			lock (lockObject)
			{
				if ((DateTime.Now - lastConfigCheck).TotalHours > 1)
				{
					log.Debug("Softphone config check started");
					if (ProcessElevationHelper.IsElevated())
					{
						endpointWithPort = softphoneServiceEndpoint.Replace("{Port}", softphoneServiceElevatedPort);
					}
					else
					{
						endpointWithPort = softphoneServiceEndpoint.Replace("{Port}", softphoneServiceDefaultPort);
					}

					if (softphoneInjector.IsConfigurationUpdateRequired(endpointWithPort))
					{
						var currentSession = Process.GetCurrentProcess().SessionId;
						var softphoneProcesses = Process.GetProcessesByName("SoftphonePro").Where(p => p.SessionId == currentSession).ToArray();
						if (softphoneProcesses.Length > 0)
						{
							Interlocked.Exchange(ref SoftphoneProcCount, softphoneProcesses.Length);
							log.DebugFormat("{0} running Softphone instance(s) found", SoftphoneProcCount);
							foreach (var proc in softphoneProcesses)
							{
								proc.EnableRaisingEvents = true;
								proc.Exited += SoftphoneOnExited;
							}
							guiContext.Post(
									_ =>
										notificationService.ShowNotification(MessageKey, TimeSpan.Zero,
											Labels.Notification_SoftphonePluginMessageTitle, Labels.Notification_SoftphonePluginMessageBody,
											Color.Crimson), null);
							IsNotificationShown = true;
							log.Debug(MessageKey + " is shown");
						}
						else
						{
							softphoneInjector.UpdateConfiguration(endpointWithPort);
						}
					}
					lastConfigCheck = DateTime.Now;
				} 
			}

			if (!string.Equals(processName, "SoftphonePro.exe", StringComparison.OrdinalIgnoreCase)) return null;

			lock (lockObject)
			{
				if (!softphoneServiceInstance.Started)
				{
					StartSoftphoneService();
				} 
			}

			return new Dictionary<string, string>
			{
				{StateKeyName, softphoneServiceInstance.GetStateString()},
				{NumberKeyName, softphoneServiceInstance.GetCallerNumber()}
			};
		}

		public IEnumerable<string> GetCapturableKeys()
		{
			yield return StateKeyName;
			yield return NumberKeyName;
		}

		public IEnumerable<string> GetParameterNames()
		{
			return Enumerable.Empty<string>();
		}

		public void SetParameter(string name, string value)
		{
		}

		private void StartSoftphoneService()
		{
			try
			{
				serviceHost = new WebServiceHost(softphoneServiceInstance, new Uri(endpointWithPort));
				serviceHost.Open();
				softphoneServiceInstance.Started = true;
				log.Info("SoftphoneService started successfully. Endpoint: " + endpointWithPort);
			}
			catch (InvalidOperationException ex)
			{
				log.Warn("SoftphoneService already listen on the given port.");
			}
			catch (Exception ex)
			{
				log.Error("Error occured while starting SoftphoneService.", ex);
			}
		}

		private void SoftphoneOnExited(object sender, EventArgs eventArgs)
		{
			var proc = sender as Process;
			Debug.Assert(proc != null);
			proc.Exited -= SoftphoneOnExited;
			Interlocked.Decrement(ref SoftphoneProcCount);
			log.DebugFormat("SoftphonPro instance is closed. Remaining: {0}", SoftphoneProcCount);
			if (SoftphoneProcCount == 0)
			{
				if (IsNotificationShown)
				{
					guiContext.Post(_ => notificationService.HideNotification(MessageKey), null);
					IsNotificationShown = false;
					log.DebugFormat(MessageKey + " is closed");
				}

				softphoneInjector.UpdateConfiguration(endpointWithPort);
			}
		}
	}
}
