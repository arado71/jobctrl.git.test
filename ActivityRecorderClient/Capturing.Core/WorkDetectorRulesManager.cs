using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.Communication;
using Tct.ActivityRecorderClient.Serialization;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Capturing.Core
{
	//todo we store server rules in two places (here and in WorkDetector) which is not ideal...
	public class WorkDetectorRulesManager : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private const int rulesUpdateInterval = 90 * 1000;	//90 secs /**/6 GetClientRules 119 bytes/call inside, in variables; but 60 packets 23925 bytes/call outside, in Ethernet packets
		private static string rulesFile { get { return "WorkDetectorRules-" + ConfigManager.UserId; } }

		public event EventHandler<SingleValueEventArgs<List<WorkDetectorRule>>> RulesChanged;

		private List<WorkDetectorRule> workDetectorRules;
		private List<WorkDetectorRule> WorkDetectorRules
		{
			get { return workDetectorRules; } //we don't need to expose this atm. (and it's not thread-safe)
			set
			{
				if (value == null) //cannot save null value
				{
					WorkDetectorRules = new List<WorkDetectorRule>();
					return;
				}
				Debug.Assert(value != null);
				if (XmlSerializationHelper.AreTheSame(workDetectorRules, value)) return;
				log.Info("Work Detector Rules changed");
				workDetectorRules = value;
				IsolatedStorageSerializationHelper.Save(rulesFile, value);
				OnRulesChanged(value);
			}
		}

		private string currentVersion;

		public WorkDetectorRulesManager()
			: base(log)
		{
			workDetectorRules = new List<WorkDetectorRule>();
		}

		protected override int ManagerCallbackInterval
		{
			get
			{
				return rulesUpdateInterval;
			}
		}

		protected override void ManagerCallbackImpl()
		{
			try
			{
				int userId = ConfigManager.UserId;
				string newVersion = null;
				var rules = ActivityRecorderClientWrapper.Execute(n => n.GetClientRules(out newVersion, userId, currentVersion));
				if (newVersion != currentVersion)
				{
					log.Debug("New version. (" + currentVersion + " -> " + newVersion + ")");
					currentVersion = newVersion;
					WorkDetectorRules = rules;
				}
			}
			catch (Exception ex)
			{
				WcfExceptionLogger.LogWcfError("get Work Detector Rules", log, ex);
			}
		}

		public void LoadRules()
		{
			log.Info("Loading Work Detector Rules from disk");
			List<WorkDetectorRule> rules;
			if (IsolatedStorageSerializationHelper.Exists(rulesFile)
				&& IsolatedStorageSerializationHelper.Load(rulesFile, out rules))
			{
				workDetectorRules = rules;
			}
			OnRulesChanged(workDetectorRules); //always raise so we know the initial state
		}

		private void OnRulesChanged(List<WorkDetectorRule> rules)
		{
			Debug.Assert(rules != null);
			EventHandler<SingleValueEventArgs<List<WorkDetectorRule>>> changed = RulesChanged;
			if (changed != null) changed(this, SingleValueEventArgs.Create(rules));
		}

		#region TestData
		////TestManagerCallbackImpl
		//WorkDetectorRules = new List<WorkDetectorRule> { 
		//    new WorkDetectorRule()
		//    {
		//        IsEnabled = true,
		//        IgnoreCase= true,
		//        IsRegex = false,
		//        Name = "XD2",
		//        ProcessRule = "*",
		//        RelatedId = -1,
		//        RuleType = WorkDetectorRuleType.TempStartWorkTemplate,
		//        TitleRule = "$CategoryName$*",
		//        UrlRule = "*",
		//        IsPermanent = false,
		//    },
		//    new WorkDetectorRule()
		//    {
		//        IsEnabled = true,
		//        IgnoreCase= true,
		//        IsRegex = false,
		//        Name = "XD",
		//        ProcessRule = "$ProjectName$",
		//        RelatedId = -1,
		//        RuleType = WorkDetectorRuleType.TempStartProjectTemplate,
		//        TitleRule = "*",
		//        UrlRule = "*",
		//        IsPermanent = false,
		//        WorkSelector = new WorkSelector() { IgnoreCase = true, IsRegex = true, Rule = "\\d2", TemplateText = "$WorkName$"},
		//    },
		//};
		//return;
		#endregion
	}
}
