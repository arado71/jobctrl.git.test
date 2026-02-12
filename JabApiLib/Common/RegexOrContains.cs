using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Tct.Java.Common
{
	public class RegexOrContains
	{
		private Regex regex;
		private string[] strings;
		private StringComparison caseComparison;
		private static readonly Regex keywordListRegex = new Regex(@"^\(\(\?=\.\*(?<Keyw>[^)]+)\)\.\*(?:\|\(\?=\.\*(?<Keyw>[^)]+)\)\.\*)*\)$");
		private static readonly Regex unEscapeRegex = new Regex(@"\\(.)");

		public RegexOrContains(Regex _regex)
		{
			regex = _regex;
		}

		public RegexOrContains(string[] _strings, bool _ignoreCase)
		{
			strings = _strings;
			caseComparison = _ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture;
		}

		public static RegexOrContains Create(string rule, bool isRegex, bool ignoreCase)
		{
			if (isRegex)
			{
				var keywordListMatch = keywordListRegex.Match(rule);
				if (keywordListMatch.Success)
				{
					var keywords = keywordListMatch.Groups["Keyw"].Captures.Cast<Capture>().Select(c => "*" + unEscapeRegex.Replace(c.Value, "$1") + "*").ToArray();
					return new RegexOrContains(keywords, ignoreCase);
				}
				var options = RegexOptions.Singleline;
				if (ignoreCase) options |= RegexOptions.IgnoreCase;
				return new RegexOrContains(new Regex(rule, options));
			}

			return new RegexOrContains(new[] { rule }, ignoreCase);
		}

		public bool IsMatch(string input)
		{
			if (regex != null)
				return regex.IsMatch(input);
			if (strings != null)
			{
				foreach (var str in strings)
				{
					if (WildcardCompare(input, str))
						return true;
				}
				return false;
			}

			throw new ArgumentException("Nor regex nor strings");
		}

		private bool WildcardCompare(string input, string text)
		{
			if (input == null) input = string.Empty;
			// empty sample causes negative result except if input is empty too
			if (string.IsNullOrEmpty(input) && string.IsNullOrEmpty(text)) return true;
			if (!string.IsNullOrEmpty(input) && string.IsNullOrEmpty(text)) return false;
			var tokens = text.Split(new[] { '*' }, StringSplitOptions.None);
			int pos = 0;
			var first = true;
			foreach (var token in tokens)
			{
				if (pos >= input.Length && token.Length > 0) return false;
				var found = input.IndexOf(token, pos, caseComparison);
				if (found < 0 || first && found > 0) return false;
				pos = found + token.Length;
				first = false;
			}
			return pos == input.Length || tokens.Last().Length == 0;
		}

		public string[] GetGroupNames()
		{
			if (regex != null)
				return regex.GetGroupNames();
			if (strings != null)
				return new string[0];
			throw new ArgumentException("Nor regex nor strings");
		}

		public MatchEx Match(string input)
		{
			if (regex != null)
				return new MatchEx(regex.Match(input));
			if (strings != null)
			{
				foreach (var str in strings)
				{
					if (input.IndexOf(str, caseComparison) >= 0)
						return new MatchEx(str);
				}

				return new MatchEx((string)null);
			}
			throw new ArgumentException("Nor regex nor strings");
		}

		public class MatchEx
		{
			private readonly Match match;
			private readonly string str;

			public MatchEx(string str)
			{
				this.str = str;
			}

			internal MatchEx(Match match)
			{
				this.match = match;
			}

			public bool Success => match?.Success ?? str != null;

			public GroupCollectionEx Groups
			{
				get { return match != null ? new GroupCollectionEx(match.Groups) : new GroupCollectionEx(str); }
			}
		}

		public class GroupCollectionEx
		{
			private GroupCollection groups;
			private string str;

			public GroupCollectionEx(string str)
			{
				this.str = str;
			}

			public GroupCollectionEx(GroupCollection groups)
			{
				this.groups = groups;
			}

			public GroupEx this[int groupnum] { get { return groups != null ? new GroupEx(groups[groupnum]) : GroupEx.Unsuccesful; } }

			public GroupEx this[string groupname] { get { return groups != null ? new GroupEx(groups[groupname]) : GroupEx.Unsuccesful; } }
		}

		public class GroupEx
		{
			private Group group;

			public GroupEx()
			{
			}

			public GroupEx(Group group)
			{
				this.group = group;
			}

			public static GroupEx Unsuccesful => new GroupEx();
			public bool Success => group?.Success ?? false;
			public string Value => group?.Value;
		}
	}
}
