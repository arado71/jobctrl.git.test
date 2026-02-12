using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Runtime.InteropServices.ComTypes;
//using log4net;
using System.Text.RegularExpressions;
using Tct.ActivityRecorderClient;
using System.ServiceModel;
using System.Threading;
using System.Windows.Forms;
using log4net;
using OutlookInteropService;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.ProcessInfo;
using Tct.ActivityRecorderClient.Serialization;

namespace OutlookMeetingCaptureService
{
	/// <summary>
	/// This is an implementation using Outlook Redemtion.
	/// We use Redemtion RDO objects for all work.
	/// </summary>
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
	public class OutlookMeetingCaptureService : IMeetingCaptureService
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly List<FinishedMeetingEntry> emptyFinishedMeetings = new List<FinishedMeetingEntry>();
		//private Redemption.RDOSession _rdoSession;
		private const string unknownEmail = "UNKNOWN";
		private const int futureSyncDays = 7;
		private readonly TimeSpan DelayedDeleteIntervalDefault = TimeSpan.FromMinutes(15);

		private bool needNonMeetingAppointments;
		private bool needUpdatesDeletes;
		private bool needTentativeMeetings;
		private MeetingStore messageStore;
		private Dictionary<string, DateTime> veryOldExistingItems;
		private Regex folderInclusionRegex;
		private Regex folderExclusionRegex;
		private TimeSpan delayedDeleteInterval;
		private TimeSpan forgetDelayedDeleteItemsAfter => TimeSpan.FromTicks(delayedDeleteInterval.Ticks * 2);

		private readonly HashSet<Object> rdoObjects = new HashSet<Object>();

		public string LocalStorePattern { get; set; }

		/// <summary>
		/// Initialization should be called on main thread.
		/// </summary>
		public void Initialize()
		{
			//if (_rdoSession != null) return;

			//try
			//{
			//    //Create RDOSession and keep it referenced on the main thread.
			//    _rdoSession = Redemption.RedemptionLoader.new_RDOSession();
			//    //_rdoSession.Logon(); //If we don't reuse the session on secondary threads, than it is not necesserary to logon with it now. Uness we loggging those informations below.

			//    //log.Info(String.Format("Initialized successfully. Outlook Ver.: {0}, ProfileName: {1}, Redemption Ver.: {2}", _rdoSession.OutlookVersion, _rdoSession.ProfileName, _rdoSession.Version));
			//}
			//catch (Exception e)
			//{
			//    if (_rdoSession != null) { int rc = Marshal.ReleaseComObject(_rdoSession); Debug.Assert(rc == 0); _rdoSession = null; }
			//    //log.Error("Initialization failed.", e);
			//}
		}

		/// <summary>
		/// Uninitialization should be called on main thread.
		/// </summary>
		public void Dispose()
		{
			//if (_rdoSession == null) return;

			//try
			//{
			//    //_rdoSession.Logoff();

			//    //log.Info("Uninitialized successfully.");
			//}
			//catch (Exception e)
			//{
			//    //log.Error("Uninitialization failed.", e);
			//}
			//finally
			//{
			//    if (_rdoSession != null) { int rc = Marshal.ReleaseComObject(_rdoSession); Debug.Assert(rc == 0); _rdoSession = null; }
			//}
		}

		public string GetVersionInfo(bool useRedemption)
		{
			Redemption.RDOSession rdoSession = null;
			Microsoft.Office.Interop.Outlook.Application oApp = null;

			log.Info("GetVersionInfo started...");
			Stopwatch sw = Stopwatch.StartNew();
			string versionInfo = "";
			try
			{
				if (useRedemption)
				{
					rdoSession = Redemption.RedemptionLoader.new_RDOSession();
					rdoSession.Logon();

					versionInfo = String.Format("Outlook Ver.: {0}, ProfileName: {1}, Redemption Ver.: {2}", rdoSession.OutlookVersion, rdoSession.ProfileName, rdoSession.Version);
					rdoSession.Logoff();
				}
				else
				{
					oApp = new Microsoft.Office.Interop.Outlook.Application();
					versionInfo = String.Format("Outlook Ver.: {0}, ProfileName: {1}", oApp.Version, oApp.DefaultProfileName);
				}


				log.Info(versionInfo);

				return versionInfo;
			}
			catch (Exception ex)
			{
				var comEx = ex as COMException;
				if (comEx != null)
				//this should only be reached if we use the GetActiveObject method
				{
					HandleCOMException(comEx);
				}
				else
				{
					log.Error("Error occured while getting version info from Outlook.", ex);

				}
				return "Outlook Ver.: N/A";
			}
			finally
			{
				ReleaseComObject(rdoSession);
				ReleaseComObject(oApp);
				log.InfoFormat("GetVersionInfo finished in {0} ms.\n", sw.Elapsed.TotalMilliseconds.ToString("0.000"));
			}
		}

		private void HandleCOMException(COMException ex)
		{

			bool isElevationChangeRequired;
			bool isOutlookElevated;
			RunningObjectTableHelper.EnsureRotRegistration("outlook.exe", out isElevationChangeRequired, out isOutlookElevated);
			//if outlook is running but not in the ROT then make sure it will be there sooner or later
			var currentProcess = Process.GetCurrentProcess();
			;
			var proc =
				Process.GetProcessesByName("OUTLOOK").Where(p => p.SessionId == currentProcess.SessionId).FirstOrDefault();
			if (proc != null)
			{
				isOutlookElevated = ProcessElevationHelper.IsElevated(proc.Id);
				isElevationChangeRequired = ProcessElevationHelper.IsElevated() != isOutlookElevated;
			}

			if (isElevationChangeRequired)
			{
				log.Info("Elevation change is needed");
				throw new FaultException(isOutlookElevated ? "Elevate" : "Unelevate");
			}
		}

		public void StopService()
		{
			log.InfoFormat("StopService started...");
			Stopwatch sw = Stopwatch.StartNew();

			try
			{
				var host = OperationContext.Current.Host;
				var sc = SynchronizationContext.Current;
				ThreadPool.QueueUserWorkItem(_ =>
				{
					Thread.Sleep(50);
					sc.Post(__ => host.Close(), null);
				});	//With .NET 4.0 simple post doesn't worked.
			}
			catch (Exception e)
			{
				log.Error("StopService failed.", e);
				throw;
			}
			finally
			{
				log.InfoFormat("StopService finished in {0} ms.\n", sw.Elapsed.TotalMilliseconds.ToString("0.000"));
			}
		}

