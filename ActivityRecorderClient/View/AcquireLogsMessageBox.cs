using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tct.ActivityRecorderClient.View
{
	public partial class AcquireLogsMessageBox : FixedMetroForm
	{
		public bool Remember { get; set; }

		public AcquireLogsMessageBox()
		{
			InitializeComponent();
		}

		private void noButton_Click(object sender, EventArgs e)
		{
			Remember = rememberCheckBox.Checked;
			DialogResult = DialogResult.No;
			Close();
		}

		private void yesButton_Click(object sender, EventArgs e)
		{
			Remember = rememberCheckBox.Checked;
			DialogResult = DialogResult.Yes;
			Close();
		}

		private void AcquireLogsMessageBox_Load(object sender, EventArgs e)
		{
			Text = Labels.AcquireLogs_MessageBoxTitle;
			rememberCheckBox.Text = Labels.AcquireLogs_Remember;
			yesButton.Text = Labels.Yes;
			noButton.Text = Labels.No;
			Icon = Properties.Resources.JobCtrl; //don't set it in the designer as it would enlarge the exe
		}
	}
}
