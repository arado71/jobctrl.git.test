using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Core;
using Xunit;

namespace Tct.Tests.ActivityRecorderClient
{
	public class AssignDataTests
	{
		[Fact]
		public void WorkIdDoesntMatterForHashSetButOtherFieldsAre()
		{
			var hashSet = new HashSet<AssignWorkData>();
			hashSet.Add(new AssignWorkData() { ServerRuleId = 3, WorkKey = "wKey", WorkId = 2 });
			Assert.True(hashSet.Contains(new AssignWorkData() { ServerRuleId = 3, WorkKey = "wKey", WorkId = 2 }));
			Assert.True(hashSet.Contains(new AssignWorkData() { ServerRuleId = 3, WorkKey = "wKey", WorkId = 3 }));
			Assert.True(hashSet.Contains(new AssignWorkData() { ServerRuleId = 3, WorkKey = "wKey", WorkId = null }));
			Assert.False(hashSet.Contains(new AssignWorkData() { ServerRuleId = 1, WorkKey = "wKey", WorkId = 2 }));
			Assert.True(hashSet.Contains(new AssignWorkData() { ServerRuleId = 3, WorkKey = "wkey", WorkId = 2 }));
		}

		[Fact]
		public void AssignDataCreateRules()
		{
			Assert.Throws<ArgumentNullException>(() => new AssignData((AssignWorkData)null));
			Assert.Throws<ArgumentNullException>(() => new AssignData((AssignProjectData)null));
			Assert.Throws<ArgumentNullException>(() => new AssignData((AssignCompositeData)null));
			Assert.Null(new AssignData(new AssignWorkData()).Project);
			Assert.Null(new AssignData(new AssignWorkData()).Composite);
			Assert.Null(new AssignData(new AssignProjectData()).Work);
			Assert.Null(new AssignData(new AssignProjectData()).Composite);
			Assert.Null(new AssignData(new AssignCompositeData()).Work);
			Assert.Null(new AssignData(new AssignCompositeData()).Project);
		}

