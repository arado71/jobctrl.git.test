namespace MouseKeyboardActivityDetector
{
	partial class MouseKeyboardActivityForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MouseKeyboardActivityForm));
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.mouseActivityLabel = new System.Windows.Forms.Label();
			this.injectedMouseActivityLabel = new System.Windows.Forms.Label();
			this.keyboardActivityLabel = new System.Windows.Forms.Label();
			this.injectedKeyboardActivityLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(81, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Mouse activity: ";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 34);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(88, 13);
			this.label2.TabIndex = 0;
			this.label2.Text = "Keyboard activity";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(12, 58);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(121, 13);
			this.label3.TabIndex = 0;
			this.label3.Text = "Injected mouse activity: ";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(12, 83);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(131, 13);
			this.label4.TabIndex = 0;
			this.label4.Text = "Injected keyboard activity:";
			// 
			// mouseActivityLabel
			// 
			this.mouseActivityLabel.AutoSize = true;
			this.mouseActivityLabel.Location = new System.Drawing.Point(149, 9);
			this.mouseActivityLabel.Name = "mouseActivityLabel";
			this.mouseActivityLabel.Size = new System.Drawing.Size(13, 13);
			this.mouseActivityLabel.TabIndex = 0;
			this.mouseActivityLabel.Text = "0";
			// 
			// injectedMouseActivityLabel
			// 
			this.injectedMouseActivityLabel.AutoSize = true;
			this.injectedMouseActivityLabel.Location = new System.Drawing.Point(149, 58);
			this.injectedMouseActivityLabel.Name = "injectedMouseActivityLabel";
			this.injectedMouseActivityLabel.Size = new System.Drawing.Size(13, 13);
			this.injectedMouseActivityLabel.TabIndex = 0;
			this.injectedMouseActivityLabel.Text = "0";
			// 
			// keyboardActivityLabel
			// 
			this.keyboardActivityLabel.AutoSize = true;
			this.keyboardActivityLabel.Location = new System.Drawing.Point(149, 34);
			this.keyboardActivityLabel.Name = "keyboardActivityLabel";
			this.keyboardActivityLabel.Size = new System.Drawing.Size(13, 13);
			this.keyboardActivityLabel.TabIndex = 0;
			this.keyboardActivityLabel.Text = "0";
			// 
			// injectedKeyboardActivityLabel
			// 
			this.injectedKeyboardActivityLabel.AutoSize = true;
			this.injectedKeyboardActivityLabel.Location = new System.Drawing.Point(149, 83);
			this.injectedKeyboardActivityLabel.Name = "injectedKeyboardActivityLabel";
			this.injectedKeyboardActivityLabel.Size = new System.Drawing.Size(13, 13);
			this.injectedKeyboardActivityLabel.TabIndex = 0;
			this.injectedKeyboardActivityLabel.Text = "0";
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(211, 108);
			this.Controls.Add(this.injectedKeyboardActivityLabel);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.keyboardActivityLabel);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.injectedMouseActivityLabel);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.mouseActivityLabel);
			this.Controls.Add(this.label1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "Form1";
			this.Text = "Activity detector";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label mouseActivityLabel;
		private System.Windows.Forms.Label injectedMouseActivityLabel;
		private System.Windows.Forms.Label keyboardActivityLabel;
		private System.Windows.Forms.Label injectedKeyboardActivityLabel;
	}
}

