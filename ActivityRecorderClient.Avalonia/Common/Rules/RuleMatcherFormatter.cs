using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Rules
{
	public class RuleMatcherFormatter<T> : RuleMatcher<T> where T : class, IFormattedRule
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly Dictionary<string, FormatterExpression> formatters = new Dictionary<string, FormatterExpression>();

		public RuleMatcherFormatter(T rule)
			: base(rule)
		{
			if (rule.FormattedNamedGroups != null)
			{
				foreach (var formatting in rule.FormattedNamedGroups)
				{
					if (string.IsNullOrEmpty(formatting.Key) || string.IsNullOrEmpty(formatting.Value)) continue; //skip empty values
					try
					{
						formatters.Add(formatting.Key, new FormatterExpression(formatting.Value));
					}
					catch (InvalidFormatException e)
					{
						log.WarnFormat("Invalid formatting in rule {0} in parameter {1} at {2}: {3}", rule, formatting.Key, e.Message, e.Position);
					}
				}
			}
		}

		public Dictionary<string, string> GetFormatted(DesktopCapture desktopCapture, params string[] fields)
		{
			var allGroups = GetAllMatchingGroups(desktopCapture);
			if (allGroups == null) return null;
			var res = new Dictionary<string, string>(fields.Length, StringComparer.OrdinalIgnoreCase);
			foreach (var field in fields)
			{
				FormatterExpression formatter;
				string unformatted;
				if (formatters.TryGetValue(field, out formatter))
				{
					Debug.Assert(!res.ContainsKey(field));
					res[field] = formatter.Format(allGroups);
				}
				else if (allGroups.TryGetValue(field, out unformatted))
				{
					Debug.Assert(!res.ContainsKey(field));
					res[field] = unformatted;
				}
			}

			return res;
		}

		public IEnumerable<string> GetFormatterKeys()
		{
			return formatters.Keys;
		}
	}
}
