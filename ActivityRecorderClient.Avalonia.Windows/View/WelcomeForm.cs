using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Tct.ActivityRecorderClient.Properties;

namespace Tct.ActivityRecorderClient.View
{
	public partial class WelcomeForm : FixedMetroForm
	{
		public WelcomeForm()
		{
			InitializeComponent();
			LoadLocalization();
			Icon = Resources.JobCtrl;
		}

		private void LoadLocalization()
		{
			btnOk.Text = Labels.Ok;
			Text = Labels.NewFeaturesTitle;
			if (new CultureInfo("hu-HU").Equals(Labels.Culture))
			{
				pbEN.Visible = false;
			}
			else
			{
				pbHU.Visible = false;
			}
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
