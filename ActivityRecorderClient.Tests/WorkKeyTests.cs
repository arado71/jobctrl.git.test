using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.Capturing.Core;
using Xunit;

namespace Tct.Tests.ActivityRecorderClient
{
	public class WorkKeyTests
	{
		[Fact]
		public void Empty()
		{
			ValidateKeysImpl("", "", 50, false, "");
		}

		[Fact]
		public void EmptySuffix()
		{
			ValidateKeysImpl("", "a", 50, false, "");
		}

		[Fact]
		public void EmptyNullSuffix()
		{
			ValidateKeysImpl("", null, 50, false, "");
		}

		[Fact]
		public void Space()
		{
			ValidateKeysImpl(" ", "", 50, false, " ");
		}

		[Fact]
		public void SpaceSuffix()
		{
			ValidateKeysImpl(" ", "a", 50, false, " ");
		}

		[Fact]
		public void Simple()
		{
			ValidateKeysImpl("a", "", 50, true, "a");
		}

		[Fact]
		public void SimpleNullSuffix()
		{
			ValidateKeysImpl("a", null, 50, true, "a");
		}

		[Fact]
		public void SimpleSuffix()
		{
			ValidateKeysImpl("a", "b", 50, true, "ab");
		}

		[Fact]
		public void SimpleTrim()
		{
			ValidateKeysImpl(" a", "", 50, true, "a");
			ValidateKeysImpl("a ", "", 50, true, "a");
		}

		[Fact]
		public void SimpleTrimSuffix()
		{
			ValidateKeysImpl(" a", "b", 50, true, "ab");
			ValidateKeysImpl("a ", "b", 50, true, "ab");
		}

		[Fact]
		public void SimpleTrimSuffixNoTrim()
		{
			ValidateKeysImpl(" a", " b", 50, true, "a b");
			ValidateKeysImpl(" a", "b ", 50, true, "ab ");
			ValidateKeysImpl("a ", " b", 50, true, "a b");
			ValidateKeysImpl("a ", "b ", 50, true, "ab ");
		}

		[Fact]
		public void WithSpace()
		{
			ValidateKeysImpl("a b", "", 50, true, "a b");
		}

		[Fact]
		public void WithSpaceTrim()
		{
			ValidateKeysImpl(" a b", "", 50, true, "a b");
			ValidateKeysImpl("a b ", "", 50, true, "a b");
		}

		[Fact]
		public void WithSpaceTrimSuffixNoTrim()
		{
			ValidateKeysImpl(" a b", " c d  ", 50, true, "a b c d  ");
			ValidateKeysImpl("a b ", "    c d", 50, true, "a b    c d");
		}

		[Fact]
		public void TruncateLong()
		{
			ValidateKeysImpl("abc", "d", 3, true, "abd");
		}

		[Fact]
		public void TruncateLongTrimSuffixNoTrim()
		{
			ValidateKeysImpl("\t abc \t", " d\t", 4, true, "a d\t");
		}

		[Fact]
		public void TruncateTooLong()
		{
			ValidateKeysImpl("a", "abc", 3, false, "a");
		}

		[Fact]
		public void TruncateTooLong2()
		{
			ValidateKeysImpl("a", "abcd", 3, false, "a");
		}

		public void ValidateKeysImpl(string key, string suffix, int maxLen, bool expectedResult, string expectedKey)
		{
			var result = WorkDetector.TryCombineKeys(ref key, suffix, maxLen);
			Assert.Equal(expectedResult, result);
			Assert.Equal(expectedKey, key);
		}
	}
}
