using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Core;
using Tct.ActivityRecorderClient.Serialization;
using Tct.ActivityRecorderClient.Stats;
using Xunit;

namespace Tct.Tests.ActivityRecorderClient
{
	public class DeepCloneTests
	{
		// Use only for value types!
		static void DeepCloneThenCompareAny<T>(T src)
		{
			var copy = src.DeepClone();
			Assert.Equal(src, copy);
		}

		enum TestEnum
		{
			One, Two, Three, Four,
		}

		struct  TestStruct
		{
			public int Field1;
			public bool Field2;
			public long Field3;
		}

		[Fact]
		public void ValueTypes()
		{
			DeepCloneThenCompareAny(true);
			DeepCloneThenCompareAny(1234);
			DeepCloneThenCompareAny(123456789123456789L);
			DeepCloneThenCompareAny(3.14);
			DeepCloneThenCompareAny(0.12415231136136136D);
			DeepCloneThenCompareAny(TestEnum.Two);
		}

		[Fact]
		public void Structures()
		{
			DeepCloneThenCompareAny(new TestStruct
			{
				Field1 = 345,
				Field2 = true,
				Field3 = 987654321
			});
			DeepCloneThenCompareAny(new DateTime(2016, 07, 13, 11, 00, 00));
		}

		[Fact]
		public void SystemRefTypes()
		{
			DeepCloneThenCompareAny("apple");
			DeepCloneThenCompareAny(new Nullable<int>(1234));
		}

		[Fact]
		public void ValueCollections()
		{
			DeepCloneThenCompareAny(new List<int>() { 1, 2, 5, 13, 7, 1 });
			DeepCloneThenCompareAny(new HashSet<string>() { "apple", "nut", "pine" });
			DeepCloneThenCompareAny(new Dictionary<int, string>() {{1, "anne"}, {5, "lilly"}, {17, "hannah"} });
		}

		[Fact]
		public void List()
		{
			var src = new List<int>() { 1, 2, 5, 13, 7, 1 };
			var copy = src.DeepClone();
			Assert.NotSame(src, copy);
			Assert.Equal(src.Count, copy.Count);
			Assert.Equal(src, copy);
		}

		[Fact]
		public void ReferenceList()
		{
			var src = new List<CategoryData> { categoryData };
			var copy = src.DeepClone();
			Assert.NotSame(src, copy);
			Assert.Equal(src.Count, copy.Count);
			Assert.NotSame(src[0], copy[0]);
			Assert.Equal(src[0].Id, copy[0].Id);
			Assert.Equal(src[0].Name, copy[0].Name);
		}

		static AssignData assignData = new AssignData(new AssignWorkData() { WorkId = 1, WorkName = "Test Work1", ProjectId = 2, });
		static WorkData workData = new WorkData() { AssignData = assignData, CategoryId = 2, CloseReasonRequiredTime = TimeSpan.FromHours(1), Name = "Work 1", ProjectId = 5, StartDate = DateTime.UtcNow };
		static CategoryData categoryData = new CategoryData() { Id = 234, Name = "Category 1" };
		static CompositeMapping compositeMapping = new CompositeMapping() { WorkIdByKey = new Dictionary<string, int>() { { "key1", 1 }, { "key2", 2 } }, ChildrenByKey = new Dictionary<string, CompositeMapping>() { { "key3", new CompositeMapping() } } };
		static DailyWorkTimeStats dailyWorkTimeStats = new DailyWorkTimeStats() { ComputerWorkTime = TimeSpan.FromMinutes(15), Day = DateTime.Today, HolidayTime = TimeSpan.Zero, TotalWorkTimeByWorkId = new Dictionary<int, TimeSpan>() { { 1, TimeSpan.FromMinutes(5) } } };
		static SimpleWorkTimeStat simpleWorkTimeStat = new SimpleWorkTimeStat() { WorkId = 1, TotalWorkTime = TimeSpan.FromDays(1) };

