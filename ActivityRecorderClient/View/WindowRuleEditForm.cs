using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Rules;

namespace Tct.ActivityRecorderClient.View
{
	public partial class WindowRuleEditForm : FixedMetroForm
	{
		private readonly BindingList<WindowRule> rules = new BindingList<WindowRule>();
		private WindowRule selectedRule;

		public WindowRuleEditForm()
		{
			InitializeComponent();
			Icon = Properties.Resources.JobCtrl; //don't set it in the designer as it would enlarge the exe
			Text = Labels.AutoRules_WindowRulesTitle;
			btnCancel.Text = Labels.Cancel;
			btnOk.Text = Labels.Ok;
			cbEnabled.Text = Labels.AutoRules_HeaderIsEnabledLong;
			cbIgnoreCase.Text = Labels.AutoRules_HeaderIgnoreCaseLong;
			cbRegex.Text = Labels.AutoRules_HeaderIsRegexLong;
			lblProcessRule.Text = Labels.AutoRules_HeaderProcessRuleLong + ":";
			lblTitleRule.Text = Labels.AutoRules_HeaderTitleRuleLong + ":";
			lblUrlRule.Text = Labels.AutoRules_HeaderUrlRuleLong + ":";
			lblWindowScope.Text = Labels.AutoRules_HeaderWindowScopeLong + ":";
			lblWinRules.Text = Labels.AutoRules_Title;
			//btnAdd.Text = todo
			//btnRemove.Text = todo

			lbWinRules.DataSource = rules;
			lbWinRules.SelectedIndexChanged += lbWinRules_SelectedIndexChanged;
			InitalizeWindowScopeCombo();
			SetSelectedRule(null);
		}

		private void lbWinRules_SelectedIndexChanged(object sender, EventArgs e)
		{
			var rule = lbWinRules.SelectedItem as WindowRule;
			SetSelectedRule(rule);
		}

		private void InitalizeWindowScopeCombo()
		{
			var listToBind = Enum.GetValues(typeof(WindowScopeType)).Cast<WindowScopeType>()
				.Select(n => new KeyValuePair<string, WindowScopeType>(RuleManagementService.GetLongNameFor(n), n))
				.ToList();
			cbWindowScope.DisplayMember = "Key";
			cbWindowScope.ValueMember = "Value";
			cbWindowScope.DataSource = listToBind;
		}

		private void SetSelectedRule(WindowRule rule)
		{
			EnableGui(rules.Count != 0);
			selectedRule = rule;
			txtProcessRule.DataBindings.Clear();
			txtTitleRule.DataBindings.Clear();
			txtUrlRule.DataBindings.Clear();
			cbWindowScope.DataBindings.Clear();
			cbEnabled.DataBindings.Clear();
			cbRegex.DataBindings.Clear();
			cbIgnoreCase.DataBindings.Clear();

			if (selectedRule == null)
			{
				return;
			}

			txtProcessRule.DataBindings.Add("Text", selectedRule, "ProcessRule", false, DataSourceUpdateMode.OnPropertyChanged, "");
			txtTitleRule.DataBindings.Add("Text", selectedRule, "TitleRule", false, DataSourceUpdateMode.OnPropertyChanged, "");
			txtUrlRule.DataBindings.Add("Text", selectedRule, "UrlRule", false, DataSourceUpdateMode.OnPropertyChanged, "");
			cbWindowScope.DataBindings.Add("SelectedValue", selectedRule, "WindowScope", false, DataSourceUpdateMode.OnPropertyChanged, WindowScopeType.Any);
			cbEnabled.DataBindings.Add("Checked", selectedRule, "IsEnabled");
			cbRegex.DataBindings.Add("Checked", selectedRule, "IsRegex");
			cbIgnoreCase.DataBindings.Add("Checked", selectedRule, "IgnoreCase");
		}

		private void EnableGui(bool enabled)
		{
			if (!enabled)
			{
				txtProcessRule.Text = "";
				txtTitleRule.Text = "";
				txtUrlRule.Text = "";
			}
			txtProcessRule.Enabled = enabled;
			txtTitleRule.Enabled = enabled;
			txtUrlRule.Enabled = enabled;
			cbWindowScope.Enabled = enabled;
			cbEnabled.Enabled = enabled;
			cbRegex.Enabled = enabled;
			cbIgnoreCase.Enabled = enabled;
		}

		public void SetRules(IEnumerable<WindowRule> windowRules)
		{
			rules.Clear();
			foreach (var rule in CloneRules(windowRules))
			{
				rules.Add(rule);
			}
		}

		public List<WindowRule> GetRules()
		{
			return CloneRules(rules);
		}

		private static List<WindowRule> CloneRules(IEnumerable<WindowRule> windowRules) //hax
		{
			var tmp = new WorkDetectorRule() { Children = windowRules.ToList() };
			return tmp.Clone().Children;
		}

		private void btnAdd_Click(object sender, EventArgs e)
		{
			rules.Add(new WindowRule()
			{
				IsEnabled = true,
				IgnoreCase = true,
				WindowScope = WindowScopeType.Any,
				ProcessRule = Labels.AutoRules_ExampleExe,
				TitleRule = "*",
				UrlRule = "*",
			});
			lbWinRules.SelectedIndex = -1;
			lbWinRules.SelectedIndex = rules.Count - 1;
		}

		private void btnRemove_Click(object sender, EventArgs e)
		{
			var idx = rules.IndexOf(selectedRule);
			if (idx == -1) return;
			rules.RemoveAt(idx);
			lbWinRules.SelectedIndex = idx - 1;
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
		}

		private void btnEditPlugins_Click(object sender, EventArgs e)
		{
			if (selectedRule == null) return;
			var currentPlugins = selectedRule.ExtensionRulesByIdByKey;
			var currentPluginParams = new Dictionary<string, List<ExtensionRuleParameter>>();

			using (var form = new PluginEditForm())
			{
				form.SetParamsVisible(false); //no params for window rules
				form.ShowEditDialog(this, currentPlugins, currentPluginParams, cbRegex.Checked);
				if (form.DialogResult != DialogResult.OK) return;
				form.GetData(out currentPlugins, out currentPluginParams);
				selectedRule.ExtensionRulesByIdByKey = currentPlugins;
			}
		}
	}
}
