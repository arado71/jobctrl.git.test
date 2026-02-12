using System.Collections.Generic;
using System.Linq;

namespace Tct.ActivityRecorderClient.View.Controls
{
	public class LineStack
	{
		private readonly LinkedList<int> container = new LinkedList<int>();

		public int Smallest { get; private set; }

		public LineStack()
		{
			Smallest = int.MaxValue;
		}

		public void Decrement()
		{
			if (container.Count == 0) return;
			if (Smallest > 0 && Smallest != int.MaxValue) Smallest--;
			for (LinkedListNode<int> node = container.First; node != container.Last.Next; node = node.Next)
			{
				node.Value = --node.Value;
			}
		}

		public void Pop()
		{
			container.RemoveLast();
			Smallest = container.Count > 0 ? container.Min() : int.MaxValue;
		}

		public void Push(int val)
		{
			container.AddLast(val);
			if (val < Smallest) Smallest = val;
		}
	}
}