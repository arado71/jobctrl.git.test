using System.ServiceModel;
using System.ServiceModel.Web;

namespace Tct.ActivityRecorderClient.Capturing.SoftphonePro
{
	[ServiceContract]
	public interface ISoftphoneService
	{
		[WebGet(UriTemplate = "/callfinished?number={number}")]
		[OperationContract]
		void CallFinished(string number);

		[WebGet(UriTemplate = "/callanswered?number={number}")]
		[OperationContract]
		void CallAnswered(string number);

		[WebGet(UriTemplate = "/calloutgoing?number={number}")]
		[OperationContract]
		void CallOutgoing(string number);
	}
}
