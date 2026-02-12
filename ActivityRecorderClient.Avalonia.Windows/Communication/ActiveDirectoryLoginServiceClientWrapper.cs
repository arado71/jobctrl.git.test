using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using log4net;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Configuration;

namespace Tct.ActivityRecorderClient.Communication
{
	public class ActiveDirectoryLoginServiceClientWrapper : IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public ActiveDirectoryLoginServiceClient Client { get; private set; }

		public static bool IsActiveDirectoryAuthEnabled => AppConfig.Current.ActiveDirectoryEndpointConfigurations.Count > 0;

		public ActiveDirectoryLoginServiceClientWrapper()
		{
			if (AppConfig.Current.ActiveDirectoryEndpointConfigurations.Count == 0) throw new InvalidOperationException("There is no endpoint for ActiveDirectoryLoginService.");

			GetClient(AppConfig.Current.ActiveDirectoryEndpointConfigurations.Values.OrderBy(e => e.Order).First());
		}

		public ActiveDirectoryLoginServiceClientWrapper(EndpointConfiguration endpointConfig)
		{
			GetClient(endpointConfig);
		}

		private void GetClient(EndpointConfiguration endpointConfig)
		{
			Client = endpointConfig.CreateClient<ActiveDirectoryLoginServiceClient, IActiveDirectoryLoginService>((b, e) => new ActiveDirectoryLoginServiceClient(b, e));
		}

		public void Dispose()
		{
			WcfClientDisposeHelper.Dispose(Client);
		}
	}
}
