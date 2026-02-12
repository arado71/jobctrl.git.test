namespace OcrConfig.Forms
{
	partial class TrainingOptimizationForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private global::System.ComponentModel.IContainer components = null;

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
			this.btnSave = new System.Windows.Forms.Button();
			this.lblLearnP8 = new System.Windows.Forms.Label();
			this.lblLearnP9 = new System.Windows.Forms.Label();
			this.lblLearnP7 = new System.Windows.Forms.Label();
			this.btnExport = new System.Windows.Forms.Button();
			this.lblLearnP6 = new System.Windows.Forms.Label();
			this.lblLearnP5 = new System.Windows.Forms.Label();
			this.lblLearnP4 = new System.Windows.Forms.Label();
			this.lblLearnP3 = new System.Windows.Forms.Label();
			this.lblLearnP2 = new System.Windows.Forms.Label();
			this.lblLearnP1 = new System.Windows.Forms.Label();
			this.pnlDetail = new System.Windows.Forms.Panel();
			this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
			this.pnlProgress = new System.Windows.Forms.Panel();
			this.pnlProgressbar = new System.Windows.Forms.Panel();
			this.pAccuracy = new System.Windows.Forms.ProgressBar();
			this.pnlProgressButton = new System.Windows.Forms.Panel();
			this.btnChangeDisplayMode = new System.Windows.Forms.Button();
			this.pnlDetails = new System.Windows.Forms.Panel();
			this.tlpDetails = new System.Windows.Forms.TableLayoutPanel();
			this.tlpResults = new System.Windows.Forms.TableLayoutPanel();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.lblRecogP6 = new System.Windows.Forms.Label();
			this.lblRecogP5 = new System.Windows.Forms.Label();
			this.lblRecogP4 = new System.Windows.Forms.Label();
			this.lblRecogP3 = new System.Windows.Forms.Label();
			this.lblRecogP2 = new System.Windows.Forms.Label();
			this.lblRecogP1 = new System.Windows.Forms.Label();
			this.dgResults = new System.Windows.Forms.DataGridView();
			this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.btnExportAll = new System.Windows.Forms.Button();
			this.label22 = new System.Windows.Forms.Label();
			this.tlpMetrics = new System.Windows.Forms.TableLayoutPanel();
			this.label10 = new System.Windows.Forms.Label();
			this.lblCharAccuracy = new System.Windows.Forms.Label();
			this.label12 = new System.Windows.Forms.Label();
			this.lblSpeed = new System.Windows.Forms.Label();
			this.label14 = new System.Windows.Forms.Label();
			this.lblIterations = new System.Windows.Forms.Label();
			this.label16 = new System.Windows.Forms.Label();
			this.lblSamples = new System.Windows.Forms.Label();
			this.label18 = new System.Windows.Forms.Label();
			this.lblSampleAccuracy = new System.Windows.Forms.Label();
			this.label20 = new System.Windows.Forms.Label();
			this.lblTrainSpeed = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.tlpMain.SuspendLayout();
			this.pnlProgress.SuspendLayout();
			this.pnlProgressbar.SuspendLayout();
			this.pnlProgressButton.SuspendLayout();
			this.pnlDetails.SuspendLayout();
			this.tlpDetails.SuspendLayout();
			this.tlpResults.SuspendLayout();
			this.groupBox4.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgResults)).BeginInit();
			this.groupBox3.SuspendLayout();
			this.tlpMetrics.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnSave
			// 
			this.btnSave.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.btnSave.Location = new System.Drawing.Point(3, 313);
			this.btnSave.Name = "btnSave";
			this.btnSave.Size = new System.Drawing.Size(202, 22);
			this.btnSave.TabIndex = 10;
			this.btnSave.Text = "Save";
			this.btnSave.UseVisualStyleBackColor = true;
			this.btnSave.Click += new System.EventHandler(this.HandleSaveClicked);
			// 
			// lblLearnP8
			// 
			this.lblLearnP8.AutoSize = true;
			this.lblLearnP8.Location = new System.Drawing.Point(7, 110);
			this.lblLearnP8.Name = "lblLearnP8";
			this.lblLearnP8.Size = new System.Drawing.Size(27, 13);
			this.lblLearnP8.TabIndex = 9;
			this.lblLearnP8.Text = "N/A";
			// 
			// lblLearnP9
			// 
			this.lblLearnP9.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblLearnP9.Location = new System.Drawing.Point(7, 122);
			this.lblLearnP9.Name = "lblLearnP9";
			this.lblLearnP9.Size = new System.Drawing.Size(200, 95);
			this.lblLearnP9.TabIndex = 7;
			this.lblLearnP9.Text = "N/A";
			// 
			// lblLearnP7
			// 
			this.lblLearnP7.AutoSize = true;
			this.lblLearnP7.Location = new System.Drawing.Point(7, 97);
			this.lblLearnP7.Name = "lblLearnP7";
			this.lblLearnP7.Size = new System.Drawing.Size(27, 13);
			this.lblLearnP7.TabIndex = 6;
			this.lblLearnP7.Text = "N/A";
			// 
			// btnExport
			// 
			this.btnExport.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.btnExport.Location = new System.Drawing.Point(3, 313);
			this.btnExport.Name = "btnExport";
			this.btnExport.Size = new System.Drawing.Size(202, 22);
			this.btnExport.TabIndex = 8;
			this.btnExport.Text = "Export traineddata";
			this.btnExport.UseVisualStyleBackColor = true;
			this.btnExport.Click += new System.EventHandler(this.HandleExportClicked);
			// 
			// lblLearnP6
			// 
			this.lblLearnP6.AutoSize = true;
			this.lblLearnP6.Location = new System.Drawing.Point(7, 84);
			this.lblLearnP6.Name = "lblLearnP6";
			this.lblLearnP6.Size = new System.Drawing.Size(27, 13);
			this.lblLearnP6.TabIndex = 5;
			this.lblLearnP6.Text = "N/A";
			// 
			// lblLearnP5
			// 
			this.lblLearnP5.AutoSize = true;
			this.lblLearnP5.Location = new System.Drawing.Point(7, 71);
			this.lblLearnP5.Name = "lblLearnP5";
			this.lblLearnP5.Size = new System.Drawing.Size(27, 13);
			this.lblLearnP5.TabIndex = 4;
			this.lblLearnP5.Text = "N/A";
			// 
			// lblLearnP4
			// 
			this.lblLearnP4.AutoSize = true;
			this.lblLearnP4.Location = new System.Drawing.Point(7, 58);
			this.lblLearnP4.Name = "lblLearnP4";
			this.lblLearnP4.Size = new System.Drawing.Size(27, 13);
			this.lblLearnP4.TabIndex = 3;
			this.lblLearnP4.Text = "N/A";
			// 
			// lblLearnP3
			// 
			this.lblLearnP3.AutoSize = true;
			this.lblLearnP3.Location = new System.Drawing.Point(7, 46);
			this.lblLearnP3.Name = "lblLearnP3";
			this.lblLearnP3.Size = new System.Drawing.Size(27, 13);
			this.lblLearnP3.TabIndex = 2;
			this.lblLearnP3.Text = "N/A";
			// 
			// lblLearnP2
			// 
			this.lblLearnP2.AutoSize = true;
			this.lblLearnP2.Location = new System.Drawing.Point(7, 33);
			this.lblLearnP2.Name = "lblLearnP2";
			this.lblLearnP2.Size = new System.Drawing.Size(27, 13);
			this.lblLearnP2.TabIndex = 1;
			this.lblLearnP2.Text = "N/A";
			// 
			// lblLearnP1
			// 
			this.lblLearnP1.AutoSize = true;
			this.lblLearnP1.Location = new System.Drawing.Point(7, 20);
			this.lblLearnP1.Name = "lblLearnP1";
			this.lblLearnP1.Size = new System.Drawing.Size(27, 13);
			this.lblLearnP1.TabIndex = 0;
			this.lblLearnP1.Text = "N/A";
			// 
			// pnlDetail
			// 
			this.pnlDetail.Dock = System.Windows.Forms.DockStyle.Top;
			this.pnlDetail.Location = new System.Drawing.Point(0, 0);
			this.pnlDetail.Name = "pnlDetail";
			this.pnlDetail.Padding = new System.Windows.Forms.Padding(3);
			this.pnlDetail.Size = new System.Drawing.Size(200, 100);
			this.pnlDetail.TabIndex = 0;
			// 
			// tlpMain
			// 
			this.tlpMain.ColumnCount = 1;
			this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tlpMain.Controls.Add(this.pnlProgress, 0, 0);
			this.tlpMain.Controls.Add(this.pnlDetails, 0, 1);
			this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tlpMain.Location = new System.Drawing.Point(0, 0);
			this.tlpMain.MinimumSize = new System.Drawing.Size(1078, 39);
			this.tlpMain.Name = "tlpMain";
			this.tlpMain.RowCount = 2;
			this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
			this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tlpMain.Size = new System.Drawing.Size(1078, 830);
			this.tlpMain.TabIndex = 2;
			// 
			// pnlProgress
			// 
			this.pnlProgress.Controls.Add(this.pnlProgressbar);
			this.pnlProgress.Controls.Add(this.pnlProgressButton);
			this.pnlProgress.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlProgress.Location = new System.Drawing.Point(3, 3);
			this.pnlProgress.MinimumSize = new System.Drawing.Size(1072, 35);
			this.pnlProgress.Name = "pnlProgress";
			this.pnlProgress.Padding = new System.Windows.Forms.Padding(3);
			this.pnlProgress.Size = new System.Drawing.Size(1072, 36);
			this.pnlProgress.TabIndex = 1;
			// 
			// pnlProgressbar
			// 
			this.pnlProgressbar.Controls.Add(this.pAccuracy);
			this.pnlProgressbar.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlProgressbar.Location = new System.Drawing.Point(3, 3);
			this.pnlProgressbar.Name = "pnlProgressbar";
			this.pnlProgressbar.Padding = new System.Windows.Forms.Padding(0, 0, 3, 0);
			this.pnlProgressbar.Size = new System.Drawing.Size(1032, 30);
			this.pnlProgressbar.TabIndex = 0;
			// 
			// pAccuracy
			// 
			this.pAccuracy.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pAccuracy.Location = new System.Drawing.Point(0, 0);
			this.pAccuracy.Name = "pAccuracy";
			this.pAccuracy.Size = new System.Drawing.Size(1029, 30);
			this.pAccuracy.TabIndex = 1;
			// 
			// pnlProgressButton
			// 
			this.pnlProgressButton.Controls.Add(this.btnChangeDisplayMode);
			this.pnlProgressButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.pnlProgressButton.Location = new System.Drawing.Point(1035, 3);
			this.pnlProgressButton.Name = "pnlProgressButton";
			this.pnlProgressButton.Size = new System.Drawing.Size(34, 30);
			this.pnlProgressButton.TabIndex = 1;
			// 
			// btnChangeDisplayMode
			// 
			this.btnChangeDisplayMode.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnChangeDisplayMode.Location = new System.Drawing.Point(0, 0);
			this.btnChangeDisplayMode.Name = "btnChangeDisplayMode";
			this.btnChangeDisplayMode.Size = new System.Drawing.Size(34, 30);
			this.btnChangeDisplayMode.TabIndex = 9;
			this.btnChangeDisplayMode.Text = "FS";
			this.btnChangeDisplayMode.UseVisualStyleBackColor = true;
			this.btnChangeDisplayMode.Click += new System.EventHandler(this.ChangeDisplayMode_Click);
			// 
			// pnlDetails
			// 
			this.pnlDetails.Controls.Add(this.tlpDetails);
			this.pnlDetails.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlDetails.Location = new System.Drawing.Point(3, 45);
			this.pnlDetails.MaximumSize = new System.Drawing.Size(1072, 790);
			this.pnlDetails.Name = "pnlDetails";
			this.pnlDetails.Size = new System.Drawing.Size(1072, 782);
			this.pnlDetails.TabIndex = 2;
			this.pnlDetails.VisibleChanged += new System.EventHandler(this.pnlDetails_VisibleChanged);
			// 
			// tlpDetails
			// 
			this.tlpDetails.ColumnCount = 1;
			this.tlpDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tlpDetails.Controls.Add(this.tlpResults, 0, 3);
			this.tlpDetails.Controls.Add(this.label22, 0, 2);
			this.tlpDetails.Controls.Add(this.tlpMetrics, 0, 1);
			this.tlpDetails.Controls.Add(this.label1, 0, 0);
			this.tlpDetails.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tlpDetails.Location = new System.Drawing.Point(0, 0);
			this.tlpDetails.Name = "tlpDetails";
			this.tlpDetails.RowCount = 4;
			this.tlpDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
			this.tlpDetails.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tlpDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 14F));
			this.tlpDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tlpDetails.Size = new System.Drawing.Size(1072, 782);
			this.tlpDetails.TabIndex = 0;
			// 
			// tlpResults
			// 
			this.tlpResults.ColumnCount = 2;
			this.tlpResults.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 80F));
			this.tlpResults.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
			this.tlpResults.Controls.Add(this.groupBox4, 0, 1);
			this.tlpResults.Controls.Add(this.dgResults, 0, 0);
			this.tlpResults.Controls.Add(this.groupBox3, 1, 0);
			this.tlpResults.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tlpResults.Location = new System.Drawing.Point(3, 91);
			this.tlpResults.Name = "tlpResults";
			this.tlpResults.RowCount = 2;
			this.tlpResults.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tlpResults.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tlpResults.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tlpResults.Size = new System.Drawing.Size(1066, 688);
			this.tlpResults.TabIndex = 8;
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.Add(this.btnSave);
			this.groupBox4.Controls.Add(this.lblRecogP6);
			this.groupBox4.Controls.Add(this.lblRecogP5);
			this.groupBox4.Controls.Add(this.lblRecogP4);
			this.groupBox4.Controls.Add(this.lblRecogP3);
			this.groupBox4.Controls.Add(this.lblRecogP2);
			this.groupBox4.Controls.Add(this.lblRecogP1);
			this.groupBox4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBox4.Location = new System.Drawing.Point(855, 347);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(208, 338);
			this.groupBox4.TabIndex = 9;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Recognition";
			// 
			// lblRecogP6
			// 
			this.lblRecogP6.AutoSize = true;
			this.lblRecogP6.Location = new System.Drawing.Point(7, 80);
			this.lblRecogP6.Name = "lblRecogP6";
			this.lblRecogP6.Size = new System.Drawing.Size(27, 13);
			this.lblRecogP6.TabIndex = 7;
			this.lblRecogP6.Text = "N/A";
			// 
			// lblRecogP5
			// 
			this.lblRecogP5.AutoSize = true;
			this.lblRecogP5.Location = new System.Drawing.Point(7, 67);
			this.lblRecogP5.Name = "lblRecogP5";
			this.lblRecogP5.Size = new System.Drawing.Size(27, 13);
			this.lblRecogP5.TabIndex = 10;
			this.lblRecogP5.Text = "N/A";
			// 
			// lblRecogP4
			// 
			this.lblRecogP4.AutoSize = true;
			this.lblRecogP4.Location = new System.Drawing.Point(7, 54);
			this.lblRecogP4.Name = "lblRecogP4";
			this.lblRecogP4.Size = new System.Drawing.Size(27, 13);
			this.lblRecogP4.TabIndex = 9;
			this.lblRecogP4.Text = "N/A";
			// 
			// lblRecogP3
			// 
			this.lblRecogP3.AutoSize = true;
			this.lblRecogP3.Location = new System.Drawing.Point(7, 42);
			this.lblRecogP3.Name = "lblRecogP3";
			this.lblRecogP3.Size = new System.Drawing.Size(27, 13);
			this.lblRecogP3.TabIndex = 8;
			this.lblRecogP3.Text = "N/A";
			// 
			// lblRecogP2
			// 
			this.lblRecogP2.AutoSize = true;
			this.lblRecogP2.Location = new System.Drawing.Point(7, 29);
			this.lblRecogP2.Name = "lblRecogP2";
			this.lblRecogP2.Size = new System.Drawing.Size(27, 13);
			this.lblRecogP2.TabIndex = 7;
			this.lblRecogP2.Text = "N/A";
			// 
			// lblRecogP1
			// 
			this.lblRecogP1.AutoSize = true;
			this.lblRecogP1.Location = new System.Drawing.Point(7, 16);
			this.lblRecogP1.Name = "lblRecogP1";
			this.lblRecogP1.Size = new System.Drawing.Size(27, 13);
			this.lblRecogP1.TabIndex = 6;
			this.lblRecogP1.Text = "N/A";
			// 
			// dgResults
			// 
			this.dgResults.AllowUserToAddRows = false;
			this.dgResults.AllowUserToDeleteRows = false;
			this.dgResults.AllowUserToOrderColumns = true;
			this.dgResults.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
			this.dgResults.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgResults.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewTextBoxColumn2,
            this.dataGridViewTextBoxColumn3,
            this.dataGridViewTextBoxColumn4,
            this.dataGridViewTextBoxColumn5});
			this.dgResults.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dgResults.Location = new System.Drawing.Point(3, 3);
			this.dgResults.Name = "dgResults";
			this.dgResults.ReadOnly = true;
			this.tlpResults.SetRowSpan(this.dgResults, 2);
			this.dgResults.Size = new System.Drawing.Size(846, 682);
			this.dgResults.TabIndex = 6;
			// 
			// dataGridViewTextBoxColumn1
			// 
			this.dataGridViewTextBoxColumn1.HeaderText = "Recognized";
			this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
			this.dataGridViewTextBoxColumn1.ReadOnly = true;
			this.dataGridViewTextBoxColumn1.Width = 89;
			// 
			// dataGridViewTextBoxColumn2
			// 
			this.dataGridViewTextBoxColumn2.HeaderText = "Expected";
			this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
			this.dataGridViewTextBoxColumn2.ReadOnly = true;
			this.dataGridViewTextBoxColumn2.Width = 77;
			// 
			// dataGridViewTextBoxColumn3
			// 
			this.dataGridViewTextBoxColumn3.HeaderText = "Errors";
			this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
			this.dataGridViewTextBoxColumn3.ReadOnly = true;
			this.dataGridViewTextBoxColumn3.Width = 59;
			// 
			// dataGridViewTextBoxColumn4
			// 
			this.dataGridViewTextBoxColumn4.HeaderText = "Accuracy";
			this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
			this.dataGridViewTextBoxColumn4.ReadOnly = true;
			this.dataGridViewTextBoxColumn4.Width = 77;
			// 
			// dataGridViewTextBoxColumn5
			// 
			this.dataGridViewTextBoxColumn5.HeaderText = "Loose acc.";
			this.dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
			this.dataGridViewTextBoxColumn5.ReadOnly = true;
			this.dataGridViewTextBoxColumn5.Width = 85;
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.btnExportAll);
			this.groupBox3.Controls.Add(this.lblLearnP8);
			this.groupBox3.Controls.Add(this.lblLearnP9);
			this.groupBox3.Controls.Add(this.lblLearnP7);
			this.groupBox3.Controls.Add(this.btnExport);
			this.groupBox3.Controls.Add(this.lblLearnP6);
			this.groupBox3.Controls.Add(this.lblLearnP5);
			this.groupBox3.Controls.Add(this.lblLearnP4);
			this.groupBox3.Controls.Add(this.lblLearnP3);
			this.groupBox3.Controls.Add(this.lblLearnP2);
			this.groupBox3.Controls.Add(this.lblLearnP1);
			this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBox3.Location = new System.Drawing.Point(855, 3);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(208, 338);
			this.groupBox3.TabIndex = 7;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Learning";
			// 
			// btnExportAll
			// 
			this.btnExportAll.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.btnExportAll.Location = new System.Drawing.Point(3, 290);
			this.btnExportAll.Name = "btnExportAll";
			this.btnExportAll.Size = new System.Drawing.Size(202, 23);
			this.btnExportAll.TabIndex = 10;
			this.btnExportAll.Text = "Export all traineddata";
			this.btnExportAll.UseVisualStyleBackColor = true;
			this.btnExportAll.Click += new System.EventHandler(this.btnExportAll_Click);
			// 
			// label22
			// 
			this.label22.AutoSize = true;
			this.label22.Location = new System.Drawing.Point(3, 74);
			this.label22.Name = "label22";
			this.label22.Size = new System.Drawing.Size(42, 13);
			this.label22.TabIndex = 6;
			this.label22.Text = "Results";
			// 
			// tlpMetrics
			// 
			this.tlpMetrics.AutoSize = true;
			this.tlpMetrics.ColumnCount = 6;
			this.tlpMetrics.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
			this.tlpMetrics.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
			this.tlpMetrics.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
			this.tlpMetrics.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
			this.tlpMetrics.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
			this.tlpMetrics.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
			this.tlpMetrics.Controls.Add(this.label10, 0, 0);
			this.tlpMetrics.Controls.Add(this.lblCharAccuracy, 0, 1);
			this.tlpMetrics.Controls.Add(this.label12, 4, 0);
			this.tlpMetrics.Controls.Add(this.lblSpeed, 4, 1);
			this.tlpMetrics.Controls.Add(this.label14, 3, 0);
			this.tlpMetrics.Controls.Add(this.lblIterations, 3, 1);
			this.tlpMetrics.Controls.Add(this.label16, 2, 0);
			this.tlpMetrics.Controls.Add(this.lblSamples, 2, 1);
			this.tlpMetrics.Controls.Add(this.label18, 1, 0);
			this.tlpMetrics.Controls.Add(this.lblSampleAccuracy, 1, 1);
			this.tlpMetrics.Controls.Add(this.label20, 5, 0);
			this.tlpMetrics.Controls.Add(this.lblTrainSpeed, 5, 1);
			this.tlpMetrics.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tlpMetrics.Location = new System.Drawing.Point(3, 25);
			this.tlpMetrics.Name = "tlpMetrics";
			this.tlpMetrics.Padding = new System.Windows.Forms.Padding(6, 5, 6, 10);
			this.tlpMetrics.RowCount = 2;
			this.tlpMetrics.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 18F));
			this.tlpMetrics.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tlpMetrics.Size = new System.Drawing.Size(1066, 46);
			this.tlpMetrics.TabIndex = 5;
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(9, 5);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(79, 13);
			this.label10.TabIndex = 0;
			this.label10.Text = "Char. accuracy";
			// 
			// lblCharAccuracy
			// 
			this.lblCharAccuracy.AutoSize = true;
			this.lblCharAccuracy.Location = new System.Drawing.Point(9, 23);
			this.lblCharAccuracy.Name = "lblCharAccuracy";
			this.lblCharAccuracy.Size = new System.Drawing.Size(13, 13);
			this.lblCharAccuracy.TabIndex = 1;
			this.lblCharAccuracy.Text = "?";
			// 
			// label12
			// 
			this.label12.AutoSize = true;
			this.label12.Location = new System.Drawing.Point(709, 5);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(94, 13);
			this.label12.TabIndex = 6;
			this.label12.Text = "Avg. recog. speed";
			// 
			// lblSpeed
			// 
			this.lblSpeed.AutoSize = true;
			this.lblSpeed.Location = new System.Drawing.Point(709, 23);
			this.lblSpeed.Name = "lblSpeed";
			this.lblSpeed.Size = new System.Drawing.Size(13, 13);
			this.lblSpeed.TabIndex = 7;
			this.lblSpeed.Text = "?";
			// 
			// label14
			// 
			this.label14.AutoSize = true;
			this.label14.Location = new System.Drawing.Point(534, 5);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(77, 13);
			this.label14.TabIndex = 4;
			this.label14.Text = "Iterations done";
			// 
			// lblIterations
			// 
			this.lblIterations.AutoSize = true;
			this.lblIterations.Location = new System.Drawing.Point(534, 23);
			this.lblIterations.Name = "lblIterations";
			this.lblIterations.Size = new System.Drawing.Size(13, 13);
			this.lblIterations.TabIndex = 5;
			this.lblIterations.Text = "0";
			// 
			// label16
			// 
			this.label16.AutoSize = true;
			this.label16.Location = new System.Drawing.Point(359, 5);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(47, 13);
			this.label16.TabIndex = 2;
			this.label16.Text = "Samples";
			// 
			// lblSamples
			// 
			this.lblSamples.AutoSize = true;
			this.lblSamples.Location = new System.Drawing.Point(359, 23);
			this.lblSamples.Name = "lblSamples";
			this.lblSamples.Size = new System.Drawing.Size(13, 13);
			this.lblSamples.TabIndex = 3;
			this.lblSamples.Text = "0";
			// 
			// label18
			// 
			this.label18.AutoSize = true;
			this.label18.Location = new System.Drawing.Point(184, 5);
			this.label18.Name = "label18";
			this.label18.Size = new System.Drawing.Size(89, 13);
			this.label18.TabIndex = 8;
			this.label18.Text = "Sample accuracy";
			// 
			// lblSampleAccuracy
			// 
			this.lblSampleAccuracy.AutoSize = true;
			this.lblSampleAccuracy.Location = new System.Drawing.Point(184, 23);
			this.lblSampleAccuracy.Name = "lblSampleAccuracy";
			this.lblSampleAccuracy.Size = new System.Drawing.Size(13, 13);
			this.lblSampleAccuracy.TabIndex = 9;
			this.lblSampleAccuracy.Text = "?";
			// 
			// label20
			// 
			this.label20.AutoSize = true;
			this.label20.Location = new System.Drawing.Point(884, 5);
			this.label20.Name = "label20";
			this.label20.Size = new System.Drawing.Size(77, 13);
			this.label20.TabIndex = 10;
			this.label20.Text = "Training speed";
			// 
			// lblTrainSpeed
			// 
			this.lblTrainSpeed.AutoSize = true;
			this.lblTrainSpeed.Location = new System.Drawing.Point(884, 23);
			this.lblTrainSpeed.Name = "lblTrainSpeed";
			this.lblTrainSpeed.Size = new System.Drawing.Size(13, 13);
			this.lblTrainSpeed.TabIndex = 11;
			this.lblTrainSpeed.Text = "?";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(44, 13);
			this.label1.TabIndex = 4;
			this.label1.Text = "Metrics:";
			// 
			// TrainingOptimizationForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.ClientSize = new System.Drawing.Size(1078, 830);
			this.Controls.Add(this.tlpMain);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "TrainingOptimizationForm";
			this.Text = "Optimization";
			this.tlpMain.ResumeLayout(false);
			this.pnlProgress.ResumeLayout(false);
			this.pnlProgressbar.ResumeLayout(false);
			this.pnlProgressButton.ResumeLayout(false);
			this.pnlDetails.ResumeLayout(false);
			this.tlpDetails.ResumeLayout(false);
			this.tlpDetails.PerformLayout();
			this.tlpResults.ResumeLayout(false);
			this.groupBox4.ResumeLayout(false);
			this.groupBox4.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgResults)).EndInit();
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			this.tlpMetrics.ResumeLayout(false);
			this.tlpMetrics.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private global::System.Windows.Forms.Panel pnlDetail;
		private global::System.Windows.Forms.Label lblLearnP6;
		private global::System.Windows.Forms.Label lblLearnP5;
		private global::System.Windows.Forms.Label lblLearnP4;
		private global::System.Windows.Forms.Label lblLearnP3;
		private global::System.Windows.Forms.Label lblLearnP2;
		private global::System.Windows.Forms.Label lblLearnP1;
		private global::System.Windows.Forms.Label lblLearnP7;
		private global::System.Windows.Forms.Label lblLearnP9;
		private global::System.Windows.Forms.Button btnExport;
		private global::System.Windows.Forms.Label lblLearnP8;
		private global::System.Windows.Forms.TableLayoutPanel tlpMain;
		private global::System.Windows.Forms.Panel pnlProgress;
		private global::System.Windows.Forms.Panel pnlProgressbar;
		private global::System.Windows.Forms.ProgressBar pAccuracy;
		private global::System.Windows.Forms.Panel pnlProgressButton;
		private global::System.Windows.Forms.Panel pnlDetails;
		private global::System.Windows.Forms.TableLayoutPanel tlpDetails;
		private global::System.Windows.Forms.TableLayoutPanel tlpMetrics;
		private global::System.Windows.Forms.Label label10;
		private global::System.Windows.Forms.Label lblCharAccuracy;
		private global::System.Windows.Forms.Label label12;
		private global::System.Windows.Forms.Label lblSpeed;
		private global::System.Windows.Forms.Label label14;
		private global::System.Windows.Forms.Label lblIterations;
		private global::System.Windows.Forms.Label label16;
		private global::System.Windows.Forms.Label lblSamples;
		private global::System.Windows.Forms.Label label18;
		private global::System.Windows.Forms.Label lblSampleAccuracy;
		private global::System.Windows.Forms.Label label20;
		private global::System.Windows.Forms.Label lblTrainSpeed;
		private global::System.Windows.Forms.Label label1;
		private global::System.Windows.Forms.TableLayoutPanel tlpResults;
		private global::System.Windows.Forms.DataGridView dgResults;
		private global::System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
		private global::System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
		private global::System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
		private global::System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
		private global::System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
		private global::System.Windows.Forms.Label label22;
		private global::System.Windows.Forms.GroupBox groupBox4;
		private global::System.Windows.Forms.Label lblRecogP6;
		private global::System.Windows.Forms.Label lblRecogP5;
		private global::System.Windows.Forms.Label lblRecogP4;
		private global::System.Windows.Forms.Label lblRecogP3;
		private global::System.Windows.Forms.Label lblRecogP2;
		private global::System.Windows.Forms.Label lblRecogP1;
		private global::System.Windows.Forms.GroupBox groupBox3;
		private global::System.Windows.Forms.Button btnSave;
		private System.Windows.Forms.Button btnChangeDisplayMode;
		private System.Windows.Forms.Button btnExportAll;
	}
}