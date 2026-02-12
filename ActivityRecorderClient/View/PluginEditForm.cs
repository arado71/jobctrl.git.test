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
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.View
{
	public partial class PluginEditForm : FixedMetroForm
	{
		private Dictionary<string, List<ExtensionRuleParameter>> currentPluginParams;
		private BindingList<SimplePluginData> dataSource;
		private bool isRegex;

		public PluginEditForm()
		{
			InitializeComponent();

			this.Text = Labels.PluginEditFormTitle;
			btnCancel.Text = Labels.Cancel;
			btnOk.Text = Labels.Ok;
			btnParams.Text = Labels.PluginEditFormParameters + @"...";
			idDataGridViewTextBoxColumn.HeaderText = Labels.PluginEditFormId;
			idDataGridViewTextBoxColumn.ToolTipText = Labels.PluginEditFormId;
			keyDataGridViewTextBoxColumn.HeaderText = Labels.PluginEditFormKey;
			keyDataGridViewTextBoxColumn.ToolTipText = Labels.PluginEditFormKey;
			valueDataGridViewTextBoxColumn.HeaderText = Labels.PluginEditFormValue;
			valueDataGridViewTextBoxColumn.ToolTipText = Labels.PluginEditFormValue;
		}

		public void SetParamsVisible(bool visible)
		{
			btnParams.Visible = visible;
		}

		protected override void OnLoad(EventArgs e)
		{
			//register RowValidating on load (not in ctor) so we can display the form with invalid data
			this.gridPlugins.RowValidating += GridPluginsRowValidating;
			//we need to force revalidation now... don't know how to do that properly so fake it.
			for (int i = 0; i < this.gridPlugins.RowCount; i++)
			{
				if (gridPlugins.Rows[i].IsNewRow) continue;
				var eargs = new DataGridViewCellCancelEventArgs(-1, i);
				GridPluginsRowValidating(this.gridPlugins, eargs);
			}

			base.OnLoad(e);
		}

		public void ShowEditDialog(IWin32Window owner, Dictionary<string, Dictionary<string, string>> currentPlugins, Dictionary<string, List<ExtensionRuleParameter>> currentParams, bool isRegexRule)
		{
			isRegex = isRegexRule;
			dataSource = new BindingList<SimplePluginData>();

			if (currentPlugins != null)
			{
				foreach (var currentPlugin in currentPlugins.OrderBy(n => n.Key))
				{
					var id = currentPlugin.Key;
					if (currentPlugin.Value == null) continue;
					foreach (var param in currentPlugin.Value.OrderBy(n => n.Key))
					{
						if (param.Key == null || param.Value == null) continue;
						dataSource.Add(new SimplePluginData() { Id = id, Key = param.Key, Value = param.Value });
					}
				}
			}
			currentPluginParams = currentParams;

			gridPlugins.DataSource = dataSource;
			ShowDialog(owner);
		}

		public void GetData(out Dictionary<string, Dictionary<string, string>> currentPlugins, out Dictionary<string, List<ExtensionRuleParameter>> currentParams)
		{
			if (dataSource != null)
			{
				currentPlugins = new Dictionary<string, Dictionary<string, string>>();
				foreach (var simplePluginData in dataSource)
				{
					if (simplePluginData.Id == null || simplePluginData.Key == null || simplePluginData.Value == null) continue;
					Dictionary<string, string> param;
					if (!currentPlugins.TryGetValue(simplePluginData.Id, out param))
					{
						param = new Dictionary<string, string>();
						currentPlugins.Add(simplePluginData.Id, param);
					}
					param[simplePluginData.Key] = simplePluginData.Value;
				}
			}
			else
			{
				currentPlugins = null;
			}
			currentParams = currentPluginParams;
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}

		private void GridPluginsRowValidating(object sender, DataGridViewCellCancelEventArgs e)
		{
			if (e.RowIndex >= dataSource.Count || e.RowIndex < 0) return;
			var item = gridPlugins.Rows[e.RowIndex].DataBoundItem as SimplePluginData;
			if (item == null) return;
			try
			{
				if (string.IsNullOrEmpty(item.Id)) throw new Exception(Labels.PluginEditFormIdEmpty);
				if (string.IsNullOrEmpty(item.Key)) throw new Exception(Labels.PluginEditFormKeyEmpty);
				if (item.Value == null) item.Value = ""; //for regexs it will match everything, for non-regexs it will match the empty string only
				if (isRegex)
				{
					var _ = new Regex(item.Value);
				}
				gridPlugins.Rows[e.RowIndex].ErrorText = "";
			}
			catch (Exception ex)
			{
				gridPlugins.Rows[e.RowIndex].ErrorText = Labels.Error + "! " + Environment.NewLine + ex.Message;
				e.Cancel = true;
			}
		}

		private void btnParams_Click(object sender, EventArgs e)
		{
			using (var form = new PluginParameterEditForm())
			{
				form.ShowEditDialog(this, currentPluginParams);
				if (form.DialogResult == DialogResult.OK)
				{
					currentPluginParams = form.GetData();
				}
				DialogResult = DialogResult.None; //prevent this form from closing //http://blogs.msdn.com/b/cumgranosalis/archive/2005/10/03/showdialog-in-showdialog.aspx
			}
		}
	}
}
