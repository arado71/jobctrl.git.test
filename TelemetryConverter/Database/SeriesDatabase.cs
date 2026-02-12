using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelemetryConverter.Database
{
	public class SeriesDatabase
	{
		private readonly Dictionary<Type, object> eventTables = new Dictionary<Type, object>();
		private readonly Dictionary<Type, object> intervalTables = new Dictionary<Type, object>();

		private readonly TimeSpan maxAge;

		public SeriesDatabase(TimeSpan maxAge)
		{
			this.maxAge = maxAge;
		}

		public void AddTable<T>() where T : ITimeBased
		{
			if(typeof(IInterval).IsAssignableFrom(typeof(T)))
			{
				var tableType = typeof(IntervalTable<>).MakeGenericType(typeof(T));
				var table = tableType.GetConstructor(null).Invoke(null);
				intervalTables.Add(typeof(T), table);
			}

			if (typeof(IEvent).IsAssignableFrom(typeof(T)))
			{
				var tableType = typeof(EventTable<>).MakeGenericType(typeof(T));
				var table = tableType.GetConstructor(null).Invoke(null);
				eventTables.Add(typeof(T), table);
			}
		}

		public EventTable<T> EnsureEventTable<T>() where T : IEvent
		{
			object tableRaw;
			if (!eventTables.TryGetValue(typeof(T), out tableRaw))
			{
				var table = new EventTable<T>();
				eventTables.Add(typeof(T), table);
				return table;
			}

			return tableRaw as EventTable<T>;
		}

		public IntervalTable<T> EnsureIntervalTable<T>() where T : IInterval
		{
			object tableRaw;
			if (!intervalTables.TryGetValue(typeof(T), out tableRaw))
			{
				var table = new IntervalTable<T>();
				intervalTables.Add(typeof(T), table);
				return table;
			}

			return tableRaw as IntervalTable<T>;
		}

		public IntervalTable<T> GetIntervalTable<T>() where T : IInterval
		{
			object tableRaw;
			if (!intervalTables.TryGetValue(typeof(T), out tableRaw)) return null;
			return tableRaw as IntervalTable<T>;
		}

		public EventTable<T> GetEventTable<T>() where T : IEvent
		{
			object tableRaw;
			if (!eventTables.TryGetValue(typeof(T), out tableRaw)) return null;
			return tableRaw as EventTable<T>;
		}
	}
}
