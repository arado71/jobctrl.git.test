using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.Update
{
	public interface IUpdateService
	{
		Version CurrentVersion { get; }
		bool Initialize();
		bool? UpdateIfApplicable(bool force = false);
		bool RestartWithNewVersion();
		string GetAppPath();

		DateTime? LastUpdateFailed { get; }
		UpdateFailureReason LastUpdateFailureReason { get; }
		bool IsFirstRun { get; }
		bool IsAppLevelStorageNeeded { get; }
		string UpdateMethodName { get; }
	}

	public enum UpdateFailureReason
	{
		InstallationNotCompleted,
		RestartRequired,
	}
}
