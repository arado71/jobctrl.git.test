using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;
using MailActivityTracker.Model;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Mail;
using Tct.ActivityRecorderClient.Communication;
using Tct.ActivityRecorderClient.Configuration;
using Tct.ActivityRecorderClient.Serialization;

namespace Tct.ActivityRecorderClient.Capturing.Core
{
	public class ClientSettingsManager : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private const int settingsUpdateInterval = 60 * 1000;	//60 secs /**/7 GetClientSettings, 234 bytes/call inside, in variables; but 60 packets 23996 bytes/call outside, in Ethernet packets
		private static string settingsFile { get { return "ClientSettings-" + ConfigManager.UserId; } }

		public event EventHandler<SingleValueEventArgs<ClientSetting>> SettingsChanged;

		private volatile ClientSetting clientSettings;
		public ClientSetting ClientSettings
		{
			get { return clientSettings; }
			private set
			{
				if (value == null) //cannot save null value
				{
					ClientSettings = new ClientSetting();
					return;
				}
				Debug.Assert(value != null);
				if (XmlSerializationHelper.AreTheSame(clientSettings, value)) return;
				log.Info("Client settings changed");
				clientSettings = value;
				IsolatedStorageSerializationHelper.Save(settingsFile, value);
			}
		}

		private string currentVersion;

		public ClientSettingsManager()
			: base(log)
		{
		}

		protected override int ManagerCallbackInterval
		{
			get
			{
				return settingsUpdateInterval;
			}
		}

		protected override void ManagerCallbackImpl()
		{
			try
			{
				int userId = ConfigManager.UserId;
				string newVersion = null;
				var settings = ActivityRecorderClientWrapper.Execute(n => n.GetClientSettings(userId, currentVersion, out newVersion));
				if (newVersion != currentVersion)
				{
					log.Debug("New version. (" + currentVersion + " -> " + newVersion + ")");
					currentVersion = newVersion;
					ClientSettings = settings;
					UpdateConfigManagerWithClientSettings(ClientSettings);
				}
			}
			catch (Exception ex)
			{
				WcfExceptionLogger.LogWcfError("get client settings", log, ex);
			}
		}

		public void LoadSettings()
		{
			if (IsolatedStorageSerializationHelper.Exists(settingsFile))
			{
				ClientSetting settings;
				if (IsolatedStorageSerializationHelper.Load(settingsFile, out settings))
				{
					log.Info("Loading client settings from disk");
					clientSettings = settings;
					UpdateConfigManagerWithClientSettings(settings);
				}
			}
		}

