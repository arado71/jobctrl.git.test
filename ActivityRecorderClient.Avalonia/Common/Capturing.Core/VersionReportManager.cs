using System;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Communication;
using log4net;
using System.Security.Principal;

namespace Tct.ActivityRecorderClient.Capturing.Core
{
	public class VersionReportManager : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private const int reportVersionInterval = 5 * 60 * 60 * 1000;  //5 hours
		private const int reportVersionRetryInterval = 60 * 1000;  //60 secs
		private const string applicationName = "JobCTRL";
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
				var compId = ConfigManager.EnvironmentInfo.ComputerId;
				var ver = ConfigManager.Version;
				ActivityRecorderClientWrapper.Execute(n => n.ReportClientVersion(userId, compId, ver.Major, ver.Minor, ver.Build, ver.Revision, applicationName));

				var osVer = ConfigManager.EnvironmentInfo.OSVersion;
				var info = new ClientComputerInfo()
				{
					UserId = userId,
					ComputerId = compId,
					OSMajor = osVer.Major,
					OSMinor = osVer.Minor,
					OSBuild = osVer.Build,
					OSRevision = osVer.Revision,
					IsNet4Available = ConfigManager.EnvironmentInfo.IsNet4Available,
					IsNet45Available = ConfigManager.EnvironmentInfo.IsNet45Available,
					HighestNetVersionAvailable = ConfigManager.EnvironmentInfo.HighestNetVersionAvailable,
					MachineName = Environment.MachineName,
					LocalUserName = OperatingSystem.IsWindows() ? WindowsIdentity.GetCurrent().Name : Environment.UserName,
				};
				ActivityRecorderClientWrapper.Execute(n => n.ReportClientComputerInfo(info));

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
