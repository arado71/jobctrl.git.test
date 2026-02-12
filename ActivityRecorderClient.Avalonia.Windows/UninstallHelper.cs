using log4net;
using System;
using System.IO;
using System.Text.RegularExpressions;
using Tct.ActivityRecorderClient.Serialization;

namespace Tct.ActivityRecorderClient
{
	public static class UninstallHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly string logFileExtension = @"\.log";

		public static string DeletePersonalData()
		{
			LogManager.ShutdownRepository();

			int rem = DeleteLogFiles();
			DeleteIsolatedStorage();

			string retMessage = "Deleting personal data finished. Remaining files: " + rem;
			return retMessage;
		}

		private static int DeleteLogFiles()
		{
			var baseDir = ConfigManager.LogPath;
			var logDir = Path.Combine(baseDir, "Logs");
			if (Directory.Exists(logDir))
			{
				int remaining = 0;
				foreach (var file in Directory.EnumerateFiles(logDir))
				{
					if (Regex.IsMatch(file, logFileExtension))
					{
						try
						{
							File.Delete(file);
						}
						catch (Exception ex)
						{
							remaining++;
							Console.Error.WriteLine("{0} can't be deleted, because {1}", file, ex.Message);
						}
					}
				}
				try
				{
					Directory.Delete(logDir);
				}
				catch (Exception ex)
				{
					Console.Error.WriteLine("{0} can't be deleted, because {1}", logDir, ex.Message);
				}
				return remaining;
			}
			Console.Error.WriteLine("Log folder not found in {0}", baseDir);
			return 0;
		}

		private static void DeleteIsolatedStorage()
		{
			var path = IsolatedStorageSerializationHelper.GetIsolatedStoragePath();
			if (path == null)
			{
				Console.Error.WriteLine("Isolated storage not found");
				return;
			}
			IsolatedStorageSerializationHelper.RemoveIsolatedStorage();
		}
	}
}
