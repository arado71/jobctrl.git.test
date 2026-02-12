using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderService;
using Tct.ActivityRecorderService.Meeting;
using Tct.ActivityRecorderService.Persistence;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class DeadLetterTests : DbTestsBase
	{
		private static readonly DateTime now = new DateTime(2013, 07, 04, 15, 00, 00);

		[Fact]
		public void AllExistingSaveMethodsTested()
		{
			Assert.Equal(this.GetType().GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).Where(n => n.Name.StartsWith("CanSerialize")).Count(),
				typeof(DeadLetterHelper).GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).Where(n => n.Name == "TrySaveItem").Count());
		}

		[Fact]
		public void CanSerializeWorkItem()
		{
			//Arrange
			var item = new WorkItem()
			{
				UserId = 1,
				WorkId = 2,
				StartDate = now,
				EndDate = now.AddMinutes(5),
				MouseActivity = 3,
				KeyboardActivity = 4,
				PhaseId = new Guid("11111111-1111-1111-1111-111111111111"),
				IsRemoteDesktop = true,
				IsVirtualMachine = true,
				DesktopCaptures = new List<DesktopCapture>()
				{
					new DesktopCapture() {
						Screens = new List<Screen>()
						{
							new Screen() { CreateDate = now.AddTicks(5), ScreenNumber = 2, X = 3, Y = 4 , Width = 5, Height = 6,}
						},
					}
				}
			};

			//Act
			string path = null;
			var data = new MyMemoryStream();
			DeadLetterHelper.FileWriteStreamFactory = p => { path = p; return data; };
			Assert.True(DeadLetterHelper.TrySaveItem(item, new Exception()));
			Assert.NotNull(path);
			data.Position = 0;
			DeadLetterHelper.FileReadStreamFactory = p => { Assert.Equal(path, p); return data; };
			WorkItem loaded;
			Assert.True(DeadLetterHelper.TryLoadItem(path, out loaded));
			data.DisposeForReal();
			DeadLetterItem dbData;
			using (var context = new AggregateDataClassesDataContext())
			{
				dbData = context.DeadLetterItems.Single();
			}

			//Assert
			Assert.Equal(dbData.UserId, loaded.UserId);
			Assert.Equal(dbData.WorkId, loaded.WorkId);
			Assert.Equal(dbData.StartDate, loaded.StartDate);
			Assert.Equal(dbData.EndDate, loaded.EndDate);
			Assert.False(Object.ReferenceEquals(item, loaded));
			Assert.Equal(item.UserId, loaded.UserId);
			Assert.Equal(item.WorkId, loaded.WorkId);
			Assert.Equal(item.StartDate, loaded.StartDate);
			Assert.Equal(item.EndDate, loaded.EndDate);
			Assert.Equal(item.MouseActivity, loaded.MouseActivity);
			Assert.Equal(item.KeyboardActivity, loaded.KeyboardActivity);
			Assert.Equal(item.PhaseId, loaded.PhaseId);
			Assert.Equal(item.IsRemoteDesktop, loaded.IsRemoteDesktop);
			Assert.Equal(item.IsVirtualMachine, loaded.IsVirtualMachine);
			Assert.Equal(item.DesktopCaptures[0].Screens[0].X, loaded.DesktopCaptures[0].Screens[0].X);
			Assert.Equal(item.DesktopCaptures[0].Screens[0].Y, loaded.DesktopCaptures[0].Screens[0].Y);
			Assert.Equal(item.DesktopCaptures[0].Screens[0].Width, loaded.DesktopCaptures[0].Screens[0].Width);
			Assert.Equal(item.DesktopCaptures[0].Screens[0].Height, loaded.DesktopCaptures[0].Screens[0].Height);
			Assert.Equal(item.DesktopCaptures[0].Screens[0].CreateDate, loaded.DesktopCaptures[0].Screens[0].CreateDate);
			Assert.Equal(item.DesktopCaptures[0].Screens[0].ScreenNumber, loaded.DesktopCaptures[0].Screens[0].ScreenNumber);
		}

		[Fact]
		public void CanSerializeManualWorkItem()
		{
			//Arrange
			var item = new ManualWorkItem()
			{
				UserId = 1,
				WorkId = 2,
				StartDate = now,
				EndDate = now.AddMinutes(5),
				Comment = "asd",
				ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
				OriginalEndDate = now.AddMinutes(10),
			};

			//Act
			string path = null;
			var data = new MyMemoryStream();
			DeadLetterHelper.FileWriteStreamFactory = p => { path = p; return data; };
			Assert.True(DeadLetterHelper.TrySaveItem(item, new Exception()));
			Assert.NotNull(path);
			data.Position = 0;
			DeadLetterHelper.FileReadStreamFactory = p => { Assert.Equal(path, p); return data; };
			ManualWorkItem loaded;
			Assert.True(DeadLetterHelper.TryLoadItem(path, out loaded));
			data.DisposeForReal();
			DeadLetterItem dbData;
			using (var context = new AggregateDataClassesDataContext())
			{
				dbData = context.DeadLetterItems.Single();
			}

			//Assert
			Assert.Equal(dbData.UserId, loaded.UserId);
			Assert.Equal(dbData.WorkId, loaded.WorkId);
			Assert.Equal(dbData.StartDate, loaded.StartDate);
			Assert.Equal(dbData.EndDate, loaded.EndDate);
			Assert.False(Object.ReferenceEquals(item, loaded));
			Assert.Equal(item.UserId, loaded.UserId);
			Assert.Equal(item.WorkId, loaded.WorkId);
			Assert.Equal(item.StartDate, loaded.StartDate);
			Assert.Equal(item.EndDate, loaded.EndDate);
			Assert.Equal(item.Comment, loaded.Comment);
			Assert.Equal(item.ManualWorkItemTypeId, loaded.ManualWorkItemTypeId);
			Assert.Equal(item.OriginalEndDate, loaded.OriginalEndDate);
		}

		[Fact]
		public void CanSerializeParallelWorkItem()
		{
			//Arrange
			var item = new ParallelWorkItem()
			{
				UserId = 1,
				WorkId = 2,
				StartDate = now,
				EndDate = now.AddMinutes(5),
				ParallelWorkItemTypeId = ParallelWorkItemTypeEnum.IEBusy,
			};

			//Act
			string path = null;
			var data = new MyMemoryStream();
			DeadLetterHelper.FileWriteStreamFactory = p => { path = p; return data; };
			Assert.True(DeadLetterHelper.TrySaveItem(item, new Exception()));
			Assert.NotNull(path);
			data.Position = 0;
			DeadLetterHelper.FileReadStreamFactory = p => { Assert.Equal(path, p); return data; };
			ParallelWorkItem loaded;
			Assert.True(DeadLetterHelper.TryLoadItem(path, out loaded));
			data.DisposeForReal();
			DeadLetterItem dbData;
			using (var context = new AggregateDataClassesDataContext())
			{
				dbData = context.DeadLetterItems.Single();
			}

			//Assert
			Assert.Equal(dbData.UserId, loaded.UserId);
			Assert.Equal(dbData.WorkId, loaded.WorkId);
			Assert.Equal(dbData.StartDate, loaded.StartDate);
			Assert.Equal(dbData.EndDate, loaded.EndDate);
			Assert.False(Object.ReferenceEquals(item, loaded));
			Assert.Equal(item.UserId, loaded.UserId);
			Assert.Equal(item.WorkId, loaded.WorkId);
			Assert.Equal(item.StartDate, loaded.StartDate);
			Assert.Equal(item.EndDate, loaded.EndDate);
			Assert.Equal(item.ParallelWorkItemTypeId, loaded.ParallelWorkItemTypeId);
		}

		[Fact]
		public void CanSerializeManualMeetingDataDead()
		{
			//Arrange
			var item = new ManualMeetingDataDead()
			{
				UserId = 1,
				ManualMeetingData = new ManualMeetingData()
				{
					WorkId = 2,
					StartTime = now,
					EndTime = now.AddMinutes(5),
					Title = "asd",
					AttendeeEmails = new List<string>() { "asd@ad.2s", "33@ret.rT" },
					IncludedIdleMinutes = 2,
					Description = "as",
					Location = "sd",
					OnGoing = true,
					OriginalStartTime = now.AddYears(-1),
				}
			};

			//Act
			string path = null;
			var data = new MyMemoryStream();
			DeadLetterHelper.FileWriteStreamFactory = p => { path = p; return data; };
			Assert.True(DeadLetterHelper.TrySaveItem(item, new Exception()));
			Assert.NotNull(path);
			data.Position = 0;
			DeadLetterHelper.FileReadStreamFactory = p => { Assert.Equal(path, p); return data; };
			ManualMeetingDataDead loaded;
			Assert.True(DeadLetterHelper.TryLoadItem(path, out loaded));
			data.DisposeForReal();
			DeadLetterItem dbData;
			using (var context = new AggregateDataClassesDataContext())
			{
				dbData = context.DeadLetterItems.Single();
			}

			//Assert
			Assert.Equal(dbData.UserId, loaded.UserId);
			Assert.Equal(dbData.WorkId, loaded.ManualMeetingData.WorkId);
			Assert.Equal(dbData.StartDate, loaded.ManualMeetingData.StartTime);
			Assert.Equal(dbData.EndDate, loaded.ManualMeetingData.EndTime);
			Assert.False(Object.ReferenceEquals(item, loaded));
			Assert.Equal(item.UserId, loaded.UserId);
			Assert.Equal(item.ManualMeetingData.WorkId, loaded.ManualMeetingData.WorkId);
			Assert.Equal(item.ManualMeetingData.StartTime, loaded.ManualMeetingData.StartTime);
			Assert.Equal(item.ManualMeetingData.EndTime, loaded.ManualMeetingData.EndTime);
			Assert.Equal(item.ManualMeetingData.Title, loaded.ManualMeetingData.Title);
			Assert.Equal(item.ManualMeetingData.IncludedIdleMinutes, loaded.ManualMeetingData.IncludedIdleMinutes);
			Assert.Equal(item.ManualMeetingData.Description, loaded.ManualMeetingData.Description);
			Assert.Equal(item.ManualMeetingData.Location, loaded.ManualMeetingData.Location);
			Assert.Equal(item.ManualMeetingData.OnGoing, loaded.ManualMeetingData.OnGoing);
			Assert.Equal(item.ManualMeetingData.OriginalStartTime, loaded.ManualMeetingData.OriginalStartTime);
			Assert.True(item.ManualMeetingData.AttendeeEmails.SequenceEqual(loaded.ManualMeetingData.AttendeeEmails));
		}

		[Fact]
		public void CanSerializeFinishedMeetingEntryDead()
		{
			//Arrange
			var item = new FinishedMeetingEntryDead()
			{
				UserId = 1,
				ComputerId = 2,
				FinishedMeetingEntry = new FinishedMeetingEntry()
				{
					StartTime = now,
					EndTime = now.AddMinutes(5),
					Title = "asd",
					Description = "as",
					Location = "sd",
					Attendees = new List<MeetingAttendee>()
					{
						new MeetingAttendee() { Email = "asr@we", ResponseStatus = MeetingAttendeeResponseStatus.ResponseDeclined, Type = MeetingAttendeeType.Organizer },
					},
					CreationTime = now.AddMinutes(6),
					Id = "a324",
					LastmodificationTime = now.AddMinutes(7),
				}
			};

			//Act
			string path = null;
			var data = new MyMemoryStream();
			DeadLetterHelper.FileWriteStreamFactory = p => { path = p; return data; };
			Assert.True(DeadLetterHelper.TrySaveItem(item, new Exception()));
			Assert.NotNull(path);
			data.Position = 0;
			DeadLetterHelper.FileReadStreamFactory = p => { Assert.Equal(path, p); return data; };
			FinishedMeetingEntryDead loaded;
			Assert.True(DeadLetterHelper.TryLoadItem(path, out loaded));
			data.DisposeForReal();
			DeadLetterItem dbData;
			using (var context = new AggregateDataClassesDataContext())
			{
				dbData = context.DeadLetterItems.Single();
			}

			//Assert
			Assert.Equal(dbData.UserId, loaded.UserId);
			Assert.Equal(2, loaded.ComputerId);
			Assert.Equal(dbData.StartDate, loaded.FinishedMeetingEntry.StartTime);
			Assert.Equal(dbData.EndDate, loaded.FinishedMeetingEntry.EndTime);
			Assert.False(Object.ReferenceEquals(item, loaded));
			Assert.Equal(item.UserId, loaded.UserId);
			Assert.Equal(item.FinishedMeetingEntry.StartTime, loaded.FinishedMeetingEntry.StartTime);
			Assert.Equal(item.FinishedMeetingEntry.EndTime, loaded.FinishedMeetingEntry.EndTime);
			Assert.Equal(item.FinishedMeetingEntry.Title, loaded.FinishedMeetingEntry.Title);
			Assert.Equal(item.FinishedMeetingEntry.Description, loaded.FinishedMeetingEntry.Description);
			Assert.Equal(item.FinishedMeetingEntry.Location, loaded.FinishedMeetingEntry.Location);
			Assert.Equal(item.FinishedMeetingEntry.Attendees.Count, loaded.FinishedMeetingEntry.Attendees.Count);
			Assert.Equal(item.FinishedMeetingEntry.Attendees[0].Email, loaded.FinishedMeetingEntry.Attendees[0].Email);
			Assert.Equal(item.FinishedMeetingEntry.Attendees[0].ResponseStatus, loaded.FinishedMeetingEntry.Attendees[0].ResponseStatus);
			Assert.Equal(item.FinishedMeetingEntry.Attendees[0].Type, loaded.FinishedMeetingEntry.Attendees[0].Type);
			Assert.Equal(item.FinishedMeetingEntry.CreationTime, loaded.FinishedMeetingEntry.CreationTime);
			Assert.Equal(item.FinishedMeetingEntry.Id, loaded.FinishedMeetingEntry.Id);
			Assert.Equal(item.FinishedMeetingEntry.LastmodificationTime, loaded.FinishedMeetingEntry.LastmodificationTime);
		}

		[Fact]
		public void WontSaveToDbOnError()
		{
			//Arrange
			var item = new ParallelWorkItem()
			{
				UserId = 1,
				WorkId = 2,
				StartDate = now,
				EndDate = now.AddMinutes(5),
				ParallelWorkItemTypeId = ParallelWorkItemTypeEnum.IEBusy,
			};
			DeadLetterHelper.FileWriteStreamFactory = p => { throw new Exception("Error while saving."); };

			//Act
			Assert.False(DeadLetterHelper.TrySaveItem(item, new Exception()));

			//Assert
			using (var context = new AggregateDataClassesDataContext())
			{
				Assert.False(context.DeadLetterItems.Any());
			}
		}

		private class MyMemoryStream : MemoryStream
		{
			protected override void Dispose(bool disposing)
			{
				return;
			}

			public void DisposeForReal()
			{
				base.Dispose(true);
			}
		}
	}
}
