using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;
using Tct.ActivityRecorderService;

namespace Tct.Tests.ActivityRecorderService
{
	public class StringStripTests
	{
		[Fact]
		public void IsInvalid16()
		{
			Assert.True("\x16".HasInvalidXmlChars());
			Assert.False("sdiopgjsdofijgsodhfgohsdfioghidofhg".HasInvalidXmlChars());
		}

		[Fact]
		public void MatchInvalid1F()
		{
			var r = new Regex("\u001f");
			var str = "\x1f";
			Assert.True(r.IsMatch(str));
			Assert.Equal("", r.Replace(str, ""));
		}

		[Fact]
		public void StripInvalid1F()
		{
			var str = "\x1f";
			Assert.Equal("", str.ReplaceInvalidXmlChars(""));
		}

		[Fact]
		public void StripInvalidVt()
		{
			var str = "\x0B";
			Assert.Equal("", str.ReplaceInvalidXmlChars(""));
		}

		[Fact]
		public void SampleTextNotStripped()
		{
			var str = "qwertyuiopasdfghjklzxcvbnm1234567890öüóőúéáűí\u00a9";
			Assert.Equal(str, str.ReplaceInvalidXmlChars(""));
			Assert.Equal(str.ToUpperInvariant(), str.ToUpperInvariant().ReplaceInvalidXmlChars(""));
		}

		[Fact]
		public void SampleTextNotStrippedWithNewLine()
		{
			var str = "qwertyuiopasdfghjklzxcvbnm1234567890öüóőúéáűí\u00a9" + Environment.NewLine + "qwertyuiopasdfghjklzxcvbnm1234567890öüóőúéáűí\u00a9";
			Assert.Equal(str, str.ReplaceInvalidXmlChars(""));
			Assert.Equal(str.ToUpperInvariant(), str.ToUpperInvariant().ReplaceInvalidXmlChars(""));
		}

		[Fact]
		public void SampleKoreanNotStripped()
		{
			var str = "漢字はユニコード";
			Assert.Equal(str, str.ReplaceInvalidXmlChars(""));
			Assert.Equal(str.ToUpperInvariant(), str.ToUpperInvariant().ReplaceInvalidXmlChars(""));
		}

		[Fact]
		public void Performance()
		{
			var str = "qwertyuiopasdfghjklzxcvbnm1234567890öüóőúéáűí\u00a9"
				+ "qwertyuiopasdfghjklzxcvbnm1234567890öüóőúéáűí\u00a9"
				+ "qwertyuiopasdfghjklzxcvbnm1234567890öüóőúéáűí\u00a9"
				+ "qwertyuiopasdfghjklzxcvbnm1234567890öüóőúéáűí\u00a9"
				+ "qwertyuiopasdfghjklzxcvbnm1234567890öüóőúéáűí\u00a9"
				+ "qwertyuiopasdfghjklzxcvbnm1234567890öüóőúéáűí\u00a9"
				+ "qwertyuiopasdfghjklzxcvbnm1234567890öüóőúéáűí\u00a9"
				+ "qwertyuiopasdfghjklzxcvbnm1234567890öüóőúéáűí\u00a9"
				+ "qwertyuiopasdfghjklzxcvbnm1234567890öüóőúéáűí\u00a9"
				+ "qwertyuiopasdfghjklzxcvbnm1234567890öüóőúéáűí\u00a9"
				+ "qwertyuiopasdfghjklzxcvbnm1234567890öüóőúéáűí\u00a9"
				+ "qwertyuiopasdfghjklzxcvbnm1234567890öüóőúéáűí\u00a9"
				+ "qwertyuiopasdfghjklzxcvbnm1234567890öüóőúéáűí\u00a9"
				+ "qwertyuiopasdfghjklzxcvbnm1234567890öüóőúéáűí\u00a9"
				+ "qwertyuiopasdfghjklzxcvbnm1234567890öüóőúéáűí\u00a9"
				+ "qwertyuiopasdfghjklzxcvbnm1234567890öüóőúéáűí\u00a9"
				;
			var temp = str.ReplaceInvalidXmlChars("");
			var start = Environment.TickCount;
			for (int i = 0; i < 1000; i++)
			{
				temp = str.ReplaceInvalidXmlChars("");
			}
			Assert.True(Object.ReferenceEquals(temp, str.ReplaceInvalidXmlChars("")));
			Console.WriteLine(Environment.TickCount - start);
		}
	}
}
