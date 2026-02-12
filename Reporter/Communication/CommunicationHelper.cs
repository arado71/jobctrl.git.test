using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Reporter.Interfaces;
using Reporter.Model;
using Reporter.Email;
using Reporter.Model.Email;
using Reporter.Model.WorkItems;

namespace Reporter.Communication
{
	public static class CommunicationHelper
	{
		private static readonly TimeSpan maxWorkItemLength = TimeSpan.FromMinutes(10);
		private static readonly TimeSpan maxManualWorkItemLength = TimeSpan.FromDays(2); //hax nothing enfoces this atm.

		public static List<int> GetUserIdForCompany(int companyId)
		{
			using (var conn = new SqlConnection(Properties.Settings.Default.recorderConnectionString))
			{
				var result = conn.Query<int>(
					"SELECT Id FROM [dbo].[User] WHERE CompanyId = @CompanyId",
					new { CompanyId = companyId }, 
					commandTimeout: 180)
					.EnsureList();
				return result;
			}
		}

		private static List<IWorkItem> GetComputerWorkItemsForUser(int[] userId, DateTime startDate, DateTime endDate)
		{
			using (Profiler.Measure())
			{
				using (var conn = new SqlConnection(Properties.Settings.Default.recorderConnectionString))
				{
					//not accurate at the beginning and at the end of the interval
					var result = conn.Query<ComputerWorkItem>(
						"SELECT [WorkId], [StartDate], [EndDate], [UserId], [ComputerId], [MouseActivity], [KeyboardActivity]"
						+ " FROM [dbo].[WorkItems] WHERE"
						+ " @MinStartDate <= [StartDate]" //no index on EndDate
						+ " AND [StartDate] < @EndDate"
							//+ " AND @StartDate < [EndDate]"
						+ " AND [UserId] IN @UserId",
						new
						{
							UserId = userId,
							MinStartDate = startDate - maxWorkItemLength,
							//StartDate = startDate,
							EndDate = endDate
						},
						commandTimeout: 180) //3mins
						.EnsureList();
					//var result = conn.Query<WorkItem>(
					//	"SELECT [WorkId], [StartDate], [EndDate], [UserId], [ComputerId], 0 AS [MouseActivity], 0 AS [KeyboardActivity]"
					//	+ " FROM [dbo].[AggregateWorkItemIntervals] WHERE"
					//	+ " @MinStartDate <= [StartDate]" //no index on EndDate
					//	+ " AND [StartDate] < @EndDate"
					//	//+ " AND @StartDate < [EndDate]"
					//	+ " AND [UserId] IN @UserId",
					//	new
					//	{
					//		UserId = userId,
					//		MinStartDate = startDate - maxWorkItemLength,
					//		//StartDate = startDate,
					//		EndDate = endDate
					//	},
					//	commandTimeout: 180) //3mins
					//	.EnsureList();
					using(Profiler.Measure())
					result.RemoveAll(n => !(startDate < n.EndDate && n.StartDate < endDate));
					//Debug.WriteLine("Total workitems: {0}", result.Sum(x => (x.EndDate - x.StartDate).TotalMinutes));
					return result.Cast<IWorkItem>().ToList();
				}
			}
		}

		private static List<IWorkItem> GetManualWorkItemsForUser(int[] userId, DateTime startDate, DateTime endDate)
		{
			var sw = Stopwatch.StartNew();
			using (var conn = new SqlConnection(Properties.Settings.Default.recorderConnectionString))
			{
				var manualOrMeetingWorkItems = conn.Query<ManualOrMeetingWorkItem>(
					"SELECT mwi.[UserId], mwi.[StartDate], mwi.[EndDate], mwi.[WorkId], mwi.[Comment], m.[MeetingId], m.[Title], m.[Description], m.[ParticipantEmails]"
					+ " FROM [dbo].[ManualWorkItems] mwi"
					+ " LEFT JOIN [dbo].[UsersToMeetings] utm ON mwi.[Id] = utm.[ManualWorkItemId]"
					+ " LEFT JOIN [dbo].[Meetings] m ON utm.[MeetingId] = m.[MeetingId]"
					+ " WHERE"
					+ " @MinStartDate <= mwi.[StartDate]" //no usable index on EndDate
					+ " AND mwi.[StartDate] < @EndDate"
					+ " AND @StartDate < DATEADD(ms, 0, mwi.[EndDate])" //don't use index on EndDate
					+ " AND mwi.[UserId] IN @UserId"
					+ " AND (mwi.[ManualWorkItemTypeId] = 0)",
					new
					{
						UserId = userId,
						MinStartDate = startDate - maxManualWorkItemLength,
						StartDate = startDate,
						EndDate = endDate
					})
					.EnsureList();

				Debug.WriteLine("Manual workItems queried at {0}ms", sw.Elapsed.TotalMilliseconds);
				manualOrMeetingWorkItems.RemoveAll(n => !(startDate < n.EndDate && n.StartDate < endDate));
				Debug.WriteLine("Manual workItems calculated at {0}ms", sw.Elapsed.TotalMilliseconds);
				return manualOrMeetingWorkItems.Select(x => x.GetWorkItem()).EnsureList();
			}
		}

