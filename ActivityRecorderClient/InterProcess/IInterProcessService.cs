using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace Tct.ActivityRecorderClient.InterProcess
{
	[ServiceContract]
	public interface IInterProcessService
	{
		[OperationContract]
		void AddProjectAndWorkByRule(string projectKey, string workName, string workKey, int ruleId);

		[OperationContract]
		void StartWork(int workId);

		[OperationContract]
		void StopWork();

		[OperationContract]
		void SwitchWork(int workId);

		[OperationContract]
		void AddExtText(string text);
	}
}
