using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelemetryConverter.Grafana;

namespace TelemetryConverter.Aggregators
{
	public class TableColumn
	{
		public string Name { get; set; }
		public Type Type { get; set; }
		public TableResult.SortType Sort { get; set; }

		public TableColumn(string name, Type columnType, TableResult.SortType sort = TableResult.SortType.None)
		{
			Name = name;
			Type = columnType;
			Sort = sort;
		}
	}
}
