using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace VoxCTRL.Controller
{
	public static class RecordingNameParser
	{
		private static readonly Regex substitutionMacher = new Regex(@"\$\(?(?<name>[\p{L}]+)\)?"); 
		public static string Parse(string input, IDictionary<string, string> mapping = null)
		{
			Match match;
			while ((match = substitutionMacher.Match(input)).Success)
			{
				var name = match.Groups["name"].ToString();
				input = input.Substring(0, match.Index) + GetEnv(name, mapping) + input.Substring(match.Index + match.Length);
			}
			return input;
		}

		private static string GetEnv(string name, IDictionary<string, string> mapping)
		{
			if (mapping != null && mapping.TryGetValue(name, out string value)) return value;
			return Environment.GetEnvironmentVariable(name);
		}
	}
}
