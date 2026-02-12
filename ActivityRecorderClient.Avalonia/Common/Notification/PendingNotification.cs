using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;

namespace Tct.ActivityRecorderClient.Notification
{
	[DataContract]
	public partial class PendingNotification
	{
		[DataMember]
		public Guid Id { get; private set; }
		[DataMember]
		public NotificationData Data { get; private set; }
		[DataMember]
		public NotificationResult Result { get; private set; }

		[IgnoreDataMember]
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
