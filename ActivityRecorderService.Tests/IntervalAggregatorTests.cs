using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderService;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class IntervalAggregatorTests
	{
		private readonly DateTime now = new DateTime(2011, 06, 14, 13, 00, 00);

		[Fact]
		public void EmptyTest()
		{
			var ia = new IntervalAggregator<int>();
			Assert.Empty(ia.GetIntervalsWithKeys());
		}

		[Fact]
		public void AddOneInterval()
		{
			//Arrange
			var ia = new IntervalAggregator<int>();
			ia.Add(1, now, now.AddHours(1));

			//Act 
			var iwk = ia.GetIntervalsWithKeys().ToArray();

			//Assert
			Assert.Equal(1, iwk.Length);
			Assert.Equal(1, iwk[0].Key);
			Assert.Equal(1, iwk[0].Value.Count);
			Assert.Equal(now, iwk[0].Value[0].StartDate);
			Assert.Equal(now.AddHours(1), iwk[0].Value[0].EndDate);
		}

		[Fact]
		public void AddTwoIntervalsWithGap()
		{
			//Arrange
			var ia = new IntervalAggregator<int>();
			ia.Add(1, now, now.AddHours(1));
			ia.Add(1, now.AddHours(2), now.AddHours(3));

			//Act 
			var iwk = ia.GetIntervalsWithKeys().ToArray();

			//Assert
			Assert.Equal(1, iwk.Length);
			Assert.Equal(1, iwk[0].Key);
			Assert.Equal(2, iwk[0].Value.Count);
			Assert.Equal(now, iwk[0].Value[0].StartDate);
			Assert.Equal(now.AddHours(1), iwk[0].Value[0].EndDate);
			Assert.Equal(now.AddHours(2), iwk[0].Value[1].StartDate);
			Assert.Equal(now.AddHours(3), iwk[0].Value[1].EndDate);
		}

		[Fact]
		public void AddTwoIntervalsWithoutGap()
		{
			//Arrange
			var ia = new IntervalAggregator<int>();
			ia.Add(1, now, now.AddHours(1));
			ia.Add(1, now.AddHours(1), now.AddHours(3));

			//Act 
			var iwk = ia.GetIntervalsWithKeys().ToArray();

			//Assert
			Assert.Equal(1, iwk.Length);
			Assert.Equal(1, iwk[0].Key);
			Assert.Equal(1, iwk[0].Value.Count);
			Assert.Equal(now, iwk[0].Value[0].StartDate);
			Assert.Equal(now.AddHours(3), iwk[0].Value[0].EndDate);
		}

		[Fact]
		public void AddTwoIntervalsWithoutGapReverse()
		{
			//Arrange
			var ia = new IntervalAggregator<int>();
			ia.Add(1, now.AddHours(1), now.AddHours(3));
			ia.Add(1, now, now.AddHours(1));

			//Act 
			var iwk = ia.GetIntervalsWithKeys().ToArray();

			//Assert
			Assert.Equal(1, iwk.Length);
			Assert.Equal(1, iwk[0].Key);
			Assert.Equal(1, iwk[0].Value.Count);
			Assert.Equal(now, iwk[0].Value[0].StartDate);
			Assert.Equal(now.AddHours(3), iwk[0].Value[0].EndDate);
		}

		[Fact]
		public void AddThreeIntervalsWithoutGapP1()
		{
			//Arrange
			var ia = new IntervalAggregator<int>();
			ia.Add(1, now, now.AddHours(1));
			ia.Add(1, now.AddHours(1), now.AddHours(2));
			ia.Add(1, now.AddHours(2), now.AddHours(3));

			//Act 
			var iwk = ia.GetIntervalsWithKeys().ToArray();

			//Assert
			Assert.Equal(1, iwk.Length);
			Assert.Equal(1, iwk[0].Key);
			Assert.Equal(1, iwk[0].Value.Count);
			Assert.Equal(now, iwk[0].Value[0].StartDate);
			Assert.Equal(now.AddHours(3), iwk[0].Value[0].EndDate);
		}

		[Fact]
		public void AddThreeIntervalsWithoutGapP2()
		{
			//Arrange
			var ia = new IntervalAggregator<int>();
			ia.Add(1, now, now.AddHours(1));
			ia.Add(1, now.AddHours(2), now.AddHours(3));
			ia.Add(1, now.AddHours(1), now.AddHours(2));

			//Act 
			var iwk = ia.GetIntervalsWithKeys().ToArray();

			//Assert
			Assert.Equal(1, iwk.Length);
			Assert.Equal(1, iwk[0].Key);
			Assert.Equal(1, iwk[0].Value.Count);
			Assert.Equal(now, iwk[0].Value[0].StartDate);
			Assert.Equal(now.AddHours(3), iwk[0].Value[0].EndDate);
		}


		[Fact]
		public void AddThreeIntervalsWithoutGapP3()
		{
			//Arrange
			var ia = new IntervalAggregator<int>();
			ia.Add(1, now.AddHours(1), now.AddHours(2));
			ia.Add(1, now.AddHours(2), now.AddHours(3));
			ia.Add(1, now, now.AddHours(1));

			//Act 
			var iwk = ia.GetIntervalsWithKeys().ToArray();

			//Assert
			Assert.Equal(1, iwk.Length);
			Assert.Equal(1, iwk[0].Key);
			Assert.Equal(1, iwk[0].Value.Count);
			Assert.Equal(now, iwk[0].Value[0].StartDate);
			Assert.Equal(now.AddHours(3), iwk[0].Value[0].EndDate);
		}

		[Fact]
		public void AddThreeIntervalsWithoutGapP4()
		{
			//Arrange
			var ia = new IntervalAggregator<int>();
			ia.Add(1, now.AddHours(1), now.AddHours(2));
			ia.Add(1, now, now.AddHours(1));
			ia.Add(1, now.AddHours(2), now.AddHours(3));

			//Act 
			var iwk = ia.GetIntervalsWithKeys().ToArray();

			//Assert
			Assert.Equal(1, iwk.Length);
			Assert.Equal(1, iwk[0].Key);
			Assert.Equal(1, iwk[0].Value.Count);
			Assert.Equal(now, iwk[0].Value[0].StartDate);
			Assert.Equal(now.AddHours(3), iwk[0].Value[0].EndDate);
		}

		[Fact]
		public void AddThreeIntervalsWithoutGapP5()
		{
			//Arrange
			var ia = new IntervalAggregator<int>();
			ia.Add(1, now.AddHours(2), now.AddHours(3));
			ia.Add(1, now.AddHours(1), now.AddHours(2));
			ia.Add(1, now, now.AddHours(1));

			//Act 
			var iwk = ia.GetIntervalsWithKeys().ToArray();

			//Assert
			Assert.Equal(1, iwk.Length);
			Assert.Equal(1, iwk[0].Key);
			Assert.Equal(1, iwk[0].Value.Count);
			Assert.Equal(now, iwk[0].Value[0].StartDate);
			Assert.Equal(now.AddHours(3), iwk[0].Value[0].EndDate);
		}

		[Fact]
		public void AddThreeIntervalsWithoutGapP6()
		{
			//Arrange
			var ia = new IntervalAggregator<int>();
			ia.Add(1, now.AddHours(2), now.AddHours(3));
			ia.Add(1, now, now.AddHours(1));
			ia.Add(1, now.AddHours(1), now.AddHours(2));

			//Act 
			var iwk = ia.GetIntervalsWithKeys().ToArray();

			//Assert
			Assert.Equal(1, iwk.Length);
			Assert.Equal(1, iwk[0].Key);
			Assert.Equal(1, iwk[0].Value.Count);
			Assert.Equal(now, iwk[0].Value[0].StartDate);
			Assert.Equal(now.AddHours(3), iwk[0].Value[0].EndDate);
		}

		[Fact]
		public void AddThreeIntervalsDifferentKeys()
		{
			//Arrange
			var ia = new IntervalAggregator<int>();
			ia.Add(1, now, now.AddHours(1));
			ia.Add(2, now.AddHours(1), now.AddHours(2));
			ia.Add(3, now.AddHours(2), now.AddHours(3));

			//Act 
			var iwk = ia.GetIntervalsWithKeys().ToArray();

			//Assert
			Assert.Equal(3, iwk.Length);
			Assert.True(new[] { 1, 2, 3 }.SequenceEqual(iwk.Select(n => n.Key).OrderBy(n => n)));
			Assert.Equal(1, iwk[0].Value.Count);
			Assert.Equal(1, iwk[1].Value.Count);
			Assert.Equal(1, iwk[2].Value.Count);
			Assert.Equal(now, iwk.OrderBy(n => n.Key).First().Value[0].StartDate);
			Assert.Equal(now.AddHours(1), iwk.OrderBy(n => n.Key).First().Value[0].EndDate);
			Assert.Equal(now.AddHours(1), iwk.OrderBy(n => n.Key).Skip(1).First().Value[0].StartDate);
			Assert.Equal(now.AddHours(2), iwk.OrderBy(n => n.Key).Skip(1).First().Value[0].EndDate);
			Assert.Equal(now.AddHours(2), iwk.OrderBy(n => n.Key).Skip(2).First().Value[0].StartDate);
			Assert.Equal(now.AddHours(3), iwk.OrderBy(n => n.Key).Skip(2).First().Value[0].EndDate);
		}

		private IntervalAggregator<int> intAggr;
		private List<Tuple<int, StartEndDateTime>> listAggr;

		private void InitInterval()
		{
			listAggr = new List<Tuple<int, StartEndDateTime>>()
			{
				new Tuple<int, StartEndDateTime>(1, new StartEndDateTime(now, now.AddMinutes(5))),
				new Tuple<int, StartEndDateTime>(1, new StartEndDateTime(now.AddMinutes(10), now.AddMinutes(15))),
				new Tuple<int, StartEndDateTime>(1, new StartEndDateTime(now.AddMinutes(20), now.AddMinutes(25))),
			};
			intAggr = new IntervalAggregator<int>();
			foreach (var tuple in listAggr)
			{
				intAggr.Add(tuple.Item1, tuple.Item2.StartDate, tuple.Item2.EndDate);
			}
		}

		[Fact]
		public void RefreshWithNewKey()
		{
			//Arrange
			InitInterval();

			//Act
			intAggr.Refresh(listAggr.Concat(new List<Tuple<int, StartEndDateTime>>() { new Tuple<int, StartEndDateTime>(2, new StartEndDateTime(now.AddMinutes(16), now.AddMinutes(18))) }).ToList());

			//Assert
			var iwk = intAggr.GetIntervalsWithKeys().ToArray();
			Assert.Equal(2, iwk.Length);
			Assert.Equal(2, iwk[1].Key);
			Assert.Equal(now.AddMinutes(16), iwk[1].Value[0].StartDate);
			Assert.Equal(now.AddMinutes(18), iwk[1].Value[0].EndDate);
		}

		[Fact]
		public void RefreshWithNewKeyOnly()
		{
			//Arrange
			InitInterval();

			//Act
			intAggr.Refresh(new List<Tuple<int, StartEndDateTime>>() { new Tuple<int, StartEndDateTime>(2, new StartEndDateTime(now.AddMinutes(16), now.AddMinutes(30))) }.ToList());

			//Assert
			var iwk = intAggr.GetIntervalsWithKeys().ToArray();
			Assert.Equal(1, iwk.Length);
			Assert.Equal(2, iwk[0].Key);
			Assert.Equal(now.AddMinutes(16), iwk[0].Value[0].StartDate);
			Assert.Equal(now.AddMinutes(30), iwk[0].Value[0].EndDate);
		}

		[Fact]
		public void RefreshWithExact()
		{
			//Arrange
			InitInterval();

			//Act
			intAggr.Refresh(listAggr);

			//Assert
			var iwk = intAggr.GetIntervalsWithKeys().ToArray();
			Assert.Equal(1, iwk.Length);
			Assert.Equal(3, iwk[0].Value.Count);
			for (var i = 0; i < listAggr.Count; i++)
			{
				Assert.Equal(listAggr[i].Item2, iwk[0].Value[i]);
			}
		}

		[Fact]
		public void RefreshWithNarrow()
		{
			//Arrange
			InitInterval();
			listAggr[1] = new Tuple<int, StartEndDateTime>(1, new StartEndDateTime(now.AddMinutes(12), now.AddMinutes(13)));

			//Act
			intAggr.Refresh(listAggr);

			//Assert
			var iwk = intAggr.GetIntervalsWithKeys().ToArray();
			Assert.Equal(1, iwk.Length);
			Assert.Equal(3, iwk[0].Value.Count);
			for (var i = 0; i < listAggr.Count; i++)
			{
				Assert.Equal(listAggr[i].Item2, iwk[0].Value[i]);
			}
		}

		[Fact]
		public void RefreshWithExtend()
		{
			//Arrange
			InitInterval();
			listAggr[1] = new Tuple<int, StartEndDateTime>(1, new StartEndDateTime(now.AddMinutes(9), now.AddMinutes(16)));

			//Act
			intAggr.Refresh(listAggr);

			//Assert
			var iwk = intAggr.GetIntervalsWithKeys().ToArray();
			Assert.Equal(1, iwk.Length);
			Assert.Equal(3, iwk[0].Value.Count);
			for (var i = 0; i < listAggr.Count; i++)
			{
				Assert.Equal(listAggr[i].Item2, iwk[0].Value[i]);
			}
		}

		[Fact]
		public void RefreshWithExtendLeft()
		{
			//Arrange
			InitInterval();
			listAggr[1] = new Tuple<int, StartEndDateTime>(1, new StartEndDateTime(now.AddMinutes(9), now.AddMinutes(13)));

			//Act
			intAggr.Refresh(listAggr);

			//Assert
			var iwk = intAggr.GetIntervalsWithKeys().ToArray();
			Assert.Equal(1, iwk.Length);
			Assert.Equal(3, iwk[0].Value.Count);
			for (var i = 0; i < listAggr.Count; i++)
			{
				Assert.Equal(listAggr[i].Item2, iwk[0].Value[i]);
			}
		}

		[Fact]
		public void RefreshWithExtendRight()
		{
			//Arrange
			InitInterval();
			listAggr[1] = new Tuple<int, StartEndDateTime>(1, new StartEndDateTime(now.AddMinutes(12), now.AddMinutes(16)));

			//Act
			intAggr.Refresh(listAggr);

			//Assert
			var iwk = intAggr.GetIntervalsWithKeys().ToArray();
			Assert.Equal(1, iwk.Length);
			Assert.Equal(3, iwk[0].Value.Count);
			for (var i = 0; i < listAggr.Count; i++)
			{
				Assert.Equal(listAggr[i].Item2, iwk[0].Value[i]);
			}
		}

		[Fact]
		public void RefreshWithExtendTwo()
		{
			//Arrange
			InitInterval();
			listAggr[0] = new Tuple<int, StartEndDateTime>(1, new StartEndDateTime(now.AddMinutes(-1), now.AddMinutes(7)));
			listAggr[2] = new Tuple<int, StartEndDateTime>(1, new StartEndDateTime(now.AddMinutes(18), now.AddMinutes(25)));

			//Act
			intAggr.Refresh(listAggr);

			//Assert
			var iwk = intAggr.GetIntervalsWithKeys().ToArray();
			Assert.Equal(1, iwk.Length);
			Assert.Equal(3, iwk[0].Value.Count);
			for (var i = 0; i < listAggr.Count; i++)
			{
				Assert.Equal(listAggr[i].Item2, iwk[0].Value[i]);
			}
		}

		[Fact]
		public void RefreshWithExtendJoin()
		{
			//Arrange
			InitInterval();
			listAggr[1] = new Tuple<int, StartEndDateTime>(1, new StartEndDateTime(now.AddMinutes(9), now.AddMinutes(26)));
			listAggr.RemoveAt(2);

			//Act
			intAggr.Refresh(listAggr);

			//Assert
			var iwk = intAggr.GetIntervalsWithKeys().ToArray();
			Assert.Equal(1, iwk.Length);
			Assert.Equal(2, iwk[0].Value.Count);
			for (var i = 0; i < listAggr.Count; i++)
			{
				Assert.Equal(listAggr[i].Item2, iwk[0].Value[i]);
			}
		}

		[Fact]
		public void RefreshWithNarrowJoin()
		{
			//Arrange
			InitInterval();
			listAggr[1] = new Tuple<int, StartEndDateTime>(1, new StartEndDateTime(now.AddMinutes(12), now.AddMinutes(23)));
			listAggr.RemoveAt(2);

			//Act
			intAggr.Refresh(listAggr);

			//Assert
			var iwk = intAggr.GetIntervalsWithKeys().ToArray();
			Assert.Equal(1, iwk.Length);
			Assert.Equal(2, iwk[0].Value.Count);
			Assert.Equal(listAggr[0].Item2, iwk[0].Value[0]);
			Assert.Equal(listAggr[1].Item2.StartDate, iwk[0].Value[1].StartDate);
			Assert.Equal(now.AddMinutes(25), iwk[0].Value[1].EndDate); // original interval continues after refreshed period
		}

		[Fact]
		public void RefreshWithNewIntervals()
		{
			//Arrange
			InitInterval();
			intAggr.Add(1, now.AddMinutes(30), now.AddMinutes(35));

			//Act
			intAggr.Refresh(listAggr);

			//Assert
			var iwk = intAggr.GetIntervalsWithKeys().ToArray();
			Assert.Equal(1, iwk.Length);
			Assert.Equal(4, iwk[0].Value.Count);
			for (var i = 0; i < listAggr.Count; i++)
			{
				Assert.Equal(listAggr[i].Item2, iwk[0].Value[i]);
			}
			Assert.Equal(now.AddMinutes(30), iwk[0].Value[3].StartDate);
			Assert.Equal(now.AddMinutes(35), iwk[0].Value[3].EndDate);
		}

		[Fact]
		public void RefreshRemoveInterval()
		{
			//Arrange
			InitInterval();
			listAggr.RemoveAt(1);

			//Act
			intAggr.Refresh(listAggr);

			//Assert
			var iwk = intAggr.GetIntervalsWithKeys().ToArray();
			Assert.Equal(1, iwk.Length);
			Assert.Equal(2, iwk[0].Value.Count);
			for (var i = 0; i < listAggr.Count; i++)
			{
				Assert.Equal(listAggr[i].Item2, iwk[0].Value[i]);
			}
		}

		[Fact]
		public void RefreshComplex()
		{
			//Arrange
			listAggr = new List<Tuple<int, StartEndDateTime>>()
			{
				new Tuple<int, StartEndDateTime>(1, new StartEndDateTime(now, now.AddMinutes(5))),
				new Tuple<int, StartEndDateTime>(1, new StartEndDateTime(now.AddMinutes(10), now.AddMinutes(15))),
				new Tuple<int, StartEndDateTime>(1, new StartEndDateTime(now.AddMinutes(20), now.AddMinutes(25))),
				new Tuple<int, StartEndDateTime>(1, new StartEndDateTime(now.AddMinutes(30), now.AddMinutes(40))),
				new Tuple<int, StartEndDateTime>(2, new StartEndDateTime(now.AddMinutes(50), now.AddMinutes(52))),
				new Tuple<int, StartEndDateTime>(2, new StartEndDateTime(now.AddMinutes(55), now.AddMinutes(58))),
				new Tuple<int, StartEndDateTime>(2, new StartEndDateTime(now.AddMinutes(60), now.AddMinutes(70))),
				new Tuple<int, StartEndDateTime>(2, new StartEndDateTime(now.AddMinutes(71), now.AddMinutes(72))),
				new Tuple<int, StartEndDateTime>(2, new StartEndDateTime(now.AddMinutes(75), now.AddMinutes(80))),
			};
			intAggr = new IntervalAggregator<int>();
			foreach (var tuple in listAggr)
			{
				intAggr.Add(tuple.Item1, tuple.Item2.StartDate, tuple.Item2.EndDate);
			}
			listAggr.RemoveAt(0);
			listAggr.RemoveAt(7);
			listAggr[0] = new Tuple<int, StartEndDateTime>(1, new StartEndDateTime(now.AddMinutes(5), now.AddMinutes(41)));
			listAggr.RemoveAt(1);
			listAggr.RemoveAt(1);
			listAggr[1] = new Tuple<int, StartEndDateTime>(2, new StartEndDateTime(now.AddMinutes(45), now.AddMinutes(80)));
			listAggr.RemoveAt(2);
			listAggr.RemoveAt(2);

			//Act
			var lookup = listAggr
				.ToLookup(l => l.Item1, l => l.Item2);
			intAggr.Refresh(listAggr);

			//Assert
			var iwk = intAggr.GetIntervalsWithKeys().ToDictionary(i => i.Key, i => i.Value);
			Assert.Equal(lookup.Count, iwk.Keys.Count);
			foreach (var item in lookup)
			{
				Assert.Equal(item.ToList().Count, iwk[item.Key].Count);
				for (var i = 0; i < item.ToList().Count; i++)
				{
					Assert.Equal(item.ToList()[i], iwk[item.Key][i]);
				}
			}
		}

	}
}
