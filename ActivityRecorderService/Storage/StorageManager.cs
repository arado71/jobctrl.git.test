using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Tct.ActivityRecorderService;
using Tct.ActivityRecorderService.Common;
using Tct.ActivityRecorderService.Maintenance;

namespace Tct.ActivityRecorderService.Storage
{
	/// <summary>
	/// Thread-safe class for managing/querying the list of current screenshot path algorithms based on Id ranges and use this data to save screenshots.
	/// </summary>
	/// <remarks>
	/// Modifing data in Storages table (without copying screen shots) will break paths to old screenshot.
	/// If a new row is inserted into the Storages table but it is not read (refreshed) in time (before reaching the specified FirstId)
	/// then there will be some screen shots with invalid paths.
	/// If there is an invalid Algorithm in the table then the service won't start. If the table is updated later while the service is 
	/// running with an invalid Algorithm then the old rules will be used until it's fixed (i.e. all new rules can be loaded).
	/// </remarks>
	public class StorageManager : PeriodicManager, IFileCleanup
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly Dictionary<string, Func<string, IScreenShotPathResolver>> knownResolvers = new Dictionary<string, Func<string, IScreenShotPathResolver>>(StringComparer.OrdinalIgnoreCase);
		private static readonly IScreenShotPathResolver defaultResolver = new ModuloIdScreenShotPathResolver(ConfigManager.ScreenShotsDir);

		private readonly object thisLock = new object();
		private List<RangedResolver> resolvers = new List<RangedResolver>();
		private List<RangedResolver> Resolvers
		{
			get { lock (thisLock) { return resolvers; } }
			set { lock (thisLock) { resolvers = value; } }
		}

		private readonly AsyncDuplicateLock<int> lockStoreByUser = new AsyncDuplicateLock<int>();

		public Maintenance.Storage Type
		{
			get
			{
				return Maintenance.Storage.Screenshot;
			}
		}

		static StorageManager()
		{
			knownResolvers.Add("Legacy", data => new LegacyScreenShotPathResolver(data));
			knownResolvers.Add("ModuloId", data => new ModuloIdScreenShotPathResolver(data));
			knownResolvers.Add("Compact", data => new CompactScreenShotPathResolver(data));

			TestIfResolversAreAdded();
		}

		[Conditional("DEBUG")]
		private static void TestIfResolversAreAdded()
		{
			//test if all ifs are added
			var type = typeof(IScreenShotPathResolver);
			var types = AppDomain.CurrentDomain.GetAssemblies().ToList()
				.SelectMany(n => n.GetTypes())
				.Where(n => !n.IsAbstract)
				.Where(n => !n.IsInterface)
				.Where(n => type.IsAssignableFrom(n));
			Debug.Assert(types.Count() == knownResolvers.Count);
		}

		public StorageManager()
			: base(log)
		{
			ManagerCallbackInterval = ConfigManager.StorageRefreshInterval;
			RefreshStorageData();
		}

		// for test purposes
		public StorageManager(IScreenShotPathResolver resolver)
			: base(log)
		{
			resolvers.Add(new RangedResolver { Resolver = resolver });
		}

		public ILookup<int, int> GetUserIds()
		{
			var localResolvers = Resolvers;
			if (localResolvers.Count > 0)
			{
				var ids = new int[0].ToLookup(k => k, v => 0); // empty lookup(!)
				foreach (var resolver in localResolvers)
				{
					ids = ids.Concat(resolver.Resolver.GetUserIds()).SelectMany(l => l.Select(v => new { l.Key, Value = v })).Distinct().ToLookup(k => k.Key, v => v.Value);
				}

				return ids;
			}
			return defaultResolver.GetUserIds();
		}

		public IEnumerable<string> GetPaths(int companyId, int? userId)
		{
			var localResolvers = Resolvers;
			if (localResolvers.Count > 0)
			{
				var paths = new HashSet<string>();
				foreach (var resolver in localResolvers)
				{
					paths.UnionWith(resolver.Resolver.GetPaths(companyId, userId));
				}

				return paths;
			}

			return defaultResolver.GetPaths(companyId, userId);
		}

