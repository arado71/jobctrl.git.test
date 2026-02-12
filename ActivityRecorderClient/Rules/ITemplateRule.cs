using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.Capturing.Plugins;

namespace Tct.ActivityRecorderClient.Rules
{
	public interface ITemplateRule : IRule
	{
		new string TitleRule { get; set; }
		new string ProcessRule { get; set; }
		new string UrlRule { get; set; }
		new IEnumerable<KeyValuePair<CaptureExtensionKey, string>> ExtensionRules { get; set; }
	}
}
