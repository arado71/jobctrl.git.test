using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Tct.ActivityRecorderService.Notifications;

namespace Tct.ActivityRecorderService.Proxy
{
	[ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IProxyCallback))]
	public interface IProxyService : IProxyServiceBase
	{
		[OperationContract(IsOneWay = false)]
		void InitiateChannel();
	}

	/// <summary>
	/// "Dummmy" base service contract is needed for callback contract inheritance
	/// https://social.msdn.microsoft.com/Forums/vstudio/en-US/ef896836-dec1-4fa6-9956-e3a4958643ce/inheritance-not-supported-on-callback-contracts?forum=wcf
	/// </summary>
	[ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IActivityRecorder))]
	// ReSharper disable once SeviceContractWithoutOperations
	public interface IProxyServiceBase : IProxyServiceBaseBase
	{

	}
	[ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(INotificationService))]
	// ReSharper disable once SeviceContractWithoutOperations
	public interface IProxyServiceBaseBase
	{

	}

}