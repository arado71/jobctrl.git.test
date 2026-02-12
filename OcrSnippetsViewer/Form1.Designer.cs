namespace OcrSnippetsViewer
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
			this.lblPath = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.panel3 = new System.Windows.Forms.Panel();
			this.panel2 = new System.Windows.Forms.Panel();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.label4 = new System.Windows.Forms.Label();
			this.lblUserid = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.tbContent = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.pib = new System.Windows.Forms.PictureBox();
			this.label6 = new System.Windows.Forms.Label();
			this.lblCreated = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.lblProcessName = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.lblProcessedAt = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.cbBaddata = new System.Windows.Forms.CheckBox();
			this.label9 = new System.Windows.Forms.Label();
			this.lblRuleId = new System.Windows.Forms.Label();
			this.btnEdit = new System.Windows.Forms.Button();
			this.btnSave = new System.Windows.Forms.Button();
			this.label10 = new System.Windows.Forms.Label();
			this.lblCompanyId = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.lblQuality = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.lbx = new System.Windows.Forms.ListBox();
			this.chkKeep = new System.Windows.Forms.CheckBox();
			this.btnDel = new System.Windows.Forms.Button();
			this.rbSrc1 = new System.Windows.Forms.RadioButton();
			this.rbSrc2 = new System.Windows.Forms.RadioButton();
			this.rbSrc3 = new System.Windows.Forms.RadioButton();
			this.rbSrc4 = new System.Windows.Forms.RadioButton();
			this.btnBrowse = new System.Windows.Forms.Button();
			this.btnRefresh = new System.Windows.Forms.Button();
			this.tbFilterRule = new System.Windows.Forms.TextBox();
			this.tbFilterCompany = new System.Windows.Forms.TextBox();
			this.btnFilter = new System.Windows.Forms.Button();
			this.lblFilterRule = new System.Windows.Forms.Label();
			this.lblFilterCompany = new System.Windows.Forms.Label();
			this.btnClearFilter = new System.Windows.Forms.Button();
			this.panel3.SuspendLayout();
			this.panel2.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pib)).BeginInit();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// lblPath
			// 
			this.lblPath.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblPath.AutoEllipsis = true;
			this.lblPath.ForeColor = System.Drawing.SystemColors.Highlight;
			this.lblPath.Location = new System.Drawing.Point(11, 24);
			this.lblPath.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.lblPath.Name = "lblPath";
			this.lblPath.Size = new System.Drawing.Size(545, 21);
			this.lblPath.TabIndex = 0;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.label2.Location = new System.Drawing.Point(72, 9);
			this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(90, 13);
			this.label2.TabIndex = 1;
			this.label2.Text = "Observed path";
			// 
			// panel3
			// 
			this.panel3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panel3.Controls.Add(this.panel2);
			this.panel3.Controls.Add(this.panel1);
			this.panel3.Location = new System.Drawing.Point(12, 69);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(544, 375);
			this.panel3.TabIndex = 7;
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.tableLayoutPanel1);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel2.Location = new System.Drawing.Point(200, 0);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(344, 375);
			this.panel2.TabIndex = 7;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this.label4, 0, 7);
			this.tableLayoutPanel1.Controls.Add(this.lblUserid, 1, 7);
			this.tableLayoutPanel1.Controls.Add(this.label3, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.tbContent, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.label5, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.pib, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.label6, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.lblCreated, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.label1, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.lblProcessName, 1, 3);
			this.tableLayoutPanel1.Controls.Add(this.label7, 0, 4);
			this.tableLayoutPanel1.Controls.Add(this.lblProcessedAt, 1, 4);
			this.tableLayoutPanel1.Controls.Add(this.label8, 0, 5);
			this.tableLayoutPanel1.Controls.Add(this.cbBaddata, 1, 5);
			this.tableLayoutPanel1.Controls.Add(this.label9, 0, 8);
			this.tableLayoutPanel1.Controls.Add(this.lblRuleId, 1, 8);
			this.tableLayoutPanel1.Controls.Add(this.btnEdit, 0, 10);
			this.tableLayoutPanel1.Controls.Add(this.btnSave, 1, 10);
			this.tableLayoutPanel1.Controls.Add(this.label10, 0, 9);
			this.tableLayoutPanel1.Controls.Add(this.lblCompanyId, 1, 9);
			this.tableLayoutPanel1.Controls.Add(this.label11, 0, 6);
			this.tableLayoutPanel1.Controls.Add(this.lblQuality, 1, 6);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(2);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 11;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(344, 375);
			this.tableLayoutPanel1.TabIndex = 5;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(2, 163);
			this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(36, 13);
			this.label4.TabIndex = 16;
			this.label4.Text = "userId";
			// 
			// lblUserid
			// 
			this.lblUserid.AutoSize = true;
			this.lblUserid.Location = new System.Drawing.Point(102, 163);
			this.lblUserid.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.lblUserid.Name = "lblUserid";
			this.lblUserid.Size = new System.Drawing.Size(0, 13);
			this.lblUserid.TabIndex = 15;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(2, 0);
			this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(43, 13);
			this.label3.TabIndex = 0;
			this.label3.Text = "content";
			// 
			// tbContent
			// 
			this.tbContent.BackColor = System.Drawing.SystemColors.Control;
			this.tbContent.Enabled = false;
			this.tbContent.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.tbContent.ForeColor = System.Drawing.SystemColors.Highlight;
			this.tbContent.Location = new System.Drawing.Point(102, 0);
			this.tbContent.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.tbContent.Name = "tbContent";
			this.tbContent.Size = new System.Drawing.Size(242, 23);
			this.tbContent.TabIndex = 1;
			this.tbContent.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tbContent_KeyPress);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(2, 20);
			this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(35, 13);
			this.label5.TabIndex = 4;
			this.label5.Text = "image";
			// 
			// pib
			// 
			this.pib.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pib.Location = new System.Drawing.Point(102, 22);
			this.pib.Margin = new System.Windows.Forms.Padding(2);
			this.pib.Name = "pib";
			this.pib.Size = new System.Drawing.Size(263, 39);
			this.pib.TabIndex = 5;
			this.pib.TabStop = false;
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(2, 63);
			this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(53, 13);
			this.label6.TabIndex = 6;
			this.label6.Text = "createdAt";
			// 
			// lblCreated
			// 
			this.lblCreated.AutoSize = true;
			this.lblCreated.Location = new System.Drawing.Point(102, 63);
			this.lblCreated.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.lblCreated.Name = "lblCreated";
			this.lblCreated.Size = new System.Drawing.Size(0, 13);
			this.lblCreated.TabIndex = 7;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(2, 83);
			this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(73, 13);
			this.label1.TabIndex = 8;
			this.label1.Text = "process name";
			// 
			// lblProcessName
			// 
			this.lblProcessName.AutoSize = true;
			this.lblProcessName.Location = new System.Drawing.Point(102, 83);
			this.lblProcessName.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.lblProcessName.Name = "lblProcessName";
			this.lblProcessName.Size = new System.Drawing.Size(0, 13);
			this.lblProcessName.TabIndex = 9;
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(2, 103);
			this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(66, 13);
			this.label7.TabIndex = 10;
			this.label7.Text = "processedAt";
			// 
			// lblProcessedAt
			// 
			this.lblProcessedAt.AutoSize = true;
			this.lblProcessedAt.Location = new System.Drawing.Point(102, 103);
			this.lblProcessedAt.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.lblProcessedAt.Name = "lblProcessedAt";
			this.lblProcessedAt.Size = new System.Drawing.Size(0, 13);
			this.lblProcessedAt.TabIndex = 11;
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(2, 123);
			this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(49, 13);
			this.label8.TabIndex = 13;
			this.label8.Text = "bad data";
			// 
			// cbBaddata
			// 
			this.cbBaddata.AutoSize = true;
			this.cbBaddata.Enabled = false;
			this.cbBaddata.Location = new System.Drawing.Point(102, 125);
			this.cbBaddata.Margin = new System.Windows.Forms.Padding(2);
			this.cbBaddata.Name = "cbBaddata";
			this.cbBaddata.Size = new System.Drawing.Size(15, 14);
			this.cbBaddata.TabIndex = 14;
			this.cbBaddata.UseVisualStyleBackColor = true;
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(3, 183);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(33, 13);
			this.label9.TabIndex = 17;
			this.label9.Text = "ruleId";
			// 
			// lblRuleId
			// 
			this.lblRuleId.AutoSize = true;
			this.lblRuleId.Location = new System.Drawing.Point(103, 183);
			this.lblRuleId.Name = "lblRuleId";
			this.lblRuleId.Size = new System.Drawing.Size(0, 13);
			this.lblRuleId.TabIndex = 18;
			// 
			// btnEdit
			// 
			this.btnEdit.Enabled = false;
			this.btnEdit.Location = new System.Drawing.Point(3, 226);
			this.btnEdit.Name = "btnEdit";
			this.btnEdit.Size = new System.Drawing.Size(75, 23);
			this.btnEdit.TabIndex = 19;
			this.btnEdit.Text = "Edit";
			this.btnEdit.UseVisualStyleBackColor = true;
			this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
			// 
			// btnSave
			// 
			this.btnSave.Enabled = false;
			this.btnSave.Location = new System.Drawing.Point(103, 226);
			this.btnSave.Name = "btnSave";
			this.btnSave.Size = new System.Drawing.Size(75, 23);
			this.btnSave.TabIndex = 20;
			this.btnSave.Text = "Save";
			this.btnSave.UseVisualStyleBackColor = true;
			this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(3, 203);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(59, 13);
			this.label10.TabIndex = 21;
			this.label10.Text = "companyId";
			// 
			// lblCompanyId
			// 
			this.lblCompanyId.AutoSize = true;
			this.lblCompanyId.Location = new System.Drawing.Point(103, 203);
			this.lblCompanyId.Name = "lblCompanyId";
			this.lblCompanyId.Size = new System.Drawing.Size(0, 13);
			this.lblCompanyId.TabIndex = 22;
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(3, 143);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(37, 13);
			this.label11.TabIndex = 23;
			this.label11.Text = "quality";
			// 
			// lblQuality
			// 
			this.lblQuality.AutoSize = true;
			this.lblQuality.Location = new System.Drawing.Point(103, 143);
			this.lblQuality.Name = "lblQuality";
			this.lblQuality.Size = new System.Drawing.Size(0, 13);
			this.lblQuality.TabIndex = 24;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.lbx);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(200, 375);
			this.panel1.TabIndex = 6;
			// 
			// lbx
			// 
			this.lbx.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lbx.FormattingEnabled = true;
			this.lbx.Location = new System.Drawing.Point(0, 0);
			this.lbx.Margin = new System.Windows.Forms.Padding(2);
			this.lbx.Name = "lbx";
			this.lbx.Size = new System.Drawing.Size(200, 375);
			this.lbx.TabIndex = 3;
			this.lbx.SelectedIndexChanged += new System.EventHandler(this.lbx_SelectedIndexChanged);
			// 
			// chkKeep
			// 
			this.chkKeep.AutoSize = true;
			this.chkKeep.Location = new System.Drawing.Point(450, 6);
			this.chkKeep.Margin = new System.Windows.Forms.Padding(2);
			this.chkKeep.Name = "chkKeep";
			this.chkKeep.Size = new System.Drawing.Size(112, 17);
			this.chkKeep.TabIndex = 8;
			this.chkKeep.Text = "keep last selected";
			this.chkKeep.UseVisualStyleBackColor = true;
			// 
			// btnDel
			// 
			this.btnDel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnDel.Location = new System.Drawing.Point(74, 450);
			this.btnDel.Margin = new System.Windows.Forms.Padding(2);
			this.btnDel.Name = "btnDel";
			this.btnDel.Size = new System.Drawing.Size(67, 23);
			this.btnDel.TabIndex = 9;
			this.btnDel.Text = "delete all";
			this.btnDel.UseVisualStyleBackColor = true;
			this.btnDel.Click += new System.EventHandler(this.button1_Click);
			// 
			// rbSrc1
			// 
			this.rbSrc1.AutoSize = true;
			this.rbSrc1.Checked = true;
			this.rbSrc1.Location = new System.Drawing.Point(182, 6);
			this.rbSrc1.Name = "rbSrc1";
			this.rbSrc1.Size = new System.Drawing.Size(48, 17);
			this.rbSrc1.TabIndex = 10;
			this.rbSrc1.TabStop = true;
			this.rbSrc1.Tag = "1";
			this.rbSrc1.Text = "base";
			this.rbSrc1.UseVisualStyleBackColor = true;
			this.rbSrc1.Click += new System.EventHandler(this.rbSrc_Click);
			// 
			// rbSrc2
			// 
			this.rbSrc2.AutoSize = true;
			this.rbSrc2.Location = new System.Drawing.Point(231, 6);
			this.rbSrc2.Name = "rbSrc2";
			this.rbSrc2.Size = new System.Drawing.Size(74, 17);
			this.rbSrc2.TabIndex = 11;
			this.rbSrc2.Tag = "2";
			this.rbSrc2.Text = "base\\data";
			this.rbSrc2.UseVisualStyleBackColor = true;
			this.rbSrc2.Click += new System.EventHandler(this.rbSrc_Click);
			// 
			// rbSrc3
			// 
			this.rbSrc3.AutoSize = true;
			this.rbSrc3.Location = new System.Drawing.Point(303, 6);
			this.rbSrc3.Name = "rbSrc3";
			this.rbSrc3.Size = new System.Drawing.Size(64, 17);
			this.rbSrc3.TabIndex = 12;
			this.rbSrc3.Tag = "3";
			this.rbSrc3.Text = "DEV Db";
			this.rbSrc3.UseVisualStyleBackColor = true;
			this.rbSrc3.Click += new System.EventHandler(this.rbSrc_Click);
			// 
			// rbSrc4
			// 
			this.rbSrc4.AutoSize = true;
			this.rbSrc4.Location = new System.Drawing.Point(372, 6);
			this.rbSrc4.Name = "rbSrc4";
			this.rbSrc4.Size = new System.Drawing.Size(65, 17);
			this.rbSrc4.TabIndex = 13;
			this.rbSrc4.Tag = "4";
			this.rbSrc4.Text = "LIVE Db";
			this.rbSrc4.UseVisualStyleBackColor = true;
			this.rbSrc4.Click += new System.EventHandler(this.rbSrc_Click);
			// 
			// btnBrowse
			// 
			this.btnBrowse.Location = new System.Drawing.Point(2, 2);
			this.btnBrowse.Margin = new System.Windows.Forms.Padding(2);
			this.btnBrowse.Name = "btnBrowse";
			this.btnBrowse.Size = new System.Drawing.Size(66, 23);
			this.btnBrowse.TabIndex = 13;
			this.btnBrowse.Text = "browse";
			this.btnBrowse.UseVisualStyleBackColor = true;
			this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
			// 
			// btnRefresh
			// 
			this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnRefresh.Location = new System.Drawing.Point(12, 450);
			this.btnRefresh.Margin = new System.Windows.Forms.Padding(2);
			this.btnRefresh.Name = "btnRefresh";
			this.btnRefresh.Size = new System.Drawing.Size(56, 23);
			this.btnRefresh.TabIndex = 14;
			this.btnRefresh.Text = "refresh";
			this.btnRefresh.UseVisualStyleBackColor = true;
			this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
			// 
			// tbFilterRule
			// 
			this.tbFilterRule.Enabled = false;
			this.tbFilterRule.Location = new System.Drawing.Point(61, 46);
			this.tbFilterRule.Name = "tbFilterRule";
			this.tbFilterRule.Size = new System.Drawing.Size(100, 20);
			this.tbFilterRule.TabIndex = 15;
			this.tbFilterRule.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tbFilterRule_KeyPress);
			// 
			// tbFilterCompany
			// 
			this.tbFilterCompany.Enabled = false;
			this.tbFilterCompany.Location = new System.Drawing.Point(267, 46);
			this.tbFilterCompany.Name = "tbFilterCompany";
			this.tbFilterCompany.Size = new System.Drawing.Size(100, 20);
			this.tbFilterCompany.TabIndex = 16;
			this.tbFilterCompany.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tbFilterCompany_KeyPress);
			// 
			// btnFilter
			// 
			this.btnFilter.Enabled = false;
			this.btnFilter.Location = new System.Drawing.Point(398, 44);
			this.btnFilter.Name = "btnFilter";
			this.btnFilter.Size = new System.Drawing.Size(75, 23);
			this.btnFilter.TabIndex = 17;
			this.btnFilter.Text = "Apply Filter";
			this.btnFilter.UseVisualStyleBackColor = true;
			this.btnFilter.Click += new System.EventHandler(this.btnFilter_Click);
			// 
			// lblFilterRule
			// 
			this.lblFilterRule.AutoSize = true;
			this.lblFilterRule.Location = new System.Drawing.Point(14, 49);
			this.lblFilterRule.Name = "lblFilterRule";
			this.lblFilterRule.Size = new System.Drawing.Size(46, 13);
			this.lblFilterRule.TabIndex = 18;
			this.lblFilterRule.Text = "Rule ID:";
			// 
			// lblFilterCompany
			// 
			this.lblFilterCompany.AutoSize = true;
			this.lblFilterCompany.Location = new System.Drawing.Point(199, 49);
			this.lblFilterCompany.Name = "lblFilterCompany";
			this.lblFilterCompany.Size = new System.Drawing.Size(68, 13);
			this.lblFilterCompany.TabIndex = 19;
			this.lblFilterCompany.Text = "Company ID:";
			// 
			// btnClearFilter
			// 
			this.btnClearFilter.Location = new System.Drawing.Point(481, 43);
			this.btnClearFilter.Name = "btnClearFilter";
			this.btnClearFilter.Size = new System.Drawing.Size(75, 23);
			this.btnClearFilter.TabIndex = 20;
			this.btnClearFilter.Text = "Clear Filter";
			this.btnClearFilter.UseVisualStyleBackColor = true;
			this.btnClearFilter.Click += new System.EventHandler(this.btnClearFilter_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(567, 480);
			this.Controls.Add(this.btnClearFilter);
			this.Controls.Add(this.lblFilterCompany);
			this.Controls.Add(this.lblFilterRule);
			this.Controls.Add(this.btnFilter);
			this.Controls.Add(this.tbFilterCompany);
			this.Controls.Add(this.tbFilterRule);
			this.Controls.Add(this.btnRefresh);
			this.Controls.Add(this.btnBrowse);
			this.Controls.Add(this.rbSrc4);
			this.Controls.Add(this.rbSrc3);
			this.Controls.Add(this.rbSrc2);
			this.Controls.Add(this.rbSrc1);
			this.Controls.Add(this.btnDel);
			this.Controls.Add(this.chkKeep);
			this.Controls.Add(this.panel3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.lblPath);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(2);
			this.Name = "Form1";
			this.Text = "OCR Snippets Viewer";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.panel3.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pib)).EndInit();
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label lblPath;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox tbContent;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.PictureBox pib;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label lblCreated;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.ListBox lbx;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label lblProcessName;
		private System.Windows.Forms.CheckBox chkKeep;
		private System.Windows.Forms.Button btnDel;
		private System.Windows.Forms.RadioButton rbSrc1;
		private System.Windows.Forms.RadioButton rbSrc2;
		private System.Windows.Forms.RadioButton rbSrc3;
		private System.Windows.Forms.RadioButton rbSrc4;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label lblProcessedAt;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.CheckBox cbBaddata;
		private System.Windows.Forms.Button btnBrowse;
		private System.Windows.Forms.Label lblUserid;
		private System.Windows.Forms.Button btnRefresh;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label lblRuleId;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Label lblQuality;
		private System.Windows.Forms.Button btnEdit;
		private System.Windows.Forms.Button btnSave;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label lblCompanyId;
		private System.Windows.Forms.TextBox tbFilterRule;
		private System.Windows.Forms.TextBox tbFilterCompany;
		private System.Windows.Forms.Button btnFilter;
		private System.Windows.Forms.Label lblFilterRule;
		private System.Windows.Forms.Label lblFilterCompany;
		private System.Windows.Forms.Button btnClearFilter;
	}
}