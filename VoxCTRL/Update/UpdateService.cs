using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using log4net;
using Tct.ActivityRecorderClient.Communication;
using VoxCTRL.Communication;

namespace VoxCTRL.Update
{
	//based on UpdateWixWinService copy paste ;/
	public class UpdateService
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly Assembly currentAssembly = Assembly.GetExecutingAssembly();

		private const string bootstrapFileName = "bootstrap.exe";
		private const string msiexecFileName = "msiexec.exe";

		public static readonly string InitializeExMsgMissingMsiexec = "WiX build without msiexec";
		public static readonly string InitializeExMsgMissingBootstrap = "WiX build without bootstrap";
		public static readonly string InitializeExMsgRestartNeeded = "WiX build restart needed";
		public static readonly string InitializeExMsgUpdatetNeeded = "WiX build update needed";

		private volatile UpdateInfo updateInfo;

		public Version CurrentVersion
		{
			get { return currentAssembly.GetName().Version; }
		}

		public UpdateService()
		{
			updateInfo = new UpdateInfo(CurrentVersion.ToString(), null);
		}

		public bool Initialize()
		{
			if (!ExistsOnPath(msiexecFileName))
			{
				log.ErrorAndFail("Future application updates can't be performed because 'msiexec' not available on search path");
				throw new Exception(InitializeExMsgMissingMsiexec);
			}
			if (!File.Exists(Path.Combine(Path.GetDirectoryName(currentAssembly.Location), bootstrapFileName)))
			{
				log.ErrorAndFail("Cannot find bootstrap file");
				throw new Exception(InitializeExMsgMissingBootstrap);
			}
			return true;
		}

		public bool UpdateIfApplicable()
		{
			log.Debug("Check if update is available");
			try
			{
				return GetUpdateWcf(updateInfo);
			}
			catch
			{
				return false;
			}
		}

		private bool GetUpdateWcf(UpdateInfo currInfo)
		{
			if (currInfo.MsiPath != null && currInfo.UpdatedVersion == null)
			{
				log.Debug("Skipping update check since we have a newer version but don't know its version number");
				return false;
			}
			var msiTempFilePath = Path.Combine(Path.GetTempPath(), "vc_" + Guid.NewGuid() + ".msi");
			try
			{
				using (var client = new ActivityRecorderClientWrapper())
				{
					var appUpdateInfo = client.Client.GetApplicationUpdate(ConfigManager.UserId, ConfigManager.ApplicationName, currInfo.UpdatedVersion);
					if (appUpdateInfo == null) return false;
					log.Info("Found new version " + appUpdateInfo.Version);
					log.Debug("Start loading, no of chunks: " + appUpdateInfo.ChunkCount);
					using (FileStream stream = File.Create(msiTempFilePath))
					{
						for (int i = 0; i < appUpdateInfo.ChunkCount; i++)
						{
							byte[] chunk = client.Client.GetUpdateChunk(appUpdateInfo.FileId, i);
							stream.Write(chunk, 0, chunk.Length);
							log.Debug("Chunk {" + i + "} loaded, size: " + chunk.Length);
						}
						log.Info("Update package downloaded, size: " + stream.Length + " ver: " + appUpdateInfo.Version);
						stream.Flush();
						stream.Close();
					}
					if (updateInfo.MsiPath != null) DeleteFileNoThrow(updateInfo.MsiPath); //delete previous version
					updateInfo = new UpdateInfo(appUpdateInfo.Version, msiTempFilePath);
					return true;
				}
			}
			catch (Exception ex)
			{
				WcfExceptionLogger.LogWcfError("get application update", log, ex);
				DeleteFileNoThrow(msiTempFilePath);
				throw;
			}
		}

		//todo fix race: delete vs restart
		public bool RestartWithNewVersion()
		{
			var currInfo = updateInfo;
			Debug.Assert(currInfo != null);
			Debug.Assert(currInfo.MsiPath != null);
			string bootstrapTmp = null;
			try
			{
				log.Info("Invoking bootsrapper to perform msi update for " + (currInfo == null ? null : currInfo.MsiPath));
				var bootstrapSrc = Path.Combine(Path.GetDirectoryName(currentAssembly.Location), bootstrapFileName);
				bootstrapTmp = Path.Combine(Path.GetTempPath(), "vcb_" + Guid.NewGuid() + ".exe"); //todo delete garbage vcb_* files
				log.Info(string.Format("Copy [{0}]->[{1}]", bootstrapSrc, bootstrapTmp));
				File.Copy(bootstrapSrc, bootstrapTmp, true);
				ProcessStartInfo startInfo = new ProcessStartInfo();
				startInfo.CreateNoWindow = false;
				startInfo.UseShellExecute = false;
				startInfo.FileName = bootstrapTmp;
				startInfo.WindowStyle = ProcessWindowStyle.Hidden;
				startInfo.Arguments = "UpdateAndRestart \"" + currInfo.MsiPath + "\"";
				startInfo.WorkingDirectory = Path.GetDirectoryName(bootstrapTmp);
				startInfo.ErrorDialog = true;
				var proc = Process.Start(startInfo);
				log.Info("Stopping application to avoid file locks");
			}
			catch (Exception ex) //it is quite important not to hit this!
			{
				log.Error("Unable start update process", ex);
				updateInfo = new UpdateInfo(CurrentVersion.ToString(), null); //reset update info
				DeleteFileNoThrow(currInfo == null ? null : currInfo.MsiPath); //todo don't delete here try to install at startup?
				DeleteFileNoThrow(bootstrapTmp);
				return false;
			}
			Application.Exit();
			return true;
		}

		private static void DeleteFileNoThrow(string path)
		{
			try
			{
				if (path == null) throw new ArgumentNullException("path");
				if (File.Exists(path))
				{
					File.Delete(path);
					log.Debug("Deleted file " + path);
				}
			}
			catch (Exception ex)
			{
				log.ErrorAndFail("Unable to delete file " + path, ex);
			}
		}

		public string GetAppPath()
		{
			return currentAssembly.Location;
		}

		private static bool ExistsOnPath(string fileName)
		{
			return GetFullPath(fileName) != null;
		}

		private static string GetFullPath(string fileName)
		{
			if (File.Exists(fileName))
				return Path.GetFullPath(fileName);

			var values = Environment.GetEnvironmentVariable("PATH");
			return values == null ? null : values.Split(';').Select(path => Path.Combine(path, fileName)).FirstOrDefault(File.Exists);
		}

		[Serializable]
		private class UpdateInfo
		{
			public readonly string UpdatedVersion;
			public readonly string MsiPath;

			public UpdateInfo(string updatedVersion, string msiPath)
			{
				UpdatedVersion = updatedVersion;
				MsiPath = msiPath;
			}
		}
	}
}
