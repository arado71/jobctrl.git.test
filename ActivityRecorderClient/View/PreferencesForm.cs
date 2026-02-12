using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Hotkeys;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Notification;
using Tct.ActivityRecorderClient.Properties;
using log4net;
using MetroFramework.Controls;
using Tct.ActivityRecorderClient.Taskbar;
using Tct.ActivityRecorderClient.Telemetry;
using Tct.ActivityRecorderClient.View.Navigation;

namespace Tct.ActivityRecorderClient.View
{
	public partial class PreferencesForm : FixedMetroForm, ILocalizableControl
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly IHotkeyService hotkeyService;
		private readonly BindingList<HotkeySetting> hotkeySettings = new BindingList<HotkeySetting>();
		private ClientMenuLookup clientMenuLookup;
		private bool settingsLoading = true;
		private Dictionary<ToggleGroup, Tuple<Action<bool>, Func<bool>>> buttonMap;
		private List<ToggleGroup> checkedToggles;

		public DesktopCapture DesktopCapture { get; set; }
		public ActivityRecorderForm MainForm { get; private set; }

		private Control GetFocusedControl()
		{
			Control focusedControl = null;
			// To get hold of the focused control:
			IntPtr focusedHandle = WinApi.GetFocus();
			if (focusedHandle != IntPtr.Zero)
				// Note that if the focused Control is not a .Net control, then this will return null.
				focusedControl = Control.FromHandle(focusedHandle);
			return focusedControl;
		}

		public PreferencesForm()
		{
			Icon = Resources.JobCtrl; //don't set it in the designer as it would enlarge the exe
			InitializeComponent();
		}

		public PreferencesForm(ActivityRecorderForm owner, IHotkeyService hotkeyService, ClientMenuLookup clientMenuLookup)
		{
			Icon = Resources.JobCtrl; //don't set it in the designer as it would enlarge the exe
			this.hotkeyService = hotkeyService;
			this.clientMenuLookup = clientMenuLookup;
			MainForm = owner;
			InitializeComponent();
			buttonMap = new Dictionary<ToggleGroup, Tuple<Action<bool>, Func<bool>>>
			{
				{ tgWeekly, new Tuple<Action<bool>, Func<bool>>(value => MainForm.ShowWeeklyStats = value, () => MainForm.ShowWeeklyStats ) },
				{ tgMonthly, new Tuple<Action<bool>, Func<bool>>(value => MainForm.ShowMonthlyStats= value, () => MainForm.ShowMonthlyStats ) },
				{ tgQuarterly, new Tuple<Action<bool>, Func<bool>>(value => MainForm.ShowQuarterlyStats= value, () => MainForm.ShowQuarterlyStats ) },
				{ tgYearly, new Tuple<Action<bool>, Func<bool>>(value => MainForm.ShowYearlyStats= value, () => MainForm.ShowYearlyStats) },
			};
			LoadLanguages();
			Localize();
		}

		public void UpdateClientMenuLookup(ClientMenuLookup menuLookup)
		{
			clientMenuLookup = menuLookup;
			BindWorkDataId(workDataIdColumn);
			ValidateAllHotkeySettings();
		}

		#region Form initialization

		private void LoadDblClick()
		{
			cbDblClick.DataSource = new[]
			{
				new ComboBoxElement(Labels.Preference_DoubleClick, true),
				new ComboBoxElement(Labels.Preference_SingleClick, false)
			};
			if (!ConfigManager.DisableManualStatusChange && ConfigManager.LocalSettingsForUser.UseDoubleClickForStatusChange.HasValue)
				cbDblClick.SelectedItem =
					cbDblClick.Items.Cast<ComboBoxElement>()
						.FirstOrDefault(x => (bool?) x.Value == ConfigManager.LocalSettingsForUser.UseDoubleClickForStatusChange);
			else
			{
				cbDblClick.DataSource = new[]
				{
					new ComboBoxElement(Labels.Preference_Hidden, true)
				};
				cbDblClick.Enabled = false;
			}
		}

		private void LoadHotkeySettings()
		{
			gvHotKey.AutoGenerateColumns = false;
			BindKeyCode(keyCodeColumn);
			BindHotkeyActionType(actionTypeColumn);
			BindWebsites(websiteColumn);
			BindWorkDataId(workDataIdColumn);
			gvHotKey.DataSource = hotkeySettings;
			LoadHotkeys();
		}

		private void LoadIntegers()
		{
			txtWorkHistory.Text = MainForm.RecentItemCount.ToString();
			txtWorkQty.Text = MainForm.TopItemsCount.ToString();
			txtFlatten.Text = MainForm.FlattenFactor.ToString();
			txtNoWorkLen.Enabled = txtNoWorkFreq.Enabled = string.IsNullOrEmpty(ConfigManager.NonWorkStateNotificationParams);
			txtActiveLen.Enabled = txtActiveFreq.Enabled = string.IsNullOrEmpty(ConfigManager.WorkStateNotificationParams);
			txtWorkChangeLen.Enabled = string.IsNullOrEmpty(ConfigManager.TasksChangedNotificationParams);
			txtNoWorkFreq.Text = (ConfigManager.LocalSettingsForUser.NotWorkingWarnInterval/1000).ToString();
			txtActiveFreq.Text = (ConfigManager.LocalSettingsForUser.WorkingWarnInterval/1000).ToString();
			txtNoWorkLen.Text = (ConfigManager.LocalSettingsForUser.NotWorkingWarnDuration/1000).ToString();
			txtActiveLen.Text = (ConfigManager.LocalSettingsForUser.WorkingWarnDuration/1000).ToString();
			txtWorkChangeLen.Text = (ConfigManager.LocalSettingsForUser.MenuChangeWarnDuration/1000).ToString();
		}

