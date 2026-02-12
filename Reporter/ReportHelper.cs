using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Reporter.CustomReporting;

namespace Reporter
{
	public static class ReportHelper
	{
		public static void GeneratePluginReports(int[] userIds, DateTime fromDate, DateTime toDate)
		{
			var currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var pluginDir = Path.Combine(currentDir, "plugins");
			var outDir = Path.Combine(currentDir, "out");
			if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);
			GeneratePluginReports(pluginDir, outDir, userIds, fromDate, toDate);
		}

		private static void GeneratePluginReports(string pluginDir, string outDir, int[] userIds, DateTime fromDate, DateTime toDate)
		{
			using (var newDomain = AppDomainWrapper.CreateDomain("CustomReports"))
			{
				var reporting = newDomain.CreateInstanceAndUnwrap<CustomReport>(pluginDir, outDir);
				reporting.GenerateReports(userIds, fromDate, toDate);
			}
		}
	}
}
