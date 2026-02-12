using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelemetryConverter.DataSources
{
	public interface IDataSource : IDisposable
	{
		void Start();
	}

	public interface IDataSource<T> : IDataSource
	{
		event EventHandler<SingleValueEventArgs<T>> DataAvailable;
	}
}