		private void LoadLanguages()
		{
			cbLanguage.DataSource = new[]
			{
				new ComboBoxElement("Magyar", "hu-HU"),
				new ComboBoxElement("English", "en-US"),
				new ComboBoxElement("Português", "pt-BR"),
				new ComboBoxElement("日本語", "ja-JP"),
				new ComboBoxElement("한국의", "ko-KR"),
				new ComboBoxElement("Español", "es-MX"),
			};
			cbLanguage.SelectedValue = Labels.Culture.Name;
		}

		private void LoadNotificationPositions()
		{
			var ds = new List<ComboBoxElement>
			{
				new ComboBoxElement(Labels.Preference_NotificationPositionBottomRight, NotificationPosition.BottomRight),
				new ComboBoxElement(Labels.Preference_NotificationPositionBottomLeft, NotificationPosition.BottomLeft),
				new ComboBoxElement(Labels.Preference_NotificationPositionCenter, NotificationPosition.Center),
				new ComboBoxElement(Labels.Preference_NotificationPositionTopRight, NotificationPosition.TopRight),
				new ComboBoxElement(Labels.Preference_NotificationPositionTopLeft, NotificationPosition.TopLeft),
			};
			if (!ConfigManager.IsNotificationShown.HasValue || !ConfigManager.IsNotificationShown.Value)
				ds.Add(new ComboBoxElement(Labels.Preference_NotificationPositionHidden, NotificationPosition.Hidden));
			cbNotificationPos.DataSource = ds;
			cbNotificationPos.SelectedItem =
				cbNotificationPos.Items.Cast<ComboBoxElement>()
					.FirstOrDefault(x => (NotificationPosition) x.Value == MainForm.NotificationPosition);
			cbNotificationPos.Enabled = !ConfigManager.IsNotificationShown.HasValue || ConfigManager.IsNotificationShown.Value;
		}

		private void LoadSettings()
		{
			LoadDblClick();
			LoadNotificationPositions();
			LoadToggles();
			LoadIntegers();
			LoadHotkeySettings();
			settingsLoading = false;
		}

		private void LoadToggles()
		{
			tgHighlight.Checked = ConfigManager.LocalSettingsForUser.HighlightNonReasonedWork;
			tgWeekly.Checked = MainForm.ShowWeeklyStats;
			tgMonthly.Checked = MainForm.ShowMonthlyStats;
			tgQuarterly.Checked = MainForm.ShowQuarterlyStats;
			tgYearly.Checked = MainForm.ShowYearlyStats;
			checkedToggles = new List<ToggleGroup>(buttonMap.Keys.Where(t => t.Checked));
			if (ConfigManager.DisplayOptions.HasValue)
			{
				tgSumDelta.Checked = MainForm.ShowSum || MainForm.ShowDelta;
				tgSumDelta.Enabled = false;
			}
			else
			{
				tgSumDelta.Checked = ConfigManager.LocalSettingsForUser.DisplaySummaDelta;
				tgSumDelta.Enabled = true;
			}
			tgSearchOwn.Checked = ConfigManager.LocalSettingsForUser.SearchOwnTasks;
			tgSearchClosed.Checked = ConfigManager.LocalSettingsForUser.SearchInClosed;
			tgOldMenu.Checked = ConfigManager.LocalSettingsForUser.ShowOldMenu;
			tgDynamicWork.Checked = MainForm.ShowDynamicWorks;
			tgWorkChangeNotification.Checked = MainForm.WorkingWarnDisplayable;
			tgShowAll.Checked = RootMenuHelper.IsMenuItem(LocationKey.All);
			tgShowDeadline.Checked = RootMenuHelper.IsMenuItem(LocationKey.Deadline);
			tgShowFavorite.Checked = RootMenuHelper.IsMenuItem(LocationKey.Favorite);
			tgShowPriority.Checked = RootMenuHelper.IsMenuItem(LocationKey.Priority);
			tgShowProgress.Checked = RootMenuHelper.IsMenuItem(LocationKey.Progress);
			tgShowRecentClosed.Checked = RootMenuHelper.IsMenuItem(LocationKey.RecentClosed);
			tgShowRecent.Checked = RootMenuHelper.IsMenuItem(LocationKey.Recent);
			tgShowRecentProject.Checked = RootMenuHelper.IsMenuItem(LocationKey.RecentProject);
		}

