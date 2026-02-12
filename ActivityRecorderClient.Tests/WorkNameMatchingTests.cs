using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.Menu;
using Tct.ActivityRecorderClient.Search;
using Xunit;

namespace Tct.Tests.ActivityRecorderClient
{
	public class WorkNameMatchingTests
	{
		private static readonly string sep = WorkDataWithParentNames.DefaultSeparator;

		[Fact]
		public void SimpleMatch()
		{
			//Arrange
			var matcher = new WorkNameMatcher();
			for (char c = 'a'; c <= 'z'; c = (char)(((int)c) + 1))
			{
				matcher.Add(c - 'a', c.ToString());
			}

			//Act
			var result = matcher.GetMatches("k").ToArray();

			//Assert
			Assert.True(result.SequenceEqual(new int[] { 'k' - 'a' }));
		}

		[Fact]
		public void FourCharMatch()
		{
			//Arrange
			var matcher = new WorkNameMatcher();
			for (char c = 'a'; c <= 'z'; c = (char)(((int)c) + 1))
			{
				matcher.Add(c - 'a', new string(c, 4));
			}

			//Act
			var result = matcher.GetMatches("k").ToArray();

			//Assert
			Assert.True(result.SequenceEqual(new int[] { 'k' - 'a' }));
		}

		[Fact]
		public void FourDiffCharMatch()
		{
			//Arrange
			var matcher = new WorkNameMatcher();
			for (int i = 'a'; i <= 'z' - 4; i++)
			{
				matcher.Add(i - 'a', "" + new string(new[] { (char)i, (char)(i + 1), (char)(i + 2), (char)(i + 3) }));
			}

			//Act
			var result = matcher.GetMatches("k").ToArray();

			//Assert
			Assert.True(result.SequenceEqual(new int[] { 'k' - 'a', 'k' - 'a' - 3, 'k' - 'a' - 2, 'k' - 'a' - 1 }));
		}

		[Fact]
		public void FourDiffTwoCharMatch()
		{
			//Arrange
			var matcher = new WorkNameMatcher();
			for (int i = 'a'; i <= 'z' - 4; i++)
			{
				matcher.Add(i - 'a', "" + new string(new[] { (char)i, (char)(i + 1), (char)(i + 2), (char)(i + 3) }));
			}

			//Act
			var result = matcher.GetMatches("kl").ToArray();

			//Assert
			Assert.True(result.SequenceEqual(new int[] { 'k' - 'a', 'k' - 'a' - 2, 'k' - 'a' - 1 }));
		}

		[Fact]
		public void FourDiffTwoSepCharMatch()
		{
			//Arrange
			var matcher = new WorkNameMatcher();
			for (int i = 'a'; i <= 'z' - 4; i++)
			{
				matcher.Add(i - 'a', "" + new string(new[] { (char)i, (char)(i + 1), (char)(i + 2), (char)(i + 3) }));
			}

			//Act
			var result = matcher.GetMatches("k m").ToArray();

			//Assert
			Assert.True(result.SequenceEqual(new int[] { 'k' - 'a', 'k' - 'a' - 1 }));
		}

		[Fact]
		public void FirstCharIsBetterThanPosition()
		{
			//Arrange
			var search = "a";
			var data = new[] { 
				"abc" + sep + "bcd",
				"bcd" + sep + "bac",
			};
			var matcher = new WorkNameMatcher();
			int i = 0;
			foreach (var datum in data.Reverse())
			{
				matcher.Add(i++, datum);

			}

			//Act
			var result = matcher.GetMatches(search).ToArray();

			//Assert
			Assert.True(result.SequenceEqual(Enumerable.Range(0, data.Length).Select(n => data.Length - 1 - n)));
		}

		[Fact(Skip = "this is not supported atm.")]
		public void FirstCharIsBetterThanPositionButPositionStillUsedAsTieBreaker()
		{
			//Arrange
			var search = "a";
			var data = new[] { 
				"abc" + sep + "bad",
				"abc" + sep + "bcd",
			};
			var matcher = new WorkNameMatcher();
			int i = 0;
			foreach (var datum in data.Reverse())
			{
				matcher.Add(i++, datum);

			}

			//Act
			var result = matcher.GetMatches(search).ToArray();

			//Assert
			Assert.True(result.SequenceEqual(Enumerable.Range(0, data.Length).Select(n => data.Length - 1 - n)));
		}

		[Fact]
		public void LastPositionIsBetterMiddleMatch()
		{
			//Arrange
			var search = "a";
			var data = new[] { 
				"bcd" + sep + "bac",
				"bac" + sep + "bcd",
			};
			var matcher = new WorkNameMatcher();
			int i = 0;
			foreach (var datum in data.Reverse())
			{
				matcher.Add(i++, datum);

			}

			//Act
			var result = matcher.GetMatches(search).ToArray();

			//Assert
			Assert.True(result.SequenceEqual(Enumerable.Range(0, data.Length).Select(n => data.Length - 1 - n)));
		}

		[Fact]
		public void LastPositionIsBetterFirstCharMatch()
		{
			//Arrange
			var search = "a";
			var data = new[] { 
				"bcd" + sep + "ac",
				"ac" + sep + "bcd",
			};
			var matcher = new WorkNameMatcher();
			int i = 0;
			foreach (var datum in data.Reverse())
			{
				matcher.Add(i++, datum);

			}

			//Act
			var result = matcher.GetMatches(search).ToArray();

			//Assert
			Assert.True(result.SequenceEqual(Enumerable.Range(0, data.Length).Select(n => data.Length - 1 - n)));
		}

		[Fact]
		public void LastPositionIsBetterBothMatchFirstCharCounts()
		{
			//Arrange
			var search = "a";
			var data = new[] { 
				"bacd" + sep + "ac",
				"ac" + sep + "bacd",
			};
			var matcher = new WorkNameMatcher();
			int i = 0;
			foreach (var datum in data.Reverse())
			{
				matcher.Add(i++, datum);

			}

			//Act
			var result = matcher.GetMatches(search).ToArray();

			//Assert
			Assert.True(result.SequenceEqual(Enumerable.Range(0, data.Length).Select(n => data.Length - 1 - n)));
		}
	}
}
