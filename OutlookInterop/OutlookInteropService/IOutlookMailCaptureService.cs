using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using Tct.ActivityRecorderClient.Capturing.Mail;

namespace OutlookInteropService
{
	[ServiceContract]
	public interface IOutlookMailCaptureService : IMailCaptureService
	{
		[OperationContract]
		void SetMailTracking(MailTrackingType trackingType, MailTrackingSettings trackingSettings, bool isSafeMailItemCommitUsable);
	}
}
