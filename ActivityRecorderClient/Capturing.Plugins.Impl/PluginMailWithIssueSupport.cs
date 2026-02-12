using JobCTRL.Plugins;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using Tct.ActivityRecorderClient.OutlookMailCaptureServiceReference;
using Tct.ActivityRecorderClient.View;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl
{
	public abstract class PluginMailWithIssueSupport : ICaptureExtension
	{
		private static readonly object issueMgrSupportedPluginChangeGlobalLock = new object();
		private static IssueManager issueManager;
		private static int imUsageCount;
		protected readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		protected const string KeyJcId = "JcId";
		protected const string KeyIssueName = "IssueName";
		protected const string KeyIssueCompany = "IssueCompany";
		protected const string KeyIssueState = "IssueState";

		private volatile bool isIssueMgrEnabled;

		protected void InitIssueManager()
		{
			lock (issueMgrSupportedPluginChangeGlobalLock)
			{
				isIssueMgrEnabled = ConfigManager.IsOutlookAddinMailTrackingId;
				if (!isIssueMgrEnabled) return;
				CreateIssueMgrIfNecessary();
			}
		}

		private void CreateIssueMgrIfNecessary()
		{
			lock (issueMgrSupportedPluginChangeGlobalLock)
			{
				if (issueManager != null)
				{
					imUsageCount++;
					log.DebugFormat("IssueMgr exists, count={0}", imUsageCount);
					return;
				}
				// to avoid multiple instantiation from same plugin => only first loaded plugin will be supported for each plugin type ATM
				issueManager = new IssueManager(Platform.Factory.GetGuiSynchronizationContext());
				imUsageCount = 1;
				log.Debug("IssueMgr created");
			}
		}

		private void DisposeIssueMgrIfNotNeeded()
		{
			lock (issueMgrSupportedPluginChangeGlobalLock)
			{
				if (issueManager != null && --imUsageCount <= 0)
				{
					issueManager.Dispose();
					log.Debug("IssueMgr disposed");
					issueManager = null;
				}
				else
					log.DebugFormat("IssueMgr still remains, count={0}", imUsageCount);
			}
		}

		void CheckIfIssueMgrEnabledStateChanged()
		{
			if (isIssueMgrEnabled == ConfigManager.IsOutlookAddinMailTrackingId) return;
			lock (issueMgrSupportedPluginChangeGlobalLock)
			{
				isIssueMgrEnabled = ConfigManager.IsOutlookAddinMailTrackingId;
				if (isIssueMgrEnabled)
				{
					CreateIssueMgrIfNecessary();
				}
				else
				{
					DisposeIssueMgrIfNotNeeded();
				}
			}
		}

		protected IEnumerable<KeyValuePair<string, string>> ExtendCaptureWithIssueData(IEnumerable<KeyValuePair<string, string>> capturedValues, IntPtr hWnd, MailCapture mail)
		{
			CheckIfIssueMgrEnabledStateChanged();
			if (!isIssueMgrEnabled || issueManager == null) return capturedValues;
			var res = capturedValues.ToList();
			if (!string.IsNullOrEmpty(mail.JcId))
				res.Add(new KeyValuePair<string, string>(KeyJcId, mail.JcId));
			var issue = issueManager.GetIssueData(hWnd, mail);
			if (issue == null) return res;
			res.Add(new KeyValuePair<string, string>(KeyIssueName, issue.Name));
			res.Add(new KeyValuePair<string, string>(KeyIssueCompany, issue.Company));
			res.Add(new KeyValuePair<string, string>(KeyIssueState, ((IssueState)issue.State).ToString()));
			return res;
		}

		public abstract string Id { get; }
		public abstract IEnumerable<string> GetParameterNames();
		public abstract void SetParameter(string name, string value);

		public virtual IEnumerable<string> GetCapturableKeys()
		{
			yield return KeyJcId;
			yield return KeyIssueName;
			yield return KeyIssueCompany;
			yield return KeyIssueState;
		}

		protected void DisposeIssueManager()
		{
			lock (issueMgrSupportedPluginChangeGlobalLock)
			{
				DisposeIssueMgrIfNotNeeded();
			}

		}

		public abstract IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName);
	}
}
