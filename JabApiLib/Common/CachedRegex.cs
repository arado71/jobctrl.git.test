using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.Java.Common
{
	public class CachedRegex
	{
		private readonly CachedDictionary<string, bool> isMatchDict = new CachedDictionary<string, bool>(TimeSpan.FromHours(1), true);
		public RegexOrContains Regex { get; set; }
		public bool IsMatch(string value)
		{
			bool isMatch;
			if (isMatchDict.TryGetValue(value, out isMatch))
				return isMatch;
			isMatch = Regex.IsMatch(value);
			isMatchDict.Add(value, isMatch);
			return isMatch;
		}
	}
}
