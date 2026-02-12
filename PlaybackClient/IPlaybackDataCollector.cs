using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlaybackClient
{
	public interface IPlaybackDataCollector
	{
		PlaybackData GetDataFor(int userId, DateTime startDate, DateTime endDate);
	}
}
