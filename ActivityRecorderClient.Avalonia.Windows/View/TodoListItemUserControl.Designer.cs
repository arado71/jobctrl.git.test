namespace Tct.ActivityRecorderClient.View
{
	partial class TodoListItemUserControl
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
			this.stateButton = new System.Windows.Forms.Button();
			this.contentTextBox = new System.Windows.Forms.TextBox();
			this.deleteButton = new System.Windows.Forms.Button();
			this.metroToolTip = new MetroFramework.Components.MetroToolTip();
			this.pictureBox = new System.Windows.Forms.PictureBox();
			this.dateLabel = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// stateButton
			// 
			this.stateButton.BackColor = System.Drawing.SystemColors.Control;
			this.stateButton.BackgroundImage = global::Tct.ActivityRecorderClient.Properties.Resources.recent;
			this.stateButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			this.stateButton.FlatAppearance.BorderSize = 0;
			this.stateButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.stateButton.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.stateButton.Location = new System.Drawing.Point(24, 0);
			this.stateButton.Name = "stateButton";
			this.stateButton.Size = new System.Drawing.Size(22, 22);
			this.stateButton.TabIndex = 1;
			this.metroToolTip.SetToolTip(this.stateButton, "Opened");
			this.stateButton.UseVisualStyleBackColor = false;
			this.stateButton.Click += new System.EventHandler(this.stateButton_Click);
			// 
			// contentTextBox
			// 
			this.contentTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.contentTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.contentTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.contentTextBox.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.contentTextBox.Location = new System.Drawing.Point(53, 0);
			this.contentTextBox.MaxLength = 200;
			this.contentTextBox.Name = "contentTextBox";
			this.contentTextBox.Size = new System.Drawing.Size(528, 20);
			this.contentTextBox.TabIndex = 0;
			this.contentTextBox.TextChanged += new System.EventHandler(this.contentTextBox_TextChanged);
			this.contentTextBox.Enter += new System.EventHandler(this.contentTextBox_Enter);
			this.contentTextBox.Leave += new System.EventHandler(this.contentTextBox_Leave);
			// 
			// deleteButton
			// 
			this.deleteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.deleteButton.BackColor = System.Drawing.Color.Transparent;
			this.deleteButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.deleteButton.FlatAppearance.BorderSize = 0;
			this.deleteButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.deleteButton.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.deleteButton.Image = global::Tct.ActivityRecorderClient.Properties.Resources.btn_delete;
			this.deleteButton.Location = new System.Drawing.Point(629, 0);
			this.deleteButton.Name = "deleteButton";
			this.deleteButton.Size = new System.Drawing.Size(22, 22);
			this.deleteButton.TabIndex = 5;
			this.metroToolTip.SetToolTip(this.deleteButton, "Delete");
			this.deleteButton.UseVisualStyleBackColor = false;
			this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
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
			// pictureBox
			// 
			this.pictureBox.Cursor = System.Windows.Forms.Cursors.Default;
			this.pictureBox.Image = global::Tct.ActivityRecorderClient.Properties.Resources.further_menu;
			this.pictureBox.Location = new System.Drawing.Point(3, 0);
			this.pictureBox.Name = "pictureBox";
			this.pictureBox.Size = new System.Drawing.Size(16, 22);
			this.pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.pictureBox.TabIndex = 7;
			this.pictureBox.TabStop = false;
			this.pictureBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TodoListItemUserControl_MouseMove);
			// 
			// dateLabel
			// 
			this.dateLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.dateLabel.AutoSize = true;
			this.dateLabel.ForeColor = System.Drawing.SystemColors.GrayText;
			this.dateLabel.Location = new System.Drawing.Point(587, 5);
			this.dateLabel.Name = "dateLabel";
			this.dateLabel.Size = new System.Drawing.Size(36, 13);
			this.dateLabel.TabIndex = 8;
			this.dateLabel.Text = "5w 6d";
			// 
			// TodoListItemUserControl
			// 
			this.AllowDrop = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Transparent;
			this.Controls.Add(this.dateLabel);
			this.Controls.Add(this.pictureBox);
			this.Controls.Add(this.contentTextBox);
			this.Controls.Add(this.deleteButton);
			this.Controls.Add(this.stateButton);
			this.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
			this.Name = "TodoListItemUserControl";
			this.Size = new System.Drawing.Size(654, 22);
			this.Load += new System.EventHandler(this.TodoListItemUserControl_Load);
			this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TodoListItemUserControl_MouseMove);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button stateButton;
		private System.Windows.Forms.TextBox contentTextBox;
		private System.Windows.Forms.Button deleteButton;
		private MetroFramework.Components.MetroToolTip metroToolTip;
		private System.Windows.Forms.PictureBox pictureBox;
		private System.Windows.Forms.Label dateLabel;
	}
}