		[Fact]
		public void IssueData()
		{
			var src = new IssueData() { Company = "Acme", Name = "Issue1", State = 1, UserId = 13, IssueCode = "2a32b36f", Modified = new DateTime(2013, 12, 24) };
			var copy = src.DeepClone();
			Assert.NotSame(src, copy);
			Assert.Equal(src.Name, copy.Name);
			Assert.Equal(src.Company, copy.Company);
			Assert.Equal(src.IssueCode, copy.IssueCode);
			Assert.Equal(src.Modified, copy.Modified);
			Assert.Equal(src.State, copy.State);
			Assert.Equal(src.UserId, copy.UserId);
		}

		[Fact]
		public void AssignWorkData()
		{
			var src = new AssignWorkData() { WorkId = 1, ProjectId = 2, ServerRuleId = 3, WorkName = "Work 1", };
			var copy = src.DeepClone();
			Assert.NotSame(src, copy);
			Assert.Equal(src.WorkName, copy.WorkName);
			Assert.Equal(src.ProjectId, copy.ProjectId);
			Assert.Equal(src.WorkId, copy.WorkId);
			Assert.Equal(src.ServerRuleId, copy.ServerRuleId);
		}

		[Fact]
		public void AssignProjectData()
		{
			var src = new AssignProjectData() { ProjectKey = "prj1", ProjectId = 1, ProjectName = "Project 1", WorkId = 2, };
			var copy = src.DeepClone();
			Assert.NotSame(src, copy);
			Assert.Equal(src.ProjectKey, copy.ProjectKey);
			Assert.Equal(src.ProjectId, copy.ProjectId);
			Assert.Equal(src.ProjectName, copy.ProjectName);
			Assert.Equal(src.WorkId, copy.WorkId);
		}

		[Fact]
		public void AssignCompositeData()
		{
			var src = new AssignCompositeData() { WorkId = 1, WorkName = "Work 1", ServerRuleId = 2, WorkKey = "work1", ProjectKeys = new List<string>(){"cat", "dog"}};
			var copy = src.DeepClone();
			Assert.NotSame(src, copy);
			Assert.Equal(src.WorkId, copy.WorkId);
			Assert.Equal(src.WorkName, copy.WorkName);
			Assert.Equal(src.ServerRuleId, copy.ServerRuleId);
			Assert.Equal(src.WorkKey, copy.WorkKey);
			Assert.Equal(src.ProjectKeys, copy.ProjectKeys);
		}

		[Fact]
		public void AssignCommonData()
		{
			var src = new AssignCommonData(new Dictionary<string, string>() { { "a", "cat" }, { "b", "dog" } });
			var copy = src.DeepClone();
			Assert.NotSame(src, copy);
			Assert.Equal(src.Data, copy.Data);
		}

		[Fact]
		public void AssignData()
		{
			var copy = assignData.DeepClone();
			Assert.NotSame(assignData, copy);
			Assert.Equal(assignData.Work.WorkName, copy.Work.WorkName);
		}

		[Fact]
		public void WorkData()
		{
			var copy = workData.DeepClone();
			Assert.NotSame(workData, copy);
			Assert.Equal(workData.Id, copy.Id);
			Assert.Equal(workData.Name, copy.Name);
			Assert.Equal(workData.CategoryId, copy.CategoryId);
			Assert.Equal(workData.CloseReasonRequiredTime, copy.CloseReasonRequiredTime);
			Assert.Equal(workData.ProjectId, copy.ProjectId);
		}

		[Fact]
		public void CategoryData()
		{
			var copy = categoryData.DeepClone();
			Assert.NotSame(categoryData, copy);
			Assert.Equal(categoryData.Id, copy.Id);
			Assert.Equal(categoryData.Name, copy.Name);
		}