		public List<FinishedMeetingEntry> CaptureMeetings(IList<string> calendarAccountEmails, DateTime startDate, DateTime endDate, int manualWorkItemEditAgeLimit, bool needNonMeetingAppointments, bool needUpdatesDeletes, string folderInclusionPattern, string folderExclusionPattern, bool needTentativeMeetings, bool useRedemption, int delayedDeleteIntervalInMins)
		{
			this.needNonMeetingAppointments = needNonMeetingAppointments;
			this.needUpdatesDeletes = needUpdatesDeletes;
			this.needTentativeMeetings = needTentativeMeetings;
			folderInclusionRegex = string.IsNullOrEmpty(folderInclusionPattern) ? null : new Regex(folderInclusionPattern);
			folderExclusionRegex = string.IsNullOrEmpty(folderExclusionPattern) ? null : new Regex(folderExclusionPattern);
			delayedDeleteInterval = delayedDeleteIntervalInMins > 0 ? TimeSpan.FromMinutes(delayedDeleteIntervalInMins) : DelayedDeleteIntervalDefault; 

			Redemption.RDOSession rdoSession = null;
			List<Redemption.RDOStore> stores = new List<Redemption.RDOStore>();
			List<Redemption.RDOFolder> calendarFolders = new List<Redemption.RDOFolder>();
			List<Appointment> finishedMeetings = new List<Appointment>();

			log.InfoFormat("CaptureMeetings started... ({0}, {1}, {2})", String.Join(", ", calendarAccountEmails.ToArray()), startDate, endDate);
			Stopwatch sw = Stopwatch.StartNew();

			try
			{
				var existingItemsFile = LocalStorePattern != null ? String.Format(LocalStorePattern, "ExistingCalItems") : "ExistingCalItems";
				if (messageStore == null)
				{
					if (IsolatedStorageSerializationHelper.Exists(existingItemsFile))
					{
						IsolatedStorageSerializationHelper.Load(existingItemsFile, out messageStore);
						if (messageStore == null)
						{
							log.Info("Existing items loading from old store");
							IsolatedStorageSerializationHelper.Load(existingItemsFile, out Dictionary<string, Dictionary<string, DateTime>>  oldExistingItems);
							if (oldExistingItems != null)
							{
								messageStore = new MeetingStore
								{
									MeetingDatas = oldExistingItems.Where(i => !i.Key.StartsWith("@")).ToDictionary(i => i.Key, i => i.Value.ToDictionary(j => j.Key, j => new MeetingData{ StartTime = j.Value })),
									ItemFirstSeen = oldExistingItems.Where(i => i.Key.StartsWith("@")).ToDictionary(i => i.Key.Substring(1), i => i.Value),
								};
							}
							else
							{
								log.Info("Existing items loading from very old store");
								IsolatedStorageSerializationHelper.Load(existingItemsFile, out Dictionary<string, DateTime> veryoldExistingItems);
							}
						}
					}
					if (messageStore == null)
						messageStore = new MeetingStore{ MeetingDatas = new Dictionary<string, Dictionary<string, MeetingData>>(), ItemFirstSeen = new Dictionary<string, Dictionary<string, DateTime>>() };
				}
				var lastModificationDatesFile = LocalStorePattern != null ? String.Format(LocalStorePattern, "LastModificationDates") : "LastModificationDates";
				if (messageStore.LastModificationDates == null)
				{
					if (IsolatedStorageSerializationHelper.Exists(lastModificationDatesFile) && IsolatedStorageSerializationHelper.Load(lastModificationDatesFile, out Dictionary<string, DateTime> lastModificationDates))
					{
						messageStore.LastModificationDates = lastModificationDates;
					}
					else
					{
						messageStore.LastModificationDates = new Dictionary<string, DateTime>();
					}
				}
				if (messageStore.ItemsToBeDeleted == null) messageStore.ItemsToBeDeleted = new Dictionary<string, Dictionary<string, DateTime>>();
				rdoSession = Redemption.RedemptionLoader.new_RDOSession();
				// Debugging
				rdoObjects.Add(rdoSession);
				rdoSession.Logon();

				bool? offline = null;
				try
				{
					offline = rdoSession.Offline;
					if (rdoSession.ExchangeConnectionMode != Redemption.rdoExchangeConnectionMode.olNoExchange)
						rdoSession.Offline = false;
				}
				catch (Exception ex)
				{
					log.Error("Get/set of offline property failed.", ex);
				}

				log.InfoFormat("Outlook Ver.: {0}, ProfileName: {1}, Redemption Ver.: {2}", rdoSession.OutlookVersion, rdoSession.ProfileName, rdoSession.Version);
				log.InfoFormat("ExchangeMailboxServerName: {0}, ExchangeMailboxServerVersion: {1}, ExchangeConnectionMode: {2}, Offline: {3}", rdoSession.ExchangeMailboxServerName, rdoSession.ExchangeMailboxServerVersion, rdoSession.ExchangeConnectionMode, offline.HasValue ? offline.Value.ToString() : "n/a");
				log.InfoFormat("ManualWorkItemEditAgeLimit: {0}, NeedNonMeetingAppointments: {1}, NeedTentativeMeetings: {2}, NeedUpdatesDeletes: {3}, includedFolders: {4}, excludedFolders: {5} delayedDeleteInterval: {6} mins", manualWorkItemEditAgeLimit, needNonMeetingAppointments, needTentativeMeetings, needUpdatesDeletes, folderInclusionPattern, folderExclusionPattern, delayedDeleteIntervalInMins);

				stores = GetAllStores(rdoSession);
				calendarFolders = GetAllCalendarFolders(stores, rdoSession);
				List<Redemption.RDOFolder> filteredCalendarFolder = FilterCalendarFolders(calendarFolders, calendarAccountEmails);
				finishedMeetings = GetFinishedMeetings(filteredCalendarFolder, startDate, endDate, manualWorkItemEditAgeLimit).ToList();
				List<FinishedMeetingEntry> finishedMeetingEntries = GetMeetingData(finishedMeetings).ToList();
				foreach (var item in finishedMeetingEntries.GroupBy(m => new { m.Id, m.Status }).Where(g => g.Count() > 1).ToList())
				{
					foreach (var meeting in item.OrderBy(i => i.Attendees != null && i.Attendees.Where(a => a.Email == unknownEmail).Any()).ThenByDescending(i => i.CreationTime).ThenByDescending(i => i.LastmodificationTime).Skip(1).ToList())
					{
						finishedMeetingEntries.Remove(meeting);
					}
				}
				foreach (var item in finishedMeetingEntries.Where(m => m.Attendees.Count == 1 && calendarAccountEmails.Contains(m.Attendees.First().Email)).ToList())
				{
					finishedMeetingEntries.Remove(item);
				}
				var now = DateTime.UtcNow;
				finishedMeetingEntries.ForEach(e => e.IsInFuture = e.EndTime > now);

				log.InfoFormat("CaptureMeetings: Returning the following meetings ({0})", finishedMeetingEntries.Count);
				string finishedMeetingEntriesStr = String.Join("\n", finishedMeetingEntries.Select(m => m.ToString()).ToArray());
				if (!String.IsNullOrEmpty(finishedMeetingEntriesStr)) log.Info(finishedMeetingEntriesStr);

				log.Verbose("Persisting local data...");
				IsolatedStorageSerializationHelper.Save(existingItemsFile, messageStore);
				IsolatedStorageSerializationHelper.Save(lastModificationDatesFile, messageStore.LastModificationDates);
				if (messageStore.LastModificationDates != null && IsolatedStorageSerializationHelper.Exists(lastModificationDatesFile)) IsolatedStorageSerializationHelper.Delete(lastModificationDatesFile);

				return finishedMeetingEntries;
			}
			catch (Exception e)
			{
				log.Error("Error occured while capturing meetings.", e);
				var comEx = e as COMException;
				if (comEx != null)
				//this should only be reached if we use the GetActiveObject method
				{
					try
					{
						HandleCOMException(comEx);
					}
					catch (Exception ex)
					{
						log.Error("HandleCOMException failed", ex);
					}
					return null;

				}
				log.Error("CaptureMeetings failed", e);
				throw;
			}
			finally
			{
				try
				{
					log.Verbose("Starting finally block... Releasing com appointment objects...");
					finishedMeetings.Where(m => m.Item != null).Select(m => m.Item).Distinct().ToList().ForEach(apnt => ReleaseComObject(apnt));
					log.Verbose("Releasing calendar folders...");
					foreach (Redemption.RDOFolder rdoFolder in calendarFolders) ReleaseComObject(rdoFolder);
					log.Verbose("Releasing store...");
					foreach (Redemption.RDOStore rdoStore in stores) ReleaseComObject(rdoStore);

					stores = null;
					finishedMeetings = null;
					calendarFolders = null;

					log.Verbose("Logging off and releasing session...");
					if(rdoSession != null)
						rdoSession.Logoff();	//We need to logoff if we used logon instead of setting MAPIOBJECT property of RDOSession.
					ReleaseComObject(rdoSession);
					rdoSession = null;

					log.Verbose("Dumping unhandled com objects...");
					foreach (var o in rdoObjects.Where(o => o != null))
					{
						log.WarnFormat("Remaining object after releasing: Type: {0}; ToString: {1};", TypeInformation.GetTypeName(o), o);
					}
					rdoObjects.Clear();

					log.Verbose("GC.Collect()...");
					//If some of the objects remain referenced then this call to GC.Collect release those objects. If there are unreleased RCWs then threads with mso.dll does not disappear in Process Explorer.
					GC.Collect();
					log.Verbose("GC.WaitForPendingFinalizers()...");
					GC.WaitForPendingFinalizers();
					log.Verbose("GC.Collect()...");
					GC.Collect();

					log.InfoFormat("Capture meetings finished in {0} ms.\n", sw.Elapsed.TotalMilliseconds.ToString("0.000"));
				}
				catch (Exception ex)
				{
					log.Error("Finalization failed", ex);
				}
			}
		}

		public List<MeetingAttendee> DisplaySelectNamesDialog(IntPtr parentWindowHandle)
		{
			Redemption.RDOSession rdoSession = null;
			Redemption.RDOSelectNamesDialog rdoSelectNamesDialog = null;
			Redemption.RDORecipients rdoRecipients = null;
			var result = new List<MeetingAttendee>();

			log.Info("DisplaySelectNamesDialog started...");

			try
			{
				rdoSession = Redemption.RedemptionLoader.new_RDOSession();
				
				rdoSession.ParentWindow = parentWindowHandle;
				rdoSession.Logon();
				log.InfoFormat("Outlook Ver.: {0}, ProfileName: {1}, Redemption Ver.: {2}", rdoSession.OutlookVersion, rdoSession.ProfileName, rdoSession.Version);

				rdoSelectNamesDialog = rdoSession.GetSelectNamesDialog();
				
				rdoSelectNamesDialog.NumberOfRecipientSelectors = 1;
				rdoSelectNamesDialog.Display(true);

				rdoRecipients = rdoSelectNamesDialog.Recipients;
				
				result = GetMeetingData(rdoRecipients);
			}
			catch (Exception e)
			{
				log.Error("Error occured while displaying SelectNamesDialog.", e);
				throw;
			}
			finally
			{
				ReleaseComObject(rdoRecipients);
				ReleaseComObject(rdoSelectNamesDialog);
				rdoRecipients = null;
				rdoSelectNamesDialog = null;
				if (rdoSession != null)
					rdoSession.Logoff();

				ReleaseComObject(rdoSession);
				rdoSession = null;

				//If some of the objects remain referenced then this call to GC.Collect release those objects. If there are unreleased RCWs then threads with mso.dll does not disappear in Process Explorer.
				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();

				log.Info("DisplaySelectNamesDialog finished.");
			}

			return result;
		}

