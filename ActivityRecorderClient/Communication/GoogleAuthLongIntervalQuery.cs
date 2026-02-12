using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Capturing.Core;
using Tct.ActivityRecorderClient.Google;

namespace Tct.ActivityRecorderClient.Communication
{
	class GoogleAuthLongIntervalQuery: ILongIntervalQuery
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public void DoWork()
		{
			try
			{
				if(ConfigManager.IsGoogleCalendarTrackingEnabled)
					GoogleCredentialManager.GetNewCredentialsIfNeeded(true, false);
			}
			catch (Exception ex)
			{
				log.Debug("Something went wrong when getting and sending auth data to the server.", ex);
			}
		}
	}
}
