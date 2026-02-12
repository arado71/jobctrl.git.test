using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Xunit;

namespace Tct.Tests.ActivityRecorderClient
{
	public class DictionaryTests
	{
		public DictionaryTests()
		{
			//jit
			var dict = new Dictionary<int, string>();
			dict.Add(1, "asd");
			dict[1] = "asd2";
			dict.Remove(1);
		}

		[Fact]
		public void IndexInsteadOfRemoveAddOverwrite()
		{
			//Arrange
			var dict = new Dictionary<int, string>();
			dict.Add(1, "asd");

			//Act
			dict[1] = "asd2";

			//Assert
			Assert.Equal("asd2", dict[1]);
		}

		[Fact]
		public void IndexInsteadOfRemoveAddNewValue()
		{
			//Arrange
			var dict = new Dictionary<int, string>();

			//Act
			dict[1] = "asd2";

			//Assert
			Assert.Equal("asd2", dict[1]);
		}

		[Fact]
		public void RemoveAddOverwrite()
		{
			//Arrange
			var dict = new Dictionary<int, string>();
			dict.Add(1, "asd");

			//Act
			dict.Remove(1);
			dict.Add(1, "asd2");

			//Assert
			Assert.Equal("asd2", dict[1]);
		}

		[Fact]
		public void RemoveAddNewValue()
		{
			//Arrange
			var dict = new Dictionary<int, string>();

			//Act
			dict.Remove(1);
			dict.Add(1, "asd2");

			//Assert
			Assert.Equal("asd2", dict[1]);
		}

		[Fact(Skip = "Needs constant throughput which is not available on the build server")]
		public void IndexIsFasterThanRemoveAddNewValue()
		{
			//Arrange
			const int iter = 1000000;
			var dict = new Dictionary<int, string>(iter);
			var dict2 = new Dictionary<int, string>(iter);
			dict[-1] = "asd";
			dict2.Remove(-1);
			dict2.Add(-1, "asd");

			//Act
			var sw = Stopwatch.StartNew();
			for (int i = 0; i < iter; i++)
			{
				dict[i] = "asd";
			}
			sw.Stop();


			var sw2 = Stopwatch.StartNew();
			for (int i = 0; i < iter; i++)
			{
				dict2.Remove(i);
				dict2.Add(i, "asd");
			}
			sw2.Stop();

			//Assert
			Console.WriteLine(sw.Elapsed.TotalMilliseconds + " < " + sw2.Elapsed.TotalMilliseconds);
			Assert.True(sw.Elapsed < sw2.Elapsed);
		}

		[Fact(Skip = "Needs constant throughput which is not available on the build server")]
		public void IndexIsFasterThanRemoveAddOverwrite()
		{
			//Arrange
			const int iter = 1000000;
			var dict = new Dictionary<int, string>();
			var dict2 = new Dictionary<int, string>();
			for (int i = 0; i < iter; i++)
			{
				dict.Add(i, "asd");
				dict2.Add(i, "asd");
			}

			//Act
			var sw = Stopwatch.StartNew();
			for (int i = 0; i < iter; i++)
			{
				dict[i] = "asd";
			}
			sw.Stop();


			var sw2 = Stopwatch.StartNew();
			for (int i = 0; i < iter; i++)
			{
				dict2.Remove(i);
				dict2.Add(i, "asd");
			}
			sw2.Stop();

			//Assert
			Assert.True(sw.Elapsed < sw2.Elapsed);
			Console.WriteLine(sw.Elapsed.TotalMilliseconds + " < " + sw2.Elapsed.TotalMilliseconds);
		}

		//ok this shouldn't be here...
		[Fact]
		public void StringBuilderReplaceReference()
		{
			var e = "3" + "4";
			Assert.False(Object.ReferenceEquals(e, new StringBuilder(e).ToString()));
			Assert.False(Object.ReferenceEquals(e, new StringBuilder(e).Replace("a", "d").ToString()));
			Assert.True(Object.ReferenceEquals(e, e.Replace("a", "d")));
		}
	}
}
