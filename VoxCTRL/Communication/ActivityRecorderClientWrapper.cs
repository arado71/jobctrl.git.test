using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Text;
using Tct.ActivityRecorderClient.Communication;

namespace VoxCTRL.Communication
{
	public class ActivityRecorderClientWrapper : IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		public readonly ActivityRecorderServiceReference.VoiceRecorderClient Client;
		internal const int DefaultTimeout = 0;

		public ActivityRecorderClientWrapper()
		{
			Client = new ActivityRecorderServiceReference.VoiceRecorderClient();
			Client.ClientCredentials.UserName.UserName = ConfigManager.UserId.ToString();
			Client.ClientCredentials.UserName.Password = ConfigManager.UserPassword;

#if DEBUG
			Client.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.None;
#endif
		}

		public static T Execute<T>(Func<ActivityRecorderServiceReference.VoiceRecorderClient, T> command)
		{
			using (var wrapper = new ActivityRecorderClientWrapper())
			{
				try
				{
					return command(wrapper.Client);
				}
				catch (CommunicationException ex)
				{
					wrapper.CloseIfUnusable(ex);
					throw;
				}
			}
		}

		public void CloseIfUnusable(Exception ex) //client might be unusable but we cannot detect it by its State
		{
			if (!(ex is CommunicationException) || ex is FaultException) return;
			log.DebugFormat("Closing client in state {0}", Client.State);
			WcfClientDisposeHelper.Dispose(Client); //now we can detect that it's not usable
		}

		public void Dispose()
		{
			WcfClientDisposeHelper.Dispose(Client);
		}
	}
}