		private void UpdateConfigManagerWithClientSettings(ClientSetting settings)
		{
			if (settings == null)
			{
				UpdateConfigManagerWithClientSettings(new ClientSetting());
				return;
			}

			FeatureSwitches.UpdateFeatures(settings.EnabledFeature);
			ConfigManager.CapturingDeadlockInMins = settings.CaptureDeadlockInMins.HasValue ? settings.CaptureDeadlockInMins.Value : AppConfig.Current.CapturingDeadlockInMins;
			ConfigManager.CaptureWorkItemInterval = settings.CaptureWorkItemInterval.HasValue ? settings.CaptureWorkItemInterval.Value : AppConfig.Current.CaptureWorkItemInterval;
			ConfigManager.CaptureActiveWindowInterval = settings.CaptureActiveWindowInterval.HasValue ? settings.CaptureActiveWindowInterval.Value : AppConfig.Current.CaptureActiveWindowInterval;
			ConfigManager.CaptureScreenShotInterval = settings.CaptureScreenShotInterval.HasValue ? settings.CaptureScreenShotInterval.Value : AppConfig.Current.CaptureScreenShotInterval;
			ConfigManager.TimeSyncThreshold = settings.TimeSyncThreshold.HasValue ? settings.TimeSyncThreshold.Value : AppConfig.Current.TimeSyncThreshold;
			ConfigManager.JpegQuality = settings.JpegQuality.HasValue ? settings.JpegQuality.Value : AppConfig.Current.JpegQuality;
			ConfigManager.JpegScalePct = settings.JpegScalePct.HasValue ? settings.JpegScalePct.Value : AppConfig.Current.JpegScalePct;
			ConfigManager.MenuUpdateInterval = settings.MenuUpdateInterval.HasValue ? settings.MenuUpdateInterval.Value : AppConfig.Current.MenuUpdateInterval;
			ConfigManager.WorkTimeStartInMins = settings.WorkTimeStartInMins.HasValue ? settings.WorkTimeStartInMins.Value : AppConfig.Current.WorkTimeStartInMins;
			ConfigManager.WorkTimeEndInMins = settings.WorkTimeEndInMins.HasValue ? settings.WorkTimeEndInMins.Value : AppConfig.Current.WorkTimeEndInMins;
			ConfigManager.AfterWorkTimeIdleInMins = settings.AfterWorkTimeIdleInMins.HasValue ? settings.AfterWorkTimeIdleInMins.Value : AppConfig.Current.AfterWorkTimeIdleInMins;
			ConfigManager.MaxOfflineWorkItems = settings.MaxOfflineWorkItems.HasValue ? settings.MaxOfflineWorkItems.Value : AppConfig.Current.MaxOfflineWorkItems;
			ConfigManager.DuringWorkTimeIdleInMins = settings.DuringWorkTimeIdleInMins.HasValue ? settings.DuringWorkTimeIdleInMins.Value : AppConfig.Current.DuringWorkTimeIdleInMins;
			ConfigManager.DuringWorkTimeIdleManualInterval = settings.DuringWorkTimeIdleManualInterval.HasValue ? settings.DuringWorkTimeIdleManualInterval.Value : AppConfig.Current.DuringWorkTimeIdleManualInterval;
			ConfigManager.MaxManualMeetingInterval = settings.MaxManualMeetingInterval.HasValue ? settings.MaxManualMeetingInterval.Value : AppConfig.Current.MaxManualMeetingInterval;
			ConfigManager.RuleRestrictions = settings.RuleRestrictions.HasValue ? (Rules.RuleRestrictions)settings.RuleRestrictions.Value : AppConfig.Current.RuleRestrictions;
			ConfigManager.IsOutlookMeetingTrackingEnabled = settings.IsMeetingTrackingEnabled.HasValue ? settings.IsMeetingTrackingEnabled.Value : AppConfig.Current.IsOutlookMeetingTrackingEnabled;
			ConfigManager.IsLotusNotesMeetingTrackingEnabled = settings.IsLotusNotesMeetingTrackingEnabled.HasValue ? settings.IsLotusNotesMeetingTrackingEnabled.Value : AppConfig.Current.IsLotusNotesMeetingTrackingEnabled;
			ConfigManager.IsMeetingSubjectMandatory = settings.IsMeetingSubjectMandatory.HasValue ? settings.IsMeetingSubjectMandatory.Value : AppConfig.Current.IsMeetingSubjectMandatory;
			ConfigManager.BusyTimeThreshold = settings.BusyTimeThreshold.HasValue ? settings.BusyTimeThreshold.Value : AppConfig.Current.BusyTimeThreshold;
			ConfigManager.CoincidentalClientsEnabled = settings.CoincidentalClientsEnabled.HasValue ? settings.CoincidentalClientsEnabled.Value : AppConfig.Current.CoincidentalClientsEnabled;
			ConfigManager.IsManualMeetingStartsOnLock = settings.IsManualMeetingStartsOnLock.HasValue ? settings.IsManualMeetingStartsOnLock.Value : AppConfig.Current.IsManualMeetingStartsOnLock;
			ConfigManager.RuleMatchingInterval = settings.RuleMatchingInterval.HasValue ? settings.RuleMatchingInterval.Value : AppConfig.Current.RuleMatchingInterval;
			ConfigManager.IsOutlookAddinRequired = settings.IsOutlookAddinRequired.HasValue ? settings.IsOutlookAddinRequired.Value : AppConfig.Current.IsOutlookAddinRequired;
			ConfigManager.IsOutlookAddinMailTrackingId = settings.IsOutlookAddinMailTrackingId.HasValue ? settings.IsOutlookAddinMailTrackingId.Value : AppConfig.Current.IsOutlookAddinMailTrackingId;
			ConfigManager.MouseMovingThreshold = settings.MouseMovingThreshold.HasValue ? settings.MouseMovingThreshold.Value : AppConfig.Current.MouseMovingThreshold;
			ConfigManager.PluginFailThreshold = settings.PluginFailThreshold.HasValue ? settings.PluginFailThreshold.Value : AppConfig.Current.PluginFailThreshold;
			ConfigManager.CollectedItemAggregateInMins = settings.CollectedItemAggregateInMins.HasValue ? settings.CollectedItemAggregateInMins.Value : AppConfig.Current.CollectedItemAggregateInMins;
			ConfigManager.ForceCountdownRules = settings.ForceCountdownRules.HasValue ? settings.ForceCountdownRules.Value : AppConfig.Current.ForceCountdownRules;
			ConfigManager.MsProjectAddress = settings.MsProjectAddress ?? AppConfig.Current.MsProjectAddress;
			ConfigManager.ManualWorkItemEditAgeLimit = settings.ManualWorkItemEditAgeLimit ?? AppConfig.Current.ManualWorkItemEditAgeLimit;
			ConfigManager.AutoReturnFromMeeting = settings.AutoReturnFromMeeting ?? ConfigManager.AutoReturnFromMeeting;
			ConfigManager.IsOutlookAddinMailTrackingUseSubject = settings.IsOutlookAddinMailTrackingUseSubject.HasValue ? settings.IsOutlookAddinMailTrackingUseSubject.Value : AppConfig.Current.IsOutlookAddinMailTrackingUseSubject;
			ConfigManager.TelemetryCollectedKeys = settings.TelemetryCollectedKeys ?? AppConfig.Current.TelemetryCollectedKeys;
			ConfigManager.TelemetryMaxAgeInMins = settings.TelemetryMaxAgeInMins.HasValue ? settings.TelemetryMaxAgeInMins.Value : AppConfig.Current.TelemetryMaxAgeInMins;
			ConfigManager.TelemetryMaxCount = settings.TelemetryMaxCount.HasValue ? settings.TelemetryMaxCount.Value : AppConfig.Current.TelemetryMaxCount;
			ConfigManager.IsMeetingTentativeSynced = settings.IsMeetingTentativeSynced.HasValue ? settings.IsMeetingTentativeSynced.Value : AppConfig.Current.IsMeetingTentativeSynced;
			ConfigManager.IsInjectedInputAllowed = settings.IsInjectedInputAllowed.HasValue ? settings.IsInjectedInputAllowed.Value : AppConfig.Current.IsInjectedInputAllowed;
			ConfigManager.ClientDataCollectionSettings = (ClientDataCollectionSettings?)settings.DataCollectionSettings;
			ConfigManager.IsNotificationShown = settings.IsNotificationShown.HasValue ? (bool?)(settings.IsNotificationShown == 1) : null;
			ConfigManager.IsTodoListEnabled = settings.IsTodoListEnabled;
			ConfigManager.MailTrackingSettings = settings.MailTrackingSettings == null
				? MailTrackingSettings.All
				: (MailTrackingSettings) settings.MailTrackingSettings;
			ConfigManager.DiagnosticOperationMode = settings.DiagnosticOperationMode != null ? (Common.DiagnosticOperationMode)settings.DiagnosticOperationMode.Value : Common.DiagnosticOperationMode.None;
			ConfigManager.IsGoogleCalendarTrackingEnabled = settings.IsGoogleCalendarTrackingEnabled ?? false;
			ConfigManager.IsAnonymModeEnabled = settings.IsAnonymModeEnabled ?? false;
			ConfigManager.DisplayOptions = settings.DisplayOptions != null ? (DisplayOptions?) settings.DisplayOptions : null;
			ConfigManager.WorkStateNotificationParams = settings.WorkStateNotificationParams;
			ConfigManager.NonWorkStateNotificationParams = settings.NonWorkStateNotificationParams;
			ConfigManager.TasksChangedNotificationParams = settings.TasksChangedNotificationParams;
			ConfigManager.AutoStartClientOnNonWorkDays = settings.AutoStartClientOnNonWorkDays ?? AppConfig.Current.AutoStartClientOnNonWorkDays;
			ConfigManager.MeetingTaskIdSettings = settings.MeetingPluginTaskIdSettings.HasValue ? (MeetingPluginTaskIdSettings)settings.MeetingPluginTaskIdSettings.Value : MeetingPluginTaskIdSettings.Description;
			ConfigManager.AdHocMeetingDefaultSelectedTaskId = settings.AdHocMeetingDefaultSelectedTaskId;
			ConfigManager.IsVideoRecordingEnabled = settings.IsVideoRecordingEnabled ?? AppConfig.Current.IsVideoRecordingEnabled;
			ConfigManager.VideoRecordingFps = settings.VideoRecordingFps ?? AppConfig.Current.VideoRecordingFps;
			ConfigManager.VideoRecordingBitRate = settings.VideoRecordingBitRate ?? AppConfig.Current.VideoRecordingBitRate;
			ConfigManager.IsWorkEnabledOutsideWorkTimeStartEnd = settings.IsWorkEnabledOutsideWorkTimeStartEnd ?? true;

			OnSettingsChanged(settings);
		}

		private void OnSettingsChanged(ClientSetting settings)
		{
			var del = SettingsChanged;
			if (del != null) del(this, SingleValueEventArgs.Create(settings));
		}
	}
}
