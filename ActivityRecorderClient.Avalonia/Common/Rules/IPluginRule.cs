using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Plugins;

namespace Tct.ActivityRecorderClient.Rules
{
	public interface IPluginRule
	{
		IEnumerable<KeyValuePair<CaptureExtensionKey, string>> ExtensionRules { get; }
		Dictionary<string, List<ExtensionRuleParameter>> ExtensionRuleParametersById { get; }
		List<WindowRule> Children { get; }
		int ServerId { set; get; }
	}
}
