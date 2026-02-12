using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelemetryConverter.DataSources
{
	public abstract class ConverterDataSource<T, TBase> : IDataSource<T>
	{
		private IDataSource<TBase> baseDataSource;

		public event EventHandler<SingleValueEventArgs<T>> DataAvailable;

		protected ConverterDataSource(IDataSource<TBase> baseDataSource)
		{
			this.baseDataSource = baseDataSource;
			baseDataSource.DataAvailable += HandleDataAvailable;
		}

		protected virtual void HandleDataAvailable(object sender, SingleValueEventArgs<TBase> singleValueEventArgs)
		{
			var convertedItems = Convert(singleValueEventArgs.Value);
			foreach (var item in convertedItems)
			{
				OnDataAvailable(item);
			}
		}

		protected void OnDataAvailable(T data)
		{
			var evt = DataAvailable;
			if(evt != null) evt(this, new SingleValueEventArgs<T>(data));
		}

		protected abstract IEnumerable<T> Convert(TBase input);

		public void Dispose()
		{
			baseDataSource.Dispose();
		}

		public void Start()
		{
			baseDataSource.Start();
		}
	}
}
