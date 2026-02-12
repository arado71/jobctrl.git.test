using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel.Configuration;
using System.Text;
using log4net;
using VoxCTRL.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Communication;

namespace VoxCTRL
{
	public class ActiveDirectoryLoginServiceClientWrapper : IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly string[] endpointNames;
		public readonly ActiveDirectoryLoginServiceClient Client;

		public static bool IsActiveDirectoryAuthEnabled
		{
			get
			{
				return endpointNames != null && endpointNames.Length > 0;
			}
		}

		public static string[] EndpointNames { get { return (string[])endpointNames.Clone(); } }


		static ActiveDirectoryLoginServiceClientWrapper()
		{
			try
			{
				var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
				var serviceModelSectionGroup = ServiceModelSectionGroup.GetSectionGroup(config);
				endpointNames = serviceModelSectionGroup.Client.Endpoints
					.OfType<ChannelEndpointElement>()
					.Where(n => String.Equals(n.Contract, "ActivityRecorderServiceReference.IActiveDirectoryLoginService", StringComparison.InvariantCultureIgnoreCase))
					.Select(n => n.Name)
					.Where(n => n != null)
					.ToArray();
			}
			catch (Exception ex)
			{
				if (endpointNames == null) endpointNames = new string[0];
				log.Error("Unalbe to load endpoint names", ex);
			}
		}

		public ActiveDirectoryLoginServiceClientWrapper()
		{
			if (endpointNames.Length == 0) throw new InvalidOperationException("There is no endpoint for ActiveDirectoryLoginService.");

			Client = new ActiveDirectoryLoginServiceClient(endpointNames[0]);
		}

		public ActiveDirectoryLoginServiceClientWrapper(string endpointName)
		{
			Client = new ActiveDirectoryLoginServiceClient(endpointName);
		}

		public void Dispose()
		{
			WcfClientDisposeHelper.Dispose(Client);
		}
	}
}
