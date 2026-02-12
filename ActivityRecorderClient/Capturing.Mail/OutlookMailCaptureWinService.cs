using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using System.Windows.Forms;
using log4net;
using OutlookInteropService;
using Tct.ActivityRecorderClient.Capturing.Core;
using Tct.ActivityRecorderClient.Communication;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Notification;
using MailAddress = Tct.ActivityRecorderClient.OutlookMailCaptureServiceReference.MailAddress;
using MailCapture = Tct.ActivityRecorderClient.OutlookMailCaptureServiceReference.MailCapture;
using MailCaptures = Tct.ActivityRecorderClient.OutlookMailCaptureServiceReference.MailCaptures;
using ClientMenu = Tct.ActivityRecorderClient.ActivityRecorderServiceReference.ClientMenu;
using System.Runtime.Serialization;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Plugins.Impl;
using Tct.ActivityRecorderClient.Configuration;
using MailActivityTracker.Model;

namespace Tct.ActivityRecorderClient.Capturing.Mail
{
	public class OutlookMailCaptureWinService : IMailCaptureService, IMenuPublisher
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static object thisLock = new object();
		private readonly INotificationService notificationService = Platform.Factory.GetNotificationService();

		private ProcessCoordinator processCoordinator;

		private bool isOutlookInstalled;
		private bool isDisposed;
		private bool isWarnShown;
		private bool isAddinStateNotificationShown;
		private volatile bool isFirstSuccesfullAccess;
		private readonly SynchronizationContext guiContext = Platform.Factory.GetGuiSynchronizationContext();
		private const string AddinStateKey = "AddinStateChangeReqd";
		private static readonly int cacheDuration = (int)TimeSpan.FromMinutes(1).TotalMilliseconds;
		private static volatile int lastUpdate = Environment.TickCount - cacheDuration - 1;
		private static volatile bool isAddInEnabled;
		private static readonly object lockObj = new object();
		private static readonly string addinFileVersion;
		private Exception prevException;
		private MailTrackingType prevOutlookMailTrackingType = ConfigManager.MailTrackingType;
		private MailTrackingSettings prevOutlookMailTrackingSettings = ConfigManager.MailTrackingSettings;
		private MailTrackingType prevAddinMailTrackingType = MailTrackingType.Disable;
		private MailTrackingSettings prevAddinMailTrackingSettings = MailTrackingSettings.None;
		private MeetingPluginTaskIdSettings prevMeetingTaskIdSettings = MeetingPluginTaskIdSettings.Description;
		private bool? prevIsSafeMailItemCommitUsable;

		static OutlookMailCaptureWinService()
		{
			addinFileVersion = FileVersionInfo.GetVersionInfo(OutlookAddinInstallHelper.AddinDir + "\\MailActivityTracker.dll").FileVersion;
		}

		public void Initialize()
		{
			isOutlookInstalled = OutlookSettingsHelper.IsOutlookInstalled;
			if (!isOutlookInstalled)
			{
				log.Info("Outlook is not installed.");
			}
		}

