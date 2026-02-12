using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Tct.ActivityRecorderClient.Rules
{
	public static class PlaceholderHelper
	{
		private static readonly Regex keyValidator = new Regex(@"^\w+$", RegexOptions.Singleline | RegexOptions.Compiled);
		private static readonly Regex placeholderMatcher = new Regex(@"\$(?<key>\w+)\$", RegexOptions.Singleline | RegexOptions.Compiled);

		/// <summary>
		/// Gets a placehoder which can be insereted into a string.
		/// </summary>
		/// <param name="key">Name of the placeholder.</param>
		/// <returns>A placeholder which can be detected in a text.</returns>
		public static string GetPlaceholder(string key)
		{
			if (!IsKeyValid(key)) throw new ArgumentException();
			return "$" + key + "$";
		}

		/// <summary>
		/// Checks if a key can be converted into a valid placeholder.
		/// </summary>
		/// <param name="key">Key which you want to use.</param>
		/// <returns>True if it is valid.</returns>
		public static bool IsKeyValid(string key)
		{
			return key != null && keyValidator.IsMatch(key);
		}

		/// <summary>
		/// Replaces all palceholders in a string.
		/// </summary>
		/// <param name="textWithPlaceholders">Input text with placeholders in it.</param>
		/// <param name="replaceFunction">Repalce function which will be called for every palceholder.</param>
		/// <returns>Output text with the replaced values.</returns>
		public static string ReplacePlaceholders(string textWithPlaceholders, Func<string, string> replaceFunction)
		{
			if (replaceFunction == null) throw new ArgumentNullException();
			if (textWithPlaceholders == null) return null;
			var result = placeholderMatcher.Replace(textWithPlaceholders, match =>
				{
					Debug.Assert(match.Success);
					if (match.Success)
					{
						return replaceFunction(match.Groups["key"].Value);
					}
					else
					{
						return match.ToString();
					}
				});
			return result;
		}

		public delegate bool TryReplaceFunc(string key, out string value);

		/// <summary>
		/// Tries to replace all palceholders in a string. Partial replaces are returned when not all palceholders are matched.
		/// </summary>
		/// <param name="textWithPlaceholders">Input text with placeholders in it.</param>
		/// <param name="tryReplaceFunction">Repalce function which will be called for every palceholder.</param>
		/// <param name="result">Output text with the replaced values.</param>
		/// <returns>True if all placeholders are matched in the input text otherwise false.</returns>
		public static bool TryReplacePlaceholders(string textWithPlaceholders, TryReplaceFunc tryReplaceFunction, out string result)
		{
			int success, failed;
			result = ReplacePlaceholdersWithTryReplaceFunc(textWithPlaceholders, tryReplaceFunction, out success, out failed);
			return (success > 0 && failed == 0);
		}

		/// <summary>
		/// Tries to replace all palceholders in a string. Partial replaces are returned when not all palceholders are matched.
		/// </summary>
		/// <remarks>
		/// This version can be used more easily in a clear way
		/// </remarks>
		/// <param name="textWithPlaceholders">Input text with placeholders in it.</param>
		/// <param name="tryReplaceFunction">Repalce function which will be called for every palceholder.</param>
		/// <param name="replaceSuccess">Number of successful replaces</param>
		/// <param name="replaceFailed">Number of failed replaces</param>
		/// <returns>Output text with the replaced values.</returns>
		public static string ReplacePlaceholdersWithTryReplaceFunc(string textWithPlaceholders, TryReplaceFunc tryReplaceFunction, out int replaceSuccess, out int replaceFailed)
		{
			if (textWithPlaceholders == null || tryReplaceFunction == null)
			{
				replaceSuccess = 0;
				replaceFailed = 0;
				return textWithPlaceholders;
			}
			int tempSuccess = 0;
			int tempFailed = 0;
			var tempRes = placeholderMatcher.Replace(textWithPlaceholders, match =>
			{
				Debug.Assert(match.Success);
				if (match.Success)
				{
					string replaceResult;
					var ok = tryReplaceFunction(match.Groups["key"].Value, out replaceResult);
					if (ok)
					{
						tempSuccess++;
						return replaceResult;
					}
					else
					{
						tempFailed++;
						return match.ToString(); //don't replace the placeholder on error
					}
				}
				else
				{
					return match.ToString();
				}
			});
			replaceSuccess = tempSuccess;
			replaceFailed = tempFailed;
			return tempRes; //return even partial replaces
		}
	}
}
