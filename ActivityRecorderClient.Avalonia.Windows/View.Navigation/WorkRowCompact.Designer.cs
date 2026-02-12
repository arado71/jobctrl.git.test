using Tct.ActivityRecorderClient.View.Controls;

namespace Tct.ActivityRecorderClient.View.Navigation
{
	partial class WorkRowCompact
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
			this.workIcon1 = new Tct.ActivityRecorderClient.View.Controls.WorkIcon();
			this.lblText = new Tct.ActivityRecorderClient.View.Controls.SmartLabel();
			this.SuspendLayout();
			// 
			// workIcon1
			// 
			this.workIcon1.AlternativeStyle = false;
			this.workIcon1.Color = System.Drawing.Color.FromArgb(((int)(((byte)(199)))), ((int)(((byte)(142)))), ((int)(((byte)(142)))));
			this.workIcon1.Initials = null;
			this.workIcon1.Location = new System.Drawing.Point(0, 0);
			this.workIcon1.Name = "workIcon1";
			this.workIcon1.Size = new System.Drawing.Size(16, 16);
			this.workIcon1.TabIndex = 1;
			this.workIcon1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HandleClicked);
			this.workIcon1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.HandleClicked);
			this.workIcon1.MouseEnter += new System.EventHandler(this.HandleMouseEntered);
			// 
			// lblText
			// 
			this.lblText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblText.AutoWrap = false;
			this.lblText.FontSize = 8F;
			this.lblText.ForeColorAlternative = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(179)))), ((int)(((byte)(179)))));
			this.lblText.HorizontalAlignment = System.Windows.Forms.HorizontalAlignment.Left;
			this.lblText.Location = new System.Drawing.Point(22, 0);
			this.lblText.Name = "lblText";
			this.lblText.Size = new System.Drawing.Size(252, 16);
			this.lblText.TabIndex = 0;
			this.lblText.Text = "lblText";
			this.lblText.VerticalAlignment = System.Windows.Forms.VisualStyles.VerticalAlignment.Top;
			this.lblText.MouseDown += new System.Windows.Forms.MouseEventHandler(this.HandleClicked);
			this.lblText.MouseEnter += new System.EventHandler(this.HandleMouseEntered);
			// 
			// WorkRowCompact
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.workIcon1);
			this.Controls.Add(this.lblText);
			this.Name = "WorkRowCompact";
			this.Size = new System.Drawing.Size(277, 18);
			this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HandleClicked);
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.HandleClicked);
			this.MouseEnter += new System.EventHandler(this.HandleMouseEntered);
			this.ResumeLayout(false);

		}

		#endregion

		private SmartLabel lblText;
		private WorkIcon workIcon1;
	}
}
