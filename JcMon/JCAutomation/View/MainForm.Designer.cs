using System.Windows.Forms;

namespace JCAutomation.View
{
	public partial class MainForm
	{

        private void InitializeComponent()
        {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.mnuFile = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuJcmon = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuJcmon2 = new System.Windows.Forms.ToolStripMenuItem();
			this.jcAccessibilityToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.javaMonToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.javaMonx64ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// menuStrip1
			// 
			this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuFile,
            this.mnuJcmon,
            this.mnuJcmon2,
            this.jcAccessibilityToolStripMenuItem,
            this.javaMonToolStripMenuItem,
            this.javaMonx64ToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Padding = new System.Windows.Forms.Padding(4, 2, 0, 2);
			this.menuStrip1.Size = new System.Drawing.Size(710, 24);
			this.menuStrip1.TabIndex = 0;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// mnuFile
			// 
			this.mnuFile.Name = "mnuFile";
			this.mnuFile.Size = new System.Drawing.Size(37, 20);
			this.mnuFile.Text = "&File";
			// 
			// mnuJcmon
			// 
			this.mnuJcmon.Name = "mnuJcmon";
			this.mnuJcmon.Size = new System.Drawing.Size(53, 20);
			this.mnuJcmon.Text = "jcMon";
			this.mnuJcmon.Click += new System.EventHandler(this.jcMonToolStripMenuItem_Click);
			// 
			// mnuJcmon2
			// 
			this.mnuJcmon2.Name = "mnuJcmon2";
			this.mnuJcmon2.Size = new System.Drawing.Size(60, 20);
			this.mnuJcmon2.Text = "JcMon2";
			this.mnuJcmon2.Click += new System.EventHandler(this.jcMon2ToolStripMenuItem_Click);
			// 
			// jcAccessibilityToolStripMenuItem
			// 
			this.jcAccessibilityToolStripMenuItem.Name = "jcAccessibilityToolStripMenuItem";
			this.jcAccessibilityToolStripMenuItem.Size = new System.Drawing.Size(94, 20);
			this.jcAccessibilityToolStripMenuItem.Text = "JcAccessibility";
			this.jcAccessibilityToolStripMenuItem.Click += new System.EventHandler(this.jcAccessibilityToolStripMenuItem_Click);
			// 
			// javaMonToolStripMenuItem
			// 
			this.javaMonToolStripMenuItem.Name = "javaMonToolStripMenuItem";
			this.javaMonToolStripMenuItem.Size = new System.Drawing.Size(66, 20);
			this.javaMonToolStripMenuItem.Text = "JavaMon";
			this.javaMonToolStripMenuItem.Click += new System.EventHandler(this.javaMonToolStripMenuItem_Click);
			// 
			// javaMonx64ToolStripMenuItem
			// 
			this.javaMonx64ToolStripMenuItem.Name = "javaMonx64ToolStripMenuItem";
			this.javaMonx64ToolStripMenuItem.Size = new System.Drawing.Size(89, 20);
			this.javaMonx64ToolStripMenuItem.Text = "JavaMon_x64";
			this.javaMonx64ToolStripMenuItem.Click += new System.EventHandler(this.javaMonx64ToolStripMenuItem_Click);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(6, 6);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(710, 691);
			this.Controls.Add(this.menuStrip1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.IsMdiContainer = true;
			this.MainMenuStrip = this.menuStrip1;
			this.MinimumSize = new System.Drawing.Size(515, 249);
			this.Name = "MainForm";
			this.Text = "JC Automation";
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private MenuStrip menuStrip1;
		private ToolStripMenuItem mnuFile;
		private ToolStripMenuItem mnuJcmon;
		private ToolStripMenuItem mnuJcmon2;
		private ToolStripSeparator toolStripMenuItem1;
		private ToolStripMenuItem javaMonToolStripMenuItem;
		private ToolStripMenuItem javaMonx64ToolStripMenuItem;
		private ToolStripMenuItem jcAccessibilityToolStripMenuItem;
	}
}
