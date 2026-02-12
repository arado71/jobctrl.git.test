using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using PlaybackClient.ActivityRecorderServiceReference;
using log4net;

namespace PlaybackClient
{
	/// <summary>
	/// Thread-safe class for sending PlaybackDataItems (at the right time) to the server.
	/// We have independent senders for JobCTRL server and Mobile server.
	/// </summary>
	public class PlaybackDataSender : IPlaybackDataSender, IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly PlaybackDataComputerSender computerSender = new PlaybackDataComputerSender();
		private readonly PlaybackDataMobileSender mobileSender = new PlaybackDataMobileSender();

		public void SendAsync(List<PlaybackDataItem> items)
		{
			var sw = Stopwatch.StartNew();
			computerSender.SendAsync(items);
			mobileSender.SendAsync(items);
			log.DebugFormat("SendAsync returned in {0:0.000}ms", sw.Elapsed.TotalMilliseconds);
		}

		public void Dispose()
		{
			computerSender.Dispose();
			mobileSender.Dispose();
			log.Info("PlaybackDataSenders Disposed");
		}
	}
}
