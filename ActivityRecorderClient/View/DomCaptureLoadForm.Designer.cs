using MetroFramework.Controls;

namespace Tct.ActivityRecorderClient.View
{
	partial class DomCaptureLoadForm
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
			this.label1 = new System.Windows.Forms.Label();
			this.txtJson = new System.Windows.Forms.TextBox();
			this.btnOk = new MetroButton();
			this.btnCancel = new MetroButton();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(27, 66);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(32, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Json:";
			// 
			// txtJson
			// 
			this.txtJson.Location = new System.Drawing.Point(81, 63);
			this.txtJson.Name = "txtJson";
			this.txtJson.Size = new System.Drawing.Size(516, 20);
			this.txtJson.TabIndex = 1;
			this.txtJson.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtJson_KeyPress);
			// 
			// btnOk
			// 
			this.btnOk.Location = new System.Drawing.Point(441, 96);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(75, 23);
			this.btnOk.TabIndex = 2;
			this.btnOk.Text = "OK";
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(522, 96);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 3;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// DomCaptureLoadForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(637, 142);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.txtJson);
			this.Controls.Add(this.label1);
			this.Location = new System.Drawing.Point(0, 0);
			this.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size(637, 142);
			this.MinimumSize = new System.Drawing.Size(637, 142);
			this.Name = "DomCaptureLoadForm";
			this.Resizable = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Load settings from json";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtJson;
		private MetroButton btnOk;
		private MetroButton btnCancel;
	}
}