		public void Localize()
		{
			settingsLoading = true;
			Text = string.Format(Labels.Preference_Title, ConfigManager.AppNameOverride ?? ConfigManager.ApplicationName);

			tabGeneric.Text = Labels.Preference_General;
			tabNotifications.Text = Labels.Preference_Notification;
			tabNavigation.Text = Labels.Preference_Navigation;
			tabHotKeys.Text = Labels.Preference_HotKey;
			tabMenu.Text = Labels.Preference_Menu;

			lblLanguage.Text = Labels.Preference_Language;
			lblDoubleClick.Text = Labels.Preference_StopResume;
			tgHighlight.Title = Labels.Preference_HighlightNonReasoned;
			tgHighlight.TextOn = Labels.Preference_On;
			tgHighlight.TextOff = Labels.Preference_Off;
			tgWeekly.Title = Labels.Preference_ShowThisWeeksStats;
			tgWeekly.TextOn = Labels.Preference_Shown;
			tgWeekly.TextOff = Labels.Preference_Hidden;
			tgMonthly.Title = Labels.Preference_ShowThisMonthStats;
			tgMonthly.TextOn = Labels.Preference_Shown;
			tgMonthly.TextOff = Labels.Preference_Hidden;
			tgQuarterly.Title = Labels.Preference_ShowThisQuarterStats;
			tgQuarterly.TextOn = Labels.Preference_Shown;
			tgQuarterly.TextOff = Labels.Preference_Hidden;
			tgYearly.Title = Labels.Preference_ShowThisYearStats;
			tgYearly.TextOn = Labels.Preference_Shown;
			tgYearly.TextOff = Labels.Preference_Hidden;
			tgSumDelta.Title = Labels.Preference_ShowSummaDelta;
			tgSumDelta.TextOn = Labels.Preference_Shown;
			tgSumDelta.TextOff = Labels.Preference_Hidden;
			tgOldMenu.Title = Labels.Preference_OldMenu;
			tgOldMenu.TextOn = Labels.Preference_On;
			tgOldMenu.TextOff = Labels.Preference_Off;
			lblFlatten.Text = Labels.Preference_FlattenFactor;

			lblWorkQty.Text = Labels.Preference_MenuTopItemsCount;
			lblWorkHistory.Text = Labels.Preference_MenuRecentItemsCount;
			tgSearchOwn.Title = Labels.Preference_SearchOwnTasks;
			tgSearchOwn.TextOn = Labels.Preference_On;
			tgSearchOwn.TextOff = Labels.Preference_Off;
			tgSearchClosed.Title = Labels.Preference_SearchInClosed;
			tgSearchClosed.TextOn = Labels.Preference_On;
			tgSearchClosed.TextOff = Labels.Preference_Off;
			tgDynamicWork.TextOn = Labels.Preference_Shown;
			tgDynamicWork.TextOff = Labels.Preference_Hidden;
			tgDynamicWork.Title = Labels.Preference_ShowDynamicWorks;
			btnRefreshWork.Text = Labels.Preference_SearchAllReload;

			tgShowFavorite.Title = Labels.NavigationFavorite;
			tgShowFavorite.TextOn = Labels.Preference_Shown;
			tgShowFavorite.TextOff = Labels.Preference_Hidden;
			tgShowRecent.Title = Labels.NavigationRecent;
			tgShowRecent.TextOn = Labels.Preference_Shown;
			tgShowRecent.TextOff = Labels.Preference_Hidden;
			tgShowRecentProject.Title = Labels.NavigationRecentProject;
			tgShowRecentProject.TextOn = Labels.Preference_Shown;
			tgShowRecentProject.TextOff = Labels.Preference_Hidden;
			tgShowRecentClosed.Title = Labels.NavigationRecentClosed;
			tgShowRecentClosed.TextOn = Labels.Preference_Shown;
			tgShowRecentClosed.TextOff = Labels.Preference_Hidden;
			tgShowDeadline.Title = Labels.NavigationDeadline;
			tgShowDeadline.TextOn = Labels.Preference_Shown;
			tgShowDeadline.TextOff = Labels.Preference_Hidden;
			tgShowPriority.Title = Labels.NavigationPriority;
			tgShowPriority.TextOn = Labels.Preference_Shown;
			tgShowPriority.TextOff = Labels.Preference_Hidden;
			tgShowProgress.Title = Labels.NavigationProgress;
			tgShowProgress.TextOn = Labels.Preference_Shown;
			tgShowProgress.TextOff = Labels.Preference_Hidden;
			tgShowAll.Title = Labels.NavigationAll;
			tgShowAll.TextOn = Labels.Preference_Shown;
			tgShowAll.TextOff = Labels.Preference_Hidden;

			lblNotificationPos.Text = Labels.Preference_NotificationPosition;
			tgWorkChangeNotification.Title = Labels.Preference_WorkingWarnDisplayable;
			tgWorkChangeNotification.TextOn = Labels.Preference_On;
			tgWorkChangeNotification.TextOff = Labels.Preference_Off;
			lblNotificationFreq.Text = Labels.Preference_PopupIntervals;
			lblNoWorkFreq.Text = string.Format("{0} ({1})", Labels.Preference_NotWorking, Labels.Preference_Second);
			lblActiveFreq.Text = string.Format("{0} ({1})", Labels.Preference_Working, Labels.Preference_Second);
			lblNotificationLen.Text = Labels.Preference_PopupDurations;
			lblActiveLen.Text = string.Format("{0} ({1})", Labels.Preference_Working, Labels.Preference_Second);
			lblNoWorkLen.Text = string.Format("{0} ({1})", Labels.Preference_NotWorking, Labels.Preference_Second);
			lblWorkChangeLen.Text = string.Format("{0} ({1})", Labels.Preference_MenuChange, Labels.Preference_Second);

			keyCodeColumn.HeaderText = Labels.Hotkeys_HeaderKey;
			shiftColumn.HeaderText = Labels.Hotkeys_HeaderShift;
			controlColumn.HeaderText = Labels.Hotkeys_HeaderCtrl;
			altColumn.HeaderText = Labels.Hotkeys_HeaderAlt;
			windowsColumn.HeaderText = Labels.Hotkeys_HeaderWin;
			actionTypeColumn.HeaderText = Labels.Hotkeys_HeaderAction;
			workDataIdColumn.HeaderText = Labels.Hotkeys_HeaderWork;
			websiteColumn.HeaderText = Labels.Hotkeys_HeaderSite;
			btnHotKeyCreate.Text = Labels.Preference_New;
			btnHotKeyDelete.Text = Labels.Preference_Delete;
			btnHotKeyReset.Text = Labels.Preference_Reset;
			btnHotKeySave.Text = Labels.Preference_Save;
			LoadSettings();
		}

