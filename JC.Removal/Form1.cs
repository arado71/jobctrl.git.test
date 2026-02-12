using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using JC.Removal.Component;
using Microsoft.Win32;

namespace JC.Removal
{
	public partial class Form1 : Form
	{
		private readonly List<BaseComponent> jcComponents;
		private const string productsRegistryPath = @"Software\Microsoft\Installer\Products";
		private const string appPath = @"Software\Microsoft\Windows\CurrentVersion\App Paths\";

		private readonly string[] possibleProductNames =
		{
			"JobCTRL",
			"VoxCTRL"
		};

		public Form1()
		{
			jcComponents = new List<BaseComponent>();
			InitializeComponent();
		}

		private void removeButton_Click(object sender, EventArgs e)
		{
			BackgroundWorker worker = new BackgroundWorker();
			worker.DoWork += (s, args) =>
			{
				bool res = true;
				string error = null;
				string userFriendlyName = null;
				foreach (var control in advancedPanel.Controls)
				{
					if (!(control is CheckBox cb) 
					    || !cb.Enabled
					    || !cb.Checked 
					    || cb.Tag == null
					    || !(cb.Tag is BaseComponent component)) continue;
					userFriendlyName = component.GetUserFriendlyName();
					if (!(res &= component.Remove(out error))) break;
				}
				string message = $"Unable to remove {userFriendlyName}." + Environment.NewLine + error;
				args.Result = new Tuple<bool, string>(res, message);
			};
			worker.RunWorkerCompleted += (o, args) =>
			{
				Tuple<bool, string> result = (Tuple<bool, string>) args.Result;
				if (result.Item1)
				{
					MessageBox.Show("Removing completed!");
				}
				else
				{
					MessageBox.Show(result.Item2, "Error!");
				}
			};
			worker.RunWorkerAsync();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			advancedPanel.Visible = false;
			Height = 83;
			BackgroundWorker worker = new BackgroundWorker();
			worker.DoWork += (s, args) =>
			{
				List<string> result = new List<string>();
				using (var regKey = Registry.CurrentUser.OpenSubKey(productsRegistryPath, RegistryKeyPermissionCheck.ReadSubTree))
				{
					if (regKey == null) return;
					foreach (var subKeyName in regKey.GetSubKeyNames())
					{
						using (var subKey = regKey.OpenSubKey(subKeyName, RegistryKeyPermissionCheck.ReadSubTree))
						{
							if (subKey == null) return;
							var productNameValue = subKey.GetValue("ProductName");
							if (productNameValue is string productNameStringValue &&
								possibleProductNames.Any(x => productNameStringValue.Contains(x)))
								result.Add(productNameStringValue);
						}
					}
				}
				args.Result = result;
			};
			worker.RunWorkerCompleted += (o, args) =>
			{
				if (args.Result == null) return;
				List<string> products = (List<string>)args.Result;
				productComboBox.Items.AddRange(products.ToArray<object>());
				if (products.Count > 0)
				{
					productComboBox.Enabled = true;
					productComboBox.SelectedIndex = 0;
					advancedButton.Enabled = true;
				}
			};
			worker.RunWorkerAsync();
		}

		private void productComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			foreach (Control advancedPanelControl in advancedPanel.Controls)
			{
				if (advancedPanelControl is CheckBox cb)
				{
					cb.Tag = null;
				}
			}
			string product = (string) productComboBox.SelectedItem;
			registryCheckBox.Tag = new MainRegistryComponent(product);
			chromeExtensionCheckBox.Tag = new ChromeExtensionComponent();
			//edgeExtensionCheckBox.Tag = new EdgeExtensionComponent();
			firefoxExtensionCheckBox.Tag = new FirefoxExtensionComponent();
			outlookAddinCheckBox.Tag = new OutlookAddinComponent();
			taskSchedulerCheckBox.Tag = new TaskSchedulerComponent();
			try
			{
				using (var jcPath = Registry.CurrentUser.OpenSubKey(Path.Combine(appPath, product + ".exe")))
				{
					if (jcPath != null)
					{
						string path = (string) jcPath.GetValue(null);
						filesCheckBox.Tag = new MainFilesComponent(path);
					}
					else
					{
						string path = Path.Combine(
							Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
							"Apps",
							product,
							product + ".exe");
						if (File.Exists(path))
							filesCheckBox.Tag = new MainFilesComponent(path);
					}
				}
			}
			catch (Exception ex)
			{
				// ignored
			}

			jcComponents.Clear();
			removeButton.Enabled = false;
			foreach (Control advancedPanelControl in advancedPanel.Controls)
			{
				if (advancedPanelControl is CheckBox cb)
				{
					if (cb.Tag != null && cb.Tag is BaseComponent component)
					{
						jcComponents.Add(component);
						cb.Enabled = cb.Checked = component.IsInstalled();
						removeButton.Enabled = true;
					}
					else
					{
						cb.Enabled = cb.Checked = false;
					}
				}
			}
		}

		private void advancedButton_Click(object sender, EventArgs e)
		{
			toggleAdvanced();
		}

		private void toggleAdvanced()
		{
			if (advancedPanel.Visible)
			{
				Height = 83;
				advancedPanel.Visible = false;
			}
			else
			{
				advancedPanel.Visible = true;
				Height = 244;
			}
		}
	}
}
