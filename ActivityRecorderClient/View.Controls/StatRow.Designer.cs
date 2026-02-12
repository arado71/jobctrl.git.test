using System.Windows.Forms;
namespace Tct.ActivityRecorderClient.View.Controls
{
	partial class StatRow
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
			this.lblDelta = new System.Windows.Forms.Label();
			this.lblTitle = new System.Windows.Forms.Label();
			this.lblLeft = new System.Windows.Forms.Label();
			this.lblRight = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// lblDelta
			// 
			this.lblDelta.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lblDelta.Font = new System.Drawing.Font("Open Sans", 8F, System.Drawing.FontStyle.Bold);
			this.lblDelta.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(77)))), ((int)(((byte)(77)))), ((int)(((byte)(77)))));
			this.lblDelta.Location = new System.Drawing.Point(247, -1);
			this.lblDelta.Name = "lblDelta";
			this.lblDelta.Size = new System.Drawing.Size(62, 15);
			this.lblDelta.TabIndex = 3;
			this.lblDelta.Text = "∆ 04:21";
			this.lblDelta.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// lblTitle
			// 
			this.lblTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblTitle.Font = new System.Drawing.Font("Open Sans", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(77)))), ((int)(((byte)(77)))), ((int)(((byte)(77)))));
			this.lblTitle.Location = new System.Drawing.Point(3, -1);
			this.lblTitle.Name = "lblTitle";
			this.lblTitle.Size = new System.Drawing.Size(126, 15);
			this.lblTitle.TabIndex = 4;
			this.lblTitle.Text = "Weekly worktime";
			// 
			// lblLeft
			// 
			this.lblLeft.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lblLeft.Font = new System.Drawing.Font("Open Sans", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.lblLeft.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(77)))), ((int)(((byte)(77)))), ((int)(((byte)(77)))));
			this.lblLeft.Location = new System.Drawing.Point(135, -1);
			this.lblLeft.Name = "lblLeft";
			this.lblLeft.Size = new System.Drawing.Size(55, 15);
			this.lblLeft.TabIndex = 5;
			this.lblLeft.Text = "3:39";
			this.lblLeft.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// lblRight
			// 
			this.lblRight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lblRight.Font = new System.Drawing.Font("Open Sans", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.lblRight.ForeColor = System.Drawing.Color.Silver;
			this.lblRight.Location = new System.Drawing.Point(189, -1);
			this.lblRight.Name = "lblRight";
			this.lblRight.Size = new System.Drawing.Size(56, 15);
			this.lblRight.TabIndex = 6;
			this.lblRight.Text = "3:39";
			this.lblRight.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// StatRow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.lblRight);
			this.Controls.Add(this.lblLeft);
			this.Controls.Add(this.lblTitle);
			this.Controls.Add(this.lblDelta);
			this.Margin = new System.Windows.Forms.Padding(0);
			this.Name = "StatRow";
			this.Size = new System.Drawing.Size(310, 14);
			this.ResumeLayout(false);

		}

		#endregion

		private Label lblDelta;
		private Label lblTitle;
		private Label lblLeft;
		private Label lblRight;
	}
}
