using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Communication;
using Tct.ActivityRecorderClient.Serialization;

namespace Tct.ActivityRecorderClient.Rules.Collector
{
	/// <summary>
	/// Class for detecting/fetching changes in collector rules.
	/// </summary>
	public class CollectorRulesManager : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private const int rulesUpdateInterval = 90 * 1000;
		private static string rulesFile { get { return "CollectorRules-" + ConfigManager.UserId; } }

		public event EventHandler<SingleValueEventArgs<CollectorRules>> RulesChanged;

		private CollectorRules collectorRules;
		private CollectorRules CollectorRules
		{
			get { return collectorRules; } //we don't need to expose this atm. (and it's not thread-safe)
			set
			{
				if (value == null) //cannot save null value
				{
					CollectorRules = new CollectorRules();
					return;
				}
				Debug.Assert(value != null);
				if (XmlSerializationHelper.AreTheSame(collectorRules, value)) return;
				log.Info("Collector Rules changed");
				collectorRules = value;
				IsolatedStorageSerializationHelper.Save(rulesFile, value);
				OnRulesChanged(value);
			}
		}

		private string currentVersion;

		public CollectorRulesManager()
			: base(log)
		{
			collectorRules = new CollectorRules();
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
				var rules = ActivityRecorderClientWrapper.Execute(n => n.GetClientCollectorRules(out newVersion, userId, currentVersion));
				if (newVersion != currentVersion)
				{
					log.Debug("New version. (" + currentVersion + " -> " + newVersion + ")");
					currentVersion = newVersion;
					CollectorRules = rules;
				}
			}
			catch (Exception ex)
			{
				WcfExceptionLogger.LogWcfError("get Collector Rules", log, ex);
			}
		}

		public void LoadRules()
		{
			log.Info("Loading Collector Rules from disk");
			CollectorRules rules;
			if (IsolatedStorageSerializationHelper.Exists(rulesFile)
				&& IsolatedStorageSerializationHelper.Load(rulesFile, out rules))
			{
				collectorRules = rules;
			}
			OnRulesChanged(collectorRules); //always raise so we know the initial state
		}

		private void OnRulesChanged(CollectorRules rules)
		{
			Debug.Assert(rules != null);
			var del = RulesChanged;
			if (del != null) del(this, SingleValueEventArgs.Create(rules));
		}
	}
}