		private List<Redemption.RDOStore> GetAllStores(Redemption.RDOSession rdoSession)
		{
			log.Info("Get all stores started...");
			Stopwatch sw = Stopwatch.StartNew();

			Redemption.RDOStores rdoStores = null;
			List<Redemption.RDOStore> result = new List<Redemption.RDOStore>();

			try
			{
				rdoStores = rdoSession.Stores;
				// Debugging
				rdoObjects.Add(rdoStores);

				for (int i = 1; i <= rdoStores.Count; i++)
				{
					Redemption.RDOStore rdoStore = null;

					try
					{
						rdoStore = rdoStores[i];
						// Debugging
						rdoObjects.Add(rdoStore);
						result.Add(rdoStore);
					}
					catch (Exception e)
					{
						if (IsNecessaryToThrow(e))
						{
							ReleaseComObject(rdoStore);
							throw;
						}
						else log.Error("Error occured while getting a store.", e);
					}
				}
			}
			catch (Exception e)
			{
				if (IsNecessaryToThrow(e))
				{
					foreach (Redemption.RDOStore rdoStore in result) ReleaseComObject(rdoStore);
					throw;
				}
				else log.Error("Error occured while getting all stores.", e);
			}
			finally
			{
				ReleaseComObject(rdoStores);

				log.InfoFormat("Get all stores finished in {0} ms.", sw.Elapsed.TotalMilliseconds.ToString("0.000"));
			}

			return result;
		}

		private List<Redemption.RDOFolder> GetAllCalendarFolders(List<Redemption.RDOStore> stores, Redemption.RDOSession rdoSession)
		{
			log.Info("Get all calendar folders for all stores started...");
			Stopwatch sw = Stopwatch.StartNew();

			List<Redemption.RDOFolder> result = new List<Redemption.RDOFolder>();

			try
			{
				foreach (Redemption.RDOStore rdoStore in stores.Select(s => Measure(() => s, "stores.get[x]")))
				{
					try
					{
						log.InfoFormat("Processing message store: {0} ({1}, {2}, {3})", Measure(() => rdoStore.Name, "rdoStore.getName"), Measure(() => rdoStore.StoreKind.ToString(), "rdoStore.getStoreKind"), Measure(() => GetAccountType(rdoStore), "GetAccountType").ToString(), Measure(() => GetSmtpAddress(rdoStore, rdoSession), "GetSmtpAddress"));

						if (rdoStore.StoreKind == Redemption.TxStoreKind.skPublicFolders)
						{
							log.Info("This is an Exchange public folder store. We do not search calendar folders under it.");
							continue;
						}
						if (rdoStore.StoreKind == Redemption.TxStoreKind.skDelegateExchangeMailbox)
						{
							log.Info("This is an Exchange delegate mailbox store. We do not search calendar folders under it.");
							continue;
						}

						if (GetAccountType(rdoStore) == Redemption.rdoAccountType.atIMAP)
						{
							log.Info("This is a store related to an IMAP account. We do not search calendar folders under it.");
							continue;
						}

						List<Redemption.RDOFolder> calendarsInStore = GetAllFoldersByItemTypeFromCache(rdoStore, Redemption.rdoItemType.olAppointmentItem);

						foreach (Redemption.RDOFolder rdoFolder in calendarsInStore)
						{
							var folderPath = TryGetFolderPath(rdoFolder);
							var needed = 
								(folderInclusionRegex == null || !string.IsNullOrEmpty(folderPath) && folderInclusionRegex.IsMatch(folderPath))
								&& (folderExclusionRegex == null || !string.IsNullOrEmpty(folderPath) && !folderExclusionRegex.IsMatch(folderPath));
							log.InfoFormat("{0} calendar folder: {1}", needed ? "Founded" : "Skipped", folderPath);
							if (needed) result.Add(rdoFolder);
							else ReleaseComObject(rdoFolder);
						}

					}
					catch (Exception e)
					{
						if (IsNecessaryToThrow(e)) throw;
						else log.Error("Error occured while getting all calendar folders from a store.", e);
					}
				}
			}
			catch (Exception e)
			{
				if (IsNecessaryToThrow(e))
				{
					foreach (Redemption.RDOFolder rdoFolder in result) ReleaseComObject(rdoFolder);
					throw;
				}
				else log.Error("Error occured while getting all calendar folders from all stores.", e);
			}
			finally
			{
				log.InfoFormat("Get all calendar folders for all stores finished in {0} ms.", sw.Elapsed.TotalMilliseconds.ToString("0.000"));
			}

			return result;
		}

		private class CachedStoreKey : IEquatable<CachedStoreKey>
		{
			public string StoreEntryId { get; private set; }
			public Redemption.rdoItemType FolderItemType { get; private set; }

			public CachedStoreKey(string storeEntryId, Redemption.rdoItemType folderItemType)
			{
				StoreEntryId = storeEntryId;
				FolderItemType = folderItemType;
			}

			public override bool Equals(object obj)
			{
				return Equals(obj as CachedStoreKey);
			}

			public override int GetHashCode()
			{
				var result = 17;
				result = 31 * result + (StoreEntryId ?? "").GetHashCode();
				result = 31 * result + FolderItemType.GetHashCode();
				return result;
			}

			public bool Equals(CachedStoreKey other)
			{
				if (other == null) return false;
				return StoreEntryId == other.StoreEntryId
					&& FolderItemType == other.FolderItemType;
			}
		}

		private CachedDictionary<CachedStoreKey, IList<string>> storeFoldersByItemTypeCache = new CachedDictionary<CachedStoreKey, IList<string>>(TimeSpan.FromMinutes(60), true);

		private List<Redemption.RDOFolder> GetAllFoldersByItemTypeFromCache(Redemption.RDOStore rdoStore, Redemption.rdoItemType rdoItemTypeFilter)
		{
			IList<string> folderIds;
			List<Redemption.RDOFolder> result = new List<Redemption.RDOFolder>();
			var key = new CachedStoreKey(rdoStore.EntryID, rdoItemTypeFilter);

			if (!storeFoldersByItemTypeCache.TryGetValue(key, out folderIds))
			{
				result = GetAllFoldersByItemType(rdoStore, rdoItemTypeFilter);
				storeFoldersByItemTypeCache.Add(key, result.Select(n => n.EntryID).ToList());
				return result;
			}

			try
			{
				foreach (var folderId in folderIds)
				{
					var folder = rdoStore.GetFolderFromID(folderId);
					// Debugging
					rdoObjects.Add(folder);
					result.Add(folder);
				}
			}
			catch (Exception e)
			{
				log.Error("Get folder from id failed.", e);
				storeFoldersByItemTypeCache.Remove(key);
				foreach (Redemption.RDOFolder rdoFolder in result) ReleaseComObject(rdoFolder);
				result = GetAllFoldersByItemType(rdoStore, rdoItemTypeFilter);
				storeFoldersByItemTypeCache.Add(key, result.Select(n => n.EntryID).ToList());
				return result;
			}

			return result;
		}


		private List<Redemption.RDOFolder> GetAllFoldersByItemType(Redemption.RDOStore rdoStore, Redemption.rdoItemType rdoItemTypeFilter)
		{
			//log.Info("Get all folders by item type started...");
			//Stopwatch sw = Stopwatch.StartNew();

			List<Redemption.RDOFolder> result = new List<Redemption.RDOFolder>();
			Queue<Redemption.RDOFolder> rdoFoldersToProcess = new Queue<Redemption.RDOFolder>();

			try
			{
				Redemption.RDOFolder folder = rdoStore.IPMRootFolder;
				// Debug
				rdoObjects.Add(folder);
				rdoFoldersToProcess.Enqueue(folder); //This call make implicit RCW on stack, but it does not matter, becaus we will Release it by getting it form the list later.

				List<string> searchExceptions = GetSearchExceptions(rdoStore);

				while (rdoFoldersToProcess.Count > 0)
				{
					Redemption.RDOFolder rdoCurrentFolder = null;
					Redemption.RDOFolders rdoSubFolders = null;

					try
					{
						rdoCurrentFolder = rdoFoldersToProcess.Dequeue();
						log.Debug(TryGetFolderPath(rdoCurrentFolder));

						Redemption.rdoFolderKind folderKind;
						if (!TryGetFolderKind(rdoCurrentFolder, out folderKind) || folderKind == Redemption.rdoFolderKind.fkSearch
							|| searchExceptions.Contains(rdoCurrentFolder.EntryID))
						{
							ReleaseComObject(rdoCurrentFolder);
							continue;
						}

						rdoSubFolders = rdoCurrentFolder.Folders;
						// Debug
						rdoObjects.Add(rdoSubFolders);
						for (int i = 1; i <= rdoSubFolders.Count; i++)
						{
							var rdoFolder = rdoSubFolders[i];
							// Debug
							rdoObjects.Add(rdoFolder);
							rdoFoldersToProcess.Enqueue(rdoFolder);
						}
						ReleaseComObject(rdoSubFolders);	//This could be in a finally block, but it is good to release COM objects as soon as you are finished using them

						if ((Redemption.rdoItemType)rdoCurrentFolder.DefaultItemType == rdoItemTypeFilter)	//Remark: DefaultItemType can throw exception on "Sync Issues/Server Failures" folders when there is no exchange connection. 
						{
							result.Add(rdoCurrentFolder);
						}
						else
						{
							ReleaseComObject(rdoCurrentFolder);
						}
					}
					catch (Exception e)
					{
						ReleaseComObject(rdoSubFolders);
						result.Remove(rdoCurrentFolder);
						ReleaseComObject(rdoCurrentFolder);

						if (IsNecessaryToThrow(e)) throw;
						else log.Error("Error occured while processing a folder in the store: " + TryGetFolderPath(rdoCurrentFolder), e);
					}
				}
			}
			catch (Exception e)
			{
				if (IsNecessaryToThrow(e))
				{
					foreach (Redemption.RDOFolder rdoFolder in result) ReleaseComObject(rdoFolder);
					throw;
				}
				else log.Error("Error occured while iterating through folders in a store.", e);
			}
			finally
			{
				foreach (Redemption.RDOFolder rdoFolder in rdoFoldersToProcess) ReleaseComObject(rdoFolder);
				//log.InfoFormat("Get all folders by item type finished in {0} ms.", sw.Elapsed.TotalMilliseconds.ToString("0.000"));
			}

			return result;
		}

