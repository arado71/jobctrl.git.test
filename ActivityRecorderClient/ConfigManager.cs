using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using log4net;
using MailActivityTracker.Model;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.EnvironmentInfo;
using Tct.ActivityRecorderClient.Capturing.Mail;
using Tct.ActivityRecorderClient.Capturing.Plugins.Impl;
using Tct.ActivityRecorderClient.Common;
using Tct.ActivityRecorderClient.Configuration;
using Tct.ActivityRecorderClient.Notification;
using Tct.ActivityRecorderClient.Screenshots;
using Tct.ActivityRecorderClient.Serialization;
using Tct.ActivityRecorderClient.Telemetry;
using Tct.ActivityRecorderClient.Update;

namespace Tct.ActivityRecorderClient
{
	public class ConfigManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static readonly Version VersionAssembly = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
		public static readonly string DebugOrReleaseString = (System.Reflection.Assembly.GetExecutingAssembly()
			.GetCustomAttributes(typeof(System.Reflection.AssemblyConfigurationAttribute), false)
			.OfType<System.Reflection.AssemblyConfigurationAttribute>()
			.FirstOrDefault(n => !string.IsNullOrEmpty(n.Configuration)) ?? new System.Reflection.AssemblyConfigurationAttribute("Unknown")).Configuration + " build";
		public static readonly string ApplicationName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
		public static readonly string Classifier;
		public static readonly Version VersionAutoUpdate;
		public static readonly bool IsAppLevelStorageNeeded;
		public static readonly bool IsWindows7 = Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version >= new Version(6, 1);
		public static readonly IEnvironmentInfoService EnvironmentInfo;
		public static readonly int CurrentProcessPid;
		public static readonly string LogPath;
		public static readonly string JcLocalPath;

		public static Version Version { get { return VersionAutoUpdate ?? VersionAssembly; } }
		public static string VersionWithClassifier { get { return Classifier != null ? Version + string.Format(" ({0})", Classifier) : Version.ToString(); } }

		public static readonly string MutexName;

		public static readonly string WebsiteUrl;
		public static readonly string WebsiteUrlFormatString;
		public static readonly string HttpApplicationUpdateUrl;
		public static readonly string ValidCertificate;
		/// <summary>
		/// <para>Determines how the app starts with elevated (admin) rights.</para>
		///	<para>- null: starts without admin rights, but the setting can be changed in old style menu;</para>
		///	<para>- true: starts as admin by default, but the setting can be changed in old style menu;</para>
		///	<para>- false: starts without admin rights and the setting cannot be changed</para>
		/// </summary>
		public static readonly bool? IsRunAsAdminDefault;
		public static readonly bool IsMeetingDescriptionSynchronized;
		public static readonly bool IsMeetingAppointmentSynchronized;
		public static readonly string CalendarFolderInclusionPattern;
		public static readonly string CalendarFolderExclusionPattern;
		public static readonly string AppNameOverride;
		public static readonly string IssueCategories;
		public static readonly DateTime StartupTime = DateTime.UtcNow;

		public static readonly int LoggedOutUserId = -1;
		private static volatile int userId = LoggedOutUserId;
		public static int UserId
		{//todo get-nel EnsureLoggedIn
			get { return userId; }
			private set
			{
				int oldUserId = userId;
				if (oldUserId == value) return;
				userId = value;
				if (value == LoggedOutUserId)
				{
					log.Info("User " + oldUserId + " logged out");
				}
				else
				{
					log.Info("UserId is set to " + value);
				}
				LocalSettingsForUser = LocalSettings.GetLocalSettings();
				LocalSettings.LogLocalSettings(log, "Loaded Local Settings:", LocalSettingsForUser);
			}
		}

		private static volatile string userPassword;
		public static string UserPassword
		{
			get { return userPassword; }
			private set { userPassword = value; }
		}

		public static string UserName { get; private set; }
		public static UserAccessLevel AccessLevel { get; private set; }
		public static TimeZoneInfo TimeZoneWeb { get; private set; } = TimeZoneInfo.Local;

		private static DateTime? userPasswordExpirationDate;
		public static DateTime? UserPasswordExpirationDate
		{
			get { return userPasswordExpirationDate; }
			private set
			{
				if (value == userPasswordExpirationDate) return;
				userPasswordExpirationDate = value;
				log.Info("UserPasswordExpirationDate is set to " + value);
			}
		}
		public static bool StartWorkAfterLogin { get; internal set; }

		public static bool IsRoamingStorageScopeNeeded { get; private set; }
		public static bool DisableManualStatusChange { get; private set; }
		public static bool IsTaskBarIconShowing { get; private set; }

		public static void ShowTaskBarIcon()
		{
			IsTaskBarIconShowing = true;
		}

		public static readonly IssuePropColumnFlag IssuePropColumns;

        private static volatile bool autoReturnFromMeeting;
        public static bool AutoReturnFromMeeting
        {
            get { return autoReturnFromMeeting; }
            set
            {
                if (autoReturnFromMeeting == value) return;
                autoReturnFromMeeting = value;
                log.Info("[AutoReturnFromMeeting] = '" + value + "'");
            }
        }

		private static int capturingDeadlockInMins;
		public static int CapturingDeadlockInMins
		{
			get { return capturingDeadlockInMins; }
			set
			{
				if (capturingDeadlockInMins == value) return;
				if (capturingDeadlockInMins < 0)
				{
					log.Warn("Tried to set [DeadlockInMins] to an invalid value");
					return;
				}

				capturingDeadlockInMins = value;
				log.Info("[CapturingDeadlockInMins] = '" + value + "'");
			}
		}

		private static volatile int captureWorkItemInterval;
		public static int CaptureWorkItemInterval
		{
			get { return captureWorkItemInterval; }
			set
			{
				if (captureWorkItemInterval == value) return;
				captureWorkItemInterval = value;
				log.Info("[CaptureWorkItemInterval] = '" + value + "'");
			}
		}

		private static volatile int captureActiveWindowInterval;
		public static int CaptureActiveWindowInterval
		{
			get { return captureActiveWindowInterval; }
			set
			{
				if (captureActiveWindowInterval == value) return;
				captureActiveWindowInterval = value;
				log.Info("[CaptureActiveWindowInterval] = '" + value + "'");
			}
		}

		private static volatile int captureScreenShotInterval;
		public static int CaptureScreenShotInterval
		{
			get { return captureScreenShotInterval; }
			set
			{
				if (captureScreenShotInterval == value) return;
				captureScreenShotInterval = value;
				log.Info("[CaptureScreenShotInterval] = '" + value + "'");
			}
		}

		private static volatile int timeSyncThreshold;
		public static int TimeSyncThreshold
		{
			get { return timeSyncThreshold; }
			set
			{
				if (timeSyncThreshold == value) return;
				timeSyncThreshold = value;
				log.Info("[TimeSyncThreshold] = '" + value + "'");
			}
		}

		private static volatile int jpegQuality;
		public static int JpegQuality
		{
			get { return jpegQuality; }
			set
			{
				if (jpegQuality == value) return;
				jpegQuality = value;
				log.Info("[JpegQuality] = '" + value + "'");
			}
		}

		private static volatile int jpegScalePct;
		public static int JpegScalePct
		{
			get { return jpegScalePct; }
			set
			{
				var newValue = Math.Min(Math.Max(1, value), 100);
				if (jpegScalePct == newValue) return;
				jpegScalePct = newValue;
				log.Info("[JpegScalePct] = '" + newValue + "'");
			}
		}

		private static volatile int menuUpdateInterval;
		public static int MenuUpdateInterval
		{
			get { return menuUpdateInterval; }
			set
			{
				if (menuUpdateInterval == value) return;
				menuUpdateInterval = value;
				log.Info("[MenuUpdateInterval] = '" + value + "'");
			}
		}

		private static volatile int workTimeStartInMins;
		public static int WorkTimeStartInMins
		{
			get { return workTimeStartInMins; }
			set
			{
				if (workTimeStartInMins == value) return;
				workTimeStartInMins = value;
				log.Info("[WorkTimeStartInMins] = '" + value + "'");
			}
		}

		private static volatile int workTimeEndInMins;
		public static int WorkTimeEndInMins
		{
			get { return workTimeEndInMins; }
			set
			{
				if (workTimeEndInMins == value) return;
				workTimeEndInMins = value;
				log.Info("[WorkTimeEndInMins] = '" + value + "'");
			}
		}

		private static volatile int afterWorkTimeIdleInMins;
		public static int AfterWorkTimeIdleInMins
		{
			get { return afterWorkTimeIdleInMins; }
			set
			{
				if (afterWorkTimeIdleInMins == value) return;
				afterWorkTimeIdleInMins = value;
				log.Info("[AfterWorkTimeIdleInMins] = '" + value + "'");
			}
		}

		private static int usageSaveInSecs;

		public static int UsageSaveInSecs
		{
			get { return usageSaveInSecs; }
			set
			{
				if (usageSaveInSecs == value) return;
				usageSaveInSecs = value;
				if (usageSaveInSecs < 5)
				{
					log.Warn("Invalid usageSaveInSecs value " + value);
					usageSaveInSecs = 5;
				}
				log.Info("[UsageSaveInSecs] = '" + value + "'");
			}
		}

