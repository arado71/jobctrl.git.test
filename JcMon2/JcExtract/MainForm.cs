using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using JcExtract.Managers;
using JcMon2.Extraction;

namespace JcExtract
{
	public partial class MainForm : Form
	{
		private readonly SynchronizationContext context;
		private readonly CaptureManager captureManager = new CaptureManager();

		public MainForm()
		{
			InitializeComponent();
			context = SynchronizationContext.Current;
			captureManager.Captured += HandleCaptured;
		}

		private void HandleCaptured(object sender, CaptureEventArgs eventArgs)
		{
			context.Post(_ => ShowCapture(eventArgs.Capture), null);
		}

		private void ShowCapture(Capture capture)
		{
			dataGridView1.Rows.Clear();
			lblHandle.Text = "Handle: " + capture.WindowHandle + " (" + capture.ProcessName + ")";
			foreach (var capturedValue in capture.Values)
			{
				dataGridView1.Rows.Add(capturedValue.Key, capturedValue.Time, capturedValue.Value);
			}
		}

		private void HandleFormLoaded(object sender, EventArgs e)
		{
			captureManager.Start();
		}

		private void HandleSettingsClicked(object sender, EventArgs e)
		{
			captureManager.Stop();
			var settingsForm = new Settings();
			settingsForm.ShowDialog(this);
			captureManager.Start();
		}

		private void HandleCompileClicked(object sender, EventArgs e)
		{
			try
			{
				Configuration.ProcessFuncs = AutomationScriptHelper.Compile(txtScript.Text);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}


	}
}
