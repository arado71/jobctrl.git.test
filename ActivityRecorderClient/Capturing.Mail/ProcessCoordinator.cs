using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using log4net;
using OutlookInteropService;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.ProcessInfo;
using Tct.ActivityRecorderClient.Communication;
using Tct.ActivityRecorderClient.Configuration;

namespace Tct.ActivityRecorderClient.Capturing.Mail
{
	public class ProcessCoordinator
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ProcessStarter processStarter;
		private readonly object thisLock = new object();
		private int referenceCount;

		static ProcessCoordinator()
		{
			Environment.SetEnvironmentVariable("JOBCTRL_LOGPATH", ConfigManager.LogPath);
			Environment.SetEnvironmentVariable("JOBCTRL_LOGPATH", ConfigManager.LogPath, EnvironmentVariableTarget.User);
		}

		private ProcessCoordinator(ProcessStarter processStarter)
		{
			this.processStarter = processStarter;
		}

		public void Start()
		{
			lock (thisLock)
			{
				if (referenceCount == 0 || !processStarter.IsServiceRunning)
				{
					processStarter.StartProcess();
				}
				referenceCount++;
			}
		}

		public void Stop()
		{
			lock (thisLock)
			{
				referenceCount--;
				if (referenceCount == 0)
				{
					processStarter.StopProcess();
				}
			}
		}

		public void RestartIfNeeded()
		{
			if (Core.CaptureCoordinator.IsStopping)
			{
				log.Info("Skip restart as we are shutting down");
				return;
			}
			lock (thisLock)
			{
				if (!processStarter.IsServiceRunning)
				{
					if (Core.CaptureCoordinator.IsStopping) //double check
					{
						log.Info("Skip restart 2 as we are shutting down");
						return;
					}
					log.Info("Could not find endpoint restarting process...");
					processStarter.StopProcess();
					processStarter.StartProcess();
					OnProcessRestarted();
				}
			}
		}

		public void ChangeElevationLevel(bool elevate)
		{
			lock (thisLock)
			{
				if (elevate == processStarter.IsElevated || processStarter.ElevationCancelled) return;

				log.Info((elevate ? "Elevating" : "Unelevating") + " process...");
				processStarter.StopProcess();
				processStarter.StartProcess(elevate);
				OnProcessRestarted();
			}
		}

		public event EventHandler ProcessRestarted;

		private void OnProcessRestarted()
		{
			var del = ProcessRestarted;
			if (del != null) del(this, EventArgs.Empty);
		}

