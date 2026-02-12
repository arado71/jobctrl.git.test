using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Markup;
using Reporter.Model;
using Reporter.Model.ProcessedItems;
using Reporter.Model.WorkItems;
using Reporter.Processing;
using Xunit;
using AdhocMeetingWorkItem = Reporter.Model.ProcessedItems.AdhocMeetingWorkItem;
using ManualWorkItem = Reporter.Model.ProcessedItems.ManualWorkItem;
using MobileWorkItem = Reporter.Model.ProcessedItems.MobileWorkItem;
using WorkItem = Reporter.Model.WorkItems.WorkItem;

namespace Reporting.Test
{
	public class ProcessingTest
	{
		[Fact]
		public void SimpleCase()
		{
			var items = new List<CollectedItem>
			{
				new ComputerCollectedItem()
				{
					ComputerId = 1234,
					CreateDate = new DateTime(2000, 1, 1, 12, 0, 0),
					Key = "ProcessName",
					Value = "something.exe",
					UserId = 5
				},
				new ComputerCollectedItem()
				{
					ComputerId = 1234,
					CreateDate = new DateTime(2000, 1, 1, 12, 0, 0),
					Key = "Title",
					Value = "ACME app",
					UserId = 5
				},
				new UniversalCollectedItem()
				{
					CreateDate = new DateTime(2000, 1, 1, 12, 2, 0),
					Key = "ProcessName",
					Value = "iexplorer.exe",
					UserId = 5
				},
				new UniversalCollectedItem()
				{
					CreateDate = new DateTime(2000, 1, 1, 12, 2, 0),
					Key = "Title",
					Value = "Internet explorer",
					UserId = 5
				},
			};
			var deletes = new List<WorkItemDeletion>();
			var workitems = new List<Reporter.Model.WorkItems.ComputerWorkItem>()
			{
				new Reporter.Model.WorkItems.ComputerWorkItem()
				{
					ComputerId = 1234, 
					StartDate = new DateTime(2000, 1, 1, 11, 58, 0),
					EndDate = new DateTime(2000, 1, 1, 12, 3, 0, 0),
					KeyboardActivity = 10,
					MouseActivity = 100,
					UserId = 5,
					WorkId = 1
				}
			};

			var result = ReportHelper.Transform(items, deletes, workitems).ToArray();
			Assert.Equal(3, result.Length);
			Assert.Equal(new DateTime(2000, 1, 1, 11, 58, 0), result[0].StartDate);
			Assert.Equal(new DateTime(2000, 1, 1, 12, 0, 0), result[0].EndDate);
			Assert.Equal(new DateTime(2000, 1, 1, 12, 0, 0), result[1].StartDate);
			Assert.Equal(new DateTime(2000, 1, 1, 12, 2, 0), result[1].EndDate);
			Assert.Equal(new DateTime(2000, 1, 1, 12, 2, 0), result[2].StartDate);
			Assert.Equal(new DateTime(2000, 1, 1, 12, 3, 0), result[2].EndDate);
			Assert.True(result.All(x => x is PcWorkItem));
			Assert.True(result.All(x => ((PcWorkItem)x).ComputerId == 1234));
			Assert.True(result.All(x => x.UserId == 5));
			Assert.True(result.All(x => x.Duration == (x.EndDate - x.StartDate)));
			Assert.False(result[0].Values.ContainsKey("ProcessName"));
			Assert.False(result[0].Values.ContainsKey("Title"));
			Assert.Equal("something.exe", ((PcWorkItem)result[1]).Values["ProcessName"]);
			Assert.Equal("ACME app", ((PcWorkItem)result[1]).Values["Title"]);
			Assert.Equal("iexplorer.exe", ((PcWorkItem)result[2]).Values["ProcessName"]);
			Assert.Equal("Internet explorer", ((PcWorkItem)result[2]).Values["Title"]);
		}

		[Fact]
		public void MultipleTypes()
		{
			var items = new List<CollectedItem>();
			var deletes = new List<WorkItemDeletion>();
			var workitems = new List<WorkItem>()
			{
				new Reporter.Model.WorkItems.ComputerWorkItem()
				{
					ComputerId = 1234,
					StartDate = new DateTime(2000, 1, 1, 12, 0, 0),
					EndDate = new DateTime(2000, 1, 1, 12, 10, 0),
					KeyboardActivity = 10,
					MouseActivity = 100,
					UserId = 123,
					WorkId = 1234,
				},
				new Reporter.Model.WorkItems.ManualWorkItem()
				{
					StartDate = new DateTime(2000, 1, 1, 12, 10, 0),
					EndDate = new DateTime(2000, 1, 1, 12, 20, 0),
					UserId = 123,
					WorkId = 1235,
					Description = "Meeting",
				},
				new Reporter.Model.WorkItems.AdhocMeetingWorkItem()
				{
					StartDate = new DateTime(2000, 1, 1, 12, 20, 0),
					EndDate = new DateTime(2000, 1, 1, 12, 30, 0),
					UserId = 123,
					WorkId = 1234,
					Title = "meetup",
					Description = "Daily scrum",
					Participants = "boss@fabrikam.com",
				},
				new Reporter.Model.WorkItems.MobileWorkItem()
				{
					StartDate = new DateTime(2000, 1, 1, 12, 30, 0),
					EndDate = new DateTime(2000, 1, 1, 12, 40, 0),
					UserId = 123,
					WorkId = 1234,
					Imei = 98765432,
                    CallId = 1111
				}
			};

            items.Add(new UniversalCollectedItem
            {
                UserId = 123,
                CreateDate = new DateTime(2000, 1, 1, 12, 35, 0),
                Key = "key1",
                Value = "Value1"
            });

			var result = ReportHelper.Transform(items, deletes, workitems).ToArray();

			Assert.Equal(5, result.Length);
			var computerSlice = result.OfType<PcWorkItem>().Single();
			Assert.Equal(1234, computerSlice.ComputerId);
			Assert.Equal(workitems[0].StartDate, computerSlice.StartDate);
			Assert.Equal(workitems[0].EndDate, computerSlice.EndDate);
			var manualSlice = result.OfType<ManualWorkItem>().Single();
			Assert.Equal("Meeting", manualSlice.Description);
			var meetingSlice = result.OfType<AdhocMeetingWorkItem>().Single();
			Assert.Equal("meetup", meetingSlice.Title);
			var mobileSlices = result.OfType<MobileWorkItem>().ToList();
		    var mobileSlice = mobileSlices[0];
            Assert.Equal(98765432, mobileSlice.Imei);
            Assert.Equal(TimeSpan.FromMinutes(5), mobileSlice.Duration);
            Assert.Equal(new DateTime(2000, 1, 1, 12, 30, 0), mobileSlice.StartDate);
            Assert.Equal(new DateTime(2000, 1, 1, 12, 35, 0), mobileSlice.EndDate);
            Assert.Equal(1111, mobileSlice.CallId);
            mobileSlice = mobileSlices[1];
            Assert.Equal(98765432, mobileSlice.Imei);
            Assert.Equal(TimeSpan.FromMinutes(5), mobileSlice.Duration);
            Assert.Equal(new DateTime(2000, 1, 1, 12, 35, 0), mobileSlice.StartDate);
            Assert.Equal(new DateTime(2000, 1, 1, 12, 40, 0), mobileSlice.EndDate);
            Assert.Equal(1111, mobileSlice.CallId);
        }