		private static volatile int maxOfflineWorkItems;
		public static int MaxOfflineWorkItems
		{
			get { return maxOfflineWorkItems; }
			set
			{
				if (maxOfflineWorkItems == value) return;
				maxOfflineWorkItems = value;
				log.Info("[MaxOfflineWorkItems] = '" + value + "'");
			}
		}

		private static volatile int duringWorkTimeIdleInMins;
		public static int DuringWorkTimeIdleInMins
		{
			get { return duringWorkTimeIdleInMins; }
			set
			{
				if (duringWorkTimeIdleInMins == value) return;
				duringWorkTimeIdleInMins = value;
				log.Info("[DuringWorkTimeIdleInMins] = '" + value + "'");
			}
		}

		private static volatile int duringWorkTimeIdleManualInterval;
		/// <summary>
		/// Longest offline work triggered by inactivity
		/// </summary>
		public static int DuringWorkTimeIdleManualInterval
		{
			get { return duringWorkTimeIdleManualInterval; }
			set
			{
				if (duringWorkTimeIdleManualInterval == value) return;
				duringWorkTimeIdleManualInterval = value;
				log.Info("[DuringWorkTimeIdleManualInterval] = '" + value + "'");
			}
		}

		private static volatile int maxManualMeetingInterval;
		/// <summary>
		/// Ad-hoc offline work maximum length
		/// </summary>
		public static int MaxManualMeetingInterval
		{
			get { return maxManualMeetingInterval; }
			set
			{
				if (maxManualMeetingInterval == value) return;
				maxManualMeetingInterval = value;
				log.Info("[MaxManualMeetingInterval] = '" + value + "'");
			}
		}

		private static volatile Rules.RuleRestrictions ruleRestrictions;
		public static Rules.RuleRestrictions RuleRestrictions
		{
			get { return ruleRestrictions; }
			set
			{
				if (ruleRestrictions == value) return;
				ruleRestrictions = value;
				log.Info("[RuleRestrictions] = '" + value + "'");
			}
		}

		private static volatile bool isMeetingSubjectMandatory;
		public static bool IsMeetingSubjectMandatory
		{
			get { return isMeetingSubjectMandatory; }
			set
			{
				if (isMeetingSubjectMandatory == value) return;
				isMeetingSubjectMandatory = value;
				log.Info("[IsMeetingSubjectMandatory] = '" + value + "'");
			}
		}

		public static bool IsMeetingTrackingEnabled
		{
			get { return IsOutlookMeetingTrackingEnabled || IsLotusNotesMeetingTrackingEnabled; }
		}

		private static volatile bool isOutlookMeetingTrackingEnabled;
		public static bool IsOutlookMeetingTrackingEnabled
		{
			get { return isOutlookMeetingTrackingEnabled; }
			set
			{
				if (isOutlookMeetingTrackingEnabled == value) return;
				isOutlookMeetingTrackingEnabled = value;
				log.Info("[IsOutlookMeetingTrackingEnabled] = '" + value + "'");
			}
		}

		private static volatile bool isLotusNotesMeetingTrackingEnabled;
		public static bool IsLotusNotesMeetingTrackingEnabled
		{
			get { return isLotusNotesMeetingTrackingEnabled; }
			set
			{
				if (isLotusNotesMeetingTrackingEnabled == value) return;
				isLotusNotesMeetingTrackingEnabled = value;
				log.Info("[IsLotusNotesMeetingTrackingEnabled] = '" + value + "'");
			}
		}

		private static volatile int busyTimeThreshold;
		public static int BusyTimeThreshold
		{
			get { return busyTimeThreshold; }
			set
			{
				if (busyTimeThreshold == value) return;
				busyTimeThreshold = value;
				log.Info("[BusyTimeThreshold] = '" + value + "'");
			}
		}

		private static volatile bool coincidentalClientsEnabled;
		public static bool CoincidentalClientsEnabled
		{
			get { return coincidentalClientsEnabled; }
			set
			{
				if (coincidentalClientsEnabled == value) return;
				coincidentalClientsEnabled = value;
				log.Info("[CoincidentalClientsEnabled] = '" + value + "'");
			}
		}

		private static volatile bool isManualMeetingStartsOnLock;
		public static bool IsManualMeetingStartsOnLock
		{
			get { return isManualMeetingStartsOnLock; }
			set
			{
				if (isManualMeetingStartsOnLock == value) return;
				isManualMeetingStartsOnLock = value;
				log.Info("[IsManualMeetingStartsOnLock] = '" + value + "'");
			}
		}

		private static volatile int ruleMatchingInterval;
		public static int RuleMatchingInterval
		{
			get { return ruleMatchingInterval; }
			set
			{
				if (value < 300 || value > 10000)
				{
					log.Warn("Invalid RuleMatchingInterval " + value);
					value = Math.Min(Math.Max(value, 300), 10000);
				}
				if (ruleMatchingInterval == value) return;
				ruleMatchingInterval = value;
				log.Info("[RuleMatchingInterval] = '" + value + "'");
			}
		}

		private static volatile int pluginFailThreshold;

		public static bool AsyncPluginsEnabled
		{
			get
			{
				return pluginFailThreshold > 0;
			}
		}

		public static int PluginFailThreshold
		{
			get { return pluginFailThreshold; }
			set
			{
				if (value < -1)
				{
					log.Warn("Invalid PluginFailTreshold " + value);
					value = 1;
				}

				if (pluginFailThreshold == value) return;
				pluginFailThreshold = value;
				log.Info("[PluginFailTreshold] = '" + value + "'");
			}
		}

		private static volatile bool isOutlookAddinRequired;
		public static bool IsOutlookAddinRequired
		{
			get { return isOutlookAddinRequired; }
			set
			{
				if (isOutlookAddinRequired == value) return;
				isOutlookAddinRequired = value;
				log.Info("[IsOutlookAddinRequired] = '" + value + "'");
			}
		}

		private static volatile bool isOutlookAddinMailTrackingId;
		public static bool IsOutlookAddinMailTrackingId
		{
			get { return isOutlookAddinMailTrackingId; }
			set
			{
				if (isOutlookAddinMailTrackingId == value) return;
				isOutlookAddinMailTrackingId = value;
				log.Info("[IsOutlookAddinMailTrackingId] = '" + value + "'");
			}
		}

		private static volatile string telemetryCollectedKeys;
		public static string TelemetryCollectedKeys
		{
			get { return telemetryCollectedKeys; }
			set
			{
				if (telemetryCollectedKeys == value) return;
				telemetryCollectedKeys = value;
				log.Info("[TelemetryCollectedKeys] = '" + value + "'");
			}
		}

		private static volatile int telemetryMaxAgeInMins;
		public static int TelemetryMaxAgeInMins
		{
			get { return telemetryMaxAgeInMins; }
			set
			{
				if (telemetryMaxAgeInMins == value) return;
				telemetryMaxAgeInMins = value;
				log.Info("[TelemetryMaxAgeInMins] = '" + value + "'");
			}
		}

		private static volatile int telemetryMaxCount;
		public static int TelemetryMaxCount
		{
			get { return telemetryMaxCount; }
			set
			{
				if (telemetryMaxCount == value) return;
				telemetryMaxCount = value;
				log.Info("[TelemetryMaxCount] = '" + value + "'");
			}
		}

		private static volatile bool isOutlookAddinMailTrackingUseSubject;
		public static bool IsOutlookAddinMailTrackingUseSubject
		{
			get { return isOutlookAddinMailTrackingUseSubject; }
			set
			{
				if (isOutlookAddinMailTrackingUseSubject == value) return;
				isOutlookAddinMailTrackingUseSubject = value;
				log.Info("[IsOutlookAddinMailTrackingUseSubject] = '" + value + "'");
			}
		}

		public static MailTrackingType MailTrackingType
		{
			get
			{
				var isTrackingEnabled = IsOutlookAddinMailTrackingId;
				var isSubjectTrackingEnabled = IsOutlookAddinMailTrackingUseSubject;

				return isTrackingEnabled ? isSubjectTrackingEnabled ? MailTrackingType.BodyAndSubject : MailTrackingType.BodyOnly : MailTrackingType.Disable;
			}
		}

		private static volatile bool useRedemptionForMeetingSync;
		public static bool UseRedemptionForMeetingSync
		{
			get { return useRedemptionForMeetingSync; }
			set
			{
				if (useRedemptionForMeetingSync == value) return;
				useRedemptionForMeetingSync = value;
				log.Info("[UseRedemptionForMeetingSync] = '" + value + "'");
			}
		}

		private static volatile MeetingPluginTaskIdSettings meetingTaskIdSettings;
		public static MeetingPluginTaskIdSettings MeetingTaskIdSettings
		{
			get { return meetingTaskIdSettings; }
			set
			{
				if (meetingTaskIdSettings == value) return;
				meetingTaskIdSettings = value;
				log.Info("[MeetingTaskIdSettings] = '" + value + "'");
			}
		}

