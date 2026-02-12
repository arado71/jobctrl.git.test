using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Office.Interop.Outlook;

namespace OutlookInteropService
{
	public static class MailItemExtensions
	{
		// http://www.lessanvaezi.com/email-headers-from-outlook-mailitem/

		private const string HeaderRegex =
			@"^(?<header_key>[-A-Za-z0-9]+)(?<seperator>:[ \t]*)" +
				"(?<header_value>([^\r\n]|\r\n[ \t]+)*)(?<terminator>\r\n)";
		private const string TransportMessageHeadersSchema =
			"http://schemas.microsoft.com/mapi/proptag/0x007D001E";

		public static string[] Headers(this MailItem mailItem, string name, string safeHeaders = null)
		{
			var headers = mailItem.HeaderLookup(safeHeaders);
			if (headers.Contains(name))
				return headers[name].ToArray();
			return new string[0];
		}

		public static ILookup<string, string> HeaderLookup(this MailItem mailItem, string safeHeaders = null)
		{
			var headerString = safeHeaders ?? mailItem.HeaderString();
			var headerMatches = Regex.Matches
				(headerString, HeaderRegex, RegexOptions.Multiline).Cast<Match>();
			return headerMatches.ToLookup(
				h => h.Groups["header_key"].Value,
				h => h.Groups["header_value"].Value,
				StringComparer.OrdinalIgnoreCase);
		}

		public static string HeaderString(this MailItem mailItem)
		{
			return (string)mailItem.PropertyAccessor
				.GetProperty(TransportMessageHeadersSchema);
		}

		private static bool DateIsValid(DateTime date)
		{
			return date > DateTime.MinValue && date.Year < 4500;
		}

		public static bool IsDeferredDeliveryItem(this MailItem mailItem)
		{
			return !mailItem.Sent && DateIsValid(mailItem.DeferredDeliveryTime) && DateIsValid(mailItem.SentOn);
		}

	}
}
