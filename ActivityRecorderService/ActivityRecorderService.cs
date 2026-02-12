using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using log4net;
using Tct.ActivityRecorderService.ActiveDirectoryIntegration;
using Tct.ActivityRecorderService.Caching;
using Tct.ActivityRecorderService.ClientComputerData;
using Tct.ActivityRecorderService.Collector;
using Tct.ActivityRecorderService.EmailStats;
using Tct.ActivityRecorderService.ErrorHandling;
using Tct.ActivityRecorderService.Kicks;
using Tct.ActivityRecorderService.Maintenance;
using Tct.ActivityRecorderService.Meeting;
using Tct.ActivityRecorderService.Notifications;
using Tct.ActivityRecorderService.OnlineStats;
using Tct.ActivityRecorderService.Persistence;
using Tct.ActivityRecorderService.Stats;
using Tct.ActivityRecorderService.Storage;
using Tct.ActivityRecorderService.Update;
using Tct.ActivityRecorderService.UsageStats;
using Tct.ActivityRecorderService.Voice;
using Tct.ActivityRecorderService.Website;
using Tct.ActivityRecorderService.WebsiteServiceReference;
using Tct.ActivityRecorderService.WorkManagement;
using Tct.ActivityRecorderService.DailyAggregation;
using Tct.ActivityRecorderService.MeetingSync;
using Tct.ActivityRecorderService.Messaging;
using Tct.ActivityRecorderService.Ocr;
using Tct.ActivityRecorderService.Telemetry;
using Tct.ActivityRecorderService.TODOs;
using Tct.ActivityRecorderService.WorkTimeHistory;
using System.ServiceModel.Web;

namespace Tct.ActivityRecorderService
{
	//For traffic capture use this behavior
	//[ServiceBehavior(AddressFilterMode = AddressFilterMode.Any, InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Multiple)]
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Multiple)]
	[ErrorHandlerBehavior]
	[WorkerThreadPoolBehavior]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public class ActivityRecorderService : IActivityRecorder, IActivityStats, IActivityMobile, IActivityMonitoring, IVoiceRecorder, IActiveDirectoryLoginService
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

#if LEGACY
		private static readonly DailyStatsBuilder statsBuilder;
		private static readonly TotalWorkTimeStatsBuilder totalWorkTimeBuilder;
