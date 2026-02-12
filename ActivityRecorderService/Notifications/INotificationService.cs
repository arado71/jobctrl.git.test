using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace Tct.ActivityRecorderService.Notifications
{
	[ServiceContract(Name = "IActivityRecorder", Namespace = "http://tempuri.org/")]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public interface INotificationService
	{
		[OperationContract]
		NotificationData GetPendingNotification(int userId, int computerId, int? lastId);

		[OperationContract]
		void ConfirmNotification(NotificationResult result);
	}
}
