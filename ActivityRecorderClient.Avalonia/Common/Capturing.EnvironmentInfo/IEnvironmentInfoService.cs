using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.Capturing.EnvironmentInfo
{
	public interface IEnvironmentInfoService
	{
		bool IsRemoteDesktop { get; }
		bool IsVirtualMachine { get; }
		int ComputerId { get; }
		bool IsNet4Available { get; }
		bool IsNet45Available { get; }
		Version OSVersion { get; }
		string OSFullName { get; }
		int HighestNetVersionAvailable { get; }
		RemoteDesktop GetCurrentRemoteDesktop();
	}
}
