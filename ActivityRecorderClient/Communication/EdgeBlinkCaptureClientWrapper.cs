using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Tct.ActivityRecorderClient.ChromeCaptureServiceReference;

namespace Tct.ActivityRecorderClient.Communication
{
	public class EdgeBlinkCaptureClientWrapper : ChromiumCaptureClientWrapperBase
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly EdgeBlinkCaptureClientWrapper shared = new EdgeBlinkCaptureClientWrapper();

		public EdgeBlinkCaptureClientWrapper() : base(log)
		{
		}

		protected override string ServiceEndpointUrl => "net.pipe://localhost/EdgeCaptureService";

		public static T Execute<T>(Func<ChromeCaptureServiceClient, T> command)
		{
			return shared.ExecuteShared<T>(command);
		}
	}
}