		private List<string> GetSearchExceptions(Redemption.RDOStore rdoStore)
		{
			List<string> searchExceptions = new List<string>();

			try
			{
				//Subfolders of "Sync Issues" folder can cause strange errors on Outlook 2013. (Hang on session release or MAPI_E_NOT_ENOUGH_MEMORY)
				if (rdoStore.StoreKind == Redemption.TxStoreKind.skPrimaryExchangeMailbox)		//When there is no exchange connection then "Sync Issues/Server Failures" folder is not reachable so iterating through folders hanging on that folder. So this is a hack for not searching under the "Sync Issues" folder.
				{
					Redemption.RDOFolder syncIssuesFolder = rdoStore.GetDefaultFolder(Redemption.rdoDefaultFolders.olFolderSyncIssues);
					// Debug
					rdoObjects.Add(syncIssuesFolder);
					string syncIssuesFolderEntryId = syncIssuesFolder.EntryID;
					searchExceptions.Add(syncIssuesFolderEntryId);
					ReleaseComObject(syncIssuesFolder);
				}
				return searchExceptions;
			}
			catch (Exception e)
			{
				log.Error("Error occured while getting search exceptions for a store.", e);
			}

			return searchExceptions;
		}

		private List<Redemption.RDOFolder> FilterCalendarFolders(List<Redemption.RDOFolder> calendarFolders, IList<string> calendarAccountEmails)
		{
			string PR_CAL_EMAIL = @"http://schemas.microsoft.com/mapi/string/{D900C334-08A3-4163-8654-2BDB59CBAB2C}/PR_CAL_EMAIL";	//This is a specific property exists only for calendars under an account created/synchronized by Google Apps Sync.

			List<Redemption.RDOFolder> result = new List<Redemption.RDOFolder>();

			foreach (Redemption.RDOFolder rdoFolder in calendarFolders)
			{
				string calendarEmail = null;
				try
				{
					calendarEmail = rdoFolder.get_Fields(PR_CAL_EMAIL) as string;
				}
				catch (Exception e)
				{
					if (IsNecessaryToThrow(e)) throw;
					else log.Error("Error occured while getting PR_CAL_EMAIL property on a folder.", e);
				}
				if (String.IsNullOrEmpty(calendarEmail) || calendarAccountEmails.Contains(calendarEmail, StringComparer.InvariantCultureIgnoreCase))
				{
					result.Add(rdoFolder);
				}
			}

			return result;
		}

		private List<Appointment> GetFinishedMeetings(List<Redemption.RDOFolder> calendarFolders, DateTime startDate, DateTime endDate, int manualWorkItemEditAgeLimit)
		{
			log.Info("Filter meetings in all calendar started...");
			Stopwatch sw = Stopwatch.StartNew();

			List<Appointment> result = new List<Appointment>();

			try
			{
				foreach (Redemption.RDOFolder rdoCalendarFolder in calendarFolders)
				{
					try
					{
						result.AddRange(GetFinishedMeetings(rdoCalendarFolder, startDate, endDate, manualWorkItemEditAgeLimit));
					}
					catch (Exception e)
					{
						if (IsNecessaryToThrow(e)) throw;
						else log.Error("Error occured while getting finished meetings from a folder: " + TryGetFolderPath(rdoCalendarFolder), e);
					}
				}
			}
			catch (Exception e)
			{
				if (IsNecessaryToThrow(e)) throw;
				else log.Error("Error occured while processing calendar folders.", e);
			}
			finally
			{
				log.InfoFormat("Filter meetings in all calendar finished in {0} ms.", sw.Elapsed.TotalMilliseconds.ToString("0.000"));
			}

			return result;
		}

