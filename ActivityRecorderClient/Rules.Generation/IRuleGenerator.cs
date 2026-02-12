using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Rules.Generation
{
	public interface IRuleGenerator
	{
		IRule GetRuleFromWindow(DesktopWindow desktopWindow, IRule matchingRule);
	}
}
