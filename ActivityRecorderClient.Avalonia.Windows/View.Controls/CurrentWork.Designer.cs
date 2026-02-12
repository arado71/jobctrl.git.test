namespace Tct.ActivityRecorderClient.View.Controls
{
	partial class CurrentWork
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CurrentWork));
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.lblPrio = new System.Windows.Forms.Label();
			this.pEdit = new System.Windows.Forms.Panel();
			this.pPrioIcon = new System.Windows.Forms.Panel();
			this.mainTable = new System.Windows.Forms.TableLayoutPanel();
			this.taskTable = new System.Windows.Forms.TableLayoutPanel();
			this.workIcon = new Tct.ActivityRecorderClient.View.Controls.WorkIcon();
			this.lblTask = new Tct.ActivityRecorderClient.View.Controls.SmartLabel();
			this.btnFavorite = new Tct.ActivityRecorderClient.View.Controls.FavoriteButton();
			this.infoTable = new System.Windows.Forms.TableLayoutPanel();
			this.pbCompletion = new Tct.ActivityRecorderClient.View.Controls.ProgressBar();
			this.lblDeadline = new Tct.ActivityRecorderClient.View.Controls.SmartLabel();
			this.lblCompletion = new Tct.ActivityRecorderClient.View.Controls.SmartLabel();
			this.pbDeadline = new Tct.ActivityRecorderClient.View.Controls.ProgressBar();
			this.mainTable.SuspendLayout();
			this.taskTable.SuspendLayout();
			this.infoTable.SuspendLayout();
			this.SuspendLayout();
			// 
			// lblPrio
			// 
			this.lblPrio.BackColor = System.Drawing.Color.Transparent;
			this.lblPrio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.lblPrio.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblPrio.Location = new System.Drawing.Point(22, 5);
			this.lblPrio.Margin = new System.Windows.Forms.Padding(0, 5, 3, 5);
			this.lblPrio.Name = "lblPrio";
			this.infoTable.SetRowSpan(this.lblPrio, 2);
			this.lblPrio.Size = new System.Drawing.Size(40, 20);
			this.lblPrio.TabIndex = 15;
			this.lblPrio.Text = "label1";
			this.lblPrio.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.lblPrio.Visible = false;
			this.lblPrio.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HandleClicked);
			// 
			// pEdit
			// 
			this.pEdit.BackColor = System.Drawing.Color.Transparent;
			this.pEdit.BackgroundImage = global::Tct.ActivityRecorderClient.Properties.Resources.details;
			this.pEdit.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.pEdit.Cursor = System.Windows.Forms.Cursors.Hand;
			this.pEdit.Location = new System.Drawing.Point(301, 22);
			this.pEdit.Margin = new System.Windows.Forms.Padding(0);
			this.pEdit.Name = "pEdit";
			this.pEdit.Size = new System.Drawing.Size(16, 16);
			this.pEdit.TabIndex = 13;
			this.pEdit.Visible = false;
			this.pEdit.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HandleEditClicked);
			this.pEdit.MouseEnter += new System.EventHandler(this.HandleEditMouseEnter);
			this.pEdit.MouseLeave += new System.EventHandler(this.HandleEditMouseLeave);
			// 
			// pPrioIcon
			// 
			this.pPrioIcon.BackColor = System.Drawing.Color.Transparent;
			this.pPrioIcon.BackgroundImage = global::Tct.ActivityRecorderClient.Properties.Resources.priority_small;
			this.pPrioIcon.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.pPrioIcon.Cursor = System.Windows.Forms.Cursors.Hand;
			this.pPrioIcon.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pPrioIcon.Location = new System.Drawing.Point(6, 5);
			this.pPrioIcon.Margin = new System.Windows.Forms.Padding(6, 5, 0, 5);
			this.pPrioIcon.Name = "pPrioIcon";
			this.infoTable.SetRowSpan(this.pPrioIcon, 2);
			this.pPrioIcon.Size = new System.Drawing.Size(16, 20);
			this.pPrioIcon.TabIndex = 14;
			this.pPrioIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HandleClicked);
			// 
			// mainTable
			// 
			this.mainTable.AutoSize = true;
			this.mainTable.ColumnCount = 1;
			this.mainTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.mainTable.Controls.Add(this.taskTable, 0, 0);
			this.mainTable.Controls.Add(this.infoTable, 0, 1);
			this.mainTable.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainTable.Location = new System.Drawing.Point(0, 0);
			this.mainTable.Name = "mainTable";
			this.mainTable.RowCount = 2;
			this.mainTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.mainTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.mainTable.Size = new System.Drawing.Size(323, 97);
			this.mainTable.TabIndex = 17;
			// 
			// taskTable
			// 
			this.taskTable.ColumnCount = 3;
			this.taskTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.taskTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.taskTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.taskTable.Controls.Add(this.workIcon, 0, 0);
			this.taskTable.Controls.Add(this.lblTask, 1, 0);
			this.taskTable.Controls.Add(this.btnFavorite, 2, 0);
			this.taskTable.Controls.Add(this.pEdit, 2, 1);
			this.taskTable.Dock = System.Windows.Forms.DockStyle.Fill;
			this.taskTable.Location = new System.Drawing.Point(3, 3);
			this.taskTable.Name = "taskTable";
			this.taskTable.RowCount = 3;
			this.taskTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.taskTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.taskTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.taskTable.Size = new System.Drawing.Size(317, 55);
			this.taskTable.TabIndex = 0;
			// 
			// workIcon
			// 
			this.workIcon.AlternativeStyle = false;
			this.workIcon.Color = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(156)))), ((int)(((byte)(223)))));
			this.workIcon.Cursor = System.Windows.Forms.Cursors.Hand;
			this.workIcon.Dock = System.Windows.Forms.DockStyle.Fill;
			this.workIcon.Initials = "We";
			this.workIcon.Location = new System.Drawing.Point(6, 7);
			this.workIcon.Margin = new System.Windows.Forms.Padding(6, 7, 4, 6);
			this.workIcon.Name = "workIcon";
			this.taskTable.SetRowSpan(this.workIcon, 3);
			this.workIcon.Size = new System.Drawing.Size(40, 42);
			this.workIcon.TabIndex = 0;
			this.workIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HandleClicked);
			// 
			// lblTask
			// 
			this.lblTask.AutoWrap = false;
			this.lblTask.Cursor = System.Windows.Forms.Cursors.Hand;
			this.lblTask.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblTask.FontSize = 8.75F;
			this.lblTask.ForeColorAlternative = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.lblTask.HorizontalAlignment = System.Windows.Forms.HorizontalAlignment.Left;
			this.lblTask.Location = new System.Drawing.Point(53, 3);
			this.lblTask.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.lblTask.Name = "lblTask";
			this.taskTable.SetRowSpan(this.lblTask, 3);
			this.lblTask.Size = new System.Drawing.Size(245, 52);
			this.lblTask.TabIndex = 11;
			this.lblTask.VerticalAlignment = System.Windows.Forms.VisualStyles.VerticalAlignment.Top;
			this.lblTask.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HandleClicked);
			// 
			// btnFavorite
			// 
			this.btnFavorite.BackColor = System.Drawing.Color.Transparent;
			this.btnFavorite.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnFavorite.BackgroundImage")));
			this.btnFavorite.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.btnFavorite.Cursor = System.Windows.Forms.Cursors.Hand;
			this.btnFavorite.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnFavorite.IsFavorite = false;
			this.btnFavorite.Location = new System.Drawing.Point(301, 3);
			this.btnFavorite.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
			this.btnFavorite.MaximumSize = new System.Drawing.Size(16, 16);
			this.btnFavorite.MinimumSize = new System.Drawing.Size(16, 16);
			this.btnFavorite.Name = "btnFavorite";
			this.btnFavorite.Size = new System.Drawing.Size(16, 16);
			this.btnFavorite.TabIndex = 0;
			this.btnFavorite.Visible = false;
			this.btnFavorite.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HandleFavoriteClicked);
			this.btnFavorite.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.HandleFavoriteClicked);
			// 
			// infoTable
			// 
			this.infoTable.AutoSize = true;
			this.infoTable.ColumnCount = 4;
			this.infoTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.infoTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.infoTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.infoTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.infoTable.Controls.Add(this.pbCompletion, 3, 1);
			this.infoTable.Controls.Add(this.lblDeadline, 2, 0);
			this.infoTable.Controls.Add(this.lblCompletion, 3, 0);
			this.infoTable.Controls.Add(this.pbDeadline, 2, 1);
			this.infoTable.Controls.Add(this.lblPrio, 1, 0);
			this.infoTable.Controls.Add(this.pPrioIcon, 0, 0);
			this.infoTable.Dock = System.Windows.Forms.DockStyle.Fill;
			this.infoTable.Location = new System.Drawing.Point(3, 64);
			this.infoTable.Name = "infoTable";
			this.infoTable.RowCount = 2;
			this.infoTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.infoTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.infoTable.Size = new System.Drawing.Size(317, 30);
			this.infoTable.TabIndex = 1;
			// 
			// pbCompletion
			// 
			this.pbCompletion.Cursor = System.Windows.Forms.Cursors.Hand;
			this.pbCompletion.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pbCompletion.Location = new System.Drawing.Point(197, 16);
			this.pbCompletion.Margin = new System.Windows.Forms.Padding(6, 0, 6, 3);
			this.pbCompletion.Name = "pbCompletion";
			this.pbCompletion.Size = new System.Drawing.Size(114, 11);
			this.pbCompletion.Style = Tct.ActivityRecorderClient.View.Controls.ProgressStyle.Fill;
			this.pbCompletion.TabIndex = 3;
			this.pbCompletion.Value = 0.7F;
			this.pbCompletion.Visible = false;
			this.pbCompletion.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HandleClicked);
			// 
			// lblDeadline
			// 
			this.lblDeadline.AutoWrap = false;
			this.lblDeadline.Cursor = System.Windows.Forms.Cursors.Hand;
			this.lblDeadline.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblDeadline.FontSize = 8F;
			this.lblDeadline.ForeColorAlternative = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.lblDeadline.HorizontalAlignment = System.Windows.Forms.HorizontalAlignment.Center;
			this.lblDeadline.Location = new System.Drawing.Point(66, 0);
			this.lblDeadline.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
			this.lblDeadline.Name = "lblDeadline";
			this.lblDeadline.Size = new System.Drawing.Size(124, 16);
			this.lblDeadline.TabIndex = 9;
			this.lblDeadline.VerticalAlignment = System.Windows.Forms.VisualStyles.VerticalAlignment.Top;
			this.lblDeadline.Visible = false;
			this.lblDeadline.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HandleClicked);
			// 
			// lblCompletion
			// 
			this.lblCompletion.AutoWrap = false;
			this.lblCompletion.Cursor = System.Windows.Forms.Cursors.Hand;
			this.lblCompletion.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblCompletion.FontSize = 8F;
			this.lblCompletion.ForeColorAlternative = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.lblCompletion.HorizontalAlignment = System.Windows.Forms.HorizontalAlignment.Center;
			this.lblCompletion.Location = new System.Drawing.Point(192, 0);
			this.lblCompletion.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
			this.lblCompletion.Name = "lblCompletion";
			this.lblCompletion.Size = new System.Drawing.Size(124, 16);
			this.lblCompletion.TabIndex = 10;
			this.lblCompletion.VerticalAlignment = System.Windows.Forms.VisualStyles.VerticalAlignment.Top;
			this.lblCompletion.Visible = false;
			this.lblCompletion.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HandleClicked);
			// 
			// pbDeadline
			// 
			this.pbDeadline.Cursor = System.Windows.Forms.Cursors.Hand;
			this.pbDeadline.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pbDeadline.Location = new System.Drawing.Point(68, 16);
			this.pbDeadline.Margin = new System.Windows.Forms.Padding(3, 0, 6, 3);
			this.pbDeadline.Name = "pbDeadline";
			this.pbDeadline.Size = new System.Drawing.Size(117, 11);
			this.pbDeadline.Style = Tct.ActivityRecorderClient.View.Controls.ProgressStyle.Dot;
			this.pbDeadline.TabIndex = 4;
			this.pbDeadline.Value = 0.9F;
			this.pbDeadline.Visible = false;
			this.pbDeadline.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HandleClicked);
			// 
			// CurrentWork
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
			this.Controls.Add(this.mainTable);
			this.Cursor = System.Windows.Forms.Cursors.Hand;
			this.MinimumSize = new System.Drawing.Size(320, 90);
			this.Name = "CurrentWork";
			this.Size = new System.Drawing.Size(323, 97);
			this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HandleClicked);
			this.mainTable.ResumeLayout(false);
			this.mainTable.PerformLayout();
			this.taskTable.ResumeLayout(false);
			this.infoTable.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private WorkIcon workIcon;
		private ProgressBar pbCompletion;
		private ProgressBar pbDeadline;
		private SmartLabel lblDeadline;
		private SmartLabel lblCompletion;
		private System.Windows.Forms.Panel pEdit;
		private System.Windows.Forms.ToolTip toolTip1;
		private FavoriteButton btnFavorite;
		private System.Windows.Forms.Panel pPrioIcon;
		private System.Windows.Forms.Label lblPrio;
		private System.Windows.Forms.TableLayoutPanel infoTable;
		private System.Windows.Forms.TableLayoutPanel taskTable;
		private System.Windows.Forms.TableLayoutPanel mainTable;
		private SmartLabel lblTask;
	}
}
