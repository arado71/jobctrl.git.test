using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using JavaMon;
using JC.IAccessibility;
using JCAutomation.Properties;

namespace JCAutomation.View
{
	public partial class MainForm : Form
	{
		private SpyForm jcmon;
		private TouchForm jcmon2;
		private JavaAccessibilityForm javaMon;
		private JCIAccessibilityForm jciaMon;

		public MainForm()
		{
			InitializeComponent();
			var version = new ToolStripMenuItem
			{
				Name = "version",
				Text = "version " + ((AssemblyFileVersionAttribute)Attribute.GetCustomAttribute(
						   Assembly.GetExecutingAssembly(),
						   typeof(AssemblyFileVersionAttribute), false)
					   ).Version
			};
			var separator = new ToolStripSeparator();
			var exitToolStripMenuItem = new ToolStripMenuItem
			{
				Name = "exitToolStripMenuItem",
				Size = new System.Drawing.Size(181, 26),
				Text = "&Exit"
			};
			exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);

			mnuFile.DropDownItems.AddRange(new ToolStripItem[] { version, separator, exitToolStripMenuItem });
		}
		private void jcMonToolStripMenuItem_Click(object sender, System.EventArgs e)
		{
			if (jcmon == null || jcmon.IsDisposed) jcmon = new SpyForm();
			jcmon.MdiParent = this;
			jcmon.WindowState = FormWindowState.Maximized;
			jcmon.Show();
			jcmon.BringToFront();
		}
		private void jcMon2ToolStripMenuItem_Click(object sender, System.EventArgs e)
		{
			if (jcmon2 == null || jcmon2.IsDisposed) jcmon2 = new TouchForm();
			jcmon2.MdiParent = this;
			jcmon2.WindowState = FormWindowState.Maximized;
			jcmon2.Show();
			jcmon2.BringToFront();
		}
		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}

		private void javaMonToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if(javaMon == null || javaMon.IsDisposed) javaMon = new JavaAccessibilityForm();
			javaMon.MdiParent = this;
			javaMon.WindowState = FormWindowState.Maximized;
			javaMon.Show();
			javaMon.BringToFront();
		}

		private void javaMonx64ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			if (directoryName == null) throw new Exception("Couldn't get this program's location.");
			var javaMon64FileName = Path.Combine(directoryName,
				"JavaAccessibility\\x64\\JavaMon.exe");
			ProcessStartInfo javaMon64ProcessInfo = new ProcessStartInfo(javaMon64FileName);
			Process p = Process.Start(javaMon64ProcessInfo);
			p.EnableRaisingEvents = true;
			p.Exited += Java64Exited;
			WindowState = FormWindowState.Minimized;
		}

		private void Java64Exited(object sender, EventArgs e)
		{
			Invoke(new Action(() => WindowState = FormWindowState.Normal));
		}

		private void jcAccessibilityToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (jciaMon == null || jciaMon.IsDisposed) jciaMon = new JCIAccessibilityForm();
			jciaMon.MdiParent = this;
			jciaMon.WindowState = FormWindowState.Maximized;
			jciaMon.Show();
			jciaMon.BringToFront();
		}
	}
}