		[Fact]
		public void CompositeMapping()
		{
			var copy = compositeMapping.DeepClone();
			Assert.NotSame(compositeMapping, copy);
			Assert.Equal(compositeMapping.ChildrenByKey.Count, copy.ChildrenByKey.Count);
			Assert.Equal(compositeMapping.WorkIdByKey.Count, copy.WorkIdByKey.Count);
		}

		[Fact]
		public void ClientMenu()
		{
			var clientMenu = new ClientMenu() { CategoriesById = new Dictionary<int, CategoryData>() { { 234, categoryData } }, ExternalCompositeMapping = compositeMapping, Works = new List<WorkData>() { workData }, ExternalProjectIdMapping = new Dictionary<string, int>() { { "key1", 1 }, { "key2", 2 } }, ExternalWorkIdMapping = new Dictionary<string, int>() { { "key3", 3 }, { "key4", 4 } } };
			var copy = clientMenu.DeepClone();
			Assert.NotSame(clientMenu, copy);
			Assert.NotSame(clientMenu.CategoriesById, copy.CategoriesById);
			Assert.NotSame(clientMenu.Works, copy.Works);
			Assert.Equal(clientMenu.CategoriesById.Count, copy.CategoriesById.Count);
			Assert.Equal(clientMenu.ExternalCompositeMapping.ChildrenByKey.Count, copy.ExternalCompositeMapping.ChildrenByKey.Count);
			Assert.Equal(clientMenu.Works.Count, copy.Works.Count);
			Assert.Equal(clientMenu.ExternalProjectIdMapping.Count, copy.ExternalProjectIdMapping.Count);
			Assert.Equal(clientMenu.ExternalWorkIdMapping.Count, copy.ExternalWorkIdMapping.Count);
		}

		[Fact]
		public void DailyWorkTimeStats()
		{
			var copy = dailyWorkTimeStats.DeepClone();
			Assert.NotSame(dailyWorkTimeStats, copy);
			Assert.Equal(dailyWorkTimeStats.Day, copy.Day);
			Assert.Equal(dailyWorkTimeStats.ComputerWorkTime, copy.ComputerWorkTime);
			Assert.Equal(dailyWorkTimeStats.HolidayTime, copy.HolidayTime);
			Assert.Equal(dailyWorkTimeStats.TotalWorkTimeByWorkId, copy.TotalWorkTimeByWorkId);
		}

		[Fact]
		public void DailyWorkTimeStatsData()
		{
			var dailyWorkTimeStatsData = new DailyWorkTimeStatsData(){DailyWorkTimes = new Dictionary<DateTime, DailyWorkTimeStats>(){{DateTime.Today, dailyWorkTimeStats}}};
			var copy = dailyWorkTimeStatsData.DeepClone();
			Assert.NotSame(dailyWorkTimeStatsData, copy);
			Assert.Equal(dailyWorkTimeStatsData.DailyWorkTimes.Keys, copy.DailyWorkTimes.Keys);
		}

		[Fact]
		public void SimpleWorkTimeStat()
		{
			var copy = simpleWorkTimeStat.DeepClone();
			Assert.NotSame(simpleWorkTimeStat, copy);
			Assert.Equal(simpleWorkTimeStat.WorkId, copy.WorkId);
			Assert.Equal(simpleWorkTimeStat.TotalWorkTime, copy.TotalWorkTime);
		}

		[Fact]
		public void SimpleWorkTimeStats()
		{
			var simpleWorkTimeStats = new SimpleWorkTimeStats(){UserId = 13, Stats = new Dictionary<int, SimpleWorkTimeStat>(){{3, simpleWorkTimeStat}}, FromDate = DateTime.UtcNow};
			var copy = simpleWorkTimeStats.DeepClone();
			Assert.NotSame(simpleWorkTimeStats, copy);
			Assert.Equal(simpleWorkTimeStats.UserId, copy.UserId);
			Assert.Equal(simpleWorkTimeStats.Stats.Count, copy.Stats.Count);
			Assert.Equal(simpleWorkTimeStats.FromDate, copy.FromDate);
		}

	}
}
