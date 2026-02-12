using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlaybackClient
{
	/// <summary>
	/// Thread-safe class for creating actualized PlaybackDataItems from PlaybackData.
	/// </summary>
	public class PlaybackDataConverter : IPlaybackDataConverter
	{
		private readonly Func<int, int> getMappedUserId;
		private readonly Func<int, int> getMappedWorkId;

		private PlaybackDataConverter(Func<int, int> threadSafeUserIdMappingFunc, Func<int, int> threadSafeWorkIdMappingFunc)
		{
			getMappedUserId = threadSafeUserIdMappingFunc;
			getMappedWorkId = threadSafeWorkIdMappingFunc;
		}

		public PlaybackDataConverter()
			: this((Func<int, int>)null, (Func<int, int>)null)
		{
		}

		public PlaybackDataConverter(IEnumerable<KeyValuePair<int, int>> userIdMapping, IEnumerable<KeyValuePair<int, int>> workIdMapping)
			: this(GetThreadSafeMappingFunc(userIdMapping), GetThreadSafeMappingFunc(workIdMapping))
		{
		}

		public PlaybackDataConverter(IEnumerable<KeyValuePair<int, int>> userIdMapping, Func<int, int> workIdMappingFunc)
			: this(GetThreadSafeMappingFunc(userIdMapping), GetThreadSafeMappingFunc(workIdMappingFunc))
		{
		}

		public PlaybackDataConverter(Func<int, int> userIdMappingFunc)
			: this(GetThreadSafeMappingFunc(userIdMappingFunc), null)
		{
		}

		public virtual List<PlaybackDataItem> GetActualizedItems(PlaybackData data, DateTime utcNewStartDate, DateTime utcSendFromDate)
		{
			var result = new List<PlaybackDataItem>();
			if (data == null) return result;

			var dbDate = data.StartDate;
			var diff = utcNewStartDate - dbDate;
			var guidMap = new Dictionary<Guid, Guid>();
			if (data.WorkItems != null)
			{
				foreach (var workItem in data.WorkItems)
				{
					workItem.StartDate += diff;
					if (workItem.StartDate < utcSendFromDate) continue;
					workItem.EndDate += diff;
					workItem.WorkId = GetMappedWorkId(workItem.WorkId);
					workItem.UserId = GetMappedUserId(workItem.UserId);
					workItem.PhaseId = GetMappedValue(workItem.PhaseId, guidMap, _ => Guid.NewGuid());
					result.Add(new PlaybackDataItem(workItem));
				}
			}
			if (data.ManualWorkItems != null)
			{
				foreach (var workItem in data.ManualWorkItems)
				{
					workItem.EndDate += diff;
					if (workItem.EndDate <= utcSendFromDate) continue;
					workItem.StartDate += diff;
					if (workItem.StartDate < utcSendFromDate) workItem.StartDate = utcSendFromDate; //workItems can be long so don't skip but rather cut them at utcSendFromDate
					if (workItem.WorkId.HasValue)
					{
						workItem.WorkId = GetMappedWorkId(workItem.WorkId.Value);
					}
					workItem.UserId = GetMappedUserId(workItem.UserId);
					result.Add(new PlaybackDataItem(workItem));
				}
			}
			result.AddRange(GetMobileDataItems(data, diff, utcSendFromDate));
			return result;
		}

		private static readonly TimeSpan mobileWorkItemGranularity = TimeSpan.FromSeconds(ConfigManager.MobileWorkItemGranularityInSec);
		private IEnumerable<PlaybackDataItem> GetMobileDataItems(PlaybackData data, TimeSpan diff, DateTime utcSendFromDate)
		{
			var locations = data.MobileLocations ?? new List<MobileClientLocation>();
			foreach (var location in locations)
			{
				location.Date += diff;
				location.UserId = GetMappedUserId(location.UserId);
				location.WorkId = GetMappedWorkId(location.WorkId);
			}
			locations.RemoveAll(n => n.Date < utcSendFromDate);

			if (data.MobileWorkItems != null)
			{
				var guidMap = new Dictionary<Guid, Guid>();
				foreach (var mobileWorkItem in data.MobileWorkItems)
				{
					mobileWorkItem.EndDate += diff;
					if (mobileWorkItem.EndDate <= utcSendFromDate) continue;
					mobileWorkItem.StartDate += diff;
					if (mobileWorkItem.StartDate < utcSendFromDate) mobileWorkItem.StartDate = utcSendFromDate; //workItems can be long so don't skip but rather cut them at utcSendFromDate
					var currEnd = mobileWorkItem.StartDate;
					var currWorkId = GetMappedWorkId(mobileWorkItem.WorkId);
					var currUserId = GetMappedUserId(mobileWorkItem.UserId);
					var currSession = GetMappedValue(mobileWorkItem.SessionId, guidMap, _ => Guid.NewGuid());
					while (currEnd < mobileWorkItem.EndDate)
					{
						currEnd += mobileWorkItemGranularity;
						if (currEnd > mobileWorkItem.EndDate) currEnd = mobileWorkItem.EndDate;
						var convertedWorkItem = new MobileServiceReference.WorkItem()
						{
							StartDateTyped = mobileWorkItem.StartDate,
							EndDateTyped = currEnd,
							SessionIdTyped = currSession,
							WorkId = currWorkId,
							Id = 0, //we don't care
							CallId = "", //we don't care ! CANNOT SEND null !
						};
						var applicableLocations = new List<MobileServiceReference.LocationInfo>();
						for (int i = 0; i < locations.Count; i++)
						{
							var location = locations[i];
							if (location.UserId == currUserId
								&& location.Imei == mobileWorkItem.Imei
								&& mobileWorkItem.StartDate <= location.Date
								&& location.Date <= currEnd)
							{
								var convertedLocation = GetConvertedLocationInfo(location);
								applicableLocations.Add(convertedLocation);
								locations.RemoveAt(i--);
							}
						}

						var request = GetRequest(currUserId, currWorkId, mobileWorkItem.Imei, MobileServiceReference.MobileClientStatus.On,
							new List<MobileServiceReference.WorkItem>(1) { convertedWorkItem }, applicableLocations);
						yield return new PlaybackDataItem(request);
					}
				}
			}
			foreach (var location in locations)
			{
				var convertedLocation = GetConvertedLocationInfo(location);
				var request = GetRequest(location.UserId, location.WorkId, location.Imei,
					MobileServiceReference.MobileClientStatus.Off, null, new List<MobileServiceReference.LocationInfo>(1) { convertedLocation });
				yield return new PlaybackDataItem(request);
			}
		}

		private static MobileServiceReference.UploadData_v4Request GetRequest(int userId, int workId, long imei,
			MobileServiceReference.MobileClientStatus status,
			List<MobileServiceReference.WorkItem> workItems,
			List<MobileServiceReference.LocationInfo> locations)
		{
			return new MobileServiceReference.UploadData_v4Request()
			{
				UserId = userId,
				WorkId = workId,
				Imei = imei,
				Status = status,
				Password = ConfigManager.ImportPassword,
				WorkItems = workItems ?? new List<MobileServiceReference.WorkItem>(0),
				LocationInfos = locations ?? new List<MobileServiceReference.LocationInfo>(0),
				Errors = new List<MobileServiceReference.ErrorData>(0),
				PhoneCalls = new List<MobileServiceReference.PhoneCall>(0),
				ReasonResponses = new List<MobileServiceReference.ReasonResponse_v4>(0),
				Rules = new List<MobileServiceReference.Rule>(0),
				TaskAssignments = new List<MobileServiceReference.TaskAssignment>(0),
				WorkPhoneNumbers = new List<MobileServiceReference.WorkPhoneNumber_v3>(0),
			};
		}

		private static MobileServiceReference.LocationInfo GetConvertedLocationInfo(MobileClientLocation location)
		{
			return new MobileServiceReference.LocationInfo()
			{
				DateTyped = location.Date,
				Latitude = location.Latitude,
				Longitude = location.Longitude,
				WorkId = location.WorkId,
				Accuracy = location.Accuracy,
				Id = 0, //we don't care
			};
		}

		private int GetMappedUserId(int fromUserId)
		{
			return getMappedUserId != null ? getMappedUserId(fromUserId) : fromUserId;
		}

		private int GetMappedWorkId(int fromWorkId)
		{
			return getMappedWorkId != null ? getMappedWorkId(fromWorkId) : fromWorkId;
		}

		private static T GetMappedValue<T>(T oldValue, Dictionary<T, T> dict, Func<T, T> mapFactory = null)
		{
			T newValue;
			if (dict == null) return oldValue;
			if (!dict.TryGetValue(oldValue, out newValue))
			{
				if (mapFactory == null) return oldValue;
				newValue = mapFactory(oldValue);
				dict.Add(oldValue, newValue);
			}
			return newValue;
		}

		//readonly dict which is not modified is thread-safe for reading //http://msdn.microsoft.com/en-us/library/xfhwa508.aspx
		//A Dictionary<TKey, TValue> can support multiple readers concurrently, as long as the collection is not modified.
		private static Func<int, int> GetThreadSafeMappingFunc(IEnumerable<KeyValuePair<int, int>> mapping)
		{
			if (mapping == null) return null;
			var dict = mapping.ToDictionary(n => n.Key, n => n.Value);
			return fromId => GetMappedValue(fromId, dict);
		}

		private static Func<int, int> GetThreadSafeMappingFunc(Func<int, int> mappingFunc)
		{
			if (mappingFunc == null) return null;
			var lockObj = new object();
			return fromId =>
					{
						lock (lockObj)
						{
							return mappingFunc(fromId);
						}
					};
		}
	}
}
