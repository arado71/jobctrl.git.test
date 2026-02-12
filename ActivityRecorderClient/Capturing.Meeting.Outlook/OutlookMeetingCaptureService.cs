using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel;
using log4net;
using Tct.ActivityRecorderClient.Capturing.Mail;
using Tct.ActivityRecorderClient.Communication;
using OutlookInteropService;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.ProcessInfo;

namespace Tct.ActivityRecorderClient.Capturing.Meeting.Outlook
{
	/// <summary>
	/// This is an implementation that start OutlookSync\OutlookMeetingCaptureServiceHost.exe in an external process
	/// and communicate with it by named pipes for retrieving meetings from Outlook.
	/// </summary>
	public class OutlookMeetingCaptureService : IMeetingCaptureService, IAddressBookService
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly List<ActivityRecorderServiceReference.FinishedMeetingEntry> emptyFinishedMeetings = new List<ActivityRecorderServiceReference.FinishedMeetingEntry>();
		private static readonly List<ActivityRecorderServiceReference.MeetingAttendee> emptySelectedAttendees = new List<ActivityRecorderServiceReference.MeetingAttendee>();

		private ProcessCoordinator processCoordinator;

		private bool isOutlookInstalled = OutlookSettingsHelper.IsOutlookInstalled; //we can access IsAddressBookServiceAvailable without calling Initialize()

		public void Initialize()
		{
			//TODO: Check default mail client. (Default key under "HKEY_LOCAL_MACHINE\SOFTWARE\Clients\Mail". ???)

			isOutlookInstalled = OutlookSettingsHelper.IsOutlookInstalled;
			if (!isOutlookInstalled)
			{
				log.Info("There is no Outlook installed on this computer.");
			}
		}

		private bool UpdateServiceIfNecessary()
		{
			var isOutlookRunning = Process.GetProcessesByName("outlook").Any(p => !p.HasExited);
			if (isOutlookRunning)
			{
				if (processCoordinator != null) return true;
				processCoordinator = ProcessCoordinator.OutlookMeetingProcessCoordinator;
				processCoordinator.Start();
				log.Info("Version info - " + GetVersionInfoInt());
				return true;
			}
			if (processCoordinator == null) return false;
			log.Info("Stopping service");
			processCoordinator.Stop();
			processCoordinator = null;
			return false;
		}

		public void Dispose()
		{
			if (!isOutlookInstalled || processCoordinator == null) return;
			log.Info("Stopping service");
			processCoordinator.Stop();
			processCoordinator = null;
		}

		public string GetVersionInfo()
		{
			if (!isOutlookInstalled || !UpdateServiceIfNecessary()) return null;

			return GetVersionInfoInt();
		}

		private string GetVersionInfoInt()
		{
			log.Debug("GetVersionInfo started...");

			try
			{
				using (var client = new OutlookMeetingCaptureClientWrapper())
				{
					return client.Client.GetVersionInfo(ConfigManager.UseRedemptionForMeetingSync);
				}
			}
			catch (Exception ex)
			{
				if (ex is FaultException)
				{
					if (ex.Message == "Elevate" || ex.Message == "Unelevate")
					{
						processCoordinator.ChangeElevationLevel(ex.Message == "Elevate");
						using (var client = new OutlookMeetingCaptureClientWrapper())
						{
							//return client.Client.GetVersionInfo(ConfigManager.UseRedemptionForMeetingSync);
							var vInfo = client.Client.GetVersionInfo(ConfigManager.UseRedemptionForMeetingSync);
							log.Info("Version info - " + vInfo);
							return vInfo;
						}
					}
				}


				WcfExceptionLogger.LogWcfError("get version info", log, ex);
				HandleError(ex);
				return null;
			}
			finally
			{
				log.Debug("GetVersionInfo finished.");
			}
		}

		public string[] ProcessNames => new[] { "outlook" };

		public List<ActivityRecorderServiceReference.FinishedMeetingEntry> CaptureMeetings(IList<string> calendarAccountEmails, DateTime startDate, DateTime endDate)
		{
			if (!isOutlookInstalled || !UpdateServiceIfNecessary()) return emptyFinishedMeetings;

			log.Debug("CaptureMeetings started...");

			try
			{
				using (var client = new OutlookMeetingCaptureClientWrapper())
				{
					var finishedMeetings = client.Client.CaptureMeetings(calendarAccountEmails.ToList(), startDate, endDate, ConfigManager.ManualWorkItemEditAgeLimit, ConfigManager.IsMeetingAppointmentSynchronized, ConfigManager.IsMeetingUploadModifications, ConfigManager.CalendarFolderInclusionPattern, ConfigManager.CalendarFolderExclusionPattern, ConfigManager.IsMeetingTentativeSynced, ConfigManager.UseRedemptionForMeetingSync, Configuration.AppConfig.Current.DelayedDeleteIntervalInMins);
					return MeetingDataMapper.To(finishedMeetings);
				}
			}
			catch (Exception ex)
			{
				if (ex.Message.Contains("MAPI_E_LOGON_FAILED"))
				{
					log.Info("Unable to capture meetings (MAPI_E_LOGON_FAILED)");
					log.Debug("Unable to capture meetings (MAPI_E_LOGON_FAILED)", ex);
				}
				else
				{
					WcfExceptionLogger.LogWcfError("capture meetings", log, ex);
				}
				HandleError(ex);
			}
			finally { log.Debug("CaptureMeetings finished."); }

			return emptyFinishedMeetings;
		}

		public bool IsAddressBookServiceAvailable
		{
			get { return isOutlookInstalled; }
		}

		public List<ActivityRecorderServiceReference.MeetingAttendee> DisplaySelectNamesDialog(IntPtr parentWindowHandle)
		{
			if (!isOutlookInstalled || !UpdateServiceIfNecessary()) return null;

			log.Debug("DisplaySelectNamesDialog started...");

			try
			{
				using (var client = new OutlookMeetingCaptureClientWrapper())
				{
					client.SetTimeout(TimeSpan.FromMinutes(10));	//Workaround: After 10 minutes selected attendees wont get back from SelectNamesDialog. TODO: replace with polling or callback
					var selectedAttendees = client.Client.DisplaySelectNamesDialog(parentWindowHandle);
					return MeetingDataMapper.To(selectedAttendees);
				}
			}
			catch (Exception ex)
			{
				WcfExceptionLogger.LogWcfError("display SelectNamesDialog", log, ex);
				HandleError(ex);
			}
			finally { log.Debug("DisplaySelectNamesDialog finished."); }

			return emptySelectedAttendees;
		}

		private void HandleError(Exception e)
		{
			//Endpoint not found
			if (e is EndpointNotFoundException && e.InnerException is PipeException exception
				&& (uint)exception.ErrorCode == 0x80131620)
			{
				processCoordinator.RestartIfNeeded();
			}
		}
	}
}
