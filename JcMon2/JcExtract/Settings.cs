using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using JcMon2;
using JcMon2.Extraction;

namespace JcExtract
{
	public partial class Settings : Form
	{
		public Settings()
		{
			InitializeComponent();
		}

		private void HandleIntervalChanged(object sender, EventArgs e)
		{
			Configuration.CaptureInterval = (int)numericUpDown1.Value;
		}

		private void HandleFormLoaded(object sender, EventArgs e)
		{
			numericUpDown1.Value = Configuration.CaptureInterval;
		}
	}
}
