using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelemetryConverter.Telemetry;

namespace TelemetryConverter.Aggregators
{
	class FeatureEventTableAggregator : EventTableAggregator
	{
		public override string GetName()
		{
			return "Feature events";
		}

		public override Predicate<TelemetryEvent> EventPredicate
		{
			get { return x => x.Name == "Feature"; }
		}
	}
}
