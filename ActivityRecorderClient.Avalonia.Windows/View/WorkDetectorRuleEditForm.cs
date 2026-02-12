using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MetroFramework.Forms;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Plugins;
using Tct.ActivityRecorderClient.Capturing.Plugins.Impl;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Rules;
using Tct.ActivityRecorderClient.Serialization;

namespace Tct.ActivityRecorderClient.View
{
	public partial class WorkDetectorRuleEditForm : FixedMetroForm
	{
		private static string DefaultOkFilePath { get { return "WorkDetectorFormOk-" + ConfigManager.UserId; } }
		private static string DefaultCancelFilePath { get { return "WorkDetectorFormCancel-" + ConfigManager.UserId; } }
		private static TimeSpan defaultCancelTime;
		private static OkValidType defaultOkValid;

		private readonly RuleRestrictions ruleRestrictions;
		private ClientMenuLookup menuLookup = new ClientMenuLookup();
		private WorkDetectorRule originalRule;
		private int? overrideRelatedId;
		private DisplayType dispType;
		private Dictionary<string, Dictionary<string, string>> currentPlugins;
		private Dictionary<string, List<ExtensionRuleParameter>> currentPluginParams;
		private List<WindowRule> currentWindowRules;

		public int PanelHeight { get { return splitAdvanced.Panel1MinSize; } }
		public WorkDetectorRule Rule { get { return originalRule; } }
		public string CancelKey { get; set; }
		public TimeSpan CancelTime { get; set; }
		public DesktopWindow MatchedWindow { get; set; }
		public DesktopCapture MatchedCapture { get; set; }

		static WorkDetectorRuleEditForm()
		{
			OkValidType loadedOkValid;
			if (IsolatedStorageSerializationHelper.Exists(DefaultOkFilePath)
				&& IsolatedStorageSerializationHelper.Load(DefaultOkFilePath, out loadedOkValid))
			{
				defaultOkValid = loadedOkValid;
			}
			else
			{
				defaultOkValid = (OkValidType)ConfigManager.SelfLearningOkValidity;
			}

			TimeSpan loadedCancelTime;
			if (IsolatedStorageSerializationHelper.Exists(DefaultCancelFilePath)
				&& IsolatedStorageSerializationHelper.Load(DefaultCancelFilePath, out loadedCancelTime))
			{
				defaultCancelTime = loadedCancelTime;
			}
			else
			{
				defaultCancelTime = TimeSpan.FromMinutes(15);
			}
		}

		public WorkDetectorRuleEditForm()
		{
			InitializeComponent();
			Icon = Properties.Resources.JobCtrl; //don't set it in the designer as it would enlarge the exe
			btnCancel.Text = Labels.Cancel;
			btnOk.Text = Labels.Ok;
			cbEnabled.Text = Labels.AutoRules_HeaderIsEnabledLong;
			cbIgnoreCase.Text = Labels.AutoRules_HeaderIgnoreCaseLong;
			cbRegex.Text = Labels.AutoRules_HeaderIsRegexLong;
			cbPermanent.Text = Labels.AutoRules_HeaderIsPermanentLong;
			lblProcessRule.Text = Labels.AutoRules_HeaderProcessRuleLong + ":";
			lblRuleType.Text = Labels.AutoRules_HeaderRuleTypeLong + ":";
			lblTitleRule.Text = Labels.AutoRules_HeaderTitleRuleLong + ":";
			lblUrlRule.Text = Labels.AutoRules_HeaderUrlRuleLong + ":";
			lblWork.Text = Labels.Work + ":";
			lblCategory.Text = Labels.Category + ":";
			cbAdvancedView.Text = Labels.AutoRules_AdvancedSettings;
			btnDontShowAgain.Text = Labels.AutoRules_DontAskAgain;
			lblHelp.Text = Labels.AutoRules_HelpAdvanced;
			lblWindowScope.Text = Labels.AutoRules_HeaderWindowScopeLong + ":";
			ruleRestrictions = ConfigManager.RuleRestrictions;
			InitializeDontShowCombo();
			IntializeOkValidCombo();
			InitalizeWindowScopeCombo();
			if ((ruleRestrictions & RuleRestrictions.CannotCreateOrModifyRules) != 0)
			{
				txtProcessRule.ReadOnly = true;
				txtTitleRule.ReadOnly = (ruleRestrictions & RuleRestrictions.CanModifyRuleTitle) == 0;
				txtUrlRule.ReadOnly = true;
				cbPermanent.AutoCheck = false;
				cbRuleType.Enabled = false;
				btnEditPlugins.Enabled = false;
				cbWindowScope.Enabled = false;
				btnWindowRules.Enabled = false;
			}
		}

