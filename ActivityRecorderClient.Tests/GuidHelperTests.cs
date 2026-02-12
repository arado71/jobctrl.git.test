using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.Serialization;
using Xunit;

namespace Tct.Tests.ActivityRecorderClient
{
	public class GuidHelperTests
	{
		[Fact]
		public void IncreaseEmpty()
		{
			Assert.Equal(new Guid(Enumerable.Repeat((byte)0, 15).Concat(new[] { (byte)1 }).ToArray()), GuidHelper.IncreaseGuid(Guid.Empty));
		}

		[Fact]
		public void IncreaseOverflow()
		{
			Assert.Equal(new Guid(Enumerable.Repeat((byte)0, 14).Concat(new[] { (byte)1, (byte)0 }).ToArray())
			, GuidHelper.IncreaseGuid(new Guid(Enumerable.Repeat((byte)0, 15).Concat(new[] { (byte)255 }).ToArray())));
		}

		[Fact]
		public void IncreaseOverflowChecked()
		{
			checked
			{
				Assert.Equal(new Guid(Enumerable.Repeat((byte)0, 14).Concat(new[] { (byte)1, (byte)0 }).ToArray())
				, GuidHelper.IncreaseGuid(new Guid(Enumerable.Repeat((byte)0, 15).Concat(new[] { (byte)255 }).ToArray())));
			}
		}

		[Fact]
		public void IncreaseOverflowTwice()
		{
			Assert.Equal(new Guid(Enumerable.Repeat((byte)0, 12).Concat(new[] { (byte)12, (byte)255, (byte)0, (byte)0 }).ToArray())
			, GuidHelper.IncreaseGuid(new Guid(Enumerable.Repeat((byte)0, 12).Concat(new[] { (byte)12, (byte)254, (byte)255, (byte)255 }).ToArray())));
		}

		[Fact]
		public void IncreaseAsmGuid()
		{
			Assert.Equal(new Guid("d242adba-0b5f-4b48-b4b3-a8bef3d117b6"), GuidHelper.IncreaseGuid(new Guid("d242adba-0b5f-4b48-b4b3-a8bef3d117b5")));
		}

		[Fact]
		public void IncreaseMax()
		{
			Assert.Equal(Guid.Empty, GuidHelper.IncreaseGuid(new Guid(Enumerable.Repeat((byte)255, 16).ToArray())));
		}

		[Fact]
		public void IncreaseAlphabeticOrderTest()
		{
			var guids = new List<Guid>();
			var curr = new Guid("d242adba-0b5f-4b48-b4b3-a8bef3d117b6");
			for (int i = 0; i < 200; i++)
			{
				curr = GuidHelper.IncreaseGuid(curr);
				guids.Add(curr);
			}
			Assert.True(guids.Select(n => n.ToString()).OrderBy(n => n).Select((n, i) => guids[i] == new Guid(n)).All(n => n));
		}
	}
}
