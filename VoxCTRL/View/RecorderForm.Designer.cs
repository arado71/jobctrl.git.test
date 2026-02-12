namespace VoxCTRL.View
{
	partial class RecorderForm
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
			this.cbDevice = new System.Windows.Forms.ComboBox();
			this.btnRecord = new System.Windows.Forms.Button();
			this.btnStop = new System.Windows.Forms.Button();
			this.lblTime = new System.Windows.Forms.Label();
			this.lblBytesWritten = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.txtId = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.tbVolume = new System.Windows.Forms.TrackBar();
			this.label3 = new System.Windows.Forms.Label();
			this.gridRecordings = new System.Windows.Forms.DataGridView();
			this.userIdDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.workIdDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.nameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.createDateDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.lengthDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Status = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.voiceBindingSource = new System.Windows.Forms.BindingSource(this.components);
			this.lblName = new System.Windows.Forms.Label();
			this.lblCreateDate = new System.Windows.Forms.Label();
			this.lblLength = new System.Windows.Forms.Label();
			this.lblStatus = new System.Windows.Forms.Label();
			this.btnPause = new System.Windows.Forms.Button();
			this.cbQuality = new System.Windows.Forms.ComboBox();
			this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.timerRetry = new System.Windows.Forms.Timer(this.components);
			this.btnDelete = new System.Windows.Forms.Button();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.miViewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.miStatusWindow = new VoxCTRL.View.BindableToolStripMenuItem();
			this.miMandatoryName = new VoxCTRL.View.BindableToolStripMenuItem();
			this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.changeUserToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.picTooSilent = new System.Windows.Forms.PictureBox();
			this.waveformPainter3 = new VoxCTRL.View.WaveformPainter();
			this.waveformPainter2 = new VoxCTRL.View.WaveformPainter();
			this.volumeMeter3 = new VoxCTRL.View.VolumeMeter();
			this.volumeMeter2 = new VoxCTRL.View.VolumeMeter();
			this.waveformPainter1 = new VoxCTRL.View.WaveformPainter();
			this.volumeMeter1 = new VoxCTRL.View.VolumeMeter();
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.btnAgc = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.tbVolume)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.gridRecordings)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.voiceBindingSource)).BeginInit();
			this.menuStrip1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.picTooSilent)).BeginInit();
			this.SuspendLayout();
			// 
			// cbDevice
			// 
			this.cbDevice.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbDevice.FormattingEnabled = true;
			this.cbDevice.Location = new System.Drawing.Point(440, 35);
			this.cbDevice.Name = "cbDevice";
			this.cbDevice.Size = new System.Drawing.Size(106, 21);
			this.cbDevice.TabIndex = 0;
			this.cbDevice.SelectedIndexChanged += new System.EventHandler(this.cbDevice_SelectedIndexChanged);
			// 
			// btnRecord
			// 
			this.btnRecord.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.btnRecord.BackgroundImage = global::VoxCTRL.Properties.Resources.Record;
			this.btnRecord.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.btnRecord.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.btnRecord.ForeColor = System.Drawing.Color.White;
			this.btnRecord.Location = new System.Drawing.Point(11, 67);
			this.btnRecord.Name = "btnRecord";
			this.btnRecord.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
			this.btnRecord.Size = new System.Drawing.Size(203, 58);
			this.btnRecord.TabIndex = 1;
			this.btnRecord.Text = "Felvétel indítása";
			this.btnRecord.UseVisualStyleBackColor = false;
			this.btnRecord.Click += new System.EventHandler(this.btnRecord_Click);
			// 
			// btnStop
			// 
			this.btnStop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(147)))), ((int)(((byte)(147)))));
			this.btnStop.BackgroundImage = global::VoxCTRL.Properties.Resources.Stop;
			this.btnStop.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.btnStop.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.btnStop.ForeColor = System.Drawing.Color.White;
			this.btnStop.Location = new System.Drawing.Point(447, 67);
			this.btnStop.Name = "btnStop";
			this.btnStop.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
			this.btnStop.Size = new System.Drawing.Size(203, 58);
			this.btnStop.TabIndex = 6;
			this.btnStop.Text = "Felvétel mentése";
			this.btnStop.UseVisualStyleBackColor = false;
			this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
			// 
			// lblTime
			// 
			this.lblTime.AutoSize = true;
			this.lblTime.BackColor = System.Drawing.Color.Transparent;
			this.lblTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.lblTime.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(153)))), ((int)(((byte)(51)))));
			this.lblTime.Location = new System.Drawing.Point(227, 71);
			this.lblTime.Name = "lblTime";
			this.lblTime.Size = new System.Drawing.Size(212, 55);
			this.lblTime.TabIndex = 5;
			this.lblTime.Text = "00:00:00";
			// 
			// lblBytesWritten
			// 
			this.lblBytesWritten.Location = new System.Drawing.Point(491, 128);
			this.lblBytesWritten.Name = "lblBytesWritten";
			this.lblBytesWritten.Size = new System.Drawing.Size(100, 12);
			this.lblBytesWritten.TabIndex = 6;
			this.lblBytesWritten.Text = "0";
			this.lblBytesWritten.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.lblBytesWritten.Visible = false;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(597, 128);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(40, 13);
			this.label1.TabIndex = 7;
			this.label1.Text = "KBytes";
			this.label1.Visible = false;
			// 
			// txtId
			// 
			this.txtId.Location = new System.Drawing.Point(90, 35);
			this.txtId.Name = "txtId";
			this.txtId.Size = new System.Drawing.Size(261, 20);
			this.txtId.TabIndex = 9;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.BackColor = System.Drawing.Color.Transparent;
			this.label2.Location = new System.Drawing.Point(19, 38);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(58, 13);
			this.label2.TabIndex = 10;
			this.label2.Text = "Azonosító:";
			// 
			// tbVolume
			// 
			this.tbVolume.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(102)))), ((int)(((byte)(0)))));
			this.tbVolume.LargeChange = 20;
			this.tbVolume.Location = new System.Drawing.Point(55, 162);
			this.tbVolume.Maximum = 100;
			this.tbVolume.Name = "tbVolume";
			this.tbVolume.Orientation = System.Windows.Forms.Orientation.Vertical;
			this.tbVolume.Size = new System.Drawing.Size(45, 149);
			this.tbVolume.SmallChange = 5;
			this.tbVolume.TabIndex = 11;
			this.tbVolume.TickFrequency = 5;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.BackColor = System.Drawing.Color.Transparent;
			this.label3.Location = new System.Drawing.Point(76, 142);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(51, 13);
			this.label3.TabIndex = 12;
			this.label3.Text = "Hangerő:";
			// 
			// gridRecordings
			// 
			this.gridRecordings.AllowUserToAddRows = false;
			this.gridRecordings.AllowUserToDeleteRows = false;
			this.gridRecordings.AutoGenerateColumns = false;
			this.gridRecordings.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.gridRecordings.ColumnHeadersVisible = false;
			this.gridRecordings.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.userIdDataGridViewTextBoxColumn,
            this.workIdDataGridViewTextBoxColumn,
            this.nameDataGridViewTextBoxColumn,
            this.createDateDataGridViewTextBoxColumn,
            this.lengthDataGridViewTextBoxColumn,
            this.Status});
			this.gridRecordings.DataSource = this.voiceBindingSource;
			this.gridRecordings.Location = new System.Drawing.Point(11, 362);
			this.gridRecordings.Name = "gridRecordings";
			this.gridRecordings.ReadOnly = true;
			this.gridRecordings.Size = new System.Drawing.Size(638, 115);
			this.gridRecordings.TabIndex = 13;
			this.gridRecordings.KeyUp += new System.Windows.Forms.KeyEventHandler(this.gridRecordings_KeyUp);
			// 
			// userIdDataGridViewTextBoxColumn
			// 
			this.userIdDataGridViewTextBoxColumn.DataPropertyName = "UserId";
			this.userIdDataGridViewTextBoxColumn.HeaderText = "UserId";
			this.userIdDataGridViewTextBoxColumn.Name = "userIdDataGridViewTextBoxColumn";
			this.userIdDataGridViewTextBoxColumn.ReadOnly = true;
			this.userIdDataGridViewTextBoxColumn.Visible = false;
			// 
			// workIdDataGridViewTextBoxColumn
			// 
			this.workIdDataGridViewTextBoxColumn.DataPropertyName = "WorkId";
			this.workIdDataGridViewTextBoxColumn.HeaderText = "WorkId";
			this.workIdDataGridViewTextBoxColumn.Name = "workIdDataGridViewTextBoxColumn";
			this.workIdDataGridViewTextBoxColumn.ReadOnly = true;
			this.workIdDataGridViewTextBoxColumn.Visible = false;
			// 
			// nameDataGridViewTextBoxColumn
			// 
			this.nameDataGridViewTextBoxColumn.DataPropertyName = "Name";
			this.nameDataGridViewTextBoxColumn.HeaderText = "Name";
			this.nameDataGridViewTextBoxColumn.Name = "nameDataGridViewTextBoxColumn";
			this.nameDataGridViewTextBoxColumn.ReadOnly = true;
			this.nameDataGridViewTextBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.nameDataGridViewTextBoxColumn.Width = 240;
			// 
			// createDateDataGridViewTextBoxColumn
			// 
			this.createDateDataGridViewTextBoxColumn.DataPropertyName = "StartDate";
			this.createDateDataGridViewTextBoxColumn.HeaderText = "StartDate";
			this.createDateDataGridViewTextBoxColumn.Name = "createDateDataGridViewTextBoxColumn";
			this.createDateDataGridViewTextBoxColumn.ReadOnly = true;
			this.createDateDataGridViewTextBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.createDateDataGridViewTextBoxColumn.Width = 130;
			// 
			// lengthDataGridViewTextBoxColumn
			// 
			this.lengthDataGridViewTextBoxColumn.DataPropertyName = "Duration";
			this.lengthDataGridViewTextBoxColumn.HeaderText = "Duration";
			this.lengthDataGridViewTextBoxColumn.Name = "lengthDataGridViewTextBoxColumn";
			this.lengthDataGridViewTextBoxColumn.ReadOnly = true;
			this.lengthDataGridViewTextBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.lengthDataGridViewTextBoxColumn.Width = 80;
			// 
			// Status
			// 
			this.Status.DataPropertyName = "Status";
			this.Status.HeaderText = "Status";
			this.Status.Name = "Status";
			this.Status.ReadOnly = true;
			this.Status.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.Status.Width = 126;
			// 
			// voiceBindingSource
			// 
			this.voiceBindingSource.DataSource = typeof(VoxCTRL.ActivityRecorderServiceReference.VoiceRecording);
			// 
			// lblName
			// 
			this.lblName.AutoSize = true;
			this.lblName.BackColor = System.Drawing.Color.Transparent;
			this.lblName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.lblName.Location = new System.Drawing.Point(63, 345);
			this.lblName.Name = "lblName";
			this.lblName.Size = new System.Drawing.Size(68, 13);
			this.lblName.TabIndex = 14;
			this.lblName.Text = "Azonosító:";
			// 
			// lblCreateDate
			// 
			this.lblCreateDate.AutoSize = true;
			this.lblCreateDate.BackColor = System.Drawing.Color.Transparent;
			this.lblCreateDate.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.lblCreateDate.Location = new System.Drawing.Point(297, 345);
			this.lblCreateDate.Name = "lblCreateDate";
			this.lblCreateDate.Size = new System.Drawing.Size(74, 13);
			this.lblCreateDate.TabIndex = 15;
			this.lblCreateDate.Text = "Létrehozva:";
			// 
			// lblLength
			// 
			this.lblLength.AutoSize = true;
			this.lblLength.BackColor = System.Drawing.Color.Transparent;
			this.lblLength.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.lblLength.Location = new System.Drawing.Point(424, 345);
			this.lblLength.Name = "lblLength";
			this.lblLength.Size = new System.Drawing.Size(64, 13);
			this.lblLength.TabIndex = 16;
			this.lblLength.Text = "Időtartam:";
			// 
			// lblStatus
			// 
			this.lblStatus.AutoSize = true;
			this.lblStatus.BackColor = System.Drawing.Color.Transparent;
			this.lblStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.lblStatus.Location = new System.Drawing.Point(511, 345);
			this.lblStatus.Name = "lblStatus";
			this.lblStatus.Size = new System.Drawing.Size(50, 13);
			this.lblStatus.TabIndex = 17;
			this.lblStatus.Text = "Állapot:";
			// 
			// btnPause
			// 
			this.btnPause.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(50)))), ((int)(((byte)(0)))));
			this.btnPause.BackgroundImage = global::VoxCTRL.Properties.Resources.Pause;
			this.btnPause.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.btnPause.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.btnPause.ForeColor = System.Drawing.Color.White;
			this.btnPause.Location = new System.Drawing.Point(11, 67);
			this.btnPause.Name = "btnPause";
			this.btnPause.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
			this.btnPause.Size = new System.Drawing.Size(203, 58);
			this.btnPause.TabIndex = 18;
			this.btnPause.Text = "Felvétel megállítása";
			this.btnPause.UseVisualStyleBackColor = false;
			this.btnPause.Click += new System.EventHandler(this.btnPause_Click);
			// 
			// cbQuality
			// 
			this.cbQuality.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbQuality.FormattingEnabled = true;
			this.cbQuality.Location = new System.Drawing.Point(355, 34);
			this.cbQuality.Name = "cbQuality";
			this.cbQuality.Size = new System.Drawing.Size(79, 21);
			this.cbQuality.TabIndex = 19;
			this.cbQuality.SelectedIndexChanged += new System.EventHandler(this.cbQuality_SelectedIndexChanged);
			// 
			// dataGridViewTextBoxColumn1
			// 
			this.dataGridViewTextBoxColumn1.DataPropertyName = "UserId";
			this.dataGridViewTextBoxColumn1.HeaderText = "UserId";
			this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
			this.dataGridViewTextBoxColumn1.ReadOnly = true;
			this.dataGridViewTextBoxColumn1.Visible = false;
			// 
			// dataGridViewTextBoxColumn2
			// 
			this.dataGridViewTextBoxColumn2.DataPropertyName = "WorkId";
			this.dataGridViewTextBoxColumn2.HeaderText = "WorkId";
			this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
			this.dataGridViewTextBoxColumn2.ReadOnly = true;
			this.dataGridViewTextBoxColumn2.Visible = false;
			// 
			// dataGridViewTextBoxColumn3
			// 
			this.dataGridViewTextBoxColumn3.DataPropertyName = "Name";
			this.dataGridViewTextBoxColumn3.HeaderText = "Name";
			this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
			this.dataGridViewTextBoxColumn3.ReadOnly = true;
			this.dataGridViewTextBoxColumn3.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.dataGridViewTextBoxColumn3.Width = 240;
			// 
			// dataGridViewTextBoxColumn4
			// 
			this.dataGridViewTextBoxColumn4.DataPropertyName = "StartDate";
			this.dataGridViewTextBoxColumn4.HeaderText = "StartDate";
			this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
			this.dataGridViewTextBoxColumn4.ReadOnly = true;
			this.dataGridViewTextBoxColumn4.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.dataGridViewTextBoxColumn4.Width = 130;
			// 
			// dataGridViewTextBoxColumn5
			// 
			this.dataGridViewTextBoxColumn5.DataPropertyName = "Duration";
			this.dataGridViewTextBoxColumn5.HeaderText = "Duration";
			this.dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
			this.dataGridViewTextBoxColumn5.ReadOnly = true;
			this.dataGridViewTextBoxColumn5.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.dataGridViewTextBoxColumn5.Width = 80;
			// 
			// dataGridViewTextBoxColumn6
			// 
			this.dataGridViewTextBoxColumn6.DataPropertyName = "Status";
			this.dataGridViewTextBoxColumn6.HeaderText = "Status";
			this.dataGridViewTextBoxColumn6.Name = "dataGridViewTextBoxColumn6";
			this.dataGridViewTextBoxColumn6.ReadOnly = true;
			this.dataGridViewTextBoxColumn6.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.dataGridViewTextBoxColumn6.Width = 120;
			// 
			// timerRetry
			// 
			this.timerRetry.Enabled = true;
			this.timerRetry.Interval = 1000;
			this.timerRetry.Tick += new System.EventHandler(this.timerRetry_Tick);
			// 
			// btnDelete
			// 
			this.btnDelete.BackColor = System.Drawing.Color.Transparent;
			this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.btnDelete.Location = new System.Drawing.Point(552, 34);
			this.btnDelete.Name = "btnDelete";
			this.btnDelete.Size = new System.Drawing.Size(98, 23);
			this.btnDelete.TabIndex = 24;
			this.btnDelete.Text = "Felvétel eldobása";
			this.btnDelete.UseVisualStyleBackColor = false;
			this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
			// 
			// menuStrip1
			// 
			this.menuStrip1.BackColor = System.Drawing.Color.Transparent;
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miViewToolStripMenuItem,
            this.helpToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(662, 24);
			this.menuStrip1.TabIndex = 25;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// miViewToolStripMenuItem
			// 
			this.miViewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miStatusWindow,
            this.miMandatoryName});
			this.miViewToolStripMenuItem.Name = "miViewToolStripMenuItem";
			this.miViewToolStripMenuItem.Size = new System.Drawing.Size(75, 20);
			this.miViewToolStripMenuItem.Text = "Beállítások";
			// 
			// miStatusWindow
			// 
			this.miStatusWindow.Checked = global::VoxCTRL.Properties.Settings.Default.IsSmallFromEnabled;
			this.miStatusWindow.CheckState = System.Windows.Forms.CheckState.Checked;
			this.miStatusWindow.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::VoxCTRL.Properties.Settings.Default, "IsSmallFromEnabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
			this.miStatusWindow.Name = "miStatusWindow";
			this.miStatusWindow.Size = new System.Drawing.Size(173, 22);
			this.miStatusWindow.Text = "Státusz ablak";
			this.miStatusWindow.CheckedChanged += new System.EventHandler(this.miStatusWindow_CheckedChanged);
			this.miStatusWindow.Click += new System.EventHandler(this.miStatusWindow_Click);
			// 
			// miMandatoryName
			// 
			this.miMandatoryName.Checked = global::VoxCTRL.Properties.Settings.Default.IsNameMandatory;
			this.miMandatoryName.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::VoxCTRL.Properties.Settings.Default, "IsNameMandatory", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
			this.miMandatoryName.Name = "miMandatoryName";
			this.miMandatoryName.Size = new System.Drawing.Size(173, 22);
			this.miMandatoryName.Text = "Kötelező azonosító";
			this.miMandatoryName.Click += new System.EventHandler(this.miMandatoryName_Click);
			// 
			// helpToolStripMenuItem
			// 
			this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openLogToolStripMenuItem,
            this.changeUserToolStripMenuItem,
            this.toolStripSeparator1,
            this.aboutToolStripMenuItem});
			this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
			this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
			this.helpToolStripMenuItem.Text = "Help";
			// 
			// openLogToolStripMenuItem
			// 
			this.openLogToolStripMenuItem.Name = "openLogToolStripMenuItem";
			this.openLogToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
			this.openLogToolStripMenuItem.Text = "Naplófájl megnyitása...";
			this.openLogToolStripMenuItem.Click += new System.EventHandler(this.openLogToolStripMenuItem_Click);
			// 
			// changeUserToolStripMenuItem
			// 
			this.changeUserToolStripMenuItem.Name = "changeUserToolStripMenuItem";
			this.changeUserToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
			this.changeUserToolStripMenuItem.Text = "Felhasználó váltás...";
			this.changeUserToolStripMenuItem.Click += new System.EventHandler(this.changeUserToolStripMenuItem_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(192, 6);
			// 
			// aboutToolStripMenuItem
			// 
			this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
			this.aboutToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
			this.aboutToolStripMenuItem.Text = "Névjegy...";
			this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
			// 
			// picTooSilent
			// 
			this.picTooSilent.BackColor = System.Drawing.Color.Transparent;
			this.picTooSilent.Image = global::VoxCTRL.Properties.Resources.TooSilent;
			this.picTooSilent.Location = new System.Drawing.Point(354, 166);
			this.picTooSilent.Name = "picTooSilent";
			this.picTooSilent.Size = new System.Drawing.Size(128, 138);
			this.picTooSilent.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.picTooSilent.TabIndex = 26;
			this.picTooSilent.TabStop = false;
			this.toolTip.SetToolTip(this.picTooSilent, "A felvétel jelenleg túl halk. Ha beszélgetés alatt látja ezt a jelzést, akkor ell" +
        "enőrizze a hangbeállításokat, valamint, hogy mikrofonja megfelelően csatlakozik-" +
        "e!");
			this.picTooSilent.Visible = false;
			// 
			// waveformPainter3
			// 
			this.waveformPainter3.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
			this.waveformPainter3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(102)))), ((int)(((byte)(0)))));
			this.waveformPainter3.Location = new System.Drawing.Point(189, 239);
			this.waveformPainter3.Name = "waveformPainter3";
			this.waveformPainter3.Size = new System.Drawing.Size(458, 67);
			this.waveformPainter3.TabIndex = 23;
			this.waveformPainter3.Text = "waveformPainter3";
			this.waveformPainter3.Visible = false;
			// 
			// waveformPainter2
			// 
			this.waveformPainter2.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
			this.waveformPainter2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(102)))), ((int)(((byte)(0)))));
			this.waveformPainter2.Location = new System.Drawing.Point(189, 165);
			this.waveformPainter2.Name = "waveformPainter2";
			this.waveformPainter2.Size = new System.Drawing.Size(458, 67);
			this.waveformPainter2.TabIndex = 22;
			this.waveformPainter2.Text = "waveformPainter2";
			this.waveformPainter2.Visible = false;
			// 
			// volumeMeter3
			// 
			this.volumeMeter3.Amplitude = 0F;
			this.volumeMeter3.Location = new System.Drawing.Point(131, 158);
			this.volumeMeter3.MaxDb = 18F;
			this.volumeMeter3.MinDb = -60F;
			this.volumeMeter3.Name = "volumeMeter3";
			this.volumeMeter3.Size = new System.Drawing.Size(20, 157);
			this.volumeMeter3.TabIndex = 21;
			this.volumeMeter3.Text = "volumeMeter3";
			this.volumeMeter3.Visible = false;
			// 
			// volumeMeter2
			// 
			this.volumeMeter2.Amplitude = 0F;
			this.volumeMeter2.Location = new System.Drawing.Point(107, 158);
			this.volumeMeter2.MaxDb = 18F;
			this.volumeMeter2.MinDb = -60F;
			this.volumeMeter2.Name = "volumeMeter2";
			this.volumeMeter2.Size = new System.Drawing.Size(20, 157);
			this.volumeMeter2.TabIndex = 20;
			this.volumeMeter2.Text = "volumeMeter2";
			this.volumeMeter2.Visible = false;
			// 
			// waveformPainter1
			// 
			this.waveformPainter1.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
			this.waveformPainter1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(102)))), ((int)(((byte)(0)))));
			this.waveformPainter1.Location = new System.Drawing.Point(189, 163);
			this.waveformPainter1.Name = "waveformPainter1";
			this.waveformPainter1.Size = new System.Drawing.Size(458, 145);
			this.waveformPainter1.TabIndex = 3;
			this.waveformPainter1.Text = "waveformPainter1";
			// 
			// volumeMeter1
			// 
			this.volumeMeter1.Amplitude = 0F;
			this.volumeMeter1.Location = new System.Drawing.Point(107, 158);
			this.volumeMeter1.MaxDb = 18F;
			this.volumeMeter1.MinDb = -60F;
			this.volumeMeter1.Name = "volumeMeter1";
			this.volumeMeter1.Size = new System.Drawing.Size(44, 157);
			this.volumeMeter1.TabIndex = 8;
			this.volumeMeter1.Text = "volumeMeter1";
			// 
			// btnAgc
			// 
			this.btnAgc.AutoSize = true;
			this.btnAgc.Image = global::VoxCTRL.Properties.Resources.AgcOff;
			this.btnAgc.Location = new System.Drawing.Point(15, 211);
			this.btnAgc.Margin = new System.Windows.Forms.Padding(0);
			this.btnAgc.Name = "btnAgc";
			this.btnAgc.Size = new System.Drawing.Size(37, 49);
			this.btnAgc.TabIndex = 28;
			this.btnAgc.UseVisualStyleBackColor = true;
			this.btnAgc.Click += new System.EventHandler(this.btnAgc_Click);
			// 
			// RecorderForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.BackgroundImage = global::VoxCTRL.Properties.Resources.BackgroundImg;
			this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
			this.ClientSize = new System.Drawing.Size(662, 518);
			this.Controls.Add(this.btnAgc);
			this.Controls.Add(this.picTooSilent);
			this.Controls.Add(this.btnDelete);
			this.Controls.Add(this.waveformPainter3);
			this.Controls.Add(this.waveformPainter2);
			this.Controls.Add(this.volumeMeter3);
			this.Controls.Add(this.volumeMeter2);
			this.Controls.Add(this.cbQuality);
			this.Controls.Add(this.lblStatus);
			this.Controls.Add(this.lblLength);
			this.Controls.Add(this.lblCreateDate);
			this.Controls.Add(this.lblName);
			this.Controls.Add(this.waveformPainter1);
			this.Controls.Add(this.gridRecordings);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.tbVolume);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.txtId);
			this.Controls.Add(this.volumeMeter1);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.lblBytesWritten);
			this.Controls.Add(this.lblTime);
			this.Controls.Add(this.btnStop);
			this.Controls.Add(this.btnRecord);
			this.Controls.Add(this.cbDevice);
			this.Controls.Add(this.btnPause);
			this.Controls.Add(this.menuStrip1);
			this.DoubleBuffered = true;
			this.MainMenuStrip = this.menuStrip1;
			this.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size(678, 557);
			this.MinimumSize = new System.Drawing.Size(678, 557);
			this.Name = "RecorderForm";
			this.Text = "VoxCTRL";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RecorderForm_FormClosing);
			this.Load += new System.EventHandler(this.RecorderForm_Load);
			((System.ComponentModel.ISupportInitialize)(this.tbVolume)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.gridRecordings)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.voiceBindingSource)).EndInit();
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.picTooSilent)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ComboBox cbDevice;
		private System.Windows.Forms.Button btnRecord;
		private System.Windows.Forms.Button btnStop;
		private View.WaveformPainter waveformPainter1;
		private System.Windows.Forms.Label lblTime;
		private System.Windows.Forms.Label lblBytesWritten;
		private System.Windows.Forms.Label label1;
		private VolumeMeter volumeMeter1;
		private System.Windows.Forms.TextBox txtId;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TrackBar tbVolume;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.DataGridView gridRecordings;
		private System.Windows.Forms.BindingSource voiceBindingSource;
		private System.Windows.Forms.DataGridViewTextBoxColumn userIdDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn workIdDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn nameDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn createDateDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn lengthDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn Status;
		private System.Windows.Forms.Label lblName;
		private System.Windows.Forms.Label lblCreateDate;
		private System.Windows.Forms.Label lblLength;
		private System.Windows.Forms.Label lblStatus;
		private System.Windows.Forms.Button btnPause;
		private System.Windows.Forms.ComboBox cbQuality;
		private VolumeMeter volumeMeter2;
		private VolumeMeter volumeMeter3;
		private WaveformPainter waveformPainter2;
		private WaveformPainter waveformPainter3;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn6;
		private System.Windows.Forms.Timer timerRetry;
		private System.Windows.Forms.Button btnDelete;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openLogToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem changeUserToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem miViewToolStripMenuItem;
		private VoxCTRL.View.BindableToolStripMenuItem miStatusWindow;
		private VoxCTRL.View.BindableToolStripMenuItem miMandatoryName;
		private System.Windows.Forms.PictureBox picTooSilent;
		private System.Windows.Forms.ToolTip toolTip;
		private System.Windows.Forms.Button btnAgc;
	}
}