		private void InitializeDontShowCombo()
		{
			var listToBind = new List<KeyValuePair<string, TimeSpan>>()
			{
				new KeyValuePair<string,TimeSpan>(Labels.AutoRules_DAAOneMinute, TimeSpan.FromMinutes(1)),
				new KeyValuePair<string,TimeSpan>(Labels.AutoRules_DAAFifteenMinutes, TimeSpan.FromMinutes(15)),
				new KeyValuePair<string,TimeSpan>(Labels.AutoRules_DAAThirtyMinutes, TimeSpan.FromMinutes(30)),
				new KeyValuePair<string,TimeSpan>(Labels.AutoRules_DAANever, TimeSpan.Zero),
			};
			cbDontShow.DisplayMember = "Key";
			cbDontShow.ValueMember = "Value";
			cbDontShow.DataSource = listToBind;
			cbDontShow.SetComboScrollWidth(n => ((KeyValuePair<string, TimeSpan>)n).Key);
			var selected = listToBind.Where(n => n.Value == defaultCancelTime).DefaultIfEmpty(listToBind[0]).First();
			cbDontShow.SelectedValue = selected.Value;
		}

		private void IntializeOkValidCombo()
		{
			var listToBind = new List<KeyValuePair<string, OkValidType>>();
			var any = (ruleRestrictions & RuleRestrictions.CannotUseAnyOkValueForLearningRule) == 0;
			if (any || ((ruleRestrictions & RuleRestrictions.CanUseOkDefault) != 0))
			{
				listToBind.Add(new KeyValuePair<string, OkValidType>(Labels.AutoRules_OKVDefault, OkValidType.Default));
			}
			if (any || ((ruleRestrictions & RuleRestrictions.CanUseOkUntilWindowClosed) != 0))
			{
				listToBind.Add(new KeyValuePair<string, OkValidType>(Labels.AutoRules_OKVUntilClosed, OkValidType.UntilWindowClosed));
			}
			if (any || ((ruleRestrictions & RuleRestrictions.CanUseOkForOneHour) != 0))
			{
				listToBind.Add(new KeyValuePair<string, OkValidType>(Labels.AutoRules_OKVForOneHour, OkValidType.ForOneHour));
			}
			if (any || ((ruleRestrictions & RuleRestrictions.CanUseOkForOneDay) != 0))
			{
				listToBind.Add(new KeyValuePair<string, OkValidType>(Labels.AutoRules_OKVForOneDay, OkValidType.ForOneDay));
			}

			//well we have to have one option at least
			if (listToBind.Count == 0)
			{
				listToBind.Add(new KeyValuePair<string, OkValidType>(Labels.AutoRules_OKVDefault, OkValidType.Default));
			}
			cbOkValid.DisplayMember = "Key";
			cbOkValid.ValueMember = "Value";
			cbOkValid.DataSource = listToBind;
			cbOkValid.SetComboScrollWidth(n => ((KeyValuePair<string, OkValidType>)n).Key);
			var selected = listToBind.Where(n => n.Value == defaultOkValid).DefaultIfEmpty(listToBind[0]).First();
			cbOkValid.SelectedValue = selected.Value;
		}