		private static volatile string oCRLanguage;
		public static string OCRLanguage
		{
			get { return oCRLanguage; }
			set
			{
				if (oCRLanguage == value) return;
				oCRLanguage = value;
				log.Info("[OCRLanguage] = '" + value + "'");
			}
		}

		private static volatile string googleClientId;
		public static string GoogleClientId
		{
			get => googleClientId;
			set
			{
				if (googleClientId == value) return;
				string decodedValue = null;
				if (value != null)
				{
					decodedValue = SimpleEncoder.Decode(value);
				}
				googleClientId = decodedValue;
				log.Info($"[GoogleClientId = '{decodedValue}']");
			}
		}

		private static volatile string googleClientSecret;
		public static string GoogleClientSecret
		{
			get => googleClientSecret;
			set
			{
				if (googleClientSecret == value) return;
				string decodedValue = null;
				if (value != null)
				{
					decodedValue = SimpleEncoder.Decode(value);
				}
				googleClientSecret = decodedValue;
				log.Info($"[GoogleClientSecret = '{decodedValue}']");
			}
		}

		private static volatile bool isMeetingUploadModifications;
		public static bool IsMeetingUploadModifications
		{
			get { return isMeetingUploadModifications; }
			set
			{
				if (isMeetingUploadModifications == value) return;
				isMeetingUploadModifications = value;
				log.Info("[isMeetingUploadModifications] = '" + value + "'");
			}
		}

		private static volatile bool isMeetingTentativeSynced;
		public static bool IsMeetingTentativeSynced
		{
			get { return isMeetingTentativeSynced; }
			set
			{
				if (isMeetingTentativeSynced == value) return;
				isMeetingTentativeSynced = value;
				log.Info("[isMeetingTentativeSynced] = '" + value + "'");
			}
		}

		private static volatile bool isInjectedInputAllowed;
		public static bool IsInjectedInputAllowed
		{
			get { return isInjectedInputAllowed; }
			set
			{
				if (isInjectedInputAllowed == value) return;
				isInjectedInputAllowed = value;
				log.Info("[isInjectedInputAllowed] = '" + value + "'");
			}
		}

		private static ClientDataCollectionSettings? clientDataCollectionSettings;
		public static ClientDataCollectionSettings? ClientDataCollectionSettings
		{
			get { return clientDataCollectionSettings; }
			set
			{
				if (clientDataCollectionSettings == value) return;
				clientDataCollectionSettings = value;
				log.Info("[clientDataCollectionSettings] = '" + value + "'");
			}
		}

		private static volatile int mouseMovingThreshold;
		public static int MouseMovingThreshold
		{
			get { return mouseMovingThreshold; }
			set
			{
				if (mouseMovingThreshold == value) return;
				mouseMovingThreshold = value;
				log.Info("[MouseMovingThreshold] = '" + value + "'");
			}
		}

		private static volatile string msProjectAddress;

		public static string MsProjectAddress
		{
			get { return msProjectAddress; }
			set
			{
				if (msProjectAddress == value) return;
				msProjectAddress = value;
#if !ProjectSync
				if(!string.IsNullOrEmpty(msProjectAddress))
				{
					log.Warn("Unsupported [MsProjectAddress] setting set. Client compiled without ProjectSync flag.");
				}
#endif
				log.Info("[MsProjectAddress] = '" + value + "'");
			}
		}

		private static volatile bool forceCountdownRules;

		public static bool ForceCountdownRules
		{
			get { return forceCountdownRules; }
			set
			{
				if (forceCountdownRules == value) return;
				forceCountdownRules = value;
				log.Info("[ForceCountdownRules] = '" + value + "'");
			}
		}


		private static volatile int collectedItemAggregateInMins;
		public static int CollectedItemAggregateInMins
		{
			get { return collectedItemAggregateInMins; }
			set
			{
				if (value < 1)
				{
					log.Warn("Invalid CollectedItemAggregateInMins " + value);
					value = 1;
				}

				if (collectedItemAggregateInMins == value) return;
				collectedItemAggregateInMins = value;
				log.Info("[CollectedItemAggregateInMins = '" + value + "'");
			}
		}

		private static volatile int manualWorkItemEditAgeLimit;
		public static int ManualWorkItemEditAgeLimit
		{
			get { return manualWorkItemEditAgeLimit; }
			set
			{
				if (value < 0)
					log.Warn("Invalid ManualWorkItemEditAgeLimit " + value);
				if (value < 1)
					value = AppConfig.Current.ManualWorkItemEditAgeLimit;

				if (manualWorkItemEditAgeLimit == value) return;
				manualWorkItemEditAgeLimit = value;
				log.Info("[ManualWorkItemEditAgeLimit = '" + value + "'");
			}
		}

		private static volatile View.WorkDetectorRuleEditForm.OkValidType selfLearningOkValidity;
		public static View.WorkDetectorRuleEditForm.OkValidType SelfLearningOkValidity
		{
			get { return selfLearningOkValidity; }
			set
			{
				if (selfLearningOkValidity == value) return;
				selfLearningOkValidity = value;
				log.Info("[SelfLearningOkValidity = '" + value + "'");
			}
		}

		private static bool? isNotificationShown;

		public static bool? IsNotificationShown
		{
			get { return isNotificationShown; }
			set
			{
				if (isNotificationShown == value) return;
				isNotificationShown = value;
				log.Info("[IsNotificationShown] = '" + value + "'");
				if (value.HasValue && value.Value && LocalSettingsForUser.NotificationPosition == NotificationPosition.Hidden)
					LocalSettingsForUser.NotificationPosition = NotificationPosition.BottomRight;
			}
		}

		private static bool autoUpdateManagerEnabled;

		public static bool AutoUpdateManagerEnabled
		{
			get => autoUpdateManagerEnabled;
			set
			{
				if (autoUpdateManagerEnabled == value) return;
				autoUpdateManagerEnabled = value;
				log.Info($"[AutoUpdateManagerEnabled] = '{value}'");
			}
		}

		private static bool isTodoListEnabled;

		public static bool IsTodoListEnabled
		{
			get { return isTodoListEnabled; }
			set
			{
				if (isTodoListEnabled == value) return;
				isTodoListEnabled = value;
				log.Info("[IsTodoListEnabled] = '" + value + "'");
			}
		}

		private static MailTrackingSettings mailTrackingSettings;

		public static MailTrackingSettings MailTrackingSettings
		{
			get => mailTrackingSettings;
			set
			{
				if (mailTrackingSettings == value) return;
				mailTrackingSettings = value;
				log.Info("[MailTrackingSettings] = '" + value + "'");
			}
		}

		private static bool isLoginRememberPasswordChecked;

		public static bool IsLoginRememberPasswordChecked
		{
			get => isLoginRememberPasswordChecked;
			set
			{
				if (isLoginRememberPasswordChecked == value) return;
				isLoginRememberPasswordChecked = value;
				log.Info($"[IsLoginRememberPasswordChecked] = '{value}'");
			}
		}

		private static int diagDebugLastCheck;
		private const int diagDebugWarnInterval = 1 * 60 * 1000;
		private static int isDiagDebugBusy;
		private static DiagnosticOperationMode diagnosticOperationMode;

		public static DiagnosticOperationMode DiagnosticOperationMode
		{
			get
			{
				if ((diagnosticOperationMode & DiagnosticOperationMode.Enabled) > 0)
				{
					if (0 == System.Threading.Interlocked.CompareExchange(ref isDiagDebugBusy, 1, 0))
					{
						var now = Environment.TickCount;
						if (diagDebugWarnInterval < now - diagDebugLastCheck)
						{
							log.Info("***************** Diagnostic Operation Mode is enabled, please check settings ****************");
							diagDebugLastCheck = now;
						}
						isDiagDebugBusy = 0;
					}
				}
				return diagnosticOperationMode;
			}
			set
			{
				if (diagnosticOperationMode == value) return;
				diagnosticOperationMode = value;
				log.Info("[DiagnosticOperationMode] = '" + value + "'");
			}
		}

		public static bool CheckDiagnosticOperationMode(DiagnosticOperationMode flags)
		{
			return (DiagnosticOperationMode & flags) == flags;
		}

		private static bool isGoogleCalendarTrackingEnabled;

		public static bool IsGoogleCalendarTrackingEnabled
		{
			get { return isGoogleCalendarTrackingEnabled; }
			set
			{
				if (isGoogleCalendarTrackingEnabled == value) return;
				isGoogleCalendarTrackingEnabled = value;
				log.Info("[IsGoogleCalendarTrackingEnabled] = '" + value + "'");
			}
		}

		private static bool isAnonymModeEnabled;

		public static bool IsAnonymModeEnabled
		{
			get { return isAnonymModeEnabled; }
			set
			{
				if (isAnonymModeEnabled == value) return;
				isAnonymModeEnabled = value;
				log.Info("[IsAnonymModeEnabled] = '" + value + "'");
			}
		}

		private static bool onlyDesktopTasksInWorktimeMod;

