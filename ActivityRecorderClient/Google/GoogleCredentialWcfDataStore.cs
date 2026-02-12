using Google.Apis.Util.Store;
using log4net;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Tct.ActivityRecorderClient.Communication;

namespace Tct.ActivityRecorderClient.Google
{
	class GoogleCredentialWcfDataStore : IDataStore
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private object cachedValue;

		public Task StoreAsync<T>(string key, T value)
		{
			cachedValue = value;
			return Task.Delay(0);
		}

		public Task DeleteAsync<T>(string key)
		{
			return Task.Delay(0);
		}

		public Task<T> GetAsync<T>(string key)
		{
			return Task<T>.Factory.StartNew(() =>
				{
					if (cachedValue != null) return (T)cachedValue;
					var token = ActivityRecorderClientWrapper.Execute(client => client.ManageCloudTokens(ConfigManager.UserId, null)).GoogleCalendarToken;
					if (token == null) return default(T);
					try
					{
						return JsonConvert.DeserializeObject<T>(token);
					}
					catch (Exception e)
					{
						log.Debug("Serialization error in getting Google auth token.", e);
						return default(T);
					}
				});
		}

		public Task ClearAsync()
		{
			return Task.Delay(0);
		}

		public void StoreOnServer()
		{
			if (cachedValue == null) return; // nothing to store
			var googleCalendarToken = JsonConvert.SerializeObject(cachedValue);
			ActivityRecorderClientWrapper.Execute(client =>
				client.ManageCloudTokens(ConfigManager.UserId, googleCalendarToken));
			cachedValue = null;
		}
	}
}
