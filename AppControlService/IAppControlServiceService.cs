using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace AppControlService
{
	[ServiceContract]
	public interface IAppControlServiceService
	{
		[OperationContract]
		void RegisterProcess(int pid);

		[OperationContract]
		void UnregisterProcess(int pid);
	}
}
