namespace Tct.ActivityRecorderClient.View
{
	partial class TodoListStateSelectorUserControl
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
			this.stateCanceledButton = new System.Windows.Forms.Button();
			this.statePostponedButton = new System.Windows.Forms.Button();
			this.stateFinishedButton = new System.Windows.Forms.Button();
			this.stateOpenedButton = new System.Windows.Forms.Button();
			this.metroToolTip = new MetroFramework.Components.MetroToolTip();
			this.SuspendLayout();
			// 
			// stateCanceledButton
			// 
			this.stateCanceledButton.BackColor = System.Drawing.SystemColors.Control;
			this.stateCanceledButton.BackgroundImage = global::Tct.ActivityRecorderClient.Properties.Resources.cancel;
			this.stateCanceledButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.stateCanceledButton.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.stateCanceledButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.stateCanceledButton.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.stateCanceledButton.Location = new System.Drawing.Point(87, 0);
			this.stateCanceledButton.Name = "stateCanceledButton";
			this.stateCanceledButton.Size = new System.Drawing.Size(22, 22);
			this.stateCanceledButton.TabIndex = 12;
			this.stateCanceledButton.UseVisualStyleBackColor = false;
			this.stateCanceledButton.Click += new System.EventHandler(this.button_Click);
			// 
			// statePostponedButton
			// 
			this.statePostponedButton.BackColor = System.Drawing.SystemColors.Control;
			this.statePostponedButton.BackgroundImage = global::Tct.ActivityRecorderClient.Properties.Resources.deadline;
			this.statePostponedButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.statePostponedButton.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.statePostponedButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.statePostponedButton.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.statePostponedButton.Location = new System.Drawing.Point(58, 0);
			this.statePostponedButton.Name = "statePostponedButton";
			this.statePostponedButton.Size = new System.Drawing.Size(22, 22);
			this.statePostponedButton.TabIndex = 11;
			this.statePostponedButton.UseVisualStyleBackColor = false;
			this.statePostponedButton.Click += new System.EventHandler(this.button_Click);
			// 
			// stateFinishedButton
			// 
			this.stateFinishedButton.BackColor = System.Drawing.SystemColors.Control;
			this.stateFinishedButton.BackgroundImage = global::Tct.ActivityRecorderClient.Properties.Resources.btn_ok;
			this.stateFinishedButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			this.stateFinishedButton.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.stateFinishedButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.stateFinishedButton.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.stateFinishedButton.Location = new System.Drawing.Point(29, 0);
			this.stateFinishedButton.Name = "stateFinishedButton";
			this.stateFinishedButton.Size = new System.Drawing.Size(22, 22);
			this.stateFinishedButton.TabIndex = 10;
			this.stateFinishedButton.UseVisualStyleBackColor = false;
			this.stateFinishedButton.Click += new System.EventHandler(this.button_Click);
			// 
			// stateOpenedButton
			// 
			this.stateOpenedButton.BackColor = System.Drawing.SystemColors.Control;
			this.stateOpenedButton.BackgroundImage = global::Tct.ActivityRecorderClient.Properties.Resources.recent;
			this.stateOpenedButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			this.stateOpenedButton.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.stateOpenedButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.stateOpenedButton.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.stateOpenedButton.Location = new System.Drawing.Point(0, 0);
			this.stateOpenedButton.Margin = new System.Windows.Forms.Padding(0);
			this.stateOpenedButton.Name = "stateOpenedButton";
			this.stateOpenedButton.Size = new System.Drawing.Size(22, 22);
			this.stateOpenedButton.TabIndex = 9;
			this.stateOpenedButton.UseVisualStyleBackColor = false;
			this.stateOpenedButton.Click += new System.EventHandler(this.button_Click);
			// 
			// metroToolTip
			// 
			this.metroToolTip.AutoPopDelay = 5000;
			this.metroToolTip.InitialDelay = 200;
			this.metroToolTip.ReshowDelay = 100;
			this.metroToolTip.Style = MetroFramework.MetroColorStyle.Blue;
			this.metroToolTip.StyleManager = null;
			this.metroToolTip.Theme = MetroFramework.MetroThemeStyle.Light;
			// 
			// TodoListStateSelectorUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.stateCanceledButton);
			this.Controls.Add(this.statePostponedButton);
			this.Controls.Add(this.stateFinishedButton);
			this.Controls.Add(this.stateOpenedButton);
			this.Margin = new System.Windows.Forms.Padding(0);
			this.Name = "TodoListStateSelectorUserControl";
			this.Size = new System.Drawing.Size(110, 22);
			this.Load += new System.EventHandler(this.TodoListStateSelectorUserControl_Load);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button stateCanceledButton;
		private System.Windows.Forms.Button statePostponedButton;
		private System.Windows.Forms.Button stateFinishedButton;
		private System.Windows.Forms.Button stateOpenedButton;
		private MetroFramework.Components.MetroToolTip metroToolTip;
	}
}
