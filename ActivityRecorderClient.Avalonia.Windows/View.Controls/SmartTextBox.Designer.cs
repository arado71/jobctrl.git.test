namespace Tct.ActivityRecorderClient.View.Controls
{
	partial class SmartTextBox
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
			this.components = new System.ComponentModel.Container();
			this.pbIcon = new System.Windows.Forms.PictureBox();
			this.ttIcon = new System.Windows.Forms.ToolTip(this.components);
			this.txtInput = new System.Windows.Forms.TextBox();
			((System.ComponentModel.ISupportInitialize)(this.pbIcon)).BeginInit();
			this.SuspendLayout();
			// 
			// pbIcon
			// 
			this.pbIcon.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.pbIcon.Image = global::Tct.ActivityRecorderClient.Properties.Resources.pencil;
			this.pbIcon.Location = new System.Drawing.Point(2, 3);
			this.pbIcon.Name = "pbIcon";
			this.pbIcon.Size = new System.Drawing.Size(16, 16);
			this.pbIcon.TabIndex = 1;
			this.pbIcon.TabStop = false;
			this.ttIcon.SetToolTip(this.pbIcon, "Lorem Ipsum");
			this.pbIcon.Visible = false;
			// 
			// txtInput
			// 
			this.txtInput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtInput.BackColor = System.Drawing.SystemColors.Control;
			this.txtInput.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtInput.Location = new System.Drawing.Point(22, 0);
			this.txtInput.MaximumSize = new System.Drawing.Size(640, 20);
			this.txtInput.Name = "txtInput";
			this.txtInput.Size = new System.Drawing.Size(65, 20);
			this.txtInput.TabIndex = 2;
			this.txtInput.TextChanged += new System.EventHandler(this.HandleInputChanged);
			this.txtInput.Enter += new System.EventHandler(this.HandleInputFocus);
			this.txtInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.HandleKeyPressing);
			this.txtInput.Leave += new System.EventHandler(this.HandleInputFocusLost);
			// 
			// SmartTextBox
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.Controls.Add(this.txtInput);
			this.Controls.Add(this.pbIcon);
			this.Name = "SmartTextBox";
			this.Size = new System.Drawing.Size(90, 23);
			((System.ComponentModel.ISupportInitialize)(this.pbIcon)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PictureBox pbIcon;
		private System.Windows.Forms.ToolTip ttIcon;
		private System.Windows.Forms.TextBox txtInput;
	}
}
