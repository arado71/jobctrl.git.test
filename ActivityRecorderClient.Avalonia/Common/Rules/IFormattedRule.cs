using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.Rules
{
	public interface IFormattedRule : IRule
	{
		Dictionary<string, string> FormattedNamedGroups { get; }
	}
}
