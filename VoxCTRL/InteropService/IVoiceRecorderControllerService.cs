using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace VoxCTRL.InteropService
{
	[ServiceContract]
	public interface IVoiceRecorderControllerService
	{
		[OperationContract]
		bool Record(string name);

		[OperationContract]
		bool Pause();

		[OperationContract]
		bool Resume();

		[OperationContract]
		bool Stop();

		[OperationContract]
		bool StopAndDelete();

		[OperationContract]
		void ChangeName(string name);
	}
}
