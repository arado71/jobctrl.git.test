using System.Windows.Forms;
using Tct.ActivityRecorderClient.View.Controls;

namespace Tct.ActivityRecorderClient.View.Navigation
{
	partial class WorkRowShort
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WorkRowShort));
			this.pEdit = new System.Windows.Forms.Panel();
			this.btnFav = new Tct.ActivityRecorderClient.View.Controls.FavoriteButton();
			this.lblSmart = new Tct.ActivityRecorderClient.View.Controls.SmartLabel();
			this.workIcon1 = new Tct.ActivityRecorderClient.View.Controls.WorkIcon();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.pbIcon = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)(this.pbIcon)).BeginInit();
			this.SuspendLayout();
			// 
			// pEdit
			// 
			this.pEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.pEdit.BackColor = System.Drawing.Color.Transparent;
			this.pEdit.BackgroundImage = global::Tct.ActivityRecorderClient.Properties.Resources.details;
			this.pEdit.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.pEdit.Location = new System.Drawing.Point(277, 23);
			this.pEdit.Name = "pEdit";
			this.pEdit.Size = new System.Drawing.Size(16, 16);
			this.pEdit.TabIndex = 5;
			this.pEdit.Visible = false;
			this.pEdit.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HandleEditClicked);
			this.pEdit.MouseEnter += new System.EventHandler(this.HandleEditMouseEntered);
			this.pEdit.MouseLeave += new System.EventHandler(this.HandleEditMouseLeft);
			this.pEdit.MouseMove += new System.Windows.Forms.MouseEventHandler(this.HandleMouseMove);
			// 
			// btnFav
			// 
			this.btnFav.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnFav.BackColor = System.Drawing.Color.Transparent;
			this.btnFav.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnFav.BackgroundImage")));
			this.btnFav.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.btnFav.Cursor = System.Windows.Forms.Cursors.Hand;
			this.btnFav.IsFavorite = false;
			this.btnFav.Location = new System.Drawing.Point(277, 5);
			this.btnFav.Margin = new System.Windows.Forms.Padding(0);
			this.btnFav.MaximumSize = new System.Drawing.Size(16, 16);
			this.btnFav.MinimumSize = new System.Drawing.Size(16, 16);
			this.btnFav.Name = "btnFav";
			this.btnFav.Size = new System.Drawing.Size(16, 16);
			this.btnFav.TabIndex = 0;
			this.btnFav.Visible = false;
			this.btnFav.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HandleFavoriteClicked);
			this.btnFav.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.HandleFavoriteClicked);
			this.btnFav.MouseEnter += new System.EventHandler(this.HandleMouseEntered);
			this.btnFav.MouseLeave += new System.EventHandler(this.HandleMouseLeft);
			this.btnFav.MouseMove += new System.Windows.Forms.MouseEventHandler(this.HandleMouseMove);
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
			this.lblSmart.Location = new System.Drawing.Point(57, 6);
			this.lblSmart.Name = "lblSmart";
			this.lblSmart.Size = new System.Drawing.Size(217, 36);
			this.lblSmart.TabIndex = 4;
			this.lblSmart.VerticalAlignment = System.Windows.Forms.VisualStyles.VerticalAlignment.Center;
			this.lblSmart.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HandleClicked);
			this.lblSmart.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.HandleClicked);
			this.lblSmart.MouseEnter += new System.EventHandler(this.HandleMouseEntered);
			this.lblSmart.MouseLeave += new System.EventHandler(this.HandleMouseLeft);
			this.lblSmart.MouseMove += new System.Windows.Forms.MouseEventHandler(this.HandleMouseMove);
			// 
			// workIcon1
			// 
			this.workIcon1.AlternativeStyle = false;
			this.workIcon1.Color = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(156)))), ((int)(((byte)(223)))));
			this.workIcon1.Initials = "Kn";
			this.workIcon1.Location = new System.Drawing.Point(9, 5);
			this.workIcon1.Name = "workIcon1";
			this.workIcon1.Size = new System.Drawing.Size(36, 36);
			this.workIcon1.TabIndex = 0;
			this.workIcon1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HandleClicked);
			this.workIcon1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.HandleClicked);
			this.workIcon1.MouseEnter += new System.EventHandler(this.HandleMouseEntered);
			this.workIcon1.MouseLeave += new System.EventHandler(this.HandleMouseLeft);
			this.workIcon1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.HandleMouseMove);
			// 
			// toolTip1
			// 
			this.toolTip1.AutoPopDelay = 15000;
			this.toolTip1.InitialDelay = 500;
			this.toolTip1.ReshowDelay = 100;
			// 
			// pbIcon
			// 
			this.pbIcon.BackColor = System.Drawing.Color.Transparent;
			this.pbIcon.Location = new System.Drawing.Point(12, 8);
			this.pbIcon.Name = "pbIcon";
			this.pbIcon.Size = new System.Drawing.Size(32, 32);
			this.pbIcon.TabIndex = 6;
			this.pbIcon.TabStop = false;
			this.pbIcon.Visible = false;
			this.pbIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HandleClicked);
			// 
			// WorkRowShort
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.pEdit);
			this.Controls.Add(this.btnFav);
			this.Controls.Add(this.lblSmart);
			this.Controls.Add(this.workIcon1);
			this.Controls.Add(this.pbIcon);
			this.Cursor = System.Windows.Forms.Cursors.Hand;
			this.Name = "WorkRowShort";
			this.Size = new System.Drawing.Size(296, 46);
			this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HandleClicked);
			this.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.HandleClicked);
			this.MouseEnter += new System.EventHandler(this.HandleMouseEntered);
			this.MouseLeave += new System.EventHandler(this.HandleMouseLeft);
			this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.HandleMouseMove);
			((System.ComponentModel.ISupportInitialize)(this.pbIcon)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private WorkIcon workIcon1;
		private SmartLabel lblSmart;
		private FavoriteButton btnFav;
		private Panel pEdit;
		private ToolTip toolTip1;
		private PictureBox pbIcon;
	}
}
