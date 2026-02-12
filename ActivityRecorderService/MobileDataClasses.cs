namespace Tct.ActivityRecorderService
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Data.Linq.Mapping;
	using System.Data.Linq;
	using System.Reflection;
	using Tct.ActivityRecorderService.EmailStats;
	using Tct.ActivityRecorderService.OnlineStats;
	using Dapper;
	using log4net;

	partial class MobileDataClassesDataContext
	{
		public static IEnumerable<MobileWorkItem> DebugMobileWorkItems = new MobileWorkItem[]{ //test data
					//new MobileWorkItem() { Id = 1, UserId=13, WorkId = 213, Imei=2, StartDate = DateTime.Today.AddHours(10), EndDate = DateTime.Today.AddHours(11), },
					//new MobileWorkItem() { Id = 2, UserId=13, WorkId = 212, Imei=2, StartDate = DateTime.Today.AddHours(9), EndDate = DateTime.Today.AddHours(10), },
					//new MobileWorkItem() { Id = 3, UserId=13, WorkId = 212, Imei=1, StartDate = DateTime.Today.AddHours(8), EndDate = DateTime.Today.AddHours(9), },
				};

		public static IEnumerable<MobileLocationInfo> DebugMobileLocations = new MobileLocationInfo[]{ //test data
					//new MobileLocationInfo() { UserId = 13, CreateDate =  DateTime.Today.AddHours(-9), Imei = 2, Latitude = 23, Longitude = 45, Accuracy = 20 },
					//new MobileLocationInfo() { UserId = 13, CreateDate =  DateTime.Today.AddHours(9), Imei = 2, Latitude = 23, Longitude = 45, Accuracy = 20 },
					//new MobileLocationInfo() { UserId = 13, CreateDate =  DateTime.Today.AddHours(10), Imei = 2, Latitude = 23, Longitude = 45, Accuracy = 20 },
					//new MobileLocationInfo() { UserId = 13, CreateDate =  DateTime.Today.AddHours(29), Imei = 2, Latitude = 23, Longitude = 45, Accuracy = 20 },
				};

		public ILookup<int, MobileLocationInfo> GetMobileLocationInfoByUser(DateTime startDate, DateTime endDate)
		{
#if DEBUG
			var dbData = DebugMobileLocations;
#else
			var dbData = Connection.Query<MobileLocationInfoInt>("exec [dbo].[Client_GetMobileLocationInfo] @startDate=@StartDate, @endDate=@EndDate", new { StartDate = startDate, EndDate = endDate })
				.Select(n => n.ToMobileLocationInfo());
#endif
			return dbData.ToLookup(n => n.UserId);
		}

		public ILookup<int, MobileActivityInfo> GetMobileActivityInfoByUser(DateTime startDate, DateTime endDate)
		{
#if DEBUG
			var dbData = Enumerable.Empty<MobileActivityInfo>();
#else
			var dbData = Connection.Query<MobileActivityInfo>("exec [dbo].[Client_GetMobileActivityInfo] @startDate=@StartDate, @endDate=@EndDate", new { StartDate = startDate, EndDate = endDate });
#endif
			return dbData.ToLookup(n => n.UserId);
		}

		public ILookup<int, MobileWorkItem> GetMobileWorkItemsByUser(DateTime startDate, DateTime endDate)
		{
#if DEBUG
			var dbData = GetMobileWorkItems()
				.Where(n => startDate < n.EndDate)
				.Where(n => n.StartDate < endDate);
#else
			var dbData = ExecuteQuery<MobileWorkItemInt>("exec [dbo].[Client_GetMobileWorkItemIntervals] @startDate={0}, @endDate={1}", startDate, endDate)
				.Select(n => n.ToMobileWorkItem())
				.Where(n => n.StartDate < n.EndDate); //we only care about valid intervals
#endif
			return dbData.ToLookup(n => n.UserId);
		}

		public List<MobileWorkItem> GetMobileWorkItemsForUser(int userId, DateTime startDate, DateTime endDate)
		{
#if DEBUG
			var dbData = GetMobileWorkItems()
				.Where(n => startDate < n.EndDate)
				.Where(n => n.StartDate < endDate)
				.Where(n => n.UserId == userId);
#else
			var dbData = ExecuteQuery<MobileWorkItemInt>("exec [dbo].[Client_GetMobileWorkItemIntervalsForUser] @userId={0}, @startDate={1}, @endDate={2}", userId, startDate, endDate)
				.Select(n => n.ToMobileWorkItem())
				.Where(n => n.StartDate < n.EndDate); //we only care about valid intervals
#endif
			return dbData.ToList();
		}

		internal int DeleteAllMobileWorkItems() //only for testing
		{
			return ExecuteCommand("DELETE FROM [dbo].[MobileWorkItems]");
		}

		internal int UpdateMobileWorkItem(MobileWorkItem item) //only for testing
		{
			return ExecuteCommand("UPDATE [dbo].[MobileWorkItems] SET [UserId]={1},[WorkId]={2},[Imei]={3},[StartDate]={4},[EndDate]={5} WHERE [Id] = {0}",
				item.Id, item.UserId, item.WorkId, item.Imei.ToString(), item.StartDate, item.EndDate);
		}

		internal List<MobileWorkItem> GetMobileWorkItems() //only for testing
		{
			return ExecuteQuery<MobileWorkItemInt>("SELECT [Id],[UserId],[WorkId],[Imei],[StartDate],[EndDate] FROM [dbo].[MobileWorkItems]")
				.Select(n => n.ToMobileWorkItem())
				.ToList();
		}

		internal int InsertMobileWorkItem(MobileWorkItem item) //only for testing
		{
#if DEBUG
			return ExecuteCommand("INSERT INTO [dbo].[MobileWorkItems] ([UserId],[WorkId],[SessionId],[Imei],[FirstReceiveDate],[LastReceiveDate],[StartDate],[EndDate])"
				+ "VALUES ({0},{1},{2},{3},GETDATE(),GETDATE(),{4},{5})",
				item.UserId, item.WorkId, Guid.NewGuid(), item.Imei.ToString(), item.StartDate, item.EndDate);
#endif
			return 0;
		}
	}

	internal class MobileWorkItemInt
	{
		public long Id { get; set; }
		public int UserId { get; set; }
		public string Imei { get; set; }
		public int WorkId { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }

		internal MobileWorkItem ToMobileWorkItem()
		{
			return new MobileWorkItem()
			{
				Id = Id,
				UserId = UserId,
				Imei = MobileHelper.GetMobileId(Imei),
				WorkId = WorkId,
				StartDate = StartDate,
				EndDate = EndDate,
			};
		}
	}

	public class MobileWorkItem : IMobileWorkItem
	{
		public long Id { get; set; }
		public int UserId { get; set; }
		public long Imei { get; set; }
		public int WorkId { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public bool IsBeacon { get; set; }
	}

	internal class MobileLocationInfoInt
	{
		public int UserId { get; set; }
		public string Imei { get; set; }
		public double? Latitude { get; set; }
		public double? Longitude { get; set; }
		public float Accuracy { get; set; }
		public DateTime Date { get; set; }
		public string LongitudeEncrypted { get; set; }
		public string LatitudeEncrypted { get; set; }

		internal MobileLocationInfo ToMobileLocationInfo()
		{
			return new MobileLocationInfo()
			{
				UserId = UserId,
				Imei = MobileHelper.GetMobileId(Imei),
				Latitude = Latitude,
				Longitude = Longitude,
				Accuracy = Accuracy,
				CreateDate = Date,
				LongitudeEncrypted = LongitudeEncrypted,
				LatitudeEncrypted = LatitudeEncrypted,
			};
		}
	}

	public class MobileLocationInfo
	{
		public int UserId { get; set; }
		public long Imei { get; set; }
		public double? Latitude { get; set; }
		public double? Longitude { get; set; }
		public double Accuracy { get; set; }
		public DateTime CreateDate { get; set; }
		public string LongitudeEncrypted { get; set; }
		public string LatitudeEncrypted { get; set; }
	}

	public class MobileActivityInfo
	{
		public int UserId { get; set; }
		public long Imei { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public int Activity { get; set; }
	}
}
