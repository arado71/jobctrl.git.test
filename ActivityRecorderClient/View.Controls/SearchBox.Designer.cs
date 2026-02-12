namespace Tct.ActivityRecorderClient.View.Controls
{
	partial class SearchBox
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
			this.txtInput = new System.Windows.Forms.TextBox();
			this.pIcon = new System.Windows.Forms.Panel();
			this.delayTimer = new System.Windows.Forms.Timer(this.components);
			this.SuspendLayout();
			// 
			// txtInput
			// 
			this.txtInput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtInput.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(77)))), ((int)(((byte)(77)))), ((int)(((byte)(77)))));
			this.txtInput.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.txtInput.Location = new System.Drawing.Point(12, 6);
			this.txtInput.Name = "txtInput";
			this.txtInput.Size = new System.Drawing.Size(106, 13);
			this.txtInput.TabIndex = 0;
			this.txtInput.TextChanged += new System.EventHandler(this.HandleInputChanged);
			this.txtInput.Enter += new System.EventHandler(this.HandleFocused);
			this.txtInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.HandleInputKeyPressing);
			this.txtInput.KeyUp += new System.Windows.Forms.KeyEventHandler(this.HandleInputKeyReleasing);
			this.txtInput.Leave += new System.EventHandler(this.HandleLostFocus);
			// 
			// pIcon
			// 
			this.pIcon.BackColor = System.Drawing.Color.Transparent;
			this.pIcon.BackgroundImage = global::Tct.ActivityRecorderClient.Properties.Resources.search;
			this.pIcon.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.pIcon.Dock = System.Windows.Forms.DockStyle.Right;
			this.pIcon.Location = new System.Drawing.Point(131, 0);
			this.pIcon.Margin = new System.Windows.Forms.Padding(4);
			this.pIcon.Name = "pIcon";
			this.pIcon.Size = new System.Drawing.Size(20, 23);
			this.pIcon.TabIndex = 1;
			// 
			// delayTimer
			// 
			this.delayTimer.Interval = 300;
			this.delayTimer.Tick += new System.EventHandler(this.HandleDelayTimerTick);
			// 
			// SearchBox
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.Controls.Add(this.pIcon);
			this.Controls.Add(this.txtInput);
			this.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.MaximumSize = new System.Drawing.Size(1000, 23);
			this.MinimumSize = new System.Drawing.Size(20, 23);
			this.Name = "SearchBox";
			this.Padding = new System.Windows.Forms.Padding(0, 0, 6, 0);
			this.Size = new System.Drawing.Size(157, 23);
			this.Load += new System.EventHandler(this.HandleLoad);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.HandlePainting);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox txtInput;
		private System.Windows.Forms.Panel pIcon;
		private System.Windows.Forms.Timer delayTimer;
	}
}
