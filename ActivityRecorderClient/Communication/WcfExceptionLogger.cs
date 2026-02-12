using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel.Dispatcher;
using System.Text;
using log4net;

namespace Tct.ActivityRecorderClient.Communication
{
	/// <summary>
	/// Class for ignoring and loging wcf exceptions
	/// </summary>
	/// <remarks>
	/// There is a buggy client computer where the network traffic seems to be altered in some way.
	/// And client crashes due to wcf message security verification.
	/// </remarks>
	public class WcfExceptionLogger : ExceptionHandler
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public override bool HandleException(Exception ex)
		{
			log.Fatal("Unhandled WCF exception reached the handler, but ignoring it", ex);
			return true;
		}

		private static volatile bool isStopped;
		public static void Shutdown()
		{
			isStopped = true;
		}

		public static void LogWcfError(string action, ILog log, Exception ex)
		{
			if (log == null) log = WcfExceptionLogger.log;
			if (ex is System.ServiceModel.EndpointNotFoundException)
			{
				log.Debug("Unable to " + action + " because the service is not found", ex);
			}
			else if (ex is TimeoutException)
			{
				log.Debug("Unable to " + action + " because the service call timed out", ex);
			}
			else if (ex is ObjectDisposedException && isStopped)
			{
				log.Debug("Unable to " + action + " because communication is closed", ex);
			}
			else
			{
				log.Error("Unable to " + action, ex);
			}
		}
	}
}
