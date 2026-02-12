using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderService.OnlineStats;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class MobileHelperTests
	{
		[Fact]
		public void Overflow()
		{
			Assert.Throws<OverflowException>(() => -1 * Math.Abs(int.MinValue));
		}

		[Fact]
		public void NoOverflow()
		{
			Assert.DoesNotThrow(() => { checked { long e = -1 * Math.Abs((long)int.MinValue); } });
		}

		[Fact]
		public void OverflowInt()
		{
			Assert.Throws<OverflowException>(() => { checked { int e = -1 * (int)Math.Abs((long)int.MinValue); } });
		}

		[Fact]
		public void NoOverflowInt()
		{
			Assert.DoesNotThrow(() => { checked { int e = (int)(-1 * Math.Abs((long)int.MinValue)); } });
		}

		[Fact]
		public void CanParseLongs()
		{
			var tests = new[] { long.MaxValue, long.MinValue, 0, long.MaxValue - 234, long.MinValue + 25 };
			foreach (var test in tests)
			{
				Assert.Equal(test, MobileHelper.GetMobileId(test.ToString()));
			}
		}

		[Fact]
		public void StringsDontThrow()
		{
			var tests = new[] { "", "asdsa", "435-43", "dsfdsfdw3r32rwefds" };
			foreach (var test in tests)
			{
				Assert.DoesNotThrow(() => MobileHelper.GetMobileId(test));
			}
		}

		[Fact]
		public void StringsNotSame()
		{
			var tests = new[] { "", "asdsa", "435-43", "dsfdsfdw3r32rwefds" };
			Assert.Equal(4, tests.Select(n => MobileHelper.GetMobileId(n)).Distinct().Count());
		}

		[Fact]
		public void NullDontThrow()
		{
			Assert.DoesNotThrow(() => MobileHelper.GetMobileId(null));
		}

		[Fact]
		public void DoubleOrDecimal() //SqlGeometry uses double too for lat and lon
		{
			decimal origM = 1.00000000000001m;
			double origD = 1.00000000000001;
			decimal convM = (decimal)origD;
			Assert.Equal(origD.ToString(), convM.ToString());
			Assert.Equal(origD.ToString(), origM.ToString());
			Assert.Equal("1.00000000000001", origM.ToString(CultureInfo.InvariantCulture));
		}

		[Fact]
		public void DoubleOrDecimalDiff()
		{
			decimal origM = 1.000000000000001m;
			double origD = 1.000000000000001;
			decimal convM = (decimal)origD;
			Assert.Equal(origD.ToString(), convM.ToString());
			Assert.NotEqual(origD.ToString(), origM.ToString());
			Assert.Equal("1", origD.ToString(CultureInfo.InvariantCulture));
			Assert.Equal("1.000000000000001", origM.ToString(CultureInfo.InvariantCulture));
		}
	}
}