		public static bool OnlyDesktopTasksInWorktimeMod
		{
			get => onlyDesktopTasksInWorktimeMod;
			set
			{
				if (onlyDesktopTasksInWorktimeMod == value) return;
				onlyDesktopTasksInWorktimeMod = value;
				log.Info($"[OnlyDesktopTasksInWorktimeMod] = '{value}'");
			}
		}

		private static bool suppressActiveDirectoryFallbackLogin;

		public static bool SuppressActiveDirectoryFallbackLogin
		{
			get => suppressActiveDirectoryFallbackLogin;
			set
			{
				if (suppressActiveDirectoryFallbackLogin == value) return;
				suppressActiveDirectoryFallbackLogin = value;
				log.Info($"[SuppressActiveDirectoryFallbackLogin] = '{value}'");
			}
		}

		private static DisplayOptions? displayOptions;

		public static DisplayOptions? DisplayOptions
		{
			get => displayOptions;
			set
			{
				if (displayOptions == value) return;
				displayOptions = value;
				log.Info($"[DisplayOptions] = '{value}'");
				if (displayOptions != null) return;
				LocalSettingsForUser.DisplaySummaDelta = AppConfig.Current.DisplaySummaDelta;
			}
		}

		private static void SetNotificationParams(string param, Action<int> notificationIntervalSetter, int notificationIntervalDefault, Action<int> notificationDurationSetter, int notificationDurationDefault)
		{
			if (string.IsNullOrEmpty(param))
			{
				notificationIntervalSetter(notificationIntervalDefault);
				notificationDurationSetter(notificationDurationDefault);
				return;
			}

			var pars = param.Split(new [] {','}, StringSplitOptions.RemoveEmptyEntries);
			if (pars.Length < 1 || !int.TryParse(pars[0], out var first)) first = 0;
			if (notificationIntervalSetter == null)
			{
				notificationDurationSetter(first);
				return;
			} 
			if (pars.Length < 2 || !int.TryParse(pars[1], out var second)) second = 0;
			notificationIntervalSetter(first);
			notificationDurationSetter?.Invoke(second);
		}

		private static string workStateNotificationParams;

		public static string WorkStateNotificationParams
		{
			get => workStateNotificationParams;
			set
			{
				if (workStateNotificationParams == value) return;
				workStateNotificationParams = value;
				log.Info($"[WorkStateNotificationParams] = '{value}'");
				SetNotificationParams(workStateNotificationParams, v => LocalSettingsForUser.WorkingWarnInterval = v, AppConfig.Current.WorkingWarnInterval, v => LocalSettingsForUser.WorkingWarnDuration = v, AppConfig.Current.WorkingWarnDuration);
				LocalSettingsForUser.IsWorkingWarnDisplayable = string.IsNullOrEmpty(workStateNotificationParams) || LocalSettingsForUser.WorkingWarnInterval >= 0 || LocalSettingsForUser.WorkingWarnDuration >= 0 || LocalSettingsForUser.NotWorkingWarnInterval >= 0 || LocalSettingsForUser.NotWorkingWarnDuration >=0;
			}
		}

		private static string nonWorkStateNotificationParams;

		public static string NonWorkStateNotificationParams
		{
			get => nonWorkStateNotificationParams;
			set
			{
				if (nonWorkStateNotificationParams == value) return;
				nonWorkStateNotificationParams = value;
				log.Info($"[NonWorkStateNotificationParams] = '{value}'");
				SetNotificationParams(nonWorkStateNotificationParams, v => LocalSettingsForUser.NotWorkingWarnInterval = v, AppConfig.Current.NotWorkingWarnInterval, v => LocalSettingsForUser.NotWorkingWarnDuration = v, AppConfig.Current.NotWorkingWarnDuration);
				LocalSettingsForUser.IsWorkingWarnDisplayable = string.IsNullOrEmpty(workStateNotificationParams) || LocalSettingsForUser.WorkingWarnInterval >= 0 || LocalSettingsForUser.WorkingWarnDuration >= 0 || LocalSettingsForUser.NotWorkingWarnInterval >= 0 || LocalSettingsForUser.NotWorkingWarnDuration >= 0;
			}
		}

		private static string tasksChangedNotificationParams;

		public static string TasksChangedNotificationParams
		{
			get => tasksChangedNotificationParams;
			set
			{
				if (tasksChangedNotificationParams == value) return;
				tasksChangedNotificationParams = value;
				log.Info($"[TasksChangedNotificationParams] = '{value}'");
				SetNotificationParams(tasksChangedNotificationParams, null, 0, v => LocalSettingsForUser.MenuChangeWarnDuration = v, AppConfig.Current.MenuChangeWarnDuration);
			}
		}

		private static bool autoStartClientOnNonWorkDays;

		public static bool AutoStartClientOnNonWorkDays
		{
			get => autoStartClientOnNonWorkDays;
			set
			{
				if (autoStartClientOnNonWorkDays == value) return;
				autoStartClientOnNonWorkDays = value;
				log.Info($"[AutoStartClientOnNonWorkDays] = '{value}'");
			}
		}

		private static int? adHocMeetingDefaultSelectedTaskId;
		public static int? AdHocMeetingDefaultSelectedTaskId
		{
			get => adHocMeetingDefaultSelectedTaskId;
			set
			{
				if (adHocMeetingDefaultSelectedTaskId == value) return;
				adHocMeetingDefaultSelectedTaskId = value;
				log.Info("[AdHocMeetingDefaultSelectedTaskId = '" + value + "'");
			}
		}

		private static bool isVideoRecordingEnabled;
		public static bool IsVideoRecordingEnabled
		{
			get => isVideoRecordingEnabled;
			set
			{
				if (isVideoRecordingEnabled == value) return;
				isVideoRecordingEnabled = value;
				log.Info("[IsVideoRecordingEnabled = '" + value + "'");
			}
		}

		private static float videoRecordingFps;
		public static float VideoRecordingFps
		{
			get => videoRecordingFps;
			set
			{
				if (Math.Abs(videoRecordingFps - value) < 1e-8) return;
				videoRecordingFps = value;
				log.Info("[VideoRecordingFps = '" + value + "'");
			}
		}

		private static int videoRecordingBitRate;
		public static int VideoRecordingBitRate
		{
			get => videoRecordingBitRate;
			set
			{
				if (videoRecordingBitRate == value) return;
				videoRecordingBitRate = value;
				log.Info("[VideoRecordingBitRate = '" + value + "'");
			}
		}

		private static bool isWorkEnabledOutsideWorkTimeStartEnd;
		public static bool IsWorkEnabledOutsideWorkTimeStartEnd
		{
			get => isWorkEnabledOutsideWorkTimeStartEnd;
			set
			{
				if (isWorkEnabledOutsideWorkTimeStartEnd == value) return;
				isWorkEnabledOutsideWorkTimeStartEnd = value;
				log.Info("[IsWorkEnabledOutsideWorkTimeStartEnd = '" + value + "'");
			}
		}

		public static SetWorkStateAfterResume SetWorkStateAfterResume { get; }

		private const string userIdPath = "DefaultUserIdPassword";
		private const string protectedUserIdPath = "ProtectedDefaultUserIdPassword";
		private const string proxySettingPath = "ProxySettings";
		private static void UserIdPasswordSave()
		{
			var userData = new UserIdPassword() { Id = UserId, Password = UserPassword, PasswordExpirationDate = UserPasswordExpirationDate, Name = UserName };
			ProtectedDataSerializationHelper.Save(protectedUserIdPath, userData);
		}
		private static void UserIdPasswordDelete()
		{
			if (ProtectedDataSerializationHelper.Exists(protectedUserIdPath))
			{
				ProtectedDataSerializationHelper.Delete(protectedUserIdPath);
			}
		}
		private static void UserIdPasswordLoad()
		{
			if (MigratePassword()) return;

			UserIdPassword userData;
			if (ProtectedDataSerializationHelper.Exists(protectedUserIdPath) && ProtectedDataSerializationHelper.Load(protectedUserIdPath, out userData))
			{
				UserId = userData.Id;
				UserPassword = userData.Password;
				UserPasswordExpirationDate = userData.PasswordExpirationDate;
				UserName = userData.Name;
			}
		}
		private static bool MigratePassword()
		{
			UserIdPassword userData;
			if (IsolatedStorageSerializationHelper.Exists(userIdPath) && IsolatedStorageSerializationHelper.Load(userIdPath, out userData))
			{
				UserId = userData.Id;
				UserPassword = userData.Password;
				UserName = userData.Name;
				IsolatedStorageSerializationHelper.Delete(userIdPath);
				UserIdPasswordSave();
				return true;
			}
			return false;
		}

		public static void RefreshUserIdPassword()
		{
			UserIdPasswordLoad();
		}

		public static LocalSettings LocalSettingsForUser { get; private set; }

