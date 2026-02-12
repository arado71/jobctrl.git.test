using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel;
using System.IdentityModel.Claims;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Tct.ActivityRecorderClient.Forms;
using Tct.ActivityRecorderClient.Notification;
using Tct.ActivityRecorderClient.Serialization;
using Binding = System.ServiceModel.Channels.Binding;

namespace Tct.ActivityRecorderClient.Configuration
{
	public abstract class AppConfig : AppConfigBase
    {
		protected static readonly X509Certificate2 defaultCertificate = X509CertificateLoader.LoadCertificate(Encoding.ASCII.GetBytes("""
-----BEGIN CERTIFICATE-----
MIICDzCCAXygAwIBAgIQE3NJW08VOqJAFmVd7vAykDAJBgUrDgMCHQUAMB8xHTAb
BgNVBAMTFHRjdC5BY3Rpdml0eVJlY29yZGVyMB4XDTEwMDMyNjEwNDM1NFoXDTM5
MTIzMTIzNTk1OVowHzEdMBsGA1UEAxMUdGN0LkFjdGl2aXR5UmVjb3JkZXIwgZ8w
DQYJKoZIhvcNAQEBBQADgY0AMIGJAoGBAPjb0Vo8+lz7jfFX2mM1REYchvRUauS4
oORrFMYtLwr6DiVStDRKA4ZmXx1aZy90Amc/ryIxHtEKppng14BR18v4fXd/W6rs
vYdX24ES+WMIRmyqQVE1omtXTpX+5LsfQQiZg90bKooqw6LVODkk26kZZD/oEy4I
H6+8LD4SqvsfAgMBAAGjVDBSMFAGA1UdAQRJMEeAEA+MWU/2+ifB3z0MO0Z3Vkmh
ITAfMR0wGwYDVQQDExR0Y3QuQWN0aXZpdHlSZWNvcmRlcoIQE3NJW08VOqJAFmVd
7vAykDAJBgUrDgMCHQUAA4GBAOGpmH7aZPS29E2sNODr/v1vwOJOPTJDfXMwSD0+
mOUyFSeUA3Nnf2Sa8AV4CyicrfyES9pyrOD5nhTCBpK1N1Tenx9MHnr6r5OzeQ66
omkCM6GCfqxAJKLR46YtFLIx6lE1pwOx6BVM/L6g+pwmj7XtrFPIL41Si2vQo0H8
2K6p
-----END CERTIFICATE-----
"""));

		static AppConfig()
		{
#if DEBUG
			Current = new AppConfigLocal();
			return;
#endif
			var configClasses = SafeAssemblyGetTypes().Where(t => t != null && !t.IsAbstract && typeof(AppConfig).IsAssignableFrom(t)).ToList();
			if (configClasses.Count == 0) throw new Exception("no config class defined");
			if (configClasses.Count > 1) throw new Exception("more than one config classes defined");
			Current = (AppConfig)Activator.CreateInstance(configClasses[0]);
		}

		public static AppConfig Current { get; }

		protected const string ServiceNetTcpName = "NetTcpBinding_IActivityRecorder";
		protected const string ServiceHttpsName = "HttpBinding_IActivityRecorder";
		protected const string ServiceBinZipHttpsName = "BinaryZipHttpBinding_IActivityRecorder";
		protected const string ActiveDirectoryNetTcpName = "NetTcpBinding_IActiveDirectoryLoginService";
		protected const string ActiveDirectoryHttpsName = "HttpBinding_IActiveDirectoryLoginService";
		protected const int DefaultMaxDepth = 32;
		protected const int DefaultMaxStringContentLength = 65536;
		protected const int DefaultMaxArrayLength = 200001;
		protected const int DefaultMaxBytesPerRead = 4096;
		protected const int DefaultMaxNameTableCharCount = 16384;

		#region ServiceNetTcp

