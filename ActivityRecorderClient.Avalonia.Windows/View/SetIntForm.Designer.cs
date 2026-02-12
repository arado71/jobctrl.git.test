namespace Tct.ActivityRecorderClient.View
{
	partial class SetIntForm
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
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnOk = new System.Windows.Forms.Button();
			this.txtValue = new System.Windows.Forms.TextBox();
			this.lblValueText = new System.Windows.Forms.Label();
			this.lblValueTitle = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(158, 70);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 8;
			this.btnCancel.Text = "Mégse";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// btnOk
			// 
			this.btnOk.Location = new System.Drawing.Point(39, 70);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(75, 23);
			this.btnOk.TabIndex = 7;
			this.btnOk.Text = "OK";
			this.btnOk.UseVisualStyleBackColor = true;
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// txtValue
			// 
			this.txtValue.Location = new System.Drawing.Point(97, 40);
			this.txtValue.Name = "txtValue";
			this.txtValue.Size = new System.Drawing.Size(116, 20);
			this.txtValue.TabIndex = 9;
			this.txtValue.TextChanged += new System.EventHandler(this.txtValue_TextChanged);
			this.txtValue.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtValue_KeyPress);
			// 
			// lblValueText
			// 
			this.lblValueText.AutoSize = true;
			this.lblValueText.Location = new System.Drawing.Point(56, 43);
			this.lblValueText.Name = "lblValueText";
			this.lblValueText.Size = new System.Drawing.Size(35, 13);
			this.lblValueText.TabIndex = 10;
			this.lblValueText.Text = "Érték:";
			// 
			// lblValueTitle
			// 
			this.lblValueTitle.Location = new System.Drawing.Point(13, 4);
			this.lblValueTitle.Name = "lblValueTitle";
			this.lblValueTitle.Size = new System.Drawing.Size(252, 28);
			this.lblValueTitle.TabIndex = 11;
			this.lblValueTitle.Text = "Hány másodpercenként jelenjen meg a nem munka státusz figyelmesztetés";
			// 
			// SetIntForm
			// 
			this.AcceptButton = this.btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(277, 105);
			this.Controls.Add(this.lblValueTitle);
			this.Controls.Add(this.lblValueText);
			this.Controls.Add(this.txtValue);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOk);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SetIntForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "SetIntForm";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.TextBox txtValue;
		private System.Windows.Forms.Label lblValueText;
		private System.Windows.Forms.Label lblValueTitle;
	}
}