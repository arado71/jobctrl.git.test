namespace Tct.ActivityRecorderClient.View.Controls
{
	partial class OfflineWorkCard
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OfflineWorkCard));
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.lblTo = new System.Windows.Forms.Label();
			this.pnlRound = new Tct.ActivityRecorderClient.View.Controls.SPanel();
			this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
			this.lblDuration = new System.Windows.Forms.Label();
			this.pnlSplit1 = new System.Windows.Forms.Panel();
			this.pbType = new System.Windows.Forms.PictureBox();
			this.pnlSplit2 = new System.Windows.Forms.Panel();
			this.lblFrom = new System.Windows.Forms.Label();
			this.lblInterval = new System.Windows.Forms.Label();
			this.pnlSplit3 = new System.Windows.Forms.Panel();
			this.pnlTask = new System.Windows.Forms.Panel();
			this.tlpDeletedInfo = new System.Windows.Forms.TableLayoutPanel();
			this.lblDeletedInfo = new System.Windows.Forms.Label();
			this.pnlWiHldr = new System.Windows.Forms.Panel();
			this.wiEmpty = new Tct.ActivityRecorderClient.View.Controls.WorkIcon();
			this.lblUndo = new System.Windows.Forms.Label();
			this.pbUndo = new System.Windows.Forms.PictureBox();
			this.workInfo = new Tct.ActivityRecorderClient.View.Navigation.WorkRowShort();
			this.lblChooseTask = new System.Windows.Forms.Label();
			this.pnlSplit4 = new System.Windows.Forms.Panel();
			this.pbDelete = new System.Windows.Forms.PictureBox();
			this.pnlEditor = new System.Windows.Forms.Panel();
			this.tlpEditor = new System.Windows.Forms.TableLayoutPanel();
			this.lblParticipants = new System.Windows.Forms.Label();
			this.txbComment = new System.Windows.Forms.TextBox();
			this.txbSubject = new System.Windows.Forms.TextBox();
			this.pnlParticipants = new System.Windows.Forms.Panel();
			this.txbParticipants = new Tct.ActivityRecorderClient.View.TextBoxWithTagSuggestion();
			this.pbParticipants = new System.Windows.Forms.PictureBox();
			this.lblTask = new System.Windows.Forms.Label();
			this.lblSubject = new System.Windows.Forms.Label();
			this.lblComment = new System.Windows.Forms.Label();
			this.pnlSearch = new System.Windows.Forms.Panel();
			this.cbTask = new Tct.ActivityRecorderClient.View.WorkSelectorComboBox();
			this.pbSearch = new System.Windows.Forms.PictureBox();
			this.pbArrow = new System.Windows.Forms.PictureBox();
			this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
			this.pnlRound.SuspendLayout();
			this.tlpMain.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbType)).BeginInit();
			this.pnlTask.SuspendLayout();
			this.tlpDeletedInfo.SuspendLayout();
			this.pnlWiHldr.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbUndo)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pbDelete)).BeginInit();
			this.pnlEditor.SuspendLayout();
			this.tlpEditor.SuspendLayout();
			this.pnlParticipants.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbParticipants)).BeginInit();
			this.pnlSearch.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbSearch)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pbArrow)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
			this.SuspendLayout();
			// 
			// lblTo
			// 
			this.lblTo.Dock = System.Windows.Forms.DockStyle.Fill;
			this.errorProvider.SetIconPadding(this.lblTo, -8);
			this.lblTo.Location = new System.Drawing.Point(158, 0);
			this.lblTo.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
			this.lblTo.Name = "lblTo";
			this.lblTo.Size = new System.Drawing.Size(40, 50);
			this.lblTo.TabIndex = 14;
			this.lblTo.Text = "13:59";
			this.lblTo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.lblTo.Click += new System.EventHandler(this.HandleClicked);
			// 
			// pnlRound
			// 
			this.pnlRound.Border = 2;
			this.pnlRound.BorderColor = System.Drawing.SystemColors.ControlText;
			this.pnlRound.Controls.Add(this.tlpMain);
			this.pnlRound.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlRound.Location = new System.Drawing.Point(0, 0);
			this.pnlRound.Name = "pnlRound";
			this.pnlRound.Padding = new System.Windows.Forms.Padding(7);
			this.pnlRound.Radius = 5;
			this.pnlRound.Size = new System.Drawing.Size(520, 168);
			this.pnlRound.TabIndex = 1;
			// 
			// tlpMain
			// 
			this.tlpMain.ColumnCount = 12;
			this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 10F));
			this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 2F));
			this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 2F));
			this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 2F));
			this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 2F));
			this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tlpMain.Controls.Add(this.lblDuration, 1, 0);
			this.tlpMain.Controls.Add(this.pnlSplit1, 2, 0);
			this.tlpMain.Controls.Add(this.pbType, 3, 0);
			this.tlpMain.Controls.Add(this.pnlSplit2, 4, 0);
			this.tlpMain.Controls.Add(this.lblFrom, 5, 0);
			this.tlpMain.Controls.Add(this.lblInterval, 6, 0);
			this.tlpMain.Controls.Add(this.lblTo, 7, 0);
			this.tlpMain.Controls.Add(this.pnlSplit3, 8, 0);
			this.tlpMain.Controls.Add(this.pnlTask, 9, 0);
			this.tlpMain.Controls.Add(this.pnlSplit4, 10, 0);
			this.tlpMain.Controls.Add(this.pbDelete, 11, 0);
			this.tlpMain.Controls.Add(this.pnlEditor, 0, 1);
			this.tlpMain.Controls.Add(this.pbArrow, 0, 0);
			this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tlpMain.Location = new System.Drawing.Point(7, 7);
			this.tlpMain.Name = "tlpMain";
			this.tlpMain.RowCount = 2;
			this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tlpMain.Size = new System.Drawing.Size(506, 154);
			this.tlpMain.TabIndex = 0;
			this.tlpMain.Click += new System.EventHandler(this.HandleClicked);
			// 
			// lblDuration
			// 
			this.lblDuration.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblDuration.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.lblDuration.Location = new System.Drawing.Point(13, 0);
			this.lblDuration.Name = "lblDuration";
			this.lblDuration.Size = new System.Drawing.Size(55, 50);
			this.lblDuration.TabIndex = 0;
			this.lblDuration.Text = "00:42";
			this.lblDuration.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.lblDuration.Click += new System.EventHandler(this.HandleClicked);
			// 
			// pnlSplit1
			// 
			this.pnlSplit1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
			this.pnlSplit1.Location = new System.Drawing.Point(71, 10);
			this.pnlSplit1.Margin = new System.Windows.Forms.Padding(0, 10, 0, 3);
			this.pnlSplit1.Name = "pnlSplit1";
			this.pnlSplit1.Size = new System.Drawing.Size(2, 30);
			this.pnlSplit1.TabIndex = 6;
			this.pnlSplit1.Click += new System.EventHandler(this.HandleClicked);
			// 
			// pbType
			// 
			this.pbType.Image = global::Tct.ActivityRecorderClient.Properties.Resources.idle;
			this.pbType.Location = new System.Drawing.Point(75, 2);
			this.pbType.Margin = new System.Windows.Forms.Padding(2);
			this.pbType.Name = "pbType";
			this.pbType.Size = new System.Drawing.Size(20, 46);
			this.pbType.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.pbType.TabIndex = 12;
			this.pbType.TabStop = false;
			this.pbType.Click += new System.EventHandler(this.HandleClicked);
			// 
			// pnlSplit2
			// 
			this.pnlSplit2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
			this.pnlSplit2.Location = new System.Drawing.Point(97, 10);
			this.pnlSplit2.Margin = new System.Windows.Forms.Padding(0, 10, 0, 3);
			this.pnlSplit2.Name = "pnlSplit2";
			this.pnlSplit2.Size = new System.Drawing.Size(2, 30);
			this.pnlSplit2.TabIndex = 7;
			this.pnlSplit2.Click += new System.EventHandler(this.HandleClicked);
			// 
			// lblFrom
			// 
			this.lblFrom.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblFrom.Location = new System.Drawing.Point(102, 0);
			this.lblFrom.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.lblFrom.Name = "lblFrom";
			this.lblFrom.Size = new System.Drawing.Size(40, 50);
			this.lblFrom.TabIndex = 13;
			this.lblFrom.Text = "12/24\r\n11:29";
			this.lblFrom.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.lblFrom.Click += new System.EventHandler(this.HandleClicked);
			// 
			// lblInterval
			// 
			this.lblInterval.AutoSize = true;
			this.lblInterval.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblInterval.Location = new System.Drawing.Point(142, 0);
			this.lblInterval.Margin = new System.Windows.Forms.Padding(0);
			this.lblInterval.Name = "lblInterval";
			this.lblInterval.Size = new System.Drawing.Size(16, 50);
			this.lblInterval.TabIndex = 2;
			this.lblInterval.Text = "►";
			this.lblInterval.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.lblInterval.Click += new System.EventHandler(this.HandleClicked);
			// 
			// pnlSplit3
			// 
			this.pnlSplit3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
			this.pnlSplit3.Location = new System.Drawing.Point(206, 10);
			this.pnlSplit3.Margin = new System.Windows.Forms.Padding(0, 10, 0, 3);
			this.pnlSplit3.Name = "pnlSplit3";
			this.pnlSplit3.Size = new System.Drawing.Size(2, 30);
			this.pnlSplit3.TabIndex = 8;
			this.pnlSplit3.Click += new System.EventHandler(this.HandleClicked);
			// 
			// pnlTask
			// 
			this.pnlTask.Controls.Add(this.tlpDeletedInfo);
			this.pnlTask.Controls.Add(this.workInfo);
			this.pnlTask.Controls.Add(this.lblChooseTask);
			this.pnlTask.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlTask.Location = new System.Drawing.Point(208, 0);
			this.pnlTask.Margin = new System.Windows.Forms.Padding(0);
			this.pnlTask.Name = "pnlTask";
			this.pnlTask.Size = new System.Drawing.Size(262, 50);
			this.pnlTask.TabIndex = 10;
			this.pnlTask.Click += new System.EventHandler(this.HandleClicked);
			// 
			// tlpDeletedInfo
			// 
			this.tlpDeletedInfo.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tlpDeletedInfo.ColumnCount = 4;
			this.tlpDeletedInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tlpDeletedInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tlpDeletedInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tlpDeletedInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tlpDeletedInfo.Controls.Add(this.lblDeletedInfo, 1, 0);
			this.tlpDeletedInfo.Controls.Add(this.pnlWiHldr, 0, 0);
			this.tlpDeletedInfo.Controls.Add(this.lblUndo, 2, 0);
			this.tlpDeletedInfo.Controls.Add(this.pbUndo, 3, 0);
			this.tlpDeletedInfo.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tlpDeletedInfo.Location = new System.Drawing.Point(0, 0);
			this.tlpDeletedInfo.Margin = new System.Windows.Forms.Padding(0);
			this.tlpDeletedInfo.Name = "tlpDeletedInfo";
			this.tlpDeletedInfo.RowCount = 1;
			this.tlpDeletedInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tlpDeletedInfo.Size = new System.Drawing.Size(262, 50);
			this.tlpDeletedInfo.TabIndex = 5;
			this.tlpDeletedInfo.Visible = false;
			// 
			// lblDeletedInfo
			// 
			this.lblDeletedInfo.AutoSize = true;
			this.lblDeletedInfo.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblDeletedInfo.ForeColor = System.Drawing.Color.Silver;
			this.lblDeletedInfo.Location = new System.Drawing.Point(57, 0);
			this.lblDeletedInfo.Name = "lblDeletedInfo";
			this.lblDeletedInfo.Size = new System.Drawing.Size(141, 50);
			this.lblDeletedInfo.TabIndex = 1;
			this.lblDeletedInfo.Text = "Do not account";
			this.lblDeletedInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// pnlWiHldr
			// 
			this.pnlWiHldr.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.pnlWiHldr.Controls.Add(this.wiEmpty);
			this.pnlWiHldr.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlWiHldr.Location = new System.Drawing.Point(3, 3);
			this.pnlWiHldr.MinimumSize = new System.Drawing.Size(43, 43);
			this.pnlWiHldr.Name = "pnlWiHldr";
			this.pnlWiHldr.Size = new System.Drawing.Size(48, 44);
			this.pnlWiHldr.TabIndex = 2;
			// 
			// wiEmpty
			// 
			this.wiEmpty.AlternativeStyle = false;
			this.wiEmpty.Color = System.Drawing.Color.Silver;
			this.wiEmpty.Initials = null;
			this.wiEmpty.Location = new System.Drawing.Point(6, 2);
			this.wiEmpty.MaximumSize = new System.Drawing.Size(36, 36);
			this.wiEmpty.Name = "wiEmpty";
			this.wiEmpty.Size = new System.Drawing.Size(36, 36);
			this.wiEmpty.TabIndex = 0;
			// 
			// lblUndo
			// 
			this.lblUndo.AutoSize = true;
			this.lblUndo.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblUndo.Location = new System.Drawing.Point(204, 0);
			this.lblUndo.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.lblUndo.Name = "lblUndo";
			this.lblUndo.Size = new System.Drawing.Size(31, 50);
			this.lblUndo.TabIndex = 3;
			this.lblUndo.Text = "undo";
			this.lblUndo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.lblUndo.Click += new System.EventHandler(this.pbUndo_Click);
			this.lblUndo.MouseEnter += new System.EventHandler(this.pbUndo_MouseEnter);
			this.lblUndo.MouseLeave += new System.EventHandler(this.pbUndo_MouseLeave);
			// 
			// pbUndo
			// 
			this.pbUndo.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pbUndo.Image = global::Tct.ActivityRecorderClient.Properties.Resources.undo;
			this.pbUndo.Location = new System.Drawing.Point(235, 2);
			this.pbUndo.Margin = new System.Windows.Forms.Padding(0, 2, 7, 2);
			this.pbUndo.Name = "pbUndo";
			this.pbUndo.Size = new System.Drawing.Size(20, 46);
			this.pbUndo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.pbUndo.TabIndex = 4;
			this.pbUndo.TabStop = false;
			this.pbUndo.Click += new System.EventHandler(this.pbUndo_Click);
			this.pbUndo.MouseEnter += new System.EventHandler(this.pbUndo_MouseEnter);
			this.pbUndo.MouseLeave += new System.EventHandler(this.pbUndo_MouseLeave);
			// 
			// workInfo
			// 
			this.workInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
			this.workInfo.Cursor = System.Windows.Forms.Cursors.Hand;
			this.workInfo.Dock = System.Windows.Forms.DockStyle.Fill;
			this.workInfo.Location = new System.Drawing.Point(0, 0);
			this.workInfo.Name = "workInfo";
			this.workInfo.Navigation = null;
			this.workInfo.Selected = false;
			this.workInfo.SelectedBackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(235)))), ((int)(((byte)(248)))));
			this.workInfo.Size = new System.Drawing.Size(262, 50);
			this.workInfo.TabIndex = 5;
			this.workInfo.UnselectedBackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
			this.workInfo.Value = null;
			this.workInfo.Visible = false;
			this.workInfo.Click += new System.EventHandler(this.HandleClicked);
			// 
			// lblChooseTask
			// 
			this.lblChooseTask.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblChooseTask.Location = new System.Drawing.Point(0, 0);
			this.lblChooseTask.Name = "lblChooseTask";
			this.lblChooseTask.Size = new System.Drawing.Size(262, 50);
			this.lblChooseTask.TabIndex = 4;
			this.lblChooseTask.Text = "Choose task...";
			this.lblChooseTask.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.lblChooseTask.Click += new System.EventHandler(this.HandleClicked);
			// 
			// pnlSplit4
			// 
			this.pnlSplit4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
			this.pnlSplit4.Location = new System.Drawing.Point(470, 10);
			this.pnlSplit4.Margin = new System.Windows.Forms.Padding(0, 10, 0, 3);
			this.pnlSplit4.Name = "pnlSplit4";
			this.pnlSplit4.Size = new System.Drawing.Size(2, 30);
			this.pnlSplit4.TabIndex = 9;
			this.pnlSplit4.Click += new System.EventHandler(this.HandleClicked);
			// 
			// pbDelete
			// 
			this.pbDelete.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pbDelete.Image = global::Tct.ActivityRecorderClient.Properties.Resources.delete;
			this.pbDelete.Location = new System.Drawing.Point(479, 2);
			this.pbDelete.Margin = new System.Windows.Forms.Padding(7, 2, 7, 2);
			this.pbDelete.Name = "pbDelete";
			this.pbDelete.Size = new System.Drawing.Size(20, 46);
			this.pbDelete.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.pbDelete.TabIndex = 11;
			this.pbDelete.TabStop = false;
			this.pbDelete.Click += new System.EventHandler(this.pbDelete_Click);
			this.pbDelete.MouseEnter += new System.EventHandler(this.pbDelete_MouseEnter);
			this.pbDelete.MouseLeave += new System.EventHandler(this.pbDelete_MouseLeave);
			// 
			// pnlEditor
			// 
			this.tlpMain.SetColumnSpan(this.pnlEditor, 12);
			this.pnlEditor.Controls.Add(this.tlpEditor);
			this.pnlEditor.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlEditor.Location = new System.Drawing.Point(0, 50);
			this.pnlEditor.Margin = new System.Windows.Forms.Padding(0);
			this.pnlEditor.Name = "pnlEditor";
			this.pnlEditor.Size = new System.Drawing.Size(506, 104);
			this.pnlEditor.TabIndex = 5;
			this.pnlEditor.Visible = false;
			// 
			// tlpEditor
			// 
			this.tlpEditor.ColumnCount = 2;
			this.tlpEditor.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tlpEditor.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tlpEditor.Controls.Add(this.lblParticipants, 0, 3);
			this.tlpEditor.Controls.Add(this.txbComment, 1, 2);
			this.tlpEditor.Controls.Add(this.txbSubject, 1, 1);
			this.tlpEditor.Controls.Add(this.pnlParticipants, 1, 3);
			this.tlpEditor.Controls.Add(this.lblTask, 0, 0);
			this.tlpEditor.Controls.Add(this.lblSubject, 0, 1);
			this.tlpEditor.Controls.Add(this.lblComment, 0, 2);
			this.tlpEditor.Controls.Add(this.pnlSearch, 1, 0);
			this.tlpEditor.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tlpEditor.Location = new System.Drawing.Point(0, 0);
			this.tlpEditor.Margin = new System.Windows.Forms.Padding(0);
			this.tlpEditor.Name = "tlpEditor";
			this.tlpEditor.Padding = new System.Windows.Forms.Padding(2);
			this.tlpEditor.RowCount = 4;
			this.tlpEditor.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tlpEditor.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tlpEditor.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tlpEditor.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tlpEditor.Size = new System.Drawing.Size(506, 104);
			this.tlpEditor.TabIndex = 0;
			// 
			// lblParticipants
			// 
			this.lblParticipants.AutoSize = true;
			this.lblParticipants.Location = new System.Drawing.Point(5, 74);
			this.lblParticipants.Margin = new System.Windows.Forms.Padding(3, 6, 16, 3);
			this.lblParticipants.Name = "lblParticipants";
			this.lblParticipants.Size = new System.Drawing.Size(65, 13);
			this.lblParticipants.TabIndex = 0;
			this.lblParticipants.Text = "Participants:";
			// 
			// txbComment
			// 
			this.txbComment.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.txbComment.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txbComment.Location = new System.Drawing.Point(89, 52);
			this.txbComment.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
			this.txbComment.Name = "txbComment";
			this.txbComment.Size = new System.Drawing.Size(412, 13);
			this.txbComment.TabIndex = 3;
			this.txbComment.TextChanged += new System.EventHandler(this.txbComment_TextChanged);
			this.txbComment.Leave += new System.EventHandler(this.TextBoxLeave);
			// 
			// txbSubject
			// 
			this.txbSubject.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
			this.txbSubject.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
			this.txbSubject.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.txbSubject.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txbSubject.Location = new System.Drawing.Point(89, 30);
			this.txbSubject.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
			this.txbSubject.Name = "txbSubject";
			this.txbSubject.Size = new System.Drawing.Size(412, 13);
			this.txbSubject.TabIndex = 2;
			this.txbSubject.TextChanged += new System.EventHandler(this.txbSubject_TextChanged);
			this.txbSubject.Leave += new System.EventHandler(this.TextBoxLeave);
			// 
			// pnlParticipants
			// 
			this.pnlParticipants.Controls.Add(this.txbParticipants);
			this.pnlParticipants.Controls.Add(this.pbParticipants);
			this.pnlParticipants.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlParticipants.Location = new System.Drawing.Point(89, 74);
			this.pnlParticipants.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
			this.pnlParticipants.Name = "pnlParticipants";
			this.pnlParticipants.Size = new System.Drawing.Size(412, 25);
			this.pnlParticipants.TabIndex = 4;
			// 
			// txbParticipants
			// 
			this.txbParticipants.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.txbParticipants.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txbParticipants.Location = new System.Drawing.Point(0, 0);
			this.txbParticipants.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
			this.txbParticipants.Name = "txbParticipants";
			this.txbParticipants.RecentTags = null;
			this.txbParticipants.Size = new System.Drawing.Size(384, 13);
			this.txbParticipants.TabIndex = 4;
			this.txbParticipants.TextChanged += new System.EventHandler(this.txbParticipants_TextChanged);
			this.txbParticipants.Leave += new System.EventHandler(this.TextBoxLeave);
			// 
			// pbParticipants
			// 
			this.pbParticipants.Dock = System.Windows.Forms.DockStyle.Right;
			this.pbParticipants.Image = global::Tct.ActivityRecorderClient.Properties.Resources.btn_adress_book;
			this.pbParticipants.Location = new System.Drawing.Point(384, 0);
			this.pbParticipants.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.pbParticipants.Name = "pbParticipants";
			this.pbParticipants.Size = new System.Drawing.Size(28, 25);
			this.pbParticipants.TabIndex = 1;
			this.pbParticipants.TabStop = false;
			this.pbParticipants.Click += new System.EventHandler(this.pbParticipants_Click);
			// 
			// lblTask
			// 
			this.lblTask.AutoSize = true;
			this.lblTask.Location = new System.Drawing.Point(5, 5);
			this.lblTask.Margin = new System.Windows.Forms.Padding(3, 3, 16, 3);
			this.lblTask.Name = "lblTask";
			this.lblTask.Size = new System.Drawing.Size(34, 13);
			this.lblTask.TabIndex = 5;
			this.lblTask.Text = "Task:";
			// 
			// lblSubject
			// 
			this.lblSubject.AutoSize = true;
			this.lblSubject.Location = new System.Drawing.Point(5, 30);
			this.lblSubject.Margin = new System.Windows.Forms.Padding(3, 6, 16, 0);
			this.lblSubject.Name = "lblSubject";
			this.lblSubject.Size = new System.Drawing.Size(46, 13);
			this.lblSubject.TabIndex = 6;
			this.lblSubject.Text = "Subject:";
			// 
			// lblComment
			// 
			this.lblComment.AutoSize = true;
			this.lblComment.Location = new System.Drawing.Point(5, 52);
			this.lblComment.Margin = new System.Windows.Forms.Padding(3, 6, 16, 3);
			this.lblComment.Name = "lblComment";
			this.lblComment.Size = new System.Drawing.Size(54, 13);
			this.lblComment.TabIndex = 7;
			this.lblComment.Text = "Comment:";
			// 
			// pnlSearch
			// 
			this.pnlSearch.Controls.Add(this.cbTask);
			this.pnlSearch.Controls.Add(this.pbSearch);
			this.pnlSearch.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlSearch.Location = new System.Drawing.Point(86, 2);
			this.pnlSearch.Margin = new System.Windows.Forms.Padding(0);
			this.pnlSearch.Name = "pnlSearch";
			this.pnlSearch.Size = new System.Drawing.Size(418, 22);
			this.pnlSearch.TabIndex = 8;
			// 
			// cbTask
			// 
			this.cbTask.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cbTask.DropDownWidth = 398;
			this.cbTask.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cbTask.FormattingEnabled = true;
			this.cbTask.IntegralHeight = false;
			this.cbTask.Location = new System.Drawing.Point(20, 0);
			this.cbTask.MaxDropDownItems = 30;
			this.cbTask.Name = "cbTask";
			this.cbTask.Size = new System.Drawing.Size(398, 21);
			this.cbTask.TabIndex = 2;
			this.cbTask.SelectionChangeCommitted += new System.EventHandler(this.cbTask_SelectionChangeCommitted);
			this.cbTask.Enter += new System.EventHandler(this.cbTask_Enter);
			this.cbTask.Leave += new System.EventHandler(this.cbTask_Leave);
			// 
			// pbSearch
			// 
			this.pbSearch.Cursor = System.Windows.Forms.Cursors.Hand;
			this.pbSearch.Dock = System.Windows.Forms.DockStyle.Left;
			this.pbSearch.Image = global::Tct.ActivityRecorderClient.Properties.Resources.search_blue;
			this.pbSearch.Location = new System.Drawing.Point(0, 0);
			this.pbSearch.Margin = new System.Windows.Forms.Padding(0);
			this.pbSearch.Name = "pbSearch";
			this.pbSearch.Padding = new System.Windows.Forms.Padding(0, 0, 3, 3);
			this.pbSearch.Size = new System.Drawing.Size(20, 22);
			this.pbSearch.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pbSearch.TabIndex = 3;
			this.pbSearch.TabStop = false;
			this.pbSearch.Click += new System.EventHandler(this.pbSearch_Click);
			// 
			// pbArrow
			// 
			this.pbArrow.Image = global::Tct.ActivityRecorderClient.Properties.Resources.arrow_blue;
			this.pbArrow.Location = new System.Drawing.Point(0, 0);
			this.pbArrow.Margin = new System.Windows.Forms.Padding(0);
			this.pbArrow.Name = "pbArrow";
			this.pbArrow.Size = new System.Drawing.Size(10, 50);
			this.pbArrow.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.pbArrow.TabIndex = 15;
			this.pbArrow.TabStop = false;
			this.pbArrow.Click += new System.EventHandler(this.HandleClicked);
			this.pbArrow.MouseEnter += new System.EventHandler(this.pbArrow_MouseEnter);
			this.pbArrow.MouseLeave += new System.EventHandler(this.pbArrow_MouseLeave);
			// 
			// errorProvider
			// 
			this.errorProvider.ContainerControl = this;
			this.errorProvider.Icon = ((System.Drawing.Icon)(resources.GetObject("errorProvider.Icon")));
			// 
			// OfflineWorkCard
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.pnlRound);
			this.DoubleBuffered = true;
			this.Name = "OfflineWorkCard";
			this.Size = new System.Drawing.Size(520, 168);
			this.pnlRound.ResumeLayout(false);
			this.tlpMain.ResumeLayout(false);
			this.tlpMain.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbType)).EndInit();
			this.pnlTask.ResumeLayout(false);
			this.tlpDeletedInfo.ResumeLayout(false);
			this.tlpDeletedInfo.PerformLayout();
			this.pnlWiHldr.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pbUndo)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pbDelete)).EndInit();
			this.pnlEditor.ResumeLayout(false);
			this.tlpEditor.ResumeLayout(false);
			this.tlpEditor.PerformLayout();
			this.pnlParticipants.ResumeLayout(false);
			this.pnlParticipants.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbParticipants)).EndInit();
			this.pnlSearch.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pbSearch)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pbArrow)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tlpMain;
		private System.Windows.Forms.Label lblInterval;
		private System.Windows.Forms.Label lblDuration;
		private SPanel pnlRound;
		private System.Windows.Forms.Panel pnlSplit4;
		private System.Windows.Forms.Panel pnlSplit3;
		private System.Windows.Forms.Panel pnlSplit2;
		private System.Windows.Forms.Panel pnlEditor;
		private System.Windows.Forms.Panel pnlSplit1;
		private System.Windows.Forms.TableLayoutPanel tlpEditor;
		private TextBoxWithTagSuggestion txbParticipants;
		private System.Windows.Forms.TextBox txbComment;
		private System.Windows.Forms.TextBox txbSubject;
		private System.Windows.Forms.Panel pnlParticipants;
		private System.Windows.Forms.PictureBox pbParticipants;
		private System.Windows.Forms.Label lblParticipants;
		private System.Windows.Forms.Label lblTask;
		private System.Windows.Forms.Label lblSubject;
		private System.Windows.Forms.Label lblComment;
		private System.Windows.Forms.Panel pnlTask;
		private System.Windows.Forms.Label lblChooseTask;
		private Navigation.WorkRowShort workInfo;
		private System.Windows.Forms.PictureBox pbDelete;
		private System.Windows.Forms.PictureBox pbType;
		private System.Windows.Forms.Label lblFrom;
		private System.Windows.Forms.Label lblTo;
		private System.Windows.Forms.ToolTip toolTip;
		private System.Windows.Forms.ErrorProvider errorProvider;
		private System.Windows.Forms.Label lblDeletedInfo;
		private WorkIcon wiEmpty;
		private System.Windows.Forms.PictureBox pbUndo;
		private System.Windows.Forms.Label lblUndo;
		private System.Windows.Forms.Panel pnlWiHldr;
		private System.Windows.Forms.TableLayoutPanel tlpDeletedInfo;
		private System.Windows.Forms.PictureBox pbArrow;
		private System.Windows.Forms.Panel pnlSearch;
		private System.Windows.Forms.PictureBox pbSearch;
		private WorkSelectorComboBox cbTask;
	}
}
