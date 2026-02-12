using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderService;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class IntervalConcatenatorTests
	{
		private static IEnumerable<IntervalConcatenator> Permutate(params DateTime[] startEndDates)
		{
			if (startEndDates == null || startEndDates.Length % 2 != 0) throw new ArgumentException();
			List<KeyValuePair<DateTime, DateTime>> dates = new List<KeyValuePair<DateTime, DateTime>>();
			for (int i = 0; i < startEndDates.Length; i += 2)
			{
				dates.Add(new KeyValuePair<DateTime, DateTime>(startEndDates[i], startEndDates[i + 1]));
			}
			foreach (var dateComb in dates.ToArray().Permute())
			{
				var curr = new IntervalConcatenator();
				foreach (KeyValuePair<DateTime, DateTime> keyValuePair in dateComb)
				{
					curr.Add(keyValuePair.Key, keyValuePair.Value);
				}
				yield return curr;
			}
		}

		#region Empty tests
		[Fact]
		public void EmptyIntervalHasZeroDuration()
		{
			//Arrange
			var ic = new IntervalConcatenator();

			//Act
			var duration = ic.Duration();

			//Assert
			Assert.Equal(TimeSpan.Zero, duration);
		}
		#endregion

		#region Add tests
		[Fact]
		public void DurationAddTwoContHours()
		{
			//Arrange
			var ic = new IntervalConcatenator();
			ic.Add(new DateTime(2010, 05, 12, 11, 00, 00), new DateTime(2010, 05, 12, 12, 00, 00));
			ic.Add(new DateTime(2010, 05, 12, 12, 00, 00), new DateTime(2010, 05, 12, 13, 00, 00));

			//Act
			var duration = ic.Duration();

			//Assert
			Assert.Equal(TimeSpan.FromHours(2), duration);
		}

		[Fact]
		public void DurationAddTwoOverlapingOneHours()
		{
			//Arrange
			var ic = new IntervalConcatenator();
			ic.Add(new DateTime(2010, 05, 12, 11, 00, 00), new DateTime(2010, 05, 12, 12, 00, 00));
			ic.Add(new DateTime(2010, 05, 12, 11, 30, 00), new DateTime(2010, 05, 12, 12, 30, 00));

			//Act
			var duration = ic.Duration();

			//Assert
			Assert.Equal(TimeSpan.FromHours(1.5), duration);
		}

		[Fact]
		public void DurationAddTwoOverlapingOneHoursReverse()
		{
			//Arrange
			var ic = new IntervalConcatenator();
			ic.Add(new DateTime(2010, 05, 12, 11, 30, 00), new DateTime(2010, 05, 12, 12, 30, 00));
			ic.Add(new DateTime(2010, 05, 12, 11, 00, 00), new DateTime(2010, 05, 12, 12, 00, 00));

			//Act
			var duration = ic.Duration();

			//Assert
			Assert.Equal(TimeSpan.FromHours(1.5), duration);
		}

		[Fact]
		public void DurationAddTwoFullOverlapingIntervals()
		{
			//Arrange
			var ic = new IntervalConcatenator();
			ic.Add(new DateTime(2010, 05, 12, 11, 00, 00), new DateTime(2010, 05, 12, 12, 00, 00));
			ic.Add(new DateTime(2010, 05, 12, 11, 30, 00), new DateTime(2010, 05, 12, 11, 50, 00));

			//Act
			var duration = ic.Duration();

			//Assert
			Assert.Equal(TimeSpan.FromHours(1), duration);
		}

		[Fact]
		public void DurationAddTwoFullOverlapingIntervalsReverse()
		{
			//Arrange
			var ic = new IntervalConcatenator();
			ic.Add(new DateTime(2010, 05, 12, 11, 30, 00), new DateTime(2010, 05, 12, 11, 50, 00));
			ic.Add(new DateTime(2010, 05, 12, 11, 00, 00), new DateTime(2010, 05, 12, 12, 00, 00));

			//Act
			var duration = ic.Duration();

			//Assert
			Assert.Equal(TimeSpan.FromHours(1), duration);
		}

		[Fact]
		public void DurationAddThreeDisjointIntervals()
		{
			//Arrange
			var ics = Permutate(
				new DateTime(2010, 05, 12, 11, 00, 00), new DateTime(2010, 05, 12, 12, 00, 00),
				new DateTime(2010, 05, 12, 13, 00, 00), new DateTime(2010, 05, 12, 14, 00, 00),
				new DateTime(2010, 05, 12, 15, 00, 00), new DateTime(2010, 05, 12, 16, 00, 00)
				);

			//Assert
			Assert.Equal(6, ics.Count());
			Assert.True(ics.All(n => n.Duration() == TimeSpan.FromHours(3)));
		}

		[Fact]
		public void DurationAddThreeContIntervals()
		{
			//Arrange
			var ics = Permutate(
				new DateTime(2010, 05, 12, 11, 00, 00), new DateTime(2010, 05, 12, 12, 00, 00),
				new DateTime(2010, 05, 12, 12, 00, 00), new DateTime(2010, 05, 12, 13, 00, 00),
				new DateTime(2010, 05, 12, 13, 00, 00), new DateTime(2010, 05, 12, 14, 00, 00)
				);

			//Assert
			Assert.Equal(6, ics.Count());
			Assert.True(ics.All(n => n.Duration() == TimeSpan.FromHours(3)));
		}

		[Fact]
		public void DurationAddThreeOverlappingIntervals()
		{
			//Arrange
			var ics = Permutate(
				new DateTime(2010, 05, 12, 11, 00, 00), new DateTime(2010, 05, 12, 12, 00, 00),
				new DateTime(2010, 05, 12, 11, 30, 00), new DateTime(2010, 05, 12, 12, 30, 00),
				new DateTime(2010, 05, 12, 11, 55, 00), new DateTime(2010, 05, 12, 13, 00, 00)
				);

			//Assert
			Assert.Equal(6, ics.Count());
			Assert.True(ics.All(n => n.Duration() == TimeSpan.FromHours(2)));
		}

		[Fact]
		public void DurationAddThreeOverlappingIntervalsOneConsistsAll()
		{
			//Arrange
			var ics = Permutate(
				new DateTime(2010, 05, 12, 11, 00, 00), new DateTime(2010, 05, 12, 14, 00, 00),
				new DateTime(2010, 05, 12, 11, 30, 00), new DateTime(2010, 05, 12, 12, 30, 00),
				new DateTime(2010, 05, 12, 11, 55, 00), new DateTime(2010, 05, 12, 13, 00, 00)
				);

			//Assert
			Assert.Equal(6, ics.Count());
			Assert.True(ics.All(n => n.Duration() == TimeSpan.FromHours(3)));
		}

		[Fact]
		public void DurationAddThreeOverlappingIntervalsTwoHaveSameStart()
		{
			//Arrange
			var ics = Permutate(
				new DateTime(2010, 05, 12, 11, 00, 00), new DateTime(2010, 05, 12, 13, 00, 00),
				new DateTime(2010, 05, 12, 11, 00, 00), new DateTime(2010, 05, 12, 12, 30, 00),
				new DateTime(2010, 05, 12, 12, 20, 00), new DateTime(2010, 05, 12, 13, 30, 00)
				);

			//Assert
			Assert.Equal(6, ics.Count());
			Assert.True(ics.All(n => n.Duration() == TimeSpan.FromHours(2.5)));
		}

		[Fact]
		public void DurationAddThreeOverlappingIntervalsPartialOverlap()
		{
			//Arrange
			var ics = Permutate(
				new DateTime(2010, 05, 12, 11, 10, 00), new DateTime(2010, 05, 12, 13, 00, 00),
				new DateTime(2010, 05, 12, 11, 00, 00), new DateTime(2010, 05, 12, 12, 30, 00),
				new DateTime(2010, 05, 12, 12, 20, 00), new DateTime(2010, 05, 12, 13, 30, 00)
				);

			//Assert
			Assert.Equal(6, ics.Count());
			Assert.True(ics.All(n => n.Duration() == TimeSpan.FromHours(2.5)));
		}

		[Fact]
		public void DurationAddTwoOverlappingIntervalsFromThreeOneConsistsAll()
		{
			//Arrange
			var ics = Permutate(
				new DateTime(2010, 05, 12, 11, 00, 00), new DateTime(2010, 05, 12, 14, 00, 00),
				new DateTime(2010, 05, 12, 11, 30, 00), new DateTime(2010, 05, 12, 12, 30, 00),
				new DateTime(2010, 05, 12, 12, 31, 00), new DateTime(2010, 05, 12, 13, 00, 00)
				);

			//Assert
			Assert.Equal(6, ics.Count());
			Assert.True(ics.All(n => n.Duration() == TimeSpan.FromHours(3)));
		}

		[Fact]
		public void DurationAddTwoOverlappingIntervalsFromThreeTwoHaveSameStart()
		{
			//Arrange
			var ics = Permutate(
				new DateTime(2010, 05, 12, 11, 00, 00), new DateTime(2010, 05, 12, 13, 00, 00),
				new DateTime(2010, 05, 12, 11, 00, 00), new DateTime(2010, 05, 12, 12, 30, 00),
				new DateTime(2010, 05, 12, 12, 31, 00), new DateTime(2010, 05, 12, 13, 30, 00)
				);

			//Assert
			Assert.Equal(6, ics.Count());
			Assert.True(ics.All(n => n.Duration() == TimeSpan.FromHours(2.5)));
		}

		[Fact]
		public void DurationAddTwoOverlappingIntervalsFromThreePartialOverlap()
		{
			//Arrange
			var ics = Permutate(
				new DateTime(2010, 05, 12, 11, 10, 00), new DateTime(2010, 05, 12, 13, 00, 00),
				new DateTime(2010, 05, 12, 11, 00, 00), new DateTime(2010, 05, 12, 12, 30, 00),
				new DateTime(2010, 05, 12, 12, 31, 00), new DateTime(2010, 05, 12, 13, 30, 00)
				);

			//Assert
			Assert.Equal(6, ics.Count());
			Assert.True(ics.All(n => n.Duration() == TimeSpan.FromHours(2.5)));
		}

		[Fact]
		public void DurationAddFiveComplexIntervals()
		{
			//Arrange
			var ics = Permutate(
				new DateTime(2010, 05, 12, 11, 10, 00), new DateTime(2010, 05, 12, 13, 00, 00),
				new DateTime(2010, 05, 12, 11, 10, 00), new DateTime(2010, 05, 12, 12, 30, 00),
				new DateTime(2010, 05, 12, 12, 31, 00), new DateTime(2010, 05, 12, 13, 30, 00),
				new DateTime(2010, 05, 12, 13, 29, 00), new DateTime(2010, 05, 12, 14, 30, 00),
				new DateTime(2010, 05, 12, 14, 31, 00), new DateTime(2010, 05, 12, 17, 11, 00)
				);

			//Assert
			Assert.Equal(120, ics.Count());
			Assert.True(ics.All(n => n.Duration() == TimeSpan.FromHours(6)));
		}
		#endregion

		#region Clone tests
		[Fact]
		public void IntervalsAreClonedInCtor()
		{
			//Arrange
			var ic = new IntervalConcatenator();
			ic.Add(new DateTime(2010, 09, 28, 11, 00, 00), new DateTime(2010, 09, 28, 12, 00, 00));

			//Act
			var ic2 = ic.Clone();
			ic.Add(new DateTime(2010, 09, 28, 12, 00, 00), new DateTime(2010, 09, 28, 13, 00, 00));

			//Assert
			Assert.Equal(TimeSpan.FromHours(2), ic.Duration());
			Assert.Equal(TimeSpan.FromHours(1), ic2.Duration());
		}

		[Fact]
		public void IntervalsAreClonedInGetIntervals()
		{
			//Arrange
			var ic = new IntervalConcatenator();
			ic.Add(new DateTime(2010, 09, 28, 11, 00, 00), new DateTime(2010, 09, 28, 12, 00, 00));

			//Act
			var ivs = ic.GetIntervals();
			ivs[0] = new IntervalConcatenator.Interval(ivs[0].StartDate, new DateTime(2010, 09, 28, 13, 00, 00));

			//Assert
			Assert.Equal(TimeSpan.FromHours(1), ic.Duration());
		}

		[Fact]
		public void DurationAddTwoOverlapingOneHoursClone()
		{
			//Arrange
			var ic = new IntervalConcatenator();
			ic.Add(new DateTime(2010, 05, 12, 11, 00, 00), new DateTime(2010, 05, 12, 12, 00, 00));

			var ic2 = ic.Clone();
			ic.Add(new DateTime(2010, 05, 12, 11, 30, 00), new DateTime(2010, 05, 12, 12, 30, 00));
			ic2.Add(new DateTime(2010, 05, 12, 11, 30, 00), new DateTime(2010, 05, 12, 12, 30, 00));

			//Act
			var duration = ic.Duration();
			var duration2 = ic2.Duration();

			//Assert
			Assert.Equal(TimeSpan.FromHours(1.5), duration);
			Assert.Equal(TimeSpan.FromHours(1.5), duration2);
		}

		[Fact]
		public void DurationAddThreeOverlappingIntervalsClone()
		{
			//Arrange
			var ics = Permutate(
				new DateTime(2010, 05, 12, 11, 00, 00), new DateTime(2010, 05, 12, 12, 00, 00),
				new DateTime(2010, 05, 12, 11, 30, 00), new DateTime(2010, 05, 12, 12, 30, 00)

				).Select(n => new { Original = n, Clone = n.Clone() }).ToList();
			ics.ForEach(n =>
			{
				n.Original.Add(new DateTime(2010, 05, 12, 11, 55, 00), new DateTime(2010, 05, 12, 13, 00, 00));
				n.Clone.Add(new DateTime(2010, 05, 12, 11, 55, 00), new DateTime(2010, 05, 12, 13, 00, 00));
			});

			var ics2 = Permutate(
				new DateTime(2010, 05, 12, 11, 00, 00), new DateTime(2010, 05, 12, 12, 00, 00),
				new DateTime(2010, 05, 12, 11, 55, 00), new DateTime(2010, 05, 12, 13, 00, 00)
				).Select(n => new { Original = n, Clone = n.Clone() }).ToList();
			ics2.ForEach(n =>
			{
				n.Original.Add(new DateTime(2010, 05, 12, 11, 30, 00), new DateTime(2010, 05, 12, 12, 30, 00));
				n.Clone.Add(new DateTime(2010, 05, 12, 11, 30, 00), new DateTime(2010, 05, 12, 12, 30, 00));
			});

			var ics3 = Permutate(
				new DateTime(2010, 05, 12, 11, 30, 00), new DateTime(2010, 05, 12, 12, 30, 00),
				new DateTime(2010, 05, 12, 11, 55, 00), new DateTime(2010, 05, 12, 13, 00, 00)
				).Select(n => new { Original = n, Clone = n.Clone() }).ToList();
			ics3.ForEach(n =>
			{
				n.Original.Add(new DateTime(2010, 05, 12, 11, 00, 00), new DateTime(2010, 05, 12, 12, 00, 00));
				n.Clone.Add(new DateTime(2010, 05, 12, 11, 00, 00), new DateTime(2010, 05, 12, 12, 00, 00));
			});


			//Assert
			Assert.True(ics.All(n => n.Original.Duration() == TimeSpan.FromHours(2)));
			Assert.True(ics.All(n => n.Clone.Duration() == TimeSpan.FromHours(2)));
			Assert.True(ics2.All(n => n.Original.Duration() == TimeSpan.FromHours(2)));
			Assert.True(ics2.All(n => n.Clone.Duration() == TimeSpan.FromHours(2)));
			Assert.True(ics3.All(n => n.Original.Duration() == TimeSpan.FromHours(2)));
			Assert.True(ics3.All(n => n.Clone.Duration() == TimeSpan.FromHours(2)));
		}
		#endregion

		#region Remove tests
		[Fact]
		public void RemoveFromEmptyIsEmpty()
		{
			//Arrange
			var ic = new IntervalConcatenator();
			ic.Remove(new DateTime(2010, 09, 28, 11, 00, 00), new DateTime(2010, 09, 28, 13, 00, 00));

			//Act
			var duration = ic.Duration();
			var intervals = ic.GetIntervals();

			//Assert
			Assert.Equal(TimeSpan.Zero, duration);
			Assert.Empty(intervals);
		}

		[Fact]
		public void RemoveStartOfAnInterval()
		{
			//Arrange
			var ic = new IntervalConcatenator();
			ic.Add(new DateTime(2010, 09, 28, 11, 00, 00), new DateTime(2010, 09, 28, 15, 00, 00));
			ic.Remove(new DateTime(2010, 09, 28, 11, 00, 00), new DateTime(2010, 09, 28, 13, 00, 00));

			//Act
			var duration = ic.Duration();
			var intervals = ic.GetIntervals();

			//Assert
			Assert.Equal(TimeSpan.FromHours(2), duration);
			Assert.True(intervals.SequenceEqual(new[] { 
				new IntervalConcatenator.Interval(new DateTime(2010, 09, 28, 13, 00, 00), new DateTime(2010, 09, 28, 15, 00, 00)),
			}));
		}

		[Fact]
		public void RemoveStartOfAnIntervalAndMore()
		{
			//Arrange
			var ic = new IntervalConcatenator();
			ic.Add(new DateTime(2010, 09, 28, 11, 00, 00), new DateTime(2010, 09, 28, 15, 00, 00));
			ic.Remove(new DateTime(2010, 09, 28, 10, 00, 00), new DateTime(2010, 09, 28, 13, 00, 00));

			//Act
			var duration = ic.Duration();
			var intervals = ic.GetIntervals();

			//Assert
			Assert.Equal(TimeSpan.FromHours(2), duration);
			Assert.True(intervals.SequenceEqual(new[] { 
				new IntervalConcatenator.Interval(new DateTime(2010, 09, 28, 13, 00, 00), new DateTime(2010, 09, 28, 15, 00, 00)),
			}));
		}

		[Fact]
		public void RemoveEndOfAnInterval()
		{
			//Arrange
			var ic = new IntervalConcatenator();
			ic.Add(new DateTime(2010, 09, 28, 11, 00, 00), new DateTime(2010, 09, 28, 15, 00, 00));
			ic.Remove(new DateTime(2010, 09, 28, 13, 00, 00), new DateTime(2010, 09, 28, 15, 00, 00));

			//Act
			var duration = ic.Duration();
			var intervals = ic.GetIntervals();

			//Assert
			Assert.Equal(TimeSpan.FromHours(2), duration);
			Assert.True(intervals.SequenceEqual(new[] { 
				new IntervalConcatenator.Interval(new DateTime(2010, 09, 28, 11, 00, 00), new DateTime(2010, 09, 28, 13, 00, 00)),
			}));
		}

		[Fact]
		public void RemoveEndOfAnIntervalAndMore()
		{
			//Arrange
			var ic = new IntervalConcatenator();
			ic.Add(new DateTime(2010, 09, 28, 11, 00, 00), new DateTime(2010, 09, 28, 15, 00, 00));
			ic.Remove(new DateTime(2010, 09, 28, 13, 00, 00), new DateTime(2010, 09, 28, 16, 00, 00));

			//Act
			var duration = ic.Duration();
			var intervals = ic.GetIntervals();

			//Assert
			Assert.Equal(TimeSpan.FromHours(2), duration);
			Assert.True(intervals.SequenceEqual(new[] { 
				new IntervalConcatenator.Interval(new DateTime(2010, 09, 28, 11, 00, 00), new DateTime(2010, 09, 28, 13, 00, 00)),
			}));
		}

		[Fact]
		public void RemoveMiddleOfAnInterval()
		{
			//Arrange
			var ic = new IntervalConcatenator();
			ic.Add(new DateTime(2010, 09, 28, 11, 00, 00), new DateTime(2010, 09, 28, 15, 00, 00));
			ic.Remove(new DateTime(2010, 09, 28, 12, 00, 00), new DateTime(2010, 09, 28, 14, 00, 00));

			//Act
			var duration = ic.Duration();
			var intervals = ic.GetIntervals();

			//Assert
			Assert.Equal(TimeSpan.FromHours(2), duration);
			Assert.True(intervals.SequenceEqual(new[] { 
				new IntervalConcatenator.Interval(new DateTime(2010, 09, 28, 11, 00, 00), new DateTime(2010, 09, 28, 12, 00, 00)),
				new IntervalConcatenator.Interval(new DateTime(2010, 09, 28, 14, 00, 00), new DateTime(2010, 09, 28, 15, 00, 00)),
			}));
		}

		[Fact]
		public void RemoveMiddleOfAnInterval2Times()
		{
			//Arrange
			var ic = new IntervalConcatenator();
			ic.Add(new DateTime(2010, 09, 28, 11, 00, 00), new DateTime(2010, 09, 28, 15, 00, 00));
			ic.Add(new DateTime(2010, 09, 28, 11, 00, 00), new DateTime(2010, 09, 28, 15, 00, 00));
			ic.Remove(new DateTime(2010, 09, 28, 12, 00, 00), new DateTime(2010, 09, 28, 14, 00, 00));
			ic.Remove(new DateTime(2010, 09, 28, 12, 00, 00), new DateTime(2010, 09, 28, 14, 00, 00));

			//Act
			var duration = ic.Duration();
			var intervals = ic.GetIntervals();

			//Assert
			Assert.Equal(TimeSpan.FromHours(2), duration);
			Assert.True(intervals.SequenceEqual(new[] { 
				new IntervalConcatenator.Interval(new DateTime(2010, 09, 28, 11, 00, 00), new DateTime(2010, 09, 28, 12, 00, 00)),
				new IntervalConcatenator.Interval(new DateTime(2010, 09, 28, 14, 00, 00), new DateTime(2010, 09, 28, 15, 00, 00)),
			}));
		}

		[Fact]
		public void Remove2FromMiddleOfAnInterval()
		{
			//Arrange
			var ic = new IntervalConcatenator();
			ic.Add(new DateTime(2010, 09, 28, 11, 00, 00), new DateTime(2010, 09, 28, 15, 00, 00));
			ic.Remove(new DateTime(2010, 09, 28, 11, 30, 00), new DateTime(2010, 09, 28, 12, 30, 00));
			ic.Remove(new DateTime(2010, 09, 28, 13, 00, 00), new DateTime(2010, 09, 28, 14, 00, 00));

			//Act
			var duration = ic.Duration();
			var intervals = ic.GetIntervals();

			//Assert
			Assert.Equal(TimeSpan.FromHours(2), duration);
			Assert.True(intervals.SequenceEqual(new[] { 
				new IntervalConcatenator.Interval(new DateTime(2010, 09, 28, 11, 00, 00), new DateTime(2010, 09, 28, 11, 30, 00)),
				new IntervalConcatenator.Interval(new DateTime(2010, 09, 28, 12, 30, 00), new DateTime(2010, 09, 28, 13, 00, 00)),
				new IntervalConcatenator.Interval(new DateTime(2010, 09, 28, 14, 00, 00), new DateTime(2010, 09, 28, 15, 00, 00)),
			}));
		}

		[Fact]
		public void RemoveEndOfAnIntervalAndSeveralMore()
		{
			//Arrange
			var ic = new IntervalConcatenator();
			ic.Add(new DateTime(2010, 09, 28, 11, 00, 00), new DateTime(2010, 09, 28, 15, 00, 00));
			ic.Add(new DateTime(2010, 09, 28, 16, 00, 00), new DateTime(2010, 09, 28, 17, 00, 00));
			ic.Add(new DateTime(2010, 09, 28, 18, 00, 00), new DateTime(2010, 09, 28, 19, 00, 00));
			ic.Remove(new DateTime(2010, 09, 28, 13, 00, 00), new DateTime(2010, 09, 28, 20, 00, 00));

			//Act
			var duration = ic.Duration();
			var intervals = ic.GetIntervals();

			//Assert
			Assert.Equal(TimeSpan.FromHours(2), duration);
			Assert.True(intervals.SequenceEqual(new[] { 
				new IntervalConcatenator.Interval(new DateTime(2010, 09, 28, 11, 00, 00), new DateTime(2010, 09, 28, 13, 00, 00)),
			}));
		}

		[Fact]
		public void RemoveEndOfAnIntervalAndSeveralMoreButTheLastOne()
		{
			//Arrange
			var ic = new IntervalConcatenator();
			ic.Add(new DateTime(2010, 09, 28, 11, 00, 00), new DateTime(2010, 09, 28, 15, 00, 00));
			ic.Add(new DateTime(2010, 09, 28, 16, 00, 00), new DateTime(2010, 09, 28, 17, 00, 00));
			ic.Add(new DateTime(2010, 09, 28, 18, 00, 00), new DateTime(2010, 09, 28, 19, 00, 00));
			ic.Add(new DateTime(2010, 09, 28, 19, 30, 00), new DateTime(2010, 09, 28, 21, 00, 00));
			ic.Remove(new DateTime(2010, 09, 28, 13, 00, 00), new DateTime(2010, 09, 28, 20, 00, 00));

			//Act
			var duration = ic.Duration();
			var intervals = ic.GetIntervals();

			//Assert
			Assert.Equal(TimeSpan.FromHours(3), duration);
			Assert.True(intervals.SequenceEqual(new[] { 
				new IntervalConcatenator.Interval(new DateTime(2010, 09, 28, 11, 00, 00), new DateTime(2010, 09, 28, 13, 00, 00)),
				new IntervalConcatenator.Interval(new DateTime(2010, 09, 28, 20, 00, 00), new DateTime(2010, 09, 28, 21, 00, 00)),
			}));
		}

		[Fact]
		public void RemoveSeveralIntervalsWithStartDateMatch()
		{
			//Arrange
			var ic = new IntervalConcatenator();
			ic.Add(new DateTime(2010, 09, 28, 11, 00, 00), new DateTime(2010, 09, 28, 15, 00, 00));
			ic.Add(new DateTime(2010, 09, 28, 16, 00, 00), new DateTime(2010, 09, 28, 17, 00, 00));
			ic.Add(new DateTime(2010, 09, 28, 18, 00, 00), new DateTime(2010, 09, 28, 19, 00, 00));
			ic.Remove(new DateTime(2010, 09, 28, 11, 00, 00), new DateTime(2010, 09, 28, 20, 00, 00));

			//Act
			var duration = ic.Duration();
			var intervals = ic.GetIntervals();

			//Assert
			Assert.Equal(TimeSpan.Zero, duration);
			Assert.Empty(intervals);
		}

		[Fact]
		public void RemoveLastInterval()
		{
			//Arrange
			var ic = new IntervalConcatenator();
			ic.Add(new DateTime(2010, 09, 28, 13, 00, 00), new DateTime(2010, 09, 28, 15, 00, 00));
			ic.Add(new DateTime(2010, 09, 28, 16, 00, 00), new DateTime(2010, 09, 28, 17, 00, 00));
			ic.Remove(new DateTime(2010, 09, 28, 15, 30, 00), new DateTime(2010, 09, 28, 18, 00, 00));

			//Act
			var duration = ic.Duration();
			var intervals = ic.GetIntervals();

			//Assert
			Assert.Equal(TimeSpan.FromHours(2), duration);
			Assert.True(intervals.SequenceEqual(new[] { 
				new IntervalConcatenator.Interval(new DateTime(2010, 09, 28, 13, 00, 00), new DateTime(2010, 09, 28, 15, 00, 00)),
			}));
		}

		[Fact]
		public void RemoveFirstInterval()
		{
			//Arrange
			var ic = new IntervalConcatenator();
			ic.Add(new DateTime(2010, 09, 28, 13, 00, 00), new DateTime(2010, 09, 28, 15, 00, 00));
			ic.Add(new DateTime(2010, 09, 28, 16, 00, 00), new DateTime(2010, 09, 28, 17, 00, 00));
			ic.Remove(new DateTime(2010, 09, 28, 12, 30, 00), new DateTime(2010, 09, 28, 15, 30, 00));

			//Act
			var duration = ic.Duration();
			var intervals = ic.GetIntervals();

			//Assert
			Assert.Equal(TimeSpan.FromHours(1), duration);
			Assert.True(intervals.SequenceEqual(new[] { 
				new IntervalConcatenator.Interval(new DateTime(2010, 09, 28, 16, 00, 00), new DateTime(2010, 09, 28, 17, 00, 00)),
			}));
		}


		[Fact]
		public void RemoveNoIntervalBetween()
		{
			//Arrange
			var ic = new IntervalConcatenator();
			ic.Add(new DateTime(2010, 09, 28, 13, 00, 00), new DateTime(2010, 09, 28, 15, 00, 00));
			ic.Add(new DateTime(2010, 09, 28, 16, 00, 00), new DateTime(2010, 09, 28, 17, 00, 00));
			ic.Remove(new DateTime(2010, 09, 28, 15, 00, 00), new DateTime(2010, 09, 28, 16, 00, 00));

			//Act
			var duration = ic.Duration();
			var intervals = ic.GetIntervals();

			//Assert
			Assert.Equal(TimeSpan.FromHours(3), duration);
			Assert.True(intervals.SequenceEqual(new[] { 
				new IntervalConcatenator.Interval(new DateTime(2010, 09, 28, 13, 00, 00), new DateTime(2010, 09, 28, 15, 00, 00)),
				new IntervalConcatenator.Interval(new DateTime(2010, 09, 28, 16, 00, 00), new DateTime(2010, 09, 28, 17, 00, 00)),
			}));
		}

		[Fact]
		public void Remove2NdFrom3Intervals()
		{
			//Arrange
			var ic = new IntervalConcatenator();
			ic.Add(new DateTime(2010, 09, 28, 13, 00, 00), new DateTime(2010, 09, 28, 15, 00, 00));
			ic.Add(new DateTime(2010, 09, 28, 16, 00, 00), new DateTime(2010, 09, 28, 17, 00, 00));
			ic.Add(new DateTime(2010, 09, 28, 18, 00, 00), new DateTime(2010, 09, 28, 19, 00, 00));
			ic.Remove(new DateTime(2010, 09, 28, 15, 00, 00), new DateTime(2010, 09, 28, 17, 00, 00));

			//Act
			var duration = ic.Duration();
			var intervals = ic.GetIntervals();

			//Assert
			Assert.Equal(TimeSpan.FromHours(3), duration);
			Assert.True(intervals.SequenceEqual(new[] { 
				new IntervalConcatenator.Interval(new DateTime(2010, 09, 28, 13, 00, 00), new DateTime(2010, 09, 28, 15, 00, 00)),
				new IntervalConcatenator.Interval(new DateTime(2010, 09, 28, 18, 00, 00), new DateTime(2010, 09, 28, 19, 00, 00)),
			}));
		}

		[Fact]
		public void RemoveZeroIntervalFromMiddleWontSplit()
		{
			//Arrange
			var ic = new IntervalConcatenator();
			ic.Add(new DateTime(2010, 09, 28, 11, 00, 00), new DateTime(2010, 09, 28, 15, 00, 00));
			ic.Remove(new DateTime(2010, 09, 28, 13, 00, 00), new DateTime(2010, 09, 28, 13, 00, 00));

			//Act
			var duration = ic.Duration();
			var intervals = ic.GetIntervals();

			//Assert
			Assert.Equal(TimeSpan.FromHours(4), duration);
			Assert.True(intervals.SequenceEqual(new[] { 
				new IntervalConcatenator.Interval(new DateTime(2010, 09, 28, 11, 00, 00), new DateTime(2010, 09, 28, 15, 00, 00)),
			}));
		}

		[Fact]
		public void RemoveSomethingFromTwoEndsOfAnIntervalAndTheWholeMiddle()
		{
			//Arrange
			var ic = new IntervalConcatenator();
			ic.Add(new DateTime(2010, 09, 28, 11, 00, 00), new DateTime(2010, 09, 28, 15, 00, 00));
			ic.Add(new DateTime(2010, 09, 28, 16, 00, 00), new DateTime(2010, 09, 28, 17, 00, 00));
			ic.Add(new DateTime(2010, 09, 28, 18, 00, 00), new DateTime(2010, 09, 28, 19, 30, 00));
			ic.Remove(new DateTime(2010, 09, 28, 13, 00, 00), new DateTime(2010, 09, 28, 18, 30, 00));

			//Act
			var duration = ic.Duration();
			var intervals = ic.GetIntervals();

			//Assert
			Assert.Equal(TimeSpan.FromHours(3), duration);
			Assert.True(intervals.SequenceEqual(new[] { 
				new IntervalConcatenator.Interval(new DateTime(2010, 09, 28, 11, 00, 00), new DateTime(2010, 09, 28, 13, 00, 00)),
				new IntervalConcatenator.Interval(new DateTime(2010, 09, 28, 18, 30, 00), new DateTime(2010, 09, 28, 19, 30, 00)),
			}));
		}
		#endregion

		#region Merge tests - trivial
		[Fact]
		public void MergeEmptyWithEmptyIsEmpty()
		{
			//Arrange
			var ic = new IntervalConcatenator();
			var ic2 = new IntervalConcatenator();

			//Act
			ic.Merge(ic2);
			var duration = ic.Duration();
			var intervals = ic.GetIntervals();

			//Assert
			Assert.Equal(TimeSpan.Zero, duration);
			Assert.Empty(intervals);
		}

		[Fact]
		public void MergeFiveComplexIntervals()
		{
			//Arrange
			var ic = new IntervalConcatenator();
			var ic2 = new IntervalConcatenator();
			var ic3 = new IntervalConcatenator();
			var ic4 = new IntervalConcatenator();
			var ic5 = new IntervalConcatenator();
			ic.Add(new DateTime(2010, 05, 12, 11, 10, 00), new DateTime(2010, 05, 12, 13, 00, 00));
			ic2.Add(new DateTime(2010, 05, 12, 11, 10, 00), new DateTime(2010, 05, 12, 12, 30, 00));
			ic3.Add(new DateTime(2010, 05, 12, 12, 31, 00), new DateTime(2010, 05, 12, 13, 30, 00));
			ic4.Add(new DateTime(2010, 05, 12, 13, 29, 00), new DateTime(2010, 05, 12, 14, 30, 00));
			ic5.Add(new DateTime(2010, 05, 12, 14, 31, 00), new DateTime(2010, 05, 12, 17, 11, 00));

			//Act
			ic.Merge(ic2).Merge(ic3.Merge(ic4)).Merge(ic5);

			//Assert
			Assert.Equal(TimeSpan.FromHours(6), ic.Duration());
		}

		[Fact]
		public void MergeThreeIntervals()
		{
			//Arrange
			var ic = new IntervalConcatenator();
			var ic2 = new IntervalConcatenator();
			ic.Add(new DateTime(2015, 03, 11, 9, 25, 18), new DateTime(2015, 03, 11, 10, 54, 52));
			ic2.Add(new DateTime(2015, 03, 11, 9, 20, 00), new DateTime(2015, 03, 11, 9, 25, 18));
			ic2.Add(new DateTime(2015, 03, 11, 10, 00, 00), new DateTime(2015, 03, 11, 10, 30, 00));

			//Act
			ic.Merge(ic2);

			//Assert
			var ivs = ic.GetIntervals();
			Assert.Equal(1, ivs.Count);
			Assert.Equal(new IntervalConcatenator.Interval(new DateTime(2015, 03, 11, 9, 20, 00), new DateTime(2015, 03, 11, 10, 54, 52)), ivs[0]);
		}
		#endregion

		#region Subtract tests - trivial
		[Fact]
		public void SubtractEmptyWithEmptyIsEmpty()
		{
			//Arrange
			var ic = new IntervalConcatenator();
			var ic2 = new IntervalConcatenator();

			//Act
			ic.Subtract(ic2);
			var duration = ic.Duration();
			var intervals = ic.GetIntervals();

			//Assert
			Assert.Equal(TimeSpan.Zero, duration);
			Assert.Empty(intervals);
		}

		[Fact]
		public void SubtractSomethingFromTwoEndsOfAnIntervalAndTheWholeMiddle()
		{
			//Arrange
			var ic = new IntervalConcatenator();
			ic.Add(new DateTime(2010, 09, 28, 11, 00, 00), new DateTime(2010, 09, 28, 15, 00, 00));
			ic.Add(new DateTime(2010, 09, 28, 16, 00, 00), new DateTime(2010, 09, 28, 17, 00, 00));
			ic.Add(new DateTime(2010, 09, 28, 18, 00, 00), new DateTime(2010, 09, 28, 19, 30, 00));
			var ic2 = new IntervalConcatenator();
			ic2.Add(new DateTime(2010, 09, 28, 13, 00, 00), new DateTime(2010, 09, 28, 18, 30, 00));

			//Act
			ic.Subtract(ic2);
			var duration = ic.Duration();
			var intervals = ic.GetIntervals();

			//Assert
			Assert.Equal(TimeSpan.FromHours(3), duration);
			Assert.True(intervals.SequenceEqual(new[] { 
				new IntervalConcatenator.Interval(new DateTime(2010, 09, 28, 11, 00, 00), new DateTime(2010, 09, 28, 13, 00, 00)),
				new IntervalConcatenator.Interval(new DateTime(2010, 09, 28, 18, 30, 00), new DateTime(2010, 09, 28, 19, 30, 00)),
			}));
		}
		#endregion

		#region Interval tests
		[Fact]
		public void IntervalEquals()
		{
			Assert.Equal(
				new IntervalConcatenator.Interval
				(
					new DateTime(2010, 09, 28, 11, 00, 00),
					new DateTime(2010, 09, 28, 18, 30, 00)
				),
				new IntervalConcatenator.Interval
				(
					new DateTime(2010, 09, 28, 11, 00, 00),
					new DateTime(2010, 09, 28, 18, 30, 00)
				)
				);
		}

		[Fact]
		public void StartDateComparerEquals()
		{
			Assert.Equal(0,
			IntervalConcatenator.Interval.StartDateComparer.Compare(
				new IntervalConcatenator.Interval
				(
					new DateTime(2010, 09, 28, 11, 00, 00),
					new DateTime(2010, 09, 28, 18, 30, 00)
				),
				new IntervalConcatenator.Interval
				(
					new DateTime(2010, 09, 28, 11, 00, 00),
					new DateTime(2010, 09, 28, 18, 30, 00)
				)
				));

			Assert.Equal(-1,
			IntervalConcatenator.Interval.StartDateComparer.Compare(
						new IntervalConcatenator.Interval
				(
					new DateTime(2010, 09, 28, 10, 00, 00),
					new DateTime(2010, 09, 28, 17, 30, 00)
				),
				new IntervalConcatenator.Interval
				(
					new DateTime(2010, 09, 28, 11, 00, 00),
					new DateTime(2010, 09, 28, 18, 30, 00)
				)
				));

			Assert.Equal(1,
			IntervalConcatenator.Interval.StartDateComparer.Compare(
				new IntervalConcatenator.Interval
				(
					new DateTime(2010, 09, 28, 12, 00, 00),
					new DateTime(2010, 09, 28, 19, 30, 00)
				),
				new IntervalConcatenator.Interval
				(
					new DateTime(2010, 09, 28, 11, 00, 00),
					new DateTime(2010, 09, 28, 18, 30, 00)
				)
				));

			//fails because of Debug.Assert
			//Assert.Equal(0, IntervalConcatenator.Interval.StartDateComparer.Compare(null, null));
			//Assert.Equal(-1, IntervalConcatenator.Interval.StartDateComparer.Compare(null, new IntervalConcatenator.Interval()
			//{
			//    StartDate = new DateTime(2010, 09, 28, 11, 00, 00),
			//    EndDate = new DateTime(2010, 09, 28, 18, 30, 00)
			//}));
			//Assert.Equal(1, IntervalConcatenator.Interval.StartDateComparer.Compare(new IntervalConcatenator.Interval()
			//{
			//    StartDate = new DateTime(2010, 09, 28, 11, 00, 00),
			//    EndDate = new DateTime(2010, 09, 28, 18, 30, 00)
			//}, null));
		}

		[Fact]
		public void StartDateComparerStartMatch()
		{
			Assert.Equal(0,
			IntervalConcatenator.Interval.StartDateComparer.Compare(
				new IntervalConcatenator.Interval
				(
					new DateTime(2010, 09, 28, 11, 00, 00),
					new DateTime(2010, 09, 28, 18, 30, 00)
				),
				new IntervalConcatenator.Interval
				(
					new DateTime(2010, 09, 28, 11, 00, 00),
					new DateTime(2010, 09, 28, 13, 33, 33)
				)
				));

			Assert.Equal(-1,
			IntervalConcatenator.Interval.StartDateComparer.Compare(
				new IntervalConcatenator.Interval
				(
					new DateTime(2010, 09, 28, 10, 00, 00),
					new DateTime(2010, 09, 28, 18, 30, 00)
				),
				new IntervalConcatenator.Interval
				(
					new DateTime(2010, 09, 28, 11, 00, 00),
					new DateTime(2010, 09, 28, 13, 33, 33)
				)
				));

			Assert.Equal(1,
			IntervalConcatenator.Interval.StartDateComparer.Compare(
				new IntervalConcatenator.Interval
				(
					new DateTime(2010, 09, 28, 12, 00, 00),
					new DateTime(2010, 09, 28, 18, 30, 00)
				),
				new IntervalConcatenator.Interval
				(
					new DateTime(2010, 09, 28, 11, 00, 00),
					new DateTime(2010, 09, 28, 13, 33, 33)
				)
				));
		}

		#endregion

		#region Regression
		[Fact]
		public void BinarySearchWontFindFirstMatching()
		{
			var s = new List<int>() { 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
			Assert.NotEqual(2, s.BinarySearch(1));
		}

		[Fact]
		public void Regression()
		{
			RegressionImpl(0, 50000, 1000, 100, 2, TimeSpan.Parse("10:42:00"), 12);
			RegressionImpl(1, 1000, 1000, 100, 10, TimeSpan.Parse("15:45:00"), 8);
			RegressionImpl(2, 10000, 100, 10, 2, TimeSpan.Parse("00:50:00"), 7);
			RegressionImpl(3, 1000, 1000, 100, 4, TimeSpan.Parse("11:11:00"), 8);
			RegressionImpl(4, 1000, 500, 100, 3, TimeSpan.Parse("07:50:00"), 7);
			RegressionImpl(5, 1000, 700, 100, 7, TimeSpan.Parse("09:44:00"), 7);
			RegressionImpl(6, 1000, 500, 50, 5, TimeSpan.Parse("07:25:00"), 6);
		}

		public void RegressionImpl(int seed, int it, int interval, int len, int delProb, TimeSpan expectedDuration, int expectedCount)
		{
			var now = new DateTime(2014, 03, 01);
			var ic = new IntervalConcatenator();
			var rnd = new Random(seed);
			for (int i = 0; i < it; i++)
			{
				var start = now.AddMinutes(rnd.Next(interval));
				var end = start.AddMinutes(rnd.Next(len));
				var del = rnd.Next(delProb) == 0;
				if (del)
				{
					ic.Remove(start, end);
				}
				else
				{
					ic.Add(start, end);
				}
			}
			var res = ic.Duration();
			var res2 = ic.GetIntervals().Count;
			Console.WriteLine(interval + " " + " " + res2 + " " + len + " " + " " + delProb + " " + res);
			Assert.Equal(expectedDuration, res);
			Assert.Equal(expectedCount, res2);
		}

		[Fact]
		public void RegressionManualAdd()
		{
			var now = new DateTime(2014, 03, 01);
			var ic = new IntervalConcatenator();
			var ic2 = new IntervalConcatenator();

			ic.Add(now.AddMinutes(0), now.AddMinutes(1));
			ic.Add(now.AddMinutes(2), now.AddMinutes(3));
			ic.Add(now.AddMinutes(4), now.AddMinutes(5));
			ic.Add(now.AddMinutes(6), now.AddMinutes(7));
			ic.Add(now.AddMinutes(8), now.AddMinutes(9));

			ic.Add(now.AddMinutes(26), now.AddMinutes(27));
			ic.Add(now.AddMinutes(24), now.AddMinutes(25));
			ic.Add(now.AddMinutes(22), now.AddMinutes(23));
			ic.Add(now.AddMinutes(20), now.AddMinutes(21));

			ic2.Add(now.AddMinutes(0), now.AddMinutes(10));
			ic2.Add(now.AddMinutes(24), now.AddMinutes(26.5));

			ic.Merge(ic2);

			Assert.Equal(TimeSpan.FromMinutes(15), ic.Duration());
			Assert.Equal(4, ic.GetIntervals().Count);
		}

		[Fact]
		public void RegressionManualRemove()
		{
			var now = new DateTime(2014, 03, 01);
			var ic = new IntervalConcatenator();
			var ic2 = new IntervalConcatenator();

			ic.Add(now.AddMinutes(0), now.AddMinutes(1));
			ic.Add(now.AddMinutes(2), now.AddMinutes(3));
			ic.Add(now.AddMinutes(4), now.AddMinutes(5));
			ic.Add(now.AddMinutes(6), now.AddMinutes(7));
			ic.Add(now.AddMinutes(8), now.AddMinutes(9));

			ic.Add(now.AddMinutes(26), now.AddMinutes(27));
			ic.Add(now.AddMinutes(24), now.AddMinutes(25));
			ic.Add(now.AddMinutes(22), now.AddMinutes(23));
			ic.Add(now.AddMinutes(20), now.AddMinutes(21));

			ic2.Add(now.AddMinutes(0), now.AddMinutes(10));
			ic2.Add(now.AddMinutes(24), now.AddMinutes(26.5));

			ic.Subtract(ic2);

			Assert.Equal(TimeSpan.FromMinutes(2.5), ic.Duration());
			Assert.Equal(3, ic.GetIntervals().Count);
		}

		[Fact]
		public void RegressionIntervalCount()
		{
			var now = new DateTime(2014, 03, 01);
			var ic = new IntervalConcatenator();
			var ic2 = new IntervalConcatenator();

			ic.Add(now.AddMinutes(0), now.AddMinutes(1));
			ic.Add(now.AddMinutes(2), now.AddMinutes(3));
			ic.Add(now.AddMinutes(4), now.AddMinutes(5));
			ic.Add(now.AddMinutes(6), now.AddMinutes(7));
			ic.Add(now.AddMinutes(8), now.AddMinutes(9));

			ic2.Add(now.AddMinutes(0), now.AddMinutes(10));
			ic2.Add(now.AddMinutes(10), now.AddMinutes(15));

			ic.Merge(ic2);

			Assert.Equal(TimeSpan.FromMinutes(15), ic.Duration());
			Assert.Equal(1, ic.GetIntervals().Count);
		}

		#endregion

		#region Performance
		[Fact]
		public void SubstractFromLot()
		{
			var st = Environment.TickCount;
			var now = new DateTime(2014, 03, 01);
			var ic = new IntervalConcatenator();
			var icDel = new IntervalConcatenator();
			var until = now.AddMonths(12);
			int i = 0;
			for (DateTime date = now; date < until; date = date.AddSeconds(60))
			{
				i++;
				ic.Add(date, date.AddSeconds(59));
			}

			int d = 0;
			for (DateTime date = now; date < until; date = date.AddDays(1))
			{
				d++;
				icDel.Add(date.AddSeconds(3), date.AddSeconds(3).AddHours(1));
			}

			Console.WriteLine("init (" + i + ") " + (Environment.TickCount - st));
			st = Environment.TickCount;
			ic.Subtract(icDel);
			Console.WriteLine("sub (" + d + ") " + (Environment.TickCount - st));
			Console.WriteLine(ic.Duration().ToString());
			//120 month //Assert.Equal(TimeSpan.Parse("3442.10:41:00"), ic.Duration());
			Assert.Equal(TimeSpan.Parse("343.23:05:00"), ic.Duration());

			//init (5260320) 5865
			//sub (3653) 1353792 (old)
			// vs
			//sub (3653) 250 (new)
		}

		[Fact]
		public void SubstractManyFromOne()
		{
			var now = new DateTime(2014, 07, 29);
			var icDel = new IntervalConcatenator();
			var until = now.AddMonths(12);

			for (DateTime date = now; date < until; date = date.AddDays(1))
			{
				icDel.Add(date.AddSeconds(3), date.AddSeconds(3).AddHours(1));
			}

			var st = Environment.TickCount;
			var res = TimeSpan.Zero;
			for (DateTime date = now; date < until; date = date.AddSeconds(60))
			{
				var ic = new IntervalConcatenator();
				ic.Add(date, date.AddSeconds(59));
				ic.Subtract(icDel);
				res += ic.Duration();
			}

			Console.WriteLine(icDel.GetIntervals().Count);
			Console.WriteLine("exec " + (Environment.TickCount - st));
			Console.WriteLine(res);
			//120 month //Assert.Equal(TimeSpan.Parse("3442.10:41:00"), res);
			Assert.Equal(TimeSpan.Parse("343.23:05:00"), res);
			//exec 60544
			//vs
			//exec 780
		}
		#endregion
	}
}