		protected virtual EndpointAddress ServiceNetTcpEndpointAddress => new EndpointAddress(
			new Uri("net.tcp://jobctrl.com:9000/ActivityRecorderService", UriKind.Absolute), 
			EndpointIdentity.CreateX509CertificateIdentity(defaultCertificate)
			);

		protected virtual Binding ServiceNetTcpBinding => new NetTcpBinding()
		{
			Name = ServiceNetTcpName,
			CloseTimeout = new TimeSpan(0, 1, 0),
			OpenTimeout = new TimeSpan(0, 1, 0),
			ReceiveTimeout = new TimeSpan(0, 10, 0),
			SendTimeout = new TimeSpan(0, 1, 0),
			TransactionFlow = false,
			TransferMode = TransferMode.Buffered,
			TransactionProtocol = TransactionProtocol.OleTransactions,
			HostNameComparisonMode = HostNameComparisonMode.StrongWildcard,
			ListenBacklog = 10,
			MaxConnections = 10,
			MaxBufferSize = 5000001,
			MaxBufferPoolSize = 12000001,
			MaxReceivedMessageSize = 5000001,
			ReaderQuotas = new XmlDictionaryReaderQuotas()
			{
				MaxDepth = DefaultMaxDepth,
				MaxStringContentLength = DefaultMaxStringContentLength,
				MaxArrayLength = DefaultMaxArrayLength,
				MaxBytesPerRead = DefaultMaxBytesPerRead,
				MaxNameTableCharCount = DefaultMaxNameTableCharCount,
			},
			ReliableSession = new OptionalReliableSession()
			{
				Ordered = true,
				InactivityTimeout = new TimeSpan(0, 10, 0),
				Enabled = false,
			},
			Security = new NetTcpSecurity() { Mode = SecurityMode.Message, Message = new MessageSecurityOverTcp() { ClientCredentialType = MessageCredentialType.UserName } }
		};

		protected virtual int? ServiceNetTcpMaxItemsInObjectGraph { get; } = 2147483647;

		protected virtual bool ServiceNetTcpCertificateValidationDisabled { get; } = true;

		#endregion ServiceNetTcp

		#region ServiceHttps

		protected virtual EndpointAddress ServiceHttpsEndpointAddress => new EndpointAddress(
			new Uri("https://backoffice.jobctrl.com:443/JobCTRL/", UriKind.Absolute)
		);

		protected virtual Binding ServiceHttpsBinding => new BasicHttpBinding()
		{
			Name = ServiceHttpsName,
			CloseTimeout = new TimeSpan(0, 1, 0),
			OpenTimeout = new TimeSpan(0, 1, 0),
			ReceiveTimeout = new TimeSpan(0, 10, 0),
			SendTimeout = new TimeSpan(0, 1, 0),
			AllowCookies = false,
			BypassProxyOnLocal = false,
			MaxBufferSize = 5000001,
			MaxBufferPoolSize = 12000001,
			MaxReceivedMessageSize = 5000001,
			ReaderQuotas = new XmlDictionaryReaderQuotas()
			{
				MaxDepth = DefaultMaxDepth,
				MaxStringContentLength = DefaultMaxStringContentLength,
				MaxArrayLength = 5000001,
				MaxBytesPerRead = DefaultMaxBytesPerRead,
				MaxNameTableCharCount = DefaultMaxNameTableCharCount,
			},
			Security = new BasicHttpSecurity() { Mode = BasicHttpSecurityMode.Transport, Transport = new HttpTransportSecurity() { ClientCredentialType = HttpClientCredentialType.Basic } }
		};

		protected virtual int? ServiceHttpsMaxItemsInObjectGraph { get; } = 2147483647;

		protected virtual bool ServiceHttpsCertificateValidationDisabled { get; } = false;

		#endregion ServiceHttps

		#region ServiceBinZipHttps

		protected virtual EndpointAddress ServiceBinZipHttpsEndpointAddress => null;