		private void InitalizeRuleTypeCombo(DisplayType type)
		{
			var listToBind = Enum.GetValues(typeof(WorkDetectorRuleType)).Cast<WorkDetectorRuleType>()
				.Where(n => ShouldDisplayRuleType(n, type))
				.Select(n => new KeyValuePair<string, WorkDetectorRuleType>(RuleManagementService.GetLongNameFor(n), n))
				.ToList();
			cbRuleType.DisplayMember = "Key";
			cbRuleType.ValueMember = "Value";
			cbRuleType.DataSource = listToBind;
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

		private static bool ShouldDisplayRuleType(WorkDetectorRuleType ruleType, DisplayType dispType)
		{
			switch (dispType)
			{
				case DisplayType.EditRule: //show the basics
					return ruleType == WorkDetectorRuleType.TempStartWork
						|| ruleType == WorkDetectorRuleType.TempStopWork;
				case DisplayType.DeleteRule:
				case DisplayType.EditRuleAdvanced: //cannot create template/assign rules from client
					return ruleType != WorkDetectorRuleType.TempStartProjectTemplate
#if !DEBUG
						&& ruleType != WorkDetectorRuleType.TempStartOrAssignWork
						&& ruleType != WorkDetectorRuleType.TempStartOrAssignProject
						&& ruleType != WorkDetectorRuleType.TempStartOrAssignProjectAndWork
#endif
						//bla - this comment line is preveting VS to remove whitespaces before the next line
						&& ruleType != WorkDetectorRuleType.CreateNewRuleAndTempStartWork //users don't understand it
						&& ruleType != WorkDetectorRuleType.TempStartWorkTemplate;
				case DisplayType.LearnNewRule: //don't overcomplicate things (do we need EndTempEffect here ?)
					return ruleType == WorkDetectorRuleType.TempStartWork
						|| ruleType == WorkDetectorRuleType.TempStopWork
						//|| ruleType == WorkDetectorRuleType.EndTempEffect
						|| ruleType == WorkDetectorRuleType.DoNothing;
				default:
					throw new ArgumentOutOfRangeException("dispType");
			}
		}

		public bool CanSelectWork(WorkData workData)
		{
			return workData != null && workData.Id.HasValue
				&& workData.IsVisibleInRules
				&& workData.IsWorkIdFromServer
				&& (ConfigManager.LocalSettingsForUser.ShowDynamicWorks || !menuLookup.IsDynamicWork(workData.Id.Value));
		}

		private void InitializeCategoriesCombo()
		{
			var listToBind = menuLookup.AllCategoriesById.Values
				.OrderBy(n => n.Name)
				.Select(n => new KeyValuePair<string, int>(n.Name + " (" + n.Id + ")", n.Id))
				.ToList();
			cbCategories.DisplayMember = "Key";
			cbCategories.ValueMember = "Value";
			cbCategories.DataSource = listToBind;
			cbCategories.SetComboScrollWidth(n => ((KeyValuePair<string, int>)n).Key);
		}

		public void UpdateMenu(ClientMenuLookup cMenuLookup)
		{
			menuLookup = cMenuLookup;

			cbWorks.UpdateMenu(cMenuLookup);

			var droppedC = cbCategories.DroppedDown;
			var selCat = cbCategories.SelectedValue;
			InitializeCategoriesCombo();
			cbCategories.SelectedValue = selCat ?? -1; //musn't use null value
			cbCategories.DroppedDown = droppedC; //if width changed then dropdown would disappear (so show it again)
		}

		public void Edit(WorkDetectorRule rule, int? overrideRelatedId, ClientMenuLookup cMenuLookup, DisplayType type, bool modal)
		{
			if (rule == null) throw new ArgumentNullException("rule");
			if (originalRule != null) throw new InvalidOperationException("Edit can only be called once for a form");
			originalRule = rule;
			menuLookup = cMenuLookup;
			if (type == DisplayType.EditRule && RuleManagementService.IsAdvancedViewNeededFor(rule)) type = DisplayType.EditRuleAdvanced;
			dispType = type;
			this.overrideRelatedId = overrideRelatedId;

			InitalizeRuleTypeCombo(type);
			cbWorks.CanSelectWork = CanSelectWork;
			cbWorks.UpdateMenu(menuLookup);
			InitializeCategoriesCombo();

			SetGuiFromRule(rule);

			var simpleEdit = type == DisplayType.EditRule;
			cbPermanent.Visible = !simpleEdit;
			cbPermanent.Enabled = !simpleEdit;
			cbRegex.Visible = !simpleEdit;
			cbRegex.Enabled = !simpleEdit;
			cbWindowScope.Visible = !simpleEdit;
			lblWindowScope.Visible = !simpleEdit;
			cbAdvancedView.Visible = type == DisplayType.LearnNewRule; //advanced view checkbox here is for learning rules only
			btnEditPlugins.Visible = type == DisplayType.EditRuleAdvanced || type == DisplayType.LearnNewRule;
			btnWindowRules.Visible = type == DisplayType.EditRuleAdvanced || type == DisplayType.LearnNewRule;
			UpdateGuiForWorkDetectorRuleType();
			if (type == DisplayType.LearnNewRule)
			{
				btnCancel.Visible = false;
				ChangeViewForLearningRule(false);
				//this.CloseBox = false;
			}
			else
			{
				cbDontShow.Visible = false;
				cbOkValid.Visible = false;
				btnDontShowAgain.Visible = false;
				btnDontShowAgain.Enabled = false;
			}
			if (type == DisplayType.DeleteRule)
			{
				txtTitleRule.ReadOnly = true;
				txtProcessRule.ReadOnly = true;
				txtUrlRule.ReadOnly = true;
				cbWindowScope.Enabled = false;
				cbRuleType.Enabled = false;
				cbPermanent.Enabled = false;
				cbRegex.Enabled = false;
				cbIgnoreCase.Enabled = false;
				cbEnabled.Enabled = false;
				btnEditPlugins.Enabled = false;
				btnWindowRules.Enabled = false;
				cbWorks.Enabled = false;
				cbCategories.Enabled = false;
				lblHelp.Text = Labels.AutoRules_HelpConfirmDelete;
				btnOk.Text = Labels.Delete;
			}
			if (modal)
			{
				ShowDialog();
			}
			else
			{
				Show();
				BringToFront();
				Focus();
			}
		}

		private void SetGuiFromRule(WorkDetectorRule rule)
		{
			txtProcessRule.Text = rule.ProcessRule;
			txtTitleRule.Text = rule.TitleRule;
			txtUrlRule.Text = rule.UrlRule;
			cbEnabled.Checked = rule.IsEnabled;
			cbPermanent.Checked = rule.IsPermanent;
			cbRegex.Checked = rule.IsRegex;
			cbIgnoreCase.Checked = rule.IgnoreCase;
			cbRuleType.SelectedValue = rule.RuleType;
			cbWindowScope.SelectedValue = rule.WindowScope;
			if (RuleManagementService.IsWorkAvailableFor(rule.RuleType))
			{
				cbWorks.SetSelectedWorkId(overrideRelatedId ?? rule.RelatedId); //validate/warn or simple empty selection is enough ? (stick with the later atm.)
			}
			else if (RuleManagementService.IsCategoryAvailableFor(rule.RuleType))
			{
				cbCategories.SelectedValue = overrideRelatedId ?? rule.RelatedId; //validate/warn or simple empty selection is enough ? (stick with the later atm.)
			}
			currentPlugins = originalRule.ExtensionRulesByIdByKey; //reset plugin changes
			currentPluginParams = originalRule.ExtensionRuleParametersById;
			currentWindowRules = originalRule.Children;
		}

		public void SetWork(int workId, bool shouldClose)
		{
			cbRuleType.SelectedValue = WorkDetectorRuleType.TempStartWork;
			cbWorks.SetSelectedWorkId(workId);
			if (!shouldClose) return;
			SaveRuleAndExitIfValid();
		}

		private Size simpleSize;
		private Size advancedSize = Size.Empty;
		private void ChangeViewForLearningRule(bool isAdvanced)
		{
			if (cbRuleType.SelectedValue == null || (WorkDetectorRuleType)cbRuleType.SelectedValue != WorkDetectorRuleType.TempStartWork)
			{
				isAdvanced = true; //override
			}
			if (advancedSize == Size.Empty) //calculate sizes for the first time
			{
				advancedSize = this.Size;
				simpleSize = new Size(this.Size.Width, this.Size.Height - PanelHeight);
			}
			this.MinimumSize = isAdvanced ? advancedSize : simpleSize;
			this.MaximumSize = isAdvanced ? advancedSize : simpleSize;
			this.Size = isAdvanced ? advancedSize : simpleSize;
			splitAdvanced.Panel1Collapsed = !isAdvanced;
			btnDontShowAgain.Visible = !isAdvanced;
			btnDontShowAgain.Enabled = !isAdvanced;
			cbDontShow.Visible = !isAdvanced;
			cbOkValid.Visible = !isAdvanced;
			this.Text = isAdvanced ? Labels.AutoRuleData_LearnNewRule : Labels.AutoRuleData_LearnNewRuleSimple;
			lblHelp.Text = isAdvanced ? Labels.AutoRules_HelpAdvanced : Labels.AutoRules_HelpSimpleLearn;
			if (dispType == DisplayType.LearnNewRule && MatchedWindow != null)
			{
				var extensionRules = originalRule.ExtensionRules;
				lblHelp.Text += Environment.NewLine + Labels.AutoRules_MatchedTitle + ": " + MatchedWindow.Title
					+ (extensionRules == null
						? ""
						: " " + Labels.AutoRules_MatchedExtensions + ": " + DesktopWindow.GetCaptureExtensionsToString(extensionRules));
			}
		}

		private void cbAdvancedView_CheckedChanged(object sender, EventArgs e)
		{
			ChangeViewForLearningRule(cbAdvancedView.Checked);
		}

		private WorkDetectorRule GetRuleFromGui()
		{
			var result = new WorkDetectorRule();
			SetRuleFromGui(result);
			return result;
		}

		private void SetRuleFromGui(WorkDetectorRule dst)
		{
			dst.ProcessRule = txtProcessRule.Text;
			dst.TitleRule = txtTitleRule.Text;
			dst.UrlRule = txtUrlRule.Text;
			dst.IsEnabled = cbEnabled.Checked;
			dst.IsRegex = cbRegex.Checked;
			dst.IgnoreCase = cbIgnoreCase.Checked;
			if (cbRuleType.SelectedValue == null) throw new Exception(Labels.AutoRuleData_SelectRuleTypeFirst);
			dst.RuleType = (WorkDetectorRuleType)cbRuleType.SelectedValue;
			if (cbWindowScope.SelectedValue == null) throw new Exception(Labels.AutoRuleData_SelectWindowScopeFirst);
			dst.WindowScope = (WindowScopeType)cbWindowScope.SelectedValue;
			dst.IsPermanent = RuleManagementService.IsPermanentAvailableFor(dst.RuleType) && cbPermanent.Checked;
			if (RuleManagementService.IsWorkAvailableFor(dst.RuleType))
			{
				var selectedWorkWithParentNames = cbWorks.SelectedItem as WorkDataWithParentNames;
				if (selectedWorkWithParentNames == null || selectedWorkWithParentNames.WorkData == null || !selectedWorkWithParentNames.WorkData.Id.HasValue) throw new Exception(Labels.AutoRuleData_SelectWorkFirst);
				dst.RelatedId = selectedWorkWithParentNames.WorkData.Id.Value;
				dst.Name = menuLookup.WorkDataById[dst.RelatedId].FullName;
				//dst.Name = cbWork.SelectedItem == null ? "" : TrimId(((KeyValuePair<string, int>)cbWork.SelectedItem).Key);
			}
			else if (RuleManagementService.IsCategoryAvailableFor(dst.RuleType))
			{
				if (cbCategories.SelectedValue == null) throw new Exception(Labels.AutoRuleData_SelectCategoryFirst);
				dst.RelatedId = (int)cbCategories.SelectedValue;
				dst.Name = menuLookup.AllCategoriesById[dst.RelatedId].Name;
			}
			else
			{
				dst.RelatedId = -1;
				dst.Name = cbRuleType.SelectedItem == null ? "" : ((KeyValuePair<string, WorkDetectorRuleType>)cbRuleType.SelectedItem).Key; //cbRuleType.SelectedText is empty
			}
			dst.ExtensionRulesByIdByKey = currentPlugins;
			dst.ExtensionRuleParametersById = currentPluginParams;
			dst.Children = currentWindowRules;
			if (cbOkValid.Visible && cbOkValid.SelectedValue is OkValidType)
			{
				var okVal = (OkValidType)cbOkValid.SelectedValue;
				switch (okVal)
				{
					case OkValidType.Default:
						break;
					case OkValidType.UntilWindowClosed:
						dst.ExtensionRulesByIdByKey = AddWindowHandleExtension(currentPlugins);
						break;
					case OkValidType.ForOneHour:
						dst.ValidUntilDate = DateTime.UtcNow.AddHours(1);
						break;
					case OkValidType.ForOneDay:
						dst.ValidUntilDate = DateTime.UtcNow.AddDays(1);
						break;
					default:
						Debug.Fail("Invalid OkValidType " + okVal);
						break;
				}
			}
			else if (cbDontShow.Visible && cbDontShow.SelectedValue is TimeSpan)
			{
				var time = (TimeSpan)cbDontShow.SelectedValue;
				if (time != TimeSpan.Zero)
				{
					dst.ValidUntilDate = DateTime.UtcNow + time; //set expiration if it's not 'Never'
				}
			}
		}

		private Dictionary<string, Dictionary<string, string>> AddWindowHandleExtension(Dictionary<string, Dictionary<string, string>> currentPlugins)
		{
			if (Rule == null || MatchedWindow == null) return currentPlugins;
			var handle = MatchedWindow.Handle.ToString();
			if (currentPlugins == null)
			{
				return new Dictionary<string, Dictionary<string, string>>()
				{
					{ 
						PluginWindowHandle.PluginId, 
						new Dictionary<string,string> {{ PluginWindowHandle.KeyHandle, handle }}
					},
				};
			}
			var curr = new Dictionary<string, Dictionary<string, string>>();
			foreach (var currentPlugin in currentPlugins)
			{
				curr.Add(currentPlugin.Key, new Dictionary<string, string>(currentPlugin.Value));
			}
			Dictionary<string, string> plg;
			if (!curr.TryGetValue(PluginWindowHandle.PluginId, out plg))
			{
				plg = new Dictionary<string, string>();
				curr.Add(PluginWindowHandle.PluginId, plg);
			}
			plg[PluginWindowHandle.KeyHandle] = handle;
			return curr;
		}

		//private static string TrimId(string src)
		//{
		//    return Regex.Replace(src, @"\s[(]\d+[)]$", "");
		//}

		private void btnDontShowAgain_Click(object sender, EventArgs e)
		{
			if (cbRuleType.SelectedValue == null || (WorkDetectorRuleType)cbRuleType.SelectedValue != WorkDetectorRuleType.TempStartWork) return; //shouldn't be enabled if it's true
			if (cbDontShow.SelectedValue == null) return; //ivalid selection
			var time = (TimeSpan)cbDontShow.SelectedValue;
			defaultCancelTime = time; //store last cancel time
			IsolatedStorageSerializationHelper.Save(DefaultCancelFilePath, defaultCancelTime);
			SetGuiFromRule(originalRule); //reset changes in the hope that SaveRuleAndExitIfValid will succeed
			cbOkValid.Visible = false; //don't use values from cbOkValid but use cbDontShow (hax)
			cbRuleType.SelectedValue = WorkDetectorRuleType.DoNothing; //we have to create a DoNothing rule
			if (SaveRuleAndExitIfValid()) return;//don't use the edited values use the original ones
			Debug.Fail("Cancel failed");
			//fallback so the user can close the window...
			//todo remove old cancel logic
			CancelTime = time;
			Close();
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			if (dispType == DisplayType.DeleteRule) //rule should be readonly, no need to save or validate
			{
				DialogResult = DialogResult.OK;
				Close();
				return;
			}
			if (cbOkValid.Visible && cbOkValid.SelectedValue is OkValidType)
			{
				defaultOkValid = (OkValidType)cbOkValid.SelectedValue; //store last OkValid
				IsolatedStorageSerializationHelper.Save(DefaultOkFilePath, defaultOkValid);
			}
			SaveRuleAndExitIfValid();
		}

		private bool SaveRuleAndExitIfValid()
		{
			try
			{
				var item = GetRuleFromGui();
				//hax if handle extension is not loaded then MatchedCapture won't match the created rule, MatchedCapture musn't be modified so remove ext
				Dictionary<string, string> plg;
				if (MatchedWindow != null
					&& item.ExtensionRulesByIdByKey != null
					&& item.ExtensionRulesByIdByKey.TryGetValue(PluginWindowHandle.PluginId, out plg))
				{
					plg.Remove(PluginWindowHandle.KeyHandle);
				}
				var matchers = item.ValidateAndGetMatchers(menuLookup);
				//valid rule
				if (MatchedWindow != null && matchers.All(n => !n.IsMatch(MatchedWindow, MatchedCapture)))
				{
					throw new Exception(Labels.AutoRuleData_CaptureNotMatched);
				}
				SetRuleFromGui(originalRule);
				DialogResult = DialogResult.OK;
				Close();
				return true;
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message, Labels.AutoRules_InvalidRuleTitle);
				return false;
			}
		}

