using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tct.ActivityRecorderService;
using Tct.ActivityRecorderService.WorkTimeHistory;
using Xunit;

namespace Tct.Tests.ActivityRecorderService
{
	public class ModifyWorkTimeHelperTests
	{
		private static readonly DateTime now = new DateTime(2015, 03, 13);
		[Fact]
		public void OrderTest()
		{
			var ivs = new[]
			{
				new ManualIntervalModification () { OriginalItem = null, NewItem = new ManualInterval() { ManualWorkItemType= ManualWorkItemTypeEnum.DeleteInterval } },
				new ManualIntervalModification () { OriginalItem = new ManualInterval() { ManualWorkItemType= ManualWorkItemTypeEnum.AddWork }, NewItem = null },
				new ManualIntervalModification () { OriginalItem = new ManualInterval() { ManualWorkItemType= ManualWorkItemTypeEnum.DeleteInterval, StartDate = now, EndDate = now.AddHours(1) } , NewItem = new ManualInterval() { ManualWorkItemType = ManualWorkItemTypeEnum.DeleteInterval, StartDate = now, EndDate = now.AddHours(2) } },

				new ManualIntervalModification () { OriginalItem = new ManualInterval() { ManualWorkItemType= ManualWorkItemTypeEnum.DeleteInterval, StartDate = now, EndDate = now.AddHours(1) } , NewItem = new ManualInterval() { ManualWorkItemType = ManualWorkItemTypeEnum.DeleteInterval, StartDate = now.AddHours(1) ,EndDate = now.AddHours(2) } },
				new ManualIntervalModification () { OriginalItem = new ManualInterval() { ManualWorkItemType= ManualWorkItemTypeEnum.AddWork, StartDate = now, EndDate = now.AddHours(1) } , NewItem = new ManualInterval() { ManualWorkItemType = ManualWorkItemTypeEnum.AddWork, StartDate = now.AddHours(1), EndDate = now.AddHours(2) } },

				new ManualIntervalModification () { OriginalItem = new ManualInterval() { ManualWorkItemType= ManualWorkItemTypeEnum.AddWork, StartDate = now, EndDate = now.AddHours(2) } , NewItem = new ManualInterval() { ManualWorkItemType = ManualWorkItemTypeEnum.AddWork, StartDate = now, EndDate = now.AddHours(1) } },
				new ManualIntervalModification () { OriginalItem = new ManualInterval() { ManualWorkItemType= ManualWorkItemTypeEnum.DeleteInterval, StartDate = now, EndDate = now.AddHours(2) } , NewItem = new ManualInterval() { ManualWorkItemType = ManualWorkItemTypeEnum.DeleteInterval, StartDate = now, EndDate = now.AddHours(1) } },
				new ManualIntervalModification () { OriginalItem = new ManualInterval() { ManualWorkItemType= ManualWorkItemTypeEnum.AddWork, StartDate = now, EndDate = now.AddHours(1) } , NewItem = new ManualInterval() { ManualWorkItemType = ManualWorkItemTypeEnum.AddWork, StartDate = now, EndDate = now.AddHours(2) } },
				new ManualIntervalModification () { OriginalItem = null, NewItem = new ManualInterval() { ManualWorkItemType= ManualWorkItemTypeEnum.AddWork } },
				new ManualIntervalModification () { OriginalItem = new ManualInterval() { ManualWorkItemType= ManualWorkItemTypeEnum.DeleteInterval }, NewItem = null },
			};
			var rnd = new Random(3459712);
			Assert.True(ivs.OrderBy(n => rnd.Next(100000)).Select((n, i) => Object.ReferenceEquals(n, ivs[i])).Any(n => !n));
			Assert.True(ivs.OrderBy(n => rnd.Next(100000)).ToArray().OrderBy(n => n, ModifyWorkTimeHelper.comparer).Select((n, i) => Object.ReferenceEquals(n, ivs[i])).All(n => n));
			Assert.True(ivs.OrderBy(n => n, ModifyWorkTimeHelper.comparer).Select((n, i) => Object.ReferenceEquals(n, ivs[i])).All(n => n));
		}
	}
}
