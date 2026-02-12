namespace Tct.ActivityRecorderClient.View
{
	partial class ActivityRecorderForm
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
			this.taskbarTimer = new System.Windows.Forms.Timer(this.components);
			this.niTaskbar = new System.Windows.Forms.NotifyIcon(this.components);
			this.cmMenu = new Tct.ActivityRecorderClient.View.ToolStrip.ScrollableContextMenuStrip(this.components);
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.miSettings = new System.Windows.Forms.ToolStripMenuItem();
			this.miLogout = new System.Windows.Forms.ToolStripMenuItem();
			this.miWorkDetectorRules = new System.Windows.Forms.ToolStripMenuItem();
			this.miRunAsAdmin = new System.Windows.Forms.ToolStripMenuItem();
			this.miErrorResolution = new System.Windows.Forms.ToolStripMenuItem();
			this.miOpenLog = new System.Windows.Forms.ToolStripMenuItem();
			this.miLogLevelChange = new System.Windows.Forms.ToolStripMenuItem();
			this.miSafeMailItemCommit = new System.Windows.Forms.ToolStripMenuItem();
			this.miDiagDebugMode = new System.Windows.Forms.ToolStripMenuItem();
			this.miDiagDebugDisableDomCapture = new System.Windows.Forms.ToolStripMenuItem();
			this.miDiagDebugDisableJcMail = new System.Windows.Forms.ToolStripMenuItem();
			this.miDiagDebugDisableOlAddin = new System.Windows.Forms.ToolStripMenuItem();
			this.miDiagDebugDisableOutlookMeetingSync = new System.Windows.Forms.ToolStripMenuItem();
			this.miDiagDebugDisableLotusMeetingSync = new System.Windows.Forms.ToolStripMenuItem();
			this.miDiagDebugDisableAutomationPlugin = new System.Windows.Forms.ToolStripMenuItem();
			this.miDiagDebugDisableAllPlugin = new System.Windows.Forms.ToolStripMenuItem();
			this.miDiagDebugDisableUrlCapture = new System.Windows.Forms.ToolStripMenuItem();
			this.miDiagDebugDisableTitleCapture = new System.Windows.Forms.ToolStripMenuItem();
			this.miDiagDebugDisableProcessCapture = new System.Windows.Forms.ToolStripMenuItem();
			this.miOpenMeetingTool = new System.Windows.Forms.ToolStripMenuItem();
			this.miOpenMeetingLog = new System.Windows.Forms.ToolStripMenuItem();
			this.miDomCapture = new System.Windows.Forms.ToolStripMenuItem();
			this.miOpenErrorReporting = new System.Windows.Forms.ToolStripMenuItem();
			this.miIdleAlert = new System.Windows.Forms.ToolStripMenuItem();
			this.miIdleAlertVisual = new System.Windows.Forms.ToolStripMenuItem();
			this.miIdleAlertBeep = new System.Windows.Forms.ToolStripMenuItem();
			this.miDiagnosticTool = new System.Windows.Forms.ToolStripMenuItem();
			this.miOpenContributionForm = new System.Windows.Forms.ToolStripMenuItem();
			this.adminCenterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.miPreferences = new System.Windows.Forms.ToolStripMenuItem();
			this.miExit = new System.Windows.Forms.ToolStripMenuItem();
			this.niStatusIcon = new System.Windows.Forms.NotifyIcon(this.components);
			this.miOpenJCMon = new System.Windows.Forms.ToolStripMenuItem();
			this.cmMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// taskbarTimer
			// 
			this.taskbarTimer.Interval = 1000;
			this.taskbarTimer.Tick += new System.EventHandler(this.taskbarTimer_Tick);
			// 
			// niTaskbar
			// 
			this.niTaskbar.Text = "ActivityRecorder";
			this.niTaskbar.Visible = true;
			this.niTaskbar.MouseClick += new System.Windows.Forms.MouseEventHandler(this.niTaskbar_MouseClick);
			this.niTaskbar.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.niTaskbar_MouseDoubleClick);
			this.niTaskbar.MouseMove += new System.Windows.Forms.MouseEventHandler(this.niTaskbar_MouseMove);
			// 
			// cmMenu
			// 
			this.cmMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator1,
            this.miSettings,
            this.miExit});
			this.cmMenu.Name = "cmMenu";
			this.cmMenu.Size = new System.Drawing.Size(181, 76);
			this.cmMenu.Opening += new System.ComponentModel.CancelEventHandler(this.cmMenu_Opening);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(177, 6);
			// 
			// miSettings
			// 
			this.miSettings.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miLogout,
            this.miWorkDetectorRules,
            this.miRunAsAdmin,
            this.miErrorResolution,
            this.adminCenterToolStripMenuItem,
            this.miPreferences});
			this.miSettings.Name = "miSettings";
			this.miSettings.Size = new System.Drawing.Size(180, 22);
			this.miSettings.Text = "Beállítások";
			// 
			// miLogout
			// 
			this.miLogout.Name = "miLogout";
			this.miLogout.Size = new System.Drawing.Size(217, 22);
			this.miLogout.Text = "UserId váltás";
			this.miLogout.Click += new System.EventHandler(this.miLogout_Click);
			// 
			// miWorkDetectorRules
			// 
			this.miWorkDetectorRules.Name = "miWorkDetectorRules";
			this.miWorkDetectorRules.Size = new System.Drawing.Size(217, 22);
			this.miWorkDetectorRules.Text = "Automatikus szabályok...";
			this.miWorkDetectorRules.Click += new System.EventHandler(this.miWorkDetectorRules_Click);
			// 
			// miRunAsAdmin
			// 
			this.miRunAsAdmin.Name = "miRunAsAdmin";
			this.miRunAsAdmin.Size = new System.Drawing.Size(217, 22);
			this.miRunAsAdmin.Text = "Futtatas rendszergazdakent";
			this.miRunAsAdmin.Click += new System.EventHandler(this.miRunAsAdmin_Click);
			// 
			// miErrorResolution
			// 
			this.miErrorResolution.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miOpenLog,
            this.miLogLevelChange,
            this.miSafeMailItemCommit,
            this.miDiagDebugMode,
            this.miOpenMeetingTool,
            this.miOpenMeetingLog,
            this.miDomCapture,
            this.miOpenErrorReporting,
            this.miIdleAlert,
            this.miDiagnosticTool,
            this.miOpenContributionForm,
            this.miOpenJCMon});
			this.miErrorResolution.Name = "miErrorResolution";
			this.miErrorResolution.Size = new System.Drawing.Size(217, 22);
			this.miErrorResolution.Text = "Hibaelhárítás";
			// 
			// miOpenLog
			// 
			this.miOpenLog.Name = "miOpenLog";
			this.miOpenLog.Size = new System.Drawing.Size(243, 22);
			this.miOpenLog.Text = "Naplófájl megnyitása...";
			this.miOpenLog.Click += new System.EventHandler(this.miOpenLog_Click);
			// 
			// miLogLevelChange
			// 
			this.miLogLevelChange.Name = "miLogLevelChange";
			this.miLogLevelChange.Size = new System.Drawing.Size(243, 22);
			this.miLogLevelChange.Text = "Részletes naplózás bekapcsolása";
			this.miLogLevelChange.Click += new System.EventHandler(this.miLogLevelChange_Click);
			// 
			// miSafeMailItemCommit
			// 
			this.miSafeMailItemCommit.Name = "miSafeMailItemCommit";
			this.miSafeMailItemCommit.Size = new System.Drawing.Size(243, 22);
			this.miSafeMailItemCommit.Text = "SafeMailItem.Commit";
			this.miSafeMailItemCommit.Click += new System.EventHandler(this.miSafeMailItemCommit_Click);
			// 
			// miDiagDebugMode
			// 
			this.miDiagDebugMode.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miDiagDebugDisableDomCapture,
            this.miDiagDebugDisableJcMail,
            this.miDiagDebugDisableOlAddin,
            this.miDiagDebugDisableOutlookMeetingSync,
            this.miDiagDebugDisableLotusMeetingSync,
            this.miDiagDebugDisableAutomationPlugin,
            this.miDiagDebugDisableAllPlugin,
            this.miDiagDebugDisableUrlCapture,
            this.miDiagDebugDisableTitleCapture,
            this.miDiagDebugDisableProcessCapture});
			this.miDiagDebugMode.Name = "miDiagDebugMode";
			this.miDiagDebugMode.Size = new System.Drawing.Size(243, 22);
			this.miDiagDebugMode.Text = "Adatkiolvasas hibakeresese";
			this.miDiagDebugMode.Visible = false;
			// 
			// miDiagDebugDisableDomCapture
			// 
			this.miDiagDebugDisableDomCapture.Name = "miDiagDebugDisableDomCapture";
			this.miDiagDebugDisableDomCapture.Size = new System.Drawing.Size(186, 22);
			this.miDiagDebugDisableDomCapture.Tag = "DisableDomCapture";
			this.miDiagDebugDisableDomCapture.Text = "domcapture";
			this.miDiagDebugDisableDomCapture.Click += new System.EventHandler(this.miDiagDebugDisableClick);
			// 
			// miDiagDebugDisableJcMail
			// 
			this.miDiagDebugDisableJcMail.Name = "miDiagDebugDisableJcMail";
			this.miDiagDebugDisableJcMail.Size = new System.Drawing.Size(186, 22);
			this.miDiagDebugDisableJcMail.Tag = "DisableOutlookJcMailCapture";
			this.miDiagDebugDisableJcMail.Text = "jc.mail";
			this.miDiagDebugDisableJcMail.Click += new System.EventHandler(this.miDiagDebugDisableClick);
			// 
			// miDiagDebugDisableOlAddin
			// 
			this.miDiagDebugDisableOlAddin.Name = "miDiagDebugDisableOlAddin";
			this.miDiagDebugDisableOlAddin.Size = new System.Drawing.Size(186, 22);
			this.miDiagDebugDisableOlAddin.Tag = "DisableOutlookAddinCapture";
			this.miDiagDebugDisableOlAddin.Text = "outlook addin";
			this.miDiagDebugDisableOlAddin.Click += new System.EventHandler(this.miDiagDebugDisableClick);
			// 
			// miDiagDebugDisableOutlookMeetingSync
			// 
			this.miDiagDebugDisableOutlookMeetingSync.Name = "miDiagDebugDisableOutlookMeetingSync";
			this.miDiagDebugDisableOutlookMeetingSync.Size = new System.Drawing.Size(186, 22);
			this.miDiagDebugDisableOutlookMeetingSync.Tag = "DisableOutlookMeetingSync";
			this.miDiagDebugDisableOutlookMeetingSync.Text = "outlook meetingsync";
			this.miDiagDebugDisableOutlookMeetingSync.Click += new System.EventHandler(this.miDiagDebugDisableClick);
			// 
			// miDiagDebugDisableLotusMeetingSync
			// 
			this.miDiagDebugDisableLotusMeetingSync.Name = "miDiagDebugDisableLotusMeetingSync";
			this.miDiagDebugDisableLotusMeetingSync.Size = new System.Drawing.Size(186, 22);
			this.miDiagDebugDisableLotusMeetingSync.Tag = "DisableLotusMeetingSync";
			this.miDiagDebugDisableLotusMeetingSync.Text = "lotus meetingsync";
			this.miDiagDebugDisableLotusMeetingSync.Click += new System.EventHandler(this.miDiagDebugDisableClick);
			// 
			// miDiagDebugDisableAutomationPlugin
			// 
			this.miDiagDebugDisableAutomationPlugin.Name = "miDiagDebugDisableAutomationPlugin";
			this.miDiagDebugDisableAutomationPlugin.Size = new System.Drawing.Size(186, 22);
			this.miDiagDebugDisableAutomationPlugin.Tag = "DisableAutomationCapture";
			this.miDiagDebugDisableAutomationPlugin.Text = "automation  plugin";
			this.miDiagDebugDisableAutomationPlugin.Click += new System.EventHandler(this.miDiagDebugDisableClick);
			// 
			// miDiagDebugDisableAllPlugin
			// 
			this.miDiagDebugDisableAllPlugin.Name = "miDiagDebugDisableAllPlugin";
			this.miDiagDebugDisableAllPlugin.Size = new System.Drawing.Size(186, 22);
			this.miDiagDebugDisableAllPlugin.Tag = "DisableAllPluginCapture";
			this.miDiagDebugDisableAllPlugin.Text = "osszes plugin";
			this.miDiagDebugDisableAllPlugin.Click += new System.EventHandler(this.miDiagDebugDisableClick);
			// 
			// miDiagDebugDisableUrlCapture
			// 
			this.miDiagDebugDisableUrlCapture.Name = "miDiagDebugDisableUrlCapture";
			this.miDiagDebugDisableUrlCapture.Size = new System.Drawing.Size(186, 22);
			this.miDiagDebugDisableUrlCapture.Tag = "DisableUrlCapture";
			this.miDiagDebugDisableUrlCapture.Text = "url";
			this.miDiagDebugDisableUrlCapture.Click += new System.EventHandler(this.miDiagDebugDisableClick);
			// 
			// miDiagDebugDisableTitleCapture
			// 
			this.miDiagDebugDisableTitleCapture.Name = "miDiagDebugDisableTitleCapture";
			this.miDiagDebugDisableTitleCapture.Size = new System.Drawing.Size(186, 22);
			this.miDiagDebugDisableTitleCapture.Tag = "DisableTitleCapture";
			this.miDiagDebugDisableTitleCapture.Text = "title";
			this.miDiagDebugDisableTitleCapture.Click += new System.EventHandler(this.miDiagDebugDisableClick);
			// 
			// miDiagDebugDisableProcessCapture
			// 
			this.miDiagDebugDisableProcessCapture.Name = "miDiagDebugDisableProcessCapture";
			this.miDiagDebugDisableProcessCapture.Size = new System.Drawing.Size(186, 22);
			this.miDiagDebugDisableProcessCapture.Tag = "DisableProcessCapture";
			this.miDiagDebugDisableProcessCapture.Text = "processz";
			this.miDiagDebugDisableProcessCapture.Click += new System.EventHandler(this.miDiagDebugDisableClick);
			// 
			// miOpenMeetingTool
			// 
			this.miOpenMeetingTool.Name = "miOpenMeetingTool";
			this.miOpenMeetingTool.Size = new System.Drawing.Size(243, 22);
			this.miOpenMeetingTool.Text = "MeetingTool...";
			this.miOpenMeetingTool.Click += new System.EventHandler(this.miOpenMeetingTool_Click);
			// 
			// miOpenMeetingLog
			// 
			this.miOpenMeetingLog.Name = "miOpenMeetingLog";
			this.miOpenMeetingLog.Size = new System.Drawing.Size(243, 22);
			this.miOpenMeetingLog.Text = "Meeting naplófájl megnyitása...";
			this.miOpenMeetingLog.Click += new System.EventHandler(this.miOpenMeetingLog_Click);
			// 
			// miDomCapture
			// 
			this.miDomCapture.Name = "miDomCapture";
			this.miDomCapture.Size = new System.Drawing.Size(243, 22);
			this.miDomCapture.Text = "DomCapture...";
			this.miDomCapture.Click += new System.EventHandler(this.miDomCapture_Click);
			// 
			// miOpenErrorReporting
			// 
			this.miOpenErrorReporting.Name = "miOpenErrorReporting";
			this.miOpenErrorReporting.Size = new System.Drawing.Size(243, 22);
			this.miOpenErrorReporting.Text = "Hibabejelentés...";
			this.miOpenErrorReporting.Click += new System.EventHandler(this.miOpenErrorReporting_Click);
			// 
			// miIdleAlert
			// 
			this.miIdleAlert.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miIdleAlertVisual,
            this.miIdleAlertBeep});
			this.miIdleAlert.Name = "miIdleAlert";
			this.miIdleAlert.Size = new System.Drawing.Size(243, 22);
			this.miIdleAlert.Text = "Idle alert";
			// 
			// miIdleAlertVisual
			// 
			this.miIdleAlertVisual.Name = "miIdleAlertVisual";
			this.miIdleAlertVisual.Size = new System.Drawing.Size(105, 22);
			this.miIdleAlertVisual.Text = "Visual";
			this.miIdleAlertVisual.Click += new System.EventHandler(this.miIdleAlertVisual_Click);
			// 
			// miIdleAlertBeep
			// 
			this.miIdleAlertBeep.Name = "miIdleAlertBeep";
			this.miIdleAlertBeep.Size = new System.Drawing.Size(105, 22);
			this.miIdleAlertBeep.Text = "Beep";
			this.miIdleAlertBeep.Click += new System.EventHandler(this.miIdleAlertBeep_Click);
			// 
			// miDiagnosticTool
			// 
			this.miDiagnosticTool.Name = "miDiagnosticTool";
			this.miDiagnosticTool.Size = new System.Drawing.Size(243, 22);
			this.miDiagnosticTool.Text = "Diagnosztika...";
			this.miDiagnosticTool.Click += new System.EventHandler(this.HandleDiagnosticToolClicked);
			// 
			// miOpenContributionForm
			// 
			this.miOpenContributionForm.Name = "miOpenContributionForm";
			this.miOpenContributionForm.Size = new System.Drawing.Size(243, 22);
			this.miOpenContributionForm.Text = "Contribution Form...";
			// 
			// adminCenterToolStripMenuItem
			// 
			this.adminCenterToolStripMenuItem.Name = "adminCenterToolStripMenuItem";
			this.adminCenterToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
			this.adminCenterToolStripMenuItem.Text = "JobCTRL AdminCenter";
			// 
			// miPreferences
			// 
			this.miPreferences.Name = "miPreferences";
			this.miPreferences.Size = new System.Drawing.Size(217, 22);
			this.miPreferences.Text = "Testreszabás";
			this.miPreferences.Click += new System.EventHandler(this.miPreferencesClick);
			// 
			// miExit
			// 
			this.miExit.Name = "miExit";
			this.miExit.Size = new System.Drawing.Size(180, 22);
			this.miExit.Text = "Exit";
			this.miExit.Click += new System.EventHandler(this.miExit_Click);
			// 
			// miOpenJCMon
			// 
			this.miOpenJCMon.Name = "miOpenJCMon";
			this.miOpenJCMon.Size = new System.Drawing.Size(243, 22);
			this.miOpenJCMon.Text = "JCMon";
			this.miOpenJCMon.Click += new System.EventHandler(this.miOpenJCMon_Click);
			// 
			// ActivityRecorderForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(124, 50);
			this.Location = new System.Drawing.Point(-2000, -2000);
			this.Name = "ActivityRecorderForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "ActivityRecorderClient";
			this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ActivityRecorderForm_FormClosed);
			this.Load += new System.EventHandler(this.ActivityRecorderForm_Load);
			this.cmMenu.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.NotifyIcon niTaskbar;
		private Tct.ActivityRecorderClient.View.ToolStrip.ScrollableContextMenuStrip cmMenu;
		private System.Windows.Forms.ToolStripMenuItem miExit;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.Timer taskbarTimer;
		private System.Windows.Forms.ToolStripMenuItem miSettings;
		private System.Windows.Forms.ToolStripMenuItem miLogout;
		private System.Windows.Forms.ToolStripMenuItem miWorkDetectorRules;
		private System.Windows.Forms.ToolStripMenuItem adminCenterToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem miErrorResolution;
		private System.Windows.Forms.ToolStripMenuItem miOpenLog;
		private System.Windows.Forms.ToolStripMenuItem miOpenMeetingTool;
		private System.Windows.Forms.ToolStripMenuItem miOpenMeetingLog;
		private System.Windows.Forms.ToolStripMenuItem miDomCapture;
		private System.Windows.Forms.ToolStripMenuItem miRunAsAdmin;
		private System.Windows.Forms.ToolStripMenuItem miPreferences;
		private System.Windows.Forms.ToolStripMenuItem miOpenErrorReporting;
		private System.Windows.Forms.ToolStripMenuItem miLogLevelChange;
		private System.Windows.Forms.ToolStripMenuItem miDiagnosticTool;
		private System.Windows.Forms.ToolStripMenuItem miOpenContributionForm;
		private System.Windows.Forms.ToolStripMenuItem miDiagDebugMode;
		private System.Windows.Forms.ToolStripMenuItem miDiagDebugDisableDomCapture;
		private System.Windows.Forms.ToolStripMenuItem miDiagDebugDisableJcMail;
		private System.Windows.Forms.ToolStripMenuItem miDiagDebugDisableOlAddin;
		private System.Windows.Forms.ToolStripMenuItem miDiagDebugDisableOutlookMeetingSync;
		private System.Windows.Forms.ToolStripMenuItem miDiagDebugDisableAutomationPlugin;
		private System.Windows.Forms.ToolStripMenuItem miDiagDebugDisableAllPlugin;
		private System.Windows.Forms.ToolStripMenuItem miDiagDebugDisableUrlCapture;
		private System.Windows.Forms.ToolStripMenuItem miDiagDebugDisableTitleCapture;
		private System.Windows.Forms.ToolStripMenuItem miDiagDebugDisableProcessCapture;
		private System.Windows.Forms.ToolStripMenuItem miDiagDebugDisableLotusMeetingSync;
		private System.Windows.Forms.ToolStripMenuItem miIdleAlert;
		private System.Windows.Forms.ToolStripMenuItem miIdleAlertVisual;
		private System.Windows.Forms.ToolStripMenuItem miIdleAlertBeep;
		public System.Windows.Forms.NotifyIcon niStatusIcon;
		private System.Windows.Forms.ToolStripMenuItem miSafeMailItemCommit;
		private System.Windows.Forms.ToolStripMenuItem miOpenJCMon;
	}
}