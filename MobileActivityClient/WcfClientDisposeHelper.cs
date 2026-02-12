using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace MobileActivityClient
{
	public class WcfClientDisposeHelper<TClient> : IDisposable
		where TClient : ICommunicationObject
	{
		public TClient Client { get; private set; }

		public WcfClientDisposeHelper(TClient client)
		{
			Client = client;
		}

		public void Dispose()
		{
			WcfClientDisposeHelper.Dispose(Client);
		}
	}

	public static class WcfClientDisposeHelper
	{
		public static WcfClientDisposeHelper<TClient> Create<TClient>(TClient client)
			where TClient : class, ICommunicationObject
		{
			return new WcfClientDisposeHelper<TClient>(client);
		}

		public static void Dispose(ICommunicationObject client)
		{
			if (client == null) return;
			bool cleanedUp = false;
			if (client.State != CommunicationState.Faulted)
			{
				try
				{
					client.Close();
					cleanedUp = true;
				}
				catch
				{
				}
			}
			if (!cleanedUp)
			{
				try
				{
					client.Abort();
				}
				catch //we don't expect exceptions here... but just in case
				{
				}
			}
		}
	}
}