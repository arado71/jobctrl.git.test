using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MetroFramework.Forms;
using Tct.ActivityRecorderClient.Capturing.Plugins.Impl;

namespace Tct.ActivityRecorderClient.View
{
	public partial class DomCaptureLoadForm : FixedMetroForm
	{
		private List<DomSettings> domSettings;
		public List<DomSettings> DomSettings
		{
			get { return domSettings; }
			private set { domSettings = value; }
		}

		public DomCaptureLoadForm()
		{
			InitializeComponent();

			//load clipboard data if its a valid setting
			try
			{
				var clip = Clipboard.GetText();
				List<DomSettings> loaded;
				JsonHelper.DeserializeData(clip, out loaded);
				if (loaded != null)
				{
					txtJson.Text = clip;
				}
			}
			catch
			{
				//we don't care about this error
			}
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			this.Close();
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			try
			{
				JsonHelper.DeserializeData(txtJson.Text, out domSettings);
				DialogResult = DialogResult.OK;
				this.Close();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Unable to create settings from json: " + ex.Message);
			}
		}

		private void txtJson_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char)Keys.Return) btnOk_Click(this, EventArgs.Empty);
		}
	}
}