		#endregion

		#region Settings Events

		private void HandleActiveFrequencyChanged(object sender, EventArgs e)
		{
			log.Debug("UI - Active frequency setting changed");
			int qty;
			if (int.TryParse(txtActiveFreq.Text, out qty))
			{
				ConfigManager.LocalSettingsForUser.WorkingWarnInterval = qty*1000;
			}
		}

		private void HandleActiveLengthChanged(object sender, EventArgs e)
		{
			log.Debug("UI - Active length setting changed");
			int qty;
			if (int.TryParse(txtActiveLen.Text, out qty))
			{
				ConfigManager.LocalSettingsForUser.WorkingWarnDuration = qty*1000;
			}
		}

		private void HandleDoubleclickChanged(object sender, EventArgs e)
		{
			log.Debug("UI - Doubleclick setting changed");
			if (!settingsLoading)
			{
				ConfigManager.LocalSettingsForUser.UseDoubleClickForStatusChange =
					(bool) ((ComboBoxElement) cbDblClick.SelectedItem).Value;		
			}
		}

		private void HandleDynamicWorkChanged(object sender, EventArgs e)
		{
			log.Debug("UI - Dynamic work setting changed");
			MainForm.ShowDynamicWorks = tgDynamicWork.Checked;
		}

		private void HandleFlattenChanged(object sender, EventArgs e)
		{
			log.Debug("UI - Flatten setting changed");
			int qty;
			if (int.TryParse(txtFlatten.Text, out qty))
			{
				MainForm.FlattenFactor = qty;
			}
		}

		private void HandleOldMenuChanged(object sender, EventArgs e)
		{
			log.Debug("UI - Old menu setting changed");
			ConfigManager.LocalSettingsForUser.ShowOldMenu = tgOldMenu.Checked;
		}

		private void HandleHightlightChanged(object sender, EventArgs e)
		{
			log.Debug("UI - Highlight setting changed");
			ConfigManager.LocalSettingsForUser.HighlightNonReasonedWork = tgHighlight.Checked;
		}

		private void HandleLanguageChange(object sender, EventArgs e)
		{
			log.Debug("UI - Language setting changed");
			if (!settingsLoading)
			{
				LocalizationHelper.SaveLocalization(new CultureInfo((string) cbLanguage.SelectedValue));
				Labels.Culture = new CultureInfo((string)cbLanguage.SelectedValue);
				foreach (Form form in Application.OpenForms)
				{
					if (form is ILocalizableControl localizableControl)
						localizableControl.Localize();
					foreach (var formControl in form.Controls)
					{
						Localize(formControl);
					}
				}
				// MessageBox.Show(Labels.NotificationLanguageChangeBody, Labels.NotificationLanguageChangeTitle);
			}
		}

		private void Localize(object control)
		{
			if (control is ILocalizableControl localizableControl)
				localizableControl.Localize();
			var children = (Control.ControlCollection)control.GetType().GetProperties().FirstOrDefault(p => p.Name == "Controls")?.GetValue(control);
			if (children == null) return;
			foreach (var child in children)
			{
				Localize(child);
			}
		}

		private void HandleNoWorkFrequencyChanged(object sender, EventArgs e)
		{
			log.Debug("UI - No work frequency setting changed");
			int qty;
			if (int.TryParse(txtNoWorkFreq.Text, out qty))
			{
				ConfigManager.LocalSettingsForUser.NotWorkingWarnInterval = qty*1000;
			}
		}

		private void HandleNoWorkLengthChanged(object sender, EventArgs e)
		{
			log.Debug("UI - No work length setting changed");
			int qty;
			if (int.TryParse(txtNoWorkLen.Text, out qty))
			{
				ConfigManager.LocalSettingsForUser.NotWorkingWarnDuration = qty*1000;
			}
		}

		private void HandleNotificationPositionChanged(object sender, EventArgs e)
		{
			log.Debug("UI - Notification position setting changed");
			if (!settingsLoading)
			{
				MainForm.NotificationPosition = (NotificationPosition) (((ComboBoxElement) cbNotificationPos.SelectedItem).Value);
			}
		}

		private void HandleRefreshWorkClicked(object sender, EventArgs e)
		{
			log.Debug("UI - Refresh work button clicked");
			MainForm.RefreshWork();
		}

		private void HandleSearchClosedChanged(object sender, EventArgs e)
		{
			log.Debug("UI - Search in closed setting changed");
			if (ConfigManager.LocalSettingsForUser.SearchInClosed != tgSearchClosed.Checked)
			{
				ConfigManager.LocalSettingsForUser.SearchInClosed = tgSearchClosed.Checked;
				MainForm.RefreshSearchContent();
			}
		}

		private void HandleShowFavoriteChanged(object sender, EventArgs e)
		{
			log.Debug("UI - Show favorite setting changed");
			if (!settingsLoading) RootMenuHelper.SetMenuItem(LocationKey.Favorite, tgShowFavorite.Checked);
		}

		private void HandleShowRecentChanged(object sender, EventArgs e)
		{
			log.Debug("UI - Show recent setting changed");
			if (!settingsLoading) RootMenuHelper.SetMenuItem(LocationKey.Recent, tgShowRecent.Checked);
		}