		private List<Appointment> GetFinishedMeetings(Redemption.RDOFolder rdoCalendarFolder, DateTime startDate, DateTime endDate, int manualWorkItemEditAgeLimit)
		{
			if (rdoCalendarFolder == null) throw new ArgumentNullException("rdoCalendarFolder");
			if (startDate == null) throw new ArgumentNullException("startDate");
			if (endDate == null) throw new ArgumentNullException("endDate");
			if (rdoCalendarFolder.DefaultItemType != (int)Redemption.rdoItemType.olAppointmentItem) throw new ArgumentException("The given folder's default item type must be appointment item.");
			if (startDate > endDate) throw new ArgumentException("Start date must be less or equal than end date.");

			List<Appointment> modifiedInRangeMeetings = new List<Appointment>();
			Redemption.RDOItems rdoAppointmentItemsInRange = null;
			var backwardCreated = startDate.AddHours(-manualWorkItemEditAgeLimit);
			var syncEndDate = endDate.AddDays(futureSyncDays);
			var folderPath = TryGetFolderPath(rdoCalendarFolder);
			var folderId = rdoCalendarFolder.EntryID;
			DateTime lastModificationDate;
			if (!messageStore.LastModificationDates.TryGetValue(folderId, out lastModificationDate) || lastModificationDate > startDate) lastModificationDate = startDate;
			Dictionary<string, MeetingData> existingItemsInFolder;
			if (!messageStore.MeetingDatas.TryGetValue(folderId, out existingItemsInFolder))
			{
				if (veryOldExistingItems != null)
				{
					existingItemsInFolder = veryOldExistingItems.ToDictionary(o => o.Key, o => new MeetingData{ StartTime = o.Value });
					veryOldExistingItems = null;
					log.InfoFormat("New folder store initialized from old store {0}", folderId);
				}
				else
				{
					existingItemsInFolder = new Dictionary<string, MeetingData>();
					log.InfoFormat("New folder store initialized as empty {0}", folderId);
				}
				messageStore.MeetingDatas[folderId] = existingItemsInFolder;
			}
			Dictionary<string, DateTime> existingItemsFirstSeen;
			if (messageStore.ItemFirstSeen.TryGetValue(folderId, out existingItemsFirstSeen))
			{
				var toDelete = existingItemsFirstSeen.Where(i => i.Value >= startDate).Select(i => i.Key).ToList();
				foreach (var key in toDelete)
				{
					existingItemsInFolder.Remove(key);
				}
			}
			else
			{
				existingItemsFirstSeen = new Dictionary<string, DateTime>();
				messageStore.ItemFirstSeen[folderId] = existingItemsFirstSeen;
			}

			if (!messageStore.ItemsToBeDeleted.TryGetValue(folderId, out var itemsToBeDeleted))
			{
				itemsToBeDeleted = new Dictionary<string, DateTime>();
				messageStore.ItemsToBeDeleted.Add(folderId, itemsToBeDeleted);
			}
			var deleted = existingItemsInFolder.Where(i => i.Value.StartTime >= backwardCreated).ToDictionary(k => k.Key, v => v.Value);

			var idx = -1;

			try
			{
				log.InfoFormat("Processing calendar folder path: {0}, id: {1}, backwardCreated:{2}, syncEndDate:{3}, lastModificationDate:{4}", folderPath, folderId, backwardCreated, syncEndDate, lastModificationDate);

				//1. Query meetings that ended in the given time interval
				Redemption.RDOFolder2 rdoCalendarFolder2 = (Redemption.RDOFolder2)rdoCalendarFolder;	//This does not return new COM object. This is an input parameter so it does not need to be realesed here.
				rdoAppointmentItemsInRange = rdoCalendarFolder2.GetActivitiesForTimeRange(backwardCreated, syncEndDate, true); //Retrieves recurring meetings too
				// Debug
				rdoObjects.Add(rdoAppointmentItemsInRange);
				var existingItemsInFolderCopy = existingItemsInFolder.ToDictionary(i => i.Key, i => i.Value.StartTime);
				for (idx = 1; idx <= rdoAppointmentItemsInRange.Count; idx++)
				{
					Redemption.RDOAppointmentItem rdoAppointmentItem = null;
					try
					{
						var rdoItem = rdoAppointmentItemsInRange[idx];
						// Debug
						rdoObjects.Add(rdoItem);
						rdoAppointmentItem = (Redemption.RDOAppointmentItem)rdoItem;
						var hasUsed = false;
						if ((rdoAppointmentItem.Sensitivity == (int)Redemption.rdoSensitivity.olNormal) &&
							(rdoAppointmentItem.MeetingStatus == Redemption.rdoMeetingStatus.olMeeting
							|| rdoAppointmentItem.MeetingStatus == Redemption.rdoMeetingStatus.olNonMeeting && needNonMeetingAppointments
							|| rdoAppointmentItem.MeetingStatus == Redemption.rdoMeetingStatus.olMeetingReceived) &&
							(rdoAppointmentItem.ResponseStatus == Redemption.rdoResponseStatus.olResponseAccepted || rdoAppointmentItem.ResponseStatus == Redemption.rdoResponseStatus.olResponseOrganized || needTentativeMeetings && (rdoAppointmentItem.ResponseStatus == Redemption.rdoResponseStatus.olResponseNotResponded || rdoAppointmentItem.ResponseStatus == Redemption.rdoResponseStatus.olResponseTentative)
							|| (rdoAppointmentItem.MeetingStatus == Redemption.rdoMeetingStatus.olNonMeeting && rdoAppointmentItem.ResponseStatus == Redemption.rdoResponseStatus.olResponseNone && needNonMeetingAppointments)
							))
						{
							string entryId;
							// in case of exception IsRecurring is not set with Outlook 2010
							if (rdoAppointmentItem.IsRecurring || rdoAppointmentItem.RecurrenceState != Redemption.rdoRecurrenceState.olApptNotRecurring)
							{
								Redemption.RDORecurrencePattern pattern = null;
								Redemption.RDOExceptions exceptions = null;
								List<Redemption.IRDOException> exceptionList = null;
								try
								{
									pattern = rdoAppointmentItem.GetRecurrencePattern();
									// Debug
									rdoObjects.Add(pattern);
									exceptions = pattern.Exceptions;
									// Debug
									rdoObjects.Add(exceptions);
									exceptionList = new List<Redemption.IRDOException>();
									var startEvent = rdoAppointmentItem.Start;
									foreach (Redemption.IRDOException rdoException in exceptions)
									{
										rdoObjects.Add(rdoException);
										exceptionList.Add(rdoException);
										if (rdoException.StartDateTime == rdoAppointmentItem.Start)
										{
											startEvent = rdoException.OriginalStartDate;
											break;
										}
									}
									//var startEvent = (exceptionList.Where(e => e.StartDateTime == rdoAppointmentItem.Start).Select(e => (DateTime?) e.OriginalStartDate).FirstOrDefault() ?? (DateTime?) rdoAppointmentItem.Start).Value;
									entryId = rdoAppointmentItem.GlobalAppointmentID + startEvent.ToBinary();
								}
								finally
								{
									if (exceptionList != null)
										foreach (var rdoException in exceptionList)
										{
											ReleaseComObject(rdoException);
										}
									ReleaseComObject(exceptions);
									ReleaseComObject(pattern);
								}
							}
							else
								entryId = rdoAppointmentItem.GlobalAppointmentID;

							var data = new MeetingData
							{
								Title = rdoAppointmentItem.Subject,
								Description = rdoAppointmentItem.Body,
								CreationTime = rdoAppointmentItem.CreationTime,
								Location = rdoAppointmentItem.Location,
								StartTime = rdoAppointmentItem.Start,
								EndTime = rdoAppointmentItem.End,
								Attendees = new HashSet<string>(rdoAppointmentItem.Recipients.OfType<Redemption.RDORecipient>().Select(GetSmtpAddress)),
							};
							if (// checking just finished meetings start
								rdoAppointmentItem.End > startDate && rdoAppointmentItem.End <= endDate
								// checking just finished meetings end

								// checking modified meetings start
								|| rdoAppointmentItem.LastModificationTime > lastModificationDate && rdoAppointmentItem.LastModificationTime <= endDate
									&& rdoAppointmentItem.End > backwardCreated && rdoAppointmentItem.End <= syncEndDate
									&& (!existingItemsInFolder.TryGetValue(entryId, out var found) || !found.Equals(data)))
								// checking modified meetings end
							{
								var nowFinished = rdoAppointmentItem.End > startDate && rdoAppointmentItem.End <= endDate;
								DateTime oldStart;
								var isNew = !existingItemsInFolderCopy.TryGetValue(entryId, out oldStart);
								if (needUpdatesDeletes || isNew)
								{
									hasUsed = true;
									modifiedInRangeMeetings.Add(new Appointment { Item = rdoAppointmentItem, OldStartDateTime = oldStart, Status = isNew ? MeetingCrudStatus.Created : MeetingCrudStatus.Updated, EntryId = entryId });
									if (!existingItemsFirstSeen.ContainsKey(entryId)) existingItemsFirstSeen[entryId] = startDate;
								}
								if (nowFinished && !isNew)
								{
									hasUsed = true;
									modifiedInRangeMeetings.Add(new Appointment { Item = rdoAppointmentItem, Status = MeetingCrudStatus.Created, EntryId = entryId });
									if (!existingItemsFirstSeen.ContainsKey(entryId)) existingItemsFirstSeen[entryId] = startDate;
								}
								existingItemsInFolder[entryId] = data;
							}

							deleted.Remove(entryId);
						}
						if (!hasUsed)
						{
							ReleaseComObject(rdoAppointmentItem);
						}
					}
					catch (Exception e)
					{
						modifiedInRangeMeetings.RemoveAll(t => t.Item == rdoAppointmentItem);
						ReleaseComObject(rdoAppointmentItem);

						if (IsNecessaryToThrow(e)) throw;
						else log.Error("Error occured while processing an appointment item (in first query).", e);
					}
				}
				// 1. register newly deleted events
				deleted.Keys.Except(itemsToBeDeleted.Keys).ToList().ForEach(k => itemsToBeDeleted.Add(k, DateTime.UtcNow));
				// 2. registered and not too old events can't be deleted
				itemsToBeDeleted.Where(i => i.Value + delayedDeleteInterval > DateTime.UtcNow).ToList().ForEach(i => deleted.Remove(i.Key));
				// 3. remove all metadata about really deleted events (also delete registration)
				deleted.Keys.ToList().ForEach(k => { existingItemsInFolder.Remove(k); existingItemsFirstSeen.Remove(k); itemsToBeDeleted.Remove(k); });
				// 4. remove unnecessary delete registrations
				itemsToBeDeleted.Where(i => i.Value + forgetDelayedDeleteItemsAfter <= DateTime.UtcNow).ToList().ForEach(i => itemsToBeDeleted.Remove(i.Key));
			}
			catch (Exception e)
			{
				if (IsNecessaryToThrow(e) || idx < 0) // throw exception also if processing not started yet 
				{
					foreach (var apnt in modifiedInRangeMeetings.Select(m => m.Item).Distinct()) ReleaseComObject(apnt);
					throw;
				}
				else log.Error("Error occured while filtering for finished meetings in a folder: " + folderPath, e);
			}
			finally
			{
				ReleaseComObject(rdoAppointmentItemsInRange);
			}

			List<Appointment> result = new List<Appointment>();
			result.AddRange(modifiedInRangeMeetings);
			if (needUpdatesDeletes)
				result.AddRange(deleted.Select(d => new Appointment { Status = MeetingCrudStatus.Deleted, OldStartDateTime = d.Value.StartTime, LastModificationOverride = endDate, EntryId = d.Key }));
			if (modifiedInRangeMeetings.Count > 0)
			{
				lastModificationDate = modifiedInRangeMeetings.Max(m => m.Item.LastModificationTime);
				if (!messageStore.LastModificationDates.ContainsKey(folderId) || messageStore.LastModificationDates[folderId] < lastModificationDate)
					messageStore.LastModificationDates[folderId] = lastModificationDate;
			}

			return result;
		}

		#region Mapping RDOAppointmentItem to FinishedMeetingEntry

		private List<FinishedMeetingEntry> GetMeetingData(List<Appointment> rdoMeetings)
		{
			log.Info("Get data from matched meetings started...");
			Stopwatch sw = Stopwatch.StartNew();

			List<FinishedMeetingEntry> result = new List<FinishedMeetingEntry>();

			try
			{
				foreach (var meeting in rdoMeetings)
				{
					try
					{
						FinishedMeetingEntry finishedmeetingEntry = GetMeetingData(meeting);
						result.Add(finishedmeetingEntry);
					}
					catch (Exception e)
					{
						if (IsNecessaryToThrow(e)) throw;
						else log.Error("Error occured while processing a meeting.", e);
					}
				}
			}
			catch (Exception e)
			{
				if (IsNecessaryToThrow(e)) throw;
				else log.Error("Error occured while processing meetings.", e);
			}
			finally
			{
				log.Info(String.Format("Get data from matched meetings finished in {0} ms.", sw.Elapsed.TotalMilliseconds.ToString("0.000")));
			}

			return result;
		}

