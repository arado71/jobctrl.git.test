namespace Tct.ActivityRecorderClient.View
{
	partial class OfflineWorkForm
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
			this.tlpMeetings = new System.Windows.Forms.TableLayoutPanel();
			this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
			this.flpIntervalButtons = new System.Windows.Forms.FlowLayoutPanel();
			this.pbMerge = new System.Windows.Forms.PictureBox();
			this.pbSplit = new System.Windows.Forms.PictureBox();
			this.pnlMeetingBlock = new System.Windows.Forms.Panel();
			this.pnlMeetingView = new System.Windows.Forms.Panel();
			this.scrMeetings = new Tct.ActivityRecorderClient.View.Controls.ScrollBar();
			this.pnlTotalBlock = new System.Windows.Forms.Panel();
			this.sspTotalBar = new Tct.ActivityRecorderClient.View.Controls.SmallSplitter();
			this.lblTotal = new System.Windows.Forms.Label();
			this.pnlControlBar = new System.Windows.Forms.Panel();
			this.pbCounter = new System.Windows.Forms.PictureBox();
			this.flpBottomButtons = new System.Windows.Forms.FlowLayoutPanel();
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnOk = new System.Windows.Forms.Button();
			this.lblSum = new System.Windows.Forms.Label();
			this.timeSplitter = new Tct.ActivityRecorderClient.View.TimeSplitterControl();
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.tlpMain.SuspendLayout();
			this.flpIntervalButtons.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbMerge)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pbSplit)).BeginInit();
			this.pnlMeetingBlock.SuspendLayout();
			this.pnlMeetingView.SuspendLayout();
			this.pnlTotalBlock.SuspendLayout();
			this.pnlControlBar.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbCounter)).BeginInit();
			this.flpBottomButtons.SuspendLayout();
			this.SuspendLayout();
			// 
			// tlpMeetings
			// 
			this.tlpMeetings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tlpMeetings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tlpMeetings.Location = new System.Drawing.Point(0, 0);
			this.tlpMeetings.Name = "tlpMeetings";
			this.tlpMeetings.RowCount = 1;
			this.tlpMeetings.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
			this.tlpMeetings.Size = new System.Drawing.Size(621, 50);
			this.tlpMeetings.TabIndex = 0;
			// 
			// tlpMain
			// 
			this.tlpMain.ColumnCount = 1;
			this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tlpMain.Controls.Add(this.flpIntervalButtons, 0, 0);
			this.tlpMain.Controls.Add(this.pnlMeetingBlock, 0, 2);
			this.tlpMain.Controls.Add(this.pnlTotalBlock, 0, 3);
			this.tlpMain.Controls.Add(this.pnlControlBar, 0, 4);
			this.tlpMain.Controls.Add(this.timeSplitter, 0, 1);
			this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tlpMain.Location = new System.Drawing.Point(20, 60);
			this.tlpMain.Name = "tlpMain";
			this.tlpMain.RowCount = 5;
			this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tlpMain.Size = new System.Drawing.Size(628, 311);
			this.tlpMain.TabIndex = 1;
			// 
			// flpIntervalButtons
			// 
			this.flpIntervalButtons.Controls.Add(this.pbMerge);
			this.flpIntervalButtons.Controls.Add(this.pbSplit);
			this.flpIntervalButtons.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flpIntervalButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
			this.flpIntervalButtons.Location = new System.Drawing.Point(0, 0);
			this.flpIntervalButtons.Margin = new System.Windows.Forms.Padding(0);
			this.flpIntervalButtons.Name = "flpIntervalButtons";
			this.flpIntervalButtons.Size = new System.Drawing.Size(628, 20);
			this.flpIntervalButtons.TabIndex = 2;
			// 
			// pbMerge
			// 
			this.pbMerge.Image = global::Tct.ActivityRecorderClient.Properties.Resources.merge;
			this.pbMerge.Location = new System.Drawing.Point(605, 0);
			this.pbMerge.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
			this.pbMerge.Name = "pbMerge";
			this.pbMerge.Size = new System.Drawing.Size(20, 20);
			this.pbMerge.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.pbMerge.TabIndex = 1;
			this.pbMerge.TabStop = false;
			this.pbMerge.Click += new System.EventHandler(this.pbMerge_Click);
			this.pbMerge.MouseEnter += new System.EventHandler(this.pbMerge_MouseEnter);
			this.pbMerge.MouseLeave += new System.EventHandler(this.pbMerge_MouseLeave);
			// 
			// pbSplit
			// 
			this.pbSplit.Image = global::Tct.ActivityRecorderClient.Properties.Resources.split;
			this.pbSplit.Location = new System.Drawing.Point(579, 0);
			this.pbSplit.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
			this.pbSplit.Name = "pbSplit";
			this.pbSplit.Size = new System.Drawing.Size(20, 20);
			this.pbSplit.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.pbSplit.TabIndex = 0;
			this.pbSplit.TabStop = false;
			this.pbSplit.Click += new System.EventHandler(this.pbSplit_Click);
			this.pbSplit.MouseEnter += new System.EventHandler(this.pbSplit_MouseEnter);
			this.pbSplit.MouseLeave += new System.EventHandler(this.pbSplit_MouseLeave);
			// 
			// pnlMeetingBlock
			// 
			this.pnlMeetingBlock.Controls.Add(this.pnlMeetingView);
			this.pnlMeetingBlock.Controls.Add(this.scrMeetings);
			this.pnlMeetingBlock.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlMeetingBlock.Location = new System.Drawing.Point(0, 86);
			this.pnlMeetingBlock.Margin = new System.Windows.Forms.Padding(0);
			this.pnlMeetingBlock.Name = "pnlMeetingBlock";
			this.pnlMeetingBlock.Size = new System.Drawing.Size(628, 153);
			this.pnlMeetingBlock.TabIndex = 0;
			// 
			// pnlMeetingView
			// 
			this.pnlMeetingView.Controls.Add(this.tlpMeetings);
			this.pnlMeetingView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlMeetingView.Location = new System.Drawing.Point(0, 0);
			this.pnlMeetingView.Margin = new System.Windows.Forms.Padding(0);
			this.pnlMeetingView.Name = "pnlMeetingView";
			this.pnlMeetingView.Size = new System.Drawing.Size(621, 153);
			this.pnlMeetingView.TabIndex = 1;
			// 
			// scrMeetings
			// 
			this.scrMeetings.Dock = System.Windows.Forms.DockStyle.Right;
			this.scrMeetings.Location = new System.Drawing.Point(621, 0);
			this.scrMeetings.Name = "scrMeetings";
			this.scrMeetings.ScrollSpeed = 5F;
			this.scrMeetings.ScrollTotalSize = 100;
			this.scrMeetings.ScrollVisibleSize = 10;
			this.scrMeetings.Size = new System.Drawing.Size(7, 153);
			this.scrMeetings.TabIndex = 0;
			this.scrMeetings.Value = 0;
			this.scrMeetings.ScrollChanged += new System.EventHandler(this.HandleScrolled);
			// 
			// pnlTotalBlock
			// 
			this.pnlTotalBlock.Controls.Add(this.sspTotalBar);
			this.pnlTotalBlock.Controls.Add(this.lblTotal);
			this.pnlTotalBlock.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlTotalBlock.Location = new System.Drawing.Point(3, 242);
			this.pnlTotalBlock.Name = "pnlTotalBlock";
			this.pnlTotalBlock.Size = new System.Drawing.Size(622, 13);
			this.pnlTotalBlock.TabIndex = 1;
			// 
			// sspTotalBar
			// 
			this.sspTotalBar.Accent = false;
			this.sspTotalBar.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.sspTotalBar.Location = new System.Drawing.Point(34, 8);
			this.sspTotalBar.MinimumSize = new System.Drawing.Size(0, 3);
			this.sspTotalBar.Name = "sspTotalBar";
			this.sspTotalBar.Padding = new System.Windows.Forms.Padding(3, 1, 3, 1);
			this.sspTotalBar.Size = new System.Drawing.Size(588, 5);
			this.sspTotalBar.TabIndex = 0;
			// 
			// lblTotal
			// 
			this.lblTotal.AutoSize = true;
			this.lblTotal.Dock = System.Windows.Forms.DockStyle.Left;
			this.lblTotal.Location = new System.Drawing.Point(0, 0);
			this.lblTotal.Margin = new System.Windows.Forms.Padding(3);
			this.lblTotal.Name = "lblTotal";
			this.lblTotal.Size = new System.Drawing.Size(34, 13);
			this.lblTotal.TabIndex = 0;
			this.lblTotal.Text = "Total:";
			this.lblTotal.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// pnlControlBar
			// 
			this.pnlControlBar.Controls.Add(this.pbCounter);
			this.pnlControlBar.Controls.Add(this.flpBottomButtons);
			this.pnlControlBar.Controls.Add(this.lblSum);
			this.pnlControlBar.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlControlBar.Location = new System.Drawing.Point(3, 261);
			this.pnlControlBar.Name = "pnlControlBar";
			this.pnlControlBar.Size = new System.Drawing.Size(622, 47);
			this.pnlControlBar.TabIndex = 2;
			// 
			// pbCounter
			// 
			this.pbCounter.Dock = System.Windows.Forms.DockStyle.Left;
			this.pbCounter.Image = global::Tct.ActivityRecorderClient.Properties.Resources.timer;
			this.pbCounter.Location = new System.Drawing.Point(176, 0);
			this.pbCounter.Margin = new System.Windows.Forms.Padding(0);
			this.pbCounter.MaximumSize = new System.Drawing.Size(32, 44);
			this.pbCounter.Name = "pbCounter";
			this.pbCounter.Size = new System.Drawing.Size(32, 44);
			this.pbCounter.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.pbCounter.TabIndex = 10;
			this.pbCounter.TabStop = false;
			this.pbCounter.Visible = false;
			// 
			// flpBottomButtons
			// 
			this.flpBottomButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.flpBottomButtons.Controls.Add(this.btnCancel);
			this.flpBottomButtons.Controls.Add(this.btnOk);
			this.flpBottomButtons.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flpBottomButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
			this.flpBottomButtons.Location = new System.Drawing.Point(176, 0);
			this.flpBottomButtons.Margin = new System.Windows.Forms.Padding(0);
			this.flpBottomButtons.Name = "flpBottomButtons";
			this.flpBottomButtons.Size = new System.Drawing.Size(446, 47);
			this.flpBottomButtons.TabIndex = 10;
			this.flpBottomButtons.WrapContents = false;
			// 
			// btnCancel
			// 
			this.btnCancel.AutoSize = true;
			this.btnCancel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.FlatAppearance.BorderSize = 0;
			this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.btnCancel.Image = global::Tct.ActivityRecorderClient.Properties.Resources.btn_delete;
			this.btnCancel.Location = new System.Drawing.Point(361, 3);
			this.btnCancel.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(82, 38);
			this.btnCancel.TabIndex = 9;
			this.btnCancel.Text = "Clear";
			this.btnCancel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.btnCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// btnOk
			// 
			this.btnOk.AutoSize = true;
			this.btnOk.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.btnOk.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(156)))), ((int)(((byte)(221)))));
			this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOk.Enabled = false;
			this.btnOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnOk.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.btnOk.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(251)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
			this.btnOk.Image = global::Tct.ActivityRecorderClient.Properties.Resources.btn_ok;
			this.btnOk.Location = new System.Drawing.Point(132, 3);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(216, 40);
			this.btnOk.TabIndex = 8;
			this.btnOk.Text = "Record offline worktime";
			this.btnOk.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.btnOk.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.btnOk.UseVisualStyleBackColor = false;
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// lblSum
			// 
			this.lblSum.AutoSize = true;
			this.lblSum.Dock = System.Windows.Forms.DockStyle.Left;
			this.lblSum.Font = new System.Drawing.Font("Microsoft Sans Serif", 28F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.lblSum.Location = new System.Drawing.Point(0, 0);
			this.lblSum.Name = "lblSum";
			this.lblSum.Size = new System.Drawing.Size(176, 44);
			this.lblSum.TabIndex = 0;
			this.lblSum.Text = "00:00:00";
			this.lblSum.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// timeSplitter
			// 
			this.timeSplitter.Dock = System.Windows.Forms.DockStyle.Fill;
			this.timeSplitter.Location = new System.Drawing.Point(0, 20);
			this.timeSplitter.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
			this.timeSplitter.Name = "timeSplitter";
			this.timeSplitter.Size = new System.Drawing.Size(628, 60);
			this.timeSplitter.TabIndex = 3;
			// 
			// OfflineWorkForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(668, 391);
			this.Controls.Add(this.tlpMain);
			this.Name = "OfflineWorkForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultLocation;
			this.Text = "OfflineWorkForm";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OfflineWorkForm_FormClosing);
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OfflineWorkForm_FormClosed);
			this.Load += new System.EventHandler(this.OfflineWorkForm_Load);
			this.LocationChanged += new System.EventHandler(this.OfflineWorkForm_LocationChanged);
			this.SizeChanged += new System.EventHandler(this.OfflineWorkForm_SizeChanged);
			this.tlpMain.ResumeLayout(false);
			this.flpIntervalButtons.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pbMerge)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pbSplit)).EndInit();
			this.pnlMeetingBlock.ResumeLayout(false);
			this.pnlMeetingView.ResumeLayout(false);
			this.pnlTotalBlock.ResumeLayout(false);
			this.pnlTotalBlock.PerformLayout();
			this.pnlControlBar.ResumeLayout(false);
			this.pnlControlBar.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbCounter)).EndInit();
			this.flpBottomButtons.ResumeLayout(false);
			this.flpBottomButtons.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tlpMeetings;
		private System.Windows.Forms.TableLayoutPanel tlpMain;
		private System.Windows.Forms.Panel pnlMeetingBlock;
		private System.Windows.Forms.Panel pnlMeetingView;
		private Controls.ScrollBar scrMeetings;
		private System.Windows.Forms.Panel pnlTotalBlock;
		private Controls.SmallSplitter sspTotalBar;
		private System.Windows.Forms.Label lblTotal;
		private System.Windows.Forms.Panel pnlControlBar;
		private System.Windows.Forms.Label lblSum;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.FlowLayoutPanel flpBottomButtons;
		private System.Windows.Forms.ToolTip toolTip;
		private System.Windows.Forms.PictureBox pbCounter;
		private TimeSplitterControl timeSplitter;
		private System.Windows.Forms.FlowLayoutPanel flpIntervalButtons;
		private System.Windows.Forms.PictureBox pbSplit;
		private System.Windows.Forms.PictureBox pbMerge;
	}
}