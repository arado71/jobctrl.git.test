using System.Diagnostics;
using System.IO;
using log4net;
using Microsoft.Win32;

namespace Tct.DeployService
{
	public class Session
	{
		private readonly static ILog logger = LogManager.GetLogger(typeof(Session));
		private string path = null;

		public string InstallPath
		{
			get
			{
				if (path != null) return path;
				return path = (string)Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\TcT Hungary Kft.\\JobCTRL\\Service", "Path", string.Empty);
			}

			set
			{
				logger.DebugFormat("Setting installation path to \"{0}\"", value);
				Registry.SetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\TcT Hungary Kft.\\JobCTRL\\Service", "Path", value, RegistryValueKind.String);
				path = value;
			}
		}

		public Version InstalledVersion
		{
			get
			{
				if (string.IsNullOrEmpty(InstallPath))
				{
					logger.Warn("No installation path found to determine version");
					return null;
				}

				if (!File.Exists(Path.Combine(InstallPath, "ActivityRecorderService.dll")))
				{
					logger.Warn("ActivityRecorderService.dll not found, version information unavailable");
					return null;
				}

				return Version.Parse(FileVersionInfo.GetVersionInfo(Path.Combine(path, "ActivityRecorderService.dll")).FileVersion);
			}
		}
	}
}
