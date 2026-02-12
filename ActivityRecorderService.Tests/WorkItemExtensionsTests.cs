using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderService;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	//very incomplete...
	public class WorkItemExtensionsTests
	{
		private static readonly DateTime now = new DateTime(2013, 01, 02, 12, 00, 00);

		[Fact]
		public void DeleteCompStart()
		{
			//Arrange
			var del = new List<ManualWorkItem>()
			{
				new ManualWorkItem() { ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteComputerInterval, StartDate = now, EndDate = now.AddHours(1) },
			};
			var data = new List<WorkItem>()
			{
				new WorkItem() { Id = 1, StartDate = now, EndDate = now.AddHours(2) },
			};

			//Act
			data.DeleteIntervals(del);

			//Assert
			Assert.Equal(1, data.Count);
			Assert.Equal(now.AddHours(1), data[0].StartDate);
			Assert.Equal(now.AddHours(2), data[0].EndDate);
		}

		[Fact]
		public void DeleteCompEnd()
		{
			//Arrange
			var del = new List<ManualWorkItem>()
			{
				new ManualWorkItem() { ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteComputerInterval, StartDate = now.AddHours(1), EndDate = now.AddHours(2) },
			};
			var data = new List<WorkItem>()
			{
				new WorkItem() { Id = 1, StartDate = now, EndDate = now.AddHours(2) },
			};

			//Act
			data.DeleteIntervals(del);

			//Assert
			Assert.Equal(1, data.Count);
			Assert.Equal(now.AddHours(0), data[0].StartDate);
			Assert.Equal(now.AddHours(1), data[0].EndDate);
		}

		[Fact]
		public void DeleteCompMiddle()
		{
			//Arrange
			var del = new List<ManualWorkItem>()
			{
				new ManualWorkItem() { ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteComputerInterval, StartDate = now.AddHours(0.5), EndDate = now.AddHours(1.5) },
			};
			var data = new List<WorkItem>()
			{
				new WorkItem() { Id = 1, StartDate = now, EndDate = now.AddHours(2) },
			};

			//Act
			data.DeleteIntervals(del);

			//Assert
			Assert.Equal(2, data.Count);
			Assert.Equal(now.AddHours(0), data[0].StartDate);
			Assert.Equal(now.AddHours(0.5), data[0].EndDate);
			Assert.Equal(now.AddHours(1.5), data[1].StartDate);
			Assert.Equal(now.AddHours(2), data[1].EndDate);
		}

		[Fact]
		public void DeleteCompAll()
		{
			//Arrange
			var del = new List<ManualWorkItem>()
			{
				new ManualWorkItem() { ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteComputerInterval, StartDate = now.AddHours(0), EndDate = now.AddHours(2) },
			};
			var data = new List<WorkItem>()
			{
				new WorkItem() { Id = 1, StartDate = now, EndDate = now.AddHours(2) },
			};

			//Act
			data.DeleteIntervals(del);

			//Assert
			Assert.Equal(0, data.Count);
		}

		[Fact]
		public void DeleteCompCombined()
		{
			//Arrange
			var del = new List<ManualWorkItem>()
			{
				new ManualWorkItem() { ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteComputerInterval, StartDate = now.AddHours(0.5), EndDate = now.AddHours(1) },
				new ManualWorkItem() { ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteComputerInterval, StartDate = now.AddHours(1.5), EndDate = now.AddHours(2) },
				new ManualWorkItem() { ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteComputerInterval, StartDate = now.AddHours(0), EndDate = now.AddHours(0.25) },
			};
			var data = new List<WorkItem>()
			{
				new WorkItem() { Id = 1, StartDate = now, EndDate = now.AddHours(2) },
			};

			//Act
			data.DeleteIntervals(del);

			//Assert
			Assert.Equal(2, data.Count);
			Assert.Equal(now.AddHours(0.25), data[0].StartDate);
			Assert.Equal(now.AddHours(0.5), data[0].EndDate);
			Assert.Equal(now.AddHours(1), data[1].StartDate);
			Assert.Equal(now.AddHours(1.5), data[1].EndDate);
		}

		[Fact]
		public void DeleteCompDesktopCaptures()
		{
			//Arrange
			var del = new List<ManualWorkItem>()
			{
				new ManualWorkItem() { ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteComputerInterval, StartDate = now.AddMinutes(-1), EndDate = now.AddMinutes(-0.5) },
				new ManualWorkItem() { ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteInterval, StartDate = now.AddMinutes(-0.5), EndDate = now.AddMinutes(0) },
			};
			var data = new List<WorkItem>()
			{
				new WorkItem()
				{
					UserId = 13,
					StartDate = now.AddMinutes(-1),
					EndDate = now.AddMinutes(1),
					DesktopCaptures = new List<DesktopCapture>()
					{
						new DesktopCapture() { 
							Screens = new List<Screen>()
							{
								new Screen(){CreateDate = now.AddMinutes(-1), ScreenNumber = 1, }
							}
						},
						new DesktopCapture() { 
							Screens = new List<Screen>()
							{
								new Screen(){CreateDate = now.AddMinutes(0), ScreenNumber = 2, }
							}
						},
						new DesktopCapture() { 
							Screens = new List<Screen>()
							{
								new Screen(){CreateDate = now.AddMinutes(1), ScreenNumber = 3, }
							}
						},
					},
				},
			};

			//Act
			data.DeleteIntervals(del);

			//Assert
			Assert.Equal(1, data.Count);
			Assert.Equal(now.AddMinutes(0), data[0].StartDate);
			Assert.Equal(now.AddMinutes(1), data[0].EndDate);
			Assert.Equal(1, data[0].DesktopCaptures.Count);
			Assert.Equal(3, data[0].DesktopCaptures[0].Screens[0].ScreenNumber);
		}

		[Fact]
		public void DeleteCompActivity()
		{
			//Arrange
			var del = new List<ManualWorkItem>()
			{
				new ManualWorkItem() { ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteComputerInterval, StartDate = now, EndDate = now.AddHours(1) },
			};
			var data = new List<WorkItem>()
			{
				new WorkItem() { Id = 1, StartDate = now, EndDate = now.AddHours(2), MouseActivity = 1000, KeyboardActivity = 100 },
			};

			//Act
			data.DeleteIntervals(del);

			//Assert
			Assert.Equal(1, data.Count);
			Assert.Equal(now.AddHours(1), data[0].StartDate);
			Assert.Equal(now.AddHours(2), data[0].EndDate);
			Assert.Equal(500, data[0].MouseActivity);
			Assert.Equal(50, data[0].KeyboardActivity);
		}

		[Fact]
		public void DeleteCompActivity2()
		{
			//Arrange
			var del = new List<ManualWorkItem>()
			{
				new ManualWorkItem() { ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteComputerInterval, StartDate = now, EndDate = now.AddHours(2).AddMinutes(-1) },
			};
			var data = new List<WorkItem>()
			{
				new WorkItem() { Id = 1, StartDate = now, EndDate = now.AddHours(2), MouseActivity = 1200, KeyboardActivity = 120 },
			};

			//Act
			data.DeleteIntervals(del);

			//Assert
			Assert.Equal(1, data.Count);
			Assert.Equal(now.AddHours(2).AddMinutes(-1), data[0].StartDate);
			Assert.Equal(now.AddHours(2), data[0].EndDate);
			Assert.Equal(10, data[0].MouseActivity);
			Assert.Equal(1, data[0].KeyboardActivity);
		}

		[Fact]
		public void DeleteCompActivityWontRoundToZero()
		{
			//Arrange
			var del = new List<ManualWorkItem>()
			{
				new ManualWorkItem() { ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteComputerInterval, StartDate = now, EndDate = now.AddHours(2).AddMinutes(-1) },
			};
			var data = new List<WorkItem>()
			{
				new WorkItem() { Id = 1, StartDate = now, EndDate = now.AddHours(2), MouseActivity = 1, KeyboardActivity = 1 },
			};

			//Act
			data.DeleteIntervals(del);

			//Assert
			Assert.Equal(1, data.Count);
			Assert.Equal(now.AddHours(2).AddMinutes(-1), data[0].StartDate);
			Assert.Equal(now.AddHours(2), data[0].EndDate);
			Assert.Equal(1, data[0].MouseActivity);
			Assert.Equal(1, data[0].KeyboardActivity);
		}

		[Fact]
		public void DeleteMobileCombined()
		{
			//Arrange
			var del = new List<ManualWorkItem>()
			{
				new ManualWorkItem() { ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteMobileInterval, StartDate = now.AddHours(0.5), EndDate = now.AddHours(1) },
				new ManualWorkItem() { ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteMobileInterval, StartDate = now.AddHours(1.5), EndDate = now.AddHours(2) },
				new ManualWorkItem() { ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteMobileInterval, StartDate = now.AddHours(0), EndDate = now.AddHours(0.25) },
			};
			var data = new List<MobileWorkItem>()
			{
				new MobileWorkItem() { Id = 1, StartDate = now, EndDate = now.AddHours(2) },
			};

			//Act
			data.DeleteIntervals(del);

			//Assert
			Assert.Equal(2, data.Count);
			Assert.Equal(now.AddHours(0.25), data[0].StartDate);
			Assert.Equal(now.AddHours(0.5), data[0].EndDate);
			Assert.Equal(now.AddHours(1), data[1].StartDate);
			Assert.Equal(now.AddHours(1.5), data[1].EndDate);
		}

		[Fact]
		public void DeleteAggrCombined()
		{
			//Arrange
			var del = new List<ManualWorkItem>()
			{
				new ManualWorkItem() { ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteComputerInterval, StartDate = now.AddHours(0.5), EndDate = now.AddHours(1) },
				new ManualWorkItem() { ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteComputerInterval, StartDate = now.AddHours(1.5), EndDate = now.AddHours(2) },
				new ManualWorkItem() { ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteComputerInterval, StartDate = now.AddHours(0), EndDate = now.AddHours(0.25) },
			};
			var data = new List<AggregateWorkItemInterval>()
			{
				new AggregateWorkItemInterval() { Id = 1, StartDate = now, EndDate = now.AddHours(2) },
			};

			//Act
			data.DeleteIntervals(del);

			//Assert
			Assert.Equal(2, data.Count);
			Assert.Equal(now.AddHours(0.25), data[0].StartDate);
			Assert.Equal(now.AddHours(0.5), data[0].EndDate);
			Assert.Equal(now.AddHours(1), data[1].StartDate);
			Assert.Equal(now.AddHours(1.5), data[1].EndDate);
		}
	}
}
