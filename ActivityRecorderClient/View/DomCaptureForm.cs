using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MetroFramework.Forms;
using Tct.ActivityRecorderClient.Capturing.Plugins.Impl;

namespace Tct.ActivityRecorderClient.View
{
	public partial class DomCaptureForm : FixedMetroForm
	{
		private readonly BindingList<DomSettings> domSettings = new BindingList<DomSettings>();

		public DomCaptureForm()
		{
			InitializeComponent();

			Icon = Properties.Resources.JobCtrl; //don't set it in the designer as it would enlarge the exe

			lbDomSettings.DataSource = domSettings;

			domSettings.ListChanged += domSettings_ListChanged;

			ClearItemBindings();

			//domSettings.Add(new PluginInternetExplorer.DomSettings() { Key = "Gugli", PropertyName = "innerText", Selector = "#gbqfsa", UrlPattern = "google\\.hu" });
			//domSettings.Add(new PluginInternetExplorer.DomSettings() { Key = "JCP", PropertyName = "innerText", Selector = "td.MenuItem.Selected", UrlPattern = "jobctrl\\.com" });
			//domSettings.Add(new PluginInternetExplorer.DomSettings() { Key = "JC", PropertyName = "innerText", Selector = "#form1 table.p tbody tr td table.page tbody tr td table tbody tr td table tbody tr td div table tbody tr td div p span", UrlPattern = "jobctrl\\.com" });
			//domSettings.Add(new PluginInternetExplorer.DomSettings() { Key = "JCT", EvalString = "document.querySelector(\"#viewIFRAME\").contentWindow.document.querySelector(\"input[type='radio']:checked\").nextSibling.nodeValue", UrlPattern = "www\\.w3schools\\.com" });
			//domSettings.Add(new PluginInternetExplorer.DomSettings() { Key = "JCM2", EvalString = "jc_res = null;for(i=0;i<5;i++){var elem = document.getElementById('_COMP_COMP_ID_ID_Comp_ID_Azonositas_'+i);if (elem && elem.checked) jc_res = elem.value;}", UrlPattern = "min" });
			//domSettings.Add(new PluginInternetExplorer.DomSettings() { Key = "JCM", EvalString = "function jc_tmp(){var e=null;for(i=0;i<5;i++){var t=document.getElementById(\"_COMP_COMP_ID_ID_Comp_ID_Azonositas_\"+i);if(t&&t.checked){e=t.value;t=document.getElementById(\"_COMP_COMP_ID_ID_Comp_ID_\"+t.value);if(t)e=e+\" \"+t.value}}return e}jc_tmp()", UrlPattern = "min" });
		}

		private void domSettings_ListChanged(object sender, ListChangedEventArgs e)
		{
			UpdateItemBindings(); //neither SelectedIndexChanged nor SelectedValueChanged is raised when item is removed
			var errorStr = "";
			try
			{
				txtJson.Text = JsonHelper.SerializeData(domSettings);
				foreach (var domSetting in domSettings)
				{

					string currError = domSetting.GetErrorStringAndInitialize();
					if (currError != "")
					{
						if (errorStr != "") errorStr += Environment.NewLine;
						errorStr += currError;
					}
				}
			}
			catch (Exception ex)
			{
				txtJson.Text = "Unable to serialize text: " + ex.Message; ;
				errorStr += txtJson.Text;
			}
			errorDomSettings.SetError(lbDomSettings, errorStr);
		}

		private void ClearItemBindings()
		{
			txtKey.DataBindings.Clear();
			txtSelector.DataBindings.Clear();
			txtPropertyName.DataBindings.Clear();
			txtUrlPattern.DataBindings.Clear();
			txtEvalString.DataBindings.Clear();
			if (lbDomSettings.SelectedIndex == -1 && domSettings.Count == 0) //hax to clear text boxes
			{
				txtKey.Text = "";
				txtSelector.Text = "";
				txtPropertyName.Text = "";
				txtUrlPattern.Text = "";
				txtEvalString.Text = "";
				errorDomSettings.SetError(txtEvalString, "");
				txtKey.Enabled = false;
				txtSelector.Enabled = false;
				txtPropertyName.Enabled = false;
				txtUrlPattern.Enabled = false;
				txtEvalString.Enabled = false;
			}
		}

		private void UpdateItemBindings()
		{
			var selected = lbDomSettings.SelectedItem as DomSettings;
			if (lastSelected == selected) return;
			if (lastSelected != null)
			{
				ClearItemBindings();
			}
			lastSelected = selected;
			if (lastSelected == null) return;
			txtKey.DataBindings.Add("Text", lastSelected, "Key", false, DataSourceUpdateMode.OnPropertyChanged);
			txtSelector.DataBindings.Add("Text", lastSelected, "Selector", true, DataSourceUpdateMode.OnPropertyChanged, "");
			txtPropertyName.DataBindings.Add("Text", lastSelected, "PropertyName", true, DataSourceUpdateMode.OnPropertyChanged, "");
			txtUrlPattern.DataBindings.Add("Text", lastSelected, "UrlPattern", true, DataSourceUpdateMode.OnPropertyChanged, "");
			txtEvalString.DataBindings.Add("Text", lastSelected, "EvalString", true, DataSourceUpdateMode.OnPropertyChanged, "");
			errorDomSettings.SetError(txtEvalString, lastSelected.GetErrorStringAndInitialize());
			txtKey.Enabled = true;
			txtSelector.Enabled = true;
			txtPropertyName.Enabled = true;
			txtUrlPattern.Enabled = true;
			txtEvalString.Enabled = true;
		}

		private DomSettings lastSelected;
		private void lbDomSettings_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateItemBindings();
		}

		private void btnAdd_Click(object sender, EventArgs e)
		{
			domSettings.Add(new DomSettings());
			lbDomSettings.SelectedIndex = domSettings.Count - 1;
		}

		private void btnRemove_Click(object sender, EventArgs e)
		{
			if (lastSelected == null) return;
			domSettings.Remove(lastSelected);
		}

		private void btnCopy_Click(object sender, EventArgs e)
		{
			try
			{
				Clipboard.SetText(txtJson.Text);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Unable to copy text to clpiboard: " + ex.Message);
			}
		}

		private void btnLoad_Click(object sender, EventArgs e)
		{
			using (var form = new DomCaptureLoadForm())
			{
				var res = form.ShowDialog();
				if (res != DialogResult.OK) return;
				domSettings.Clear();
				if (form.DomSettings == null) return;
				foreach (var setting in form.DomSettings)
				{
					domSettings.Add(setting);
				}
			}
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == Keys.Escape) this.Close();
			return base.ProcessCmdKey(ref msg, keyData);
		}
	}
}
