using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;

namespace TelemetryConverter.Grafana
{
	public class TableResult
	{
		public enum SortType
		{
			None = 0,
			Ascending,
			Descending,
		}

		[JsonProperty(PropertyName="columns")]
		public List<Dictionary<string, object>> Columns { get; private set; }
		[JsonProperty(PropertyName="rows")]
		public List<object[]> Rows { get; private set; }
		[JsonProperty(PropertyName="type")]
		public string Type { get { return "table"; } }

		public TableResult()
		{
			Columns = new List<Dictionary<string, object>>();
			Rows = new List<object[]>();
		}

		private List<int> dateColumnIdx = new List<int>();

		public void AddColumn(string name, Type tableType, SortType sorting = SortType.None)
		{
			var columnDef = new Dictionary<string, object>();
			columnDef.Add("text", name);
			if (sorting == SortType.Ascending)
			{
				columnDef.Add("sort", true);
			} else if (sorting == SortType.Descending)
			{
				columnDef.Add("sort", true);
				columnDef.Add("desc", true);
			}

			if (tableType == typeof(DateTime))
			{
				dateColumnIdx.Add(Columns.Count);
				columnDef.Add("type", "time");
			}

			Columns.Add(columnDef);
		}

		public void AddRow(object[] values)
		{
			foreach (var dateColIdx in dateColumnIdx)
			{
				values[dateColIdx] = ((DateTime) values[dateColIdx]).ToUnixTime();
			}

			Rows.Add(values);
		}
	}
}
