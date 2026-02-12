using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Communication;
using Tct.ActivityRecorderClient.Meeting;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Menu.Selector;
using Tct.ActivityRecorderClient.Serialization;

namespace Tct.ActivityRecorderClient.Capturing.Core
{
	//todo reduce the number of ClientMenuLookups
	/// <summary>
	/// Thread-safe class for coordinating menu changes
	/// </summary>
	public class MenuCoordinator : IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static string MenuFile { get { return "LocalMenu-" + ConfigManager.UserId; } }

		private readonly MenuManager menuManager = new MenuManager();
		private readonly WorkAssigner workAssigner = new WorkAssigner();
		private readonly object thisLock = new object();
		private readonly ClientSettingsManager clientSettings;
		private readonly ClientMenuEditLookup menuLookup = new ClientMenuEditLookup();
		private readonly AsyncWorkQueue<AssignData> assignWorkQueue;
		private readonly DefaultWorkIdSelector defaultWorkIdSelector;
		private readonly HashSet<AssignData> sentKeys = new HashSet<AssignData>();
		private readonly HashSet<AssignData> rejectedKeys = new HashSet<AssignData>();
		private Dictionary<AssignData, int> tempWorkIdDict = new Dictionary<AssignData, int>();
		private int currentTempWorkId = -110;
		private ClientMenu lastMenu;
		private IEnumerable<IMenuPublisher> menuPublishers;

		public event EventHandler<MenuEventArgs> CurrentMenuChanged;

		public MenuCoordinator(DefaultWorkIdSelector defaultWorkIdSelector, ClientSettingsManager clientSettings)
		{
			assignWorkQueue = new AsyncWorkQueue<AssignData>(AssignWork);
			menuPublishers = Platform.Factory.GetMenuPublishers();
			workAssigner.DataAssigned += WorkAssignerDataAssigned;
			workAssigner.DataRejected += WorkAssignerDataRejected;
			menuManager.CurrentMenuChanged += MenuManagerCurrentMenuChanged;
			this.defaultWorkIdSelector = defaultWorkIdSelector;
			this.clientSettings = clientSettings;
			clientSettings.SettingsChanged += HandleClientSettingChanged;
		}

		public ClientMenuLookup LoadMenu()
		{
			lock (thisLock)
			{
				LoadLocal();
				foreach (var key in tempWorkIdDict.Keys) //try assign saved keys
				{
					if (!sentKeys.Add(key)) continue; //already sent
					log.Debug("Assigning key " + key);
					workAssigner.AssignWorkAsync(key);
				}
				menuManager.LoadMenu();
				return MenuChanged(menuManager.CurrentMenu);
			}
		}

		public void Start()
		{
			menuManager.Start();
		}

		public void Stop()
		{
			menuManager.Stop();
		}

		//called from BG and GUI thread
		public bool CanSendItem(IWorkItem item, out bool isChanged)
		{
			isChanged = false;
			if (item == null || !item.HasWorkId || IsWorkIdFromServer(item.GetWorkId())) return true;
			if (item.AssignData == null)
			{
				log.ErrorAndFail("AssignData is missing");
				return TrySetWorkId(item, null, ref isChanged); //user should choose a new work
			}
			lock (thisLock)
			{
				bool ignored;
				var workData = menuLookup.GetWorkForAssignData(item.AssignData, out ignored);
				if (workData != null) //we either have a real work id or a temp one
				{
					return TrySetWorkId(item, workData.WorkData.Id.Value, ref isChanged);
				}
				if (ignored)
				{
					log.ErrorAndFail("Cannot have a valid workid due to ignored key " + item.AssignData);
					return TrySetWorkId(item, null, ref isChanged); //user should choose a new work
				}
				if (!sentKeys.Contains(item.AssignData))
				{
					Debug.Assert(!tempWorkIdDict.ContainsKey(item.AssignData)); //this is checked earlier with GetWorkForAssignData
					Debug.Assert(!rejectedKeys.Contains(item.AssignData)); //since it's not sent
					//assign unknown key (we shouldn't reach this normally probably LocalMenu was corrupt)
					AssignWorkAsync(item.AssignData);
				}
				else if (rejectedKeys.Contains(item.AssignData))
				{
					log.Debug("Using default workId for rejected key " + item.AssignData);
					return TrySetWorkId(item, null, ref isChanged); //user should choose a new work
				}
				//else it is in sentKeys and waiting for assignment (as it is not rejected nor removed yet)
			}
			return false;
		}

