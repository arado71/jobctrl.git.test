namespace Tct.ActivityRecorderClient.View
{
	partial class WorkSelectorForm
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
			this.lblWork = new System.Windows.Forms.Label();
			this.cbWorks = new System.Windows.Forms.ComboBox();
			this.btnOk = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.lblDescription = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// lblWork
			// 
			this.lblWork.Location = new System.Drawing.Point(42, 75);
			this.lblWork.Name = "lblWork";
			this.lblWork.Size = new System.Drawing.Size(114, 13);
			this.lblWork.TabIndex = 20;
			this.lblWork.Text = "Munka:";
			this.lblWork.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// cbWorks
			// 
			this.cbWorks.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbWorks.FormattingEnabled = true;
			this.cbWorks.Location = new System.Drawing.Point(162, 72);
			this.cbWorks.Name = "cbWorks";
			this.cbWorks.Size = new System.Drawing.Size(470, 21);
			this.cbWorks.TabIndex = 19;
			// 
			// btnOk
			// 
			this.btnOk.Location = new System.Drawing.Point(476, 108);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(75, 23);
			this.btnOk.TabIndex = 21;
			this.btnOk.Text = "OK";
			this.btnOk.UseVisualStyleBackColor = true;
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(557, 108);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 22;
			this.btnCancel.Text = "20";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// lblDescription
			// 
			this.lblDescription.Location = new System.Drawing.Point(39, 13);
			this.lblDescription.Name = "lblDescription";
			this.lblDescription.Size = new System.Drawing.Size(593, 41);
			this.lblDescription.TabIndex = 23;
			this.lblDescription.Text = "Kérem válasszon egy munkát";
			// 
			// WorkSelectorForm
			// 
			this.AcceptButton = this.btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(675, 144);
			this.Controls.Add(this.lblDescription);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.lblWork);
			this.Controls.Add(this.cbWorks);
			this.Name = "WorkSelectorForm";
			this.Text = "Kérem válasszon egy munkát";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label lblWork;
		private System.Windows.Forms.ComboBox cbWorks;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Label lblDescription;

	}
}