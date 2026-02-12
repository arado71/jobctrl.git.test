using System;

namespace Tct.ActivityRecorderClient.Update
{
	// TODO: mac
	public class UpdateMacService : IUpdateService
	{
		public bool CheckForUpdate()
		{
			return false;
		}

		public bool Update()
		{
			throw new NotImplementedException();
		}

		public bool Initialize()
		{
			return false;
		}

		public bool? UpdateIfApplicable(bool force = false)
		{
			return false;
		}

		public bool RestartWithNewVersion()
		{
			return false;
		}

		public string GetAppPath()
		{
			return "";
		}

		public Version CurrentVersion {
			get {
				return new Version();
			}
		}

		public DateTime? LastUpdateFailed => null;

		public UpdateFailureReason LastUpdateFailureReason => UpdateFailureReason.InstallationNotCompleted;

		public bool IsFirstRun => false;

		public bool IsAppLevelStorageNeeded => false;

		public string UpdateMethodName => "";
	}
}