		private void UpdateGuiForWorkDetectorRuleType()
		{
			var worksVisible = cbRuleType.SelectedValue != null
				&& RuleManagementService.IsWorkAvailableFor((WorkDetectorRuleType)cbRuleType.SelectedValue);
			var categoriesVisible = cbRuleType.SelectedValue != null
				&& RuleManagementService.IsCategoryAvailableFor((WorkDetectorRuleType)cbRuleType.SelectedValue);
			cbWorks.Visible = worksVisible;
			cbWorks.Enabled = worksVisible;
			lblWork.Visible = worksVisible;
			cbCategories.Visible = categoriesVisible;
			cbCategories.Enabled = categoriesVisible;
			lblCategory.Visible = categoriesVisible;
			cbPermanent.Visible = cbPermanent.Enabled
				&& cbRuleType.SelectedValue != null
				&& RuleManagementService.IsPermanentAvailableFor((WorkDetectorRuleType)cbRuleType.SelectedValue);
			cbAdvancedView.Enabled = cbRuleType.SelectedValue != null
				&& (WorkDetectorRuleType)cbRuleType.SelectedValue == WorkDetectorRuleType.TempStartWork;
		}

		private void cbRuleType_SelectedValueChanged(object sender, EventArgs e)
		{
			UpdateGuiForWorkDetectorRuleType();
		}

