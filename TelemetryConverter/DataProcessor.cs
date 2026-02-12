using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tct.ActivityRecorderService;
using TelemetryConverter.Aggregators;
using TelemetryConverter.Database;
using TelemetryConverter.DataSources;
using TelemetryConverter.Grafana;
using TelemetryConverter.Telemetry;

namespace TelemetryConverter
{
	public class DataProcessor : IDisposable
	{
		private readonly Dictionary<string, List<ISeriesAggregator>> seriesAggregators = new Dictionary<string, List<ISeriesAggregator>>();
		private readonly Dictionary<string, List<ITableAggregator>> tableAggregators = new Dictionary<string, List<ITableAggregator>>();
		private readonly List<IDataSource> dataSources = new List<IDataSource>();
		private readonly SeriesDatabase database = new SeriesDatabase(TimeSpan.FromDays(60));
		private readonly object thisLock = new object();

		public bool IsDisposed { get; private set; }

		public IEnumerable<string> Categories { get { return seriesAggregators.Keys; } }

		public void AddEventSource<TSource>(IDataSource<TSource> dataSource) where TSource : IEvent
		{
			lock (thisLock)
			{
				dataSource.DataAvailable += (sender, args) =>
				{
					database.EnsureEventTable<TSource>().Add(args.Value);
				};
				dataSources.Add(dataSource);
			}
		}

		public void AddIntervalSource<TSource>(IDataSource<TSource> dataSource) where TSource : IInterval
		{
			lock (thisLock)
			{
				dataSource.DataAvailable += (sender, args) =>
				{
					database.EnsureIntervalTable<TSource>().Add(args.Value);
				};
				dataSources.Add(dataSource);
			}
		}

		public void AddAggregator(ISeriesAggregator seriesAggregator)
		{
			var aggregateList = seriesAggregators.GetValueOrCreate(seriesAggregator.Category, () => new List<ISeriesAggregator>());
			aggregateList.Add(seriesAggregator);
		}

		public void AddAggregator(ITableAggregator tableAggregator)
		{
			var aggregateList = tableAggregators.GetValueOrCreate(tableAggregator.Category, () => new List<ITableAggregator>());
			aggregateList.Add(tableAggregator);
		}

		public IEnumerable<string> ListAggregatorNames(string category)
		{
			return GetSeriesAggregators(category).Select(x => x.Name).Union(GetTableAggregators(category).Select(x => x.Name));
		}

		public void Start()
		{
			lock (thisLock)
			{
				foreach (var dataSource in dataSources)
				{
					dataSource.Start();
				}
			}
		}

		public IEnumerable<TableResult> GetTableAggregate(string category, IEnumerable<string> aggregatorNames, DateTime startDate, DateTime endDate, TimeSpan resolution)
		{
			var nameFilter = new HashSet<string>(aggregatorNames, StringComparer.InvariantCultureIgnoreCase);
			var filteredAggregators = GetTableAggregators(category).Where(x => nameFilter.Contains(x.Name));
			return GetTableAggregate(filteredAggregators, database, startDate, endDate, resolution);
		}

		public IEnumerable<QueryResult> GetSeriesAggregate(string category, IEnumerable<string> aggregatorNames, DateTime startDate, DateTime endDate, TimeSpan resolution)
		{
			var nameFilter = new HashSet<string>(aggregatorNames, StringComparer.InvariantCultureIgnoreCase);
			var filteredAggregators = GetSeriesAggregators(category).Where(x => nameFilter.Contains(x.Name));
			if (!filteredAggregators.Any() && aggregatorNames.FirstOrDefault().Contains('|'))
			{

				ISeriesAggregator aggregator;
				if (category == "server")
				{
					aggregator = new GenericSeriesAggregator<LogEvent>();
				}
				else
				{
					aggregator = new GenericSeriesAggregator<TelemetryEvent>(){CustomCategory = category,CustomName = aggregatorNames.FirstOrDefault()};
				}
				filteredAggregators = new[]{ aggregator };
			}
			return GetSeriesAggregate(filteredAggregators, database, startDate, endDate, resolution);
		}

		private static IEnumerable<TableResult> GetTableAggregate(IEnumerable<ITableAggregator> aggregators, SeriesDatabase database,
			DateTime startDate, DateTime endDate, TimeSpan resolution)
		{
			foreach (var aggregator in aggregators)
			{
				var result = new TableResult();
				foreach (var column in aggregator.Columns)
				{
					result.AddColumn(column.Name, column.Type, column.Sort);
				}

				foreach (var row in aggregator.GetRows(database, new Interval(startDate, endDate), resolution))
				{
					result.AddRow(row);
				}

				yield return result;
			}
		}

		private static IEnumerable<QueryResult> GetSeriesAggregate(IEnumerable<ISeriesAggregator> aggregators, SeriesDatabase database,
			DateTime startDate, DateTime endDate, TimeSpan resolution)
		{
			var binCount = (endDate.Ticks - startDate.Ticks - 1) / resolution.Ticks + 1;
			var series = aggregators.ToDictionary(aggregator => aggregator, aggregator => new QueryResult(aggregator.Name));

			var intervalStart = startDate;
			for (var i = 0; i < binCount; ++i)
			{
				var intervalEnd = new DateTime(startDate.Ticks + (i + 1) * resolution.Ticks);
				foreach (var aggregator in series.Keys)
				{
					series[aggregator].Add(aggregator.GetResult(database, new Interval(intervalStart, intervalEnd)), intervalStart);
				}

				intervalStart = intervalEnd;
			}

			return series.Values;
		}

		private IEnumerable<ITableAggregator> GetTableAggregators(string category)
		{
			var aggregatorList = tableAggregators.GetValueOrDefault(category);
			if (aggregatorList == null) return Enumerable.Empty<ITableAggregator>();
			return aggregatorList;
		}

		private IEnumerable<ISeriesAggregator> GetSeriesAggregators(string category)
		{
			var aggregatorList = seriesAggregators.GetValueOrDefault(category);
			if (aggregatorList == null) return Enumerable.Empty<ISeriesAggregator>();
			return aggregatorList;
		}

		public void Dispose()
		{
			lock (thisLock)
			{
				foreach (var dataSource in dataSources)
				{
					dataSource.Dispose();
				}

				IsDisposed = true;
			}
		}
	}
}
