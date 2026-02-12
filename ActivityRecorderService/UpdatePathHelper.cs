using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using log4net;

namespace Tct.ActivityRecorderService
{
	public static class UpdatePathHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly Dictionary<string, string> updatePaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		static UpdatePathHelper()
		{
			foreach (var key in ConfigManager.UpdatePathStrings)
			{
				var segments = key.Split('|');
				if (segments.Length != 3)
				{
					log.Error("Invalid key in config " + key);
					continue;
				}
				if (updatePaths.ContainsKey(segments[1]))
				{
					log.Error("Duplicate entry ignored for key " + key);
				}
				else
				{
					updatePaths.Add(segments[1], segments[2]);
				}
			}
		}

		public static bool TryGetPathFor(string application, out string path)
		{
			return updatePaths.TryGetValue(application, out path);
		}

	}
}
