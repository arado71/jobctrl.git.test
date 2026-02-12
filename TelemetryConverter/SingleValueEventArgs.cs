using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelemetryConverter
{
	public class SingleValueEventArgs<T> : EventArgs
	{
		public T Value { get; private set; }

		public SingleValueEventArgs(T value)
		{
			Value = value;
		}
	}
}
