using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.Serialization;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Communication;

namespace Tct.ActivityRecorderClient.Capturing.Core
{
	/// <summary>
	/// Class for updating learning rule generators
	/// </summary>
	class LearningRuleManager : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private const int callbackInterval = 8 * 60 * 60 * 1000;  //8 hours
		private const int callbackRetryInterval = 60 * 1000;  //60 secs
		private static string rulesFile { get { return "LearningRuleGenerators-" + ConfigManager.UserId; } }

		public event EventHandler<SingleValueEventArgs<List<RuleGeneratorData>>> LearningRuleGeneratorsChanged;

		private List<RuleGeneratorData> learningRuleGenerators = new List<RuleGeneratorData>();
		private List<RuleGeneratorData> LearningRuleGenerators
		{
			get { return learningRuleGenerators; } //we don't need to expose this atm. (and it's not thread-safe)
			set
			{
				if (value == null) //cannot save null value
				{
					LearningRuleGenerators = new List<RuleGeneratorData>();
					return;
				}
				Debug.Assert(value != null);
				if (XmlSerializationHelper.AreTheSame(learningRuleGenerators, value)) return;
				log.Info("Learning Rule Generators changed");
				learningRuleGenerators = value;
				IsolatedStorageSerializationHelper.Save(rulesFile, value);
				OnRuleGeneratorsChanged(value);
			}
		}

		private bool lastSendFailed;
		private string currentVersion;

		public LearningRuleManager()
			: base(log)
		{
		}

		protected override int ManagerCallbackInterval
		{
			get { return lastSendFailed ? callbackRetryInterval : callbackInterval; }
		}

		protected override void ManagerCallbackImpl()
		{
			try
			{
				int userId = ConfigManager.UserId;
				string newVersion = null;
				var rules = ActivityRecorderClientWrapper.Execute(n => n.GetLearningRuleGenerators(out newVersion, userId, currentVersion));
				lastSendFailed = false;
				if (newVersion != currentVersion)
				{
					currentVersion = newVersion;
					LearningRuleGenerators = rules;
				}
			}
			catch (Exception ex)
			{
				WcfExceptionLogger.LogWcfError("get Learning Rule Generators", log, ex);
				lastSendFailed = true; //retry shortly
			}
		}

		public void LoadRules()
		{
			log.Info("Loading Learning Rule Generators from disk");
			List<RuleGeneratorData> rules;
			if (IsolatedStorageSerializationHelper.Exists(rulesFile)
				&& IsolatedStorageSerializationHelper.Load(rulesFile, out rules))
			{
				learningRuleGenerators = rules;
			}
			OnRuleGeneratorsChanged(learningRuleGenerators); //always raise so we know the initial state
		}

		private void OnRuleGeneratorsChanged(List<RuleGeneratorData> value)
		{
			Debug.Assert(value != null);
			var del = LearningRuleGeneratorsChanged;
			if (del == null) return;
			del(this, SingleValueEventArgs.Create(value));
		}
	}
}
