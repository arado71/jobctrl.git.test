namespace Reporter
{
	partial class Form1
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
			this.dtFrom = new System.Windows.Forms.DateTimePicker();
			this.dtTo = new System.Windows.Forms.DateTimePicker();
			this.numUserId = new System.Windows.Forms.NumericUpDown();
			this.label1 = new System.Windows.Forms.Label();
			this.btnAddUserId = new System.Windows.Forms.Button();
			this.lbUserId = new System.Windows.Forms.ListBox();
			this.btnRemoveUser = new System.Windows.Forms.Button();
			this.gbFetch = new System.Windows.Forms.GroupBox();
			this.button1 = new System.Windows.Forms.Button();
			this.btnCompanyAdd = new System.Windows.Forms.Button();
			this.btnFetch = new System.Windows.Forms.Button();
			this.gbTransform = new System.Windows.Forms.GroupBox();
			this.cbReports = new System.Windows.Forms.CheckBox();
			this.btnExport = new System.Windows.Forms.Button();
			this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
			this.pbLoading = new System.Windows.Forms.ProgressBar();
			((System.ComponentModel.ISupportInitialize)(this.numUserId)).BeginInit();
			this.gbFetch.SuspendLayout();
			this.gbTransform.SuspendLayout();
			this.SuspendLayout();
			// 
			// dtFrom
			// 
			this.dtFrom.Location = new System.Drawing.Point(6, 19);
			this.dtFrom.Name = "dtFrom";
			this.dtFrom.Size = new System.Drawing.Size(151, 20);
			this.dtFrom.TabIndex = 0;
			// 
			// dtTo
			// 
			this.dtTo.Location = new System.Drawing.Point(164, 19);
			this.dtTo.Name = "dtTo";
			this.dtTo.Size = new System.Drawing.Size(151, 20);
			this.dtTo.TabIndex = 1;
			// 
			// numUserId
			// 
			this.numUserId.Location = new System.Drawing.Point(25, 45);
			this.numUserId.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
			this.numUserId.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numUserId.Name = "numUserId";
			this.numUserId.Size = new System.Drawing.Size(77, 20);
			this.numUserId.TabIndex = 2;
			this.numUserId.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 47);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(16, 13);
			this.label1.TabIndex = 3;
			this.label1.Text = "Id";
			// 
			// btnAddUserId
			// 
			this.btnAddUserId.Location = new System.Drawing.Point(154, 45);
			this.btnAddUserId.Name = "btnAddUserId";
			this.btnAddUserId.Size = new System.Drawing.Size(70, 23);
			this.btnAddUserId.TabIndex = 4;
			this.btnAddUserId.Text = "Add User";
			this.btnAddUserId.UseVisualStyleBackColor = true;
			this.btnAddUserId.Click += new System.EventHandler(this.HandleUserAddClicked);
			// 
			// lbUserId
			// 
			this.lbUserId.FormattingEnabled = true;
			this.lbUserId.Location = new System.Drawing.Point(6, 73);
			this.lbUserId.Name = "lbUserId";
			this.lbUserId.Size = new System.Drawing.Size(309, 121);
			this.lbUserId.TabIndex = 5;
			// 
			// btnRemoveUser
			// 
			this.btnRemoveUser.Location = new System.Drawing.Point(6, 199);
			this.btnRemoveUser.Name = "btnRemoveUser";
			this.btnRemoveUser.Size = new System.Drawing.Size(75, 23);
			this.btnRemoveUser.TabIndex = 6;
			this.btnRemoveUser.Text = "Remove";
			this.btnRemoveUser.UseVisualStyleBackColor = true;
			this.btnRemoveUser.Click += new System.EventHandler(this.HandleRemoveUserClicked);
			// 
			// gbFetch
			// 
			this.gbFetch.Controls.Add(this.button1);
			this.gbFetch.Controls.Add(this.btnCompanyAdd);
			this.gbFetch.Controls.Add(this.btnFetch);
			this.gbFetch.Controls.Add(this.dtFrom);
			this.gbFetch.Controls.Add(this.btnRemoveUser);
			this.gbFetch.Controls.Add(this.dtTo);
			this.gbFetch.Controls.Add(this.lbUserId);
			this.gbFetch.Controls.Add(this.numUserId);
			this.gbFetch.Controls.Add(this.btnAddUserId);
			this.gbFetch.Controls.Add(this.label1);
			this.gbFetch.Location = new System.Drawing.Point(12, 12);
			this.gbFetch.Name = "gbFetch";
			this.gbFetch.Size = new System.Drawing.Size(321, 230);
			this.gbFetch.TabIndex = 7;
			this.gbFetch.TabStop = false;
			this.gbFetch.Text = "Fetch";
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(82, 199);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 9;
			this.button1.Text = "Remove All";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.HandleRemoveAllClicked);
			// 
			// btnCompanyAdd
			// 
			this.btnCompanyAdd.Location = new System.Drawing.Point(230, 45);
			this.btnCompanyAdd.Name = "btnCompanyAdd";
			this.btnCompanyAdd.Size = new System.Drawing.Size(85, 23);
			this.btnCompanyAdd.TabIndex = 8;
			this.btnCompanyAdd.Text = "Add Company";
			this.btnCompanyAdd.UseVisualStyleBackColor = true;
			this.btnCompanyAdd.Click += new System.EventHandler(this.HandleCompanyAddClicked);
			// 
			// btnFetch
			// 
			this.btnFetch.Location = new System.Drawing.Point(240, 199);
			this.btnFetch.Name = "btnFetch";
			this.btnFetch.Size = new System.Drawing.Size(75, 23);
			this.btnFetch.TabIndex = 7;
			this.btnFetch.Text = "Fetch";
			this.btnFetch.UseVisualStyleBackColor = true;
			this.btnFetch.Click += new System.EventHandler(this.HandleFetchClicked);
			// 
			// gbTransform
			// 
			this.gbTransform.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.gbTransform.Controls.Add(this.cbReports);
			this.gbTransform.Controls.Add(this.btnExport);
			this.gbTransform.Enabled = false;
			this.gbTransform.Location = new System.Drawing.Point(12, 248);
			this.gbTransform.Name = "gbTransform";
			this.gbTransform.Size = new System.Drawing.Size(321, 50);
			this.gbTransform.TabIndex = 8;
			this.gbTransform.TabStop = false;
			this.gbTransform.Text = "Transform";
			// 
			// cbReports
			// 
			this.cbReports.AutoSize = true;
			this.cbReports.Location = new System.Drawing.Point(6, 19);
			this.cbReports.Name = "cbReports";
			this.cbReports.Size = new System.Drawing.Size(133, 17);
			this.cbReports.TabIndex = 9;
			this.cbReports.Text = "Include custom reports";
			this.cbReports.UseVisualStyleBackColor = true;
			// 
			// btnExport
			// 
			this.btnExport.Enabled = false;
			this.btnExport.Location = new System.Drawing.Point(240, 15);
			this.btnExport.Name = "btnExport";
			this.btnExport.Size = new System.Drawing.Size(75, 23);
			this.btnExport.TabIndex = 9;
			this.btnExport.Text = "Export";
			this.btnExport.UseVisualStyleBackColor = true;
			this.btnExport.Click += new System.EventHandler(this.HandleExportClicked);
			// 
			// saveFileDialog1
			// 
			this.saveFileDialog1.DefaultExt = "xlsx";
			this.saveFileDialog1.Filter = "Excel document|*.xlsx";
			// 
			// pbLoading
			// 
			this.pbLoading.Location = new System.Drawing.Point(12, 304);
			this.pbLoading.Name = "pbLoading";
			this.pbLoading.Size = new System.Drawing.Size(321, 23);
			this.pbLoading.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
			this.pbLoading.TabIndex = 10;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(345, 334);
			this.Controls.Add(this.pbLoading);
			this.Controls.Add(this.gbTransform);
			this.Controls.Add(this.gbFetch);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "Form1";
			this.Text = "JC Administrative Reporting Tool";
			((System.ComponentModel.ISupportInitialize)(this.numUserId)).EndInit();
			this.gbFetch.ResumeLayout(false);
			this.gbFetch.PerformLayout();
			this.gbTransform.ResumeLayout(false);
			this.gbTransform.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.DateTimePicker dtFrom;
		private System.Windows.Forms.DateTimePicker dtTo;
		private System.Windows.Forms.NumericUpDown numUserId;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button btnAddUserId;
		private System.Windows.Forms.ListBox lbUserId;
		private System.Windows.Forms.Button btnRemoveUser;
		private System.Windows.Forms.GroupBox gbFetch;
		private System.Windows.Forms.Button btnFetch;
		private System.Windows.Forms.GroupBox gbTransform;
		private System.Windows.Forms.Button btnExport;
		private System.Windows.Forms.SaveFileDialog saveFileDialog1;
		private System.Windows.Forms.Button btnCompanyAdd;
		private System.Windows.Forms.CheckBox cbReports;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.ProgressBar pbLoading;
	}
}

