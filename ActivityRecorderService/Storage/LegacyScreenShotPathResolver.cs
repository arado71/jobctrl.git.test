using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using log4net;

namespace Tct.ActivityRecorderService.Storage
{
	/// <summary>
	/// Thread-safe immutabe class for resolving paths to screenshots
	/// </summary>
	public class LegacyScreenShotPathResolver : IScreenShotPathResolver
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly DateTime legacyCutOff = new DateTime(2010, 10, 11); //there is a legacy cutoff inside legacy... nice and easy
		private readonly IScreenShotPathResolver resolver;
		private readonly string rootDir;

		private readonly string data;
		public string Data
		{
			get { return data; }
		}

		public LegacyScreenShotPathResolver(string rootDirString)
		{
			if (rootDirString == null) throw new ArgumentNullException();
			if (!Path.IsPathRooted(rootDirString)) throw new ArgumentException();
			var newResolver = new ModuloIdScreenShotPathResolver(rootDirString); //hax use a ModuloId with one rootDir
			resolver = newResolver;
			rootDir = rootDirString;
			data = rootDirString;
		}

		public void GetPath(ScreenShot screenShot, bool forWrite, out string dir, out string fileName, out long offset, out int length)
		{
			if (screenShot == null) throw new ArgumentNullException();
			if (screenShot.CreateDate < legacyCutOff)
			{
				fileName = screenShot.WorkItem.UserId
					+ "_" + screenShot.CreateDate.ToString("HH-mm-ss")
					+ "_" + screenShot.ScreenNumber
					+ "_" + screenShot.Id
					+ "." + screenShot.Extension;
				string subdirs = Path.Combine(screenShot.CreateDate.ToString("yyyy-MM-dd"), screenShot.CreateDate.ToString("HH"));
				dir = Path.Combine(rootDir, subdirs);
				offset = -1L;
				length = -1;
			}
			else
			{
				resolver.GetPath(screenShot, forWrite, out dir, out fileName, out offset, out length);
			}
		}

		public ILookup<int, int> GetUserIds()
		{
			var result = new List<Tuple<int, int>>();
			List<string> directories;
			try
			{
				directories = Directory.EnumerateDirectories(rootDir).ToList();
			}
			catch (IOException ex)
			{
				log.Warn("Failed to enumerate directories in " + rootDir, ex);
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
			var path = Path.Combine(rootDir, companyId.ToString(CultureInfo.InvariantCulture));
			return new []{ userId.HasValue ? Path.Combine(path, userId.Value.ToString(CultureInfo.InvariantCulture)) : path };
		}
	}
}