		public MailCaptures GetMailCaptures()
		{
			if (!isOutlookInstalled || isDisposed) return null;
			CheckAddinStateChanges();
			if (IsAddinInstalled && !ConfigManager.CheckDiagnosticOperationMode(Common.DiagnosticOperationMode.DisableOutlookAddinCapture))
				try
				{
					using (var client = new OutlookAddinMailCaptureClientWrapper())
					{
						client.Client.Heartbeat();
						if (prevAddinMailTrackingType != ConfigManager.MailTrackingType)
						{
							client.Client.SetMailTrackingBehavior(ConfigManager.MailTrackingType != MailTrackingType.Disable, ConfigManager.MailTrackingType == MailTrackingType.BodyAndSubject);
							prevAddinMailTrackingType = ConfigManager.MailTrackingType;
						}
						if (prevAddinMailTrackingSettings != ConfigManager.MailTrackingSettings)
						{
							client.Client.SetMailTrackingSettings((OutlookAddinMailCaptureServiceReference.MailTrackingSettings)ConfigManager.MailTrackingSettings);
							prevAddinMailTrackingSettings = ConfigManager.MailTrackingSettings;
						}
						var result = Map(client.Client.GetMailCaptures());
						if (!isFirstSuccesfullAccess)
						{
							prevAddinMailTrackingType = MailTrackingType.Disable; // to send actual states again
							prevAddinMailTrackingSettings = MailTrackingSettings.None;
							string installedVersion = null;
							try
							{
								installedVersion = client.Client.GetVersion();
							}
							catch (ActionNotSupportedException)
							{
								// do nothing, older addin
							}
							if (installedVersion == null || installedVersion != addinFileVersion)
							{
								lastUpdate = Environment.TickCount;
								isAddInEnabled = false;
							}
							guiContext.Post((e) =>
							{
								try
								{
									CaptureCoordinator.Instance?.PublishMenu(exc =>
									{
										log.Error("PublishMenuForOutlook call failed", exc);
										isFirstSuccesfullAccess = false;
									});
								}
								catch (Exception ex)
								{
									log.Error("PublishMenuForOutlook call failed", ex);
									isFirstSuccesfullAccess = false;
								}
							}, null);
						}
						isFirstSuccesfullAccess = true;
						if (processCoordinator != null)
						{
							processCoordinator.Stop();
							processCoordinator = null;
							HideAddinStateNotfication();
						}
						isWarnShown = false;
						prevException = null;
						return result;
					}
				}
				catch (EndpointNotFoundException ex)
				{
					// addin cannot be accessed, continue normally with jcmail process
					if (!isWarnShown)
					{
						log.Warn("outlook addin enabled, but cannot be accessed");
						isWarnShown = true;
						isFirstSuccesfullAccess = false;
					}
				}
				catch (Exception ex)
				{
					if (prevException == null || prevException.GetType() != ex.GetType() || !prevException.Message.Equals(ex.Message))
					{
						log.Error("failed", ex);
						prevException = ex;
					}
					return null;
				}

			var isOutlookRunning = Process.GetProcessesByName("outlook").Any(p => !p.HasExited);
			if (ConfigManager.CheckDiagnosticOperationMode(Common.DiagnosticOperationMode.DisableOutlookJcMailCapture) || !isOutlookRunning) 
			{
				if (processCoordinator != null)
				{
					processCoordinator.Stop();
					processCoordinator = null;
					HideAddinStateNotfication();
				}
				return null;
			}
			if (processCoordinator == null)
			{
				processCoordinator = ProcessCoordinator.OutlookMailProcessCoordinator;
				processCoordinator.Start();
				prevIsSafeMailItemCommitUsable = null;
			}
			try
			{
				using (var client = new OutlookMailCaptureClientWrapper())
				{
					if (ConfigManager.MailTrackingType != prevOutlookMailTrackingType || ConfigManager.MailTrackingSettings != prevOutlookMailTrackingSettings || ConfigManager.LocalSettingsForUser.IsSafeMailItemCommitUsable != prevIsSafeMailItemCommitUsable)
					{
						client.Client.SetMailTracking((OutlookMailCaptureServiceReference.MailTrackingType)ConfigManager.MailTrackingType, (OutlookMailCaptureServiceReference.MailTrackingSettings)ConfigManager.MailTrackingSettings, ConfigManager.LocalSettingsForUser.IsSafeMailItemCommitUsable);
						prevOutlookMailTrackingType = ConfigManager.MailTrackingType;
						prevOutlookMailTrackingSettings = ConfigManager.MailTrackingSettings;
						prevIsSafeMailItemCommitUsable = ConfigManager.LocalSettingsForUser.IsSafeMailItemCommitUsable;
					}

					var mailCaptures = client.Client.GetMailCaptures();
					if (mailCaptures.IsSafeMailItemCommitUsable != ConfigManager.LocalSettingsForUser.IsSafeMailItemCommitUsable)
						prevIsSafeMailItemCommitUsable = ConfigManager.LocalSettingsForUser.IsSafeMailItemCommitUsable = mailCaptures.IsSafeMailItemCommitUsable;
					return mailCaptures;
				}
			}
			catch (FaultException ex)
			{
				if (ex.Message == "Elevate" || ex.Message == "Unelevate")
				{
					processCoordinator.ChangeElevationLevel(ex.Message == "Elevate");
					prevIsSafeMailItemCommitUsable = null;
				}
			}
			catch (Exception ex)
			{
				WcfExceptionLogger.LogWcfError("get mail captures", log, ex);
				if (ex.InnerException is PipeException
					&& ((PipeException)ex.InnerException).ErrorCode == -2146232800)
				{
					processCoordinator.RestartIfNeeded();
					prevIsSafeMailItemCommitUsable = null;
				}
			}
			return null;
		}

