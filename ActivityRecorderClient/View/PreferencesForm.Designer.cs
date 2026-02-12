using Tct.ActivityRecorderClient.View.Controls;

namespace Tct.ActivityRecorderClient.View
{
	partial class PreferencesForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
			Tct.ActivityRecorderClient.View.DataGridViewFilterColumnHeaderCell dataGridViewFilterColumnHeaderCell1 = new Tct.ActivityRecorderClient.View.DataGridViewFilterColumnHeaderCell();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
			Tct.ActivityRecorderClient.View.DataGridViewFilterColumnHeaderCell dataGridViewFilterColumnHeaderCell2 = new Tct.ActivityRecorderClient.View.DataGridViewFilterColumnHeaderCell();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
			this.metroStyleManager1 = new MetroFramework.Components.MetroStyleManager(this.components);
			this.metroTab = new MetroFramework.Controls.MetroTabControl();
			this.tabGeneric = new MetroFramework.Controls.MetroTabPage();
			this.pGeneralInt = new MetroFramework.Controls.MetroPanel();
			this.txtFlatten = new Tct.ActivityRecorderClient.View.Controls.SmartTextBox();
			this.lblFlatten = new MetroFramework.Controls.MetroLabel();
			this.pShow = new MetroFramework.Controls.MetroPanel();
			this.tgYearly = new Tct.ActivityRecorderClient.View.ToggleGroup();
			this.tgQuarterly = new Tct.ActivityRecorderClient.View.ToggleGroup();
			this.tgMonthly = new Tct.ActivityRecorderClient.View.ToggleGroup();
			this.tgOldMenu = new Tct.ActivityRecorderClient.View.ToggleGroup();
			this.tgHighlight = new Tct.ActivityRecorderClient.View.ToggleGroup();
			this.tgSumDelta = new Tct.ActivityRecorderClient.View.ToggleGroup();
			this.tgWeekly = new Tct.ActivityRecorderClient.View.ToggleGroup();
			this.cbDblClick = new MetroFramework.Controls.MetroComboBox();
			this.lblDoubleClick = new MetroFramework.Controls.MetroLabel();
			this.cbLanguage = new MetroFramework.Controls.MetroComboBox();
			this.lblLanguage = new MetroFramework.Controls.MetroLabel();
			this.tabNavigation = new MetroFramework.Controls.MetroTabPage();
			this.btnRefreshWork = new MetroFramework.Controls.MetroButton();
			this.pNavigationInts = new MetroFramework.Controls.MetroPanel();
			this.txtWorkHistory = new Tct.ActivityRecorderClient.View.Controls.SmartTextBox();
			this.txtWorkQty = new Tct.ActivityRecorderClient.View.Controls.SmartTextBox();
			this.lblWorkHistory = new MetroFramework.Controls.MetroLabel();
			this.lblWorkQty = new MetroFramework.Controls.MetroLabel();
			this.tgDynamicWork = new Tct.ActivityRecorderClient.View.ToggleGroup();
			this.tgSearchClosed = new Tct.ActivityRecorderClient.View.ToggleGroup();
			this.tgSearchOwn = new Tct.ActivityRecorderClient.View.ToggleGroup();
			this.tabMenu = new MetroFramework.Controls.MetroTabPage();
			this.tgShowRecentClosed = new Tct.ActivityRecorderClient.View.ToggleGroup();
			this.tgShowAll = new Tct.ActivityRecorderClient.View.ToggleGroup();
			this.tgShowProgress = new Tct.ActivityRecorderClient.View.ToggleGroup();
			this.tgShowPriority = new Tct.ActivityRecorderClient.View.ToggleGroup();
			this.tgShowDeadline = new Tct.ActivityRecorderClient.View.ToggleGroup();
			this.tgShowRecentProject = new Tct.ActivityRecorderClient.View.ToggleGroup();
			this.tgShowRecent = new Tct.ActivityRecorderClient.View.ToggleGroup();
			this.tgShowFavorite = new Tct.ActivityRecorderClient.View.ToggleGroup();
			this.tabNotifications = new MetroFramework.Controls.MetroTabPage();
			this.pNotificationLen = new MetroFramework.Controls.MetroPanel();
			this.txtWorkChangeLen = new Tct.ActivityRecorderClient.View.Controls.SmartTextBox();
			this.txtActiveLen = new Tct.ActivityRecorderClient.View.Controls.SmartTextBox();
			this.txtNoWorkLen = new Tct.ActivityRecorderClient.View.Controls.SmartTextBox();
			this.lblWorkChangeLen = new MetroFramework.Controls.MetroLabel();
			this.lblNoWorkLen = new MetroFramework.Controls.MetroLabel();
			this.lblActiveLen = new MetroFramework.Controls.MetroLabel();
			this.lblNotificationLen = new MetroFramework.Controls.MetroLabel();
			this.pNotificationFreq = new MetroFramework.Controls.MetroPanel();
			this.txtActiveFreq = new Tct.ActivityRecorderClient.View.Controls.SmartTextBox();
			this.txtNoWorkFreq = new Tct.ActivityRecorderClient.View.Controls.SmartTextBox();
			this.lblActiveFreq = new MetroFramework.Controls.MetroLabel();
			this.lblNoWorkFreq = new MetroFramework.Controls.MetroLabel();
			this.lblNotificationFreq = new MetroFramework.Controls.MetroLabel();
			this.cbNotificationPos = new MetroFramework.Controls.MetroComboBox();
			this.lblNotificationPos = new MetroFramework.Controls.MetroLabel();
			this.tgWorkChangeNotification = new Tct.ActivityRecorderClient.View.ToggleGroup();
			this.tabHotKeys = new MetroFramework.Controls.MetroTabPage();
			this.btnHotKeyReset = new MetroFramework.Controls.MetroButton();
			this.btnHotKeySave = new MetroFramework.Controls.MetroButton();
			this.gvHotKey = new System.Windows.Forms.DataGridView();
			this.keyCodeColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.shiftColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.controlColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.altColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.windowsColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.actionTypeColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.workDataIdColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.websiteColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.btnHotKeyCreate = new MetroFramework.Controls.MetroButton();
			this.btnHotKeyDelete = new MetroFramework.Controls.MetroButton();
			this.dataGridViewComboBoxColumn1 = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.dataGridViewCheckBoxColumn1 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.dataGridViewCheckBoxColumn2 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.dataGridViewCheckBoxColumn3 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.dataGridViewCheckBoxColumn4 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.dataGridViewComboBoxColumn2 = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.dataGridViewComboBoxColumn3 = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.isRegexDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.titleRuleDataGridViewTextBoxColumn = new Tct.ActivityRecorderClient.View.DataGridViewFilterTextBoxColumn();
			this.processRuleDataGridViewTextBoxColumn = new Tct.ActivityRecorderClient.View.DataGridViewFilterTextBoxColumn();
			this.ignoreCaseDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.isEnabledDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			((System.ComponentModel.ISupportInitialize)(this.metroStyleManager1)).BeginInit();
			this.metroTab.SuspendLayout();
			this.tabGeneric.SuspendLayout();
			this.pGeneralInt.SuspendLayout();
			this.pShow.SuspendLayout();
			this.tabNavigation.SuspendLayout();
			this.pNavigationInts.SuspendLayout();
			this.tabMenu.SuspendLayout();
			this.tabNotifications.SuspendLayout();
			this.pNotificationLen.SuspendLayout();
			this.pNotificationFreq.SuspendLayout();
			this.tabHotKeys.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gvHotKey)).BeginInit();
			this.SuspendLayout();
			// 
			// metroStyleManager1
			// 
			this.metroStyleManager1.Owner = null;
			// 
			// metroTab
			// 
			this.metroTab.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.metroTab.Controls.Add(this.tabGeneric);
			this.metroTab.Controls.Add(this.tabNavigation);
			this.metroTab.Controls.Add(this.tabMenu);
			this.metroTab.Controls.Add(this.tabNotifications);
			this.metroTab.Controls.Add(this.tabHotKeys);
			this.metroTab.Location = new System.Drawing.Point(36, 63);
			this.metroTab.Name = "metroTab";
			this.metroTab.SelectedIndex = 0;
			this.metroTab.Size = new System.Drawing.Size(511, 401);
			this.metroTab.Style = MetroFramework.MetroColorStyle.Blue;
			this.metroTab.TabIndex = 0;
			this.metroTab.Theme = MetroFramework.MetroThemeStyle.Light;
			this.metroTab.UseSelectable = true;
			this.metroTab.SelectedIndexChanged += new System.EventHandler(this.metroTab_SelectedIndexChanged);
			// 
			// tabGeneric
			// 
			this.tabGeneric.Controls.Add(this.pGeneralInt);
			this.tabGeneric.Controls.Add(this.pShow);
			this.tabGeneric.Controls.Add(this.cbDblClick);
			this.tabGeneric.Controls.Add(this.lblDoubleClick);
			this.tabGeneric.Controls.Add(this.cbLanguage);
			this.tabGeneric.Controls.Add(this.lblLanguage);
			this.tabGeneric.HorizontalScrollbarBarColor = true;
			this.tabGeneric.HorizontalScrollbarHighlightOnWheel = false;
			this.tabGeneric.HorizontalScrollbarSize = 10;
			this.tabGeneric.Location = new System.Drawing.Point(4, 38);
			this.tabGeneric.Name = "tabGeneric";
			this.tabGeneric.Size = new System.Drawing.Size(503, 359);
			this.tabGeneric.Style = MetroFramework.MetroColorStyle.Blue;
			this.tabGeneric.TabIndex = 0;
			this.tabGeneric.Text = "Általános";
			this.tabGeneric.Theme = MetroFramework.MetroThemeStyle.Light;
			this.tabGeneric.VerticalScrollbar = true;
			this.tabGeneric.VerticalScrollbarBarColor = true;
			this.tabGeneric.VerticalScrollbarHighlightOnWheel = false;
			this.tabGeneric.VerticalScrollbarSize = 10;
			// 
			// pGeneralInt
			// 
			this.pGeneralInt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.pGeneralInt.Controls.Add(this.txtFlatten);
			this.pGeneralInt.Controls.Add(this.lblFlatten);
			this.pGeneralInt.HorizontalScrollbarBarColor = true;
			this.pGeneralInt.HorizontalScrollbarHighlightOnWheel = false;
			this.pGeneralInt.HorizontalScrollbarSize = 10;
			this.pGeneralInt.Location = new System.Drawing.Point(3, 312);
			this.pGeneralInt.Name = "pGeneralInt";
			this.pGeneralInt.Size = new System.Drawing.Size(500, 42);
			this.pGeneralInt.Style = MetroFramework.MetroColorStyle.Blue;
			this.pGeneralInt.TabIndex = 19;
			this.pGeneralInt.Theme = MetroFramework.MetroThemeStyle.Light;
			this.pGeneralInt.VerticalScrollbarBarColor = true;
			this.pGeneralInt.VerticalScrollbarHighlightOnWheel = false;
			this.pGeneralInt.VerticalScrollbarSize = 10;
			// 
			// txtFlatten
			// 
			this.txtFlatten.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.txtFlatten.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.txtFlatten.InputType = Tct.ActivityRecorderClient.View.Controls.SmartTextBoxType.PositiveInteger;
			this.txtFlatten.Location = new System.Drawing.Point(441, 10);
			this.txtFlatten.Name = "txtFlatten";
			this.txtFlatten.Size = new System.Drawing.Size(59, 23);
			this.txtFlatten.TabIndex = 9;
			this.txtFlatten.TextSaved += new System.EventHandler(this.HandleFlattenChanged);
			// 
			// lblFlatten
			// 
			this.lblFlatten.AutoSize = true;
			this.lblFlatten.Location = new System.Drawing.Point(0, 11);
			this.lblFlatten.Name = "lblFlatten";
			this.lblFlatten.Size = new System.Drawing.Size(125, 19);
			this.lblFlatten.Style = MetroFramework.MetroColorStyle.Blue;
			this.lblFlatten.TabIndex = 22;
			this.lblFlatten.Text = "Feladatok mélysége";
			this.lblFlatten.Theme = MetroFramework.MetroThemeStyle.Light;
			// 
			// pShow
			// 
			this.pShow.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.pShow.Controls.Add(this.tgYearly);
			this.pShow.Controls.Add(this.tgQuarterly);
			this.pShow.Controls.Add(this.tgMonthly);
			this.pShow.Controls.Add(this.tgOldMenu);
			this.pShow.Controls.Add(this.tgHighlight);
			this.pShow.Controls.Add(this.tgSumDelta);
			this.pShow.Controls.Add(this.tgWeekly);
			this.pShow.HorizontalScrollbarBarColor = true;
			this.pShow.HorizontalScrollbarHighlightOnWheel = false;
			this.pShow.HorizontalScrollbarSize = 10;
			this.pShow.Location = new System.Drawing.Point(3, 90);
			this.pShow.Name = "pShow";
			this.pShow.Size = new System.Drawing.Size(500, 222);
			this.pShow.Style = MetroFramework.MetroColorStyle.Blue;
			this.pShow.TabIndex = 6;
			this.pShow.Theme = MetroFramework.MetroThemeStyle.Light;
			this.pShow.VerticalScrollbarBarColor = true;
			this.pShow.VerticalScrollbarHighlightOnWheel = false;
			this.pShow.VerticalScrollbarSize = 10;
			// 
			// tgYearly
			// 
			this.tgYearly.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tgYearly.BackColor = System.Drawing.SystemColors.Window;
			this.tgYearly.Checked = true;
			this.tgYearly.Location = new System.Drawing.Point(0, 131);
			this.tgYearly.MinimumSize = new System.Drawing.Size(400, 22);
			this.tgYearly.Name = "tgYearly";
			this.tgYearly.Size = new System.Drawing.Size(497, 22);
			this.tgYearly.TabIndex = 8;
			this.tgYearly.TextOff = "Elrejtve";
			this.tgYearly.TextOn = "Megjelenítve";
			this.tgYearly.Title = "Éves munkaidő megjelenítése";
			this.tgYearly.CheckedChanged += new System.EventHandler(this.HandleWorktimeStatIntervalChanged);
			// 
			// tgQuarterly
			// 
			this.tgQuarterly.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tgQuarterly.BackColor = System.Drawing.SystemColors.Window;
			this.tgQuarterly.Checked = true;
			this.tgQuarterly.Location = new System.Drawing.Point(0, 99);
			this.tgQuarterly.MinimumSize = new System.Drawing.Size(400, 22);
			this.tgQuarterly.Name = "tgQuarterly";
			this.tgQuarterly.Size = new System.Drawing.Size(497, 22);
			this.tgQuarterly.TabIndex = 7;
			this.tgQuarterly.TextOff = "Elrejtve";
			this.tgQuarterly.TextOn = "Megjelenítve";
			this.tgQuarterly.Title = "Negyedéves munkaidő megjelenítése";
			this.tgQuarterly.CheckedChanged += new System.EventHandler(this.HandleWorktimeStatIntervalChanged);
			// 
			// tgMonthly
			// 
			this.tgMonthly.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tgMonthly.BackColor = System.Drawing.SystemColors.Window;
			this.tgMonthly.Checked = true;
			this.tgMonthly.Location = new System.Drawing.Point(0, 67);
			this.tgMonthly.MinimumSize = new System.Drawing.Size(400, 22);
			this.tgMonthly.Name = "tgMonthly";
			this.tgMonthly.Size = new System.Drawing.Size(497, 22);
			this.tgMonthly.TabIndex = 6;
			this.tgMonthly.TextOff = "Elrejtve";
			this.tgMonthly.TextOn = "Megjelenítve";
			this.tgMonthly.Title = "Havi munkaidő megjelenítése";
			this.tgMonthly.CheckedChanged += new System.EventHandler(this.HandleWorktimeStatIntervalChanged);
			// 
			// tgOldMenu
			// 
			this.tgOldMenu.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tgOldMenu.BackColor = System.Drawing.SystemColors.Window;
			this.tgOldMenu.Checked = false;
			this.tgOldMenu.Location = new System.Drawing.Point(0, 196);
			this.tgOldMenu.MinimumSize = new System.Drawing.Size(400, 22);
			this.tgOldMenu.Name = "tgOldMenu";
			this.tgOldMenu.Size = new System.Drawing.Size(497, 22);
			this.tgOldMenu.TabIndex = 10;
			this.tgOldMenu.TextOff = "Kikapcsolva";
			this.tgOldMenu.TextOn = "Bekapcsolva";
			this.tgOldMenu.Title = "Régi menü megjelenés";
			this.tgOldMenu.CheckedChanged += new System.EventHandler(this.HandleOldMenuChanged);
			// 
			// tgHighlight
			// 
			this.tgHighlight.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tgHighlight.BackColor = System.Drawing.SystemColors.Window;
			this.tgHighlight.Checked = true;
			this.tgHighlight.Location = new System.Drawing.Point(0, 3);
			this.tgHighlight.MinimumSize = new System.Drawing.Size(400, 22);
			this.tgHighlight.Name = "tgHighlight";
			this.tgHighlight.Size = new System.Drawing.Size(497, 22);
			this.tgHighlight.TabIndex = 4;
			this.tgHighlight.TextOff = "Kikapcsolva";
			this.tgHighlight.TextOn = "Bekapcsolva";
			this.tgHighlight.Title = "Megjegyzés nélküli feladatok kiemelése";
			this.tgHighlight.CheckedChanged += new System.EventHandler(this.HandleHightlightChanged);
			// 
			// tgSumDelta
			// 
			this.tgSumDelta.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tgSumDelta.BackColor = System.Drawing.SystemColors.Window;
			this.tgSumDelta.Checked = true;
			this.tgSumDelta.Location = new System.Drawing.Point(0, 163);
			this.tgSumDelta.MinimumSize = new System.Drawing.Size(400, 22);
			this.tgSumDelta.Name = "tgSumDelta";
			this.tgSumDelta.Size = new System.Drawing.Size(497, 22);
			this.tgSumDelta.TabIndex = 9;
			this.tgSumDelta.TextOff = "Elrejtve";
			this.tgSumDelta.TextOn = "Megjelenítve";
			this.tgSumDelta.Title = "Szumma és delta megjelenítése";
			this.tgSumDelta.CheckedChanged += new System.EventHandler(this.HandleSumDeltaChanged);
			// 
			// tgWeekly
			// 
			this.tgWeekly.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tgWeekly.BackColor = System.Drawing.SystemColors.Window;
			this.tgWeekly.Checked = true;
			this.tgWeekly.Location = new System.Drawing.Point(0, 35);
			this.tgWeekly.MinimumSize = new System.Drawing.Size(400, 22);
			this.tgWeekly.Name = "tgWeekly";
			this.tgWeekly.Size = new System.Drawing.Size(497, 22);
			this.tgWeekly.TabIndex = 5;
			this.tgWeekly.TextOff = "Elrejtve";
			this.tgWeekly.TextOn = "Megjelenítve";
			this.tgWeekly.Title = "Heti munkaidő megjelenítése";
			this.tgWeekly.CheckedChanged += new System.EventHandler(this.HandleWorktimeStatIntervalChanged);
			// 
			// cbDblClick
			// 
			this.cbDblClick.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cbDblClick.FormattingEnabled = true;
			this.cbDblClick.ItemHeight = 23;
			this.cbDblClick.Items.AddRange(new object[] {
            "Rögzítés elindítás/megállítás",
            "Nincs művelet"});
			this.cbDblClick.Location = new System.Drawing.Point(322, 50);
			this.cbDblClick.Name = "cbDblClick";
			this.cbDblClick.Size = new System.Drawing.Size(181, 29);
			this.cbDblClick.Style = MetroFramework.MetroColorStyle.Blue;
			this.cbDblClick.TabIndex = 3;
			this.cbDblClick.Theme = MetroFramework.MetroThemeStyle.Light;
			this.cbDblClick.UseSelectable = true;
			this.cbDblClick.SelectedValueChanged += new System.EventHandler(this.HandleDoubleclickChanged);
			// 
			// lblDoubleClick
			// 
			this.lblDoubleClick.AutoSize = true;
			this.lblDoubleClick.Location = new System.Drawing.Point(3, 55);
			this.lblDoubleClick.Name = "lblDoubleClick";
			this.lblDoubleClick.Size = new System.Drawing.Size(106, 19);
			this.lblDoubleClick.Style = MetroFramework.MetroColorStyle.Blue;
			this.lblDoubleClick.TabIndex = 4;
			this.lblDoubleClick.Text = "Elindítás/leállítás:";
			this.lblDoubleClick.Theme = MetroFramework.MetroThemeStyle.Light;
			// 
			// cbLanguage
			// 
			this.cbLanguage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cbLanguage.DisplayMember = "Text";
			this.cbLanguage.ItemHeight = 23;
			this.cbLanguage.Location = new System.Drawing.Point(322, 15);
			this.cbLanguage.Name = "cbLanguage";
			this.cbLanguage.Size = new System.Drawing.Size(181, 29);
			this.cbLanguage.Style = MetroFramework.MetroColorStyle.Blue;
			this.cbLanguage.TabIndex = 2;
			this.cbLanguage.Theme = MetroFramework.MetroThemeStyle.Light;
			this.cbLanguage.UseSelectable = true;
			this.cbLanguage.ValueMember = "Value";
			this.cbLanguage.SelectedValueChanged += new System.EventHandler(this.HandleLanguageChange);
			// 
			// lblLanguage
			// 
			this.lblLanguage.AutoSize = true;
			this.lblLanguage.Location = new System.Drawing.Point(3, 20);
			this.lblLanguage.Name = "lblLanguage";
			this.lblLanguage.Size = new System.Drawing.Size(105, 19);
			this.lblLanguage.Style = MetroFramework.MetroColorStyle.Blue;
			this.lblLanguage.TabIndex = 2;
			this.lblLanguage.Text = "Program nyelve:";
			this.lblLanguage.Theme = MetroFramework.MetroThemeStyle.Light;
			// 
			// tabNavigation
			// 
			this.tabNavigation.Controls.Add(this.btnRefreshWork);
			this.tabNavigation.Controls.Add(this.pNavigationInts);
			this.tabNavigation.Controls.Add(this.tgDynamicWork);
			this.tabNavigation.Controls.Add(this.tgSearchClosed);
			this.tabNavigation.Controls.Add(this.tgSearchOwn);
			this.tabNavigation.HorizontalScrollbarBarColor = true;
			this.tabNavigation.HorizontalScrollbarHighlightOnWheel = false;
			this.tabNavigation.HorizontalScrollbarSize = 10;
			this.tabNavigation.Location = new System.Drawing.Point(4, 38);
			this.tabNavigation.Name = "tabNavigation";
			this.tabNavigation.Size = new System.Drawing.Size(503, 359);
			this.tabNavigation.Style = MetroFramework.MetroColorStyle.Blue;
			this.tabNavigation.TabIndex = 4;
			this.tabNavigation.Text = "Navigáció";
			this.tabNavigation.Theme = MetroFramework.MetroThemeStyle.Light;
			this.tabNavigation.VerticalScrollbarBarColor = true;
			this.tabNavigation.VerticalScrollbarHighlightOnWheel = false;
			this.tabNavigation.VerticalScrollbarSize = 10;
			// 
			// btnRefreshWork
			// 
			this.btnRefreshWork.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnRefreshWork.Location = new System.Drawing.Point(184, 188);
			this.btnRefreshWork.Name = "btnRefreshWork";
			this.btnRefreshWork.Size = new System.Drawing.Size(131, 30);
			this.btnRefreshWork.Style = MetroFramework.MetroColorStyle.Blue;
			this.btnRefreshWork.TabIndex = 16;
			this.btnRefreshWork.Text = "Feladatok frissítése";
			this.btnRefreshWork.Theme = MetroFramework.MetroThemeStyle.Light;
			this.btnRefreshWork.UseSelectable = true;
			this.btnRefreshWork.Visible = false;
			this.btnRefreshWork.Click += new System.EventHandler(this.HandleRefreshWorkClicked);
			// 
			// pNavigationInts
			// 
			this.pNavigationInts.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.pNavigationInts.Controls.Add(this.txtWorkHistory);
			this.pNavigationInts.Controls.Add(this.txtWorkQty);
			this.pNavigationInts.Controls.Add(this.lblWorkHistory);
			this.pNavigationInts.Controls.Add(this.lblWorkQty);
			this.pNavigationInts.HorizontalScrollbarBarColor = true;
			this.pNavigationInts.HorizontalScrollbarHighlightOnWheel = false;
			this.pNavigationInts.HorizontalScrollbarSize = 10;
			this.pNavigationInts.Location = new System.Drawing.Point(3, 17);
			this.pNavigationInts.Name = "pNavigationInts";
			this.pNavigationInts.Size = new System.Drawing.Size(500, 56);
			this.pNavigationInts.Style = MetroFramework.MetroColorStyle.Blue;
			this.pNavigationInts.TabIndex = 2;
			this.pNavigationInts.Theme = MetroFramework.MetroThemeStyle.Light;
			this.pNavigationInts.VerticalScrollbarBarColor = true;
			this.pNavigationInts.VerticalScrollbarHighlightOnWheel = false;
			this.pNavigationInts.VerticalScrollbarSize = 10;
			// 
			// txtWorkHistory
			// 
			this.txtWorkHistory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.txtWorkHistory.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.txtWorkHistory.InputType = Tct.ActivityRecorderClient.View.Controls.SmartTextBoxType.PositiveInteger;
			this.txtWorkHistory.Location = new System.Drawing.Point(432, 31);
			this.txtWorkHistory.Name = "txtWorkHistory";
			this.txtWorkHistory.Size = new System.Drawing.Size(68, 23);
			this.txtWorkHistory.TabIndex = 12;
			this.txtWorkHistory.TextSaved += new System.EventHandler(this.HandleWorkHistoryChanged);
			// 
			// txtWorkQty
			// 
			this.txtWorkQty.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.txtWorkQty.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.txtWorkQty.InputType = Tct.ActivityRecorderClient.View.Controls.SmartTextBoxType.PositiveInteger;
			this.txtWorkQty.Location = new System.Drawing.Point(432, 0);
			this.txtWorkQty.Name = "txtWorkQty";
			this.txtWorkQty.Size = new System.Drawing.Size(68, 23);
			this.txtWorkQty.TabIndex = 11;
			this.txtWorkQty.TextSaved += new System.EventHandler(this.HandleWorkQuantityChanged);
			// 
			// lblWorkHistory
			// 
			this.lblWorkHistory.AutoSize = true;
			this.lblWorkHistory.Location = new System.Drawing.Point(0, 32);
			this.lblWorkHistory.Name = "lblWorkHistory";
			this.lblWorkHistory.Size = new System.Drawing.Size(167, 19);
			this.lblWorkHistory.Style = MetroFramework.MetroColorStyle.Blue;
			this.lblWorkHistory.TabIndex = 24;
			this.lblWorkHistory.Text = "Legutóbbi feladatok száma";
			this.lblWorkHistory.Theme = MetroFramework.MetroThemeStyle.Light;
			// 
			// lblWorkQty
			// 
			this.lblWorkQty.AutoSize = true;
			this.lblWorkQty.Location = new System.Drawing.Point(0, 0);
			this.lblWorkQty.Name = "lblWorkQty";
			this.lblWorkQty.Size = new System.Drawing.Size(188, 19);
			this.lblWorkQty.Style = MetroFramework.MetroColorStyle.Blue;
			this.lblWorkQty.TabIndex = 22;
			this.lblWorkQty.Text = "Legfontosabb feladatok száma";
			this.lblWorkQty.Theme = MetroFramework.MetroThemeStyle.Light;
			// 
			// tgDynamicWork
			// 
			this.tgDynamicWork.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tgDynamicWork.BackColor = System.Drawing.SystemColors.Window;
			this.tgDynamicWork.Checked = false;
			this.tgDynamicWork.Location = new System.Drawing.Point(3, 83);
			this.tgDynamicWork.MinimumSize = new System.Drawing.Size(400, 22);
			this.tgDynamicWork.Name = "tgDynamicWork";
			this.tgDynamicWork.Size = new System.Drawing.Size(500, 22);
			this.tgDynamicWork.TabIndex = 13;
			this.tgDynamicWork.TextOff = "Elrejtve";
			this.tgDynamicWork.TextOn = "Megjelenítve";
			this.tgDynamicWork.Title = "Dinamikus feladatok megjelenítése";
			this.tgDynamicWork.CheckedChanged += new System.EventHandler(this.HandleDynamicWorkChanged);
			// 
			// tgSearchClosed
			// 
			this.tgSearchClosed.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tgSearchClosed.BackColor = System.Drawing.SystemColors.Window;
			this.tgSearchClosed.Checked = false;
			this.tgSearchClosed.Location = new System.Drawing.Point(3, 151);
			this.tgSearchClosed.MinimumSize = new System.Drawing.Size(400, 22);
			this.tgSearchClosed.Name = "tgSearchClosed";
			this.tgSearchClosed.Size = new System.Drawing.Size(500, 22);
			this.tgSearchClosed.TabIndex = 15;
			this.tgSearchClosed.TextOff = "Kikapcsolva";
			this.tgSearchClosed.TextOn = "Bekapcsolva";
			this.tgSearchClosed.Title = "Lezárt feladatok között keresés";
			this.tgSearchClosed.Visible = false;
			this.tgSearchClosed.CheckedChanged += new System.EventHandler(this.HandleSearchClosedChanged);
			// 
			// tgSearchOwn
			// 
			this.tgSearchOwn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tgSearchOwn.BackColor = System.Drawing.SystemColors.Window;
			this.tgSearchOwn.Checked = true;
			this.tgSearchOwn.Location = new System.Drawing.Point(3, 117);
			this.tgSearchOwn.MinimumSize = new System.Drawing.Size(400, 22);
			this.tgSearchOwn.Name = "tgSearchOwn";
			this.tgSearchOwn.Size = new System.Drawing.Size(500, 22);
			this.tgSearchOwn.TabIndex = 14;
			this.tgSearchOwn.TextOff = "Kikapcsolva";
			this.tgSearchOwn.TextOn = "Bekapcsolva";
			this.tgSearchOwn.Title = "Saját feladatok között keresés";
			this.tgSearchOwn.Visible = false;
			this.tgSearchOwn.CheckedChanged += new System.EventHandler(this.HandleSearchOwnChanged);
			// 
			// tabMenu
			// 
			this.tabMenu.Controls.Add(this.tgShowRecentClosed);
			this.tabMenu.Controls.Add(this.tgShowAll);
			this.tabMenu.Controls.Add(this.tgShowProgress);
			this.tabMenu.Controls.Add(this.tgShowPriority);
			this.tabMenu.Controls.Add(this.tgShowDeadline);
			this.tabMenu.Controls.Add(this.tgShowRecentProject);
			this.tabMenu.Controls.Add(this.tgShowRecent);
			this.tabMenu.Controls.Add(this.tgShowFavorite);
			this.tabMenu.HorizontalScrollbarBarColor = true;
			this.tabMenu.HorizontalScrollbarHighlightOnWheel = false;
			this.tabMenu.HorizontalScrollbarSize = 10;
			this.tabMenu.Location = new System.Drawing.Point(4, 38);
			this.tabMenu.Name = "tabMenu";
			this.tabMenu.Size = new System.Drawing.Size(503, 359);
			this.tabMenu.TabIndex = 5;
			this.tabMenu.Text = "Menü";
			this.tabMenu.VerticalScrollbarBarColor = true;
			this.tabMenu.VerticalScrollbarHighlightOnWheel = false;
			this.tabMenu.VerticalScrollbarSize = 10;
			// 
			// tgShowRecentClosed
			// 
			this.tgShowRecentClosed.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tgShowRecentClosed.BackColor = System.Drawing.SystemColors.Window;
			this.tgShowRecentClosed.Checked = false;
			this.tgShowRecentClosed.Location = new System.Drawing.Point(3, 109);
			this.tgShowRecentClosed.MinimumSize = new System.Drawing.Size(400, 22);
			this.tgShowRecentClosed.Name = "tgShowRecentClosed";
			this.tgShowRecentClosed.Size = new System.Drawing.Size(500, 22);
			this.tgShowRecentClosed.TabIndex = 23;
			this.tgShowRecentClosed.TextOff = null;
			this.tgShowRecentClosed.TextOn = null;
			this.tgShowRecentClosed.Title = "Title";
			this.tgShowRecentClosed.CheckedChanged += new System.EventHandler(this.HandleShowRecentClosedChanged);
			// 
			// tgShowAll
			// 
			this.tgShowAll.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tgShowAll.BackColor = System.Drawing.SystemColors.Window;
			this.tgShowAll.Checked = false;
			this.tgShowAll.Location = new System.Drawing.Point(3, 232);
			this.tgShowAll.MinimumSize = new System.Drawing.Size(400, 22);
			this.tgShowAll.Name = "tgShowAll";
			this.tgShowAll.Size = new System.Drawing.Size(500, 22);
			this.tgShowAll.TabIndex = 27;
			this.tgShowAll.TextOff = null;
			this.tgShowAll.TextOn = null;
			this.tgShowAll.Title = "Title";
			this.tgShowAll.CheckedChanged += new System.EventHandler(this.HandleShowAllChanged);
			// 
			// tgShowProgress
			// 
			this.tgShowProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tgShowProgress.BackColor = System.Drawing.SystemColors.Window;
			this.tgShowProgress.Checked = false;
			this.tgShowProgress.Location = new System.Drawing.Point(3, 201);
			this.tgShowProgress.MinimumSize = new System.Drawing.Size(400, 22);
			this.tgShowProgress.Name = "tgShowProgress";
			this.tgShowProgress.Size = new System.Drawing.Size(500, 22);
			this.tgShowProgress.TabIndex = 26;
			this.tgShowProgress.TextOff = null;
			this.tgShowProgress.TextOn = null;
			this.tgShowProgress.Title = "Title";
			this.tgShowProgress.CheckedChanged += new System.EventHandler(this.HandleShowProgressChanged);
			// 
			// tgShowPriority
			// 
			this.tgShowPriority.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tgShowPriority.BackColor = System.Drawing.SystemColors.Window;
			this.tgShowPriority.Checked = false;
			this.tgShowPriority.Location = new System.Drawing.Point(3, 170);
			this.tgShowPriority.MinimumSize = new System.Drawing.Size(400, 22);
			this.tgShowPriority.Name = "tgShowPriority";
			this.tgShowPriority.Size = new System.Drawing.Size(500, 22);
			this.tgShowPriority.TabIndex = 25;
			this.tgShowPriority.TextOff = null;
			this.tgShowPriority.TextOn = null;
			this.tgShowPriority.Title = "Title";
			this.tgShowPriority.CheckedChanged += new System.EventHandler(this.HandleShowPriorityChanged);
			// 
			// tgShowDeadline
			// 
			this.tgShowDeadline.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tgShowDeadline.BackColor = System.Drawing.SystemColors.Window;
			this.tgShowDeadline.Checked = false;
			this.tgShowDeadline.Location = new System.Drawing.Point(3, 140);
			this.tgShowDeadline.MinimumSize = new System.Drawing.Size(400, 22);
			this.tgShowDeadline.Name = "tgShowDeadline";
			this.tgShowDeadline.Size = new System.Drawing.Size(500, 22);
			this.tgShowDeadline.TabIndex = 24;
			this.tgShowDeadline.TextOff = null;
			this.tgShowDeadline.TextOn = null;
			this.tgShowDeadline.Title = "Title";
			this.tgShowDeadline.CheckedChanged += new System.EventHandler(this.HandleShowDeadlineChanged);
			// 
			// tgShowRecentProject
			// 
			this.tgShowRecentProject.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tgShowRecentProject.BackColor = System.Drawing.SystemColors.Window;
			this.tgShowRecentProject.Checked = false;
			this.tgShowRecentProject.Location = new System.Drawing.Point(3, 79);
			this.tgShowRecentProject.MinimumSize = new System.Drawing.Size(400, 22);
			this.tgShowRecentProject.Name = "tgShowRecentProject";
			this.tgShowRecentProject.Size = new System.Drawing.Size(500, 22);
			this.tgShowRecentProject.TabIndex = 22;
			this.tgShowRecentProject.TextOff = null;
			this.tgShowRecentProject.TextOn = null;
			this.tgShowRecentProject.Title = "Title";
			this.tgShowRecentProject.CheckedChanged += new System.EventHandler(this.HandleShowRecentProjectChanged);
			// 
			// tgShowRecent
			// 
			this.tgShowRecent.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tgShowRecent.BackColor = System.Drawing.SystemColors.Window;
			this.tgShowRecent.Checked = false;
			this.tgShowRecent.Location = new System.Drawing.Point(3, 48);
			this.tgShowRecent.MinimumSize = new System.Drawing.Size(400, 22);
			this.tgShowRecent.Name = "tgShowRecent";
			this.tgShowRecent.Size = new System.Drawing.Size(500, 22);
			this.tgShowRecent.TabIndex = 21;
			this.tgShowRecent.TextOff = null;
			this.tgShowRecent.TextOn = null;
			this.tgShowRecent.Title = "Title";
			this.tgShowRecent.CheckedChanged += new System.EventHandler(this.HandleShowRecentChanged);
			// 
			// tgShowFavorite
			// 
			this.tgShowFavorite.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tgShowFavorite.BackColor = System.Drawing.SystemColors.Window;
			this.tgShowFavorite.Checked = false;
			this.tgShowFavorite.Location = new System.Drawing.Point(3, 17);
			this.tgShowFavorite.MinimumSize = new System.Drawing.Size(400, 22);
			this.tgShowFavorite.Name = "tgShowFavorite";
			this.tgShowFavorite.Size = new System.Drawing.Size(500, 22);
			this.tgShowFavorite.TabIndex = 20;
			this.tgShowFavorite.TextOff = null;
			this.tgShowFavorite.TextOn = null;
			this.tgShowFavorite.Title = "Kedvencek";
			this.tgShowFavorite.CheckedChanged += new System.EventHandler(this.HandleShowFavoriteChanged);
			// 
			// tabNotifications
			// 
			this.tabNotifications.Controls.Add(this.pNotificationLen);
			this.tabNotifications.Controls.Add(this.pNotificationFreq);
			this.tabNotifications.Controls.Add(this.cbNotificationPos);
			this.tabNotifications.Controls.Add(this.lblNotificationPos);
			this.tabNotifications.Controls.Add(this.tgWorkChangeNotification);
			this.tabNotifications.HorizontalScrollbarBarColor = true;
			this.tabNotifications.HorizontalScrollbarHighlightOnWheel = false;
			this.tabNotifications.HorizontalScrollbarSize = 10;
			this.tabNotifications.Location = new System.Drawing.Point(4, 38);
			this.tabNotifications.Name = "tabNotifications";
			this.tabNotifications.Size = new System.Drawing.Size(503, 359);
			this.tabNotifications.Style = MetroFramework.MetroColorStyle.Blue;
			this.tabNotifications.TabIndex = 3;
			this.tabNotifications.Text = "Figyelmeztetések";
			this.tabNotifications.Theme = MetroFramework.MetroThemeStyle.Light;
			this.tabNotifications.VerticalScrollbarBarColor = true;
			this.tabNotifications.VerticalScrollbarHighlightOnWheel = false;
			this.tabNotifications.VerticalScrollbarSize = 10;
			// 
			// pNotificationLen
			// 
			this.pNotificationLen.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.pNotificationLen.Controls.Add(this.txtWorkChangeLen);
			this.pNotificationLen.Controls.Add(this.txtActiveLen);
			this.pNotificationLen.Controls.Add(this.txtNoWorkLen);
			this.pNotificationLen.Controls.Add(this.lblWorkChangeLen);
			this.pNotificationLen.Controls.Add(this.lblNoWorkLen);
			this.pNotificationLen.Controls.Add(this.lblActiveLen);
			this.pNotificationLen.Controls.Add(this.lblNotificationLen);
			this.pNotificationLen.HorizontalScrollbarBarColor = true;
			this.pNotificationLen.HorizontalScrollbarHighlightOnWheel = false;
			this.pNotificationLen.HorizontalScrollbarSize = 10;
			this.pNotificationLen.Location = new System.Drawing.Point(3, 176);
			this.pNotificationLen.Name = "pNotificationLen";
			this.pNotificationLen.Size = new System.Drawing.Size(500, 112);
			this.pNotificationLen.Style = MetroFramework.MetroColorStyle.Blue;
			this.pNotificationLen.TabIndex = 22;
			this.pNotificationLen.Theme = MetroFramework.MetroThemeStyle.Light;
			this.pNotificationLen.VerticalScrollbarBarColor = true;
			this.pNotificationLen.VerticalScrollbarHighlightOnWheel = false;
			this.pNotificationLen.VerticalScrollbarSize = 10;
			// 
			// txtWorkChangeLen
			// 
			this.txtWorkChangeLen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.txtWorkChangeLen.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.txtWorkChangeLen.InputType = Tct.ActivityRecorderClient.View.Controls.SmartTextBoxType.PositiveInteger;
			this.txtWorkChangeLen.Location = new System.Drawing.Point(441, 82);
			this.txtWorkChangeLen.Name = "txtWorkChangeLen";
			this.txtWorkChangeLen.Size = new System.Drawing.Size(59, 23);
			this.txtWorkChangeLen.TabIndex = 38;
			this.txtWorkChangeLen.TextSaved += new System.EventHandler(this.HandleWorkChangeLengthChanged);
			// 
			// txtActiveLen
			// 
			this.txtActiveLen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.txtActiveLen.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.txtActiveLen.InputType = Tct.ActivityRecorderClient.View.Controls.SmartTextBoxType.PositiveIntegerAndMinusOne;
			this.txtActiveLen.Location = new System.Drawing.Point(441, 50);
			this.txtActiveLen.Name = "txtActiveLen";
			this.txtActiveLen.Size = new System.Drawing.Size(59, 23);
			this.txtActiveLen.TabIndex = 37;
			this.txtActiveLen.TextSaved += new System.EventHandler(this.HandleActiveLengthChanged);
			// 
			// txtNoWorkLen
			// 
			this.txtNoWorkLen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.txtNoWorkLen.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.txtNoWorkLen.InputType = Tct.ActivityRecorderClient.View.Controls.SmartTextBoxType.PositiveIntegerAndMinusOne;
			this.txtNoWorkLen.Location = new System.Drawing.Point(441, 20);
			this.txtNoWorkLen.Name = "txtNoWorkLen";
			this.txtNoWorkLen.Size = new System.Drawing.Size(59, 23);
			this.txtNoWorkLen.TabIndex = 36;
			this.txtNoWorkLen.TextSaved += new System.EventHandler(this.HandleNoWorkLengthChanged);
			// 
			// lblWorkChangeLen
			// 
			this.lblWorkChangeLen.AutoSize = true;
			this.lblWorkChangeLen.Location = new System.Drawing.Point(21, 85);
			this.lblWorkChangeLen.Name = "lblWorkChangeLen";
			this.lblWorkChangeLen.Size = new System.Drawing.Size(98, 19);
			this.lblWorkChangeLen.Style = MetroFramework.MetroColorStyle.Blue;
			this.lblWorkChangeLen.TabIndex = 24;
			this.lblWorkChangeLen.Text = "Feladatváltozás";
			this.lblWorkChangeLen.Theme = MetroFramework.MetroThemeStyle.Light;
			// 
			// lblNoWorkLen
			// 
			this.lblNoWorkLen.AutoSize = true;
			this.lblNoWorkLen.Location = new System.Drawing.Point(21, 27);
			this.lblNoWorkLen.Name = "lblNoWorkLen";
			this.lblNoWorkLen.Size = new System.Drawing.Size(123, 19);
			this.lblNoWorkLen.Style = MetroFramework.MetroColorStyle.Blue;
			this.lblNoWorkLen.TabIndex = 22;
			this.lblNoWorkLen.Text = "Nem munka státusz";
			this.lblNoWorkLen.Theme = MetroFramework.MetroThemeStyle.Light;
			// 
			// lblActiveLen
			// 
			this.lblActiveLen.AutoSize = true;
			this.lblActiveLen.Location = new System.Drawing.Point(21, 56);
			this.lblActiveLen.Name = "lblActiveLen";
			this.lblActiveLen.Size = new System.Drawing.Size(81, 19);
			this.lblActiveLen.Style = MetroFramework.MetroColorStyle.Blue;
			this.lblActiveLen.TabIndex = 18;
			this.lblActiveLen.Text = "Aktív feladat";
			this.lblActiveLen.Theme = MetroFramework.MetroThemeStyle.Light;
			// 
			// lblNotificationLen
			// 
			this.lblNotificationLen.AutoSize = true;
			this.lblNotificationLen.FontWeight = MetroFramework.MetroLabelWeight.Bold;
			this.lblNotificationLen.Location = new System.Drawing.Point(0, 0);
			this.lblNotificationLen.Name = "lblNotificationLen";
			this.lblNotificationLen.Size = new System.Drawing.Size(173, 19);
			this.lblNotificationLen.Style = MetroFramework.MetroColorStyle.Blue;
			this.lblNotificationLen.TabIndex = 17;
			this.lblNotificationLen.Text = "Figyelmeztetések hossza";
			this.lblNotificationLen.Theme = MetroFramework.MetroThemeStyle.Light;
			// 
			// pNotificationFreq
			// 
			this.pNotificationFreq.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.pNotificationFreq.Controls.Add(this.txtActiveFreq);
			this.pNotificationFreq.Controls.Add(this.txtNoWorkFreq);
			this.pNotificationFreq.Controls.Add(this.lblActiveFreq);
			this.pNotificationFreq.Controls.Add(this.lblNoWorkFreq);
			this.pNotificationFreq.Controls.Add(this.lblNotificationFreq);
			this.pNotificationFreq.HorizontalScrollbarBarColor = true;
			this.pNotificationFreq.HorizontalScrollbarHighlightOnWheel = false;
			this.pNotificationFreq.HorizontalScrollbarSize = 10;
			this.pNotificationFreq.Location = new System.Drawing.Point(3, 89);
			this.pNotificationFreq.Name = "pNotificationFreq";
			this.pNotificationFreq.Size = new System.Drawing.Size(500, 84);
			this.pNotificationFreq.Style = MetroFramework.MetroColorStyle.Blue;
			this.pNotificationFreq.TabIndex = 33;
			this.pNotificationFreq.Theme = MetroFramework.MetroThemeStyle.Light;
			this.pNotificationFreq.VerticalScrollbarBarColor = true;
			this.pNotificationFreq.VerticalScrollbarHighlightOnWheel = false;
			this.pNotificationFreq.VerticalScrollbarSize = 10;
			// 
			// txtActiveFreq
			// 
			this.txtActiveFreq.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.txtActiveFreq.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.txtActiveFreq.InputType = Tct.ActivityRecorderClient.View.Controls.SmartTextBoxType.PositiveInteger;
			this.txtActiveFreq.Location = new System.Drawing.Point(441, 55);
			this.txtActiveFreq.Name = "txtActiveFreq";
			this.txtActiveFreq.Size = new System.Drawing.Size(59, 23);
			this.txtActiveFreq.TabIndex = 35;
			this.txtActiveFreq.TextSaved += new System.EventHandler(this.HandleActiveFrequencyChanged);
			// 
			// txtNoWorkFreq
			// 
			this.txtNoWorkFreq.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.txtNoWorkFreq.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.txtNoWorkFreq.InputType = Tct.ActivityRecorderClient.View.Controls.SmartTextBoxType.PositiveInteger;
			this.txtNoWorkFreq.Location = new System.Drawing.Point(441, 21);
			this.txtNoWorkFreq.Name = "txtNoWorkFreq";
			this.txtNoWorkFreq.Size = new System.Drawing.Size(59, 23);
			this.txtNoWorkFreq.TabIndex = 34;
			this.txtNoWorkFreq.TextSaved += new System.EventHandler(this.HandleNoWorkFrequencyChanged);
			// 
			// lblActiveFreq
			// 
			this.lblActiveFreq.AutoSize = true;
			this.lblActiveFreq.Location = new System.Drawing.Point(21, 55);
			this.lblActiveFreq.Name = "lblActiveFreq";
			this.lblActiveFreq.Size = new System.Drawing.Size(81, 19);
			this.lblActiveFreq.Style = MetroFramework.MetroColorStyle.Blue;
			this.lblActiveFreq.TabIndex = 20;
			this.lblActiveFreq.Text = "Aktív feladat";
			this.lblActiveFreq.Theme = MetroFramework.MetroThemeStyle.Light;
			// 
			// lblNoWorkFreq
			// 
			this.lblNoWorkFreq.AutoSize = true;
			this.lblNoWorkFreq.Location = new System.Drawing.Point(21, 25);
			this.lblNoWorkFreq.Name = "lblNoWorkFreq";
			this.lblNoWorkFreq.Size = new System.Drawing.Size(123, 19);
			this.lblNoWorkFreq.Style = MetroFramework.MetroColorStyle.Blue;
			this.lblNoWorkFreq.TabIndex = 2244;
			this.lblNoWorkFreq.Text = "Nem munka státusz";
			this.lblNoWorkFreq.Theme = MetroFramework.MetroThemeStyle.Light;
			// 
			// lblNotificationFreq
			// 
			this.lblNotificationFreq.AutoSize = true;
			this.lblNotificationFreq.FontWeight = MetroFramework.MetroLabelWeight.Bold;
			this.lblNotificationFreq.Location = new System.Drawing.Point(0, 0);
			this.lblNotificationFreq.Name = "lblNotificationFreq";
			this.lblNotificationFreq.Size = new System.Drawing.Size(212, 19);
			this.lblNotificationFreq.Style = MetroFramework.MetroColorStyle.Blue;
			this.lblNotificationFreq.TabIndex = 17;
			this.lblNotificationFreq.Text = "Figyelmeztetések gyakorisága";
			this.lblNotificationFreq.Theme = MetroFramework.MetroThemeStyle.Light;
			// 
			// cbNotificationPos
			// 
			this.cbNotificationPos.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cbNotificationPos.DisplayMember = "Text";
			this.cbNotificationPos.ItemHeight = 23;
			this.cbNotificationPos.Items.AddRange(new object[] {
            "Jobb lent",
            "Bal lent",
            "Középen",
            "Jobb fent",
            "Bal fent"});
			this.cbNotificationPos.Location = new System.Drawing.Point(329, 16);
			this.cbNotificationPos.Name = "cbNotificationPos";
			this.cbNotificationPos.Size = new System.Drawing.Size(174, 29);
			this.cbNotificationPos.Style = MetroFramework.MetroColorStyle.Blue;
			this.cbNotificationPos.TabIndex = 30;
			this.cbNotificationPos.Theme = MetroFramework.MetroThemeStyle.Light;
			this.cbNotificationPos.UseSelectable = true;
			this.cbNotificationPos.ValueMember = "Value";
			this.cbNotificationPos.SelectedValueChanged += new System.EventHandler(this.HandleNotificationPositionChanged);
			// 
			// lblNotificationPos
			// 
			this.lblNotificationPos.AutoSize = true;
			this.lblNotificationPos.Location = new System.Drawing.Point(3, 21);
			this.lblNotificationPos.Name = "lblNotificationPos";
			this.lblNotificationPos.Size = new System.Drawing.Size(126, 19);
			this.lblNotificationPos.Style = MetroFramework.MetroColorStyle.Blue;
			this.lblNotificationPos.TabIndex = 2;
			this.lblNotificationPos.Text = "Felugró ablak helye:";
			this.lblNotificationPos.Theme = MetroFramework.MetroThemeStyle.Light;
			// 
			// tgWorkChangeNotification
			// 
			this.tgWorkChangeNotification.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tgWorkChangeNotification.BackColor = System.Drawing.SystemColors.Window;
			this.tgWorkChangeNotification.Checked = false;
			this.tgWorkChangeNotification.Location = new System.Drawing.Point(3, 54);
			this.tgWorkChangeNotification.MinimumSize = new System.Drawing.Size(400, 22);
			this.tgWorkChangeNotification.Name = "tgWorkChangeNotification";
			this.tgWorkChangeNotification.Size = new System.Drawing.Size(500, 22);
			this.tgWorkChangeNotification.TabIndex = 32;
			this.tgWorkChangeNotification.TextOff = "Kikapcsolva";
			this.tgWorkChangeNotification.TextOn = "Bekapcsolva";
			this.tgWorkChangeNotification.Title = "Feladatváltás esetén figyelmeztetés";
			this.tgWorkChangeNotification.CheckedChanged += new System.EventHandler(this.HandleWorkChangeNotificationChanged);
			// 
			// tabHotKeys
			// 
			this.tabHotKeys.Controls.Add(this.btnHotKeyReset);
			this.tabHotKeys.Controls.Add(this.btnHotKeySave);
			this.tabHotKeys.Controls.Add(this.gvHotKey);
			this.tabHotKeys.Controls.Add(this.btnHotKeyCreate);
			this.tabHotKeys.Controls.Add(this.btnHotKeyDelete);
			this.tabHotKeys.HorizontalScrollbarBarColor = true;
			this.tabHotKeys.HorizontalScrollbarHighlightOnWheel = false;
			this.tabHotKeys.HorizontalScrollbarSize = 10;
			this.tabHotKeys.Location = new System.Drawing.Point(4, 38);
			this.tabHotKeys.Name = "tabHotKeys";
			this.tabHotKeys.Size = new System.Drawing.Size(503, 359);
			this.tabHotKeys.Style = MetroFramework.MetroColorStyle.Blue;
			this.tabHotKeys.TabIndex = 1;
			this.tabHotKeys.Text = "Gyorsbillentyűk";
			this.tabHotKeys.Theme = MetroFramework.MetroThemeStyle.Light;
			this.tabHotKeys.VerticalScrollbarBarColor = true;
			this.tabHotKeys.VerticalScrollbarHighlightOnWheel = false;
			this.tabHotKeys.VerticalScrollbarSize = 10;
			// 
			// btnHotKeyReset
			// 
			this.btnHotKeyReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnHotKeyReset.Location = new System.Drawing.Point(367, 262);
			this.btnHotKeyReset.Name = "btnHotKeyReset";
			this.btnHotKeyReset.Size = new System.Drawing.Size(133, 35);
			this.btnHotKeyReset.Style = MetroFramework.MetroColorStyle.Blue;
			this.btnHotKeyReset.TabIndex = 44;
			this.btnHotKeyReset.Text = "Cancel changes";
			this.btnHotKeyReset.Theme = MetroFramework.MetroThemeStyle.Light;
			this.btnHotKeyReset.UseSelectable = true;
			this.btnHotKeyReset.Click += new System.EventHandler(this.HandleHotkeyResetClicked);
			// 
			// btnHotKeySave
			// 
			this.btnHotKeySave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnHotKeySave.Location = new System.Drawing.Point(88, 262);
			this.btnHotKeySave.Name = "btnHotKeySave";
			this.btnHotKeySave.Size = new System.Drawing.Size(86, 35);
			this.btnHotKeySave.Style = MetroFramework.MetroColorStyle.Blue;
			this.btnHotKeySave.TabIndex = 42;
			this.btnHotKeySave.Text = "Save";
			this.btnHotKeySave.Theme = MetroFramework.MetroThemeStyle.Light;
			this.btnHotKeySave.UseSelectable = true;
			this.btnHotKeySave.Click += new System.EventHandler(this.HandleHotkeySaveClicked);
			// 
			// gvHotKey
			// 
			this.gvHotKey.AllowUserToAddRows = false;
			this.gvHotKey.AllowUserToResizeRows = false;
			this.gvHotKey.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.gvHotKey.BackgroundColor = System.Drawing.SystemColors.ControlLightLight;
			this.gvHotKey.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.gvHotKey.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleVertical;
			this.gvHotKey.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
			dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.ControlLightLight;
			dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.HotTrack;
			dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.gvHotKey.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
			this.gvHotKey.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.gvHotKey.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.keyCodeColumn,
            this.shiftColumn,
            this.controlColumn,
            this.altColumn,
            this.windowsColumn,
            this.actionTypeColumn,
            this.workDataIdColumn,
            this.websiteColumn});
			this.gvHotKey.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
			this.gvHotKey.EnableHeadersVisualStyles = false;
			this.gvHotKey.GridColor = System.Drawing.SystemColors.ControlLightLight;
			this.gvHotKey.Location = new System.Drawing.Point(0, 3);
			this.gvHotKey.MultiSelect = false;
			this.gvHotKey.Name = "gvHotKey";
			this.gvHotKey.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
			dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.ControlLightLight;
			dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.HotTrack;
			dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.gvHotKey.RowHeadersDefaultCellStyle = dataGridViewCellStyle2;
			this.gvHotKey.Size = new System.Drawing.Size(503, 250);
			this.gvHotKey.TabIndex = 40;
			this.gvHotKey.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.HandleHotkeyPainting);
			this.gvHotKey.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.HandleHotkeyChanged);
			this.gvHotKey.CurrentCellDirtyStateChanged += new System.EventHandler(this.gvHotKey_CurrentCellDirtyStateChanged);
			this.gvHotKey.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.gvHotKey_DataError);
			this.gvHotKey.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(this.HandleHotkeyEditShowing);
			this.gvHotKey.RowValidating += new System.Windows.Forms.DataGridViewCellCancelEventHandler(this.HandleHotkeyValidating);
			// 
			// keyCodeColumn
			// 
			this.keyCodeColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
			this.keyCodeColumn.DataPropertyName = "KeyCode";
			this.keyCodeColumn.HeaderText = "KeyCode";
			this.keyCodeColumn.Name = "keyCodeColumn";
			this.keyCodeColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.keyCodeColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			this.keyCodeColumn.Width = 73;
			// 
			// shiftColumn
			// 
			this.shiftColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
			this.shiftColumn.DataPropertyName = "Shift";
			this.shiftColumn.HeaderText = "Shift";
			this.shiftColumn.Name = "shiftColumn";
			this.shiftColumn.Width = 32;
			// 
			// controlColumn
			// 
			this.controlColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
			this.controlColumn.DataPropertyName = "Control";
			this.controlColumn.HeaderText = "Ctrl";
			this.controlColumn.Name = "controlColumn";
			this.controlColumn.Width = 26;
			// 
			// altColumn
			// 
			this.altColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
			this.altColumn.DataPropertyName = "Alt";
			this.altColumn.HeaderText = "Alt";
			this.altColumn.Name = "altColumn";
			this.altColumn.Width = 23;
			// 
			// windowsColumn
			// 
			this.windowsColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
			this.windowsColumn.DataPropertyName = "Windows";
			this.windowsColumn.HeaderText = "Windows";
			this.windowsColumn.Name = "windowsColumn";
			this.windowsColumn.Width = 55;
			// 
			// actionTypeColumn
			// 
			this.actionTypeColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.actionTypeColumn.DataPropertyName = "ActionType";
			this.actionTypeColumn.HeaderText = "ActionType";
			this.actionTypeColumn.Name = "actionTypeColumn";
			this.actionTypeColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			this.actionTypeColumn.Width = 84;
			// 
			// workDataIdColumn
			// 
			this.workDataIdColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.workDataIdColumn.DataPropertyName = "WorkDataId";
			this.workDataIdColumn.HeaderText = "WorkDataId";
			this.workDataIdColumn.Name = "workDataIdColumn";
			this.workDataIdColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			this.workDataIdColumn.Width = 88;
			// 
			// websiteColumn
			// 
			this.websiteColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.websiteColumn.DataPropertyName = "Website";
			this.websiteColumn.HeaderText = "Website";
			this.websiteColumn.Name = "websiteColumn";
			this.websiteColumn.Width = 50;
			// 
			// btnHotKeyCreate
			// 
			this.btnHotKeyCreate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnHotKeyCreate.Location = new System.Drawing.Point(3, 262);
			this.btnHotKeyCreate.Name = "btnHotKeyCreate";
			this.btnHotKeyCreate.Size = new System.Drawing.Size(79, 35);
			this.btnHotKeyCreate.Style = MetroFramework.MetroColorStyle.Blue;
			this.btnHotKeyCreate.TabIndex = 41;
			this.btnHotKeyCreate.Text = "New";
			this.btnHotKeyCreate.Theme = MetroFramework.MetroThemeStyle.Light;
			this.btnHotKeyCreate.UseSelectable = true;
			this.btnHotKeyCreate.Click += new System.EventHandler(this.HandleHotkeyCreateClicked);
			// 
			// btnHotKeyDelete
			// 
			this.btnHotKeyDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnHotKeyDelete.Location = new System.Drawing.Point(278, 262);
			this.btnHotKeyDelete.Name = "btnHotKeyDelete";
			this.btnHotKeyDelete.Size = new System.Drawing.Size(83, 35);
			this.btnHotKeyDelete.Style = MetroFramework.MetroColorStyle.Blue;
			this.btnHotKeyDelete.TabIndex = 43;
			this.btnHotKeyDelete.Text = "Delete";
			this.btnHotKeyDelete.Theme = MetroFramework.MetroThemeStyle.Light;
			this.btnHotKeyDelete.UseSelectable = true;
			this.btnHotKeyDelete.Click += new System.EventHandler(this.HandleHotkeyDeleteClicked);
			// 
			// dataGridViewComboBoxColumn1
			// 
			this.dataGridViewComboBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
			this.dataGridViewComboBoxColumn1.DataPropertyName = "KeyCode";
			this.dataGridViewComboBoxColumn1.HeaderText = "KeyCode";
			this.dataGridViewComboBoxColumn1.Name = "dataGridViewComboBoxColumn1";
			this.dataGridViewComboBoxColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.dataGridViewComboBoxColumn1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			// 
			// dataGridViewCheckBoxColumn1
			// 
			this.dataGridViewCheckBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
			this.dataGridViewCheckBoxColumn1.DataPropertyName = "Shift";
			this.dataGridViewCheckBoxColumn1.HeaderText = "Shift";
			this.dataGridViewCheckBoxColumn1.Name = "dataGridViewCheckBoxColumn1";
			// 
			// dataGridViewCheckBoxColumn2
			// 
			this.dataGridViewCheckBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
			this.dataGridViewCheckBoxColumn2.DataPropertyName = "Control";
			this.dataGridViewCheckBoxColumn2.HeaderText = "Ctrl";
			this.dataGridViewCheckBoxColumn2.Name = "dataGridViewCheckBoxColumn2";
			// 
			// dataGridViewCheckBoxColumn3
			// 
			this.dataGridViewCheckBoxColumn3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
			this.dataGridViewCheckBoxColumn3.DataPropertyName = "Alt";
			this.dataGridViewCheckBoxColumn3.HeaderText = "Alt";
			this.dataGridViewCheckBoxColumn3.Name = "dataGridViewCheckBoxColumn3";
			// 
			// dataGridViewCheckBoxColumn4
			// 
			this.dataGridViewCheckBoxColumn4.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
			this.dataGridViewCheckBoxColumn4.DataPropertyName = "Windows";
			this.dataGridViewCheckBoxColumn4.HeaderText = "Windows";
			this.dataGridViewCheckBoxColumn4.Name = "dataGridViewCheckBoxColumn4";
			// 
			// dataGridViewComboBoxColumn2
			// 
			this.dataGridViewComboBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.dataGridViewComboBoxColumn2.DataPropertyName = "ActionType";
			this.dataGridViewComboBoxColumn2.HeaderText = "ActionType";
			this.dataGridViewComboBoxColumn2.Name = "dataGridViewComboBoxColumn2";
			this.dataGridViewComboBoxColumn2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			// 
			// dataGridViewComboBoxColumn3
			// 
			this.dataGridViewComboBoxColumn3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.dataGridViewComboBoxColumn3.DataPropertyName = "WorkDataId";
			this.dataGridViewComboBoxColumn3.HeaderText = "WorkDataId";
			this.dataGridViewComboBoxColumn3.Name = "dataGridViewComboBoxColumn3";
			this.dataGridViewComboBoxColumn3.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			// 
			// isRegexDataGridViewCheckBoxColumn
			// 
			this.isRegexDataGridViewCheckBoxColumn.DataPropertyName = "IsRegex";
			this.isRegexDataGridViewCheckBoxColumn.HeaderText = "Reguláris";
			this.isRegexDataGridViewCheckBoxColumn.Name = "isRegexDataGridViewCheckBoxColumn";
			this.isRegexDataGridViewCheckBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.isRegexDataGridViewCheckBoxColumn.ToolTipText = "Reguláris kifejezés";
			this.isRegexDataGridViewCheckBoxColumn.Width = 70;
			// 
			// titleRuleDataGridViewTextBoxColumn
			// 
			this.titleRuleDataGridViewTextBoxColumn.DataPropertyName = "TitleRule";
			this.titleRuleDataGridViewTextBoxColumn.FilterString = "";
			dataGridViewFilterColumnHeaderCell1.ErrorText = "";
			dataGridViewFilterColumnHeaderCell1.FilterString = "";
			dataGridViewCellStyle3.Padding = new System.Windows.Forms.Padding(0, 0, 54, 0);
			dataGridViewFilterColumnHeaderCell1.Style = dataGridViewCellStyle3;
			dataGridViewFilterColumnHeaderCell1.Value = "Címsor szabály";
			dataGridViewFilterColumnHeaderCell1.ValueType = typeof(object);
			this.titleRuleDataGridViewTextBoxColumn.HeaderCell = dataGridViewFilterColumnHeaderCell1;
			this.titleRuleDataGridViewTextBoxColumn.HeaderText = "Címsor szabály";
			this.titleRuleDataGridViewTextBoxColumn.Name = "titleRuleDataGridViewTextBoxColumn";
			this.titleRuleDataGridViewTextBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			this.titleRuleDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			this.titleRuleDataGridViewTextBoxColumn.ToolTipText = "Címsor szabály";
			this.titleRuleDataGridViewTextBoxColumn.Width = 160;
			// 
			// processRuleDataGridViewTextBoxColumn
			// 
			this.processRuleDataGridViewTextBoxColumn.DataPropertyName = "ProcessRule";
			this.processRuleDataGridViewTextBoxColumn.FilterString = "";
			dataGridViewFilterColumnHeaderCell2.ErrorText = "";
			dataGridViewFilterColumnHeaderCell2.FilterString = "";
			dataGridViewCellStyle4.Padding = new System.Windows.Forms.Padding(0, 0, 18, 0);
			dataGridViewFilterColumnHeaderCell2.Style = dataGridViewCellStyle4;
			dataGridViewFilterColumnHeaderCell2.Value = "Processz szabály";
			dataGridViewFilterColumnHeaderCell2.ValueType = typeof(object);
			this.processRuleDataGridViewTextBoxColumn.HeaderCell = dataGridViewFilterColumnHeaderCell2;
			this.processRuleDataGridViewTextBoxColumn.HeaderText = "Processz szabály";
			this.processRuleDataGridViewTextBoxColumn.Name = "processRuleDataGridViewTextBoxColumn";
			this.processRuleDataGridViewTextBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			this.processRuleDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			this.processRuleDataGridViewTextBoxColumn.ToolTipText = "Processz szabály";
			// 
			// ignoreCaseDataGridViewCheckBoxColumn
			// 
			this.ignoreCaseDataGridViewCheckBoxColumn.DataPropertyName = "IgnoreCase";
			this.ignoreCaseDataGridViewCheckBoxColumn.HeaderText = "Kis/nagybet. megegyezik";
			this.ignoreCaseDataGridViewCheckBoxColumn.Name = "ignoreCaseDataGridViewCheckBoxColumn";
			this.ignoreCaseDataGridViewCheckBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.ignoreCaseDataGridViewCheckBoxColumn.ToolTipText = "Kis- és nagybetű megegyezik";
			this.ignoreCaseDataGridViewCheckBoxColumn.Width = 70;
			// 
			// isEnabledDataGridViewCheckBoxColumn
			// 
			this.isEnabledDataGridViewCheckBoxColumn.DataPropertyName = "IsEnabled";
			this.isEnabledDataGridViewCheckBoxColumn.HeaderText = "Aktív";
			this.isEnabledDataGridViewCheckBoxColumn.Name = "isEnabledDataGridViewCheckBoxColumn";
			this.isEnabledDataGridViewCheckBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.isEnabledDataGridViewCheckBoxColumn.ToolTipText = "Aktív szabály";
			this.isEnabledDataGridViewCheckBoxColumn.Width = 50;
			// 
			// PreferencesForm
			// 
			this.AccessibleName = "";
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(570, 487);
			this.Controls.Add(this.metroTab);
			this.MinimumSize = new System.Drawing.Size(570, 487);
			this.Name = "PreferencesForm";
			this.StyleManager = this.metroStyleManager1;
			this.Text = "JobCTRL testreszabás";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.HandleFormClosed);
			((System.ComponentModel.ISupportInitialize)(this.metroStyleManager1)).EndInit();
			this.metroTab.ResumeLayout(false);
			this.tabGeneric.ResumeLayout(false);
			this.tabGeneric.PerformLayout();
			this.pGeneralInt.ResumeLayout(false);
			this.pGeneralInt.PerformLayout();
			this.pShow.ResumeLayout(false);
			this.tabNavigation.ResumeLayout(false);
			this.pNavigationInts.ResumeLayout(false);
			this.pNavigationInts.PerformLayout();
			this.tabMenu.ResumeLayout(false);
			this.tabNotifications.ResumeLayout(false);
			this.tabNotifications.PerformLayout();
			this.pNotificationLen.ResumeLayout(false);
			this.pNotificationLen.PerformLayout();
			this.pNotificationFreq.ResumeLayout(false);
			this.pNotificationFreq.PerformLayout();
			this.tabHotKeys.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.gvHotKey)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal MetroFramework.Components.MetroStyleManager metroStyleManager1;
		private MetroFramework.Controls.MetroTabControl metroTab;
		private MetroFramework.Controls.MetroTabPage tabGeneric;
		private MetroFramework.Controls.MetroTabPage tabHotKeys;
		private MetroFramework.Controls.MetroTabPage tabNotifications;
		private MetroFramework.Controls.MetroPanel pShow;
		private MetroFramework.Controls.MetroComboBox cbDblClick;
		private MetroFramework.Controls.MetroLabel lblDoubleClick;
		private MetroFramework.Controls.MetroComboBox cbLanguage;
		private MetroFramework.Controls.MetroLabel lblLanguage;
		private MetroFramework.Controls.MetroPanel pNotificationLen;
		private MetroFramework.Controls.MetroLabel lblWorkChangeLen;
		private MetroFramework.Controls.MetroLabel lblNoWorkLen;
		private MetroFramework.Controls.MetroLabel lblActiveLen;
		private MetroFramework.Controls.MetroLabel lblNotificationLen;
		private MetroFramework.Controls.MetroPanel pNotificationFreq;
		private MetroFramework.Controls.MetroLabel lblActiveFreq;
		private MetroFramework.Controls.MetroLabel lblNoWorkFreq;
		private MetroFramework.Controls.MetroLabel lblNotificationFreq;
		private MetroFramework.Controls.MetroComboBox cbNotificationPos;
		private MetroFramework.Controls.MetroLabel lblNotificationPos;
		private MetroFramework.Controls.MetroPanel pGeneralInt;
		private MetroFramework.Controls.MetroButton btnHotKeyDelete;
		private MetroFramework.Controls.MetroButton btnHotKeyCreate;
		private System.Windows.Forms.DataGridView gvHotKey;
		private System.Windows.Forms.DataGridViewComboBoxColumn dataGridViewComboBoxColumn1;
		private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn1;
		private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn2;
		private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn3;
		private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn4;
		private System.Windows.Forms.DataGridViewComboBoxColumn dataGridViewComboBoxColumn2;
		private System.Windows.Forms.DataGridViewComboBoxColumn dataGridViewComboBoxColumn3;
		private ToggleGroup tgSumDelta;
		private ToggleGroup tgWeekly;
		private ToggleGroup tgHighlight;
		private ToggleGroup tgWorkChangeNotification;
		private MetroFramework.Controls.MetroLabel lblFlatten;
		private MetroFramework.Controls.MetroTabPage tabNavigation;
		private ToggleGroup tgSearchClosed;
		private ToggleGroup tgSearchOwn;
		private MetroFramework.Controls.MetroPanel pNavigationInts;
		private MetroFramework.Controls.MetroLabel lblWorkHistory;
		private MetroFramework.Controls.MetroLabel lblWorkQty;
		private MetroFramework.Controls.MetroButton btnRefreshWork;
		private MetroFramework.Controls.MetroButton btnHotKeyReset;
		private MetroFramework.Controls.MetroButton btnHotKeySave;
		
		private System.Windows.Forms.DataGridViewCheckBoxColumn isRegexDataGridViewCheckBoxColumn;
		private DataGridViewFilterTextBoxColumn titleRuleDataGridViewTextBoxColumn;
		private DataGridViewFilterTextBoxColumn processRuleDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewCheckBoxColumn ignoreCaseDataGridViewCheckBoxColumn;
		private System.Windows.Forms.DataGridViewCheckBoxColumn isEnabledDataGridViewCheckBoxColumn;
		private ToggleGroup tgDynamicWork;
		private SmartTextBox txtWorkQty;
		private SmartTextBox txtWorkHistory;
		private SmartTextBox txtFlatten;
		private SmartTextBox txtActiveLen;
		private SmartTextBox txtNoWorkLen;
		private SmartTextBox txtActiveFreq;
		private SmartTextBox txtNoWorkFreq;
		private SmartTextBox txtWorkChangeLen;
		private ToggleGroup tgOldMenu;
		private MetroFramework.Controls.MetroTabPage tabMenu;
		private ToggleGroup tgShowAll;
		private ToggleGroup tgShowProgress;
		private ToggleGroup tgShowPriority;
		private ToggleGroup tgShowDeadline;
		private ToggleGroup tgShowRecentProject;
		private ToggleGroup tgShowRecent;
		private ToggleGroup tgShowFavorite;
		private ToggleGroup tgShowRecentClosed;
		private System.Windows.Forms.DataGridViewComboBoxColumn keyCodeColumn;
		private System.Windows.Forms.DataGridViewCheckBoxColumn shiftColumn;
		private System.Windows.Forms.DataGridViewCheckBoxColumn controlColumn;
		private System.Windows.Forms.DataGridViewCheckBoxColumn altColumn;
		private System.Windows.Forms.DataGridViewCheckBoxColumn windowsColumn;
		private System.Windows.Forms.DataGridViewComboBoxColumn actionTypeColumn;
		private System.Windows.Forms.DataGridViewComboBoxColumn workDataIdColumn;
		private System.Windows.Forms.DataGridViewComboBoxColumn websiteColumn;
		private ToggleGroup tgYearly;
		private ToggleGroup tgQuarterly;
		private ToggleGroup tgMonthly;
	}
}