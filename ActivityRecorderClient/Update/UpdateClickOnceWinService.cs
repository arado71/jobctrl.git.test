using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using log4net;

namespace Tct.ActivityRecorderClient.Update
{
	//todo if new version is installed outside this app then CheckForUpdate will return false (with Latest version already installed...) so the client won't restart
	public class UpdateClickOnceWinService : IUpdateService
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly bool isClickOnce = (AppDomain.CurrentDomain.ActivationContext != null && ApplicationDeployment.IsNetworkDeployed);
		private static readonly Assembly currentAssembly = Assembly.GetExecutingAssembly();
		private const string publisherName = "JobCTRL";
		private static readonly string productName = ConfigManager.ApplicationName; //assume this is also the product name

		private const string bootstrapFileName = "bootstrap.exe";

		public static readonly string InitializeExMsgNotStartedFromMenu = "ClickOnce build not started from menu";

		private readonly ClickOnceMigrator migration = new ClickOnceMigrator();
		private bool restartWithMigratedVersion;

		private Version UpdatedVersion
		{
			get { return ApplicationDeployment.CurrentDeployment.UpdatedVersion; }
		}

		public Version CurrentVersion
		{
			get { return ApplicationDeployment.CurrentDeployment.CurrentVersion; }
		}

		public bool Initialize()
		{
			if (!isClickOnce
				&& currentAssembly.Location.IndexOf("\\Apps\\2.0\\", StringComparison.OrdinalIgnoreCase) > -1 //this looks like a ClickOnce build
				&& !File.Exists(Path.Combine(Path.GetDirectoryName(currentAssembly.Location), bootstrapFileName)) //this is excluded from ClickOnce builds
				)
			{
				log.ErrorAndFail("ClickOnce build not started with the proper shortcut");
				throw new Exception(InitializeExMsgNotStartedFromMenu);
			}
			return isClickOnce;
		}

		public bool? UpdateIfApplicable(bool force)
		{
			var migrationAvailable = migration.MigrateIfApplicable();
			restartWithMigratedVersion = restartWithMigratedVersion || migrationAvailable;
			if (restartWithMigratedVersion) return migrationAvailable;

			log.Debug("Check for update " + CurrentVersion);
			var updInfo = ApplicationDeployment.CurrentDeployment.CheckForDetailedUpdate();
			if (!updInfo.UpdateAvailable) return false;
			if (updInfo.AvailableVersion == UpdatedVersion)
			{
				log.Debug("Latest version already installed but not yet started " + updInfo.AvailableVersion);
				return false;
			}
			return Update();
		}

		private bool Update()
		{
			log.Debug("Updating...");
			var result = ApplicationDeployment.CurrentDeployment.Update();
			if (result)
			{
				log.Info("New version installed " + UpdatedVersion);
			}
			return result;
		}

		public bool RestartWithNewVersion()
		{
			try
			{
				if (restartWithMigratedVersion) return migration.RestartWithNewVersion();

				Application.Restart();
				return true;
			}
			catch (Exception ex) //ClickOnce bugs (NullReferenceException, InvalidOperationException)
			{
				log.Error("Failed to restart program", ex);
				return false;
			}
		}

		public string GetAppPath() //this is not water-tight at all (if the link is deleted it won't work)
		{
			var allProgramsPath = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
			var shortcutPath = Path.Combine(allProgramsPath, publisherName);
			shortcutPath = Path.Combine(shortcutPath, productName) + ".appref-ms";
			return shortcutPath;
		}

		public DateTime? LastUpdateFailed { get { return null; } }
		public UpdateFailureReason LastUpdateFailureReason { get; }
		public bool IsFirstRun { get { return ApplicationDeployment.CurrentDeployment.IsFirstRun; } }
		public bool IsAppLevelStorageNeeded { get { return true; } }
		public string UpdateMethodName { get { return "CO"; } }
	}
}
