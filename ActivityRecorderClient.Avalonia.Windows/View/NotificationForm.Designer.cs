namespace Tct.ActivityRecorderClient.View
{
	partial class NotificationForm
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
			this.pnlMsg = new System.Windows.Forms.Panel();
			this.lnkMore = new System.Windows.Forms.LinkLabel();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.lblTitle = new System.Windows.Forms.Label();
			this.pnlInner = new System.Windows.Forms.Panel();
			this.lblMessage = new System.Windows.Forms.LinkLabel();
			this.pColor = new System.Windows.Forms.Panel();
			this.timerClose = new System.Windows.Forms.Timer(this.components);
			this.pnlMsg.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.pnlInner.SuspendLayout();
			this.SuspendLayout();
			// 
			// pnlMsg
			// 
			this.pnlMsg.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.pnlMsg.BackColor = System.Drawing.Color.White;
			this.pnlMsg.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.pnlMsg.Controls.Add(this.lnkMore);
			this.pnlMsg.Controls.Add(this.pictureBox1);
			this.pnlMsg.Controls.Add(this.lblTitle);
			this.pnlMsg.Controls.Add(this.pnlInner);
			this.pnlMsg.Location = new System.Drawing.Point(9, 1);
			this.pnlMsg.Margin = new System.Windows.Forms.Padding(0);
			this.pnlMsg.MinimumSize = new System.Drawing.Size(315, 93);
			this.pnlMsg.Name = "pnlMsg";
			this.pnlMsg.Size = new System.Drawing.Size(315, 93);
			this.pnlMsg.TabIndex = 0;
			this.pnlMsg.Click += new System.EventHandler(this.HandleClicked);
			this.pnlMsg.Paint += new System.Windows.Forms.PaintEventHandler(this.HandlePainting);
			// 
			// lnkMore
			// 
			this.lnkMore.BackColor = System.Drawing.Color.Transparent;
			this.lnkMore.Location = new System.Drawing.Point(11, 64);
			this.lnkMore.Name = "lnkMore";
			this.lnkMore.Size = new System.Drawing.Size(100, 20);
			this.lnkMore.TabIndex = 4;
			this.lnkMore.TabStop = true;
			this.lnkMore.Text = "Bővebben...";
			this.lnkMore.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.HandleMoreClicked);
			// 
			// pictureBox1
			// 
			this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.pictureBox1.Cursor = System.Windows.Forms.Cursors.Hand;
			this.pictureBox1.Image = global::Tct.ActivityRecorderClient.Properties.Resources.cancel;
			this.pictureBox1.Location = new System.Drawing.Point(288, 11);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(16, 16);
			this.pictureBox1.TabIndex = 3;
			this.pictureBox1.TabStop = false;
			this.pictureBox1.Click += new System.EventHandler(this.HandleCloseClicked);
			// 
			// lblTitle
			// 
			this.lblTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblTitle.AutoEllipsis = true;
			this.lblTitle.BackColor = System.Drawing.Color.Transparent;
			this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.lblTitle.Location = new System.Drawing.Point(9, 6);
			this.lblTitle.Name = "lblTitle";
			this.lblTitle.Size = new System.Drawing.Size(265, 27);
			this.lblTitle.TabIndex = 2;
			this.lblTitle.Text = "label1";
			this.lblTitle.Click += new System.EventHandler(this.HandleClicked);
			// 
			// pnlInner
			// 
			this.pnlInner.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.pnlInner.BackColor = System.Drawing.Color.Transparent;
			this.pnlInner.Controls.Add(this.lblMessage);
			this.pnlInner.Location = new System.Drawing.Point(12, 33);
			this.pnlInner.Name = "pnlInner";
			this.pnlInner.Size = new System.Drawing.Size(285, 51);
			this.pnlInner.TabIndex = 6;
			// 
			// lblMessage
			// 
			this.lblMessage.ActiveLinkColor = System.Drawing.Color.Black;
			this.lblMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblMessage.BackColor = System.Drawing.Color.Transparent;
			this.lblMessage.LinkColor = System.Drawing.Color.Black;
			this.lblMessage.Location = new System.Drawing.Point(0, 0);
			this.lblMessage.Name = "lblMessage";
			this.lblMessage.Size = new System.Drawing.Size(285, 51);
			this.lblMessage.TabIndex = 5;
			this.lblMessage.TabStop = true;
			this.lblMessage.Text = "Notification message";
			this.lblMessage.VisitedLinkColor = System.Drawing.Color.Black;
			this.lblMessage.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.HandleMessageLinkClicked);
			this.lblMessage.SizeChanged += new System.EventHandler(this.HandlePanelSizeChanged);
			this.lblMessage.Click += new System.EventHandler(this.HandleClicked);
			// 
			// pColor
			// 
			this.pColor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.pColor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(152)))), ((int)(((byte)(29)))));
			this.pColor.Location = new System.Drawing.Point(0, 1);
			this.pColor.Name = "pColor";
			this.pColor.Size = new System.Drawing.Size(10, 93);
			this.pColor.TabIndex = 1;
			this.pColor.Click += new System.EventHandler(this.HandleClicked);
			// 
			// timerClose
			// 
			this.timerClose.Tick += new System.EventHandler(this.HandleTicked);
			// 
			// NotificationForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Black;
			this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.ClientSize = new System.Drawing.Size(325, 95);
			this.ControlBox = false;
			this.Controls.Add(this.pnlMsg);
			this.Controls.Add(this.pColor);
			this.DoubleBuffered = true;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "NotificationForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "NotificationForm";
			this.TransparencyKey = System.Drawing.Color.Magenta;
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.HandleClosing);
			this.pnlMsg.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.pnlInner.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel pnlMsg;
		private System.Windows.Forms.Label lblTitle;
		private System.Windows.Forms.Panel pColor;
		private System.Windows.Forms.Timer timerClose;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.LinkLabel lnkMore;
		private System.Windows.Forms.LinkLabel lblMessage;
		private System.Windows.Forms.Panel pnlInner;
	}
}