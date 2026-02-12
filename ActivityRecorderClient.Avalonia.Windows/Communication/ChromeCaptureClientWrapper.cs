using System;
using System.ServiceModel;
using log4net;
using Tct.ActivityRecorderClient.ChromeCaptureServiceReference;

namespace Tct.ActivityRecorderClient.Communication
{
	public class ChromeCaptureClientWrapper : ChromiumCaptureClientWrapperBase
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly ChromeCaptureClientWrapper shared = new ChromeCaptureClientWrapper();

		public ChromeCaptureClientWrapper() : base(log)
		{
		}

		protected override string ServiceEndpointUrl => "net.pipe://localhost/ChromeCaptureService";

		public static T Execute<T>(Func<ChromeCaptureServiceClient, T> command)
		{
			return shared.ExecuteShared<T>(command);
		}
	}
}
