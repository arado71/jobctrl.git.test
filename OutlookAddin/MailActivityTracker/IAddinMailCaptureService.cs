using System.ServiceModel;
using OutlookInteropService;
using Tct.ActivityRecorderClient.Capturing.Mail;

namespace MailActivityTracker
{
	[ServiceContract]
	public interface IAddinMailCaptureService : IMailCaptureService
	{
		[OperationContract]
		void FilterMails(string[] keywords);

        [OperationContract]
        void TransferMenuData(byte[] buffer);

        [OperationContract]
		void UpdateMenu(string placeHolder);

		[OperationContract]
		string GetVersion();

		[OperationContract]
		void SetMailTrackingBehavior(bool isTrackingEnabled, bool isSubjectTrackingEnabled);
		[OperationContract]
		void Heartbeat();

		[OperationContract]
		void SetMailTrackingSettings(MailTrackingSettings settings);

		[OperationContract]
		void SetTaskIdSettings(int settings);
	}
}