		private void HandleShowRecentClosedChanged(object sender, EventArgs e)
		{
			log.Debug("UI - Show recent closed setting changed");
			if (!settingsLoading) RootMenuHelper.SetMenuItem(LocationKey.RecentClosed, tgShowRecent.Checked);
		}

		private void HandleShowRecentProjectChanged(object sender, EventArgs e)
		{
			log.Debug("UI - Show recent project setting changed");
			if (!settingsLoading) RootMenuHelper.SetMenuItem(LocationKey.RecentProject, tgShowRecentProject.Checked);
		}

		private void HandleShowDeadlineChanged(object sender, EventArgs e)
		{
			log.Debug("UI - Show deadline setting changed");
			if (!settingsLoading) RootMenuHelper.SetMenuItem(LocationKey.Deadline, tgShowDeadline.Checked);
		}

		private void HandleShowPriorityChanged(object sender, EventArgs e)
		{
			log.Debug("UI - Show priority setting changed");
			if (!settingsLoading) RootMenuHelper.SetMenuItem(LocationKey.Priority, tgShowPriority.Checked);
		}

		private void HandleShowProgressChanged(object sender, EventArgs e)
		{
			log.Debug("UI - Show progress setting changed");
			if (!settingsLoading) RootMenuHelper.SetMenuItem(LocationKey.Progress, tgShowProgress.Checked);
		}

		private void HandleShowAllChanged(object sender, EventArgs e)
		{
			log.Debug("UI - Show all setting changed");
			if (!settingsLoading) RootMenuHelper.SetMenuItem(LocationKey.All, tgShowAll.Checked);
		}

		private void HandleSearchOwnChanged(object sender, EventArgs e)
		{
			log.Debug("UI - Search own setting changed");
			if (ConfigManager.LocalSettingsForUser.SearchOwnTasks != tgSearchOwn.Checked)
			{
				ConfigManager.LocalSettingsForUser.SearchOwnTasks = tgSearchOwn.Checked;
				MainForm.RefreshSearchContent();
			}
		}

		private void HandleSumDeltaChanged(object sender, EventArgs e)
		{
			log.Debug("UI - Sum/delta setting changed");
			ConfigManager.LocalSettingsForUser.DisplaySummaDelta = tgSumDelta.Checked;
			MainForm.ShowDelta = tgSumDelta.Checked;
			MainForm.ShowSum = tgSumDelta.Checked;
		}

		private void HandleWorktimeStatIntervalChanged(object sender, EventArgs e)
		{
			if (!(sender is ToggleGroup toggleGroup) || checkedToggles == null) return;
			log.Debug($"UI - Show {toggleGroup.Name.Replace("tg", "").ToLower()} setting changed ({toggleGroup.Checked})");
			if (toggleGroup.Checked)
			{
				checkedToggles.Add(toggleGroup);
				if (checkedToggles.Count > 2)
				{
					checkedToggles[0].Checked = false;
				}
			}
			else
			{
				checkedToggles.Remove(toggleGroup);
				if (checkedToggles.Count < 1)
				{
					tgMonthly.Checked = true;
				}
			}
			buttonMap[toggleGroup].Item1(toggleGroup.Checked);
		}

		private void HandleWorkChangeLengthChanged(object sender, EventArgs e)
		{
			log.Debug("UI - Work change length setting changed");
			int qty;
			if (int.TryParse(txtWorkChangeLen.Text, out qty))
			{
				ConfigManager.LocalSettingsForUser.MenuChangeWarnDuration = qty*1000;
			}
		}

		private void HandleWorkChangeNotificationChanged(object sender, EventArgs e)
		{
			log.Debug("UI - Work change notification setting changed");
			MainForm.WorkingWarnDisplayable = tgWorkChangeNotification.Checked;
		}

		private void HandleWorkHistoryChanged(object sender, EventArgs e)
		{
			log.Debug("UI - Work history setting changed");
			int qty;
			if (int.TryParse(txtWorkHistory.Text, out qty))
			{
				MainForm.RecentItemCount = qty;
			}
		}

		private void HandleWorkQuantityChanged(object sender, EventArgs e)
		{
			log.Debug("UI - Work quantity setting changed");
			int qty;
			if (int.TryParse(txtWorkQty.Text, out qty))
			{
				MainForm.TopItemsCount = qty;
			}
		}

		#endregion

		#region Hotkeys

		protected List<HotkeySetting> HotkeySettings
		{
			get { return hotkeySettings.ToList(); }
			set
			{
				hotkeySettings.Clear();
				foreach (HotkeySetting item in value) hotkeySettings.Add(item);
			}
		}

		private static void BindHotkeyActionType(DataGridViewComboBoxColumn cb)
		{
			List<KeyValuePair<string, HotkeyActionType>> listToBind =
				Enum.GetValues(typeof (HotkeyActionType)).Cast<HotkeyActionType>()
					.Select(n => new KeyValuePair<string, HotkeyActionType>(GetNameFor(n), n))
					.Where(n => n.Value != HotkeyActionType.TodoList || ConfigManager.IsTodoListEnabled)
					.ToList();
			cb.DisplayMember = "Key";
			cb.ValueMember = "Value";
			cb.DataSource = listToBind;
		}

		private void BindWebsites(DataGridViewComboBoxColumn cb)
		{
			cb.DisplayMember = "Key";
			cb.ValueMember = "Value";
			cb.DataSource = new[] { new KeyValuePair<string, string>(Labels.Menu_AcDefault, "") }.Union(RecentUrlQuery.Instance.Lookup).ToArray();
		}

