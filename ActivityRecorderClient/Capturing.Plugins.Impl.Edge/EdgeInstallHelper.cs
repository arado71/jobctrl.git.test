using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Threading;
using log4net;
using Microsoft.Win32;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.ProcessInfo;
using Tct.ActivityRecorderClient.Communication;

namespace Tct.ActivityRecorderClient.Capturing.Plugins.Impl.Edge
{
	public static class EdgeInstallHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static int isInstalled;
#if DEBUG
		internal static readonly string subDirName = "EdgeInterop";
#else
		internal static readonly string subDirName = Path.Combine("v" + ConfigManager.Version, "EdgeInterop");
#endif
		private const string extensionFileName = "edgeExtension.appx";
		private const string extensionResourceName = "Tct.ActivityRecorderClient.Capturing.Plugins.Impl.Edge.JobCTRLExtension.edgeextension.package." + extensionFileName;
		private static int isCleaned;
		private static string currentId = "com.jobctrl.jobctrl_0.0.1.6_neutral__exy19pxymbyha"; //also hardcoded in msi's custom action
		private static readonly string[] oldExtensionIds = { "com.jobctrl.jobctrl_0.0.1.3_neutral__b4qk06jgasqa2" };

		public static void InstallExtensionOneTimeIfApplicable()
		{
			if (Interlocked.CompareExchange(ref isInstalled, 1, 0) != 0) return;
			ThreadPool.QueueUserWorkItem(_ => InstallExtensionNoThrow());
		}

		private static void InstallExtensionNoThrow()
		{
			try
			{
				var currDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
				var dir = Path.Combine(currDir, subDirName);
				var extFilePath = Path.Combine(dir, extensionFileName);
				if (ProcessElevationHelper.IsElevated()) // changing HKLM registry only if process is elevated
				{
					RegisterExtension(extFilePath); //Registration to auto-install (or update) extension from Chrome Web Store
					foreach (var id in oldExtensionIds)
					{
						UnregisterExtension(id);
					}
				}
			}
			catch (Exception ex)
			{
				log.ErrorAndFail("Unexpected error during extension install", ex);
			}
		}


		private static void RegisterExtension(string path)
		{
			using (var PowerShellInstance = System.Management.Automation.PowerShell.Create())
			{
				string[] scripts = {"Add-AppxPackage {0}".FS(path),
									   "CheckNetIsolation LoopbackExempt -a -n=\"Microsoft.MicrosoftEdge_8wekyb3d8bbwe\""};
				foreach (var script in scripts)
				{
					PowerShellInstance.AddScript(script);
					PowerShellInstance.Invoke();
					if (PowerShellInstance.Streams.Error.Count > 0)
					{
						log.Debug(String.Join<ErrorRecord>(Environment.NewLine, PowerShellInstance.Streams.Error.ToArray()));
					}
				}
			}
		}

		private static void UnregisterExtension(string id)
		{
			using (var PowerShellInstance = System.Management.Automation.PowerShell.Create())
			{
				PowerShellInstance.AddScript("Remove-AppxPackage -Package".FS(id));
				PowerShellInstance.Invoke();
			}
		}

		private static void ExtractResource(string resourceName, string path)
		{
			var exists = File.Exists(path);
			try
			{
				log.Info("Extracting resource to " + (exists ? "(exists) " : "") + path);
				using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
				using (var file = File.Create(path))
				{
					CopyStream(stream, file);
				}
			}
			catch (Exception ex)
			{
				if (exists) //probably it's the same file
				{
					log.Debug("Unable to extract resource", ex);
				}
				else
				{
					throw;
				}
			}
		}

		private static void CopyStream(Stream input, Stream output)
		{
			if (input == null) throw new ArgumentNullException("input");
			if (output == null) throw new ArgumentNullException("output");

			var buffer = new byte[8192];
			int bytesRead;
			while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
			{
				output.Write(buffer, 0, bytesRead);
			}
		}
	}
}
