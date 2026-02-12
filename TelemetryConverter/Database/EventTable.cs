using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelemetryConverter.Database
{
	public class EventTable<T> : IEnumerable<T>
		where T : IEvent
	{
		protected readonly DateOrderedList<T> container = new DateOrderedList<T>();

		public DateOrderedList<T> Data { get { return container; } }

		public EventTable()
		{
		}

		public void Add(T item)
		{
			container.Add(item.Timestamp, item);
		}

		public void AddRange(IEnumerable<T> items)
		{
			foreach (var item in items)
			{
				Add(item);
			}
		}

		public IEnumerator<T> GetEnumerator()
		{
			return container.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
