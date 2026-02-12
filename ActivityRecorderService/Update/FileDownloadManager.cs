using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using log4net;

namespace Tct.ActivityRecorderService.Update
{
	public class FileDownloadManager : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly int FileCleanUpInterval = (int)TimeSpan.FromMinutes(10).TotalMilliseconds;
		private static readonly int FileCacheMaxAge = (int)TimeSpan.FromMinutes(20).TotalMilliseconds;

		private readonly object thisLock = new object(); //protecting bufferedFileSessions
		private readonly Dictionary<Guid, TransferInfo> bufferedFileSessions = new Dictionary<Guid, TransferInfo>();

		public FileDownloadManager()
		{
			ManagerCallbackInterval = FileCleanUpInterval;
		}

		public DownloadSessionInfo StartDownload(IFileData fileData)
		{
			return StartDownload(new MemoryStream(fileData.Data), fileData.FilePath, "cached data");
		}

		public DownloadSessionInfo StartDownload(string filePath)
		{
			return StartDownload(File.OpenRead(filePath), filePath, "file");
		}

		private DownloadSessionInfo StartDownload(Stream stream, string filePath, string type)
		{
			var fileId = Guid.NewGuid();
			log.Debug("Opening " + type + " for reading id " + fileId + " path " + filePath);

			long remainder;
			long chunkCount;
			long chunkSize;
			int sessionCount;
			CalculateChunkCountSizeAndRemainder(stream.Length, out remainder, out chunkCount, out chunkSize);
			lock (thisLock)
			{
				bufferedFileSessions.Add(fileId,
										 new TransferInfo
											 {
												 FileToDownload = stream,
												 LastAccess = Environment.TickCount,
											 });
				sessionCount = bufferedFileSessions.Count;
			}
			log.Debug("Opened sessions (Add) " + sessionCount);
			return new DownloadSessionInfo
				{
					ChunkCount = chunkCount + (remainder > 0 ? 1 : 0),
					FileId = fileId
				};
		}

		private void ClearExpired()
		{
			int sessionCount, sessionCountBefore;
			lock (thisLock)
			{
				sessionCountBefore = bufferedFileSessions.Count;
				var itemsToRemove = bufferedFileSessions.Where(t => (uint)(Environment.TickCount - t.Value.LastAccess) > FileCacheMaxAge).ToList();
				foreach (var item in itemsToRemove)
				{
					item.Value.FileToDownload.Flush();
					item.Value.FileToDownload.Close();
					bufferedFileSessions.Remove(item.Key);
					log.Info("Clearing item with id " + item.Key);
				}
				sessionCount = bufferedFileSessions.Count;
			}
			log.Debug("Opened sessions (ClearExpired) " + sessionCountBefore + " -> " + sessionCount);
		}

		public byte[] DownloadChunk(Guid fileId, long chunkIndex)
		{
			Stream chunkBuffer;
			long remainder;
			long chunkCount;
			long chunkSize;
			int sessionCount;
			lock (thisLock)
			{
				TransferInfo transferInfo;
				if (!bufferedFileSessions.TryGetValue(fileId, out transferInfo))
				{
					throw new ArgumentException("Invalid sessionId");
				}
				chunkBuffer = transferInfo.FileToDownload;
				transferInfo.LastAccess = Environment.TickCount;
				sessionCount = bufferedFileSessions.Count;
			}
			log.Debug("Opened sessions (GetChunk) " + sessionCount);
			CalculateChunkCountSizeAndRemainder(chunkBuffer.Length, out remainder, out chunkCount, out chunkSize);

			long totalChunks = chunkCount + (remainder > 0 ? 1 : 0);
			if (chunkIndex >= totalChunks)
				throw new ArgumentException(string.Format("Cannot download chunk index {0} for file because it there are only {1} chunks total", chunkIndex, totalChunks));

			var thisChunkSize = chunkSize;

			if (chunkIndex == chunkCount && remainder > 0)
			{
				thisChunkSize = remainder;
			}

			var chunk = new byte[thisChunkSize];
			chunkBuffer.Read(chunk, 0, (int)thisChunkSize);
			if (chunkIndex == chunkCount)
			{
				chunkBuffer.Flush();
				chunkBuffer.Close();
				lock (thisLock)
				{
					bufferedFileSessions.Remove(fileId);
					sessionCount = bufferedFileSessions.Count;
				}
				log.Debug("Opened sessions (Done) " + sessionCount);
			}
			return chunk;
		}

		private static void CalculateChunkCountSizeAndRemainder(long dataLength, out long remainder, out long chunkCount, out long chunkSize)
		{
			// Compute how much will be left over for the last chunk
			chunkSize = ConfigManager.DownloadChunkSizeBytes;
			remainder = dataLength % chunkSize;
			chunkCount = dataLength / chunkSize;

			// Handle the case of small files
			if (chunkSize >= dataLength)
			{
				chunkSize = dataLength;
				remainder = 0;
				chunkCount = 1;
			}
		}

		protected override void ManagerCallbackImpl()
		{
			// remove old items
			ClearExpired();
		}
	}

	internal class TransferInfo
	{
		public Stream FileToDownload { get; set; }
		public long LastAccess { get; set; }
	}
}
