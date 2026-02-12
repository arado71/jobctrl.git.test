using System;
using System.Windows.Forms;
using log4net;

namespace JCAutomation.View
{
	public partial class Settings : Form
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public Settings()
		{
			InitializeComponent();
			numUpdateInterval.Value = Configuration.CaptureInterval;
			cbCaptureCom.Checked = Configuration.CaptureCom;
			cbScreenshots.Checked = Configuration.IncludeScreenshots;
			numSaveInterval.Value = Configuration.RecordInterval / 1000;
		}

		private void HandleUpdateIntervalChanged(object sender, EventArgs e)
		{
			Configuration.CaptureInterval = (int)numUpdateInterval.Value;
		}

		private void HandleComCheckboxChanged(object sender, EventArgs e)
		{
			Configuration.CaptureCom = cbCaptureCom.Checked;
		}

		private void HandleScreenshotChanged(object sender, EventArgs e)
		{
			Configuration.IncludeScreenshots = cbScreenshots.Checked;
		}

		private void HandleSaveIntervalChanged(object sender, EventArgs e)
		{
			Configuration.RecordInterval = ((int)numSaveInterval.Value) * 1000;
		}
	}
}
