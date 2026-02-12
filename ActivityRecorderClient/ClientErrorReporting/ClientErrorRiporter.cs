using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Plugins.Impl.Chrome;
using Tct.ActivityRecorderClient.Capturing.Plugins.Impl.Firefox;
using Tct.ActivityRecorderClient.Communication;
using Tct.ActivityRecorderClient.View;
using Timer = System.Threading.Timer;

namespace Tct.ActivityRecorderClient.ClientErrorReporting
{
	public abstract class ClientErrorReporter : IErrorReporter
	{
		private readonly ILog log;
		private readonly int guiThreadId;

		private const int maxChunkSize = 64 * 1024;
		private const int retryDelayInMilliseconds = 10000;
		private const int delayUnitInMilliseconds = 100;

		public ClientErrorReporter()
		{
			log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		}

		protected ClientErrorReporter(ILog log)
		{
			guiThreadId = Thread.CurrentThread.ManagedThreadId;
			this.log = log;
		}

		public abstract void LogSystemInfo();

		public abstract bool IsFromMessageLoop { get; }

		public bool ReportClientError(string description, bool attachLogs, Action<ReportingProgress> reportProgress = null, Func<bool> getCancellationPending = null)
		{
			LogSystemInfo();
			log.InfoFormat("Sending client error report. Logs: {0}, Description: {1}", attachLogs, !string.IsNullOrEmpty(description) ? (Environment.NewLine + description) : "");
			try
			{
				var ver = ConfigManager.Version;
				var enabledFeatures = string.Join(",", FeatureSwitches.GetEnabledFeatures().ToArray());
				var clientError = new ClientComputerError()
				{
					ClientId = Guid.NewGuid(),
					UserId = ConfigManager.UserId,
					ComputerId = ConfigManager.EnvironmentInfo.ComputerId,
					Major = ver.Major,
					Minor = ver.Minor,
					Build = ver.Build,
					Revision = ver.Revision,
					Description = description,
					Features = ConfigManager.Classifier != null ? string.Format("({0}) {1}", ConfigManager.Classifier, enabledFeatures) : enabledFeatures,
					HasAttachment = attachLogs,
					Data = null,
					Offset = 0,
					IsCompleted = false,
					IsCancelled = false,
					NeedCancelOnServer = false,
				};

				if (reportProgress != null) reportProgress(GetProgress(0, ""));
				byte[] logData = null;
				if (attachLogs)
				{
					string[] logFiles;
					int folderOffset;
					GetLogData(out logFiles, out folderOffset);

					log.Info("Attaching log files to client error riport:" + Environment.NewLine + string.Join(Environment.NewLine, logFiles));

					using (var stream = new MemoryStream())
					{
						var progressAction = reportProgress != null ? (s, i) => reportProgress(GetProgress(i, s)) : default(Action<string, int>);
						CompressHelper.WriteZipData(stream, logFiles, folderOffset, progressAction, getCancellationPending);
						logData = stream.ToArray();
					}
				}
				if (IsCancelled(getCancellationPending, clientError)) return false;

				if (reportProgress != null) reportProgress(GetProgress(0));
				while (!clientError.IsCompleted)
				{
					if (attachLogs)
					{
						var length = Math.Min(maxChunkSize, logData.Length - clientError.Offset);
						if (clientError.Data == null || clientError.Data.Length != length) clientError.Data = new byte[length];
						Array.Copy(logData, clientError.Offset, clientError.Data, 0, length);
						clientError.IsCompleted = logData.Length == clientError.Offset + length;
					}
					else
					{
						clientError.IsCompleted = true;
					}

					var success = false;
					while (!success)
					{
						if (IsCancelled(getCancellationPending, clientError)) return false;
						try
						{
							clientError.NeedCancelOnServer = true; //We have to signal the server about cancellation from now on.
							ActivityRecorderClientWrapper.Execute(n => n.ReportClientError(clientError));
							success = true;
						}
						catch (Exception ex)
						{
							WcfExceptionLogger.LogWcfError("report clienterror", log, ex);
							var start = Environment.TickCount;
							while (Environment.TickCount - start < retryDelayInMilliseconds)
							{
								Thread.Sleep(delayUnitInMilliseconds);
								if (IsCancelled(getCancellationPending, clientError)) return false;
							}
						}
					}
					if (attachLogs)
					{
						clientError.Offset += clientError.Data.Length;
						clientError.Description = null; //We have to send description only in the first chunk.
					}
					if (reportProgress != null) reportProgress(GetProgress(attachLogs ? (100 * clientError.Offset / logData.Length) : 100));
				}
			}
			catch (Exception ex)
			{
				log.ErrorAndFail("ReportClientError failed", ex);
				throw;
			}

			log.Info("ReportClientError finished successfully.");
			return true;
		}

		private void GetLogData(out string[] logFiles, out int folderOffset)
		{
			try
			{
				var dir = ConfigManager.LogPath;
				var searchDirs = new List<string>()
				{
					"Logs",
					@"OutlookSync\Logs", @"LotusNotesSync\Logs",
					Path.Combine(FirefoxInstallHelper.xpiDirName, "Logs"),
					Path.Combine(ChromiumInstallHelperBase.NativeHostDir, "Logs")
				};
				var searchPatterns = new List<string>() { "*.log.*" }; // it matches with both JobCTRL.log and JobCTRL.log.1
				logFiles =
					searchDirs
					.Select(n => Path.Combine(dir, n))
					.Where(n => Directory.Exists(n))
					.SelectMany(x => searchPatterns
						.SelectMany(y => Directory.GetFiles(x, y, SearchOption.TopDirectoryOnly)))
					.ToArray();
				folderOffset = dir.Length + (dir.EndsWith("\\") ? 0 : 1);
			}
			catch (Exception ex)
			{
				log.ErrorAndFail("Unable to get log data", ex);
				throw;
			}
		}

		private static ReportingProgress GetProgress(int value, string compressingFileName = null)
		{
			var text = compressingFileName != null
				? (Labels.ErrorReporting_ProgressBody_Preparing + (!string.IsNullOrEmpty(compressingFileName) ? (" - " + compressingFileName) : ""))
				: Labels.ErrorReporting_ProgressBody_Uploading;

			return new ReportingProgress
			{
				NumberOfPhases = 2,
				CurrentPhase = compressingFileName != null ? 1 : 2,
				PhaseText = text,
				Value = value
			};
		}

		private bool IsCancelled(Func<bool> getCancellationPending, ClientComputerError clientError)
		{
			if (getCancellationPending == null || !getCancellationPending()) return false;
			log.Info("ReportClientError cancelled.");
			if (clientError.NeedCancelOnServer) ScheduleTask(() => CancelClientError(clientError), 0);
			return true;
		}

		private void CancelClientError(ClientComputerError clientError)
		{
			try
			{
				clientError.IsCancelled = true;
				clientError.IsCompleted = true;
				clientError.Data = null;
				ActivityRecorderClientWrapper.Execute(n => n.ReportClientError(clientError));
				log.Info("Client error cancellation sent succesfully to server.");
			}
			catch (Exception ex)
			{
				WcfExceptionLogger.LogWcfError("report cancelled clienterror", log, ex);
				ScheduleTask(() => CancelClientError(clientError), retryDelayInMilliseconds);
			}
		}

		private void ScheduleTask(Action action, int delayInMilliseconds)
		{
			var timer = new Timer(self =>
			{
				try
				{
					action();
				}
				finally
				{
					((Timer)self).Dispose();
				}
			});
			timer.Change(delayInMilliseconds, Timeout.Infinite);
		}

	}
}