		private static List<IWorkItem> GetMobileWorkItemsForUser(int[] userId, DateTime startDate, DateTime endDate)
		{
			var sw = Stopwatch.StartNew();
			using (var conn = new SqlConnection(Properties.Settings.Default.recorderConnectionString))
			{
				var result = conn.Query<MobileWorkItem>(
					"SELECT [UserId], [StartDate], [EndDate], [WorkId], [Imei] FROM [dbo].[MobileWorkItems] WHERE"
					+ " @MinStartDate <= [StartDate]" //no usable index on EndDate
					+ " AND [StartDate] < @EndDate"
					+ " AND @StartDate < DATEADD(ms, 0, [EndDate])" //don't use index on EndDate
					+ " AND [UserId] IN @UserId",
					new
					{
						UserId = userId,
						MinStartDate = startDate - maxManualWorkItemLength,
						StartDate = startDate,
						EndDate = endDate
					})
					.Cast<IWorkItem>()
					.EnsureList();

				Debug.WriteLine("Mobile workItems queried at {0}ms", sw.Elapsed.TotalMilliseconds);
				result.RemoveAll(n => !(startDate < n.EndDate && n.StartDate < endDate));
				Debug.WriteLine("Mobile workItems calculated at {0}ms", sw.Elapsed.TotalMilliseconds);
				return result.EnsureList();
			}
		}

		public static List<EmailUser> GetUsersEmails(int[] userIds)
		{
			var sw = Stopwatch.StartNew();
			using (var conn = new SqlConnection(Properties.Settings.Default.recorderConnectionString))
			{
				var result = conn.Query<EmailUser>(@"SELECT Id, Email FROM [dbo].[User] WHERE Id IN @UserId",
					new { UserId = userIds})
					.EnsureList();
				Debug.WriteLine("GetUsersFromPhoneBooks finished in {0}ms", sw.Elapsed.TotalMilliseconds);
				return result;
			}
		}

		private static List<IWorkItemDeletion> GetManualWorkItemsForUserCovered(int[] userId, DateTime startDate, DateTime endDate)
		{
			var sw = Stopwatch.StartNew();
			using (var conn = new SqlConnection(Properties.Settings.Default.recorderConnectionString))
			{
				var result = conn.Query<WorkItemDeletion>(
					"SELECT [UserId], [StartDate], [EndDate] FROM [dbo].[ManualWorkItems] WHERE"
					+ " @MinStartDate <= [StartDate]" //no usable index on EndDate
					+ " AND [StartDate] < @EndDate"
					+ " AND @StartDate < DATEADD(ms, 0, [EndDate])" //don't use index on EndDate
					+ " AND [UserId] IN @UserId"
					+ " AND ([ManualWorkItemTypeId] IN (1,2,3,6))", // is removal
					new
					{
						UserId = userId,
						MinStartDate = startDate - maxManualWorkItemLength,
						StartDate = startDate,
						EndDate = endDate
					})
					.Cast<IWorkItemDeletion>()
					.EnsureList();

				//Debug.WriteLine("Total deletion: {0}", result.Sum(x => (x.EndDate - x.StartDate).TotalMinutes));
				Debug.WriteLine("Deletions queried in {0}ms", sw.Elapsed.TotalMilliseconds);
				return result;
			}
		}

		public static Dictionary<int, string> GetUserNames(int[] userId)
		{
			var sw = Stopwatch.StartNew();
			using (var conn = new SqlConnection(Properties.Settings.Default.recorderConnectionString))
			{
				var result = conn.Query<User>(
					"SELECT [Id], [FirstName], [LastName] FROM [dbo].[User] WHERE"
					+ "[Id] IN @UserId",
					new
					{
						UserId = userId
					}).EnsureList();

				Debug.WriteLine("Users queried in {0}ms", sw.Elapsed.TotalMilliseconds);
				return result.ToDictionary(x => x.Id, y => y.LastName + " " + y.FirstName);
			}
		}

		private static List<ICollectedItem> GetCollectedItemsForUser(int[] userId, DateTime startDate, DateTime endDate)
		{
			var sw = Stopwatch.StartNew();
			using (var conn = new SqlConnection(Properties.Settings.Default.recorderConnectionString))
			{
				var result = conn.Query<ComputerCollectedItem>(
					"SELECT c.UserId, c.ComputerId, c.CreateDate, [Key], Value FROM [dbo].[CollectedItems] AS c"
					+ " JOIN [dbo].[CollectedKeyLookup] AS k ON c.KeyId = k.Id"
					+ " LEFT JOIN [dbo].[CollectedValueLookup] AS v ON c.ValueId = v.Id"
					+ " WHERE c.UserId IN @UserId AND c.CreateDate >= @StartDate AND c.CreateDate <= @endDate",
					new
					{
						UserId = userId,
						StartDate = startDate.AddDays(-1),
						EndDate = endDate
					})
					.EnsureList();
				Debug.WriteLine("CollectedItems queried in {0}ms", sw.Elapsed.TotalMilliseconds);

				result.Sort(CollectedItem.DefaultCreateDateComparer);
				int? idx = null;
				for (int i = 0; i < result.Count; i++)
				{
					if (result[i].CreateDate >= startDate)
					{
						idx = i;
						break;
					}
				}

				if (idx != null && idx.Value > 1)
				{
					result.RemoveRange(0, idx.Value - 2);
				}

				Debug.WriteLine("CollectedItems calculated at {0}ms", sw.Elapsed.TotalMilliseconds);
				return result.Cast<ICollectedItem>().ToList();
			}
		}

		public static QueryResult Query(int[] userId, DateTime startDate, DateTime endDate)
		{
			var sw = Stopwatch.StartNew();
			try
			{
				var items = GetComputerWorkItemsForUser(userId, startDate, endDate);
				items.AddRange(GetManualWorkItemsForUser(userId, startDate,endDate));
				items.AddRange(GetMobileWorkItemsForUser(userId, startDate, endDate));
				return new QueryResult
				{
					CollectedItems = GetCollectedItemsForUser(userId, startDate, endDate),
					ManualWorkItems = GetManualWorkItemsForUserCovered(userId, startDate, endDate),
					WorkItems = items,
				};
			}
			finally
			{
				Debug.WriteLine("Queries done in {0}ms", sw.Elapsed.TotalMilliseconds);
			}
		}
	}
}
