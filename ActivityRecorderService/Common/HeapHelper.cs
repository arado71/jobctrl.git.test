using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace Tct.ActivityRecorderService.Common
{
	public static class HeapHelper
	{
		private const int MB = 1024 * 1024;

		public static void CompactLargeObjectHeap(ILog log)
		{
			LogMemoryInfo(log, "Before compacting:");
			GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			LogMemoryInfo(log, "After compacting:");
		}

		private static void LogMemoryInfo(ILog log, string text)
		{
			try
			{
				using (var process = System.Diagnostics.Process.GetCurrentProcess())
				{
					log.Info($"{text} Working Set: {process.WorkingSet64 / MB} MB, Private Mem: {process.PrivateMemorySize64 / MB} MB, Total Heap Mem: {GC.GetTotalMemory(false) / MB} MB");
				}
			}
			catch (Exception ex)
			{
				log.Error("Cannot log memory info", ex);
			}
		}


	}
}