		protected virtual Binding ServiceBinZipHttpsBinding =>
			new CustomBinding()
			{
				Name = ServiceHttpsName,
				CloseTimeout = new TimeSpan(0, 1, 0),
				OpenTimeout = new TimeSpan(0, 1, 0),
				ReceiveTimeout = new TimeSpan(0, 10, 0),
				SendTimeout = new TimeSpan(0, 1, 0),
				Elements =
				{
					SecurityBindingElement.CreateUserNameOverTransportBindingElement(),
					// TODO: mac
					/*new TctMessageEncodingBindingElement()
					{
						ReaderQuotas =
						{
							MaxDepth = DefaultMaxDepth,
							MaxStringContentLength = DefaultMaxStringContentLength,
							MaxArrayLength = 5000001,
							MaxBytesPerRead = DefaultMaxBytesPerRead,
							MaxNameTableCharCount = DefaultMaxNameTableCharCount,
						}
					},*/
					new HttpsTransportBindingElement()
					{
						AllowCookies = false,
						BypassProxyOnLocal = false,
						MaxBufferSize = 5000001,
						MaxBufferPoolSize = 12000001,
						MaxReceivedMessageSize = 5000001,
						TransferMode = TransferMode.Buffered,
						UseDefaultWebProxy = true,
					},
				},
			};

		protected virtual int? ServiceBinZipHttpsMaxItemsInObjectGraph { get; } = 2147483647;

		protected virtual bool ServiceBinZipHttpsCertificateValidationDisabled { get; } = false;

		#endregion ServiceBinZipHttps

		#region ActiveDirectoryNetTcp

		protected virtual EndpointAddress ActiveDirectoryNetTcpEndpointAddress => null;

		protected virtual Binding ActiveDirectoryNetTcpBinding => new NetTcpBinding()
		{
			Name = ActiveDirectoryNetTcpName,
			// TODO: mac
			//Security = new NetTcpSecurity() { Mode = SecurityMode.Message, Message = new MessageSecurityOverTcp() { ClientCredentialType = MessageCredentialType.Windows } }
		};

		protected virtual int? ActiveDirectoryNetTcpMaxItemsInObjectGraph { get; } = 2147483647;

		protected virtual bool ActiveDirectoryNetTcpCertificateValidationDisabled { get; } = false;

		#endregion ActiveDirectoryNetTcp

		#region ActiveDirectoryHttps

		protected virtual EndpointAddress ActiveDirectoryHttpsEndpointAddress => null;

		protected virtual Binding ActiveDirectoryHttpsBinding => new BasicHttpBinding()
		{
			Name = ActiveDirectoryHttpsName,
			Security = new BasicHttpSecurity() { Mode = BasicHttpSecurityMode.Transport, Transport = new HttpTransportSecurity() { ClientCredentialType = HttpClientCredentialType.Windows } }
		};

		protected virtual int? ActiveDirectoryHttpsMaxItemsInObjectGraph { get; } = 2147483647;

		protected virtual bool ActiveDirectoryHttpsCertificateValidationDisabled { get; } = false;

		#endregion ActiveDirectoryHttps

