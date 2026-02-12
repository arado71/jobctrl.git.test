using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.Controller
{
	public interface IMutualWorkTypeService
	{
		bool IsWorking { get; }
		string StateString { get; }
		MutualWorkTypeInfo RequestStopWork(bool isForced);
		void RequestKickWork();
	}
}
