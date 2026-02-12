using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.Capturing.Plugins;
using Tct.ActivityRecorderClient.Rules;

namespace Tct.ActivityRecorderClient.ActivityRecorderServiceReference
{
	public partial class CensorRule : IRule
	{
		public WindowScopeType WindowScope { get { return WindowScopeType.Any; } }

		public bool IsEnabledInNonWorkStatus { get { return false; } }

		public IEnumerable<IRule> Children { get { return null; } } //not supported atm. on the website so we won't support it here (and we don't really need it anyway)

		public IEnumerable<KeyValuePair<CaptureExtensionKey, string>> ExtensionRules
		{
			get
			{
				return null; //not supported atm. on the website so we won't support it here
			}
		}

		public override string ToString()
		{
			return "CensorRule "
				+ RuleType
				+ (IsEnabled ? "" : " DISABLED")
				+ " n:" + Name
				+ " title:" + TitleRule
				+ " proc:" + ProcessRule
				+ " url:" + UrlRule
				+ (IsRegex ? " Regex" : "")
				+ (IgnoreCase ? "" : " CaseSensitive")
				;
		}
	}
}