		private void btnEditPlugins_Click(object sender, EventArgs e)
		{
			using (var form = new PluginEditForm())
			{
				form.ShowEditDialog(this, currentPlugins, currentPluginParams, cbRegex.Checked);
				if (form.DialogResult != DialogResult.OK) return;
				form.GetData(out currentPlugins, out currentPluginParams);
			}
		}

		private void btnWindowRules_Click(object sender, EventArgs e)
		{
			using (var form = new WindowRuleEditForm())
			{
				form.SetRules(currentWindowRules ?? Enumerable.Empty<WindowRule>());
				form.ShowDialog(this);
				if (form.DialogResult != DialogResult.OK) return;
				currentWindowRules = form.GetRules();
			}
		}

		//http://www.codeproject.com/Articles/20379/Disabling-Close-Button-on-Forms
		private const int CP_NOCLOSE_BUTTON = 0x200;
		protected override CreateParams CreateParams
		{
			get
			{
				if (dispType == DisplayType.LearnNewRule)
				{
					CreateParams myCp = base.CreateParams;
					myCp.ClassStyle = myCp.ClassStyle | CP_NOCLOSE_BUTTON;
					return myCp;
				}
				return base.CreateParams;
			}
		}

		public enum OkValidType
		{
			Default,
			UntilWindowClosed,
			ForOneHour,
			ForOneDay,
		}

		public enum DisplayType
		{
			EditRule,
			EditRuleAdvanced,
			LearnNewRule,
			DeleteRule,
		}
	}
}
