using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Plugins;

namespace Tct.ActivityRecorderClient.Rules
{
	public interface IRule
	{
		string Name { get; }
		bool IsEnabled { get; }
		bool IsRegex { get; }
		bool IgnoreCase { get; }
		string TitleRule { get; }
		string ProcessRule { get; }
		string UrlRule { get; }
		IEnumerable<KeyValuePair<CaptureExtensionKey, string>> ExtensionRules { get; }
		WindowScopeType WindowScope { get; }
		IEnumerable<IRule> Children { get; }
	}
}
