using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using log4net;

namespace Tct.ActivityRecorderClient.Communication
{
	/// <summary>
	/// Class for periodically searching for the best possible endpoint
	/// </summary>
	/// <remarks>
	/// Lot of optimizations could have been done. eg.: 
	///  - if we are using the best possible endpoint wihout any error then there is no need to refresh
	///  - if we are offline then we could refresh less frequently
	/// Since we need the endpoint choosing logic in the AuthenticationHelper.TryAuthenticate method we simply call that method.
	/// </remarks>
	public class PreferredEndpointManager : PeriodicManager
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly int endpointDetectionInterval = (int)TimeSpan.FromMinutes(5).TotalMilliseconds;

		public PreferredEndpointManager()
			: base(log)
		{
		}

		protected override int ManagerCallbackInterval
		{
			get { return endpointDetectionInterval; }
		}

		protected override void ManagerCallbackImpl()
		{
			if (ConfigManager.UserId == ConfigManager.LoggedOutUserId) return; //don't do anything until the user is not logged in
			AuthenticationHelper.DetectPreferredEndpoint(ConfigManager.UserId.ToString(CultureInfo.InvariantCulture), ConfigManager.UserPassword);
		}
	}
}
