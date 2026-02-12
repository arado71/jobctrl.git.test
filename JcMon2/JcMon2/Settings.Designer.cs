namespace JcMon2
{
	partial class Settings
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
			this.numUpdateInterval = new System.Windows.Forms.NumericUpDown();
			this.cbCaptureCom = new System.Windows.Forms.CheckBox();
			this.label2 = new System.Windows.Forms.Label();
			this.numSaveInterval = new System.Windows.Forms.NumericUpDown();
			this.cbScreenshots = new System.Windows.Forms.CheckBox();
			((System.ComponentModel.ISupportInitialize)(this.numUpdateInterval)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numSaveInterval)).BeginInit();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 14);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(101, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Update interval (ms)";
			// 
			// numUpdateInterval
			// 
			this.numUpdateInterval.Location = new System.Drawing.Point(175, 12);
			this.numUpdateInterval.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
			this.numUpdateInterval.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
			this.numUpdateInterval.Name = "numUpdateInterval";
			this.numUpdateInterval.Size = new System.Drawing.Size(97, 20);
			this.numUpdateInterval.TabIndex = 1;
			this.numUpdateInterval.Value = new decimal(new int[] {
            300,
            0,
            0,
            0});
			this.numUpdateInterval.ValueChanged += new System.EventHandler(this.HandleUpdateIntervalChanged);
			// 
			// cbCaptureCom
			// 
			this.cbCaptureCom.AutoSize = true;
			this.cbCaptureCom.Checked = true;
			this.cbCaptureCom.CheckState = System.Windows.Forms.CheckState.Checked;
			this.cbCaptureCom.Location = new System.Drawing.Point(12, 73);
			this.cbCaptureCom.Name = "cbCaptureCom";
			this.cbCaptureCom.Size = new System.Drawing.Size(142, 17);
			this.cbCaptureCom.TabIndex = 3;
			this.cbCaptureCom.Text = "COM interfaces as notes";
			this.cbCaptureCom.UseVisualStyleBackColor = true;
			this.cbCaptureCom.CheckedChanged += new System.EventHandler(this.HandleComCheckboxChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(9, 40);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(133, 13);
			this.label2.TabIndex = 4;
			this.label2.Text = "Recording save interval (s)";
			// 
			// numSaveInterval
			// 
			this.numSaveInterval.Location = new System.Drawing.Point(175, 38);
			this.numSaveInterval.Maximum = new decimal(new int[] {
            600,
            0,
            0,
            0});
			this.numSaveInterval.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
			this.numSaveInterval.Name = "numSaveInterval";
			this.numSaveInterval.Size = new System.Drawing.Size(97, 20);
			this.numSaveInterval.TabIndex = 5;
			this.numSaveInterval.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
			this.numSaveInterval.ValueChanged += new System.EventHandler(this.HandleSaveIntervalChanged);
			// 
			// cbScreenshots
			// 
			this.cbScreenshots.AutoSize = true;
			this.cbScreenshots.Location = new System.Drawing.Point(11, 96);
			this.cbScreenshots.Name = "cbScreenshots";
			this.cbScreenshots.Size = new System.Drawing.Size(111, 17);
			this.cbScreenshots.TabIndex = 6;
			this.cbScreenshots.Text = "Save screenshots";
			this.cbScreenshots.UseVisualStyleBackColor = true;
			this.cbScreenshots.CheckedChanged += new System.EventHandler(this.HandleScreenshotChanged);
			// 
			// Settings
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 182);
			this.Controls.Add(this.cbScreenshots);
			this.Controls.Add(this.numSaveInterval);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.cbCaptureCom);
			this.Controls.Add(this.numUpdateInterval);
			this.Controls.Add(this.label1);
			this.MinimumSize = new System.Drawing.Size(300, 221);
			this.Name = "Settings";
			this.Text = "Settings";
			((System.ComponentModel.ISupportInitialize)(this.numUpdateInterval)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numSaveInterval)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.NumericUpDown numUpdateInterval;
		private System.Windows.Forms.CheckBox cbCaptureCom;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.NumericUpDown numSaveInterval;
		private System.Windows.Forms.CheckBox cbScreenshots;
	}
}