using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Tct.ActivityRecorderClient.ChromeCaptureServiceReference;

namespace Tct.ActivityRecorderClient.Communication
{
	public abstract class ChromiumCaptureClientWrapperBase : IDisposable
	{
		private ChromeCaptureServiceClient sharedClient;
		private readonly object lockObj = new object();

		private readonly ILog log;
		private ChromeCaptureServiceClient client;

		public ChromeCaptureServiceClient Client
		{
			get
			{
				if (client != null) return client;
				lock (lockObj)
				{
					client = CreateClient();
				}
				return client;
			}
		}

		private ChromeCaptureServiceClient CreateClient()
		{
			var client = new ChromeCaptureServiceClient("NetNamedPipeBinding_IChromeCaptureService", ServiceEndpointUrl);
			SetTimeout(client, TimeSpan.FromSeconds(3));
			return client;
		}

		protected ChromiumCaptureClientWrapperBase(ILog log)
		{
			this.log = log;
		}

		protected abstract string ServiceEndpointUrl { get; }

		private static void SetTimeout(ChromeCaptureServiceClient client, TimeSpan timeout)
		{
			client.Endpoint.Binding.OpenTimeout = timeout;
			client.Endpoint.Binding.SendTimeout = timeout;
			client.Endpoint.Binding.ReceiveTimeout = timeout;
		}

		protected T ExecuteShared<T>(Func<ChromeCaptureServiceClient, T> command)
		{
			lock (lockObj)
			{
				try
				{
					if (sharedClient == null)
					{
						sharedClient = CreateClient();
					}

					return command(sharedClient);
				}
				catch (ObjectDisposedException objectDisposedException)
				{
					sharedClient = CreateClient();
					return command(sharedClient);

				}
				catch (CommunicationException ex)
				{
					CloseIfUnusable(ex);
					throw;
				}
			}
		}

		private void CloseIfUnusable(Exception ex)
		{
			if (!(ex is CommunicationException) || ex is FaultException) return;
			if (sharedClient != null)
			{
				LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType).VerboseFormat("Closing shared client in state {0}", sharedClient.State);
				WcfClientDisposeHelper.Dispose(sharedClient);
				sharedClient = null;
			}
		}

		public void Dispose()
		{
			WcfClientDisposeHelper.Dispose(client);
		}
	}
}

