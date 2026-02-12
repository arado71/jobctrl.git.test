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
	/// <summary>
	/// Class for updating censor rules
	/// </summary>
	public class CensorRulesManager : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private const int rulesUpdateInterval = 10 * 60 * 1000; //10 mins /**/9 GetClientCensorRules 113 bytes/call inside, in variables; but 60 packets 23971 bytes/call outside, in Ethernet packets
		private static string rulesFile { get { return "CensorRules-" + ConfigManager.UserId; } }

		public event EventHandler<SingleValueEventArgs<List<CensorRule>>> RulesChanged;

		private List<CensorRule> censorRules = new List<CensorRule>();
		private List<CensorRule> CensorRules
		{
			get { return censorRules; } //we don't need to expose this atm. (and it's not thread-safe)
			set
			{
				if (value == null) //cannot save null value
				{
					CensorRules = new List<CensorRule>();
					return;
				}
				Debug.Assert(value != null);
				if (XmlSerializationHelper.AreTheSame(censorRules, value)) return;
				log.Info("Censor Rules changed");
				censorRules = value;
				IsolatedStorageSerializationHelper.Save(rulesFile, value);
				OnRulesChanged(value);
			}
		}

		private string currentVersion;

		public CensorRulesManager()
			: base(log)
		{
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
				var rules = ActivityRecorderClientWrapper.Execute(n => n.GetClientCensorRules(out newVersion, userId, currentVersion));
				if (newVersion != currentVersion)
				{
					log.Debug("New version. (" + currentVersion + " -> " + newVersion + ")");
					currentVersion = newVersion;
					CensorRules = rules;
				}
			}
			catch (Exception ex)
			{
				WcfExceptionLogger.LogWcfError("get Censor Rules", log, ex);
			}
		}

		public void LoadRules()
		{
			log.Info("Loading Censor Rules from disk");
			List<CensorRule> rules;
			if (IsolatedStorageSerializationHelper.Exists(rulesFile)
				&& IsolatedStorageSerializationHelper.Load(rulesFile, out rules))
			{
				censorRules = rules;
			}
			OnRulesChanged(censorRules); //always raise so we know the initial state
		}

		protected virtual void OnRulesChanged(List<CensorRule> rules)
		{
			Debug.Assert(rules != null);
			EventHandler<SingleValueEventArgs<List<CensorRule>>> changed = RulesChanged;
			if (changed != null) changed(this, SingleValueEventArgs.Create(rules));
		}

	}
}
