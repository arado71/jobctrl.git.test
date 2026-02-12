using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Markup;
using Reporter.Model;
using Reporter.Model.WorkItems;
using Reporter.Processing;
using Xunit;

namespace Reporting.Test
{
	public class IntervalTests
	{
		[Fact]
		public void ValidInterval()
		{
			Assert.DoesNotThrow(() =>
			{
				var i = new Interval
				{
					StartDate = new DateTime(2000, 1, 1, 12, 0, 0),
					EndDate = new DateTime(2000, 1, 1, 12, 1, 0),
				};
				var s = i.StartDate;
				var e = i.EndDate;
			});
		}

		[Fact]
		public void IntervalNoLength()
		{
			Assert.DoesNotThrow(() =>
			{
				var i = new Interval
				{
					StartDate = new DateTime(2000, 1, 1, 12, 0, 0),
					EndDate = new DateTime(2000, 1, 1, 12, 0, 0),
				};
				var s = i.StartDate;
				var e = i.EndDate;
			});
		}

		[Fact(Skip="Contract checking")]
		public void InvalidInterval()
		{
			var i = new Interval
			{
				StartDate = new DateTime(2000, 1, 1, 12, 1, 0),
				EndDate = new DateTime(2000, 1, 1, 12, 0, 0),
			};
			var s = i.StartDate;
			var e = i.EndDate;
		}
	}
}