		private void HideAddinStateNotfication()
		{
			if (isAddinStateNotificationShown)
			{
				guiContext.Post(_ => notificationService.HideNotification(AddinStateKey), null);
				isAddinStateNotificationShown = false;
			}
		}

		private void CheckAddinStateChanges()
		{
			var addinReqd = ConfigManager.IsOutlookAddinRequired;
			var addinInstd = IsAddinInstalled;
			//var isTrackingEnabled = ConfigManager.IsOutlookAddinMailTrackingId;
			//var isSubjectTrackingEnabled = ConfigManager.IsOutlookAddinMailTrackingUseSubject;

			////var addSubjectSuffix = isTrackingEnabled ? isSubjectTrackingEnabled ? MailTrackingType.BodyAndSubject : MailTrackingType.BodyOnly : MailTrackingType.Disable;


			if (addinReqd == addinInstd)
			{
				return;
			}
			IsAddinInstalled = addinReqd;
			prevOutlookMailTrackingType = prevAddinMailTrackingType = MailTrackingType.Disable; // to send actual states again
			prevAddinMailTrackingSettings = prevAddinMailTrackingSettings = MailTrackingSettings.None;
			if (!IsOutlookRunning()) return;

			log.Debug("NotificationOutlookPluginSettingChange shown");
			guiContext.Post(
				_ =>
					notificationService.ShowNotification(AddinStateKey, TimeSpan.Zero,
						Labels.NotificationOutlookPluginSettingChangeTitle, Labels.NotificationOutlookPluginSettingChangeBody,
						Color.Crimson), null);
			isAddinStateNotificationShown = true;
		}

		private static bool IsAddinInstalled
		{
			get
			{
				lock (lockObj)
				{
					if ((uint)(Environment.TickCount - lastUpdate) > cacheDuration)
					{
						isAddInEnabled = OutlookAddinInstallHelper.CheckIfInstalledAndNotDisabled();
						lastUpdate = Environment.TickCount;
					}
					return isAddInEnabled;
				}
			}
			set
			{
				lock (lockObj)
				{
					var addinKey = OutlookAddinInstallHelper.GetAddinNode();
					if (value)
					{
						var names = Directory.GetFiles(OutlookAddinInstallHelper.AddinDir, "*.vsto");
						if (names.Length == 0)
						{
							log.ErrorAndFail("Outlook plugin's vsto file not found in app directory");
							return;
						}
						var vstoFilePath = Path.Combine(OutlookAddinInstallHelper.AddinDir, names[0]);
						addinKey = OutlookAddinInstallHelper.InstallAddin(vstoFilePath, addinKey);
						if (addinKey == null)
						{
							log.ErrorAndFail("Outlook plugin's registry cannot be written");
							return;
						}
					}
					else
					{
						if (OutlookAddinInstallHelper.IsInstalled(addinKey))
							OutlookAddinInstallHelper.UninstallPlugin(addinKey);
					}
					isAddInEnabled = value;
					lastUpdate = Environment.TickCount;
				}
			}
		}

		private static bool IsOutlookRunning()
		{
			var currentSession = Process.GetCurrentProcess().SessionId;
			return Process.GetProcessesByName("outlook").Any(p => p.SessionId == currentSession);
		}