		public virtual int CaptureWorkItemInterval { get; } = 30 * 1000; //30 secs (SQL) /**/3 AddWorkItemEx 28192 bytes/call inside, in variables; but 85 packets 54161 bytes/call outside, in Ethernet packets
		public virtual int CaptureActiveWindowInterval { get; } = 30 * 1000; //30 secs (SQL)
		public virtual int CaptureScreenShotInterval { get; } = 30 * 1000; //30 secs (SQL)     "-1" ==> disables ScreenShots. This, I mean the "-1", seriously slows down the server. ActivityRecorderDataClasses.cs/ActivityRecorderDataClassesDataContext/SubmitChanges/Line 124: workItem.ScreenShots.AddRange(DesktopLayoutVisualizer.GetScreenShotsFromCapture(screenCapture)); /The end of this call is here:DesktopLayoutVisualizer.cs/GetScreenShotForScreen
		public virtual int TimeSyncThreshold { get; } = 5000;
		public virtual int JpegQuality { get; } = 20;
		public virtual int JpegScalePct { get; } = 50;
		public virtual int MenuUpdateInterval { get; } = 60 * 1000;   //60 secs (SQL) /**/4 GetClientMenu 3566 bytes/call inside, in variables; but 60 packets 23972 bytes/call outside, in Ethernet packets
		public virtual int WorkTimeStartInMins { get; } = 420;
		public virtual int WorkTimeEndInMins { get; } = 1080;
		public virtual int AfterWorkTimeIdleInMins { get; } = 20;
		public virtual int MaxOfflineWorkItems { get; } = 0;
		public virtual int DuringWorkTimeIdleInMins { get; } = 0;
		public virtual int DuringWorkTimeIdleManualInterval { get; } = 60 * 60 * 1000; //1 hour
		public virtual int MaxManualMeetingInterval { get; } = 4 * 60 * 60 * 1000;
		public virtual bool IsMeetingSubjectMandatory { get; } = false;
		public virtual Rules.RuleRestrictions RuleRestrictions { get; } = Rules.RuleRestrictions.None;
		public virtual bool IsOutlookMeetingTrackingEnabled { get; } = true;
		public virtual bool IsLotusNotesMeetingTrackingEnabled { get; } = false;
		public virtual int BusyTimeThreshold { get; } = 0;
		public virtual bool CoincidentalClientsEnabled { get; } = true;
		public virtual bool IsManualMeetingStartsOnLock { get; } = true;
		public virtual int RuleMatchingInterval { get; } = 300; //300 millisecs
		public virtual bool IsOutlookAddinRequired { get; } = false;
		public virtual bool IsOutlookAddinMailTrackingId { get; } = false;
		public virtual bool IsOutlookAddinMailTrackingUseSubject { get; } = true;
		public virtual bool UseRedemptionForMeetingSync { get; } = true;
		public virtual bool IsMeetingUploadModifications { get; } = true;
		public virtual bool IsMeetingTentativeSynced { get; } = true;
		public virtual bool IsInjectedInputAllowed { get; } = false;
		public virtual ClientDataCollectionSettings? ClientDataCollectionSettings { get; } = null;
		public virtual int MouseMovingThreshold { get; } = 30;
		public virtual int UsageSaveInSeconds { get; } = 60;
		public virtual int PluginFailThreshold { get; } = -1;
		public virtual int CollectedItemAggregateInMins { get; } = 60;
		public virtual bool ForceCountdownRules { get; } = false;
		public virtual string MsProjectAddress { get; } = null;
		public virtual int ManualWorkItemEditAgeLimit { get; } = 30 * 24; //30 days
		public virtual bool AutoReturnFromMeeting { get; } = false;
		public virtual int CapturingDeadlockInMins { get; } = 10;
		public virtual int TelemetryMaxCount { get; } = 0;
		public virtual int TelemetryMaxAgeInMins { get; } = 0;
		public virtual string TelemetryCollectedKeys { get; } = null;
		public virtual IssuePropColumnFlag IssuePropColumns { get; } = IssuePropColumnFlag.CompanyVisible |
		                                                               IssuePropColumnFlag.NameVisible |
		                                                               IssuePropColumnFlag.StateVisible |
		                                                               IssuePropColumnFlag.IssuesButtonVisible;
		public virtual string IssueCategories { get; } = null;
		public virtual string OCRLanguage { get; } = "eng";
		public abstract string GoogleClientId { get; } 
		public abstract string GoogleClientSecret { get; }
		public virtual View.WorkDetectorRuleEditForm.OkValidType SelfLearningOkValidity { get; } = View.WorkDetectorRuleEditForm.OkValidType.Default;
#if InstallScopePerMachine
		public virtual bool AutoUpdateManagerEnabled { get; } = false;
#else
		public virtual bool AutoUpdateManagerEnabled { get; } = true;
#endif
		public virtual bool OnlyDesktopTasksInWorktimeMod { get; } = false;
		public virtual int ActivityValueWhenCollectingDisabled { get; } = -1;
		public virtual bool IsLoginRememberPasswordChecked { get; } = false;
		public virtual string WebsiteUrl { get; } = "https://jobctrl.com/";
		public virtual string ValidCertificate { get; } = null;
		public virtual bool? IsRunAsAdminDefault { get; } = true;
		public virtual bool IsMeetingDescriptionSynchronized { get; } = true;
		public virtual bool IsMeetingAppointmentSynchronized { get; } = false;
		public virtual string CalendarFolderInclusionPattern { get; } = null;
		public virtual string CalendarFolderExclusionPattern { get; } = null;
		public virtual string AppNameOverride { get; } = null;
		public abstract string AppClassifier { get; }
		public virtual int NotWorkingWarnInterval { get; } = 300000;
		public virtual int WorkingWarnInterval { get; } = 0;
		public virtual int NotWorkingWarnDuration { get; } = 10000;
		public virtual int WorkingWarnDuration { get; } = 2000;
		public virtual int MenuChangeWarnDuration { get; } = 0;
		public virtual int MenuFlattenFactor { get; } = 1;
		public virtual ActivityRecorderServiceReference.WorktimeStatIntervals DisplayWorktimeStats { get; } = ActivityRecorderServiceReference.WorktimeStatIntervals.Today | ActivityRecorderServiceReference.WorktimeStatIntervals.Week | ActivityRecorderServiceReference.WorktimeStatIntervals.Month;
		public virtual int MenuTopItemsCount { get; } = 10;
		public virtual bool DisplaySearchWorks { get; } = true;
		public virtual bool DisplaySummaDelta { get; } = true;
		public virtual int MenuRecentItemsCount { get; } = 10;
		public virtual Keys? ManualMeetingHotKey { get; } = Keys.Control | Keys.F12;
		public virtual bool ShowDynamicWorks { get; } = false;
		public virtual bool ShowOldMenu { get; } = false;
		public virtual bool SearchOwn { get; } = true;
		public virtual bool SearchClosed { get; } = false;
		public virtual NotificationPosition NotificationPosition { get; } = NotificationPosition.BottomRight;
		public virtual string TaskPlaceholder { get; } = "JobCTRL";
		public virtual bool UseDoubleClickForStatusChange { get; } = true;
		public virtual bool? AutoSendLogFiles { get; } = null;
		public virtual bool DisableManualStatusChange { get; } = false;
		public virtual bool IsRoamingStorageScopeNeeded { get; } = false;
		public virtual bool IdleAlertVisual { get; } = false;
		public virtual bool IdleAlertBeep { get; } = false;
		public virtual bool IsTaskBarIconShowing { get; } = true;
		public virtual bool SuppressActiveDirectoryFallbackLogin { get; } = false;
		public virtual string MutexName { get; } = "ActivityRecorderClient";
		public virtual bool StartWorkAfterLogin { get; } = false;
		public virtual SetWorkStateAfterResume SetWorkStateAfterResume { get; } = SetWorkStateAfterResume.No;
		public virtual bool IsWorkingWarnDisplayable { get; } = true;
		public virtual bool IsSafeMailItemCommitUsable { get; } = true;
		public virtual int DelayedDeleteIntervalInMins { get; } = 15;
		public virtual bool AutoStartClientOnNonWorkDays { get; } = false;
		public virtual Keys? ClearAutoRuleTimersHotKey { get; } = null;
		public virtual TimeSpan? OutlookMeetingCaptureClientTimeout { get; } = TimeSpan.FromMinutes(5);
		public virtual bool IsVideoRecordingEnabled { get; } = false;
		public virtual float VideoRecordingFps { get; } = 1.0f;
		public virtual int VideoRecordingBitRate { get; } = 500000;