		private FinishedMeetingEntry GetMeetingData(Appointment apnt)
		{
			Redemption.RDOAppointmentItem rdoMeeting = apnt.Item;
			Redemption.RDORecipients rdoRecipients = null;

			FinishedMeetingEntry meetingEntry = new FinishedMeetingEntry { OldStartTime = apnt.OldStartDateTime.HasValue ? apnt.OldStartDateTime.Value.ToUniversalTime() : (DateTime?)null, LastmodificationTime = apnt.LastModificationOverride.HasValue ? apnt.LastModificationOverride.Value.ToUniversalTime() : DateTime.MinValue, Status = apnt.Status, Id = apnt.EntryId };

			if (rdoMeeting == null) return meetingEntry;

			try
			{
				meetingEntry.Id = apnt.EntryId ?? rdoMeeting.GlobalAppointmentID;	//TODO: At some rare situations this id is null. This could be happen on an unsaved meetingitem, but it should not retrieved by GetFinishedmeetings.
				meetingEntry.CreationTime = rdoMeeting.CreationTime.ToUniversalTime();
				meetingEntry.LastmodificationTime = (apnt.LastModificationOverride ?? rdoMeeting.LastModificationTime).ToUniversalTime();
				meetingEntry.Title = rdoMeeting.Subject;
				meetingEntry.Description = rdoMeeting.Body;
				meetingEntry.Location = rdoMeeting.Location;
				meetingEntry.StartTime = rdoMeeting.Start.ToUniversalTime();
				meetingEntry.EndTime = rdoMeeting.End.ToUniversalTime();
				//meetingEntry.ResponseStatus = (MeetingAttendeeResponseStatus)(int)meeting.ResponseStatus;

				//meetingEntry.IsRecurring = rdoMeeting.IsRecurring;	//TODO: Add IsRecurring to datacontract
				//if (!TryGetTaskId(meetingEntry.Description, out meetingEntry.TaskId)) TryGetTaskId(meetingEntry.Title, out meetingEntry.TaskId);	//TODO: Add TaskId to datacontract
				const int maxDescriptionLength = 8000;
				if (meetingEntry.Description != null && meetingEntry.Description.Length > maxDescriptionLength) meetingEntry.Description = meetingEntry.Description.Substring(0, maxDescriptionLength);

				if (String.IsNullOrEmpty(meetingEntry.Id)) log.ErrorFormat("Meeting with empty GlobalAppointmentID. (Title:{0}, Start: {1})", meetingEntry.Title, meetingEntry.StartTime);

				rdoRecipients = rdoMeeting.Recipients;
				// Debug
				rdoObjects.Add(rdoRecipients);
				meetingEntry.Attendees = GetMeetingData(rdoRecipients);

				//Getting organizer if recipients does not contain it already.
				if (meetingEntry.Attendees.Count(a => a.Type == MeetingAttendeeType.Organizer) == 0)
				{
					string organizerSmtpAddress = GetOrganizerSmtpAddress(rdoMeeting);
					MeetingAttendee attendee = meetingEntry.Attendees.FirstOrDefault(a => String.Equals(a.Email, organizerSmtpAddress, StringComparison.InvariantCultureIgnoreCase));
					if (attendee != null)
					{
						attendee.Type = MeetingAttendeeType.Organizer;
						attendee.ResponseStatus = MeetingAttendeeResponseStatus.ResponseOrganized;
					}
					else
					{
						MeetingAttendee organizer = new MeetingAttendee() { Email = organizerSmtpAddress, Type = MeetingAttendeeType.Organizer, ResponseStatus = MeetingAttendeeResponseStatus.ResponseOrganized };
						meetingEntry.Attendees.Add(organizer);
					}
				}

				//Getting AccountEmail for meeting directly is not possible:
				// -Account property returns false acount when meetings received by an IMAP mailbox created in the default calendar under another mailbox.
				// -ReceivedBy properties are not set on an AppointmentItem object, just on meeting request objects. But theese objects ca not be retrived from AppointmentItem object.
			}
			catch (Exception e)
			{
				if (IsNecessaryToThrow(e)) throw;
				else log.Error("Error occured while processing a meeting.", e);
			}
			finally
			{
				ReleaseComObject(rdoRecipients);
			}

			return meetingEntry;
		}

		private static bool TryGetTaskId(string text, out int? taskId)
		{
			const string taskIdPattern = @"\[\s*JobCTRL\s*\#\(?<taskid>d+)\s*\]";

			Match m = Regex.Match(text, taskIdPattern);
			if (m.Success)
			{
				string taskIdStr = m.Groups["taskid"].Value;
				int tid = 0;
				if (Int32.TryParse(taskIdStr, out tid))
				{
					taskId = tid;
					return true;
				}
			}

			taskId = null;
			return false;
		}

		private List<MeetingAttendee> GetMeetingData(Redemption.RDORecipients rdoRecipients)
		{
			List<MeetingAttendee> result = new List<MeetingAttendee>();

			try
			{
				for (int i = 1; i <= rdoRecipients.Count; i++)
				{
					Redemption.RDORecipient rdoRecipient = null;
					try
					{
						rdoRecipient = rdoRecipients[i];
						//Debug
						rdoObjects.Add(rdoRecipient);
						MeetingAttendee attendee = GetMeetingData(rdoRecipient);
						result.Add(attendee);
					}
					catch (Exception e)
					{
						if (IsNecessaryToThrow(e)) throw;
						else log.Error("Creating attendee from a recipient failed.", e);
					}
					finally
					{
						ReleaseComObject(rdoRecipient);
					}
				}
			}
			catch (Exception e)
			{
				if (IsNecessaryToThrow(e)) throw;
				else log.Error("Error occured while processing recipients.", e);
			}

			return result;
		}

		private MeetingAttendee GetMeetingData(Redemption.RDORecipient rdoRecipient)
		{
			string PR_RECIPIENT_FLAGS = @"http://schemas.microsoft.com/mapi/proptag/0x5FFD0003";
			int RECIP_ORGANIZER = 0x2;

			MeetingAttendee attendee = new MeetingAttendee() { Type = MeetingAttendeeType.Required };	//This is the default attendee that is returned when any error occurs.

			try
			{
				int recipientFlags = (int)(rdoRecipient.get_Fields(PR_RECIPIENT_FLAGS) ?? 0);
				bool isOrganizerByRecipientFlag = (recipientFlags & RECIP_ORGANIZER) == RECIP_ORGANIZER;
				bool isOrganizerByMeetingResponseStatus = rdoRecipient.MeetingResponseStatus == (int)Redemption.rdoResponseStatus.olResponseOrganized;
				bool isOrganizer = isOrganizerByRecipientFlag || isOrganizerByMeetingResponseStatus;

				attendee.Email = GetSmtpAddress(rdoRecipient);
				attendee.Type = isOrganizer ? MeetingAttendeeType.Organizer : (MeetingAttendeeType)rdoRecipient.Type;	//TODO: Implement explicit mapping
				attendee.ResponseStatus = (MeetingAttendeeResponseStatus)(int)rdoRecipient.MeetingResponseStatus;	//TODO: Implement explicit mapping
			}
			catch (Exception e)
			{
				if (IsNecessaryToThrow(e)) throw;
				else log.Error("Getting recipient properties failed.", e);
			}

			return attendee;
		}

		#endregion

		#region Smtp email address handling

		private string GetSmtpAddress(Redemption.RDOStore rdoStore, Redemption.RDOSession rdoSession)
		{
			Redemption.RDOAccount rdoAccount = null;

			try
			{
				rdoAccount = rdoStore.StoreAccount;
				// Debug
				rdoObjects.Add(rdoAccount);
				return rdoAccount != null ? GetSmtpAddress(rdoAccount, rdoSession) : null;
			}
			catch (Exception)
			{
				log.Error("Getting smtp email address for store failed.");
				throw;
			}
			finally
			{
				ReleaseComObject(rdoAccount);
			}
		}

		private string GetSmtpAddress(Redemption.RDOAccount rdoAccount, Redemption.RDOSession rdoSession)
		{
			try
			{
				switch (rdoAccount.AccountType)
				{
					case Redemption.rdoAccountType.atExchange:
						return GetSmtpAddress((Redemption.RDOExchangeAccount)rdoAccount);
					case Redemption.rdoAccountType.atPOP3:
						return ((Redemption.RDOPOP3Account)rdoAccount).SMTPAddress;
					case Redemption.rdoAccountType.atIMAP:
						return ((Redemption.RDOIMAPAccount)rdoAccount).SMTPAddress;
					case Redemption.rdoAccountType.atMAPI:
						return GetSmtpAddress((Redemption.RDOMAPIAccount)rdoAccount, rdoSession);
					case Redemption.rdoAccountType.atHTTP:
						return ((Redemption.RDOHTTPAccount)rdoAccount).SMTPAddress;
					case Redemption.rdoAccountType.atLDAP:
						return null;
					case Redemption.rdoAccountType.atOther:
						return null;
					case Redemption.rdoAccountType.atPST:
						return null;
					default:
						return null;
				}
			}
			catch (Exception)
			{
				log.Error("Getting smtp email address for account failed.");
				throw;
			}
		}

		private string GetSmtpAddress(Redemption.RDOExchangeAccount rdoExchangeAccount)
		{
			Redemption.RDOAddressEntry rdoAddressEntry = null;
			try
			{
				//Get smtp address from the related address entry.
				//When there is no exchange connection:
				// -Work Offline is switched off: User property throws COMException (MAPI_E_NETWORK_ERROR) but it takes much time (30 sec or more). 
				// -Work Offline is switched on: User property returns the address entry, but it's SMTPAddress property will be null.
				rdoAddressEntry = rdoExchangeAccount.User;
				// Debug
				rdoObjects.Add(rdoAddressEntry);
				string smtpAddress = rdoAddressEntry.SMTPAddress;

				if (!String.IsNullOrEmpty(smtpAddress)) return smtpAddress;
				else throw new SmtpAddressNotFoundException("Getting smtp address for an address entry failed. Probably 'Work Offline' mode is switched on in Outlook and the given address entry is in the Global Address List which is not cached locally.");
			}
			catch (Exception)
			{
				log.Error("Getting smtp email address for an Exchange account failed.");
				throw;
			}
			finally
			{
				ReleaseComObject(rdoAddressEntry);
			}

			//Alternative solution 1: Try to get smtp address from global profile section (string PROP_EXCHANGE_EMAILID = @"http://schemas.microsoft.com/mapi/proptag/0x663D001F";)
			//Is this work in general? Is it work when there are multiple EX account in the profile (in Outlook 2010/2013)?
			//TODO

			//Alternative solution 2: Try to interpret the related store's name as an SMTP address.
			//This is an uggly hack, but usually name of the store (related to Exchange account) is the primary smtp address for Exchange account.
			//this method can give a false result, if user rename the store to another valid smtp address.
			//Redemption.RDOExchangeMailboxStore primaryStore = null;
			//try
			//{
			//    primaryStore = rdoExchangeAccount.PrimaryStore;
			//    string storeName = primaryStore.Name;
			//    if (IsValidEmail(storeName)) return storeName;
			//}
			//catch (Exception ex)
			//{
			//    Debug.Print(ex.Message);
			//}
			//finally
			//{
			//    ReleaseComObject(primaryStore);
			//}
		}

