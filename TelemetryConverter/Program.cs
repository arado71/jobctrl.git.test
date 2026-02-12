using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TelemetryConverter.Aggregators;
using TelemetryConverter.Grafana;
using TelemetryConverter.ServerLog;
using TelemetryConverter.Telemetry;
using TelemetryItem = TelemetryConverter.Telemetry.TelemetryItem;

namespace TelemetryConverter
{
	class Program
	{
		private static readonly AutoResetEvent canTerminateLoop = new AutoResetEvent(false);

		public static void Main(string[] args)
		{
			if (args.Length < 2)
			{
				Console.WriteLine("Invalid parameters");
				return;
			}

			var path = args[0];
			var logPath = args[1];
			//var path = args.Length > 0 ? args[0] : Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			if (!Directory.Exists(path))
			{
				var color = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Invalid path");
				Console.ForegroundColor = color;
				return;
			}

			Console.CancelKeyPress += HandleConsoleCancelKeypressed;

			using (var processor = new DataProcessor())
			{
				var telemetrySource = new TelemetryItemDataSource(path);

				processor.AddEventSource(new ServerLogDataSource(logPath));
				processor.AddEventSource(new TelemetryEventDataSource(telemetrySource));
				processor.AddIntervalSource(telemetrySource);

				processor.AddAggregator(new EventCount<LogEvent>("server", "Total events"));
				processor.AddAggregator(new EventCount<TelemetryEvent>("telemetry", "Total events"));
				processor.AddAggregator(new ServerErrorCount());
				processor.AddAggregator(new EventTableAggregator());
				processor.AddAggregator(new FeatureEventTableAggregator());
				processor.AddAggregator(new VersionDataAggregator());		
				processor.AddAggregator(new TelemetryUploadCount());
				processor.AddAggregator(new ErrorTableAggregator());

				processor.Start();

				using (var grafanaServer = new GrafanaHttpServer("http://localhost:3333", processor))
				{
					grafanaServer.Start();
					canTerminateLoop.WaitOne();
				}

				//DumpEvents(Path.Combine(path, "result.csv"), fileListDatasource.Events);
			}

			canTerminateLoop.Set();
		}

		private static void DumpEvents(string fileName, IEnumerable<TelemetryEvent> events)
		{
			using (var stream = File.CreateText(fileName))
			{
				stream.WriteLine("UserId\tComputerId\tTimestamp\tEvent\tParameter");
				foreach (var row in events.OrderBy(x => x.Timestamp))
				{
					stream.WriteLine(string.Join("\t", row.UserId, row.ComputerId, row.Timestamp.ToString("yyyy.MM.dd HH:mm:ss.fff"), row.Name, row.Parameter));
				}
			}
		}

		private static void HandleConsoleCancelKeypressed(object sender, ConsoleCancelEventArgs consoleCancelEventArgs)
		{
			canTerminateLoop.Set();
			canTerminateLoop.WaitOne();
		}
	}
}
