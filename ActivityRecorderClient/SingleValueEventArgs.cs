using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.View;

namespace Tct.ActivityRecorderClient
{
	public class SingleValueEventArgs<T> : EventArgs
	{
		public T Value { get; private set; }

		public SingleValueEventArgs(T value)
		{
			Value = value;
		}
	}

	public static class SingleValueEventArgs
	{
		public static SingleValueEventArgs<T> Create<T>(T value)
		{
			return new SingleValueEventArgs<T>(value);
		}
	}
}