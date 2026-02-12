using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using log4net;
using Tct.ActivityRecorderClient.View;

namespace Tct.ActivityRecorderClient.ClientErrorReporting
{
	public class ClientErrorWinReporter : ClientErrorReporter
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		const int MB = 1024 * 1024;
		private static readonly bool isVistaOrLater = Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major >= 6;

		private ErrorReportingForm errorReportingForm;

		public ClientErrorWinReporter() : base(log)
		{

		}

		public override void LogSystemInfo()
		{
			try
			{
				using (var process = System.Diagnostics.Process.GetCurrentProcess())
				{
					log.Info(string.Format("Machinename: {0}, OSVersion: {1}, Framework version: {2}, Bitness: {3}",
						Environment.MachineName, Environment.OSVersion, Environment.Version, IntPtr.Size == 4 ? "x86" : "x64"));
					log.Info("Up Time: " + TimeSpan.FromMilliseconds(isVistaOrLater ? GetTickCount64() : (ulong)Environment.TickCount));
					log.Info("Start Date: " + process.StartTime);
					log.Info("Running Time: " + (DateTime.Now - process.StartTime));
					log.Info("Processor Time: " + process.TotalProcessorTime);
					log.Info("Thread Count: " + process.Threads.Count);
					log.Info("Working Set: " + process.WorkingSet64 / MB + " MB");
					log.Info("Private Mem: " + process.PrivateMemorySize64 / MB + " MB");
					log.Info("Total Heap Mem: " + GC.GetTotalMemory(false) / MB + " MB");
					log.Info("Handle Count: " + process.HandleCount);
					log.Info("GDI objects: " + GetGuiResources(process.Handle, 0));
					log.Info("USER objects: " + GetGuiResources(process.Handle, 1));
					var compInfo = new Microsoft.VisualBasic.Devices.ComputerInfo();
					log.Info("Available Physical Memory: " + compInfo.AvailablePhysicalMemory / MB + " MB");
					log.Info("Total Physical Memory: " + compInfo.TotalPhysicalMemory / MB + " MB");
				}
			}
			catch (Exception ex)
			{
				log.Error("Cannot log System info", ex);
			}
		}

		public override bool IsFromMessageLoop
		{
			get { return Application.MessageLoop; }
		}

		[System.Runtime.InteropServices.DllImport("User32")]
		private extern static int GetGuiResources(IntPtr hProcess, int uiFlags);

		[System.Runtime.InteropServices.DllImport("kernel32")]
		private extern static ulong GetTickCount64();


	}
}
