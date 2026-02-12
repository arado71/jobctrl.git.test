using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class IntDiffOverflowTests
	{
		private readonly int[] interestingInts = new[] { int.MinValue, int.MinValue + 1,  int.MinValue / 2 - 1,int.MinValue / 2,int.MinValue / 2 + 1,
			-1, 0, 1, int.MaxValue / 2 - 1, int.MaxValue / 2, int.MaxValue / 2 + 1, int.MaxValue - 1, int.MaxValue};

		private int GetDiffWrong(int start, int end)
		{
			return end - start;
		}

		private uint GetDiff(int start, int end)
		{
			return (uint)(end - start);
		}

		private uint GetDiff2(int start, int end)
		{
			if (start <= end)
			{
				return (uint)end - (uint)start;
			}
			else
			{
				return UInt32.MaxValue - (uint)start + (uint)end + 1;
			}
		}

		[Fact]
		public void OnlineStatsBug()
		{
			Console.WriteLine(int.MinValue);
			Console.WriteLine(int.MaxValue);
			var a = int.MaxValue - 1363575147;
			var b = -535750085 - int.MinValue;
			Console.WriteLine(a);
			Console.WriteLine(b);
			//-1899325232
			Console.WriteLine(-535750085 - 1363575147);
			Console.WriteLine(a + b + 1);
			//2395642064
			Console.WriteLine(((uint)a + b + 1));
			Console.WriteLine((UInt32.MaxValue - 1363575147 - 535750085 + 1));

			Console.WriteLine(-535750085 == (1363575147 + a + b + 1));

			Console.WriteLine(GetDiffWrong(1363575147, -535750085));
			Console.WriteLine(GetDiff(1363575147, -535750085));
			//Console.WriteLine((1363575147 + 2395642064));
			Assert.True(GetDiff(1363575147, -535750085) > 4000);
			var start = 1363575147;
			var end = -535750085;
			Assert.False(end - start > 4000);
			Assert.True((uint)(end - start) > 4000);
		}

		[Fact]
		public void OverflowTests()
		{
			for (int i = 0; i < interestingInts.Length; i++)
			{
				for (int j = 0; j < interestingInts.Length; j++)
				{
					var start = interestingInts[i];
					var end = interestingInts[j];
					var diff = GetDiff(start, end);
					Assert.True(end == (int)(start + diff), end + " != " + (start + diff) + " (" + start + "+" + diff + ") start:" + start + " ,end:" + end);
				}
			}
		}

		[Fact]
		public void OverflowTestsDiffCast()
		{
			for (int i = 0; i < interestingInts.Length; i++)
			{
				for (int j = 0; j < interestingInts.Length; j++)
				{
					var start = interestingInts[i];
					var end = interestingInts[j];
					var diff = (int)GetDiff(start, end);
					Assert.True(end == start + diff, end + " != " + (start + diff) + " (" + start + "+" + diff + ") start:" + start + " ,end:" + end);
				}
			}
		}

		[Fact]
		public void OverflowTests2()
		{
			for (int i = 0; i < interestingInts.Length; i++)
			{
				for (int j = 0; j < interestingInts.Length; j++)
				{
					var start = interestingInts[i];
					var end = interestingInts[j];
					var diff = GetDiff2(start, end);
					Assert.True(end == (int)(start + diff), end + " != " + (start + diff) + " (" + start + "+" + diff + ") start:" + start + " ,end:" + end);
				}
			}
		}

		[Fact]
		public void ExpireImmediateTest()
		{
			int now = int.MaxValue;
			int exp = now + 2;
			bool isExpiredAllWrong = exp < now;
			bool isExpiredStillWrong = exp - now < 0;
			Assert.False(isExpiredStillWrong);
			Assert.True(isExpiredAllWrong);
		}

		[Fact]
		public void ExpireLongTimeTest()
		{
			int maxAge = 2;
			int now = 0;
			int exp = now + maxAge;
			now += int.MaxValue; //significant time passed
			now += 1000;
			bool isExpiredBetter = exp - now < 0 || exp - now > maxAge;
			bool isExpiredStillWrong = exp - now < 0;
			Assert.True(isExpiredBetter);
			Assert.False(isExpiredStillWrong);
		}
	}
}

/*
//http://www.interact-sw.co.uk/utilities/WaitFor/source/
using System;

/// <summary>
/// Provides a way of testing for whether a certain amount of time has
/// elapsed.
/// </summary>
/// <remarks>
/// <p>This is intended for use in loops which need to do some work until
/// a specific amount of time has elapsed. For example, it can be used
/// to test if a timeout has occurred yet. It can also be used in simple
/// benchmark tests to see how many operations can be performed in a
/// given amount of time.</p>
/// 
/// <p>Its usage is:</p>
/// <code>
/// WaitForTicks wait = new WaitForTicks(TimeSpan.FromSeconds(2));
/// while (!wait.TimeIsUp)
/// {
///     ... do work ...
/// }
/// </code>
/// 
/// <p>This uses the <see cref="Environment.TickCount"/>, which increases
/// monotonically, so it will be unperturbed by changes to the system
/// time. This class deals correctly with wrapping around, both when the
/// tick count rolls from positive to negative, and when it rolls around
/// through 0. However, because the tick count is a 32 bit number, this
/// class cannot wait for more than <see cref="Int32.MaxValue"/>
/// milliseconds.</p>
/// </remarks>
public class WaitForTicks
{
    private readonly int startTicks;
    private readonly uint maxTicks;

    /// <summary>
    /// Create a WaitForTicks object initialized to expire in the
    /// specified amount of time.
    /// </summary>
    /// <param name="time">The amount of time into the future in
    /// which this timer's <see cref="TimeIsUp"/> property will
    /// return true.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if
    /// the specified time is more than <see cref="Int32.MaxValue"/>
    /// millseconds in the future.</exception>
    public WaitForTicks(TimeSpan time)
    {
        double fullMaxTicks = time.TotalMilliseconds;
        if (fullMaxTicks > int.MaxValue)
        {
            // The int.MaxValue limit is a bit arbitrary. In practice,
            // the limit will depend on how often code checks the 
            // TimeIsUp property. Here, they have another
            // int.MaxValue seconds to check it. (They have to
            // check it before the difference rolls round.)
            // You could narrow the margin.  For example,
            // testing for fullMaxTicks > uint.MaxValue - 10000
            // would be OK if you were testing the value at least
            // once every 10 seconds.
            // With the code as it is, there's a safety margin of
            // some 24.5 days!

            throw new ArgumentOutOfRangeException("time", time,
                "Cannot wait for more than Int32.MaxValue milliseconds");
        }

        maxTicks = (uint) fullMaxTicks;
        startTicks = Environment.TickCount;
    }

    /// <summary>
    /// Returns true if the amount of time specified at construction has
    /// elapsed since construction.
    /// </summary>
    public bool TimeIsUp
    {
        get
        {
            uint diff = (uint) (Environment.TickCount - startTicks);
            return diff >= maxTicks;
        }
    }
}
*/