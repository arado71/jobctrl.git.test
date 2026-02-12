using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelemetryConverter.DataSources;

namespace TelemetryConverter.Telemetry
{
	public class TelemetryItemDataSource : ConverterDataSource<TelemetryItem, string>
	{
		public TelemetryItemDataSource(string basePath) : base(new FileListDatasource(basePath, "*.jtd"))
		{
		}

		protected override IEnumerable<TelemetryItem> Convert(string filename)
		{
			var item = SafeLoadTelemetryItem(filename);
			if (item == null) yield break;
			yield return new TelemetryItem
			{
				UserId = item.UserId,
				ComputerId = item.ComputerId,
				StartDate = item.StartDate,
				EndDate = item.EndDate,
				Events = GetEventsFromTelemetryItem(item).ToArray(),
			};
		}

		private IEnumerable<TelemetryEvent> GetEventsFromTelemetryItem(Tct.ActivityRecorderService.Telemetry.TelemetryItem item)
		{
			foreach (var evt in item.EventNameValueOccurences)
			{
				var eventName = evt.Key;
				foreach (var param in evt.Value)
				{
					var parameter = param.Key;
					foreach (var date in param.Value)
					{
						yield return new TelemetryEvent(item.UserId, item.ComputerId, date, eventName, parameter);
					}
				}
			}
		}

		private Tct.ActivityRecorderService.Telemetry.TelemetryItem SafeLoadTelemetryItem(string filename)
		{
			try
			{
				using (var stream = File.Open(filename, FileMode.Open))
				{
					return Tct.ActivityRecorderService.Telemetry.TelemetryItem.ReadFrom(stream);
				}
			}
			catch (Exception ex)
			{
				return null;
			}
		}
	}
}
