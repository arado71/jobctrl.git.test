using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Reporter.Model.Mobile;

namespace Reporter.Mobile
{
	public class MobileDbContext : IMobileDbContext
	{
		public List<MobileUser> GetUsersFromPhoneBooks()
		{
			var sw = Stopwatch.StartNew();
			using (var conn = new SqlConnection(Properties.Settings.Default.recorderConnectionString))
			{
				var result = conn.Query<MobileUser>(
@"SELECT PhoneNumber, NULL AS UserId, FirstName, LastName
FROM (
	SELECT PhoneNumber, FirstName, LastName, [Count], ROW_NUMBER() OVER (PARTITION by PhoneNumber ORDER BY [Count] DESC) AS [Rank]
	FROM (
		SELECT PhoneNumber, FirstName, LastName, COUNT(*) AS [Count] FROM MobileWorkPhoneBook b
		JOIN MobileAssignedWorkPhoneNumber a ON b.WorkPhoneNumberAssignmentId = a.Id
		JOIN MobileWorkPhoneNumbers p ON a.WorkPhoneNumberId = p.Id
		GROUP by PhoneNumber, FirstName, LastName
	) AS d1
) AS d2
WHERE [Rank] =1")
					.EnsureList();
				Debug.WriteLine("GetUsersFromPhoneBooks finished in {0}ms", sw.Elapsed.TotalMilliseconds);
				return result;
			}
		}

		public List<MobileUser> GetUsersFromIvr()
		{
			var sw = Stopwatch.StartNew();
			using (var conn = new SqlConnection(Properties.Settings.Default.recorderConnectionString))
			{
				var result = conn.Query<MobileUser>(
@"SELECT PhoneNumber, i.UserId, u.FirstName, u.LastName 
FROM IvrUserWorks i
JOIN [user] u ON u.Id = i.UserId")
					.EnsureList();
				Debug.WriteLine("GetUsersFromIvr finished in {0}ms", sw.Elapsed.TotalMilliseconds);
				return result;
			}
		}

		public List<MobileUser> GetUsers()
		{
			var sw = Stopwatch.StartNew();
			using (var conn = new SqlConnection(Properties.Settings.Default.recorderConnectionString))
			{
				var result = conn.Query<MobileUser>(
@"SELECT NULL AS PhoneNumber, u.Id AS UserId, u.FirstName, u.LastName 
FROM [user] u")
					.EnsureList();
				Debug.WriteLine("GetUsers finished in {0}ms", sw.Elapsed.TotalMilliseconds);
				return result;
			}
		}

		public List<MobilePhoneCall> GetMobilePhoneCalls(int[] userIds, DateTime startDate, DateTime endDate)
		{
			var sw = Stopwatch.StartNew();
			using (var conn = new SqlConnection(Properties.Settings.Default.recorderConnectionString))
			{
				var result = conn.Query<MobilePhoneCall>(
					"SELECT UserId, u.FirstName, u.LastName, PhoneNumber, StartDate, EndDate, IsInbound FROM MobileClientPhoneCalls"
					+ " JOIN [user] u ON u.Id = UserId"
					+ " WHERE"
					+ " [StartDate] < @EndDate"
					+ " AND @StartDate < [EndDate]"
					+ " AND [UserId] IN @UserIds",
					new
					{
						UserIds = userIds,
						StartDate = startDate,
						EndDate = endDate
					})
					.EnsureList();
				Debug.WriteLine("GetMobilePhoneCalls " + string.Join(",", userIds) + " s:" + startDate + " e:" + endDate + " finished in {0}ms", sw.Elapsed.TotalMilliseconds);
				return result;
			}
		}

		public List<int> GetUserIdsForCompany(int companyId)
		{
			var sw = Stopwatch.StartNew();
			using (var conn = new SqlConnection(Properties.Settings.Default.recorderConnectionString))
			{
				var result = conn.Query<int>(
					"SELECT Id FROM [user] u WHERE CompanyId = @companyId"
					, new { CompanyId = companyId })
					.EnsureList();
				Debug.WriteLine("GetUserIdsForCompany finished in {0}ms", sw.Elapsed.TotalMilliseconds);
				return result;
			}
		}

	}
}
