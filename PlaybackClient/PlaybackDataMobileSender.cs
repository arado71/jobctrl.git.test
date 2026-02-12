using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using PlaybackClient.MobileServiceReference;
using log4net;

namespace PlaybackClient
{
	public class PlaybackDataMobileSender : PlaybackDataBaseSender<MobileJobcontrolServiceClient>
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		static PlaybackDataMobileSender()
		{
			Debug.Assert(typeof(IMobileJobcontrolService).IsAssignableFrom(typeof(MobileJobcontrolServiceClient)));
		}

		public PlaybackDataMobileSender()
			: base(log)
		{
		}

		protected override MobileJobcontrolServiceClient GetWcfClient()
		{
			return new MobileJobcontrolServiceClient();
		}

		protected override bool CanHandleItem(PlaybackDataItem item)
		{
			return item.MobileRequest != null;
		}

		protected override IAsyncResult BeginSendItem(AsyncCallback callback, AsyncData data)
		{
			return ((IMobileJobcontrolService)data.Client).BeginUploadData_v4(data.Data.MobileRequest, callback, data);
		}

		protected override void EndSendItem(IAsyncResult ar)
		{
			var data = (AsyncData)ar.AsyncState;
			try
			{
				((IMobileJobcontrolService)data.Client).EndUploadData_v4(ar); //we don't care about acks atm.
				log.Debug("Successfully sent " + data.Data);
			}
			catch (Exception ex)
			{
				log.Error("UploadData failed " + data.Data, ex);
				EnqueueRetryItem(data.Data);
			}
		}
	}
}
