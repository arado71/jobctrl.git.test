using System.Collections;
using System.Collections.Generic;

namespace Tct.ActivityRecorderClient.View.Controls
{
	public class FixedStack<T> : IEnumerable<T>
	{
		private readonly LinkedList<T> container = new LinkedList<T>();
		private readonly int navigationSize;

		public int Count
		{
			get { return container.Count; }
		}

		public FixedStack(int size)
		{
			navigationSize = size;
		}

		public IEnumerator<T> GetEnumerator()
		{
			return container.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return container.GetEnumerator();
		}

		public T Pop()
		{
			T res = container.First.Value;
			container.RemoveFirst();
			return res;
		}

		public void Push(T element)
		{
			if (container.Count == navigationSize) container.RemoveLast();
			container.AddFirst(element);
		}
	}
}