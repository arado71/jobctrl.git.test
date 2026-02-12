using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Configuration;
using System.Text;
using System.Threading;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Serialization;
using log4net;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Security;
using Tct.ActivityRecorderClient.Configuration;

namespace Tct.ActivityRecorderClient.Communication
{
	public class ActivityRecorderClientWrapper : IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		internal const int DefaultTimeout = 0;
		private static readonly string lastEndpointPath = "LastEndpoint";
		private static volatile EndpointConfiguration preferredEndpoint;
		private static readonly BlockingClientPool<ActivityRecorderServiceReference.ActivityRecorderClient> sharedPool;
		private static volatile ConfigManager.ProxySettings proxySettings = ConfigManager.Proxy;
		private static readonly IWebProxy configurationWebProxy;
		public readonly ActivityRecorderServiceReference.ActivityRecorderClient Client;
		private readonly bool isSharedClient;

		public static EndpointConfiguration PreferredEndpoint
		{
			get => preferredEndpoint;
			set
			{
				if (preferredEndpoint == value) return;
				SetPreferredEndpoint(value.Name, true);
			}
		}

		static ActivityRecorderClientWrapper()
		{
			ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
			// We use the fact, that the first time WebRequest.DefaultWebProxy is called, it is automatically initialized based on the configuration file.
			// Please note, that setting this value to null will disable proxy usage, and using WebRequest.GetSystemWebProxy() fails to set proper credentials 
			// even if defined in the configuration file.
			configurationWebProxy = WebRequest.DefaultWebProxy;

			try
			{
				sharedPool = new BlockingClientPool<ActivityRecorderServiceReference.ActivityRecorderClient>(() => GetClient(null), IsValid, WcfClientDisposeHelper.Dispose);
				SetProxy(proxySettings);
			}
			catch (Exception ex)
			{
				log.Error("Unable to load endpoint names", ex);
			}
			finally
			{
				string loadedEndpoint;
				if (IsolatedStorageSerializationHelper.Exists(lastEndpointPath)
					&& IsolatedStorageSerializationHelper.Load(lastEndpointPath, out loadedEndpoint))
				{
					SetPreferredEndpoint(loadedEndpoint, false);
				}
			}
		}

		public static void SetProxy(ConfigManager.ProxySettings settings)
		{
			if (!settings.IsAutomatic)
			{
				var proxy = new WebProxy(proxySettings.Address);
				proxy.Credentials = new NetworkCredential(settings.Username, settings.Password);
				WebRequest.DefaultWebProxy = proxy;
			}
			else
			{
				WebRequest.DefaultWebProxy = configurationWebProxy;
			}

			proxySettings = settings;
		}

		public ActivityRecorderClientWrapper()
			: this(null, DefaultTimeout)
		{
		}

		public ActivityRecorderClientWrapper(int timeout)
			: this(null, timeout)
		{
		}

		public ActivityRecorderClientWrapper(EndpointConfiguration endpointConfig)
			: this(endpointConfig, DefaultTimeout)
		{
		}

		public ActivityRecorderClientWrapper(EndpointConfiguration endpointConfig, int timeout)
		{
			isSharedClient = endpointConfig == null;
			Client = isSharedClient ? sharedPool.Get(timeout) : GetClient(endpointConfig);
			if (isStopped) WcfClientDisposeHelper.Dispose(Client);
		}

        public static void Execute(Action<ActivityRecorderServiceReference.ActivityRecorderClient> action, int timeout = DefaultTimeout, EndpointConfiguration endpointConfig = null)
		{
			using (var wrapper = new ActivityRecorderClientWrapper(endpointConfig, timeout))
			{
				try
				{
					action(wrapper.Client);
				}
				catch (CommunicationException ex)
				{
					wrapper.CloseIfUnusable(ex);
					throw;
				}
			}
		}

