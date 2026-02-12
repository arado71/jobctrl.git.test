using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Tct.ActivityRecorderClient;
using Tct.ActivityRecorderService.Caching;

namespace Tct.ActivityRecorderService.Storage
{
	public class CompactScreenShotPathResolver : ModuloIdScreenShotPathResolver
	{
		private static readonly Func<string, IndexData> getCachedIndexForWrite = CachedFunc.CreateThreadSafe<string, IndexData>(GetCachedIndexForWriteImpl, TimeSpan.FromHours(12));
		private static readonly ThreadSafeCachedFunc<string, Dictionary<long, Tuple<long, int>>> getCachedIndexForReadCachedFunc = new ThreadSafeCachedFunc<string, Dictionary<long, Tuple<long, int>>>(GetCachedIndexForReadImpl, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(50));

		public CompactScreenShotPathResolver(string rootDirsString) : base(rootDirsString)
		{
		}

		private static IndexData GetCachedIndexForWriteImpl(string path)
		{
			path += ".idx";
			var offset = 0L;
			try
			{
				using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096, FileOptions.SequentialScan))
				{
					using (var reader = new BinaryReader(stream))
					{
						while (stream.Position < stream.Length)
						{
							var id = reader.ReadInt64();
							var length = reader.ReadInt32();
							offset += length;
						}
					}
				}
			}
			catch (FileNotFoundException)
			{
				// do nothing
			}
			catch (DirectoryNotFoundException)
			{
				// do nothing
			}

			return new IndexData { Offset = offset };
		}

		private static Dictionary<long, Tuple<long, int>> GetCachedIndexForReadImpl(string path)
		{
			path += ".idx";
			var offset = 0L;
			var index = new Dictionary<long, Tuple<long, int>>();
			try
			{
				using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096, FileOptions.SequentialScan))
				{
					while (stream.Position < stream.Length)
					{
						var buffer = new byte[sizeof(long) + sizeof(int)];
						stream.Read(buffer, 0, buffer.Length);
						var id = BitConverter.ToInt64(buffer, 0);
						var length = BitConverter.ToInt32(buffer, sizeof(long));
						index[id] = new Tuple<long, int>(offset, length);
						offset += length;
					}
				}
			}
			catch (FileNotFoundException)
			{
				// do nothing
			}
			catch (DirectoryNotFoundException)
			{
				// do nothing
			}

			return index;
		}

		public override void GetPath(ScreenShot screenShot, bool forWrite, out string dir, out string fileName, out long offset, out int length)
		{
			if (screenShot == null) throw new ArgumentNullException();
			var name = screenShot.WorkItem.UserId + "_" + screenShot.CreateDate.ToString("yyyy-MM-dd");
			fileName = name + ".dat";
			var subDirs = Path.Combine(screenShot.WorkItem.CompanyId.ToString(), screenShot.WorkItem.UserId.ToString());
			subDirs = Path.Combine(subDirs, screenShot.CreateDate.ToString("yyyy-MM"));
			dir = Path.Combine(GetRootDir(screenShot.Id), subDirs);
			var idxPath = Path.Combine(dir, name);
			if (forWrite)
			{
				var idxData = getCachedIndexForWrite(idxPath);
				lock (idxData)
				{
					if (GetPathForRead(screenShot.Id, out offset, out length, idxPath, false))
						return;
					offset = idxData.Offset;
					length = screenShot.Data.Length;
					Directory.CreateDirectory(dir);
					using (var stream = new FileStream(idxPath + ".idx", FileMode.Append, FileAccess.Write, FileShare.Read, 4096, FileOptions.SequentialScan))
					{
						var buffer = new byte[sizeof(long) + sizeof(int)];
						Buffer.BlockCopy(BitConverter.GetBytes(screenShot.Id), 0, buffer, 0, sizeof(long));
						Buffer.BlockCopy(BitConverter.GetBytes(screenShot.Data.Length), 0, buffer, sizeof(long), sizeof(int));
						stream.Write(buffer, 0, buffer.Length);
					}

					idxData.Offset += screenShot.Data.Length;
					var index = getCachedIndexForReadCachedFunc.GetOrCalculateValue(idxPath);
					index[screenShot.Id] = Tuple.Create(offset, length);
				}
				return;
			}

			if (!GetPathForRead(screenShot.Id, out offset, out length, idxPath))
				throw new Exception($@"ScreenShot id: {screenShot.Id} not found");
		}

		private static bool GetPathForRead(long id, out long offset, out int length, string idxPath, bool rereadIfNotFound = true)
		{
			var index = getCachedIndexForReadCachedFunc.GetOrCalculateValue(idxPath);
			if (!index.TryGetValue(id, out var data))
			{
				if (rereadIfNotFound)
				{
					getCachedIndexForReadCachedFunc.Remove(idxPath);
					index = getCachedIndexForReadCachedFunc.GetOrCalculateValue(idxPath);
				}
				if (!rereadIfNotFound || !index.TryGetValue(id, out data))
				{
					offset = -1L;
					length = 0;
					return false;
				}
			}

			offset = data.Item1;
			length = data.Item2;
			return true;
		}

		private class IndexData
		{
			public long Offset { get; set; }
		}
	}
}
