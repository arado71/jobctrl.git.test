using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.ClientErrorReporting
{
	static class CompressHelper
	{
		public static void WriteZipData(Stream stream, IList<string> fileNames, int folderOffset, Action<string, int> progress = null, Func<bool> cancellationPending = null)
		{
			using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
			{
				var sizeOfFiles = fileNames.Sum(x => new FileInfo(x).Length);
				long readSum = 0;
				foreach (var fileName in fileNames)
				{
					if (cancellationPending != null && cancellationPending()) return;
					var name = Path.GetFileName(fileName);
					if (progress != null) progress(name, 0);
					WriteZipEntry(zip, fileName, folderOffset, progress != null ? p => progress(name, (int)(100 * (readSum += p) / sizeOfFiles)) : default(Action<long>), cancellationPending);
				}
			}
		}

		private static void WriteZipEntry(ZipArchive zip, string fileName, int folderOffset, Action<long> progress = null, Func<bool> cancellationPending = null)
		{
			if (!File.Exists(fileName)) return;

			var fi = new FileInfo(fileName);
			var entryName = fileName.Substring(folderOffset).Replace('\\', '/');
			var entry = zip.CreateEntry(entryName, CompressionLevel.Optimal);
			entry.LastWriteTime = fi.SafeLastWriteTime();

			using (var entryStream = entry.Open())
			using (var reader = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				var buffer = new byte[4096];
				CopyStream(reader, entryStream, buffer, progress, cancellationPending);
			}
		}

		private static DateTime SafeLastWriteTime(this FileInfo fileInfo)
		{
			try
			{
				return fileInfo.LastWriteTime;
			}
			catch (ArgumentOutOfRangeException)
			{
				log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType).Warn($"The file has invalid creation time ({fileInfo.FullName})");
				return DateTime.Now;
			}
		}

		//It can be made faster, because read and write waits each other. (http://stackoverflow.com/questions/230128/best-way-to-copy-between-two-stream-instances#comment1560483_230141)
		private static void CopyStream(Stream input, Stream output, byte[] buffer, Action<long> progress = null, Func<bool> cancellationPending = null)
		{
			int read;
			while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
			{
				if (cancellationPending != null && cancellationPending()) return;
				output.Write(buffer, 0, read);
				if (progress != null) progress(read);
			}
		}
	}
}
