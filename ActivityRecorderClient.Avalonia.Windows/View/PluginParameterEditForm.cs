using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MetroFramework.Forms;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.View
{
	public partial class PluginParameterEditForm : FixedMetroForm
	{
		private BindingList<SimplePluginParameterData> dataSource;

		public PluginParameterEditForm()
		{
			InitializeComponent();

			this.Text = Labels.PluginEditFormTitle + " - " + Labels.PluginEditFormParameters;
			btnCancel.Text = Labels.Cancel;
			btnOk.Text = Labels.Ok;
			idDataGridViewTextBoxColumn.HeaderText = Labels.PluginEditFormId;
			idDataGridViewTextBoxColumn.ToolTipText = Labels.PluginEditFormId;
			nameDataGridViewTextBoxColumn.HeaderText = Labels.PluginEditFormName;
			nameDataGridViewTextBoxColumn.ToolTipText = Labels.PluginEditFormName;
			valueDataGridViewTextBoxColumn.HeaderText = Labels.PluginEditFormValue;
			valueDataGridViewTextBoxColumn.ToolTipText = Labels.PluginEditFormValue;
		}

		public void ShowEditDialog(IWin32Window owner, Dictionary<string, List<ExtensionRuleParameter>> currentParams)
		{
			dataSource = new BindingList<SimplePluginParameterData>();

			if (currentParams != null)
			{
				foreach (var param in currentParams.OrderBy(n => n.Key))
				{
					var id = param.Key;
					if (param.Value == null || param.Value.Count == 0) continue;
					foreach (var paramVal in param.Value)
					{
						if (paramVal.Name == null || paramVal.Value == null) continue;
						dataSource.Add(new SimplePluginParameterData()
						{
							Id = id,
							Name = paramVal.Name,
							Value = paramVal.Value,
						});
					}
				}
			}

			gridParameters.DataSource = dataSource;
			this.ShowDialog(owner);
		}

		public Dictionary<string, List<ExtensionRuleParameter>> GetData()
		{
			if (dataSource == null) return null;

			var result = new Dictionary<string, List<ExtensionRuleParameter>>();
			foreach (var parameterData in dataSource)
			{
				List<ExtensionRuleParameter> values;
				if (!result.TryGetValue(parameterData.Id, out values))
				{
					values = new List<ExtensionRuleParameter>();
					result[parameterData.Id] = values;
				}
				values.Add(new ExtensionRuleParameter() { Name = parameterData.Name, Value = parameterData.Value });
			}

			return result;
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
	}
}
