namespace VoxCTRL.View
{
	partial class SmallRecorderForm
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
			this.btnRecord = new System.Windows.Forms.Button();
			this.txtId = new System.Windows.Forms.TextBox();
			this.btnStop = new System.Windows.Forms.Button();
			this.btnPause = new System.Windows.Forms.Button();
			this.lblTime = new System.Windows.Forms.Label();
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.picTooSilent = new System.Windows.Forms.PictureBox();
			this.pnlMain = new System.Windows.Forms.Panel();
			((System.ComponentModel.ISupportInitialize)(this.picTooSilent)).BeginInit();
			this.pnlMain.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnRecord
			// 
			this.btnRecord.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.btnRecord.BackgroundImage = global::VoxCTRL.Properties.Resources.Record_small;
			this.btnRecord.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.btnRecord.ForeColor = System.Drawing.Color.White;
			this.btnRecord.Location = new System.Drawing.Point(17, 31);
			this.btnRecord.Name = "btnRecord";
			this.btnRecord.Size = new System.Drawing.Size(48, 48);
			this.btnRecord.TabIndex = 0;
			this.btnRecord.UseVisualStyleBackColor = false;
			this.btnRecord.Click += new System.EventHandler(this.btnRecord_Click);
			// 
			// txtId
			// 
			this.txtId.Location = new System.Drawing.Point(4, 5);
			this.txtId.Name = "txtId";
			this.txtId.Size = new System.Drawing.Size(206, 20);
			this.txtId.TabIndex = 1;
			// 
			// btnStop
			// 
			this.btnStop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(147)))), ((int)(((byte)(147)))));
			this.btnStop.BackgroundImage = global::VoxCTRL.Properties.Resources.Stop_small;
			this.btnStop.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.btnStop.ForeColor = System.Drawing.Color.White;
			this.btnStop.Location = new System.Drawing.Point(141, 31);
			this.btnStop.Name = "btnStop";
			this.btnStop.Size = new System.Drawing.Size(48, 48);
			this.btnStop.TabIndex = 3;
			this.btnStop.UseVisualStyleBackColor = false;
			this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
			// 
			// btnPause
			// 
			this.btnPause.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(50)))), ((int)(((byte)(0)))));
			this.btnPause.BackgroundImage = global::VoxCTRL.Properties.Resources.Pause_small;
			this.btnPause.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.btnPause.ForeColor = System.Drawing.Color.White;
			this.btnPause.Location = new System.Drawing.Point(17, 31);
			this.btnPause.Name = "btnPause";
			this.btnPause.Size = new System.Drawing.Size(48, 48);
			this.btnPause.TabIndex = 3;
			this.btnPause.UseVisualStyleBackColor = false;
			this.btnPause.Click += new System.EventHandler(this.btnPause_Click);
			// 
			// lblTime
			// 
			this.lblTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.lblTime.Location = new System.Drawing.Point(71, 46);
			this.lblTime.Name = "lblTime";
			this.lblTime.Size = new System.Drawing.Size(64, 23);
			this.lblTime.TabIndex = 4;
			this.lblTime.Text = "00:00:00";
			this.lblTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// picTooSilent
			// 
			this.picTooSilent.BackColor = System.Drawing.Color.Transparent;
			this.picTooSilent.Image = global::VoxCTRL.Properties.Resources.TooSilent_small;
			this.picTooSilent.Location = new System.Drawing.Point(76, 31);
			this.picTooSilent.Name = "picTooSilent";
			this.picTooSilent.Size = new System.Drawing.Size(54, 48);
			this.picTooSilent.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.picTooSilent.TabIndex = 6;
			this.picTooSilent.TabStop = false;
			this.toolTip.SetToolTip(this.picTooSilent, "A felvétel jelenleg túl halk. Ha beszélgetés alatt látja ezt a jelzést, akkor ell" +
        "enőrizze a hangbeállításokat, valamint, hogy mikrofonja megfelelően csatlakozik-" +
        "e!");
			this.picTooSilent.Visible = false;
			// 
			// pnlMain
			// 
			this.pnlMain.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pnlMain.Controls.Add(this.picTooSilent);
			this.pnlMain.Controls.Add(this.txtId);
			this.pnlMain.Controls.Add(this.lblTime);
			this.pnlMain.Controls.Add(this.btnStop);
			this.pnlMain.Controls.Add(this.btnRecord);
			this.pnlMain.Controls.Add(this.btnPause);
			this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlMain.Location = new System.Drawing.Point(0, 0);
			this.pnlMain.Name = "pnlMain";
			this.pnlMain.Size = new System.Drawing.Size(217, 88);
			this.pnlMain.TabIndex = 5;
			// 
			// SmallRecorderForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(217, 88);
			this.ControlBox = false;
			this.Controls.Add(this.pnlMain);
			this.DataBindings.Add(new System.Windows.Forms.Binding("Location", global::VoxCTRL.Properties.Settings.Default, "SmallFormLocation", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Location = global::VoxCTRL.Properties.Settings.Default.SmallFormLocation;
			this.MaximumSize = new System.Drawing.Size(219, 90);
			this.MinimumSize = new System.Drawing.Size(219, 90);
			this.Name = "SmallRecorderForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.TopMost = true;
			((System.ComponentModel.ISupportInitialize)(this.picTooSilent)).EndInit();
			this.pnlMain.ResumeLayout(false);
			this.pnlMain.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button btnRecord;
		private System.Windows.Forms.TextBox txtId;
		private System.Windows.Forms.Button btnStop;
		private System.Windows.Forms.Button btnPause;
		private System.Windows.Forms.Label lblTime;
		private System.Windows.Forms.ToolTip toolTip;
		private System.Windows.Forms.Panel pnlMain;
		private System.Windows.Forms.PictureBox picTooSilent;
	}
}