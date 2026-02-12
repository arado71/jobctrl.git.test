using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace Tct.ActivityRecorderService.Maintenance
{
	public class TelemetryFileStorage : IFileCleanup
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static TelemetryFileStorage instance = null;
		private static readonly object creationLock = new object();

		public static TelemetryFileStorage Instance
		{
			get
			{
				if (instance == null)
				{
					lock (creationLock)
					{
						if (instance == null)
						{
							instance = new TelemetryFileStorage();
						}
					}
				}

				return instance;
			}
		}

		public Storage Type
		{
			get
			{
				return Storage.Telemetry;
			}
		}

		public ILookup<int, int> GetUserIds()
		{
			var result = new List<Tuple<int, int>>();
			List<string> directories;
			try
			{
				directories = Directory.EnumerateDirectories(ConfigManager.TelemetryDataDir).ToList();
			}
			catch (IOException ex)
			{
				log.Warn("Failed to enumerate directories in " + ConfigManager.TelemetryDataDir, ex);
				return result.ToLookup(k => k.Item1, v => v.Item2);
			}
			foreach (var dir in directories)
			{
				if (!int.TryParse(Path.GetFileName(dir), out var companyId)) continue;
				try
				{
					var subDirectories = Directory.EnumerateDirectories(dir).ToList();
					foreach (var subDir in subDirectories)
					{
						if (!int.TryParse(Path.GetFileName(subDir), out var userId)) continue;
						result.Add(Tuple.Create(companyId, userId));
					}
				}
				catch (IOException ex)
				{
					log.Warn("Failed to enumerate subdirectories in " + dir, ex);
				}
			}

			return result.ToLookup(k => k.Item1, v => v.Item2);
		}

		public IEnumerable<string> GetPaths(int companyId, int? userId)
		{
			var path = Path.Combine(ConfigManager.TelemetryDataDir, companyId.ToString(CultureInfo.InvariantCulture));
			return new[] { userId.HasValue ? Path.Combine(path, userId.Value.ToString(CultureInfo.InvariantCulture)) : path };
		}
	}
}
