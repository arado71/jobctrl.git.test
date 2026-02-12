using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Tct.ActivityRecorderClient.Communication;

namespace Tct.ActivityRecorderClient.View
{
	public partial class LoginSettings : FixedMetroForm
	{
		public LoginSettings()
		{
			InitializeComponent();
			RefreshControls();
			RefreshLocalization();
			errorProvider1.SetIconAlignment(tbAddress, ErrorIconAlignment.MiddleLeft);
		}

		private void RefreshLocalization()
		{
			lblAddress.Text = Labels.LoginSettings_Address;
			lblUsername.Text = Labels.LoginSettings_Username;
			lblPassword.Text = Labels.LoginSettings_Password;
			rbAutomatic.Text = Labels.LoginSettings_Automatic;
			rbCustom.Text = Labels.LoginSettings_Custom;
			btnSave.Text = Labels.LoginSettings_Save;
			Text = Labels.LoginSettings_Title;
		}

		private void RefreshControls()
		{
			var settings = ConfigManager.Proxy;
			rbAutomatic.Checked = settings.IsAutomatic;
			rbCustom.Checked = !settings.IsAutomatic;
			if (!settings.IsAutomatic)
			{
				tbAddress.Text = settings.Address;
				tbUsername.Text = settings.Username;
				tbPassword.Text = settings.Password;
			}
		}

		private void HandleProxyModeChanged(object sender, EventArgs e)
		{
			var isAutomatic = rbAutomatic.Checked;
			pCustom.Enabled = !isAutomatic;
		}

		private void HandleSaveClicked(object sender, EventArgs e)
		{
			var isAutomatic = rbAutomatic.Checked;
			ConfigManager.ProxySettings settings;
			if (isAutomatic)
			{
				settings = new ConfigManager.ProxySettings()
				{
					IsAutomatic = true,
				};
			}
			else
			{
				var address = tbAddress.Text;
				if (!address.Contains("://"))
				{
					address = "http://" + address;
				}

				if (!Uri.IsWellFormedUriString(address, UriKind.Absolute))
				{
					errorProvider1.SetError(tbAddress, Labels.LoginSettings_InvalidAddress);
					return;
				}

				errorProvider1.SetError(tbAddress, "");
				settings = new ConfigManager.ProxySettings()
				{
					IsAutomatic = false,
					Address = address,
					Username = tbUsername.Text,
					Password = tbPassword.Text,
				};
			}

			ConfigManager.Proxy = settings;
			ActivityRecorderClientWrapper.SetProxy(settings);
			Close();
		}
	}
}
