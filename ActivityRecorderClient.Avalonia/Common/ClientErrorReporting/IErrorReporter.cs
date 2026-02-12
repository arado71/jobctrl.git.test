using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.ActivityRecorderClient.ClientErrorReporting
{
	public interface IErrorReporter
	{
		bool ReportClientError(string description, bool attachLogs, Action<ReportingProgress> reportProgress = null, Func<bool> getCancellationPending = null);
		void LogSystemInfo();
	}
}
