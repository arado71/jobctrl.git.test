using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using log4net;
using Reporter.Communication;
using Reporter.Excel;
using Reporter.Model.ProcessedItems;

namespace Reporter.CustomReporting
{
	public class CustomReport : MarshalByRefObject
	{
		private static ILog log = LogManager.GetLogger(typeof(CustomReport));
		private readonly string pluginFolder;
		private readonly string outFolder;
		private readonly IWebApi webApi;
		private List<IReport> plugins;

		public CustomReport(string pluginFolder, string outFolder)
		{
			this.pluginFolder = pluginFolder;
			this.outFolder = outFolder;
			this.webApi = new WebApi();
		}

		public void GenerateReports(int[] userIds, DateTime from, DateTime to)
		{
			EnsurePluginsLoaded();
			var query = CommunicationHelper.Query(userIds, from, to);
			var netQueryResult = query.CalculateNet();
			foreach (var data in Processing.ReportHelper.Transform(netQueryResult))
			{
				Process(data);
			}

			WriteReports();
		}

		private void Process(WorkItem data)
		{
			var failedPlugins = new List<IReport>();
			foreach (var plugin in plugins)
			{
				try
				{
					plugin.Process(data);
				}
				catch (Exception ex)
				{
					failedPlugins.Add(plugin);
					log.Error("Plugin " + plugin.Name + " failed while processing", ex);
				}
			}

			foreach (var failedPlugin in failedPlugins)
			{
				plugins.Remove(failedPlugin);
			}
		}

		private void WriteReports()
		{
			foreach (var plugin in plugins)
			{
				try
				{
					var result = plugin.GetResults();
					var fileName = plugin.Name + "_" + DateTime.Now.ToString("yyyy-MM-dd hh-mm") + ".xlsx";
					ExcelHelper.Export(Path.Combine(outFolder, fileName), result);
				}
				catch (Exception ex)
				{
					log.Error("Plugin " + plugin.Name + "failed while getting results", ex);
				}
			}
		}

		private void EnsurePluginsLoaded()
		{
			if (plugins == null)
			{
				plugins = PluginHelper.LoadPlugins<IReport>(pluginFolder, webApi).ToList();
			}
		}
	}
}