		private void HandleClientSettingChanged(object sender, SingleValueEventArgs<ClientSetting> eventArgs)
		{
			WorkDataWithParentNames newDefaultWork = null;
			lock (thisLock)
			{
				if (eventArgs.Value.DefaultWorkId != null && menuLookup != null)
				{
					menuLookup.WorkDataById.TryGetValue(eventArgs.Value.DefaultWorkId.Value, out newDefaultWork);
				}

				if (menuLookup != null && !WorkDataWithParentNames.WorkDataIdComparer.Equals(menuLookup.DefaultWork, newDefaultWork))
				{
					log.Debug("Default work changed to " + newDefaultWork);
					menuLookup.DefaultWork = newDefaultWork;
					OnCurrentMenuChanged(menuLookup.ClientMenu, menuLookup.ClientMenu, menuLookup.GetReadonlyCopy());
				}
			}
		}

		private bool TrySetWorkId(IWorkItem item, int? workId, ref bool isChanged)
		{
			if (workId == null)
			{
				workId = defaultWorkIdSelector.GetDefaultWorkId();
			}
			if (workId != null && IsWorkIdFromServer(workId.Value))
			{
				item.SetWorkId(workId.Value);
				isChanged = true;
				return true;
			}
			return false;
		}

		public static bool IsWorkIdFromServer(int workId)
		{
			return workId > 0;
		}

		private void OnCurrentMenuChanged(ClientMenu clientMenu, ClientMenu oldMenu, ClientMenuLookup clientMenuLookup)
		{
			try
			{
				var del = CurrentMenuChanged;
				if (del != null) del(this, new MenuEventArgs(clientMenu, oldMenu, clientMenuLookup));
			}
			catch (Exception ex)
			{
				log.ErrorAndFail("Unexpected error in OnCurrentMenuChanged", ex);
			}
		}

		private void LoadLocal()
		{
			//assert thisLock is held
			if (IsolatedStorageSerializationHelper.Exists(MenuFile))
			{
				LocalMenu data;
				if (IsolatedStorageSerializationHelper.Load(MenuFile, out data))
				{
					tempWorkIdDict = data.TempWorkIdDict;
					currentTempWorkId = data.CurrentTempWorkId;
					Debug.Assert(currentTempWorkId <= tempWorkIdDict.Select(n => n.Value).DefaultIfEmpty(currentTempWorkId).Min());
				}
			}
		}

		private void SaveLocal() //this is not water-tight, but tempWorkId is not really used for anything important, so it is ok if we loose this and reuse some tempWorkIds
		{
			//assert thisLock is held
			IsolatedStorageSerializationHelper.Save(MenuFile, new LocalMenu() { TempWorkIdDict = tempWorkIdDict, CurrentTempWorkId = currentTempWorkId });
		}

		public void RefreshMenu()
		{
			menuManager.RefreshMenu();
		}

		/// <summary>
		/// Updates the lookup with the provided menu extended with temporary local tasks.
		/// </summary>
		/// <param name="menu">The new <see cref="ClientMenu"/> to base the new lookup on.</param>
		/// <returns>Lookup created from the provided ClientMenu.</returns>
		/// <remarks>Function also updates the internal state of temporary tasks as a side-effect.</remarks>
		private ClientMenuLookup MenuChanged(ClientMenu menu)
		{
			Debug.Assert(clientSettings != null);
			lock (thisLock)
			{
				menuLookup.ClientMenu = menu; // Generate new ClientMenuLookup
				var currentClientSettings = clientSettings.ClientSettings;
				if (currentClientSettings != null && currentClientSettings.DefaultWorkId != null && menuLookup != null)
				{
					WorkDataWithParentNames newDefaultWork;
					menuLookup.WorkDataById.TryGetValue(currentClientSettings.DefaultWorkId.Value, out newDefaultWork);
					menuLookup.DefaultWork = newDefaultWork;
				}
				// Refresh sentKeys
				var keysToRemove = new List<AssignData>();
				foreach (var assignData in sentKeys)
				{
					bool ignored;
					if (menuLookup.GetWorkForAssignData(assignData, out ignored) != null || ignored)
					{
						keysToRemove.Add(assignData); //remove keys in menu so when a work is closed it could be reopened
					}
				}
				foreach (var assignData in keysToRemove)
				{
					sentKeys.Remove(assignData);
				}
				// Refresh rejectedKeys
				keysToRemove.Clear();
				foreach (var assignData in rejectedKeys)
				{
					bool ignored;
					if (menuLookup.GetWorkForAssignData(assignData, out ignored) != null || ignored)
					{
						keysToRemove.Add(assignData); //remove keys in menu because obviously they were not rejected
					}
				}
				foreach (var assignData in keysToRemove)
				{
					rejectedKeys.Remove(assignData);
				}
				// Add temporary work or remove from internal list if present in menu
				keysToRemove.Clear();
				foreach (var kvp in tempWorkIdDict)
				{
					if (!menuLookup.AddTempWorkToMenu(kvp.Key, kvp.Value)) //add temp works if not yet assigned
					{
						keysToRemove.Add(kvp.Key); //remove keys for temp works if they are assigned
					}
				}
				foreach (var assignData in keysToRemove)
				{
					tempWorkIdDict.Remove(assignData);
				}
				log.Debug("MenuChange: Sent " + sentKeys.Count + " rejected " + rejectedKeys.Count);
				return SaveChangesAndRaiseMenuChanged();
			}
		}

