using System;
using System.Collections.Generic;
using System.Diagnostics;
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
	public class ModuloIdScreenShotPathResolver : IScreenShotPathResolver
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private readonly string[] rootDirs;

		private readonly string data;
		public string Data
		{
			get { return data; }
		}

		public ModuloIdScreenShotPathResolver(string rootDirsString)
		{
			if (rootDirsString == null) throw new ArgumentNullException();
			var newRootDirs = rootDirsString.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
			if (newRootDirs.Length == 0) throw new ArgumentException();
			if (newRootDirs.Any(n => !Path.IsPathRooted(n))) throw new ArgumentException();
			rootDirs = newRootDirs;
			data = rootDirsString;
		}

		public virtual void GetPath(ScreenShot screenShot, bool forWrite, out string dir, out string fileName, out long offset, out int length)
		{
			if (screenShot == null) throw new ArgumentNullException();
			fileName = screenShot.WorkItem.UserId
				+ "_" + screenShot.CreateDate.ToString("HH-mm-ss")
				+ "_" + screenShot.ScreenNumber
				+ "_" + screenShot.Id
				+ "." + screenShot.Extension;
			string subdirs = Path.Combine(screenShot.WorkItem.CompanyId.ToString(), screenShot.WorkItem.UserId.ToString());
			subdirs = Path.Combine(subdirs, screenShot.CreateDate.ToString("yyyy-MM-dd"));
			subdirs = Path.Combine(subdirs, screenShot.CreateDate.ToString("HH"));
			dir = Path.Combine(GetRootDir(screenShot.Id), subdirs);
			offset = -1L;
			length = -1;
		}

		protected string GetRootDir(long id)
		{
			return rootDirs[id % rootDirs.Length];
		}

		public ILookup<int, int> GetUserIds()
		{
			var result = new List<Tuple<int, int>>();
			foreach (var rootDir in rootDirs)
			{
				List<string> directories;
				try
				{
					directories = Directory.EnumerateDirectories(rootDir).ToList();
				}
				catch (IOException ex)
				{
					log.Warn("Failed to enumerate directories in " + rootDir, ex);
					continue;
				}
				foreach (var dir in directories)
				{
					Debug.Assert(dir.Substring(dir.LastIndexOf('\\')+1) == Path.GetFileName(dir));
					if (int.TryParse(Path.GetFileName(dir), out var companyId))
					{
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
				}
			}

			return result.ToLookup(k => k.Item1, v => v.Item2);
		}

		public IEnumerable<string> GetPaths(int companyId, int? userId)
		{
			return rootDirs.Select(rootDir => userId.HasValue ? Path.Combine(rootDir, companyId.ToString(CultureInfo.InvariantCulture), userId.Value.ToString(CultureInfo.InvariantCulture)) : Path.Combine(rootDir, companyId.ToString(CultureInfo.InvariantCulture)));
		}
	}
}