		[Fact]
		public void AssignDataEquals()
		{
			AssertEquals(new AssignData(new AssignWorkData() { WorkKey = "x" }), new AssignData(new AssignWorkData() { WorkKey = "x" }));
			AssertEquals(new AssignData(new AssignWorkData() { WorkKey = "x", ServerRuleId = 1 }), new AssignData(new AssignWorkData() { WorkKey = "x", ServerRuleId = 1 }));
			AssertEquals(new AssignData(new AssignWorkData() { WorkKey = "x", ServerRuleId = 1, WorkName = "xy" }), new AssignData(new AssignWorkData() { WorkKey = "x", ServerRuleId = 1, WorkName = "xyz" }));
			Assert.False(new AssignData(new AssignWorkData() { WorkKey = "x", ServerRuleId = 1 }).Equals(new AssignData(new AssignWorkData() { WorkKey = "x", ServerRuleId = 2 })));
			Assert.False(new AssignData(new AssignWorkData() { WorkKey = "x", ServerRuleId = 1 }).Equals(new AssignData(new AssignWorkData() { WorkKey = "y", ServerRuleId = 1 })));
			Assert.False(new AssignData(new AssignWorkData() { WorkKey = "x" }).Equals(new AssignData(new AssignProjectData() { ProjectKey = "x" })));

			AssertEquals(new AssignData(new AssignProjectData() { ProjectKey = "x" }), new AssignData(new AssignProjectData() { ProjectKey = "x" }));
			AssertEquals(new AssignData(new AssignProjectData() { ProjectKey = "x", ServerRuleId = 1 }), new AssignData(new AssignProjectData() { ProjectKey = "x", ServerRuleId = 1 }));
			AssertEquals(new AssignData(new AssignProjectData() { ProjectKey = "x", ServerRuleId = 1, ProjectName = "xy" }), new AssignData(new AssignProjectData() { ProjectKey = "x", ServerRuleId = 1, ProjectName = "xyz" }));
			Assert.False(new AssignData(new AssignProjectData() { ProjectKey = "x", ServerRuleId = 1 }).Equals(new AssignData(new AssignProjectData() { ProjectKey = "x", ServerRuleId = 2 })));
			Assert.False(new AssignData(new AssignProjectData() { ProjectKey = "x", ServerRuleId = 1 }).Equals(new AssignData(new AssignProjectData() { ProjectKey = "y", ServerRuleId = 1 })));
			Assert.False(new AssignData(new AssignProjectData() { ProjectKey = "x" }).Equals(new AssignData(new AssignWorkData() { WorkKey = "x" })));

			AssertEquals(new AssignData(new AssignCompositeData() { WorkKey = "x" }), new AssignData(new AssignCompositeData() { WorkKey = "x" }));
			AssertEquals(new AssignData(new AssignCompositeData() { WorkKey = "x", ProjectKeys = new List<string> { "y", "z" } }), new AssignData(new AssignCompositeData() { WorkKey = "x", ProjectKeys = new List<string> { "y", "z" } }));
			AssertEquals(new AssignData(new AssignCompositeData() { WorkKey = "x", ServerRuleId = 1 }), new AssignData(new AssignCompositeData() { WorkKey = "x", ServerRuleId = 1 }));
			AssertEquals(new AssignData(new AssignCompositeData() { WorkKey = "x", ServerRuleId = 1, WorkName = "xy" }), new AssignData(new AssignCompositeData() { WorkKey = "x", ServerRuleId = 1, WorkName = "xyz" }));
			Assert.False(new AssignData(new AssignCompositeData() { WorkKey = "x", ServerRuleId = 1 }).Equals(new AssignData(new AssignCompositeData() { WorkKey = "x", ServerRuleId = 2 })));
			Assert.False(new AssignData(new AssignCompositeData() { WorkKey = "x", ProjectKeys = new List<string> { "y", "z" }, ServerRuleId = 1 }).Equals(new AssignData(new AssignCompositeData() { WorkKey = "x", ProjectKeys = new List<string> { "y", "z" }, ServerRuleId = 2 })));
			Assert.False(new AssignData(new AssignCompositeData() { WorkKey = "x", ServerRuleId = 1 }).Equals(new AssignData(new AssignCompositeData() { WorkKey = "y", ServerRuleId = 1 })));
			Assert.False(new AssignData(new AssignCompositeData() { WorkKey = "x", ProjectKeys = new List<string> { "y", "z" }, ServerRuleId = 1 }).Equals(new AssignData(new AssignCompositeData() { WorkKey = "x", ProjectKeys = new List<string> { "y", "s" }, ServerRuleId = 1 })));
			Assert.False(new AssignData(new AssignCompositeData() { WorkKey = "x", ProjectKeys = new List<string> { "y", "z" }, ServerRuleId = 1 }).Equals(new AssignData(new AssignCompositeData() { WorkKey = "x", ProjectKeys = new List<string> { "y" }, ServerRuleId = 1 })));
			Assert.False(new AssignData(new AssignCompositeData() { WorkKey = "x" }).Equals(new AssignData(new AssignWorkData() { WorkKey = "x" })));
		}

		[Fact]
		public void AssignDataIsCaseInsensitive()
		{
			AssertEquals(new AssignData(new AssignWorkData() { WorkKey = "X" }), new AssignData(new AssignWorkData() { WorkKey = "x" }));
			AssertEquals(new AssignData(new AssignProjectData() { ProjectKey = "X" }), new AssignData(new AssignProjectData() { ProjectKey = "x" }));
			AssertEquals(new AssignData(new AssignCompositeData() { WorkKey = "X" }), new AssignData(new AssignCompositeData() { WorkKey = "x" }));
			AssertEquals(new AssignData(new AssignCompositeData() { WorkKey = "X", ProjectKeys = new List<string> { "Y" } }), new AssignData(new AssignCompositeData() { WorkKey = "x", ProjectKeys = new List<string> { "y" } }));
			AssertEquals(new AssignData(new AssignCompositeData() { WorkKey = "X", ProjectKeys = new List<string> { "Y", "Z" } }), new AssignData(new AssignCompositeData() { WorkKey = "x", ProjectKeys = new List<string> { "y", "z" } }));
		}

		private static void AssertEquals(AssignData data1, AssignData data2)
		{
			Assert.Equal(data1.GetHashCode(), data2.GetHashCode());
			Assert.True(data1.Equals(data2));
			var hashSet = new HashSet<AssignData>();
			Assert.True(hashSet.Add(data1));
			Assert.False(hashSet.Add(data2));
		}
	}
}
