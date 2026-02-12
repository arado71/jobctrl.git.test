using System.Windows.Forms;

namespace Tct.ActivityRecorderClient.View.Controls
{
	partial class StatGrid
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
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.lblBaseRowTitle = new System.Windows.Forms.Label();
			this.smallSplitter1 = new Tct.ActivityRecorderClient.View.Controls.SmallSplitter();
			this.smallSplitter2 = new Tct.ActivityRecorderClient.View.Controls.SmallSplitter();
			this.lblExtraRowDelta = new System.Windows.Forms.Label();
			this.lblBaseRowDelta = new System.Windows.Forms.Label();
			this.lblDayTotal = new System.Windows.Forms.Label();
			this.lblExtraTotal = new System.Windows.Forms.Label();
			this.lblBaseRowTotal = new System.Windows.Forms.Label();
			this.lblDayUsed = new System.Windows.Forms.Label();
			this.lblExtraRowUsed = new System.Windows.Forms.Label();
			this.lblBaseRowUsed = new System.Windows.Forms.Label();
			this.lblDayTitle = new System.Windows.Forms.Label();
			this.lblExtraRowTitle = new System.Windows.Forms.Label();
			this.pStatsBtn = new System.Windows.Forms.Panel();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.AutoSize = true;
			this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel1.ColumnCount = 4;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this.lblBaseRowTitle, 0, 4);
			this.tableLayoutPanel1.Controls.Add(this.smallSplitter1, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.smallSplitter2, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.lblExtraRowDelta, 3, 2);
			this.tableLayoutPanel1.Controls.Add(this.lblBaseRowDelta, 3, 4);
			this.tableLayoutPanel1.Controls.Add(this.lblDayTotal, 2, 0);
			this.tableLayoutPanel1.Controls.Add(this.lblExtraTotal, 2, 2);
			this.tableLayoutPanel1.Controls.Add(this.lblBaseRowTotal, 2, 4);
			this.tableLayoutPanel1.Controls.Add(this.lblDayUsed, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.lblExtraRowUsed, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.lblBaseRowUsed, 1, 4);
			this.tableLayoutPanel1.Controls.Add(this.lblDayTitle, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.lblExtraRowTitle, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.pStatsBtn, 3, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 5;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(443, 74);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// lblBaseRowTitle
			// 
			this.lblBaseRowTitle.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblBaseRowTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.lblBaseRowTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(77)))), ((int)(((byte)(77)))), ((int)(((byte)(77)))));
			this.lblBaseRowTitle.Location = new System.Drawing.Point(3, 48);
			this.lblBaseRowTitle.Name = "lblBaseRowTitle";
			this.lblBaseRowTitle.Size = new System.Drawing.Size(304, 26);
			this.lblBaseRowTitle.TabIndex = 16;
			this.lblBaseRowTitle.Text = "Weekly worktime";
			this.lblBaseRowTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// smallSplitter1
			// 
			this.smallSplitter1.Accent = false;
			this.tableLayoutPanel1.SetColumnSpan(this.smallSplitter1, 4);
			this.smallSplitter1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.smallSplitter1.Location = new System.Drawing.Point(0, 21);
			this.smallSplitter1.Margin = new System.Windows.Forms.Padding(0);
			this.smallSplitter1.MinimumSize = new System.Drawing.Size(0, 3);
			this.smallSplitter1.Name = "smallSplitter1";
			this.smallSplitter1.Padding = new System.Windows.Forms.Padding(3, 1, 3, 1);
			this.smallSplitter1.Size = new System.Drawing.Size(443, 3);
			this.smallSplitter1.TabIndex = 1;
			// 
			// smallSplitter2
			// 
			this.smallSplitter2.Accent = false;
			this.tableLayoutPanel1.SetColumnSpan(this.smallSplitter2, 4);
			this.smallSplitter2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.smallSplitter2.Location = new System.Drawing.Point(0, 45);
			this.smallSplitter2.Margin = new System.Windows.Forms.Padding(0);
			this.smallSplitter2.MinimumSize = new System.Drawing.Size(0, 3);
			this.smallSplitter2.Name = "smallSplitter2";
			this.smallSplitter2.Padding = new System.Windows.Forms.Padding(3, 1, 3, 1);
			this.smallSplitter2.Size = new System.Drawing.Size(443, 3);
			this.smallSplitter2.TabIndex = 4;
			// 
			// lblExtraRowDelta
			// 
			this.lblExtraRowDelta.AutoSize = true;
			this.lblExtraRowDelta.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblExtraRowDelta.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
			this.lblExtraRowDelta.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(77)))), ((int)(((byte)(77)))), ((int)(((byte)(77)))));
			this.lblExtraRowDelta.Location = new System.Drawing.Point(389, 24);
			this.lblExtraRowDelta.Name = "lblExtraRowDelta";
			this.lblExtraRowDelta.Size = new System.Drawing.Size(51, 21);
			this.lblExtraRowDelta.TabIndex = 6;
			this.lblExtraRowDelta.Text = "∆ 04:21";
			this.lblExtraRowDelta.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblBaseRowDelta
			// 
			this.lblBaseRowDelta.AutoSize = true;
			this.lblBaseRowDelta.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblBaseRowDelta.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
			this.lblBaseRowDelta.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(77)))), ((int)(((byte)(77)))), ((int)(((byte)(77)))));
			this.lblBaseRowDelta.Location = new System.Drawing.Point(389, 48);
			this.lblBaseRowDelta.Name = "lblBaseRowDelta";
			this.lblBaseRowDelta.Size = new System.Drawing.Size(51, 26);
			this.lblBaseRowDelta.TabIndex = 7;
			this.lblBaseRowDelta.Text = "∆ 04:21";
			this.lblBaseRowDelta.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblDayTotal
			// 
			this.lblDayTotal.AutoSize = true;
			this.lblDayTotal.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblDayTotal.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.lblDayTotal.ForeColor = System.Drawing.Color.Silver;
			this.lblDayTotal.Location = new System.Drawing.Point(351, 0);
			this.lblDayTotal.Name = "lblDayTotal";
			this.lblDayTotal.Size = new System.Drawing.Size(32, 21);
			this.lblDayTotal.TabIndex = 8;
			this.lblDayTotal.Text = "3:39";
			this.lblDayTotal.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblExtraTotal
			// 
			this.lblExtraTotal.AutoSize = true;
			this.lblExtraTotal.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblExtraTotal.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.lblExtraTotal.ForeColor = System.Drawing.Color.Silver;
			this.lblExtraTotal.Location = new System.Drawing.Point(351, 24);
			this.lblExtraTotal.Name = "lblExtraTotal";
			this.lblExtraTotal.Size = new System.Drawing.Size(32, 21);
			this.lblExtraTotal.TabIndex = 9;
			this.lblExtraTotal.Text = "3:39";
			this.lblExtraTotal.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblBaseRowTotal
			// 
			this.lblBaseRowTotal.AutoSize = true;
			this.lblBaseRowTotal.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblBaseRowTotal.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.lblBaseRowTotal.ForeColor = System.Drawing.Color.Silver;
			this.lblBaseRowTotal.Location = new System.Drawing.Point(351, 48);
			this.lblBaseRowTotal.Name = "lblBaseRowTotal";
			this.lblBaseRowTotal.Size = new System.Drawing.Size(32, 26);
			this.lblBaseRowTotal.TabIndex = 10;
			this.lblBaseRowTotal.Text = "3:39";
			this.lblBaseRowTotal.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblDayUsed
			// 
			this.lblDayUsed.AutoSize = true;
			this.lblDayUsed.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblDayUsed.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.lblDayUsed.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(77)))), ((int)(((byte)(77)))), ((int)(((byte)(77)))));
			this.lblDayUsed.Location = new System.Drawing.Point(313, 0);
			this.lblDayUsed.Name = "lblDayUsed";
			this.lblDayUsed.Size = new System.Drawing.Size(32, 21);
			this.lblDayUsed.TabIndex = 11;
			this.lblDayUsed.Text = "3:39";
			this.lblDayUsed.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblExtraRowUsed
			// 
			this.lblExtraRowUsed.AutoSize = true;
			this.lblExtraRowUsed.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblExtraRowUsed.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.lblExtraRowUsed.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(77)))), ((int)(((byte)(77)))), ((int)(((byte)(77)))));
			this.lblExtraRowUsed.Location = new System.Drawing.Point(313, 24);
			this.lblExtraRowUsed.Name = "lblExtraRowUsed";
			this.lblExtraRowUsed.Size = new System.Drawing.Size(32, 21);
			this.lblExtraRowUsed.TabIndex = 12;
			this.lblExtraRowUsed.Text = "3:39";
			this.lblExtraRowUsed.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblBaseRowUsed
			// 
			this.lblBaseRowUsed.AutoSize = true;
			this.lblBaseRowUsed.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblBaseRowUsed.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.lblBaseRowUsed.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(77)))), ((int)(((byte)(77)))), ((int)(((byte)(77)))));
			this.lblBaseRowUsed.Location = new System.Drawing.Point(313, 48);
			this.lblBaseRowUsed.Name = "lblBaseRowUsed";
			this.lblBaseRowUsed.Size = new System.Drawing.Size(32, 26);
			this.lblBaseRowUsed.TabIndex = 13;
			this.lblBaseRowUsed.Text = "3:39";
			this.lblBaseRowUsed.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblDayTitle
			// 
			this.lblDayTitle.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblDayTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.lblDayTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(77)))), ((int)(((byte)(77)))), ((int)(((byte)(77)))));
			this.lblDayTitle.Location = new System.Drawing.Point(3, 0);
			this.lblDayTitle.Name = "lblDayTitle";
			this.lblDayTitle.Size = new System.Drawing.Size(304, 21);
			this.lblDayTitle.TabIndex = 14;
			this.lblDayTitle.Text = "Weekly worktime";
			this.lblDayTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblExtraRowTitle
			// 
			this.lblExtraRowTitle.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblExtraRowTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.lblExtraRowTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(77)))), ((int)(((byte)(77)))), ((int)(((byte)(77)))));
			this.lblExtraRowTitle.Location = new System.Drawing.Point(3, 24);
			this.lblExtraRowTitle.Name = "lblExtraRowTitle";
			this.lblExtraRowTitle.Size = new System.Drawing.Size(304, 21);
			this.lblExtraRowTitle.TabIndex = 15;
			this.lblExtraRowTitle.Text = "Weekly worktime";
			this.lblExtraRowTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// pStatsBtn
			// 
			this.pStatsBtn.BackgroundImage = global::Tct.ActivityRecorderClient.Properties.Resources.stats;
			this.pStatsBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.pStatsBtn.Cursor = System.Windows.Forms.Cursors.Hand;
			this.pStatsBtn.Dock = System.Windows.Forms.DockStyle.Right;
			this.pStatsBtn.Location = new System.Drawing.Point(423, 0);
			this.pStatsBtn.Margin = new System.Windows.Forms.Padding(0);
			this.pStatsBtn.Name = "pStatsBtn";
			this.pStatsBtn.Size = new System.Drawing.Size(20, 21);
			this.pStatsBtn.TabIndex = 17;
			this.pStatsBtn.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HandleStatsClicked);
			// 
			// StatGrid
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Margin = new System.Windows.Forms.Padding(0);
			this.Name = "StatGrid";
			this.Size = new System.Drawing.Size(443, 74);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private SmallSplitter smallSplitter1;
		private SmallSplitter smallSplitter2;
		private System.Windows.Forms.Label lblExtraRowDelta;
		private System.Windows.Forms.Label lblBaseRowDelta;
		private System.Windows.Forms.Label lblDayTotal;
		private System.Windows.Forms.Label lblExtraTotal;
		private System.Windows.Forms.Label lblBaseRowTotal;
		private System.Windows.Forms.Label lblDayUsed;
		private System.Windows.Forms.Label lblExtraRowUsed;
		private System.Windows.Forms.Label lblBaseRowUsed;
		private System.Windows.Forms.Label lblBaseRowTitle;
		private System.Windows.Forms.Label lblDayTitle;
		private System.Windows.Forms.Label lblExtraRowTitle;
		private System.Windows.Forms.Panel pStatsBtn;
		private ToolTip toolTip1;

	}
}