		protected override void ManagerCallbackImpl()
		{
			RefreshStorageData();
		}

		private void RefreshStorageData()
		{
			try
			{
				using (var context = new StorageDataClassesDataContext())
				{
					Resolvers = context.Storages //KISS: we don't check for changes, but overwrite every time
						.OrderByDescending(n => n.FirstId) //the order is reveresed as we expect to match the first rule 99% of the time
						.Select(n => new RangedResolver() { FirstId = n.FirstId, Resolver = GetResolver(n.Algorithm, n.Data) })
						.ToList();
				}
			}
			catch (Exception ex)
			{
				log.Error("Unable to refresh screenshot path resolvers", ex);
				throw;
			}
		}

		private static IScreenShotPathResolver GetResolver(string algorithm, string data)
		{
			Func<string, IScreenShotPathResolver> resolverFactory;
			if (knownResolvers.TryGetValue(algorithm, out resolverFactory))
			{
				return resolverFactory(data);
			}
			throw new ArgumentException("Unknown resolver algorithm", "algorithm");
		}

		public string GetPath(ScreenShot screenShot, out long offset, out int length, bool ensureDirExists = true)
		{
			Debug.Assert(screenShot != null);
			string dir, fileName;
			var currResolvers = Resolvers;
			IScreenShotPathResolver currIResolver = defaultResolver;
			for (int i = 0; i < currResolvers.Count; i++) //no linq here as we want this to be as fast as possible
			{
				if (currResolvers[i].FirstId <= screenShot.Id)
				{
					currIResolver = currResolvers[i].Resolver;
					break;
				}
			}
			currIResolver.GetPath(screenShot, ensureDirExists, out dir, out fileName, out offset, out length);
			//ensure that the dir exists
			if (ensureDirExists && !Directory.Exists(dir))
			{
				try
				{
					Directory.CreateDirectory(dir);
				}
				catch (Exception ex)
				{
					log.Error("Unable to create directory " + dir, ex);
					throw;
				}
			}
			return Path.Combine(dir, fileName);
		}

		public async Task<bool> TrySaveScreenShotsAsync(WorkItem workItem)
		{
			Debug.Assert(workItem != null);
			using(await lockStoreByUser.LockAsync(workItem.UserId).ConfigureAwait(false))
				try
				{
					if (workItem.ScreenShots == null || workItem.ScreenShots.Count == 0) return true;
					foreach (var screenShot in workItem.ScreenShots
						.Where(n => n.Data != null && n.Width > 0 && n.Height > 0)) // excl virtual screenshots
					{
						Debug.Assert(screenShot.Extension != null);
						string path = GetPath(screenShot, out var offset, out var length);
						Debug.Assert(length == -1 || length == screenShot.Data.Length);
						using (var stream = new FileStream(path, offset >= 0 ? FileMode.OpenOrCreate : FileMode.Create, offset >= 0 ? FileAccess.ReadWrite : FileAccess.Write, FileShare.Read, 4096, FileOptions.RandomAccess | FileOptions.Asynchronous))
						{
							var dataLength = screenShot.Data.Length;
							if (offset >= 0)
							{
								stream.Seek(0, SeekOrigin.End);
								if (offset > stream.Position)
								{
									stream.Seek(offset, SeekOrigin.Begin);
								}
								if (offset < stream.Position)
								{
									var buffer = new byte[dataLength];
									stream.Seek(offset, SeekOrigin.Begin);
									var read = await stream.ReadAsync(buffer, 0, dataLength);
									if (read != dataLength || buffer.Any(b => b != 0))
										throw new Exception($"File at position not empty path: {path}, offset {offset}, length: {dataLength}");
									stream.Seek(offset, SeekOrigin.Begin);
								}
							}

							await stream.WriteAsync(screenShot.Data.ToArray(), 0, dataLength);
						}
					}
					return true;
				}
				catch (Exception ex)
				{
					log.Error("Unable to save Screen Shots", ex);
					return false;
				}
		}

		private class RangedResolver
		{
			public long FirstId { get; set; }
			public IScreenShotPathResolver Resolver { get; set; }
		}
	}
}
