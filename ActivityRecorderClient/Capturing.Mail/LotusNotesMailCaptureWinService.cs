using System;
using System.IO;
using System.ServiceModel;
using log4net;
using LotusNotesMeetingCaptureServiceNamespace;
using Tct.ActivityRecorderClient.Communication;
using Tct.ActivityRecorderClient.OutlookMailCaptureServiceReference;

namespace Tct.ActivityRecorderClient.Capturing.Mail
{
	public class LotusNotesMailCaptureWinService : IMailCaptureService
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ProcessCoordinator processCoordinator = ProcessCoordinator.LotusNotesProcessCoordinator;

		private bool isLotusNotesInstalled;
		private bool isDisposed;

		public void Initialize()
		{
			isLotusNotesInstalled = LotusNotesSettingsHelper.IsLotusNotesInstalled;
			if (!isLotusNotesInstalled)
			{
				log.Info("LotusNotes is not installed.");
				return;
			}

			processCoordinator.Start();
		}

		public MailCaptures GetMailCaptures()
		{
			if (!isLotusNotesInstalled || isDisposed) return null;
			try
			{
				using (var client = new LotusNotesMeetingCaptureClientWrapper())
				{
					return MapMailCaptures(client.Client.GetMailCaptures());
				}
			}
			catch (FaultException ex)
			{
				if (ex.Message == "Elevate" || ex.Message == "Unelevate")
				{
					processCoordinator.ChangeElevationLevel(ex.Message == "Elevate");
				}
			}
			catch (Exception ex)
			{
				WcfExceptionLogger.LogWcfError("get mail captures", log, ex);
				if (ex.InnerException is PipeException
					&& ((PipeException)ex.InnerException).ErrorCode == -2146232800)
				{
					processCoordinator.RestartIfNeeded();
				}
			}
			return null;
		}

		private static MailCaptures MapMailCaptures(LotusNotesMeetingCaptureServiceReference.MailCaptures mailCaptures)
		{
			if (mailCaptures == null) return null;

			var serializedData = JsonHelper.SerializeData(mailCaptures);
			MailCaptures result;
			JsonHelper.DeserializeData(serializedData, out result);
			return result;
		}

		public void Dispose()
		{
			if (isDisposed) return;
			isDisposed = true;
			log.Info("Stopping service");
			if (!isLotusNotesInstalled) return;
			processCoordinator.Stop();
		}
	}
}
