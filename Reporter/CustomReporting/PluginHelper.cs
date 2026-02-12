using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace Reporter.CustomReporting
{
	public static class PluginHelper
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(PluginHelper));

		public static IEnumerable<T> LoadPlugins<T>(string folder, params object[] constructorArgs)
		{
			foreach (var file in Directory.EnumerateFiles(folder, "*.dll"))
			{
				T plugin;
				if (TryLoadPlugin(file, constructorArgs, out plugin))
				{
					yield return plugin;
				}
			}
		}

		public static bool TryLoadPlugin<T>(string filename, object[] constructorArgs, out T plugin)
		{
			log.Debug("Loading file: " + filename);
			plugin = default(T);
			try
			{
				var a = Assembly.LoadFile(filename);
				var reportType = a.GetTypes().FirstOrDefault(x => (typeof(IReport).IsAssignableFrom(x)) && x.IsPublic);
				if (reportType == null)
					return false;
				plugin = (T)Activator.CreateInstance(reportType, constructorArgs);
				return true;
			}
			catch (Exception ex)
			{
				log.Warn("File \"" + filename + "\" failed to load", ex);
				return false;
			}
		}
	}
}
