using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlaybackClient
{
	public interface IPlaybackDataConverter
	{
		List<PlaybackDataItem> GetActualizedItems(PlaybackData data, DateTime utcNewStartDate, DateTime utcSendFromDate);
	}
}
