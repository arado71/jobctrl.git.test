namespace TcT.OcrSnippets
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.notifyMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuSnip = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuShowSnippets = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuLanguageCombo = new System.Windows.Forms.ToolStripComboBox();
			this.mnuCharSetCombo = new System.Windows.Forms.ToolStripComboBox();
			this.mnuContributionModeCombo = new System.Windows.Forms.ToolStripComboBox();
	        this.mnuContentRegexTextbox = new System.Windows.Forms.ToolStripTextBox();
	        this.mnuIgnoreCaseCombo = new System.Windows.Forms.ToolStripComboBox(); ;
			this.mnuProcessSnippets = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuProcessAgain = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuClipboardNow = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMonitorClipboard = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripSeparator();
            this.mnuExit = new System.Windows.Forms.ToolStripMenuItem();
            this.notifyMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // notifyIcon
            // 
            this.notifyIcon.BalloonTipText = "Hola";
            this.notifyIcon.BalloonTipTitle = "OCR";
            this.notifyIcon.ContextMenuStrip = this.notifyMenu;
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "OCR";
            this.notifyIcon.Visible = true;
            this.notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon_MouseDoubleClick);
            // 
            // notifyMenu
            // 
            this.notifyMenu.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.notifyMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuSnip,
            this.mnuShowSnippets,
            this.mnuLanguageCombo,
			this.mnuCharSetCombo,
			this.mnuContributionModeCombo,
			this.mnuContentRegexTextbox,
			this.mnuIgnoreCaseCombo,
			this.mnuProcessSnippets,
            this.mnuProcessAgain,
            this.mnuClipboardNow,
            this.mnuMonitorClipboard,
            this.exitToolStripMenuItem,
            this.mnuExit});
            this.notifyMenu.Name = "notifyMenu";
            this.notifyMenu.Size = new System.Drawing.Size(224, 238);
            // 
            // mnuSnip
            // 
            this.mnuSnip.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.mnuSnip.Name = "mnuSnip";
            this.mnuSnip.Size = new System.Drawing.Size(223, 24);
            this.mnuSnip.Text = "Snip (CTRL+WIN+C)";
            this.mnuSnip.Click += new System.EventHandler(this.mnuSnip_Click);
            // 
            // mnuShowSnippets
            // 
            this.mnuShowSnippets.Enabled = false;
            this.mnuShowSnippets.Name = "mnuShowSnippets";
            this.mnuShowSnippets.Size = new System.Drawing.Size(223, 24);
            this.mnuShowSnippets.Text = "Display Snippets";
            this.mnuShowSnippets.Click += new System.EventHandler(this.mnuShowSnippets_Click);
            // 
            // mnuLanguageCombo
            // 
            this.mnuLanguageCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.mnuLanguageCombo.Name = "mnuLanguageCombo";
            this.mnuLanguageCombo.Size = new System.Drawing.Size(150, 28);
			// 
			// mnuCharSetCombo
			// 
			this.mnuCharSetCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.mnuCharSetCombo.Name = "mnuCharSetCombo";
			this.mnuCharSetCombo.Size = new System.Drawing.Size(150, 28);
			// 
			// mnuContributionModeCombo
			// 
			this.mnuContributionModeCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.mnuContributionModeCombo.Name = "mnuContributionModeCombo";
			this.mnuContributionModeCombo.Size = new System.Drawing.Size(150, 28);
			// 
			// mnuContentRegexTextbox
			// 
	        this.mnuContentRegexTextbox.Name = "mnuContentRegexTextbox";
	        this.mnuContentRegexTextbox.Size = new System.Drawing.Size(150, 28);
			// 
			// mnuIgnoreCaseCombo
			// 
			this.mnuIgnoreCaseCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
	        this.mnuIgnoreCaseCombo.Name = "mnuIgnoreCaseCombo";
	        this.mnuIgnoreCaseCombo.Size = new System.Drawing.Size(150, 28);
			// 
			// mnuProcessSnippets
			// 
			this.mnuProcessSnippets.Enabled = false;
            this.mnuProcessSnippets.Name = "mnuProcessSnippets";
            this.mnuProcessSnippets.Size = new System.Drawing.Size(223, 24);
            this.mnuProcessSnippets.Text = "Process Snippets";
            this.mnuProcessSnippets.Click += new System.EventHandler(this.mnuProcessSnippets_Click);
            // 
            // mnuProcessAgain
            // 
            this.mnuProcessAgain.Enabled = false;
            this.mnuProcessAgain.Name = "mnuProcessAgain";
            this.mnuProcessAgain.Size = new System.Drawing.Size(223, 24);
            this.mnuProcessAgain.Text = "Try out prev. results";
            this.mnuProcessAgain.ToolTipText = "Processing new sources using previous results as base metric";
            this.mnuProcessAgain.Click += new System.EventHandler(this.mnuProcessAgain_Click);
            // 
            // mnuClipboardNow
            // 
            this.mnuClipboardNow.Enabled = false;
            this.mnuClipboardNow.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.mnuClipboardNow.Name = "mnuClipboardNow";
            this.mnuClipboardNow.Size = new System.Drawing.Size(223, 24);
            this.mnuClipboardNow.Text = "Process Clipboard";
            this.mnuClipboardNow.Click += new System.EventHandler(this.mnuClipboardNow_Click);
            // 
            // mnuMonitorClipboard
            // 
            this.mnuMonitorClipboard.Enabled = false;
            this.mnuMonitorClipboard.Name = "mnuMonitorClipboard";
            this.mnuMonitorClipboard.Size = new System.Drawing.Size(223, 24);
            this.mnuMonitorClipboard.Text = "Monitor clipboard";
            this.mnuMonitorClipboard.Click += new System.EventHandler(this.mnuMonitorClipboard_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(220, 6);
            // 
            // mnuExit
            // 
            this.mnuExit.Name = "mnuExit";
            this.mnuExit.Size = new System.Drawing.Size(223, 24);
            this.mnuExit.Text = "Exit";
            this.mnuExit.Click += new System.EventHandler(this.mnuExit_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(839, 543);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "MainForm";
            this.Text = "OCR";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.notifyMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip notifyMenu;
        private System.Windows.Forms.ToolStripMenuItem mnuSnip;
        private System.Windows.Forms.ToolStripMenuItem mnuClipboardNow;
        private System.Windows.Forms.ToolStripMenuItem mnuMonitorClipboard;
        private System.Windows.Forms.ToolStripMenuItem mnuShowSnippets;
        private System.Windows.Forms.ToolStripMenuItem mnuProcessSnippets;
        private System.Windows.Forms.ToolStripComboBox mnuLanguageCombo;
		private System.Windows.Forms.ToolStripComboBox mnuCharSetCombo;
		private System.Windows.Forms.ToolStripComboBox mnuContributionModeCombo;
	    private System.Windows.Forms.ToolStripTextBox mnuContentRegexTextbox;
	    private System.Windows.Forms.ToolStripComboBox mnuIgnoreCaseCombo;
		private System.Windows.Forms.ToolStripSeparator exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mnuExit;
        private System.Windows.Forms.ToolStripMenuItem mnuProcessAgain;
    }
}

