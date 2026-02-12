using MetroFramework.Controls;

namespace Tct.ActivityRecorderClient.View
{
	partial class WelcomeForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WelcomeForm));
			this.btnOk = new MetroFramework.Controls.MetroButton();
			this.pbHU = new System.Windows.Forms.PictureBox();
			this.pbEN = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)(this.pbHU)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pbEN)).BeginInit();
			this.SuspendLayout();
			// 
			// btnOk
			// 
			this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOk.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnOk.Location = new System.Drawing.Point(565, 436);
			this.btnOk.Name = "btnOk";
			this.btnOk.TabIndex = 1;
			this.btnOk.Text = "Ok";
			this.btnOk.UseSelectable = true;
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// pbHU
			// 
			this.pbHU.Image = ((System.Drawing.Image)(resources.GetObject("pbHU.Image")));
			this.pbHU.Location = new System.Drawing.Point(23, 22);
			this.pbHU.Name = "pbHU";
			this.pbHU.Size = new System.Drawing.Size(614, 385);
			this.pbHU.TabIndex = 2;
			this.pbHU.TabStop = false;
			// 
			// pbEN
			// 
			this.pbEN.Image = ((System.Drawing.Image)(resources.GetObject("pbEN.Image")));
			this.pbEN.Location = new System.Drawing.Point(23, 22);
			this.pbEN.Name = "pbEN";
			this.pbEN.Size = new System.Drawing.Size(614, 385);
			this.pbEN.TabIndex = 3;
			this.pbEN.TabStop = false;
			// 
			// WelcomeForm
			// 
			this.AcceptButton = this.btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnOk;
			this.ClientSize = new System.Drawing.Size(664, 472);
			this.ControlBox = false;
			this.Controls.Add(this.pbEN);
			this.Controls.Add(this.pbHU);
			this.Controls.Add(this.btnOk);
			this.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size(664, 472);
			this.MinimumSize = new System.Drawing.Size(664, 472);
			this.Name = "WelcomeForm";
			this.Resizable = false;
			this.Style = MetroFramework.MetroColorStyle.Orange;
			this.Text = "JobCTRL";
			((System.ComponentModel.ISupportInitialize)(this.pbHU)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pbEN)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.PictureBox pbHU;
		private System.Windows.Forms.PictureBox pbEN;
		private MetroButton btnOk;
	}
}