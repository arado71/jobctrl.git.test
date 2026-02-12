using log4net;
using MetroFramework;
using MetroFramework.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Core;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Telemetry;
using Tct.ActivityRecorderClient.View.Controls;
using Tct.ActivityRecorderClient.View.Navigation;
using Message = System.Windows.Forms.Message;
using Screen = System.Windows.Forms.Screen;

namespace Tct.ActivityRecorderClient.View
{
	public sealed partial class ContextMenu : Form, IMessageFilter, ILocalizableControl
	{
		public const int FullWidth = 365;

#if DEV || DEBUG
		private const int DisplayedReportsOutDatedInterval = 1 * 60 * 1000;
#else
		private const int DisplayedReportsOutDatedInterval = 60 * 60 * 1000;
#endif

		public enum TaskView
		{
			Priority,
			Deadline,
			Progress,
			Search
		}

		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private readonly ActivityRecorderForm parent;
		// ReSharper disable once NotAccessedField.Local - Required to keep reference
		private Form shadowForm;
		private bool canClose = false;

		public event EventHandler<WorkDataEventArgs> WorkClick;
		public bool SuppressHide { get; set; }
		private readonly MenuReportHelper menuReportHelper;
		private readonly MenuTabHelper menuTabHelper;
		private StatText statText;

		private Font boldFont = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
		private Font normalFont = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
		private Font boldFontSM = new System.Drawing.Font("Segoe UI", 7.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
		private Font normalFontSM = new System.Drawing.Font("Segoe UI", 7.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));

		private readonly WorktimeStatIntervals[] worktimeStatIntervalEnums = { WorktimeStatIntervals.Week, WorktimeStatIntervals.Month, WorktimeStatIntervals.Quarter, WorktimeStatIntervals.Year };

		private WorktimeStatIntervals FirstRow => worktimeStatIntervalEnums.FirstOrDefault(w => (ConfigManager.LocalSettingsForUser.DisplayWorktimeStats & w) == w);
		private WorktimeStatIntervals SecondRow => worktimeStatIntervalEnums.Where(w => (ConfigManager.LocalSettingsForUser.DisplayWorktimeStats & w) == w).Take(2).LastOrDefault();

		private readonly Dictionary<WorktimeStatIntervals, Func<string>> textFuncs = new Dictionary<WorktimeStatIntervals, Func<string>>()
		{
			{ WorktimeStatIntervals.None, () => @"n/a" },
			{ WorktimeStatIntervals.Week, () => Labels.ThisWeeksWorkTime },
			{ WorktimeStatIntervals.Month, () => Labels.ThisMonthsWorkTime },
			{ WorktimeStatIntervals.Quarter, () => Labels.ThisQuarterWorkTime },
			{ WorktimeStatIntervals.Year, () => Labels.ThisYearWorkTime },
		};
		private readonly Dictionary<WorktimeStatIntervals, Func<WorkTimeStats, long>> targetTimeFuncs = new Dictionary<WorktimeStatIntervals, Func<WorkTimeStats, long>>()
		{
			{ WorktimeStatIntervals.Week, stats => stats.ThisWeeksTargetNetWorkTimeInMs },
			{ WorktimeStatIntervals.Month, stats => stats.ThisMonthsTargetNetWorkTimeInMs },
			{ WorktimeStatIntervals.Quarter, stats => stats.ThisQuarterTargetNetWorkTimeInMs },
			{ WorktimeStatIntervals.Year, stats => stats.ThisYearTargetNetWorkTimeInMs }
		};
		private readonly Dictionary<WorktimeStatIntervals, Func<WorkTimeStats, long>> workTimeFuncs = new Dictionary<WorktimeStatIntervals, Func<WorkTimeStats, long>>()
		{
			{ WorktimeStatIntervals.Week, stats => stats.ThisWeeksWorkTimeInMs },
			{ WorktimeStatIntervals.Month, stats => stats.ThisMonthsWorkTimeInMs },
			{ WorktimeStatIntervals.Quarter, stats => stats.ThisQuarterWorkTimeInMs },
			{ WorktimeStatIntervals.Year, stats => stats.ThisYearWorkTimeInMs }
		};
		private readonly Dictionary<WorktimeStatIntervals, Func<WorkTimeStats, long>> targetUntilTodayTimeFuncs = new Dictionary<WorktimeStatIntervals, Func<WorkTimeStats, long>>()
		{
			{ WorktimeStatIntervals.Week, stats => stats.ThisWeeksTargetUntilTodayNetWorkTimeInMs },
			{ WorktimeStatIntervals.Month, stats => stats.ThisMonthsTargetUntilTodayNetWorkTimeInMs },
			{ WorktimeStatIntervals.Quarter, stats => stats.ThisQuarterTargetUntilTodayNetWorkTimeInMs },
			{ WorktimeStatIntervals.Year, stats => stats.ThisYearTargetUntilTodayNetWorkTimeInMs }
		};

