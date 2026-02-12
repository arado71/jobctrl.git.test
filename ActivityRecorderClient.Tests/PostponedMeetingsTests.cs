using System;
using System.Diagnostics;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Meeting;
using Xunit;

namespace Tct.Tests.ActivityRecorderClient
{
	public class PostponedMeetingsTests
	{
		[Fact]
		public void BulkCreate()
		{
			for (int i = 1; i < 5; i++)
			{
				ManualMeetingData mmd = new ManualMeetingData
				{
					Description = "test meeting",
					StartTime = DateTime.Now.AddMinutes(i - 5),
					EndTime = DateTime.Now
				};
				ManualMeetingItem wi = new ManualMeetingItem(mmd) { Id = Guid.NewGuid(), UserId = 13 };
				Debug.Assert(PostponedMeetingsManager.Save(wi), "Save error");
			}
		}
		[Fact]
		public void LoadItems()
		{
			PostponedMeetingsManager.Items(e => Debug.Assert(e.UserId == 13, "Loading failed"));
		}
	}
}
