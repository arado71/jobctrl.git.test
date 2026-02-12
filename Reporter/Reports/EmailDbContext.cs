using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using Dapper;
using Reporter.Interfaces;
using Reporter.Model;
using Reporter.Model.Email;
using Reporter.Model.WorkItems;

namespace Reporter.Email
{
	public class EmailDbContext : IEmailDbContext
	{
		private static readonly TimeSpan maxWorkItemLength = TimeSpan.FromMinutes(10);
		private static readonly TimeSpan maxManualWorkItemLength = TimeSpan.FromDays(2); //hax nothing enfoces this atm.

		public List<IWorkItem> GetWorkItems(int[] userIds, DateTime startDate, DateTime endDate)
		{
			var sw = Stopwatch.StartNew();
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
						UserId = userIds,
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
				Debug.WriteLine("WorkItems queried at {0}ms", sw.Elapsed.TotalMilliseconds);
				result.RemoveAll(n => !(startDate < n.EndDate && n.StartDate < endDate));
				Debug.WriteLine("WorkItems calculated at {0}ms", sw.Elapsed.TotalMilliseconds);
				//Debug.WriteLine("Total workitems: {0}", result.Sum(x => (x.EndDate - x.StartDate).TotalMinutes));
				return result.Cast<IWorkItem>().ToList();
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

		public List<IWorkItemDeletion> GetDeletions(int[] userIds, DateTime startDate, DateTime endDate)
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
					+ " AND ([ManualWorkItemTypeId] = 1 OR [ManualWorkItemTypeId] = 3)", // is computer or all delete
					new
					{
						UserId = userIds,
						MinStartDate = startDate - maxManualWorkItemLength,
						StartDate = startDate,
						EndDate = endDate
					})
					.Cast<IWorkItemDeletion>()
					.EnsureList();

				//Debug.WriteLine("Total deletion: {0}", result.Sum(x => (x.EndDate - x.StartDate).TotalMinutes));
				Debug.WriteLine("ManualWorkItems queried in {0}ms", sw.Elapsed.TotalMilliseconds);
				return result;
			}
		}

		public List<IEmailUser> GetUsers(int[] userIds)
		{
			var sw = Stopwatch.StartNew();
			using (var conn = new SqlConnection(Properties.Settings.Default.recorderConnectionString))
			{
				var result = conn.Query<EmailUser>(
					"SELECT [Id], [FirstName], [LastName], [Email] FROM [dbo].[User] WHERE"
					+ "[Id] IN @UserId",
					new
					{
						UserId = userIds
					})
					.Cast<IEmailUser>().EnsureList();

				Debug.WriteLine("Users queried in {0}ms", sw.Elapsed.TotalMilliseconds);
				return result;
			}
		}

		public List<ICollectedItem> GetCollectedItems(int[] userIds, DateTime startDate, DateTime endDate)
		{
			var sw = Stopwatch.StartNew();
			using (var conn = new SqlConnection(Properties.Settings.Default.recorderConnectionString))
			{
				var result = conn.Query<CollectedItem>(
					"SELECT c.UserId, c.ComputerId, c.CreateDate, [Key], Value FROM [dbo].[CollectedItems] AS c"
					+ " JOIN [dbo].[CollectedKeyLookup] AS k ON c.KeyId = k.Id"
					+ " LEFT JOIN [dbo].[CollectedValueLookup] AS v ON c.ValueId = v.Id"
					+ " WHERE c.UserId IN @UserId AND c.CreateDate >= @StartDate AND c.CreateDate <= @endDate",
					new
					{
						UserId = userIds,
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

		private readonly static EmailDbContext instance = new EmailDbContext();

		public static QueryResult Query(int[] userId, DateTime startDate, DateTime endDate)
		{
			var sw = Stopwatch.StartNew();
			try
			{
				return new QueryResult
				{
					CollectedItems = instance.GetCollectedItems(userId, startDate, endDate),
					ManualWorkItems = instance.GetDeletions(userId, startDate, endDate),
					WorkItems = instance.GetWorkItems(userId, startDate, endDate)
				};
			}
			finally
			{
				Debug.WriteLine("Queries done in {0}ms", sw.Elapsed.TotalMilliseconds);
			}
		}
	}
}
