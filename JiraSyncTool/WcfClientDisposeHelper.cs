using System;
using System.ServiceModel;

namespace JiraSyncTool
{
	//http://msdn.microsoft.com/en-us/library/aa355056.aspx
	//http://social.msdn.microsoft.com/forums/en-US/wcf/thread/b95b91c7-d498-446c-b38f-ef132989c154/
	//http://bloggingabout.net/blogs/erwyn/archive/2006/12/09/WCF-Service-Proxy-Helper.aspx
	//http://nimtug.org/blogs/damien-mcgivern/archive/2009/05/26/wcf-communicationobjectfaultedexception-quot-cannot-be-used-for-communication-because-it-is-in-the-faulted-state-quot-messagesecurityexception-quot-an-error-occurred-when-verifying-security-for-the-message-quot.aspx
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