		public static T Execute<T>(Func<ActivityRecorderServiceReference.ActivityRecorderClient, T> command, int timeout = DefaultTimeout, EndpointConfiguration endpointConfig = null)
		{
			using (var wrapper = new ActivityRecorderClientWrapper(endpointConfig, timeout))
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
			Debug.Assert(isSharedClient);
			if (!(ex is CommunicationException) || ex is FaultException) return;
			log.VerboseFormat("Closing client in state {0}", Client.State);
			WcfClientDisposeHelper.Dispose(Client); //now we can detect that it's not usable
		}

		private static ActivityRecorderServiceReference.ActivityRecorderClient GetClient(EndpointConfiguration endpointConfig)
		{
			endpointConfig = endpointConfig ?? GetPreferredEndpointOrDefault();
			var result = endpointConfig.CreateClient<ActivityRecorderServiceReference.ActivityRecorderClient, IActivityRecorder>((b, e) => new ActivityRecorderServiceReference.ActivityRecorderClient(b, e));
			if (!proxySettings.IsAutomatic)
			{
				var httpBinding = result.Endpoint.Binding as BasicHttpBinding;
				if (httpBinding != null)
				{
					httpBinding.UseDefaultWebProxy = true;
				}
			}
			result.EndpointName = endpointConfig.Name;
			result.ClientCredentials.UserName.UserName = ConfigManager.UserId.ToString();
			result.ClientCredentials.UserName.Password = ConfigManager.UserPassword;
#if DEBUG
			result.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.None;
#endif
			result.ProxySetting = proxySettings;
			return result;
		}

		private static bool IsValid(ActivityRecorderServiceReference.ActivityRecorderClient client)
		{
			return client != null && client.State != CommunicationState.Faulted && client.State != CommunicationState.Closed
				&& client.EndpointName == GetPreferredEndpointOrDefault().Name
				&& proxySettings.Equals(client.ProxySetting)
				&& client.ClientCredentials.UserName.Password == ConfigManager.UserPassword;
		}

		public void SetTimeout(TimeSpan timeout)
		{
			Debug.Assert(!isSharedClient);
			Client.Endpoint.Binding.OpenTimeout = timeout;
			Client.Endpoint.Binding.SendTimeout = timeout;
			Client.Endpoint.Binding.ReceiveTimeout = timeout;
			Client.OperationTimeout = timeout;
		}

		public static EndpointConfiguration GetPreferredEndpointOrDefault()
		{
			return PreferredEndpoint ?? AppConfig.Current.ServiceEndpointConfigurations.Values.OrderBy(e => e.Order).First();
		}

		private static void SetPreferredEndpoint(string endpointName, bool save)
		{
			EndpointConfiguration endpointConfig = null;
			if (endpointName != null && !AppConfig.Current.ServiceEndpointConfigurations.TryGetValue(endpointName, out endpointConfig))
			{
				log.Error("Invalid endpoint " + endpointName);
				Debug.Fail("Invalid endpoint " + endpointName);
				return;
			}
			preferredEndpoint = endpointConfig;
			if (save)
			{
				if (endpointName == null)
				{
					if (IsolatedStorageSerializationHelper.Exists(lastEndpointPath))
					{
						IsolatedStorageSerializationHelper.Delete(lastEndpointPath);
					}
				}
				else
				{
					IsolatedStorageSerializationHelper.Save(lastEndpointPath, endpointName);
				}
			}
			log.Info("Preferred endpoint is " + (endpointName ?? "(null)"));
		}

		private static volatile bool isStopped;
		public static void Shutdown()
		{
			isStopped = true;
			log.Info("Stopping shared communication");
			WcfExceptionLogger.Shutdown();
			sharedPool.Dispose();
		}

		public void Dispose()
		{
			if (isSharedClient)
			{
				sharedPool.Release(Client);
			}
			else
			{
				WcfClientDisposeHelper.Dispose(Client);
			}
		}
	}
}