		private static void BindKeyCode(DataGridViewComboBoxColumn cb)
		{
			List<KeyValuePair<string, Keys>> listToBind =
				HotkeySetting.ValidKeys.Select(n => new KeyValuePair<string, Keys>(HotkeySetting.GetNameForKey(n), n)).ToList();
			cb.DisplayMember = "Key";
			cb.ValueMember = "Value";
			cb.DataSource = listToBind;
		}

		private static string GetNameFor(HotkeyActionType value)
		{
			switch (value)
			{
				case HotkeyActionType.ResumeOrStopWork:
					return Labels.Hotkeys_ActionTypeResumeOrStopWork;
				case HotkeyActionType.NewWorkDetectorRule:
					return Labels.Hotkeys_ActionTypeNewWorkDetectorRule;
				case HotkeyActionType.DeleteCurrentWorkDetectorRule:
					return Labels.Hotkeys_ActionTypeDeleteCurrentWorkDetectorRule;
				case HotkeyActionType.StartWork:
					return Labels.Hotkeys_ActionTypeStartWork;
				case HotkeyActionType.StartManualMeeting:
					return Labels.Hotkeys_ActionTypeStartManualMeeting;
				case HotkeyActionType.JobCTRL_com:
					return Labels.Hotkeys_ActionTypeJobCTRLcom;
				case HotkeyActionType.AddReason:
					return Labels.Hotkeys_ActionTypeAddReason;
				case HotkeyActionType.ToggleMenu:
					return Labels.Hotkeys_ActionTypeToggleMenu;
				case HotkeyActionType.CreateWork:
					return Labels.NewWork;
				case HotkeyActionType.TodoList:
					return Labels.TODOs;
				case HotkeyActionType.ClearAutoRuleTimer:
					return Labels.Hotkeys_ActionTypeClearAutoRuleTimer;
				case HotkeyActionType.WorkTimeHistory:
					return Labels.Worktime_Tooltip;
				default:
					return Enum.GetName(typeof (HotkeyActionType), value);
			}
		}

		private void BindWorkDataId(DataGridViewComboBoxColumn cb)
		{
			List<KeyValuePair<string, int>> listToBind =
				MenuHelper.FlattenDistinctWorkDataThatHasId(clientMenuLookup.ClientMenu, true)
					.Where(
						n => ConfigManager.LocalSettingsForUser.ShowDynamicWorks || !clientMenuLookup.IsDynamicWork(n.WorkData.Id.Value))
					.Where(n => n.WorkData.IsWorkIdFromServer)
					.Select(n => new KeyValuePair<string, int>(n.FullName + " (" + n.WorkData.Id + ")", n.WorkData.Id.Value))
					.ToList();
			cb.DisplayMember = "Key";
			cb.ValueMember = "Value";
			cb.DataSource = listToBind;
			//cb.SetComboScrollWidth(n => ((KeyValuePair<string, int>)n).Key);	//TODO

			BindWorkDataIdOnEditControlIfApplicable();
		}

		private void BindWorkDataIdOnEditControlIfApplicable()
		{
			if (gvHotKey.CurrentCell == null || gvHotKey.CurrentCell.ColumnIndex != workDataIdColumn.Index ||
				!(gvHotKey.EditingControl is DataGridViewComboBoxEditingControl)) return;
			Debug.Assert(gvHotKey.CurrentRow != null);

			var editControl = ((DataGridViewComboBoxEditingControl)gvHotKey.EditingControl);
			var ds = workDataIdColumn.DataSource as List<KeyValuePair<string, int>>;
			var hk = gvHotKey.CurrentRow.DataBoundItem as HotkeySetting;
			if (ds != null && hk != null)
			{
				var selectedValue = editControl.SelectedValue;
				editControl.DataSource = ds.Where(kv => IsWorkDataIdValidForActionType(kv.Value, hk.ActionType)).ToList();
				editControl.SelectedValue = selectedValue ?? string.Empty;	//TODO: Fix for cell value change after removing edit mode
			}
		}

		private bool CanRegister(HotkeySetting hks)
		{
			//Is it registerable?
			var hk = new Hotkey
			{
				KeyCode = hks.KeyCode,
				Control = hks.Control,
				Shift = hks.Shift,
				Alt = hks.Alt,
				Windows = hks.Windows
			};
			return hotkeyService.CanRegister(hk);
		}

		private HotkeySetting GetSelectedHotKeyItem(out int index)
		{
			index = -1;
			HotkeySetting selectedItem = null;
			if (gvHotKey.SelectedRows.Count > 0)
			{
				selectedItem = gvHotKey.SelectedRows[0].DataBoundItem as HotkeySetting;
			}
			else if (gvHotKey.SelectedCells.Count > 0)
			{
				selectedItem = gvHotKey.Rows[gvHotKey.SelectedCells[0].RowIndex].DataBoundItem as HotkeySetting;
			}
			if (selectedItem == null) return null;
			index = hotkeySettings.IndexOf(selectedItem);
			return selectedItem;
		}

		private bool IsRegistered(HotkeySetting hks)
		{
			//Has already registered?
			var hk = new Hotkey
			{
				KeyCode = hks.KeyCode,
				Control = hks.Control,
				Shift = hks.Shift,
				Alt = hks.Alt,
				Windows = hks.Windows
			};
			return hotkeyService.IsRegistered(hk);
		}

