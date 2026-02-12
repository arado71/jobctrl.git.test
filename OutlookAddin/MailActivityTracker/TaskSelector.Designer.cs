namespace MailActivityTracker
{
    partial class TaskSelector
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
            this.lbTask = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // lbTask
            // 
            this.lbTask.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbTask.FormattingEnabled = true;
            this.lbTask.ItemHeight = 16;
            this.lbTask.Location = new System.Drawing.Point(0, 0);
            this.lbTask.Name = "lbTask";
            this.lbTask.Size = new System.Drawing.Size(282, 253);
            this.lbTask.TabIndex = 0;
            this.lbTask.Click += new System.EventHandler(this.lbTask_Click);
            this.lbTask.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lbTask_KeyDown);
            this.lbTask.KeyUp += new System.Windows.Forms.KeyEventHandler(this.lbTask_KeyUp);
            // 
            // TaskSelector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(282, 253);
            this.Controls.Add(this.lbTask);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "TaskSelector";
            this.Text = "TaskResults";
            this.Deactivate += new System.EventHandler(this.TaskSelector_Deactivate);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.TaskSelector_FormClosed);
            this.Load += new System.EventHandler(this.TaskSelector_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox lbTask;
    }
}