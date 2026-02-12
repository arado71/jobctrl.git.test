using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class CollectionAllocationTests
	{
		[Fact]
		public void DictionaryAllocAndShrink()
		{
			//Arrange
			var dict = new Dictionary<Guid, string>();
			for (var i = 0; i < 1000; i++)
			{
				dict.Add(Guid.NewGuid(), Guid.NewGuid().ToString());
			}

			//Act
			for (var i = 0; i < 900; i++)
				dict.Remove(dict.Keys.First());

			//Assert
			var fi = dict.GetType().GetField("entries", BindingFlags.NonPublic | BindingFlags.Instance);
			var entries = fi.GetValue(dict) as Array;
			Assert.True(1000 < entries?.Length);
			dict = dict.ToDictionary(d => d.Key, d => d.Value);
			entries = fi.GetValue(dict) as Array;
			Assert.True(200 > entries?.Length);
		}

		[Fact]
		public void ListAllocationAndShrink()
		{
			//Arrange
			var list = new List<Guid>();
			for (var i = 0; i < 1000; i++)
			{
				list.Add(Guid.NewGuid());
			}

			//Act
			for (var i = 0; i < 900; i++)
				list.Remove(list.First());

			//Assert
			Assert.True(1000 < list.Capacity);
			list.Clear();
			Assert.True(1000 < list.Capacity);
			list.Capacity = 0;
			Assert.Equal(0, list.Capacity);
		}
	}
}