		public void PublishMenu(ClientMenu clientMenu, Action<Exception> onErrorCallback)
		{
			if (!ConfigManager.IsOutlookAddinRequired) return;
			if (clientMenu == null) return;
			ThreadPool.QueueUserWorkItem((_) =>
			{
				using (var client = new OutlookAddinMailCaptureClientWrapper())
				{
					try
					{
						FilteringForInvisibleItemsInAdhocMeeting(clientMenu.Works);
#if DEBUG
						using (Stream s = new MemoryStream())
						{
							System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
							formatter.Serialize(s, clientMenu);
							log.InfoFormat("Trying to transfer menu to Outlook in size of {0} bytes", s.Length);
						}
#endif
						lock (thisLock)
						{
							byte[] compressed;
							using (var outStream = new MemoryStream())
							{
								DataContractSerializer serializer = new DataContractSerializer(typeof(ClientMenu));
								serializer.WriteObject(outStream, clientMenu);
								compressed = outStream.ToArray();
							}
							int offset = 0;
							byte[] buffer = new byte[1024];
							while (true)
							{
								buffer = compressed.Skip(offset).Take(1024).ToArray();
								if (buffer.Length == 0) break;
								offset += 1024;
								client.Client.TransferMenuData(buffer);
							}
							if (prevMeetingTaskIdSettings != ConfigManager.MeetingTaskIdSettings)
							{
								client.Client.SetTaskIdSettings((int)ConfigManager.MeetingTaskIdSettings);
								prevMeetingTaskIdSettings = ConfigManager.MeetingTaskIdSettings;
								log.Debug("MeetingTaskIdSettings sent to Outlook");
							}
							client.Client.UpdateMenu(AppConfig.Current.TaskPlaceholder);
							log.Debug("Menu sent to Outlook");
						}
					}
					catch (Exception ex)
					{
						if (onErrorCallback != null)
							onErrorCallback(ex);
						else
							log.Error("Updating plugin menu failed", ex);
					}
				}
			}, null);
		}
		private bool FilteringForInvisibleItemsInAdhocMeeting(List<ActivityRecorderServiceReference.WorkData> root)
		{
			foreach (var e in root.Reverse<ActivityRecorderServiceReference.WorkData>())
				if (e.Children != null && e.Children.Count > 0)
				{
					if (FilteringForInvisibleItemsInAdhocMeeting(e.Children))
						root.Remove(e);
				}
				else if (!e.IsVisibleInAdhocMeeting) root.Remove(e);
			return root.Count == root.Count(f => !f.IsVisibleInAdhocMeeting);
		}

		private MailCaptures Map(OutlookAddinMailCaptureServiceReference.MailCaptures src)
		{
			return new MailCaptures { MailCaptureByHWnd = Map(src.MailCaptureByHWnd) };
		}

		private Dictionary<int, MailCapture> Map(Dictionary<int, OutlookAddinMailCaptureServiceReference.MailCapture> src)
		{
			return src != null && src.Count > 0 ? src.ToDictionary(m => m.Key, m => Map(m.Value)) : null;
		}

		private MailCapture Map(OutlookAddinMailCaptureServiceReference.MailCapture src)
		{
			return new MailCapture
			{
				To = Map(src.To),
				From = Map(src.From),
				Cc = Map(src.Cc),
				Subject = src.Subject,
				Id = src.Id,
				JcId = src.JcId,
				ExtensionData = src.ExtensionData,
			};
		}

		private List<MailAddress> Map(OutlookAddinMailCaptureServiceReference.MailAddress[] src)
		{
			return src.Select(a => Map(a)).ToList();
		}

		private MailAddress Map(OutlookAddinMailCaptureServiceReference.MailAddress src)
		{
			return new MailAddress { Name = src.Name, Email = src.Email, ExtensionData = src.ExtensionData, };
		}

		public void Dispose()
		{
			if (isDisposed) return;
			isDisposed = true;
			log.Info("Stopping service");
			if (!isOutlookInstalled) return;
			if (processCoordinator != null)
				processCoordinator.Stop();
		}
	}
}