		static ConfigManager()
		{
			var logRepository = LogManager.GetRepository() as log4net.Repository.Hierarchy.Hierarchy;
			log4net.Core.Level initialLevel = null;
			if (logRepository != null)
			{
				initialLevel = logRepository.Root.Level;
				logRepository.Root.Level = logRepository.LevelMap["ERROR"];
			}
			var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			var pFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
			var pFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
			var jcLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			JcLocalPath = jcLocation.StartsWith(localAppData) || jcLocation.StartsWith(pFiles) || !string.IsNullOrEmpty(pFilesX86) && jcLocation.StartsWith(pFilesX86) ? Path.Combine(localAppData, "JobCTRL") : jcLocation;
			try
			{
				var updateService = Platform.Factory.GetUpdateService();
				VersionAutoUpdate = updateService.CurrentVersion;
				IsAppLevelStorageNeeded = updateService.GetAppPath().EndsWith(".appref-ms"); //hax for CO detection
			}
			catch (Exception ex)
			{
				log.ErrorAndFail("Unable to retrieve autoupdate version", ex);
			}
			TelemetryHelper.Observe(TelemetryHelper.KeyOsVersion, Environment.OSVersion.ToString());
			TelemetryHelper.Observe(TelemetryHelper.KeyNetVersion, Environment.Version.ToString());
			TelemetryHelper.Observe(TelemetryHelper.KeyJcVersion, Version.ToString());
			var appClsRaw = AppConfig.Current.AppClassifier;
			if (appClsRaw != null)
			{
				Classifier = SimpleEncoder.Decode(appClsRaw);
#if DEBUG
				if (Classifier == null)
					log.Debug("Encoded classifier: " + SimpleEncoder.Encode(appClsRaw));
#endif
			}
			MutexName = AppConfig.Current.MutexName;
			DisableManualStatusChange = AppConfig.Current.DisableManualStatusChange;
			IssuePropColumns = AppConfig.Current.IssuePropColumns;
			autoReturnFromMeeting = AppConfig.Current.AutoReturnFromMeeting;
			capturingDeadlockInMins = AppConfig.Current.CapturingDeadlockInMins;
			captureWorkItemInterval = AppConfig.Current.CaptureWorkItemInterval;
			captureActiveWindowInterval = AppConfig.Current.CaptureActiveWindowInterval;
			captureScreenShotInterval = AppConfig.Current.CaptureScreenShotInterval;
			timeSyncThreshold = AppConfig.Current.TimeSyncThreshold;
			jpegQuality = AppConfig.Current.JpegQuality;
			jpegScalePct = AppConfig.Current.JpegScalePct;
			menuUpdateInterval = AppConfig.Current.MenuUpdateInterval;
			workTimeStartInMins = AppConfig.Current.WorkTimeStartInMins;
			workTimeEndInMins = AppConfig.Current.WorkTimeEndInMins;
			afterWorkTimeIdleInMins = AppConfig.Current.AfterWorkTimeIdleInMins;
			usageSaveInSecs = AppConfig.Current.UsageSaveInSeconds;
			maxOfflineWorkItems = AppConfig.Current.MaxOfflineWorkItems;
			duringWorkTimeIdleInMins = AppConfig.Current.DuringWorkTimeIdleInMins;
			duringWorkTimeIdleManualInterval = AppConfig.Current.DuringWorkTimeIdleManualInterval;
			maxManualMeetingInterval = AppConfig.Current.MaxManualMeetingInterval;
			ruleRestrictions = AppConfig.Current.RuleRestrictions;
			isMeetingSubjectMandatory = AppConfig.Current.IsMeetingSubjectMandatory;
			isOutlookMeetingTrackingEnabled = AppConfig.Current.IsOutlookMeetingTrackingEnabled;
			isLotusNotesMeetingTrackingEnabled = AppConfig.Current.IsLotusNotesMeetingTrackingEnabled;
			busyTimeThreshold = AppConfig.Current.BusyTimeThreshold;
			coincidentalClientsEnabled = AppConfig.Current.CoincidentalClientsEnabled;
			isManualMeetingStartsOnLock = AppConfig.Current.IsManualMeetingStartsOnLock;
			ruleMatchingInterval = AppConfig.Current.RuleMatchingInterval;
			pluginFailThreshold = AppConfig.Current.PluginFailThreshold;
			isOutlookAddinRequired = AppConfig.Current.IsOutlookAddinRequired;
			isOutlookAddinMailTrackingId = AppConfig.Current.IsOutlookAddinMailTrackingId;
			telemetryCollectedKeys = AppConfig.Current.TelemetryCollectedKeys;
			telemetryMaxAgeInMins = AppConfig.Current.TelemetryMaxAgeInMins;
			telemetryMaxCount = AppConfig.Current.TelemetryMaxCount;
			isOutlookAddinMailTrackingUseSubject = AppConfig.Current.IsOutlookAddinMailTrackingUseSubject;
			useRedemptionForMeetingSync = AppConfig.Current.UseRedemptionForMeetingSync;
			oCRLanguage = AppConfig.Current.OCRLanguage;
			isMeetingUploadModifications = AppConfig.Current.IsMeetingUploadModifications;
			isMeetingTentativeSynced = AppConfig.Current.IsMeetingTentativeSynced;
			isInjectedInputAllowed = AppConfig.Current.IsInjectedInputAllowed;
			clientDataCollectionSettings = AppConfig.Current.ClientDataCollectionSettings;
			mouseMovingThreshold = AppConfig.Current.MouseMovingThreshold;
			msProjectAddress = AppConfig.Current.MsProjectAddress;
			forceCountdownRules = AppConfig.Current.ForceCountdownRules;
			collectedItemAggregateInMins = AppConfig.Current.CollectedItemAggregateInMins;
			manualWorkItemEditAgeLimit = AppConfig.Current.ManualWorkItemEditAgeLimit;
			selfLearningOkValidity = AppConfig.Current.SelfLearningOkValidity;
			autoUpdateManagerEnabled = AppConfig.Current.AutoUpdateManagerEnabled;
			isLoginRememberPasswordChecked = AppConfig.Current.IsLoginRememberPasswordChecked;
			onlyDesktopTasksInWorktimeMod = AppConfig.Current.OnlyDesktopTasksInWorktimeMod;
			suppressActiveDirectoryFallbackLogin =  AppConfig.Current.SuppressActiveDirectoryFallbackLogin;
			WebsiteUrl = AppConfig.Current.WebsiteUrl;
			if (!WebsiteUrl.EndsWith("/")) WebsiteUrl += "/";
			WebsiteUrlFormatString = WebsiteUrl + "Account/Login.aspx?ticket={0}&url=/UserCenter/";
			HttpApplicationUpdateUrl = WebsiteUrl + "UserCenter/ClientUpdate.aspx";
			ValidCertificate = AppConfig.Current.ValidCertificate;
			IsRunAsAdminDefault = AppConfig.Current.IsRunAsAdminDefault;
			IsMeetingDescriptionSynchronized = AppConfig.Current.IsMeetingDescriptionSynchronized;
			IsMeetingAppointmentSynchronized = AppConfig.Current.IsMeetingAppointmentSynchronized;
			CalendarFolderInclusionPattern = AppConfig.Current.CalendarFolderInclusionPattern;
			CalendarFolderExclusionPattern = AppConfig.Current.CalendarFolderExclusionPattern;
			AppNameOverride = AppConfig.Current.AppNameOverride;
			IsOutlookAddinMailTrackingUseSubject = AppConfig.Current.IsOutlookAddinMailTrackingUseSubject;
			UseRedemptionForMeetingSync = AppConfig.Current.UseRedemptionForMeetingSync;
			OCRLanguage = AppConfig.Current.OCRLanguage;
			GoogleClientId = AppConfig.Current.GoogleClientId;
			GoogleClientSecret = AppConfig.Current.GoogleClientSecret;
			IssueCategories = AppConfig.Current.IssueCategories;
			SelfLearningOkValidity = AppConfig.Current.SelfLearningOkValidity;
			OnlyDesktopTasksInWorktimeMod = AppConfig.Current.OnlyDesktopTasksInWorktimeMod;
			IsRoamingStorageScopeNeeded = AppConfig.Current.IsRoamingStorageScopeNeeded;    //This must preceed the call of UserIdPasswordLoad
			IsTaskBarIconShowing = AppConfig.Current.IsTaskBarIconShowing;
			StartWorkAfterLogin = AppConfig.Current.StartWorkAfterLogin;
			SetWorkStateAfterResume = AppConfig.Current.SetWorkStateAfterResume;

			if (initialLevel != null) logRepository.Root.Level = initialLevel; // restore initial log level
			EnvironmentInfo = Platform.Factory.GetEnvironmentInfoService(); //todo should we allow invalid compid ? 
			log.Info("Initializing " + ApplicationName + " " + (VersionAutoUpdate != null && AutoUpdateManagerEnabled ? "AutoUpdate " : "NonAutoUpdate ") + (IsAppLevelStorageNeeded ? "CO " : "WiX ") + DebugOrReleaseString + " Ver.:" + VersionWithClassifier);
			log.Info(string.Format("Machinename: {0}, OSVersion: {1}, Framework version: {2}, Bitness: {3}", Environment.MachineName, EnvironmentInfo.OSVersion + (EnvironmentInfo.OSFullName != null ? " (" + EnvironmentInfo.OSFullName + ")": ""), Environment.Version, IntPtr.Size == 4 ? "x86" : "x64"));
			log.Info("TickCount: " + Environment.TickCount + " (" + TimeSpan.FromMilliseconds(Environment.TickCount).ToHourMinuteSecondString() + ") Now: " + DateTime.Now + " UtcNow: " + DateTime.UtcNow);
			log.Info("Computer Id: " + EnvironmentInfo.ComputerId);
			using (var process = Process.GetCurrentProcess())
			{
				CurrentProcessPid = process.Id;
				log.Info("Current pid is " + CurrentProcessPid);
			}
			log.Info("Current directory: " + Directory.GetCurrentDirectory());

			LogPath = LogManager.GetRepository().GetAppenders().OfType<log4net.Appender.FileAppender>()
				.Select(n => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(n.File), "..")))
				.DefaultIfEmpty(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location))
				.First();
			log.Info("LogPath: " + LogPath);

			LocalSettingsForUser = LocalSettings.Empty;
			LocalSettingsForUser.ShowDynamicWorks = AppConfig.Current.ShowDynamicWorks;
			//http://andrewensley.com/2009/06/c-detect-windows-os-part-1/
			if (IsWindows7) log.Info("Windows7 or later detected");

			UserIdPasswordLoad();

			LoadProxySettings();

			log.Info("Client Settings Defaults: [CaptureWorkItemInterval] = '" + CaptureWorkItemInterval + "'"
				+ ", [CaptureActiveWindowInterval] = '" + CaptureActiveWindowInterval + "'"
				+ ", [CaptureScreenShotInterval] = '" + CaptureScreenShotInterval + "'"
				+ ", [TimeSyncThreshold] = '" + TimeSyncThreshold + "'"
				+ ", [JpegQuality] = '" + JpegQuality + "'"
				+ ", [JpegScalePct] = '" + JpegScalePct + "'"
				+ ", [MenuUpdateInterval] = '" + MenuUpdateInterval + "'"
				+ ", [WorkTimeStartInMins] = '" + WorkTimeStartInMins + "'"
				+ ", [WorkTimeEndInMins] = '" + WorkTimeEndInMins + "'"
				+ ", [AfterWorkTimeIdleInMins] = '" + AfterWorkTimeIdleInMins + "'"
				+ ", [MaxOfflineWorkItems] = '" + MaxOfflineWorkItems + "'"
				+ ", [DuringWorkTimeIdleInMins] = '" + DuringWorkTimeIdleInMins + "'"
				+ ", [DuringWorkTimeIdleManualInterval] = '" + DuringWorkTimeIdleManualInterval + "'"
				+ ", [MaxManualMeetingInterval] = '" + MaxManualMeetingInterval + "'"
				+ ", [RuleRestrictions] = '" + RuleRestrictions + "'"
				+ ", [IsOutlookMeetingTrackingEnabled] = '" + IsOutlookMeetingTrackingEnabled + "'"
				+ ", [IsLotusNotesMeetingTrackingEnabled] = '" + IsLotusNotesMeetingTrackingEnabled + "'"
				+ ", [IsMeetingSubjectMandatory] = '" + IsMeetingSubjectMandatory + "'"
				+ ", [BusyTimeThreshold] = '" + BusyTimeThreshold + "'"
				+ ", [CoincidentalClientsEnabled] = '" + CoincidentalClientsEnabled + "'"
				+ ", [IsManualMeetingStartsOnLock] = '" + IsManualMeetingStartsOnLock + "'"
				+ ", [RuleMatchingInterval] = '" + RuleMatchingInterval + "'"
				+ ", [IsOutlookAddinRequired] = '" + IsOutlookAddinRequired + "'"
				+ ", [IsOutlookAddinMailTrackingId] = '" + IsOutlookAddinMailTrackingId + "'"
				+ ", [MouseMovingThreshold] = '" + MouseMovingThreshold + "'"
				+ ", [PluginFailThreshold] = '" + PluginFailThreshold + "'"
				+ ", [CollectedItemAggregateInMins] = '" + CollectedItemAggregateInMins + "'"
				+ ", [ForceCountdownRules] = '" + ForceCountdownRules + "'"
				+ ", [MsProjectAddress] = '" + MsProjectAddress + "'"
				+ ", [ManualWorkItemEditAgeLimit] = '" + ManualWorkItemEditAgeLimit + "'"
				+ ", [AutoWorkingStatusChange] = '" + AutoReturnFromMeeting + "'"
				+ ", [IsOutlookAddinMailTrackingUseSubject] = '" + IsOutlookAddinMailTrackingUseSubject + "'"
				+ ", [UseRedemptionForMeetingSync] = '" + UseRedemptionForMeetingSync + "'"
				+ ", [AutoUpdateManagerEnabled] = '" + AutoUpdateManagerEnabled + "'"
				+ ", [IsTodoListEnabled] = '" + IsTodoListEnabled + "'"
				+ ", [MailTrackingSettings] = '" + MailTrackingSettings + "'"
				+ ", [DiagnosticOperationMode] = '" + DiagnosticOperationMode + "'"
			    + ", [IsGoogleCalendarTrackingEnabled] = '" + IsGoogleCalendarTrackingEnabled + "'"
				+ ", [IsAnonymModeEnabled] = '" + IsAnonymModeEnabled + "'"
		        + ", [OnlyDesktopTasksInWorktimeMod] = '" + OnlyDesktopTasksInWorktimeMod + "'"
				+ ", [DisplayOptions] = '" + DisplayOptions + "'"
				+ ", [WorkStateNotificationParams] = '" + WorkStateNotificationParams + "'"
				+ ", [NonWorkStateNotificationParams] = '" + NonWorkStateNotificationParams + "'"
				+ ", [TasksChangedNotificationParams] = '" + TasksChangedNotificationParams + "'"
				+ ", [AutoStartClientOnNonWorkDays] = '" + AutoStartClientOnNonWorkDays + "'"
				+ ", [AdHocMeetingDefaultSelectedTaskId] = '" + AdHocMeetingDefaultSelectedTaskId + "'"
);
		}

		public static void Initialize()
		{
			//do nothing, this is to make sure cctor would run
		}

		public static bool EnsureLoggedIn(Func<LoginData> loginDataFactory)
		{
			//if the userId and password was saved to the disk
			if (ConfigManager.UserId != ConfigManager.LoggedOutUserId) return true;

			Debug.Assert(loginDataFactory != null);
			var loginData = loginDataFactory();
			return SetLoginData(loginData, false);
		}

		public static void ShowPasswordDialog(Func<LoginData, LoginData> passwordDataFactory)
		{
			Debug.Assert(passwordDataFactory != null);
			var loginDataIn = new LoginData()
			{
				UserId = ConfigManager.UserId,
				RememberMe = ProtectedDataSerializationHelper.Exists(protectedUserIdPath),
			};

			var loginData = passwordDataFactory(loginDataIn);
			SetLoginData(loginData, true);
		}

		public static void RefreshPasswordTo(LoginData loginData)
		{
			if (loginData == null) return;
			Debug.Assert(loginData.UserId == ConfigManager.UserId);
			SetLoginData(loginData, true);
		}

		public class LoginData
		{
			public int UserId { get; set; }
			public string UserPassword { get; set; }
			public DateTime? UserPasswordExpirationDate { get; set; }
			public bool RememberMe { get; set; }
			public bool StartWorkAfterLogin { get; set; }
			public AuthData AuthData { get; set; }
			public System.Globalization.CultureInfo Culture { get; set; }
		}

		//hax Concurency between ConfigManager.ShowPasswordDialog and ConfigManager.RefreshPasswordTo causes race on LoginData related properties.
		private static readonly object sharedLock = new object();
		private static bool SetLoginData(LoginData loginData, bool isPasswordChange)
		{
			if (loginData == null)
			{
				log.Info("Login " + (isPasswordChange ? "password change " : "") + "cancelled");
				return false;
			}
			lock (sharedLock)
			{
				if (!isPasswordChange) ConfigManager.UserId = loginData.UserId;
				ConfigManager.UserPassword = loginData.UserPassword;
				ConfigManager.UserPasswordExpirationDate = loginData.UserPasswordExpirationDate;
				StartWorkAfterLogin = loginData.StartWorkAfterLogin;
				SetAuthData(loginData.AuthData);
				if (loginData.RememberMe)
				{
					ConfigManager.UserIdPasswordSave();
					if (loginData.Culture != null) LocalizationHelper.SaveLocalization(loginData.Culture);
				}
				else
				{
					ConfigManager.UserIdPasswordDelete();
				}
				log.Info("Login " + (isPasswordChange ? "password change " : "") + "for user: " + ConfigManager.UserId);
				return true;
			}
		}

		private static void LoadProxySettings()
		{
			ProxySettings settings;
			if (ProtectedDataSerializationHelper.Exists(proxySettingPath) &&
			    ProtectedDataSerializationHelper.Load(proxySettingPath, out settings))
			{
				proxySettings = settings;
			}
			else
			{
				proxySettings = new ProxySettings();
			}
		}

		private static ProxySettings proxySettings;
		public static ProxySettings Proxy
		{
			get
			{
				return proxySettings;
			}

			set
			{
				proxySettings = value;
				ProtectedDataSerializationHelper.Save(proxySettingPath, value);
			}
		}

		public static bool IsAuthDataRequired { get { return ConfigManager.UserName == null; } }

		public static void SetAndSaveAuthDataIfApplicable(AuthData authData)
		{
			lock (sharedLock)
			{
				SetAuthData(authData);
				if (authData == null ||
				    authData.FullName == ConfigManager.UserName) return;
				var rememberMe = ProtectedDataSerializationHelper.Exists(protectedUserIdPath);
				if (rememberMe)
				{
					ConfigManager.UserIdPasswordSave();
				}
			}
		}

		private static void SetAuthData(AuthData authData)
		{
			if (authData == null) return;
			ConfigManager.UserName = authData.FullName;
			ConfigManager.AccessLevel = authData.AccessLevel;
			if (authData.TimeZoneData != null)
				ConfigManager.TimeZoneWeb = TimeZoneInfo.FromSerializedString(authData.TimeZoneData);
			else
			{
				ConfigManager.TimeZoneWeb = TimeZoneInfo.Local;
				log.Warn("No timezone data from server");
			}
		}

		public static void Logout()
		{
			ConfigManager.UserId = ConfigManager.LoggedOutUserId;
			ConfigManager.UserIdPasswordDelete();
		}

		[Serializable]
		[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
		public class LocalSettings
		{
			[OptionalField]
			private int? notWorkingWarnInterval;
			public int NotWorkingWarnInterval
			{
				get { return notWorkingWarnInterval ?? AppConfig.Current.NotWorkingWarnInterval; }
				set
				{
					if (notWorkingWarnInterval == value) return;
					notWorkingWarnInterval = value;
					log.Info("[NotWorkingWarnInterval] = '" + value + "'");
					SaveSettings();
				}
			}

			[OptionalField]
			private int? workingWarnInterval;
			public int WorkingWarnInterval
			{
				get { return workingWarnInterval ?? AppConfig.Current.WorkingWarnInterval; }
				set
				{
					if (workingWarnInterval == value) return;
					workingWarnInterval = value;
					log.Info("[WorkingWarnInterval] = '" + value + "'");
					SaveSettings();
				}
			}

			[OptionalField]
			private Keys? manualMeetingHotKey;
			public Keys? ManualMeetingHotKey
			{
				get { return manualMeetingHotKey ?? AppConfig.Current.ManualMeetingHotKey; }
				set
				{
					if (manualMeetingHotKey == value) return;
					manualMeetingHotKey = value;
					log.Info("[ManualMeetingHotKey] = '" + value + "'");
					SaveSettings();
				}
			}

			[OptionalField]
			private Keys? clearAutoRuleTimersHotKey;
			public Keys? ClearAutoRuleTimersHotKey
			{
				get { return clearAutoRuleTimersHotKey ?? AppConfig.Current.ClearAutoRuleTimersHotKey; }
				set
				{
					if (clearAutoRuleTimersHotKey == value) return;
					clearAutoRuleTimersHotKey = value;
					log.Info("[ClearAutoRuleTimersHotKey] = '" + value + "'");
					SaveSettings();
				}
			}

			[OptionalField]
			private Keys? hotKey;
			public Keys? HotKey
			{
				get { return hotKey; }
				set
				{
					if (hotKey == value) return;
					hotKey = value;
					log.Info("[HotKey] = '" + value + "'");
					SaveSettings();
				}
			}

			[OptionalField]
			private int? notWorkingWarnDuration;
			public int NotWorkingWarnDuration
			{
				get { return notWorkingWarnDuration ?? AppConfig.Current.NotWorkingWarnDuration; }
				set
				{
					if (notWorkingWarnDuration == value) return;
					notWorkingWarnDuration = value;
					log.Info("[NotWorkingWarnDuration] = '" + value + "'");
					SaveSettings();
				}
			}

			[OptionalField]
			private int? workingWarnDuration;
			public int WorkingWarnDuration
			{
				get { return workingWarnDuration ?? AppConfig.Current.WorkingWarnDuration; }
				set
				{
					if (workingWarnDuration == value) return;
					workingWarnDuration = value;
					log.Info("[WorkingWarnDuration] = '" + value + "'");
					SaveSettings();
				}
			}

			[OptionalField]
			private int? menuChangeWarnDuration;
			public int MenuChangeWarnDuration
			{
				get { return menuChangeWarnDuration ?? AppConfig.Current.MenuChangeWarnDuration; }
				set
				{
					if (menuChangeWarnDuration == value) return;
					menuChangeWarnDuration = value;
					log.Info("[MenuChangeWarnDuration] = '" + value + "'");
					SaveSettings();
				}
			}

			[OptionalField]
			private bool? useDoubleClickForStatusChange;
			public bool? UseDoubleClickForStatusChange
			{
				get
				{
					if (DisableManualStatusChange) return null;
					return useDoubleClickForStatusChange ?? AppConfig.Current.UseDoubleClickForStatusChange;
				}
				set
				{
					if(useDoubleClickForStatusChange == value) return; 
					useDoubleClickForStatusChange = value;
					log.Info("[UseDoubleClickForStatusChange] = '" + value + "'");
					SaveSettings();
				}
			}

			[OptionalField]
			private int? menuFlattenFactor;
			public int MenuFlattenFactor
			{
				get { return menuFlattenFactor ?? AppConfig.Current.MenuFlattenFactor; }
				set
				{
					if (menuFlattenFactor == value) return;
					menuFlattenFactor = value;
					log.Info("[MenuFlattenFactor] = '" + value + "'");
					SaveSettings();
				}
			}

			[OptionalField]
			private bool? displayThisWeeksStats;
			public bool DisplayThisWeeksStats
			{
				get { return displayThisWeeksStats ?? (AppConfig.Current.DisplayWorktimeStats & WorktimeStatIntervals.Week) > 0; }
				set
				{
					if (displayThisWeeksStats == value) return;
					displayThisWeeksStats = value;
					log.Info("[DisplayThisWeeksStats] = '" + value + "'");
					SaveSettings();
				}
			}

			[OptionalField]
			private WorktimeStatIntervals? displayWorktimeStats;
			public WorktimeStatIntervals DisplayWorktimeStats
			{
				get { return displayWorktimeStats ?? AppConfig.Current.DisplayWorktimeStats | (DisplayThisWeeksStats ? WorktimeStatIntervals.Week : 0); }
				set
				{
					if (displayWorktimeStats == value) return;
					displayWorktimeStats = value;
					log.Info("[DisplayWorktimeStats] = '" + value + "'");
					SaveSettings();
				}
			}

			[OptionalField]
			private int? menuTopItemsCount;
			public int MenuTopItemsCount
			{
				get { return menuTopItemsCount ?? AppConfig.Current.MenuTopItemsCount; }
				set
				{
					if (menuTopItemsCount == value) return;
					menuTopItemsCount = value;
					log.Info("[MenuTopItemsCount] = '" + value + "'");
					SaveSettings();
				}
			}

			[OptionalField]
			private bool? displaySearchWorks;
			public bool DisplaySearchWorks
			{
				get { return displaySearchWorks ?? AppConfig.Current.DisplaySearchWorks; }
				set
				{
					if (displaySearchWorks == value) return;
					displaySearchWorks = value;
					log.Info("[DisplaySearchWorks] = '" + value + "'");
					SaveSettings();
				}
			}

			[OptionalField]
			private bool? displaySummaDelta;
			public bool DisplaySummaDelta
			{
				get { return displaySummaDelta ?? AppConfig.Current.DisplaySummaDelta; }
				set
				{
					if (displaySummaDelta == value) return;
					displaySummaDelta = value;
					log.Info("[DisplaySummaDelta] = '" + value + "'");
					SaveSettings();
				}
			}

			[OptionalField]
			private int? menuRecentItemsCount;
			public int MenuRecentItemsCount
			{
				get { return menuRecentItemsCount ?? AppConfig.Current.MenuRecentItemsCount; }
				set
				{
					if (menuRecentItemsCount == value) return;
					menuRecentItemsCount = value;
					log.Info("[MenuRecentItemsCount] = '" + value + "'");
					SaveSettings();
				}
			}

			[OptionalField]
			private bool? searchOwnTasks;
			public bool SearchOwnTasks
			{
				get { return /*searchOwnTasks ??*/ AppConfig.Current.SearchOwn; }
				set
				{
					if (searchOwnTasks == value) return;
					searchOwnTasks = value;
					log.Info("[SearchOwnTasks] = '" + value + "'");
					SaveSettings();
				}
			}

			[OptionalField]
			private bool? searchInClosed;
			public bool SearchInClosed
			{
				get { return /*searchInClosed ??*/ AppConfig.Current.SearchClosed; }
				set
				{
					if (searchInClosed == value) return;
					searchInClosed = value;
					log.Info("[SearchInClosed] = '" + value + "'");
					SaveSettings();
				}
			}

			[OptionalField]
			private bool? highlightNonReasonedWork;
			public bool HighlightNonReasonedWork
			{
				get { return highlightNonReasonedWork ?? false; }
				set
				{
					if (highlightNonReasonedWork == value) return;
					highlightNonReasonedWork = value;
					log.Info("[HighlightNonReasonedWork] = '" + value + "'");
					SaveSettings();
				}
			}

			[OptionalField]
			private string recentWorksInAddMeeting;
			public string RecentWorksInAddMeeting
			{
				get { return recentWorksInAddMeeting; }
				set
				{
					if (recentWorksInAddMeeting == value) return;
					recentWorksInAddMeeting = value;
					log.Info("[RecentWorksInAddMeeting] = '" + value + "'");
					SaveSettings();
				}
			}

			[OptionalField]
			private NotificationPosition? notificationPosition;
			public NotificationPosition NotificationPosition
			{
				get { return notificationPosition ?? AppConfig.Current.NotificationPosition; }
				set
				{
					if (notificationPosition == value) return;
					notificationPosition = value;
					log.Info("[NotificationPosition] = '" + value + "'");
					SaveSettings();
				}
			}

			[OptionalField]
			private string taskPlaceholder;
			public string TaskPlaceholder
			{
				get { return taskPlaceholder ?? AppConfig.Current.TaskPlaceholder; }
				set
				{
					if (taskPlaceholder == value) return;
					taskPlaceholder = value;
					log.Info("[TaskPlaceholder] = '" + value + "'");
					SaveSettings();
				}
			}

			[OptionalField]
			private bool? isWorkingWarnDisplayable;
			public bool IsWorkingWarnDisplayable
			{
				get { return isWorkingWarnDisplayable ?? AppConfig.Current.IsWorkingWarnDisplayable; }
				set
				{
					if (isWorkingWarnDisplayable == value) return;
					isWorkingWarnDisplayable = value;
					log.Info("[IsWorkingWarnDisplayable] = '" + value + "'");
					SaveSettings();
				}
			}

			[OptionalField]
			private bool? showDynamicWorks;
			public bool ShowDynamicWorks
			{
				get { return showDynamicWorks ?? AppConfig.Current.ShowDynamicWorks; }
				set
				{
					if (showDynamicWorks == value) return;
					showDynamicWorks = value;
					log.Info("[ShowDynamicWorks] = '" + value + "'");
					SaveSettings();
				}
			}

			[OptionalField]
			private bool? showOldMenu;

			public bool ShowOldMenu
			{
				get { return showOldMenu ?? AppConfig.Current.ShowOldMenu; }
				set
				{
					if (showOldMenu == value) return;
					showOldMenu = value;
					log.Info("[ShowOldMenu] = '" + value + "'");
					SaveSettings();
				}
			}

			[OptionalField]
			private bool? autoSendLogFiles;

			public bool? AutoSendLogFiles
			{
				get => autoSendLogFiles ?? AppConfig.Current.AutoSendLogFiles;
				set
				{
					if (autoSendLogFiles == value) return;
					autoSendLogFiles = value;
					log.Info($"[AutoSendLogFiles] = '{value}'");
					SaveSettings();
				}
			}

			[OptionalField]
			private bool? idleAlertVisual;

			public bool IdleAlertVisual
			{
				get => idleAlertVisual ?? AppConfig.Current.IdleAlertVisual;
				set
				{
					if (idleAlertVisual == value) return;
					idleAlertVisual = value;
					log.Info($"[IdleAlertVisual] = '{value}'");
					SaveSettings();
				}
			}

			[OptionalField]
			private bool? idleAlertBeep;

			public bool IdleAlertBeep
			{
				get => idleAlertBeep ?? AppConfig.Current.IdleAlertBeep;
				set
				{
					if (idleAlertBeep == value) return;
					idleAlertBeep = value;
					log.Info($"[IdleAlertBeep] = '{value}'");
					SaveSettings();
				}
			}

			[OptionalField]
			private bool? isSafeMailItemCommitUsable;

			public bool IsSafeMailItemCommitUsable
			{
				get => isSafeMailItemCommitUsable ?? AppConfig.Current.IsSafeMailItemCommitUsable;
				set
				{
					if (isSafeMailItemCommitUsable == value) return;
					isSafeMailItemCommitUsable = value;
					log.Info($"[IsSafeMailItemCommitUsable] = '{value}'");
					SaveSettings();
				}
			}

			[OptionalField]
			private ScreenshotAnalyzerConfigs screenshotAnalyzerConfigs;

			public ScreenshotAnalyzerConfigs ScreenshotAnalyzerConfigs
			{
				get => screenshotAnalyzerConfigs ?? ScreenshotAnalyzerConfigs.Default;
				set
				{
					if (screenshotAnalyzerConfigs == value) return;
					screenshotAnalyzerConfigs = value;
					log.Info($"[ScreenshotAnalyzerConfigs] = '{value}'");
					SaveSettings();
				}
			}

			private LocalSettings()
			{
			}

			private static string LocalSettingsPath
			{
				get { return "LocalSettings-" + UserId; }
			}

			private void SaveSettings()
			{
				IsolatedStorageSerializationHelper.Save(LocalSettingsPath, this);
			}

			private static readonly LocalSettings empty = new LocalSettings();
			public static LocalSettings Empty { get { return empty; } }

			public static LocalSettings GetLocalSettings()
			{
				if (IsolatedStorageSerializationHelper.Exists(LocalSettingsPath))
				{
					LocalSettings saved;
					if (IsolatedStorageSerializationHelper.Load(LocalSettingsPath, out saved))
					{
						return saved;
					}
				}
				return empty;
			}

			public static void LogLocalSettings(ILog log, string prefix, LocalSettings settings)
			{
				log.Info(prefix + " "
					+ "[HotKey] = '" + settings.HotKey + "'"
					+ ", [NotWorkingWarnInterval] = '" + settings.NotWorkingWarnInterval + "'"
					+ ", [WorkingWarnInterval] = '" + settings.WorkingWarnInterval + "'"
					+ ", [NotWorkingWarnDuration] = '" + settings.NotWorkingWarnDuration + "'"
					+ ", [WorkingWarnDuration] = '" + settings.WorkingWarnDuration + "'"
					+ ", [MenuChangeWarnDuration] = '" + settings.MenuChangeWarnDuration + "'"
					+ ", [UseDoubleClickForStatusChange] = '" + settings.UseDoubleClickForStatusChange + "'"
					+ ", [MenuFlattenFactor] = '" + settings.MenuFlattenFactor + "'"
					+ ", [MenuTopItemsCount] = '" + settings.MenuTopItemsCount + "'"
					+ ", [DisplaySearchWorks] = '" + settings.DisplaySearchWorks + "'"
					+ ", [SearchOwnTasks] = '" + settings.SearchOwnTasks + "'"
					+ ", [SearchInClosed] = '" + settings.SearchInClosed + "'"
					+ ", [DisplaySummaDelta] = '" + settings.DisplaySummaDelta + "'"
					+ ", [MenuRecentItemsCount] = '" + settings.MenuRecentItemsCount + "'"
					+ ", [HighlightNonReasonedWork] = '" + settings.HighlightNonReasonedWork + "'"
					+ ", [RecentWorksInAddMeeting] = '" + settings.RecentWorksInAddMeeting + "'"
					+ ", [NotificationPosition] = '" + settings.NotificationPosition + "'"
					+ ", [IsWorkingWarnDisplayable] = '" + settings.IsWorkingWarnDisplayable + "'"
					+ ", [ShowDynamicWorks] = '" + settings.ShowDynamicWorks + "'"
					+ ", [IsSafeMailItemCommitUsable] = '" + settings.IsSafeMailItemCommitUsable + "'"
					);
			}
		}

		[Serializable]
		[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
		private class UserIdPassword
		{
			public int Id;
			public string Password;
			public DateTime? PasswordExpirationDate;
			public string Name;
		}

		[Serializable]
		[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
		public class ProxySettings : IEquatable<ProxySettings>
		{
			public bool IsAutomatic;
			public string Address;
			public string Username;
			public string Password;

			public ProxySettings()
			{
				IsAutomatic = true;
			}

			public bool Equals(ProxySettings other)
			{
				return other != null && (IsAutomatic && other.IsAutomatic || (
					IsAutomatic == other.IsAutomatic && Address == other.Address && Username == other.Username && Password == other.Password
					));
			}

			public override bool Equals(object obj)
			{
				return Equals((ProxySettings)obj);
			}
		}
	}

	[Flags]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public enum ClientDataCollectionSettings
	{
		None = 0,
		Url = 1,
		WindowTitle = 2,
		EmailAddress = 4,
		DocumentNameAndPath = 8,
		MobileLocation = 16,
		PhoneNumber = 32,
		Other = 64,
		Screenshot = 128,
		Sensor = 256,
		PcActivity = 512,
		EmailSubject = 2048,
		ProcessName = 1 << 12,
	}

	[Flags]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public enum DisplayOptions
	{
		None = 0,
		ShowTargetWorkTimes = 1,
		ShowDiffWorkTimes = 2,
	}

	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public enum SetWorkStateAfterResume
	{
		No,
		RetainPrevious,
		Always,
	}
}