		public static ProcessCoordinator LotusNotesProcessCoordinator = new ProcessCoordinator(new ProcessStarter(
			Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "LotusNotesSync\\JC.Meeting.exe"),
			new[] { ConfigManager.CurrentProcessPid.ToString(CultureInfo.InvariantCulture) },
			TimeSpan.FromMilliseconds(30000),
			() => { using (var client = new LotusNotesMeetingCaptureClientWrapper()) ((ICommunicationObject)client.Client).Open(TimeSpan.FromMilliseconds(500)); },
			() => { using (var client = new LotusNotesMeetingCaptureClientWrapper()) client.Client.StopService(); }));

		public static ProcessCoordinator OutlookMeetingProcessCoordinator = new ProcessCoordinator(new ProcessStarter(
			Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), String.Format("OutlookSync\\JC.Meeting{0}.exe", OutlookSettingsHelper.OutlookBitness == Bitness.X64 ? "64" : "")),
			new[] { ConfigManager.CurrentProcessPid.ToString(CultureInfo.InvariantCulture), "{0}-" + ConfigManager.UserId, AppConfig.Current.OutlookMeetingCaptureClientTimeout.HasValue ? ((int)AppConfig.Current.OutlookMeetingCaptureClientTimeout.Value.TotalSeconds + 60).ToString() : string.Empty },
			TimeSpan.FromMilliseconds(30000),
			() => { using (var client = new OutlookMeetingCaptureClientWrapper(null)) ((ICommunicationObject)client.Client).Open(TimeSpan.FromMilliseconds(500)); },
			() => { using (var client = new OutlookMeetingCaptureClientWrapper(TimeSpan.FromMilliseconds(10000))) client.Client.StopService(); }));

		public static ProcessCoordinator OutlookMailProcessCoordinator = new ProcessCoordinator(new ProcessStarter(
			Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), String.Format("OutlookSync\\JC.Mail{0}.exe", OutlookSettingsHelper.OutlookBitness == Bitness.X64 ? "64" : "")),
			new[] { ConfigManager.CurrentProcessPid.ToString(CultureInfo.InvariantCulture), ((int)ConfigManager.MailTrackingType).ToString(), ((int)ConfigManager.MailTrackingSettings).ToString() },
			TimeSpan.FromMilliseconds(10000),
			() => { using (var client = new OutlookMailCaptureClientWrapper()) ((ICommunicationObject)client.Client).Open(TimeSpan.FromMilliseconds(500)); },
			() => { using (var client = new OutlookMailCaptureClientWrapper()) client.Client.StopService(); }));

		public static ProcessCoordinator JavaCaptureProcessCoordinator = new ProcessCoordinator(new ProcessStarter(
			Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "JavaCapture\\JC.JavaCapture.exe"),
			new[] { ConfigManager.CurrentProcessPid.ToString(CultureInfo.InvariantCulture) },
			TimeSpan.FromMilliseconds(10000),
			() =>
			{
				using (var client = new JavaCaptureClientWrapper())
					((ICommunicationObject) client.Client).Open(TimeSpan.FromMilliseconds(500));
			},
			() =>
			{
				using (var client = new JavaCaptureClientWrapper()) client.Client.StopService();
			}
		));

		private class ProcessStarter
		{
			private readonly string path;
			private readonly string[] args;
			private readonly TimeSpan serviceStartTimeout;
			private readonly Action openService;
			private readonly Action stopService;
			private bool elevationNeeded;

			private Process process;
			private bool hasStartedWithoutProcRef;

			public ProcessStarter(string path, string[] args, TimeSpan serviceStartTimeout, Action openService, Action stopService)
			{
				this.path = path;
				this.args = args;
				this.serviceStartTimeout = serviceStartTimeout;
				this.openService = openService;
				this.stopService = stopService;
				elevationNeeded = ProcessElevationHelper.IsElevated();
			}

			public bool IsElevated
			{
				get { return (process != null || hasStartedWithoutProcRef) && elevationNeeded; }
			}

			public bool ElevationCancelled { get; private set; }

			public void StartProcess()
			{
				StartProcess(false);
			}

			public void StartProcess(bool startElevated)
			{
				Debug.Assert(process == null && !hasStartedWithoutProcRef, process != null ? "Process already exists" : "Process started without procref");
				if (process != null || hasStartedWithoutProcRef) return;

				try
				{
					elevationNeeded = startElevated;
					var startInfo = elevationNeeded == ProcessElevationHelper.IsElevated()
						? new ProcessStartInfo(path, String.Join(" ", args)) { UseShellExecute = false }
						: (elevationNeeded
							? ProcessElevationHelper.GetElevatedProcessStartInfo(path, args)
							: TryStartProcessUnelevated());
					var indirectStartOfProcess = startInfo == null || startInfo.FileName != path;

					if (startInfo != null)
					{ 
						process = Process.Start(startInfo); 
						if (indirectStartOfProcess)
						{
							process.Dispose();
							process = null;
						}
					}

					hasStartedWithoutProcRef = indirectStartOfProcess;

					log.InfoFormat("Process started successfully. ({0})", ProcessInfo);
				}
				catch (Exception ex)
				{
					if (ex is Win32Exception && (ex as Win32Exception).NativeErrorCode == 1223)
					{
						var ns = Platform.Factory.GetNotificationService();
						var res = ns.ShowMessageBox(
							String.Format(Labels.NotificationExternalCaptureServiceElevationCancelledByUserBody, Path.GetFileName(path)),
							Labels.NotificationExternalCaptureServiceElevationCancelledByUserTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
						ElevationCancelled = res != DialogResult.Yes;
						log.InfoFormat("Starting process was cancelled by user. (Confirmed for this session: {1}) ({0})", ProcessInfo, ElevationCancelled);
						StartProcess(!ElevationCancelled);
						return;
					}
					else
					{
						log.ErrorAndFail(String.Format("Failure of starting process. ({0})", ProcessInfo), ex);
					}
					return;
				}
				if (!WaitForServiceStart((int)serviceStartTimeout.TotalMilliseconds)) log.WarnFormat("Service does not started within the given timeout. ({0}ms) ({1})", (int)serviceStartTimeout.TotalMilliseconds, ProcessInfo);
			}

			private ProcessStartInfo TryStartProcessUnelevated()
			{
				try
				{
					ProcessElevationHelper.StartProcessUnelevated(path, args);
					return null;
				}
				catch (Exception ex)
				{
					log.Warn($"Unable to unelevate process via token change, trying vbs method... ({ex})");
					return ProcessElevationHelper.GetUnelevatedProcessStartInfo(path, args);
				}
			}

			public void StopProcess()
			{
				Debug.Assert(process != null || hasStartedWithoutProcRef);
				if (process == null && !hasStartedWithoutProcRef) return;

				try
				{
					if (process == null || !process.HasExited)
					{
						bool hasExited = false;
						try
						{
							stopService();
							hasExited = process == null || process.WaitForExit(2000);
						}
						catch (Exception ex)
						{
							WcfExceptionLogger.LogWcfError("stop service", log, ex);
						}

						if (hasExited)
						{
							log.InfoFormat("Process stopped successfully. ({0})", ProcessInfo);
						}
						else if (process != null)
						{
							process.Kill();
							log.InfoFormat("Process failed to stop so we killed it. ({0})", ProcessInfo);
						}
					}
					else
					{
						log.InfoFormat("Process has already been terminated. ({0})", ProcessInfo);
					}
				}
				catch (Exception e)
				{
					log.ErrorAndFail(String.Format("Failure of stopping process. ({0})", ProcessInfo), e);
				}
				finally
				{
					if (process != null)
					{
						process.Dispose();
						process = null;
					}
					hasStartedWithoutProcRef = false;
				}
			}

			public bool IsServiceRunning
			{
				get { return (process != null || hasStartedWithoutProcRef) && WaitForServiceStart(0); }
			}

			private bool WaitForServiceStart(int milliseconds)
			{
				int startTickCount = Environment.TickCount;

				do
				{
					try
					{
						openService();
						return true;
					}
					catch (Exception e)
					{
						log.Verbose("WaitForServiceStart inner ex: ", e);
						if (Core.CaptureCoordinator.IsStopping)
						{
							log.Info("Skip wait as we are shutting down");
							break;
						}
						Thread.Sleep(100);
					}
				} while (milliseconds < 0 || Environment.TickCount - startTickCount < milliseconds);

				return false;
			}

			private string ProcessInfo
			{
				get
				{
					var fullDir = Path.GetDirectoryName(path);
					var directoryName = String.IsNullOrEmpty(fullDir) ? "" : new DirectoryInfo(fullDir).Name;
					var arguments = String.Join(" ", args);
					var sb = new StringBuilder();
					sb.Append(directoryName + Path.DirectorySeparatorChar + Path.GetFileName(path));
					if (!String.IsNullOrEmpty(arguments)) sb.Append(" " + arguments);
					sb.Append(" " + (elevationNeeded ? "(Elevated)" : "(Unelevated)"));
					return sb.ToString();
				}
			}
		}
	}
}
