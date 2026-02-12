using System;
using log4net;
using Tct.ActivityRecorderClient;
using Tct.ActivityRecorderClient.Communication;
using VoxCTRL.Communication;

namespace VoxCTRL.VersionReporting
{
	public class VersionReportManager : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private const int reportVersionInterval = 5 * 60 * 60 * 1000;  //5 hours
		private const int reportVersionRetryInterval = 60 * 1000;  //60 secs
		private bool lastSendFailed;

		public VersionReportManager()
			: base(log)
		{
		}

		protected override int ManagerCallbackInterval
		{
			get { return lastSendFailed ? reportVersionRetryInterval : reportVersionInterval; }
		}

		protected override void ManagerCallbackImpl()
		{
			try
			{
				var userId = ConfigManager.UserId;
				var compId = ConfigManager.CompIdGenerator.ComputerId;
				var ver = ConfigManager.Version;
				var application = ConfigManager.ApplicationName;
				using (var client = new ActivityRecorderClientWrapper())
				{
					client.Client.ReportClientVersion(userId, compId, ver.Major, ver.Minor, ver.Build, ver.Revision, application);
				}

				lastSendFailed = false;
			}
			catch (Exception ex)
			{
				WcfExceptionLogger.LogWcfError("report version", log, ex);
				lastSendFailed = true; //retry shortly
			}
		}

	}
}