		public bool IsForeground
		{
			get { return FromHandle(WinApi.GetForegroundWindow()) != null; }
		}

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams parms = base.CreateParams;
				parms.Style &= ~WinApi.WS_CLIPCHILDREN;
				parms.ClassStyle |= WinApi.CS_DBLCLKS;
				return parms;
			}
		}

		public ContextMenu(ActivityRecorderForm parent)
		{
			this.parent = parent;
			statText = new StatText();
			InitializeComponent();
			tableLayoutPanel1.Width = FullWidth;
			metroTabControl.Width = FullWidth;
			tasksMetroTabPage.Width = FullWidth - 8;
			workGrid1.Width = FullWidth - 14;
			userDisplay.Width = FullWidth - 8;
			bigSplitter1.Width = FullWidth - 16;
			statGrid.Width = FullWidth - 16;
			currentWork1.Width = FullWidth;
			pSearchBg.Width = FullWidth;
			searchBox1.Width = FullWidth - 20;
			ClientSize = new Size(FullWidth, ClientSize.Height);
			SetColorScheme();
			Microsoft.Win32.SystemEvents.UserPreferenceChanged += UserPreferenceChanged;
			var factory = new NavigationFactory(workGrid1);
			workGrid1.NavigationFactory = factory;
			currentWork1.NavigationFactory = factory;
			userDisplay.MainForm = this;
			statGrid.StatsClicked += HandleStatsClicked;
			statText.StatsClicked += HandleStatsClicked;
			workGrid1.WorkClick += HandleWorkClicked;
			currentWork1.WorkClick += HandleWorkClicked;
			workGrid1.DropdownClosed += HandleDropdownClosed;
			userDisplay.DropdownClosed += HandleDropdownClosed;
			workGrid1.CreateWorkClicked += HandleCreateWorkClicked;
			parent.ConfigChanged += HandleConfigChanged;
			searchBox1.Navigator = workGrid1;
			searchBox1.OnDropdownHidden += HandleSearchboxDropdownClosed;
			searchBox1.EmptyEscapePressed += (sender, args) => Hide();
			HandleConfigChanged(this, EventArgs.Empty);
			MenuQuery.Instance.SimpleWorkTimeStats.Changed +=
				(_, __) => UpdateSimpleStats(MenuQuery.Instance.SimpleWorkTimeStats.Value);
			menuReportHelper = parent.MenuReportHelper;
			menuReportHelper.FeatureDisabled += MenuReportHelper_FeatureDisabled;
			menuReportHelper.FeatureEnabled += MenuReportHelper_FeatureEnabled;
			menuReportHelper.DisplayedReportQuerying += MenuReportHelper_DisplayedReportQuerying;
			menuReportHelper.DisplayedReportError += MenuReportHelper_DisplayedReportError;
			menuTabHelper = parent.MenuTabHelper;
			menuTabHelper.TabsChanged += MenuTabHelper_TabsChanged;
		}

		private void MenuTabHelper_TabsChanged(object sender, List<ClientTab> e)
		{
			if (InvokeRequired) { Invoke(new Action(() => refreshCustomTabNames(e))); return; }
			refreshCustomTabNames(e);
		}

		private void SetColorScheme()
		{
			if (SystemInformation.HighContrast)
			{
				BackColor = SystemColors.WindowText;
				pSearchBg.BackColor = SystemColors.Window;
				metroTabControl.Theme = MetroThemeStyle.Dark;
				metroTabControl.FontWeight = MetroTabControlWeight.Bold;
				metroTabControl.Style = MetroColorStyle.White;
				tasksMetroTabPage.BackColor = SystemColors.Window;
				tableLayoutPanel1.BackColor = SystemColors.Window;
				Padding = new Padding(3);
			}
			else
			{
				Padding = Padding.Empty;
				BackColor = StyleUtils.BackgroundInactive;
				pSearchBg.BackColor = System.Drawing.Color.Transparent;
				metroTabControl.Theme = MetroThemeStyle.Light;
				metroTabControl.FontWeight = MetroTabControlWeight.Light;
				metroTabControl.Style = MetroColorStyle.Blue;
				tasksMetroTabPage.BackColor = SystemColors.Control;
				tableLayoutPanel1.BackColor = StyleUtils.BackgroundInactive;
			}
		}

		public void UserPreferenceChanged(object sender, Microsoft.Win32.UserPreferenceChangedEventArgs e)
		{
			SetColorScheme();
			userDisplay.SetColorScheme();
			statGrid.SetColorScheme();
			currentWork1.SetColorScheme();
			workGrid1.SetColorScheme();
		}

		private void MenuReportHelper_DisplayedReportError(object sender, EventArgs e)
		{
			if (InvokeRequired)
			{
				Invoke(new Action(() => MenuReportHelper_DisplayedReportError(sender, e)));
			}
			else
			{
				statText.setProgressSpinner(false);
				//TODO: error handling

			}
		}

		private void MenuReportHelper_DisplayedReportQuerying(object sender, EventArgs e)
		{
			if (InvokeRequired)
			{
				Invoke(new Action(() => MenuReportHelper_DisplayedReportQuerying(sender, e)));
			}
			else
			{
				statText.setProgressSpinner(true);
			}
		}

		private void MenuReportHelper_FeatureEnabled(object sender, EventArgs e)
		{
			if (InvokeRequired) { Invoke(new Action(() => enableReportsInMenu())); return; }
			enableReportsInMenu();
		}

		private void MenuReportHelper_FeatureDisabled(object sender, EventArgs e)
		{
			if (InvokeRequired) { Invoke(new Action(() => disableReportsInMenu())); return; }
			disableReportsInMenu();
		}

		private void enableReportsInMenu()
		{
#if !DEBUG && !DEV
			refreshFavoritesButton.Visible = false;
			favoritePanel.Location = new Point(0, 0);
#endif
			if (!tableLayoutPanel1.Controls.Contains(workGrid1))
			{
				if (!metroTabControl.TabPages.Contains(favoriteReportsMetroTabPage))
					metroTabControl.TabPages.Add(favoriteReportsMetroTabPage);
				if (!metroTabControl.TabPages.Contains(overviewMetroTabPage))
					metroTabControl.TabPages.Add(overviewMetroTabPage); return;
			}
			tableLayoutPanel1.Controls.Remove(workGrid1);
			tableLayoutPanel1.Controls.Add(metroTabControl, 0, 5);
			tasksMetroTabPage.Controls.Add(workGrid1);
			workGrid1.Anchor = AnchorStyles.None;
			workGrid1.AutoSize = false;
			workGrid1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
			workGrid1.Location = new System.Drawing.Point(0, 0);
			workGrid1.Margin = new System.Windows.Forms.Padding(0);
			workGrid1.Size = tasksMetroTabPage.ClientSize;
			workGrid1.TabIndex = 3;
			menuReportHelper.RefreshDisplayedReports();
			menuReportHelper.RefreshFavoriteReports();
		}

		private void disableReportsInMenu()
		{
			if (tableLayoutPanel1.Controls.Contains(workGrid1)) return;
			tasksMetroTabPage.Controls.Clear();
			tableLayoutPanel1.Controls.Remove(metroTabControl);
			tableLayoutPanel1.Controls.Add(workGrid1, 0, 6);
			workGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			workGrid1.AutoSize = true;
			workGrid1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			workGrid1.Location = new System.Drawing.Point(0, 214);
			workGrid1.Margin = new System.Windows.Forms.Padding(0);
			workGrid1.Size = tasksMetroTabPage.ClientSize;
			workGrid1.TabIndex = 6;
		}

		private List<PictureBox> overViewPictureBoxes = new List<PictureBox>();

		private void refreshOverview()
		{
			DebugEx.EnsureGuiThread();
			overviewMetroTabPage.Controls.Clear();
			int rowCount = menuReportHelper.GetTableRowCount();
			statText.Location = new Point(0, 3);
			statText.Size = new Size(overviewMetroTabPage.Width, statText.Height);
			statText.SetText(menuReportHelper.DisplayedReportsLastQueryTime?.ToString(DateTimeFormatInfo.CurrentInfo.ShortTimePattern));
			overviewMetroTabPage.Controls.Add(statText);
			overViewPictureBoxes.Clear();
			statText.setProgressSpinner(false);
			if (rowCount > 0)
			{
				Point tableLocation = new Point(0, statText.Location.Y + statText.Height + 3);
				for (int i = 0; i < rowCount; i++)
				{
					int columnCount = menuReportHelper.GetTableColumnCount(i);
					TableLayoutPanel tablePanel = new TableLayoutPanel();
					tablePanel.Margin = Padding.Empty;
					tablePanel.Padding = Padding.Empty;
					tablePanel.RowCount = 1;
					tablePanel.ColumnCount = columnCount;
					tablePanel.Width = overviewMetroTabPage.Width;
					tablePanel.Height = menuReportHelper.GetRowHeight(overviewMetroTabPage.Height, i);
					tablePanel.BackColor = Color.Transparent;
					tablePanel.Location = tableLocation;
					tableLocation.Y += tablePanel.Height;
					for (int j = 0; j < columnCount; j++)
					{
						ColumnStyle cs;
						if (j == columnCount - 1)
						{
							cs = new ColumnStyle(SizeType.AutoSize);
						}
						else
						{
							cs = new ColumnStyle(SizeType.Absolute, menuReportHelper.GetImageWidth(overviewMetroTabPage.Width, i, j));
						}

						tablePanel.ColumnStyles.Add(cs);
						PictureBox pb = new PictureBox();
						pb.Padding = Padding.Empty;
						pb.Margin = Padding.Empty;
						tablePanel.Controls.Add(pb, j, 0);
						pb.Image = menuReportHelper.GetImage(i, j);
						pb.SizeMode = PictureBoxSizeMode.Zoom;
						pb.Dock = DockStyle.Fill;
						overViewPictureBoxes.Add(pb);
					}
					overviewMetroTabPage.Controls.Add(tablePanel);
				}
			}
		}

		private void DisplayedReportsTimer_Tick(object sender, EventArgs e)
		{
			foreach (var pb in overViewPictureBoxes)
			{
				pb.Image = ImageTransformations.DisableImage(pb.Image);
			}
			displayedReportsTimer.Stop();
		}

		private void MenuReportHelper_ImagesChanged(object sender, EventArgs e)
		{
			displayedReportsTimer.Stop();
			refreshOverview();
			displayedReportsTimer.Start();
		}

		public bool PreFilterMessage(ref Message m)
		{
			if (!Visible || !this.CheckCursorIsInsideControl()) return false;
			if (m.Msg == (int)WinApi.Messages.WM_MOUSEWHEEL)
			{
				var scrollDelta = (short)(((long)m.WParam >> 16) & 0xffff);
				scrollDelta = (short)((scrollDelta < 0 ? scrollDelta - 2 : scrollDelta + 2) / 3); //take one third of the original value
				workGrid1.ScrollDelta(scrollDelta);
				favoriteReportsScrollBar.ScrollDelta(scrollDelta);
				foreach (MetroTabPage tabPage in metroTabControl.TabPages)
				{
					foreach (Control control in tabPage.Controls)
					{
						if (control is Controls.ScrollBar scrollBar)
							scrollBar.ScrollDelta(scrollDelta);
					}
				}
				return true;
			}

			if (m.Msg == (int)WinApi.Messages.WM_KEYDOWN)
			{
				if ((long)m.WParam == (long)Keys.C && ModifierKeys == Keys.Control)
				{
					log.Debug("UI - Ctrl + C pressed in new menu");
					TelemetryHelper.RecordFeature("MainMenu", "Copy");
					var selection = searchBox1.Selection;
					if (selection != null)
					{
						Debug.Assert(selection.WorkData.Id != null);
						log.DebugFormat("Work {0} copied from search", selection.WorkData.Id);
						ClipboardHelper.SetClipboardData(selection);
						return true;
					}

					selection = workGrid1.Selection;
					if (selection != null)
					{
						Debug.Assert(selection.WorkData.Id != null);
						log.DebugFormat("Work {0} copied from work grid", selection.WorkData.Id);
						ClipboardHelper.SetClipboardData(selection);
						return true;
					}

					//This should be the third option. (Until a fix for highlighting issues.)
					selection = currentWork1.Selection;
					if (selection != null)
					{
						Debug.Assert(selection.WorkData.Id != null);
						log.DebugFormat("Work {0} copied from current work", selection.WorkData.Id);
						ClipboardHelper.SetClipboardData(selection);
						return true;
					}

				}
			}

			return false;
		}

		public void SetCurrentWork(WorkData work, string taskName, bool active)
		{
			currentWork1.SetWork(work);
			currentWork1.Active = active;
		}
		public void Show(Point position)
		{
			Rectangle screenBounds = Screen.FromPoint(position).WorkingArea;
			Location = new Point(
				position.X,
				position.Y - (screenBounds.Height / 2 > position.Y ? 0 : Height)
				);
			if (Location.Y < screenBounds.Top) Location = new Point(Location.X, screenBounds.Top);
			if (Location.X < screenBounds.Left) Location = new Point(screenBounds.Left, Location.Y);
			if (Height > screenBounds.Height) Height = screenBounds.Height;
			if (Location.X + Width > screenBounds.Right) Location = new Point(screenBounds.Right - Width, Location.Y);
			if (Location.Y + Height > screenBounds.Bottom) Location = new Point(Location.X, screenBounds.Bottom - Height);
			UpdateData();
			Show();
			Activate();
			workGrid1.SetClientMenuLookup(MenuQuery.Instance.ClientMenuLookup.Value);
			workGrid1.ClearSelection();
			searchBox1.Text = string.Empty;
			searchBox1.Focus();
			ThreadPool.QueueUserWorkItem(_ =>
			{
				var quitEnabled = !AppControlServiceHelper.Ping();
				parent.GuiContext.Post(__ =>
				{
					Application.DoEvents();
					userDisplay.QuitEnabled = quitEnabled;
				}, null);
			});
		}

		public void UpdateAllWorks(List<WorkData> allWorkDatas)
		{
			workGrid1.UpdateAllWorks(allWorkDatas);
			searchBox1.UpdateAllWorks(allWorkDatas);
		}

		public void UpdateSimpleStats(SimpleWorkTimeStats stats)
		{
			UpdateData();
		}

		public void UpdateStats(TimeSpan todaysWork, ClientWorkTimeStats stats)
		{
			statGrid.DayUsed = todaysWork;
			statGrid.DaySum = null;
			statGrid.BaseRowUsed = stats != null ? (TimeSpan?)stats.ThisMonthsWorkTime.NetWorkTime : null;
			statGrid.BaseRowSum = stats != null ? (TimeSpan?)stats.ThisMonthsTargetNetWorkTime : null;
			statGrid.ExtraRowUsed = stats != null ? (TimeSpan?)stats.ThisWeeksWorkTime.NetWorkTime : null;
			statGrid.ExtraRowSum = stats != null ? (TimeSpan?)stats.ThisWeeksTargetNetWorkTime : null;
			statGrid.ExtraRowDelta = stats != null
				? (TimeSpan?)stats.ThisWeeksWorkTime.NetWorkTime - stats.ThisWeeksTargetUntilTodayNetWorkTime
				: null;
			statGrid.BaseRowDelta = stats != null
				? (TimeSpan?)stats.ThisMonthsWorkTime.NetWorkTime - stats.ThisMonthsTargetUntilTodayNetWorkTime
				: null;
		}

		public void UpdateStats(WorkTimeStats stats)
		{
			statGrid.DayUsed = TimeSpan.FromMilliseconds(stats.TodaysWorkTimeInMs);
			statGrid.DaySum = TimeSpan.FromMilliseconds(stats.TodaysTargetNetWorkTimeInMs);
			var targetTime2 = targetTimeFuncs[SecondRow](stats); // no data if target is 0
			statGrid.BaseRowUsed = targetTime2 > 0L ? (TimeSpan?)TimeSpan.FromMilliseconds(workTimeFuncs[SecondRow](stats)) : null;
			statGrid.BaseRowSum = targetTime2 > 0L ? (TimeSpan?)TimeSpan.FromMilliseconds(targetTime2) : null;
			statGrid.BaseRowDelta = targetTime2 > 0L ? (TimeSpan?)TimeSpan.FromMilliseconds(workTimeFuncs[SecondRow](stats) - targetUntilTodayTimeFuncs[SecondRow](stats)) : null;
			var targetTime = targetTimeFuncs[FirstRow](stats);
			statGrid.ExtraRowUsed = targetTime > 0L ? (TimeSpan?)TimeSpan.FromMilliseconds(workTimeFuncs[FirstRow](stats)) : null;
			statGrid.ExtraRowSum = targetTime > 0L ? (TimeSpan?)TimeSpan.FromMilliseconds(targetTime) : null;
			statGrid.ExtraRowDelta = targetTime > 0L ? (TimeSpan?)TimeSpan.FromMilliseconds(workTimeFuncs[FirstRow](stats) - targetUntilTodayTimeFuncs[FirstRow](stats)) : null;
		}

		public void UpdateStats(bool calculating, TimeSpan todaysWork)
		{
			statGrid.DayUsed = todaysWork;
			statGrid.DaySum = null;
			statGrid.BaseRowUsed = null;
			statGrid.BaseRowSum = null;
			statGrid.BaseRowDelta = null;
			statGrid.ExtraRowUsed = null;
			statGrid.ExtraRowSum = null;
			statGrid.ExtraRowDelta = null;
		}

		private void HandleConfigChanged(object sender, EventArgs e)
		{
			var rows = worktimeStatIntervalEnums.Count(mask => (ConfigManager.LocalSettingsForUser.DisplayWorktimeStats & mask) == mask);
			statGrid.ExtraRowVisible = rows > 1;
			statGrid.DeltaVisible = parent.ShowDelta;
			statGrid.SumVisible = parent.ShowSum;
			statGrid.ExtraRowTitle = textFuncs[FirstRow]();
			statGrid.BaseRowTitle = textFuncs[SecondRow]();
			metroTabControl.Height = statGrid.ExtraRowVisible ? 392 : 418;
			foreach (Control tabPage in metroTabControl.TabPages)
			{
				tabPage.Height = statGrid.ExtraRowVisible ? 350 : 376;
				Controls.ScrollBar scrollBar = null;
				Panel panel = null;
				foreach (Control control in tabPage.Controls)
				{
					if (control is Controls.ScrollBar) scrollBar = (Controls.ScrollBar)control;
					if (control is Panel) panel = (Panel)control;
				}
				if (panel != null && scrollBar != null)
				{
					panel.Height = panel.PreferredSize.Height;
					RefreshTabPageScroll(scrollBar, tabPage, panel);
				}
			}
		}

		private void HandleCreateWorkClicked(object sender, EventArgs e)
		{
			parent.DisplayCreateWorkGui();
		}

		private void HandleProjectUploadClicked(object sender, EventArgs e)
		{
			parent.DisplayProjectSyncGui();
		}

		private void HandleDeactivated(object sender, EventArgs e)
		{
			if ((!searchBox1.DropdownShown || !searchBox1.DropdownFocus) && !workGrid1.DropdownShown &&
				!userDisplay.DropdownShown)
			{
				log.Debug("UI - Deactivated");
				TelemetryHelper.RecordFeature("MainMenu", "Close");
				Hide();
			}

			if (searchBox1.DropdownShown && !searchBox1.DropdownFocus) searchBox1.HideDropdown();
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);

			if (!Visible)
			{
				currentWork1.ClearSelection();
				searchBox1.ClearSelection();
				workGrid1.ClearSelection();
			}
		}

		private void HandleDropdownClosed(object sender, EventArgs e)
		{
			if (IsForeground)
			{
				log.Debug("Dropdown closed but menu still active");
				Activate();
			}
			else
			{
				log.Debug("Dropdown closed with menu");
				Hide();
			}
		}

		public long AddEtcExtraMenuitem(Func<string> textAccessor, Action clickHandler)
		{
			return userDisplay.AddEtcExtraMenuitem(textAccessor, clickHandler);
		}

		public void RemoveEtcExtraMenuitem(long menuid)
		{
			userDisplay.RemoveEtcExtraMenuitem(menuid);
		}

		public void ShowWorkHistory()
		{
#if NET4
			parent.HandleWorkTimeService.ShowModification();
#endif
		}

		private void HandleStatsClicked(object sender, EventArgs e)
		{
			log.Debug("UI - WorktimeHistory clicked");
#if NET4
			ShowWorkHistory();
#endif
		}

		private void HandleLoaded(object sender, EventArgs e)
		{
			this.FillAccessibilityFields();
			userDisplay.FillAccessibilityFields();
			statGrid.FillAccessibilityFields();
			Application.AddMessageFilter(this);
			CreateShadow();
			UpdateFavoriteReports(menuReportHelper.FavoriteReports);
			Localize();
			displayedReportsTimer.Interval = DisplayedReportsOutDatedInterval;
			displayedReportsTimer.Tick += DisplayedReportsTimer_Tick;
			menuReportHelper.ImagesChanged += MenuReportHelper_ImagesChanged;
			menuReportHelper.FavoriteReportsChanged += MenuReportHelper_FavoriteReportsChanged;
			if (menuReportHelper.IsFeatureEnabled)
			{
				enableReportsInMenu();
				refreshOverview();
			}
			else
			{
				disableReportsInMenu();
			}
			enableCustomTabNamesInMenu();
		}

		public void Localize()
		{
			tasksMetroTabPage.Text = Labels.Tasks;
			favoriteReportsMetroTabPage.Text = Labels.ContextMenu_FavoriteReports;
			overviewMetroTabPage.Text = Labels.ContextMenu_Overview;
			refreshFavoritesButton.Text = Labels.ContextMenu_Refresh;
			statGrid.ExtraRowTitle = textFuncs[FirstRow]();
			statGrid.BaseRowTitle = textFuncs[SecondRow]();
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			log.Debug("Closing context menu canClose: " + canClose + " reason: " + e.CloseReason);
			if (e.CloseReason != CloseReason.WindowsShutDown)
			{
				e.Cancel = !canClose;
				if (!canClose)
				{
					Quit();
				}
			}
			else
			{
				log.Info("Closing context menu due to shutdown");
				TelemetryHelper.RecordFeature("MainMenu", "Shutdown");
				Quit(true);
			}
			base.OnFormClosing(e);
		}

		protected override void OnClosed(EventArgs e)
		{
			Microsoft.Win32.SystemEvents.UserPreferenceChanged -= UserPreferenceChanged;
			Application.RemoveMessageFilter(this);
			base.OnClosed(e);
		}

		private void HandlePreferenceClicked(object sender, EventArgs e)
		{
			parent.ShowPreferences();
		}

		private void HandleQuitClicked(object sender, EventArgs e)
		{
			Quit();
		}

		private void Quit(bool force = false)
		{
			Hide();
			canClose = true;
			parent.Exit(force);
			canClose = false;
		}

		private void HandleSearchboxDropdownClosed(object sender, EventArgs e)
		{
			if (!ContainsFocus) Hide();
			SuppressHide = false;
		}

		private void HandleWorkClicked(object sender, WorkDataEventArgs e)
		{
			if (searchBox1.DropdownShown) searchBox1.HideDropdown();
			Hide();
			EventHandler<WorkDataEventArgs> evt = WorkClick;
			if (evt != null) evt(sender, e);
		}

		private void CreateShadow()
		{
			shadowForm = new Shadow(this, 6);
		}

		private void UpdateData()
		{
			userDisplay.UserName = ConfigManager.IsAnonymModeEnabled ? ConfigManager.AppNameOverride ?? ConfigManager.ApplicationName : ConfigManager.UserName;
			userDisplay.UserId = ConfigManager.IsAnonymModeEnabled ? null : ConfigManager.UserId.ToString();
			userDisplay.WebVisible = !ConfigManager.IsAnonymModeEnabled;
		}

		private void HandleUserChangedClick(object sender, EventArgs e)
		{
			parent.Logout();
		}

		private void HandleHelpClick(object sender, EventArgs e)
		{
			parent.ShowHelp();
		}

		private void HandleRulesClicked(object sender, EventArgs e)
		{
			Hide();
			parent.ShowRules();
		}

		private void HandleErrorReportClicked(object sender, EventArgs e)
		{
			Hide();
			parent.ShowErrorReport();
		}

		private void HandleClosed(object sender, FormClosedEventArgs e)
		{
			workGrid1.SavePath();
		}

		private void MenuReportHelper_FavoriteReportsChanged(object sender, IEnumerable<FavoriteReport> e)
		{
			Invoke(new Action(() => UpdateFavoriteReports(e)));
		}

		public void UpdateFavoriteReports(IEnumerable<FavoriteReport> reports)
		{
			favoriteReportsFlowLayoutPanel.Controls.Clear();
			if (reports == null)
			{
				favoriteReportsRefreshScroll();
				favoriteReportsScrollTo(0);
				return;
			}
			foreach (var favoriteReport in reports)
			{
				Bitmap bmp;
				using (MemoryStream ms = new MemoryStream(favoriteReport.Icon))
				{
					bmp = new Bitmap(ms);
				}
				var fvb = new FavoriteReportBox(favoriteReport.Name, bmp, favoriteReport.Url);
				favoriteReportsFlowLayoutPanel.Controls.Add(fvb);
			}
			favoriteReportsRefreshScroll();
			favoriteReportsScrollTo(0);
		}

		private void favoriteReportsScrollTo(int verticalPosition)
		{
			if (verticalPosition < 0) verticalPosition = 0;
			if (verticalPosition > favoriteReportsScrollBar.ScrollTotalSize - favoriteReportsScrollBar.ScrollVisibleSize) verticalPosition = favoriteReportsScrollBar.ScrollTotalSize - favoriteReportsScrollBar.ScrollVisibleSize;
			favoriteReportsScrollBar.Value = verticalPosition;
			if (verticalPosition < 0)
				favoriteReportsFlowLayoutPanel.Location = new Point(0, 0);
			else
				favoriteReportsFlowLayoutPanel.Location = new Point(0, -verticalPosition);
		}

		private void favoriteReportsRefreshScroll()
		{
			favoriteReportsFlowLayoutPanel.Height = favoriteReportsFlowLayoutPanel.PreferredSize.Height;
			favoriteReportsScrollBar.ScrollTotalSize = favoriteReportsFlowLayoutPanel.Height;
			favoriteReportsScrollBar.ScrollVisibleSize = favoritePanel.Height;
			favoriteReportsScrollTo(favoriteReportsScrollBar.Value);
		}

		private void refreshFavoritesButton_Click(object sender, EventArgs e)
		{
			log.Debug("Refresh favorite reports button pressed.");
			ThreadPool.QueueUserWorkItem(x =>
			{
				menuReportHelper.RefreshFavoriteReportsNoCheck();
				menuReportHelper.ResetLastQueryTime();
				menuReportHelper.RefreshDisplayedReports();
			}, null);
		}

		private void favoriteReportsScrollBar_ScrollChanged(object sender, EventArgs e)
		{
			favoriteReportsScrollTo(favoriteReportsScrollBar.Value);
		}

		private void enableCustomTabNamesInMenu()
		{
			if (!tableLayoutPanel1.Controls.Contains(workGrid1))
			{
				foreach (var clientTab in menuTabHelper.GetTabNames())
				{
					metroTabControl.TabPages.Add(clientTab.TabId, clientTab.LocalizedTitle);
				}
				return;
			}
			tableLayoutPanel1.Controls.Remove(workGrid1);
			tableLayoutPanel1.Controls.Add(metroTabControl, 0, 5);
			tasksMetroTabPage.Controls.Add(workGrid1);
			workGrid1.Anchor = AnchorStyles.None;
			workGrid1.AutoSize = false;
			workGrid1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
			workGrid1.Location = new System.Drawing.Point(0, 0);
			workGrid1.Margin = new System.Windows.Forms.Padding(0);
			workGrid1.Size = tasksMetroTabPage.ClientSize;
			workGrid1.TabIndex = 3;
			metroTabControl.TabPages.Remove(favoriteReportsMetroTabPage);
			metroTabControl.TabPages.Remove(overviewMetroTabPage);
			foreach (var clientTab in menuTabHelper.GetTabNames())
			{
				metroTabControl.TabPages.Add(clientTab.TabId, clientTab.LocalizedTitle);
				metroTabControl.TabPages[clientTab.TabId].Cursor = Cursors.Arrow;
			}
		}

		private void disableCustomTabNamesInMenu()
		{
			if (tableLayoutPanel1.Controls.Contains(workGrid1)) return;
			tasksMetroTabPage.Controls.Clear();
			tableLayoutPanel1.Controls.Remove(metroTabControl);
			tableLayoutPanel1.Controls.Add(workGrid1, 0, 6);
			workGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			workGrid1.AutoSize = true;
			workGrid1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			workGrid1.Location = new System.Drawing.Point(0, 214);
			workGrid1.Margin = new System.Windows.Forms.Padding(0);
			workGrid1.Size = tableLayoutPanel1.ClientSize;
			workGrid1.TabIndex = 6;
			if (menuReportHelper.IsFeatureEnabled)
			{
				enableReportsInMenu();
			}
		}

		private void refreshCustomTabNames(List<ClientTab> oldTabNames)
		{
			//We should do this on bg thread, but there are just 1 or 2 tabs atm
			var newTabNames = menuTabHelper.GetTabNames();
			foreach (var oldTabName in oldTabNames)
			{
				if (newTabNames.Any(x => x.TabId == oldTabName.TabId && x.LocalizedTitle == oldTabName.LocalizedTitle)) continue;
				metroTabControl.TabPages.RemoveByKey(oldTabName.TabId);
			}
			foreach (var newClientTab in menuTabHelper.GetTabNames())
			{
				if (oldTabNames.Any(x => x.TabId == newClientTab.TabId && x.LocalizedTitle == newClientTab.LocalizedTitle)) continue;
				MetroTabPage metroTabPage = new MetroTabPage()
				{
					Location = new System.Drawing.Point(4, 38),
					Name = newClientTab.TabId,
					Size = new System.Drawing.Size(312, 376),
					TabIndex = 2,
					Text = newClientTab.LocalizedTitle,
					Cursor = System.Windows.Forms.Cursors.Arrow

				};
				metroTabControl.TabPages.Add(metroTabPage);
			}
		}

		private void requestTabContent(string tabId)
		{
			ThreadPool.QueueUserWorkItem(_ => menuTabHelper.RequestTabContent(tabId, x => Invoke(new Action(() => RefreshTabContent(tabId, x))), () => Invoke(new Action(() => TabContentQueryError(tabId)))));
		}

		internal void RefreshTabContent(string tabId, List<MenuTabRow> tabRows)
		{
			if (!metroTabControl.TabPages.ContainsKey(tabId))
			{
				log.Warn($"Tab control doesn't contains key: {tabId}");
				return;
			}
			var tabPage = metroTabControl.TabPages[metroTabControl.TabPages.IndexOfKey(tabId)];
			tabPage.Controls.Clear();
			Panel panel = new Panel
			{
				Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right))),
				BackColor = System.Drawing.Color.Transparent,
				Location = new System.Drawing.Point(0, 0),
				Margin = Padding.Empty,
				Name = tabId + "Panel",
				Padding = Padding.Empty,
				Size = new System.Drawing.Size(FullWidth - 15, 345),
				TabIndex = 0,
			};
			tabPage.Controls.Add(panel);
			var scrollBar = new Tct.ActivityRecorderClient.View.Controls.ScrollBar();
			scrollBar.Dock = System.Windows.Forms.DockStyle.Right;
			scrollBar.Location = new System.Drawing.Point(FullWidth - 15, 0);
			scrollBar.Name = tabId + "ScrollBar";
			scrollBar.ScrollSpeed = 1F;
			scrollBar.ScrollTotalSize = 100;
			scrollBar.ScrollVisibleSize = 10;
			scrollBar.Size = new System.Drawing.Size(7, 598);
			scrollBar.TabIndex = 1;
			scrollBar.Value = 0;
			scrollBar.ScrollChanged += new System.EventHandler(HandleTabPageScrolled);
			tabPage.Controls.Add(scrollBar);
			int currentY = 3;
			int currentRow = 0;

			foreach (var row in tabRows)
			{
				TableLayoutPanel tablePanel = new TableLayoutPanel();
				tablePanel.Margin = Padding.Empty;
				tablePanel.Padding = Padding.Empty;
				tablePanel.RowCount = 1;
				tablePanel.ColumnCount = row.Elements.Count;
				tablePanel.Width = panel.Width;
				tablePanel.BackColor = Color.Transparent;
				tablePanel.Location = new Point(0, currentY);
				for (int j = 0; j < row.Elements.Count; j++)
				{
					ColumnStyle cs = new ColumnStyle(SizeType.AutoSize);

					tablePanel.ColumnStyles.Add(cs);

					var element = row.Elements[j];
					switch (element.Type)
					{
						case MenuTabRow.ElementType.Image:
							PictureBox pb = new PictureBox();
							pb.Padding = Padding.Empty;
							pb.Margin = Padding.Empty;
							pb.Image = element.Image;
							pb.SizeMode = PictureBoxSizeMode.Zoom;
							pb.Size = pb.Image.Size;
							if (pb.Width > tablePanel.Width)
							{
								double scalePercent = ((double)tablePanel.Width / (double)pb.Width);
								pb.Width = tablePanel.Width;
								pb.Height = (int)(pb.Height * scalePercent);
							}
							tablePanel.Controls.Add(pb, j, 0);
							break;
						case MenuTabRow.ElementType.Table:
							var dataGridView = createDataGridView(element.Table);
							tablePanel.Controls.Add(dataGridView, j, 0);
							break;
						case MenuTabRow.ElementType.Link:
							LinkWithIconUserControl lwiuc = new LinkWithIconUserControl();
							lwiuc.Image = element.Image;
							lwiuc.Location = new Point(0, 0);
							lwiuc.Text = element.Text;
							lwiuc.Tag = element.Url;
							lwiuc.Click += new EventHandler(LinkWithIconUserControl_Click);
							
							tablePanel.Controls.Add(lwiuc);
							break;
						case MenuTabRow.ElementType.StatText:
							var stat = new StatText();
							stat.Location = new Point(0, 0);
							stat.Size = new Size(panel.Width, stat.Height);
							stat.SetText(menuTabHelper.DisplayedTabsLastQueryTime?.ToString(DateTimeFormatInfo.CurrentInfo.ShortTimePattern));
							stat.setProgressSpinner(false);
							stat.StatsClicked += HandleStatsClicked;
							panel.Controls.Add(stat);
							panel.Height = panel.PreferredSize.Height;
							break;
						case MenuTabRow.ElementType.Label:
							var label = new SmartLabel();
							label.AutoSize = true;
							label.AutoWrap = false;
							// Can't be transparent :(
							label.BackColor = SystemColors.Window;
							label.Font = element.IsSmallCaps ? element.IsBold ? boldFontSM : normalFontSM : element.IsBold ? boldFont : normalFont;
							label.FontSize = 10F;
							label.HorizontalAlignment = HorizontalAlignment.Left;
							label.Location = new Point(3, 0);
							if (element.IsBold) label.AddWeightChange();
							label.AddText(element.IsSmallCaps ? element.Text.ToUpper() : element.Text, true).RenderText();
							label.Size = new Size(label.PreferredWidth, label.PreferredSize.Height);
							if (label.Width > tablePanel.Width)
							{
								label.Height *= (label.Width / tablePanel.Width + 1);
								label.Width = tablePanel.Width;
							}
							label.RenderText();
							tablePanel.Controls.Add(label);
							break;
						case MenuTabRow.ElementType.FavoriteReport:
							var favReport = new FavoriteReportBox(element.Text, element.Image, element.Url);
							tablePanel.Controls.Add(favReport);
							break;
						case MenuTabRow.ElementType.FeatureLink:
							LinkWithIconUserControl featurelwiuc = new LinkWithIconUserControl();
							featurelwiuc.Image = MenuTabRow.GetFeatureImage(element.FeatureLink);
							featurelwiuc.Location = new Point(0, 0);
							featurelwiuc.Text = element.Text;
							featurelwiuc.Tag = element.FeatureLink;
							featurelwiuc.Click += new EventHandler(FeatureLink_Click);
							tablePanel.Controls.Add(featurelwiuc);
							break;
						default:
							break;
					}
				}
				if (row.HeightPercent != -1)
				{
					tablePanel.Height = panel.Height * row.HeightPercent / 100;
					foreach (Control c in tablePanel.Controls)
					{
						c.Height = tablePanel.Height - 6;
						foreach (Control c2 in c.Controls)
						{
							c2.Location = new Point(0, 0);
						}
						c.Width = c.Width * row.HeightPercent / 100;
					}
					tablePanel.Width = panel.Width;
				}
				else
					tablePanel.Height = tablePanel.PreferredSize.Height;
				currentY += tablePanel.Height + 3;
				panel.Controls.Add(tablePanel);
				panel.Height = panel.PreferredSize.Height;
				currentRow++;
			}
			RefreshTabPageScroll(scrollBar, tabPage, panel);
		}

		private void FeatureLink_Click(object sender, EventArgs e)
		{
			var senderControl = (Control)sender;
			var feature = ((MenuTabRow.Feature)senderControl.Tag);
			switch (feature)
			{
				case MenuTabRow.Feature.Messaging:
					Platform.Factory.GetMessageService().ShowMessages();
					break;
				default:
					break;
			}
		}

		private void LinkWithIconUserControl_Click(object sender, EventArgs e)
		{
			var lwiuc = (LinkWithIconUserControl)sender;
			RecentUrlQuery.Instance.OpenUrl((string)lwiuc.Tag);
		}

		private void HandleTabPageScrolled(object sender, EventArgs e)
		{
			var scrollBar = (Controls.ScrollBar)sender;
			ScrollTabPageTo(scrollBar.Parent, scrollBar.Value);
		}

		private void ScrollTabPageTo(Control tabPage, int verticalPosition)
		{
			foreach (Control control in tabPage.Controls)
			{
				if (control is Panel)
					control.Location = new Point(0, -verticalPosition);
			}
		}

		private void RefreshTabPageScroll(Controls.ScrollBar scrollBar, Control tabPage, Panel panel)
		{
			scrollBar.ScrollTotalSize = panel.Height;
			scrollBar.ScrollVisibleSize = tabPage.Height;
			ScrollTabPageTo(tabPage, scrollBar.Value);
		}

		private DataGridView createDataGridView(MenuTabRow.Table table)
		{
			var dataGridView = new DataGridView();
			dataGridView.AllowUserToAddRows = false;
			dataGridView.AllowUserToDeleteRows = false;
			dataGridView.AllowUserToResizeColumns = false;
			dataGridView.AllowUserToResizeRows = false;
			dataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
			dataGridView.BackgroundColor = System.Drawing.Color.White;
			dataGridView.BorderStyle = System.Windows.Forms.BorderStyle.None;
			dataGridView.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
			dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			dataGridView.ColumnHeadersVisible = false;
			dataGridView.CellMouseEnter += DataGridView_CellMouseEnter;
			dataGridView.Enabled = true;
			dataGridView.Location = new System.Drawing.Point(0, 0);
			dataGridView.MultiSelect = false;
			dataGridView.Name = "";
			dataGridView.ReadOnly = false;
			dataGridView.RowHeadersVisible = false;
			dataGridView.ScrollBars = System.Windows.Forms.ScrollBars.None;
			dataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
			dataGridView.ShowCellToolTips = false;
			dataGridView.Size = new System.Drawing.Size(10, 10);
			dataGridView.TabIndex = 0;
			dataGridView.SelectionChanged += new System.EventHandler((x, y) => dataGridView.ClearSelection());
			dataGridView.CellBorderStyle = DataGridViewCellBorderStyle.None;
			dataGridView.Tag = table;
			for (int i = 0; i < table.ColumnCount; i++)
			{
				dataGridView.Columns.Add("", "");
			}
			dataGridView.Rows.Add(table.RowCount);
			var boldFont = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			var normalFont = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			var boldFontSM = new System.Drawing.Font("Segoe UI", 7.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			var normalFontSM = new System.Drawing.Font("Segoe UI", 7.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			for (int i = 0; i < table.ColumnCount; i++)
			{
				for (int j = 0; j < table.RowCount; j++)
				{
					var cell = table.Content[j, i];
					var minHeight = 20;
					if (cell.Image != null) {
						minHeight = minHeight < cell.Image.Height ? cell.Image.Height : minHeight;
						dataGridView.Columns[i].MinimumWidth = dataGridView.Columns[i].MinimumWidth < cell.Image.Height ? cell.Image.Height : dataGridView.Columns[i].MinimumWidth;
					}
					dataGridView.Rows[j].MinimumHeight = minHeight;
					dataGridView.Rows[j].Cells[i].Value = cell.IsSmallCaps ? cell.Content.ToUpper() : cell.Content;
					dataGridView.Rows[j].Cells[i].Style.BackColor = cell.BackgroundColor;
					dataGridView.Rows[j].Cells[i].Style.ForeColor = cell.TextColor;
					dataGridView.Rows[j].Cells[i].Style.Font = cell.IsSmallCaps ? cell.IsBold ? boldFontSM : normalFontSM : cell.IsBold ? boldFont : normalFont;
					dataGridView.Rows[j].Cells[i].Style.Alignment = cell.TextAlign;
				}
			}
			dataGridView.Size = dataGridView.PreferredSize;
			DataGridViewElementStates states = DataGridViewElementStates.None;
			dataGridView.Height = dataGridView.Rows.GetRowsHeight(states);
			dataGridView.CellPainting += dataGridView_CellPainting;
			return dataGridView;
		}

		private void DataGridView_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
		{
			if (e.ColumnIndex >= 0 & e.RowIndex >= 0 && sender is DataGridView)
			{
				var currentDgv = (DataGridView)sender;
				var table = (MenuTabRow.Table)currentDgv.Tag;
				var cell = table.Content[e.RowIndex, e.ColumnIndex];
				metroToolTip.SetToolTip(currentDgv, cell.Tooltip);
			}
		}

		private void dataGridView_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
		{
			var table = (MenuTabRow.Table)((DataGridView)sender).Tag;
			e.Paint(e.ClipBounds, DataGridViewPaintParts.All);
			using (Brush gridBrush = new SolidBrush(((DataGridView)sender).GridColor))
			{
				using (Pen gridLinePen = new Pen(gridBrush))
				{
					var cell = table.Content[e.RowIndex, e.ColumnIndex];
					if (cell.BottomBorder)
						e.Graphics.DrawLine(gridLinePen, e.CellBounds.Left,
						e.CellBounds.Bottom - 1, e.CellBounds.Right - 1,
						e.CellBounds.Bottom - 1);
					if (cell.RightBorder)
						e.Graphics.DrawLine(gridLinePen, e.CellBounds.Right - 1,
						e.CellBounds.Top, e.CellBounds.Right - 1,
						e.CellBounds.Bottom - 1);
					if (cell.LeftBorder)
						e.Graphics.DrawLine(gridLinePen, e.CellBounds.Left,
						e.CellBounds.Top - 1, e.CellBounds.Left,
						e.CellBounds.Bottom - 1);
					if (cell.TopBorder)
						e.Graphics.DrawLine(gridLinePen, e.CellBounds.Left,
						e.CellBounds.Top, e.CellBounds.Right - 1,
						e.CellBounds.Top);
					if (cell.Image != null)
						e.Graphics.DrawImageUnscaled(cell.Image, e.CellBounds.Left, e.CellBounds.Top);

					e.Handled = true;
				}
			}
		}

		internal void TabContentQueryError(string tabId)
		{
			log.Error($"Couldn't get tab content, id: {tabId}");
		}

		private void metroTabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			string selectedTabName = metroTabControl.TabPages[metroTabControl.SelectedIndex].Name;
			if (menuTabHelper.GetTabNames().Any(x => x.TabId == selectedTabName))
			{
				requestTabContent(selectedTabName);
			}
		}
	}
}