		private bool IsUnique(HotkeySetting hks)
		{
			//Differ from all other (valid?) hotkeys
			return
				hotkeySettings.Count(
					h =>
						h.KeyCode == hks.KeyCode && h.Shift == hks.Shift && h.Control == hks.Control && h.Alt == hks.Alt &&
						h.Windows == hks.Windows) == 1;
		}

		private bool IsWorkDataIdValidForActionType(int workDataId, HotkeyActionType actionType)
		{
			var workWithParent = clientMenuLookup.GetWorkDataWithParentNames(workDataId);
			return workWithParent != null && (actionType == HotkeyActionType.StartWork && workWithParent.WorkData.IsVisibleInMenu ||
											  actionType == HotkeyActionType.StartManualMeeting && workWithParent.WorkData.IsVisibleInAdhocMeeting);
		}

		private void LoadHotkeys()
		{
			HotkeySettings = MainForm.HotKeys;
			websiteColumn.Visible = hotkeySettings.Any(x => HotkeySetting.IsWebsiteAvailableFor(x.ActionType));
			workDataIdColumn.Visible = hotkeySettings.Any(x => HotkeySetting.IsWorkAvailableFor(x.ActionType));
			ValidateAllHotkeySettings();
		}

		private void SaveHotkeys()
		{
			MainForm.HotKeys = HotkeySettings;
		}

		private void SelectHotKeyRow(int idx)
		{
			gvHotKey.ClearSelection();
			gvHotKey.Rows[idx].Selected = true;
			gvHotKey.CurrentCell = gvHotKey.Rows[idx].Cells[0];
			gvHotKey.Focus();
		}

		private void ValidateAllHotkeySettings()
		{
			if (hotkeySettings.Count != gvHotKey.RowCount) return;
			for (int i = 0; i < hotkeySettings.Count && i < gvHotKey.RowCount; i++)
			{
				try
				{
					HotkeySetting curHotkey = hotkeySettings[i];
					ValidateHotkeySetting(curHotkey);
					if (!IsRegistered(curHotkey)) throw new Exception(Labels.Hotkeys_HasNotBeenRegisteredError);
					gvHotKey.Rows[i].ErrorText = "";
				}
				catch (Exception ex)
				{
					gvHotKey.Rows[i].ErrorText = Labels.Error + "! " + Environment.NewLine + ex.Message;
				}
			}
		}

		private void ValidateHotkeySetting(HotkeySetting hks)
		{
			if (hks.KeyCode == Keys.None) throw new ValidationException(Labels.Hotkeys_EmptyHotkeyError);

			if (!HotkeySetting.ValidKeys.Contains(hks.KeyCode)) throw new ValidationException(Labels.Hotkeys_InvalidKeyValue);
			if (hks.IsInRange(Keys.F1, Keys.F12) == false && hks.HasModifier == false)
				throw new ValidationException(Labels.Hotkeys_OtherThanFKeysMustContainModifiers);

			if (HotkeySetting.IsWorkAvailableFor(hks.ActionType))
			{
				if (!hks.WorkDataId.HasValue && HotkeySetting.IsWorkNeededFor(hks.ActionType))
					throw new ValidationException(Labels.Hotkeys_WorkDataIdNeededError);
				if (hks.WorkDataId.HasValue && !IsWorkDataIdValidForActionType(hks.WorkDataId.Value, hks.ActionType))
					throw new ValidationException(Labels.Hotkeys_WorkDataIdInvalidError, true);
			}
		}

		#region Events

		private void HandleHotkeyChanged(object sender, DataGridViewCellEventArgs e)
		{
			if (e.ColumnIndex < 0 || e.RowIndex < 0) return;
			if (e.RowIndex >= hotkeySettings.Count) return;
			if (actionTypeColumn.Index == e.ColumnIndex)
				//ActionType change may causes that some column (in the row) will be unnecessary.
			{
				log.Debug("UI - Hotkey setting changed");
				TelemetryHelper.RecordFeature("Hotkey", "Modified");
				gvHotKey.InvalidateRow(e.RowIndex); //Inavlidate row to repaint only that row that is necessary.

				if (!websiteColumn.Visible)	//Don't hide column if it is already visible, because clicking that column just before hide occures cause an InvalidOperationException (Current cell cannot be set to an invisible cell.)
				{
					websiteColumn.Visible = hotkeySettings.Any(x => HotkeySetting.IsWebsiteAvailableFor(x.ActionType));
				}
				if (!workDataIdColumn.Visible) //Don't hide column if it is already visible, because clicking that column just before hide occures cause an InvalidOperationException (Current cell cannot be set to an invisible cell.)
				{
					workDataIdColumn.Visible = hotkeySettings.Any(x => HotkeySetting.IsWorkAvailableFor(x.ActionType));
				}
			}
		}

		private void HandleHotkeyCreateClicked(object sender, EventArgs e)
		{
			log.Debug("UI - Create hotkey clicked");
			var newItem = new HotkeySetting();
			List<Keys> usedKeys = hotkeySettings.Select(hk => hk.KeyCode).ToList();
			Keys newUnusedKey = HotkeySetting.ValidKeys.FirstOrDefault(k => usedKeys.Contains(k) == false);
			newItem.KeyCode = newUnusedKey;
			newItem.Control = true;
			hotkeySettings.Add(newItem);
			SelectHotKeyRow(hotkeySettings.IndexOf(newItem));
		}

