using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Util.Store;
using Newtonsoft.Json;

namespace Tct.ActivityRecorderService.MeetingSync
{
	class GoogleDatabaseDataStore : IDataStore
	{
		public Task StoreAsync<T>(string key, T value)
		{
			var userId = int.Parse(key);
			var authToken = JsonConvert.SerializeObject(value);
			return Task.Factory.StartNew(() =>
			{
				using (var context = new ActivityRecorderDataClassesDataContext())
				{
					context.Client_SetCloudToken(userId, authToken);
				}
			});
		}

		public Task DeleteAsync<T>(string key)
		{
			var userId = int.Parse(key);
			return Task.Factory.StartNew(() =>
			{
				using (var context = new ActivityRecorderDataClassesDataContext())
				{
					context.Client_DeleteCloudToken(userId);
				}
			});
		}

		public Task<T> GetAsync<T>(string key)
		{
			var userId = int.Parse(key);
			return Task<T>.Factory.StartNew(() =>
			{
				using (var context = new ActivityRecorderDataClassesDataContext())
				{
					var res = context.Client_GetCloudTokenByUserId(userId).FirstOrDefault();
					return res?.AuthToken != null ? JsonConvert.DeserializeObject<T>(res?.AuthToken) : default(T);
				}
			});
		}

		public Task ClearAsync()
		{
			// not needed
			return new Task(() => { });
		}
	}
}
