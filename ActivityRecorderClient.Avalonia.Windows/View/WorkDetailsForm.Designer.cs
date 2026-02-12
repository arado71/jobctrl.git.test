using MetroFramework.Controls;

namespace Tct.ActivityRecorderClient.View
{
	partial class WorkDetailsForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;


		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.pnlWorkInfo = new System.Windows.Forms.Panel();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.lblProject = new MetroFramework.Controls.MetroLabel();
			this.lblDesc = new MetroFramework.Controls.MetroLabel();
			this.lblName = new MetroFramework.Controls.MetroLabel();
			this.txtDescription = new System.Windows.Forms.TextBox();
			this.txtName = new System.Windows.Forms.TextBox();
			this.lblPrio = new MetroFramework.Controls.MetroLabel();
			this.lblDuration = new MetroFramework.Controls.MetroLabel();
			this.lblStart = new MetroFramework.Controls.MetroLabel();
			this.txtPriority = new Tct.ActivityRecorderClient.View.Controls.FilteredTextBox();
			this.dtpStartDate = new System.Windows.Forms.DateTimePicker();
			this.dtpEndDate = new System.Windows.Forms.DateTimePicker();
			this.lblCategory = new MetroFramework.Controls.MetroLabel();
			this.cbCategory = new System.Windows.Forms.ComboBox();
			this.lblTotalWorkTime = new MetroFramework.Controls.MetroLabel();
			this.dtpDuration = new Tct.ActivityRecorderClient.View.HourMinutePicker();
			this.pnlProject = new System.Windows.Forms.Panel();
			this.llbProjectInstr = new System.Windows.Forms.LinkLabel();
			this.lblProjWm = new System.Windows.Forms.Label();
			this.cbProject = new Tct.ActivityRecorderClient.View.ProjectSelectorComboBox();
			this.pReason = new System.Windows.Forms.Panel();
			this.lblTaskReasonsTitle = new System.Windows.Forms.Label();
			this.dgvReasonList = new System.Windows.Forms.DataGridView();
			this.colDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colReason = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colReasonText = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.pnlEditReason = new System.Windows.Forms.Panel();
			this.lblReasonSelected = new System.Windows.Forms.Label();
			this.lblReason = new System.Windows.Forms.Label();
			this.txtReason = new System.Windows.Forms.TextBox();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.miCannedReasons = new System.Windows.Forms.ToolStripMenuItem();
			this.pnlButtons = new System.Windows.Forms.TableLayoutPanel();
			this.btnAddReason = new MetroFramework.Controls.MetroButton();
			this.btnCloseWork = new MetroFramework.Controls.MetroButton();
			this.btnCancel = new MetroFramework.Controls.MetroButton();
			this.btnOk = new MetroFramework.Controls.MetroButton();
			this.cbStart = new System.Windows.Forms.CheckBox();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
			this.tableLayoutPanel1.SuspendLayout();
			this.pnlWorkInfo.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			this.pnlProject.SuspendLayout();
			this.pReason.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvReasonList)).BeginInit();
			this.pnlEditReason.SuspendLayout();
			this.menuStrip1.SuspendLayout();
			this.pnlButtons.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.AutoSize = true;
			this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this.pnlWorkInfo, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.pReason, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.pnlEditReason, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.pnlButtons, 0, 3);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(20, 60);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 4;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(532, 576);
			this.tableLayoutPanel1.TabIndex = 8;
			// 
			// pnlWorkInfo
			// 
			this.pnlWorkInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.pnlWorkInfo.Controls.Add(this.tableLayoutPanel2);
			this.pnlWorkInfo.Location = new System.Drawing.Point(0, 0);
			this.pnlWorkInfo.Margin = new System.Windows.Forms.Padding(0);
			this.pnlWorkInfo.Name = "pnlWorkInfo";
			this.pnlWorkInfo.Size = new System.Drawing.Size(532, 280);
			this.pnlWorkInfo.TabIndex = 8;
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.ColumnCount = 3;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel2.Controls.Add(this.lblProject, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this.lblDesc, 0, 6);
			this.tableLayoutPanel2.Controls.Add(this.lblName, 0, 1);
			this.tableLayoutPanel2.Controls.Add(this.txtDescription, 0, 7);
			this.tableLayoutPanel2.Controls.Add(this.txtName, 1, 1);
			this.tableLayoutPanel2.Controls.Add(this.lblPrio, 0, 4);
			this.tableLayoutPanel2.Controls.Add(this.lblDuration, 0, 3);
			this.tableLayoutPanel2.Controls.Add(this.lblStart, 0, 2);
			this.tableLayoutPanel2.Controls.Add(this.txtPriority, 1, 4);
			this.tableLayoutPanel2.Controls.Add(this.dtpStartDate, 1, 2);
			this.tableLayoutPanel2.Controls.Add(this.dtpEndDate, 2, 2);
			this.tableLayoutPanel2.Controls.Add(this.lblCategory, 0, 5);
			this.tableLayoutPanel2.Controls.Add(this.cbCategory, 1, 5);
			this.tableLayoutPanel2.Controls.Add(this.lblTotalWorkTime, 2, 3);
			this.tableLayoutPanel2.Controls.Add(this.dtpDuration, 1, 3);
			this.tableLayoutPanel2.Controls.Add(this.pnlProject, 1, 0);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 9;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(532, 280);
			this.tableLayoutPanel2.TabIndex = 9;
			// 
			// lblProject
			// 
			this.lblProject.AutoSize = true;
			this.lblProject.FontWeight = MetroFramework.MetroLabelWeight.Bold;
			this.lblProject.Location = new System.Drawing.Point(3, 0);
			this.lblProject.Name = "lblProject";
			this.lblProject.Size = new System.Drawing.Size(57, 19);
			this.lblProject.TabIndex = 0;
			this.lblProject.Text = "Project";
			// 
			// lblDesc
			// 
			this.lblDesc.AutoSize = true;
			this.tableLayoutPanel2.SetColumnSpan(this.lblDesc, 3);
			this.lblDesc.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblDesc.Location = new System.Drawing.Point(3, 195);
			this.lblDesc.Margin = new System.Windows.Forms.Padding(3, 7, 3, 0);
			this.lblDesc.Name = "lblDesc";
			this.lblDesc.Size = new System.Drawing.Size(526, 19);
			this.lblDesc.TabIndex = 12;
			this.lblDesc.Text = "Description";
			// 
			// lblName
			// 
			this.lblName.AutoSize = true;
			this.lblName.FontWeight = MetroFramework.MetroLabelWeight.Bold;
			this.lblName.Location = new System.Drawing.Point(3, 54);
			this.lblName.Name = "lblName";
			this.lblName.Size = new System.Drawing.Size(49, 19);
			this.lblName.TabIndex = 2;
			this.lblName.Text = "Name";
			// 
			// txtDescription
			// 
			this.txtDescription.AcceptsReturn = true;
			this.tableLayoutPanel2.SetColumnSpan(this.txtDescription, 3);
			this.txtDescription.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtDescription.Location = new System.Drawing.Point(20, 217);
			this.txtDescription.Margin = new System.Windows.Forms.Padding(20, 3, 3, 3);
			this.txtDescription.MaxLength = 1000;
			this.txtDescription.Multiline = true;
			this.txtDescription.Name = "txtDescription";
			this.txtDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtDescription.Size = new System.Drawing.Size(509, 60);
			this.txtDescription.TabIndex = 7;
			this.txtDescription.TextChanged += new System.EventHandler(this.HandleInputChanged);
			// 
			// txtName
			// 
			this.tableLayoutPanel2.SetColumnSpan(this.txtName, 2);
			this.txtName.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtName.Location = new System.Drawing.Point(90, 57);
			this.txtName.Margin = new System.Windows.Forms.Padding(20, 3, 3, 3);
			this.txtName.Name = "txtName";
			this.txtName.Size = new System.Drawing.Size(439, 20);
			this.txtName.TabIndex = 1;
			this.txtName.TextChanged += new System.EventHandler(this.HandleInputChanged);
			// 
			// lblPrio
			// 
			this.lblPrio.AutoSize = true;
			this.lblPrio.Location = new System.Drawing.Point(3, 132);
			this.lblPrio.Name = "lblPrio";
			this.lblPrio.Size = new System.Drawing.Size(51, 19);
			this.lblPrio.TabIndex = 4;
			this.lblPrio.Text = "Priority";
			// 
			// lblDuration
			// 
			this.lblDuration.AutoSize = true;
			this.lblDuration.Location = new System.Drawing.Point(3, 106);
			this.lblDuration.Name = "lblDuration";
			this.lblDuration.Size = new System.Drawing.Size(59, 19);
			this.lblDuration.TabIndex = 6;
			this.lblDuration.Text = "Duration";
			// 
			// lblStart
			// 
			this.lblStart.AutoSize = true;
			this.lblStart.Location = new System.Drawing.Point(3, 80);
			this.lblStart.Name = "lblStart";
			this.lblStart.Size = new System.Drawing.Size(47, 19);
			this.lblStart.TabIndex = 8;
			this.lblStart.Text = "Period";
			// 
			// txtPriority
			// 
			this.txtPriority.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtPriority.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
			this.txtPriority.Location = new System.Drawing.Point(90, 135);
			this.txtPriority.Margin = new System.Windows.Forms.Padding(20, 3, 3, 3);
			this.txtPriority.Name = "txtPriority";
			this.txtPriority.Size = new System.Drawing.Size(208, 23);
			this.txtPriority.TabIndex = 5;
			this.txtPriority.TextChanged += new System.EventHandler(this.HandleInputChanged);
			// 
			// dtpStartDate
			// 
			this.dtpStartDate.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dtpStartDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
			this.dtpStartDate.Location = new System.Drawing.Point(90, 83);
			this.dtpStartDate.Margin = new System.Windows.Forms.Padding(20, 3, 3, 3);
			this.dtpStartDate.Name = "dtpStartDate";
			this.dtpStartDate.ShowCheckBox = true;
			this.dtpStartDate.Size = new System.Drawing.Size(208, 20);
			this.dtpStartDate.TabIndex = 2;
			this.dtpStartDate.ValueChanged += new System.EventHandler(this.HandleInputChanged);
			// 
			// dtpEndDate
			// 
			this.dtpEndDate.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dtpEndDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
			this.dtpEndDate.Location = new System.Drawing.Point(321, 83);
			this.dtpEndDate.Margin = new System.Windows.Forms.Padding(20, 3, 3, 3);
			this.dtpEndDate.Name = "dtpEndDate";
			this.dtpEndDate.ShowCheckBox = true;
			this.dtpEndDate.Size = new System.Drawing.Size(208, 20);
			this.dtpEndDate.TabIndex = 3;
			this.dtpEndDate.ValueChanged += new System.EventHandler(this.HandleInputChanged);
			// 
			// lblCategory
			// 
			this.lblCategory.AutoSize = true;
			this.lblCategory.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblCategory.Location = new System.Drawing.Point(3, 161);
			this.lblCategory.Name = "lblCategory";
			this.lblCategory.Size = new System.Drawing.Size(64, 27);
			this.lblCategory.TabIndex = 26;
			this.lblCategory.Text = "Category";
			// 
			// cbCategory
			// 
			this.cbCategory.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cbCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbCategory.FormattingEnabled = true;
			this.cbCategory.Location = new System.Drawing.Point(90, 164);
			this.cbCategory.Margin = new System.Windows.Forms.Padding(20, 3, 3, 3);
			this.cbCategory.Name = "cbCategory";
			this.cbCategory.Size = new System.Drawing.Size(208, 21);
			this.cbCategory.TabIndex = 6;
			this.cbCategory.TextChanged += new System.EventHandler(this.HandleInputChanged);
			// 
			// lblTotalWorkTime
			// 
			this.lblTotalWorkTime.AutoSize = true;
			this.lblTotalWorkTime.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblTotalWorkTime.Location = new System.Drawing.Point(321, 106);
			this.lblTotalWorkTime.Margin = new System.Windows.Forms.Padding(20, 0, 3, 0);
			this.lblTotalWorkTime.Name = "lblTotalWorkTime";
			this.lblTotalWorkTime.Size = new System.Drawing.Size(208, 26);
			this.lblTotalWorkTime.TabIndex = 30;
			this.lblTotalWorkTime.Text = "metroLabel1";
			this.lblTotalWorkTime.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// dtpDuration
			// 
			this.dtpDuration.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dtpDuration.Location = new System.Drawing.Point(90, 109);
			this.dtpDuration.Margin = new System.Windows.Forms.Padding(20, 3, 3, 3);
			this.dtpDuration.Name = "dtpDuration";
			this.dtpDuration.Size = new System.Drawing.Size(208, 20);
			this.dtpDuration.TabIndex = 4;
			this.dtpDuration.Value = null;
			this.dtpDuration.TextChanged += new System.EventHandler(this.HandleInputChanged);
			// 
			// pnlProject
			// 
			this.tableLayoutPanel2.SetColumnSpan(this.pnlProject, 2);
			this.pnlProject.Controls.Add(this.llbProjectInstr);
			this.pnlProject.Controls.Add(this.lblProjWm);
			this.pnlProject.Controls.Add(this.cbProject);
			this.pnlProject.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlProject.Location = new System.Drawing.Point(90, 3);
			this.pnlProject.Margin = new System.Windows.Forms.Padding(20, 3, 3, 3);
			this.pnlProject.Name = "pnlProject";
			this.pnlProject.Size = new System.Drawing.Size(439, 48);
			this.pnlProject.TabIndex = 0;
			// 
			// llbProjectInstr
			// 
			this.llbProjectInstr.AutoSize = true;
			this.llbProjectInstr.LinkArea = new System.Windows.Forms.LinkArea(0, 5);
			this.llbProjectInstr.Location = new System.Drawing.Point(0, 28);
			this.llbProjectInstr.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.llbProjectInstr.Name = "llbProjectInstr";
			this.llbProjectInstr.Size = new System.Drawing.Size(63, 17);
			this.llbProjectInstr.TabIndex = 32;
			this.llbProjectInstr.TabStop = true;
			this.llbProjectInstr.Text = "project instr";
			this.llbProjectInstr.UseCompatibleTextRendering = true;
			this.llbProjectInstr.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llbProjectInstr_LinkClicked);
			// 
			// lblProjWm
			// 
			this.lblProjWm.AutoSize = true;
			this.lblProjWm.ForeColor = System.Drawing.Color.Gray;
			this.lblProjWm.Location = new System.Drawing.Point(4, 3);
			this.lblProjWm.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
			this.lblProjWm.Name = "lblProjWm";
			this.lblProjWm.Size = new System.Drawing.Size(44, 13);
			this.lblProjWm.TabIndex = 31;
			this.lblProjWm.Text = "Search:";
			this.lblProjWm.Click += new System.EventHandler(this.lblProjWm_Click);
			// 
			// cbProject
			// 
			this.cbProject.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cbProject.DropDownWidth = 439;
			this.cbProject.FormattingEnabled = true;
			this.cbProject.IntegralHeight = false;
			this.cbProject.Location = new System.Drawing.Point(0, 0);
			this.cbProject.Margin = new System.Windows.Forms.Padding(20, 3, 3, 3);
			this.cbProject.MaxDropDownItems = 30;
			this.cbProject.Name = "cbProject";
			this.cbProject.Size = new System.Drawing.Size(439, 21);
			this.cbProject.TabIndex = 0;
			this.cbProject.SelectedIndexChanged += new System.EventHandler(this.HandleProjectChanged);
			this.cbProject.TextChanged += new System.EventHandler(this.cbProject_TextChanged);
			// 
			// pReason
			// 
			this.pReason.Controls.Add(this.lblTaskReasonsTitle);
			this.pReason.Controls.Add(this.dgvReasonList);
			this.pReason.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pReason.Location = new System.Drawing.Point(3, 283);
			this.pReason.Name = "pReason";
			this.pReason.Size = new System.Drawing.Size(526, 118);
			this.pReason.TabIndex = 11;
			this.pReason.Visible = false;
			// 
			// lblTaskReasonsTitle
			// 
			this.lblTaskReasonsTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblTaskReasonsTitle.AutoSize = true;
			this.lblTaskReasonsTitle.Location = new System.Drawing.Point(6, 1);
			this.lblTaskReasonsTitle.Name = "lblTaskReasonsTitle";
			this.lblTaskReasonsTitle.Size = new System.Drawing.Size(158, 13);
			this.lblTaskReasonsTitle.TabIndex = 9;
			this.lblTaskReasonsTitle.Text = "Feladathoz rogzített indoklasok:";
			// 
			// dgvReasonList
			// 
			this.dgvReasonList.AllowUserToAddRows = false;
			this.dgvReasonList.AllowUserToDeleteRows = false;
			this.dgvReasonList.AllowUserToResizeColumns = false;
			this.dgvReasonList.AllowUserToResizeRows = false;
			this.dgvReasonList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.dgvReasonList.BackgroundColor = System.Drawing.SystemColors.Window;
			this.dgvReasonList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgvReasonList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colDate,
            this.colReason,
            this.colReasonText});
			this.dgvReasonList.GridColor = System.Drawing.SystemColors.Window;
			this.dgvReasonList.Location = new System.Drawing.Point(12, 17);
			this.dgvReasonList.MultiSelect = false;
			this.dgvReasonList.Name = "dgvReasonList";
			this.dgvReasonList.ReadOnly = true;
			this.dgvReasonList.RowHeadersVisible = false;
			this.dgvReasonList.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
			this.dgvReasonList.ShowEditingIcon = false;
			this.dgvReasonList.ShowRowErrors = false;
			this.dgvReasonList.Size = new System.Drawing.Size(514, 91);
			this.dgvReasonList.TabIndex = 10;
			// 
			// colDate
			// 
			this.colDate.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.colDate.HeaderText = "idopont";
			this.colDate.Name = "colDate";
			this.colDate.ReadOnly = true;
			this.colDate.Width = 67;
			// 
			// colReason
			// 
			this.colReason.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.colReason.HeaderText = "indoklas";
			this.colReason.MinimumWidth = 50;
			this.colReason.Name = "colReason";
			this.colReason.ReadOnly = true;
			// 
			// colReasonText
			// 
			this.colReasonText.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.colReasonText.FillWeight = 50F;
			this.colReasonText.HeaderText = "szoveg";
			this.colReasonText.MinimumWidth = 50;
			this.colReasonText.Name = "colReasonText";
			this.colReasonText.ReadOnly = true;
			// 
			// pnlEditReason
			// 
			this.pnlEditReason.Controls.Add(this.lblReasonSelected);
			this.pnlEditReason.Controls.Add(this.lblReason);
			this.pnlEditReason.Controls.Add(this.txtReason);
			this.pnlEditReason.Controls.Add(this.menuStrip1);
			this.pnlEditReason.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlEditReason.Location = new System.Drawing.Point(0, 404);
			this.pnlEditReason.Margin = new System.Windows.Forms.Padding(0);
			this.pnlEditReason.Name = "pnlEditReason";
			this.pnlEditReason.Padding = new System.Windows.Forms.Padding(10, 0, 3, 0);
			this.pnlEditReason.Size = new System.Drawing.Size(532, 139);
			this.pnlEditReason.TabIndex = 8;
			this.pnlEditReason.Visible = false;
			// 
			// lblReasonSelected
			// 
			this.lblReasonSelected.AutoSize = true;
			this.lblReasonSelected.Location = new System.Drawing.Point(61, 32);
			this.lblReasonSelected.Name = "lblReasonSelected";
			this.lblReasonSelected.Size = new System.Drawing.Size(0, 13);
			this.lblReasonSelected.TabIndex = 7;
			// 
			// lblReason
			// 
			this.lblReason.AutoSize = true;
			this.lblReason.Location = new System.Drawing.Point(9, 32);
			this.lblReason.Name = "lblReason";
			this.lblReason.Size = new System.Drawing.Size(50, 13);
			this.lblReason.TabIndex = 3;
			this.lblReason.Text = "Indoklas:";
			// 
			// txtReason
			// 
			this.txtReason.AcceptsReturn = true;
			this.txtReason.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtReason.Location = new System.Drawing.Point(15, 52);
			this.txtReason.Multiline = true;
			this.txtReason.Name = "txtReason";
			this.txtReason.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtReason.Size = new System.Drawing.Size(514, 76);
			this.txtReason.TabIndex = 0;
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miCannedReasons});
			this.menuStrip1.Location = new System.Drawing.Point(10, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Padding = new System.Windows.Forms.Padding(0, 2, 0, 2);
			this.menuStrip1.ShowItemToolTips = true;
			this.menuStrip1.Size = new System.Drawing.Size(519, 24);
			this.menuStrip1.TabIndex = 4;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// miCannedReasons
			// 
			this.miCannedReasons.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.miCannedReasons.Name = "miCannedReasons";
			this.miCannedReasons.Size = new System.Drawing.Size(76, 20);
			this.miCannedReasons.Text = "Indoklasok";
			// 
			// pnlButtons
			// 
			this.pnlButtons.ColumnCount = 5;
			this.pnlButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.pnlButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.pnlButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.pnlButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.pnlButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.pnlButtons.Controls.Add(this.btnAddReason, 0, 0);
			this.pnlButtons.Controls.Add(this.btnCloseWork, 1, 0);
			this.pnlButtons.Controls.Add(this.btnCancel, 4, 0);
			this.pnlButtons.Controls.Add(this.btnOk, 3, 0);
			this.pnlButtons.Controls.Add(this.cbStart, 2, 0);
			this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlButtons.Location = new System.Drawing.Point(0, 543);
			this.pnlButtons.Margin = new System.Windows.Forms.Padding(0);
			this.pnlButtons.Name = "pnlButtons";
			this.pnlButtons.Padding = new System.Windows.Forms.Padding(9, 0, 0, 0);
			this.pnlButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.pnlButtons.Size = new System.Drawing.Size(532, 35);
			this.pnlButtons.TabIndex = 8;
			// 
			// btnAddReason
			// 
			this.btnAddReason.AutoSize = true;
			this.btnAddReason.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnAddReason.Location = new System.Drawing.Point(14, 5);
			this.btnAddReason.Margin = new System.Windows.Forms.Padding(5);
			this.btnAddReason.Name = "btnAddReason";
			this.btnAddReason.Size = new System.Drawing.Size(76, 25);
			this.btnAddReason.Style = MetroFramework.MetroColorStyle.Blue;
			this.btnAddReason.TabIndex = 0;
			this.btnAddReason.Text = "indoklas";
			this.btnAddReason.Theme = MetroFramework.MetroThemeStyle.Light;
			this.btnAddReason.UseSelectable = true;
			this.btnAddReason.Visible = false;
			this.btnAddReason.Click += new System.EventHandler(this.HandleAddReasonClicked);
			// 
			// btnCloseWork
			// 
			this.btnCloseWork.AutoSize = true;
			this.btnCloseWork.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnCloseWork.Location = new System.Drawing.Point(100, 5);
			this.btnCloseWork.Margin = new System.Windows.Forms.Padding(5);
			this.btnCloseWork.Name = "btnCloseWork";
			this.btnCloseWork.Size = new System.Drawing.Size(75, 25);
			this.btnCloseWork.Style = MetroFramework.MetroColorStyle.Blue;
			this.btnCloseWork.TabIndex = 1;
			this.btnCloseWork.Text = "lezaras";
			this.btnCloseWork.Theme = MetroFramework.MetroThemeStyle.Light;
			this.btnCloseWork.UseSelectable = true;
			this.btnCloseWork.Visible = false;
			this.btnCloseWork.Click += new System.EventHandler(this.HandleCloseWorkClicked);
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnCancel.Location = new System.Drawing.Point(452, 5);
			this.btnCancel.Margin = new System.Windows.Forms.Padding(5);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 25);
			this.btnCancel.Style = MetroFramework.MetroColorStyle.Blue;
			this.btnCancel.TabIndex = 2;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.Theme = MetroFramework.MetroThemeStyle.Light;
			this.btnCancel.UseSelectable = true;
			this.btnCancel.Click += new System.EventHandler(this.HandleCancelClicked);
			// 
			// btnOk
			// 
			this.btnOk.AutoSize = true;
			this.btnOk.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.btnOk.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnOk.Location = new System.Drawing.Point(367, 5);
			this.btnOk.Margin = new System.Windows.Forms.Padding(5);
			this.btnOk.MinimumSize = new System.Drawing.Size(75, 0);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(75, 25);
			this.btnOk.Style = MetroFramework.MetroColorStyle.Blue;
			this.btnOk.TabIndex = 3;
			this.btnOk.Text = "OK";
			this.btnOk.Theme = MetroFramework.MetroThemeStyle.Light;
			this.btnOk.UseSelectable = true;
			this.btnOk.Click += new System.EventHandler(this.HandleOkClicked);
			// 
			// cbStart
			// 
			this.cbStart.AutoSize = true;
			this.cbStart.Dock = System.Windows.Forms.DockStyle.Left;
			this.cbStart.Location = new System.Drawing.Point(183, 3);
			this.cbStart.Name = "cbStart";
			this.cbStart.Size = new System.Drawing.Size(74, 29);
			this.cbStart.TabIndex = 4;
			this.cbStart.Text = "Start work";
			this.cbStart.UseVisualStyleBackColor = true;
			// 
			// timer1
			// 
			this.timer1.Interval = 500;
			this.timer1.Tick += new System.EventHandler(this.HandleTimerTicked);
			// 
			// dataGridViewTextBoxColumn1
			// 
			this.dataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.dataGridViewTextBoxColumn1.HeaderText = "Időpont";
			this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
			this.dataGridViewTextBoxColumn1.ReadOnly = true;
			// 
			// dataGridViewTextBoxColumn2
			// 
			this.dataGridViewTextBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.dataGridViewTextBoxColumn2.HeaderText = "Indoklás";
			this.dataGridViewTextBoxColumn2.MinimumWidth = 50;
			this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
			this.dataGridViewTextBoxColumn2.ReadOnly = true;
			// 
			// dataGridViewTextBoxColumn3
			// 
			this.dataGridViewTextBoxColumn3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.dataGridViewTextBoxColumn3.FillWeight = 50F;
			this.dataGridViewTextBoxColumn3.HeaderText = "Szöveg";
			this.dataGridViewTextBoxColumn3.MinimumWidth = 50;
			this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
			this.dataGridViewTextBoxColumn3.ReadOnly = true;
			// 
			// errorProvider
			// 
			this.errorProvider.ContainerControl = this;
			// 
			// WorkDetailsForm
			// 
			this.AcceptButton = this.btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(572, 656);
			this.Controls.Add(this.tableLayoutPanel1);
			this.MainMenuStrip = this.menuStrip1;
			this.MaximizeBox = false;
			this.Name = "WorkDetailsForm";
			this.Resizable = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "TaskInformation";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.pnlWorkInfo.ResumeLayout(false);
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			this.pnlProject.ResumeLayout(false);
			this.pnlProject.PerformLayout();
			this.pReason.ResumeLayout(false);
			this.pReason.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvReasonList)).EndInit();
			this.pnlEditReason.ResumeLayout(false);
			this.pnlEditReason.PerformLayout();
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.pnlButtons.ResumeLayout(false);
			this.pnlButtons.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Panel pnlWorkInfo;
		private System.Windows.Forms.Label lblTaskReasonsTitle;
		private MetroButton btnCloseWork;
		private MetroButton btnAddReason;
		private System.Windows.Forms.Panel pnlEditReason;
		private System.Windows.Forms.Label lblReasonSelected;
		private System.Windows.Forms.Label lblReason;
		private MetroButton btnCancel;
		private MetroButton btnOk;
		private System.Windows.Forms.TextBox txtReason;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem miCannedReasons;
		private System.Windows.Forms.TableLayoutPanel pnlButtons;
		private System.Windows.Forms.DataGridView dgvReasonList;
		private System.Windows.Forms.DataGridViewTextBoxColumn colDate;
		private System.Windows.Forms.DataGridViewTextBoxColumn colReason;
		private System.Windows.Forms.DataGridViewTextBoxColumn colReasonText;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
		private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
		private MetroLabel lblProject;
		private MetroLabel lblDesc;
		private MetroLabel lblName;
		private System.Windows.Forms.TextBox txtDescription;
		private System.Windows.Forms.TextBox txtName;
		private MetroLabel lblPrio;
		private MetroLabel lblDuration;
		private MetroLabel lblStart;
		private Controls.FilteredTextBox txtPriority;
		private System.Windows.Forms.DateTimePicker dtpStartDate;
		private System.Windows.Forms.DateTimePicker dtpEndDate;
		private MetroLabel lblCategory;
		private System.Windows.Forms.ComboBox cbCategory;
		private MetroLabel lblTotalWorkTime;
		private HourMinutePicker dtpDuration;
		private System.Windows.Forms.ErrorProvider errorProvider;
		private ProjectSelectorComboBox cbProject;
		private System.Windows.Forms.Panel pReason;
		private System.Windows.Forms.CheckBox cbStart;
		private System.Windows.Forms.Panel pnlProject;
		private System.Windows.Forms.Label lblProjWm;
		private System.Windows.Forms.LinkLabel llbProjectInstr;
	}
}