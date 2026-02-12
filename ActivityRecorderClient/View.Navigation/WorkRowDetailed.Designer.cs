using System.Windows.Forms;
using Tct.ActivityRecorderClient.View.Controls;

namespace Tct.ActivityRecorderClient.View.Navigation
{
	partial class WorkRowDetailed
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WorkRowDetailed));
			this.pEdit = new System.Windows.Forms.Panel();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.favBtn = new Tct.ActivityRecorderClient.View.Controls.FavoriteButton();
			this.lblSmart = new Tct.ActivityRecorderClient.View.Controls.SmartLabel();
			this.lblAdditional = new Tct.ActivityRecorderClient.View.Controls.SmartLabel();
			this.workIcon1 = new Tct.ActivityRecorderClient.View.Controls.WorkIcon();
			this.SuspendLayout();
			// 
			// pEdit
			// 
			this.pEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.pEdit.BackColor = System.Drawing.Color.Transparent;
			this.pEdit.BackgroundImage = global::Tct.ActivityRecorderClient.Properties.Resources.details;
			this.pEdit.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.pEdit.Location = new System.Drawing.Point(274, 28);
			this.pEdit.Name = "pEdit";
			this.pEdit.Size = new System.Drawing.Size(16, 16);
			this.pEdit.TabIndex = 6;
			this.pEdit.Visible = false;
			this.pEdit.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HandleEditClicked);
			this.pEdit.MouseEnter += new System.EventHandler(this.HandleEditMouseEntered);
			this.pEdit.MouseLeave += new System.EventHandler(this.HandleEditMouseLeft);
			// 
			// toolTip1
			// 
			this.toolTip1.AutoPopDelay = 15000;
			this.toolTip1.InitialDelay = 500;
			this.toolTip1.ReshowDelay = 100;
			// 
			// favBtn
			// 
			this.favBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.favBtn.BackColor = System.Drawing.Color.Transparent;
			this.favBtn.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("favBtn.BackgroundImage")));
			this.favBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.favBtn.Cursor = System.Windows.Forms.Cursors.Hand;
			this.favBtn.IsFavorite = false;
			this.favBtn.Location = new System.Drawing.Point(274, 8);
			this.favBtn.Margin = new System.Windows.Forms.Padding(0);
			this.favBtn.MaximumSize = new System.Drawing.Size(16, 16);
			this.favBtn.MinimumSize = new System.Drawing.Size(16, 16);
			this.favBtn.Name = "favBtn";
			this.favBtn.Size = new System.Drawing.Size(16, 16);
			this.favBtn.TabIndex = 5;
			this.favBtn.Visible = false;
			this.favBtn.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HandleFavoriteClicked);
			this.favBtn.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.HandleFavoriteClicked);
			this.favBtn.MouseEnter += new System.EventHandler(this.HandleMouseEntered);
			this.favBtn.MouseLeave += new System.EventHandler(this.HandleMouseLeft);
			// 
			// lblSmart
			// 
			this.lblSmart.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblSmart.AutoWrap = false;
			this.lblSmart.FontSize = 8.75F;
			this.lblSmart.ForeColorAlternative = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(179)))), ((int)(((byte)(179)))));
			this.lblSmart.HorizontalAlignment = System.Windows.Forms.HorizontalAlignment.Left;
			this.lblSmart.Location = new System.Drawing.Point(57, 3);
			this.lblSmart.Name = "lblSmart";
			this.lblSmart.Size = new System.Drawing.Size(215, 50);
			this.lblSmart.TabIndex = 4;
			this.lblSmart.VerticalAlignment = System.Windows.Forms.VisualStyles.VerticalAlignment.Center;
			this.lblSmart.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HandleClicked);
			this.lblSmart.MouseEnter += new System.EventHandler(this.HandleMouseEntered);
			this.lblSmart.MouseLeave += new System.EventHandler(this.HandleMouseLeft);
			// 
			// lblAdditional
			// 
			this.lblAdditional.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblAdditional.AutoSize = true;
			this.lblAdditional.AutoWrap = false;
			this.lblAdditional.Font = new System.Drawing.Font("Open Sans", 8F);
			this.lblAdditional.FontSize = 8F;
			this.lblAdditional.ForeColorAlternative = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(179)))), ((int)(((byte)(179)))));
			this.lblAdditional.HorizontalAlignment = System.Windows.Forms.HorizontalAlignment.Left;
			this.lblAdditional.Location = new System.Drawing.Point(56, 52);
			this.lblAdditional.Name = "lblAdditional";
			this.lblAdditional.Size = new System.Drawing.Size(237, 15);
			this.lblAdditional.TabIndex = 3;
			this.lblAdditional.Text = "15 min. left";
			this.lblAdditional.VerticalAlignment = System.Windows.Forms.VisualStyles.VerticalAlignment.Top;
			this.lblAdditional.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HandleClicked);
			this.lblAdditional.MouseEnter += new System.EventHandler(this.HandleMouseEntered);
			this.lblAdditional.MouseLeave += new System.EventHandler(this.HandleMouseLeft);
			// 
			// workIcon1
			// 
			this.workIcon1.AlternativeStyle = false;
			this.workIcon1.Color = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(156)))), ((int)(((byte)(223)))));
			this.workIcon1.Initials = "Kn";
			this.workIcon1.Location = new System.Drawing.Point(9, 7);
			this.workIcon1.Name = "workIcon1";
			this.workIcon1.Size = new System.Drawing.Size(40, 40);
			this.workIcon1.TabIndex = 0;
			this.workIcon1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HandleClicked);
			this.workIcon1.MouseEnter += new System.EventHandler(this.HandleMouseEntered);
			this.workIcon1.MouseLeave += new System.EventHandler(this.HandleMouseLeft);
			// 
			// WorkRowDetailed
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.pEdit);
			this.Controls.Add(this.favBtn);
			this.Controls.Add(this.lblSmart);
			this.Controls.Add(this.lblAdditional);
			this.Controls.Add(this.workIcon1);
			this.Cursor = System.Windows.Forms.Cursors.Hand;
			this.Name = "WorkRowDetailed";
			this.Size = new System.Drawing.Size(296, 69);
			this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HandleClicked);
			this.MouseEnter += new System.EventHandler(this.HandleMouseEntered);
			this.MouseLeave += new System.EventHandler(this.HandleMouseLeft);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private WorkIcon workIcon1;
		private SmartLabel lblAdditional;
		private SmartLabel lblSmart;
		private FavoriteButton favBtn;
		private Panel pEdit;
		private ToolTip toolTip1;
	}
}
