using System.Drawing;
using System.Security.AccessControl;
using System.Windows.Forms;

namespace Tct.ActivityRecorderClient.View
{
	partial class ContextMenu
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
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.metroTabControl = new MetroFramework.Controls.MetroTabControl();
			this.tasksMetroTabPage = new MetroFramework.Controls.MetroTabPage();
			this.workGrid1 = new Tct.ActivityRecorderClient.View.Controls.WorkGrid();
			this.overviewMetroTabPage = new MetroFramework.Controls.MetroTabPage();
			this.favoriteReportsMetroTabPage = new MetroFramework.Controls.MetroTabPage();
			this.refreshFavoritesButton = new MetroFramework.Controls.MetroButton();
			this.favoritePanel = new System.Windows.Forms.Panel();
			this.favoriteReportsScrollBar = new Tct.ActivityRecorderClient.View.Controls.ScrollBar();
			this.favoriteReportsFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
			this.favoriteReportBox1 = new Tct.ActivityRecorderClient.View.Controls.FavoriteReportBox();
			this.userDisplay = new Tct.ActivityRecorderClient.View.Controls.UserDisplay();
			this.bigSplitter1 = new Tct.ActivityRecorderClient.View.Controls.BigSplitter();
			this.statGrid = new Tct.ActivityRecorderClient.View.Controls.StatGrid();
			this.currentWork1 = new Tct.ActivityRecorderClient.View.Controls.CurrentWork();
			this.pSearchBg = new System.Windows.Forms.Panel();
			this.searchBox1 = new Tct.ActivityRecorderClient.View.Controls.SearchBox();
			this.displayedReportsTimer = new System.Windows.Forms.Timer(this.components);
			this.metroToolTip = new MetroFramework.Components.MetroToolTip();
			this.tableLayoutPanel1.SuspendLayout();
			this.metroTabControl.SuspendLayout();
			this.tasksMetroTabPage.SuspendLayout();
			this.favoriteReportsMetroTabPage.SuspendLayout();
			this.favoritePanel.SuspendLayout();
			this.favoriteReportsFlowLayoutPanel.SuspendLayout();
			this.pSearchBg.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.AutoSize = true;
			this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.metroTabControl, 0, 5);
			this.tableLayoutPanel1.Controls.Add(this.userDisplay, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.bigSplitter1, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.statGrid, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.currentWork1, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.pSearchBg, 0, 4);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 7;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 4F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(340, 650);
			this.tableLayoutPanel1.TabIndex = 3;
			// 
			// metroTabControl
			// 
			this.metroTabControl.Controls.Add(this.tasksMetroTabPage);
			this.metroTabControl.Controls.Add(this.overviewMetroTabPage);
			this.metroTabControl.Controls.Add(this.favoriteReportsMetroTabPage);
			this.metroTabControl.Cursor = System.Windows.Forms.Cursors.Hand;
			this.metroTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.metroTabControl.HotTrack = true;
			this.metroTabControl.Location = new System.Drawing.Point(0, 232);
			this.metroTabControl.Margin = new System.Windows.Forms.Padding(0);
			this.metroTabControl.Name = "metroTabControl";
			this.metroTabControl.SelectedIndex = 0;
			this.metroTabControl.Size = new System.Drawing.Size(340, 418);
			this.metroTabControl.Style = MetroFramework.MetroColorStyle.Blue;
			this.metroTabControl.TabIndex = 9;
			this.metroTabControl.Theme = MetroFramework.MetroThemeStyle.Light;
			this.metroTabControl.UseSelectable = true;
			this.metroTabControl.SelectedIndexChanged += new System.EventHandler(this.metroTabControl_SelectedIndexChanged);
			// 
			// tasksMetroTabPage
			// 
			this.tasksMetroTabPage.Controls.Add(this.workGrid1);
			this.tasksMetroTabPage.Cursor = System.Windows.Forms.Cursors.Arrow;
			this.tasksMetroTabPage.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.tasksMetroTabPage.HorizontalScrollbarBarColor = true;
			this.tasksMetroTabPage.HorizontalScrollbarHighlightOnWheel = false;
			this.tasksMetroTabPage.HorizontalScrollbarSize = 10;
			this.tasksMetroTabPage.Location = new System.Drawing.Point(4, 38);
			this.tasksMetroTabPage.Name = "tasksMetroTabPage";
			this.tasksMetroTabPage.Size = new System.Drawing.Size(332, 376);
			this.tasksMetroTabPage.TabIndex = 0;
			this.tasksMetroTabPage.Text = "Feladatok";
			this.tasksMetroTabPage.VerticalScrollbarBarColor = true;
			this.tasksMetroTabPage.VerticalScrollbarHighlightOnWheel = false;
			this.tasksMetroTabPage.VerticalScrollbarSize = 10;
			// 
			// workGrid1
			// 
			this.workGrid1.AutoSize = true;
			this.workGrid1.BackColor = System.Drawing.Color.White;
			this.workGrid1.Location = new System.Drawing.Point(0, 0);
			this.workGrid1.Margin = new System.Windows.Forms.Padding(0);
			this.workGrid1.Name = "workGrid1";
			this.workGrid1.NavigationFactory = null;
			this.workGrid1.Size = new System.Drawing.Size(326, 373);
			this.workGrid1.TabIndex = 3;
			// 
			// overviewMetroTabPage
			// 
			this.overviewMetroTabPage.Cursor = System.Windows.Forms.Cursors.Arrow;
			this.overviewMetroTabPage.HorizontalScrollbarBarColor = true;
			this.overviewMetroTabPage.HorizontalScrollbarHighlightOnWheel = false;
			this.overviewMetroTabPage.HorizontalScrollbarSize = 10;
			this.overviewMetroTabPage.Location = new System.Drawing.Point(4, 38);
			this.overviewMetroTabPage.Name = "overviewMetroTabPage";
			this.overviewMetroTabPage.Size = new System.Drawing.Size(312, 376);
			this.overviewMetroTabPage.TabIndex = 1;
			this.overviewMetroTabPage.Text = "Teljesítményem";
			this.overviewMetroTabPage.VerticalScrollbarBarColor = true;
			this.overviewMetroTabPage.VerticalScrollbarHighlightOnWheel = false;
			this.overviewMetroTabPage.VerticalScrollbarSize = 10;
			// 
			// favoriteReportsMetroTabPage
			// 
			this.favoriteReportsMetroTabPage.Controls.Add(this.refreshFavoritesButton);
			this.favoriteReportsMetroTabPage.Controls.Add(this.favoritePanel);
			this.favoriteReportsMetroTabPage.Cursor = System.Windows.Forms.Cursors.Arrow;
			this.favoriteReportsMetroTabPage.HorizontalScrollbarBarColor = true;
			this.favoriteReportsMetroTabPage.HorizontalScrollbarHighlightOnWheel = false;
			this.favoriteReportsMetroTabPage.HorizontalScrollbarSize = 10;
			this.favoriteReportsMetroTabPage.Location = new System.Drawing.Point(4, 38);
			this.favoriteReportsMetroTabPage.Name = "favoriteReportsMetroTabPage";
			this.favoriteReportsMetroTabPage.Size = new System.Drawing.Size(312, 376);
			this.favoriteReportsMetroTabPage.TabIndex = 2;
			this.favoriteReportsMetroTabPage.Text = "Kedvenc reportok";
			this.favoriteReportsMetroTabPage.VerticalScrollbarBarColor = true;
			this.favoriteReportsMetroTabPage.VerticalScrollbarHighlightOnWheel = false;
			this.favoriteReportsMetroTabPage.VerticalScrollbarSize = 10;
			// 
			// refreshFavoritesButton
			// 
			this.refreshFavoritesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.refreshFavoritesButton.Location = new System.Drawing.Point(3, 3);
			this.refreshFavoritesButton.Name = "refreshFavoritesButton";
			this.refreshFavoritesButton.Size = new System.Drawing.Size(62, 23);
			this.refreshFavoritesButton.TabIndex = 5;
			this.refreshFavoritesButton.Text = "Frissít";
			this.refreshFavoritesButton.UseSelectable = true;
			this.refreshFavoritesButton.Click += new System.EventHandler(this.refreshFavoritesButton_Click);
			// 
			// favoritePanel
			// 
			this.favoritePanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.favoritePanel.BackColor = System.Drawing.Color.Transparent;
			this.favoritePanel.Controls.Add(this.favoriteReportsScrollBar);
			this.favoritePanel.Controls.Add(this.favoriteReportsFlowLayoutPanel);
			this.favoritePanel.Location = new System.Drawing.Point(0, 31);
			this.favoritePanel.Name = "favoritePanel";
			this.favoritePanel.Size = new System.Drawing.Size(312, 345);
			this.favoritePanel.TabIndex = 2;
			// 
			// favoriteReportsScrollBar
			// 
			this.favoriteReportsScrollBar.Location = new System.Drawing.Point(299, 0);
			this.favoriteReportsScrollBar.Name = "favoriteReportsScrollBar";
			this.favoriteReportsScrollBar.ScrollSpeed = 10F;
			this.favoriteReportsScrollBar.ScrollTotalSize = 100;
			this.favoriteReportsScrollBar.ScrollVisibleSize = 10;
			this.favoriteReportsScrollBar.Size = new System.Drawing.Size(10, 368);
			this.favoriteReportsScrollBar.TabIndex = 6;
			this.favoriteReportsScrollBar.Value = 0;
			this.favoriteReportsScrollBar.ScrollChanged += new System.EventHandler(this.favoriteReportsScrollBar_ScrollChanged);
			// 
			// favoriteReportsFlowLayoutPanel
			// 
			this.favoriteReportsFlowLayoutPanel.BackColor = System.Drawing.Color.Transparent;
			this.favoriteReportsFlowLayoutPanel.Controls.Add(this.favoriteReportBox1);
			this.favoriteReportsFlowLayoutPanel.Cursor = System.Windows.Forms.Cursors.Arrow;
			this.favoriteReportsFlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.favoriteReportsFlowLayoutPanel.Location = new System.Drawing.Point(3, 3);
			this.favoriteReportsFlowLayoutPanel.Name = "favoriteReportsFlowLayoutPanel";
			this.favoriteReportsFlowLayoutPanel.Size = new System.Drawing.Size(296, 162);
			this.favoriteReportsFlowLayoutPanel.TabIndex = 4;
			// 
			// favoriteReportBox1
			// 
			this.favoriteReportBox1.BackColor = System.Drawing.SystemColors.ControlLight;
			this.favoriteReportBox1.Cursor = System.Windows.Forms.Cursors.Hand;
			this.favoriteReportBox1.Image = null;
			this.favoriteReportBox1.Location = new System.Drawing.Point(3, 3);
			this.favoriteReportBox1.Name = "favoriteReportBox1";
			this.favoriteReportBox1.ReportName = null;
			this.favoriteReportBox1.Size = new System.Drawing.Size(287, 66);
			this.favoriteReportBox1.TabIndex = 0;
			this.favoriteReportBox1.Url = null;
			// 
			// userDisplay
			// 
			this.userDisplay.AccessibleRole = System.Windows.Forms.AccessibleRole.Pane;
			this.userDisplay.AutoSize = true;
			this.userDisplay.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.userDisplay.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
			this.userDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
			this.userDisplay.Location = new System.Drawing.Point(5, 3);
			this.userDisplay.MainForm = null;
			this.userDisplay.Margin = new System.Windows.Forms.Padding(5, 3, 3, 0);
			this.userDisplay.MinimumSize = new System.Drawing.Size(0, 22);
			this.userDisplay.Name = "userDisplay";
			this.userDisplay.QuitEnabled = false;
			this.userDisplay.Size = new System.Drawing.Size(332, 22);
			this.userDisplay.TabIndex = 2;
			this.userDisplay.UserId = "2564";
			this.userDisplay.UserName = "Antal Róbert ";
			this.userDisplay.PreferenceClick += new System.EventHandler(this.HandlePreferenceClicked);
			this.userDisplay.QuitClick += new System.EventHandler(this.HandleQuitClicked);
			this.userDisplay.UserChangeClick += new System.EventHandler(this.HandleUserChangedClick);
			this.userDisplay.HelpClick += new System.EventHandler(this.HandleHelpClick);
			this.userDisplay.RulesClick += new System.EventHandler(this.HandleRulesClicked);
			this.userDisplay.ErrorReportClick += new System.EventHandler(this.HandleErrorReportClicked);
			this.userDisplay.ProjectUploadClick += new System.EventHandler(this.HandleProjectUploadClicked);
			// 
			// bigSplitter1
			// 
			this.bigSplitter1.AutoSize = true;
			this.bigSplitter1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.bigSplitter1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.bigSplitter1.Location = new System.Drawing.Point(8, 25);
			this.bigSplitter1.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
			this.bigSplitter1.MinimumSize = new System.Drawing.Size(0, 3);
			this.bigSplitter1.Name = "bigSplitter1";
			this.bigSplitter1.Padding = new System.Windows.Forms.Padding(3, 1, 3, 1);
			this.bigSplitter1.Size = new System.Drawing.Size(324, 4);
			this.bigSplitter1.TabIndex = 3;
			// 
			// statGrid
			// 
			this.statGrid.AutoSize = true;
			this.statGrid.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
			this.statGrid.DeltaVisible = false;
			this.statGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.statGrid.ExtraRowVisible = false;
			this.statGrid.Location = new System.Drawing.Point(8, 30);
			this.statGrid.Margin = new System.Windows.Forms.Padding(8, 1, 8, 5);
			this.statGrid.Name = "statGrid";
			this.statGrid.Size = new System.Drawing.Size(324, 50);
			this.statGrid.SumVisible = false;
			this.statGrid.TabIndex = 4;
			// 
			// currentWork1
			// 
			this.currentWork1.Active = false;
			this.currentWork1.AutoSize = true;
			this.currentWork1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
			this.currentWork1.Cursor = System.Windows.Forms.Cursors.Hand;
			this.currentWork1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.currentWork1.Location = new System.Drawing.Point(0, 85);
			this.currentWork1.Margin = new System.Windows.Forms.Padding(0);
			this.currentWork1.MinimumSize = new System.Drawing.Size(320, 97);
			this.currentWork1.Name = "currentWork1";
			this.currentWork1.NavigationFactory = null;
			this.currentWork1.Size = new System.Drawing.Size(340, 97);
			this.currentWork1.TabIndex = 5;
			// 
			// pSearchBg
			// 
			this.pSearchBg.BackColor = System.Drawing.Color.Transparent;
			this.pSearchBg.BackgroundImage = global::Tct.ActivityRecorderClient.Properties.Resources.search_bg;
			this.pSearchBg.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.pSearchBg.Controls.Add(this.searchBox1);
			this.pSearchBg.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pSearchBg.Location = new System.Drawing.Point(0, 182);
			this.pSearchBg.Margin = new System.Windows.Forms.Padding(0);
			this.pSearchBg.MinimumSize = new System.Drawing.Size(300, 50);
			this.pSearchBg.Name = "pSearchBg";
			this.pSearchBg.Padding = new System.Windows.Forms.Padding(10);
			this.pSearchBg.Size = new System.Drawing.Size(340, 50);
			this.pSearchBg.TabIndex = 7;
			// 
			// searchBox1
			// 
			this.searchBox1.AutoSize = true;
			this.searchBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.searchBox1.Location = new System.Drawing.Point(10, 10);
			this.searchBox1.Margin = new System.Windows.Forms.Padding(10, 9, 10, 3);
			this.searchBox1.MaximumSize = new System.Drawing.Size(1000, 30);
			this.searchBox1.MinimumSize = new System.Drawing.Size(20, 30);
			this.searchBox1.Name = "searchBox1";
			this.searchBox1.Navigator = null;
			this.searchBox1.Padding = new System.Windows.Forms.Padding(0, 0, 6, 0);
			this.searchBox1.Size = new System.Drawing.Size(320, 30);
			this.searchBox1.TabIndex = 7;
			// 
			// metroToolTip
			// 
			this.metroToolTip.AutoPopDelay = 5000;
			this.metroToolTip.InitialDelay = 200;
			this.metroToolTip.ReshowDelay = 100;
			this.metroToolTip.Style = MetroFramework.MetroColorStyle.Blue;
			this.metroToolTip.StyleManager = null;
			this.metroToolTip.Theme = MetroFramework.MetroThemeStyle.Light;
			// 
			// ContextMenu
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(340, 650);
			this.ControlBox = false;
			this.Controls.Add(this.tableLayoutPanel1);
			this.DoubleBuffered = true;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "ContextMenu";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "JobCTRL";
			this.TopMost = true;
			this.Deactivate += new System.EventHandler(this.HandleDeactivated);
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.HandleClosed);
			this.Load += new System.EventHandler(this.HandleLoaded);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.metroTabControl.ResumeLayout(false);
			this.tasksMetroTabPage.ResumeLayout(false);
			this.tasksMetroTabPage.PerformLayout();
			this.favoriteReportsMetroTabPage.ResumeLayout(false);
			this.favoritePanel.ResumeLayout(false);
			this.favoriteReportsFlowLayoutPanel.ResumeLayout(false);
			this.pSearchBg.ResumeLayout(false);
			this.pSearchBg.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Controls.UserDisplay userDisplay;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private Controls.BigSplitter bigSplitter1;
		private Controls.StatGrid statGrid;
		private Controls.CurrentWork currentWork1;
		private MetroFramework.Controls.MetroTabControl metroTabControl;
		private MetroFramework.Controls.MetroTabPage tasksMetroTabPage;
		private Controls.WorkGrid workGrid1;
		private MetroFramework.Controls.MetroTabPage overviewMetroTabPage;
		private MetroFramework.Controls.MetroTabPage favoriteReportsMetroTabPage;
		private Panel favoritePanel;
		private FlowLayoutPanel favoriteReportsFlowLayoutPanel;
		private MetroFramework.Controls.MetroButton refreshFavoritesButton;
		private Controls.FavoriteReportBox favoriteReportBox1;
		private Controls.ScrollBar favoriteReportsScrollBar;
		private Panel pSearchBg;
		private Controls.SearchBox searchBox1;
		private Timer displayedReportsTimer;
		private MetroFramework.Components.MetroToolTip metroToolTip;
	}
}