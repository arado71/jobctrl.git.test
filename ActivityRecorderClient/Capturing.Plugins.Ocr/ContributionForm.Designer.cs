namespace Tct.ActivityRecorderClient.Capturing.Plugins.Ocr
{
	partial class ContributionForm
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
			this.pnlButtons = new System.Windows.Forms.Panel();
			this.btnClear = new MetroFramework.Controls.MetroButton();
			this.btnOk = new MetroFramework.Controls.MetroButton();
			this.btnCancel = new MetroFramework.Controls.MetroButton();
			this.pnlTable = new System.Windows.Forms.Panel();
			this.tblSamples = new System.Windows.Forms.TableLayoutPanel();
			this.pnlButtons.SuspendLayout();
			this.SuspendLayout();
			// 
			// pnlButtons
			// 
			this.pnlButtons.Controls.Add(this.btnClear);
			this.pnlButtons.Controls.Add(this.btnOk);
			this.pnlButtons.Controls.Add(this.btnCancel);
			this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.pnlButtons.Location = new System.Drawing.Point(20, 383);
			this.pnlButtons.Name = "pnlButtons";
			this.pnlButtons.Size = new System.Drawing.Size(436, 55);
			this.pnlButtons.TabIndex = 1;
			// 
			// btnClear
			// 
			this.btnClear.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.btnClear.Location = new System.Drawing.Point(179, 24);
			this.btnClear.Margin = new System.Windows.Forms.Padding(4);
			this.btnClear.Name = "btnClear";
			this.btnClear.Size = new System.Drawing.Size(81, 27);
			this.btnClear.Style = MetroFramework.MetroColorStyle.Blue;
			this.btnClear.TabIndex = 40;
			this.btnClear.Text = "Clear";
			this.btnClear.Theme = MetroFramework.MetroThemeStyle.Light;
			this.btnClear.UseSelectable = true;
			this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
			// 
			// btnOk
			// 
			this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnOk.Location = new System.Drawing.Point(4, 24);
			this.btnOk.Margin = new System.Windows.Forms.Padding(4);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(84, 27);
			this.btnOk.Style = MetroFramework.MetroColorStyle.Blue;
			this.btnOk.TabIndex = 38;
			this.btnOk.Text = "Save";
			this.btnOk.Theme = MetroFramework.MetroThemeStyle.Light;
			this.btnOk.UseSelectable = true;
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.AutoSize = true;
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(335, 24);
			this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(97, 27);
			this.btnCancel.Style = MetroFramework.MetroColorStyle.Blue;
			this.btnCancel.TabIndex = 42;
			this.btnCancel.Text = "cancel";
			this.btnCancel.Theme = MetroFramework.MetroThemeStyle.Light;
			this.btnCancel.UseSelectable = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// pnlTable
			// 
			this.pnlTable.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlTable.Location = new System.Drawing.Point(20, 60);
			this.pnlTable.Margin = new System.Windows.Forms.Padding(3, 3, 3, 20);
			this.pnlTable.Name = "pnlTable";
			this.pnlTable.Size = new System.Drawing.Size(436, 323);
			this.pnlTable.TabIndex = 2;
			// 
			// tblSamples
			// 
			this.tblSamples.AutoScroll = true;
			this.tblSamples.BackColor = System.Drawing.Color.Transparent;
			this.tblSamples.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 395F));
			this.tblSamples.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tblSamples.Location = new System.Drawing.Point(20, 60);
			this.tblSamples.Name = "tblSamples";
			this.tblSamples.Size = new System.Drawing.Size(436, 323);
			this.tblSamples.TabIndex = 1;
			// 
			// ContributionForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(476, 458);
			this.Controls.Add(this.tblSamples);
			this.Controls.Add(this.pnlTable);
			this.Controls.Add(this.pnlButtons);
			this.Name = "ContributionForm";
			this.Text = "Contribution Form";
			this.Load += new System.EventHandler(this.ContributionForm_Load);
			this.pnlButtons.ResumeLayout(false);
			this.pnlButtons.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel pnlButtons;
		private MetroFramework.Controls.MetroButton btnCancel;
		private MetroFramework.Controls.MetroButton btnOk;
		private System.Windows.Forms.Panel pnlTable;
		private System.Windows.Forms.TableLayoutPanel tblSamples;
        private MetroFramework.Controls.MetroButton btnClear;
    }
}