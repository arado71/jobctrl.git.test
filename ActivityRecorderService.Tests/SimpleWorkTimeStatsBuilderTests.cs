using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderService;
using Tct.ActivityRecorderService.OnlineStats;
using Tct.ActivityRecorderService.Stats;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class SimpleWorkTimeStatsBuilderTests //very incomplete (cases from IntervalWorkTimeStatsBuilderTests)
	{
		#region Empty tests
		[Fact]
		public void EmptyStats()
		{
			var result = SimpleWorkTimeStatsBuilder.GetSimpleWorkTime(13, DateTime.MinValue, DateTime.MaxValue,
				Enumerable.Empty<IComputerWorkItem>(),
				Enumerable.Empty<IManualWorkItem>(),
				Enumerable.Empty<IMobileWorkItem>());

			Assert.Equal(13, result.UserId);
			Assert.Equal(DateTime.MinValue, result.FromDate);
			Assert.Equal(DateTime.MaxValue, result.ToDate);
			Assert.Equal(0, result.Stats.Count);
		}
		#endregion

		#region Computer tests
		[Fact]
		public void SimpleOverlappingCompIntervals()
		{
			var result = SimpleWorkTimeStatsBuilder.GetSimpleWorkTime(13, DateTime.MinValue, DateTime.MaxValue,
				new[] {
					new AggregateWorkItemInterval()
					{
						Id = 1,
						WorkId = 1,
						StartDate = new DateTime(2010, 10, 04, 10, 00, 00),
						EndDate = new DateTime(2010, 10, 04, 12, 00, 00),
					},
					new AggregateWorkItemInterval()
					{
						Id = 2,
						WorkId = 1,
						StartDate = new DateTime(2010, 10, 04, 11, 00, 00),
						EndDate = new DateTime(2010, 10, 04, 13, 00, 00),
					}
				},
				Enumerable.Empty<IManualWorkItem>(),
				Enumerable.Empty<IMobileWorkItem>());

			Assert.Equal(13, result.UserId);
			Assert.Equal(DateTime.MinValue, result.FromDate);
			Assert.Equal(DateTime.MaxValue, result.ToDate);
			Assert.Equal(1, result.Stats.Count);
			Assert.Equal(TimeSpan.FromHours(4), result.Stats[1].TotalWorkTime);
		}

		[Fact]
		public void SimpleOverlappingCompIntervalsGetSmallerInterval()
		{
			var result = SimpleWorkTimeStatsBuilder.GetSimpleWorkTime(13, new DateTime(2010, 10, 04, 10, 20, 00), new DateTime(2010, 10, 04, 12, 20, 00),
				new[] {
					new AggregateWorkItemInterval()
					{
						Id = 1,
						WorkId = 1,
						StartDate = new DateTime(2010, 10, 04, 10, 00, 00),
						EndDate = new DateTime(2010, 10, 04, 12, 00, 00),
					},
					new AggregateWorkItemInterval()
					{
						Id = 2,
						WorkId = 1,
						StartDate = new DateTime(2010, 10, 04, 11, 00, 00),
						EndDate = new DateTime(2010, 10, 04, 13, 00, 00),
					}
				},
				Enumerable.Empty<IManualWorkItem>(),
				Enumerable.Empty<IMobileWorkItem>());

			Assert.Equal(13, result.UserId);
			Assert.Equal(new DateTime(2010, 10, 04, 10, 20, 00), result.FromDate);
			Assert.Equal(new DateTime(2010, 10, 04, 12, 20, 00), result.ToDate);
			Assert.Equal(1, result.Stats.Count);
			Assert.Equal(TimeSpan.FromHours(3), result.Stats[1].TotalWorkTime);


			result = SimpleWorkTimeStatsBuilder.GetSimpleWorkTime(13, new DateTime(2010, 10, 03, 10, 30, 00), new DateTime(2010, 10, 03, 12, 30, 00),
				new[] {
					new AggregateWorkItemInterval()
					{
						Id = 1,
						WorkId = 1,
						StartDate = new DateTime(2010, 10, 04, 10, 00, 00),
						EndDate = new DateTime(2010, 10, 04, 12, 00, 00),
					},
					new AggregateWorkItemInterval()
					{
						Id = 2,
						WorkId = 1,
						StartDate = new DateTime(2010, 10, 04, 11, 00, 00),
						EndDate = new DateTime(2010, 10, 04, 13, 00, 00),
					}
				},
				Enumerable.Empty<IManualWorkItem>(),
				Enumerable.Empty<IMobileWorkItem>());

			Assert.Equal(13, result.UserId);
			Assert.Equal(new DateTime(2010, 10, 03, 10, 30, 00), result.FromDate);
			Assert.Equal(new DateTime(2010, 10, 03, 12, 30, 00), result.ToDate);
			Assert.Equal(0, result.Stats.Count);
		}

		[Fact]
		public void SimpleOverlappingCompIntervalsWithDeleteAndSmallerInterval()
		{
			var result = SimpleWorkTimeStatsBuilder.GetSimpleWorkTime(13, new DateTime(2010, 10, 04, 10, 20, 00), new DateTime(2010, 10, 04, 12, 20, 00),
				new[] {
					new AggregateWorkItemInterval()
					{
						Id = 1,
						WorkId = 1,
						StartDate = new DateTime(2010, 10, 04, 10, 00, 00),
						EndDate = new DateTime(2010, 10, 04, 12, 00, 00),
					},
					new AggregateWorkItemInterval()
					{
						Id = 2,
						WorkId = 1,
						StartDate = new DateTime(2010, 10, 04, 11, 00, 00),
						EndDate = new DateTime(2010, 10, 04, 13, 00, 00),
					}
				},
				new[] {
					new ManualWorkItem()
					{
						Id = 1,
						ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteInterval,
						StartDate = new DateTime(2010, 10, 04, 10, 30, 00),
						EndDate = new DateTime(2010, 10, 04, 11, 30, 00),
					}
				},
				Enumerable.Empty<IMobileWorkItem>());

			Assert.Equal(13, result.UserId);
			Assert.Equal(new DateTime(2010, 10, 04, 10, 20, 00), result.FromDate);
			Assert.Equal(new DateTime(2010, 10, 04, 12, 20, 00), result.ToDate);
			Assert.Equal(1, result.Stats.Count);
			Assert.Equal(TimeSpan.FromHours(1.5), result.Stats[1].TotalWorkTime);
		}
		#endregion

		#region Manual tests
		[Fact]
		public void CanAddHoliday()
		{
			var result = SimpleWorkTimeStatsBuilder.GetSimpleWorkTime(13, new DateTime(2010, 10, 26, 00, 00, 00), new DateTime(2010, 10, 27, 00, 00, 00),
				Enumerable.Empty<IComputerWorkItem>(),
				new[] {
					new ManualWorkItem()
					{
						Id = 1,
						WorkId = 2,
						ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddHoliday,
						StartDate = new DateTime(2010, 10, 26, 10, 00, 00),
						EndDate = new DateTime(2010, 10, 26, 18, 00, 00),
					}
				}, Enumerable.Empty<IMobileWorkItem>());

			Assert.Equal(13, result.UserId);
			Assert.Equal(new DateTime(2010, 10, 26, 00, 00, 00), result.FromDate);
			Assert.Equal(new DateTime(2010, 10, 27, 00, 00, 00), result.ToDate);
			Assert.Equal(1, result.Stats.Count);
			Assert.Equal(TimeSpan.FromHours(8), result.Stats[2].TotalWorkTime);
		}

		[Fact]
		public void CanAddSickLeave()
		{
			var result = SimpleWorkTimeStatsBuilder.GetSimpleWorkTime(13, new DateTime(2010, 10, 26, 00, 00, 00), new DateTime(2010, 10, 27, 00, 00, 00),
				Enumerable.Empty<IComputerWorkItem>(),
				new[] {
					new ManualWorkItem()
					{
						Id = 1,
						WorkId = 2,
						ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddSickLeave,
						StartDate = new DateTime(2010, 10, 26, 10, 00, 00),
						EndDate = new DateTime(2010, 10, 26, 18, 00, 00),
					}
				}, Enumerable.Empty<IMobileWorkItem>());

			Assert.Equal(13, result.UserId);
			Assert.Equal(new DateTime(2010, 10, 26, 00, 00, 00), result.FromDate);
			Assert.Equal(new DateTime(2010, 10, 27, 00, 00, 00), result.ToDate);
			Assert.Equal(1, result.Stats.Count);
			Assert.Equal(TimeSpan.FromHours(8), result.Stats[2].TotalWorkTime);
		}

		[Fact]
		public void CanAddWorkTime()
		{
			var result = SimpleWorkTimeStatsBuilder.GetSimpleWorkTime(13, new DateTime(2010, 10, 26, 00, 00, 00), new DateTime(2010, 10, 27, 00, 00, 00),
				Enumerable.Empty<IComputerWorkItem>(),
				new[] {
					new ManualWorkItem()
					{
						Id = 1,
						WorkId = 2,
						ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
						StartDate = new DateTime(2010, 10, 26, 10, 00, 00),
						EndDate = new DateTime(2010, 10, 26, 18, 00, 00),
					}
				}, Enumerable.Empty<IMobileWorkItem>());

			Assert.Equal(13, result.UserId);
			Assert.Equal(new DateTime(2010, 10, 26, 00, 00, 00), result.FromDate);
			Assert.Equal(new DateTime(2010, 10, 27, 00, 00, 00), result.ToDate);
			Assert.Equal(1, result.Stats.Count);
			Assert.Equal(TimeSpan.FromHours(8), result.Stats[2].TotalWorkTime);
		}

		[Fact]
		public void ManuallyAddedWorkTimeIsTruncated()
		{
			var result = SimpleWorkTimeStatsBuilder.GetSimpleWorkTime(13, new DateTime(2010, 10, 26, 12, 00, 00), new DateTime(2010, 10, 26, 14, 00, 00),
				Enumerable.Empty<IComputerWorkItem>(),
				new[] {
					new ManualWorkItem()
					{
						Id = 1,
						WorkId = 2,
						ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddWork,
						StartDate = new DateTime(2010, 10, 26, 10, 00, 00),
						EndDate = new DateTime(2010, 10, 26, 18, 00, 00),
					}
				}, Enumerable.Empty<IMobileWorkItem>());

			Assert.Equal(13, result.UserId);
			Assert.Equal(new DateTime(2010, 10, 26, 12, 00, 00), result.FromDate);
			Assert.Equal(new DateTime(2010, 10, 26, 14, 00, 00), result.ToDate);
			Assert.Equal(1, result.Stats.Count);
			Assert.Equal(TimeSpan.FromHours(2), result.Stats[2].TotalWorkTime);
		}

		[Fact]
		public void HolidayIsTruncated()
		{
			var result = SimpleWorkTimeStatsBuilder.GetSimpleWorkTime(13, new DateTime(2010, 10, 26, 12, 00, 00), new DateTime(2010, 10, 26, 14, 00, 00),
				Enumerable.Empty<IComputerWorkItem>(),
				new[] {
					new ManualWorkItem()
					{
						Id = 1,
						WorkId = 2,
						ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddHoliday,
						StartDate = new DateTime(2010, 10, 26, 10, 00, 00),
						EndDate = new DateTime(2010, 10, 26, 18, 00, 00),
					}
				}, Enumerable.Empty<IMobileWorkItem>());

			Assert.Equal(13, result.UserId);
			Assert.Equal(new DateTime(2010, 10, 26, 12, 00, 00), result.FromDate);
			Assert.Equal(new DateTime(2010, 10, 26, 14, 00, 00), result.ToDate);
			Assert.Equal(1, result.Stats.Count);
			Assert.Equal(TimeSpan.FromHours(2), result.Stats[2].TotalWorkTime);
		}

		[Fact]
		public void SickLeaveIsTruncated()
		{
			var result = SimpleWorkTimeStatsBuilder.GetSimpleWorkTime(13, new DateTime(2010, 10, 26, 12, 00, 00), new DateTime(2010, 10, 26, 14, 00, 00),
				Enumerable.Empty<IComputerWorkItem>(),
				new[] {
					new ManualWorkItem()
					{
						Id = 1,
						WorkId = 2,
						ManualWorkItemTypeId = ManualWorkItemTypeEnum.AddSickLeave,
						StartDate = new DateTime(2010, 10, 26, 10, 00, 00),
						EndDate = new DateTime(2010, 10, 26, 18, 00, 00),
					}
				}, Enumerable.Empty<IMobileWorkItem>());

			Assert.Equal(13, result.UserId);
			Assert.Equal(new DateTime(2010, 10, 26, 12, 00, 00), result.FromDate);
			Assert.Equal(new DateTime(2010, 10, 26, 14, 00, 00), result.ToDate);
			Assert.Equal(1, result.Stats.Count);
			Assert.Equal(TimeSpan.FromHours(2), result.Stats[2].TotalWorkTime);
		}
		#endregion

		#region Mobile tests
		[Fact]
		public void SimpleOverlappingMobileIntervals()
		{
			var result = SimpleWorkTimeStatsBuilder.GetSimpleWorkTime(13, DateTime.MinValue, DateTime.MaxValue,
				Enumerable.Empty<IComputerWorkItem>(),
				Enumerable.Empty<IManualWorkItem>(),
				new[] {
					new MobileWorkItem()
					{
						Id = 1,
						WorkId = 1,
						StartDate = new DateTime(2010, 10, 04, 10, 00, 00),
						EndDate = new DateTime(2010, 10, 04, 12, 00, 00),
					},
					new MobileWorkItem()
					{
						Id = 2,
						WorkId = 1,
						StartDate = new DateTime(2010, 10, 04, 11, 00, 00),
						EndDate = new DateTime(2010, 10, 04, 13, 00, 00),
					}
				}
				);

			Assert.Equal(13, result.UserId);
			Assert.Equal(DateTime.MinValue, result.FromDate);
			Assert.Equal(DateTime.MaxValue, result.ToDate);
			Assert.Equal(1, result.Stats.Count);
			Assert.Equal(TimeSpan.FromHours(4), result.Stats[1].TotalWorkTime);
		}

		[Fact]
		public void SimpleOverlappingMobileIntervalsGetSmallerInterval()
		{
			var result = SimpleWorkTimeStatsBuilder.GetSimpleWorkTime(13, new DateTime(2010, 10, 04, 10, 20, 00), new DateTime(2010, 10, 04, 12, 20, 00),
				Enumerable.Empty<IComputerWorkItem>(),
				Enumerable.Empty<IManualWorkItem>(),
				new[] {
					new MobileWorkItem()
					{
						Id = 1,
						WorkId = 1,
						StartDate = new DateTime(2010, 10, 04, 10, 00, 00),
						EndDate = new DateTime(2010, 10, 04, 12, 00, 00),
					},
					new MobileWorkItem()
					{
						Id = 2,
						WorkId = 1,
						StartDate = new DateTime(2010, 10, 04, 11, 00, 00),
						EndDate = new DateTime(2010, 10, 04, 13, 00, 00),
					}
				}
				);

			Assert.Equal(13, result.UserId);
			Assert.Equal(new DateTime(2010, 10, 04, 10, 20, 00), result.FromDate);
			Assert.Equal(new DateTime(2010, 10, 04, 12, 20, 00), result.ToDate);
			Assert.Equal(1, result.Stats.Count);
			Assert.Equal(TimeSpan.FromHours(3), result.Stats[1].TotalWorkTime);

			result = SimpleWorkTimeStatsBuilder.GetSimpleWorkTime(13, new DateTime(2010, 10, 03, 10, 30, 00), new DateTime(2010, 10, 03, 12, 30, 00),
				Enumerable.Empty<IComputerWorkItem>(),
				Enumerable.Empty<IManualWorkItem>(),
				new[] {
					new MobileWorkItem()
					{
						Id = 1,
						WorkId = 1,
						StartDate = new DateTime(2010, 10, 04, 10, 00, 00),
						EndDate = new DateTime(2010, 10, 04, 12, 00, 00),
					},
					new MobileWorkItem()
					{
						Id = 2,
						WorkId = 1,
						StartDate = new DateTime(2010, 10, 04, 11, 00, 00),
						EndDate = new DateTime(2010, 10, 04, 13, 00, 00),
					}
				}
				);

			Assert.Equal(13, result.UserId);
			Assert.Equal(new DateTime(2010, 10, 03, 10, 30, 00), result.FromDate);
			Assert.Equal(new DateTime(2010, 10, 03, 12, 30, 00), result.ToDate);
			Assert.Equal(0, result.Stats.Count);
		}

		[Fact]
		public void SimpleOverlappingMobileIntervalsWithDeleteAndSmallerInterval()
		{
			var result = SimpleWorkTimeStatsBuilder.GetSimpleWorkTime(13, new DateTime(2010, 10, 04, 10, 20, 00), new DateTime(2010, 10, 04, 12, 20, 00),
				Enumerable.Empty<IComputerWorkItem>(),
				new[] {
					new ManualWorkItem()
					{
						Id = 1,
						ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteInterval,
						StartDate = new DateTime(2010, 10, 04, 10, 30, 00),
						EndDate = new DateTime(2010, 10, 04, 11, 30, 00),
					}
				}, 
				new[] {
					new MobileWorkItem()
					{
						Id = 1,
						WorkId = 1,
						StartDate = new DateTime(2010, 10, 04, 10, 00, 00),
						EndDate = new DateTime(2010, 10, 04, 12, 00, 00),
					},
					new MobileWorkItem()
					{
						Id = 2,
						WorkId = 1,
						StartDate = new DateTime(2010, 10, 04, 11, 00, 00),
						EndDate = new DateTime(2010, 10, 04, 13, 00, 00),
					}
				}
				);

			Assert.Equal(13, result.UserId);
			Assert.Equal(new DateTime(2010, 10, 04, 10, 20, 00), result.FromDate);
			Assert.Equal(new DateTime(2010, 10, 04, 12, 20, 00), result.ToDate);
			Assert.Equal(1, result.Stats.Count);
			Assert.Equal(TimeSpan.FromHours(1.5), result.Stats[1].TotalWorkTime);
		}

		[Fact]
		public void SimpleOverlappingMobileIntervalsWithDeleteSpecificAndSmallerInterval()
		{
			var result = SimpleWorkTimeStatsBuilder.GetSimpleWorkTime(13, new DateTime(2010, 10, 04, 10, 20, 00), new DateTime(2010, 10, 04, 12, 20, 00),
				Enumerable.Empty<IComputerWorkItem>(),
				new[] {
					new ManualWorkItem()
					{
						Id = 1,
						ManualWorkItemTypeId = ManualWorkItemTypeEnum.DeleteMobileInterval,
						StartDate = new DateTime(2010, 10, 04, 10, 30, 00),
						EndDate = new DateTime(2010, 10, 04, 11, 30, 00),
					}
				},
				new[] {
					new MobileWorkItem()
					{
						Id = 1,
						WorkId = 1,
						StartDate = new DateTime(2010, 10, 04, 10, 00, 00),
						EndDate = new DateTime(2010, 10, 04, 12, 00, 00),
					},
					new MobileWorkItem()
					{
						Id = 2,
						WorkId = 1,
						StartDate = new DateTime(2010, 10, 04, 11, 00, 00),
						EndDate = new DateTime(2010, 10, 04, 13, 00, 00),
					}
				}
				);

			Assert.Equal(13, result.UserId);
			Assert.Equal(new DateTime(2010, 10, 04, 10, 20, 00), result.FromDate);
			Assert.Equal(new DateTime(2010, 10, 04, 12, 20, 00), result.ToDate);
			Assert.Equal(1, result.Stats.Count);
			Assert.Equal(TimeSpan.FromHours(1.5), result.Stats[1].TotalWorkTime);
		}

		[Fact]
		public void SimpleOverlappingMobileIntervalWithZeroDuration()
		{
			var result = SimpleWorkTimeStatsBuilder.GetSimpleWorkTime(13, new DateTime(2010, 10, 04, 11, 10, 00), new DateTime(2010, 10, 04, 11, 10, 00),
				Enumerable.Empty<IComputerWorkItem>(),
				Enumerable.Empty<IManualWorkItem>(),
				new[] {
					new MobileWorkItem()
					{
						Id = 1,
						WorkId = 1,
						StartDate = new DateTime(2010, 10, 04, 10, 00, 00),
						EndDate = new DateTime(2010, 10, 04, 12, 00, 00),
					},
					new MobileWorkItem()
					{
						Id = 2,
						WorkId = 1,
						StartDate = new DateTime(2010, 10, 04, 11, 00, 00),
						EndDate = new DateTime(2010, 10, 04, 13, 00, 00),
					}
				}
				);

			Assert.Equal(13, result.UserId);
			Assert.Equal(new DateTime(2010, 10, 04, 11, 10, 00), result.FromDate);
			Assert.Equal(new DateTime(2010, 10, 04, 11, 10, 00), result.ToDate);
			Assert.Equal(0, result.Stats.Count);
		}
		#endregion

	}
}