		protected virtual void Initialize()
		{
			ServiceEndpointConfigurations = new Dictionary<string, EndpointConfiguration>();
			if (ServiceNetTcpEndpointAddress != null)
				ServiceEndpointConfigurations.Add(ServiceNetTcpName, new EndpointConfiguration(ServiceNetTcpName, ServiceNetTcpEndpointAddress, ServiceNetTcpBinding, ServiceNetTcpMaxItemsInObjectGraph.HasValue ? new ReaderQuotaExtension(ServiceNetTcpMaxItemsInObjectGraph.Value) : null, ServiceNetTcpCertificateValidationDisabled, 1));
			if (ServiceHttpsEndpointAddress != null)
				ServiceEndpointConfigurations.Add(ServiceHttpsName, new EndpointConfiguration(ServiceHttpsName, ServiceHttpsEndpointAddress, ServiceHttpsBinding, ServiceHttpsMaxItemsInObjectGraph.HasValue ? new ReaderQuotaExtension(ServiceHttpsMaxItemsInObjectGraph.Value) : null, ServiceHttpsCertificateValidationDisabled, 2));
			if (ServiceBinZipHttpsEndpointAddress != null)
				ServiceEndpointConfigurations.Add(ServiceBinZipHttpsName, new EndpointConfiguration(ServiceBinZipHttpsName, ServiceBinZipHttpsEndpointAddress, ServiceBinZipHttpsBinding, ServiceBinZipHttpsMaxItemsInObjectGraph.HasValue ? new ReaderQuotaExtension(ServiceBinZipHttpsMaxItemsInObjectGraph.Value) : null, ServiceBinZipHttpsCertificateValidationDisabled, 3));

			ActiveDirectoryEndpointConfigurations = new Dictionary<string, EndpointConfiguration>();
			if (ActiveDirectoryNetTcpEndpointAddress != null)
				ActiveDirectoryEndpointConfigurations.Add(ActiveDirectoryNetTcpName, new EndpointConfiguration(ActiveDirectoryNetTcpName, ActiveDirectoryNetTcpEndpointAddress, ActiveDirectoryNetTcpBinding, ActiveDirectoryNetTcpMaxItemsInObjectGraph.HasValue ? new ReaderQuotaExtension(ActiveDirectoryNetTcpMaxItemsInObjectGraph.Value) : null, ActiveDirectoryNetTcpCertificateValidationDisabled, 1));
			if (ActiveDirectoryHttpsEndpointAddress != null)
				ActiveDirectoryEndpointConfigurations.Add(ActiveDirectoryHttpsName, new EndpointConfiguration(ActiveDirectoryHttpsName, ActiveDirectoryHttpsEndpointAddress, ActiveDirectoryHttpsBinding, ActiveDirectoryHttpsMaxItemsInObjectGraph.HasValue ? new ReaderQuotaExtension(ActiveDirectoryHttpsMaxItemsInObjectGraph.Value) : null, ActiveDirectoryHttpsCertificateValidationDisabled, 2));
		}

		protected AppConfig()
		{
			InitializeInt();
		}

		private void InitializeInt()
		{
			// To avoid virtual member call from constructor
			Initialize();
		}

		public Dictionary<string, EndpointConfiguration> ServiceEndpointConfigurations { get; private set; }

		public Dictionary<string, EndpointConfiguration> ActiveDirectoryEndpointConfigurations { get; private set; }

		private static Type[] SafeAssemblyGetTypes()
		{
			try
			{
				return Assembly.GetExecutingAssembly().GetTypes();
			}
			catch (ReflectionTypeLoadException ex) // some types can't be loaded
			{
				return ex.Types; // loaded types
			}
		}

		public string GetLocalizationStringOverride(string name, string defaultValue)
		{
			// TODO: mac
			var lang = Labels.Culture?.Parent?.IetfLanguageTag;
			return (string)GetType()?.GetProperty(name + lang?.ToUpper())?.GetValue(this, null) ?? defaultValue;
		}

	}

}
