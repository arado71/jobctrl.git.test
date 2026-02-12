using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Notification
{
	[Serializable]
	public class PendingNotification
	{
		public readonly Guid Id;
		public readonly NotificationData Data;
		public readonly NotificationResult Result;

		public bool IsConfirmed
		{
			get { return Result.ConfirmDate != DateTime.MinValue; }
		}

		public PendingNotification(NotificationData data)
		{
			Id = Guid.NewGuid();
			Data = data;
			Result = new NotificationResult() { Id = data.Id, UserId = ConfigManager.UserId };
		}

		public override string ToString()
		{
			return "id: " + Data.Id + " show: " + Result.ShowDate + " conf: " + Result.ConfirmDate + " res: " + Result.Result;
		}
	}
}