		private void HandleHotkeyDeleteClicked(object sender, EventArgs e)
		{
			log.Debug("UI - Delete hotkey changed");
			int idx;
			HotkeySetting itemToDelete = GetSelectedHotKeyItem(out idx);
			if (itemToDelete == null) return;
			hotkeySettings.RemoveAt(idx);
			if (hotkeySettings.Count > 0) SelectHotKeyRow(idx < hotkeySettings.Count ? idx : hotkeySettings.Count - 1);
		}

		private void HandleHotkeyEditShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
		{
			if (gvHotKey.CurrentCell is DataGridViewComboBoxCell
				&& e.Control is DataGridViewComboBoxEditingControl
				/*&& IsNullableType(hotkeysGridView.CurrentCell.ValueType)*/)
			{
				var dgvcbec = ((DataGridViewComboBoxEditingControl) e.Control);
				BindWorkDataIdOnEditControlIfApplicable();
				dgvcbec.SelectedValue = gvHotKey.CurrentCell.Value ?? string.Empty; //Hax for empty the selected value of a combobox when current value is not valid in combobox.
			}
		}

		private void HandleHotkeyPainting(object sender, DataGridViewCellPaintingEventArgs e)
		{
			if (e.ColumnIndex < 0 || e.RowIndex < 0) return;
			if (e.RowIndex >= hotkeySettings.Count) return;

			if (e.ColumnIndex == workDataIdColumn.Index) 
			{
				HideCellValueIfNeeded(e, HotkeySetting.IsWorkAvailableFor);
			}

			if (e.ColumnIndex == websiteColumn.Index)
			{
				HideCellValueIfNeeded(e, HotkeySetting.IsWebsiteAvailableFor);
			}
		}

		private void HideCellValueIfNeeded(DataGridViewCellPaintingEventArgs e, Func<HotkeyActionType, bool> isValueAvailableForActionType)
		{
			var item = gvHotKey.Rows[e.RowIndex].DataBoundItem as HotkeySetting;
			Debug.Assert(item != null);
			if (!isValueAvailableForActionType(item.ActionType))
			{
				using (Brush backColorBrush = new SolidBrush(e.CellStyle.BackColor))
				{
					e.Graphics.FillRectangle(backColorBrush, e.CellBounds);
				}
				e.PaintBackground(e.CellBounds, true);
				e.Handled = true;

				gvHotKey.Rows[e.RowIndex].Cells[e.ColumnIndex].ReadOnly = true;
				//This will prevent from displaying it when clicking in the cell.
			}
			else
			{
				gvHotKey.Rows[e.RowIndex].Cells[e.ColumnIndex].ReadOnly = false;
			}
		}

		private void HandleHotkeyResetClicked(object sender, EventArgs e)
		{
			log.Debug("UI - Reset hotkeys clicked");
			LoadHotkeys();
		}

		private void HandleHotkeySaveClicked(object sender, EventArgs e)
		{
			log.Debug("UI - Save hotkeys clicked");
			SaveHotkeys();
			LoadHotkeys();
			TelemetryHelper.RecordFeature("Hotkey", "Saved");
		}

		private void HandleHotkeyValidating(object sender, DataGridViewCellCancelEventArgs e)
		{
			if (e.RowIndex >= hotkeySettings.Count || GetFocusedControl() == btnHotKeyDelete) return; //This happens on deletion of the last row
			try
			{
				var curHotkey = gvHotKey.Rows[e.RowIndex].DataBoundItem as HotkeySetting;
				ValidateHotkeySetting(curHotkey);
				if (!IsUnique(curHotkey)) throw new ValidationException(Labels.Hotkeys_MustBeUniqeError);
				if (!IsRegistered(curHotkey))
				{
					var canRegister = CanRegister(curHotkey);
					if (!canRegister) throw new ValidationException(Labels.Hotkeys_UnregisterableError, true);
				}
				gvHotKey.Rows[e.RowIndex].ErrorText = "";
			}
			catch (ValidationException ex)
			{
				gvHotKey.Rows[e.RowIndex].ErrorText = Labels.Error + "! " + Environment.NewLine + ex.Message;
				if (hotkeysShown && !ex.AllowedToProceed) e.Cancel = true;
			}
			catch (Exception ex)
			{
				log.ErrorAndFail("Validation exception should be thrown.", ex);
			}
		}

		private bool hotkeysShown = false;

		private void metroTab_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (metroTab.SelectedTab != tabHotKeys || hotkeysShown) return;

			hotkeysShown = true;
			ValidateAllHotkeySettings();
		}

		private void gvHotKey_DataError(object sender, DataGridViewDataErrorEventArgs e)
		{
			//There is nothing to do. This eventhandler supresses the default error message that pops up when a DataGridViewComboboxCell has an invalid value.
		}

		//TODO: Private exception class may be dangerous. 
		//TODO: Replace exception throwings with Validate(string out errorText, bool out allowedToProceed)
		private class ValidationException : Exception
		{
			public ValidationException(string message, bool allowedToProceed = false) : base(message)
			{
				AllowedToProceed = allowedToProceed;
			}

			public bool AllowedToProceed { get; private set; }
		}

		#endregion

		private void HandleFormClosed(object sender, FormClosedEventArgs e)
		{
			TelemetryHelper.RecordFeature("Settings", "Close");
		}

		#endregion

		private void gvHotKey_CurrentCellDirtyStateChanged(object sender, EventArgs e)
		{
			if (gvHotKey.IsCurrentCellDirty)
			{
				gvHotKey.CommitEdit(DataGridViewDataErrorContexts.Commit);
			}
		}
	}
}
