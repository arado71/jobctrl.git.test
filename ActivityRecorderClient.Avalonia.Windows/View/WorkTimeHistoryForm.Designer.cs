using Tct.ActivityRecorderClient.View.Controls;

namespace Tct.ActivityRecorderClient.View
{
	partial class WorkTimeHistoryForm
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
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.chart = new Tct.ActivityRecorderClient.View.Controls.WorkTimeChart();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.lblTotalValue = new MetroFramework.Controls.MetroLabel();
			this.lblStart = new MetroFramework.Controls.MetroLabel();
			this.lblTotal = new MetroFramework.Controls.MetroLabel();
			this.lblStartValue = new MetroFramework.Controls.MetroLabel();
			this.lblEnd = new MetroFramework.Controls.MetroLabel();
			this.lblEndValue = new MetroFramework.Controls.MetroLabel();
			this.lblDuration = new MetroFramework.Controls.MetroLabel();
			this.lblDurationValue = new MetroFramework.Controls.MetroLabel();
			this.cbShowDeleted = new MetroFramework.Controls.MetroCheckBox();
			this.lblGrouping = new MetroFramework.Controls.MetroLabel();
			this.cbGroupBy = new MetroFramework.Controls.MetroComboBox();
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.btnPrev = new MetroFramework.Controls.MetroButton();
			this.dtpDay = new System.Windows.Forms.DateTimePicker();
			this.btnNext = new MetroFramework.Controls.MetroButton();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.chart)).BeginInit();
			this.tableLayoutPanel2.SuspendLayout();
			this.flowLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.chart, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(20, 60);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(720, 370);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// chart
			// 
			this.chart.Dock = System.Windows.Forms.DockStyle.Fill;
			this.chart.GroupByWork = false;
			this.chart.IsDeletedVisible = false;
			this.chart.Location = new System.Drawing.Point(3, 92);
			this.chart.Name = "chart";
			this.chart.Size = new System.Drawing.Size(714, 275);
			this.chart.TabIndex = 1;
			this.chart.Text = "chart1";
			this.chart.Visible = false;
			this.chart.SelectionRangeChanging += new System.EventHandler<System.Windows.Forms.DataVisualization.Charting.CursorEventArgs>(this.HandleSelectionRangeChanging);
			this.chart.SelectionRangeChanged += new System.EventHandler<System.Windows.Forms.DataVisualization.Charting.CursorEventArgs>(this.HandleSelectionRangeChanged);
			this.chart.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HandleChartMouseClick);
			this.chart.MouseMove += new System.Windows.Forms.MouseEventHandler(this.HandleChartMouseMove);
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.AutoSize = true;
			this.tableLayoutPanel2.ColumnCount = 7;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel2.Controls.Add(this.lblTotalValue, 0, 1);
			this.tableLayoutPanel2.Controls.Add(this.lblStart, 1, 0);
			this.tableLayoutPanel2.Controls.Add(this.lblTotal, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this.lblStartValue, 1, 1);
			this.tableLayoutPanel2.Controls.Add(this.lblEnd, 2, 0);
			this.tableLayoutPanel2.Controls.Add(this.lblEndValue, 2, 1);
			this.tableLayoutPanel2.Controls.Add(this.lblDuration, 3, 0);
			this.tableLayoutPanel2.Controls.Add(this.lblDurationValue, 3, 1);
			this.tableLayoutPanel2.Controls.Add(this.cbShowDeleted, 5, 1);
			this.tableLayoutPanel2.Controls.Add(this.lblGrouping, 4, 0);
			this.tableLayoutPanel2.Controls.Add(this.cbGroupBy, 4, 1);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 36);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 2;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.Size = new System.Drawing.Size(714, 50);
			this.tableLayoutPanel2.TabIndex = 3;
			// 
			// lblTotalValue
			// 
			this.lblTotalValue.AutoSize = true;
			this.lblTotalValue.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblTotalValue.Location = new System.Drawing.Point(3, 19);
			this.lblTotalValue.Name = "lblTotalValue";
			this.lblTotalValue.Size = new System.Drawing.Size(43, 31);
			this.lblTotalValue.TabIndex = 3;
			this.lblTotalValue.Text = "...";
			// 
			// lblStart
			// 
			this.lblStart.AutoSize = true;
			this.lblStart.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblStart.FontWeight = MetroFramework.MetroLabelWeight.Bold;
			this.lblStart.Location = new System.Drawing.Point(52, 0);
			this.lblStart.Name = "lblStart";
			this.lblStart.Size = new System.Drawing.Size(41, 19);
			this.lblStart.TabIndex = 4;
			this.lblStart.Text = "Start";
			// 
			// lblTotal
			// 
			this.lblTotal.AutoSize = true;
			this.lblTotal.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblTotal.FontWeight = MetroFramework.MetroLabelWeight.Bold;
			this.lblTotal.Location = new System.Drawing.Point(3, 0);
			this.lblTotal.Name = "lblTotal";
			this.lblTotal.Size = new System.Drawing.Size(43, 19);
			this.lblTotal.TabIndex = 5;
			this.lblTotal.Text = "Total";
			// 
			// lblStartValue
			// 
			this.lblStartValue.AutoSize = true;
			this.lblStartValue.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblStartValue.Location = new System.Drawing.Point(52, 19);
			this.lblStartValue.Name = "lblStartValue";
			this.lblStartValue.Size = new System.Drawing.Size(41, 31);
			this.lblStartValue.TabIndex = 6;
			this.lblStartValue.Text = "...";
			// 
			// lblEnd
			// 
			this.lblEnd.AutoSize = true;
			this.lblEnd.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblEnd.FontWeight = MetroFramework.MetroLabelWeight.Bold;
			this.lblEnd.Location = new System.Drawing.Point(99, 0);
			this.lblEnd.Name = "lblEnd";
			this.lblEnd.Size = new System.Drawing.Size(33, 19);
			this.lblEnd.TabIndex = 7;
			this.lblEnd.Text = "End";
			// 
			// lblEndValue
			// 
			this.lblEndValue.AutoSize = true;
			this.lblEndValue.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblEndValue.Location = new System.Drawing.Point(99, 19);
			this.lblEndValue.Name = "lblEndValue";
			this.lblEndValue.Size = new System.Drawing.Size(33, 31);
			this.lblEndValue.TabIndex = 8;
			this.lblEndValue.Text = "...";
			// 
			// lblDuration
			// 
			this.lblDuration.AutoSize = true;
			this.lblDuration.FontWeight = MetroFramework.MetroLabelWeight.Bold;
			this.lblDuration.Location = new System.Drawing.Point(138, 0);
			this.lblDuration.Name = "lblDuration";
			this.lblDuration.Size = new System.Drawing.Size(33, 19);
			this.lblDuration.TabIndex = 9;
			this.lblDuration.Text = "Dur";
			// 
			// lblDurationValue
			// 
			this.lblDurationValue.AutoSize = true;
			this.lblDurationValue.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblDurationValue.Location = new System.Drawing.Point(138, 19);
			this.lblDurationValue.Name = "lblDurationValue";
			this.lblDurationValue.Size = new System.Drawing.Size(33, 31);
			this.lblDurationValue.TabIndex = 10;
			this.lblDurationValue.Text = "...";
			// 
			// cbShowDeleted
			// 
			this.cbShowDeleted.AutoSize = true;
			this.cbShowDeleted.Dock = System.Windows.Forms.DockStyle.Left;
			this.cbShowDeleted.FontSize = MetroFramework.MetroCheckBoxSize.Medium;
			this.cbShowDeleted.Location = new System.Drawing.Point(308, 22);
			this.cbShowDeleted.Name = "cbShowDeleted";
			this.cbShowDeleted.Size = new System.Drawing.Size(105, 25);
			this.cbShowDeleted.TabIndex = 11;
			this.cbShowDeleted.Text = "ShowDeleted";
			this.cbShowDeleted.UseSelectable = true;
			this.cbShowDeleted.CheckedChanged += new System.EventHandler(this.HandleShowDeletedChanged);
			// 
			// lblGrouping
			// 
			this.lblGrouping.AutoSize = true;
			this.lblGrouping.FontWeight = MetroFramework.MetroLabelWeight.Bold;
			this.lblGrouping.Location = new System.Drawing.Point(177, 0);
			this.lblGrouping.Name = "lblGrouping";
			this.lblGrouping.Size = new System.Drawing.Size(51, 19);
			this.lblGrouping.TabIndex = 12;
			this.lblGrouping.Text = "Group";
			// 
			// cbGroupBy
			// 
			this.cbGroupBy.FontSize = MetroFramework.MetroComboBoxSize.Small;
			this.cbGroupBy.FormattingEnabled = true;
			this.cbGroupBy.ItemHeight = 19;
			this.cbGroupBy.Location = new System.Drawing.Point(177, 22);
			this.cbGroupBy.Name = "cbGroupBy";
			this.cbGroupBy.Size = new System.Drawing.Size(125, 25);
			this.cbGroupBy.TabIndex = 13;
			this.cbGroupBy.UseSelectable = true;
			this.cbGroupBy.SelectedIndexChanged += new System.EventHandler(this.HandleGroupByChanged);
			// 
			// flowLayoutPanel1
			// 
			this.flowLayoutPanel1.Controls.Add(this.btnPrev);
			this.flowLayoutPanel1.Controls.Add(this.dtpDay);
			this.flowLayoutPanel1.Controls.Add(this.btnNext);
			this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 3);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new System.Drawing.Size(714, 27);
			this.flowLayoutPanel1.TabIndex = 4;
			// 
			// btnPrev
			// 
			this.btnPrev.Location = new System.Drawing.Point(3, 3);
			this.btnPrev.Name = "btnPrev";
			this.btnPrev.Size = new System.Drawing.Size(75, 20);
			this.btnPrev.TabIndex = 0;
			this.btnPrev.Text = "<";
			this.btnPrev.UseSelectable = true;
			this.btnPrev.Click += new System.EventHandler(this.btnPrev_Click);
			// 
			// dtpDay
			// 
			this.dtpDay.Location = new System.Drawing.Point(84, 3);
			this.dtpDay.Name = "dtpDay";
			this.dtpDay.Size = new System.Drawing.Size(200, 20);
			this.dtpDay.TabIndex = 2;
			this.dtpDay.CloseUp += new System.EventHandler(this.HandlePickerClosedUp);
			this.dtpDay.ValueChanged += new System.EventHandler(this.HandleDateChanged);
			this.dtpDay.DropDown += new System.EventHandler(this.HandlePickerDropDown);
			// 
			// btnNext
			// 
			this.btnNext.Location = new System.Drawing.Point(290, 3);
			this.btnNext.Name = "btnNext";
			this.btnNext.Size = new System.Drawing.Size(75, 20);
			this.btnNext.TabIndex = 3;
			this.btnNext.Text = ">";
			this.btnNext.UseSelectable = true;
			this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
			// 
			// WorkTimeHistoryForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(760, 450);
			this.Controls.Add(this.tableLayoutPanel1);
			this.MinimumSize = new System.Drawing.Size(600, 350);
			this.Name = "WorkTimeHistoryForm";
			this.Text = "WorkStats";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.chart)).EndInit();
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			this.flowLayoutPanel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private WorkTimeChart chart;
		private System.Windows.Forms.DateTimePicker dtpDay;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
		private MetroFramework.Controls.MetroLabel lblTotalValue;
		private MetroFramework.Controls.MetroLabel lblStart;
		private MetroFramework.Controls.MetroLabel lblTotal;
		private MetroFramework.Controls.MetroLabel lblStartValue;
		private MetroFramework.Controls.MetroLabel lblEnd;
		private MetroFramework.Controls.MetroLabel lblEndValue;
		private MetroFramework.Controls.MetroLabel lblDuration;
		private MetroFramework.Controls.MetroLabel lblDurationValue;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		private MetroFramework.Controls.MetroButton btnPrev;
		private MetroFramework.Controls.MetroButton btnNext;
		private MetroFramework.Controls.MetroCheckBox cbShowDeleted;
		private MetroFramework.Controls.MetroLabel lblGrouping;
		private MetroFramework.Controls.MetroComboBox cbGroupBy;
	}
}