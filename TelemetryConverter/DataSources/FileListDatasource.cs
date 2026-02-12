using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace TelemetryConverter.DataSources
{
	public class FileListDatasource : IDataSource<string>, IDisposable
	{
		private readonly CancellationTokenSource cts = new CancellationTokenSource();
		private readonly BlockingCollection<string> fileQueue = new BlockingCollection<string>();
		private FileSystemWatcher watcher; 
		private readonly HashSet<string> processed = new HashSet<string>();
		private Thread worker;
		private readonly string path;
		private readonly string fileFilter;

		public event EventHandler<SingleValueEventArgs<string>> DataAvailable;

		public FileListDatasource(string path, string fileFilter)
		{
			this.path = path;
			this.fileFilter = fileFilter;
		}

		public void Start()
		{
			worker = new Thread(() => ProcessingJob(cts.Token)) { IsBackground = false };
			worker.Start();
			InitWatcher();
			ProcessPath(path);
		}

		private void ProcessingJob(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				try
				{
					string currentFile;
					if (fileQueue.TryTake(out currentFile, -1, token))
					{
						OnDataAvailable(currentFile);
					}
				}
				catch(OperationCanceledException)
				{ }
			}
		}

		private void OnDataAvailable(string file)
		{
			var evt = DataAvailable;
			if(evt != null) evt(this, new SingleValueEventArgs<string>(file));
		}

		private void InitWatcher()
		{
			//watcher = new FileSystemWatcher(path);
			//watcher.Created += HandleFileCreated;
			//watcher.IncludeSubdirectories = true;
			//watcher.NotifyFilter = NotifyFilters.LastWrite;
			//watcher.Filter = fileFilter;
			//watcher.EnableRaisingEvents = true;

			var periodicWatcher = new PeriodicFileSystemWatcher(path);
			periodicWatcher.DataAvailable += HandleFileCreated;
			periodicWatcher.Start();

		}

		private void HandleFileCreated(object sender, FileSystemEventArgs fileSystemEventArgs)
		{
			if (fileSystemEventArgs.ChangeType == WatcherChangeTypes.Created)
			{
				EnsureProcessing(fileSystemEventArgs.FullPath);
			}
		}

		private void EnsureProcessing(string fileName)
		{
			lock (processed)
			{
				if (processed.Contains(fileName)) return;
				processed.Add(fileName);
			}

			try
			{
				fileQueue.TryAdd(fileName, -1, cts.Token);
			}
			catch(OperationCanceledException)
			{ }
		}

		private void ProcessPath(string path)
		{
			foreach (var file in Directory.EnumerateFiles(path, fileFilter))
			{
				EnsureProcessing(file);
			}

			foreach (var folder in Directory.EnumerateDirectories(path))
			{
				ProcessPath(folder);
			}
		}

		public void Dispose()
		{
			watcher.EnableRaisingEvents = false;
			cts.Cancel();
			worker.Join();
			cts.Dispose();
			fileQueue.Dispose();
		}
	}
}
