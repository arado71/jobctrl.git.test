using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.ActivityRecorderServiceReference
{
	public partial class WorkSelector
	{
		public override string ToString()
		{
			return "WorkSelector "
				+ " n:" + Name
				+ " rule:" + Rule
				+ " template:" + TemplateText
				+ (IsRegex ? " Regex" : "")
				+ (IgnoreCase ? "" : " CaseSensitive")
				;
		}
	}
}
