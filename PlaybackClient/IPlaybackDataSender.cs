using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlaybackClient
{
	public interface IPlaybackDataSender
	{
		void SendAsync(List<PlaybackDataItem> items);
	}
}
