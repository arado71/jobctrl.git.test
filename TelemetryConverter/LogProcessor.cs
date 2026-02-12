using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tct.ActivityRecorderService.Telemetry;

namespace TelemetryConverter
{
	public class LogProcessor : IDisposable
	{
		private readonly CancellationTokenSource cts = new CancellationTokenSource();
		private readonly BlockingCollection<string> fileQueue = new BlockingCollection<string>();
		private readonly List<TelemetryEvent> eventList = new List<TelemetryEvent>();
		private FileSystemWatcher watcher; 
		private readonly HashSet<string> processed = new HashSet<string>();
		private Thread worker;
		private string path;

		public event EventHandler<SingleValueEventArgs<TelemetryEvent[]>> EventsReceived;

		public TelemetryEvent[] Events
		{
			get
			{
				lock (eventList)
				{
					return eventList.ToArray();
				}
			}
		}

		public LogProcessor(string path)
		{
			this.path = path;
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
				string currentFile;
				try
				{
					if (fileQueue.TryTake(out currentFile, -1, token))
					{
						var events = Load(currentFile);
						ProcessEvents(events.ToArray());
					}
				}
				catch(OperationCanceledException)
				{ }
			}
		}

		private void ProcessEvents(TelemetryEvent[] events)
		{
			lock (eventList)
			{
				eventList.AddRange(events);
			}

			OnEventsReceived(events.ToArray());
		}

		private void OnEventsReceived(TelemetryEvent[] events)
		{
			var evt = EventsReceived;
			if(evt != null) evt(this, new SingleValueEventArgs<TelemetryEvent[]>(events));
		}

		private void InitWatcher()
		{
			watcher = new FileSystemWatcher(path);
			watcher.Created += HandleFileCreated;
			watcher.IncludeSubdirectories = true;
			watcher.NotifyFilter = NotifyFilters.LastWrite;
			watcher.Filter = "*.jtf";
			watcher.EnableRaisingEvents = true;
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
			foreach (var file in Directory.EnumerateFiles(path, "*.jtd"))
			{
				EnsureProcessing(file);
			}

			foreach (var folder in Directory.EnumerateDirectories(path))
			{
				ProcessPath(folder);
			}
		}

		private IEnumerable<TelemetryEvent> Load(string file)
		{
			TelemetryItem item;
			try
			{
				using (var stream = File.Open(file, FileMode.Open))
				{
					item = TelemetryItem.ReadFrom(stream);
				}
			}
			catch (Exception ex)
			{
				yield break;
			}

			foreach (var evt in item.EventNameValueOccurences)
			{
				var eventName = evt.Key;
				foreach (var param in evt.Value)
				{
					var parameter = param.Key;
					foreach (var date in param.Value)
					{
						yield return new TelemetryEvent(item.UserId, item.ComputerId, date, eventName, parameter);
					}
				}
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
