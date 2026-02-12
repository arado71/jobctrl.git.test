using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using log4net;
using Microsoft.Office.Interop.Outlook;
using Redemption;
using Tct.ActivityRecorderClient.Capturing.Mail;
using Exception = System.Exception;
using Win32Process;
using System.Management;

namespace OutlookInteropService
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
	public class OutlookMailCaptureService : IOutlookMailCaptureService
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private readonly OutlookVersion olVersion;
		private Application application;
		private OutlookCaptureLib captureLib;
		private MailTrackingType mailTrackingType;
		private MailTrackingSettings mailTrackingSettings;
		private readonly object lockObject = new Object();
		private int lastPid;
		private bool isElevationChangeReqWhileInit;

		public OutlookMailCaptureService(MailTrackingType trackingType, MailTrackingSettings trackingSettings)
		{
			//load redemption
			//var item = RedemptionLoader.new_SafeMailItem(); //this will show a popup if no outlook is installed
			//Marshal.ReleaseComObject(item);
			mailTrackingType = trackingType;
			mailTrackingSettings = trackingSettings;
			olVersion = OutlookSettingsHelper.OutlookVersion;
			log.Info("Outlook version is " + olVersion);
			var outlookApp = new ProcessInfo("outlook.exe");
			outlookApp.Started += AppStarted;
			outlookApp.Terminated += AppTerminated;
			if (Process.GetProcessesByName("outlook").Any(p =>
			{
				try
				{
					return !p.HasExited;
				}
				catch (Win32Exception ex)
				{
					if ((uint)ex.ErrorCode != 0x80004005) throw;
					// Access denied: existing process is elevated
					log.Debug("Elevation change required");
					isElevationChangeReqWhileInit = true;
					return false;
				}
			}))
			{
				log.Debug("StaSend InitApp in ctor");
				StaSynchronizationContext.Current.Post(InitApp);
			}
		}

		private void AppStarted(object sender, EventArrivedEventArgs e)
		{
			if (!(e.NewEvent.Properties["TargetInstance"].Value is ManagementBaseObject targetInstance)) return;
			var pid = Convert.ToInt32(targetInstance.Properties["ProcessID"].Value);
			log.Debug("AppStarted " + targetInstance.Properties["CommandLine"].Value + $" pid: {pid}");
			if (pid == lastPid || targetInstance.Properties["CommandLine"].Value is string cmdLine && cmdLine.Contains("-Embedding")) return;
			lastPid = pid;
			log.Debug("StaSend InitApp");
			StaSynchronizationContext.Current.Send(InitApp);
		}

		private void InitApp()
		{
			log.Debug("InitApp started");
			try
			{
				log.Debug("Creating Application interop object...");
				application = new Application();
				log.Debug("Created Application interop object");
				captureLib = new OutlookCaptureLib(application, SafeHandlerExtension.GetHandle, StaSynchronizationContext.Current.Send, true) { TrackingType = mailTrackingType, TrackingSettings = mailTrackingSettings };
				log.Info("Application prepared");
			}
			catch (Exception ex)
			{
				if (ex is COMException comEx && comEx.ErrorCode == -2146959355)
				{
					log.Debug("Elevation change required");
					isElevationChangeReqWhileInit = true;
					return;
				}
				log.Error("InitApp failed", ex);
			}
		}

		private void AppTerminated(object sender, EventArrivedEventArgs e)
		{
			if (!(e.NewEvent.Properties["TargetInstance"].Value is ManagementBaseObject targetInstance)) return;
			var pid = Convert.ToInt32(targetInstance.Properties["ProcessID"].Value);
			log.Debug("AppTerminated " + targetInstance.Properties["CommandLine"].Value + $" pid: {pid}");
			if (targetInstance.Properties["CommandLine"].Value is string cmdLine && cmdLine.Contains("-Embedding")) return;
			StaSynchronizationContext.Current.Send(() =>
			{
				captureLib?.ReleaseApp(false);
				if (application != null)
				{
					Marshal.ReleaseComObject(application);
					application = null;
					log.Info("Application released");
				}
			});
		}

		public MailCaptures GetMailCaptures()
		{
			if (captureLib != null && !isElevationChangeReqWhileInit)
			{
				try
				{
					captureLib.LastHeartbeat = DateTime.UtcNow;
					return captureLib.GetMailCaptures();
				}
				catch (COMException)
				{
					// Do nothing
				}
				catch (Exception ex)
				{
					log.Error("GetMailCaptures failed", ex);
					return new MailCaptures { IsSafeMailItemCommitUsable = OutlookWindowWrapper.IsSafeMailItemCommitUsable };
				}
			}
			else 
				if (!isElevationChangeReqWhileInit)
					return new MailCaptures { IsSafeMailItemCommitUsable = OutlookWindowWrapper.IsSafeMailItemCommitUsable };

			if (RunningObjectTableHelper.EnsureRotRegistration("outlook.exe", out var isElevationChangeRequired, out var isOutlookElevated))
			{
				isElevationChangeReqWhileInit = false;
				if (isElevationChangeRequired)
				{
					log.Info("Elevation change is needed");
					throw new FaultException(isOutlookElevated ? "Elevate" : "Unelevate");
				}
			}

			return null;
		}

		public void StopService()
		{
			log.Info("StopService called");
			var sw = Stopwatch.StartNew();
			try
			{
				captureLib?.Dispose();
				ServiceHostBase host = OperationContext.Current.Host;
				var sc = SynchronizationContext.Current;
				ThreadPool.QueueUserWorkItem(_ =>
				{
					Thread.Sleep(50);
					sc.Post(__ => host.Close(), null);
				}); //With .NET 4.0 simple post doesn't worked.
			}
			catch (Exception ex)
			{
				log.Error("StopService failed", ex);
				throw;
			}
			finally
			{
				log.InfoFormat("StopService finished in {0:0.000}ms.", sw.Elapsed.TotalMilliseconds);
			}
		}

		public void SetMailTracking(MailTrackingType trackingType, MailTrackingSettings trackingSettings, bool isSafeMailItemCommitUsable)
		{
			if (captureLib == null)
			{
				mailTrackingType = trackingType;
				mailTrackingSettings = trackingSettings;
				log.Debug("IsSafeMailItemCommitUsable: " + isSafeMailItemCommitUsable);
				OutlookWindowWrapper.IsSafeMailItemCommitUsable = isSafeMailItemCommitUsable;
				return;
			}
			captureLib.TrackingType = trackingType;
			captureLib.TrackingSettings = trackingSettings;
			if (OutlookWindowWrapper.IsSafeMailItemCommitUsable != isSafeMailItemCommitUsable)
			{
				log.Debug("IsSafeMailItemCommitUsable: " + isSafeMailItemCommitUsable);
				OutlookWindowWrapper.IsSafeMailItemCommitUsable = isSafeMailItemCommitUsable;
			}
		}
	}
}
