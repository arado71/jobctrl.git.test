using System.Windows.Forms;

namespace JCAutomation.View
{
	partial class SpyForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		private ContextMenuStrip cmMenu;
		private ToolStripMenuItem miGenerateCode;
		private ToolStripMenuItem miGenerateCodeStale;
		private ToolStripMenuItem miRefresh;
		private ToolStripMenuItem miSavePathToElement;
		private Timer timerUpdate;

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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SpyForm));
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.panel1 = new System.Windows.Forms.Panel();
			this.pbtOcr = new System.Windows.Forms.PictureBox();
			this.btnPlugins = new System.Windows.Forms.Button();
			this.txtVisible = new System.Windows.Forms.TextBox();
			this.chbPath = new System.Windows.Forms.CheckBox();
			this.chbUpdate = new System.Windows.Forms.CheckBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.txtText = new System.Windows.Forms.TextBox();
			this.txtValue = new System.Windows.Forms.TextBox();
			this.txtName = new System.Windows.Forms.TextBox();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.tvNodes = new System.Windows.Forms.TreeView();
			this.tabPages = new System.Windows.Forms.TabControl();
			this.tabCature = new System.Windows.Forms.TabPage();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.dataGridView1 = new System.Windows.Forms.DataGridView();
			this.NameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ValueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colElapsed = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.txtScript = new System.Windows.Forms.TextBox();
			this.pnlCommon = new System.Windows.Forms.Panel();
			this.pnlCompile = new System.Windows.Forms.Panel();
			this.btnCompile = new System.Windows.Forms.Button();
			this.lblHandle = new System.Windows.Forms.Label();
			this.pnlSetting = new System.Windows.Forms.Panel();
			this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
			this.label4 = new System.Windows.Forms.Label();
			this.tabAutomat = new System.Windows.Forms.TabPage();
			this.txtLog = new System.Windows.Forms.TextBox();
			this.generateQueryStringUsingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.nameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.textToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.valueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.timerUpdate = new System.Windows.Forms.Timer(this.components);
			this.cmMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.miRefresh = new System.Windows.Forms.ToolStripMenuItem();
			this.miSavePathToElement = new System.Windows.Forms.ToolStripMenuItem();
			this.miGenerateCode = new System.Windows.Forms.ToolStripMenuItem();
			this.miGenerateCodeStale = new System.Windows.Forms.ToolStripMenuItem();
			this.radionameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.radiovalueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.tableLayoutPanel1.SuspendLayout();
			this.panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbtOcr)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.tabPages.SuspendLayout();
			this.tabCature.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
			this.pnlCommon.SuspendLayout();
			this.pnlCompile.SuspendLayout();
			this.pnlSetting.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
			this.tabAutomat.SuspendLayout();
			this.cmMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.splitContainer1, 0, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(774, 709);
			this.tableLayoutPanel1.TabIndex = 15;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.pbtOcr);
			this.panel1.Controls.Add(this.btnPlugins);
			this.panel1.Controls.Add(this.txtVisible);
			this.panel1.Controls.Add(this.chbPath);
			this.panel1.Controls.Add(this.chbUpdate);
			this.panel1.Controls.Add(this.label3);
			this.panel1.Controls.Add(this.label2);
			this.panel1.Controls.Add(this.label1);
			this.panel1.Controls.Add(this.txtText);
			this.panel1.Controls.Add(this.txtValue);
			this.panel1.Controls.Add(this.txtName);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(3, 3);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(768, 94);
			this.panel1.TabIndex = 1;
			// 
			// pbtOcr
			// 
			this.pbtOcr.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.pbtOcr.Image = global::Properties.Resources.ocrd;
			this.pbtOcr.Location = new System.Drawing.Point(685, 6);
			this.pbtOcr.Margin = new System.Windows.Forms.Padding(2);
			this.pbtOcr.Name = "pbtOcr";
			this.pbtOcr.Size = new System.Drawing.Size(75, 81);
			this.pbtOcr.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pbtOcr.TabIndex = 10;
			this.pbtOcr.TabStop = false;
			this.pbtOcr.Click += new System.EventHandler(this.pbtOcr_Click);
			// 
			// btnPlugins
			// 
			this.btnPlugins.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnPlugins.Location = new System.Drawing.Point(598, 6);
			this.btnPlugins.Name = "btnPlugins";
			this.btnPlugins.Size = new System.Drawing.Size(75, 23);
			this.btnPlugins.TabIndex = 9;
			this.btnPlugins.Text = "Plugins...";
			this.btnPlugins.UseVisualStyleBackColor = true;
			this.btnPlugins.Click += new System.EventHandler(this.btnPlugins_Click);
			// 
			// txtVisible
			// 
			this.txtVisible.Location = new System.Drawing.Point(314, 62);
			this.txtVisible.Name = "txtVisible";
			this.txtVisible.ReadOnly = true;
			this.txtVisible.Size = new System.Drawing.Size(121, 20);
			this.txtVisible.TabIndex = 8;
			// 
			// chbPath
			// 
			this.chbPath.AutoSize = true;
			this.chbPath.Checked = true;
			this.chbPath.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chbPath.Location = new System.Drawing.Point(314, 37);
			this.chbPath.Name = "chbPath";
			this.chbPath.Size = new System.Drawing.Size(147, 17);
			this.chbPath.TabIndex = 7;
			this.chbPath.Text = "Get path on scriptCapture";
			this.chbPath.UseVisualStyleBackColor = true;
			// 
			// chbUpdate
			// 
			this.chbUpdate.AutoSize = true;
			this.chbUpdate.Location = new System.Drawing.Point(314, 11);
			this.chbUpdate.Name = "chbUpdate";
			this.chbUpdate.Size = new System.Drawing.Size(123, 17);
			this.chbUpdate.TabIndex = 6;
			this.chbUpdate.Text = "Atuomatically refresh";
			this.chbUpdate.UseVisualStyleBackColor = true;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(11, 65);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(31, 13);
			this.label3.TabIndex = 5;
			this.label3.Text = "Text:";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(11, 38);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(37, 13);
			this.label2.TabIndex = 4;
			this.label2.Text = "Value:";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(11, 12);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(38, 13);
			this.label1.TabIndex = 3;
			this.label1.Text = "Name:";
			// 
			// txtText
			// 
			this.txtText.Location = new System.Drawing.Point(51, 62);
			this.txtText.Name = "txtText";
			this.txtText.ReadOnly = true;
			this.txtText.Size = new System.Drawing.Size(253, 20);
			this.txtText.TabIndex = 2;
			// 
			// txtValue
			// 
			this.txtValue.Location = new System.Drawing.Point(51, 35);
			this.txtValue.Name = "txtValue";
			this.txtValue.ReadOnly = true;
			this.txtValue.Size = new System.Drawing.Size(253, 20);
			this.txtValue.TabIndex = 1;
			// 
			// txtName
			// 
			this.txtName.Location = new System.Drawing.Point(51, 9);
			this.txtName.Name = "txtName";
			this.txtName.ReadOnly = true;
			this.txtName.Size = new System.Drawing.Size(253, 20);
			this.txtName.TabIndex = 0;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(3, 103);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.tvNodes);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.tabPages);
			this.splitContainer1.Size = new System.Drawing.Size(768, 603);
			this.splitContainer1.SplitterDistance = 371;
			this.splitContainer1.TabIndex = 2;
			// 
			// tvNodes
			// 
			this.tvNodes.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tvNodes.HideSelection = false;
			this.tvNodes.Location = new System.Drawing.Point(0, 0);
			this.tvNodes.Name = "tvNodes";
			this.tvNodes.Size = new System.Drawing.Size(768, 371);
			this.tvNodes.TabIndex = 0;
			this.tvNodes.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.tvNodes_BeforeExpand);
			this.tvNodes.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvNodes_AfterSelect);
			this.tvNodes.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.tvNodes_NodeMouseClick);
			// 
			// tabPages
			// 
			this.tabPages.Alignment = System.Windows.Forms.TabAlignment.Bottom;
			this.tabPages.Controls.Add(this.tabCature);
			this.tabPages.Controls.Add(this.tabAutomat);
			this.tabPages.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabPages.Location = new System.Drawing.Point(0, 0);
			this.tabPages.Margin = new System.Windows.Forms.Padding(2);
			this.tabPages.Name = "tabPages";
			this.tabPages.SelectedIndex = 0;
			this.tabPages.Size = new System.Drawing.Size(768, 228);
			this.tabPages.TabIndex = 0;
			// 
			// tabCature
			// 
			this.tabCature.Controls.Add(this.tableLayoutPanel2);
			this.tabCature.Location = new System.Drawing.Point(4, 4);
			this.tabCature.Margin = new System.Windows.Forms.Padding(2);
			this.tabCature.Name = "tabCature";
			this.tabCature.Padding = new System.Windows.Forms.Padding(2);
			this.tabCature.Size = new System.Drawing.Size(760, 202);
			this.tabCature.TabIndex = 0;
			this.tabCature.Text = "Captures";
			this.tabCature.UseVisualStyleBackColor = true;
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.ColumnCount = 1;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Controls.Add(this.dataGridView1, 0, 2);
			this.tableLayoutPanel2.Controls.Add(this.txtScript, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this.pnlCommon, 0, 1);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(2, 2);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 4;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(756, 198);
			this.tableLayoutPanel2.TabIndex = 4;
			// 
			// dataGridView1
			// 
			this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.NameColumn,
            this.ValueColumn,
            this.colElapsed});
			this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dataGridView1.Location = new System.Drawing.Point(3, 105);
			this.dataGridView1.Name = "dataGridView1";
			this.dataGridView1.Size = new System.Drawing.Size(750, 69);
			this.dataGridView1.TabIndex = 0;
			// 
			// NameColumn
			// 
			this.NameColumn.HeaderText = "Name";
			this.NameColumn.Name = "NameColumn";
			this.NameColumn.ReadOnly = true;
			// 
			// ValueColumn
			// 
			this.ValueColumn.HeaderText = "Value";
			this.ValueColumn.Name = "ValueColumn";
			this.ValueColumn.ReadOnly = true;
			// 
			// colElapsed
			// 
			this.colElapsed.HeaderText = "Elapsed";
			this.colElapsed.Name = "colElapsed";
			this.colElapsed.ReadOnly = true;
			// 
			// txtScript
			// 
			this.txtScript.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtScript.Location = new System.Drawing.Point(3, 3);
			this.txtScript.Multiline = true;
			this.txtScript.Name = "txtScript";
			this.txtScript.Size = new System.Drawing.Size(750, 69);
			this.txtScript.TabIndex = 2;
			// 
			// pnlCommon
			// 
			this.pnlCommon.Controls.Add(this.pnlCompile);
			this.pnlCommon.Controls.Add(this.pnlSetting);
			this.pnlCommon.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlCommon.Location = new System.Drawing.Point(2, 77);
			this.pnlCommon.Margin = new System.Windows.Forms.Padding(2);
			this.pnlCommon.Name = "pnlCommon";
			this.pnlCommon.Size = new System.Drawing.Size(752, 23);
			this.pnlCommon.TabIndex = 3;
			// 
			// pnlCompile
			// 
			this.pnlCompile.Controls.Add(this.btnCompile);
			this.pnlCompile.Controls.Add(this.lblHandle);
			this.pnlCompile.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlCompile.Location = new System.Drawing.Point(0, 0);
			this.pnlCompile.Margin = new System.Windows.Forms.Padding(2);
			this.pnlCompile.Name = "pnlCompile";
			this.pnlCompile.Size = new System.Drawing.Size(590, 23);
			this.pnlCompile.TabIndex = 3;
			// 
			// btnCompile
			// 
			this.btnCompile.Location = new System.Drawing.Point(0, 0);
			this.btnCompile.Name = "btnCompile";
			this.btnCompile.Size = new System.Drawing.Size(75, 23);
			this.btnCompile.TabIndex = 3;
			this.btnCompile.Text = "Compile";
			this.btnCompile.UseVisualStyleBackColor = true;
			this.btnCompile.Click += new System.EventHandler(this.HandleCompileClicked);
			// 
			// lblHandle
			// 
			this.lblHandle.AutoSize = true;
			this.lblHandle.Location = new System.Drawing.Point(102, 3);
			this.lblHandle.Margin = new System.Windows.Forms.Padding(5, 8, 3, 0);
			this.lblHandle.Name = "lblHandle";
			this.lblHandle.Size = new System.Drawing.Size(16, 13);
			this.lblHandle.TabIndex = 4;
			this.lblHandle.Text = "...";
			this.lblHandle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// pnlSetting
			// 
			this.pnlSetting.Controls.Add(this.numericUpDown1);
			this.pnlSetting.Controls.Add(this.label4);
			this.pnlSetting.Dock = System.Windows.Forms.DockStyle.Right;
			this.pnlSetting.Location = new System.Drawing.Point(590, 0);
			this.pnlSetting.Margin = new System.Windows.Forms.Padding(2);
			this.pnlSetting.Name = "pnlSetting";
			this.pnlSetting.Size = new System.Drawing.Size(162, 23);
			this.pnlSetting.TabIndex = 2;
			// 
			// numericUpDown1
			// 
			this.numericUpDown1.Dock = System.Windows.Forms.DockStyle.Right;
			this.numericUpDown1.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
			this.numericUpDown1.Location = new System.Drawing.Point(112, 0);
			this.numericUpDown1.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
			this.numericUpDown1.Minimum = new decimal(new int[] {
            200,
            0,
            0,
            0});
			this.numericUpDown1.Name = "numericUpDown1";
			this.numericUpDown1.Size = new System.Drawing.Size(50, 20);
			this.numericUpDown1.TabIndex = 5;
			this.numericUpDown1.Value = new decimal(new int[] {
            200,
            0,
            0,
            0});
			this.numericUpDown1.ValueChanged += new System.EventHandler(this.HandleIntervalChanged);
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(4, 2);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(102, 14);
			this.label4.TabIndex = 4;
			this.label4.Text = "Update interval (ms)";
			// 
			// tabAutomat
			// 
			this.tabAutomat.Controls.Add(this.txtLog);
			this.tabAutomat.Location = new System.Drawing.Point(4, 4);
			this.tabAutomat.Margin = new System.Windows.Forms.Padding(2);
			this.tabAutomat.Name = "tabAutomat";
			this.tabAutomat.Padding = new System.Windows.Forms.Padding(2);
			this.tabAutomat.Size = new System.Drawing.Size(760, 202);
			this.tabAutomat.TabIndex = 1;
			this.tabAutomat.Text = "Automation Result";
			this.tabAutomat.UseVisualStyleBackColor = true;
			// 
			// txtLog
			// 
			this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtLog.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.txtLog.Location = new System.Drawing.Point(2, 2);
			this.txtLog.MaxLength = 327670;
			this.txtLog.Multiline = true;
			this.txtLog.Name = "txtLog";
			this.txtLog.ReadOnly = true;
			this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtLog.Size = new System.Drawing.Size(756, 198);
			this.txtLog.TabIndex = 2;
			this.txtLog.WordWrap = false;
			// 
			// generateQueryStringUsingToolStripMenuItem
			// 
			this.generateQueryStringUsingToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.nameToolStripMenuItem,
            this.textToolStripMenuItem,
            this.valueToolStripMenuItem,
            this.radionameToolStripMenuItem,
            this.radiovalueToolStripMenuItem});
			this.generateQueryStringUsingToolStripMenuItem.Name = "generateQueryStringUsingToolStripMenuItem";
			this.generateQueryStringUsingToolStripMenuItem.Size = new System.Drawing.Size(324, 22);
			this.generateQueryStringUsingToolStripMenuItem.Text = "Generate query string using";
			this.generateQueryStringUsingToolStripMenuItem.Click += new System.EventHandler(this.generateQueryStringUsingToolStripMenuItem_Click);
			// 
			// nameToolStripMenuItem
			// 
			this.nameToolStripMenuItem.Name = "nameToolStripMenuItem";
			this.nameToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
			this.nameToolStripMenuItem.Text = "name";
			this.nameToolStripMenuItem.Click += new System.EventHandler(this.generateQueryStringUsingToolStripMenuItem_Click);
			// 
			// textToolStripMenuItem
			// 
			this.textToolStripMenuItem.Name = "textToolStripMenuItem";
			this.textToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
			this.textToolStripMenuItem.Text = "text";
			this.textToolStripMenuItem.Click += new System.EventHandler(this.generateQueryStringUsingToolStripMenuItem_Click);
			// 
			// valueToolStripMenuItem
			// 
			this.valueToolStripMenuItem.Name = "valueToolStripMenuItem";
			this.valueToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
			this.valueToolStripMenuItem.Text = "value";
			this.valueToolStripMenuItem.Click += new System.EventHandler(this.generateQueryStringUsingToolStripMenuItem_Click);
			// 
			// timerUpdate
			// 
			this.timerUpdate.Interval = 1000;
			this.timerUpdate.Tick += new System.EventHandler(this.timerUpdate_Tick);
			// 
			// cmMenu
			// 
			this.cmMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.cmMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miRefresh,
            this.miSavePathToElement,
            this.miGenerateCode,
            this.miGenerateCodeStale,
            this.generateQueryStringUsingToolStripMenuItem});
			this.cmMenu.Name = "cmMenu";
			this.cmMenu.Size = new System.Drawing.Size(325, 136);
			this.cmMenu.Opening += new System.ComponentModel.CancelEventHandler(this.cmMenu_Opening);
			// 
			// miRefresh
			// 
			this.miRefresh.Name = "miRefresh";
			this.miRefresh.Size = new System.Drawing.Size(324, 22);
			this.miRefresh.Text = "Refresh element";
			this.miRefresh.Click += new System.EventHandler(this.miRefresh_Click);
			// 
			// miSavePathToElement
			// 
			this.miSavePathToElement.Name = "miSavePathToElement";
			this.miSavePathToElement.Size = new System.Drawing.Size(324, 22);
			this.miSavePathToElement.Text = "Save path to element...";
			this.miSavePathToElement.Click += new System.EventHandler(this.miSavePathToElement_Click);
			// 
			// miGenerateCode
			// 
			this.miGenerateCode.Name = "miGenerateCode";
			this.miGenerateCode.Size = new System.Drawing.Size(324, 22);
			this.miGenerateCode.Text = "Generate plugin for element...";
			this.miGenerateCode.Click += new System.EventHandler(this.miGenerateCode_Click);
			// 
			// miGenerateCodeStale
			// 
			this.miGenerateCodeStale.Name = "miGenerateCodeStale";
			this.miGenerateCodeStale.Size = new System.Drawing.Size(324, 22);
			this.miGenerateCodeStale.Text = "Generate plugin for element (using stale data)...";
			this.miGenerateCodeStale.Click += new System.EventHandler(this.miGenerateCodeStale_Click);
			// 
			// radionameToolStripMenuItem
			// 
			this.radionameToolStripMenuItem.Name = "radionameToolStripMenuItem";
			this.radionameToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
			this.radionameToolStripMenuItem.Text = "radioname";
			this.radionameToolStripMenuItem.Click += new System.EventHandler(this.generateQueryStringUsingToolStripMenuItem_Click);
			// 
			// radiovalueToolStripMenuItem
			// 
			this.radiovalueToolStripMenuItem.Name = "radiovalueToolStripMenuItem";
			this.radiovalueToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
			this.radiovalueToolStripMenuItem.Text = "radiovalue";
			this.radiovalueToolStripMenuItem.Click += new System.EventHandler(this.generateQueryStringUsingToolStripMenuItem_Click);
			// 
			// SpyForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(774, 709);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(2);
			this.Name = "SpyForm";
			this.Text = "JC Spy";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.jcmon_FormClosed);
			this.Load += new System.EventHandler(this.jcmon_Load);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbtOcr)).EndInit();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.tabPages.ResumeLayout(false);
			this.tabCature.ResumeLayout(false);
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
			this.pnlCommon.ResumeLayout(false);
			this.pnlCompile.ResumeLayout(false);
			this.pnlCompile.PerformLayout();
			this.pnlSetting.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
			this.tabAutomat.ResumeLayout(false);
			this.tabAutomat.PerformLayout();
			this.cmMenu.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private ToolStripMenuItem generateQueryStringUsingToolStripMenuItem;
		private ToolStripMenuItem nameToolStripMenuItem;
		private ToolStripMenuItem textToolStripMenuItem;
		private ToolStripMenuItem valueToolStripMenuItem;
		private TableLayoutPanel tableLayoutPanel1;
		private Panel panel1;
		private PictureBox pbtOcr;
		private Button btnPlugins;
		private TextBox txtVisible;
		private CheckBox chbPath;
		private CheckBox chbUpdate;
		private Label label3;
		private Label label2;
		private Label label1;
		private TextBox txtText;
		private TextBox txtValue;
		private TextBox txtName;
		private SplitContainer splitContainer1;
		private TreeView tvNodes;
		private TabControl tabPages;
		private TabPage tabCature;
		private TabPage tabAutomat;
		private TextBox txtLog;
		private TableLayoutPanel tableLayoutPanel2;
		private DataGridView dataGridView1;
		private DataGridViewTextBoxColumn NameColumn;
		private DataGridViewTextBoxColumn ValueColumn;
		private DataGridViewTextBoxColumn colElapsed;
		private TextBox txtScript;
		private Panel pnlCommon;
		private Panel pnlCompile;
		private Button btnCompile;
		private Label lblHandle;
		private Panel pnlSetting;
		private NumericUpDown numericUpDown1;
		private Label label4;
		private ToolStripMenuItem radionameToolStripMenuItem;
		private ToolStripMenuItem radiovalueToolStripMenuItem;
	}
}