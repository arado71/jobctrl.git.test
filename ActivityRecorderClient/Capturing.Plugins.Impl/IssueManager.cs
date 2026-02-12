using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.EnumWindows;
using Tct.ActivityRecorderClient.Communication;
using Tct.ActivityRecorderClient.OutlookMailCaptureServiceReference;
using Tct.ActivityRecorderClient.Serialization;
using Tct.ActivityRecorderClient.View;
using Timer = System.Threading.Timer;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	[Serializable]
	public class IssueNamesAndCompanies
	{
		public List<Entry> IssueNames { get; private set; }
		public List<Entry> IssueCompanies { get; private set; }
		public List<Entry> IssueCategories { get; private set; }

		[NonSerialized]
		public Dictionary<string, DateTime> NameDict;
		[NonSerialized]
		public Dictionary<string, DateTime> CompanyDict;
		[NonSerialized]
		public Dictionary<string, DateTime> CategoryDict;

		[Serializable]
		[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
		public class Entry
		{
			public string Name { get; set; }
			public DateTime LastUsed { get; set; }
		}

		public IssueNamesAndCompanies()
		{
			NameDict = new Dictionary<string, DateTime>();
			CompanyDict = new Dictionary<string, DateTime>();
			InitCategories();
		}

		private void InitCategories()
		{
			if (ConfigManager.IssueCategories != null)
			{
				var j = 0;
				CategoryDict =
					ConfigManager.IssueCategories.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
						.ToDictionary(i => i, i => DateTime.Now.AddSeconds(j++));
			}
			else
				CategoryDict = new Dictionary<string, DateTime>();
		}

		public void TransformForSaving()
		{
			IssueNames = NameDict.Select(e => new Entry() { Name = e.Key, LastUsed = e.Value })
				.OrderByDescending(e => e.LastUsed)
				.Take(20)
				.ToList();

			IssueCompanies = CompanyDict.Select(e => new Entry() { Name = e.Key, LastUsed = e.Value })
				.OrderByDescending(e => e.LastUsed)
				.Take(20)
				.ToList();

			IssueCategories = CategoryDict.Select(e => new Entry() { Name = e.Key, LastUsed = e.Value })
				.OrderByDescending(e => e.LastUsed)
				.Take(20)
				.ToList();
		}

		public void TransformForLoading()
		{
			NameDict = IssueNames.ToDictionary(e => e.Name, e => e.LastUsed);
			CompanyDict = IssueCompanies.ToDictionary(e => e.Name, e => e.LastUsed);
			if (IssueCategories != null)
				CategoryDict = IssueCategories.ToDictionary(e => e.Name, e => e.LastUsed);
			else
				InitCategories();
		}

	}

	public class IssueManager : IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly Regex subjectCoreSplit = new Regex(@"^((re:|fw:|fwd:|vá:)\s)*(?<subj>.*?)\s?(\[\*.*\*\])?$", RegexOptions.IgnoreCase);
		private static readonly Regex fromDomainSplit = new Regex("^([^@]*@)*(?<domain>[^@]*)", RegexOptions.IgnoreCase);
		private static string IssuesFile { get { return "Issues-" + ConfigManager.UserId; } }
		private static string IssueNameAndCompanyFile { get { return "IssueNameAndCompany-" + ConfigManager.UserId; } }
		private static readonly TimeSpan defaultLocalIssuesCacheTimeout = TimeSpan.FromDays(1);

		private readonly SynchronizationContext context;
		private IssuePropsForm editForm;
		private string issueId;
		private string subject = string.Empty;
		private IssueData issue;
		private readonly CachedDictionary<string, IssueData> localIssues;
		private readonly IssueNamesAndCompanies localIssueNamesAndCompanies;
		private readonly object lockObj = new object();
		private readonly CachedDictionary<IntPtr, long> lastActiveWnd = new CachedDictionary<IntPtr, long>(TimeSpan.FromMilliseconds(PluginMail.CaptureCachingDurationInSeconds * 1000 + 1000), true);
		private long issueListId;
		private IssueFilterForm issueFilterForm;
		private Point lastPos;
		private int lastQuery;
		private Timer updateIssuesTimer;

		public IssueManager(SynchronizationContext guiSynchronizationContext)
		{
			context = guiSynchronizationContext;
			localIssues = new CachedDictionary<string, IssueData>(defaultLocalIssuesCacheTimeout, true);
			if (IsolatedStorageSerializationHelper.Exists(IssuesFile) &&
				IsolatedStorageSerializationHelper.Load(IssuesFile, out Dictionary<string, IssueData> tmpIssues))
			{
				foreach (var item in tmpIssues)
				{
					localIssues.Add(item.Key, item.Value);
				}
				log.Info("Cached issues loaded from disk");
			}

			if (IsolatedStorageSerializationHelper.Exists(IssueNameAndCompanyFile) &&
			    IsolatedStorageSerializationHelper.Load(IssueNameAndCompanyFile, out localIssueNamesAndCompanies))
			{
				localIssueNamesAndCompanies.TransformForLoading();
				log.Info("Cached issue names and companies loaded from disk");
			}
			else
				localIssueNamesAndCompanies = new IssueNamesAndCompanies();

			updateIssuesTimer = new Timer(UpdateIssuesTimerOnTick, null, 60000, 60000);
		}

		private void UpdateIssuesTimerOnTick(object state)
		{
			if (issueFilterForm == null || !issueFilterForm.Visible) return;
			var loaded =
				ActivityRecorderClientWrapper.Execute(
					n =>
						n.FilterIssues(ConfigManager.UserId, null, null, null));
			var isChanged = loaded.Count != localIssues.ToDictionary().Count;
			context.Post(_ =>
			{
				foreach (var issueData in loaded)
				{
					lock (lockObj)
					{
						if (!localIssues.TryGetValue(issueData.IssueCode, out var found)
						    || found.State != issueData.State)
						{
							LocalizeUserName(issueData);
							localIssues.Set(issueData.IssueCode, issueData);
							isChanged = true;
						}
					}
					if (issueId == issueData.IssueCode)
					{
						issue = issueData;
						editForm.InitFields(issue.Name, issue.Company, issue.Name, (IssueState)issue.State);
					}
				}
				IsolatedStorageSerializationHelper.Save(IssuesFile, localIssues.ToDictionary());
				if (isChanged) issueFilterForm.Issues = localIssues.ToDictionary().Values.ToList();
			}, null);
		}

		private void IssueListClicked()
		{
			if (issueFilterForm != null)
			{
				if (issueFilterForm.WindowState == FormWindowState.Minimized)
					issueFilterForm.WindowState = FormWindowState.Normal;
				issueFilterForm.Activate();
			}
			else
			{
				issueFilterForm = new IssueFilterForm(this);
				issueFilterForm.Closed += IssueFilterFormClosed;
				issueFilterForm.Issues = localIssues.ToDictionary().Values.ToList();
				issueFilterForm.IssueActionFired += IssueFilterFormOnIssueActionFired;
				issueFilterForm.IssueSelectionChanged += IssueFilterFormIssueSelectionChanged;
				issueFilterForm.Show();
				issueFilterForm.SelectIssue(this.issueId);
			}

			updateIssuesTimer.Change(1500, 60000);
		}

		void IssueFilterFormClosed(object sender, EventArgs e)
		{
			issueFilterForm.Closed -= IssueFilterFormClosed;
			issueFilterForm.IssueActionFired -= IssueFilterFormOnIssueActionFired;
			issueFilterForm.IssueSelectionChanged -= IssueFilterFormIssueSelectionChanged;
			issueFilterForm = null;
		}

		private void IssueFilterFormOnIssueActionFired(object sender, SingleValueEventArgs<string> e)
		{
			ThreadPool.QueueUserWorkItem(_ =>
			{
				using (var client = new OutlookAddinMailCaptureClientWrapper())
				{
					try
					{
						client.Client.FilterMails(new[] { e.Value });
					}
					catch (Exception ex)
					{
						log.Error("Filtering outlook items failed", ex);
					}
				}

			});
		}

		private string issueFilterSelectedId;
		private string lastIssueFilterSelectedId;
		private void IssueFilterFormIssueSelectionChanged(object sender, SingleValueEventArgs<string> e)
		{
			if (e.Value != null || !lastActiveWnd.TryGetValue(issueFilterForm.Handle, out var _))
				lastIssueFilterSelectedId = e.Value;
			issueFilterSelectedId = e.Value;
		}

		// Invoked from BG thread
		internal IssueData GetIssueData(IntPtr hWnd, MailCapture mail)
		{
			context.Post(_ => DisplayEditorWhenNeeded(hWnd, mail.JcId, mail.Subject, !string.IsNullOrEmpty(mail.From?.Email ?? mail.From?.Name) ? mail.From : mail.To.FirstOrDefault()), null);
			if (string.IsNullOrEmpty(mail.JcId)) return null;
			IssueData lastIssue;
			lock (lockObj)
			{
				localIssues.TryGetValue(mail.JcId, out lastIssue);
			}
			return lastIssue;
		}

		private void DisplayEditorWhenNeeded(IntPtr hWnd, string jcId, string subject, MailAddress mailAddress)
		{
			if (subject == null || string.IsNullOrEmpty(mailAddress?.Email ?? mailAddress?.Name) || !ConfigManager.MailTrackingSettings.HasFlag(Mail.MailTrackingSettings.ShowPopupWindow))
				return; ; //needed to prevent incorrect data due to slow email loading
			DebugEx.EnsureGuiThread();
			if (issueListId == 0)
				issueListId = ((Platform.PlatformWinFactory)Platform.Factory).MainForm.AddEtcExtraMenuitem(() => Labels.IssueMgr_Issues, IssueListClicked);
			var activeWindow = WinApi.GetForegroundWindow();
			if (issueFilterForm != null && !issueFilterForm.IsDisposed && (issueFilterSelectedId != null || editForm != null && !editForm.IsDisposed && lastActiveWnd.TryGetValue(issueFilterForm.Handle, out _) && activeWindow == editForm.Handle))
			{
				hWnd = issueFilterForm.Handle;
				jcId = lastIssueFilterSelectedId;
				subject = null;
				mailAddress = null;
			}
			var isIssueWindow = (activeWindow != IntPtr.Zero && activeWindow == hWnd) && !string.IsNullOrEmpty(jcId);

			if (isIssueWindow)
			{
				lastActiveWnd.Set(hWnd, Environment.TickCount);
				var winRect = GetWindowRect(hWnd);
				var targetPos = new Point(winRect.X + winRect.Width - 150, winRect.Y >= 0 ? winRect.Y : winRect.Y + 5); // hax for full screen window that reports (-8;-8) position
				if (editForm == null)
				{
					editForm = new IssuePropsForm(ConfigManager.IssuePropColumns);
					editForm.Location = targetPos - new Size(editForm.Width, 0);
					editForm.PulledUp += EditFormPulledUp;
					editForm.DroppedDown += EditFormDroppedDown;
					editForm.IssuesButtonClicked += EditFormIssuesButtonClicked;
					editForm.LocalIssueNamesAndCompanies = localIssueNamesAndCompanies;
					editForm.Show();
					log.Debug("New IssuePropsForm created");
				}
				else
				{
					if (targetPos != lastPos)
						editForm.Location = targetPos - new Size(editForm.Width, 0);
					if (!editForm.Visible)
					{
						editForm.Show();
						log.Debug("Existing IssuePropsForm shown");
					}
				}
				lastPos = targetPos;
				editForm.Ping();

				if (!string.IsNullOrEmpty(jcId) && !jcId.Equals(issueId) && subject != this.subject)
				{
					if (!string.IsNullOrEmpty(issueId) && editForm.IsModified)
					{
						if (!editForm.IssueState.HasValue) editForm.IssueState = IssueState.Opened;
						SaveModifications(issueId, editForm.IssueName, editForm.Company, editForm.Category, editForm.IssueState.Value);
						log.Debug("Modified issue saved before new issue shown, id=" + issueId);
					}
					issueId = jcId;
					lock (lockObj)
					{
						localIssues.TryGetValue(jcId, out issue);
					}
					string subj;
					string comp;
					string cat;
					IssueState? issueState;
					if (issue != null)
					{
						subj = issue.Name;
						comp = issue.Company;
						cat = subj;
						issueState = (IssueState)issue.State;
						log.DebugFormat("Existing issue retrieved from local cache, id={0}, name={1}", jcId, subj);
					}
					else
					{
						subj = !string.IsNullOrEmpty(subject)
							? subjectCoreSplit.Match(subject).Groups["subj"].Value.Trim()
							: "(unnamed)";
						comp = mailAddress != null && !string.IsNullOrEmpty(mailAddress.Email)
							? fromDomainSplit.Match(mailAddress.Email).Groups["domain"].Value
							: "(unknown)";
						cat = null;
						issueState = null;
						log.DebugFormat("New issue created, id={0}, name={1}", jcId, subj);
					}
					this.subject = subj;
					if (!editForm.IsBusy)
					{
						editForm.IsBusy = true;
						ThreadPool.QueueUserWorkItem(_ => GetIssuesAsync(jcId));
					}
					editForm.InitFields(subj, comp, cat, issueState);
				}
			}
			else
			{
				lastActiveWnd.Remove(hWnd);
				if (editForm != null && !editForm.IsDisposed && editForm.Visible && !editForm.IsActivated && lastActiveWnd.ToDictionary().Count == 0)
				{
					editForm.Hide();
					Debug.Assert(!editForm.IsModified || !string.IsNullOrEmpty(issueId), "Modified issue with id null");
					if (editForm.IsModified && !string.IsNullOrEmpty(issueId))
					{
						if (!editForm.IssueState.HasValue) editForm.IssueState = IssueState.Opened;
						SaveModifications(issueId, editForm.IssueName, editForm.Company, editForm.Category, editForm.IssueState.Value);
						log.Debug("Modified issue saved after form hided, id=" + issueId);
					}
					issueId = null;
				}
			}
		}

		void EditFormIssuesButtonClicked(object sender, EventArgs e)
		{
			IssueListClicked();
		}

		void EditFormPulledUp(object sender, EventArgs e)
		{
			if (issueId != null && editForm.IsModified)
			{
				if (!editForm.IssueState.HasValue) editForm.IssueState = IssueState.Opened;
				SaveModifications(issueId, editForm.IssueName, editForm.Company, editForm.Category, editForm.IssueState.Value);
				log.Debug("Modified issue saved after form pulled up, id=" + issueId);
			}
		}

		void EditFormDroppedDown(object sender, EventArgs e)
		{
			if (Environment.TickCount - lastQuery < 60000 || issueId == null) return;
			editForm.IsBusy = true;
			ThreadPool.QueueUserWorkItem(_ => GetIssuesAsync(issueId));
		}

		public void GetIssuesAsync(string id)
		{
			try
			{
				log.Debug("Issue query from server started, id=" + id);
				var loaded = ActivityRecorderClientWrapper.Execute(n => n.GetIssue(ConfigManager.UserId, id));
				context.Post(_ =>
				{
					try
					{
						if (loaded != null)
						{
							lastQuery = Environment.TickCount;
							log.DebugFormat("Issue query from server was successful, id={0}, name={1}", id, loaded.Name);
							lock (lockObj)
							{
								LocalizeUserName(loaded);
								localIssues.Set(id, loaded);
							}
							IsolatedStorageSerializationHelper.Save(IssuesFile, localIssues.ToDictionary()); // CachedDirectory can't be stored because it contains local tick values as item age
							if (issueId == id) // if current issue not changed only
							{
								issue = loaded;
								editForm.InitFields(issue.Name, issue.Company, issue.Name, (IssueState)issue.State);
							}
							issueFilterForm?.RefreshIssue(loaded);
						}
					}
					finally
					{
						editForm.IsBusy = false;
					}
				}, null);
			}
			catch (Exception ex)
			{
				log.Error("Getting issue: " + id + " failed", ex);
				context.Post(_ => editForm.IsBusy = false, null);
			}
		}

		public void QueryIssuesByFilter(string searchText, int? filterState, bool? filterOwner)
		{
			try
			{
				log.DebugFormat("QueryIssuesByFilter from server started, text={0}, state={1}, owner={2}", searchText, filterState, filterOwner);
				var loaded =
					ActivityRecorderClientWrapper.Execute(
						n =>
							n.FilterIssues(ConfigManager.UserId, string.IsNullOrEmpty(searchText) ? null : new[] { searchText }.ToList(),
								filterState, filterOwner));
				context.Post(_ =>
				{
					if (loaded != null)
					{
						log.Debug("QueryIssuesByFilter was successful");
						lock (lockObj)
						{
							foreach (var item in loaded)
							{
								LocalizeUserName(item);
								localIssues.Set(item.IssueCode, item);
							}
							IsolatedStorageSerializationHelper.Save(IssuesFile, localIssues.ToDictionary());
							issueFilterForm.Issues = localIssues.ToDictionary().Values.ToList();
						}
					}
				}, null);
			}
			catch (Exception ex)
			{
				log.Error("QueryIssuesByFilter failed", ex);
			}
		}

		private void LocalizeUserName(IssueData item)
		{
			if (item.CreatedByName != null) { 
				var createdByNameArr = item.CreatedByName.Split((','));
				item.CreatedByName = Labels.Culture.GetCultureSpecificName(createdByNameArr.Count() > 1 ? string.Join(" ", createdByNameArr.Skip(1).ToArray()).Trim() : "", createdByNameArr[0].Trim());
			}
			else
				item.CreatedByName = "";

			if (item.ModifiedByName != null) { 
				var modifiedByNameArr = item.ModifiedByName.Split((','));
				item.ModifiedByName = Labels.Culture.GetCultureSpecificName(modifiedByNameArr.Count() > 1 ? string.Join(" ", modifiedByNameArr.Skip(1).ToArray()).Trim() : "", modifiedByNameArr[0].Trim());
			}
			else
				item.ModifiedByName = "";
		}

		private void SaveModifications(string id, string issueName, string company, string category, IssueState issueState)
		{
			var isNewIssue = issue == null;
			if (isNewIssue)
				issue = new IssueData { IssueCode = id };
			else
				if (issueName.Trim() == issue.Name && company.Trim() == issue.Company && issueState == (IssueState)issue.State && (category == null || category.Trim() == issue.Name)) return;
			lock (lockObj)
			{
				issue.Name = (category ?? issueName).Trim();
				issue.Company = company.Trim();
				issue.State = (int)issueState;
				issue.Modified = DateTime.UtcNow;
				issue.UserId = ConfigManager.UserId;
			}
			var copy = issue.DeepClone();

			var command = isNewIssue
				? new Action<ActivityRecorderServiceReference.ActivityRecorderClient>(n => n.AddIssue(copy))
				: n => n.ModifyIssue(copy);
			if (!editForm.IsBusy)
			{
				editForm.IsBusy = true;
				ThreadPool.QueueUserWorkItem(_ =>
				{
					while (true)
						try
						{
							ActivityRecorderClientWrapper.Execute(command);
							break;
						}
						catch (Exception ex)
						{
							log.Error("Issue modification failed id=" + id, ex);
							Thread.Sleep(30000);
						}
					context.Post(__ =>
					{
						lock (lockObj)
						{
							localIssues.Set(id, copy);
						}
						IsolatedStorageSerializationHelper.Save(IssuesFile, localIssues.ToDictionary());
						localIssueNamesAndCompanies.TransformForSaving();
						IsolatedStorageSerializationHelper.Save(IssueNameAndCompanyFile, localIssueNamesAndCompanies);
						editForm.IsBusy = false;
						log.Debug("Issue saved successfully, id=" + id);
					}, null);
				});
			}
			if (issueFilterForm != null)
				issueFilterForm.RefreshIssue(issue);
		}

		public void Dispose()
		{
			updateIssuesTimer.Dispose();
			updateIssuesTimer = null;
			context.Post(_ =>
			{
				if (issueListId != 0) ((Platform.PlatformWinFactory)Platform.Factory).MainForm.RemoveEtcExtraMenuitem(issueListId);
				if (issueFilterForm != null) issueFilterForm.Close();
				if (editForm == null) return;
				editForm.Hide();
				editForm.Close();
			}, null);
		}

		private static Rectangle GetWindowRect(IntPtr hWnd)
		{
			var rect = new WinApi.RECT();
			//DwmGetWindowAttribute is 0,1 ms while GetWindowRect is 0,003ms per window and there is litte difference between the two... so skip using it atm.
			//if (!isVistaOrLater || DwmGetWindowAttribute(hWnd, DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS, out rect, 4 * 4) != 0)
			//{
			WinApi.GetWindowRect(hWnd, out rect);
			//}
			return new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
		}
	}

	[Flags]
	public enum IssuePropColumnFlag
	{
		CompanyVisible = 1 << 0,
		NameVisible = 1 << 1,
		CategoryVisible = 1 << 2,
		StateVisible = 1 << 3,
		IssuesButtonVisible = 1 << 4,
	}
}
