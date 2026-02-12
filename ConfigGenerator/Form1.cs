using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace ConfigGenerator
{
	public partial class Form1 : Form
	{
		//private readonly object[] configKeys = { "AppClassifier", "AutoUpdateManagerEnabled", "GoogleClientId", "GoogleClientSecret", "IsLoginRememberPasswordChecked", "IsRoamingStorageScopeNeeded", "IsRunAsAdminDefault", "ValidCertificate", "WebsiteUrl" };
		private readonly List<string> configKeys = new List<string>();
		private List<XElement> endpoints;

		public Form1()
		{
			InitializeComponent();
			LoadConfig(Path.Combine("ConfigTemplate", "app.Release.config"));
		}

		private void tbCompanyName_TextChanged(object sender, EventArgs e)
		{
			tbConfigName.Text = tbCompanyName.Text;
			tbDisplayName.Text = tbConfigName.Text;
		}

		private void btnAddRow_Click(object sender, EventArgs e)
		{
			AddRowToAppSettings();
		}

		private void AddRowToAppSettings()
		{
			tableLayoutPanel3.Controls.Remove(btnAddRow);
			tableLayoutPanel3.RowCount++;
			tableLayoutPanel3.RowStyles.Add(new RowStyle());
			tableLayoutPanel3.Controls.Add(CreateCombobox(), 0, tableLayoutPanel3.RowCount - 2);
			tableLayoutPanel3.Controls.Add(CreateTextbox(), 1, tableLayoutPanel3.RowCount - 2);
			tableLayoutPanel3.Controls.Add(btnAddRow, 0, tableLayoutPanel3.RowCount - 1);
		}

		private ComboBox CreateCombobox()
		{
			var cb = new ComboBox
			{
				Anchor = ((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left) | AnchorStyles.Right,
				DropDownStyle = ComboBoxStyle.DropDown,
				FormattingEnabled = true,
				Sorted = true
			};
			cb.Items.AddRange(configKeys.ToArray());
			return cb;
		}

		private TextBox CreateTextbox()
		{
			return new TextBox
			{
				Anchor = ((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left) | AnchorStyles.Right,
			};
		}

		private Dictionary<string, string> GetAppSettings()
		{
			var dict = new Dictionary<string, string>();
			for (int row = 0; row < tableLayoutPanel3.RowCount - 1; row++)
			{
				dict.Add(tableLayoutPanel3.GetControlFromPosition(0, row).Text, tableLayoutPanel3.GetControlFromPosition(1, row).Text);
			}
			return dict;
		}

		private void btnGenerate_Click(object sender, EventArgs e)
		{
			try
			{
				var test = GetAppSettings();
			}
			catch (ArgumentException ex)
			{
				if (!ex.Message.Equals("An item with the same key has already been added.")) throw;
				MessageBox.Show("Configuration error in AppSettings section. Same key can't be configured more than once.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void LoadConfig(string path)
		{
			var xml = XDocument.Load(path);
			if (xml.Root == null)
			{
				MessageBox.Show("Loading of the config has failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			LoadEndpoints(xml);
			LoadAppSettings(xml);
		}

		private void LoadAppSettings(XDocument xml)
		{
			var appSettings = xml.Root.Elements("appSettings").Elements("add").Where(e => e.Attributes("value").Any()).ToList();
			configKeys.AddRange(appSettings.Select(ap => ap.Attribute("key")?.Value));
			cbConfigKey1.Items.AddRange(configKeys.ToArray());
			int row = 0;
			foreach (var appSetting in appSettings)
			{
				if (tableLayoutPanel3.GetControlFromPosition(0, row) is Button)
				{
					AddRowToAppSettings();
				}
				var key = appSetting.Attribute("key")?.Value;
				tableLayoutPanel3.GetControlFromPosition(0, row).Text = key;
				tableLayoutPanel3.GetControlFromPosition(1, row).Text = appSetting.Attribute("value")?.Value;
				row++;
			}
		}

		private void LoadEndpoints(XDocument xml)
		{
			endpoints = xml.Root.Elements("system.serviceModel").Elements("client").Elements("endpoint").ToList();
			endpoints.ForEach(e =>
			{
				var epName = e.Attribute("name")?.Value;
				if (epName != null) { clbEndpoints.Items.Add(epName); }
			});
		}

		private void btnLoad_Click(object sender, EventArgs e)
		{
			using (OpenFileDialog dlg = new OpenFileDialog())
			{
				dlg.InitialDirectory = Directory.GetCurrentDirectory();
				dlg.Filter = "Config files (*.config)|*.config";
				if (dlg.ShowDialog() == DialogResult.OK)
				{
					var filePath = dlg.FileName;
					var configName = dlg.SafeFileName?.Replace("app.Release.", "").Replace(".config", "");
					LoadConfig(filePath);
					lbLoadedConfig.Text = configName + " config loaded";
				}
			}
		}

		private void clbEndpoints_SelectedIndexChanged(object sender, EventArgs e)
		{
			var selectedEndpoint = endpoints.Single(ep => clbEndpoints.SelectedItem.ToString().Equals(ep.Attribute("name")?.Value));
			tbUrl.Enabled = tbPublicKey.Enabled = clbEndpoints.CheckedItems.Contains(clbEndpoints.SelectedItem);
			tbUrl.Text = selectedEndpoint.Attribute("address")?.Value;
			tbPublicKey.Text = selectedEndpoint.Element("identity")?.Element("certificate")?.Attribute("encodedValue")?.Value;
		}
	}
}
