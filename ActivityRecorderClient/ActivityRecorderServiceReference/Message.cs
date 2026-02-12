using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Tct.ActivityRecorderClient.ActivityRecorderServiceReference
{
	public partial class Message
	{
		public string ContentWithoutFormatting
		{
			get
			{
				var result = Regex.Replace(Content, "<b>", "");
				result = Regex.Replace(result, "<\\/b>", "");
				result = Regex.Replace(result, "<i>", "");
				result = Regex.Replace(result, "<\\/i>", "");
				result = Regex.Replace(result, "<a\\s+(?:[^>]*?\\s+)?href=([\"'])(.*?)\\1[^>]*>([^<]*)<\\/a>", "$3");
				return result;
			}
		}
	}
}
