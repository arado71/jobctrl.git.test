using System;
using System.IO;
using System.Windows.Forms;
using log4net;
using Tct.ActivityRecorderClient.Serialization;

namespace Tct.ActivityRecorderClient.Update
{
	class ClickOnceMigrator
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly bool isMsiExecAvailable = File.Exists(UpdateWixWinService.MsiExecFullPath);
		private readonly UpdateWixWinService wixUpdateService = new UpdateWixWinService();
		private const string uninstallInfoPath = "UninstallInfo";
		private bool restartWithWix;

		public bool MigrateIfApplicable()
		{
			if (!isMsiExecAvailable)
			{
				log.Debug("Skipping migration because 'msiexec' not available on search path");
				return false;
			}

			log.Debug("Check for msi migration");
			//wixUpdateService.Initialize(); //We can't call it because this is a CO application.
			var result = wixUpdateService.UpdateIfApplicable();
			restartWithWix = restartWithWix || result.HasValue && result.Value;
			return restartWithWix;
		}

		public bool RestartWithNewVersion()
		{
			if (!restartWithWix) return false;
			
			IsolatedStorageSerializationHelper.Save(uninstallInfoPath,
				new UninstallInfo()
				{
					PublicKeyToken = ClickOnceHelper.GetPublicKeyToken(),
					DisplayName = ConfigManager.ApplicationName
				});
			IsolatedStorageSerializationHelper.SaveAllDataForMigration();

			if (wixUpdateService.InstallAndStartNewVersion(false))
			{
				Application.Exit();
				return true;
			}
			return false;
		}

		public static void TryFinishMigration()
		{
			try
			{
				if (!IsolatedStorageSerializationHelper.TryMigrateData()) return;
				ConfigManager.RefreshUserIdPassword();

				try//???
				{
					UninstallInfo appToUninstall;
					IsolatedStorageSerializationHelper.Load(uninstallInfoPath, out appToUninstall);
					if (appToUninstall == null)
					{
						log.Warn("There is no uninstall info.");
						return;
					}

					log.Info("Uninstall of previous ClickOnce application...");
					bool? forceSuccess, waitSuccess;
					var success = ClickOnceHelper.UninstallApplication(appToUninstall.PublicKeyToken, appToUninstall.DisplayName, true, true, out forceSuccess, out waitSuccess);
					log.InfoFormat("Uninstall result of previous ClickOnce application: success: {0}, forceSuccess: {1}, waitSuccess: {2}", success, forceSuccess, waitSuccess);

					IsolatedStorageSerializationHelper.Delete(uninstallInfoPath);
				}
				catch (Exception ex)
				{
					log.Error("Unable to uninstall ClickOnce application.", ex);
				}
			}
			catch (Exception ex)
			{
				log.Error("Unable to finish migration.", ex);
			}
		}

		[Serializable]
		public class UninstallInfo
		{
			public string PublicKeyToken { get; set; }
			public string DisplayName { get; set; }
		}
	}
}
