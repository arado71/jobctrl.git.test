using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using log4net;
using Microsoft.Win32;
using Tct.ActivityRecorderClient.Communication;

namespace Tct.ActivityRecorderClient.Update
{
	public class UpdateWinService : IUpdateService
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static readonly IUpdateService Instance = new UpdateWinService();

		private readonly IUpdateService inner;

		static UpdateWinService() //explicit cctor to avoid premature initialization
		{
		}

		private UpdateWinService() //we need to get CurrentVersion before running Initialize, also we have to avoid circular depenencies between cctors (ConfigManager, ElevatedPrivilegesHelper, IsolatedStorageSerializationHelper)
		{
			try
			{
				inner = new UpdateWixWinService();
			}
			catch (Exception ex)
			{
				log.Debug("Unable to initialize UpdateClickOnceWinService", ex);
			}
		}

		public Version CurrentVersion
		{
			get { return inner.CurrentVersion; }
		}

		public bool Initialize()
		{
			return inner.Initialize();
		}

		public bool? UpdateIfApplicable(bool force)
		{
			return inner.UpdateIfApplicable(force);
		}

		public bool RestartWithNewVersion()
		{
			return inner.RestartWithNewVersion();
		}

		public string GetAppPath()
		{
			return inner.GetAppPath();
		}

		public DateTime? LastUpdateFailed { get { return inner.LastUpdateFailed; } }
		public UpdateFailureReason LastUpdateFailureReason => inner.LastUpdateFailureReason;
		public bool IsFirstRun { get { return inner.IsFirstRun; } }
		public bool IsAppLevelStorageNeeded { get { return inner.IsAppLevelStorageNeeded; } }
		public string UpdateMethodName { get { return inner.UpdateMethodName; } }
	}
}
