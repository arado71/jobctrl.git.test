using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelemetryConverter.Database
{
	public class IntervalTable<T> : IEnumerable<T>
		where T : IInterval
	{
		protected readonly IntervalOrderedList<T> container = new IntervalOrderedList<T>();

		public IntervalOrderedList<T> Data { get { return container; } }

		public IntervalTable()
		{
		}

		public void Add(T item)
		{
			container.Add(item.StartDate, item.EndDate, item);
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