		private string GetSmtpAddress(Redemption.RDOMAPIAccount rdoMAPIAccount, Redemption.RDOSession rdoSession)
		{
			int PROP_MAPI_IDENTITY_ENTRYID = 0x20020102;
			Redemption.RDOAddressEntry rdoAddressEntry = null;

			try
			{
				string identityEntryid = rdoMAPIAccount.get_Fields(PROP_MAPI_IDENTITY_ENTRYID) as string;
				if (!String.IsNullOrEmpty(identityEntryid))
				{
					rdoAddressEntry = rdoSession.GetAddressEntryFromID(identityEntryid);
					// Debug
					rdoObjects.Add(rdoAddressEntry);
					string smtpAddress = rdoAddressEntry.SMTPAddress;
					if (!String.IsNullOrEmpty(smtpAddress)) return smtpAddress;
					else throw new SmtpAddressNotFoundException("Getting smtp address for an address entry failed. Probably 'Work Offline' mode is switched on in Outlook and the given address entry is in the Global Address List which is not cached locally.");
				}
				else return null;
			}
			catch (Exception)
			{
				log.Error("Getting smtp email address for a MAPI account failed.");
				throw;
			}
			finally
			{
				ReleaseComObject(rdoAddressEntry);
			}
		}

		private string GetOrganizerSmtpAddress(Redemption.RDOAppointmentItem rdoMeeting)
		{
			//We assume that the sender of the meeting who is the organizer. Actually the user that the meeting is sent on behalf of.
			//If a sending mailbox owner is sending on his or her own behalf, than PidTagSentRepresenting<...> properteis are set to the value of the corresponding PidTagSender<...> property.	(http://msdn.microsoft.com/en-us/library/ee124379%28v=exchg.80%29.aspx)
			//RDOMail SentOnBehalfOf<...> properties correspond to the MAPI property PR_SENT_REPRESENTING<...>.		(http://www.dimastr.com/redemption/RDOMail.htm)

			Redemption.RDOAddressEntry rdoAddressEntry = null;

			try
			{
				string addressType = rdoMeeting.SentOnBehalfOfEmailType;
				if (String.Equals(addressType, "SMTP", StringComparison.InvariantCultureIgnoreCase))
				{
					string smtpAddress = rdoMeeting.SentOnBehalfOfEmailAddress;
					if (!String.IsNullOrEmpty(smtpAddress)) return smtpAddress;
					else throw new SmtpAddressNotFoundException("Sent representing email address of the meeting is null or empty.");
				}
				else if (String.Equals(addressType, "EX", StringComparison.InvariantCultureIgnoreCase))
				{
					rdoAddressEntry = rdoMeeting.SentOnBehalfOf;
					// Debug
					rdoObjects.Add(rdoAddressEntry);
					string smtpAddress = rdoAddressEntry.SMTPAddress;
					if (!String.IsNullOrEmpty(smtpAddress)) return smtpAddress;
					else throw new SmtpAddressNotFoundException("Getting smtp address for an address entry failed. Probably 'Work Offline' mode is switched on in Outlook and the given address entry is in the Global Address List which is not cached locally.");
				}
				else throw new SmtpAddressNotFoundException("Email type is neither SMTP nor EX. " + "(" + addressType + ")");
			}
			catch (Exception e)
			{
				if (IsNecessaryToThrow(e)) throw;
				else
				{
					log.Error("Getting smtp email address of the sender from meeting item failed.", e);
					return unknownEmail;
				}
			}
			finally
			{
				ReleaseComObject(rdoAddressEntry);
			}
		}

		private string GetSmtpAddress(Redemption.RDORecipient rdoRecipient)
		{
			string PR_ADDRTYPE = @"http://schemas.microsoft.com/mapi/proptag/0x3002001E";
			string PR_SMTP_ADDRESS = @"http://schemas.microsoft.com/mapi/proptag/0x39FE001E";

			//1. Getting smtp email address from recipient.
			try
			{
				string addressType = rdoRecipient.get_Fields(PR_ADDRTYPE) as string;
				if (String.Equals(addressType, "SMTP", StringComparison.InvariantCultureIgnoreCase))
				{
					string smtpAddress = rdoRecipient.Address;
					if (!String.IsNullOrEmpty(smtpAddress)) return smtpAddress;
					else throw new SmtpAddressNotFoundException("Recipient's address property is empty or null.");
				}
				else if (String.Equals(addressType, "EX", StringComparison.InvariantCultureIgnoreCase))
				{
					string smtpAddress = rdoRecipient.get_Fields(PR_SMTP_ADDRESS) as string;
					if (!String.IsNullOrEmpty(smtpAddress)) return smtpAddress;
					else log.Error("PR_SMTP_ADDRESS is empty or null. " + "(" + rdoRecipient.Address + ")");

					return GetSmtpAddressFromRelatedAddressEntry(rdoRecipient);
				}
				else throw new SmtpAddressNotFoundException("Email type is neither SMTP nor EX. " + "(" + addressType + ")");
			}
			catch (Exception e)
			{
				if (IsNecessaryToThrow(e)) throw;
				else
				{
					log.Error("Getting smtp email address from recipient failed.", e);
					return unknownEmail;
				}
			}
		}

		private string GetSmtpAddressFromRelatedAddressEntry(Redemption.RDORecipient rdoRecipient)
		{
			//2. Get smtp email address from the related address entry.
			//When there is no exchange connection:
			// -Work Offline is switched off: AddressEntry property throws COMException (MAPI_E_NETWORK_ERROR) but it takes much time (30 sec or more). 
			// -Work Offline is switched on: AddressEntry property returns the address entry, but it's SMTPAddress property will be null.
			Redemption.IRDOAddressEntry rdoAddressEntry = null;
			try
			{
				rdoAddressEntry = rdoRecipient.AddressEntry;
				// Debug
				rdoObjects.Add(rdoAddressEntry);
				string smtpAddress = rdoAddressEntry.SMTPAddress;
				if (!String.IsNullOrEmpty(smtpAddress)) return smtpAddress;
				else throw new SmtpAddressNotFoundException("Getting smtp address for an address entry failed. Probably 'Work Offline' mode is switched on in Outlook and the given address entry is in the Global Address List which is not cached locally.");
			}
			catch (Exception e)
			{
				if (IsNecessaryToThrow(e)) throw;
				else
				{
					log.Error("Getting smtp email address from address entry failed.", e);
					return unknownEmail;
				}
			}
			finally
			{
				ReleaseComObject(rdoAddressEntry);
			}
		}

		const uint MAPI_E_NETWORK_ERROR = 0x80040115;
		const uint MAPI_E_DISK_ERROR = 0x80040116;
		const uint MAPI_E_INVALID_ENTRYID = 0x80040107;
		const uint MAPI_E_UNEXPECTED = 0x8000FFFF;
		private bool IsNecessaryToThrow(Exception e)
		{
			bool result = false;

			if (e is COMException)
			{
				COMException ee = e as COMException;
				if (ee.ErrorCode == unchecked((int)MAPI_E_NETWORK_ERROR) || ee.ErrorCode == unchecked((int)MAPI_E_INVALID_ENTRYID)
				                                                         || ee.ErrorCode == unchecked((int)MAPI_E_DISK_ERROR) 
				                                                         || ee.ErrorCode == unchecked((int)MAPI_E_UNEXPECTED)) result = true;
			}

			return result;
		}

		private bool IsValidEmail(string emailAddress)
		{
			string emailPattern = @"(?=^.{1,100}$)^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,5}$";
			return Regex.IsMatch(emailAddress, emailPattern, RegexOptions.IgnoreCase);
		}

		#endregion

		private Redemption.rdoAccountType GetAccountType(Redemption.RDOStore rdoStore)
		{
			Redemption.RDOAccount rdoAccount = null;
			try
			{
				rdoAccount = rdoStore.StoreAccount;
				// Debug
				rdoObjects.Add(rdoAccount);
				return rdoAccount != null ? rdoAccount.AccountType : Redemption.rdoAccountType.atOther;
			}
			finally
			{
				ReleaseComObject(rdoAccount);
			}
		}

