using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace PlaybackClient.Tests
{
	public class ProgramParametersTests
	{
		[Fact]
		public void ExactMapping()
		{
			var arg = "234=123";
			Func<int, int> mappingFunc;
			Assert.True(Program.TryParseMapping(arg, out mappingFunc));
			Assert.NotNull(mappingFunc);
			Assert.Equal(123, mappingFunc(234));
			Assert.Equal(23, mappingFunc(23));
		}

		[Fact]
		public void ExactAndAvailMapping()
		{
			var arg = "234=123,2";
			Func<int, int> mappingFunc;
			Assert.True(Program.TryParseMapping(arg, out mappingFunc));
			Assert.NotNull(mappingFunc);
			Assert.Equal(123, mappingFunc(234));
			Assert.Equal(2, mappingFunc(23));
		}

		[Fact]
		public void ExactAndAvailMappingComplex()
		{
			var arg = "234=123,2,1=2,4,5";
			Func<int, int> mappingFunc;
			Assert.True(Program.TryParseMapping(arg, out mappingFunc));
			Assert.NotNull(mappingFunc);
			Assert.Equal(123, mappingFunc(234));
			Assert.Equal(2, mappingFunc(1));
			var f23 = mappingFunc(23);
			Assert.True(new[] { 2, 4, 5 }.Contains(f23));
			Assert.Equal(f23, mappingFunc(23));
		}

		[Fact]
		public void AvailMapping()
		{
			var arg = "2,3,4";
			Func<int, int> mappingFunc;
			Assert.True(Program.TryParseMapping(arg, out mappingFunc));
			Assert.NotNull(mappingFunc);
			var f1 = mappingFunc(10);
			var f2 = mappingFunc(11);
			var f3 = mappingFunc(12);

			Assert.Equal(arg, string.Join(",", new[] { f1, f2, f3 }.OrderBy(n => n)));

			Assert.Equal(f1, mappingFunc(10));
			Assert.Equal(f2, mappingFunc(11));
			Assert.Equal(f3, mappingFunc(12));
		}

		[Fact]
		public void AvailMappingSameUsed()
		{
			var arg = "2,3,4,5,123,353245";
			Func<int, int> mappingFunc;
			Assert.True(Program.TryParseMapping(arg, out mappingFunc));
			Assert.NotNull(mappingFunc);
			var f1 = mappingFunc(10);
			var f2 = mappingFunc(11);
			var f3 = mappingFunc(12);
			Assert.Equal(f1, mappingFunc(10));
			Assert.Equal(f2, mappingFunc(11));
			Assert.Equal(f3, mappingFunc(12));
		}
	}
}
