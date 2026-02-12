namespace Tct.ActivityRecorderService
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Data.Linq;
	using System.Data.Linq.Mapping;
	using System.Data.SqlClient;
	using System.Diagnostics;
	using System.Linq;
	using System.Reflection;
	using DailyAggregation;

	partial class AggregateDataClassesDataContext
	{
		private string originalConnectionString;
		partial void OnCreated()
		{
			originalConnectionString = Connection.ConnectionString; //Persist Security Info=True; is needed for Connection.ConnectionString to work after connection is opened, so capture it before it's opened
		}

		public void UpdateDailyAggregateWorkTimeTables(int userId, DateTime day, int netWorkTime, int computerWorkTime,
			int mobileWorkTime, int manualWorkTime, int holidayTime, int sickLeaveTime, Binary oldVersion,
			IEnumerable<KeyValuePair<int, int>> workTimesById)
		{
			var workTimeTable = new DataTable();
			workTimeTable.Columns.Add("WorkId", typeof(int));
			workTimeTable.Columns.Add("WorkTime", typeof(int));

			foreach (var kvp in workTimesById)
			{
				workTimeTable.Rows.Add(kvp.Key, kvp.Value);
			}

			using (var conn = new SqlConnection(originalConnectionString))
			using (var cmd = new SqlCommand("UpdateDailyAggregateWorkTimeTables", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;

				cmd.Parameters.AddWithValue("@userId", userId);
				cmd.Parameters.AddWithValue("@day", day);
				cmd.Parameters.AddWithValue("@netWorkTime", netWorkTime);
				cmd.Parameters.AddWithValue("@computerWorkTime", computerWorkTime);
				cmd.Parameters.AddWithValue("@ivrWorkTime", 0); //TODO need to remove after parameter eliminated
				cmd.Parameters.AddWithValue("@mobileWorkTime", mobileWorkTime);
				cmd.Parameters.AddWithValue("@manualWorkTime", manualWorkTime);
				cmd.Parameters.AddWithValue("@holidayTime", holidayTime);
				cmd.Parameters.AddWithValue("@sickLeaveTime", sickLeaveTime);
				cmd.Parameters.AddWithValue("@oldVersion", oldVersion.ToArray());

				var p = cmd.Parameters.AddWithValue("@workTimesById", workTimeTable);
				p.SqlDbType = SqlDbType.Structured;
				p.TypeName = "WorkTimesById";

				conn.Open();
				cmd.ExecuteNonQuery();
			}
		}

		[Function(Name = "dbo.GetLatestDailyAggregateWorkTimeTablesForUser")]
		[ResultType(typeof(AggregateDailyWorkTime))]
		[ResultType(typeof(AggregateDailyWorkTimesByWorkId))]
		private IMultipleResults GetLatestDailyAggregateWorkTimeTablesForUserImpl(
			[Parameter(DbType = "Int")] int? userId,
			[Parameter(DbType = "Binary(8)")] Binary oldVersion)
		{
			var result = ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), userId, oldVersion);
			return (IMultipleResults)result.ReturnValue;
		}

		public IEnumerable<DailyWorkTimeStats> GetLatestDailyAggregateWorkTimeTablesForUser(int userId, long oldVersion)
		{
			var multiResult = GetLatestDailyAggregateWorkTimeTablesForUserImpl(userId, oldVersion.ToBinary());
			//the order should be the same as in the sproc
			var times = multiResult.GetResult<AggregateDailyWorkTime>().ToDictionary(
				n => n.Day,
				n => new DailyWorkTimeStats()
					{
						UserId = n.UserId,
						Day = n.Day,
						Version = n.Version.ToLong(),
						ComputerWorkTime = TimeSpan.FromMilliseconds(n.ComputerWorkTime),
						HolidayTime = TimeSpan.FromMilliseconds(n.HolidayTime),
						ManuallyAddedWorkTime = TimeSpan.FromMilliseconds(n.ManualWorkTime),
						MobileWorkTime = TimeSpan.FromMilliseconds(n.MobileWorkTime),
						NetWorkTime = TimeSpan.FromMilliseconds(n.NetWorkTime),
						SickLeaveTime = TimeSpan.FromMilliseconds(n.SickLeaveTime),
						TotalWorkTimeByWorkId = new Dictionary<int, TimeSpan>(),
					});
			Debug.Assert(times.All(n => n.Value.UserId == userId));
			foreach (var workTimeById in multiResult.GetResult<AggregateDailyWorkTimesByWorkId>())
			{
				Debug.Assert(workTimeById.UserId == userId);
				times[workTimeById.Day].TotalWorkTimeByWorkId.Add(workTimeById.WorkId, TimeSpan.FromMilliseconds(workTimeById.TotalWorkTime));
			}
			return times.Values;
		}
	}
}