        private static T Measure<T>(Func<T> inner, string name)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                return inner();
            }
            finally
            {
                log.Debug($@"Method ({name ?? inner.Method.Name}) finished in {sw.ElapsedMilliseconds} ms");
            }
        }

        private static void Measure(Action inner, string name)
        {
            Measure(() => { inner(); return true; }, name);
        }

        private string TryGetFolderPath(Redemption.RDOFolder rdoFolder)
		{
			try
			{
				Redemption.rdoFolderKind folderKind;
				if (TryGetFolderKind(rdoFolder, out folderKind) && folderKind == Redemption.rdoFolderKind.fkSearch) return String.Empty;
				Redemption.RDOFolder2 rdoFolder2 = (Redemption.RDOFolder2)rdoFolder;
				return rdoFolder2.FolderPath;
			}
			catch (Exception e)
			{
				if (IsNecessaryToThrow(e)) throw;
				log.Error("Error occured while try to get folder path.", e);
				return String.Empty;
			}
		}

		private static bool TryGetFolderKind(Redemption.RDOFolder rdoFolder, out Redemption.rdoFolderKind folderKind)
		{
			try
			{
				folderKind = rdoFolder.FolderKind;
				return true;
			}
			catch (Exception e)
			{
				log.Error("Error occured while try to get folder kind.", e);
				folderKind = 0;
				return false;
			}
		}

		private void ReleaseComObject(object o)
		{
			try
			{
				if (o != null)
				{
					//Debug
					if (rdoObjects.Contains(o))
						rdoObjects.Remove(o);
					else
						log.DebugFormat("rdoObjects doesn't contains element: Type: {0}; toString: {1}", TypeInformation.GetTypeName(o), o.ToString());

					int rc = Marshal.ReleaseComObject(o);
					if (rc != 0)
						log.DebugFormat("Remaining reference after ReleaseComObject. rc counter value: {0}; Object Type: {1}; ToString: {2};", rc, TypeInformation.GetTypeName(o), o.ToString());
					//Debug.Assert(rc == 0);	//For example static COM objects will map to the same RCW, so RCW's reference count can be more than one.
				}
			}
			catch (Exception e)
			{
				log.Error("Error occured while releasing COM object.", e);
			}
		}

#if DEBUG
		private List<FinishedMeetingEntry> CreateDummyMeetings(int numberOfMeetingsToCreate)
		{
			List<FinishedMeetingEntry> result = new List<FinishedMeetingEntry>();

			for (int i = 1; i <= numberOfMeetingsToCreate; i++)
			{
				FinishedMeetingEntry meetingEntry = new FinishedMeetingEntry();

				meetingEntry.Id = "040000008200E00074C5B7101A82E0080000000000000000000000000000000000000000320000007643616C2D556964010000007672706C34687074676A3174763433713766617275673166326740676F6F676C652E636F6D00";
				meetingEntry.Id += "_" + i.ToString();
				meetingEntry.CreationTime = new DateTime(2013, 1, 30, 14, 27, 00);
				meetingEntry.LastmodificationTime = new DateTime(2013, 1, 31, 12, 52, 0);
				meetingEntry.Title = "Teszt meeting";
				//meetingEntry.Description = "\r\nmore details  <https://www.google.com/calendar/event?action=VIEW&eid=dnJwbDRocHRnajF0djQzcTdmYXJ1ZzFmMmcgdGN0em9sdGFuQG0&tok=MjEjYm9yYmVseS5hdHRpbGFAdGN0Lmh1MDZmNDQ4NjBlZTg1ODBkNjU5ZjViZWFmYWZiZTZkYTQ4MzYwNjkyMQ&ctz=Europe/Budapest&hl=en> »\r\n\r\n\r\nTeszt meeting\r\n\r\ndsa\r\n\r\nWhen\r\nThu Jan 31, 2013 12am – 1am Budapest\t\r\n\r\nWhere\r\nasd (map <http://maps.google.hu/maps?q=asd&hl=en> )\t\r\n\r\nCalendar\r\ntctzoltan@gmail.com\t\r\n\r\nWho\r\n•\t\r\nborbely.attila@tct.hu - organizer\r\n•\t\r\nZoltán Zöld\r\n\r\nGoing?   \r\n\r\nYes <https://www.google.com/calendar/event?action=RESPOND&eid=dnJwbDRocHRnajF0djQzcTdmYXJ1ZzFmMmcgdGN0em9sdGFuQG0&rst=1&tok=MjEjYm9yYmVseS5hdHRpbGFAdGN0Lmh1MDZmNDQ4NjBlZTg1ODBkNjU5ZjViZWFmYWZiZTZkYTQ4MzYwNjkyMQ&ctz=Europe/Budapest&hl=en>  - \r\nMaybe <https://www.google.com/calendar/event?action=RESPOND&eid=dnJwbDRocHRnajF0djQzcTdmYXJ1ZzFmMmcgdGN0em9sdGFuQG0&rst=3&tok=MjEjYm9yYmVseS5hdHRpbGFAdGN0Lmh1MDZmNDQ4NjBlZTg1ODBkNjU5ZjViZWFmYWZiZTZkYTQ4MzYwNjkyMQ&ctz=Europe/Budapest&hl=en>  - \r\nNo <https://www.google.com/calendar/event?action=RESPOND&eid=dnJwbDRocHRnajF0djQzcTdmYXJ1ZzFmMmcgdGN0em9sdGFuQG0&rst=2&tok=MjEjYm9yYmVseS5hdHRpbGFAdGN0Lmh1MDZmNDQ4NjBlZTg1ODBkNjU5ZjViZWFmYWZiZTZkYTQ4MzYwNjkyMQ&ctz=Europe/Budapest&hl=en>     more options  <https://www.google.com/calendar/event?action=VIEW&eid=dnJwbDRocHRnajF0djQzcTdmYXJ1ZzFmMmcgdGN0em9sdGFuQG0&tok=MjEjYm9yYmVseS5hdHRpbGFAdGN0Lmh1MDZmNDQ4NjBlZTg1ODBkNjU5ZjViZWFmYWZiZTZkYTQ4MzYwNjkyMQ&ctz=Europe/Budapest&hl=en> »\r\n\r\n\r\nInvitation from Google Calendar <https://www.google.com/calendar/> \r\n\r\nYou are receiving this email at the account tctzoltan@gmail.com because you are subscribed for invitations on calendar tctzoltan@gmail.com.\r\n\r\nTo stop receiving these notifications, please log in to https://www.google.com/calendar/ and change your notification settings for this calendar.\r\n\r\n";
				meetingEntry.Description = RandomString(8200);
				meetingEntry.Location = "Location";
				meetingEntry.StartTime = new DateTime(2013, 1, 31, 0, 0, 0);
				meetingEntry.EndTime = new DateTime(2013, 1, 31, 1, 0, 0);
				meetingEntry.Attendees = new List<MeetingAttendee>()
				{
					new MeetingAttendee()
					{
						Email = "borbely.attila@tct.hu",
						Type = MeetingAttendeeType.Organizer,
						ResponseStatus = MeetingAttendeeResponseStatus.ResponseAccepted,
					},
					new MeetingAttendee()
					{
						Email = "tctzoltan@gmail.com",
						Type = MeetingAttendeeType.Required,
						ResponseStatus = MeetingAttendeeResponseStatus.ResponseAccepted,
					}
				};

				result.Add(meetingEntry);
			}

			return result;
		}

		private static string RandomString(int size)
		{
			StringBuilder builder = new StringBuilder();
			Random random = new Random();
			char ch;
			for (int i = 0; i < size; i++)
			{
				ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
				builder.Append(ch);
			}
			return builder.ToString();
		}
#endif

	}

	public class Appointment
	{
		public Redemption.RDOAppointmentItem Item { get; set; }
		public DateTime? OldStartDateTime { get; set; }
		public DateTime? LastModificationOverride { get; set; }
		public MeetingCrudStatus Status { get; set; }
		public string EntryId { get; set; }
	}
	public static class TypeInformation
	{
		public static string GetTypeName(object comObject)
		{
			var dispatch = comObject as IDispatch;

			if (dispatch == null)
			{
				return null;
			}

			var pTypeInfo = dispatch.GetTypeInfo(0, 1033);

			string pBstrName;
			string pBstrDocString;
			int pdwHelpContext;
			string pBstrHelpFile;
			pTypeInfo.GetDocumentation(
				-1,
				out pBstrName,
				out pBstrDocString,
				out pdwHelpContext,
				out pBstrHelpFile);

			string str = pBstrName;
			if (str[0] == 95)
			{
				// remove leading '_'
				str = str.Substring(1);
			}

			return str;
		}

		[ComImport]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		[Guid("00020400-0000-0000-C000-000000000046")]
		private interface IDispatch
		{
			int GetTypeInfoCount();

			[return: MarshalAs(UnmanagedType.Interface)]
			ITypeInfo GetTypeInfo(
				[In, MarshalAs(UnmanagedType.U4)] int iTInfo,
				[In, MarshalAs(UnmanagedType.U4)] int lcid);

			void GetIDsOfNames(
				[In] ref Guid riid,
				[In, MarshalAs(UnmanagedType.LPArray)] string[] rgszNames,
				[In, MarshalAs(UnmanagedType.U4)] int cNames,
				[In, MarshalAs(UnmanagedType.U4)] int lcid,
				[Out, MarshalAs(UnmanagedType.LPArray)] int[] rgDispId);
		}
	}	
}
