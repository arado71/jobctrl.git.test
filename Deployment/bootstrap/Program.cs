using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Threading;

namespace bootstrap
{
	static class Program
	{
		private static readonly string msiExecFullPath = GetFullPath("msiexec.exe") ?? "msiexec.exe";
	
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(String[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			if (args.Length >= 2 && args.Length <= 4 && args[0] == "UpdateAndRestart")
			{
				try
				{
					using (var parentProcess = ParentProcessUtilities.GetParentProcess())
					{
						if (!parentProcess.WaitForExit(15000))
							parentProcess.Kill();
					}
				}
				catch
				{
					//assume parent already exited
				}
				var msiFilePath = args[1];
				var logPathParam = args.Length >= 3 && args[2].IndexOfAny(Path.GetInvalidPathChars()) == -1
					? " /L*v \"" + args[2] + "\""
					: "";
				var installDirPath = args.Length >= 4 && args[3].IndexOfAny(Path.GetInvalidPathChars()) == -1
					? " INSTALLDIR=\"" + args[3] + "\""
					: "";
				var startInfo = new ProcessStartInfo
					{
						CreateNoWindow = false,
						UseShellExecute = false,
						FileName = msiExecFullPath,
						WindowStyle = ProcessWindowStyle.Hidden,
						Arguments = "/qn /i \"" + msiFilePath + "\"" + logPathParam + " /norestart RUNAFTERINSTALL=1" + installDirPath
					};

				try
				{
					using (Process exeProcess = Process.Start(startInfo))
					{
						exeProcess.WaitForExit();
					}
				}
				catch (Exception e)
				{
					MessageBox.Show(e.Message, "Error");
				}
			}
			else
			{
				MessageBox.Show("Usage: " + Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) +
								" UpdateAndRestart application.msi [log file]");
			}
		}

		private static string GetFullPath(string fileName)
		{
			if (File.Exists(fileName)) return Path.GetFullPath(fileName);

			var fullPath = Path.Combine(Environment.SystemDirectory, fileName);
			if (File.Exists(fullPath)) return fullPath;

			var values = Environment.GetEnvironmentVariable("PATH");
			return values == null ? null : values.Split(';').Select(path => Path.Combine(path, fileName)).FirstOrDefault(File.Exists);
		}
	}
}