#endif
		private static readonly LearningManager learningManager;
		private static readonly CalendarManager calendarManager;
		private static readonly WorkItemAggregator workItemAggregator;
		private static readonly UsageStatsManager usageStatsManager;
		private static readonly SimpleWorkTimeStatsManager simpleWorkTimeStatsManager;
		private static readonly OnlineStatsManager onlineStatsManager;
		private static readonly StorageManager storageManager;
		private static readonly KickManager kickManager;
		private static readonly DailyWorkTimesManager dailyWorkTimesManager;
		private static readonly VersionCacheManager menuVersionCacheManager;
		private static readonly VersionCacheManager workDetectorRulesVersionCacheManager;
		private static readonly VersionCacheManager censorRulesVersionCacheManager;
		private static readonly VersionCacheManager clientSettingsVersionCacheManager;
		private static readonly VersionCacheManager collectorRulesVersionCacheManager;
		private static readonly FileDownloadManager fileDownloadManager;
		private static readonly MsiFileCache msiFileCache;
		private static readonly FileCleanupManager maintenanceManager;
		private static readonly NotificationCacheManager notificationService;
		private static readonly WorkManagementService workManagementService = new WorkManagementService();
		private static readonly TimeSpan maxWorkItemLength = TimeSpan.FromMilliseconds(ConfigManager.MaxWorkItemLength);
		private static readonly TimeSpan maxWorkItemAge = TimeSpan.FromDays(ConfigManager.MaxWorkItemAgeInDays);
		private static readonly MeetingSyncManager meetingSyncManager;
		private static readonly SnippetFilter snippetFilter;
		private static readonly WorktimeStatIntervalAggregator worktimeStatIntervalAggregator = new WorktimeStatIntervalAggregator();

		public static Version Version { get { return ConfigManager.Version; } }

		static ActivityRecorderService()
		{
			try
			{
				System.Net.ServicePointManager.SecurityProtocol |= System.Net.SecurityProtocolType.Tls12;
				AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
				log.Info("ActivityRecorderService cctor begin");
				ThreadPool.GetMinThreads(out var workerThreadsMin, out var completionPortThreadsMin);
				ThreadPool.GetMaxThreads(out var workerThreadsMax, out var completionPortThreadsMax);
				ThreadPool.SetMinThreads(ConfigManager.WorkerThreadsMin, ConfigManager.CompletionPortThreadsMin);
				ThreadPool.SetMaxThreads(ConfigManager.WorkerThreadsMax, ConfigManager.CompletionPortThreadsMax);
				log.Debug($"workerThreads min: {workerThreadsMin} -> {ConfigManager.WorkerThreadsMin}, max: {workerThreadsMax} -> {ConfigManager.WorkerThreadsMax}, completionPortThreads min: {completionPortThreadsMin} -> {ConfigManager.CompletionPortThreadsMin}, max: {completionPortThreadsMax} -> {ConfigManager.CompletionPortThreadsMax}");
#if DEBUG
				Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
#endif
				ManualWorkItemTypeHelper.InitializeDbData();
				ParallelWorkItemTypeHelper.InitializeDbData();
				calendarManager = new CalendarManager();
				onlineStatsManager = new OnlineStatsManager(calendarManager);
				onlineStatsManager.Start();
				simpleWorkTimeStatsManager = new SimpleWorkTimeStatsManager(onlineStatsManager);
				simpleWorkTimeStatsManager.Start();
#if LEGACY
				statsBuilder = new DailyStatsBuilder(onlineStatsManager);
				statsBuilder.Start();
#endif
				UserIdManager.Instance.RefreshUserIds();
				workItemAggregator = new WorkItemAggregator();
				workItemAggregator.WorkItemsAggregated += WorkItemsAggregated;
				workItemAggregator.Start();
				usageStatsManager = new UsageStatsManager();
				usageStatsManager.Start();
				storageManager = new StorageManager();
				storageManager.Start();
				maintenanceManager = new FileCleanupManager(storageManager, ClientComputerErrorPath.Instance, VoiceRecordingPath.Instance, MobileFileStorage.Instance, TelemetryFileStorage.Instance);
				maintenanceManager.Start(60000);
				kickManager = new KickManager();
				kickManager.Start();
				dailyWorkTimesManager = new DailyWorkTimesManager();
				dailyWorkTimesManager.Start();
				menuVersionCacheManager = VersionCacheManager.GetMenuVersionCacheManager();
				menuVersionCacheManager.Start();
				workDetectorRulesVersionCacheManager = VersionCacheManager.GetWorkDetectorRulesVersionCacheManager();
				workDetectorRulesVersionCacheManager.Start();
				censorRulesVersionCacheManager = VersionCacheManager.GetCensorRulesVersionCacheManager();
				censorRulesVersionCacheManager.Start();
				clientSettingsVersionCacheManager = VersionCacheManager.GetClientSettingsVersionCacheManager();
				clientSettingsVersionCacheManager.Start();
				collectorRulesVersionCacheManager = VersionCacheManager.GetCollectorRulesVersionCacheManager();
				collectorRulesVersionCacheManager.Start();
				msiFileCache = new MsiFileCache();
				fileDownloadManager = new FileDownloadManager();
				fileDownloadManager.Start();
				learningManager = new LearningManager();
				learningManager.Start();
				notificationService = new NotificationCacheManager();
				notificationService.Start();
				meetingSyncManager = new MeetingSyncManager(new GoogleCalendarSource(new GoogleDatabaseDataStore()));
				meetingSyncManager.Start();
				snippetFilter = new SnippetFilter();
#if LEGACY
				totalWorkTimeBuilder = new TotalWorkTimeStatsBuilder();
#endif
				EmailManager.Instance.GetType(); //hax make sure unsent emails are loaded at the start of the service
				log.Info("ActivityRecorderService cctor end");
			}
			catch (Exception ex)
			{
				log.Error("Unexpected error in cctor", ex);
				throw;
			}
		}

		private static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			//foreach (var appender in LogManager.GetRepository().GetAppenders().OfType<TextWriterAppender>())
			//{
			//    appender.ImmediateFlush = true;
			//}
			var ex = e.ExceptionObject as Exception;
			log.Fatal("Unhandled exception IsTerminating:" + e.IsTerminating, ex);
			if (ex != null && ex.ToString().Contains("System.ServiceModel.Channels.SecurityChannelListener`1.ReceiveItemAndVerifySecurityAsyncResult`2.StartInnerReceive()"))
			{
				log.Fatal("Preventing shutdown...");
				//hax thanks to a wcf bug framework...
				//https://connect.microsoft.com/VisualStudio/feedback/details/617491/during-security-negotiation-if-the-client-goes-away-wcf-service-is-terminated-timing-dependent
				//http://connect.microsoft.com/wcf/feedback/details/622164/wcf-servicehost-crashes-when-usernamepasswordvalidator-takes-too-long-to-process
			}
			else
			{
				log.Fatal("Initiating shutdown...");
				//exit if we have any other exception
				Environment.Exit(-1);
			}
		}

		private static void WorkItemsAggregated(object sender, EventArgs e)
		{
#if LEGACY
			totalWorkTimeBuilder.SetTotalWorkTimeStatsRefreshRequired(); //this will be calculated on bg thread
#endif
			//SendStatsEmailsIfApplicableAsync(); //don't block aggregation
			log.Debug("SendStatsEmailsIfApplicableAsync skipped");
		}

		private static readonly object emailLock = new object();
		private static void SendStatsEmailsIfApplicableAsync()
		{
			ThreadPool.QueueUserWorkItem(_ =>
				{
					var lockTaken = false;
					try
					{
						Monitor.TryEnter(emailLock, 0, ref lockTaken);
						if (!lockTaken)
						{
							log.Info("SendStatsEmailsIfApplicable is still running");
							return;
						}
						//send out daily, weekly and monthly emails if applicable
						EmailStatsAutoSendHelper.SendStatsEmailsIfApplicable();
					}
					catch (Exception ex)
					{
						log.Error("Unexpected error in SendStatsEmailsIfApplicableAsync", ex);
					}
					finally
					{
						if (lockTaken) Monitor.Exit(emailLock);
					}
				});
		}

		#region Generate Custom binding from NetTcp into a config file
		//private static void GetCustomConfig()
		//{
		//    var binding = new NetTcpBinding("Binding1");
		//    CustomBinding custom = new CustomBinding(binding);

		//    Configuration machineConfig = ConfigurationManager.OpenMachineConfiguration();
		//    ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
		//    fileMap.ExeConfigFilename = "out.config";
		//    fileMap.MachineConfigFilename = machineConfig.FilePath;
		//    Configuration config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
		//    config.NamespaceDeclared = true;
		//    ServiceContractGenerator scg = new ServiceContractGenerator(config);
		//    string sectionName, configName;
		//    scg.GenerateBinding(custom, out sectionName, out configName);
		//    config.Save();
		//}
		#endregion

		#region SafeRun

		private void SafeRun(int userId, Action action, [CallerMemberName] string name = null)
		{
			log.Verbose($"{name} started uid: {userId.ToInvariantString()}");
			try
			{
				var sw = Stopwatch.StartNew();
				action();
				log.Debug($"Service call {name} userId: {userId.ToInvariantString()} finished in {sw.ToTotalMillisecondsString()} ms");
			}
			catch (Exception ex)
			{
				log.Error($"{name} failed userId: {userId.ToInvariantString()}", ex);
				throw;
			}
		}

		private T SafeRun<T>(int userId, Func<T> func, [CallerMemberName] string name = null)
		{
			log.Verbose($"{name} started uid: {userId.ToInvariantString()}");
			try
			{
				var sw = Stopwatch.StartNew();
				var result = func();
				log.Debug($"Service call {name} userId: {userId.ToInvariantString()} finished in {sw.ToTotalMillisecondsString()} ms");
				return result;
			}
			catch (Exception ex)
			{
				log.Error($"{name} failed userId: {userId.ToInvariantString()}", ex);
				throw;
			}
		}

		#endregion

		#region AddWorkItem

		public void AddWorkItem(WorkItem workItem)
		{
			if (workItem == null) return;
#if DEBUG || STRESSTEST
			if (workItem.PhaseId == new Guid("11111111-1111-1111-1111-111111111111")) //test guid
			{
				if (workItem.WorkId > 0) Thread.Sleep(workItem.WorkId);
				return;
			}
#endif
			log.Verbose($"AddWorkItem started uid: {workItem.UserId}");
			Stopwatch swAddWi = Stopwatch.StartNew();
			Stopwatch swSql;
			Stopwatch swSql2;
			EnsureAccess(workItem.UserId);
			int groupId;
			int companyId;
			var isXff = false;
			string unknownHeaders = null;
#if STRESSTEST
			groupId = -2;
			companyId = -2;
#else
			if (!UserIdManager.Instance.TryGetIdsForUser(workItem.UserId, out groupId, out companyId))
			{
				//don't need to log this as error so it is outside of the try block
				log.Info("AddWorkItem failed " + workItem + " user is not active");
				throw new FaultException("User is not active");
			}
#endif
			workItem.GroupId = groupId;
			workItem.CompanyId = companyId;
			var moveToDeadLetter = false;
			// Remove background windows
			var captureWindows = workItem.DesktopCaptures.ToDictionary(x => x, y => y.DesktopWindows.ToArray());
			foreach (var capture in workItem.DesktopCaptures)
			{
				var activeWindow = capture.DesktopWindows.LastOrDefault(x => x.IsActive);
				capture.DesktopWindows.Clear();
				if (activeWindow != null) capture.DesktopWindows.Add(activeWindow);
			}
			try
			{
#if EncodeTransmissionScreen
				Screenshots.ScreenshotDecoderHelper.DecodeImages(workItem);
#endif
				workItem.IPAddress = GetClientAddress(out isXff, out unknownHeaders);
#if !STRESSTEST
				if (workItem.StartDate > DateTime.UtcNow.AddMinutes(10))
				{
					moveToDeadLetter = workItem.StartDate > DateTime.UtcNow.AddDays(2); //far in the future
					throw new FaultException("WorkItem is in the future");
				}
#endif
				if (workItem.EndDate - workItem.StartDate > maxWorkItemLength)
				{
					moveToDeadLetter = true;
					throw new FaultException("WorkItem too long");
				}
				if (workItem.EndDate < workItem.StartDate)
				{
					moveToDeadLetter = true;
					throw new FaultException("Invalid interval for WorkItem");
				}
				if (DateTime.UtcNow - workItem.StartDate > maxWorkItemAge)
				{
					moveToDeadLetter = true;
					throw new FaultException("WorkItem too old");
				}
				if (workItem.WorkId <= 0)
				{
					moveToDeadLetter = true;
					Debug.Fail("Invalid workid for WorkItem");
					throw new FaultException("Invalid workid for WorkItem");
				}
				using (var contex = new ActivityRecorderDataClassesDataContext())
				{
					contex.SetXactAbortOn();
					contex.WorkItems.InsertOnSubmit(workItem);
					swSql = Stopwatch.StartNew();
					contex.SubmitChanges();
					swSql.Stop();
					//if the ReportClientComputerAddress is the 2nd call it's two times faster... don't know why (linq overhead)
					swSql2 = Stopwatch.StartNew();
					int tries = 0;
					const int MAX_TRIES = 10;
					const int SLEEP_GAP_MS = 10;
					while (true)
						try
						{
							string ipList = workItem.LocalIPAddressesSeparated;
							if (workItem.LocalIPAddressesSeparated.Length > 150)
							{
								ipList = ipList.Substring(0, 150);
								log.Warn("Subject to put more then 10 IP addresses. It has been truncated.");
							}
							contex.ReportClientComputerAddress(workItem.UserId, workItem.ComputerId, workItem.IPAddress, ipList);
							break;
						}
						catch (Exception ex)
						{
							const string INDEX_ALREADY_INSERTED_ERROR = "2601";
							var match = Regex.Match(ex.Message, @"\d{4}");
							if (!match.Success || match.Value != INDEX_ALREADY_INSERTED_ERROR || ++tries >= MAX_TRIES)
								throw;
							Thread.Sleep(SLEEP_GAP_MS << tries);
						}
					swSql2.Stop();
				}
			}
			catch (Exception ex)
			{
				if (moveToDeadLetter && DeadLetterHelper.TrySaveItem(workItem, ex))
				{
					log.Warn("AddWorkItem failed and moved to dead letter " + workItem + " (" + GetServerScheme() + ")", ex);
					return;
				}
				swSql = null;
				swSql2 = null;
				var sqlex = ex as SqlException;
				if (sqlex != null && sqlex.Message != null && sqlex.Message.Contains("IX_WorkItems_Unique"))
				{
					try
					{
						using (var contex = new ActivityRecorderDataClassesDataContext())
						{
							contex.SetXactAbortOn();
							var dbitem = contex.WorkItems.FirstOrDefault(
								n => n.StartDate == workItem.StartDate
								     && n.PhaseId == workItem.PhaseId
								     && n.UserId == workItem.UserId
								     && n.ComputerId == workItem.ComputerId
								     && n.WorkId == workItem.WorkId
							);
							if (dbitem != null)
							{
								if (dbitem.EndDate.ToString("F") == workItem.EndDate.ToString("F"))
								{
									log.Debug("Duplicate workItem received uid: " + workItem + " (" + GetServerScheme() + ")");
									return;
								}

								if (dbitem.EndDate > workItem.EndDate)
								{
									log.Debug("Shorter workItem received uid: " + workItem + " (" + GetServerScheme() + ")");
									return;
								}

								log.Debug("Longer workItem received uid: " + workItem + " (" + GetServerScheme() + ")");
								dbitem.EndDate = workItem.EndDate;
								swSql = Stopwatch.StartNew();
								contex.SubmitChanges();
								swSql.Stop();
								swSql2 = Stopwatch.StartNew();
								ex = null;
							}
						}
					}
					catch (Exception exc)
					{
						log.Error("AddWorkItem duplicate scan failed " + workItem + " (" + GetServerScheme() + ")", exc);
					}
				}
				if (ex != null)
				{
					if (workItem.StartDate < ConfigManager.IgnoreErrorsCutOff)
					{
						log.Fatal("AddWorkItem failed " + workItem + " (" + GetServerScheme() + ") but ignoring errors before " + ConfigManager.IgnoreErrorsCutOff.ToInvariantString());
						return;
					}
					if (ex.Message == Screenshots.ScreenshotDecoderHelper.MasterScreenNedded)
					{
						log.Info("AddWorkItem failed " + workItem + " (" + GetServerScheme() + ")", ex);
					}
					else
					{
						if (moveToDeadLetter)
							log.Error("AddWorkItem failed " + workItem + " (" + GetServerScheme() + ")", ex);
						else
							log.Warn("AddWorkItem failed " + workItem + " (" + GetServerScheme() + ")", ex);
					}
					throw;
				}
			}
			// Restore desktop windows
			foreach (var capture in workItem.DesktopCaptures)
			{
				DesktopWindow[] res;
				if (captureWindows.TryGetValue(capture, out res))
				{
					capture.DesktopWindows.Clear();
					capture.DesktopWindows.AddRange(res);
				}
			}
			RemoveCensoredScreenShots(workItem); //so they won't cause problems with storageManager and onlineStatsManager etc.
			Thread.MemoryBarrier(); //I'm not sure if this is needed or not... so to be on the safe side we sync
			ThreadPool.QueueUserWorkItem(_ =>		/*-*/
											{
												onlineStatsManager.AddWorkItem(workItem); //since we use Start/EndDate in OM without refreshing them from DB it isn't accurate
#if LEGACY
												statsBuilder.AddWorkItem(workItem);
#endif
												RetryHelper.RetryAtEvenIntervalsAsync(
													() => storageManager.TrySaveScreenShotsAsync(workItem),
													10, 10000, "saving screenshot")
													.ContinueWith(n => log.Error("Unexpected error in TrySaveScreenShotsAsync", n.Exception), TaskContinuationOptions.OnlyOnFaulted);
											});
			log.Debug("AddWorkItem success " + workItem.ToString() + " (" + GetServerScheme() + (isXff ? ",XFF": "") + ") in " + swAddWi.ToTotalMillisecondsString() + " ms, from which the SQL took " + swSql.ToTotalMillisecondsString() + " + " + swSql2.ToTotalMillisecondsString() + " ms" + (unknownHeaders != null ? $" (Headers: {unknownHeaders})" : ""));
		}

		public void AddWorkItemEx(WorkItem workItem)
		{
			Debug.Assert(workItem.ScreenShots.Count == 0);
			Debug.Assert(workItem.ActiveWindows == null || workItem.ActiveWindows.Count == 0);
			AddWorkItem(workItem);
		}

		private static void RemoveCensoredScreenShots(WorkItem workItem)
		{
			//old format
			if (workItem.ScreenShots != null && workItem.ScreenShots.Count != 0)
			{
				for (int i = 0; i < workItem.ScreenShots.Count; i++)
				{
					if (workItem.ScreenShots[i].Extension == "*C*")
					{
						workItem.ScreenShots.RemoveAt(i);
						i--;
					}
				}
			}
			//new format
			if (workItem.DesktopCaptures == null || workItem.DesktopCaptures.Count == 0) return;
			foreach (var desktopCapture in workItem.DesktopCaptures)
			{
				if (desktopCapture.Screens == null) continue;
				for (int i = 0; i < desktopCapture.Screens.Count; i++)
				{
					if (desktopCapture.Screens[i].Extension == "*C*")
					{
						desktopCapture.Screens.RemoveAt(i);
						i--;
					}
				}
			}
		}

		private static readonly string[] knownHeaders = new[] { "SOAPAction", "Content-Length", "Content-Type", "Accept-Encoding", "Authorization", "Expect", "Host"/*, "X-Forwarded-For"*/ };
		private static readonly Regex cutIpRegex = new Regex(@"\D*(?<ip>\d+\.\d+\.\d+\.\d+)\D*");
		private static string GetClientAddress(out bool isXff, out string unknownHeaders)
		{
			var webCtx = WebOperationContext.Current;
			if (webCtx != null)
			{
				try
				{
					var unknownKeys = webCtx.IncomingRequest.Headers.Keys.Cast<string>().Where(k => !knownHeaders.Contains(k)).ToArray();
					if (unknownKeys.Length > 0)
					{
						unknownHeaders = string.Join(", ", unknownKeys.Select(k => $"{k}={webCtx.IncomingRequest.Headers[k]}"));
					}
					else
						unknownHeaders = null;
					var xff = webCtx.IncomingRequest.Headers["X-Forwarded-For"];
					if (xff != null)
					{
						var match = cutIpRegex.Match(xff);
						if (match.Success)
						{
							isXff = true;
							return match.Groups["ip"].Value;
						}
					}
				}
				catch(InvalidOperationException)
				{
					// no http headers, no matter
					unknownHeaders = null;
				}
			}
			else
				unknownHeaders = null;
			var context = OperationContext.Current;
			var messageProperties = context.IncomingMessageProperties;
			var endpointProperty = messageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
			isXff = false;
			return endpointProperty == null ? null : endpointProperty.Address;
		}

		private static string GetServerScheme()
		{
			try
			{
				//return OperationContext.Current.EndpointDispatcher.EndpointAddress.Uri.Scheme;
				return OperationContext.Current.Channel.LocalAddress.Uri.Scheme;
			}
			catch (Exception ex)
			{
				log.Error("Unable to get server scheme", ex);
				return "N/A";
			}
		}

		#endregion

		#region GetClientMenu

		public ClientMenu GetClientMenu(int userId, string oldVersion, out string newVersion)
		{
			log.Verbose($"GetClientMenu started uid: {userId}");
			var sw = Stopwatch.StartNew();
			try
			{
				EnsureAccess(userId);
				using (var context = new ActivityRecorderDataClassesDataContext())
				{
					//fast path
					var version = menuVersionCacheManager.Get(userId, context);
					if (version == null)
					{
						newVersion = null;
						return null;
					}
					else if (version.ToString() == oldVersion)
					{
						newVersion = version.ToString();
						return null;
					}

					//slow path
					var clientSetting = context.GetClientMenu(userId);
					if (clientSetting == null)
					{
						newVersion = null;
						return null;
					}
					newVersion = clientSetting.Version;
					if (oldVersion == newVersion) return null;

					return clientSetting.Value;
				}
			}
			catch (Exception ex)
			{
				log.Error("GetClientMenu failed userId: " + userId.ToInvariantString(), ex);
				throw;
			}
			finally
			{
				log.Debug("Service call GetClientMenu userId: " + userId.ToInvariantString() + ", oldVersion: " + oldVersion + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
			//newVersion = "";

			//var cm = new ClientMenu()
			//{
			//    Works = new List<WorkData>()
			//    {
			//        new WorkData("őőúűáóüöíé", 23, 1),
			//        new WorkData("X", null, null)
			//            {
			//                Children = new List<WorkData>()
			//                            {
			//                                new WorkData("XY", null, null)
			//                                    {
			//                                        Children = new List<WorkData>()
			//                                                    {
			//                                                        new WorkData("XYX2", 2, 0) { CategoryId = 2 },
			//                                                        new WorkData("XYY4", 4, 0) { CategoryId = 1 },
			//                                                        new WorkData("XYZ3", 3, 0),
			//                                                    }
			//                                    },
			//                                new WorkData("", null, 0),
			//                                new WorkData("XZ1", 1, null),
			//                            }
			//            },
			//        new WorkData("devenv.exe", null, null)
			//            {
			//                ProjectId = 2,
			//                Children = new List<WorkData>()
			//                    {
			//                        new WorkData("40", 40, null),
			//                        new WorkData("41", 41, null),
			//                        new WorkData("42", 42, null),
			//                    }
			//            },
			//        new WorkData("firefox.exe", null, null)
			//            {
			//                ProjectId = 3,
			//                Children = new List<WorkData>()
			//                    {
			//                        new WorkData("50", 50, null),
			//                        new WorkData("51", 51, null),
			//                        new WorkData("52", 52, null),
			//                    }
			//            },
			//    },
			//    CategoriesById = new Dictionary<int, CategoryData>() { 
			//        {1, new CategoryData() { Id = 1, Name = "Gmail -"}},
			//        {2, new CategoryData() { Id = 2, Name = "Total"}},
			//    },
			//};
			//SetClientMenu(25, cm);
			////XmlPersistenceManager.SaveToFile("c:\\z\\xxx", cm);
			////XmlPersistenceManager.LoadFromFile("c:\\z\\xxx", out cm);

			////XmlPersistenceManager<ClientMenu>.SaveToFile("c:\\z\\xxx", cm);
			////cm = XmlPersistenceManager<ClientMenu>.LoadFromFile("c:\\z\\xxx");

			//return cm;
		}

		#endregion

		#region SetClientMenu

		public string SetClientMenu(int userId, ClientMenu newMenu)
		{
			log.Debug("Service call SetClientMenu userId: " + userId.ToInvariantString());
			try
			{
				EnsureAccess(userId); //it's not used atm.
				string menuData;
				using (var stream = new MemoryStream())
				{
					XmlPersistenceHelper.WriteToStream(stream, newMenu);
					menuData = Encoding.UTF8.GetString(stream.ToArray());
				}
				using (var contex = new ActivityRecorderDataClassesDataContext())
				{
					contex.SetXactAbortOn();
					var entity = contex.ClientSettings.Where(n => n.UserId == userId).SingleOrDefault();
					if (entity == null)
					{
						entity = new ClientSetting() { UserId = userId, Menu = menuData };
						contex.ClientSettings.InsertOnSubmit(entity);
					}
					else
					{
						entity.Menu = menuData;
					}
					contex.SubmitChanges();
					menuVersionCacheManager.Remove(userId);
					return entity.MenuVersion.ToString();
				}
			}
			catch (Exception ex)
			{
				log.Error("SetClientMenu failed userId: " + userId.ToInvariantString(), ex);
				throw;
			}
		}

		#endregion

		#region GetClientSettings

		public ClientSetting GetClientSettings(int userId, string oldVersion, out string newVersion)
		{
			log.Verbose($"GetClientSettings started uid: {userId}");
			var sw = Stopwatch.StartNew();
			try
			{
				EnsureAccess(userId);
				using (var contex = new ActivityRecorderDataClassesDataContext())
				{
					//fast path
					var version = clientSettingsVersionCacheManager.Get(userId, contex);
					if (version == null)
					{
						newVersion = null;
						return null;
					}
					else if (version.ToString() == oldVersion)
					{
						newVersion = version.ToString();
						return null;
					}

					//slow path
					var clientSetting = contex.ClientSettings.Where(n => n.UserId == userId).SingleOrDefault();
					if (clientSetting == null)
					{
						newVersion = null;
						return null;
					}
					newVersion = clientSetting.ClientSettingsVersion.ToString();
					if (oldVersion == newVersion) return null;

					return clientSetting;
				}
			}
			catch (Exception ex)
			{
				log.Error("GetClientSettings failed userId: " + userId.ToInvariantString(), ex);
				throw;
			}
			finally
			{
				log.Debug("Service call GetClientSettings userId: " + userId.ToInvariantString() + ", oldVersion: " + oldVersion + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}

		#endregion

		#region GetTodaysWorkTimeStats

#if LEGACY
		public WorkTimeStats GetTodaysWorkTimeStats(int userId)
		{
			var sw = Stopwatch.StartNew();
			try
			{
				EnsureAccess(userId);
				int groupId;
				int companyId;
				if (!UserIdManager.Instance.TryGetIdsForUser(userId, out groupId, out companyId))
				{
					return null;
				}
				return statsBuilder.GetTodaysWorkTimeStats(userId, groupId, companyId);
			}
			catch (Exception ex)
			{
				log.Error("GetTodaysWorkTimeStats failed userId: " + userId.ToInvariantString(), ex);
				throw;
			}
			finally
			{
				log.Debug("Service call GetTodaysWorkTimeStats userId: " + userId.ToInvariantString() + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}
#endif
		#endregion

		#region GetClientWorkTimeStats

		public ClientWorkTimeStats GetClientWorkTimeStats(int userId)
		{
			log.Verbose($"GetClientWorkTimeStats started uid: {userId}");
			var sw = Stopwatch.StartNew();
			try
			{
				EnsureAccess(userId);
				int groupId;
				int companyId;
				if (!UserIdManager.Instance.TryGetIdsForUser(userId, out groupId, out companyId))
				{
					return null; //User is not active
				}
				var stats = onlineStatsManager.GetBriefUserStats(userId);
				if (stats == null) return null;
				return new ClientWorkTimeStats()
				{
					TodaysWorkTime = stats.TodaysWorkTime,
					ThisWeeksWorkTime = stats.ThisWeeksWorkTime,
					ThisMonthsWorkTime = stats.ThisMonthsWorkTime,
					TodaysTargetNetWorkTime = stats.TodaysTargetNetWorkTime,
					ThisWeeksTargetNetWorkTime = stats.ThisWeeksTargetNetWorkTime,
					ThisMonthsTargetNetWorkTime = stats.ThisMonthsTargetNetWorkTime,
					ThisWeeksTargetUntilTodayNetWorkTime = stats.ThisWeeksTargetUntilTodayNetWorkTime,
					ThisMonthsTargetUntilTodayNetWorkTime = stats.ThisMonthsTargetUntilTodayNetWorkTime,
				};
			}
			catch (Exception ex)
			{
				log.Error("GetClientWorkTimeStats failed userId: " + userId.ToInvariantString(), ex);
				throw;
			}
			finally
			{
				log.Debug("Service call GetClientWorkTimeStats userId: " + userId.ToInvariantString() + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}

		#endregion

		#region GetTotalWorkTimeStats

		public TotalWorkTimeStats GetTotalWorkTimeStats(int userId)
		{
			log.Verbose($"GetTotalWorkTimeStats started uid: {userId}");
			var sw = Stopwatch.StartNew();
			try
			{
				EnsureAccess(userId);
#if LEGACY
				return totalWorkTimeBuilder.GetTotalWorkTimeStats(userId);
#endif
				//use lightweight and more accurate simpleWorkTimeStatsManager insted of totalWorkTimeBuilder
				var simple = simpleWorkTimeStatsManager.GetSimpleWorkTimeStats(userId, DateTime.UtcNow);
				if (simple == null) return null;
				var result = new TotalWorkTimeStats()
				{
					UserId = simple.UserId,
					FromDate = simple.FromDate,
					ToDate = simple.ToDate,
					Stats = new Dictionary<int, TotalWorkTimeStat>(),
				};
				foreach (var simpleWorkTimeStat in simple.Stats.Values)
				{
					result.Stats.Add(simpleWorkTimeStat.WorkId, TotalWorkTimeStat.CreateFrom(simpleWorkTimeStat));
				}
				return result;
			}
			catch (Exception ex)
			{
				log.Error("GetTotalWorkTimeStats failed userId: " + userId.ToInvariantString(), ex);
				throw;
			}
			finally
			{
				log.Debug("Service call GetTotalWorkTimeStats userId: " + userId.ToInvariantString() + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}

		#endregion

		#region GeStartOfDayOffset

		public TimeSpan GetStartOfDayOffset(int userId)
		{
			log.Verbose($"GetStartOfDayOffset started uid: {userId}");
			var sw = Stopwatch.StartNew();
			try
			{
				EnsureAccess(userId);
				var stat = StatsDbHelper.GetUserStatsInfo(new List<int>(new[] { userId })).FirstOrDefault();
				return stat != null ? stat.StartOfDayOffset : new TimeSpan(3, 0, 0);
			}
			catch (Exception ex)
			{
				log.Error("GetStartOfDayOffset failed userId: " + userId.ToInvariantString(), ex);
				throw;
			}
			finally
			{
				log.Debug("Service call GetStartOfDayOffset userId: " + userId.ToInvariantString() + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}
		#endregion

		#region GetSimpleWorkTimeStats

		public SimpleWorkTimeStats GetSimpleWorkTimeStats(int userId, DateTime? desiredEndDate)
		{
			log.Verbose($"GetSimpleWorkTimeStats started uid: {userId}");
			var sw = Stopwatch.StartNew();
			try
			{
				EnsureAccess(userId);
				return simpleWorkTimeStatsManager.GetSimpleWorkTimeStats(userId, desiredEndDate ?? DateTime.UtcNow);
			}
			catch (Exception ex)
			{
				log.Error("GetSimpleWorkTimeStats failed userId: " + userId.ToInvariantString(), ex);
				throw;
			}
			finally
			{
				log.Debug("Service call GetSimpleWorkTimeStats userId: " + userId.ToInvariantString() + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}

		public SimpleWorkTimeStats GetSimpleWorkTimeStatsById(int userId, DateTime? desiredEndDate)
		{
			var sw = Stopwatch.StartNew();
			try
			{
				return simpleWorkTimeStatsManager.GetSimpleWorkTimeStats(userId, desiredEndDate ?? DateTime.UtcNow);
			}
			catch (Exception ex)
			{
				log.Error("GetSimpleWorkTimeStatsById failed userId: " + userId.ToInvariantString(), ex);
				throw;
			}
			finally
			{
				log.Debug("Service call GetSimpleWorkTimeStatsById userId: " + userId.ToInvariantString() + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}

		#endregion

		#region GetDailyWorkTimeStats

		public List<DailyWorkTimeStats> GetDailyWorkTimeStats(int userId, long oldVersion)
		{
			log.Verbose($"GetDailyWorkTimeStats started uid: {userId}");
			var sw = Stopwatch.StartNew();
			try
			{
				EnsureAccess(userId);
				return dailyWorkTimesManager.GetDailyWorkTimeStats(userId, oldVersion);
			}
			catch (Exception ex)
			{
				log.Error("GetDailyWorkTimeStats failed userId: " + userId.ToInvariantString(), ex);
				throw;
			}
			finally
			{
				log.Debug("Service call GetDailyWorkTimeStats userId: " + userId.ToInvariantString() + " ver: " + oldVersion.ToInvariantString() + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}

		#endregion

		#region Authenticate

		public AuthData Authenticate(string clientInfo)
		{
			//we are already authenticated if this is called, so we just return some useful info
			var sw = Stopwatch.StartNew();
			string info = clientInfo;
			try
			{
				int userId = GetUserId(null);
				if (!string.IsNullOrEmpty(info))
				{
					var computerId = -1;
					var osName = "n/a";
					var szAddress = "n/a";
					try
					{
						var json = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(info);
						try
						{
							computerId = int.Parse(json.Value<string>("ComputerId"), NumberStyles.HexNumber);
						}
						catch (Exception ex)
						{
							log.Debug($"Can't get computerId from clientinfo userId: {userId} info: {info}", ex);
						}
						try
						{
							osName = json.Value<string>("OSName");
						}
						catch (Exception ex)
						{
							log.Debug($"Can't get OS name from clientinfo userId: {userId} info: {info}", ex);
						}
					}
					catch (Exception ex)
					{
						log.Debug($"Can't deserialize clientinfo userId: {userId} info: {info}", ex);
					}

					try
					{
						OperationContext oOperationContext = OperationContext.Current;
						MessageProperties oMessageProperties = oOperationContext.IncomingMessageProperties;
						RemoteEndpointMessageProperty oRemoteEndpointMessageProperty = (RemoteEndpointMessageProperty)oMessageProperties[RemoteEndpointMessageProperty.Name];
						szAddress = oRemoteEndpointMessageProperty.Address;
					}
					catch (Exception ex)
					{
						log.Debug($"Can't query client's ip userId: {userId} info: {info}", ex);
					}
					bool isNewDevice;
					using (var context = new ClientComputerInfoDataClassesDataContext())
					{
						isNewDevice = !context.IsComputerUsedByUser(userId, computerId);
					}

					log.Debug($"Login from client userId: {userId} computerId: {computerId} clientIP: {szAddress} os: {osName} newDevice: {isNewDevice}");

#if DEBUG
					if (false)
#endif
					using (var client = new Website.WebsiteClientWrapper())
					using (var context = new JobControlDataClassesDataContext())
					{
						client.Client.SignedInFromClient(new Guid(context.GetAuthTicket(userId)), computerId, isNewDevice, szAddress, osName);
					}
				}
				log.Verbose($"Authenticate started uid: {userId}");
				info = "userId:" + userId.ToInvariantString() + " Info:" + clientInfo;
				using (var context = new JobControlDataClassesDataContext())
				{
					var userStatInfo = context.GetUserStatInfoById(userId); //if the user is Authenticated then it should have and email
					return new AuthData() { Id = userStatInfo.Id, Email = userStatInfo.Email, Name = userStatInfo.Name, FirstName = userStatInfo.FirstName, LastName = userStatInfo.LastName, AccessLevel = userStatInfo.AccessLevel, TimeZoneData = userStatInfo.TimeZone.ToSerializedString() };
				}
			}
			catch (Exception ex)
			{
				log.Error("Authenticate (" + info + ") failed", ex);
				throw;
			}
			finally
			{
				log.Info("Service call Authenticate (" + info + ") finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}

		#endregion

		#region Logout

		public void Disconnect(int userId, int computerId, string osName)
		{
			var sw = Stopwatch.StartNew();
			try
			{
				string szAddress = null;
				try
				{
					OperationContext oOperationContext = OperationContext.Current;
					MessageProperties oMessageProperties = oOperationContext.IncomingMessageProperties;
					RemoteEndpointMessageProperty oRemoteEndpointMessageProperty = (RemoteEndpointMessageProperty)oMessageProperties[RemoteEndpointMessageProperty.Name];
					szAddress = oRemoteEndpointMessageProperty.Address;
				}
				catch (Exception ex)
				{
					log.Debug($"Can't query client's ip userId: {userId} computerId: {computerId}", ex);
				}

#if DEBUG
				if (false)
#endif
					using (var client = new Website.WebsiteClientWrapper())
					using (var context = new JobControlDataClassesDataContext())
					{
						client.Client.SignedOutFromClient(new Guid(context.GetAuthTicket(userId)), computerId, szAddress, osName);
					}
			}
			catch (Exception ex)
			{
				log.Error($"Disconnect (userId: {userId}) failed", ex);
				throw;
			}
			finally
			{
				log.Info($"Service call Disconnect (userId: {userId}) finished in {sw.ToTotalMillisecondsString()}ms");
			}
		}

		#endregion

		#region GetAuthTicket

		public string GetAuthTicket(int userId)
		{
			log.Verbose($"GetAuthTicket started uid: {userId}");
			var sw = Stopwatch.StartNew();
			try
			{
				EnsureAccess(userId);
				using (var context = new JobControlDataClassesDataContext())
				{
					return context.GetAuthTicket(userId);
				}
			}
			catch (Exception ex)
			{
				log.Error("GetAuthTicket failed for userId: " + userId.ToInvariantString(), ex);
				throw;
			}
			finally
			{
				log.Info("Service call GetAuthTicket userId: " + userId.ToInvariantString() + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}

		#endregion

		#region GetClientRules

		public List<WorkDetectorRule> GetClientRules(int userId, string oldVersion, out string newVersion)
		{
			log.Verbose($"GetClientRules started uid: {userId}");
			var sw = Stopwatch.StartNew();
			try
			{
				EnsureAccess(userId);
				using (var context = new ActivityRecorderDataClassesDataContext())
				{
#if DEBUG
					if (isAutomaticRulesSwitchedOffDebug)
					{
						var setting = context.GetWorkDetectorRules(userId);
						setting.Version += "!";
						newVersion = setting.Version;
						if (oldVersion == newVersion) return null;

						setting.Value.ForEach(r => r.ProcessRule = r.ProcessRule.Replace(@"((?=.*\.).*)", "!nomatch!"));
						return setting.Value;
					}
#endif
					//fast path
					var version = workDetectorRulesVersionCacheManager.Get(userId, context);
					if (version == null)
					{
						newVersion = null;
						return null;
					}
					else if (WorkTimeSpecificBuilder.PatchRules(userId, version.ToString(), ref oldVersion, out newVersion,
						         out var value))
					{
						return value;
					}
					else if (version.ToString() == oldVersion)
					{
						newVersion = version.ToString();
						return null;
					}

					//slow path
					var clientSetting = context.GetWorkDetectorRules(userId);
					if (clientSetting == null)
					{
						newVersion = null;
						return null;
					}
					newVersion = clientSetting.Version;
					if (oldVersion == newVersion) return null;

					return clientSetting.Value;
				}
			}
			catch (Exception ex)
			{
				log.Error("GetClientRules failed userId: " + userId.ToInvariantString(), ex);
				throw;
			}
			finally
			{
				log.Debug("Service call GetClientRules userId: " + userId.ToInvariantString() + ", oldVersion: " + oldVersion + " finished in " +
						  sw.ToTotalMillisecondsString() + "ms");
			}
		}
		#endregion

		#region SetClientRules

		public string SetClientRules(int userId, List<WorkDetectorRule> newRules)
		{
			log.Debug("Service call SetClientRules userId: " + userId.ToInvariantString());
			try
			{
				EnsureAccess(userId); //it's not used atm.
				string rulesData;
				using (var stream = new MemoryStream())
				{
					XmlPersistenceHelper.WriteToStream(stream, newRules);
					rulesData = Encoding.UTF8.GetString(stream.ToArray());
				}
				using (var contex = new ActivityRecorderDataClassesDataContext())
				{
					contex.SetXactAbortOn();
					var entity = contex.ClientSettings.Where(n => n.UserId == userId).SingleOrDefault();
					if (entity == null)
					{
						entity = new ClientSetting() { UserId = userId, WorkDetectorRules = rulesData };
						contex.ClientSettings.InsertOnSubmit(entity);
					}
					else
					{
						entity.WorkDetectorRules = rulesData;
					}
					contex.SubmitChanges();
					workDetectorRulesVersionCacheManager.Remove(userId);
					return entity.WorkDetectorRulesVersion.ToString();
				}
			}
			catch (Exception ex)
			{
				log.Error("SetClientRules failed userId: " + userId.ToInvariantString(), ex);
				throw;
			}
		}
		#endregion

		#region GetClientCensorRules

		public List<CensorRule> GetClientCensorRules(int userId, string oldVersion, out string newVersion)
		{
			log.Verbose($"GetClientCensorRules started uid: {userId}");
			var sw = Stopwatch.StartNew();
			try
			{
				EnsureAccess(userId);
				using (var context = new ActivityRecorderDataClassesDataContext())
				{
					//fast path
					var version = censorRulesVersionCacheManager.Get(userId, context);
					if (version == null)
					{
						newVersion = null;
						return null;
					}
					else if (version.ToString() == oldVersion)
					{
						newVersion = version.ToString();
						return null;
					}

					//slow path
					var clientSetting = context.GetCensorRules(userId);
					if (clientSetting == null)
					{
						newVersion = null;
						return null;
					}
					newVersion = clientSetting.Version;
					if (oldVersion == newVersion) return null;

					return clientSetting.Value;
				}
			}
			catch (Exception ex)
			{
				log.Error("GetClientCensorRules failed userId: " + userId.ToInvariantString(), ex);
				throw;
			}
			finally
			{
				log.Debug("Service call GetClientCensorRules userId: " + userId.ToInvariantString() + ", oldVersion: " + oldVersion + " finished in " +
						  sw.ToTotalMillisecondsString() + "ms");
			}
		}
		//if (userId == 13) SetClientCensorRules(userId, new List<CensorRule>() { 
		//    new CensorRule() { IgnoreCase = true, IsEnabled = true, IsRegex = false, Name = "Secret studio", ProcessRule = "devenv.exe", TitleRule = "*", RuleType = CensorRuleType.HideScreenShot | CensorRuleType.HideTitle }, 
		//    new CensorRule() { IgnoreCase = true, IsEnabled = true, IsRegex = false, Name = "Secret title jc", ProcessRule = "JobCTRL.exe", TitleRule = "*", RuleType = CensorRuleType.HideTitle }, 
		//    new CensorRule() { IgnoreCase = true, IsEnabled = true, IsRegex = false, Name = "Secret shot tc", ProcessRule = "*", TitleRule = "Total Commander*", RuleType = CensorRuleType.HideScreenShot }, 
		//});
		#endregion

		#region SetClientCensorRules

		public string SetClientCensorRules(int userId, List<CensorRule> newRules)
		{
			log.Debug("Service call SetClientCensorRules userId: " + userId.ToInvariantString());
			try
			{
				EnsureAccess(userId); //it's not used atm.
				string rulesData;
				using (var stream = new MemoryStream())
				{
					XmlPersistenceHelper.WriteToStream(stream, newRules);
					rulesData = Encoding.UTF8.GetString(stream.ToArray());
				}
				using (var contex = new ActivityRecorderDataClassesDataContext())
				{
					contex.SetXactAbortOn();
					var entity = contex.ClientSettings.Where(n => n.UserId == userId).SingleOrDefault();
					if (entity == null)
					{
						entity = new ClientSetting() { UserId = userId, CensorRules = rulesData };
						contex.ClientSettings.InsertOnSubmit(entity);
					}
					else
					{
						entity.CensorRules = rulesData;
					}
					contex.SubmitChanges();
					censorRulesVersionCacheManager.Remove(userId);
					return entity.CensorRulesVersion.ToString();
				}
			}
			catch (Exception ex)
			{
				log.Error("SetClientCensorRules failed userId: " + userId.ToInvariantString(), ex);
				throw;
			}
		}
		#endregion

		#region GetClientCapturingRules

		public CollectorRules GetClientCollectorRules(int userId, string oldVersion, out string newVersion)
		{
			//newVersion = "3";
			//return new CollectorRules()
			//{
			//	Rules = new List<CollectorRule>()
			//	{
			//		new CollectorRule()
			//		{
			//			IsEnabled = true,
			//			IsRegex = true,
			//			ProcessRule = "(?<ProcessName>.*)",
			//			TitleRule = "(?<Title>.*)",
			//			UrlRule = "^(?<Url>[^?#@]*)",
			//			ExtensionRulesByIdByKey = new Dictionary<string, Dictionary<string, string>>()
			//			{
			//				{ "JobCTRL.Outlook", new Dictionary<string, string>() { 
			//					{ "FromEmail", "(?<MailFrom>.*)" },
			//					{ "RecipientsEmail", "(?<MailTo>.*)" },
			//					{ "Id", "(?<MailId>.*)" },
			//				}},
			//				{ "JobCTRL.Office", new Dictionary<string, string>() { 
			//					{ "DocumentPath", "(?<DocumentPath>.*)" },
			//					{ "DocumentFileName", "(?<DocumentFileName>.*)" },
			//				}},
			//				{ "JobCTRL.DomCapture", new Dictionary<string, string>() { 
			//					{ "UserAgentKey", "(?<UserAgentGroup>.*)" },
			//				}},
			//			},
			//			ExtensionRuleParametersById = new Dictionary<string,List<ExtensionRuleParameter>>()
			//			{
			//				{"JobCTRL.DomCapture", new List<ExtensionRuleParameter> { new ExtensionRuleParameter { Name = "DomCapture" , Value = @"[{""EvalString"":""navigator.userAgent"",""Key"":""UserAgentKey"",""PropertyName"":null,""Selector"":null,""UrlPattern"":"".*""}]"}}}
			//			},
			//			FormattedNamedGroups = new Dictionary<string,string>() { { "UserAgent" , "{UserAgentGroup}" }},
			//			CapturedKeys = new List<string>() { "ProcessName", "Title", "Url", "MailFrom" , "MailTo" , "MailId", "DocumentPath", "DocumentFileName", "UserAgent" },
			//		}
			//	}
			//};

			log.Verbose($"GetClientCollectorRules started uid: {userId}");
			var sw = Stopwatch.StartNew();
			try
			{
				EnsureAccess(userId);
				using (var context = new ActivityRecorderDataClassesDataContext())
				{
					//fast path
					var version = collectorRulesVersionCacheManager.Get(userId, context);
					if (version == null)
					{
						newVersion = null;
						return null;
					}
					else if (version.ToString() == oldVersion)
					{
						newVersion = version.ToString();
						return null;
					}

					//slow path
					var clientSetting = context.GetCollectorRules(userId);
					if (clientSetting == null)
					{
						newVersion = null;
						return null;
					}
					newVersion = clientSetting.Version;
					if (oldVersion == newVersion) return null;

					return clientSetting.Value;
				}
			}
			catch (Exception ex)
			{
				log.Error("GetClientCollectorRules failed userId: " + userId.ToInvariantString(), ex);
				throw;
			}
			finally
			{
				log.Debug("Service call GetClientCollectorRules userId: " + userId.ToInvariantString() + ", oldVersion: " + oldVersion + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}
		#endregion

		#region SetClientCapturingRules

		public string SetClientCollectorRules(int userId, CollectorRules newRules)
		{
			log.Debug("Service call SetClientCapturingRules userId: " + userId.ToInvariantString());
			try
			{
				EnsureAccess(userId); //it's not used atm.
				string rulesData;
				using (var stream = new MemoryStream())
				{
					XmlPersistenceHelper.WriteToStream(stream, newRules);
					rulesData = Encoding.UTF8.GetString(stream.ToArray());
				}
				using (var contex = new ActivityRecorderDataClassesDataContext())
				{
					contex.SetXactAbortOn();
					var entity = contex.ClientSettings.Where(n => n.UserId == userId).SingleOrDefault();
					if (entity == null)
					{
						entity = new ClientSetting() { UserId = userId, CollectorRules = rulesData };
						contex.ClientSettings.InsertOnSubmit(entity);
					}
					else
					{
						entity.CollectorRules = rulesData;
					}
					contex.SubmitChanges();
					collectorRulesVersionCacheManager.Remove(userId);
					return entity.CollectorRulesVersion.ToString();
				}
			}
			catch (Exception ex)
			{
				log.Error("SetClientCollectorRules failed userId: " + userId.ToInvariantString(), ex);
				throw;
			}
		}
		#endregion

		#region AddCollectedItem

		public void AddCollectedItem(CollectedItem collectedItem)
		{
			log.Verbose($"AddCollectedItem started uid: {collectedItem.UserId}");
			if (collectedItem == null) return;
			var sw = Stopwatch.StartNew();
			try
			{
				EnsureAccess(collectedItem.UserId);
				CollectedItemDbHelper.Insert(collectedItem);
			}
			catch (Exception ex)
			{
				log.Error("AddCollectedItem failed userId: " + collectedItem.UserId.ToInvariantString(), ex);
				throw;
			}
			finally
			{
				log.Debug("Service call AddCollectedItem (" + (collectedItem.CapturedValues == null ? "(null)" : collectedItem.CapturedValues.Count.ToInvariantString()) + ") userId: " + collectedItem.UserId.ToInvariantString() + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}
		#endregion

		#region AddAggregateCollectedItems

		public void AddAggregateCollectedItems(AggregateCollectedItems collectedItems)
		{
			log.Verbose($"AddAggregateCollectedItems started uid: {collectedItems?.UserId}");
			if (collectedItems == null) return;
			var sw = Stopwatch.StartNew();
			try
			{
				EnsureAccess(collectedItems.UserId);
				if (collectedItems.CreateDate != null)
				{
					var uploadTime = DateTime.UtcNow - collectedItems.CreateDate.Value;
					log.Debug("CollectedItems userId: " + collectedItems.UserId.ToInvariantString() + " count: " + collectedItems.Items.Count + " uploaded after " + uploadTime.ToHourMinuteString());
				}

				CollectedItemDbHelper.Insert(collectedItems);
			}
			catch (Exception ex)
			{
				log.Error("AddAggregateCollectedItems failed userId: " + collectedItems.UserId.ToInvariantString(), ex);
				throw;
			}
			finally
			{
				log.Debug("Service call AddAggregateCollectedItems (" + (collectedItems.Items == null ? "(null)" : collectedItems.Items.Count.ToInvariantString()) + ") userId: " + collectedItems.UserId.ToInvariantString() + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}
		#endregion

		#region Issue Mgmt

		public void AddIssue(IssueData issue)
		{
			log.Verbose($"AddIssue started uid: {issue?.UserId}");
			if (issue == null) return;
			var sw = Stopwatch.StartNew();
			try
			{
				EnsureAccess(issue.UserId);
				int groupId;
				int companyId;
				if (!UserIdManager.Instance.TryGetIdsForUser(issue.UserId, out groupId, out companyId))
				{
					throw new ArgumentException("User is not active");
				}
				using (var context = new ActivityRecorderDataClassesDataContext())
				{
					context.SetXactAbortOn();
					var item = new Issue
					{
						IssueCode = issue.IssueCode,
						CompanyId = companyId,
						Name = issue.Name.Length > 100 ? issue.Name.Substring(0, 100) : issue.Name,
						Company = issue.Company.Length > 50 ? issue.Company.Substring(0, 50) : issue.Company,
						State = issue.State,
						CreatedAt = issue.Modified,
						CreatedBy = issue.UserId,
						ModifiedAt = issue.Modified,
						ModifiedBy = issue.UserId
					};
					context.Issues.InsertOnSubmit(item);
					context.SubmitChanges();
				}
			}
			catch (SqlException ex)
			{
				if (ex.Number != 2627) throw;
				ModifyIssue(issue);
			}
			catch (Exception ex)
			{
				log.Error("AddIssue failed userId: " + issue.UserId.ToInvariantString(), ex);
				throw;
			}
			finally
			{
				log.Debug("Service call AddIssue (" + issue.IssueCode + ") userId: " + issue.UserId.ToInvariantString() + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}

		public void ModifyIssue(IssueData issue)
		{
			if (issue == null) return;
			var sw = Stopwatch.StartNew();
			try
			{
				EnsureAccess(issue.UserId);
				int groupId;
				int companyId;
				if (!UserIdManager.Instance.TryGetIdsForUser(issue.UserId, out groupId, out companyId))
				{
					throw new ArgumentException("User is not active");
				}
				using (var context = new ActivityRecorderDataClassesDataContext())
				{
					context.SetXactAbortOn();
					var item = context.Issues.SingleOrDefault(i => i.IssueCode == issue.IssueCode && i.CompanyId == companyId);
					if (item == null) return;
					item.Name = issue.Name.Length > 100 ? issue.Name.Substring(0, 100) : issue.Name;
					item.Company = issue.Company.Length > 50 ? issue.Company.Substring(0, 50) : issue.Company;
					item.State = issue.State;
					item.ModifiedAt = issue.Modified;
					item.ModifiedBy = issue.UserId;
					context.SubmitChanges();
				}
			}
			catch (Exception ex)
			{
				log.Error("ModifyIssue failed userId: " + issue.UserId.ToInvariantString(), ex);
				throw;
			}
			finally
			{
				log.Debug("Service call ModifyIssue (" + issue.IssueCode + ") userId: " + issue.UserId.ToInvariantString() + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}

		public IssueData GetIssue(int userId, string issueCode)
		{
			var sw = Stopwatch.StartNew();
			try
			{
				EnsureAccess(userId);
				int groupId;
				int companyId;
				if (!UserIdManager.Instance.TryGetIdsForUser(userId, out groupId, out companyId))
				{
					throw new ArgumentException("User is not active");
				}
				using (var context = new ActivityRecorderDataClassesDataContext())
				{
					var item = context.Issues.SingleOrDefault(i => i.IssueCode == issueCode && i.CompanyId == companyId);
					if (item == null) return null;
					var issue = new IssueData
					{
						IssueCode = issueCode,
						Name = item.Name,
						Company = item.Company,
						State = item.State,
						Modified = item.ModifiedAt,
						UserId = item.ModifiedBy,
						CreatedByUserId = item.CreatedBy,
						ModifiedByName = UserNameLookup(item.ModifiedBy),
						CreatedByName = UserNameLookup(item.CreatedBy),
					};
					return issue;
				}
			}
			catch (Exception ex)
			{
				log.Error("GetIssue failed userId: " + userId.ToInvariantString(), ex);
				throw;
			}
			finally
			{
				log.Debug("Service call GetIssue (" + issueCode + ") userId: " + userId.ToInvariantString() + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}

		/// <summary>
		/// Queries a filtered set of issues
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="keywords">searchable keywords in lower case</param>
		/// <param name="filterState"></param>
		/// <param name="filterOwner">true: issues created by me, false: issues modified by me, null: both of them</param>
		/// <returns></returns>
		public List<IssueData> FilterIssues(int userId, List<string> keywords, int? filterState, bool? filterOwner)
		{
			var sw = Stopwatch.StartNew();
			try
			{
				EnsureAccess(userId);
				int groupId;
				int companyId;
				if (!UserIdManager.Instance.TryGetIdsForUser(userId, out groupId, out companyId))
				{
					throw new ArgumentException("User is not active");
				}
				using (var context = new ActivityRecorderDataClassesDataContext())
				{
					var issues = context.Issues.Where(i => i.CompanyId == companyId);
					if (keywords != null && keywords.Count > 0)
					{
						issues =
							keywords.Select(
								filter => issues.Where(i => i.Name.ToLower().Contains(filter) || i.Company.ToLower().Contains(filter)))
								.Aggregate<IQueryable<Issue>, IQueryable<Issue>>(null,
									(current, one) => current != null ? current.Union(one) : one);
					}
					if (filterState.HasValue)
						issues = issues.Where(i => filterState.Value == i.State);
					issues = filterOwner.HasValue
						? issues.Where(i => filterOwner.Value && i.CreatedBy == userId || !filterOwner.Value && i.ModifiedBy == userId)
						: issues.Where(i => i.CreatedBy == userId || i.ModifiedBy == userId);
					return issues.Select(i => new IssueData
					{
						IssueCode = i.IssueCode,
						Name = i.Name,
						Company = i.Company,
						State = i.State,
						Modified = i.ModifiedAt,
						UserId = i.ModifiedBy,
						CreatedByUserId = i.CreatedBy,
						ModifiedByName = UserNameLookup(i.ModifiedBy),
						CreatedByName = UserNameLookup(i.CreatedBy),
					}).ToList();
				}
			}
			catch (Exception ex)
			{
				log.Error("FilterIssues failed userId: " + userId.ToInvariantString(), ex);
				throw;
			}
			finally
			{
				log.Debug("Service call FilterIssues userId: " + userId.ToInvariantString() + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}

		private string UserNameLookup(int userId)
		{
			var userStat = onlineStatsManager.GetBriefUserStats(userId);
			return userStat != null ? userStat.UserName : userId > 0 ? "UserId: " + userId : "";
		}

		#endregion

		#region ClientMessageService

		public List<WebsiteServiceReference.Message> GetMessages(int userId, DateTime? lastMessageLastChangeDate, int computerId)
		{
			return SafeRun(userId, () => MessageService.Instance.GetMessages(userId, lastMessageLastChangeDate, computerId));
		}

		public DateTime MarkMessageAsRead(int userId, int messageId, int computerId)
			{
			return SafeRun(userId, () => MessageService.Instance.MarkMassageAsRead(userId, messageId, computerId));
		}

		#endregion

		#region TodoLists

		public TodoListDTO GetTodoList(int userId, DateTime date)
		{
			log.Verbose($"GetTodoList started uid: {userId}");
			return TodoListService.Instance.GetTodoList(userId, date);
		}
		
		public bool CreateOrUpdateTodoList(TodoListDTO todoListDTO)
		{
			log.Verbose($"CreateOrUpdateTodoList started uid: {todoListDTO.UserId}");
			return TodoListService.Instance.CreateOrUpdateTodoList(todoListDTO);
		}

		public List<TodoListItemStatusDTO> GetTodoListItemStatuses()
		{
			log.Verbose($"GetTodoListItemStatuses started");
			return TodoListService.Instance.GetTodoListItemStatuses();
		}

		public TodoListDTO GetMostRecentTodoList(int userId)
		{
			log.Verbose($"GetMostRecentTodoList started uid: {userId}");
			return TodoListService.Instance.GetLastTodoList(userId);
		}

		public TodoListToken AcquireTodoListLock(int userId, int todoListId)
		{
			log.Verbose($"AcquireTodoListLock started uid: {userId}");
			return TodoListService.Instance.AcquireTodoListLock(userId, todoListId);
		}

		public bool ReleaseTodoListLock(int userId, int todoListId)
		{
			log.Verbose($"ReleaseTodoListLock started uid: {userId}");
			return TodoListService.Instance.ReleaseTodoListLock(userId, todoListId);
		}

		#endregion

		#region ReportsInClient

		public DisplayedReports GetDisplayedReports(int userId, string culture)
		{
			log.Verbose($"GetDisplayedReports started uid: {userId}");
			var sw = Stopwatch.StartNew();
			try {
#if DEBUG
				//throw new FaultException("Error occurred in PcServerMyPerformance (custom) report");
				System.Drawing.ImageConverter converter = new System.Drawing.ImageConverter();
				var result = new DisplayedReports()
				{
					LayoutXml = "<row><img id=\"pic1\" /></row><row><img id=\"pic2\" /></row><row><img id=\"pic3\" /></row>",
					ReportImages = new DisplayedReportImage[3]
					{
						new DisplayedReportImage()
						{
							Id = "pic1",
							ReportImage = (byte[])converter.ConvertTo(Properties.Resources.barchart, typeof(byte[]))
						},
						new DisplayedReportImage()
						{
							Id = "pic2",
							ReportImage = (byte[])converter.ConvertTo(Properties.Resources.productivity, typeof(byte[]))
						},
						new DisplayedReportImage()
						{
							Id = "pic3",
							ReportImage = (byte[])converter.ConvertTo(Properties.Resources.devices, typeof(byte[]))
						}
					}
				};
				return result;

#else
				using (var clientWrapper = new WebsiteClientWrapper())
				using (var context = new JobControlDataClassesDataContext())
				{
					try
					{
						var reports = clientWrapper.Client.GetDisplayedReportsForUser(new Guid(context.GetAuthTicket(userId)), culture, null);
						return reports;
					}
					catch (FaultException ex)
					{
						log.Error(ex);
						throw;
					}
					catch (Exception ex)
					{
						log.Error("Unexpected exception in GetDisplayedReports", ex);
						throw new FaultException("Unexpected exception.");
					}
				}
#endif
			}
			catch (Exception ex)
			{
				log.Error("GetDisplayedReports failed for user " + userId.ToInvariantString(), ex);
				throw;
			}
			finally
			{
				log.Debug("GetDisplayedReports for user " + userId.ToInvariantString() + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}

		public FavoriteReport[] GetFavoriteReports(int userId, string culture)
		{
			log.Verbose($"GetFavoriteReportsForUser started uid: {userId}");
			var sw = Stopwatch.StartNew();
			try
			{
#if DEBUG
				//throw new FaultException("Error occurred in PcServerMyPerformance (custom) report");
				System.Drawing.ImageConverter converter = new System.Drawing.ImageConverter();
				var reports = new FavoriteReport[3]
				{
					new FavoriteReport()
					{
						Name = "Long report text for two rows",
						Icon = (byte[]) converter.ConvertTo(Properties.Resources.barchart, typeof(byte[])),
						Url = ""
					},
					new FavoriteReport()
					{
						Name = "Short report text",
						Icon = (byte[]) converter.ConvertTo(Properties.Resources.productivity, typeof(byte[]))
					},
					new FavoriteReport()
					{
						Name = "Another report",
						Icon = (byte[]) converter.ConvertTo(Properties.Resources.devices, typeof(byte[]))
					}
				};
				return reports;

#else
				using (var clientWrapper = new WebsiteClientWrapper())
				using (var context = new JobControlDataClassesDataContext())
				{
					try
					{
						var reports = clientWrapper.Client.GetFavoriteReportsForUser(new Guid(context.GetAuthTicket(userId)), culture);
						return reports;
					}
					catch (FaultException ex)
					{
						log.Error(ex);
						throw;
					}
					catch (Exception ex)
					{
						log.Error("Unexpected exception in GetFavoriteReports", ex);
						throw new FaultException("Unexpected exception.");
					}
				}
#endif
			}
			catch (Exception ex)
			{
				log.Error("GetFavoriteReports failed for user " + userId.ToInvariantString(), ex);
				throw;
			}
			finally
			{
				log.Debug("GetFavoriteReports for user " + userId.ToInvariantString() + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}

		public ClientTab[] GetCustomTabs(int userId, string culture)
		{
#if DEBUG
			System.Drawing.ImageConverter converter = new System.Drawing.ImageConverter();
			var result = new ClientTab[2]
			{
				new ClientTab()
				{
					LocalizedTitle = "Teszt",
					TabId = "teszt"
				},
				new ClientTab()
				{
					LocalizedTitle = "Teszt2",
					TabId = "teszt2"
				}
			};
			return result;
#else
			var sw = Stopwatch.StartNew();
			try
			{
				using (var clientWrapper = new WebsiteClientWrapper())
				using (var context = new JobControlDataClassesDataContext())
				{
					try
					{
						var reports = clientWrapper.Client.GetCustomTabsForUser(new Guid(context.GetAuthTicket(userId)), culture);
						return reports;
					}
					catch (FaultException ex)
					{
						log.Error(ex);
						throw;
					}
					catch (Exception ex)
					{
						log.Error("Unexpected exception in GetCustomTabs.", ex);
						throw new FaultException("Unexpected exception.");
					}
				}
			}
			finally
			{
				log.Debug("GetCustomTabss for user " + userId.ToInvariantString() + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
#endif
		}

		public DisplayedReports GetDisplayedReportForTabId(int userId, string culture, string tabId, DateTime? localToday = null)
		{
#if DEBUG
			//throw new FaultException("Error occurred in PcServerMyPerformance (custom) report");
			System.Drawing.ImageConverter converter = new System.Drawing.ImageConverter();
			var result = new DisplayedReports()
			{
				LayoutXml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><tab id=\"Gamification\"><row height=\"15%\"><img id=\"pic1\" /></row><row><table><trow><tcell border=\"0,1,1,0\"></tcell><tcell border=\"0,1,1,1\" fontstyle=\"bold\">Érték</tcell><tcell border=\"0,1,1,1\" fontstyle=\"bold\">Min</tcell><tcell border=\"0,1,1,1\" fontstyle=\"bold\">Target</tcell><tcell border=\"0,1,1,1\" fontstyle=\"bold\">Max</tcell></trow><trow><tcell border=\"1,1,1,0\" fontstyle=\"bold\">Munkaidő egyenleg (%)</tcell><tcell bgcolor=\"blue\">73%</tcell><tcell>30%</tcell><tcell>70%</tcell><tcell>90%</tcell></trow><trow><tcell border=\"1,1,1,0\" fontstyle=\"bold\">Produktivitás (%)</tcell><tcell bgcolor=\"blue\">82%</tcell><tcell>30%</tcell><tcell>70%</tcell><tcell>90%</tcell></trow><trow><tcell border=\"1,1,1,0\" fontstyle=\"bold\">My Custom Kpi I. (%)</tcell><tcell bgcolor=\"red\">22%</tcell><tcell>30%</tcell><tcell>70%</tcell><tcell>90%</tcell></trow></table></row><row height=\"50%\"><img id=\"pic2\" /></row></tab>",
				ReportImages = new DisplayedReportImage[2]
				{
						new DisplayedReportImage()
						{
							Id = "pic1",
							ReportImage = (byte[])converter.ConvertTo(Properties.Resources.barchart, typeof(byte[]))
						},
						new DisplayedReportImage()
						{
							Id = "pic2",
							ReportImage = (byte[])converter.ConvertTo(Properties.Resources.productivity, typeof(byte[]))
						}
				}
			};
			return result;

#else
			var sw = Stopwatch.StartNew();
			try
			{
				using (var clientWrapper = new WebsiteClientWrapper())
				using (var context = new JobControlDataClassesDataContext())
				{
					try
					{
						var reports = clientWrapper.Client.GetContentForUserTab(new Guid(context.GetAuthTicket(userId)), tabId, culture, localToday);
						return reports;
					}
					catch (FaultException ex)
					{
						log.Error(ex);
						throw;
					}
					catch (Exception ex)
					{
						log.Error("Unexpected exception in GetDisplayedReportForTabId", ex);
						throw new FaultException("Unexpected exception.");
					}
				}
			}
			finally
			{
				log.Debug("GetDisplayedReportForTabId for user " + userId.ToInvariantString() + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
#endif
		}
#endregion

#region AddManualWorkItem

		public void AddManualWorkItem(ManualWorkItem manualWorkItem)
		{
			log.Verbose($"AddManualWorkItem started uid: {manualWorkItem?.UserId}");
			if (manualWorkItem == null) return;
			int userId = manualWorkItem.UserId;
			var sw = Stopwatch.StartNew();
			var moveToDeadLetter = false;
			try
			{
				EnsureAccess(userId);
				int groupId;
				int companyId;
				if (!UserIdManager.Instance.TryGetIdsForUser(userId, out groupId, out companyId))
				{
					throw new FaultException("User is not active");
				}
				if (manualWorkItem.EndDate < manualWorkItem.StartDate)
				{
					moveToDeadLetter = true;
					throw new FaultException("Invalid interval for ManualWorkItem");
				}
				if (manualWorkItem.EndDate - manualWorkItem.StartDate > TimeSpan.FromHours(30)) //todo enforce in db?
				{
					moveToDeadLetter = true;
					throw new FaultException("ManualWorkItem is too long");
				}
				if (manualWorkItem.WorkId <= 0)
				{
					moveToDeadLetter = true;
					throw new FaultException("Invalid workid for ManualWorkItem");
				}
				if (DateTime.UtcNow - manualWorkItem.StartDate > maxWorkItemAge)
				{
					moveToDeadLetter = true;
					throw new FaultException("ManualWorkItem too old");
				}
				manualWorkItem.GroupId = groupId;
				manualWorkItem.CompanyId = companyId;
				manualWorkItem.CreatedBy = GetUserId(userId); //it's the same as userId atm. but it could change...
				manualWorkItem.SourceId = (byte)ManualWorkItemSourceEnum.Server;
				using (var context = new ManualDataClassesDataContext())
				{
					context.Connection.Open();
					context.Connection.SetXactAbortOn();
					using (context.Transaction = context.Connection.BeginTransaction(System.Data.IsolationLevel.Serializable))
					{
						var dupeQueryNoEndDate = context.ManualWorkItems.Where(
							  n => n.StartDate == manualWorkItem.StartDate
								&& n.ManualWorkItemTypeId == manualWorkItem.ManualWorkItemTypeId
								&& n.UserId == manualWorkItem.UserId);
						dupeQueryNoEndDate = (manualWorkItem.WorkId == null) ? dupeQueryNoEndDate.Where(n => n.WorkId == null) : dupeQueryNoEndDate.Where(n => n.WorkId == manualWorkItem.WorkId.Value);
						dupeQueryNoEndDate = (manualWorkItem.CreatedBy == null) ? dupeQueryNoEndDate.Where(n => n.CreatedBy == null) : dupeQueryNoEndDate.Where(n => n.CreatedBy == manualWorkItem.CreatedBy.Value);

						if (dupeQueryNoEndDate.Where(n => n.EndDate == manualWorkItem.EndDate).Any())
						{
							log.Debug("Duplicate manualWorkItem received " + manualWorkItem);
							return;
						}

						if (manualWorkItem.OriginalEndDate == null) //inserting new item
						{
							var existing = dupeQueryNoEndDate.FirstOrDefault();
							// check if this item already inserted before client crash
							if (existing != null) existing.EndDate = manualWorkItem.EndDate;
							else context.ManualWorkItems.InsertOnSubmit(manualWorkItem);
						}
						else //if (manualWorkItem.OriginalEndDate != null) //updating item (we can only update one time which is actually a cancel)
						{
							//we could use a new guid/int/short/byte (rnd) field (or comment) to identify the original data, but I think start-end-userid-type is enough for us atm. (although it's not watertight)
							// if (manualWorkItem.OriginalEndDate.Value < manualWorkItem.EndDate) throw new Exception("Cannot increase the length of a manual work item"); 
							var itemToUpdate = dupeQueryNoEndDate.Where(n => n.EndDate == manualWorkItem.OriginalEndDate.Value).FirstOrDefault(); //throw if not found (probably the original item was not received yet)
							if (itemToUpdate == null)
							{
								var existing = dupeQueryNoEndDate.FirstOrDefault();
								if (existing != null)
								{
									if (existing.EndDate < manualWorkItem.EndDate)
									{
										existing.EndDate = manualWorkItem.EndDate;
										log.Debug("Missing Original Item, but existing can be expanded with " + manualWorkItem);
									}
									else
									{
										log.Debug("Missing Original Item, but existing newer so received dropped " + manualWorkItem);
									}
								}
								else
								{
									context.ManualWorkItems.InsertOnSubmit(manualWorkItem);
									log.Debug("Missing Original item inserted " + manualWorkItem);
								}
							}
							else itemToUpdate.EndDate = manualWorkItem.EndDate;
						}
						context.SubmitChanges();
						context.Transaction.Commit();
					}
				}
			}
			catch (Exception ex)
			{
				if (moveToDeadLetter && DeadLetterHelper.TrySaveItem(manualWorkItem, ex))
				{
					log.Warn("AddManualWorkItem failed and moved to dead letter " + manualWorkItem.ToString(), ex);
					return;
				}
				if (manualWorkItem.StartDate < ConfigManager.IgnoreErrorsCutOff)
				{
					log.Fatal("AddManualWorkItem failed " + manualWorkItem.ToString() + " but ignoring errors before " + ConfigManager.IgnoreErrorsCutOff.ToInvariantString());
					return;
				}
				log.Error("AddManualWorkItem failed " + manualWorkItem.ToString(), ex);
				throw;
			}
			finally
			{
				log.Debug("Service call AddManualWorkItem " + manualWorkItem.ToString() + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}

		}
#endregion

#region AddParallelWorkItem

		public void AddParallelWorkItem(ParallelWorkItem parallelWorkItem)
		{
			log.Verbose($"AddParallelWorkItem started uid: {parallelWorkItem?.UserId}");
			if (parallelWorkItem == null) return;
			int userId = parallelWorkItem.UserId;
			var sw = Stopwatch.StartNew();
			var moveToDeadLetter = false;
			try
			{
				EnsureAccess(userId);
				int groupId;
				int companyId;
				if (!UserIdManager.Instance.TryGetIdsForUser(userId, out groupId, out companyId))
				{
					throw new FaultException("User is not active");
				}
				if (parallelWorkItem.EndDate < parallelWorkItem.StartDate)
				{
					moveToDeadLetter = true;
					throw new FaultException("Invalid interval for ParallelWorkItem");
				}
				if (parallelWorkItem.EndDate - parallelWorkItem.StartDate > TimeSpan.FromHours(23)) //todo enforce in db?
				{
					moveToDeadLetter = true;
					throw new FaultException("ParallelWorkItem is too long");
				}
				if (parallelWorkItem.WorkId <= 0)
				{
					moveToDeadLetter = true;
					throw new FaultException("Invalid workid for ParallelWorkItem");
				}
				if (DateTime.UtcNow - parallelWorkItem.StartDate > maxWorkItemAge)
				{
					moveToDeadLetter = true;
					throw new FaultException("ParallelWorkItem too old");
				}
				using (var context = new ManualDataClassesDataContext())
				{
					context.Connection.Open();
					context.Connection.SetXactAbortOn();
					using (context.Transaction = context.Connection.BeginTransaction(System.Data.IsolationLevel.Serializable))
					{
						//not a water-tight dupe check... //todo unique constraint ?
						var dupeQuery = context.ParallelWorkItems.Where(
							  n => n.UserId == parallelWorkItem.UserId
								&& n.StartDate == parallelWorkItem.StartDate
								&& n.ParallelWorkItemTypeId == parallelWorkItem.ParallelWorkItemTypeId
								&& n.EndDate == parallelWorkItem.EndDate
								);

						if (dupeQuery.Any())
						{
							log.Debug("Duplicate parallelWorkItem received " + parallelWorkItem.ToString());
							return;
						}

						context.ParallelWorkItems.InsertOnSubmit(parallelWorkItem);
						context.SubmitChanges();
						context.Transaction.Commit();
					}
				}
			}
			catch (Exception ex)
			{
				if (moveToDeadLetter && DeadLetterHelper.TrySaveItem(parallelWorkItem, ex))
				{
					log.Warn("AddParallelWorkItem failed and moved to dead letter " + parallelWorkItem.ToString(), ex);
					return;
				}
				if (parallelWorkItem.StartDate < ConfigManager.IgnoreErrorsCutOff)
				{
					log.Fatal("AddParallelWorkItem failed " + parallelWorkItem.ToString() + " but ignoring errors before " + ConfigManager.IgnoreErrorsCutOff.ToInvariantString());
					return;
				}
				log.Error("AddParallelWorkItem failed " + parallelWorkItem.ToString(), ex);
				throw;
			}
			finally
			{
				log.Debug("Service call AddParallelWorkItem " + parallelWorkItem.ToString() + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}
#endregion

#region StartWork

		public void StartWork(int userId, int workId, int computerId, DateTime createDate, DateTime sendDate)
		{
			log.Verbose($"StartWork started uid: {userId}");
			var serverTime = DateTime.UtcNow;
			var sw = Stopwatch.StartNew();
			try
			{
				EnsureAccess(userId);
				onlineStatsManager.StartComputerWork(userId, workId, computerId, createDate, sendDate, serverTime);
			}
			catch (Exception ex)
			{
				log.Error("StartWork failed for user " + userId.ToInvariantString() + " (workId:" + workId.ToInvariantString() + ")", ex);
				throw;
			}
			finally
			{
				log.Info("StartWork for user " + userId.ToInvariantString() + " (workId:" + workId.ToInvariantString() + ") finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}
#endregion

#region StopWork

		public void StopWork(int userId, int computerId, DateTime createDate, DateTime sendDate)
		{
			log.Verbose($"StopWork started uid: {userId}");
			var serverTime = DateTime.UtcNow;
			var sw = Stopwatch.StartNew();
			try
			{
				EnsureAccess(userId);
				onlineStatsManager.StopComputerWork(userId, computerId, createDate, sendDate, serverTime);
			}
			catch (Exception ex)
			{
				log.Error("StopWork failed for user " + userId.ToInvariantString(), ex);
				throw;
			}
			finally
			{
				log.Info("StopWork for user " + userId.ToInvariantString() + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}
#endregion

#region ReportClientVersion

		public void ReportClientVersion(int userId, int computerId, int major, int minor, int build, int revision, string application)
		{
			log.Verbose($"ReportClientVersion started uid: {userId}");
			var sw = Stopwatch.StartNew();
			if (application == null) application = "JobCTRL"; //backward compatibility
			var verStr = application + " v" + major.ToInvariantString() + "." + minor.ToInvariantString() + "." + build.ToInvariantString() + "." + revision.ToInvariantString();
			try
			{
				EnsureAccess(userId);
				using (var context = new ClientComputerInfoDataClassesDataContext())
				{
					context.ReportClientComputerVersion(userId, computerId, major, minor, build, revision, application);
				}
			}
			catch (Exception ex)
			{
				log.Error("ReportClientVersion failed for user " + userId.ToInvariantString() + " (comp:" + computerId.ToInvariantString() + ") " + verStr, ex);
				throw;
			}
			finally
			{
				log.Info("Service call ReportClientVersion for user " + userId.ToInvariantString() + " (comp:" + computerId.ToInvariantString() + ") " + verStr + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}
#endregion

#region ReportClientEnvInfo

		public void ReportClientComputerInfo(ClientComputerInfo info)
		{
			log.Verbose($"ReportClientComputerInfo started uid: {info.UserId}");
			var sw = Stopwatch.StartNew();
			try
			{
				EnsureAccess(info.UserId);
				using (var context = new ClientComputerInfoDataClassesDataContext())
				{
					context.SetXactAbortOn();
					var prevInfo = context.ClientComputerInfos.SingleOrDefault(x => x.UserId == info.UserId && x.ComputerId == info.ComputerId);
					if (prevInfo == null)
					{
						info.CreateDate = DateTime.UtcNow;
						context.ClientComputerInfos.InsertOnSubmit(info);
					}
					else
					{
						prevInfo.OSMajor = info.OSMajor;
						prevInfo.OSMinor = info.OSMinor;
						prevInfo.OSBuild = info.OSBuild;
						prevInfo.OSRevision = info.OSRevision;
						prevInfo.IsNet4Available = info.IsNet4Available;
						prevInfo.IsNet45Available = info.IsNet45Available;
						prevInfo.HighestNetVersionAvailable = info.HighestNetVersionAvailable;
						prevInfo.CreateDate = DateTime.UtcNow;
						prevInfo.MachineName = info.MachineName;
						prevInfo.LocalUserName = info.LocalUserName;
					}

					context.SubmitChanges();
				}
			}
			catch (Exception ex)
			{
				log.Error("ReportClientEnvInfo failed for user " + info.UserId.ToInvariantString() + " (comp:" + info.ComputerId.ToInvariantString() + ") " + info.ToString(), ex);
				throw;
			}
			finally
			{
				log.Info("Service call ReportClientEnvInfo for user " + info.UserId.ToInvariantString() + " (comp:" + info.UserId.ToInvariantString() + ") " + info.ToString() + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}
#endregion

#region ReportClientError

		private static readonly StreamDataUploadStore clientLogStore = new StreamDataUploadStore();

		public void ReportClientError(ClientComputerError clientError)
		{
			int userId = clientError.UserId;
			log.Verbose($"ReportClientError started uid: {userId}");
			var sw = Stopwatch.StartNew();
			try
			{
				EnsureAccess(userId);
				int groupId;
				int companyId;
				if (!UserIdManager.Instance.TryGetIdsForUser(userId, out groupId, out companyId))
				{
					throw new FaultException("User is not active");
				}
				clientError.GroupId = groupId;
				clientError.CompanyId = companyId;

				using (var context = new ClientComputerInfoDataClassesDataContext())
				{
					context.Connection.Open();
					using (context.Transaction = context.Connection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
					{
						try
						{
							var upd = context.UpsertClientComputerError(clientError);
							if (upd == 0)
							{
								log.Debug("Duplicate UpsertClientComputerError (update) received uid: " + clientError.UserId.ToInvariantString() + " compId: " + clientError.ComputerId.ToInvariantString() + " cid: " + clientError.ClientId.ToString() + " l:" + clientError.Length.ToInvariantString() + " o:" + clientError.Offset.ToInvariantString());
								return; //data was commited once so file should not be updated on the disk
							}
							if (clientError.HasAttachment)
							{
								if (!clientError.IsCancelled)
								{
									if (clientError.Offset == 0) clientLogStore.Open(clientError);
									clientLogStore.AddData(clientError);
									if (clientError.IsCompleted) clientLogStore.Close(clientError);
								}
								else
								{
									clientLogStore.Delete(clientError);
								}
							}
							context.Transaction.Commit();
						}
						catch (SqlException sqlex)
						{
							if (sqlex != null && sqlex.Message != null && sqlex.Message.Contains("IX_ClientComputerErrors_ClientId"))
							{
								log.Debug("Duplicate UpsertClientComputerError (insert) received uid: " + clientError.UserId.ToInvariantString() + " compId: " + clientError.ComputerId.ToInvariantString() + " cid: " + clientError.ClientId.ToString() + " l:" + clientError.Length.ToInvariantString() + " o:" + clientError.Offset.ToInvariantString());
								return; //data was commited once so file should not be updated on the disk
							}
							throw;
						}
					}

					if (clientError.IsCompleted && !clientError.IsCancelled)
					{
						if (clientError.Description == null)	//Refresh description from db if needed. (Only the first chunk contains description.)
						{
							try
							{
								var dbClientError = context.ClientComputerErrors.Single(x => x.ClientId == clientError.ClientId && x.UserId == clientError.UserId);
								clientError.Description = dbClientError.Description;
							}
							catch (Exception e)
							{
								log.Error("Refreshing description from DB (before sending email notification) failed.", e);
							}
						}
						AcquireClientLogHelper.DeleteAcquireClientRequest(clientError.UserId);
						SendEmailAboutClientErrorReport(clientError);
					}
				}
			}
			catch (Exception ex)
			{
				//if (clientError.StartDate < ConfigManager.IgnoreErrorsCutOff)
				//{
				//	log.Fatal("UpsertVoiceRecording failed uid: " + clientError.UserId + " compId: " + clientError.ComputerId + " cid: " + clientError.ClientId + " l:" + clientError.Length + " o:" + clientError.Offset + " but ignoring errors before " + ConfigManager.IgnoreErrorsCutOff);
				//	return;
				//}
				log.Error("UpsertClientComputerError failed userId: " + userId.ToInvariantString() + " compId: " + clientError.ComputerId.ToInvariantString() + " cid: " + clientError.ClientId.ToString() + " l:" + clientError.Length.ToInvariantString() + " o:" + clientError.Offset.ToInvariantString(), ex);
				throw;
			}
			finally
			{
				log.Debug("Service call UpsertClientComputerError userId: " + userId.ToInvariantString() + " compId: " + clientError.ComputerId.ToInvariantString() + " cid: " + clientError.ClientId.ToString() + " l:" + clientError.Length.ToInvariantString() + " o:" + clientError.Offset.ToInvariantString() + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}

		private void SendEmailAboutClientErrorReport(ClientComputerError clientError)
		{
			try
			{
				var eb = new EmailBuilder();
				using (var context = new JobControlDataClassesDataContext())
				{
					var userInfo = context.GetUserStatInfoById(clientError.UserId);
					if (userInfo != null)
					{
						eb.AppendLine("User: " + userInfo.Name + " <" + userInfo.Email + ">");
					}
				}
				eb.AppendLine("UserId: " + clientError.UserId);
				eb.AppendLine("CompanyId: " + clientError.CompanyId);
				eb.AppendLine("ComputerId: " + clientError.ComputerId);
				eb.AppendLine("Client version: " + clientError.VersionString);
				eb.AppendLine("Enabled features: " + clientError.Features).AppendLine();

				eb.AppendLine("Error description:");
				eb.BodyHtml.Append("<PRE>");
				eb.AppendLine(clientError.Description);
				eb.BodyHtml.Append("</PRE>");
				eb.AppendLine();

				if (!clientError.HasAttachment)
				{
					eb.AppendLine("Log files have not been attached.");
				}
				else
				{
					eb.Append("Log files have been attached: ");
					eb.AppendLink(clientError.GetUrl(), Path.GetFileName(clientError.GetPath()));
					eb.AppendLine();
				}

				EmailManager.Instance.Send(new EmailMessage()
				{
					To = ConfigManager.ClientLogsEmailTo,
					Subject = "[JobCTRL] - Client Error Report " + clientError.VersionString + " - UserId: " + clientError.UserId,
					PlainBody = eb.GetPlainText(),
					HtmlBody = eb.GetHtmlText(),
				});
			}
			catch (Exception ex)
			{
				log.Error("Unexpected error in SendEmailAboutClientErrorReport", ex);
			}
		}

#endregion

#region GetPendingKick

		ClientComputerKick IActivityRecorder.GetPendingKick(int userId, int computerId)
		{
			return GetPendingKick(userId, computerId);
		}

		public ClientComputerKick GetPendingKick(int userId, long deviceId)
		{
			log.Verbose($"GetPendingKick started uid: {userId}");
			var sw = Stopwatch.StartNew();
			try
			{
				EnsureAccess(userId);
				return kickManager.GetPendingKick(userId, deviceId);
			}
			catch (Exception ex)
			{
				log.Error("GetPendingKick failed for user " + userId.ToInvariantString() + " (dev:" + deviceId.ToInvariantString() + ")", ex);
				throw;
			}
			finally
			{
				log.Info("Service call GetPendingKick for user " + userId.ToInvariantString() + " (dev:" + deviceId.ToInvariantString() + ") finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}
#endregion

#region ConfirmKick

		void IActivityRecorder.ConfirmKick(int userId, int computerId, int kickId, KickResult result)
		{
			ConfirmKick(userId, computerId, kickId, result);
		}

		public void ConfirmKick(int userId, long deviceId, int kickId, KickResult result)
		{
			log.Verbose($"ConfirmKick started uid: {userId}");
			var sw = Stopwatch.StartNew();
			try
			{
				EnsureAccess(userId);
				kickManager.ConfirmKick(userId, deviceId, kickId, result);
			}
			catch (Exception ex)
			{
				log.Error("ConfirmKick failed for user " + userId.ToInvariantString() + " (dev:" + deviceId.ToInvariantString() + ") kickId:" + kickId.ToInvariantString() + " result:" + result.ToString(), ex);
				throw;
			}
			finally
			{
				log.Info("Service call ConfirmKick for user " + userId.ToInvariantString() + " (dev:" + deviceId.ToInvariantString() + ") kickId:" + kickId.ToInvariantString() + " result:" + result.ToString() + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}
#endregion

#region MakeClientActive

		void IActivityRecorder.MakeClientActive(int userId, int computerId, bool isActive)
		{
			MakeClientActive(userId, computerId, isActive);
		}

		public void MakeClientActive(int userId, long deviceId, bool isActive)
		{
			log.Verbose($"MakeClientActive started uid: {userId}");
			TimeSpan expiration = TimeSpan.FromSeconds(ConfigManager.ClientKickTimeoutInSec);
			var sw = Stopwatch.StartNew();
			try
			{
				EnsureAccess(userId);
				kickManager.MakeClientActive(userId, deviceId, isActive, expiration);
			}
			catch (Exception ex)
			{
				log.Error("MakeClientActive failed for user " + userId.ToInvariantString() + " (dev:" + deviceId.ToInvariantString() + ")", ex);
				throw;
			}
			finally
			{
				log.Info("Service call MakeClientActive for user " + userId.ToInvariantString() + " (dev:" + deviceId.ToInvariantString() + ")  finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}

#endregion

#region KickUserComputer

		public KickResult KickUserComputer(int userId, int computerId, string reason, TimeSpan expiration)
		{
			log.Verbose($"KickUserComputer started uid: {userId}");
			var sw = Stopwatch.StartNew();
			KickResult? result = null;
			try
			{
				int userIdCreatedBy = GetUserIdAllowWebSite();
				var monUserIds = GetFilteredMonitorableUserIdsForUser(userIdCreatedBy, new List<int> { userId }).ToList();
				if (monUserIds.Count == 0)
				{
					log.Error("User: " + userIdCreatedBy.ToInvariantString() + " has no access for userId: " + userId.ToInvariantString());
					throw new FaultException("Access denied");
				}
				result = kickManager.KickUserComputer(userId, computerId, reason, expiration, userIdCreatedBy);
			}
			catch (Exception ex)
			{
				log.Error("KickUserComputer failed for user " + userId.ToInvariantString() + " (comp:" + computerId.ToInvariantString() + ") " + reason, ex);
				throw;
			}
			finally
			{
				log.Info("Service call KickUserComputer for user " + userId.ToInvariantString() + " (comp:" + computerId.ToInvariantString() + ") '" + reason + "' Result:'" + result.ToString() + "' finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
			if (result == null)
			{
				throw new FaultException<KickTimeoutException>(new KickTimeoutException());
			}
			return result.Value;
		}
#endregion

#region AssignWorkByKey

		public async Task<bool> AssignWorkByKeyAsync(int userId, AssignWorkData assignWorkData)
		{
			log.Verbose($"AssignWorkByKeyAsync started uid: {userId}");
			bool? result = null;
			var sw = Stopwatch.StartNew();
			try
			{
				EnsureAccess(userId);
#if DEBUG
				if (AssignWorkByKeyDebug(userId, assignWorkData.WorkKey, assignWorkData.WorkName, assignWorkData.Description))
				{
					menuVersionCacheManager.Remove(userId);
					return true;
				}
#endif
				string ticket;
				using (var context = new JobControlDataClassesDataContext())
				{
					ticket = context.GetAuthTicket(userId);
				}
				using (var client = new Website.WebsiteClientWrapper())
				{
					var t = client.Client.CreateTaskDynamicallyForExternalKeyAsync(new Guid(ticket), assignWorkData.ServerRuleId, assignWorkData.ProjectId, assignWorkData.WorkKey, assignWorkData.WorkName.ReplaceInvalidXmlChars(" "), assignWorkData.Description.ReplaceInvalidXmlChars(" ")/*, assignWorkData.WorkName*/);
					await t;
					result = t.Result.CreateTaskDynamicallyForExternalKeyResult;
					if (result.Value)
					{
						menuVersionCacheManager.Remove(userId);
					}
					return result.Value;
				}
			}
			catch (Exception ex)
			{
				log.Error("AssignWorkByKey failed for user " + userId.ToInvariantString() + " key: " + assignWorkData, ex);
				if (assignWorkData.WorkKey.HasInvalidXmlChars())
				{
					log.Error("AssignWorkByKey failed with invalid xml for user " + userId.ToInvariantString() + " key: " + assignWorkData);
					return false; //we can never send this data to the web so reject it
				}
				throw;
			}
			finally
			{
				log.Info("AssignWorkByKey for user " + userId.ToInvariantString() + " key: " + assignWorkData + " result: " + result.ToString() + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}

#if DEBUG
		private bool AssignWorkByKeyDebug(int userId, string workKey, string workName, string workDescription)
		{
			string newVersion;
			var menu = GetClientMenu(userId, "", out newVersion);
			var dyn = menu.Works.Where(n => n.Name == "Dinamikus munkak").FirstOrDefault();
			if (dyn == null)
			{
				dyn = new WorkData("Dinamikus munkak", null, null);
				menu.Works.Add(dyn);
			}
			if (dyn.Children == null) dyn.Children = new List<WorkData>();
			if (menu.ExternalWorkIdMapping == null) menu.ExternalWorkIdMapping = new Dictionary<string, int>();
			var nextId = dyn.Children.Where(n => n.Id.HasValue).Select(n => n.Id.Value).DefaultIfEmpty(10000).Max() + 1;
			dyn.Children.Add(new WorkData((workName ?? "Dyn") + ": " + workKey, nextId, null) { Description = workDescription });
			menu.ExternalWorkIdMapping[workKey] = nextId;
			SetClientMenu(userId, menu);
			return true;
		}
#endif

#endregion

#region AssignProjectByKey

		public bool AssignProjectByKey(int userId, AssignProjectData assignProjectData)
		{
			log.Verbose($"AssignProjectByKey started uid: {userId}");
			bool? result = null;
			var sw = Stopwatch.StartNew();
			try
			{
				EnsureAccess(userId);
#if DEBUG
				if (AssignProjectByKeyDebug(userId, assignProjectData.ProjectKey, assignProjectData.ProjectName, assignProjectData.Description))
				{
					menuVersionCacheManager.Remove(userId);
					return true;
				}
#endif
				string ticket;
				using (var context = new JobControlDataClassesDataContext())
				{
					ticket = context.GetAuthTicket(userId);
				}
				using (var client = new Website.WebsiteClientWrapper())
				{
					result = client.Client.CreateTaskDynamicallyForExternalKey(new Guid(ticket), assignProjectData.ServerRuleId, null, assignProjectData.ProjectKey, assignProjectData.ProjectName.ReplaceInvalidXmlChars(" "), assignProjectData.Description.ReplaceInvalidXmlChars(" ")/*, assignProjectData.ProjectName*/);
					if (result.Value)
					{
						menuVersionCacheManager.Remove(userId);
					}
					return result.Value;
				}
			}
			catch (Exception ex)
			{
				log.Error("AssignProjectByKey failed for user " + userId.ToInvariantString() + " key: " + assignProjectData, ex);
				if (assignProjectData.ProjectKey.HasInvalidXmlChars())
				{
					log.Error("AssignProjectByKey failed with invalid xml for user " + userId.ToInvariantString() + " key: " + assignProjectData);
					return false; //we can never send this data to the web so reject it
				}
				throw;
			}
			finally
			{
				log.Info("AssignProjectByKey for user " + userId.ToInvariantString() + " key: " + assignProjectData + " result: " + result.ToString() + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}

#if DEBUG
		private bool AssignProjectByKeyDebug(int userId, string projectKey, string projectName, string projectDescription)
		{
			string newVersion;
			var menu = GetClientMenu(userId, "", out newVersion);
			var dyn = menu.Works.Where(n => n.Name == "Dinamikus projektek").FirstOrDefault();
			if (dyn == null)
			{
				dyn = new WorkData("Dinamikus projektek", null, null);
				menu.Works.Add(dyn);
			}
			if (dyn.Children == null) dyn.Children = new List<WorkData>();
			if (menu.ExternalProjectIdMapping == null) menu.ExternalProjectIdMapping = new Dictionary<string, int>();
			var nextId = dyn.Children.Where(n => n.ProjectId.HasValue).Select(n => n.ProjectId.Value).DefaultIfEmpty(12000).Max() + 1;
			var proj = new WorkData((projectName ?? "DynProj") + ": " + projectKey, null, null) { ProjectId = nextId + 2, Description = projectDescription };
			proj.Children = new List<WorkData>();
			proj.Children.Add(new WorkData("Work1", nextId, null));
			proj.Children.Add(new WorkData("Work2", nextId + 1, null));
			proj.Children.Add(new WorkData("Work3", nextId + 2, null));
			dyn.Children.Add(proj);
			menu.ExternalProjectIdMapping[projectKey] = proj.ProjectId.Value;
			SetClientMenu(userId, menu);
			return true;
		}
#endif

#endregion

#region AssignProjectAndWorkByKey

		public async Task<bool> AssignProjectAndWorkByKeyAsync(int userId, AssignCompositeData assignCompositeData)
		{
			log.Verbose($"AssignProjectAndWorkByKeyAsync started uid: {userId}");
			bool? result = null;
			var sw = Stopwatch.StartNew();
			try
			{
				EnsureAccess(userId);
#if DEBUG
				if (AssignProjectAndWorkByKeyDebug(userId, assignCompositeData.WorkKey, assignCompositeData.ProjectKeys, assignCompositeData.Description))
				{
					menuVersionCacheManager.Remove(userId);
					return true;
				}
#endif
				string ticket;
				using (var context = new JobControlDataClassesDataContext())
				{
					ticket = context.GetAuthTicket(userId);
				}
				using (var client = new Website.WebsiteClientWrapper())
				{
					result = await client.Client.CreateTaskDynamicallyUnderPathAsync(new Guid(ticket), assignCompositeData.ServerRuleId, assignCompositeData.WorkKey, assignCompositeData.WorkName.ReplaceInvalidXmlChars(" "), assignCompositeData.ProjectKeys, assignCompositeData.Description.ReplaceInvalidXmlChars(" "));
					if (result.Value)
					{
						menuVersionCacheManager.Remove(userId);
					}
					return result.Value;
				}
			}
			catch (Exception ex)
			{
				log.Error("AssignProjectAndWorkByKey failed for user " + userId.ToInvariantString() + " key: " + assignCompositeData, ex);
				if (assignCompositeData.WorkKey.HasInvalidXmlChars() || HasInvalidXmlChars(assignCompositeData.ProjectKeys))
				{
					log.Error("AssignProjectAndWorkByKey failed with invalid xml for user " + userId.ToInvariantString() + " key: " + assignCompositeData);
					return false; //we can never send this data to the web so reject it
				}
				throw;
			}
			finally
			{
				log.Info("AssignProjectAndWorkByKey for user " + userId.ToInvariantString() + " key: " + assignCompositeData + " result: " + result.ToString() + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}

		private static bool HasInvalidXmlChars(string[] projectKeys) //no linq to avoid allocations
		{
			if (projectKeys == null || projectKeys.Length == 0) return false;
			foreach (var projectKey in projectKeys)
			{
				if (projectKey.HasInvalidXmlChars()) return true;
			}
			return false;
		}

#if DEBUG
		private bool AssignProjectAndWorkByKeyDebug(int userId, string workKey, string[] projectKeys, string description)
		{
			string newVersion;
			var menu = GetClientMenu(userId, "", out newVersion);
			var dyn = menu.Works.Where(n => n.Name == "Dinamikus proj+munkak").FirstOrDefault();
			if (dyn == null)
			{
				dyn = new WorkData("Dinamikus proj+munkak", null, null);
				menu.Works.Add(dyn);
			}
			if (dyn.Children == null) dyn.Children = new List<WorkData>();
			if (menu.ExternalCompositeMapping == null) menu.ExternalCompositeMapping = new CompositeMapping();
			var nextId = dyn.ProjectId.GetValueOrDefault(14000) + 1;

			var curr = dyn;
			var currMap = menu.ExternalCompositeMapping;
			foreach (var projectKey in projectKeys)
			{
				if (projectKey == null) break;
				if (curr.Children == null) curr.Children = new List<WorkData>();
				var child = curr.Children.Where(n => n.Name == projectKey).FirstOrDefault();
				if (child == null)
				{
					child = new WorkData(projectKey, null, null);
					curr.Children.Add(child);
				}

				CompositeMapping childMap;
				if (currMap.ChildrenByKey == null) currMap.ChildrenByKey = new Dictionary<string, CompositeMapping>();
				if (!currMap.ChildrenByKey.TryGetValue(projectKey, out childMap))
				{
					childMap = new CompositeMapping();
					currMap.ChildrenByKey[projectKey] = childMap;
				}

				curr = child;
				currMap = childMap;
			}


			if (curr.Children == null) curr.Children = new List<WorkData>();
			var dst = curr.Children.Where(n => n.Name == workKey).FirstOrDefault();
			if (dst == null)
			{
				dst = new WorkData(workKey, nextId, null) { Description = description };
				curr.Children.Add(dst);
				dyn.ProjectId = nextId;
			}

			if (currMap.WorkIdByKey == null) currMap.WorkIdByKey = new Dictionary<string, int>();
			currMap.WorkIdByKey[workKey] = nextId;

			SetClientMenu(userId, menu);
			return true;
		}
#endif

#endregion

#region GetServerTime

		public DateTime GetServerTime(int userId, int computerId, DateTime clientTime)
		{
			log.Verbose($"GetServerTime started uid: {userId}");
			var sw = Stopwatch.StartNew();
			var result = DateTime.UtcNow;
			var clientTimeStr = clientTime.TimeOfDay.ToHourMinuteSecondString();
			var diffStr = (clientTime - result).ToHourMinuteSecondString();
			try
			{
				EnsureAccess(userId);
				return result;
			}
			catch (Exception ex)
			{
				log.Error("GetServerTime failed for user " + userId.ToInvariantString() + " (comp:" + computerId.ToInvariantString() + ") client:" + clientTimeStr + " (" + diffStr + ") result:" + result.ToInvariantString(), ex);
				throw;
			}
			finally
			{
				log.Info("Service call GetServerTime for user " + userId.ToInvariantString() + " (comp:" + computerId.ToInvariantString() + ") client:" + clientTimeStr + " (" + diffStr + ") result:" + result.ToInvariantString() + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}
#endregion

#region GetLearningRuleGenerators

		public List<RuleGeneratorData> GetLearningRuleGenerators(int userId, string oldVersion, out string newVersion)
		{
			log.Verbose($"GetLearningRuleGenerators started uid: {userId}");
			var sw = Stopwatch.StartNew();
			try
			{
				EnsureAccess(userId);
				using (var context = new ActivityRecorderDataClassesDataContext())
				{
					string version = null;
					var result = context.GetLearningRuleGenerators(userId, oldVersion, ref version).SingleOrDefault();

					newVersion = version;
					if (result == null //no result
						|| result.LearningRuleGenerators == null //empty result
						|| oldVersion == newVersion) //don't send the same data if the version is not changed
					{
						return null;
					}
					else
					{
						using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(result.LearningRuleGenerators)))
						{
							List<RuleGeneratorData> rules;
							XmlPersistenceHelper.ReadFromStream(stream, out rules);
							return rules;
						}
					}
				}
			}
			catch (Exception ex)
			{
				log.Error("GetLearningRuleGenerators failed userId: " + userId.ToInvariantString(), ex);
				throw;
			}
			finally
			{
				log.Debug("Service call GetLearningRuleGenerators userId: " + userId.ToInvariantString() + ", oldVersion: " + oldVersion + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}
#endregion

#region GetCannedCloseReasons

		public CannedCloseReasons GetCannedCloseReasons(int userId)
		{
			log.Verbose($"GetCannedCloseReasons started uid: {userId}");
			var sw = Stopwatch.StartNew();
			try
			{
				EnsureAccess(userId);
				using (var context = new JobControlDataClassesDataContext())
				{
					return context.GetCannedCloseReasons(userId);
				}
			}
			catch (Exception ex)
			{
				log.Error("GetCannedCloseReasons failed userId: " + userId.ToInvariantString(), ex);
				throw;
			}
			finally
			{
				log.Debug("Service call GetCannedCloseReasons userId: " + userId.ToInvariantString() + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}
#endregion

#region GetProjectManagementConstraints

		public async Task<ProjectManagementConstraints> GetProjectManagementConstraintsAsync(int userId, int projectId)
		{
			log.Verbose($"GetProjectManagementConstraintsAsync started uid: {userId}");
			var sw = Stopwatch.StartNew();
			try
			{
				EnsureAccess(userId);
				return await workManagementService.GetProjectManagementConstraintsAsync(userId, projectId);
			}
			catch (Exception ex)
			{
				log.Error("GetProjectManagementConstraints failed userId: " + userId.ToInvariantString() + " projectId: " + projectId.ToInvariantString(), ex);
				throw;
			}
			finally
			{
				log.Debug("Service call GetProjectManagementConstraints userId: " + userId.ToInvariantString() + " projectId: " + projectId.ToInvariantString() + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}
#endregion

#region CreateWork

		public async Task<int> CreateWorkAsync(int userId, int projectId, WorkData workData)
		{
			log.Verbose($"CreateWorkAsync started uid: {userId}");
			var sw = Stopwatch.StartNew();
			int? result = null;
			try
			{
				EnsureAccess(userId);
				result = await workManagementService.CreateWorkAsync(userId, projectId, workData);
				menuVersionCacheManager.Remove(userId);
				return result.Value;
			}
			catch (Exception ex)
			{
				log.Error("CreateWork failed userId: " + userId.ToInvariantString() + " projectId: " + projectId.ToInvariantString() + " workName: " + workData.Name, ex);
				throw;
			}
			finally
			{
				log.Debug("Service call CreateWork userId: " + userId.ToInvariantString() + " projectId: " + projectId.ToInvariantString() + " workName: " + workData.Name + " result: " + result.ToInvariantString() + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}
#endregion

#region UpdateWork

		public async Task UpdateWorkAsync(int userId, WorkData workData)
		{
			log.Verbose($"UpdateWorkAsync started uid: {userId}");
			var sw = Stopwatch.StartNew();
			try
			{
				EnsureAccess(userId);
				await workManagementService.UpdateWorkAsync(userId, workData);
				menuVersionCacheManager.Remove(userId);
			}
			catch (Exception ex)
			{
				log.Error("UpdateWork failed userId: " + userId.ToInvariantString() + " workId: " + workData.Id.ToInvariantString() + " projectId: " + workData.ProjectId.ToInvariantString() + " workName: " + workData.Name, ex);
				throw;
			}
			finally
			{
				log.Debug("Service call UpdateWork userId: " + userId.ToInvariantString() + " workId: " + workData.Id.ToInvariantString() + " projectId: " + workData.ProjectId.ToInvariantString() + " workName: " + workData.Name + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}
#endregion

#region CloseWork

		public CloseWorkResult CloseWork(int userId, int workId, string reason, int? reasonItemId)
		{
			log.Verbose($"CloseWork started uid: {userId}");
			var sw = Stopwatch.StartNew();
			CloseWorkResult? result = null;
			try
			{
				EnsureAccess(userId);
				result = workManagementService.CloseWork(userId, workId, reason, reasonItemId);
				if (result.Value == CloseWorkResult.Ok
					|| result.Value == CloseWorkResult.AlreadyClosed)
				{
					menuVersionCacheManager.Remove(userId);
				}
				return result.Value;
			}
			catch (Exception ex)
			{
				log.Error("CloseWork failed userId: " + userId.ToInvariantString() + " workId: " + workId.ToInvariantString() + " reason: " + reason + " reasonItemId: " + reasonItemId.ToInvariantString(), ex);
				throw;
			}
			finally
			{
				log.Debug("Service call CloseWork userId: " + userId.ToInvariantString() + " workId: " + workId.ToInvariantString() + " reason: " + reason + " reasonItemId: " + reasonItemId.ToInvariantString() + " result: " + result.ToString() + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}
#endregion

#region AddReason

		public int AddReason(int userId, int workId, string reason, int? reasonItemId)
		{
			log.Verbose($"AddReason started uid: {userId}");
			var sw = Stopwatch.StartNew();
			int? result = null;
			try
			{
				EnsureAccess(userId);
				using (var context = new JobControlDataClassesDataContext())
				{
					return (result = context.AddReason(userId, workId, reason, reasonItemId)).Value;
				}
			}
			catch (Exception ex)
			{
				log.Error("AddReason failed userId: " + userId.ToInvariantString() + " workId: " + workId.ToInvariantString() + " reason: " + reason + " reasonItemId: " + reasonItemId.ToInvariantString(), ex);
				throw;
			}
			finally
			{
				log.Debug("Service call AddReason userId: " + userId.ToInvariantString() + " workId: " + workId.ToInvariantString() + " reason: " + reason + " reasonItemId: " + reasonItemId.ToInvariantString() + " result: " + result.ToInvariantString() + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}

		public int AddReasonEx(WorkManagement.ReasonItem reasonItem)
		{
			if (reasonItem == null) return 0;
			var sw = Stopwatch.StartNew();
			int? result = null;
			try
			{
				EnsureAccess(reasonItem.UserId);
				using (var context = new JobControlDataClassesDataContext())
				{
					return (result = context.AddReason(reasonItem.UserId, reasonItem.WorkId, reasonItem.Reason, reasonItem.ReasonItemId, reasonItem.StartDate)).Value;
				}
			}
			catch (Exception ex)
			{
				log.Error("AddReason failed userId: " + reasonItem.UserId.ToInvariantString() + " workId: " + reasonItem.WorkId.ToInvariantString() + " reason: " + reasonItem.Reason + " reasonItemId: " + reasonItem.ReasonItemId.ToInvariantString(), ex);
				throw;
			}
			finally
			{
				log.Debug("Service call AddReason userId: " + reasonItem.UserId.ToInvariantString() + " workId: " + reasonItem.WorkId.ToInvariantString() + " reason: " + reasonItem.Reason + " reasonItemId: " + reasonItem.ReasonItemId.ToInvariantString() + " result: " + result.ToInvariantString() + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}
#endregion

#region GetReasonStats

		public ReasonStats GetReasonStats(int userId)
		{
			log.Verbose($"GetReasonStats started uid: {userId}");
			var sw = Stopwatch.StartNew();
			try
			{
				EnsureAccess(userId);
				using (var context = new JobControlDataClassesDataContext())
				{
					return context.GetReasonStats(userId);
				}
			}
			catch (Exception ex)
			{
				log.Error("GetReasonStats failed userId: " + userId.ToInvariantString(), ex);
				throw;
			}
			finally
			{
				log.Debug("Service call GetReasonStats userId: " + userId.ToInvariantString() + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}
#endregion

#region GetMeetingData

		public MeetingData GetMeetingData(int userId)
		{
			log.Verbose($"GetMeetingData started uid: {userId}");
			var sw = Stopwatch.StartNew();
			try
			{
				EnsureAccess(userId);
				using (var context = new JobControlDataClassesDataContext())
				{
					return context.GetMeetingData(userId);
				}
			}
			catch (Exception ex)
			{
				log.Error("GetMeetingData failed userId: " + userId.ToInvariantString(), ex);
				throw;
			}
			finally
			{
				log.Debug("Service call GetMeetingData userId: " + userId.ToInvariantString() + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}
#endregion

#region ManageMeetings

#if DEBUG
		private static ConcurrentDictionary<int, DateTime> lastSyncDateTimesDebug = new ConcurrentDictionary<int, DateTime>();
		private static ConcurrentDictionary<int, Dictionary<string, FinishedMeetingEntry>> syncedMeetingsDebug = new ConcurrentDictionary<int, Dictionary<string, FinishedMeetingEntry>>();
#endif

		public async Task<MeetingData> ManageMeetingsAsync(int userId, int computerId, FinishedMeetingData finishedMeetingData)
		{
			log.Verbose($"ManageMeetingsAsync started uid: {userId}");
			var sw = Stopwatch.StartNew();
			ManageMeetingsResponse res = null;
			MeetingData result = null;
			List<FinishedMeetingEntryDead> deads = null;
			
			try
			{
				EnsureAccess(userId);
				if (finishedMeetingData == null) finishedMeetingData = new FinishedMeetingData() { FinishedMeetings = new List<FinishedMeetingEntry>(), LastQueryIntervalEndDate = null };
				if (finishedMeetingData.FinishedMeetings != null)
				{
					for (int i = 0; i < finishedMeetingData.FinishedMeetings.Count; i++)
					{
						var curr = finishedMeetingData.FinishedMeetings[i];
						if (curr.Status != MeetingCrudStatus.Deleted && DateTime.UtcNow - curr.StartTime > maxWorkItemAge)
						{
							if (deads == null) deads = new List<FinishedMeetingEntryDead>();
							var dead = new FinishedMeetingEntryDead() { UserId = userId, ComputerId = computerId, FinishedMeetingEntry = curr };
							deads.Add(dead);
							log.Warn("FinishedMeetingEntry too old so moving to dead letter " + dead);
							finishedMeetingData.FinishedMeetings.RemoveAt(i--);
						}
					}
				}

#if DEBUG
				var meetingsToUpload = MeetingDataMapper.To(finishedMeetingData.FinishedMeetings);  //Teszt data mapping

				//Create sample result
				var pending = new PendingMeeting[] 
				{
					new PendingMeeting() { Title = "MinMax", StartDate = DateTime.MinValue, EndDate = DateTime.MaxValue, OrganizerId = userId, OrganizerEmail = "test1@tct.hu", OrganizerFirstName = "Elek1", OrganizerLastName = "Teszt1"},
					new PendingMeeting() { Title = "MaxMin", StartDate = DateTime.MaxValue, EndDate = DateTime.MinValue, OrganizerId = userId, OrganizerEmail = "test3@tct.hu", OrganizerFirstName = "Elek2", OrganizerLastName = "Teszt2"},
					new PendingMeeting() { Title = "tct workshop sad asd asd asd as das ds a d", StartDate = DateTime.UtcNow.Date.AddHours(13), EndDate = DateTime.UtcNow.Date.AddHours(14), OrganizerEmail = "test3@tct.hu", OrganizerFirstName = "Elek3", OrganizerLastName = "Teszt3"},
				};

				DateTime lastSync;
				if (finishedMeetingData.LastQueryIntervalEndDate.HasValue)
					lastSync = lastSyncDateTimesDebug[userId] = finishedMeetingData.LastQueryIntervalEndDate.Value;
				else
					if (!lastSyncDateTimesDebug.TryGetValue(userId, out lastSync))
						lastSync = DateTime.SpecifyKind(DateTime.Today - TimeSpan.FromDays(90), DateTimeKind.Utc);
				if (finishedMeetingData.FinishedMeetings != null)
				{
					syncedMeetingsDebug.AddOrUpdate(userId, i => finishedMeetingData.FinishedMeetings.ToDictionary(m => m.Id), (i, entries) =>
					{
						foreach (var meeting in finishedMeetingData.FinishedMeetings)
						{
							if (entries.ContainsKey(meeting.Id))
								entries[meeting.Id] = meeting;
							else
								entries.Add(meeting.Id, meeting);
						}
						return entries;
					});
				}

				res = new ManageMeetingsResponse()
				{
					CalendarEmailAccounts = new string[] { "rado.andras@tct.hu", "buzas.ferenc@tct.hu", "dobosy.kristof@tct.hu" },
					LastSuccessfulSyncDate = lastSync,
					MeetingstoApprove = pending,
					UpcomingMeetings =  syncedMeetingsDebug.TryGetValue(userId, out var list) ? list.Values.Where(m => m.StartTime.Date == DateTime.Today).Select(m => new PendingMeeting{Id = m.Id.GetHashCode(), StartDate = m.StartTime, EndDate = m.EndTime, Title = m.Title, OrganizerEmail = m.Attendees.FirstOrDefault(a => a.Type == MeetingAttendeeType.Organizer)?.Email }).ToArray() : new PendingMeeting[0],
					Result = ManageMeetingsRet.OK
				};

#else
				string ticket;
				using (var context = new JobControlDataClassesDataContext())
				{
					ticket = context.GetAuthTicket(userId);
				}

				using (var client = new Website.WebsiteClientWrapper())
				{
					ManageMeetingsRequest req = new ManageMeetingsRequest()
					{
						LoginTicket = new Guid(ticket),
						ComputerId = computerId,
						MeetingsToUpload = MeetingDataMapper.To(finishedMeetingData.FinishedMeetings),
						LastQueryIntervalEndDate = finishedMeetingData.LastQueryIntervalEndDate,
					};
					log.DebugFormat("Api.ManageMeetingsAsync calling with userId:{0}, ComputerId:{1}, MessageCnt:{2} LastQueryIntervalEndDate:{3}", userId, req.ComputerId, req.MeetingsToUpload.Length, req.LastQueryIntervalEndDate);
					res = await client.Client.ManageMeetingsAsync(req);

				}
#endif
				result = MeetingDataMapper.From(res);

				//TODO: Handling response result status. But there is nothing to do with it.

				if (deads != null) //try to save dead letters if there were no exceptions (this is not water-tight at all...)
				{
					foreach (var dead in deads)
					{
						var resd = DeadLetterHelper.TrySaveItem(dead, new FaultException("FinishedMeetingEntry too old")); //we have only one kind of an exception for dead letters
						if (!resd) log.Error("Unable to save FinishedMeetingEntry into dead letter " + dead); //this is lost forever
					}
				}

				return result;
			}
			catch (Exception ex)
			{
				log.Error("ManageMeetings failed userId: " + userId.ToInvariantString(), ex);
				throw;
			}
			finally
			{
				string finishedMeetingCountStr = finishedMeetingData != null && finishedMeetingData.FinishedMeetings != null
					? (finishedMeetingData.FinishedMeetings.Count +
					   ((deads != null && deads.Count > 0) ? "(deads: " + deads.Count + ")" : ""))
					: "";
				log.DebugFormat(
					"Service call ManageMeetings finished in {0:0.000}ms (UserId: {1}, FinishMeetingsCount: {4}, UpcomingMeetingsCnt: {5}, LastSuccessfulSyncDate: {2}, WebSiteResultCode: {3})",
					sw.Elapsed.TotalMilliseconds, userId,
					(result != null && result.LastSuccessfulSyncDate.HasValue) ? result.LastSuccessfulSyncDate.Value.ToString() : "null",
					res != null ? res.Result.ToString() : "null",
					finishedMeetingCountStr, res?.UpcomingMeetings?.Length ?? 0);
			}
		}

#endregion

#region AddManualMeeting

		public async Task AddManualMeetingAsync(int userId, ManualMeetingData manualMeeting, int? computerId)
		{
			log.Verbose($"AddManualMeetingAsync started uid: {userId}");
			var sw = Stopwatch.StartNew();
			ManualMeetingDataDead dead = null;
			try
			{
				EnsureAccess(userId);
				int groupId;
				int companyId;
				if (!UserIdManager.Instance.TryGetIdsForUser(userId, out groupId, out companyId))
				{
					throw new FaultException("User is not active");
				}
				var createdBy = GetUserId(userId);
				ManualWorkItem manualWorkItem = new ManualWorkItem
				{
					UserId = userId,
					StartDate = manualMeeting.StartTime,
					EndDate = manualMeeting.EndTime,
					WorkId = manualMeeting.WorkId,
					ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
					Comment = manualMeeting.Title,
					GroupId = groupId,
					CompanyId = companyId,
					CreatedBy = createdBy,
					SourceId = (byte)ManualWorkItemSourceEnum.ServerAdhocMeeting,
				};
				if (manualWorkItem.EndDate < manualWorkItem.StartDate)
				{
					dead = new ManualMeetingDataDead() { ManualMeetingData = manualMeeting, UserId = userId };
					throw new FaultException("Invalid interval for ManualWorkItem");
				}
				if (manualWorkItem.EndDate - manualWorkItem.StartDate > TimeSpan.FromHours(30)) //todo enforce in db?
				{
					dead = new ManualMeetingDataDead() { ManualMeetingData = manualMeeting, UserId = userId };
					throw new FaultException("ManualWorkItem is too long");
				}
				if (manualWorkItem.WorkId < 0) //0 is used for something....
				{
					dead = new ManualMeetingDataDead() { ManualMeetingData = manualMeeting, UserId = userId };
					throw new FaultException("Invalid workid for ManualWorkItem");
				}
				if (DateTime.UtcNow - manualWorkItem.StartDate > maxWorkItemAge)
				{
					dead = new ManualMeetingDataDead() { ManualMeetingData = manualMeeting, UserId = userId };
					throw new FaultException("ManualWorkItem too old");
				}
				using (var context = new ManualDataClassesDataContext())
				{
					context.Connection.Open();
					context.Connection.SetXactAbortOn();
					if (manualMeeting.OnGoing)
					{
						//using (context.Transaction = context.Connection.BeginTransaction(System.Data.IsolationLevel.Serializable))
						//{
						var matchedWorkitems = context.ManualWorkItems.Where(
							  n => n.StartDate == manualWorkItem.StartDate
								&& n.ManualWorkItemTypeId == manualWorkItem.ManualWorkItemTypeId
								&& n.UserId == manualWorkItem.UserId);
						if (manualWorkItem.CreatedBy != null && matchedWorkitems.Any(n => n.CreatedBy == null))
						{
							log.Debug("Ongoing manualMeeting after meeting created received uid: " + userId.ToInvariantString() + " " + manualWorkItem);
							return;
						}
						matchedWorkitems = (manualWorkItem.CreatedBy == null) ? matchedWorkitems.Where(n => n.CreatedBy == null) : matchedWorkitems.Where(n => n.CreatedBy == manualWorkItem.CreatedBy.Value);

						if (matchedWorkitems.Where(n => n.EndDate == manualWorkItem.EndDate).Any())
						{
							log.Debug("Duplicate manualMeeting received uid: " + userId.ToInvariantString() + " " + manualWorkItem);
							return;
						}
						var item = matchedWorkitems.FirstOrDefault();
						if (item != null)
						{
							item.WorkId = manualMeeting.WorkId;
							item.EndDate = manualMeeting.EndTime;
						}
						else
							context.ManualWorkItems.InsertOnSubmit(manualWorkItem);
						context.SubmitChanges();
						//	context.Transaction.Commit();
						//}
					}
					else
					{
#if !DEBUG
						if (manualMeeting.EndTime > manualMeeting.StartTime) // meeting time = 0 for to delete workitem only 
						{
							string ticket;
							using (var jccontext = new JobControlDataClassesDataContext())
							{
								ticket = jccontext.GetAuthTicket(userId);
							}
							using (var client = new Website.WebsiteClientWrapper())
							{
								var res = await client.Client.AddManualMeetingsAsync(new Guid(ticket), new ManualMeeting[] { MeetingDataMapper.To(manualMeeting) });
								if (res != AddManualMeetingsRet.OK)
								{
									throw new FaultException("AddManualMeetings returned with: " + res); // TODO persists as dead letter and processed later instead of passing back
								}
							}
						}
#endif
						using (context.Transaction = context.Connection.BeginTransaction(System.Data.IsolationLevel.Serializable))
						{
							var matchedWorkitems = context.ManualWorkItems.Where(
								n => n.StartDate == (manualMeeting.OriginalStartTime ?? manualWorkItem.StartDate)
									 && n.ManualWorkItemTypeId == manualWorkItem.ManualWorkItemTypeId
									 && n.UserId == manualWorkItem.UserId);
							matchedWorkitems = (manualWorkItem.CreatedBy == null) ? matchedWorkitems.Where(n => n.CreatedBy == null) : matchedWorkitems.Where(n => n.CreatedBy == manualWorkItem.CreatedBy.Value);

							context.ManualWorkItems.DeleteAllOnSubmit(matchedWorkitems);
#if DEBUG // delete inactive time in debug
							if (manualMeeting.OriginalStartTime.HasValue && manualMeeting.IncludedIdleMinutes > 0)
							{
								// for backward compatibility we test if inactivity time deleted at form popped up
								var removedWorkitem =
									context.ManualWorkItems.FirstOrDefault(
										m =>
											m.UserId == userId && m.StartDate == manualMeeting.OriginalStartTime.Value &&
											m.ManualWorkItemTypeId == ManualWorkItemTypeEnum.DeleteComputerInterval);
								if (removedWorkitem == null)
								{
									ManualWorkItem workItemToRemove = new ManualWorkItem
									{
										UserId = userId,
										StartDate = manualMeeting.OriginalStartTime.Value,
										EndDate = manualMeeting.OriginalStartTime.Value.AddMinutes(manualMeeting.IncludedIdleMinutes),
										ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteComputerInterval,
										GroupId = groupId,
										CompanyId = companyId,
										CreatedBy = createdBy,
										SourceId = (byte)ManualWorkItemSourceEnum.ServerAdhocMeeting,
									};
									context.ManualWorkItems.InsertOnSubmit(workItemToRemove);
								}
							}
#endif
							context.SubmitChanges();
							context.Transaction.Commit();
						}
					}
				}
#if !DEBUG // delete inactive time in release
				if (manualMeeting.OriginalStartTime.HasValue && manualMeeting.IncludedIdleMinutes > 0)
				{
					var startTime = manualMeeting.OriginalStartTime.Value;
					var endTime = startTime.AddMinutes(manualMeeting.IncludedIdleMinutes);
					if (startTime < DateTime.UtcNow - WorkTimeHistoryDbHelper.GetUserSettings(userId).ModificationAgeLimit)
					{
						log.Warn($"Delete inactive time on too old interval uid: {userId}, start: {startTime}, end: {endTime}");
						return;
					}

					if (manualMeeting.IncludedIdleMinutes >= 24 * 60)
					{
						log.Warn($"Delete inactive time on too long interval uid: {userId}, start: {startTime}, end: {endTime}");
						return;
					}

					ModifyWorkTimeHelper.ModifyWorkTime(userId, groupId, companyId, createdBy, new WorkTimeModifications
					{
						ManualIntervalModifications = new List<ManualIntervalModification> { new ManualIntervalModification{ NewItem = new ManualInterval
						{
							StartDate = startTime,
							EndDate = endTime,
							ManualWorkItemType = ManualWorkItemTypeEnum.DeleteComputerInterval,
							SourceId = (byte)ManualWorkItemSourceEnum.ServerAdhocMeeting,
						} } },
						ComputerId = computerId
					});
				}
#endif
			}
			catch (Exception ex)
			{
				if (dead != null && DeadLetterHelper.TrySaveItem(dead, ex))
				{
					log.Warn("AddManualMeetings failed and moved to dead letter " + dead, ex);
					return;
				}
				if (manualMeeting.StartTime < ConfigManager.IgnoreErrorsCutOff)
				{
					log.Fatal("AddManualMeetings failed uid: " + userId.ToInvariantString() + " " + manualMeeting + " but ignoring errors before " + ConfigManager.IgnoreErrorsCutOff.ToInvariantString());
					return;
				}
				log.Error("AddManualMeetings failed uid: " + userId.ToInvariantString() + " " + manualMeeting, ex);
				throw;
			}
			finally
			{
				log.Debug("Service call AddManualMeetings uid: " + userId.ToInvariantString() + " " + manualMeeting + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}

		public async Task<AddManualMeetingsResult> AddManualMeetings(int userId, IEnumerable<ManualMeetingData> manualMeetings)
		{
			var sw = Stopwatch.StartNew();
			try
			{
#if DEBUG
				return AddManualMeetingsResult.OK;
#endif
				foreach (var manualMeeting in manualMeetings) // iteration is formal, only one item is expected
					await AddManualMeetingAsync(userId, manualMeeting, null);
			}
			catch (Exception ex)
			{
				log.Error("AddManualMeetings failed userId: " + userId.ToInvariantString(), ex);
				return AddManualMeetingsResult.UnknownError;
			}
			finally
			{
				log.Debug("Service call AddManualMeetings userId: " + userId.ToInvariantString() + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
			return AddManualMeetingsResult.OK;
		}

#endregion

#region GetAllWorks

		public List<AllWorkItem> GetAllWorks(int userId)
		{
			log.Verbose($"GetAllWorks started uid: {userId}");
			var sw = Stopwatch.StartNew();
			try
			{
				EnsureAccess(userId);
				using (var context = new JobControlDataClassesDataContext())
				{
					var result = context.GetAllWorks(userId).ToList();
					if (result.Count > 0) //remove the root as it is the company's name (there should be only one)
					{
						var root = result.Where(n => n.ParentId == null && n.Type == 0).Single();
						result.Remove(root);
						foreach (var workEntry in result.Where(n => n.ParentId == root.TaskId))
						{
							workEntry.ParentId = null;
						}
					}
					return result;
				}
			}
			catch (Exception ex)
			{
				log.Error("GetAllWorks failed userId: " + userId.ToInvariantString(), ex);
				throw;
			}
			finally
			{
				log.Debug("Service call GetAllWorks userId: " + userId.ToInvariantString() + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}

#endregion

#region AssignTask

		public async Task<AssignTaskResult> AssignTaskAsync(int userId, int taskId)
		{
			log.Verbose($"AssignTaskAsync started uid: {userId}");
			var sw = Stopwatch.StartNew();
			try
			{
				EnsureAccess(userId);
#if DEBUG
				return AssignTaskDebug(userId, taskId) ? AssignTaskResult.Ok : AssignTaskResult.UnknownError;
#endif
				string ticket;
				using (var context = new JobControlDataClassesDataContext())
				{
					ticket = context.GetAuthTicket(userId);
				}
				using (var client = new Website.WebsiteClientWrapper())
				{
					AssignTaskResult result;
					var res = await client.Client.AssignUserToTaskByGuidAsync(new Guid(ticket), taskId);
					switch (res)
					{
						case AssignUserToTaskRet.OK:
							result = AssignTaskResult.Ok;
							menuVersionCacheManager.Remove(userId);
							break;
						case AssignUserToTaskRet.AccessDenied:
							result = AssignTaskResult.AccessDenied;
							break;
						case AssignUserToTaskRet.UnknownError:
							result = AssignTaskResult.UnknownError;
							break;
						default:
							result = AssignTaskResult.UnknownError;
							log.Warn("AssignTask unknown result " + res);
							break;
					}
					return result;
				}
			}
			catch (Exception ex)
			{
				log.Error("AssignTask failed userId: " + userId.ToInvariantString(), ex);
				throw;
			}
			finally
			{
				log.Debug("Service call AssignTask userId: " + userId.ToInvariantString() + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}

#if DEBUG
		private bool AssignTaskDebug(int userId, int workId)
		{
			string newVersion;
			var menu = GetClientMenu(userId, "", out newVersion);
			var queue = new Queue<WorkData>();
			if (menu == null) menu = new ClientMenu();
			if (menu.Works == null) menu.Works = new List<WorkData>();
			menu.Works.ForEach(n => queue.Enqueue(n));
			while (queue.Count > 0)
			{
				var curr = queue.Dequeue();
				if (curr.Id.HasValue && curr.Id.Value == workId) return false;
				if (curr.Children != null)
				{
					curr.Children.ForEach(n => queue.Enqueue(n));
				}
			}
			var dyn = menu.Works.Where(n => n.Name == "Hozzarendelt munkak").FirstOrDefault();
			if (dyn == null)
			{
				dyn = new WorkData("Hozzarendelt munkak", null, null);
				menu.Works.Add(dyn);
			}
			dyn.ProjectId = 11000;
			if (dyn.Children == null) dyn.Children = new List<WorkData>();
			dyn.Children.Add(new WorkData("Munka " + workId, workId, null));
			SetClientMenu(userId, menu);
			return true;
		}
#endif
#endregion

#region GetTaskReasons

		public TaskReasons GetTaskReasons(int userId)
		{
			log.Verbose($"GetTaskReasons started uid: {userId}");
			var sw = Stopwatch.StartNew();
			try
			{
				EnsureAccess(userId);
				using (var context = new JobControlDataClassesDataContext())
				{
					var result = context.GetTaskReasons(userId);
					return result;
				}
			}
			catch (Exception ex)
			{
				log.Error("GetTaskReasons failed userId: " + userId.ToInvariantString(), ex);
				throw;
			}
			finally
			{
				log.Debug("Service call GetTaskReasons userId: " + userId.ToInvariantString() + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}

#endregion

#region GetExpiryDay

		public DateTime? GetExpiryDay(int userId)
		{
			log.Verbose($"GetExpiryDay started uid: {userId}");
			var sw = Stopwatch.StartNew();
			try
			{
				EnsureAccess(userId);
				if (!ConfigManager.CreditRunOutCheckingEnabled) return null;
				using (var context = new JobControlDataClassesDataContext())
				{
					var result = context.GetExpiryDay(userId);
					return result;
				}
			}
			catch (Exception ex)
			{
				log.Error("GetExpiryDay failed userId: " + userId.ToInvariantString(), ex);
				throw;
			}
			finally
			{
				log.Debug("Service call GetExpiryDay userId: " + userId.ToInvariantString() + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}

#endregion

#region GetWorkTimeHistory

		public ClientWorkTimeHistory GetWorkTimeHistory(int userId, DateTime startDate, DateTime endDate)
		{
			log.Verbose($"GetWorkTimeHistory started uid: {userId}");
			var sw = Stopwatch.StartNew();
			try
			{
				EnsureAccess(userId);
				if (endDate < startDate) throw new FaultException("Invalid interval");
				if (endDate - startDate > TimeSpan.FromDays(2)) throw new FaultException("Interval too big");
				ClientWorkTimeHistory result;
#if DEBUG
				result = GetWorkTimeHistoryDebug(userId, startDate, endDate);
#else
				result = new ClientWorkTimeHistory();
				try
				{
					string ticket;
					using (var context = new JobControlDataClassesDataContext())
					{
						ticket = context.GetAuthTicket(userId);
					}
					var targetDay = endDate.Date;
					if (endDate.Hour < 12) targetDay = targetDay.AddDays(-1); // hax to get day in client local time
					using (var client = new WebsiteClientWrapper())

					{
						var apiResult = client.Client.GetCarpetDiagramForUser(new Guid(ticket), userId, targetDay);
						result.IsModificationApprovalNeeded = apiResult.CarpetPermissions.IsModificationApprovalNeeded;
						result.ModificationAgeLimit = TimeSpan.FromHours(apiResult.CarpetPermissions.ModificationAgeLimitInHours);

						result.ComputerIntervals = apiResult.CarpetPcWorkItems.Select(x => 
							new ComputerInterval
							{
								ComputerId = x.ComputerId,
								EndDate = x.EndDate,
								StartDate = x.StartDate,
								WorkId = x.WorkId
							}).ToList();
						result.ManualIntervals = apiResult.CarpetManualWorkItems.Select(x => 
							new ManualInterval
							{
								IsPendingDeleteAlso = false,
								IsEditable = true,
								IsPending = false,
								ManualWorkItemType = ManualWorkItemTypeEnum.AddWork,
								IsMeeting = false,
								EndDate = x.EndDate,
								StartDate = x.StartDate,
								WorkId = x.WorkId
							}).ToList();
						result.ManualIntervals.AddRange(apiResult.CarpetMeetingWorkItems.Select(x => 
							new ManualInterval
							{
								IsPendingDeleteAlso = false,
								IsEditable = true,
								IsPending = false,
								ManualWorkItemType = ManualWorkItemTypeEnum.AddWork,
								IsMeeting = true,
								Description = x.Description,
								EndDate = x.EndDate,
								StartDate = x.StartDate,
								WorkId = x.TaskId
							}).ToList());
						result.ManualIntervals.AddRange(apiResult.CarpetRequestedManualWorkItems.Select(x =>
							new ManualInterval
							{
								IsPendingDeleteAlso = false,
								IsEditable = true,
								IsPending = true,
								ManualWorkItemType = ManualWorkItemTypeEnum.AddWork,
								IsMeeting = false,
								EndDate = x.EndDate,
								StartDate = x.StartDate,
								WorkId = x.WorkId
							}).ToList());
						result.ManualIntervals.AddRange(apiResult.CarpetRequestedMeetingWorkItems.Select(x => 
							new ManualInterval
							{
								IsPendingDeleteAlso = false,
								IsEditable = true,
								IsPending = true,
								ManualWorkItemType = ManualWorkItemTypeEnum.AddWork,
								IsMeeting = true,
								Description = x.Description,
								EndDate = x.EndDate,
								StartDate = x.StartDate,
								WorkId = x.TaskId
							}).ToList());
						result.MobileIntervals = apiResult.CarpetMobileWorkItems.Select(x => 
							new MobileInterval
							{
								Imei = x.Imei,
								EndDate = x.EndDate,
								StartDate = x.StartDate,
								WorkId = x.WorkId
							}).ToList();
						if(apiResult.PaidLeaveTimeInMs > 0)
						{
							result.ManualIntervals.Add(new ManualInterval
							{
								IsPendingDeleteAlso = false,
								IsEditable = true,
								IsPending = true,
								ManualWorkItemType = ManualWorkItemTypeEnum.AddHoliday,
								IsMeeting = false,
								EndDate = startDate.AddMilliseconds(apiResult.PaidLeaveTimeInMs),
								StartDate = startDate,
								WorkId = HOLIDAY_WORK_ID
							});
						}
						if (apiResult.SickLeaveTimeInMs > 0)
						{
							result.ManualIntervals.Add(new ManualInterval
							{
								IsPendingDeleteAlso = false,
								IsEditable = true,
								IsPending = true,
								ManualWorkItemType = ManualWorkItemTypeEnum.AddSickLeave,
								IsMeeting = false,
								EndDate = startDate.AddMilliseconds(apiResult.SickLeaveTimeInMs),
								StartDate = startDate,
								WorkId = SICKDAY_WORK_ID
							});
						}

						result.TotalTimeInMs = apiResult.DailyWorkingTimeStats.TotalTimeInMs;
						result.StartTimeInMs = apiResult.DailyWorkingTimeStats.StartTimeInMs;
						result.EndTimeInMs = apiResult.DailyWorkingTimeStats.EndTimeInMs;
						result.StartEndDiffInMs = apiResult.DailyWorkingTimeStats.StartEndDiffInMs;
						result.LastComputerWorkitemEndTime = apiResult.DailyWorkingTimeStats.LastComputerWorkitemEndTime;
					}
				}
				catch (Exception ex)
				{
					log.Error("Getting worktime history carpet diagram report failed", ex);
					throw;
				}
#endif
				result.ComputerIntervals.ForEach(x => x.EndDate = x.EndDate < endDate ? x.EndDate : endDate);
				result.ComputerIntervals.ForEach(x => x.StartDate = x.StartDate > startDate ? x.StartDate : startDate);
				result.MobileIntervals.ForEach(x => x.EndDate = x.EndDate < endDate ? x.EndDate : endDate);
				result.MobileIntervals.ForEach(x => x.StartDate = x.StartDate > startDate ? x.StartDate : startDate);
				result.ManualIntervals.ForEach(x => x.EndDate = x.EndDate < endDate ? x.EndDate : endDate);
				result.ManualIntervals.ForEach(x => x.StartDate = x.StartDate > startDate ? x.StartDate : startDate);
				return result;
			}
			catch (Exception ex)
			{
				log.Error("GetWorkTimeHistory failed", ex);
				throw;
			}
			finally
			{
				log.DebugFormat("Service call GetWorkTimeHistory finished for userId: {0} between {1} and {2} in {3} ms", userId.ToInvariantString(), startDate.ToInvariantString(), endDate.ToInvariantString(), sw.ToTotalMillisecondsString());
			}
		}

#if DEBUG
		private ClientWorkTimeHistory GetWorkTimeHistoryDebug(int userId, DateTime startDate, DateTime endDate)
		{
			var workTimeHistory = new ClientWorkTimeHistory()
			{
				IsModificationApprovalNeeded = (Environment.TickCount / 1000) % 3 == 0,
				ComputerIntervals = StatsDbHelper.GetAggregateWorkItemIntervalsForUserCovered(userId, startDate, endDate).Select(n => new ComputerInterval() { WorkId = n.WorkId, StartDate = n.StartDate, EndDate = n.EndDate, ComputerId = n.ComputerId }).ToList(),
				MobileIntervals = StatsDbHelper.GetMobileWorkItemsForUserCovered(userId, startDate, endDate).Select(n => new MobileInterval() { WorkId = n.WorkId, StartDate = n.StartDate, EndDate = n.EndDate, Imei = n.Imei }).ToList(),
				ManualIntervals = StatsDbHelper.GetManualWorkItemsForUser(userId, startDate, endDate).Select(n => new ManualInterval() { WorkId = n.WorkId.GetValueOrDefault(), StartDate = n.StartDate, EndDate = n.EndDate, Id = n.Id, Comment = n.Comment, ManualWorkItemType = n.ManualWorkItemTypeId, SourceId = n.SourceId }).ToList(),
				ModificationAgeLimit = new TimeSpan(3, 0, 0, 0),
			};
			var start = workTimeHistory.ComputerIntervals.Min(c => (DateTime?)c.StartDate);
			workTimeHistory.StartTimeInMs = start.HasValue ? (long?)(start.Value.FromUtcToLocal(TimeZoneInfo.Local) - start.Value.Date).TotalMilliseconds : null;
			var end = workTimeHistory.ComputerIntervals.Max(c => (DateTime?)c.EndDate);
			workTimeHistory.EndTimeInMs = end.HasValue ? (long?)(end.Value.FromUtcToLocal(TimeZoneInfo.Local) - end.Value.Date).TotalMilliseconds : null;
			workTimeHistory.StartEndDiffInMs = workTimeHistory.EndTimeInMs - workTimeHistory.StartTimeInMs;
			if (start != null)
				workTimeHistory.TotalTimeInMs = workTimeHistory.ComputerIntervals.Sum(c =>(long)(c.EndDate - c.StartDate).TotalMilliseconds);
			workTimeHistory.LastComputerWorkitemEndTime = end;
			return workTimeHistory;
		}
#endif
#endregion

#region ModifyWorkTime

		public bool ModifyWorkTime(int userId, WorkTimeModifications modifications)
		{
			log.Verbose($"ModifyWorkTime started uid: {userId}");
			var sw = Stopwatch.StartNew();
			try
			{
				EnsureAccess(userId);
				int groupId;
				int companyId;
				if (!UserIdManager.Instance.TryGetIdsForUser(userId, out groupId, out companyId))
				{
					throw new FaultException("User is not active");
				}
				var createdBy = GetUserId(userId);
				if (modifications == null || modifications.ManualIntervalModifications == null || modifications.ManualIntervalModifications.Count == 0) return false;
#if DEBUG
				return ModifyWorkTimeDebug(userId, groupId, companyId, createdBy, modifications);
#endif
				return ModifyWorkTimeHelper.ModifyWorkTime(userId, groupId, companyId, createdBy, modifications);
			}
			catch (Exception ex)
			{
				log.Error("ModifyWorkTime failed", ex);
				throw;
			}
			finally
			{
				log.DebugFormat("Service call ModifyWorkTime finished for userId: {0} {1} in {2} ms", userId.ToInvariantString(), modifications == null ? "" : modifications.ToString(), sw.ToTotalMillisecondsString());
			}
		}

#if DEBUG
		public bool ModifyWorkTimeDebug(int userId, int groupId, int companyId, int createdBy, WorkTimeModifications modifications)
		{
			using (var context = new ManualDataClassesDataContext())
			{
				context.SetXactAbortOn();
				foreach (var modification in modifications.ManualIntervalModifications)
				{
					if (modification.OriginalItem != null && modification.NewItem == null) //delete
					{
						var item = context.ManualWorkItems.Single(n => n.Id == modification.OriginalItem.Id && n.UserId == userId);
						context.ManualWorkItems.DeleteOnSubmit(item);
					}
					else if (modification.OriginalItem == null && modification.NewItem != null) //insert
					{
						context.ManualWorkItems.InsertOnSubmit(new ManualWorkItem()
						{
							UserId = userId,
							WorkId = modification.NewItem.GetWorkId(),
							StartDate = modification.NewItem.StartDate,
							EndDate = modification.NewItem.EndDate,
							ManualWorkItemTypeId = modification.NewItem.ManualWorkItemType,
							SourceId = (byte)ManualWorkItemSourceEnum.ServerAdhocMeeting,
							Comment = modification.NewItem.Comment,
							GroupId = groupId,
							CompanyId = companyId,
							CreatedBy = createdBy,
						});
					}
					else if ((modification.OriginalItem != null && modification.NewItem != null)) //update
					{
						var item = context.ManualWorkItems.Single(n => n.Id == modification.OriginalItem.Id && n.UserId == userId);
						item.WorkId = modification.NewItem.GetWorkId();
						item.StartDate = modification.NewItem.StartDate;
						item.EndDate = modification.NewItem.EndDate;
						item.ManualWorkItemTypeId = modification.NewItem.ManualWorkItemType;
						item.SourceId = (byte)ManualWorkItemSourceEnum.ServerAdhocMeeting;
						item.Comment = modification.NewItem.Comment;
						//item.CreatedBy = createdBy;
						//item.CreateDate = DateTime.UtcNow;
					}
					else
					{
						log.ErrorAndFail("Invalid modification (null)");
					}
				}
				context.SubmitChanges();
			}
			return true;
		}
#endif
#endregion

#region GetWorkNames

		private const int HOLIDAY_WORK_ID = int.MaxValue;
		private const int SICKDAY_WORK_ID = int.MaxValue - 1;
		public WorkNames GetWorkNames(int userId, List<int> workIds)
		{
			log.Verbose($"GetWorkNames started uid: {userId}");
			var sw = Stopwatch.StartNew();
			try
			{
				EnsureAccess(userId);
				WorkNames result;
#if DEBUG

				result = new WorkNames() { Names = workIds.Select(n => new WorkOrProjectName() { Id = n, Name = "Ismeretlen munka " + n, CategoryId = (n % 4 == 0 ? ((n / 4) % 4) + 1 : new int?()) }).ToList() };
				result.Names.RemoveAll(x => x.Id == HOLIDAY_WORK_ID || x.Id == SICKDAY_WORK_ID);
#else
				result = WorkTimeHistoryDbHelper.GetWorkNames(userId, workIds);
#endif
				bool containsHoliday = workIds.Contains(HOLIDAY_WORK_ID);
				bool containsSickday = workIds.Contains(SICKDAY_WORK_ID);
				if(containsHoliday || containsSickday)
				{
					result.Names.AddRange(getHolidayNames(userId, containsHoliday, containsSickday));
				}
				return result;
			}
			catch (Exception ex)
			{
				log.Error("GetWorkNames failed", ex);
				throw;
			}
			finally
			{
				log.DebugFormat("Service call GetWorkNames finished for userId: {0} work count {1} in {2} ms", userId.ToInvariantString(), workIds == null ? "(null)" : workIds.Count.ToInvariantString(), sw.ToTotalMillisecondsString());
			}
		}

		private List<WorkOrProjectName> getHolidayNames(int userId, bool holiday, bool sickday)
		{
			UserStatInfo userStatInfo = StatsDbHelper.GetUserStatsInfo(new List<int>(new int[] { userId })).First();
			var culture = CultureInfo.GetCultureInfo(string.IsNullOrEmpty(userStatInfo.CultureId) ? EmailStatsHelper.DefaultCulture : userStatInfo.CultureId);
			Thread.CurrentThread.CurrentCulture = culture;
			Thread.CurrentThread.CurrentUICulture = culture;
			var result = new List<WorkOrProjectName>();
			if(holiday)
				result.Add(new WorkOrProjectName { Id = HOLIDAY_WORK_ID, Name = EmailStats.EmailStats.HolidayTime });
			if(sickday)
				result.Add(new WorkOrProjectName { Id = SICKDAY_WORK_ID, Name = EmailStats.EmailStats.SickLeaveTime });
			return result;

		}

#endregion

#region AddStats

		public void AddTelemetry(TelemetryItem telemetryItem)
		{
			log.Verbose($"AddTelemetry started uid: {telemetryItem?.UserId}");
			if (telemetryItem == null) return;
			var sw = Stopwatch.StartNew();
			try
			{
				EnsureAccess(telemetryItem.UserId);
				TelemetryHelper.Save(telemetryItem);
			}
			catch (Exception ex)
			{
				log.Error("Service call AddTelemetry userId: " + telemetryItem.UserId.ToInvariantString() + " failed", ex);
			}
			finally
			{
				log.Debug("Service call AddTelemetry userId: " + telemetryItem.UserId.ToInvariantString() + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}
#endregion

#region AddSnippet

		public bool AddSnippet(Snippet sd)
		{
			log.Verbose($"AddSnippet started uid: {sd?.UserId}");
			if (sd == null) return false;
			if (snippetFilter.IsDuplicate(sd))
			{
				log.Debug("Service call AddSnippet w/ userId: " + sd.UserId.ToInvariantString() + "Snippet was a duplicate");
				return true;
			}
			var sw = Stopwatch.StartNew();
			try
			{
				EnsureAccess(sd.UserId);
				using (var context = new ActivityRecorderDataClassesDataContext())
				{
					context.SetXactAbortOn();
					sd.CreatedAt = DateTime.Now;
					sd.Quality = 5;
					context.Connection.Open();
					context.Snippets.InsertOnSubmit(sd);
					context.SubmitChanges();
					context.Connection.Close();
				}
			}
			catch (Exception ex)
			{
				log.Error("Service call AddSnippet w/ userId: " + sd.UserId.ToInvariantString() + " failed", ex);
				return false;
			}
			finally
			{
				log.Debug("Service call AddSnippet w/ userId: " + sd.UserId.ToInvariantString() + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
			return true;
		}


#endregion

#region GetDppMessage

		public string GetDppMessage(int userId)
		{
			log.Verbose($"GetDppMessage started uid: {userId}");
			var sw = Stopwatch.StartNew();
			UserStatInfo stat = null;
			try
			{
				EnsureAccess(userId);
				using (var context = new JobControlDataClassesDataContext())
				{
					stat = context.GetUserStatInfoById(userId);
					return stat.IsClientAcceptanceMessageNeeded && !stat.ClientAcceptanceMessageAcceptedAt.HasValue ? stat.ClientAcceptanceMessage : null;
				}
			}
			catch (Exception ex)
			{
				log.Error("GetDppMessage failed for user " + userId.ToInvariantString(), ex);
				throw;
			}
			finally
			{
				log.Info("GetDppMessage for user " + userId.ToInvariantString() +"(" + (stat != null ? string.Format("needed: {0}, len: {1}, accepted: {2}", stat.IsClientAcceptanceMessageNeeded, stat.ClientAcceptanceMessage?.Length ?? 0, stat.ClientAcceptanceMessageAcceptedAt) : "") + ") finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}

#endregion

#region SetDppAcceptanceDate

		public AcceptanceData GetDppInformation(int userId)
		{
			log.Verbose($"GetDppInformation started uid: {userId}");
			var sw = Stopwatch.StartNew();
			UserStatInfo stat = null;
			try
			{
				EnsureAccess(userId);
				using (var context = new JobControlDataClassesDataContext())
				{
					stat = context.GetUserStatInfoById(userId);
					return stat.IsClientAcceptanceMessageNeeded ? new AcceptanceData(){ AcceptedAt = stat.ClientAcceptanceMessageAcceptedAt, Message = stat.ClientAcceptanceMessage } : null;
				}
			}
			catch (Exception ex)
			{
				log.Error("GetDppInformation failed for user " + userId.ToInvariantString(), ex);
				throw;
			}
			finally
			{
				log.Info("GetDppInformation for user " + userId.ToInvariantString() + "(" + (stat != null ? string.Format("needed: {0}, len: {1}, accepted: {2}", stat.IsClientAcceptanceMessageNeeded, stat.ClientAcceptanceMessage?.Length ?? 0, stat.ClientAcceptanceMessageAcceptedAt) : "") + ") finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}


#endregion

#region SetDppAcceptanceDate

		public bool SetDppAcceptanceDate(int userId, DateTime acceptedAt)
		{
			log.Verbose($"SetDppAcceptanceDate started uid: {userId}");
			var sw = Stopwatch.StartNew();
			bool ret = false;
			try
			{
				EnsureAccess(userId);
				using (var context = new JobControlDataClassesDataContext())
				{
					ret = context.SetDppAcceptanceDate(userId, acceptedAt);
					return ret;
				}
			}
			catch (Exception ex)
			{
				log.Error("SetDppAcceptanceDate failed for user " + userId.ToInvariantString(), ex);
				throw;
			}
			finally
			{
				log.Info("SetDppAcceptanceDate for user " + userId.ToInvariantString() + "(ret:" + ret + ") finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}

#endregion

#region ManageCloudTokens

		public CloudTokenData ManageCloudTokens(int userId, string googleCalendarToken)
		{
			log.Verbose($"ManageCloudTokens started uid: {userId}");
			var sw = Stopwatch.StartNew();
			var ret = new CloudTokenData();
			try
			{
				EnsureAccess(userId);
				using (var context = new ActivityRecorderDataClassesDataContext())
				{
					if (googleCalendarToken != null)
					{
						context.Client_SetCloudToken(userId, googleCalendarToken);
						ret.GoogleCalendarToken = googleCalendarToken;
					}
					else
					{
						var cloudToken = context.Client_GetCloudTokenByUserId(userId).FirstOrDefault();
						ret.GoogleCalendarToken = cloudToken?.AuthToken;
					}
					return ret;
				}
			}
			catch (Exception ex)
			{
				log.Error("ManageCloudTokens failed for user " + userId.ToInvariantString(), ex);
				throw;
			}
			finally
			{
				log.Info("ManageCloudTokens for user " + userId.ToInvariantString() + "(ret:" + ret + ") finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}

#endregion

#region ShouldSendLogs

		public bool ShouldSendLogs(int userId)
		{
			log.Verbose($"ShouldSendLogs started uid: {userId}");
			var sw = Stopwatch.StartNew();
			try
			{
				EnsureAccess(userId);
				return AcquireClientLogHelper.ShouldSendLogs(userId);
			}
			catch (Exception ex)
			{
				log.Error("ShouldSendLogs failed for user " + userId.ToInvariantString(), ex);
				throw;
			}
			finally
			{
				log.Debug("ShouldSendLogs for user " + userId.ToInvariantString() + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}

#endregion

#region GetWorkTimeStatsForUser

		public WebsiteServiceReference.WorkTimeStats GetWorkTimeStatsForUser(int userId, int computerId, WorktimeStatIntervals intervals)
		{
			log.Verbose($"GetWorkTimeStatsForUser started uid: {userId}");
			var sw = Stopwatch.StartNew();
			try
			{
				EnsureAccess(userId);
				if (intervals == 0) intervals = WorktimeStatIntervals.Today | WorktimeStatIntervals.Month | WorktimeStatIntervals.Week;
				var changedIntervals = worktimeStatIntervalAggregator.Add(userId, computerId, intervals, DateTime.UtcNow);
#if DEBUG
				//throw new FaultException("Unexpected exception.");
				Thread.Sleep(10000);
				var stats = onlineStatsManager.GetDetailedUserStats(userId);
				var ret = new WebsiteServiceReference.WorkTimeStats
				{
					LastComputerWorkitemEndTime = DateTime.UtcNow.AddHours(-6),
					TodaysWorkTimeInMs = (changedIntervals & WorktimeStatIntervals.Today) > 0 ? (int)stats.TodaysWorkTime.NetWorkTime.TotalMilliseconds : 0,
					ThisWeeksWorkTimeInMs = (changedIntervals & WorktimeStatIntervals.Week) > 0 ? 76914578 : 0,
					ThisMonthsWorkTimeInMs = (changedIntervals & WorktimeStatIntervals.Month) > 0 ? 84951625 : 0,
					ThisQuarterWorkTimeInMs = (changedIntervals & WorktimeStatIntervals.Quarter) > 0 ? 213453455 : 0,
					ThisYearWorkTimeInMs = (changedIntervals & WorktimeStatIntervals.Year) > 0 ? 8223235435 : 0,
					TodaysTargetNetWorkTimeInMs = (changedIntervals & WorktimeStatIntervals.Today) > 0 ? 28800000 : 0,
					ThisWeeksTargetNetWorkTimeInMs = (changedIntervals & WorktimeStatIntervals.Week) > 0 ? 144000000 : 0,
					ThisMonthsTargetNetWorkTimeInMs = (changedIntervals & WorktimeStatIntervals.Month) > 0 ? 604800000 : 0,
					ThisQuarterTargetNetWorkTimeInMs = (changedIntervals & WorktimeStatIntervals.Quarter) > 0 ? 1814400000 : 0,
					ThisYearTargetNetWorkTimeInMs = (changedIntervals & WorktimeStatIntervals.Year) > 0 ? 7257600000 : 0,
					ThisWeeksTargetUntilTodayNetWorkTimeInMs = (changedIntervals & WorktimeStatIntervals.Week) > 0 ? 86400000 : 0,
					ThisMonthsTargetUntilTodayNetWorkTimeInMs = (changedIntervals & WorktimeStatIntervals.Month) > 0 ? 86400000 : 0,
					ThisQuarterTargetUntilTodayNetWorkTimeInMs = (changedIntervals & WorktimeStatIntervals.Quarter) > 0 ? 86400000 : 0,
					ThisYearTargetUntilTodayNetWorkTimeInMs = (changedIntervals & WorktimeStatIntervals.Year) > 0 ? 86400000 : 0,
				};
				return ret;
#else
				using (var clientWrapper = new WebsiteClientWrapper())
				using (var context = new JobControlDataClassesDataContext())
				{
					try
					{
						var stats = clientWrapper.Client.GetWorktimeStatsForUser(new Guid(context.GetAuthTicket(userId)), null, changedIntervals);
						return stats;
					}
					catch (FaultException ex)
					{
						log.Error(ex);
						throw;
					}
					catch (Exception ex)
					{
						log.Error("Unexpected exception in GetWorkTimeStatsForUser", ex);
						throw new FaultException("Unexpected exception.");
					}
				}
#endif
			}
			catch (Exception ex)
			{
				log.Error("GetWorkTimeStatsForUser failed for user " + userId.ToInvariantString(), ex);
				throw;
			}
			finally
			{
				log.Debug("GetWorkTimeStatsForUser for user " + userId.ToInvariantString() + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}

#if DEBUG
		private static volatile bool isAutomaticRulesSwitchedOffDebug;
#endif

		public bool SwitchAutomaticRules(int userId)
		{
#if DEBUG
			isAutomaticRulesSwitchedOffDebug = true;
			return true;
#endif
			log.Verbose($"SwitchAutomaticRules started uid: {userId}");
			var sw = Stopwatch.StartNew();
			try
			{
				using (var clientWrapper = new WebsiteClientWrapper())
				using (var context = new JobControlDataClassesDataContext())
				{
					var res = clientWrapper.Client.DisableCompanyDictionaryAutoRules(new Guid(context.GetAuthTicket(userId)));
					log.Debug($"Api.DeleteCompanyDictionaryAutoRules calling with userId:{userId}, returned: {res}");
					if (res == DisableCompanyDictionaryAutoRulesRet.UnknownError) throw new FaultException("unexpected error while deleting company dictionary auto rules");
					return res == DisableCompanyDictionaryAutoRulesRet.OK;
				}
			}
			catch (Exception ex)
			{
				log.Error("SwitchAutomaticRules failed for user " + userId.ToInvariantString(), ex);
				throw;
			}
			finally
			{
				log.Debug("SwitchAutomaticRules for user " + userId.ToInvariantString() + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}

		#endregion

		#region GetUserWorkdays

		public List<DateTime> GetUserWorkdays(int userId)
		{
			var sw = Stopwatch.StartNew();
			try
			{
				EnsureAccess(userId);
				return onlineStatsManager.GetUserWorkdays(userId);
			}
			catch (Exception ex)
			{
				log.Error("GetUserWorkdays failed for user " + userId.ToInvariantString(), ex);
				throw;
			}
			finally
			{
				log.Debug("GetUserWorkdays for user " + userId.ToInvariantString() + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}

		#endregion

		#region INotificationService
		public NotificationData GetPendingNotification(int userId, int computerId, int? lastId)
		{
			log.Verbose($"GetPendingNotification started uid: {userId}");
			var sw = Stopwatch.StartNew();
			try
			{
				EnsureAccess(userId);
				return notificationService.GetPendingNotification(userId, computerId, lastId);
			}
			catch (Exception ex)
			{
				log.Error("GetPendingNotification failed uid: " + userId.ToInvariantString() + " cid: " + computerId.ToInvariantString() + " lid: " + lastId.ToInvariantString(), ex);
				throw;
			}
			finally
			{
				log.Debug("Service call GetPendingNotification uid: " + userId.ToInvariantString() + " cid: " + computerId.ToInvariantString() + " lid: " + lastId.ToInvariantString() + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}

		public void ConfirmNotification(NotificationResult result)
		{
			if (result == null) return;
			var sw = Stopwatch.StartNew();
			try
			{
				EnsureAccess(result.UserId);
				notificationService.ConfirmNotification(result);
			}
			catch (Exception ex)
			{
				log.Error("ConfirmNotification failed " + result, ex);
				throw;
			}
			finally
			{
				log.Debug("Service call ConfirmNotification " + result + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}
#endregion

		private static void EnsureAccess(int userId)
		{
			if (!string.IsNullOrEmpty(ConfigManager.ImportPassword)) return; //allow import user to do anything
			var username = ServiceSecurityContext.Current.PrimaryIdentity.Name;
			var userIdRaw = AuthenticationManager.GetUserIdCached(username);
			if (userIdRaw == null || userIdRaw.Value == userId) return;
			log.Error("User: " + username + " has no access for userId: " + userId.ToInvariantString());
			throw new FaultException("Access denied");
		}

		private static int GetUserId(int? importFallback)
		{
			var username = ServiceSecurityContext.Current.PrimaryIdentity.Name;
			var userId = AuthenticationManager.GetUserIdCached(username);
			if (userId == null)
			{
				if (!string.IsNullOrEmpty(ConfigManager.ImportUserName)
					&& ConfigManager.ImportUserName == username
					&& importFallback.HasValue)
				{
					userId = importFallback.Value;
				}
				else
				{
					throw new Exception("Invalid userId");
				}
			}
			return userId.Value;
		}

		private static int GetUserIdAllowWebSite()
		{
			var userIdStr = ServiceSecurityContext.Current.PrimaryIdentity.Name;
			return int.Parse(
				userIdStr.StartsWith(AuthenticationManager.WebsiteUserPrefix)
				? userIdStr.Substring(AuthenticationManager.WebsiteUserPrefix.Length)
				: userIdStr);
		}

#region IActivityStats Members

#if LEGACY
		public DailyStats GetDailyStats()
		{
			var sw = Stopwatch.StartNew();
			try
			{
				return statsBuilder.GetDailyStatsFiltered(new StatsFilter(null, null, null));
			}
			catch (Exception ex)
			{
				log.Error("IActivityStats: GetDailyStats failed", ex);
				throw;
			}
			finally
			{
				log.Info("IActivityStats: GetDailyStats finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}

		public DailyStats GetDailyStatsByUserId(int userId)
		{
			var sw = Stopwatch.StartNew();
			try
			{
				return statsBuilder.GetDailyStatsFiltered(new StatsFilter(new[] { userId }, null, null));
			}
			catch (Exception ex)
			{
				log.Error("IActivityStats: GetDailyStatsByUserId " + userId.ToInvariantString() + " failed", ex);
				throw;
			}
			finally
			{
				log.Info("IActivityStats: GetDailyStatsByUserId " + userId.ToInvariantString() + "  finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}

		public DailyStats GetDailyStatsByUserIds(List<int> userIds)
		{
			var sw = Stopwatch.StartNew();
			try
			{
				return statsBuilder.GetDailyStatsFiltered(new StatsFilter(userIds, null, null));
			}
			catch (Exception ex)
			{
				log.Error("IActivityStats: GetDailyStatsByUserIds "
				+ (userIds == null ? "null" : string.Join(", ", userIds.Select(n => n.ToString()).ToArray())) + " failed", ex);
				throw;
			}
			finally
			{
				log.Info("IActivityStats: GetDailyStatsByUserIds "
				+ (userIds == null ? "null" : string.Join(", ", userIds.Select(n => n.ToString()).ToArray())) + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}

		public DailyStats GetDailyStatsByGroupId(int groupId)
		{
			var sw = Stopwatch.StartNew();
			try
			{
				return statsBuilder.GetDailyStatsFiltered(new StatsFilter(null, groupId, null));
			}
			catch (Exception ex)
			{
				log.Error("IActivityStats: GetDailyStatsByGroupId " + groupId + " failed", ex);
				throw;
			}
			finally
			{
				log.Info("IActivityStats: GetDailyStatsByGroupId " + groupId + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}

		public DailyStats GetDailyStatsByCompanyId(int companyId)
		{
			var sw = Stopwatch.StartNew();
			try
			{
				return statsBuilder.GetDailyStatsFiltered(new StatsFilter(null, null, companyId));
			}
			catch (Exception ex)
			{
				log.Error("IActivityStats: GetDailyStatsByCompanyId " + companyId + " failed", ex);
				throw;
			}
			finally
			{
				log.Info("IActivityStats: GetDailyStatsByCompanyId " + companyId + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}
#endif

		public void SendDailyEmails(DateTime date, List<int> userIds)
		{
			log.Verbose($"SendDailyEmails started");
			var sw = Stopwatch.StartNew();
			try
			{
				EmailStatsHelper.Send(date, ReportType.Daily, userIds);
			}
			catch (Exception ex)
			{
				log.Error("IActivityStats: SendDailyEmails Date:" + date.ToInvariantString() + " userIds: " + (userIds == null ? "null" : string.Join(", ", userIds.Select(n => n.ToString()).ToArray())) + " failed", ex);
				throw;
			}
			finally
			{
				log.Info("IActivityStats: SendDailyEmails Date:" + date.ToInvariantString() + " userIds: " + (userIds == null ? "null" : string.Join(", ", userIds.Select(n => n.ToString()).ToArray())) + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}

		public void SendDailyEmailsTo(DateTime date, List<int> userIds, List<string> toAddresses)
		{
			log.Verbose($"SendDailyEmailsTo started");
			var sw = Stopwatch.StartNew();
			try
			{
				EmailStatsHelper.Send(date, ReportType.Daily, userIds, toAddresses);
			}
			catch (Exception ex)
			{
				log.Error(
					"IActivityStats: SendDailyEmails Date:" + date.ToInvariantString() + " userIds: " + (userIds == null ? "null" : string.Join(", ", userIds.Select(n => n.ToString()).ToArray()))
					+ " toAddresses: " + (toAddresses == null ? "null" : string.Join(", ", toAddresses.Select(n => n.ToString()).ToArray())) + " failed", ex);
				throw;
			}
			finally
			{
				log.Info("IActivityStats: SendDailyEmails Date:" + date.ToInvariantString() + " userIds: " + (userIds == null ? "null" : string.Join(", ", userIds.Select(n => n.ToString()).ToArray()))
					+ " toAddresses: " + (toAddresses == null ? "null" : string.Join(", ", toAddresses.Select(n => n.ToString()).ToArray())) + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}

		public void SendWeeklyEmails(DateTime date, List<int> userIds)
		{
			var sw = Stopwatch.StartNew();
			try
			{
				EmailStatsHelper.Send(date, ReportType.Weekly, userIds);
				//ThreadPool.QueueUserWorkItem(_ => EmailStatsHelper.Send(date, ReportType.Weekly, userIds));
			}
			catch (Exception ex)
			{
				log.Error("IActivityStats: SendWeeklyEmails Date:" + date.ToInvariantString() + " userIds: " + (userIds == null ? "null" : string.Join(", ", userIds.Select(n => n.ToString()).ToArray())) + " failed", ex);
				throw;
			}
			finally
			{
				log.Info("IActivityStats: SendWeeklyEmails Date:" + date.ToInvariantString() + " userIds: " + (userIds == null ? "null" : string.Join(", ", userIds.Select(n => n.ToString()).ToArray())) + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}

		public void SendWeeklyEmailsTo(DateTime date, List<int> userIds, List<string> toAddresses)
		{
			log.Verbose($"SendWeeklyEmailsTo started");
			var sw = Stopwatch.StartNew();
			try
			{
				EmailStatsHelper.Send(date, ReportType.Weekly, userIds, toAddresses);
			}
			catch (Exception ex)
			{
				log.Error(
					"IActivityStats: SendWeeklyEmails Date:" + date.ToInvariantString() + " userIds: " + (userIds == null ? "null" : string.Join(", ", userIds.Select(n => n.ToString()).ToArray()))
					+ " toAddresses: " + (toAddresses == null ? "null" : string.Join(", ", toAddresses.Select(n => n.ToString()).ToArray())) + " failed", ex);
				throw;
			}
			finally
			{
				log.Info("IActivityStats: SendWeeklyEmails Date:" + date.ToInvariantString() + " userIds: " + (userIds == null ? "null" : string.Join(", ", userIds.Select(n => n.ToString()).ToArray()))
					+ " toAddresses: " + (toAddresses == null ? "null" : string.Join(", ", toAddresses.Select(n => n.ToString()).ToArray())) + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}

		public void SendMonthlyEmails(DateTime date, List<int> userIds)
		{
			log.Verbose($"SendMonthlyEmails started");
			var sw = Stopwatch.StartNew();
			try
			{
				EmailStatsHelper.Send(date, ReportType.Monthly, userIds);
				//ThreadPool.QueueUserWorkItem(_ => EmailStatsHelper.Send(date, ReportType.Monthly, userIds));
			}
			catch (Exception ex)
			{
				log.Error("IActivityStats: SendMonthlyEmails Date:" + date.ToInvariantString() + " userIds: " + (userIds == null ? "null" : string.Join(", ", userIds.Select(n => n.ToString()).ToArray())) + " failed", ex);
				throw;
			}
			finally
			{
				log.Info("IActivityStats: SendMonthlyEmails Date:" + date.ToInvariantString() + " userIds: " + (userIds == null ? "null" : string.Join(", ", userIds.Select(n => n.ToString()).ToArray())) + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}

		public void SendMonthlyEmailsTo(DateTime date, List<int> userIds, List<string> toAddresses)
		{
			log.Verbose($"SendMonthlyEmailsTo started");
			var sw = Stopwatch.StartNew();
			try
			{
				EmailStatsHelper.Send(date, ReportType.Monthly, userIds, toAddresses);
			}
			catch (Exception ex)
			{
				log.Error(
					"IActivityStats: SendMonthlyEmails Date:" + date.ToInvariantString() + " userIds: " + (userIds == null ? "null" : string.Join(", ", userIds.Select(n => n.ToString()).ToArray()))
					+ " toAddresses: " + (toAddresses == null ? "null" : string.Join(", ", toAddresses.Select(n => n.ToString()).ToArray())) + " failed", ex);
				throw;
			}
			finally
			{
				log.Info("IActivityStats: SendMonthlyEmails Date:" + date.ToInvariantString() + " userIds: " + (userIds == null ? "null" : string.Join(", ", userIds.Select(n => n.ToString()).ToArray()))
					+ " toAddresses: " + (toAddresses == null ? "null" : string.Join(", ", toAddresses.Select(n => n.ToString()).ToArray())) + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}

		public void SendProjectEmails(DateTime utcStartDate, DateTime utcEndDate, int reportUserId, bool isInternal, List<int> projectRootIds, List<string> toAddresses)
		{
			log.Verbose($"SendProjectEmails started");
			var sw = Stopwatch.StartNew();
			try
			{
				//todo EnsureAccess(reportUserId);
				if (projectRootIds == null || projectRootIds.Count == 0) return;
				if (toAddresses == null || toAddresses.Count == 0) return;
				var toList = toAddresses.Select(t => new EmailTarget { Address = t, CultureId = EmailStatsHelper.DefaultCulture }).ToList(); // TODO: should be sent with real locale
				EmailProjectStatsHelper.Send(utcStartDate, utcEndDate, reportUserId, isInternal, projectRootIds, toList);
			}
			catch (Exception ex)
			{
				log.Error("IActivityStats: SendProjectEmails from:" + utcStartDate + " to:" + utcEndDate + " reportUserId:" + reportUserId + " isInternal:" + isInternal
				+ " projectRootIds: " + (projectRootIds == null ? "null" : string.Join(", ", projectRootIds.Select(n => n.ToString()).ToArray()))
				+ " toAddresses: " + (toAddresses == null ? "null" : string.Join(", ", toAddresses.ToArray()))
				+ " failed", ex);
				throw;
			}
			finally
			{
				log.Info("IActivityStats: SendProjectEmails from:" + utcStartDate + " to:" + utcEndDate + " reportUserId:" + reportUserId + " isInternal:" + isInternal
				+ " projectRootIds: " + (projectRootIds == null ? "null" : string.Join(", ", projectRootIds.Select(n => n.ToString()).ToArray()))
				+ " toAddresses: " + (toAddresses == null ? "null" : string.Join(", ", toAddresses.ToArray()))
				+ " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}

		private static readonly Func<int, List<MonitorableUser>> getMonitorableUsersFunc = CachedFunc.CreateThreadSafe<int, List<MonitorableUser>>(userId => StatsDbHelper.GetMonitorableUsers(userId), TimeSpan.FromMinutes(5));
		private static List<MonitorableUser> GetFilteredMonitorableUserIdsForUser(int userId, List<int> interestedUserIds)
		{
			var availUserIds = getMonitorableUsersFunc(userId);
			if (interestedUserIds == null || interestedUserIds.Count == 0)
			{
				return availUserIds.ToList();
			}
			else
			{
				var interestedUserIdsSet = new HashSet<int>(interestedUserIds);
				return availUserIds.Where(n => interestedUserIdsSet.Contains(n.UserId)).ToList();
			}
		}

		public List<BriefUserStats> GetBriefUserStats(List<int> userIds)
		{
			log.Verbose($"GetBriefUserStats started");
			var sw = Stopwatch.StartNew();
			var result = new List<BriefUserStats>();
			int userId = -1;
			try
			{
				userId = GetUserIdAllowWebSite();
				var monUserIds = GetFilteredMonitorableUserIdsForUser(userId, userIds);
				foreach (var monUserId in monUserIds)
				{
					try
					{
						var stats = onlineStatsManager.GetBriefUserStats(monUserId.UserId);
						if (stats == null) continue;
						result.Add(stats);
					}
					catch (Exception ex)
					{
						log.Error("IActivityStats: GetBriefUserStats from " + userId.ToInvariantString() + " for userId: " + monUserId.UserId + " failed", ex);
					}
				}
			}
			catch (Exception ex)
			{
				log.Info("IActivityStats: GetBriefUserStats from " + userId.ToInvariantString() + " for userIds: "
						 + (userIds == null ? "null" : string.Join(", ", userIds.Select(n => n.ToString()).ToArray())) +
						 " failed", ex);
				throw;
			}
			finally
			{
				log.Info("IActivityStats: GetBriefUserStats from " + userId.ToInvariantString() + " for userIds: "
						 + (userIds == null ? "null" : string.Join(", ", userIds.Select(n => n.ToString()).ToArray())) +
						 " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
			return result;
		}

		public List<DetailedUserStats> GetDetailedUserStats(List<int> userIds)
		{
			log.Verbose($"GetDetailedUserStats started");
			var sw = Stopwatch.StartNew();
			var result = new List<DetailedUserStats>();
			int userId = -1;
			try
			{
				userId = GetUserIdAllowWebSite();
				var monUserIds = GetFilteredMonitorableUserIdsForUser(userId, userIds);
				foreach (var monUserId in monUserIds)
				{
					try
					{
						var stats = onlineStatsManager.GetDetailedUserStats(monUserId.UserId);
						if (stats == null) continue;
						if (monUserId.ScreenShotsHidden)
						{
							foreach (var compStats in stats.ComputerStatsByCompId)
							{
								if (compStats.Value.RecentComputerActivity == null) continue;
								compStats.Value.RecentComputerActivity.LastScreenShots.Clear();
							}
						}
						result.Add(stats);
					}
					catch (Exception ex)
					{
						log.Error("IActivityStats: GetDetailedUserStats from " + userId.ToInvariantString() + " for userId: " + monUserId.UserId + " failed", ex);
					}
				}
			}
			catch (Exception ex)
			{
				log.Info("IActivityStats: GetDetailedUserStats from " + userId.ToInvariantString() + " for userIds: "
						 + (userIds == null ? "null" : string.Join(", ", userIds.Select(n => n.ToString()).ToArray())) +
						 " failed", ex);
				throw;
			}
			finally
			{
				log.Info("IActivityStats: GetDetailedUserStats from " + userId.ToInvariantString() + " for userIds: "
						 + (userIds == null ? "null" : string.Join(", ", userIds.Select(n => n.ToString()).ToArray())) +
						 " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
			return result;
		}


#endregion

#region GetApplicationUpdate

		private static string GetMsiPathFor(int userId, string application, string currentVersion) //todo shadow copy?
		{
			if (application == null) application = "JobCTRL"; //backward compatibility
			if (application.Contains('#') && !ConfigManager.GiveUpdateByClientAssemblyName)
				application = application.Split('#')[0];
			string path;
			if (UpdatePathHelper.TryGetPathFor(application, out path)) //check overrides in config
			{
				return path;
			}
			if (IsDebug) //only for debug builds
			{
				return "c:\\" + application + ".msi";
			}
			if (application.StartsWith("JobCTRL", StringComparison.OrdinalIgnoreCase))
			{
				using (var context = new JobControlDataClassesDataContext()) //todo we need to pass application to the db to support voxctrl
				{
					return context.GetUpdatePackage(userId, currentVersion, application);
				}
			}
			throw new Exception(application + " application not found");
		}

		public ApplicationUpdateInfo GetApplicationUpdate(int userId, string application, string currentVersion)
		{
			log.Verbose($"GetApplicationUpdate started uid: {userId}");
			var sw = Stopwatch.StartNew();
			ApplicationUpdateInfo result = null;
			try
			{
				EnsureAccess(userId);
				var msiFilePath = GetMsiPathFor(userId, application, currentVersion); //assume application is not changed
				if (string.IsNullOrEmpty(msiFilePath)) return null; // no update has been configured
				if (IsDebug && !File.Exists(msiFilePath)) return null; //this is only valid in debug builds
				var msiFileInfo = msiFileCache.GetMsiFileInfo(msiFilePath);
				string updateVersion = msiFileInfo != null ? msiFileInfo.MsiVersion : null;
				Version currentFileVersion, updateFileVersion;
				if (updateVersion == null
					|| !Version.TryParse(currentVersion, out currentFileVersion)
					|| !Version.TryParse(updateVersion, out updateFileVersion))
				{
					log.ErrorFormat("Unable to match versions userId: {3} path: {0} msiver: {1} ver: {2} ", msiFilePath, updateVersion, currentVersion, userId.ToInvariantString());
				}
				else if (updateFileVersion > currentFileVersion)
				{
					var res = fileDownloadManager.StartDownload(msiFileInfo);
					result = new ApplicationUpdateInfo()
					{
						FileId = res.FileId,
						ChunkCount = res.ChunkCount,
						Version = updateVersion
					};
				}

				return result;
			}
			catch (Exception ex)
			{
				log.Error("GetApplicationUpdate failed userId: " + userId.ToInvariantString(), ex);
				throw;
			}
			finally
			{
				log.Debug("Service call GetApplicationUpdate userId: " + userId.ToInvariantString() + " app: " + application + " ver: " + currentVersion + " res: " + result + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}

		public byte[] GetUpdateChunk(Guid fileId, long chunkIndex)
		{
			log.Verbose($"GetUpdateChunk started fileId: {fileId} idx: {chunkIndex}");
			var sw = Stopwatch.StartNew();
			try
			{
				return fileDownloadManager.DownloadChunk(fileId, chunkIndex);
			}
			catch (Exception ex)
			{
				log.Error("GetUpdateChunk failed fileId: " + fileId.ToString() + " ch: " + chunkIndex.ToInvariantString(), ex);
				throw;
			}
			finally
			{
				log.Debug("Service call GetUpdateChunk fileId: " + fileId.ToString() + " ch: " + chunkIndex.ToInvariantString() + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}

#endregion

#region IVoiceRecorder

		private static readonly StreamDataUploadStore voiceStore = new StreamDataUploadStore();

		public void UpsertVoiceRecording(VoiceRecording voiceRecording)
		{
			int userId = voiceRecording.UserId;
			log.Verbose($"UpsertVoiceRecording started uid: {userId}");
			var sw = Stopwatch.StartNew();
			try
			{
				EnsureAccess(userId);
				int groupId;
				int companyId;
				if (!UserIdManager.Instance.TryGetIdsForUser(userId, out groupId, out companyId))
				{
					throw new FaultException("User is not active");
				}
				voiceRecording.GroupId = groupId;
				voiceRecording.CompanyId = companyId;

				using (var context = new VoiceRecorderDataClassesDataContext())
				{
					context.Connection.Open();
					using (context.Transaction = context.Connection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
					{
						try
						{
							var upd = context.UpsertVoiceRecording(voiceRecording);
							if (upd == 0)
							{
								log.Debug("Duplicate UpsertVoiceRecording (update) received uid: " + voiceRecording.UserId + " start: " + voiceRecording.StartDate + "/" + voiceRecording.EndDate + " dur: " + voiceRecording.Duration + " cid: " + voiceRecording.ClientId + " l:" + voiceRecording.Length + " o:" + voiceRecording.Offset);
								return; //data was commited once so file should not be updated on the disk
							}
							if (voiceRecording.Offset == 0) voiceStore.Open(voiceRecording);
							voiceStore.AddData(voiceRecording);
							if (voiceRecording.EndDate != null) voiceStore.Close(voiceRecording);
							context.Transaction.Commit();
						}
						catch (SqlException sqlex)
						{
							if (sqlex != null && sqlex.Message != null && sqlex.Message.Contains("IX_VoiceRecordings_ClientId"))
							{
								log.Debug("Duplicate UpsertVoiceRecording (insert) received uid: " + voiceRecording.UserId + " start: " + voiceRecording.StartDate + "/" + voiceRecording.EndDate + " dur: " + voiceRecording.Duration + " cid: " + voiceRecording.ClientId + " l:" + voiceRecording.Length + " o:" + voiceRecording.Offset);
								return; //data was commited once so file should not be updated on the disk
							}
							throw;
						}
					}
				}
			}
			catch (Exception ex)
			{
				if (voiceRecording.StartDate < ConfigManager.IgnoreErrorsCutOff)
				{
					log.Fatal("UpsertVoiceRecording failed uid: " + voiceRecording.UserId + " start: " + voiceRecording.StartDate + "/" + voiceRecording.EndDate + " dur: " + voiceRecording.Duration + " cid: " + voiceRecording.ClientId + " l:" + voiceRecording.Length + " o:" + voiceRecording.Offset + " but ignoring errors before " + ConfigManager.IgnoreErrorsCutOff);
					return;
				}
				log.Error("UpsertVoiceRecording failed userId: " + userId.ToInvariantString() + " start: " + voiceRecording.StartDate + "/" + voiceRecording.EndDate + " dur: " + voiceRecording.Duration + " cid: " + voiceRecording.ClientId + " l:" + voiceRecording.Length + " o:" + voiceRecording.Offset, ex);
				throw;
			}
			finally
			{
				log.Debug("Service call UpsertVoiceRecording userId: " + userId.ToInvariantString() + " start: " + voiceRecording.StartDate + "/" + voiceRecording.EndDate + " dur: " + voiceRecording.Duration + " cid: " + voiceRecording.ClientId + " l:" + voiceRecording.Length + " o:" + voiceRecording.Offset + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}

		public void DeleteVoiceRecording(VoiceRecording voiceRecording)
		{
			int userId = voiceRecording.UserId;
			log.Verbose($"DeleteVoiceRecording started uid: {userId}");
			var sw = Stopwatch.StartNew();
			try
			{
				EnsureAccess(userId);
				int groupId;
				int companyId;
				if (!UserIdManager.Instance.TryGetIdsForUser(userId, out groupId, out companyId))
				{
					throw new FaultException("User is not active");
				}
				voiceRecording.GroupId = groupId;
				voiceRecording.CompanyId = companyId;

				using (var context = new VoiceRecorderDataClassesDataContext())
				{
					var upd = context.DeleteThisVoiceRecording(voiceRecording);
					if (upd == 0)
					{
						log.Debug("Duplicate DeleteVoiceRecording received uid: " + voiceRecording.UserId + " start: " + voiceRecording.StartDate + "/" + voiceRecording.EndDate + " dur: " + voiceRecording.Duration + " cid: " + voiceRecording.ClientId + " l:" + voiceRecording.Length + " o:" + voiceRecording.Offset);
					}
				}
			}
			catch (Exception ex)
			{
				if (voiceRecording.StartDate < ConfigManager.IgnoreErrorsCutOff)
				{
					log.Fatal("DeleteVoiceRecording failed uid: " + voiceRecording.UserId + " start: " + voiceRecording.StartDate + "/" + voiceRecording.EndDate + " dur: " + voiceRecording.Duration + " cid: " + voiceRecording.ClientId + " l:" + voiceRecording.Length + " o:" + voiceRecording.Offset + " but ignoring errors before " + ConfigManager.IgnoreErrorsCutOff);
					return;
				}
				log.Error("DeleteVoiceRecording failed userId: " + userId.ToInvariantString() + " start: " + voiceRecording.StartDate + "/" + voiceRecording.EndDate + " dur: " + voiceRecording.Duration + " cid: " + voiceRecording.ClientId + " l:" + voiceRecording.Length + " o:" + voiceRecording.Offset, ex);
				throw;
			}
			finally
			{
				log.Debug("Service call DeleteVoiceRecording userId: " + userId.ToInvariantString() + " start: " + voiceRecording.StartDate + "/" + voiceRecording.EndDate + " dur: " + voiceRecording.Duration + " cid: " + voiceRecording.ClientId + " l:" + voiceRecording.Length + " o:" + voiceRecording.Offset + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}

#endregion

#region IActiveDirectoryLoginService

		public ClientLoginTicket GetClientLoginTicket()
		{
			log.Verbose($"GetClientLoginTicket started");
			var sw = Stopwatch.StartNew();
			int? userId = null;
			string sid = "";
			try
			{
				if (ServiceSecurityContext.Current.WindowsIdentity.User == null) throw new FaultException("Missing SID.");

				using (var context = new JobControlDataClassesDataContext())
				{
					sid = ServiceSecurityContext.Current.WindowsIdentity.User.Value;
					userId = context.GetUserIdFromSid(sid);
					if (!userId.HasValue) return null;
				}

				return ActivityRecorderUserNamePasswordValidator.GetNewTicketForUser(userId.Value);
			}
			catch (Exception ex)
			{
				log.Error("GetClientLoginToken failed for sid: " + sid + " userId: " + userId.ToInvariantString(), ex);
				throw;
			}
			finally
			{
				log.Info("Service call GetClientLoginToken sid: " + sid + " userId: " + userId.ToInvariantString() + " finished in " + sw.ToTotalMillisecondsString() + "ms");
			}
		}

#endregion

		private static bool IsDebug
		{
			get
			{
#if DEBUG
				return true;
#endif
				return false;
			}
		}
	}
}