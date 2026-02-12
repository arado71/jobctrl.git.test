using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Tct.ActivityRecorderService.Proxy
{
	[ServiceContract]
	public interface IProxyCallback : IActivityRecorder
	{
		[OperationContract]
		bool CheckCredential(string userId, string hash);
	}
}
