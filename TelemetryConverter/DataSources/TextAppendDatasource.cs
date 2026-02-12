using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace TelemetryConverter.DataSources
{
	public class TextAppendDatasource : IDataSource<string[]>
	{
		private readonly CancellationTokenSource cts = new CancellationTokenSource();
		private readonly BlockingCollection<string> fileQueue = new BlockingCollection<string>();
		private readonly BlockingCollection<string[]> newLines = new BlockingCollection<string[]>();
		private readonly Dictionary<string, long> filePositions = new Dictionary<string, long>();
		private FileSystemWatcher watcher;
		private Thread diffWorker, eventWorker;
		private readonly string headFile;

		public event EventHandler<SingleValueEventArgs<string[]>> DataAvailable;

		public TextAppendDatasource(string headFile)
		{
			this.headFile = headFile;
		}

		public void Start()
		{
			diffWorker = new Thread(() => DiffJob(cts.Token)) { IsBackground = false };
			diffWorker.Start();
			eventWorker = new Thread(() => EventJob(cts.Token)) { IsBackground = true };
			eventWorker.Start();
			InitWatcher();
			EnsureProcessing(headFile);
		}

		private void EventJob(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				try
				{
					string[] currentLines;
					if (newLines.TryTake(out currentLines, -1, token))
					{
						OnDataAvailable(currentLines);
					}
				}
				catch (OperationCanceledException) { }
			}
		}

		private void DiffJob(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				try
				{
					string currentFile;
					if (fileQueue.TryTake(out currentFile, -1, token))
					{
						try
						{
							var lines = ReadUnreadLines(currentFile, Encoding.UTF8).ToArray();
							newLines.TryAdd(lines);
						}
						catch (IOException ex)
						{
							fileQueue.TryAdd(currentFile); // Add back to queue
							Thread.Sleep(100); // meh
						}
					}
				}
				catch (OperationCanceledException)
				{ }
			}
		}

		private void DiffJob2(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				try
				{
					var lines = ReadUnreadLines(headFile, Encoding.UTF8).ToArray();
					newLines.TryAdd(lines);
					Thread.Sleep(1000); // meh
				}
				catch (OperationCanceledException)
				{ }
			}
		}

		private IEnumerable<string> ReadUnreadLines(string filename, Encoding encoding)
		{
			string newData;
			var lastPosition = filePositions.GetValueOrCreate(filename, () => 0);
			using (var stream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			using (var reader = new StreamReader(stream, encoding))
			{
				var fileSize = new FileInfo(filename);
				if (fileSize.Length < lastPosition)
				{
					lastPosition = 0;
				}

				stream.Seek(lastPosition, SeekOrigin.Begin);
				newData = reader.ReadToEnd();
			}

			if (string.IsNullOrEmpty(newData)) return Enumerable.Empty<string>();
			var lines = newData.Split(new[] { "\r\n" }, StringSplitOptions.None).ToList();
			if (lines.Count == 0) return Enumerable.Empty<string>();
			lines.RemoveAt(lines.Count - 1);
			filePositions[filename] = lastPosition + lines.Sum(x => encoding.GetByteCount(x)) + lines.Count * 2;
			return lines;
		}

		private void OnDataAvailable(string[] lines)
		{
			var evt = DataAvailable;
			if (evt != null) evt(this, new SingleValueEventArgs<string[]>(lines));
		}

		private void InitWatcher()
		{
			watcher = new FileSystemWatcher(Path.GetDirectoryName(headFile));
			watcher.Changed += HandleFileChanged;
			watcher.IncludeSubdirectories = false;
			watcher.NotifyFilter = NotifyFilters.LastWrite;
			watcher.Filter = Path.GetFileName(headFile);
			watcher.EnableRaisingEvents = true;
		}

		private void HandleFileChanged(object sender, FileSystemEventArgs fileSystemEventArgs)
		{
			EnsureProcessing(fileSystemEventArgs.FullPath);
		}

		private void EnsureProcessing(string fileName)
		{
			try
			{
				fileQueue.TryAdd(fileName, -1, cts.Token);
			}
			catch (OperationCanceledException)
			{ }
		}

		public void Dispose()
		{
			watcher.EnableRaisingEvents = false;
			cts.Cancel();
			diffWorker.Join();
			eventWorker.Join();
			cts.Dispose();
			fileQueue.Dispose();
			newLines.Dispose();
		}
	}
}
