using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.Rules.Collector
{
	public class CollectorRuleDetectorResult
	{
		public readonly Dictionary<string, string> CapturedValues;

		public CollectorRuleDetectorResult(Dictionary<string, string> capturedValues)
		{
			DebugEx.EnsureBgThread();
			CapturedValues = capturedValues;
		}

		public override string ToString()
		{
			if (CapturedValues == null || CapturedValues.Count == 0) return "";
			//return string.Join(", ", CapturedValues.Select(n => n.Key + ":" + n.Value).ToArray());
			var sb = new StringBuilder();
			var isFirst = true;
			foreach (var kvp in CapturedValues)
			{
				sb.Append(isFirst ? "" : ", ").Append(kvp.Key).Append(":").Append(kvp.Value);
				isFirst = false;
			}
			return sb.ToString();
		}
	}
}