		[Fact]
		public void SimpleCase2()
		{
			var items = new List<CollectedItem>();
			var deletes = new List<WorkItemDeletion>()
			{
				new WorkItemDeletion()
				{
					UserId = 65,
					StartDate = new DateTime(2015, 05, 04, 10, 44, 42, 900),
					EndDate = new DateTime(2015, 05, 04, 11, 36, 02, 370)
				},
				new WorkItemDeletion()
				{
					UserId = 65,
					StartDate = new DateTime(2015, 05, 04, 11, 36, 45, 063),
					EndDate = new DateTime(2015, 05, 04, 12, 03, 43, 787)
				},
				new WorkItemDeletion()
				{
					UserId = 65,
					StartDate = new DateTime(2015, 05, 04, 08, 26, 19, 000),
					EndDate = new DateTime(2015, 05, 04, 08, 40, 53, 000)
				},
				new WorkItemDeletion()
				{
					UserId = 65,
					StartDate = new DateTime(2015, 05, 04, 10, 44, 42, 900),
					EndDate = new DateTime(2015, 05, 04, 10, 49, 42, 900)
				},
				new WorkItemDeletion()
				{
					UserId = 65,
					StartDate = new DateTime(2015, 05, 04, 11, 36, 45, 063),
					EndDate = new DateTime(2015, 05, 04, 11, 41, 45, 063)
				},
			};
			var workitems = new List<Reporter.Model.WorkItems.ComputerWorkItem>()
			{
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,6,17,16,440), EndDate = new DateTime(2015,05,04,6,25,52,163), UserId = 65, WorkId = 16768},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,6,26,53,733), EndDate = new DateTime(2015,05,04,6,28,43,307), UserId = 65, WorkId = 7617662},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,6,28,43,307), EndDate = new DateTime(2015,05,04,6,28,56,257), UserId = 65, WorkId = 16768},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,6,28,56,257), EndDate = new DateTime(2015,05,04,6,29,41,277), UserId = 65, WorkId = 7617662},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,6,29,41,277), EndDate = new DateTime(2015,05,04,6,29,46,910), UserId = 65, WorkId = 16768},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,7,32,32,233), EndDate = new DateTime(2015,05,04,7,40,26,570), UserId = 65, WorkId = 16768},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,7,40,26,570), EndDate = new DateTime(2015,05,04,7,41,55,817), UserId = 65, WorkId = 7617662},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,7,41,55,817), EndDate = new DateTime(2015,05,04,7,41,58,673), UserId = 65, WorkId = 16768},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,7,41,58,673), EndDate = new DateTime(2015,05,04,7,42,4,880), UserId = 65, WorkId = 7617662},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,7,42,4,880), EndDate = new DateTime(2015,05,04,7,42,11,543), UserId = 65, WorkId = 16768},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,7,42,11,543), EndDate = new DateTime(2015,05,04,7,42,34,83), UserId = 65, WorkId = 7617662},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,7,42,34,83), EndDate = new DateTime(2015,05,04,7,43,32,227), UserId = 65, WorkId = 16768},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,7,43,32,227), EndDate = new DateTime(2015,05,04,7,45,48,510), UserId = 65, WorkId = 7617662},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,7,45,48,510), EndDate = new DateTime(2015,05,04,7,47,47,663), UserId = 65, WorkId = 16768},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,7,47,47,663), EndDate = new DateTime(2015,05,04,7,47,49,223), UserId = 65, WorkId = 7617662},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,7,47,49,223), EndDate = new DateTime(2015,05,04,7,58,15,503), UserId = 65, WorkId = 16768},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,7,58,15,503), EndDate = new DateTime(2015,05,04,7,58,33,117), UserId = 65, WorkId = 7617662},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,7,58,33,117), EndDate = new DateTime(2015,05,04,7,58,35,487), UserId = 65, WorkId = 16768},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,7,58,35,487), EndDate = new DateTime(2015,05,04,7,58,39,43), UserId = 65, WorkId = 7617662},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,7,58,39,43), EndDate = new DateTime(2015,05,04,7,58,44,410), UserId = 65, WorkId = 16768},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,7,58,44,410), EndDate = new DateTime(2015,05,04,7,58,45,50), UserId = 65, WorkId = 7617662},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,7,58,45,50), EndDate = new DateTime(2015,05,04,7,59,12,710), UserId = 65, WorkId = 16768},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,7,59,12,710), EndDate = new DateTime(2015,05,04,7,59,59,667), UserId = 65, WorkId = 9846510},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,7,59,59,667), EndDate = new DateTime(2015,05,04,8,0,12,893), UserId = 65, WorkId = 7943637},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,8,0,12,893), EndDate = new DateTime(2015,05,04,8,0,14,503), UserId = 65, WorkId = 7617662},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,8,0,14,503), EndDate = new DateTime(2015,05,04,8,0,17,13), UserId = 65, WorkId = 7943637},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,8,0,17,13), EndDate = new DateTime(2015,05,04,8,0,18,603), UserId = 65, WorkId = 7617662},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,8,0,18,603), EndDate = new DateTime(2015,05,04,8,5,30,653), UserId = 65, WorkId = 7943637},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,8,38,42,77), EndDate = new DateTime(2015,05,04,9,19,48,310), UserId = 65, WorkId = 7943637},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,9,19,48,310), EndDate = new DateTime(2015,05,04,9,19,52,523), UserId = 65, WorkId = 7617662},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,9,19,52,523), EndDate = new DateTime(2015,05,04,9,19,58,967), UserId = 65, WorkId = 7943637},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,9,19,58,967), EndDate = new DateTime(2015,05,04,9,20,6,970), UserId = 65, WorkId = 7617662},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,9,20,6,970), EndDate = new DateTime(2015,05,04,9,28,25,563), UserId = 65, WorkId = 7943637},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,9,52,51,413), EndDate = new DateTime(2015,05,04,9,53,9,850), UserId = 65, WorkId = 7943637},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,9,53,9,850), EndDate = new DateTime(2015,05,04,9,55,45,727), UserId = 65, WorkId = 7617662},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,9,55,45,727), EndDate = new DateTime(2015,05,04,9,55,49,50), UserId = 65, WorkId = 7943637},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,10,42,45,970), EndDate = new DateTime(2015,05,04,10,49,42,867), UserId = 65, WorkId = 7943637},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,11,36,3,220), EndDate = new DateTime(2015,05,04,11,41,45,3), UserId = 65, WorkId = 7943637},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,12,3,44,910), EndDate = new DateTime(2015,05,04,12,3,46,423), UserId = 65, WorkId = 7943637},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,12,3,46,423), EndDate = new DateTime(2015,05,04,12,4,31,913), UserId = 65, WorkId = 7617662},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,12,4,31,913), EndDate = new DateTime(2015,05,04,12,4,42,257), UserId = 65, WorkId = 7943637},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,12,4,42,257), EndDate = new DateTime(2015,05,04,12,10,46,50), UserId = 65, WorkId = 7617662},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,12,12,36,247), EndDate = new DateTime(2015,05,04,12,12,36,683), UserId = 65, WorkId = 7943637},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,12,12,36,683), EndDate = new DateTime(2015,05,04,12,13,10,347), UserId = 65, WorkId = 7617662},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,14,47,13,437), EndDate = new DateTime(2015,05,04,14,47,13,920), UserId = 65, WorkId = 7943637},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,14,47,13,920), EndDate = new DateTime(2015,05,04,14,47,17,853), UserId = 65, WorkId = 7617662},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,19,47,31,333), EndDate = new DateTime(2015,05,04,19,48,34,687), UserId = 65, WorkId = 7943637},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,19,48,34,687), EndDate = new DateTime(2015,05,04,19,48,45,403), UserId = 65, WorkId = 7617662},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,19,48,45,403), EndDate = new DateTime(2015,05,04,19,55,21,773), UserId = 65, WorkId = 7943637},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,19,55,21,773), EndDate = new DateTime(2015,05,04,19,55,32,253), UserId = 65, WorkId = 7617662},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,19,55,32,253), EndDate = new DateTime(2015,05,04,19,56,38,913), UserId = 65, WorkId = 7943637},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,19,56,38,913), EndDate = new DateTime(2015,05,04,19,57,23,857), UserId = 65, WorkId = 7617662},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,19,57,23,857), EndDate = new DateTime(2015,05,04,19,58,3,827), UserId = 65, WorkId = 7943637},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,19,58,3,827), EndDate = new DateTime(2015,05,04,20,0,5,913), UserId = 65, WorkId = 7617662},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,20,0,5,913), EndDate = new DateTime(2015,05,04,20,0,53,960), UserId = 65, WorkId = 7943637},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,20,0,53,960), EndDate = new DateTime(2015,05,04,20,0,56,190), UserId = 65, WorkId = 7617662},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,20,0,56,190), EndDate = new DateTime(2015,05,04,20,2,24,643), UserId = 65, WorkId = 7943637},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,20,2,24,643), EndDate = new DateTime(2015,05,04,20,3,33,20), UserId = 65, WorkId = 7617662},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,20,3,33,20), EndDate = new DateTime(2015,05,04,20,4,11,473), UserId = 65, WorkId = 7943637},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,20,4,11,473), EndDate = new DateTime(2015,05,04,20,4,15,577), UserId = 65, WorkId = 7617662},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,20,4,15,577), EndDate = new DateTime(2015,05,04,20,4,44,827), UserId = 65, WorkId = 7943637},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,20,4,44,827), EndDate = new DateTime(2015,05,04,20,7,37,333), UserId = 65, WorkId = 7617662},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,20,7,37,333), EndDate = new DateTime(2015,05,04,20,17,5,987), UserId = 65, WorkId = 7943637},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,20,17,5,987), EndDate = new DateTime(2015,05,04,20,17,7,577), UserId = 65, WorkId = 7617662},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,20,17,7,577), EndDate = new DateTime(2015,05,04,20,18,18,637), UserId = 65, WorkId = 7943637},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,20,18,18,637), EndDate = new DateTime(2015,05,04,20,23,57,127), UserId = 65, WorkId = 16768},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,20,55,45,273), EndDate = new DateTime(2015,05,04,20,58,50,443), UserId = 65, WorkId = 16768},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,20,58,50,443), EndDate = new DateTime(2015,05,04,20,58,51,413), UserId = 65, WorkId = 7617662},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,20,58,51,413), EndDate = new DateTime(2015,05,04,21,10,13,853), UserId = 65, WorkId = 16768},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,21,10,13,853), EndDate = new DateTime(2015,05,04,21,10,16,427), UserId = 65, WorkId = 7617662},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,21,10,16,427), EndDate = new DateTime(2015,05,04,21,11,9,220), UserId = 65, WorkId = 16768},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,21,33,0,807), EndDate = new DateTime(2015,05,04,21,44,1,657), UserId = 65, WorkId = 16768},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,21,44,1,657), EndDate = new DateTime(2015,05,04,21,45,45,570), UserId = 65, WorkId = 7617662},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,21,45,45,570), EndDate = new DateTime(2015,05,04,21,56,51,443), UserId = 65, WorkId = 16768},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,21,56,51,443), EndDate = new DateTime(2015,05,04,21,56,55,547), UserId = 65, WorkId = 7617662},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,21,56,55,547), EndDate = new DateTime(2015,05,04,22,9,9,717), UserId = 65, WorkId = 16768},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,22,9,9,717), EndDate = new DateTime(2015,05,04,22,9,10,997), UserId = 65, WorkId = 7617662},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,22,9,10,997), EndDate = new DateTime(2015,05,04,22,9,12,247), UserId = 65, WorkId = 16768},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,22,9,12,247), EndDate = new DateTime(2015,05,04,22,9,15,397), UserId = 65, WorkId = 7617662},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,22,9,15,397), EndDate = new DateTime(2015,05,04,22,13,34,407), UserId = 65, WorkId = 16768},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,22,13,34,407), EndDate = new DateTime(2015,05,04,22,17,54,490), UserId = 65, WorkId = 7617662},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,22,17,54,490), EndDate = new DateTime(2015,05,04,22,17,58,920), UserId = 65, WorkId = 16768},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,22,17,58,920), EndDate = new DateTime(2015,05,04,22,19,13,410), UserId = 65, WorkId = 7617662},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,22,19,13,410), EndDate = new DateTime(2015,05,04,22,19,26,967), UserId = 65, WorkId = 16768},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,22,19,26,967), EndDate = new DateTime(2015,05,04,22,19,30,493), UserId = 65, WorkId = 7617662},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,22,19,30,493), EndDate = new DateTime(2015,05,04,22,19,41,880), UserId = 65, WorkId = 16768},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,22,19,41,880), EndDate = new DateTime(2015,05,04,22,19,44,143), UserId = 65, WorkId = 7617662},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,22,19,44,143), EndDate = new DateTime(2015,05,04,22,19,46,187), UserId = 65, WorkId = 16768},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,22,19,46,187), EndDate = new DateTime(2015,05,04,22,19,52,457), UserId = 65, WorkId = 7617662},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,22,19,52,457), EndDate = new DateTime(2015,05,04,22,29,11,253), UserId = 65, WorkId = 16768},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,22,29,11,253), EndDate = new DateTime(2015,05,04,22,34,1,73), UserId = 65, WorkId = 7617662},
				new Reporter.Model.WorkItems.ComputerWorkItem() { ComputerId = -1161288070, StartDate = new DateTime(2015,05,04,22,34,1,73), EndDate = new DateTime(2015,05,04,22,34,4,7), UserId = 65, WorkId = 16768},
			};

			var result = ReportHelper.Transform(items, deletes, workitems).ToArray();
			Assert.Equal(92, result.Length);
			Assert.True(workitems.Aggregate(TimeSpan.Zero, (x, y) => x + (y.EndDate - y.StartDate)) > result.Aggregate(TimeSpan.Zero, (x, y) => x + y.Duration));
		}

		[Fact]
		public void SplitCase()
		{
			var items = new List<CollectedItem>
			{
				new ComputerCollectedItem()
				{
					ComputerId = 1234,
					CreateDate = new DateTime(2000, 1, 1, 12, 0, 0),
					Key = "ProcessName",
					Value = "something.exe",
					UserId = 5
				},
				new ComputerCollectedItem()
				{
					ComputerId = 1234,
					CreateDate = new DateTime(2000, 1, 1, 12, 0, 0),
					Key = "Title",
					Value = "ACME app",
					UserId = 5
				},
				new ComputerCollectedItem()
				{
					ComputerId = 1234,
					CreateDate = new DateTime(2000, 1, 1, 12, 2, 0),
					Key = "ProcessName",
					Value = "iexplorer.exe",
					UserId = 5
				},
				new ComputerCollectedItem()
				{
					ComputerId = 1234,
					CreateDate = new DateTime(2000, 1, 1, 12, 2, 0),
					Key = "Title",
					Value = "Internet explorer",
					UserId = 5
				},
			};
			var deletes = new List<WorkItemDeletion>();
			var workitems = new List<Reporter.Model.WorkItems.ComputerWorkItem>()
			{
				new Reporter.Model.WorkItems.ComputerWorkItem()
				{
					ComputerId = 1234, 
					StartDate = new DateTime(2000, 1, 1, 11, 59, 0),
					EndDate = new DateTime(2000, 1, 1, 12, 1, 0, 0),
					KeyboardActivity = 10,
					MouseActivity = 100,
					UserId = 5,
					WorkId = 1
				}
			};

			var result = ReportHelper.Transform(items, deletes, workitems).ToArray();
			Assert.Equal(2, result.Length);
			Assert.Equal(new DateTime(2000, 1, 1, 11, 59, 0), result[0].StartDate);
			Assert.Equal(new DateTime(2000, 1, 1, 12, 0, 0), result[0].EndDate);
			Assert.Equal(new DateTime(2000, 1, 1, 12, 0, 0), result[1].StartDate);
			Assert.Equal(new DateTime(2000, 1, 1, 12, 1, 0), result[1].EndDate);
			Assert.True(result.All(x => ((PcWorkItem)x).ComputerId == 1234));
			Assert.True(result.All(x => x.UserId == 5));
			Assert.True(result.All(x => x.Duration == (x.EndDate - x.StartDate)));
			Assert.False(result[0].Values.ContainsKey("ProcessName"));
			Assert.False(result[0].Values.ContainsKey("Title"));
			Assert.Equal("something.exe", ((PcWorkItem)result[1]).Values["ProcessName"]);
			Assert.Equal("ACME app", ((PcWorkItem)result[1]).Values["Title"]);
			Assert.Equal(5, ((PcWorkItem)result[0]).KeyboardActivity);
			Assert.Equal(50, ((PcWorkItem)result[0]).MouseActivity);
			Assert.Equal(5, ((PcWorkItem)result[1]).KeyboardActivity);
			Assert.Equal(50, ((PcWorkItem)result[1]).MouseActivity);
		}

		[Fact]
		public void SplitCaseDeletion()
		{
			var items = new List<CollectedItem>
			{
				new ComputerCollectedItem()
				{
					ComputerId = 1234,
					CreateDate = new DateTime(2000, 1, 1, 12, 0, 0),
					Key = "ProcessName",
					Value = "something.exe",
					UserId = 5
				},
				new ComputerCollectedItem()
				{
					ComputerId = 1234,
					CreateDate = new DateTime(2000, 1, 1, 12, 0, 0),
					Key = "Title",
					Value = "ACME app",
					UserId = 5
				},
				new ComputerCollectedItem()
				{
					ComputerId = 1234,
					CreateDate = new DateTime(2000, 1, 1, 12, 2, 0),
					Key = "ProcessName",
					Value = "iexplorer.exe",
					UserId = 5
				},
				new ComputerCollectedItem()
				{
					ComputerId = 1234,
					CreateDate = new DateTime(2000, 1, 1, 12, 2, 0),
					Key = "Title",
					Value = "Internet explorer",
					UserId = 5
				},
			};
			var deletes = new List<WorkItemDeletion>()
			{
				new WorkItemDeletion()
				{
					Type = DeletionTypes.Computer,
					StartDate = new DateTime(2000, 1, 1, 12, 0, 0),
					EndDate = new DateTime(2000, 1, 1, 12, 0, 30),
					UserId = 5
				}
			};
			var workitems = new List<Reporter.Model.WorkItems.ComputerWorkItem>()
			{
				new Reporter.Model.WorkItems.ComputerWorkItem()
				{
					ComputerId = 1234, 
					StartDate = new DateTime(2000, 1, 1, 11, 59, 0),
					EndDate = new DateTime(2000, 1, 1, 12, 1, 0, 0),
					KeyboardActivity = 12,
					MouseActivity = 100,
					UserId = 5,
					WorkId = 1
				}
			};

			var result = ReportHelper.Transform(items, deletes, workitems).ToArray();
			Assert.Equal(2, result.Length);
			Assert.Equal(new DateTime(2000, 1, 1, 11, 59, 0), result[0].StartDate);
			Assert.Equal(new DateTime(2000, 1, 1, 12, 0, 0), result[0].EndDate);
			Assert.Equal(new DateTime(2000, 1, 1, 12, 0, 30), result[1].StartDate);
			Assert.Equal(new DateTime(2000, 1, 1, 12, 1, 0), result[1].EndDate);
			Assert.True(result.All(x => ((PcWorkItem)x).ComputerId == 1234));
			Assert.True(result.All(x => x.UserId == 5));
			Assert.True(result.All(x => x.Duration == (x.EndDate - x.StartDate)));
			Assert.False(result[0].Values.ContainsKey("ProcessName"));
			Assert.False(result[0].Values.ContainsKey("Title"));
			Assert.Equal("something.exe", result[1].Values["ProcessName"]);
			Assert.Equal("ACME app", result[1].Values["Title"]);
			Assert.Equal(6, ((PcWorkItem)result[0]).KeyboardActivity);
			Assert.Equal(50, ((PcWorkItem)result[0]).MouseActivity);
			Assert.Equal(3, ((PcWorkItem)result[1]).KeyboardActivity);
			Assert.Equal(25, ((PcWorkItem)result[1]).MouseActivity);
		}


		[Fact]
		public void MultpleUserCase()
		{
			var items = new List<CollectedItem>
			{
				new ComputerCollectedItem()
				{
					ComputerId = 1234,
					CreateDate = new DateTime(2000, 1, 1, 12, 0, 0),
					Key = "ProcessName",
					Value = "something.exe",
					UserId = 5
				},
				new ComputerCollectedItem()
				{
					ComputerId = 1234,
					CreateDate = new DateTime(2000, 1, 1, 12, 0, 0),
					Key = "Title",
					Value = "ACME app",
					UserId = 5
				},
				new ComputerCollectedItem()
				{
					ComputerId = 5678,
					CreateDate = new DateTime(2000, 1, 1, 12, 0, 0),
					Key = "ProcessName",
					Value = "iexplorer.exe",
					UserId = 10
				},
				new ComputerCollectedItem()
				{
					ComputerId = 5678,
					CreateDate = new DateTime(2000, 1, 1, 12, 0, 0),
					Key = "Title",
					Value = "Internet explorer",
					UserId = 10
				},
			};
			var deletes = new List<WorkItemDeletion>();
			var workitems = new List<Reporter.Model.WorkItems.ComputerWorkItem>()
			{
				new Reporter.Model.WorkItems.ComputerWorkItem()
				{
					ComputerId = 1234, 
					StartDate = new DateTime(2000, 1, 1, 11, 58, 0),
					EndDate = new DateTime(2000, 1, 1, 12, 2, 0, 0),
					KeyboardActivity = 10,
					MouseActivity = 100,
					UserId = 5,
					WorkId = 1
				},
				new Reporter.Model.WorkItems.ComputerWorkItem()
				{
					ComputerId = 5678, 
					StartDate = new DateTime(2000, 1, 1, 11, 58, 0),
					EndDate = new DateTime(2000, 1, 1, 12, 2, 0, 0),
					KeyboardActivity = 15,
					MouseActivity = 200,
					UserId = 10,
					WorkId = 1
				}
			};

			var result = ReportHelper.Transform(items, deletes, workitems).ToArray();
			Assert.Equal(4, result.Length);
			Assert.True(result.Any(x => ((PcWorkItem)x).ComputerId == 1234 && x.UserId == 5 && x.StartDate.Minute == 58 && x.EndDate.Minute == 0 && !x.Values.ContainsKey("Title") && !x.Values.ContainsKey("ProcessName")));
			Assert.True(result.Any(x => ((PcWorkItem)x).ComputerId == 1234 && x.UserId == 5 && x.StartDate.Minute == 0 && x.EndDate.Minute == 2 && x.Values["Title"] == "ACME app" && x.Values["ProcessName"] == "something.exe"));
			Assert.True(result.Any(x => ((PcWorkItem)x).ComputerId == 5678 && x.UserId == 10 && x.StartDate.Minute == 58 && x.EndDate.Minute == 0 && !x.Values.ContainsKey("Title") && !x.Values.ContainsKey("ProcessName")));
			Assert.True(result.Any(x => ((PcWorkItem)x).ComputerId == 5678 && x.UserId == 10 && x.StartDate.Minute == 0 && x.EndDate.Minute == 2 && x.Values["Title"] == "Internet explorer" && x.Values["ProcessName"] == "iexplorer.exe"));
			Assert.True(result.All(x => x.Duration == (x.EndDate - x.StartDate)));
		}

		[Fact]
		public void SimpleCase3()
		{
			var collItems = new List<ComputerCollectedItem>();
			var deletions = new List<WorkItemDeletion>();
			var workitems = new List<WorkItem>();

			workitems.Add(new ComputerWorkItem
			{
				UserId = 411,
				WorkId = 6689379,
				StartDate = new DateTime(2016, 4, 7, 8, 18, 20),
				EndDate = new DateTime(2016, 4, 7, 8, 51, 49),
				ComputerId = -482141750
			});

			workitems.Add(new ComputerWorkItem
			{
				UserId = 411,
				WorkId = 6689379,
				StartDate = new DateTime(2016, 4, 7, 9, 1, 50),
				EndDate = new DateTime(2016, 4, 7, 9, 32, 35),
				ComputerId = -482141750
			});

			workitems.Add(new ComputerWorkItem
			{
				UserId = 411,
				WorkId = 130,
				StartDate = new DateTime(2016, 4, 7, 9, 32, 35),
				EndDate = new DateTime(2016, 4, 7, 10, 4, 59),
				ComputerId = -482141750
			});

			workitems.Add(new ComputerWorkItem
			{
				UserId = 411,
				WorkId = 130,
				StartDate = new DateTime(2016, 4, 7, 10, 8, 44),
				EndDate = new DateTime(2016, 4, 7, 10, 11, 12, 47),
				ComputerId = -482141750
			});

			workitems.Add(new ComputerWorkItem
			{
				UserId = 411,
				WorkId = 121,
				StartDate = new DateTime(2016, 4, 7, 10, 11, 12, 47),
				EndDate = new DateTime(2016, 4, 7, 10, 12, 57),
				ComputerId = -482141750
			});

			workitems.Add(new ComputerWorkItem
			{
				UserId = 411,
				WorkId = 121,
				StartDate = new DateTime(2016, 4, 7, 10, 22, 58),
				EndDate = new DateTime(2016, 4, 7, 11, 1, 35),
				ComputerId = -482141750
			});

			workitems.Add(new ComputerWorkItem
			{
				UserId = 411,
				WorkId = 121,
				StartDate = new DateTime(2016, 4, 7, 11, 11, 36),
				EndDate = new DateTime(2016, 4, 7, 11, 22, 58),
				ComputerId = -482141750
			});

			workitems.Add(new ComputerWorkItem
			{
				UserId = 411,
				WorkId = 130,
				StartDate = new DateTime(2016, 4, 7, 11, 22, 58),
				EndDate = new DateTime(2016, 4, 7, 11, 31, 53),
				ComputerId = -482141750
			});

			deletions.Add(new WorkItemDeletion
			{
				UserId = 411,
				StartDate = new DateTime(2016, 4, 7, 9, 32, 35),
				EndDate = new DateTime(2016, 4, 7, 10, 4, 59),
				Type = DeletionTypes.All
			});

			deletions.Add(new WorkItemDeletion
			{
				UserId = 411,
				StartDate = new DateTime(2016, 4, 7, 10, 11, 12, 47),
				EndDate = new DateTime(2016, 4, 7, 10, 12, 57),
				Type = DeletionTypes.All
			});

			deletions.Add(new WorkItemDeletion
			{
				UserId = 411,
				StartDate = new DateTime(2016, 4, 7, 10, 22, 58),
				EndDate = new DateTime(2016, 4, 7, 11, 1, 35),
				Type = DeletionTypes.All
			});

			deletions.Add(new WorkItemDeletion
			{
				UserId = 411,
				StartDate = new DateTime(2016, 4, 7, 11, 11, 36),
				EndDate = new DateTime(2016, 4, 7, 11, 22, 58),
				Type = DeletionTypes.All
			});

			workitems.Add(new Reporter.Model.WorkItems.ManualWorkItem
			{
				UserId = 411,
				WorkId = 108,
				StartDate = new DateTime(2016, 4, 7, 8, 51, 49),
				EndDate = new DateTime(2016, 4, 7, 9, 1, 49),
			});

			workitems.Add(new Reporter.Model.WorkItems.ManualWorkItem
			{
				UserId = 411,
				WorkId = 108,
				StartDate = new DateTime(2016, 4, 7, 10, 12, 57),
				EndDate = new DateTime(2016, 4, 7, 10, 22, 57),
			});

			workitems.Add(new Reporter.Model.WorkItems.ManualWorkItem
			{
				UserId = 411,
				WorkId = 108,
				StartDate = new DateTime(2016, 4, 7, 11, 1, 35),
				EndDate = new DateTime(2016, 4, 7, 11, 11, 35),
			});

			var tups = ReportHelper.Transform(collItems, deletions, workitems).OrderBy(t => t.StartDate).ToList();
			Assert.Equal(7, tups.Count);
		}

        [Fact]
        public void ItWareMeetingBugfix()
        {
            var collItems = new List<ComputerCollectedItem>();
            var deletions = new List<WorkItemDeletion>();
            var workitems = new List<WorkItem>();

            #region 11 pcs of deletions 

            deletions.Add(new WorkItemDeletion
            {
                UserId = 65,
                StartDate = new DateTime(2017, 5, 29, 7, 39, 10, 530),
                EndDate = new DateTime(2017, 5, 29, 7, 49, 10, 530),
                Type = DeletionTypes.Computer
            });

            deletions.Add(new WorkItemDeletion
            {
                UserId = 65,
                StartDate = new DateTime(2017, 5, 29, 7, 39, 10, 530),
                EndDate = new DateTime(2017, 5, 29, 7, 54, 10, 530),
                Type = DeletionTypes.All
            });

            deletions.Add(new WorkItemDeletion
            {
                UserId = 65,
                StartDate = new DateTime(2017, 5, 29, 10, 34, 4, 843),
                EndDate = new DateTime(2017, 5, 29, 10, 44, 4, 843),
                Type = DeletionTypes.Computer
            });

            deletions.Add(new WorkItemDeletion
            {
                UserId = 65,
                StartDate = new DateTime(2017, 5, 29, 10, 34, 4, 843),
                EndDate = new DateTime(2017, 5, 29, 10, 50, 36, 983),
                Type = DeletionTypes.All
            });

            deletions.Add(new WorkItemDeletion
            {
                UserId = 65,
                StartDate = new DateTime(2017, 5, 29, 11, 30, 0, 0),
                EndDate = new DateTime(2017, 5, 29, 12, 30, 0, 0),
                Type = DeletionTypes.All
            });

            deletions.Add(new WorkItemDeletion
            {
                UserId = 65,
                StartDate = new DateTime(2017, 5, 29, 11, 49, 38, 857),
                EndDate = new DateTime(2017, 5, 29, 11, 59, 38, 857),
                Type = DeletionTypes.Computer
            });

            deletions.Add(new WorkItemDeletion
            {
                UserId = 65,
                StartDate = new DateTime(2017, 5, 29, 12, 0, 8, 873),
                EndDate = new DateTime(2017, 5, 29, 12, 0, 18, 873),
                Type = DeletionTypes.Computer
            });

            deletions.Add(new WorkItemDeletion
            {
                UserId = 65,
                StartDate = new DateTime(2017, 5, 29, 13, 29, 13, 547),
                EndDate = new DateTime(2017, 5, 29, 13, 39, 13, 547),
                Type = DeletionTypes.Computer
            });

            deletions.Add(new WorkItemDeletion
            {
                UserId = 65,
                StartDate = new DateTime(2017, 5, 29, 13, 29, 13, 547),
                EndDate = new DateTime(2017, 5, 29, 13, 41, 7, 453),
                Type = DeletionTypes.All
            });

            deletions.Add(new WorkItemDeletion
            {
                UserId = 65,
                StartDate = new DateTime(2017, 5, 29, 13, 50, 44, 597),
                EndDate = new DateTime(2017, 5, 29, 14, 0, 44, 597),
                Type = DeletionTypes.Computer
            });

            deletions.Add(new WorkItemDeletion
            {
                UserId = 65,
                StartDate = new DateTime(2017, 5, 29, 13, 50, 44, 597),
                EndDate = new DateTime(2017, 5, 29, 14, 4, 55, 33),
                Type = DeletionTypes.All
            });

            #endregion

            #region 16 pcs of Pc worktime

            workitems.Add(new ComputerWorkItem
            {
                UserId = 65,
                WorkId = 3609152,
                StartDate = new DateTime(2017, 5, 29, 4, 46, 43, 20),
                EndDate = new DateTime(2017, 5, 29, 5, 6, 20, 490),
                ComputerId = -1913942103
            });

            workitems.Add(new ComputerWorkItem
            {
                UserId = 65,
                WorkId = 6771273,
                StartDate = new DateTime(2017, 5, 29, 5, 6, 20, 490),
                EndDate = new DateTime(2017, 5, 29, 5, 59, 30, 287),
                ComputerId = -1913942103
            });

            workitems.Add(new ComputerWorkItem
            {
                UserId = 65,
                WorkId = 3609152,
                StartDate = new DateTime(2017, 5, 29, 5, 59, 30, 287),
                EndDate = new DateTime(2017, 5, 29, 6, 0, 12, 193),
                ComputerId = -1913942103
            });

            workitems.Add(new ComputerWorkItem
            {
                UserId = 65,
                WorkId = 3609152,
                StartDate = new DateTime(2017, 5, 29, 6, 20, 33, 470),
                EndDate = new DateTime(2017, 5, 29, 7, 49, 10, 393),
                ComputerId = -1913942103
            });

            workitems.Add(new ComputerWorkItem
            {
                UserId = 65,
                WorkId = 3609152,
                StartDate = new DateTime(2017, 5, 29, 8, 4, 42, 683),
                EndDate = new DateTime(2017, 5, 29, 9, 29, 57, 577),
                ComputerId = -1913942103
            });

            workitems.Add(new ComputerWorkItem
            {
                UserId = 65,
                WorkId = 3609152,
                StartDate = new DateTime(2017, 5, 29, 10, 31, 5, 203),
                EndDate = new DateTime(2017, 5, 29, 10, 44, 5, 47),
                ComputerId = -1913942103
            });

            workitems.Add(new ComputerWorkItem
            {
                UserId = 65,
                WorkId = 3609152,
                StartDate = new DateTime(2017, 5, 29, 10, 50, 36, 980),
                EndDate = new DateTime(2017, 5, 29, 11, 25, 33, 887),
                ComputerId = -1913942103
            });

            workitems.Add(new ComputerWorkItem
            {
                UserId = 65,
                WorkId = 16745,
                StartDate = new DateTime(2017, 5, 29, 11, 25, 33, 887),
                EndDate = new DateTime(2017, 5, 29, 11, 59, 38, 730),
                ComputerId = -1913942103
            });

            workitems.Add(new ComputerWorkItem
            {
                UserId = 65,
                WorkId = 16745,
                StartDate = new DateTime(2017, 5, 29, 11, 59, 48, 247),
                EndDate = new DateTime(2017, 5, 29, 12, 10, 8, 747),
                ComputerId = -1913942103
            });

            workitems.Add(new ComputerWorkItem
            {
                UserId = 65,
                WorkId = 16745,
                StartDate = new DateTime(2017, 5, 29, 12, 10, 15, 573),
                EndDate = new DateTime(2017, 5, 29, 12, 10, 23, 913),
                ComputerId = -1913942103
            });

            workitems.Add(new ComputerWorkItem
            {
                UserId = 65,
                WorkId = 6771273,
                StartDate = new DateTime(2017, 5, 29, 12, 10, 23, 913),
                EndDate = new DateTime(2017, 5, 29, 12, 33, 48, 523),
                ComputerId = -1913942103
            });

            workitems.Add(new ComputerWorkItem
            {
                UserId = 65,
                WorkId = 3609152,
                StartDate = new DateTime(2017, 5, 29, 12, 33, 48, 523),
                EndDate = new DateTime(2017, 5, 29, 13, 39, 13, 463),
                ComputerId = -1913942103
            });

            workitems.Add(new ComputerWorkItem
            {
                UserId = 65,
                WorkId = 3609152,
                StartDate = new DateTime(2017, 5, 29, 13, 41, 7, 480),
                EndDate = new DateTime(2017, 5, 29, 14, 0, 44, 493),
                ComputerId = -1913942103
            });

            workitems.Add(new ComputerWorkItem
            {
                UserId = 65,
                WorkId = 3609152,
                StartDate = new DateTime(2017, 5, 29, 14, 4, 55, 47),
                EndDate = new DateTime(2017, 5, 29, 14, 13, 36, 157),
                ComputerId = -1913942103
            });

            workitems.Add(new ComputerWorkItem
            {
                UserId = 65,
                WorkId = 3609152,
                StartDate = new DateTime(2017, 5, 29, 17, 18, 18, 330),
                EndDate = new DateTime(2017, 5, 29, 17, 34, 43, 783),
                ComputerId = -1913942103
            });

            workitems.Add(new ComputerWorkItem
            {
                UserId = 65,
                WorkId = 3609152,
                StartDate = new DateTime(2017, 5, 29, 17, 38, 42, 380),
                EndDate = new DateTime(2017, 5, 29, 18, 44, 25, 70),
                ComputerId = -1913942103
            });

            #endregion

            #region 5 pcs of Meetings

            workitems.Add(new Reporter.Model.WorkItems.AdhocMeetingWorkItem
            {
                UserId = 65,
                WorkId = 3609152,
                StartDate = new DateTime(2017, 5, 29, 7, 39, 10, 530),
                EndDate = new DateTime(2017, 5, 29, 7, 54, 10, 530)
            });

            workitems.Add(new Reporter.Model.WorkItems.AdhocMeetingWorkItem
            {
                UserId = 65,
                WorkId = 3609152,
                StartDate = new DateTime(2017, 5, 29, 10, 34, 4, 843),
                EndDate = new DateTime(2017, 5, 29, 10, 50, 36, 983)
            });

            workitems.Add(new Reporter.Model.WorkItems.AdhocMeetingWorkItem
            {
                UserId = 65,
                WorkId = 3609152,
                StartDate = new DateTime(2017, 5, 29, 13, 29, 13, 547),
                EndDate = new DateTime(2017, 5, 29, 13, 41, 7, 453)
            });

            workitems.Add(new Reporter.Model.WorkItems.AdhocMeetingWorkItem
            {
                UserId = 65,
                WorkId = 3609152,
                StartDate = new DateTime(2017, 5, 29, 13, 50, 44, 597),
                EndDate = new DateTime(2017, 5, 29, 14, 4, 55, 33)
            });

            workitems.Add(new Reporter.Model.WorkItems.CalendarMeetingWorkItem
            {
                UserId = 65,
                WorkId = 6771273,
                StartDate = new DateTime(2017, 5, 29, 11, 30, 0),
                EndDate = new DateTime(2017, 5, 29, 12, 30, 0),
                Description = @"Sziasztok!\r\n\r\n \r\n\r\nKüldöm a heti rendszeres PO/SM vezetői prezentációra vonatkozó meeting requestet.\r\n\r\n \r\n\r\nKérem az aktuális riportotokat legkésőbb hétfő 11:00 óráig írjátok meg confluenceben és küldjétek el csatolt dokumentumként is a managers@itware.hu <mailto:managers@itware.hu>  címre.\r\n\r\n \r\n\r\nA riport tartalmazza minden esetben:\r\n\r\n-          projekt életképesség mutatókat (idő, budget, szkóp)\r\n\r\n-          projekt kontrollingot jobctrl alapján\r\n\r\n-          projekt főbb mérföldköveit a teljes projekt időtartamra vonatkozóan és azok státuszát\r\n\r\n-          projekt közeljövő almérföldköveit és azok státuszát\r\n\r\n-          projekt főbb leszállítandóit és azok státuszát a teljes projekt szkópra vonatkozóan\r\n\r\n-          előző sprint retrospective összefoglalóját\r\n\r\n-          aktuális sprint státusza (PO: backlogból inszkópolt user storyk és azok státusza, burndown chart; SM: scrum, impedimentek, csapat)\r\n\r\n-          következő sprint előkészítésének státusza\r\n\r\n-          (pre)sal",
                Participants = @"Gabor.Hajdu@itware.hu;Tamas.Frivalszki@itware.hu;Tamas.Jakli@itware.hu;tamas.csikasz@itware.hu;Zsolt.Molnar@itware.hu;rezso.barna@itware.hu;laszlo.szilagyi@itware.hu;robert.szekeres@itware.hu;sandor.danko@itware.hu;karoly.hajdu@itware.hu",
                Title = @"Heti PO/SM vezetői prezentáció"
            });

            #endregion

            #region 1 pcs of ManualWorkitems

            workitems.Add(new Reporter.Model.WorkItems.ManualWorkItem
            {
                UserId = 65,
                WorkId = 3609152,
                StartDate = new DateTime(2017, 5, 29, 16, 40, 0),
                EndDate = new DateTime(2017, 5, 29, 17, 10, 0),
            });

            #endregion

            var tups = ReportHelper.Transform(collItems, deletions, workitems).OrderBy(t => t.StartDate).ToList();
            var totalDuration = TimeSpan.Zero;
            foreach (var duration in tups.Select(t => t.Duration))
                totalDuration += duration;

            Assert.Null(tups.Find(t => t.Type == ItemType.Pc && t.StartDate == new DateTime(2017, 5, 29, 12, 10, 23, 913)));
            Assert.Equal(new TimeSpan(0, 9, 47, 0, 183), totalDuration);
        }
    }
}
