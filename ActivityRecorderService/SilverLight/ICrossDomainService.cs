using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;

namespace Tct.ActivityRecorderService.SilverLight
{
	[ServiceContract]
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
	public interface ICrossDomainService
	{
		[OperationContract]
		[WebGet(UriTemplate = "ClientAccessPolicy.xml")]
		Message ProvidePolicyFile();

		[OperationContract]
		[WebGet(UriTemplate = "/")]
		Stream GetIndexHtml();

		[OperationContract]
		[WebGet(UriTemplate = "{filename}.xap")]
		Stream GetXap(string filename);

		[OperationContract]
		[WebGet(UriTemplate = "Silverlight.js")]
		Stream GetJavaScript();
	}
}
