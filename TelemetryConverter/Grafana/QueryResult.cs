using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TelemetryConverter.Grafana
{
	public class QueryResult
	{
		[JsonProperty(PropertyName = "target")]
		public string Name { get; private set; }
		[JsonProperty(PropertyName = "datapoints")]
		public List<object[]> Values { get; private set; }

		public QueryResult(string name)
		{
			Name = name;
			Values = new List<object[]>();
		}

		public void Add(double value, DateTime timestamp)
		{
			Values.Add(new object[] { value, timestamp.ToUnixTime() });
		}
	}
}
