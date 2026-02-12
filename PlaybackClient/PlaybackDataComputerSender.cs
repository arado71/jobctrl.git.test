using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PlaybackClient.ActivityRecorderServiceReference;
using log4net;

namespace PlaybackClient
{
	public class PlaybackDataComputerSender : PlaybackDataBaseSender<ActivityRecorderClient>
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public PlaybackDataComputerSender()
			: base(log)
		{
		}

		protected override ActivityRecorderClient GetWcfClient()
		{
			var client = new ActivityRecorderClient();
			client.ClientCredentials.UserName.UserName = ConfigManager.ImportUserName;
			client.ClientCredentials.UserName.Password = ConfigManager.ImportPassword;
			return client;
		}

		protected override bool CanHandleItem(PlaybackDataItem item)
		{
			return item.MobileRequest == null;
		}

		protected override IAsyncResult BeginSendItem(AsyncCallback callback, AsyncData data)
		{
			if (data.Data.WorkItem != null)
			{
				return data.Client.BeginAddWorkItemEx(data.Data.WorkItem, callback, data);
			}
			else if (data.Data.ManualWorkItem != null)
			{
				return data.Client.BeginAddManualWorkItem(data.Data.ManualWorkItem, callback, data);
			}
			else
			{
				log.ErrorAndFail("Invalid data " + data.Data);
				return null;
			}
		}

		protected override void EndSendItem(IAsyncResult ar)
		{
			var data = (AsyncData)ar.AsyncState;
			if (data.Data.WorkItem != null)
			{
				EndAddWorkItem(ar);
			}
			else if (data.Data.ManualWorkItem != null)
			{
				EndAddManualWorkItem(ar);
			}
			else
			{
				log.ErrorAndFail("Invalid data " + data.Data);
			}
		}

		private void EndAddWorkItem(IAsyncResult ar)
		{
			var data = (AsyncData)ar.AsyncState;
			try
			{
				data.Client.EndAddWorkItemEx(ar);
				log.Debug("Successfully sent " + data.Data);
			}
			catch (Exception ex)
			{
				log.Error("AddWorkItem failed " + data.Data, ex);
				EnqueueRetryItem(data.Data);
			}
		}

		private void EndAddManualWorkItem(IAsyncResult ar)
		{
			var data = (AsyncData)ar.AsyncState;
			try
			{
				data.Client.EndAddManualWorkItem(ar);
				log.Debug("Successfully sent " + data.Data);
			}
			catch (Exception ex)
			{
				log.Error("AddManualWorkItem failed " + data.Data, ex);
				EnqueueRetryItem(data.Data);
			}
		}
	}
}
