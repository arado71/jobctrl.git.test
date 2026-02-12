namespace JC.Removal
{
	partial class Form1
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
			this.removeButton = new System.Windows.Forms.Button();
			this.productComboBox = new System.Windows.Forms.ComboBox();
			this.registryCheckBox = new System.Windows.Forms.CheckBox();
			this.chromeExtensionCheckBox = new System.Windows.Forms.CheckBox();
			this.edgeExtensionCheckBox = new System.Windows.Forms.CheckBox();
			this.firefoxExtensionCheckBox = new System.Windows.Forms.CheckBox();
			this.filesCheckBox = new System.Windows.Forms.CheckBox();
			this.outlookAddinCheckBox = new System.Windows.Forms.CheckBox();
			this.taskSchedulerCheckBox = new System.Windows.Forms.CheckBox();
			this.advancedPanel = new System.Windows.Forms.Panel();
			this.advancedButton = new System.Windows.Forms.Button();
			this.advancedPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// removeButton
			// 
			this.removeButton.Enabled = false;
			this.removeButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.removeButton.Location = new System.Drawing.Point(265, 12);
			this.removeButton.Name = "removeButton";
			this.removeButton.Size = new System.Drawing.Size(75, 23);
			this.removeButton.TabIndex = 0;
			this.removeButton.Text = "Remove";
			this.removeButton.UseVisualStyleBackColor = true;
			this.removeButton.Click += new System.EventHandler(this.removeButton_Click);
			// 
			// productComboBox
			// 
			this.productComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.productComboBox.Enabled = false;
			this.productComboBox.FormattingEnabled = true;
			this.productComboBox.Location = new System.Drawing.Point(12, 12);
			this.productComboBox.Name = "productComboBox";
			this.productComboBox.Size = new System.Drawing.Size(166, 21);
			this.productComboBox.TabIndex = 1;
			this.productComboBox.SelectedIndexChanged += new System.EventHandler(this.productComboBox_SelectedIndexChanged);
			// 
			// registryCheckBox
			// 
			this.registryCheckBox.AutoSize = true;
			this.registryCheckBox.Checked = true;
			this.registryCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.registryCheckBox.Location = new System.Drawing.Point(5, 4);
			this.registryCheckBox.Name = "registryCheckBox";
			this.registryCheckBox.Size = new System.Drawing.Size(64, 17);
			this.registryCheckBox.TabIndex = 2;
			this.registryCheckBox.Text = "Registry";
			this.registryCheckBox.UseVisualStyleBackColor = true;
			// 
			// chromeExtensionCheckBox
			// 
			this.chromeExtensionCheckBox.AutoSize = true;
			this.chromeExtensionCheckBox.Checked = true;
			this.chromeExtensionCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chromeExtensionCheckBox.Location = new System.Drawing.Point(5, 27);
			this.chromeExtensionCheckBox.Name = "chromeExtensionCheckBox";
			this.chromeExtensionCheckBox.Size = new System.Drawing.Size(110, 17);
			this.chromeExtensionCheckBox.TabIndex = 2;
			this.chromeExtensionCheckBox.Text = "Chrome extension";
			this.chromeExtensionCheckBox.UseVisualStyleBackColor = true;
			// 
			// edgeExtensionCheckBox
			// 
			this.edgeExtensionCheckBox.AutoSize = true;
			this.edgeExtensionCheckBox.Checked = true;
			this.edgeExtensionCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.edgeExtensionCheckBox.Location = new System.Drawing.Point(5, 50);
			this.edgeExtensionCheckBox.Name = "edgeExtensionCheckBox";
			this.edgeExtensionCheckBox.Size = new System.Drawing.Size(99, 17);
			this.edgeExtensionCheckBox.TabIndex = 2;
			this.edgeExtensionCheckBox.Text = "Edge extension";
			this.edgeExtensionCheckBox.UseVisualStyleBackColor = true;
			// 
			// firefoxExtensionCheckBox
			// 
			this.firefoxExtensionCheckBox.AutoSize = true;
			this.firefoxExtensionCheckBox.Checked = true;
			this.firefoxExtensionCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.firefoxExtensionCheckBox.Location = new System.Drawing.Point(5, 73);
			this.firefoxExtensionCheckBox.Name = "firefoxExtensionCheckBox";
			this.firefoxExtensionCheckBox.Size = new System.Drawing.Size(105, 17);
			this.firefoxExtensionCheckBox.TabIndex = 2;
			this.firefoxExtensionCheckBox.Text = "Firefox extension";
			this.firefoxExtensionCheckBox.UseVisualStyleBackColor = true;
			// 
			// filesCheckBox
			// 
			this.filesCheckBox.AutoSize = true;
			this.filesCheckBox.Checked = true;
			this.filesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.filesCheckBox.Location = new System.Drawing.Point(5, 142);
			this.filesCheckBox.Name = "filesCheckBox";
			this.filesCheckBox.Size = new System.Drawing.Size(47, 17);
			this.filesCheckBox.TabIndex = 3;
			this.filesCheckBox.Text = "Files";
			this.filesCheckBox.UseVisualStyleBackColor = true;
			// 
			// outlookAddinCheckBox
			// 
			this.outlookAddinCheckBox.AutoSize = true;
			this.outlookAddinCheckBox.Checked = true;
			this.outlookAddinCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.outlookAddinCheckBox.Location = new System.Drawing.Point(5, 96);
			this.outlookAddinCheckBox.Name = "outlookAddinCheckBox";
			this.outlookAddinCheckBox.Size = new System.Drawing.Size(92, 17);
			this.outlookAddinCheckBox.TabIndex = 2;
			this.outlookAddinCheckBox.Text = "Outlook addin";
			this.outlookAddinCheckBox.UseVisualStyleBackColor = true;
			// 
			// taskSchedulerCheckBox
			// 
			this.taskSchedulerCheckBox.AutoSize = true;
			this.taskSchedulerCheckBox.Checked = true;
			this.taskSchedulerCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.taskSchedulerCheckBox.Location = new System.Drawing.Point(5, 119);
			this.taskSchedulerCheckBox.Name = "taskSchedulerCheckBox";
			this.taskSchedulerCheckBox.Size = new System.Drawing.Size(99, 17);
			this.taskSchedulerCheckBox.TabIndex = 2;
			this.taskSchedulerCheckBox.Text = "Task scheduler";
			this.taskSchedulerCheckBox.UseVisualStyleBackColor = true;
			// 
			// advancedPanel
			// 
			this.advancedPanel.Controls.Add(this.filesCheckBox);
			this.advancedPanel.Controls.Add(this.taskSchedulerCheckBox);
			this.advancedPanel.Controls.Add(this.outlookAddinCheckBox);
			this.advancedPanel.Controls.Add(this.firefoxExtensionCheckBox);
			this.advancedPanel.Controls.Add(this.edgeExtensionCheckBox);
			this.advancedPanel.Controls.Add(this.chromeExtensionCheckBox);
			this.advancedPanel.Controls.Add(this.registryCheckBox);
			this.advancedPanel.Location = new System.Drawing.Point(12, 39);
			this.advancedPanel.Name = "advancedPanel";
			this.advancedPanel.Size = new System.Drawing.Size(328, 160);
			this.advancedPanel.TabIndex = 4;
			// 
			// advancedButton
			// 
			this.advancedButton.Location = new System.Drawing.Point(184, 12);
			this.advancedButton.Name = "advancedButton";
			this.advancedButton.Size = new System.Drawing.Size(75, 23);
			this.advancedButton.TabIndex = 0;
			this.advancedButton.Text = "Advanced";
			this.advancedButton.UseVisualStyleBackColor = true;
			this.advancedButton.Click += new System.EventHandler(this.advancedButton_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(350, 205);
			this.Controls.Add(this.advancedPanel);
			this.Controls.Add(this.productComboBox);
			this.Controls.Add(this.advancedButton);
			this.Controls.Add(this.removeButton);
			this.Name = "Form1";
			this.Text = "JobCTRL Removal Tool";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.advancedPanel.ResumeLayout(false);
			this.advancedPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button removeButton;
		private System.Windows.Forms.ComboBox productComboBox;
		private System.Windows.Forms.CheckBox registryCheckBox;
		private System.Windows.Forms.CheckBox chromeExtensionCheckBox;
		private System.Windows.Forms.CheckBox edgeExtensionCheckBox;
		private System.Windows.Forms.CheckBox firefoxExtensionCheckBox;
		private System.Windows.Forms.CheckBox filesCheckBox;
		private System.Windows.Forms.CheckBox outlookAddinCheckBox;
		private System.Windows.Forms.CheckBox taskSchedulerCheckBox;
		private System.Windows.Forms.Panel advancedPanel;
		private System.Windows.Forms.Button advancedButton;
	}
}

