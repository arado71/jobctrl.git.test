using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.Capturing.Desktop.Windows.Url;
using Xunit;

namespace Tct.Tests.ActivityRecorderClient
{
	public class ChromeUrlResolverTests
	{
		[Fact]
		public void GetFixedUrls()
		{
			var tests = new[] { 
				new {Input = "asd", Expected = "http://asd" },
				new {Input = "localhost/?q=http://asd", Expected = "http://localhost/?q=http://asd" },
				new {Input = "https://asd", Expected = "https://asd" },
				new {Input = "file://a:/floppy.txt", Expected = "file://a:/floppy.txt" },
				new {Input = (string)null, Expected = (string)null },
				new {Input = "", Expected = "" },
				new {Input = "chrome://config", Expected = "chrome://config" },
				new {Input = "sad asd", Expected = ChromeUrlResolver.GoogleSearch + "sad+asd" }, //this is a lie but we cannot do better from v29 atm.
				new {Input = "sad:// asd", Expected = ChromeUrlResolver.GoogleSearch + "sad://+asd" }, //this is a lie but we cannot do better from v29 atm.
			};

			foreach (var test in tests)
			{
				Assert.Equal(test.Expected, ChromeUrlResolver.GetFixedUrl(test.Input));
			}
		}
	}
}
