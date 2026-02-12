using Tct.ActivityRecorderClient.View.Navigation;

namespace Tct.ActivityRecorderClient.View.Controls
{
	partial class WorkGrid
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
			this.pVisibleArea = new System.Windows.Forms.Panel();
			this.lblPath = new System.Windows.Forms.Label();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.pHome = new System.Windows.Forms.Panel();
			this.pBack = new System.Windows.Forms.Panel();
			this.mainGrid = new System.Windows.Forms.TableLayoutPanel();
			this.pCreateWork = new System.Windows.Forms.Panel();
			this.workTable = new Tct.ActivityRecorderClient.View.Controls.CustomTableLayoutPanel();
			this.scrollBar1 = new Tct.ActivityRecorderClient.View.Controls.ScrollBar();
			this.workRow1 = new Tct.ActivityRecorderClient.View.Navigation.WorkRowShort();
			this.smallSplitter1 = new Tct.ActivityRecorderClient.View.Controls.SmallSplitter();
			this.workRow2 = new Tct.ActivityRecorderClient.View.Navigation.WorkRowShort();
			this.smallSplitter2 = new Tct.ActivityRecorderClient.View.Controls.SmallSplitter();
			this.workRow3 = new Tct.ActivityRecorderClient.View.Navigation.WorkRowShort();
			this.smallSplitter3 = new Tct.ActivityRecorderClient.View.Controls.SmallSplitter();
			this.workRow4 = new Tct.ActivityRecorderClient.View.Navigation.WorkRowShort();
			this.pVisibleArea.SuspendLayout();
			this.mainGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// pVisibleArea
			// 
			this.pVisibleArea.AllowDrop = true;
			this.pVisibleArea.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.mainGrid.SetColumnSpan(this.pVisibleArea, 4);
			this.pVisibleArea.Controls.Add(this.workTable);
			this.pVisibleArea.Location = new System.Drawing.Point(0, 34);
			this.pVisibleArea.Margin = new System.Windows.Forms.Padding(0);
			this.pVisibleArea.Name = "pVisibleArea";
			this.pVisibleArea.Size = new System.Drawing.Size(355, 604);
			this.pVisibleArea.TabIndex = 2;
			this.pVisibleArea.DragDrop += new System.Windows.Forms.DragEventHandler(this.HandleDragDropped);
			this.pVisibleArea.DragEnter += new System.Windows.Forms.DragEventHandler(this.HandleDragEntered);
			this.pVisibleArea.DragOver += new System.Windows.Forms.DragEventHandler(this.HandleDragMoved);
			this.pVisibleArea.GiveFeedback += new System.Windows.Forms.GiveFeedbackEventHandler(this.HandleDragging);
			// 
			// lblPath
			// 
			this.lblPath.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblPath.Location = new System.Drawing.Point(59, 6);
			this.lblPath.Name = "lblPath";
			this.lblPath.Size = new System.Drawing.Size(278, 28);
			this.lblPath.TabIndex = 4;
			this.lblPath.Text = "label1";
			this.lblPath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.lblPath.UseMnemonic = false;
			// 
			// pHome
			// 
			this.pHome.BackColor = System.Drawing.Color.Transparent;
			this.pHome.BackgroundImage = global::Tct.ActivityRecorderClient.Properties.Resources.home;
			this.pHome.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.pHome.Cursor = System.Windows.Forms.Cursors.Hand;
			this.pHome.Location = new System.Drawing.Point(3, 9);
			this.pHome.Name = "pHome";
			this.pHome.Size = new System.Drawing.Size(22, 22);
			this.pHome.TabIndex = 5;
			this.pHome.Paint += new System.Windows.Forms.PaintEventHandler(this.PanelPaint);
			this.pHome.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HandleHomeClicked);
			this.pHome.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.HandleHomeClicked);
			// 
			// pBack
			// 
			this.pBack.BackColor = System.Drawing.Color.Transparent;
			this.pBack.BackgroundImage = global::Tct.ActivityRecorderClient.Properties.Resources.parent;
			this.pBack.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.pBack.Cursor = System.Windows.Forms.Cursors.Hand;
			this.pBack.Location = new System.Drawing.Point(31, 9);
			this.pBack.Name = "pBack";
			this.pBack.Size = new System.Drawing.Size(22, 22);
			this.pBack.TabIndex = 3;
			this.pBack.Paint += new System.Windows.Forms.PaintEventHandler(this.PanelPaint);
			this.pBack.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HandleUpClicked);
			this.pBack.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.HandleUpClicked);
			// 
			// mainGrid
			// 
			this.mainGrid.ColumnCount = 5;
			this.mainGrid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.mainGrid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.mainGrid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.mainGrid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.mainGrid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.mainGrid.Controls.Add(this.scrollBar1, 4, 1);
			this.mainGrid.Controls.Add(this.pVisibleArea, 0, 1);
			this.mainGrid.Controls.Add(this.lblPath, 2, 0);
			this.mainGrid.Controls.Add(this.pHome, 0, 0);
			this.mainGrid.Controls.Add(this.pBack, 1, 0);
			this.mainGrid.Controls.Add(this.pCreateWork, 3, 0);
			this.mainGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainGrid.Location = new System.Drawing.Point(0, 0);
			this.mainGrid.Margin = new System.Windows.Forms.Padding(6);
			this.mainGrid.Name = "mainGrid";
			this.mainGrid.Padding = new System.Windows.Forms.Padding(0, 6, 0, 6);
			this.mainGrid.RowCount = 2;
			this.mainGrid.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.mainGrid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.mainGrid.Size = new System.Drawing.Size(368, 644);
			this.mainGrid.TabIndex = 1;
			// 
			// pCreateWork
			// 
			this.pCreateWork.BackgroundImage = global::Tct.ActivityRecorderClient.Properties.Resources.new_task;
			this.pCreateWork.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.mainGrid.SetColumnSpan(this.pCreateWork, 2);
			this.pCreateWork.Cursor = System.Windows.Forms.Cursors.Hand;
			this.pCreateWork.Location = new System.Drawing.Point(343, 9);
			this.pCreateWork.Name = "pCreateWork";
			this.pCreateWork.Size = new System.Drawing.Size(22, 22);
			this.pCreateWork.TabIndex = 6;
			this.pCreateWork.Paint += new System.Windows.Forms.PaintEventHandler(this.PanelPaint);
			this.pCreateWork.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HandleCreateWorkClicked);
			// 
			// workTable
			// 
			this.workTable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.workTable.Location = new System.Drawing.Point(0, 0);
			this.workTable.Margin = new System.Windows.Forms.Padding(6, 0, 10, 0);
			this.workTable.Name = "workTable";
			this.workTable.Padding = new System.Windows.Forms.Padding(6);
			this.workTable.Size = new System.Drawing.Size(355, 315);
			this.workTable.TabIndex = 0;
			// 
			// scrollBar1
			// 
			this.scrollBar1.Dock = System.Windows.Forms.DockStyle.Right;
			this.scrollBar1.Location = new System.Drawing.Point(358, 37);
			this.scrollBar1.Name = "scrollBar1";
			this.scrollBar1.ScrollSpeed = 1F;
			this.scrollBar1.ScrollTotalSize = 100;
			this.scrollBar1.ScrollVisibleSize = 10;
			this.scrollBar1.Size = new System.Drawing.Size(7, 598);
			this.scrollBar1.TabIndex = 1;
			this.scrollBar1.Value = 0;
			this.scrollBar1.ScrollChanged += new System.EventHandler(this.HandleScrolled);
			// 
			// workRow1
			// 
			this.workRow1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.workRow1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
			this.workRow1.Cursor = System.Windows.Forms.Cursors.Hand;
			this.workRow1.Location = new System.Drawing.Point(0, 0);
			this.workRow1.Margin = new System.Windows.Forms.Padding(0);
			this.workRow1.Name = "workRow1";
			this.workRow1.Navigation = null;
			this.workRow1.Selected = false;
			this.workRow1.SelectedBackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(235)))), ((int)(((byte)(248)))));
			this.workRow1.Size = new System.Drawing.Size(360, 75);
			this.workRow1.TabIndex = 0;
			this.workRow1.UnselectedBackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
			this.workRow1.Value = null;
			// 
			// smallSplitter1
			// 
			this.smallSplitter1.Accent = false;
			this.smallSplitter1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.smallSplitter1.Location = new System.Drawing.Point(0, 75);
			this.smallSplitter1.Margin = new System.Windows.Forms.Padding(0);
			this.smallSplitter1.MinimumSize = new System.Drawing.Size(0, 3);
			this.smallSplitter1.Name = "smallSplitter1";
			this.smallSplitter1.Padding = new System.Windows.Forms.Padding(3, 1, 3, 1);
			this.smallSplitter1.Size = new System.Drawing.Size(360, 3);
			this.smallSplitter1.TabIndex = 1;
			// 
			// workRow2
			// 
			this.workRow2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.workRow2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
			this.workRow2.Cursor = System.Windows.Forms.Cursors.Hand;
			this.workRow2.Location = new System.Drawing.Point(0, 78);
			this.workRow2.Margin = new System.Windows.Forms.Padding(0);
			this.workRow2.Name = "workRow2";
			this.workRow2.Navigation = null;
			this.workRow2.Selected = false;
			this.workRow2.SelectedBackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(235)))), ((int)(((byte)(248)))));
			this.workRow2.Size = new System.Drawing.Size(360, 75);
			this.workRow2.TabIndex = 2;
			this.workRow2.UnselectedBackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
			this.workRow2.Value = null;
			// 
			// smallSplitter2
			// 
			this.smallSplitter2.Accent = false;
			this.smallSplitter2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.smallSplitter2.Location = new System.Drawing.Point(0, 153);
			this.smallSplitter2.Margin = new System.Windows.Forms.Padding(0);
			this.smallSplitter2.MinimumSize = new System.Drawing.Size(0, 3);
			this.smallSplitter2.Name = "smallSplitter2";
			this.smallSplitter2.Padding = new System.Windows.Forms.Padding(3, 1, 3, 1);
			this.smallSplitter2.Size = new System.Drawing.Size(360, 3);
			this.smallSplitter2.TabIndex = 3;
			// 
			// workRow3
			// 
			this.workRow3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.workRow3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
			this.workRow3.Cursor = System.Windows.Forms.Cursors.Hand;
			this.workRow3.Location = new System.Drawing.Point(0, 156);
			this.workRow3.Margin = new System.Windows.Forms.Padding(0);
			this.workRow3.Name = "workRow3";
			this.workRow3.Navigation = null;
			this.workRow3.Selected = false;
			this.workRow3.SelectedBackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(235)))), ((int)(((byte)(248)))));
			this.workRow3.Size = new System.Drawing.Size(360, 75);
			this.workRow3.TabIndex = 4;
			this.workRow3.UnselectedBackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
			this.workRow3.Value = null;
			// 
			// smallSplitter3
			// 
			this.smallSplitter3.Accent = false;
			this.smallSplitter3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.smallSplitter3.Location = new System.Drawing.Point(0, 231);
			this.smallSplitter3.Margin = new System.Windows.Forms.Padding(0);
			this.smallSplitter3.MinimumSize = new System.Drawing.Size(0, 3);
			this.smallSplitter3.Name = "smallSplitter3";
			this.smallSplitter3.Padding = new System.Windows.Forms.Padding(3, 1, 3, 1);
			this.smallSplitter3.Size = new System.Drawing.Size(360, 3);
			this.smallSplitter3.TabIndex = 5;
			// 
			// workRow4
			// 
			this.workRow4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.workRow4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
			this.workRow4.Cursor = System.Windows.Forms.Cursors.Hand;
			this.workRow4.Location = new System.Drawing.Point(0, 234);
			this.workRow4.Margin = new System.Windows.Forms.Padding(0);
			this.workRow4.Name = "workRow4";
			this.workRow4.Navigation = null;
			this.workRow4.Selected = false;
			this.workRow4.SelectedBackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(235)))), ((int)(((byte)(248)))));
			this.workRow4.Size = new System.Drawing.Size(360, 75);
			this.workRow4.TabIndex = 6;
			this.workRow4.UnselectedBackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
			this.workRow4.Value = null;
			// 
			// WorkGrid
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.mainGrid);
			this.Margin = new System.Windows.Forms.Padding(0);
			this.Name = "WorkGrid";
			this.Size = new System.Drawing.Size(368, 644);
			this.GiveFeedback += new System.Windows.Forms.GiveFeedbackEventHandler(this.HandleDragging);
			this.pVisibleArea.ResumeLayout(false);
			this.mainGrid.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private CustomTableLayoutPanel workTable;
		private WorkRowShort workRow1;
		private SmallSplitter smallSplitter1;
		private WorkRowShort workRow2;
		private SmallSplitter smallSplitter2;
		private WorkRowShort workRow3;
		private SmallSplitter smallSplitter3;
		private WorkRowShort workRow4;
		private System.Windows.Forms.Panel pVisibleArea;
		private ScrollBar scrollBar1;
		private System.Windows.Forms.Panel pBack;
		private System.Windows.Forms.Label lblPath;
		private System.Windows.Forms.Panel pHome;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.TableLayoutPanel mainGrid;
		private System.Windows.Forms.Panel pCreateWork;

	}
}