		//called on BG thread (or GUI when loading?)
		private void MenuManagerCurrentMenuChanged(object sender, MenuEventArgs e)
		{
			MenuChanged(e.Menu);
			PublishExternally(e.Menu, null);
		}

		public void PublishMenu(Action<Exception> onErrorCallback)
		{
			PublishExternally(menuManager.CurrentMenu, onErrorCallback);
		}

		private void PublishExternally(ClientMenu clientMenu, Action<Exception> onErrorCallback)
		{
			if (menuPublishers == null) return;
			foreach(var publisher in menuPublishers) 
				publisher.PublishMenu(clientMenu, onErrorCallback);
		}

		/// <summary>
		/// Invoked when <see cref="AssignData"/> is rejected by the server.
		/// </summary>
		/// <param name="e">Rejected AssignData.</param>
		private void WorkAssignerDataRejected(object sender, SingleValueEventArgs<AssignData> e)
		{
			DebugEx.EnsureBgThread();
			lock (thisLock)
			{
				rejectedKeys.Add(e.Value);
				tempWorkIdDict.Remove(e.Value);
				menuLookup.RemoveKeyFromMenu(e.Value); //removing the work would cause warning popup on the client (working on invalid work)
				SaveChangesAndRaiseMenuChanged();
			}
		}

		private ClientMenuLookup SaveChangesAndRaiseMenuChanged()
		{
			SaveLocal();
			var guiLookup = menuLookup.GetReadonlyCopy();
			OnCurrentMenuChanged(guiLookup.ClientMenu, lastMenu, guiLookup);
			lastMenu = guiLookup.ClientMenu;
			return guiLookup;
		}

		private void WorkAssignerDataAssigned(object sender, SingleValueEventArgs<AssignData> e)
		{
			DebugEx.EnsureBgThread();
			menuManager.RefreshMenu();
		}

		//called on GUI or BG thread
		/// <summary>
		/// Registers an AssignData for processing. From the provided AssignData a new temporary WorkItem is created, 
		/// and it is inserted into the ClientMenu.
		/// </summary>
		/// <param name="key">The AssignData to create a new temporary WorkItem from</param>
		public void AssignWorkAsync(AssignData key)
		{
			if (key == null
				|| (key.Work == null && key.Project == null && key.Composite == null)
				|| (key.Work != null && key.Work.WorkKey.IsNullOrWhiteSpace())
				|| (key.Project != null && key.Project.ProjectKey.IsNullOrWhiteSpace())
				|| (key.Composite != null && key.Composite.WorkKey.IsNullOrWhiteSpace())
				)
			{
				log.ErrorAndFail("Invalid assignData " + key);
				return;
			}
			//we don't want to block GUI nor want to flood threadpool as this can be called quite rapidly and could have many queued calls due to contention on thisLock
			//so instead of ThreadPool.QueueUserWorkItem(_ => AssignWork(key), null); we use assignWorkQueue.EnqueueAsync(key);
			assignWorkQueue.EnqueueAsync(key);
		}

		private void AssignWork(AssignData key)
		{
			DebugEx.EnsureBgThread();
			lock (thisLock)
			{
				var tempWorkId = currentTempWorkId - 1;
				if (IsWorkIdFromServer(tempWorkId))
				{
					log.ErrorAndFail("Invalid tempWorkId " + tempWorkId);
					return;
				}
				if (!sentKeys.Add(key)) return; //already sent
				log.Debug("Assigning key " + key);
				workAssigner.AssignWorkAsync(key);
				try
				{
					if (menuLookup.AddTempWorkToMenu(key, tempWorkId))
					{
						currentTempWorkId = tempWorkId;
						tempWorkIdDict.Add(key, tempWorkId);
						SaveChangesAndRaiseMenuChanged();
					}
				}
				catch (Exception ex)
				{
					log.ErrorAndFail("Unable to create work for key " + key, ex);
				}
			}
		}

		public void Dispose()
		{
			assignWorkQueue.Dispose();
			log.Debug("assignWorkQueue Disposed");
			workAssigner.Dispose();
			log.Debug("workAssigner Disposed");
		}

		[DataContract]
		[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
		internal class LocalMenu
		{
			[DataMember]
			public Dictionary<AssignData, int> TempWorkIdDict;
			[DataMember]
			public int CurrentTempWorkId;
		}
	}
}
