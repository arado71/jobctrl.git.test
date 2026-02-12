using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using log4net;

namespace Tct.ActivityRecorderService.Maintenance
{
	public class FileCleanup
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly Func<DateTime, DateTime> dateSelector = time => new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, 0);
		private static readonly Func<DateTime, DateTime> dateUpperLimit = time => time.AddMinutes(1);

		private static readonly Dictionary<string, long> blockSizeCache = new Dictionary<string, long>();
		private readonly IFileCleanup[] storages;

		public FileCleanup(params IFileCleanup[] storages)
		{
			this.storages = storages;
		}

		public void Cleanup(FileCleanupSection configuration)
		{
			if (configuration == null) return;
			log.Debug("Loading company ids from storages");
			var sw = new Stopwatch();
			sw.Start();

			var userIdsByCompanyId = new int[0].ToLookup(k => k, v => 0); // empty lookup(!)
			var userIdsByStorage = new Dictionary<IFileCleanup, ILookup<int, int>>();
			foreach (var storage in storages)
			{
				try
				{
					var userIds = storage.GetUserIds();
					userIdsByStorage.Add(storage, userIds);
					userIdsByCompanyId = userIdsByCompanyId.Concat(userIds).SelectMany(l => l.Select(v => new { l.Key, Value = v })).Distinct().ToLookup(k => k.Key, v => v.Value);
				}
				catch (Exception e)
				{
					log.Warn("Unable to determine contents of storage", e);
				}
			}

			sw.Stop();
			log.DebugFormat("Company ids loaded in {0}", sw.Elapsed);
			sw.Restart();
			var userIdConfigMap = configuration.Limits.OfType<LimitElement>().Where(l => l.UserId.HasValue).ToDictionary(l => l.UserId.Value);
			foreach (var idItem in userIdsByCompanyId)
			{
				var paths = new HashSet<string>();
				foreach (var storage in storages)
				{
					var storagePaths = storage.GetPaths(idItem.Key, null).ToArray();
					paths.UnionWith(storagePaths);
					var storageConfigElement = configuration.Limits.Cast<LimitElement>()
						.FirstOrDefault(x => x.CompanyId == idItem.Key && x.Storage == storage.Type)
								  ??
								  configuration.Limits.Cast<LimitElement>()
									  .FirstOrDefault(x => x.CompanyId == null && x.Storage == storage.Type);
					if (storageConfigElement != null)
					{
						var sw1 = Stopwatch.StartNew();
						log.DebugFormat("Cleaning up company {0} storage {1} at {2}", idItem.Key, storage.Type, string.Join(", ", storagePaths));
						CleanupPaths(storageConfigElement, storagePaths.ToArray());
						log.DebugFormat($"Cleaning up company {idItem.Key} finished in {sw1.Elapsed.ToTotalMillisecondsString()}ms");
					}

					LimitElement userConfigElement = null;
					if (!userIdsByStorage[storage].Contains(idItem.Key)) continue;
					foreach (var userId in userIdsByStorage[storage][idItem.Key].Where(id => userIdConfigMap.TryGetValue(id, out userConfigElement) && (userConfigElement.Storage == storage.Type || userConfigElement.Storage == Storage.None)))
					{
						var sw1 = Stopwatch.StartNew();
						var userStoragePaths = storage.GetPaths(idItem.Key, userId).ToArray();
						log.DebugFormat("Cleaning up user {0} storage {1} at {2}", userId, storage.Type, string.Join(", ", userStoragePaths));
						CleanupPaths(userConfigElement, userStoragePaths.ToArray());
						log.DebugFormat($"Cleaning up user {userId} finished in {sw1.Elapsed.ToTotalMillisecondsString()}ms");
					}
				}

				var companyConfigElement = configuration.Limits.Cast<LimitElement>()
					.FirstOrDefault(x => x.CompanyId == idItem.Key && x.Storage == Storage.None)
							  ??
							  configuration.Limits.Cast<LimitElement>()
								  .FirstOrDefault(x => x.CompanyId == null && x.UserId == null && x.Storage == Storage.None);

				if (companyConfigElement != null)
				{
					var sw1 = Stopwatch.StartNew();
					log.DebugFormat("Cleaning up company {0} at: {1}", idItem.Key, string.Join(", ", paths));
					CleanupPaths(companyConfigElement, paths.ToArray());
					log.DebugFormat($"Cleaning up company {idItem.Key} finished in {sw1.Elapsed.ToTotalMillisecondsString()}ms");
				}
			}

			sw.Stop();
			log.DebugFormat("Cleanup finished in {0}", sw.Elapsed);
		}

		private static void CleanupPaths(LimitElement config, string[] rootPaths)
		{
			DateTime? sizeTrimDate = null;
			DateTime? ageTrimDate = null;

			if (!string.IsNullOrEmpty(config.MaxSize))
			{
				var ageMatch = Regex.Match(config.MaxSize, "([0-9]+)\\s*(eb|pb|tb|gb|mb|kb|b)", RegexOptions.IgnoreCase);
				if (ageMatch.Success)
				{
					long value;
					if (!long.TryParse(ageMatch.Groups[1].Value, out value))
					{
						log.Warn("Invalid numeric value in MaintenanceManager configuration");
						return;
					}

					long magnitude;
					switch (ageMatch.Groups[2].Value.ToLower())
					{
						case "b":
							magnitude = 1L;
							break;
						case "kb":
							magnitude = 1024L;
							break;
						case "mb":
							magnitude = 1024L * 1024;
							break;
						case "gb":
							magnitude = 1024L * 1024 * 1024;
							break;
						case "tb":
							magnitude = 1024L * 1024 * 1024 * 1024;
							break;
						case "pb":
							magnitude = 1024L * 1024 * 1024 * 1024 * 1024;
							break;
						case "eb":
							magnitude = 1024L * 1024 * 1024 * 1024 * 1024 * 1024;
							break;
						default:
							magnitude = 0;
							log.WarnFormat("Invalid modifier value {0}", ageMatch.Groups[2].Value);
							Debug.Fail(string.Format("Unkown modifier: {0}", ageMatch.Groups[2].Value));
							break;
					}

					if (magnitude != 0)
					{
						log.DebugFormat("Size trim detected as: {0} bytes", value * magnitude);
						sizeTrimDate = GetTrimDate(value * magnitude, rootPaths);
						if (sizeTrimDate != null)
						{
							sizeTrimDate = dateUpperLimit(sizeTrimDate.Value);
						}

						log.DebugFormat("Size trim date detected as: {0}", sizeTrimDate);
					}
				}
			}

			if (!string.IsNullOrEmpty(config.MaxAge))
			{
				var ageMatch = Regex.Match(config.MaxAge, "([0-9]+)\\s*(y|m|d|h)", RegexOptions.IgnoreCase);
				if (ageMatch.Success)
				{
					int value;
					if (!int.TryParse(ageMatch.Groups[1].Value, out value))
					{
						log.Warn("Invalid numeric value in MaintenanceManager configuration");
						return;
					}

					switch (ageMatch.Groups[2].Value.ToLower())
					{
						case "y":
							ageTrimDate = DateTime.Now.AddYears(-value);
							break;
						case "m":
							ageTrimDate = DateTime.Now.AddMonths(-value);
							break;
						case "d":
							ageTrimDate = DateTime.Now.AddDays(-value);
							break;
						case "h":
							ageTrimDate = DateTime.Now.AddHours(-value);
							break;
						default:
							log.WarnFormat("Invalid modifier value {0}", ageMatch.Groups[2].Value);
							Debug.Fail(string.Format("Unkown modifier {0}", ageMatch.Groups[2].Value));
							break;
					}

					if (ageTrimDate != null)
					{
						log.DebugFormat("Age trim detected as: {0}", ageTrimDate);
					}
				}
			}

			if (sizeTrimDate != null && ageTrimDate != null)
			{
				TrimByDate(sizeTrimDate.Value > ageTrimDate.Value ? sizeTrimDate.Value : ageTrimDate.Value, rootPaths);
				return;
			}

			if (sizeTrimDate == null && ageTrimDate == null)
			{
				log.Debug("No date for trimming");
				return;
			}

			TrimByDate(sizeTrimDate != null ? sizeTrimDate.Value : ageTrimDate.Value, rootPaths);
		}

		private static void TrimByDate(DateTime trimDate, string[] rootPaths)
		{
			log.DebugFormat("Deleting files recursively before {0} in {1}", trimDate, string.Join(", ", rootPaths));
			foreach (var root in rootPaths)
			{
				DeleteBefore(root, trimDate);
			}
            var handler = OnAfterFileCleanup;
            if (handler != null)
                handler(null, new FileCleanupEventArgs(trimDate));
        }

		private static DateTime? GetTrimDate(long sizeLimit, string[] rootPaths)
		{
			var directoryQueue = new Queue<string>();
			var dateSize = new Dictionary<DateTime, long>();
			foreach (var root in rootPaths)
			{
				var drive = Path.GetPathRoot(root);
				if (drive == null) continue;
				drive = drive.TrimEnd('\\');
				var blockSize = GetBlockSizeCached(drive);

				if (!Directory.Exists(root))
				{
					log.WarnFormat("Directory root {0} not found!", root);
					continue;
				}

				directoryQueue.Enqueue(root);

				while (directoryQueue.Count > 0)
				{
					var currentDir = directoryQueue.Dequeue();
					try
					{
						foreach (var directory in Directory.EnumerateDirectories(currentDir))
						{
							directoryQueue.Enqueue(directory);
						}

						foreach (var fileName in Directory.EnumerateFiles(currentDir))
						{
							var fileInfo = new FileInfo(fileName);
							var dateKey = dateSelector(fileInfo.CreationTime);
							if (!dateSize.ContainsKey(dateKey))
							{
								dateSize.Add(dateKey, 0);
							}

							dateSize[dateKey] += GetDiskSpace(fileInfo.Length, blockSize);
						}
					}
					catch (Exception e)
					{
						log.Warn("Directory scanning failed: " + currentDir, e);
					}
				}
			}

			var dateKeys = dateSize.Keys.OrderByDescending(x => x.Ticks);
			long currentSize = 0;
			DateTime? trimDate = null;
			foreach (var dateKey in dateKeys)
			{
				currentSize += dateSize[dateKey];
				if (currentSize <= sizeLimit) continue;
				trimDate = dateKey;
				break;
			}

			return trimDate;
		}
        public class FileCleanupEventArgs : EventArgs {
            public DateTime TrimDate { set; get; }
            public FileCleanupEventArgs(DateTime trimDate) {
                TrimDate = trimDate;
            }
        }
        public static event EventHandler<FileCleanupEventArgs> OnAfterFileCleanup;
		private static void DeleteBefore(string path, DateTime trimDate)
		{
			try
			{
				foreach (var directory in Directory.EnumerateDirectories(path))
				{
					DeleteBefore(directory, trimDate);
				}

				var sw = Stopwatch.StartNew();
				int filesDeleted = 0, filesFound = 0;
				foreach (var file in Directory.EnumerateFiles(path))
				{
					filesFound++;
					if (new FileInfo(file).CreationTime < trimDate)
					{
						try
						{
							File.Delete(file);
							filesDeleted++;
                           
						}
						catch (Exception ex)
						{
							log.Warn("Unable to delete file: " + file, ex);
						}
					}
				}
				if ((filesFound == 0 || filesDeleted > 0)
				    && !Directory.EnumerateFileSystemEntries(path).Any()) //if empty
				{
					try
					{
						Directory.Delete(path);
					}
					catch (Exception ex)
					{
						log.Warn("Unable to delete dir: " + path, ex);
					}
				}
				if (filesDeleted > 0)
					log.Debug("Deleted " + filesDeleted.ToString() + " / " + filesFound.ToString() + " files from " + path + " in " +
					          sw.Elapsed.ToTotalMillisecondsString() + "ms ");
			}
			catch (Exception e)
			{
				log.Warn("Deletion of directory failed " + path, e);
			}
		}

		private static long GetBlockSizeCached(string drive)
		{
			long result;
			lock (blockSizeCache)
			{
				if (!blockSizeCache.TryGetValue(drive, out result))
				{
					result = GetBlockSize(drive);
					blockSizeCache.Add(drive, result);
				}
			}
			return result;
		}

		internal static long GetBlockSize(string drive)
		{
			using (var searcher = new ManagementObjectSearcher("select BlockSize,NumberOfBlocks from Win32_Volume WHERE DriveLetter = '" + drive + "'"))
			using (var mgmtObj = ((ManagementObject)(searcher.Get().Cast<object>().First())))
			{
				return (long)((ulong)(mgmtObj["BlockSize"]));
			}
		}

		private static long GetDiskSpace(long fileSize, long blockSize)
		{
			return ((fileSize + blockSize - 1) / blockSize) * blockSize;
		}
	}
}
