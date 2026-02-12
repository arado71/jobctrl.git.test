using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Tct.ActivityRecorderService;
using Tct.ActivityRecorderService.MeetingSync;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class GoogleCalendarSyncTests : DbTestsBase
	{
		private Mock<IGoogleCalendarSource> calendarSourceMock;
		private static readonly string firstSyncToken = null;
		private static readonly string nextSyncToken = "xyz1";
		private static readonly DateTime eventsAfter = new DateTime(2018, 1, 1);
		private static readonly DateTime syncTime = new DateTime(2018, 1, 1, 12, 0, 0);
		private MeetingSyncManager meetingSync;
		private static readonly string authToken = "authToken1";
		private static readonly List<FinishedMeetingEntry> meetingEntries = new List<FinishedMeetingEntry>()
		{
			new FinishedMeetingEntry() { StartTime = eventsAfter.AddDays(1), EndTime = eventsAfter.AddDays(1).AddHours(1), Id = Guid.NewGuid().ToString(), Status = MeetingCrudStatus.Created, CreationTime = eventsAfter, LastmodificationTime = eventsAfter },
			new FinishedMeetingEntry() { StartTime = eventsAfter.AddDays(2), EndTime = eventsAfter.AddDays(2).AddHours(1), Id = Guid.NewGuid().ToString(), Status = MeetingCrudStatus.Created, CreationTime = eventsAfter, LastmodificationTime = eventsAfter },
			new FinishedMeetingEntry() { StartTime = eventsAfter.AddDays(3), EndTime = eventsAfter.AddDays(3).AddHours(1), Id = Guid.NewGuid().ToString(), Status = MeetingCrudStatus.Created, CreationTime = eventsAfter, LastmodificationTime = eventsAfter },
		};

		private delegate void GetEventsCallback(int userId, ref string syncToken, DateTime eventsAfter, bool needTentativeMeetings);

		public GoogleCalendarSyncTests()
		{
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				context.Client_SetCloudToken(1, authToken);
				context.ClientSettings.InsertOnSubmit(new ClientSetting(){UserId = 1, IsGoogleCalendarTrackingEnabled = true});
				context.SubmitChanges();
			}
		}

		[Fact]
		public void NoMeetingsInCalendar()
		{
			calendarSourceMock = new Mock<IGoogleCalendarSource>();
			var firstSyncTokenInst = firstSyncToken;
			calendarSourceMock.Setup(c => c.GetEvents(1, ref firstSyncTokenInst, It.IsAny<DateTime>(), true))
				.Returns(new List<FinishedMeetingEntry>());
			meetingSync = new MeetingSyncManager(calendarSourceMock.Object);

			var cnts = meetingSync.PerformSync(syncTime);
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				var eventDatesCount = context.ClientUserCloudEventDates.Count();

				Assert.Equal(1, cnts.Count);
				Assert.Equal(0, cnts[0]?.Count ?? 0);
				Assert.Equal(0, eventDatesCount);
			}
		}

		[Fact]
		public void SimpleSync()
		{
			calendarSourceMock = new Mock<IGoogleCalendarSource>();
			var firstSyncTokenInst = firstSyncToken;
			calendarSourceMock.Setup(c => c.GetEvents(1, ref firstSyncTokenInst, It.IsAny<DateTime>(), true))
				.Returns(meetingEntries);
			meetingSync = new MeetingSyncManager(calendarSourceMock.Object);

			var cnts = meetingSync.PerformSync(syncTime);
			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				var eventDatesCount = context.ClientUserCloudEventDates.Count();

				Assert.Equal(1, cnts.Count);
				var meetings = cnts[0];
				Assert.Equal(3, meetings?.Count ?? 0);
				Assert.Equal(meetingEntries[0].Id, meetings[0].Id);
				Assert.Equal(MeetingCrudStatus.Created, meetings[0].Status);
				Assert.Equal(3, eventDatesCount);

				var eventDateItem = context.ClientUserCloudEventDates.Where(e => e.EventId == meetings[0].Id).First();
				Assert.Equal(meetingEntries[0].StartTime, eventDateItem.StartTime);
			}
		}

		[Fact]
		public void MeetingUpdate()
		{
			calendarSourceMock = new Mock<IGoogleCalendarSource>();

			var meetingEntriesUpdated = meetingEntries.ToList(); // copy
			meetingEntriesUpdated[0] = new FinishedMeetingEntry()
			{
				StartTime = meetingEntries[0].StartTime.AddHours(10),
				EndTime = meetingEntries[0].EndTime,
				Id = meetingEntries[0].Id,
				CreationTime = meetingEntries[0].CreationTime.AddHours(1),
				LastmodificationTime = meetingEntries[0].LastmodificationTime.AddHours(1),
				Status = MeetingCrudStatus.Created,
			};

			calendarSourceMock.Setup(c => c.GetEvents(1, ref It.Ref<string>.IsAny, It.IsAny<DateTime>(), true))
				.Returns((int userId, string syncToken, DateTime after, bool needTentativeMeetings) => syncToken == firstSyncToken ? meetingEntries : meetingEntriesUpdated)
				.Callback(new GetEventsCallback((int userId, ref string syncToken, DateTime after, bool needTentativeMeetings) => { if (syncToken == firstSyncToken) syncToken = nextSyncToken; }));

			meetingSync = new MeetingSyncManager(calendarSourceMock.Object);

			meetingSync.PerformSync(syncTime); // first upload from meetingEntries
			var cnts = meetingSync.PerformSync(syncTime.AddHours(1)); // perform update with meetingEntriesUpdated

			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				var eventDatesCount = context.ClientUserCloudEventDates.Count();

				Assert.Equal(1, cnts.Count);
				var meetings = cnts[0];
				Assert.Equal(3, meetings?.Count ?? 0);
				Assert.Equal(meetingEntries[0].Id, meetings[0].Id);
				Assert.Equal(MeetingCrudStatus.Updated, meetings[0].Status);
				Assert.Equal(3, eventDatesCount);

				var eventDateItem = context.ClientUserCloudEventDates.Where(e => e.EventId == meetings[0].Id).First();
				Assert.Equal(meetingEntriesUpdated[0].StartTime, eventDateItem.StartTime);
			}
		}

		[Fact]
		public void MeetingDelete()
		{
			calendarSourceMock = new Mock<IGoogleCalendarSource>();

			var meetingEntriesUpdated = meetingEntries.ToList(); // copy
			meetingEntriesUpdated[0] = new FinishedMeetingEntry()
			{
				StartTime = meetingEntries[0].StartTime,
				EndTime = meetingEntries[0].EndTime,
				Id = meetingEntries[0].Id,
				CreationTime = meetingEntries[0].CreationTime,
				LastmodificationTime = meetingEntries[0].LastmodificationTime,
				Status = MeetingCrudStatus.Deleted,
			};

			calendarSourceMock.Setup(c => c.GetEvents(1, ref It.Ref<string>.IsAny, It.IsAny<DateTime>(), true))
				.Returns((int userId, string syncToken, DateTime after, bool needTentativeMeetings) => syncToken == firstSyncToken ? meetingEntries : meetingEntriesUpdated)
				.Callback(new GetEventsCallback((int userId, ref string syncToken, DateTime after, bool needTentativeMeetings) => { if (syncToken == firstSyncToken) syncToken = nextSyncToken; }));

			meetingSync = new MeetingSyncManager(calendarSourceMock.Object);

			meetingSync.PerformSync(syncTime); // first upload from meetingEntries
			var cnts = meetingSync.PerformSync(syncTime.AddHours(1)); // perform update with meetingEntriesUpdated

			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				var eventDatesCount = context.ClientUserCloudEventDates.Count();

				Assert.Equal(1, cnts.Count);
				var meetings = cnts[0];
				Assert.Equal(3, meetings?.Count ?? 0);
				Assert.Equal(meetingEntries[0].Id, meetings[0].Id);
				Assert.Equal(MeetingCrudStatus.Deleted, meetings[0].Status);
				Assert.Equal(2, eventDatesCount);

				var eventDateItem = context.ClientUserCloudEventDates.Where(e => e.EventId == meetings[0].Id).FirstOrDefault();
				Assert.Null(eventDateItem);
			}
		}

		[Fact]
		public void BackwardSync()
		{
			calendarSourceMock = new Mock<IGoogleCalendarSource>();

			calendarSourceMock.Setup(c => c.GetEvents(1, ref It.Ref<string>.IsAny, It.IsAny<DateTime>(), true))
				.Returns((int userId, string syncToken, DateTime after, bool needTentativeMeetings) => syncToken == firstSyncToken ? meetingEntries.Where(m => m.StartTime >= after).ToList() : null)
				.Callback(new GetEventsCallback((int userId, ref string syncToken, DateTime after, bool needTentativeMeetings) => { if (syncToken == firstSyncToken) syncToken = nextSyncToken; }));

			meetingSync = new MeetingSyncManager(calendarSourceMock.Object);

			meetingSync.PerformSync(syncTime); // first upload 
			meetingSync.LastSuccessfulSyncDebug = meetingEntries[0].StartTime.AddHours(1);
			var cnts = meetingSync.PerformSync(syncTime.AddHours(1)); // perform update backward

			using (var context = new ActivityRecorderDataClassesDataContext())
			{
				var eventDatesCount = context.ClientUserCloudEventDates.Count();

				Assert.Equal(1, cnts.Count);
				var meetings = cnts[0];
				Assert.Equal(2, meetings?.Count ?? 0);
				Assert.Equal(meetingEntries[1].Id, meetings[0].Id);
				Assert.Equal(MeetingCrudStatus.Created, meetings[0].Status);
				Assert.Equal(3, eventDatesCount);

				var eventDateItem = context.ClientUserCloudEventDates.Where(e => e.EventId == meetings[0].Id).First();
				Assert.Equal(meetingEntries[1].StartTime, eventDateItem.StartTime);
			}
		}
	}
}
