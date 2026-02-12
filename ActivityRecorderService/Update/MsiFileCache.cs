using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using log4net;

namespace Tct.ActivityRecorderService.Update
{
	public class MsiFileCache : PeriodicManager, IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly int cleanUpInterval = (int)TimeSpan.FromDays(1).TotalMilliseconds;

		private readonly object thisLock = new object();
		private readonly Dictionary<string, CachedMsiFile> cachedFiles = new Dictionary<string, CachedMsiFile>();

		public MsiFileCache()
		{
			ManagerCallbackInterval = cleanUpInterval;
		}

		public IMsiFileData GetMsiFileInfo(string msiFilePath)
		{
			try
			{
				return GetOrAdd(msiFilePath);
			}
			catch (Exception e)
			{
				log.Error("GetMsiFileInfo failed.", e);
				return null;
			}
		}

		private CachedMsiFile GetOrAdd(string filePath)
		{
			lock (thisLock)
			{
				CachedMsiFile cachedFile;
				if (!cachedFiles.TryGetValue(filePath, out cachedFile))
				{
					cachedFile = new CachedMsiFile(filePath, OnCachedFileChanged);
					cachedFiles.Add(filePath, cachedFile);
					log.Debug("File added to cache (" + filePath + ") Number of files in cache:  " + cachedFiles.Count);
				}

				return cachedFile;
			}
		}

		private void OnCachedFileChanged(CachedMsiFile cachedFile)
		{
			lock (thisLock)
			{
				if (cachedFiles.Remove(cachedFile.FilePath))
				{
					log.Debug("File removed from cache (" + cachedFile.FilePath + ") Number of files in cache:  " + cachedFiles.Count);
					cachedFile.Dispose();
				}
			}
		}

		private void Clear()
		{
			lock (thisLock)
			{
				foreach (var cachedFile in cachedFiles.Values)
				{
					cachedFile.Dispose();
				}
				cachedFiles.Clear();
			}
			log.Debug("Cache cleared.");
		}

		public void Dispose()
		{
			Clear();
		}

		protected override void ManagerCallbackImpl()
		{
			Clear();
		}

		private class CachedMsiFile : IMsiFileData, IDisposable
		{
			private readonly FileSystemWatcher watcher;
			private readonly Action<CachedMsiFile> changeCallback;

			public string FilePath { get; private set; }
			public string MsiVersion { get; private set; }
			public byte[] Data { get; private set; }

			public CachedMsiFile(string filePath, Action<CachedMsiFile> changeCallback)
			{
				try
				{
					FilePath = filePath;
					this.changeCallback = changeCallback;
					using (File.OpenRead(filePath))	//Block file while caching its data
					{
						if (filePath.EndsWith("zip", StringComparison.InvariantCultureIgnoreCase))
						{
							MsiVersion =  GetInstallerVersionFromZip(filePath);
						} else if (Path.GetExtension(filePath).Equals(".pkg", StringComparison.InvariantCultureIgnoreCase))
						{
							MsiVersion = GetInstallerVersionFromPkg(filePath);
						}
						else
						{
							MsiVersion = InstallerHelper.GetMsiProperty(filePath, "ProductVersion");
						}

						if (string.IsNullOrEmpty(MsiVersion)) throw new ArgumentException("Failed to get Msi version of file.", "filePath");
						Data = File.ReadAllBytes(filePath);
						watcher = new FileSystemWatcher(Path.GetDirectoryName(filePath), Path.GetFileName(filePath));
						watcher.Changed += OnChange;
						watcher.Created += OnChange;
						watcher.Deleted += OnChange;
						watcher.Renamed += OnChange;
						watcher.Error += OnChange;
						watcher.EnableRaisingEvents = true;
					}
				}
				catch (Exception e)
				{
					log.Error("CachedFile initialization failed.", e);
					Dispose();
					throw;
				}
			}

			private string GetInstallerVersionFromZip(string filePath)
			{
				var r = "";

				using (ZipArchive archive = ZipFile.OpenRead(filePath))
				{
					var versionEntry = archive.GetEntry(".version.txt");
					if (versionEntry != null)
					{
						using (var zipEntryStream = versionEntry.Open())
						{
							using (var sr = new StreamReader(zipEntryStream))
							{
								r = sr.ReadToEnd();
							}
						}
					}
				}
				return r;
			}

			private string GetInstallerVersionFromPkg(string filePath)
			{
				string versionFile = Path.ChangeExtension(filePath, ".ver");
				return File.ReadAllText(versionFile);
			}

			public void Dispose()
			{
				try
				{
					if (watcher != null) watcher.Dispose();
				}
				catch (Exception e)
				{
					log.Error("CachedFile dispose failed.", e);
				}
			}

			private void OnChange(object sender, EventArgs args)
			{
				try
				{
					log.Debug(GetMessage(args), GetException(args));
					changeCallback(this);
				}
				catch (Exception e)
				{
					log.Error("Change callback failed.", e);
				}
			}

			private static string GetMessage(EventArgs args)
			{
				var renamedEventArgs = args as RenamedEventArgs;
				if (renamedEventArgs != null) return renamedEventArgs.ChangeType + " (" + renamedEventArgs.OldFullPath + " -> " + renamedEventArgs.FullPath + ")";
				var fileSystemEventArgs = args as FileSystemEventArgs;
				if (fileSystemEventArgs != null) return fileSystemEventArgs.ChangeType + " (" + fileSystemEventArgs.FullPath + ")";
				var errorEventArgs = args as ErrorEventArgs;
				if (errorEventArgs != null) return "FileSystemWatcher error.";
				return "";
			}

			private static Exception GetException(EventArgs args)
			{
				var errorEventArgs = args as ErrorEventArgs;
				return errorEventArgs != null ? errorEventArgs.GetException() : null;
			}
		}
	}
}
