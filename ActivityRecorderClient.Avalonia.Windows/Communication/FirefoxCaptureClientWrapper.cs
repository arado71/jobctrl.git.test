using System;
using System.ServiceModel;
using log4net;
using Tct.ActivityRecorderClient.ChromeCaptureServiceReference;

namespace Tct.ActivityRecorderClient.Communication
{
	public class FirefoxCaptureClientWrapper : IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static ChromeCaptureServiceClient sharedClient;
		private static object lockObj = new object();
		public readonly ChromeCaptureServiceClient Client;

		public FirefoxCaptureClientWrapper()
		{
			// TODO: mac
			Client = new ChromeCaptureServiceClient();
			SetTimeout(Client, TimeSpan.FromSeconds(3));
		}

		private static void SetTimeout(ChromeCaptureServiceClient client, TimeSpan timeout)
		{
			client.Endpoint.Binding.OpenTimeout = timeout;
			client.Endpoint.Binding.SendTimeout = timeout;
			client.Endpoint.Binding.ReceiveTimeout = timeout;
		}

		public static T Execute<T>(Func<ChromeCaptureServiceClient, T> command)
		{
			lock (lockObj)
			{
				try
				{
					if (sharedClient == null)
					{
                        // TODO: mac
                        sharedClient = new ChromeCaptureServiceClient();
						SetTimeout(sharedClient, TimeSpan.FromSeconds(3));
					}

					return command(sharedClient);
				}
				catch (ObjectDisposedException objectDisposedException)
				{
                    // TODO: mac
                    sharedClient = new ChromeCaptureServiceClient();
					SetTimeout(sharedClient, TimeSpan.FromSeconds(3));
					return command(sharedClient);

				}
				catch (CommunicationException ex)
				{
					CloseIfUnusable(ex);
					throw;
				}
			}
		}

		private static void CloseIfUnusable(Exception ex)
		{
			if (!(ex is CommunicationException) || ex is FaultException) return;
			if (sharedClient != null)
			{
				log.VerboseFormat("Closing shared client in state {0}", sharedClient.State);
				WcfClientDisposeHelper.Dispose(sharedClient);
				sharedClient = null;
			}
		}

		public void Dispose()
		{
			WcfClientDisposeHelper.Dispose(Client);
		}
	}
}
