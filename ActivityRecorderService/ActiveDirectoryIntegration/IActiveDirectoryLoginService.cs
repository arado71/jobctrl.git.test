using System.ServiceModel;

namespace Tct.ActivityRecorderService.ActiveDirectoryIntegration
{
	[ServiceContract]
	public interface IActiveDirectoryLoginService
	{
		[OperationContract]
		ClientLoginTicket GetClientLoginTicket();
	}
}
