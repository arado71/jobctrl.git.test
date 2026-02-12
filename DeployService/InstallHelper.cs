using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Odbc;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.ServiceProcess;
using ICSharpCode.SharpZipLib.Zip;
using log4net;

namespace Tct.DeployService
{
	public static class InstallHelper
	{
		private const string ResourceName = "Tct.DeployService.Setup.zip";

		private static readonly ILog logger = LogManager.GetLogger(typeof(InstallHelper));

		public static bool IsInstallPath(string path)
		{
			return File.Exists(Path.Combine(path, "ActivityRecorderService.dll"));
		}

		public static int ExecuteCommand(string cmd, Dictionary<string, string> environmentVariables = null)
		{
			var processInfo = new ProcessStartInfo("cmd.exe", "/c \"" + cmd + "\"")
			{
				CreateNoWindow = true,
				UseShellExecute = false,
				RedirectStandardOutput = true
			};
			if (environmentVariables != null)
			{
				foreach (var env in environmentVariables)
				{
					logger.DebugFormat("Setting environment variable \"{0}\" to \"{1}\"", env.Key, env.Value);
					processInfo.EnvironmentVariables.Add(env.Key, env.Value);
				}
			}

			var process = Process.Start(processInfo);
			process.WaitForExit();
			logger.DebugFormat("Command \"{1}\" finished running with following output: {0}", process.StandardOutput.ReadToEnd(), cmd);
			var exitCode = process.ExitCode;
			logger.DebugFormat("Command \"{1}\" returned with exit code {0}", exitCode, cmd);
			process.Close();
			return exitCode;
		}

		public static bool StartService(string name)
		{
			try
			{
				using (var sc = new ServiceController(name))
				{
					sc.Start();
					return true;
				}
			}
			catch (Exception e)
			{
				logger.WarnFormat("Error while starting service \"{0}\": {1}", name, e.Message);
				return false;
			}
		}

		public static bool StopService(string name)
		{
			try
			{
				using (var sc = new ServiceController(name))
				{
					if (sc.Status != ServiceControllerStatus.Running) return false;

					sc.Stop();
					return true;
				}
			}
			catch (Exception e)
			{
				logger.WarnFormat("Error while stopping service \"{0}\": {1}", name, e.Message);
				return false;
			}
		}

		private static Version payloadVersion = null;
		public static Version PayloadVersion()
		{
			if (payloadVersion == null)
			{
				using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName))
				{
					using (var zipStream = new ZipInputStream(stream))
					{
						ZipEntry zipEntry;
						while ((zipEntry = zipStream.GetNextEntry()) != null)
						{
							if (zipEntry.Name == "ActivityRecorderService.dll")
							{
								var tempFile = Path.GetTempFileName();
								using (FileStream streamWriter = File.OpenWrite(tempFile))
								{
									zipStream.CopyTo(streamWriter);
								}

								payloadVersion = Version.Parse(FileVersionInfo.GetVersionInfo(tempFile).FileVersion);
								File.Delete(tempFile);
							}
						}
					}
				}
			}

			return payloadVersion;
		}

		public static string GetThumbprint(string certificateName)
		{
			var store = new X509Store("My", StoreLocation.LocalMachine);
			store.Open(OpenFlags.ReadOnly);
			var certs = store.Certificates.Find(X509FindType.FindBySubjectName, certificateName, true);
			return certs.Count > 0 ? certs[certs.Count - 1].Thumbprint : null;
		}

		public static bool ExtractPayload(string path)
		{
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName))
			{
				using (var zipStream = new ZipInputStream(stream))
				{
					ZipEntry zipEntry;
					while ((zipEntry = zipStream.GetNextEntry()) != null)
					{
						var dirName = Path.GetDirectoryName(zipEntry.Name) ?? string.Empty;
						var fileName = Path.GetFileName(zipEntry.Name) ?? string.Empty;
						try
						{
							if (!string.IsNullOrEmpty(dirName) && !Directory.Exists(Path.Combine(path, dirName)))
							{
								Directory.CreateDirectory(Path.Combine(path, dirName));
							}

							if (!string.IsNullOrEmpty(fileName))
							{
								using (FileStream streamWriter = File.Create(Path.Combine(path, zipEntry.Name)))
								{
									zipStream.CopyTo(streamWriter);
								}
							}
						}
						catch (Exception e)
						{
							logger.ErrorFormat("Error while writing file {0}: {1}", fileName, e.Message);
							return false;
						}
					}
				}
			}

			return true;
		}
	}
}
