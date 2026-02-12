using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderService.EmailStats;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class UrlFormatTests
	{
		[Fact]
		public void SimpleRawTest()
		{
			//var uri = new Uri("http://google.com/asdasd/asd?23432=35&456=45");
			//var uri = new Uri("about:blank");
			//var uri = new Uri("file://d:/temp/asd.txt");
			var uri = new Uri("https://mail.google.com/mail/?shva=1&ert=e4#label/TcT/13282327b68fb8e3");
			Console.WriteLine(string.Join(Environment.NewLine,
				new[] { uri.Scheme, uri.Host, uri.Port.ToString() }
				.Concat(uri.Segments)
				.Concat(new[] { uri.Query, uri.Fragment })));
		}

		[Fact]
		public void GetShortUrlFromWontThrow()
		{
			var urlsThatThrows = new[] { "", "http://", "http://asd asd", "ieframe.dll" };
			foreach (var urlThatThrowsLoop in urlsThatThrows)
			{
				var urlThatThrows = urlThatThrowsLoop;
				Assert.Throws<UriFormatException>(() => new Uri(urlThatThrows));
				Assert.DoesNotThrow(() => UrlFormatHelper.GetShortUrlFrom(urlThatThrows));
			}
		}

		[Fact]
		public void SeveralFormats()
		{
			var tests = new[] { 
				new { Input = "http://google.com/asdasd/asd?23432=35&456=45", Expected = "google.com" },
				new { Input = "https://google.com/asdasd/asd?23432=35&456=45", Expected = "google.com" },
				new { Input = "about:blank", Expected = "about:blank" },
				new { Input = "https://mail.google.com/mail/?shva=1&ert=e4#label/TcT/13282327b68fb8e3", Expected = "mail.google.com" },
				new { Input = "http://msdn.microsoft.com/en-us/library/ms186981.aspx", Expected = "msdn.microsoft.com" },
				new { Input = "http://msdn.microsoft.com/en-us/library/ms143432.aspx", Expected = "msdn.microsoft.com" },
				new { Input = "file://d:/temp/asd.txt", Expected = "file" },
				new { Input = "http://", Expected = "" },
				new { Input = "http://asd asd", Expected = "" },
				new { Input = "ieframe.dll", Expected = "ieframe.dll" },
				new { Input = "", Expected = "" },
			};

			foreach (var test in tests)
			{
				Assert.Equal(test.Expected, UrlFormatHelper.GetShortUrlFrom(test.Input));
			}
		}

		[Fact]
		public void DbFriendlyTests()
		{
			var tests = new[] { 
				new { Input = "http://google.com/#adsfdsaf", Expected = "http://google.com/" },
				new { Input = "http://google.com/asdasd/asd?23432=35&456=45", Expected = "http://google.com/asdasd/asd" },
				new { Input = "https://google.com/asdasd/asd?23432=35&456=45", Expected = "https://google.com/asdasd/asd" },
				new { Input = "about:blank", Expected = "about:blank" },
				new { Input = "https://mail.google.com/mail/?shva=1&ert=e4#label/TcT/13282327b68fb8e3", Expected = "https://mail.google.com/mail/" },
				new { Input = "http://msdn.microsoft.com/en-us/library/ms186981.aspx", Expected = "http://msdn.microsoft.com/en-us/library/ms186981.aspx" },
				new { Input = "http://msdn.microsoft.com/en-us/library/ms143432.aspx", Expected = "http://msdn.microsoft.com/en-us/library/ms143432.aspx" },
				new { Input = "file://d:/temp/asd.txt", Expected = "file://d:/temp/asd.txt" },
				new { Input = "http://", Expected = "" },
				new { Input = "http://asd asd", Expected = "" },
				new { Input = "ieframe.dll", Expected = "ieframe.dll" },
				new { Input = "", Expected = "" },
			};

			foreach (var test in tests)
			{
				Assert.Equal(test.Expected, UrlFormatHelper.GetDbFriendlyUrl(test.Input));
			}
		}
	}
}
