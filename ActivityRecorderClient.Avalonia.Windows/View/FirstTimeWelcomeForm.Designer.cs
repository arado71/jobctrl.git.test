namespace Tct.ActivityRecorderClient.View
{
	partial class FirstTimeWelcomeForm
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
			this.btnOk = new MetroFramework.Controls.MetroButton();
			this.chbDontShow = new MetroFramework.Controls.MetroCheckBox();
			this.pbInstruction = new System.Windows.Forms.PictureBox();
			this.lblInstructionText = new System.Windows.Forms.LinkLabel();
			this.lblInstructionTextAlt1 = new MetroFramework.Controls.MetroLabel();
			this.lblInstructionTextAlt2 = new MetroFramework.Controls.MetroLabel();
			this.lblWelcomeText = new MetroFramework.Controls.MetroLabel();
			this.btnBack = new MetroFramework.Controls.MetroButton();
			this.pnlWorkModes = new MetroFramework.Controls.MetroPanel();
			this.lblYellowDesc = new MetroFramework.Controls.MetroLabel();
			this.lblRedDesc = new MetroFramework.Controls.MetroLabel();
			this.lblGreenDesc = new MetroFramework.Controls.MetroLabel();
			this.pbYellow = new System.Windows.Forms.PictureBox();
			this.pbGreen = new System.Windows.Forms.PictureBox();
			this.pbRed = new System.Windows.Forms.PictureBox();
			this.flpDotButtons = new System.Windows.Forms.FlowLayoutPanel();
			this.flpNavButtons = new System.Windows.Forms.FlowLayoutPanel();
			this.pnlInstructionView = new System.Windows.Forms.Panel();
			this.scrInstructionView = new Tct.ActivityRecorderClient.View.Controls.ScrollBar();
			((System.ComponentModel.ISupportInitialize)(this.pbInstruction)).BeginInit();
			this.pnlWorkModes.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbYellow)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pbGreen)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pbRed)).BeginInit();
			this.flpNavButtons.SuspendLayout();
			this.pnlInstructionView.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnOk
			// 
			this.btnOk.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(156)))), ((int)(((byte)(221)))));
			this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOk.FontSize = MetroFramework.MetroButtonSize.Medium;
			this.btnOk.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(251)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
			this.btnOk.Location = new System.Drawing.Point(339, 3);
			this.btnOk.MinimumSize = new System.Drawing.Size(75, 32);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(75, 32);
			this.btnOk.TabIndex = 0;
			this.btnOk.TabStop = false;
			this.btnOk.Text = "OK";
			this.btnOk.UseCustomBackColor = true;
			this.btnOk.UseCustomForeColor = true;
			this.btnOk.UseSelectable = true;
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// chbDontShow
			// 
			this.chbDontShow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.chbDontShow.AutoSize = true;
			this.chbDontShow.FontSize = MetroFramework.MetroCheckBoxSize.Medium;
			this.chbDontShow.FontWeight = MetroFramework.MetroCheckBoxWeight.Light;
			this.chbDontShow.Location = new System.Drawing.Point(29, 350);
			this.chbDontShow.Name = "chbDontShow";
			this.chbDontShow.Size = new System.Drawing.Size(87, 19);
			this.chbDontShow.TabIndex = 1;
			this.chbDontShow.Text = "don\'t show";
			this.chbDontShow.UseSelectable = true;
			this.chbDontShow.CheckedChanged += new System.EventHandler(this.chbImmStartWork_CheckedChanged);
			// 
			// pbInstruction
			// 
			this.pbInstruction.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
			this.pbInstruction.Location = new System.Drawing.Point(29, 98);
			this.pbInstruction.Margin = new System.Windows.Forms.Padding(0);
			this.pbInstruction.Name = "pbInstruction";
			this.pbInstruction.Size = new System.Drawing.Size(510, 227);
			this.pbInstruction.TabIndex = 2;
			this.pbInstruction.TabStop = false;
			// 
			// lblInstructionText
			// 
			this.lblInstructionText.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
			this.lblInstructionText.Font = new System.Drawing.Font("Segoe UI Light", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(238)));
			this.lblInstructionText.Location = new System.Drawing.Point(0, 0);
			this.lblInstructionText.Name = "lblInstructionText";
			this.lblInstructionText.Size = new System.Drawing.Size(261, 76);
			this.lblInstructionText.TabIndex = 3;
			this.lblInstructionText.TabStop = true;
			this.lblInstructionText.Text = "lblInstructionText";
			this.lblInstructionText.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lblInstructionText_LinkClicked);
			// 
			// lblInstructionTextAlt1
			// 
			this.lblInstructionTextAlt1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
			this.lblInstructionTextAlt1.Location = new System.Drawing.Point(351, 135);
			this.lblInstructionTextAlt1.Name = "lblInstructionTextAlt1";
			this.lblInstructionTextAlt1.Size = new System.Drawing.Size(178, 60);
			this.lblInstructionTextAlt1.TabIndex = 4;
			this.lblInstructionTextAlt1.Text = "metroLabel1";
			this.lblInstructionTextAlt1.UseCustomBackColor = true;
			this.lblInstructionTextAlt1.Visible = false;
			this.lblInstructionTextAlt1.WrapToLine = true;
			// 
			// lblInstructionTextAlt2
			// 
			this.lblInstructionTextAlt2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
			this.lblInstructionTextAlt2.Location = new System.Drawing.Point(351, 219);
			this.lblInstructionTextAlt2.Name = "lblInstructionTextAlt2";
			this.lblInstructionTextAlt2.Size = new System.Drawing.Size(178, 60);
			this.lblInstructionTextAlt2.TabIndex = 5;
			this.lblInstructionTextAlt2.Text = "metroLabel2";
			this.lblInstructionTextAlt2.UseCustomBackColor = true;
			this.lblInstructionTextAlt2.Visible = false;
			this.lblInstructionTextAlt2.WrapToLine = true;
			// 
			// lblWelcomeText
			// 
			this.lblWelcomeText.AutoSize = true;
			this.lblWelcomeText.Location = new System.Drawing.Point(25, 64);
			this.lblWelcomeText.Margin = new System.Windows.Forms.Padding(0);
			this.lblWelcomeText.Name = "lblWelcomeText";
			this.lblWelcomeText.Size = new System.Drawing.Size(61, 19);
			this.lblWelcomeText.TabIndex = 6;
			this.lblWelcomeText.Text = "welcome";
			// 
			// btnBack
			// 
			this.btnBack.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(156)))), ((int)(((byte)(221)))));
			this.btnBack.FontSize = MetroFramework.MetroButtonSize.Medium;
			this.btnBack.FontWeight = MetroFramework.MetroButtonWeight.Regular;
			this.btnBack.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(251)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
			this.btnBack.Location = new System.Drawing.Point(258, 3);
			this.btnBack.Name = "btnBack";
			this.btnBack.Size = new System.Drawing.Size(75, 32);
			this.btnBack.TabIndex = 7;
			this.btnBack.TabStop = false;
			this.btnBack.Text = "back";
			this.btnBack.UseCustomBackColor = true;
			this.btnBack.UseCustomForeColor = true;
			this.btnBack.UseSelectable = true;
			this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
			// 
			// pnlWorkModes
			// 
			this.pnlWorkModes.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
			this.pnlWorkModes.Controls.Add(this.lblYellowDesc);
			this.pnlWorkModes.Controls.Add(this.lblRedDesc);
			this.pnlWorkModes.Controls.Add(this.lblGreenDesc);
			this.pnlWorkModes.Controls.Add(this.pbYellow);
			this.pnlWorkModes.Controls.Add(this.pbGreen);
			this.pnlWorkModes.Controls.Add(this.pbRed);
			this.pnlWorkModes.HorizontalScrollbarBarColor = true;
			this.pnlWorkModes.HorizontalScrollbarHighlightOnWheel = false;
			this.pnlWorkModes.HorizontalScrollbarSize = 10;
			this.pnlWorkModes.Location = new System.Drawing.Point(265, 163);
			this.pnlWorkModes.Name = "pnlWorkModes";
			this.pnlWorkModes.Size = new System.Drawing.Size(274, 136);
			this.pnlWorkModes.TabIndex = 8;
			this.pnlWorkModes.UseCustomBackColor = true;
			this.pnlWorkModes.VerticalScrollbarBarColor = true;
			this.pnlWorkModes.VerticalScrollbarHighlightOnWheel = false;
			this.pnlWorkModes.VerticalScrollbarSize = 10;
			// 
			// lblYellowDesc
			// 
			this.lblYellowDesc.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
			this.lblYellowDesc.Location = new System.Drawing.Point(29, 91);
			this.lblYellowDesc.Margin = new System.Windows.Forms.Padding(0);
			this.lblYellowDesc.Name = "lblYellowDesc";
			this.lblYellowDesc.Size = new System.Drawing.Size(245, 40);
			this.lblYellowDesc.TabIndex = 7;
			this.lblYellowDesc.Text = "metroLabel1";
			this.lblYellowDesc.UseCustomBackColor = true;
			this.lblYellowDesc.WrapToLine = true;
			// 
			// lblRedDesc
			// 
			this.lblRedDesc.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
			this.lblRedDesc.Location = new System.Drawing.Point(29, 10);
			this.lblRedDesc.Margin = new System.Windows.Forms.Padding(0);
			this.lblRedDesc.Name = "lblRedDesc";
			this.lblRedDesc.Size = new System.Drawing.Size(245, 40);
			this.lblRedDesc.TabIndex = 6;
			this.lblRedDesc.Text = "metroLabel1";
			this.lblRedDesc.UseCustomBackColor = true;
			this.lblRedDesc.WrapToLine = true;
			// 
			// lblGreenDesc
			// 
			this.lblGreenDesc.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
			this.lblGreenDesc.Location = new System.Drawing.Point(29, 50);
			this.lblGreenDesc.Margin = new System.Windows.Forms.Padding(0);
			this.lblGreenDesc.Name = "lblGreenDesc";
			this.lblGreenDesc.Size = new System.Drawing.Size(245, 40);
			this.lblGreenDesc.TabIndex = 5;
			this.lblGreenDesc.Text = "metroLabel1";
			this.lblGreenDesc.UseCustomBackColor = true;
			this.lblGreenDesc.WrapToLine = true;
			// 
			// pbYellow
			// 
			this.pbYellow.Location = new System.Drawing.Point(7, 97);
			this.pbYellow.Name = "pbYellow";
			this.pbYellow.Size = new System.Drawing.Size(16, 16);
			this.pbYellow.TabIndex = 4;
			this.pbYellow.TabStop = false;
			// 
			// pbGreen
			// 
			this.pbGreen.Location = new System.Drawing.Point(7, 56);
			this.pbGreen.Name = "pbGreen";
			this.pbGreen.Size = new System.Drawing.Size(16, 16);
			this.pbGreen.TabIndex = 3;
			this.pbGreen.TabStop = false;
			// 
			// pbRed
			// 
			this.pbRed.Location = new System.Drawing.Point(7, 16);
			this.pbRed.Name = "pbRed";
			this.pbRed.Size = new System.Drawing.Size(16, 16);
			this.pbRed.TabIndex = 2;
			this.pbRed.TabStop = false;
			// 
			// flpDotButtons
			// 
			this.flpDotButtons.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.flpDotButtons.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
			this.flpDotButtons.Location = new System.Drawing.Point(285, 309);
			this.flpDotButtons.Name = "flpDotButtons";
			this.flpDotButtons.Size = new System.Drawing.Size(11, 11);
			this.flpDotButtons.TabIndex = 8;
			// 
			// flpNavButtons
			// 
			this.flpNavButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.flpNavButtons.Controls.Add(this.btnOk);
			this.flpNavButtons.Controls.Add(this.btnBack);
			this.flpNavButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
			this.flpNavButtons.Location = new System.Drawing.Point(122, 340);
			this.flpNavButtons.Margin = new System.Windows.Forms.Padding(0);
			this.flpNavButtons.Name = "flpNavButtons";
			this.flpNavButtons.Size = new System.Drawing.Size(417, 38);
			this.flpNavButtons.TabIndex = 9;
			// 
			// pnlInstructionView
			// 
			this.pnlInstructionView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
			this.pnlInstructionView.Controls.Add(this.lblInstructionText);
			this.pnlInstructionView.Location = new System.Drawing.Point(265, 123);
			this.pnlInstructionView.Margin = new System.Windows.Forms.Padding(0);
			this.pnlInstructionView.Name = "pnlInstructionView";
			this.pnlInstructionView.Size = new System.Drawing.Size(261, 176);
			this.pnlInstructionView.TabIndex = 10;
			// 
			// scrInstructionView
			// 
			this.scrInstructionView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
			this.scrInstructionView.Location = new System.Drawing.Point(526, 123);
			this.scrInstructionView.Name = "scrInstructionView";
			this.scrInstructionView.ScrollSpeed = 1F;
			this.scrInstructionView.ScrollTotalSize = 100;
			this.scrInstructionView.ScrollVisibleSize = 10;
			this.scrInstructionView.Size = new System.Drawing.Size(7, 176);
			this.scrInstructionView.TabIndex = 4;
			this.scrInstructionView.Value = 0;
			this.scrInstructionView.Visible = false;
			this.scrInstructionView.ScrollChanged += new System.EventHandler(this.scrInstructionView_ScrollChanged);
			// 
			// FirstTimeWelcomeForm
			// 
			this.AcceptButton = this.btnOk;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(570, 400);
			this.Controls.Add(this.chbDontShow);
			this.Controls.Add(this.flpNavButtons);
			this.Controls.Add(this.flpDotButtons);
			this.Controls.Add(this.pnlWorkModes);
			this.Controls.Add(this.lblInstructionTextAlt2);
			this.Controls.Add(this.lblInstructionTextAlt1);
			this.Controls.Add(this.lblWelcomeText);
			this.Controls.Add(this.pnlInstructionView);
			this.Controls.Add(this.scrInstructionView);
			this.Controls.Add(this.pbInstruction);
			this.KeyPreview = true;
			this.MaximizeBox = false;
			this.Name = "FirstTimeWelcomeForm";
			this.Resizable = false;
			this.Text = "Welcome";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FirstTimeWelcomeFormFormClosed);
			this.Load += new System.EventHandler(this.FirstTimeWelcomeForm_Load);
			((System.ComponentModel.ISupportInitialize)(this.pbInstruction)).EndInit();
			this.pnlWorkModes.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pbYellow)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pbGreen)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pbRed)).EndInit();
			this.flpNavButtons.ResumeLayout(false);
			this.pnlInstructionView.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private MetroFramework.Controls.MetroButton btnOk;
		private MetroFramework.Controls.MetroCheckBox chbDontShow;
		private System.Windows.Forms.PictureBox pbInstruction;
		private System.Windows.Forms.LinkLabel lblInstructionText;
		private MetroFramework.Controls.MetroLabel lblInstructionTextAlt1;
		private MetroFramework.Controls.MetroLabel lblInstructionTextAlt2;
		private MetroFramework.Controls.MetroLabel lblWelcomeText;
		private MetroFramework.Controls.MetroButton btnBack;
		private System.Windows.Forms.FlowLayoutPanel flpDotButtons;
		private MetroFramework.Controls.MetroPanel pnlWorkModes;
		private System.Windows.Forms.PictureBox pbRed;
		private System.Windows.Forms.PictureBox pbYellow;
		private System.Windows.Forms.PictureBox pbGreen;
		private MetroFramework.Controls.MetroLabel lblYellowDesc;
		private MetroFramework.Controls.MetroLabel lblRedDesc;
		private MetroFramework.Controls.MetroLabel lblGreenDesc;
		private System.Windows.Forms.FlowLayoutPanel flpNavButtons;
		private System.Windows.Forms.Panel pnlInstructionView;
		private Controls.ScrollBar scrInstructionView;
	}
}