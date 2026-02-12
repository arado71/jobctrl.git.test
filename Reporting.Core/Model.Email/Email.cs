using System;
using System.Collections.Generic;

namespace Reporter.Model.Email
{
	internal class Email
	{
		public TimeSpan Span { get; set; }
		public string From { get; set; }
		public string[] To { get; set; }
		public int User { get; set; }

		public IEnumerable<string> GetAddresses()
		{
			yield return From;
			foreach (var to in To)
			{
				yield return to;
			}
		}
	}
}
